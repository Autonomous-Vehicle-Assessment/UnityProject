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
    private AnimationCurve ClutchPedalCurve;

    // ----- GearBox ----- //
    private GearBox gearBox;

    // ----- Torque Converter ---- //
    private AnimationCurve torqueConverterCurve;
    private AnimationCurve torqueConverterEfficiencyCurve;

    // ----- DataBus ----- //
    private int[][] Data;


    public Transmission(GearBox _gearBox, AnimationCurve clutchCurve, AnimationCurve _torqueConverterCurve, AnimationCurve _torqueConverterEfficiencyCurve)
    {
        gearBox = _gearBox;
        ClutchPedalCurve = clutchCurve;
        torqueConverterCurve = _torqueConverterCurve;
        torqueConverterEfficiencyCurve = _torqueConverterEfficiencyCurve;
    }

    public int[][] Update(int[][] data)
    {
        Data = data;

        TorqueConverter();

        // ----- Gearbox Update ----- //
        Data = gearBox.Update(Data);

        float clutchLock = ClutchLock();
        Data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(clutchLock * 1000f);
        //data[Channel.Vehicle][VehicleData.TransmissionRpm] = Wheel and Differential Data

        

        return Data;
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
    public float ClutchLock()
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

    /// <summary>
    /// Calculates the torque output of the Torque Converter.
    /// </summary>
    public void TorqueConverter()
    {
        float engineTorque = Data[Channel.Vehicle][VehicleData.EngineTorque];
        float clutchTorque = engineTorque * torqueConverterCurve.Evaluate(TorqueConverterSpeedRatio());
        Data[Channel.Vehicle][VehicleData.ClutchTorque] = (int)(clutchTorque * 1000);
    }

    /// <summary>
    /// Calculates Speed Ratio in the torque converter (Out/In).
    /// </summary>
    /// <returns>Speed Ratio
    /// </returns>    
    private float TorqueConverterSpeedRatio()
    {
        int ClutchRpm = Data[Channel.Vehicle][VehicleData.ClutchRpm];
        int EngineRpm = Data[Channel.Vehicle][VehicleData.EngineRpm];

        float speedRatio = ClutchRpm/EngineRpm;

        return Mathf.Min(speedRatio,1);
    }

}
