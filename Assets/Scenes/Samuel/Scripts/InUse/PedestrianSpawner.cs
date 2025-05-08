using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PedestrianSpawner : MonoBehaviour
{

    [SerializeField] private List<GameObject> pedestrian = new List<GameObject>();
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
        while (count < pedestrianAmount)
        {
            // Pick a random child waypoint
            Transform child = transform.GetChild(Random.Range(0, transform.childCount));

            // Try to find nearest point on NavMesh within 1 unit
            NavMeshHit hit;
            if (NavMesh.SamplePosition(child.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                // Instantiate at a valid NavMesh position
                GameObject obj = Instantiate(pedestrian[Random.Range(0, pedestrian.Count)], hit.position, Quaternion.identity);
                obj.GetComponent<WaypintNavigator>().currenwaypoint = child.GetComponent<Waypoint>();

                count++; // Only count successful spawns
            }
            else
            {
                Debug.LogWarning("Could not find NavMesh near waypoint: " + child.name);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
