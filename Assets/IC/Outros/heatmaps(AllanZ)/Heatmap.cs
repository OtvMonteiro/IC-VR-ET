// Alan Zucconi
// www.alanzucconi.com
using UnityEngine;
using System.Collections;

public class Heatmap : MonoBehaviour
{

    public Vector3[] positions;
    public float[] radiuses;
    public float[] intensities;

    public Material material;

    public int count = 50;

    private Camera cam;

    void Start ()
    {
        positions = new Vector3[count];
        radiuses = new float[count];
        intensities= new float[count];

        //Pega a camera principal
        cam = Camera.main;
        //
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector3(0, 0, 0);//new Vector2(Random.Range(-0.4f, +0.4f), Random.Range(-0.4f, +0.4f));
            radiuses[i] = 0.25f;//Random.Range(0f, 0.25f);
            intensities[i] = 1f;// Random.Range(-0.25f, 1f);
        }
    }

    void Update()
    {

        //Cria um raio saindo do centro da camera escolhida e o ponto no mundo referente à visao da tela
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, cam.nearClipPlane));

        //Faz um raycast e atualiza um heatmap caso haja colisao
        RaycastHit pontoNoMundo;
        bool Colidiu = Physics.Raycast(raio, out pontoNoMundo, 100.0f);

        if (Colidiu)
        {
            material.SetInt("_Points_Length", positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = pontoNoMundo.point;//Todas as posicoes atualmente estam com o mesmo valor, modificar
                material.SetVector("_Points" + i.ToString(), positions[i]);

                Vector2 properties = new Vector2(radiuses[i], intensities[i]);
                material.SetVector("_Properties" + i.ToString(), properties);
            }
        }
    }
}