

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

using heatmapDOTS;
using System;

public class heatmapGhost : MonoBehaviour
{
    [SerializeField] public bool EnableGhost = false;
    [SerializeField] private int originalFrequency = 40;

    [SerializeField] private float scale = 0.1f;
    //[SerializeField] public Material ghostMaterial;
    public List<Vector3> positions;
    private GameObject ghost;
    private bool last_EnableGhost;
    private bool background_update = true;
    private EntityManager entityManager;
    protected int indexPositions;

    

    void Start()
    {
        //Creates and sets the ghost indicator
        GameObject ghost_model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ghost = GameObject.Instantiate(ghost_model,Vector3.zero,Quaternion.identity, gameObject.transform); //as GameObject;
        GameObject.Destroy(ghost_model);
        ghost.transform.localScale = new Vector3(scale, scale, scale);
        ghost.SetActive(false);

        last_EnableGhost = false;
        indexPositions = 1;
        //Get the entity manager
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    
    
    void Update()
    {
        
        if(EnableGhost!=last_EnableGhost){
            last_EnableGhost = EnableGhost;
            
            ghost.SetActive(EnableGhost);
            
            if(EnableGhost) {
                getPositions();
            }
            else{//reset counters and list
            positions.Clear();
            indexPositions = 1;
            }
            
            
            
        }
        

        if (EnableGhost && background_update)
        {
            StartCoroutine(moveGhost());  
        }

    }

    public void getPositions(){
       
        Entity[] all_entities = entityManager.GetAllEntities(Unity.Collections.Allocator.Temp).ToArray();
        foreach (Entity entity in all_entities)
        {
            //Try-finally to get the heatmapidentifier and then the translation
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
            
            if(found) positions.Add(entityManager.GetComponentData<Translation>(entity).Value);
        }
    }

    private IEnumerator moveGhost(){
        background_update = false;
        
        if(indexPositions<positions.Count){  
            Vector3 pos = positions.GetRange(indexPositions, 1).ToArray()[0];
            ghost.transform.position = pos;
            //Debug.Log("Moved ghost to:"+pos.x+"; "+pos.y+"; "+pos.z+"; ");
        }
                
        indexPositions++;
        yield return new WaitForSecondsRealtime(1/(originalFrequency));
        background_update = true;

    }


}
