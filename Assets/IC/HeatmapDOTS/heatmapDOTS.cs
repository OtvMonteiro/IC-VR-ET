using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;



//[assembly: RegisterGenericComponentType(typeof(heatmapDOTS.heatmapIdentifier<bool>))]

namespace heatmapDOTS{
public class heatmapDOTS : MonoBehaviour
{
   
   [SerializeField] public bool enableHeatmap = true;
   [SerializeField] public int pointF_Hz = 10;
   [SerializeField] public bool populate = false;
    public GameObject heatmapCubePrefab; //Cube prefab (instanciating heatmap Elements via GameObject conversion)
    private bool background_update = true;
    private Entity heatmapCube;
    private EntityManager entityManager;
    private Vector3 currentPoint, lastPoint;

    

    //[SerializeField] public float heatmapPointsScale = 1.0f;
    //[HideInInspector] public float last_heatmapPointsScale;
    
    //Heatmap Scale (for system)
    [SerializeField] public float update_heatmap_scale = 0.1f;

    private static float heatmapPointsScale;
    public static float HeatmapPointsScale
    {
        get { return heatmapPointsScale; }
        set { 
                //Debug.Log("Updating the scale (try): "+value);
                if(value>0.0f) heatmapPointsScale = value; 
            }
    }
    private static float last_heatmapPointsScale;
    public static float Last_HeatmapPointsScale
    {
        get { return last_heatmapPointsScale; }
        set { 
                if(value>0.0f) last_heatmapPointsScale = value; 
            }
    }


    //Heatmap Trail Mode (for system)
    [SerializeField] public bool EnableTrailMode = false;
    private static bool trailMode, last_trailMode;
        public static bool TrailMode { get => trailMode; set => trailMode = value; }
        public static bool Last_TrailMode { get => last_trailMode; set => last_trailMode = value; }




        void Start()
    {
        //Gets entity manager and settings
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        //Create Entity based on GameObject prefab
        heatmapCube = GameObjectConversionUtility.ConvertGameObjectHierarchy(heatmapCubePrefab, settings);
        //Add identifier component to facilitate identification by Systems
        entityManager.AddComponent<heatmapIdentifier>(heatmapCube);
        //Setting starting points in the world space
        currentPoint = lastPoint = Vector3.zero;

        //Updating starting values
        HeatmapPointsScale = update_heatmap_scale;
        Last_HeatmapPointsScale = HeatmapPointsScale;
        Last_TrailMode = TrailMode = EnableTrailMode;
    }

    
    void Update()
    {
        if(enableHeatmap && background_update){
          //Captures point (simple, dwell or wait)
            // //Simple point capture with normal camera
            // currentPoint = capturePoint(Camera.main);
            //Dwell   
            StartCoroutine(dwellPoints());
            
            //Calls method to instanciate Entities
            if(currentPoint != lastPoint) {createEntities(currentPoint);if(populate){Populate(currentPoint);}}

            //Properties update
            HeatmapPointsScale = update_heatmap_scale;  //Update scale property
            TrailMode = EnableTrailMode; //Update trail mode property

        }

    }

    private Vector3 capturePoint(Camera cam)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit worldPoint;
        //Raycasthit with 100f depth
        if (Physics.Raycast(ray, out worldPoint, 100.0f)) 
        {
            return worldPoint.point;
        }
        //Hit nothing: returns last point to receive treatment
        else return lastPoint;
    }

    private void createEntities(Vector3 point){
        //Instanciate from entity (converted prefab)
        var instance = entityManager.Instantiate(heatmapCube);
        entityManager.SetComponentData(instance, new Translation {Value = point});
        entityManager.SetComponentData(instance, new NonUniformScale {Value = new Unity.Mathematics.float3(HeatmapPointsScale)});
    }

    private void Populate (Vector3 point){
        for (int i = 0; i < 10; i++)
        {
            Vector3 pop_point = point + new Vector3(Random.value*HeatmapPointsScale*5,
                                        Random.value*HeatmapPointsScale*5, Random.value*HeatmapPointsScale*5);
            createEntities(pop_point);
        }
    }

    public IEnumerator dwellPoints()
    {
        //Stops updates for this time
        background_update = false;

        Vector3[] dwell_points = new Vector3[10];
        Vector3 centerOfMass = new Vector3(0,0,0);
        
        for(int n=0; n<10; n++)
        {
            //A method of capture could be chosen
            dwell_points[n] = capturePoint(Camera.main);
            centerOfMass += dwell_points[n];
            
            //Waits the specified time for new capture
            yield return new WaitForSecondsRealtime(1/(pointF_Hz*10));
        }

        centerOfMass = centerOfMass/10;
        bool closeEnough = true;
        
        
        //Verify if the points are close enough to register
        for(int n=0; n<10 && closeEnough ; n++)
        {
            if(Vector3.Distance(centerOfMass, dwell_points[n]) < 1/HeatmapPointsScale){}
            else closeEnough = false;
        }

        if(closeEnough) currentPoint = centerOfMass;
        else            currentPoint = lastPoint;
    
        //Restarts the Update method
        background_update = true;

    }

}

}
