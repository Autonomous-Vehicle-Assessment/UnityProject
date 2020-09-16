using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPS
{
    // Variables of the class
    private Vector3 coordinates;
    private Quaternion orientation;
    private float accuracy;
    public float updateFrequency;
    private float time_real;
    private int[,] signalMap;
    private float sateliteStrength;

    private Transform transform;



    public GPS(float _updateFrequency, Transform _transform)//, int[,] _signalMap)
    {
        updateFrequency = _updateFrequency;
        //signalMap = _signalMap;
        coordinates = new Vector3();
        orientation = new Quaternion();
        accuracy = 0.2f;
        transform = _transform;
        time_real = Time.time;
    }

    public string GetGPSData()
    {
        string data = "X: " + coordinates.x.ToString() + "m Y: " + coordinates.y.ToString() + "m Z: " + coordinates.z.ToString() + "m Time: " + time_real.ToString() + "s";
        return data;
    }


    public void UpdateState()
    {
        UpdateCoordinate();
        UpdateOrientation();
        UpdateTime();
    }


    private void UpdateCoordinate()
    {
        coordinates = transform.position + Vector3.one * GetNoise();
    } 

    private void UpdateOrientation()
    {
        orientation = transform.rotation;
    }

    private void UpdateTime()
    {
        time_real = Time.time;
    }

    private float GetNoise()
    {
        float noise = Random.Range(-accuracy / 2f, accuracy / 2f);
        return noise;
    }


    

}


