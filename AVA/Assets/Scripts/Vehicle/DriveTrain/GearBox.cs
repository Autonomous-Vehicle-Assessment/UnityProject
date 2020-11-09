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

/// <summary>
/// Store gear ratios and efficiencies of a gearbox.
/// </summary>
public class GearBox
{
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

    // ----- DataBus ----- //
    private int[][] Data;

    /// <summary>
    /// Create a Gearbox from a list of gear ratios and efficiencies.
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
    public GearBox(float[] gearRatio, float[] gearEff, float[] reverseGearRatio, float[] reverseGearEff)
    {
        GearRatio = new float[gearRatio.Length + 1];
        GearRatio[0] = 0f;
        gearRatio.CopyTo(GearRatio, 1);

        GearEff = new float[gearEff.Length + 1];
        GearEff[0] = 0f;
        gearEff.CopyTo(GearEff, 1);

        ReverseGearRatio = reverseGearRatio;
        ReverseGearEff = reverseGearEff;
    }
    
    /// <summary>
    /// Updates the gearbox handling gearshift requests and gearmode changes.
    /// </summary>
    /// <param name="data">Vehicle databus.</param>
    /// <returns>Updated vehicle databus.</returns>
    public int[][] Update(int[][] data)
    {
        Data = data;

        GearBoxMode();

        return Data;
    }

    /// <summary>
    /// Updates the current Gearbox Mode based on the automatic/manual gear stick.
    /// <para>Gearshift, manual/automatic, neutral, park, etc.</para>
    /// </summary>
    private void GearBoxMode()
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
    /// Shifting out of current gear and into desired gear.
    /// <para>Currently missing check for limits of gear</para>
    /// </summary>
    private void ShiftGear(int DesiredGear)
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
    private void ShiftNeutral()
    {
        Data[Channel.Vehicle][VehicleData.GearboxGear] = 0;
    }

    /// <summary>
    /// Shifting into desired gear.
    /// </summary>
    private void ShiftIntoGear(int DesiredGear)
    {
        // Set gear to desired gear
        Data[Channel.Vehicle][VehicleData.GearboxGear] = DesiredGear;
    }

    /// <summary>
    /// Sets parking pawl locking the gearbox
    /// </summary>
    private void SetPark()
    {

    }

    /// <summary>
    /// Calculates current gear ratio.
    /// </summary>
    /// <returns>Gear ratio</returns>
    public float Ratio(int currentGear)
    {
        float ratio;

        if (currentGear >= 0)
        {
            // Forward gears
            ratio = GearRatio[currentGear];
        }
        else
        {
            // Reverse gears
            ratio = ReverseGearRatio[Mathf.Abs(currentGear) - 1];
        }
        return ratio;
    }

    /// <summary>
    /// Calculates current gear transmission efficiency
    /// </summary>
    /// <returns>Transmission efficiency</returns>
    public float Efficiency(int currentGear)
    {
        float efficiency;

        if (currentGear >= 0)
        {
            // Forward gears
            efficiency = GearEff[currentGear];
        }
        else
        {
            // Reverse gears
            efficiency = ReverseGearEff[Mathf.Abs(currentGear) - 1];
        }
        return efficiency;
    }

}
