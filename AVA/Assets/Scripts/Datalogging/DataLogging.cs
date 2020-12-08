using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogging : MonoBehaviour
{
    private string filePath;
    private EngineModel engine;

    // Start is called before the first frame update
    void Start()
    {
        engine = GetComponent<EngineModel>();

        InitializeLogFile();
    }

    private void InitializeLogFile()
    {
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd_MM_HH_mm_ss");
        filePath = (Application.dataPath).Replace("/Assets", "/Logs/DataLogging/" + LogTime + ".csv");

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                // Debug.Log("file deleted");
            }
            catch
            {
                Debug.LogError("Cannot delete file!");
            }
        }

        WriteToFile("Time; Velocity; WheelForce; TransmissionTorque; Gear; EngineRPM; EngineTorque \n");
    }

    /// <summary>
    /// Stamp vehicle state in log file.
    /// </summary>
    /// <param name="engine">Vehicle model</param>
    public void UpdateLog()
    {
        string s_Time = Time.time.ToString();
        string s_Velocity = engine.speed.ToString();
        string s_EngineRPM = engine.engineRPM.ToString();
        string s_EngineTorque = engine.engineTorque.ToString();

        float m_TransmissionTorque = 0;
        float m_WheelForce = 0;

        for (int i = 0; i < engine.wheels.Count; i++)
        {
            m_TransmissionTorque += engine.wheels[i].collider.motorTorque;
            m_WheelForce += engine.wheels[i].collider.motorTorque * engine.wheels[i].collider.radius;
        }

        string s_WheelForce = m_WheelForce.ToString();
        string s_TransmissionTorque = m_TransmissionTorque.ToString();

        string s_CurrentGear = (engine.currentGear + 1).ToString();

        // Log data
        if (isActiveAndEnabled) WriteToFile(s_Time + ";" + s_Velocity + ";" + s_WheelForce + ";" + s_TransmissionTorque + ";" + s_CurrentGear + ";" + s_EngineRPM + ";" + s_EngineTorque + "\n");
    }


    /// <summary>
    /// Write msg to csv file.
    /// </summary>
    /// <param name="msg"></param>
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
