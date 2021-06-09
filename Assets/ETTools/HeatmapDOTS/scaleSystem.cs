using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace heatmapDOTS.Systems
{
    public class scaleSystem : ComponentSystem
    {
        //private heatmap heatmap; //Bad practice?:creates an object
        //private EntityQuery group;

        protected override void OnCreate()
        {
            // this.group = GetEntityQuery(
            //     ComponentType.ReadWrite<Transform>()
            // );
            // this.RequireForUpdate(this.group);
        }

        protected override void OnUpdate()
        {
            float h_scale = heatmapDOTS.HeatmapPointsScale;
            float last_h_scale = heatmapDOTS.Last_HeatmapPointsScale;
            //Debug.Log("UPDATE SCALE SYSTEM: scale="+h_scale+";  last_scale="+last_h_scale);
            if(h_scale != last_h_scale){
                updateScale(new float3(h_scale));
                heatmapDOTS.Last_HeatmapPointsScale = h_scale;
            }
            
        }

        protected void updateScale(float3 newScale){
            
            Entities.WithAll<NonUniformScale>().ForEach((ref NonUniformScale scale) =>
            {
                scale.Value = newScale;
            });

        }
    }
}