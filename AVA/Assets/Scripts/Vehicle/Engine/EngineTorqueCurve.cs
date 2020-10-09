using UnityEngine;

public class EngineTorqueCurve
{
    private AnimationCurve TorqueCurve;
    private int MaxTorqueRpm;

    public EngineTorqueCurve(AnimationCurve torqueCurve)
    {
        TorqueCurve = torqueCurve;

        foreach (Keyframe keyframe in torqueCurve.keys)
        {
            int MaxTorque = 0;
            if (keyframe.value > MaxTorque)
            {
                MaxTorque = (int)keyframe.value;
                MaxTorqueRpm = (int)keyframe.time;
            }
        }
    }

    public float EngineTorque(float rpm)
    {
        return TorqueCurve.Evaluate(rpm);
    }
}
