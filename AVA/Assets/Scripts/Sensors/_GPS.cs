using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _GPS : MonoBehaviour
{
    private GPS GPS01;
    private Transform transform;
    public bool active;
    private bool active_prev;
    private IEnumerator activeGPS;
    

    void Start()
    {
        transform = GetComponent<Transform>();
        GPS01 = new GPS(0.5f, transform);
        active = active_prev = false;
        activeGPS = ActiveGPS(GPS01.updateFrequency);
    }

    public void OnGUI()
    {
        if (active != active_prev)
        {
            StartGPS(active);
            active_prev = active;
        }

    }

    private void StartGPS(bool _active)
    {
        active = _active;
        if (active)
        {
            //Start Coroutine
            StartCoroutine(activeGPS);
            Debug.Log("Come on MadsR!");

        }
        else
        {
            //Stop coroutine
            StopCoroutine(activeGPS);
            Debug.Log("Chill the fuck down.");

        }
    }
    private IEnumerator ActiveGPS(float _updateFrequency)
    {
        while (active)
        {
            GPS01.UpdateState();
            Debug.Log(GPS01.GetGPSData());
            yield return new WaitForSeconds(1f / _updateFrequency);
        }

    }
}
