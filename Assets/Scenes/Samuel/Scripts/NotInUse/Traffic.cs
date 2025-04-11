using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class Traffic : MonoBehaviour
{
   /* [SerializeField] private Transform[] waypoints;
    [SerializeField] public NavMeshAgent agent = null;
    private int destinationPoint;
    private int nextwaypoint;
    private bool[] visited;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visited = new bool[waypoints.Length];
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (nextwaypoint >= 0 && nextwaypoint < visited.Length)
            {
                visited[nextwaypoint] = true;
            }

            FindStartWaypoint();
            FindNextWaypoint();

            if (AllWaypointsVisited())
            {
                ResetVisitedWaypoints();
                Debug.Log("All waypoints have been visited. Resetting visited array.");
            }
        }
    }

    private Transform FindStartWaypoint()
    {
        Transform closestWaypoint = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(agent.transform.position, waypoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestWaypoint = waypoints[i];
            }
        }
        Debug.Log("Start waypoint is " + closestWaypoint);
        return closestWaypoint;
    }

    private Transform FindNextWaypoint()
    {
        Transform nextWaypoint = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (visited[i]) continue; // Skip already visited waypoints

            float distance = Vector3.Distance(agent.transform.position, waypoints[i].position);
            if (distance > 2 && distance < minDistance)
            {
                minDistance = distance;
                nextWaypoint = waypoints[i];
                nextwaypoint = i;
            }
        }

        if (nextWaypoint != null)
        {
            agent.SetDestination(nextWaypoint.position);
        }
        return nextWaypoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(agent.transform.position, 5);
    }

    private bool AllWaypointsVisited()
    {
        foreach (bool visitedWaypoint in visited)
        {
            if (!visitedWaypoint)
            {
                return false;
            }
        }
        return true;
    }
    private void ResetVisitedWaypoints()
    {
        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = false;
        }
    }
*/
}






































