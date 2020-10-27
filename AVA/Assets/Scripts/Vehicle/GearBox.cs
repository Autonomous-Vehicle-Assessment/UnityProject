using UnityEngine;

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
