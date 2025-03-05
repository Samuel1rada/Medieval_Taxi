using UnityEngine;

public class RagdollTest : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    public Collider specificTrigger; // Assign the specific collider in the inspector
    public string targetTag = "Player"; // The tag of the object that triggers the ragdoll, assignable in the Inspector

    void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();
    }

    private void Start()
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnableRagdoll();
        }
    }

    // OnTriggerEnter is fired when something enters the trigger area
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the correct tag
        if (other.CompareTag(targetTag))  // Check if the object has the correct tag
        {
            Debug.Log($"OnTriggerEnter: {other.name} has the correct tag, enabling ragdoll.");
            EnableRagdoll();
        }
        // else
        // {
        //     Debug.Log($"OnTriggerEnter: {other.name} does not have the correct tag.");
        // }
    }

    // OnTriggerExit is fired when something exits the trigger area
    // private void OnTriggerExit(Collider other)
    // {
    //     // Check if the object exiting the trigger has the correct tag
    //     if (other.CompareTag(targetTag))  // Check if the object has the correct tag
    //     {
    //         Debug.Log($"OnTriggerExit: {other.name} has the correct tag, disabling ragdoll.");
    //         DisableRagdoll();
    //     }
    //     else
    //     {
    //         Debug.Log($"OnTriggerExit: {other.name} does not have the correct tag.");
    //     }
    // }

    private void DisableRagdoll()
    {
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }
}
