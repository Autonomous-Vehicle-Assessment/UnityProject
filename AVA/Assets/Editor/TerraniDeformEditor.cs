using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainDeform))]
public class TerrainDeformEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainDeform terrainDeform = (TerrainDeform)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load and wrap terrain"))
        {
            terrainDeform.LoadTerrain();
            terrainDeform.WrapTerrain();
            //terrainDeform.OpenDialog();
            //terrainDeform.ReadTextFile();
            //terrainDeform.GeneratePath();
        }

        if (GUILayout.Button("Reset"))
        {
            terrainDeform.ResetTerrain();
        }

        if (GUILayout.Button("Init"))
        {
            terrainDeform.InitTerrain();
        }

        EditorGUILayout.EndHorizontal();

    }
}
