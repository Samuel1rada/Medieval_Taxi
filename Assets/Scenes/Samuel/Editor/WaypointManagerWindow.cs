using UnityEditor;
using UnityEngine;

public class WaypointManagerWindow : EditorWindow
{
    [MenuItem("Tool/Waypoint Editor")]

    public static void Open()
    {
        GetWindow<WaypointManagerWindow>();
    }

    public Transform waypointRoot;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty("waypointRoot"));

        if(waypointRoot == null) 
        {
            EditorGUILayout.HelpBox(" Root transfrom must be selected. Please assign a root transfrom", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginVertical("Box");
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        obj.ApplyModifiedProperties();
    }

    void DrawButtons()
    {
        if(GUILayout.Button("Create Waypoint"))
        {
            CreateWaypoint();
        }
    }

    void CreateWaypoint()
    {
        GameObject waypointObj = new GameObject(" Waypoint " + waypointRoot.childCount, typeof(Waypoint));
        waypointObj.transform.SetParent(waypointRoot, false);

        Waypoint waypoint = waypointObj.GetComponent<Waypoint>();
        if(waypointRoot.childCount > 1)
        {
            waypoint.previousWaypoint = waypointRoot.GetChild(waypointRoot.childCount - 2).GetComponent<Waypoint>();
            waypoint.previousWaypoint.nextWaypoint = waypoint;

            waypoint.transform.position = waypoint.previousWaypoint.transform.position;
            waypoint.transform.forward = waypoint.previousWaypoint.transform.forward;
        }

        Selection.activeGameObject = waypoint.gameObject;

    }
}
