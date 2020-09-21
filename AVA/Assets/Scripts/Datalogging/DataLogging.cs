using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogging : MonoBehaviour
{
    private string filePath;
    // Start is called before the first frame update
    void Start()
    {
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd_MM_HH_mm_ss");
        filePath = (Application.dataPath).Replace("/Assets","/Logs/" + LogTime + ".csv");

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log("file deleted");
            }
            catch
            {
                Debug.LogError("Cannot delete file!");
            }
        }

        WriteToFile("Time; Velocity; WheelForce; TransmissionTorque; Gear; EngineRPM; EngineTorque \n");
    }


    public void WriteToFile(string msg)
    {
        try
        {
            StreamWriter fileWriter = new StreamWriter(filePath, true);
            fileWriter.Write(msg);
            fileWriter.Close();
        }
        catch
        {
            Debug.LogError("Cannot write to the file");
        }
    }
}
