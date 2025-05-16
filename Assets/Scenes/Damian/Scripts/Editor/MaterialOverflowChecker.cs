using UnityEditor;
using UnityEngine;

public class MaterialOverflowChecker : EditorWindow
{
    [MenuItem("Tools/Check Material Overflow")]
    public static void ShowWindow()
    {
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.sharedMaterials.Length > 128)
            {
                Debug.LogError(
                    $"[Material Overflow] GameObject '{renderer.gameObject.name}' has {renderer.sharedMaterials.Length} materials!",
                    renderer.gameObject
                );
            }
        }

        Debug.Log("Material Overflow Check Completed.");
    }
}
