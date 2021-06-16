using Tobii.G2OM;
using UnityEngine;


//Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
public class GazeTrigger : MonoBehaviour, IGazeFocusable
{
    [SerializeField] private float activationRadius = 5.0f;
    private bool _hasFocus;

    //Example: Spawn pill
    [SerializeField] private GameObject pill;

    private void Start()
    {
        _hasFocus = false;
        pill = GameObject.Instantiate(pill, gameObject.transform);
    }

    private void Update()
    {
        if (_hasFocus) //Trigger
        {
            //doSomething();
            spawnPill();
        }
        else
        {
            pill.SetActive(false);
        }
    }

    //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
    public void GazeFocusChanged(bool hasFocus)
    {
        //True if this object received focus
        _hasFocus = hasFocus;
        return;
    }


    private void spawnPill()
    {
        Vector3 objPosition = gameObject.transform.position;
        Vector3 playerPosition = Camera.main.transform.position;
        //Checking if inside radius
        if (Vector3.Distance(playerPosition, objPosition) < activationRadius)
        {
            pill.transform.position = objPosition + new Vector3(0,1,0);
            pill.SetActive(true);
        }
        return;
    }
}

