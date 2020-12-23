using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDriver : MonoBehaviour
{
    private EngineModel engine;
    private VehicleStats vehicleStats;
    private Rigidbody rb;

    [Header("Autonomous Driver")]
    public bool active;
    public float targetVelocity;
    [Range(-1, 1)]
    public float throttleSet;
    [Range(0, 1)]
    public float brakeSet;
    [Range(-.3f, .3f)]
    public float steering;
    private const float proportionalGain = 0.2f;
    private const float brakeVel = 10f;
    private const float throttleCap = 1f;
    public bool coastDownActive;
    private bool coastDown;


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
        coastDown = false;
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, 0, targetVelocity / GenericFunctions.SpeedCoefficient(vehicleStats.speedType));
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

        if (coastDown && coastDownActive)
        {
            throttle = 0;
            brake = 1;
        }
        if (active) engine.Move(steer, throttle, brake, 0);
    }

    private void Drive()
    {
        float vehicleSpeed = engine.speed;

        float speedError = targetVelocity - vehicleSpeed;
        throttle = speedError * proportionalGain;

        if (speedError < 1) coastDown = true;

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
