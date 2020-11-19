using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class CameraFeedGenerator : MonoBehaviour
{
    private Camera _camera;
    [HideInInspector]
    public RenderTexture cameraTexture;
    public bool recordFeed;
    public string path;
    [HideInInspector]
    public int id;
    [Range(0.1f,30)]
    public float saveFrequency;

    private IEnumerator saveFeedRoutine;
    private IEnumerator snapShotRoutine;
    private Thread saveFeedProcess;
    private Thread snapShotProcess;
    private bool activeRoutine;
    private string filePath;

    private List<Texture2D> snapShotList;
    private List<string> fileNameList;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        cameraTexture = new RenderTexture(672, 376, 8, RenderTextureFormat.ARGB32);
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
        snapShotList = new List<Texture2D>();
        fileNameList = new List<string>();

        // Snapshot
        StartCoroutine(snapShotRoutine);
        //snapShotProcess.Start();

        // Save
        //StartCoroutine(saveFeedRoutine);
        saveFeedProcess.Start();

        activeRoutine = true;

        filePath = path + "/Feed" + id.ToString();

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
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
                File.WriteAllBytes(filePath + "/" + fileNameList[0], snapShotList[0].EncodeToEXR());
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
                File.WriteAllBytes(filePath + "/" + fileNameList[0], snapShotList[0].EncodeToJPG());
                snapShotList.RemoveAt(0);
                fileNameList.RemoveAt(0);
            }

            yield return new WaitForSeconds(10f);
        }
    }

    public void Snapshot()
    {
        string fileName = Time.time.ToString() + ".png";

        Texture2D snapshot = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = cameraTexture;
        snapshot.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);

        snapShotList.Add(snapshot);
        fileNameList.Add(fileName);
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        File.WriteAllBytes(_fullPath, _bytes);
        // Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

}
