using UnityEngine;

public class Conditions : MonoBehaviour
{
    public bool isOnSlipperySurface;
    public float checkDistance = 1.5f;

    void Update()
    {
        isOnSlipperySurface = CheckIfSlippery();
    }

    bool CheckIfSlippery()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("Slippery"))
            {
                Debug.Log("On Slippery Surface: " + hit.collider.name);
                return true;
            }
        }
        return false;
    }
}
