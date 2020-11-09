using System.Collections.Generic;
using UnityEngine;

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

public class DriveTrain
{
    // ----- Clutch ----- //
    private AnimationCurve ClutchPedalCurve;

    // ----- GearBox ----- //
    private GearBox gearBox;
    private TransferCase transferCase;
    private TorqueConverter torqueConverter;
    private Differential differential;
    
    // ----- Wheels ----- //
    public List<StandardWheel> wheels;
    public float maximumInnerSteerAngle = 34f;
    public float maximumOuterSteerAngle = 30f;

    // ----- DataBus ----- //
    private int[][] Data;

    public DriveTrain(GearBox _gearBox, TransferCase _transferCase, AnimationCurve clutchCurve, TorqueConverter _torqueConverter, Differential _differential, List<StandardWheel> _wheels)
    {
        gearBox = _gearBox;
        transferCase = _transferCase;
        ClutchPedalCurve = clutchCurve;
        torqueConverter = _torqueConverter;
        differential = _differential;
        wheels = _wheels;
    }

    public int[][] Update(int[][] data)
    {
        Data = data;

        // ----- Drivetrain Updates ----- //
        Data = gearBox.Update(Data);

        // ----- Torque pipeline ----- //
        Data = torqueConverter.Update(Data);

        
        TransmissionUpdate();
        TransferCaseUpdate();
        Data = differential.Update(Data);

        Move();
        UpdateRpms();

        float clutchLock = ClutchLock();
        Data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(clutchLock * 1000f);
        //data[Channel.Vehicle][VehicleData.TransmissionRpm] = Wheel and Differential Data

        

        return Data;
    }

    /// <summary>
    /// Calculates the torque output of the Transmission.
    /// </summary>
    private void TransmissionUpdate()
    {
        int currentGear = Data[Channel.Vehicle][VehicleData.GearboxGear];

        float gearRatio = gearBox.Ratio(currentGear);
        float efficiency = gearBox.Efficiency(currentGear);

        float clutchTorque = Data[Channel.Vehicle][VehicleData.ClutchTorque] / 1000f;

        Data[Channel.Vehicle][VehicleData.TransmissionTorque] = (int)(clutchTorque * gearRatio * efficiency * 1000f);


    }

    /// <summary>
    /// Calculates the torque output of the Transfer Case.
    /// </summary>
    private void TransferCaseUpdate()
    {
        int transferCaseGear = Data[Channel.Vehicle][VehicleData.TransferCaseGear];

        float gearRatio = transferCase.Ratio(transferCaseGear);
        float efficiency = transferCase.Efficiency(transferCaseGear);

        float TransmissionTorque = Data[Channel.Vehicle][VehicleData.TransmissionTorque] / 1000f;

        Data[Channel.Vehicle][VehicleData.TransferCaseTorque] = (int)(TransmissionTorque * gearRatio * efficiency * 1000f);
    }


    private void Move()
    {
        // Update mesh position and rotation
        for (int i = 0; i < wheels.Count; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheels[i].collider.GetWorldPose(out pos, out quat);
            wheels[i].mesh.transform.position = pos;
            wheels[i].mesh.transform.rotation = quat;
        }

        // Clamp input values
        float steering = Data[Channel.Input][InputData.Steer] / 10000f;
        float braking = Data[Channel.Input][InputData.Brake] / 10000f;

        // Input to colliders
        float steerAngleInner = steering * maximumInnerSteerAngle;
        float steerAngleOuter = steering * maximumOuterSteerAngle;

        for (int i = 0; i < wheels.Count; i++)
        {
            // Apply steering
            if (wheels[i].steering)        
            {
                // Turning right, apply outer and inner steering angle
                if (steering > 0) 
                {
                    switch (wheels[i].wheelSide)
                    {
                        case WheelSide.Left:
                            wheels[i].collider.steerAngle = steerAngleOuter;
                            break;
                        case WheelSide.Right:
                            wheels[i].collider.steerAngle = steerAngleInner;
                            break;
                    }
                }
                else
                {
                    switch (wheels[i].wheelSide)
                    {
                        case WheelSide.Left:
                            wheels[i].collider.steerAngle = steerAngleInner;
                            break;
                        case WheelSide.Right:
                            wheels[i].collider.steerAngle = steerAngleOuter;
                            break;
                    }
                }
            }            
        }

        wheels[0].collider.motorTorque = Data[Channel.Vehicle][VehicleData.FrontAxleLeftTorque] / 1000f;
        wheels[1].collider.motorTorque = Data[Channel.Vehicle][VehicleData.FrontAxleRightTorque] / 1000f;
        wheels[2].collider.motorTorque = Data[Channel.Vehicle][VehicleData.RearAxleLeftTorque] / 1000f;
        wheels[3].collider.motorTorque = Data[Channel.Vehicle][VehicleData.RearAxleRightTorque] / 1000f;
    }

    /// <summary>
    /// Updates Drivetrain angular velocities.
    /// </summary>
    private void UpdateRpms()
    {
        Data[Channel.Vehicle][VehicleData.FrontAxleLeftRpm]     = (int)(wheels[0].collider.rpm * 1000f);
        Data[Channel.Vehicle][VehicleData.FrontAxleRightRpm]    = (int)(wheels[1].collider.rpm * 1000f);
        Data[Channel.Vehicle][VehicleData.RearAxleLeftRpm]      = (int)(wheels[2].collider.rpm * 1000f);
        Data[Channel.Vehicle][VehicleData.RearAxleRightRpm]     = (int)(wheels[3].collider.rpm * 1000f);


        float maxWheelRpm = Mathf.Max(wheels[0].collider.rpm, wheels[1].collider.rpm, wheels[2].collider.rpm, wheels[3].collider.rpm);

        int transferCaseGear    = Data[Channel.Vehicle][VehicleData.TransferCaseGear];
        int currentGear         = Data[Channel.Vehicle][VehicleData.GearboxGear];

        float transferCaseRpm   = maxWheelRpm       / differential.finalDriveRatio;
        float transmissionRpm   = transferCaseRpm   / transferCase.Ratio(transferCaseGear);
        float clutchRpm         = transmissionRpm   / gearBox.Ratio(currentGear);

        Data[Channel.Vehicle][VehicleData.TransferCaseRpm]  = (int)(transferCaseRpm * 1000f);
        Data[Channel.Vehicle][VehicleData.TransmissionRpm]  = (int)(transmissionRpm * 1000f);
        Data[Channel.Vehicle][VehicleData.ClutchRpm]        = (int)(clutchRpm * 1000f);

    }
    



    /// <summary>
    /// Calculates ClutchLock based on Gear Mode.
    /// <para>Manual: Clutch pedal.</para>
    /// <para>Park and Neutral: Gearbox disengaged from engine.</para>
    /// <para>Reverse, Drive and Low: Automatic gearbox and clutch.</para>
    /// </summary>
    /// <returns>Clutch Lock ratio
    /// <para>0: Clutch fully disengaged.</para>
    /// <para>1: Clutch fully engaged.</para></returns>
    private float ClutchLock()
    {
        GearMode gearMode = (GearMode)Data[Channel.Input][InputData.AutomaticGear];
        float clutchLock = 0;

        switch (gearMode)
        {
            case GearMode.Manual:
                float Clutch = Data[Channel.Input][InputData.Clutch] / 10000f;
                clutchLock = ClutchPedalCurve.Evaluate(Clutch);
                break;
            case GearMode.Park:
                clutchLock = 0;
                break;
            case GearMode.Neutral:
                clutchLock = 0;
                break;
            case GearMode.Reverse:
                // ClutchLock in automatic
                break;
            case GearMode.Drive:
                // ClutchLock in automatic
                break;
            case GearMode.Low:
                // ClutchLock in automatic
                break;
        }

        return clutchLock;
    }






    


}
