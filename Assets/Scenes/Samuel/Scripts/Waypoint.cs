using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] public Waypoint previousWaypoint;
    [SerializeField] public Waypoint nextWaypoint;

    [Range(0f, 10f)]
    public float width = 10f;

    public Vector3 Getposition()
    {
        Vector3 minBound = transform.position + transform.right * width / 2f;
        Vector3 maxBound = transform.position - transform.right * width / 2f;

        return Vector3.Lerp(minBound, maxBound, Random.Range(0f,10f)); 

    }
}
