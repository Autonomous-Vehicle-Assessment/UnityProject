using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum WaypointFormat
{
    Simple,
    Custom,
    KRC
}

[ExecuteInEditMode]
public class AIPathLoader : MonoBehaviour
{
    public SpeedType speedType;
    public WaypointFormat waypointFormat;

    private StreamReader fileReader;
    private string path;
    
    private List<string> stringList;
    private List<string[]> parsedList;

    private string pathDescription;
    private Color pathColor;
    private List<Vector3> pathPoints;
    private List<float> pathVel;

    public void ReadTextFile()
    {
        if (path != null && path.Length > 0)
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

            ConstructPath();
        }
    }

    /// <summary>
    /// Parse read list based on waypoint format (delimiter) adding rows to the parsedList.
    /// </summary>
    private void ParseList()
    {
        switch (waypointFormat)
        {
            case WaypointFormat.Simple:
                ParseListSimple();
                break;

            case WaypointFormat.Custom:
                ParseListCustom();
                break;

            case WaypointFormat.KRC:
                ParseListKRC();
                break;

        }
    }

    private void ParseListSimple()
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

    private void ParseListCustom()
    {
        parsedList = new List<string[]>();

        for (int i = 0; i < stringList.Count; i++)
        {
            string[] temp = stringList[i].Split(';');
            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = temp[j].Trim();  //removed the blank spaces
            }

            // Characters from start and end of line
            temp[0] = ""; temp[temp.Length] = "";

            parsedList.Add(temp);
        }
    }

    private void ParseListKRC()
    {
        parsedList = new List<string[]>();

        for (int i = 0; i < stringList.Count; i++)
        {
            string[] temp = stringList[i].Split(';');
            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = temp[j].Trim();  //removed the blank spaces
                temp[j] = temp[j].Trim('\"');
            }

            // Characters from start and end of line
            

            parsedList.Add(temp);
        }
    }

    private void ConstructPath()
    {
        switch (waypointFormat)
        {
            case WaypointFormat.Simple:
                ConstructPathSimple();
                break;

            case WaypointFormat.Custom:
                ConstructPathCustom();
                break;

            case WaypointFormat.KRC:
                ConstructPathKRC();
                break;

        }
    }

    private void ConstructPathSimple()
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

    private void ConstructPathCustom()
    {
        ClearPath();

        for (int row = 1; row < parsedList.Count; row++)
        {
            pathPoints = new List<Vector3>();
            pathVel = new List<float>();

            pathDescription = parsedList[row][0];
            float colorR = float.Parse(parsedList[row][1]);
            float colorG = float.Parse(parsedList[row][2]);
            float colorB = float.Parse(parsedList[row][3]);
            pathColor = new Color(colorR, colorG, colorB);

            string[] xStr = parsedList[row][4].Split(',');
            string[] yStr = parsedList[row][5].Split(',');
            string[] velStrs = parsedList[row][6].Split(',');

            for (int point = 0; point < xStr.Length; point++)
            {
                Vector3 position = ConvertXY(xStr[point], yStr[point]);

                System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                string velStr = velStrs[point];

                velStr = velStr.Replace('.', ',');

                float vel = float.Parse(velStr, numberStyle);

                pathPoints.Add(position);
                pathVel.Add(vel);
            }

            GeneratePath();
        }
    }

    private void ConstructPathKRC()
    {
        ClearPath();

        for (int row = 1; row < parsedList.Count; row++)
        {
            pathPoints = new List<Vector3>();
            pathVel = new List<float>();

            pathDescription = parsedList[row][0];
            float colorR = float.Parse(parsedList[row][1]);
            float colorG = float.Parse(parsedList[row][2]);
            float colorB = float.Parse(parsedList[row][3]);
            pathColor = new Color(colorR, colorG, colorB);

            string[] latStr = parsedList[row][4].Split(',');
            string[] lonStr = parsedList[row][5].Split(',');
            // string[] velStrs = parsedList[row][6].Split(',');


            for (int point = 0; point < lonStr.Length; point++)
            {
                Vector3 position = ConvertLatLon(latStr[point], lonStr[point]);

                //System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                //string velStr = velStrs[point];

                //velStr = velStr.Replace('.', ',');

                //float vel = float.Parse(velStr, numberStyle);

                float vel = 15; // <--- Get velocities

                pathPoints.Add(position);
                pathVel.Add(vel);
            }

            GeneratePath();
        }
    }


    public void GeneratePath()
    {
        GameObject path = new GameObject(pathDescription);
        path.transform.parent = transform;
        path.AddComponent<AIPath>();
        path.GetComponent<AIPath>().lineColor = pathColor;

        for (int point = 0; point < pathPoints.Count; point++)
        {
            GameObject _point = new GameObject($"Point ({point})");
            
            _point.transform.parent = path.transform;

            _point.transform.position = transform.TransformPoint(pathPoints[point]);

            _point.AddComponent<PathNode>();

            _point.GetComponent<PathNode>().targetVelocity = pathVel[point];
            _point.GetComponent<PathNode>().targetHeight = 2;
            _point.GetComponent<PathNode>().speedType = speedType;
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

    public void UpdateSpeeds()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<PathNode>().RecalculateSpeed();

    }

    public Vector3 ConvertXY(string xPosStr, string yPosStr)
    {
        System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
        xPosStr = xPosStr.Replace('.', ',');
        yPosStr = yPosStr.Replace('.', ',');

        float xPos = float.Parse(xPosStr, numberStyle);
        float yPos = float.Parse(yPosStr, numberStyle);

        float x = (float)(73231.60681f * xPos + 6480662.43837f);
        float z = (float)(110085.03103f * yPos - 5192951.31201f);

        Vector3 position = new Vector3(x, 100, z);

        return position;
    }

    public Vector3 ConvertLatLon(string latitude, string longitude)
    {
        System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
        latitude = latitude.Replace('.', ',');
        longitude = longitude.Replace('.', ',');


        decimal lat = decimal.Parse(latitude, numberStyle);
        decimal lon = decimal.Parse(longitude, numberStyle);

        float x = (float)(73231.60681m * lon + 6480662.43837m);
        float z = (float)(110085.03103m * lat - 5192951.31201m);
        


        Vector3 position = new Vector3(x, 100, z);

        return position;
    }
}
