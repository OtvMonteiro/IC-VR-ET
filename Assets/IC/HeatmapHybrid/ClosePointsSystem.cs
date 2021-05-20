using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

//using Unity.Mathematics;

public class ClosePointsSystem : ComponentSystem {

    
    protected override void OnUpdate() {
        Entities.ForEach((ref ClosePointsComponent closePointComponent) => {
            
            
        });
    }

}
