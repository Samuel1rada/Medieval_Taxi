using UnityEngine;
using UnityEngine.AI;

public class WaypintNavigator : MonoBehaviour
{
    NavMeshAgent agent;
    public Waypoint currenwaypoint;
    private bool reachedDestination = false;

    int direction;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

       
    }


    void Start()
    {
        direction = Mathf.RoundToInt(Random.Range(0f, 1f));
        agent.SetDestination(currenwaypoint.Getposition());

        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.Warp(hit.position); // warp ensures full grounding
        }
        else
        {
            Debug.LogError($"{gameObject.name} is not on the NavMesh!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(agent.remainingDistance < 2f)
        {
            reachedDestination = true;
            if(reachedDestination == true) 
            {
                bool shouldbranch = false;

                if(currenwaypoint.branches != null && currenwaypoint.branches.Count > 0) 
                {
                    shouldbranch = Random.Range(0f,1f) <= currenwaypoint.branchRatio ? true : false;
                }
                if(shouldbranch)
                {
                    currenwaypoint = currenwaypoint.branches[Random.Range(0, currenwaypoint.branches.Count - 1)];   
                }
                else
                {
                    if (direction == 0)
                    {
                        if(currenwaypoint.nextWaypoint != null)
                        {
                            currenwaypoint = currenwaypoint.nextWaypoint;
                        }
                        else
                        {
                            currenwaypoint = currenwaypoint.previousWaypoint;
                            direction = 1;
                        }
                    }
                    else if (direction == 1)
                    {
                        if(currenwaypoint.previousWaypoint != null)
                        {
                            currenwaypoint = currenwaypoint.previousWaypoint;
                        }
                        else
                        {
                            currenwaypoint = currenwaypoint.nextWaypoint;
                            direction = 0;
                        }
                    }
                }

                agent.SetDestination(currenwaypoint.Getposition());
            }
        }
    }
}
