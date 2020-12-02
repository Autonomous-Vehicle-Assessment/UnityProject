using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum WaypointFormat
{
    AVA,
    UnitTest,
    KRC
}

[ExecuteInEditMode]
public class AIPathLoader : MonoBehaviour
{
    public SpeedType speedType;
    public WaypointFormat waypointFormat;

    private StreamReader fileReader;
    private StreamWriter fileWriter;
    private string pathRead;
    private string pathWrite;

    private List<string> stringList;
    private List<string[]> parsedList;

    private string pathDescription;
    private Color pathColor;
    private List<Vector3> pathPoints;
    private List<float> pathVel;

    public void ReadTextFile()
    {
        if (pathRead != null && pathRead.Length > 0)
        {
            fileReader = new StreamReader(pathRead);
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

    public void WriteTextFile()
    {
        if (pathWrite != null && pathWrite.Length > 0)
        {
            fileWriter = new StreamWriter(pathWrite);

            AIPath[] aiPath = transform.GetComponentsInChildren<AIPath>();

            List<string> pathList = new List<string>();
            pathList.Add("Name;R;G;B;X;Z;Vel");
            foreach (AIPath path in aiPath)
            {
                string pathString = "";
                string description = path.name;
                string rStr = path.lineColor.r.ToString();
                string gStr = path.lineColor.g.ToString();
                string bStr = path.lineColor.b.ToString();

                string nodeStringX = "";
                string nodeStringZ = "";
                string nodeStringVel = "";
                foreach (PathNode node in path.pathNodes)
                {
                    nodeStringX += node.transform.position.x.ToString().Replace(',','.') + ',';
                    nodeStringZ += node.transform.position.z.ToString().Replace(',', '.') + ',';
                    nodeStringVel += node.targetVelocity.ToString().Replace(',', '.') + ',';
                }

                nodeStringX = nodeStringX.Substring(0, nodeStringX.Length - 1);
                nodeStringZ = nodeStringZ.Substring(0, nodeStringZ.Length - 1);
                nodeStringVel = nodeStringVel.Substring(0, nodeStringVel.Length - 1);

                pathString = description + ';' + rStr + ';' + gStr + ';' + bStr + ';' + nodeStringX + ';' + nodeStringZ + ';' + nodeStringVel;
                pathList.Add(pathString);
            }

            foreach (string path in pathList)
            {
                fileWriter.WriteLine(path);
            }

            fileWriter.Close();

            // Inverse AVA
            //ParseList();

            // Inverse AVA
            //ConstructPath();
        }
    }

    /// <summary>
    /// Parse read list based on waypoint format (delimiter) adding rows to the parsedList.
    /// </summary>
    private void ParseList()
    {
        switch (waypointFormat)
        {
            case WaypointFormat.AVA:
                ParseListAVA();
                break;

            case WaypointFormat.UnitTest:
                ParseListUnitTest();
                break;

            case WaypointFormat.KRC:
                ParseListKRC();
                break;

        }
    }

    private void ParseListAVA()
    {
        parsedList = new List<string[]>();

        for (int i = 0; i < stringList.Count; i++)
        {
            string[] temp = stringList[i].Split(';');
            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = temp[j].Trim();  //removed the blank spaces
            }
            parsedList.Add(temp);
        }
    }

    private void ParseListUnitTest()
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
            parsedList.Add(temp);
        }
    }

    private void ConstructPath()
    {
        switch (waypointFormat)
        {
            case WaypointFormat.AVA:
                ConstructPathAVA();
                break;

            case WaypointFormat.UnitTest:
                ConstructPathUnitTest();
                break;

            case WaypointFormat.KRC:
                ConstructPathKRC();
                break;

        }
    }

    private void ConstructPathAVA()
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
            string[] xStrs = parsedList[row][4].Split(',');
            string[] zStrs = parsedList[row][5].Split(',');
            string[] velStrs = parsedList[row][6].Split(',');

            for (int point = 0; point < xStrs.Length; point++)
            {
                System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                float x = float.Parse(xStrs[point].Replace('.', ','), numberStyle);
                float z = float.Parse(zStrs[point].Replace('.', ','), numberStyle);
                float vel = float.Parse(velStrs[point].Replace('.', ','), numberStyle);

                Vector3 position = new Vector3(x, 100, z);
                pathPoints.Add(position);
                pathVel.Add(vel);
            }
            GeneratePath();
        }
    }

    private void ConstructPathUnitTest()
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
                //float vel = 15;

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
            _point.GetComponent<PathNode>().targetHeight = 1;
            _point.GetComponent<PathNode>().speedType = speedType;
            _point.GetComponent<PathNode>().SetHeight();
        }
    }

    public void SavePath()
    {

    }
    public void OpenDialog()
    {
        pathRead = EditorUtility.OpenFilePanel(
                    "Open file",
                    "",
                    "*");
    }

    public void OpenDialogSave()
    {
        pathWrite = EditorUtility.OpenFilePanel(
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
        float knownOffsetX = 1058.333f;
        //float observedOffsetX = 16.85f;
        //float observedOffsetZ = 4.33f;
        System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
        xPosStr = xPosStr.Replace('.', ',');
        yPosStr = yPosStr.Replace('.', ',');

        float xPos = float.Parse(xPosStr, numberStyle);
        float yPos = float.Parse(yPosStr, numberStyle);

        float z = -1f * xPos;
        float x = yPos - knownOffsetX;

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

        float x = (float)(73231.60681m * lon + 6480662.43837m) * 1.08f + 48.5f;
        float z = (float)(110085.03103m * lat - 5192951.31201m) +2.57f;

        //float x = (float)(73231.60681m * lon + 6480662.43837m);
        //float z = (float)(110085.03103m * lat - 5192951.31201m);


        Vector3 position = new Vector3(x, 100, z);

        return position;
    }
}
