using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Tooltip("Waypoints in order to follow")]
    public List<Transform> waypoints = new List<Transform>();

    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Count)
            return null;

        return waypoints[index];
    }

    public int Count => waypoints.Count;
}
