using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapSO : MonoBehaviour
{
    public int x_size = 128; //width of the textures
    public int y_size = 128; //height of the textures
    public int num_shells = 10; //number of slices of the data
    public GameObject plane_prefab; //the slice geometry and shader
                                    //this prefab is just a plane with the same area 
                                    //as the building, with a material that has its
                                    //shader set to Unlit/Transparent

    public float height = 10; //how tall in world units the dataset is
    public Color LowColor; //the cold color
    public Color HiColor; //the hot color

    void Start()
    {
        for (int i = 0; i < num_shells; i++)
        {

            var shell = GameObject.Instantiate(this.plane_prefab);  //create a slice
            shell.transform.parent = this.transform;
            shell.transform.position += new Vector3(0, (float)i / (float)this.num_shells * this.height, 0);

            var texture = new Texture2D(x_size, y_size);
            //for each texel in the shell's texture
            for (int x = 0; x < x_size; x++)
            {
                for (int y = 0; y < y_size; y++)
                {
                    var heatLocation = new Vector3((float)x / (float)x_size, (float)y / (float)y_size, (float)i / (float)num_shells); //find the coordinate in the heatmap for this texel
                    var heat = 1f+ x/248f - y/248f;//(Perlin.Fbm(heatLocation, 4) + 1) / 2f; //grab the heat map data
                    texture.SetPixel(x, y, Color.Lerp(this.LowColor, this.HiColor, heat)); //interpolate the color based on heat map
                }
            }

            texture.Apply();


            shell.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);

        }
    }
}