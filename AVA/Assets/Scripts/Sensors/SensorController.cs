using System.Collections;
using UnityEngine;

public class SensorController : MonoBehaviour
{
    [Header("GPS Controls")]
    public  GameObject[]    gPSObjects;         // GPS objects
    private GPS[]           gPSStack;           // Instances of GPS class
    private Vector3[]       gPSPos;             // Positions of GPS objects
    private IEnumerator     getData;            // Coroutine setup
    private bool            gPSActive_Prev;     // For determining change in activation state
    [Space(10)]
    public bool             gPSActive;          // Activation of GPS data stream

    [Header("GPS Data")]

    public Vector3 vehiclePosition;         // Position of vehicle center
    public Vector3 vehicleOrientation; // Orientation of vehicle arround the y axis

    // Setup GPS instances
    void Start()
    {

        getData = ActiveGPS(0.5f);                  // Set coroutine update frequency
        gPSPos = new Vector3[gPSObjects.Length];    // Initialize GPS position array
        gPSStack = new GPS[gPSObjects.Length];      // Initialize GPS instances array
        gPSActive = true;
        // Create GPS instances and insert into array
        for (int i = 0; i < gPSObjects.Length; i++)
        {
            gPSStack[i] = new GPS(gPSObjects[i].GetComponent<Transform>());
        }
    }

    // Set GPS to being active from inspector
    public void OnGUI()
    {
        // Run in active state changes
        if (gPSActive != gPSActive_Prev)
        {
            StartGPS(gPSActive);
            gPSActive_Prev = gPSActive; // Update changed active state 
        }
    }

    // starts and stops coroutine for getting GPS data depending on active state
    public void StartGPS(bool _active)
    {
        gPSActive = _active;
        if (gPSActive)
        {
            //Start Coroutine
            StartCoroutine(getData);
        } 
        else
        {
            //Stop coroutine
            StopCoroutine(getData);
        }
    }

    // Coroutine for getting GPS data at set update frequency
    public IEnumerator ActiveGPS(float _updateFrequency)
    {
        while (gPSActive)
        {
            // Gets GPS data for each instance in the stack
            for (int i = 0; i < gPSObjects.Length; i++)
            {
                // localPosition fucks up the data.. Why??
                gPSPos[i] = gPSStack[i].GetGPSData();// - gPSObjects[i].GetComponent<Transform>().localPosition;
                
            }

            // Calculate orientation arround y-axis (based on first two GPS instances)
            vehicleOrientation = gPSPos[0] - gPSPos[1];
            vehicleOrientation.y = 0f;
            vehicleOrientation = vehicleOrientation.normalized;

            // Calculate vehicle centre position
            vehiclePosition = new Vector3(gPSPos[0].x + gPSPos[1].x, gPSPos[0].y + gPSPos[1].y, gPSPos[0].z + gPSPos[1].z)/2f;


            // Maybe assemble into a transform (position and rotation) if possible. 
            // Orientation normal vector just 



            // Waits for set ammount of time before running coroutine again
            yield return new WaitForSeconds(1f / _updateFrequency);
        }

    }
}
