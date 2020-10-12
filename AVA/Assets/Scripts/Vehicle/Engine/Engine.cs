using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;


public class Engine
{
    // ----- Curves ----- // 
    private AnimationCurve TorqueCurve;
    private AnimationCurve PowerCurve;

    // ----- Engine ----- // 
    public int MinRpm, MaxRpm;                  // Engine maximum/minimum RPM
    public int MaxTorqueRpm, MaxPowerRpm;       // RPM at maximum Torque and Power
    public float Rpm;                           // Current engine status
    private float Inertia;                      // Mass moment of inertia [kg*m^2]
    private float AngularAccel;                 // Engine Angular acceleration 

    /// <summary>
    /// Instantiate an engine model from torque and power curves.
    /// </summary>
    /// <param name="torqueCurve">Engine torque curve (Torque/RPM)</param>
    /// <param name="powerCurve">Engine torque curve (Power/RPM)</param>
    /// <param name="inertia">Engine mass moment of inertia [kg m^3]</param>
    public Engine(AnimationCurve torqueCurve, AnimationCurve powerCurve, float inertia)
    {
        TorqueCurve = torqueCurve;
        PowerCurve = powerCurve;
        Inertia = inertia;

        // Extract MaxTorqueRpm from TorqueCurve
        int MaxTorque = 0;
        foreach (Keyframe keyframe in torqueCurve.keys)
        {
            if (keyframe.value > MaxTorque)
            {
                MaxTorque = (int)keyframe.value;
                MaxTorqueRpm = (int)keyframe.time;
            }
        }

        // Extract MaxPowerRpm from PowerCurve
        int MaxPower = 0;
        foreach (Keyframe keyframe in powerCurve.keys)
        {
            if (keyframe.value > MaxPower)
            {
                MaxPower = (int)keyframe.value;
                MaxPowerRpm = (int)keyframe.time;
            }
        }
    }

    /// <summary>
    /// Calculates engine torque from engine torque curve and current RPM.
    /// </summary>
    /// <param name="rpm">Current engine RPM.</param>
    /// <returns>Current engine torque.</returns>
    public float Torque()
    {
        return TorqueCurve.Evaluate(Rpm);
    }

    /// <summary>
    /// Calculates engine power from engine power curve and current RPM.
    /// </summary>
    /// <param name="rpm">Current engine RPM</param>
    /// <returns>Current engine power.</returns>
    public float Power()
    {
        return PowerCurve.Evaluate(Rpm);
    }

    /// <summary>
    /// Updates the engine RPM based on clutch and accelerator. When clutch is fully engaged transmission rpm = engine rpm.
    /// </summary>
    /// <param name="clutch">Clutch values (1 fully disengaged (pedel pressed), 0 fully engaged (pedal released))</param>
    /// <param name="throttle">Accelerator pedal (1 fully pressed, 0 fully released)</param>
    /// <param name="TransmissionRpm">Current transmission rpm (engine side)</param>
    public int[][] Update(int[][] data)
    {
        float clutch = data[(int)Channel.Input][(int)InputData.Clutch] / 10000.0f;
        float throttle = data[(int)Channel.Input][(int)InputData.Throttle] / 10000.0f;
        float transmissionRpm = data[(int)Channel.Vehicle][(int)VehicleData.TransmissionRpm] / 10000.0f;

        if (clutch == 1.0f)
        {
            Rpm += AngularAccel * Time.deltaTime;
            AngularAccel = Torque() / Inertia * throttle;
        }
        else
        {
            // To do: Differential equation during gear shift - RPM difference between engine and transmission.
            Rpm = transmissionRpm;
        }

        data[(int)Channel.Vehicle][(int)VehicleData.EngineRpm] = (int)(Rpm * 10000f);

        return data;
    }
}
