using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class replayHeatmap : StoreViewpoints
{
    //
    //public string arquivo;
    private FileStream fs;



    void Start()
    {
        
        //Abrir o arquivo fornecido
        fs = File.OpenRead(CaminhoDoArquivoDeReplay);

        //Garantindo que o replay acontecera
        EmExecucao = false;
        ModoPlayer = true;

        //Metodo para armazenar os dados do arquivo
        StreamReader sr = new StreamReader(fs);
        lerDados(sr);

    }


    void Update()
    {

        //Cria o replay considerando o tempo armazenado
        replayTempoReal();
    }

    //Descarta os valores desnecessarios para o heatmap do modelo antigo de captura
    private void lerDados(StreamReader sr)
    {
        //Descarta cabecalho
        for (int n = 0; n <= 8; n++)
        {
            sr.ReadLine();
        }

        //Começa a ler os valores
        while (sr.Peek()!=-1) {

            //Adiciona o tempo 'a lista de tempos
            tempos.Add((float)sr.Read());

            //Descarta 7
            for (int n = 0; n <= 7; n++) {sr.Read();}

            //Aquisição dos dados de ET
            Vector3 posicaoOlhosCombinada = new Vector3((float)sr.Read(), (float)sr.Read(), (float)sr.Read());
            Vector3 direcaoOlhosCombinada = new Vector3((float)sr.Read(), (float)sr.Read(), (float)sr.Read());
            //Transformar os dados capturados em um ponto no mundo
            Vector3 pontoHeatmap = encontrarPontoNoMundo(posicaoOlhosCombinada, direcaoOlhosCombinada);
            pontos.Add(pontoHeatmap);
            Debug.Log("Armazenado o ponto:  " + pontoHeatmap.x + ";" + pontoHeatmap.y + ";" + pontoHeatmap.z + ";\n");

            //imprimirPontoIndividualmente(pontoHeatmap);//Imprime todos os pontos no começo da execucao

            //Descarta 18
            for (int n = 0; n <= 18; n++) { sr.Read(); }

        }

        
    }

    private Vector3 encontrarPontoNoMundo(Vector3 posicaoOlhosCombinada, Vector3 direcaoOlhosCombinada)
    {
        RaycastHit pontoNoMundo;
        Ray raio = new Ray(posicaoOlhosCombinada, direcaoOlhosCombinada);
        if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))
        {
            return pontoNoMundo.point;
        }
        else return Vector3.zero;
        //else return posicaoOlhosCombinada;
    }

}
