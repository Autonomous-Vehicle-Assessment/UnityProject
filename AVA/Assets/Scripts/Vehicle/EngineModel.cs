using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;


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
    public WheelCollider m_collider;
    public GameObject mesh;
    public bool steering;
    public bool drive;
    public bool serviceBrake;
    public bool handBrake;
    public WheelSide wheelSide;
}

public class EngineModel : MonoBehaviour
{
    // ----- Engine ----- // 
    public int m_MaxTorque = 450;           // [Nm]
    public int m_MaxPower = 340;            // [HP]

    public int m_MaxTorqueRpm = 3500;       // [rpm]
    public int m_MaxPowerRpm = 6500;        // [rpm]

    public float m_EngineRPM = 1000;          // [rpm]
    public float m_EngineTorque = 306;      // [rpm]

    // ----- Vehicle ----- //
    public float m_TransmissionRPM = 0;       // [rpm]
    public float m_MaximumInnerSteerAngle = 34f;
    public float m_MaximumOuterSteerAngle = 30f;
    public float m_HandbrakeTorque = 50000f;
    public float m_BrakeTorque = 3000f;
    public TransferCase m_TransferCase;
    public Rigidbody m_Rigidbody;
    public int NumberofWheels = 4;
    public int NumberofDrivingWheels;
    public float m_Speed;
    public GameObject m_CenterofMass;
    public List<StandardWheel> m_Wheel;

    // Motor torque vs Speed
    public AnimationCurve m_MotorCurve = new AnimationCurve();
    public List<int> e_SpeedCurveValues = new List<int> { 1000, 2020, 2990, 3500, 5000, 6500 };
    public List<int> e_TorqueCurveValues = new List<int> { 306, 385, 439, 450, 450, 367 };
    public int e_CurvePoints = 6;

    public int m_MinRpm = 1000;
    public int m_MaxRpm = 6500;

    public int m_MinRpmTorque;
    public int m_PeakRpmTorque;

    // Gearbox
    public int m_CurrentGear;
    public int m_NumberOfGears;
    public int m_CurrentTransferCaseIndex;
    public TransferCase m_CurrentTransferCase;
    public float[] m_TransferCaseEff = new float[2];
    public float[] m_GearRatio = new float[10];
    public float[] m_GearEff = new float[10];
    public float m_ReverseGearRatio = -1f;
    public float m_ReverseGearEff = 1f;
    public float m_FinalDriveRatio = 1f;
    public float m_FinalDriveEff = 1f;
    public float[] m_TransferCaseRatio = { 2.72f, 1.0f };

    // Anti-Sway bar
    public bool swayBarActive = false;
    public float AntiRoll = 5000.0f;

    //TerrainTracker
    public TerrainTracker terrainTracker;

    // Initialize
    public void Awake()
    {
        // Update Center of Mass
        if (m_CenterofMass != null)
        {
            m_Wheel[0].m_collider.attachedRigidbody.centerOfMass = m_CenterofMass.transform.localPosition;
        }

        m_Rigidbody = GetComponent<Rigidbody>();
        terrainTracker = GetComponent<TerrainTracker>();

        NumberofDrivingWheels = 0;
        for (int i = 0; i < NumberofWheels; i++)
        {
            if (m_Wheel[i].drive)
            {
                NumberofDrivingWheels += 1;
            }
        }
        m_Wheel[0].m_collider.suspensionExpansionLimited = true;
    }

    public void FixedUpdate()
    {
        
        // Sway Bar (Anti-Roll bar)
        if (swayBarActive)
        {
            float travelL = 1.0f;
            float travelR = 1.0f;
            for (int i = 0; i < NumberofWheels / 2;)
            {
                WheelCollider WheelL = m_Wheel[i].m_collider;
                i++;
                WheelCollider WheelR = m_Wheel[i].m_collider;
                i++;

                bool groundedL = WheelL.GetGroundHit(out WheelHit hitL);
                if (groundedL)
                    travelL = (-WheelL.transform.InverseTransformPoint(hitL.point).y - WheelL.radius) / WheelL.suspensionDistance;

                bool groundedR = WheelR.GetGroundHit(out WheelHit hitR);
                if (groundedR)
                    travelR = (-WheelR.transform.InverseTransformPoint(hitR.point).y - WheelR.radius) / WheelR.suspensionDistance;

                float antiRollForce = (travelL - travelR) * AntiRoll;
            

                if (groundedL)
                    m_Rigidbody.AddForceAtPosition(WheelL.transform.up * -antiRollForce,
                           WheelL.transform.position);
                if (groundedR)
                    m_Rigidbody.AddForceAtPosition(WheelR.transform.up * antiRollForce,
                           WheelR.transform.position);
            }

        }
    }
    public void UpdateState()
    {
        //UpdateTerrainWheelParameters();
        m_TransmissionRPM = (m_Wheel[2].m_collider.rpm + m_Wheel[3].m_collider.rpm) / 2f;
        m_EngineRPM = m_TransmissionRPM * GearingRatioEff();
        m_EngineRPM = Mathf.Abs(m_EngineRPM);
        m_EngineRPM = Mathf.Clamp(m_EngineRPM, m_MinRpm, m_MaxRpm);

        ShiftScheduler();
        m_EngineRPM = m_TransmissionRPM * GearingRatioEff();
        m_EngineRPM = Mathf.Abs(m_EngineRPM);
        m_EngineRPM = Mathf.Clamp(m_EngineRPM, m_MinRpm, m_MaxRpm);

        m_EngineTorque = m_MotorCurve.Evaluate(m_EngineRPM);
    }
    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        // Update mesh position and rotation
        for (int i = 0; i < NumberofWheels; i++)
        {
            Quaternion quat;
            Vector3 pos;
            m_Wheel[i].m_collider.GetWorldPose(out pos, out quat);
            m_Wheel[i].mesh.transform.position = pos;
            m_Wheel[i].mesh.transform.rotation = quat; // * new Quaternion(1, 1, 1, 1);
            m_Wheel[i].mesh.transform.Rotate(0, 0, 180f, Space.Self);
        }

        // Clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        accel = Mathf.Clamp(accel, -1, 1); // <------  Throttle cap
        footbrake = -1 * Mathf.Clamp(footbrake, 0, 1);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        // Input to colliders
        float m_SteerAngleInner = steering * m_MaximumInnerSteerAngle;
        float m_SteerAngleOuter = steering * m_MaximumOuterSteerAngle;
        float m_TransmissionTorque = TransmissionTorque() / (float)NumberofDrivingWheels;

        for (int i = 0; i < NumberofWheels; i++)
        {
            if (m_Wheel[i].steering)        // Apply steering
            {
                if (steering > 0) // Turning right, apply outer and inner steering angle
                {
                    switch (m_Wheel[i].wheelSide)
                    {
                        case WheelSide.Left:
                            m_Wheel[i].m_collider.steerAngle = m_SteerAngleOuter;
                            break;
                        case WheelSide.Right:
                            m_Wheel[i].m_collider.steerAngle = m_SteerAngleInner;
                            break;
                    }
                }
                else
                {
                    switch (m_Wheel[i].wheelSide)
                    {
                        case WheelSide.Left:
                            m_Wheel[i].m_collider.steerAngle = m_SteerAngleInner;
                            break;
                        case WheelSide.Right:
                            m_Wheel[i].m_collider.steerAngle = m_SteerAngleOuter;
                            break;
                    }
                }
                
            }

            if (m_Wheel[i].drive)           // Apply torque
            {
                m_Wheel[i].m_collider.motorTorque = m_TransmissionTorque * accel;             
            }

            if (m_Wheel[i].handBrake)       // Apply handbrake
            {
                m_Wheel[i].m_collider.brakeTorque = m_HandbrakeTorque * -handbrake;
            }

            if (m_Wheel[i].serviceBrake)    // Apply servicebrake (footbrake)
            {
                m_Wheel[i].m_collider.brakeTorque = m_BrakeTorque * -footbrake;
            }

            if (footbrake == 0 && accel == 0 && handbrake == 0)     // Motor braking
            {
                m_Wheel[i].m_collider.brakeTorque = m_TransmissionTorque * 0.5f;
            }
        }

    }
      
    public float TransmissionTorque()
    {
        float TransmissionTorque = m_EngineTorque * GearingRatioEff();

        return TransmissionTorque;
    }

    public float GearingRatioEff()
    {
        float Gearing = m_GearRatio[m_CurrentGear] * m_GearEff[m_CurrentGear] * m_TransferCaseRatio[(int)m_CurrentTransferCase] * m_TransferCaseEff[(int)m_CurrentTransferCase] * m_FinalDriveRatio * m_FinalDriveEff;
        return Gearing;
    }

    public void UpdateTerrainWheelParameters()
    {
        float ForwardStiffness = 1f;
        float SidewaysStiffness = 1f;

        if (terrainTracker.surfaceIndex == 1)
        {
            ForwardStiffness = 0.3f;
            SidewaysStiffness = 0.3f;
        }
        if (terrainTracker.surfaceIndex == 2)
        {
            ForwardStiffness = 0.6f;
            SidewaysStiffness = 0.6f;
        }
        for (int i = 0; i < NumberofWheels; i++)
        {
            WheelFrictionCurve fFriction = m_Wheel[i].m_collider.forwardFriction;
            WheelFrictionCurve sFriction = m_Wheel[i].m_collider.sidewaysFriction;
            fFriction.stiffness = ForwardStiffness;
            sFriction.stiffness = SidewaysStiffness;
            m_Wheel[i].m_collider.forwardFriction = fFriction;
            m_Wheel[i].m_collider.sidewaysFriction = sFriction;
        }
    }

    // ----- Gear shift scheduler - automatic gear ----- // 
    public void ShiftScheduler()
    {
        if (m_CurrentGear != m_NumberOfGears - 1)   // Highest gear
        {
            if (m_EngineRPM >= m_MaxPowerRpm)
            {
                m_CurrentGear += 1;
            }
        }
        if (m_CurrentGear != 0)                     // Lowest gear
        {
            if (m_EngineRPM <= m_MaxTorqueRpm)
            {
                m_CurrentGear -= 1;
            }
        }
    }

}
