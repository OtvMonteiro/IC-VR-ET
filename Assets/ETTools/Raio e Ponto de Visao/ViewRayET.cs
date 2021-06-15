using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ViveSR.anipal.Eye
{
    public class ViewRayET : MonoBehaviour
    {

        //Controle de acionamento
        public bool PontoDeVisao, RaioDeVisao;
        private bool PontoDeVisao_anterior, RaioDeVisao_anterior;

        //Variaveis 
        private bool stateRay, statePoint, enabled_particle_system;
        [SerializeField] private GameObject indicadorDePonto;
        private GameObject point_indicator;
        private ParticleSystem ps;
   

        //Variaveis SRanipal
        private FocusInfo FocusInfo;
        private readonly float MaxDistance = 20;
        private readonly GazeIndex[] GazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
        private static EyeData_v2 eyeData = new EyeData_v2();
        private bool eye_callback_registered = false;



        /////// 
        void Start()
        {
            stateRay = statePoint = false;
            PontoDeVisao_anterior = PontoDeVisao = false;
            RaioDeVisao_anterior  = RaioDeVisao = false;

            //Sistema de particulas usado para o ponto ou esfera como indicador
            
            if (!gameObject.TryGetComponent<ParticleSystem>(out ps)) {
                 enabled_particle_system = false;
                 //Debug.Log("ParticleSystem não encontrada no objeto");
                 //point_indicator = GameObject.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                 point_indicator = GameObject.Instantiate(indicadorDePonto) as GameObject;
                 //point_indicator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
            else {
                enabled_particle_system = true;
            }
            hidePoint();

            //Framework SRanipal
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }

        
        }

        void Update()
        {
            if(RaioDeVisao==RaioDeVisao_anterior){}
            else
            {
                stateRay = !stateRay;
                RaioDeVisao_anterior=RaioDeVisao;
                //Debugando
                //if (stateRay) { Debug.Log("Raio de Visão Ativado"); }
                //else { Debug.Log("Raio de Visão Desativado"); }
            }
            
            
            if(PontoDeVisao==PontoDeVisao_anterior){}
            else
            {
                statePoint = !statePoint;
                PontoDeVisao_anterior=PontoDeVisao;
                
                if(!statePoint) { hidePoint(); }
            }




            //Caso o estado determinado indique que deve ser desenhado vetor de visao e/ou o ponto
            if (!stateRay && !statePoint) {  }
            else
            {
                Ray _ray = GetGazeRay();
                //Cam as starting point (better to use the HMD position)
                Camera _cam = getCam();
                Vector3 pointInScreen = _cam.ScreenToWorldPoint(new Vector3(_cam.scaledPixelWidth / 2, _cam.scaledPixelHeight / 2, _cam.nearClipPlane));
                RaycastHit worldPoint;

                //Raycast
                bool Colidiu = Physics.Raycast(_ray, out worldPoint, 100.0f);
                
                //desenha o raio caso haja colisao
                if (Colidiu && stateRay)
                {
                    Debug.DrawLine(pointInScreen, worldPoint.point, Color.cyan, 0.03f);
                }
                
                //Cria o ponto com o sistema de particulas
                if (Colidiu && statePoint)
                {
                    showPoint(worldPoint.point);
                    
                }
                else hidePoint();//Garantindo que a particula pare


            }

        }

        


        private Camera getCam()
        { //Funciona para HMD? Deveriamos alternar?
            if(Camera.main.isActiveAndEnabled) return Camera.main;
            else return Camera.current;
        }


        private Ray GetGazeRay()
        {
            Camera _cam = getCam();
                    
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT){
                    //Caso falhe a deteccao do olhar, pega a camera principal como olhar
                    return _cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            } 


            //Captura os dados de ET (refresh rate maior)
            if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
            {
                SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                eye_callback_registered = true;
            }
            else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
            {
                SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                eye_callback_registered = false;
            }
            else { return _cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0)); }

            //Analisa a prioridade dos dados e envia como retorno um GazeRay caso esteja com foco
            foreach (GazeIndex index in GazePriority)
            {
                Ray GazeRay;
                bool eye_focus;
                if (eye_callback_registered)
                    eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, MaxDistance, eyeData);
                else
                    eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, MaxDistance);

                if (eye_focus)
                {
                    return GazeRay;
                }
               
            }

            //Garantindo retorno
            return _cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        }


        private static void EyeCallback(ref EyeData_v2 eye_data)
        {
            eyeData = eye_data;
        }


        private void showPoint(Vector3 point){
            if(enabled_particle_system){
                // if(ps.isPlaying){} //Cria flickering dependendo do tempo do SP
                // else ps.Play();
                ps.Play();
                gameObject.transform.SetPositionAndRotation(point, Quaternion.identity) ;
            }
            else{
                point_indicator.SetActive(true);
                point_indicator.transform.position = point;
            }
        }

        private void hidePoint(){
            if(enabled_particle_system){
                ps.Stop();
            }
            else{
                point_indicator.SetActive(false);
            }
        }

    }


}

