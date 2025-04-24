using UnityEngine;
using System.Collections;
using MalbersAnimations.Utilities;

public class EffectDestructable : MonoBehaviour
{

    public GameObject effects;
    public GameObject spawnPos;
    public float destroyEffect = 6f;



    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Animal") || collision.gameObject.CompareTag("Wagon"))
        {
            // Instantiate the broken version of the object
            GameObject InsideEffect = Instantiate(effects, spawnPos.transform.position, new Quaternion(0, 90, 0 , transform.rotation.w));

            // Destroy the original object and the broken instance after a delay
            Destroy(gameObject);
         
            Destroy(InsideEffect, destroyEffect);
        }
    }
}