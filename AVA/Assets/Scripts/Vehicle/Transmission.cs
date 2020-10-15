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
    private TransferCase transferCase;

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
        float EngineTorque = data[Channel.Vehicle][VehicleData.EngineTorque] / 1000f;
        float ClutchTorque, ClutchSlipTorque;
        data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(ClutchLock * 1000f);
        //data[Channel.Vehicle][VehicleData.TransmissionRpm] = Wheel and Differential Data

        // Clutch disengaged fully, engine free rotation
        if (ClutchLock == 0f)
        {
            ClutchSlipTorque = 0f;
            ClutchTorque = 0f;
        }
        // Clutch engaged, friction and torque transfer between transmission and engine
        else
        {
            float ClutchRpm = data[Channel.Vehicle][VehicleData.ClutchRpm] / 1000.0f;
            float EngineRpm = data[Channel.Vehicle][VehicleData.EngineRpm] / 1000.0f;
            float ClutchSlip = Slip(EngineRpm, ClutchRpm);

            // Frictiondisc force based on slip (To do: add slip-friction curve, currently linear)
            ClutchSlipTorque = ClutchLock * ClutchSlip * 10000;
            ClutchTorque = ClutchLock * EngineTorque + ClutchSlipTorque;
        }

        data[Channel.Vehicle][VehicleData.ClutchTorque] = (int)(ClutchTorque * 1000);
        data[Channel.Vehicle][VehicleData.ClutchSlipTorque] = (int)(ClutchSlipTorque * 1000);
        

        // ----- Gearbox Update ----- //
        data = gearBox.Update(data);


        return data;
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
