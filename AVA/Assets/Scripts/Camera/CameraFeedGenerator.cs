﻿using UnityEngine;
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
    [HideInInspector]
    public int id;
    [Range(0.1f,30)]
    public float saveFrequency;

    private IEnumerator saveFeedRoutine;
    private IEnumerator snapShotRoutine;
    private Thread saveFeedProcess;
    private Thread snapShotProcess;
    private bool activeRoutine;

    private List<byte[]> snapShotList;
    private List<string> fileNameList;

    private CameraDepth cameraDepth;
    private string dataFilePath;
    private string dataFolder;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        cameraDepth = GetComponent<CameraDepth>();
        cameraTexture = new RenderTexture(672, 376, 8, RenderTextureFormat.ARGB64);
        _camera.targetTexture = cameraTexture;

        snapShotRoutine = SnapShotRoutine();
        saveFeedRoutine = SaveFeedRoutine();

        snapShotProcess = new Thread(new ThreadStart(SnapShotProcess));
        saveFeedProcess = new Thread(new ThreadStart(SaveFeedProcess));

        if (recordFeed)
        {
            StartRecording();
        }

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

    private void OnApplicationQuit()
    {
        if (activeRoutine)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        snapShotList = new List<byte[]>();
        fileNameList = new List<string>();

        // Snapshot
        StartCoroutine(snapShotRoutine);
        //snapShotProcess.Start();

        // Save
        //StartCoroutine(saveFeedRoutine);
        saveFeedProcess.Start();

        activeRoutine = true;
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd_MM_HH_mm_ss");

        dataFolder = (Application.dataPath).Replace("/Assets", "/Logs/CameraFeed/" + LogTime) + "/Feed" + id.ToString();

        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        SetupDataLog();
    }

    private void StopRecording()
    {
        // Snapshot
        StopCoroutine(snapShotRoutine);
        //snapShotProcess.Abort();

        // Save
        //StopCoroutine(saveFeedRoutine);
        saveFeedProcess.Abort();

        activeRoutine = false;
    }

    private void SnapShotProcess()
    {
        while (recordFeed)
        {
            Thread.Sleep((int)(1000f / saveFrequency));

            Snapshot();
        }
    }

    private void SaveFeedProcess()
    {
        while (recordFeed)
        {
            while (snapShotList.Count > 0)
            {
                File.WriteAllBytes(dataFolder + "/" + fileNameList[0], snapShotList[0]);
                snapShotList.RemoveAt(0);
                fileNameList.RemoveAt(0);
            }

            Thread.Sleep((int)(1000f / saveFrequency));
        }
    }

    IEnumerator SnapShotRoutine()
    {
        while (recordFeed)
        {
            yield return new WaitForSeconds(1f / saveFrequency);

            Snapshot();
        }
    }

    IEnumerator SaveFeedRoutine()
    {
        while (recordFeed)
        {
            while (snapShotList.Count > 0)
            {
                File.WriteAllBytes(dataFolder + "/" + fileNameList[0], snapShotList[0]);
                snapShotList.RemoveAt(0);
                fileNameList.RemoveAt(0);
            }

            yield return new WaitForSeconds(10f);
        }
    }

    public void Snapshot()
    {
        string fileName = Time.time.ToString() + ".png";

        Texture2D snapshot = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.ARGB32, false);

        RenderTexture.active = cameraTexture;
        snapshot.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        
        snapShotList.Add(snapshot.EncodeToPNG());
        fileNameList.Add(fileName);

        float[,] depthArray = cameraDepth.GetDepth();

        string depthArrayString = depthArray.ToString();
        WriteToFile(depthArrayString);
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        File.WriteAllBytes(_fullPath, _bytes);
        // Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

    public void SetupDataLog()
    {
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd-MM HH:mm:ss");
        dataFilePath = (dataFolder + "/DataStream.csv");

        if (File.Exists(dataFilePath))
        {
            try
            {
                File.Delete(dataFilePath);
                // Debug.Log("file deleted");
            }
            catch
            {
                Debug.LogError("Cannot delete file!");
            }
        }

        WriteToFile($"Example Data Structure; Date: {LogTime} \n");
        WriteToFile($"Depth data width: {cameraDepth.width}; Depth data height: {cameraDepth.height} \n");
        WriteToFile("Time; Position X; Position Y; Position Z; DepthData \n \n");
    }

    public void WriteToFile(string msg)
    {
        try
        {
            StreamWriter fileWriter = new StreamWriter(dataFilePath, true);
            fileWriter.Write(msg);
            fileWriter.Close();
        }
        catch
        {
            Debug.LogError("Cannot write to the file");
        }
    }
}

