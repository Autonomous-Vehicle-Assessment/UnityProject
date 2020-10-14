using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum GearMode
{
    Manual,
    Park,
    Reverse,
    Neutral,
    Drive,
    Low
}
public class Transmission
{
    // ----- Clutch ----- //
    private AnimationCurve ClutchCurve;

    // ----- GearBox ----- //
    private GearBox gearBox;
    private int CurrentGear;

    // ----- Differential ----- //
    private float diffSlipLimitFront = 1.0f;
    private float diffSlipLimitRear = 1.0f;
    private float diffTransferLimitFront = 1.0f;
    private float diffTransferLimitRear = 1.0f;

    
    public Transmission(GearBox _gearBox, AnimationCurve clutchCurve)
    {
        gearBox = _gearBox;
        ClutchCurve = clutchCurve;
    }

    public int[][] Update(int[][] data)
    {
        float Clutch = data[Channel.Input][InputData.Clutch] / 10000f;
        float ClutchLock = ClutchCurve.Evaluate(Clutch);
        float EngineTorque = data[Channel.Vehicle][VehicleData.EngineTorque] / 10000f;
        float ClutchSlipTorque;
        data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(ClutchLock * 10000f);
        //data[Channel.Vehicle][VehicleData.TransmissionRpm] = 

        // Clutch disengaged fully, engine free rotation
        if (ClutchLock == 0f)
        {
            ClutchSlipTorque = 0f;
        }
        // Clutch engaged, friction and torque transfer between transmission and engine
        else
        {
            float TransmissionRpm = data[Channel.Vehicle][VehicleData.TransmissionRpm] / 10000.0f;
            float EngineRpm = data[Channel.Vehicle][VehicleData.EngineRpm] / 10000.0f;
            float ClutchSlip = Slip(EngineRpm, TransmissionRpm);

            // Frictiondisc force based on slip (To do: add slip-friction curve, currently linear)
            ClutchSlipTorque = ClutchLock * ClutchSlip * 10000;
        }

        data[Channel.Vehicle][VehicleData.ClutchTorque] = (int)((ClutchSlipTorque + EngineTorque) * 10000);
        return data;
    }

    public int[][] GearStatus(int[][] data)
    {
        GearMode gearMode = (GearMode)data[Channel.Input][InputData.AutomaticGear];
        CurrentGear = data[Channel.Vehicle][VehicleData.GearboxGear];

        switch (gearMode)
        {
            case GearMode.Manual:
                break;
            case GearMode.Park:
                if (CurrentGear != 0)
                {
                    ShiftNeutral();
                    SetPark();
                }
                break;
            case GearMode.Reverse:
                if (CurrentGear != -1)
                {
                    ShiftGear(-1);
                }
                break;
            case GearMode.Neutral:
                if (CurrentGear != 0)
                {
                    ShiftNeutral();
                }
                break;
            case GearMode.Drive:
                break;
            case GearMode.Low:
                break;
            default:
                break;
        }

        data[Channel.Vehicle][VehicleData.GearboxMode] = (int)gearMode;
        data[Channel.Vehicle][VehicleData.GearboxGear] = CurrentGear;
        return data;
    }

    /// <summary>
    /// Shifting out of current gear and into desired gear.
    /// </summary>
    public void ShiftGear(int DesiredGear)
    {
        ShiftNeutral();
        ShiftIntoGear(DesiredGear);
    }

    /// <summary>
    /// Shifting out to the Neutral gear, disconnecting the gearbox from the engine.
    /// </summary>
    public void ShiftNeutral()
    {
        // Set gear to neutral
        CurrentGear = 0;
    }

    /// <summary>
    /// Shifting into desired gear.
    /// </summary>
    public void ShiftIntoGear(int DesiredGear)
    {
        // Set gear to desired gear
        CurrentGear = DesiredGear;
    }

    public void SetPark()
    {

    }
    /// <summary>
    /// Calculates transmission slip.
    /// </summary>
    /// <param name="EngineRpm">Engine output RPM.</param>
    /// <param name="TransmissionRpm">Transmission RPM on engine side.</param>
    /// <returns>Slipvalue 
    /// (   1: EngineRpm >> TransmissionRpm) 
    /// (  -1: EngineRpm << TransmissionRpm) 
    /// (   0: EngineRpm == TransmissionRpm
    /// </returns>
    public float Slip(float EngineRpm, float TransmissionRpm)
    {
        float slip;
        if (EngineRpm >= TransmissionRpm)
        {
            slip = 1 - TransmissionRpm / EngineRpm;
        }
        else
        {
            slip = EngineRpm / TransmissionRpm - 1;
        }
        return slip;
    }
}
