using UnityEngine;
using System.Collections;

public class Destructable : MonoBehaviour
{
    public GameObject BrokenVersion;
    public float destroyDelay = 5f; 

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Animal") || collision.gameObject.CompareTag("Wagon"))
        {
            GameObject brokenInstance = Instantiate(BrokenVersion, transform.position, new Quaternion(transform.rotation.x, transform.rotation.y + 90, transform.rotation.z, transform.rotation.w));
            Destroy(gameObject); 
            Destroy(brokenInstance, destroyDelay);
        }
    }
}