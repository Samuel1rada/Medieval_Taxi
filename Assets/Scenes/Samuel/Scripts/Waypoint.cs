using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
    [SerializeField] public Waypoint previousWaypoint;
    [SerializeField] public Waypoint nextWaypoint;

    [Range(1f, 5f)]
    public float width = 1f;

    public List<Waypoint> branches = new List<Waypoint>();

    [Range(0f, 1f)]
    public float branchRatio = 0.5f;

    public Vector3 Getposition()
    {
        Vector3 minBound = transform.position - transform.right * width / 2f;
        Vector3 maxBound = transform.position + transform.right * width / 2f;

        return Vector3.Lerp(minBound, maxBound, Random.Range(0f,1f)); 
    }
}
