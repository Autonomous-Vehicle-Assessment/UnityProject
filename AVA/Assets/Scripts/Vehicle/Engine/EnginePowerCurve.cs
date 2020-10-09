using UnityEngine;

public class EnginePowerCurve
{
    private AnimationCurve PowerCurve;
    private int MaxPowerRpm;

    public EnginePowerCurve(AnimationCurve powerCurve)
    {
        PowerCurve = powerCurve;

        foreach (Keyframe keyframe in powerCurve.keys)
        {
            int MaxPower = 0;
            if (keyframe.value > MaxPower)
            {
                MaxPower = (int)keyframe.value;
                MaxPowerRpm = (int)keyframe.time;
            }
        }
    }

    public float EnginePower(float rpm)
    {
        return PowerCurve.Evaluate(rpm);
    }
}
