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
    private GameObject focusedObject, cloneObject;
    private RaycastHit pontoAnterior;
    
    void Start()
    {
        //original_shader = outline_shader;
        //previousObject = gameObject;
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
            if (GameObject.Equals(focusedObject, pontoAnterior.collider.gameObject) ||
                GameObject.Equals(cloneObject, pontoAnterior.collider.gameObject)  ) { }
            else
            {
                //Changing objects
                focusedObject = pontoAnterior.collider.gameObject;
                CreateOutlining(focusedObject);
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
        if (hasFocus && !GameObject.Equals(focusedObject, Tobii.Gaming.TobiiAPI.GetFocusedObject() ))
        {
            focusedObject = Tobii.Gaming.TobiiAPI.GetFocusedObject();
            CreateOutlining(focusedObject);
        }
        else {
            RemoveOutline(cloneObject);
        }
    }

    private void CreateOutlining(GameObject focusedObject)
    {
        //Guarantees that last one is removed from scene
        RemoveOutline(cloneObject);

        //Creates a clone of the focused object
        cloneObject = GameObject.Instantiate(focusedObject, focusedObject.transform.position,
                                        focusedObject.transform.rotation, gameObject.transform) as GameObject;
        
        // Alters slightly the scale to bring the clone to the front
        cloneObject.transform.localScale = cloneObject.transform.localScale * 1.05f;
        // Set the shader, for higlighting and outlining effect
        if(cloneObject.TryGetComponent<Renderer>(out _renderer)){
            _renderer.material.shader = outline_shader;
        }
        else{
            _renderer = cloneObject.AddComponent<Renderer>();
            _renderer.material.shader = outline_shader;
        }

    }

    private void RemoveOutline(GameObject clone)
    {
        GameObject.Destroy(clone);
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
        RaycastHit pontoNoMundo;
        if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))
        {
            return pontoNoMundo;
        }
        else return pontoAnterior;
    }

}
