

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;

using heatmapDOTS;
using System;

public class heatmapColorizer : MonoBehaviour
{
    [SerializeField] public bool Colorize = false;
    [SerializeField] private float distance;
    [SerializeField] private Material mat_default, mat_tmp1, mat_tmp2, mat_tmp3;
    private Entity[] heatmapEntities;
    private GameObject ghost;
    private bool last_Colorize;
    private EntityManager entityManager;


    

    void Start()
    {
    
        last_Colorize = false;
        //Get the entity manager
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    
    
    void Update()
    {
        
        if(Colorize!=last_Colorize){
            last_Colorize = Colorize;
            
            if(Colorize) {
                //Get array of entities from heatmap
                getEntities();
                //Resetting count of closebyes
                resetCount();
                //Counting closebys
                countClose();
                //Setting temperature material
                setTemperature();
            }
            else{
                //Reset texture
                getEntities();
                resetMaterial();
            }
            
            
            
        }
        

    }

    public void getEntities(){
        List<Entity> heatmap_entities = new List<Entity>();
        Entity[] all_entities = entityManager.GetAllEntities(Unity.Collections.Allocator.Temp).ToArray();
        foreach (Entity entity in all_entities)
        {
            //Try-finally to get the heatmapidentifier
            bool found = true;
            try
            {
                entityManager.GetComponentData<heatmapIdentifier>(entity);
            }
            catch (ArgumentException)
            {
                Debug.Log("Exception: entity not from heatmap");
                found = false;
            }
            
            if(found){
                heatmap_entities.Add(entity);
            } //positions.Add(entityManager.GetComponentData<Translation>(entity).Value);
        }

        heatmapEntities = heatmap_entities.ToArray();
    }



    private void resetCount()
    {
        for (int i = 0; i < heatmapEntities.Length; i++)
        {
            entityManager.SetComponentData(heatmapEntities[i], new heatmapIdentifier {closeBy = 0});
        }
    }

    private void countClose()
    {
        for (int i = 0; i < heatmapEntities.Length; i++)
        {
            Vector3 a = entityManager.GetComponentData<Translation>(heatmapEntities[i]).Value;
            for (int j = i; j < heatmapEntities.Length; j++)
            {
                Vector3 b = entityManager.GetComponentData<Translation>(heatmapEntities[j]).Value;
                if(Vector3.Distance(a,b)< distance){
                    heatmapIdentifier hIA =  entityManager.GetComponentData<heatmapIdentifier>(heatmapEntities[i]);
                    heatmapIdentifier hIB =  entityManager.GetComponentData<heatmapIdentifier>(heatmapEntities[j]);
                    hIA.closeBy += 1;
                    hIB.closeBy += 1;
                    entityManager.SetComponentData(heatmapEntities[i], hIA);
                    entityManager.SetComponentData(heatmapEntities[j], hIB);
                }
            }
        }
    }
    private void setTemperature()
    {
        //Using the close by objects of each entity, choose the temperature and alter the material accordingly
        for (int i = 1; i < heatmapEntities.Length; i++) //Skips the first, to avoid changing the baseline for others
        {
            int n = entityManager.GetComponentData<heatmapIdentifier>(heatmapEntities[i]).closeBy;

            switch (n)
            {
                case int c when (c >= 10):
                    setMaterial(heatmapEntities[i], mat_tmp3);
                    break;
                case int c when (c > 5 && c < 10):
                    setMaterial(heatmapEntities[i], mat_tmp2);
                    break;
                default:
                    setMaterial(heatmapEntities[i], mat_tmp1);
                    break;
            }
        }
    }

    private void setMaterial(Entity entity, Material material){
        RenderMesh r = entityManager.GetSharedComponentData<RenderMesh>(entity);
        r.material = material;
        entityManager.SetSharedComponentData(entity, r);
    }

    private void resetMaterial(){
        for (int i = 0; i < heatmapEntities.Length; i++)
        {
            setMaterial(heatmapEntities[i], mat_default);    
        }
    }
}
