using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ViveSR.anipal.Eye
{
    public class ViewRayET : MonoBehaviour
    {
        //Variaveis 
        private bool stateRay, statePoint;
        private ParticleSystem ps;
        private Camera cam = Camera.main;
   

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
            //Sistema de particulas usado para o ponto
            ps = gameObject.GetComponent("ParticleSystem") as ParticleSystem;
            if (ps == null) { Debug.Log("ps = gameObject.GetComponent(\"ParticleSystem\") as ParticleSystem retornou nulo"); }
            ps.Stop();

            //Framework SRanipal

            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
        }

        void Update()
        {
            // Ao pressionar 'V' o estado de ativação da funcao de raio muda
            if (!Input.GetKeyDown(KeyCode.V)) { }
            else
            {
                stateRay = !stateRay;
                if (stateRay) { Debug.Log("Raio de Visão Ativado"); }
                else { Debug.Log("Raio de Visão Desativado"); }
            }
            // Ao pressionar 'C' o estado de ativação da funcao de ponto muda
            if (!Input.GetKeyDown(KeyCode.C)) { }
            else
            {
                statePoint = !statePoint;
                if (statePoint) { Debug.Log("Ponto da Visão Ativado"); }
                else { Debug.Log("Ponto da Visão Desativado"); }
            }




            //Caso o estado determinado indique que deve ser desenhado vetor de visao e/ou o ponto
            if (!stateRay && !statePoint) { ps.Stop(); }
            else
            {
                //Cria um raio saindo do usuario e o ponto no mundo referente ao olhar
                Ray raio = RaioOlhar();
                //Atualmente pega a camera principal como origem
                Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, cam.nearClipPlane));
                RaycastHit pontoNoMundo;

                //Faz um raycast e desenha o raio caso haja colisao
                bool Colidiu = Physics.Raycast(raio, out pontoNoMundo, 100.0f);
                if (Colidiu && stateRay)
                {
                    Debug.DrawLine(pontoVistoPelaCamera, pontoNoMundo.point, Color.cyan, 0.03f);
                }

                //Cria o ponto com o sistema de particulas
                if (statePoint)
                {
                    ps.Play();
                    if (Colidiu)
                    {
                        Vector3 colisao = pontoNoMundo.normal;//No momento a rotacao de ps esta errada, sempre cortado ao meio
                        ps.transform.SetPositionAndRotation(pontoNoMundo.point, new Quaternion(colisao.x, colisao.y, colisao.z, 0));
                    }
                }
                else { ps.Stop(); }//Garantindo que a particula pare



            }

        }

        private Ray RaioOlhar()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) Debug.LogError("Erro de Framework - Não funcionando");

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
            else { return cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0)); }

            foreach (GazeIndex index in GazePriority)
            {
                Ray GazeRay;
                int dart_board_layer_id = LayerMask.NameToLayer("NoReflection");
                bool eye_focus;
                if (eye_callback_registered)
                    eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, MaxDistance, (1 << dart_board_layer_id), eyeData);
                else
                    eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, MaxDistance, (1 << dart_board_layer_id));

                if (eye_focus)
                {
                    return GazeRay;
                }
               
            }
            //Caso falhe a deteccao do olhar, pega a camera principal como olhar
              return cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        }


        private static void EyeCallback(ref EyeData_v2 eye_data)
        {
            eyeData = eye_data;
        }


    }


}

