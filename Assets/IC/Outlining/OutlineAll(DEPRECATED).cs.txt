using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


using Tobii.G2OM;
using ViveSR.anipal.Eye;
using Tobii.Gaming;

public class OutlineAll : MonoBehaviour//, IGazeFocusable
{
    public Shader outline_shader;
    //private bool _outlineDesativado;
    //private bool _hasFocus;

    private Shader original_shader;
    private Renderer _renderer;
    private float _gazeStickinessSeconds = 1f;
    private GameObject focusedObject, previousObject;
    private RaycastHit pontoAnterior;
    
    void Start()
    {
        original_shader = outline_shader;
        previousObject = gameObject;
        //_renderer = GetComponent<Renderer>();
        //original_shader = _renderer.material.shader; 
        //Debug.Log("Outliner iniciado");
    }

   
    void Update()
    {
        //Checando se o HMD esta funcionando corretamente e se pode ser usado
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                 SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
        {
            //Debug.Log("Não achou o Framework SRanipal");
            pontoAnterior = capturaPonto(Camera.main);
            if (GameObject.Equals(previousObject, pontoAnterior.collider.gameObject)) { }
            else
            {
                //Debug.Log("Entrou para modificar o shader");
                RemoveOutline(previousObject);
                //Changing objects
                previousObject = pontoAnterior.collider.gameObject;
                CriarOutline(previousObject);
            }

        }
        else
        {
            //Debug.Log("Achou o Framework SRanipal");
        }

    }

    /// <summary>
    /// Called by TobiiXR when object receives or loses focus.
    /// </summary>
    /// <param name="hasFocus"></param> 
    public void GazeFocusChanged(bool hasFocus)
    {
        //Debug.Log("Um objeto esta sob foco");
        //_hasFocus = hasFocus;
        //StartOutlineAnimation(hasFocus);
        if (hasFocus)
        {
            previousObject = Tobii.Gaming.TobiiAPI.GetFocusedObject();
            CriarOutline(previousObject);
        }
        else {
            RemoveOutline(previousObject);
        }
    }

    private void CriarOutline(GameObject focusedObject)
    {
        original_shader = focusedObject.GetComponent<Renderer>().material.shader;

        focusedObject.GetComponent<Renderer>().material.shader = outline_shader;


        //original_shader = focusedObject.GetComponent<Shader>();
        //Debug.Log("Shader original:  "+ original_shader.ToString());

        //focusedObject.GetComponent<Shader>() = GetComponent<Shader>("Outline_shader");

        // if(hasFocus){
        //      _renderer.material.shader = outline_shader;
        // }   
        // else{ //Ineficiente, pois não considera se é realmente preciso executar laço
        //     waitABit();
        //     _renderer.material.shader = original_shader;
        // }


    }

    private void RemoveOutline(GameObject previousObject)
    {
        if(previousObject.TryGetComponent<Renderer>(out _renderer))
        {
            _renderer.material.shader = original_shader;
        }
        
    }

    //Evitar flickering
    public float GazeStickinessSeconds
        {
            get { return _gazeStickinessSeconds; }
            set { _gazeStickinessSeconds = value; }
        }

    // private IEnumerator waitABit(){
    //     yield return new WaitForSecondsRealtime(_gazeStickinessSeconds);
    // }

    private RaycastHit capturaPonto(Camera cam)
    {
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, cam.nearClipPlane));//Deprecated?
        RaycastHit pontoNoMundo;
        if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))//Distância arbitraria demais?
        {
            return pontoNoMundo;
        }
        else return pontoAnterior;
    }

}
