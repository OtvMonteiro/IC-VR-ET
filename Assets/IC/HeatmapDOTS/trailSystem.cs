using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Burst;


namespace heatmapDOTS.Systems
{
    [BurstCompile]
    public class trailSystem : ComponentSystem
    {
        public int trailSize = 10; //Pode ser parametro passado pelo MonoBehaviour
        private Entity[]  entitiesWindow;
        private int windowCounter=0;

        protected override void OnCreate()
        {
            entitiesWindow = new Entity[trailSize];
        }

        protected override void OnUpdate()
        {
            bool trailMode = heatmapDOTS.TrailMode;
            bool last_trailMode = heatmapDOTS.Last_TrailMode;
            if(trailMode != last_trailMode){
                if(trailMode){  createTrailMode();}
                else         {  normalMode();}
                heatmapDOTS.Last_TrailMode = trailMode;
            }

            //With every update, re-run the code
            if(trailMode) createTrailMode();
        }

        protected void createTrailMode(){

        {//PODE SER MUITO INEFICIENTE SE ATUALIZA A TODO UPDATE
        //     //EntityManager.SetEnabled(entity, false); pode servir , mas ainda não determina para cada posicao
        //     //int entityInQueryIndex — the index of the entity in the list of all entities selected by the query:: só pra job?, ainda precisaria saber do comprimento
        //     //NativeArray<Entity> heatmap_entities = EntityManager.HasComponent<heatmapIdentifier>().Entities;
        //     Debug.Log("Creating trail");
        //     EntityQuery ent_query = GetEntityQuery(typeof(heatmapIdentifier), ComponentType.ReadOnly<heatmapIdentifier>());
        //     NativeArray<Entity> heatmap_entities = ent_query.ToEntityArray(Unity.Collections.Allocator.Temp);
        //     NativeArray<Entity> trail_entities = heatmap_entities.GetSubArray((heatmap_entities.Length - trailSize), trailSize);
            
        //     //heatmap_entities[index]
                

        //     heatmap_entities.Dispose();
        //     trail_entities.Dispose();
        }//).ScheduleParallel();
            
            float current_scale = heatmapDOTS.HeatmapPointsScale;
            Entities
                .WithAll<heatmapIdentifier>()
                .ForEach( (Entity entity, ref NonUniformScale scale) => 
            {
                //Slides the windows throughout entities (not very efficient)
                entitiesWindow[windowCounter] = entity;
                //Sets the scale as small for the 'old' heatmap points
                scale.Value = current_scale/10;

                windowCounter++;//setting the window sliding
                if(windowCounter == trailSize){windowCounter=0;}
            });

            //For the window with the last entities, set the scale back
            for (int i = 0; i < trailSize; i++)
            {
                EntityManager.SetComponentData<NonUniformScale>(entitiesWindow[i],
                                                 new NonUniformScale {Value = current_scale});
            }

        
        }

        protected void normalMode(){
            float current_scale = heatmapDOTS.HeatmapPointsScale;
            Entities.WithAll<heatmapIdentifier>().ForEach((ref NonUniformScale scale) =>
            {
                scale.Value = current_scale;
            });
        }

    }
}