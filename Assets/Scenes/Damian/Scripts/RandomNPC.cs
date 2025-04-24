using UnityEngine;

public class RandomNPCSpawner : MonoBehaviour
{
    [Header("NPC Models")]
    public GameObject[] npcPrefabs; // Drag all your NPC model prefabs here

    [Header("Color Materials")]
    public Material[] colorMaterials; // Drag all your color materials here

    [Header("Spawn Settings")]
    public Transform spawnPoint; // Optional: set a spawn point
    public bool randomPosition = false; // If true, spawn randomly around spawnPoint

    public void SpawnRandomNPC()
    {
        if (npcPrefabs.Length == 0 || colorMaterials.Length == 0)
        {
            Debug.LogWarning("Please assign NPC prefabs and materials!");
            return;
        }

        // Pick random prefab and material
        GameObject chosenPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
        Material chosenMaterial = colorMaterials[Random.Range(0, colorMaterials.Length)];

        // Determine spawn position
        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        if (randomPosition)
        {
            position += new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        }

        // Instantiate NPC
        GameObject npc = Instantiate(chosenPrefab, position, Quaternion.identity);

        // Apply random color material to all MeshRenderers
        MeshRenderer[] renderers = npc.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material = chosenMaterial;
        }
    }

    // Optional: Spawn on start
    void Start()
    {
        SpawnRandomNPC();
    }
}
