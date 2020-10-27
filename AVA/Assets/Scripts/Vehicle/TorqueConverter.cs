using UnityEngine;

public class TorqueConverter
{
    private AnimationCurve torqueConverterCurve;

    public TorqueConverter(AnimationCurve _torqueConverterCurve)
    {
        torqueConverterCurve = _torqueConverterCurve;
    }
    public float Ratio(float clutchRpm, float engineRpm)
    {
        float speedRatio = SpeedRatio(clutchRpm, engineRpm);

        return speedRatio;
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


