using UnityEngine;

public class AntiFlip : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 currentEuler = transform.eulerAngles;

        // X axis
        float x = currentEuler.x > 180f ? currentEuler.x - 360f : currentEuler.x;
        x = Mathf.Clamp(x, -30f, 30f);
        currentEuler.x = x < 0 ? x + 360f : x;

        // Z axis
        float z = currentEuler.z > 180f ? currentEuler.z - 360f : currentEuler.z;
        z = Mathf.Clamp(z, -45f, 45f);
        currentEuler.z = z < 0 ? z + 360f : z;

        transform.eulerAngles = currentEuler;
    }
}
