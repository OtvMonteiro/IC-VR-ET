using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

//using Unity.Mathematics;

public class TemperatureSystem : ComponentSystem {

    protected int highestClosePoints = 0;
    protected override void OnUpdate() {
        // Entities.ForEach((Entity entity, ref ClosePointsComponent closeC) => {
            
        //     if(closeC.closePoints>highestClosePoints)highestClosePoints=closeC.closePoints;
        //     RenderMesh _renderMesh = GetComponentDataFromEntity<RenderMesh>();
        //     Material heatmap_material = new Material(_renderMesh.material.shader);
        //     heatmap_material = _renderMesh.material;
        //     float atualizaCor =  (float) closeC.closePoints/highestClosePoints;
        //     heatmap_material.color = new Color(heatmap_material.color.r+atualizaCor,heatmap_material.color.g - atualizaCor,0.0f);
        //     _renderMesh.material = heatmap_material;
        // });

    }

}
