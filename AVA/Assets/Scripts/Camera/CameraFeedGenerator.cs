using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;

public class CameraFeedGenerator : MonoBehaviour
{
    private Camera _camera;
    [HideInInspector]
    public RenderTexture cameraTexture;
    public bool recordFeed;
    public bool logDepth;
    [HideInInspector]
    public int id;
    [Range(0.1f,15)]
    public float saveFrequency;

    private IEnumerator snapShotRoutine;
    private Thread saveFeedProcess;
    private bool activeRoutine;

    private List<byte[]> snapShotList;
    private List<string> fileNameList;
    private List<string> depthLogList;
    private StreamWriter fileWriter;

    private CameraDepth cameraDepth;
    private string dataFilePath;
    private string dataFolder;

    void Awake()
    {
        // Acquire depth camera object.
        Transform camerasObject = transform.Find("Cameras");
        Transform depthCameraObject = camerasObject.Find("Depth Camera");
        _camera = depthCameraObject.GetComponent<Camera>();
        cameraDepth = GetComponent<CameraDepth>();

        // Initialize depth camera target textures to camera specifications.
        cameraTexture = new RenderTexture(672, 376, 8, RenderTextureFormat.ARGB32);
        _camera.targetTexture = cameraTexture;

        // Initialize multithreading routines.
        snapShotRoutine = SnapShotRoutine();
        saveFeedProcess = new Thread(new ThreadStart(SaveFeedProcess));

        recordFeed = false;
    }


    private void OnGUI()
    {
        if (recordFeed && !activeRoutine)
        {
            StartRecording();
        }
        else if (!recordFeed && activeRoutine)
        {
            StopRecording();
        }
    }

    /// <summary>
    /// Halts any active live feed.
    /// </summary>
    private void OnApplicationQuit()
    {
        if (activeRoutine)
        {
            StopRecording();
        }
    }

    /// <summary>
    /// Starts live feed coroutine and thread, initialize feed queues.
    /// <para>Sets up data folder and log file.</para>
    /// </summary>
    private void StartRecording()
    {
        snapShotList = new List<byte[]>();
        fileNameList = new List<string>();
        depthLogList = new List<string>();

        // Snapshot
        StartCoroutine(snapShotRoutine);

        // Save
        saveFeedProcess.Start();

        activeRoutine = true;

        SetupDataFolder();

        SetupDataLog();
    }

    /// <summary>
    /// Stops active live feed.
    /// <para>Halting coroutine and save thread.</para>
    /// </summary>
    private void StopRecording()
    {
        // Snapshot
        StopCoroutine(snapShotRoutine);

        // Save
        saveFeedProcess.Abort();

        if (fileWriter != null)
        {
            fileWriter.Close();
        }        

        activeRoutine = false;
    }

    /// <summary>
    /// File handling thread (saves images and writes to log file).
    /// </summary>
    private void SaveFeedProcess()
    {
        while (recordFeed)
        {
            SaveFeed();

            Thread.Sleep((int)(1000f / saveFrequency));
        }
    }

    /// <summary>
    /// Snapshot routine, grabs feed from front (depth) camera.
    /// </summary>
    /// <returns></returns>
    IEnumerator SnapShotRoutine()
    {
        while (recordFeed)
        {
            yield return new WaitForSeconds(1f / saveFrequency);

            Snapshot();
        }
    }

    /// <summary>
    /// Takes a snapshot from the camera feed, current position and depth measurements. 
    /// Adds snapshot and measurements to save feed queue.
    /// </summary>
    public void Snapshot()
    {
        string fileName = Time.time.ToString() + ".png";

        Texture2D snapshot = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.ARGB32, false);

        RenderTexture.active = cameraTexture;
        snapshot.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);

        string pointCloudString = "";
        string depthOffset = "";

        if (logDepth)
        {
            Vector3[,] pointCloud = cameraDepth.GetVehicleDepth();

            foreach (Vector3 point in pointCloud)
            {
                pointCloudString += point.ToString() + ";";
            }

            depthOffset = cameraDepth.hStepStart.ToString() + ";" + cameraDepth.vStepStart.ToString() + ";";
        }

        string logString = $"{Time.time};{transform.position.x};{transform.position.y};{transform.position.z};";


        snapShotList.Add(snapshot.EncodeToJPG());
        fileNameList.Add(fileName);
        depthLogList.Add(logString + depthOffset + pointCloudString + "\n");
    }

    /// <summary>
    /// Creates log folder.
    /// </summary>
    private void SetupDataFolder()
    {
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd_MM_HH_mm_ss");

        dataFolder = (Application.dataPath).Replace("/Assets", "/Logs/CameraFeed/" + LogTime) + "/Feed" + id.ToString();

        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
    }

    /// <summary>
    /// Initializes log file with the following:
    /// <para>Header containing local time and date</para>
    /// <para>If (logDepth) depth data width and height </para>
    /// <para>CSV header, Time; PosX; PosY; PosZ; (DepthData)</para>
    /// </summary>
    private void SetupDataLog()
    {
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd-MM HH:mm:ss");
        dataFilePath = (dataFolder + "/DataStream.csv");

        // Initialize filewriter and data file with header
        fileWriter = new StreamWriter(dataFilePath, true);
        WriteToFile($"Example Data Structure; Date: {LogTime} \n");
        if (logDepth)
        {
            WriteToFile($"Depth data width: {cameraDepth.width}; Depth data height: {cameraDepth.height} \n \n");
            WriteToFile("Time; Position X; Position Y; Position Z; xOffset; yOffset; DepthData... \n");
        }
        else
        {
            WriteToFile("Time; Position X; Position Y; Position Z \n");
        }
    }

    /// <summary>
    /// Writes message string to opened log file.
    /// </summary>
    /// <param name="msg"></param>
    public void WriteToFile(string msg)
    {
        try
        {
            fileWriter.Write(msg);
        }
        catch
        {
            Debug.LogError("Cannot write to the file");
        }
    }

    /// <summary>
    /// Saves camera feed and stamps log file.
    /// </summary>
    private void SaveFeed()
    {
        while (snapShotList.Count > 0)
        {
            // Save camera feed
            File.WriteAllBytes(dataFolder + "/" + fileNameList[0], snapShotList[0]);
            snapShotList.RemoveAt(0);
            fileNameList.RemoveAt(0);

            // Save log file
            WriteToFile(depthLogList[0]);
            depthLogList.RemoveAt(0);
        }
    }
}


