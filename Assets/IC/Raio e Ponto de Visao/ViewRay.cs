using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewRay : MonoBehaviour
{
    public bool PontoDeVisao, RaioDeVisao;
    
    private bool stateRay, statePoint;
    private Camera cam;
    private ParticleSystem ps;

    //Controle de acionamento
    private bool PontoDeVisao_anterior, RaioDeVisao_anterior;



    void Start()
    {
        stateRay = statePoint = false;
        PontoDeVisao_anterior = PontoDeVisao;
        RaioDeVisao_anterior  = RaioDeVisao;

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
        //if (!Input.GetKeyDown(KeyCode.V)){ }
       
        if(RaioDeVisao==RaioDeVisao_anterior){}
        else
        {
            stateRay = !stateRay;
           //Debugando
            //if (stateRay) { Debug.Log("Raio de Visão Ativado"); }
            //else { Debug.Log("Raio de Visão Desativado"); }
        }
       
        // Ao pressionar 'C' o estado de ativação da funcao de ponto muda
        //if (!Input.GetKeyDown(KeyCode.C)) { }
       
       if(PontoDeVisao==PontoDeVisao_anterior){}
        else
        {
            statePoint = !statePoint;
            //Debugando
            //if (statePoint) { Debug.Log("Ponto da Visão Ativado"); }
            //else { Debug.Log("Ponto da Visão Desativado"); }
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

            //Faz um raycast
            bool Colidiu = Physics.Raycast(raio, out pontoNoMundo, 100.0f);
            
            //desenha o raio caso haja colisao
            if (Colidiu && stateRay)
            {
                Debug.DrawLine(pontoVistoPelaCamera, pontoNoMundo.point, Color.cyan, 0.03f);
            }
            
            //Cria o ponto com o sistema de particulas
            if (Colidiu && statePoint)
            {
                ps.Play();
                Vector3 colisao = pontoNoMundo.normal;//No momento a rotacao de ps esta errada, sempre cortado ao meio
                ps.transform.SetPositionAndRotation(pontoNoMundo.point, new Quaternion(colisao.x, colisao.y, colisao.z, 0)) ;
                 
            }
            else ps.Stop();//Garantindo que a particula pare



        }

    }

    


}
