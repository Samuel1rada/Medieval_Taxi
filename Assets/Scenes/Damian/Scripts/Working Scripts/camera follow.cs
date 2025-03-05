using UnityEngine;

public class camerafollow : MonoBehaviour
{
    public Transform player;
    public Vector3 positionOffset = new Vector3(0, 2, -4); // Camera position offset
    public Vector3 rotationOffset = new Vector3(0, 15, 0); // Rotation offset
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;
    public float maxXRotation = 80f; // Prevent upside-down movement
    public float maxYRotation = 80f; // Prevent upside-down movement


    void Update()
    {
        if (player == null) return;

        // Calculate target position
        Vector3 targetPosition = player.position + player.rotation * positionOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Calculate target rotation with offset
        Quaternion targetRotation = player.rotation * Quaternion.Euler(rotationOffset);
        Vector3 eulerAngles = targetRotation.eulerAngles;

        // Prevent upside-down rotation by clamping X angle (Pitch)
        if (eulerAngles.x > 180) eulerAngles.x -= 360; // Convert to -180 to 180 range
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, -maxXRotation, maxXRotation);
        
        if (eulerAngles.y > 180) eulerAngles.y -= 360; // Convert to -180 to 180 range
        eulerAngles.y = Mathf.Clamp(eulerAngles.y, -maxYRotation, maxYRotation);


        // Apply clamped rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(eulerAngles), rotationSpeed * Time.deltaTime);
    }

}



