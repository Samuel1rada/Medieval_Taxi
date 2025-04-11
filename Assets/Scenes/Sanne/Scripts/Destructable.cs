using UnityEngine;
using System.Collections;

public class Destructable : MonoBehaviour
{
    public GameObject BrokenVersion;
    public float destroyDelay = 6f;
    public float explosionForce = 0.00008f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Animal") || collision.gameObject.CompareTag("Wagon"))
        {
            // Instantiate the broken version of the object
            GameObject brokenInstance = Instantiate(BrokenVersion, transform.position, new Quaternion(transform.rotation.x, transform.rotation.y + 90, transform.rotation.z, transform.rotation.w));

            // Apply random forces to each piece of the broken object
            foreach (Rigidbody rb in brokenInstance.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 randomDirection = Random.insideUnitSphere;
                rb.AddForce(randomDirection * explosionForce);
            }

            // Destroy the original object and the broken instance after a delay
            Destroy(gameObject);
            Destroy(brokenInstance, destroyDelay);
        }
    }
}