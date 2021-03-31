using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;

//MODIFICADO PARA TESTES DE GRAVAÇÃO DE DADOS DO HEATMAP E EYETRACKING

public static class BinaryManager {


	public static void SaveInfo (ItemEntry dados, string path){
		BinaryFormatter bf = new BinaryFormatter();
		FileStream stream = new FileStream (path, FileMode.Create);

		PlayerData[] user = new PlayerData[dados.infos.Count];

		for (int i = 0; i < dados.infos.Count; i++) {
			user [i] = new PlayerData (dados.infos [i]);
		}

		user[0].name = dados.name;
        user[0].heatmapPoint[0] = dados.heatmapPoint.x;
        user[0].heatmapPoint[1] = dados.heatmapPoint.y;
        user[0].heatmapPoint[2] = dados.heatmapPoint.z;

        user[0].eyeTrackingOriginCombinedLocal[0] = dados.eyeTrackingOriginCombinedLocal.x;
        user[0].eyeTrackingOriginCombinedLocal[1] = dados.eyeTrackingOriginCombinedLocal.y;
        user[0].eyeTrackingOriginCombinedLocal[2] = dados.eyeTrackingOriginCombinedLocal.z;


        user[0].eyeTrackingDirectionCombinedLocal[0] = dados.eyeTrackingDirectionCombinedLocal.x;
        user[0].eyeTrackingDirectionCombinedLocal[1] = dados.eyeTrackingDirectionCombinedLocal.y;
        user[0].eyeTrackingDirectionCombinedLocal[2] = dados.eyeTrackingDirectionCombinedLocal.z;


        user[0].eyeTrackingDirectionLeftLocal[0] = dados.eyeTrackingDirectionLeftLocal.x;
        user[0].eyeTrackingDirectionLeftLocal[1] = dados.eyeTrackingDirectionLeftLocal.y;
        user[0].eyeTrackingDirectionLeftLocal[2] = dados.eyeTrackingDirectionLeftLocal.z;


        user[0].eyeTrackingOriginLeftLocal[0] = dados.eyeTrackingOriginLeftLocal.x;
        user[0].eyeTrackingOriginLeftLocal[1] = dados.eyeTrackingOriginLeftLocal.y;
        user[0].eyeTrackingOriginLeftLocal[2] = dados.eyeTrackingOriginLeftLocal.z;


        user[0].eyeTrackingDirectionRightLocal[0] = dados.eyeTrackingDirectionRightLocal.x;
        user[0].eyeTrackingDirectionRightLocal[1] = dados.eyeTrackingDirectionRightLocal.y;
        user[0].eyeTrackingDirectionRightLocal[2] = dados.eyeTrackingDirectionRightLocal.z;


        user[0].eyeTrackingOriginRightLocal[0] = dados.eyeTrackingOriginRightLocal.x;
        user[0].eyeTrackingOriginRightLocal[1] = dados.eyeTrackingOriginRightLocal.y;
        user[0].eyeTrackingOriginRightLocal[2] = dados.eyeTrackingOriginRightLocal.z;

        


		bf.Serialize (stream, user);
		stream.Close ();
	}
    

    public static List<InfoperSecond> LoadInfo (string path) {
		if (File.Exists(path)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream (path, FileMode.Open);

			PlayerData[] user = bf.Deserialize (stream) as PlayerData[];
			List<InfoperSecond> infos = new List<InfoperSecond> ();

			for (int i = 0; i < user.Length; i++) {
				infos.Add (new InfoperSecond (user [i]));
			}



			stream.Close ();

			return infos;

		} else {
			return new List<InfoperSecond>();
		}
	}

	public static ItemEntry LoadAllData (string path) {
		if (File.Exists(path)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream (path, FileMode.Open);

			PlayerData[] user = bf.Deserialize (stream) as PlayerData[];
			List<InfoperSecond> infos = new List<InfoperSecond> ();

			for (int i = 0; i < user.Length; i++) {
				infos.Add (new InfoperSecond (user [i]));
			}

			ItemEntry data = new ItemEntry (user [0].name, infos);

			
			stream.Close ();

			return data;

		} else {
			return new ItemEntry();
		}
	}

}


[Serializable]
//classe usada para serializar em binario(contém definições em baixo nível)
public class PlayerData {
	
	public string name;


    public float[] heatmapPoint;


    public float time;
	

    
    public float[] eyeTrackingOriginCombinedLocal;
    public float[] eyeTrackingDirectionCombinedLocal;
    public float[] eyeTrackingDirectionLeftLocal;
    public float[] eyeTrackingOriginLeftLocal;
    public float[] eyeTrackingDirectionRightLocal;
    public float[] eyeTrackingOriginRightLocal;

    public PlayerData() {
	}

	public PlayerData (InfoperSecond infos) {


        heatmapPoint = new float[3];

        eyeTrackingOriginCombinedLocal = new float[3];
        eyeTrackingDirectionCombinedLocal = new float[3];
        eyeTrackingDirectionLeftLocal = new float[3];
        eyeTrackingOriginLeftLocal = new float[3];
        eyeTrackingDirectionRightLocal = new float[3];
        eyeTrackingOriginRightLocal = new float[3];

        

		time = infos.time;

        heatmapPoint[0] = infos.heatmapPoint.x;
        heatmapPoint[1] = infos.heatmapPoint.y;
        heatmapPoint[2] = infos.heatmapPoint.z;

        eyeTrackingOriginCombinedLocal[0] = infos.eyeTrackingOriginCombinedLocal.x;
        eyeTrackingOriginCombinedLocal[1] = infos.eyeTrackingOriginCombinedLocal.y;
        eyeTrackingOriginCombinedLocal[2] = infos.eyeTrackingOriginCombinedLocal.z;


        eyeTrackingDirectionCombinedLocal[0] = infos.eyeTrackingDirectionCombinedLocal.x;
        eyeTrackingDirectionCombinedLocal[1] = infos.eyeTrackingDirectionCombinedLocal.y;
        eyeTrackingDirectionCombinedLocal[2] = infos.eyeTrackingDirectionCombinedLocal.z;


        eyeTrackingDirectionLeftLocal[0] = infos.eyeTrackingDirectionLeftLocal.x;
        eyeTrackingDirectionLeftLocal[1] = infos.eyeTrackingDirectionLeftLocal.y;
        eyeTrackingDirectionLeftLocal[2] = infos.eyeTrackingDirectionLeftLocal.z;


        eyeTrackingOriginLeftLocal[0] = infos.eyeTrackingOriginLeftLocal.x;
        eyeTrackingOriginLeftLocal[1] = infos.eyeTrackingOriginLeftLocal.y;
        eyeTrackingOriginLeftLocal[2] = infos.eyeTrackingOriginLeftLocal.z;


        eyeTrackingDirectionRightLocal[0] = infos.eyeTrackingDirectionRightLocal.x;
        eyeTrackingDirectionRightLocal[1] = infos.eyeTrackingDirectionRightLocal.y;
        eyeTrackingDirectionRightLocal[2] = infos.eyeTrackingDirectionRightLocal.z;


        eyeTrackingOriginRightLocal[0] = infos.eyeTrackingOriginRightLocal.x;
        eyeTrackingOriginRightLocal[1] = infos.eyeTrackingOriginRightLocal.y;
        eyeTrackingOriginRightLocal[2] = infos.eyeTrackingOriginRightLocal.z;


    }
}

//classes usadas para gravar em tempo real, com definições do Unity de alto nível
public class InfoperSecond {


    public Vector3 heatmapPoint;


    public float time;


    public Vector3 eyeTrackingOriginCombinedLocal;
    public Vector3 eyeTrackingDirectionCombinedLocal;
    public Vector3 eyeTrackingDirectionLeftLocal;
    public Vector3 eyeTrackingOriginLeftLocal;
    public Vector3 eyeTrackingDirectionRightLocal;
    public Vector3 eyeTrackingOriginRightLocal;
 


	//necessario para o serializer
	public InfoperSecond() {
	}

    public InfoperSecond (PlayerData data) {


        heatmapPoint.x = data.heatmapPoint[0];
        heatmapPoint.y = data.heatmapPoint[1];
        heatmapPoint.z = data.heatmapPoint[2];


        eyeTrackingOriginCombinedLocal.x = data.eyeTrackingOriginCombinedLocal[0];
        eyeTrackingOriginCombinedLocal.y = data.eyeTrackingOriginCombinedLocal[1];
        eyeTrackingOriginCombinedLocal.z = data.eyeTrackingOriginCombinedLocal[2];


        eyeTrackingDirectionCombinedLocal.x = data.eyeTrackingDirectionCombinedLocal[0];
        eyeTrackingDirectionCombinedLocal.y = data.eyeTrackingDirectionCombinedLocal[1];
        eyeTrackingDirectionCombinedLocal.z = data.eyeTrackingDirectionCombinedLocal[2];


        eyeTrackingDirectionLeftLocal.x = data.eyeTrackingDirectionLeftLocal[0];
        eyeTrackingDirectionLeftLocal.y = data.eyeTrackingDirectionLeftLocal[1];
        eyeTrackingDirectionLeftLocal.z = data.eyeTrackingDirectionLeftLocal[2];


        eyeTrackingOriginLeftLocal.x = data.eyeTrackingOriginLeftLocal[0];
        eyeTrackingOriginLeftLocal.y = data.eyeTrackingOriginLeftLocal[1];
        eyeTrackingOriginLeftLocal.z = data.eyeTrackingOriginLeftLocal[2];


        eyeTrackingDirectionRightLocal.x = data.eyeTrackingDirectionRightLocal[0];
        eyeTrackingDirectionRightLocal.y = data.eyeTrackingDirectionRightLocal[1];
        eyeTrackingDirectionRightLocal.z = data.eyeTrackingDirectionRightLocal[2];


        eyeTrackingOriginRightLocal.x = data.eyeTrackingOriginRightLocal[0];
        eyeTrackingOriginRightLocal.y = data.eyeTrackingOriginRightLocal[1];
        eyeTrackingOriginRightLocal.z = data.eyeTrackingOriginRightLocal[2];

        time = data.time;
	}

    public InfoperSecond(float tempo, Vector3 pontoHeatmap,
       Vector3 posicaoOlhosCombinados, Vector3 direcaoOlhosCombinados, Vector3 direcaoOlhoEsquerdo, Vector3 posicaoOlhoEsquerdo,
       Vector3 direcaoOlhoDireito, Vector3 posicaoOlhoDireito)
    {
        time = tempo;

        heatmapPoint = pontoHeatmap;

        eyeTrackingOriginCombinedLocal = posicaoOlhosCombinados;
        eyeTrackingDirectionCombinedLocal = direcaoOlhosCombinados;
        eyeTrackingDirectionLeftLocal = direcaoOlhoEsquerdo;
        eyeTrackingOriginLeftLocal = posicaoOlhoEsquerdo;
        eyeTrackingDirectionRightLocal = direcaoOlhoDireito;
        eyeTrackingOriginRightLocal = posicaoOlhoDireito;


    }
}

public class ItemEntry {

    public string name;

    public Vector3 heatmapPoint;

    public Vector3 eyeTrackingOriginCombinedLocal;
    public Vector3 eyeTrackingDirectionCombinedLocal;
    public Vector3 eyeTrackingDirectionLeftLocal;
    public Vector3 eyeTrackingOriginLeftLocal;
    public Vector3 eyeTrackingDirectionRightLocal;
    public Vector3 eyeTrackingOriginRightLocal;



    public List<InfoperSecond> infos = new List<InfoperSecond> ();

	//necessario para o serializer
	public ItemEntry () {
	}
	
    //public ItemEntry(string nome, Vector3 pontoHeatmap, Vector3 posicaoOlhosCombinados, Vector3 direcaoOlhosCombinados, Vector3 direcaoOlhoEsquerdo, Vector3 posicaoOlhoEsquerdo,
    //    Vector3 direcaoOlhoDireito, Vector3 posicaoOlhoDireito)
    //{
    //    name = nome;

    //    heatmapPoint = pontoHeatmap;

    //    eyeTrackingOriginCombinedLocal = posicaoOlhosCombinados;
    //    eyeTrackingDirectionCombinedLocal = direcaoOlhosCombinados;
    //    eyeTrackingDirectionLeftLocal = direcaoOlhoEsquerdo;
    //    eyeTrackingOriginLeftLocal = posicaoOlhoEsquerdo;
    //    eyeTrackingDirectionRightLocal = direcaoOlhoDireito;
    //    eyeTrackingOriginRightLocal = posicaoOlhoDireito;

    //}

    public ItemEntry(string nome, List<InfoperSecond> informacoes)
    {
        name = nome;
        infos = informacoes;
    }

}