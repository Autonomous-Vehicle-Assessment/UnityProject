using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathLoader))]
public class PathLoaderEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathLoader pathLoader = (PathLoader)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load Waypoints"))
        {
            pathLoader.OpenDialog();
            pathLoader.ReadTextFile();
        }

        if (GUILayout.Button("Clear"))
        {
            pathLoader.ClearPath();
        }

        if (GUILayout.Button("Save Path"))
        {
            pathLoader.OpenDialogSave();
            pathLoader.WriteTextFile();
        }

        EditorGUILayout.EndHorizontal();

    }
}
