using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
public class AIPathLoader : MonoBehaviour
{
    private StreamReader fileReader;
    private string path;
    
    private List<string> stringList;
    private List<string[]> parsedList;
    
    private List<Vector3> pathPoints;
    private List<float> pathVel;

    public void ReadTextFile()
    {
        if (path != null)
        {
            fileReader = new StreamReader(path);
            stringList = new List<string>();

            while (!fileReader.EndOfStream)
            {
                string inp_ln = fileReader.ReadLine();

                stringList.Add(inp_ln);
            }

            fileReader.Close();

            ParseList();
            DeConstructor();
        }
    }

    private void ParseList()
    {
        parsedList = new List<string[]>();

        for (int i = 0; i < stringList.Count; i++)
        {
            string[] temp = stringList[i].Split(',');
            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = temp[j].Trim();  //removed the blank spaces
            }
            parsedList.Add(temp);
        }
    }

    private void DeConstructor()
    {
        pathPoints = new List<Vector3>();
        pathVel = new List<float>();

        for (int row = 1; row < parsedList.Count; row++)
        {
            float posX = float.Parse(parsedList[row][0]);
            float posZ = float.Parse(parsedList[row][1]);
            float vel = float.Parse(parsedList[row][2]);

            Vector3 transform = new Vector3(posX, 100, posZ);
            
            pathPoints.Add(transform);
            pathVel.Add(vel);
        }
    }

    public void GeneratePath()
    {
        for (int i = transform.childCount; i > 0; --i)
            DestroyImmediate(transform.GetChild(0).gameObject);

        for (int point = 0; point < pathPoints.Count; point++)
        {
            GameObject _point = new GameObject($"Point ({point})");
            
            _point.transform.parent = transform;

            _point.transform.position = transform.TransformPoint(pathPoints[point]);

            _point.AddComponent<PathNode>();

            _point.GetComponent<PathNode>().targetVelocity = pathVel[point];
            _point.GetComponent<PathNode>().targetHeight = 2;
            _point.GetComponent<PathNode>().SetHeight();
        }
    }

    public void OpenDialog()
    {
        path = EditorUtility.OpenFilePanel(
                    "Open file",
                    "",
                    "*");
    }

    public void ClearPath()
    {
        for (int i = transform.childCount; i > 0; --i)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }
}
