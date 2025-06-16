using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public Transform playerTransform;
    public Transform npcTransform;
    public float npcFOV = 40f;
    public float playerFOV = 60f;

    // Call this when the animation starts
    public void OnAnimationStart()
    {
        virtualCamera.Follow = npcTransform;
        virtualCamera.m_Lens.FieldOfView = npcFOV;
    }

    // Call this when the animation ends
    public void OnAnimationEnd()
    {
        virtualCamera.Follow = playerTransform;
        virtualCamera.m_Lens.FieldOfView = playerFOV;
    }
}