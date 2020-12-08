using System.Collections.Generic;
using UnityEngine;


public enum TransferCase
{
    Low,
    High
}

public enum WheelSide
{
    Left,
    Right
}

[System.Serializable]
public class StandardWheel
{
    public WheelCollider collider;
    public GameObject mesh;
    public bool steering;
    public bool drive;
    public bool serviceBrake;
    public bool handBrake;
    public WheelSide wheelSide;
}

/// <summary>
/// Handles drivetrain and torque delivery to 4WD from driver input using Move command.
/// </summary>
[RequireComponent(typeof(VehicleStats))]
public class EngineModel : MonoBehaviour
{
    // ----- Engine ----- // 
    public int maxTorque = 450;           // [Nm]
    public int maxPower = 340;            // [HP]

    public int maxTorqueRpm = 3500;       // [rpm]
    public int maxPowerRpm = 6500;        // [rpm]

    public float engineRPM = 1000;          // [rpm]
    public float engineTorque = 306;      // [rpm]

    // ----- Vehicle ----- //
    public float transmissionRPM = 0;       // [rpm]
    public float maximumInnerSteerAngle = 34f;
    public float maximumOuterSteerAngle = 30f;
    public float handbrakeTorque = 50000f;
    public float brakeTorque = 3000f;
    public TransferCase transferCase;
    public Rigidbody rb;
    public int numberofWheels = 4;
    public int numberofDrivingWheels;
    public float speed;
    public GameObject centerofMass;
    public List<StandardWheel> wheels;
    private float throttleInput;

    // Motor torque vs Speed
    public AnimationCurve motorCurve = new AnimationCurve();
    public List<int> speedCurveValues = new List<int> { 1000, 2020, 2990, 3500, 5000, 6500 };
    public List<int> torqueCurveValues = new List<int> { 306, 385, 439, 450, 450, 367 };
    public int curvePoints = 6;

    public int minRpm = 1000;
    public int maxRpm = 6500;

    public int minRpmTorque;
    public int peakRpmTorque;

    // Gearbox
    public int currentGear;
    public int numberOfGears;
    public int currentTransferCaseIndex;
    public TransferCase currentTransferCase;
    public float[] transferCaseEff = new float[2];
    public float[] gearRatio = new float[10];
    public float[] gearEff = new float[10];
    public float reverseGearRatio = -1f;
    public float reverseGearEff = 1f;
    public float finalDriveRatio = 1f;
    public float finalDriveEff = 1f;
    public float[] transferCaseRatio = { 2.72f, 1.0f };

    // Anti-Sway bar
    public bool swayBarActive = false;
    public float antiRoll = 5000.0f;

    //TerrainTracker
    public TerrainTracker terrainTracker;
    private VehicleStats vehicleStats;

    // Initialize
    public void Awake()
    {
        // Update Center of Mass
        if (centerofMass != null)
        {
            wheels[0].collider.attachedRigidbody.centerOfMass = centerofMass.transform.localPosition;
        }

        rb = GetComponent<Rigidbody>();
        terrainTracker = GetComponent<TerrainTracker>();

        numberofDrivingWheels = 0;
        for (int i = 0; i < numberofWheels; i++)
        {
            if (wheels[i].drive)
            {
                numberofDrivingWheels += 1;
            }
        }
        wheels[0].collider.suspensionExpansionLimited = true;

        vehicleStats = GetComponent<VehicleStats>();
    }

    public void FixedUpdate()
    {
        // Sway Bar (Anti-Roll bar)
        if (swayBarActive)
        {
            float travelL = 1.0f;
            float travelR = 1.0f;
            for (int i = 0; i < numberofWheels / 2;)
            {
                WheelCollider WheelL = wheels[i].collider;
                i++;
                WheelCollider WheelR = wheels[i].collider;
                i++;

                bool groundedL = WheelL.GetGroundHit(out WheelHit hitL);
                if (groundedL)
                    travelL = (-WheelL.transform.InverseTransformPoint(hitL.point).y - WheelL.radius) / WheelL.suspensionDistance;

                bool groundedR = WheelR.GetGroundHit(out WheelHit hitR);
                if (groundedR)
                    travelR = (-WheelR.transform.InverseTransformPoint(hitR.point).y - WheelR.radius) / WheelR.suspensionDistance;

                float antiRollForce = (travelL - travelR) * antiRoll;


                if (groundedL)
                    rb.AddForceAtPosition(WheelL.transform.up * -antiRollForce,
                           WheelL.transform.position);
                if (groundedR)
                    rb.AddForceAtPosition(WheelR.transform.up * antiRollForce,
                           WheelR.transform.position);
            }

        }
    }

    public void UpdateState()
    {
        if (terrainTracker != null)
        {
            UpdateTerrainWheelParameters();
        }

        speed = rb.velocity.magnitude * GenericFunctions.SpeedCoefficient(vehicleStats.speedType) * Mathf.Sign(transform.InverseTransformDirection(rb.velocity).z);

        transmissionRPM = Mathf.Max(wheels[0].collider.rpm, wheels[1].collider.rpm, wheels[2].collider.rpm, wheels[3].collider.rpm);
        engineRPM = transmissionRPM * GearingRatioEff();
        engineRPM = Mathf.Abs(engineRPM);
        engineRPM = Mathf.Clamp(engineRPM, minRpm, maxRpm);

        ShiftScheduler();
        engineRPM = transmissionRPM * GearingRatioEff();
        engineRPM = Mathf.Abs(engineRPM);
        engineRPM = Mathf.Clamp(engineRPM, minRpm, maxRpm);

        engineTorque = motorCurve.Evaluate(engineRPM);
    }

    public void LockedDifferential()
    {
        float avgRpm = 0;
        // locked differential
        foreach (StandardWheel wheel in wheels)
        {
            avgRpm += wheel.collider.rpm;
        }

        avgRpm /= wheels.Count;

        foreach (StandardWheel wheel in wheels)
        {
            float slip = 0;
            float wheelRpm = wheel.collider.rpm;

            if (wheelRpm > avgRpm)
            {
                slip = avgRpm / wheelRpm - 1;
            }
            else if (wheelRpm < avgRpm)
            {
                slip = 1 - wheelRpm / avgRpm;
            }

        }

    }

    /// <summary>
    /// Applies torque and steering to wheels based on input.
    /// </summary>
    /// <param name="steer"> -1.0 .. 1.0 (full left to full right)</param>
    /// <param name="throttle"> 0.0 .. 1.0 (0% to 100% throttle)</param>
    /// <param name="footbrake">0.0 .. 1.0 (0% to 100% brake force)</param>
    /// <param name="handbrake">0.0 .. 1.0 (0% to 100% applied handbrake)</param>
    public void Move(float steer, float throttle, float footbrake, float handbrake)
    {
        // Update mesh position and rotation
        for (int i = 0; i < numberofWheels; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheels[i].collider.GetWorldPose(out pos, out quat);
            wheels[i].mesh.transform.position = pos;
            wheels[i].mesh.transform.rotation = quat; // * new Quaternion(1, 1, 1, 1);
            wheels[i].mesh.transform.Rotate(0, 0, 180f, Space.Self);
        }

        // Clamp input values
        steer = Mathf.Clamp(steer, -1, 1);
        throttle = Mathf.Clamp(throttle, -1, 1);
        footbrake = Mathf.Clamp(footbrake, 0, 1);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        throttleInput = throttle;

        // Input to colliders
        float steerAngleInner = steer * maximumInnerSteerAngle;
        float steerAngleOuter = steer * maximumOuterSteerAngle;
        float sransmissionTorque = TransmissionTorque() / (float)numberofDrivingWheels;

        for (int i = 0; i < numberofWheels; i++)
        {
            if (wheels[i].steering)        // Apply steering
            {
                float currentAngle = wheels[i].collider.steerAngle;

                float appliedAngleOuter = Mathf.Lerp(currentAngle, steerAngleOuter, 3 * Time.deltaTime);
                float appliedAngleInner = Mathf.Lerp(currentAngle, steerAngleInner, 3 * Time.deltaTime);


                if (steer > 0) // Turning right, apply outer and inner steering angle
                {
                    switch (wheels[i].wheelSide)
                    {
                        case WheelSide.Left:
                            wheels[i].collider.steerAngle = appliedAngleOuter;
                            break;
                        case WheelSide.Right:
                            wheels[i].collider.steerAngle = appliedAngleInner;
                            break;
                    }
                }
                else
                {
                    switch (wheels[i].wheelSide)
                    {
                        case WheelSide.Left:
                            wheels[i].collider.steerAngle = appliedAngleInner;
                            break;
                        case WheelSide.Right:
                            wheels[i].collider.steerAngle = appliedAngleOuter;
                            break;
                    }
                }

            }

            if (wheels[i].drive)           // Apply torque
            {
                wheels[i].collider.motorTorque = sransmissionTorque * throttle;
            }

            if (wheels[i].handBrake)       // Apply handbrake
            {
                wheels[i].collider.brakeTorque = handbrakeTorque * handbrake;
            }

            if (wheels[i].serviceBrake)    // Apply servicebrake (footbrake)
            {
                if (handbrake <= 0 || !wheels[i].handBrake)
                {
                    wheels[i].collider.brakeTorque = brakeTorque * footbrake;
                }
            }

            if (footbrake == 0 && throttle == 0 && handbrake == 0)     // Motor braking
            {
                wheels[i].collider.brakeTorque = sransmissionTorque * 0.5f;
            }
        }

    }

    public float TransmissionTorque()
    {
        float TransmissionTorque = engineTorque * GearingRatioEff();

        return TransmissionTorque;
    }

    public float GearingRatioEff()
    {
        float gearing;
        if (throttleInput >= 0)
        {
            if (currentGear < 0) currentGear = 1;
            gearing = 1.7f * gearRatio[currentGear] * gearEff[currentGear] * transferCaseRatio[(int)currentTransferCase] * transferCaseEff[(int)currentTransferCase] * finalDriveRatio * finalDriveEff;
        }
        else
        {
            gearing = -1.7f * reverseGearRatio * reverseGearEff * transferCaseRatio[(int)currentTransferCase] * transferCaseEff[(int)currentTransferCase] * finalDriveRatio * finalDriveEff;
        }
        
        return gearing;
    }

    public void UpdateTerrainWheelParameters()
    {
        for (int i = 0; i < numberofWheels; i++)
        {
            int surfaceIndex = terrainTracker.GetMainTexture(wheels[i].collider.transform.position);
            float ForwardStiffness = terrainTracker.frictionValues[surfaceIndex];
            float SidewaysStiffness = terrainTracker.frictionValues[surfaceIndex];
            WheelFrictionCurve fFriction = wheels[i].collider.forwardFriction;
            WheelFrictionCurve sFriction = wheels[i].collider.sidewaysFriction;
            fFriction.stiffness = ForwardStiffness;
            sFriction.stiffness = SidewaysStiffness;
            wheels[i].collider.forwardFriction = fFriction;
            wheels[i].collider.sidewaysFriction = sFriction;
        }
    }

    // ----- Gear shift scheduler - automatic gear ----- // 
    public void ShiftScheduler()
    {
        if(throttleInput >= 0)
        {
            if (currentGear < 0) currentGear = 0;
            if (currentGear != numberOfGears - 1)   // Highest gear
            {
                if (engineRPM >= maxPowerRpm)
                {
                    currentGear += 1;
                }
            }
            if (currentGear != 0)                     // Lowest gear
            {
                if (engineRPM <= maxTorqueRpm)
                {
                    currentGear -= 1;
                }
            }
        }
        else
        {
            currentGear = -2;
        }

    }

}
