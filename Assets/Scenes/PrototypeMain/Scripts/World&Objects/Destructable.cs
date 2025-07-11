using UnityEngine;
using System.Collections;
using MalbersAnimations.Utilities;

public class Destructable : MonoBehaviour
{
    public GameObject brokenVersion;
    public float destroyDelay = 6f;
    public float explosionForce = 0f;
    public float speedThreshold = 5f;

    public AudioClip destroySound; //Add an audio clip for the destruction sound
    public float destroySoundVolume = 1f; //Volume for the destruction sound

    void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed > speedThreshold)
        {
            Vector3 hitPoint = collision.contacts[0].point;

            if (collision.gameObject.CompareTag("Animal") || collision.gameObject.CompareTag("Wagon"))
            {
                //Play the destruction sound if it exists
                if (destroySound != null)
                {
                    AudioSource.PlayClipAtPoint(destroySound, hitPoint, destroySoundVolume);
                }

                // Instantiate the broken version of the object
                GameObject brokenInstance = Instantiate(brokenVersion, transform.position, new Quaternion(transform.rotation.x, transform.rotation.y + 90, transform.rotation.z, transform.rotation.w));


                // Apply random forces to each piece of the broken object
                foreach (Rigidbody rb in brokenInstance.GetComponentsInChildren<Rigidbody>())
                {
                    Vector3 direction = (rb.transform.position - hitPoint).normalized;
                    rb.AddForce(direction * explosionForce / 70000, ForceMode.Impulse);
                }

                // Destroy the original object and the broken instance after a delay
                Destroy(gameObject);
                Destroy(brokenInstance, destroyDelay);
            }
        }
    }
}