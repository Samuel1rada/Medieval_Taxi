using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PresetManager : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnLocations;
    [SerializeField] private List<GameObject> presets;

    private List<GameObject> activePresets = new List<GameObject>();

    private void Awake()
    {
        activePresets.Clear();
        SpawnAllPresets();
    }

    void SpawnAllPresets()
    {
        for (int i = 0; i < spawnLocations.Count; i++)
        {
            SpawnPresetAt(i);
        }
    }
    // spawn logic. spawn the preset and attaches the presetmonitor script to them
    void SpawnPresetAt(int index)
    {
        Transform spawnPos = spawnLocations[index];
        GameObject prefab = presets[Random.Range(0, presets.Count)];
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject obj = Instantiate(prefab, spawnPos.position, randomRotation);

        var monitor = obj.AddComponent<PresetMonitor>();
        monitor.manager = this;
        monitor.spawnIndex = index;

        
        if (index < activePresets.Count)
        {
            activePresets[index] = obj;  
        }
        else
        {
            activePresets.Add(obj);      
        }
    }

    public void RespawnAt(int index)
    {
        Debug.Log($"RespawnAt called for index {index}");

        if (index < activePresets.Count && activePresets[index] != null)
        {
            Destroy(activePresets[index]);
            activePresets[index] = null;
        }

        SpawnPresetAt(index);
    }

}