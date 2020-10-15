using UnityEngine;

public class GearBox
{
    // ----- Gearbox Data ----- //
    private int[][] Data;

    // ----- Gearbox - SETUP ----- //
    /// <summary>
    /// Array of Gear Ratio's
    /// <para>[0] = Neutral or Park</para>
    /// <para>[1] = 1st</para>
    /// <para>[2] = 2nd</para>
    /// <para>...</para>
    /// </summary>
    private float[] GearRatio = new float[10];
    /// <summary>
    /// Array of Gear Efficiencies
    /// <para>[0] = Neutral or Park</para>
    /// <para>[1] = 1st</para>
    /// <para>[2] = 2nd</para>
    /// <para>...</para>
    /// </summary>
    private float[] GearEff = new float[10];
    /// <summary>
    /// Array of reverse Gear ratios
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para>
    /// </summary>
    private float[] ReverseGearRatio = new float[10];
    /// <summary>
    /// Array of reverse Gear Efficiencies
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para>
    /// </summary>
    private float[] ReverseGearEff = new float[10];
    /// <summary>
    /// Array of Transfer Case ratios
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para>
    /// </summary>
    private float[] TransferCaseRatio = { 2.72f, 1.0f };
    /// <summary>
    /// Array of Transfer Case efficiencies
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para>
    /// </summary>
    private float[] TransferCaseEff = new float[2];
    /// <summary>
    /// Gearing ratio of the final drive
    /// </summary>
    private float FinalDriveRatio = 1.0f;
    /// <summary>
    /// Gearing efficiency of the final drive
    /// </summary>
    private float FinalDriveEff = 1.0f;

    /// <summary>
    /// Gearbox constructor for instantiating a gearbox to handle gearshifts, gearing ratios and more.
    /// </summary>
    /// <param name="gearRatio"> Array of Gear Ratio's
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para></param>
    /// <param name="gearEff">Array of Gear Efficiencies
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para></param>
    /// <param name="reverseGearRatio">Array of reverse Gear ratios
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para></param>
    /// <param name="reverseGearEff">Array of reverse Gear Efficiencies
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para></param>
    /// <param name="transferCaseRatio">Array of Transfer Case ratios
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para></param>
    /// <param name="transferCaseEff">Array of Transfer Case efficiencies
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para></param>
    /// <param name="finalDriveRatio">Gearing ratio of the final drive</param>
    /// <param name="finalDriveEff">Gearing efficiency of the final drive</param>
    public GearBox(float[] gearRatio, float[] gearEff, float[] reverseGearRatio, float[] reverseGearEff, float[] transferCaseRatio, float[] transferCaseEff, float finalDriveRatio, float finalDriveEff)
    {
        GearRatio = new float[gearRatio.Length + 1];
        GearRatio[0] = 0f;
        gearRatio.CopyTo(GearRatio, 1);

        GearEff = new float[gearEff.Length + 1];
        GearEff[0] = 0f;
        gearEff.CopyTo(GearEff, 1);

        ReverseGearRatio = reverseGearRatio;
        ReverseGearEff = reverseGearEff;
        TransferCaseRatio = transferCaseRatio;
        TransferCaseEff = transferCaseEff;
        FinalDriveRatio = finalDriveRatio;
        FinalDriveEff = finalDriveEff;
    }
    
   
    /// <summary>
    /// Updates gearbox.
    /// <para>Handles GearboxMode and Gearshifts</para>
    /// <para>Calculates Transmission Torque form current active gear</para>
    /// </summary>
    /// <param name="data">Vehicle data bus</param>
    /// <returns></returns>
    public int[][] Update(int[][] data)
    {
        Data = data;
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

        Data[Channel.Vehicle][VehicleData.TransmissionTorque]   = (int)(TransmissionTorque() * 1000f);
        Data[Channel.Vehicle][VehicleData.GearboxMode]          = (int)gearMode;

        return Data;
    }

    public float TransmissionTorque()
    {
        float ClutchTorque = Data[Channel.Vehicle][VehicleData.ClutchTorque];
        return ClutchTorque * Ratio() * Efficiency();
    }

    /// <summary>
    /// Calculates current gear ratio.
    /// </summary>
    /// <returns>Gear ratio</returns>
    public float Ratio()
    {
        float ratio;
        int transferCase = Data[Channel.Input][InputData.TransferCase];
        int CurrentGear = Data[Channel.Vehicle][VehicleData.GearboxGear];

        
        if (CurrentGear >= 0)
        {
            // Forward gears
            ratio = GearRatio[CurrentGear] * TransferCaseRatio[transferCase] * FinalDriveRatio;
        }
        else
        {
            // Reverse gears
            ratio = ReverseGearRatio[Mathf.Abs(CurrentGear) - 1] * TransferCaseRatio[transferCase] * FinalDriveRatio;
        }
        return ratio;
    }

    /// <summary>
    /// Calculates current gear transmission efficiency
    /// </summary>
    /// <returns>Transmission efficiency</returns>
    public float Efficiency()
    {
        float efficiency;
        int transferCase = Data[Channel.Input][InputData.TransferCase];
        int CurrentGear = Data[Channel.Vehicle][VehicleData.GearboxGear];

        if (CurrentGear >= 0)
        {
            // Forward gears
            efficiency = GearEff[CurrentGear] * TransferCaseEff[transferCase] * FinalDriveEff;
        }
        else
        {
            // Reverse gears
            efficiency = ReverseGearEff[Mathf.Abs(CurrentGear) - 1] * TransferCaseEff[transferCase] * FinalDriveEff;
        }
        return efficiency;
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
