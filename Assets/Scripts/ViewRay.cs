using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewRay : MonoBehaviour
{
    private bool stateRay, statePoint;
    private Camera cam;
    private ParticleSystem ps;

    void Start()
    {
        stateRay = statePoint = false;
        //Sistema de particulas usado para o ponto
        ps = gameObject.GetComponent("ParticleSystem") as ParticleSystem;
        if (ps == null) { Debug.Log("ps = gameObject.GetComponent(\"ParticleSystem\") as ParticleSystem retornou nulo"); }
        ps.Stop();
        //Pega a camera principal
        cam = Camera.main;
    }

    void Update()
    {
        // Ao pressionar 'V' o estado de ativação da funcao de raio muda
        if (!Input.GetKeyDown(KeyCode.V)){ }
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
            //Exemplo com camera
            //Cria um raio saindo do centro da camera escolhida e o ponto no mundo referente à visao da tela
            Ray raio = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth/2, cam.scaledPixelHeight/2, cam.nearClipPlane));
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
                    ps.transform.SetPositionAndRotation(pontoNoMundo.point, new Quaternion(colisao.x, colisao.y, colisao.z, 0)) ;
                } 
            }
            else { ps.Stop(); }//Garantindo que a particula pare



        }

    }
}
