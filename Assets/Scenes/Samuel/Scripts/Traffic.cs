using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class Traffic : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;

    private int destinationPoint;
    private int[] l;

}


/*    [SerializeField] private Transform[] waypoints;
    private NavMeshAgent agent;

    private int startWaypoint;
    private int targetWaypoint;
    private float[,] distanceMatrix;
    private float[] distance;
    private int[] previous;
    private bool[] visited;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startWaypoint = FindClosestWaypoint();
        targetWaypoint = 3;

        CalculateDistances();
        Pathfinding();
    }

    void CalculateDistances()
    {
        int numberOfWaypoints = waypoints.Length;
        distanceMatrix = new float[numberOfWaypoints, numberOfWaypoints];

        for (int i = 0; i < numberOfWaypoints; i++)
        {
            for (int j = 0; j < numberOfWaypoints; j++)
            {
                if (i != j)
                {
                    distanceMatrix[i, j] = Vector3.Distance(waypoints[i].position, waypoints[j].position);
                }
                else
                {
                    distanceMatrix[i, j] = Mathf.Infinity; 
                }
            }
        }
        for (int i = 0; i < waypoints.Length; i++)
        {
            for (int j = 0; j < waypoints.Length; j++)
            {
                Debug.Log($"Distance[{i},{j}] = {distanceMatrix[i, j]}");
            }
        }
    }

    void Pathfinding()
    {
        int numWaypoints = waypoints.Length;
        distance = new float[numWaypoints];
        previous = new int[numWaypoints];
        visited = new bool[numWaypoints];

        for (int i = 0; i < numWaypoints; i++)
        {
            distance[i] = Mathf.Infinity;
            previous[i] = -1;
        }
        distance[startWaypoint] = 0;

        for (int i = 0; i < numWaypoints; i++)
        {
            int currentWaypoint = -1;
            float smallestDistance = Mathf.Infinity;

            for (int j = 0; j < numWaypoints; j++)
            {
                if (!visited[j] && distance[j] < smallestDistance)
                {
                    smallestDistance = distance[j];
                    currentWaypoint = j;
                }
            }

            if (currentWaypoint == -1) break;

            visited[currentWaypoint] = true;

            for (int j = 0; j < numWaypoints; j++)
            {
                if (!visited[j] && distanceMatrix[currentWaypoint, j] > 0)
                {
                    float newDist = distance[currentWaypoint] + distanceMatrix[currentWaypoint, j];
                    if (newDist < distance[j])
                    {
                        distance[j] = newDist;
                        previous[j] = currentWaypoint;
                    }
                }
            }
        }
        List<int> path = new List<int>();
        int current = targetWaypoint;

        while (current != -1)
        {
            path.Insert(0, current);
            current = previous[current];
        }

        StartCoroutine(FollowPath(path));
        Debug.Log("Generated path: " + string.Join(" -> ", path));
    }
    IEnumerator FollowPath(List<int> path)
    {
        foreach (int waypointIndex in path)
        {
            Debug.Log("Moving to waypoint: " + waypointIndex);
            agent.SetDestination(waypoints[waypointIndex].position);
            agent.isStopped = false;
            Debug.Log("Agent moving to: " + waypoints[waypointIndex].position);
            while (agent.remainingDistance > 0.5f)
            {
                yield return null;
            }
        }
    }
    int FindClosestWaypoint()
    {
        int closestIndex = 0;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        return closestIndex;

    }*/
