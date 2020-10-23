using UnityEngine;

public class TorqueConverter
{
    private int[][] Data;
    private AnimationCurve torqueConverterCurve;

    public TorqueConverter(AnimationCurve _torqueConverterCurve)
    {
        torqueConverterCurve = _torqueConverterCurve;
    }
    public int[][] Update(int[][] data)
    {
        Data = data;

        float clutchLock = Data[Channel.Vehicle][VehicleData.ClutchLock];
        float ClutchTorque, ClutchSlipTorque;

        //data[Channel.Vehicle][VehicleData.TransmissionRpm] = Wheel and Differential Data

        // Clutch disengaged fully, engine free rotation
        if (clutchLock == 0f)
        {
            ClutchSlipTorque = 0f;
            ClutchTorque = 0f;
        }
        // Clutch engaged, friction and torque transfer between transmission and engine
        else
        {
            float ClutchRpm = Data[Channel.Vehicle][VehicleData.ClutchRpm] / 1000.0f;
            float EngineRpm = Data[Channel.Vehicle][VehicleData.EngineRpm] / 1000.0f;
            float ClutchSlip = SlipRatio(EngineRpm, ClutchRpm);

            float EngineTorque = Data[Channel.Vehicle][VehicleData.EngineTorque] / 1000f;

            // Frictiondisc force based on slip (To do: add slip-friction curve, currently linear)
            ClutchSlipTorque = clutchLock * ClutchSlip * 1000;
            ClutchTorque = clutchLock * EngineTorque + ClutchSlipTorque;
        }

        Data[Channel.Vehicle][VehicleData.ClutchTorque] = (int)(ClutchTorque * 1000);
        Data[Channel.Vehicle][VehicleData.ClutchSlipTorque] = (int)(ClutchSlipTorque * 1000);

        return Data;
    }

    /// <summary>
    /// Calculates transmission slip.
    /// </summary>
    /// <param name="EngineRpm">Engine output RPM.</param>
    /// <param name="TransmissionRpm">Transmission RPM on engine side.</param>
    /// <returns>Slipvalue
    /// <para>( 1: EngineRpm >> TransmissionRpm)</para>
    /// <para>(-1: EngineRpm &lt;&lt; TransmissionRpm)</para>
    /// <para>( 0: EngineRpm = TransmissionRpm)</para>
    /// </returns>
    public float SlipRatio(float EngineRpm, float ClutchRpm)
    {
        float slip;
        if (EngineRpm > ClutchRpm)
        {
            slip = 1 - ClutchRpm / EngineRpm;
        }
        else if (ClutchRpm == 0)
        {
            slip = 0; // Avoid NaN return (division with 0)
        }
        else
        {
            slip = EngineRpm / ClutchRpm - 1;
        }
        return slip;
    }


}


