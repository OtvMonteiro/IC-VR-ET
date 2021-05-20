using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class heatmapGhost : MonoBehaviour
{
    [SerializeField] public bool EnableGhost = false;
    [SerializeField] public Material ghostMaterial;
    [SerializeField] private int originalFrequency = 40;

    [SerializeField] private float scale = 0.1f;
    private GameObject ghost;
    private bool background_update = true;

    private EntityManager entityManager;

    

    void Start()
    {
        ghost = GameObject.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere)) as GameObject;
        ghost.transform.localScale = new Vector3(scale, scale, scale);
        ghost.SetActive(false);
    }

    
    
    void Update()
    {
        ghost.SetActive(EnableGhost);
        
        
        if (EnableGhost && background_update)
        {
            ghost.SetActive(true);
            Debug.Log("Trying to move ghost");
            moveGhost();
            
        }

    }

    public IEnumerator moveGhost(){
        background_update = false;

        
        Entity[] aux_entities = entityManager.GetAllEntities().ToArray();
        for(int n=2; n<aux_entities.Length ; n++)
        {
            float3 translation = entityManager.GetComponentData<Translation>(aux_entities[n]).Value;
            Debug.Log("Atualizando a posição do ghost: "+translation.x+"; "+translation.y+"; "+translation.z+"; ");
            ghost.transform.position = new Vector3(translation.x, translation.y, translation.z);

            yield return new WaitForSecondsRealtime(1/(originalFrequency));


            //yield return new WaitForSecondsRealtime(1/(originalFrequency*2));


        }


        background_update = true;
    }


}
