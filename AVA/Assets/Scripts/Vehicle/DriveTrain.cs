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

public class DriveTrain
{
    // ----- Clutch ----- //
    private AnimationCurve ClutchPedalCurve;

    // ----- GearBox ----- //
    private GearBox gearBox;
    private TransferCase transferCase;

    // ----- Torque Converter ---- //
    private AnimationCurve torqueConverterCurve;

    // ----- DataBus ----- //
    private int[][] Data;

    public DriveTrain(GearBox _gearBox, TransferCase _transferCase, AnimationCurve clutchCurve, AnimationCurve _torqueConverterCurve)
    {
        gearBox = _gearBox;
        transferCase = _transferCase;
        ClutchPedalCurve = clutchCurve;
        torqueConverterCurve = _torqueConverterCurve;
    }

    public int[][] Update(int[][] data)
    {
        Data = data;

        // ----- Gearbox Mode ----- //
        GearboxUpdate();

        // ----- Torque pipeline ----- //
        TorqueConverterUpdate();
        TransmissionUpdate();
        TransferCaseUpdate();

        float clutchLock = ClutchLock();
        Data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(clutchLock * 1000f);
        //data[Channel.Vehicle][VehicleData.TransmissionRpm] = Wheel and Differential Data

        

        return Data;
    }

    /// <summary>
    /// Updates the current Gearbox Mode based on the automatic/manual gear stick.
    /// <para>Gearshift, manual/automatic, neutral, park, etc.</para>
    /// </summary>
    private void GearboxUpdate()
    {
        GearMode gearMode = (GearMode)Data[Channel.Input][InputData.AutomaticGear];
        int CurrentGear = Data[Channel.Vehicle][VehicleData.GearboxGear];

        switch (gearMode)
        {
            case GearMode.Manual:

                int SelectedGear = Data[Channel.Input][InputData.ManualGear];
                int GearIncrement = Data[Channel.Input][InputData.GearShift];

                if (CurrentGear != SelectedGear)
                {
                    ShiftGear(SelectedGear);
                }
                else if (GearIncrement != 0)
                {
                    Data[Channel.Input][InputData.ManualGear] = CurrentGear + GearIncrement;
                    ShiftGear(CurrentGear + GearIncrement);
                    Data[Channel.Input][InputData.GearShift] = 0;
                }
                break;

            case GearMode.Park:
                if (CurrentGear != 0)
                {
                    ShiftNeutral();
                    SetPark();
                }
                break;

            case GearMode.Neutral:
                if (CurrentGear != 0)
                {
                    ShiftNeutral();
                }
                break;

            case GearMode.Reverse:
                if (CurrentGear != -1)
                {
                    ShiftGear(-1);
                }
                break;

            case GearMode.Drive:
                // Engage automatic gearbox
                // AutomaticGearboxShift()
                break;

            case GearMode.Low:
                if (CurrentGear != 1)
                {
                    ShiftGear(1);
                }
                break;
        }

        Data[Channel.Vehicle][VehicleData.GearboxMode] = (int)gearMode;
    }

    /// <summary>
    /// Calculates the torque output of the Torque Converter.
    /// </summary>
    private void TorqueConverterUpdate()
    {
        float speedRatio = TorqueConverterSpeedRatio();
        float engineTorque = Data[Channel.Vehicle][VehicleData.EngineTorque];

        // To do: 
        // ClutchLock strategy
        // Torque Converter efficiency
        float torqueConverterRatio = torqueConverterCurve.Evaluate(speedRatio);
        float clutchTorque = engineTorque * torqueConverterRatio;

        Data[Channel.Vehicle][VehicleData.ClutchTorque] = (int)(clutchTorque * 1000);
    }

    /// <summary>
    /// Calculates the torque output of the Transmission.
    /// </summary>
    private void TransmissionUpdate()
    {
        int currentGear = Data[Channel.Vehicle][VehicleData.GearboxGear];

        float gearRatio = gearBox.Ratio(currentGear);
        float efficiency = gearBox.Efficiency(currentGear);

        float clutchTorque = Data[Channel.Vehicle][VehicleData.ClutchTorque];

        Data[Channel.Vehicle][VehicleData.TransmissionTorque] = (int)(clutchTorque * gearRatio * efficiency * 1000f);
    }

    /// <summary>
    /// Calculates the torque output of the Transfer Case.
    /// </summary>
    private void TransferCaseUpdate()
    {
        int transferCaseGear = Data[Channel.Vehicle][VehicleData.TransferCaseGear];

        float gearRatio = transferCase.Ratio(transferCaseGear);
        float efficiency = gearBox.Efficiency(transferCaseGear);

        float transferCaseTorque = Data[Channel.Vehicle][VehicleData.ClutchTorque];

        Data[Channel.Vehicle][VehicleData.TransferCaseTorque] = (int)(transferCaseTorque * gearRatio * efficiency * 1000f);
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
    private float ClutchLock()
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
    /// Shifting out of current gear and into desired gear.
    /// <para>Currently missing check for limits of gear</para>
    /// </summary>
    public void ShiftGear(int DesiredGear)
    {
        if (Data[Channel.Vehicle][VehicleData.GearboxShifting] != 1)
        {
            Data[Channel.Vehicle][VehicleData.GearboxShifting] = 1;
            ShiftNeutral();
        }
        else
        {
            ShiftIntoGear(DesiredGear);
            Data[Channel.Vehicle][VehicleData.GearboxShifting] = 0;
        }
    }

    /// <summary>
    /// Shifting out to the Neutral gear, disconnecting the gearbox from the engine.
    /// </summary>
    public void ShiftNeutral()
    {
        Data[Channel.Vehicle][VehicleData.GearboxGear] = 0;
    }

    /// <summary>
    /// Shifting into desired gear.
    /// </summary>
    public void ShiftIntoGear(int DesiredGear)
    {
        // Set gear to desired gear
        Data[Channel.Vehicle][VehicleData.GearboxGear] = DesiredGear;
    }

    /// <summary>
    /// Sets parking pawl locking the gearbox
    /// </summary>
    public void SetPark()
    {

    }


}
