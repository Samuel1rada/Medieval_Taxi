using UnityEngine;

public class GateManager : MonoBehaviour
{
    public Transform gateTransform;
    public Transform openPos;
    public float liftSpeed;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Vector3 posOpen;

    private bool playerInside = false;

    void Start()
    {
        if (gateTransform == null)
        {
            gateTransform = transform; 
        }
            

        initialPosition = gateTransform.position;
        posOpen = openPos.transform.position;
        targetPosition = initialPosition;
    }

    void Update()
    {
        Debug.DrawLine(gateTransform.position, targetPosition);
        gateTransform.position = Vector3.MoveTowards(gateTransform.position, targetPosition, liftSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            targetPosition = posOpen;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            targetPosition = initialPosition;
        }
    }
}
