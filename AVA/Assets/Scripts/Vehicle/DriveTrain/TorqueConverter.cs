using UnityEngine;

public class TorqueConverter
{
    private AnimationCurve torqueConverterCurve;

    public TorqueConverter(AnimationCurve _torqueConverterCurve)
    {
        torqueConverterCurve = _torqueConverterCurve;
    }

    /// <summary>
    /// Updates the torque converter calculating the clutchtorque at the outputshaft of the torque converter.
    /// </summary>
    /// <param name="data">Vehicle Databus.</param>
    /// <returns>Updated Vehicle Databus.</returns>
    public int[][] Update(int[][] data)
    {
        
        float clutchRpm = data[Channel.Vehicle][VehicleData.ClutchRpm] / 1000f;
        float engineRpm = data[Channel.Vehicle][VehicleData.EngineRpm] / 1000f;

        float speedRatio = SpeedRatio(clutchRpm, engineRpm);
        float engineTorque = data[Channel.Vehicle][VehicleData.EngineTorque] / 1000f;

        // To do: 
        // ClutchLock strategy
        // Torque Converter efficiency
        float torqueConverterRatio = torqueConverterCurve.Evaluate(speedRatio);

        float clutchTorque = engineTorque * torqueConverterRatio;
        data[Channel.Vehicle][VehicleData.ClutchTorque] = (int)(clutchTorque * 1000);

        return data;

    }

    /// <summary>
    /// Calculates Speed Ratio in the torque converter (Out/In).
    /// </summary>
    /// <param name="clutchRpm">Current RPM at the output shaft of the clutch 
    /// <para></para>(Transmission input / turbine)</param>
    /// <param name="engineRpm">Current RPM at the input shaft of the clutch (Engine speed)</param>
    /// <returns></returns>
    private float SpeedRatio(float clutchRpm, float engineRpm)
    {
        float speedRatio = clutchRpm / engineRpm;

        return Mathf.Min(speedRatio, 1);
    }


}


