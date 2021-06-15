using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ViveSR.anipal.Eye
{
    public class AOI : MonoBehaviour
    {
        [SerializeField] [ComAliasName("Activate AOIs")] private bool active;
        [SerializeField] protected Collider[] aoiColliders;
        public int[] occurencies; 
        
        private Collider lastCollision;


        //Variaveis SRanipal
        private FocusInfo FocusInfo;
        private readonly float MaxDistance = 20;
        private readonly GazeIndex[] GazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
        private static EyeData_v2 eyeData = new EyeData_v2();
        private bool eye_callback_registered = false;



        /////// 
        void Start()
        {  
            lastCollision = new Collider();
            //Framework SRanipal
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
        }

        void Update()
        {
            //if not active do nothing
            if (!active) {  }
            else
            {
                //Raycast
                bool collided = Physics.Raycast(GetGazeRay(), out RaycastHit worldPoint, 100.0f);
                if(collided){
                    lastCollision = worldPoint.collider;
                }

                Compare();
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

        private void Compare()
        {
            int n_aoi = aoiColliders.Length;
            for (int i=0; i < n_aoi; i++)
            {
                if(lastCollision == aoiColliders[i]){occurencies[i]++;}
            }
        }

    }
}

