using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    private Animator animator;
    public Collider specificTrigger;
    public string targetTag = "Player";
    public bool DebugRagdoll = false; 
    public float DestroyAfter = 10f;
    [Tooltip("Randomized between min/max values")] 
    public float minImpactForce = 20f;
    public float maxImpactForce = 100f;
    [Range(0, 1)] public float UpwardsForceRatio = 1f;

    void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
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
        if (DebugRagdoll && Input.GetKeyDown(KeyCode.Space))
        {
            float randomForce = Random.Range(minImpactForce, maxImpactForce);
            EnableRagdoll(transform.position, (Vector3.up + Vector3.forward).normalized, randomForce);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Vector3 impactDirection = (transform.position - other.transform.position).normalized;
            impactDirection = AddUpwardForce(impactDirection);
            
            float randomForce = Random.Range(minImpactForce, maxImpactForce);
            EnableRagdoll(other.ClosestPoint(transform.position), impactDirection, randomForce);
        }
    }

    private Vector3 AddUpwardForce(Vector3 originalDirection)
    {
        return (originalDirection + Vector3.up * UpwardsForceRatio).normalized;
    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }

    private void EnableRagdoll(Vector3 impactPoint, Vector3 direction, float force)
    {
        if (animator != null)
        {
            animator.enabled = false; 
        }

        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
            
            float distance = Vector3.Distance(impactPoint, rigidbody.position);
            float forceMultiplier = 1f / (distance + 1f);

            // Apply main force
            rigidbody.AddForce(direction * force * forceMultiplier, ForceMode.Impulse);
            
            // Extra upward boost for core body parts
            if (rigidbody.transform == transform || rigidbody.transform.parent == transform)
            {
                rigidbody.AddForce(Vector3.up * force * 0.5f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject, DestroyAfter);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 impactDirection = (contact.normal + Vector3.up * UpwardsForceRatio).normalized;
            float randomForce = Random.Range(minImpactForce, maxImpactForce);
            EnableRagdoll(contact.point, -impactDirection, randomForce);
        }
    }
}