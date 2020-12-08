using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EngineModel))]
public class CarUserControl : MonoBehaviour
{
    [Header("Vehicle State")]
    public bool active;
    public float speed;
    public TransferCase currentTransfercase;

    [Header("Output")]
    [Range(-1, 1)]
    public float throttle;
    [Range(0, 1)]
    public float brake;
    [Range(0, 1)]
    public float handbrake;
    [Range(-1, 1)]
    public float steer;

    private EngineModel engine;
    private DataLogging dataLogger;
    private VehicleStats vehicleStats;

    private void Awake()
    {
        engine = GetComponent<EngineModel>();
        dataLogger = GetComponent<DataLogging>();
        vehicleStats = GetComponent<VehicleStats>();
    }


    private void LateUpdate()
    {
        // pass the input to the car!
        steer = Input.GetAxis("Horizontal");
        throttle = Input.GetAxis("Vertical");
        brake = Input.GetAxis("FootBrake");
        handbrake = Input.GetAxis("HandBrake");

        if (Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (currentTransfercase == TransferCase.High)
            {
                currentTransfercase = TransferCase.Low;
            }
            else
            {
                currentTransfercase = TransferCase.High;
            }
        }
            
        engine.currentTransferCase = currentTransfercase;

        if (engine.speed > 10 && throttle < 0) brake = 1;
        if (engine.speed < -10 && throttle > 0) brake = 1;

        if (active) engine.Move(steer, throttle, brake, handbrake);

        speed = engine.speed;
    }
}