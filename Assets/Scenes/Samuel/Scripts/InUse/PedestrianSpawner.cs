using UnityEngine;
using System.Collections;

public class PedestrianSpawner : MonoBehaviour
{

    [SerializeField] private GameObject pedestrian;
    public int pedestrianAmount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Spawn()
    {
        int count = 0;
        while(count < pedestrianAmount)
        {
            GameObject obj = Instantiate(pedestrian);
            Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));
            obj.GetComponent<WaypintNavigator>().currenwaypoint = child.GetComponent<Waypoint>();
            obj.transform.position = child.position;

            yield return new WaitForEndOfFrame();

            count++;
        }
    }
}
