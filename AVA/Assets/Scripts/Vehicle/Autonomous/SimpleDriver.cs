using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDriver : MonoBehaviour
{
    private EngineModel engine;
    private VehicleStats vehicleStats;

    [Header("Autonomous Driver")]
    public bool active;
    public float targetVelocity;
    [Range(-1, 1)]
    public float throttleSet;
    [Range(0, 1)]
    public float brakeSet;
    [Range(-1, 1)]
    public float steering;
    private const float proportionalGain = 0.2f;
    private const float brakeVel = 10f;
    private const float throttleCap = 1f;

    [Header("Output")]
    [Range(-1, 1)]
    public float throttle;
    [Range(0, 1)]
    public float brake;
    [Range(-1, 1)]
    public float steer;

    // Start is called before the first frame update
    void Start()
    {
        // get the controller
        engine = GetComponent<EngineModel>();
        vehicleStats = GetComponent<VehicleStats>();
    }

    // Update is called once per frame
    void Update()
    {
        Drive();
        steer = steering;
        if (throttleSet != 0)
        {
            throttle = throttleSet;
            brake = 0;
        }
        if(brakeSet != 0)
        {
            throttle = 0;
            brake = brakeSet;
        }

        if (active) engine.Move(steer, throttle, brake, 0);
    }

    private void Drive()
    {
        float vehicleSpeed = engine.speed;

        float speedError = targetVelocity - vehicleSpeed;
        throttle = speedError * proportionalGain;

        if (speedError < 0)
        {
            if (vehicleSpeed > brakeVel)
            {
                brake = Mathf.Min(1, Mathf.Abs(throttle));
                throttle = 0;
            }
            else
            {
                brake = 0;
                throttle = Mathf.Max(-throttleCap, throttle);
            }
        }

        else
        {
            if (vehicleSpeed < -brakeVel)
            {
                brake = Mathf.Min(1, Mathf.Abs(throttle));
                throttle = 0;
            }
            else
            {
                brake = 0;
                throttle = Mathf.Min(throttleCap, throttle);
            }
        }
    }
}
