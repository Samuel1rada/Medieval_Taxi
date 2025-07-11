using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PresetMonitor : MonoBehaviour
{
    [HideInInspector] public PresetManager manager; 
    /*[HideInInspector]*/ public int spawnIndex;

    private List<Transform> children = new List<Transform>();
    private bool respawning = false;

    void Start()
    {
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
    }

    void Update()
    {
        if(!respawning && AnyChildDestroyed())
        {
            Debug.Log($"[{name}] All children destroyed, starting respawn coroutine.");
            StartCoroutine(HandleRespawn());
        }
    }

    //checks if any child is destroyed of the object this script is attached to is destroyed. 
    bool AnyChildDestroyed()
    {
        foreach (var child in children)
        {
            if (child == null) 
            {
                return true;
            }
        }
        return false;
    }
    // handels respanwing logic and condition. 
    // condition 1: waits for a set amount of time before it initates respawn
    // condition 2: checks if the camera it not looking at the preset, if it is dont yet respawn
    IEnumerator HandleRespawn()
    {
        respawning = true;

        yield return new WaitForSeconds(15f);

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            //Debug.LogWarning("No main camera found!");
            yield break;
        }

        while (IsCameraLookingAtPreset(mainCam))
        {
            float lookAwayTime = 0f;
            while (IsCameraLookingAtPreset(mainCam) == false)
            {
                lookAwayTime += Time.deltaTime;
                if (lookAwayTime >= 3f)
                {
                    break;
                }
                yield return null;
            }

            if (lookAwayTime < 1f)
            {
                yield return null; 
            }
            else
            {
                break;
            }
        }

        manager.RespawnAt(spawnIndex);
        Destroy(gameObject);
    }

    // Helper function to check if camera is looking at the preset
    bool IsCameraLookingAtPreset(Camera cam)
    {
        Vector3 toPreset = (transform.position - cam.transform.position).normalized;
        float angle = Vector3.Angle(cam.transform.forward, toPreset);

        float lookThresholdAngle = 90f;
        return angle < lookThresholdAngle;
    }
}
