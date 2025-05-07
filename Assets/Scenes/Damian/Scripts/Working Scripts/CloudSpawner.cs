using UnityEngine;
using System.Collections.Generic;

public class CloudSpawner : MonoBehaviour
{
    [Header("CLOUD PREFABS")]
    public List<GameObject> cloudPrefabs = new List<GameObject>();

    [Header("SPAWN SETTINGS")]
    [Tooltip("Set to 0 for instant spawning")]
    [Range(0, 5)] public float spawnInterval = 0.1f;
    [Tooltip("How many clouds spawn at once")]
    [Range(1, 20)] public int burstSpawnCount = 3;

    [Header("CLOUD SIZE")]
    [MinMaxSlider(0.1f, 5f)] public Vector2 widthRange = new Vector2(0.8f, 1.2f);
    [MinMaxSlider(0.1f, 5f)] public Vector2 lengthRange = new Vector2(0.8f, 1.2f);
    [MinMaxSlider(0.1f, 5f)] public Vector2 heightRange = new Vector2(0.8f, 1.2f);
    public bool uniformScaling = false;

    [Header("MOVEMENT")]
    public Vector3 moveDirection = Vector3.right;
    public float moveSpeed = 1f;
    public float cloudLifetime = 20f;

    [Header("SPAWN AREA")]
    public Vector3 spawnAreaSize = new Vector3(10f, 5f, 2f);

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            for (int i = 0; i < burstSpawnCount; i++)
            {
                SpawnCloud();
            }
            timer = 0f;
        }
    }

    void SpawnCloud()
    {
        if (cloudPrefabs.Count == 0) return;

        // Random selection and position
        GameObject cloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Count)];
        Vector3 spawnPosition = transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x/2f, spawnAreaSize.x/2f),
            Random.Range(-spawnAreaSize.y/2f, spawnAreaSize.y/2f),
            Random.Range(-spawnAreaSize.z/2f, spawnAreaSize.z/2f)
        );

        // Instantiate and apply random scale
        GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);
        
        Vector3 newScale = cloud.transform.localScale;
        if (uniformScaling)
        {
            float uniformScale = Random.Range(widthRange.x, widthRange.y);
            newScale *= uniformScale;
        }
        else
        {
            newScale.x *= Random.Range(widthRange.x, widthRange.y);
            newScale.y *= Random.Range(heightRange.x, heightRange.y);
            newScale.z *= Random.Range(lengthRange.x, lengthRange.y);
        }
        cloud.transform.localScale = newScale;

        // Add movement
        CloudMover mover = cloud.AddComponent<CloudMover>();
        mover.moveDirection = moveDirection;
        mover.moveSpeed = moveSpeed;
        mover.cloudLifetime = cloudLifetime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}

// Add this to Editor folder for MinMaxSlider
#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        var attr = attribute as MinMaxSliderAttribute;
        if (property.propertyType == UnityEditor.SerializedPropertyType.Vector2)
        {
            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            position = UnityEditor.EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            var min = property.vector2Value.x;
            var max = property.vector2Value.y;
            
            UnityEditor.EditorGUI.MinMaxSlider(position, ref min, ref max, attr.min, attr.max);
            property.vector2Value = new Vector2(min, max);
                
            UnityEditor.EditorGUI.EndProperty();
        }
    }
}

public class MinMaxSliderAttribute : PropertyAttribute
{
    public float min;
    public float max;
    
    public MinMaxSliderAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
#endif