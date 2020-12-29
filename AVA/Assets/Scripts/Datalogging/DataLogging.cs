using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogging : MonoBehaviour
{
    private string filePath;
    private EngineModel engine;
    public float previousVelocity;
    public float previousVerticalVelocity;
    

    public float acceleration;
    public float vAcceleration;
    public float maxVerticalAcceleration;

    private bool brakeActive;
    private SimpleDriver driver;
    private float initPositionZ;
    private StreamWriter fileWriter;

    // Start is called before the first frame update
    void Start()
    {
        driver = GetComponent<SimpleDriver>();
        engine = GetComponent<EngineModel>();

        InitializeLogFile();

        previousVelocity = 0;
        previousVerticalVelocity = 0;
        maxVerticalAcceleration = 0;
        initPositionZ = transform.position.z;
        brakeActive = false;


    }

    private void InitializeLogFile()
    {
        string LogTime = DateTime.UtcNow.ToLocalTime().ToString("dd_MM_HH_mm_ss");
        filePath = (Application.dataPath).Replace("/Assets", "/Logs/DataLogging/" + transform.name + "_" + LogTime + ".csv");

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

        fileWriter = new StreamWriter(filePath, true);

        WriteToFile("Time; Velocity; Acceleration; PositionZ; Brake; Vertical Velocity; Vertical Acceleration; WheelForce; TransmissionTorque; Gear; EngineRPM; EngineTorque \n");
    }

    /// <summary>
    /// Stamp vehicle state in log file.
    /// </summary>
    /// <param name="engine">Vehicle model</param>
    public void UpdateLog()
    {
        acceleration = ((engine.speed / GenericFunctions.SpeedCoefficient(engine.vehicleStats.speedType) - previousVelocity) / Time.fixedDeltaTime);
        vAcceleration = ((transform.InverseTransformVector(engine.rb.velocity).y / GenericFunctions.SpeedCoefficient(engine.vehicleStats.speedType) - previousVerticalVelocity) / Time.fixedDeltaTime) / 9.81f;// + transform.InverseTransformVector(new Vector3(0, 1, 0)).y;
        maxVerticalAcceleration = Mathf.Max(maxVerticalAcceleration, vAcceleration);
        string s_acceleration = acceleration.ToString();
        string s_verticalAcceleration = vAcceleration.ToString();
        string s_Time = Time.time.ToString();
        string s_Velocity = engine.speed.ToString(); //  / GenericFunctions.SpeedCoefficient(engine.vehicleStats.speedType)
        string s_verticalVelocity = (transform.InverseTransformVector(engine.rb.velocity).y / GenericFunctions.SpeedCoefficient(engine.vehicleStats.speedType)).ToString();
        string s_EngineRPM = engine.engineRPM.ToString();
        string s_EngineTorque = engine.engineTorque.ToString();


        string s_PositionZ = "0";
        if (driver.brake == 1)
        {
            if (!brakeActive)
            {
                brakeActive = true;
                initPositionZ = transform.position.z;
            }
            s_PositionZ = (transform.position.z - initPositionZ).ToString();
        }

      

        string s_Brake = driver.brake.ToString();

        float m_TransmissionTorque = 0;
        float m_WheelForce = 0;

        previousVelocity = engine.speed / GenericFunctions.SpeedCoefficient(engine.vehicleStats.speedType);
        previousVerticalVelocity = transform.InverseTransformVector(engine.rb.velocity).y / GenericFunctions.SpeedCoefficient(engine.vehicleStats.speedType);

        for (int i = 0; i < engine.wheels.Count; i++)
        {
            m_TransmissionTorque += engine.wheels[i].collider.motorTorque;
            m_WheelForce += engine.wheels[i].collider.motorTorque * engine.wheels[i].collider.radius;
        }

        string s_WheelForce = m_WheelForce.ToString();
        string s_TransmissionTorque = m_TransmissionTorque.ToString();

        string s_CurrentGear = (engine.currentGear + 1).ToString();

        // Log data
        if (isActiveAndEnabled) WriteToFile(s_Time + ";" + s_Velocity + ";" + s_acceleration + ";" + s_PositionZ + ";" + s_Brake + ";" + s_verticalVelocity + ";" + s_verticalAcceleration + ";" + s_WheelForce + ";" + s_TransmissionTorque + ";" + s_CurrentGear + ";" + s_EngineRPM + ";" + s_EngineTorque + "\n");
    }


    /// <summary>
    /// Write msg to csv file.
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

    private void OnApplicationQuit()
    {
        fileWriter.Close();
    }


}
