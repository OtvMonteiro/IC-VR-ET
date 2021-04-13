using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace ViveSR.anipal.Eye
{
public class ETDataSample : MonoBehaviour
{

    public float proporcaoRelativa = 1;
 //Variaveis para captura de dados de rastreamento ocular
    public int eyeCollisionLayer = 0;

    private FocusInfo FocusInfo;
    private readonly float cast_distance = 20;
    private readonly GazeIndex[] GazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
    private static EyeData_v2 eyeData = new EyeData_v2();
    private bool eye_callback_registered = false;
    private bool eye_focus = false;

 //Outras variaveis
    private GameObject sphere;//esferaAnterior, esferaAtual;
    private Vector3 proporcaoOriginal;
       
        
    private void Start()
    {
        //Checa se o framework do HMD esta funcionando
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "PosicaoDaVisao";
        proporcaoOriginal = sphere.localScale;
        sphere.localScale = proporcaoOriginal*proporcaoRelativa;

    }

    private void Update()
    {
        GetEyeData();
        RunEyeData();

        //Muda a posição da esfera para o ponto em que o usuário esta olhando
        if (eye_focus)
        {
            sphere.transform.position = FocusInfo.point;
        }
        
        //Mudando proporcao
        if(sphere.localScale != proporcaoOriginal*proporcaoRelativa)
        {
            sphere.localScale = proporcaoOriginal*proporcaoRelativa;
        }

    }

        private void RunEyeData()
        {
            foreach (GazeIndex index in GazePriority)
            {
                Ray GazeRay;
                
                if (eye_callback_registered)
                    eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, cast_distance, (1 << eyeCollisionLayer), eyeData);
                else
                    eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, cast_distance, (1 << eyeCollisionLayer));
                
            }
        }

        private void GetEyeData()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

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
    }

    private void Release()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }

    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
    }


}

}
