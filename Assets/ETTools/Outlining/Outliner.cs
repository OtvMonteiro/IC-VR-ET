using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;
using System;

using Tobii.Gaming;

public class Outliner : MonoBehaviour, IGazeFocusable
{
    public Shader outline_shader;
    //private bool _outlineDesativado;
    //private bool _hasFocus;

    private Shader original_shader;
    private Renderer _renderer;
    private float _gazeStickinessSeconds = 1f;
    private GameObject focusedObject;
    
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        original_shader = _renderer.material.shader; 
        //Debug.Log("Outliner iniciado");
    }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    /* /// <summary>
    /// Called by TobiiXR when object receives or loses focus.
    /// </summary>
    /// <param name="hasFocus"></param> */
    public void GazeFocusChanged(bool hasFocus)
    {
        //Debug.Log("Um objeto esta sob foco");
        //_hasFocus = hasFocus;
        //StartOutlineAnimation(hasFocus);
        CriarOutline(hasFocus);
    }

    private void CriarOutline(bool hasFocus)
    {
        //focusedObject = Tobii.Gaming.TobiiAPI.GetFocusedObject();
        //original_shader = focusedObject.GetComponent<Shader>();
        //Debug.Log("Shader original:  "+ original_shader.ToString());
        
        //focusedObject.GetComponent<Shader>() = GetComponent<Shader>("Outline_shader");
       
        if(hasFocus){
             _renderer.material.shader = outline_shader;
        }   
        else{ //Ineficiente, pois não considera se é realmente preciso executar laço
            waitABit();
            _renderer.material.shader = original_shader;
        }


    }


    //Evitar flickering
    public float GazeStickinessSeconds
        {
            get { return _gazeStickinessSeconds; }
            set { _gazeStickinessSeconds = value; }
        }

    private IEnumerator waitABit(){
        yield return new WaitForSecondsRealtime(_gazeStickinessSeconds);
    }

}
