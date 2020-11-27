using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIPathLoader))]
public class PathLoaderEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AIPathLoader pathLoader = (AIPathLoader)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load Waypoints"))
        {
            pathLoader.OpenDialog();
            pathLoader.ReadTextFile();
            pathLoader.GeneratePath();
        }

        if (GUILayout.Button("Clear"))
        {
            pathLoader.ClearPath();
        }

        //if(GUILayout.Button("Update Speeds"))
        //{
        //    pathLoader.UpdateSpeeds();
        //}

        EditorGUILayout.EndHorizontal();

    }
}
