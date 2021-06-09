using Tobii.G2OM;
using UnityEngine;

namespace Tobii.XR.Examples //Necessario? parece que nao
{
//Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
    public class GazeTriggerTeleport : MonoBehaviour, IGazeFocusable
    {
        public float distanciaMinima;
        private bool emFoco;

        //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
        public void GazeFocusChanged(bool hasFocus)
        {
            //If this object received focus
            if (hasFocus)
            {
                emFoco = true;
            }
            //If this object lost focus
            else
            {
                emFoco = false;
            }
        }

        private void Start()
        {
            emFoco = false;
            distanciaMinima = (float)5.0;
        }

        private void Update()
        {
            if (emFoco) //gatilho acionado
            {
                acao();
            }
            else return;
        }

        private void acao()
        {
        //Checando se o usuario nao esta muito perto da plataforma
        if (Vector3.Distance(Camera.main.transform.position, gameObject.transform.position) > distanciaMinima)
        {
            //Teleporta a camera principal para a posição do objeto que contem o script (com pequeno ajuste)
            Vector3 novaPosicao = gameObject.transform.position + new Vector3(0,3,0);
            Camera.main.transform.position = novaPosicao;
            Debug.Log("realizou a acao:  \n NovaPosicao:  "+ novaPosicao);
        }
            return;
        }
    }
}
