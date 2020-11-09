/// <summary>
/// Store gear ratios and efficiencies of a transfer case.
/// </summary>
public class TransferCase
{
    // ----- TransferCase Parameters ----- //
    /// <summary>
    /// Array of Transfer Case ratios
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para>
    /// </summary>
    private float[] gearRatio = new float[10];
    /// <summary>
    /// Array of Transfer Case efficiencies
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para>
    /// </summary>
    private float[] gearEff = new float[10];

    /// <summary>
    /// Create a Transfer Case from a list of gear ratios and efficiencies.
    /// </summary>
    /// <param name="_gearRatio">Array of transfer case gear ratios.</param>
    /// <param name="_gearEff">Array of transfer case gear efficiencies.</param>
    public TransferCase(float[] _gearRatio, float[] _gearEff)
    {
        gearRatio = _gearRatio;
        gearEff = _gearEff;
    }
    
    /// <summary>
    /// Returns current gear ratio.
    /// </summary>
    /// <param name="currentGear">Current active transfer case gear.</param>
    /// <returns></returns>
    public float Ratio(int currentGear)
    {
        return gearRatio[currentGear];
    }

    /// <summary>
    /// Returns current gear efficiency.
    /// </summary>
    /// <param name="currentGear">Current active transfer case gear.</param>
    /// <returns></returns>
    public float Efficiency(int currentGear)
    {
        return gearEff[currentGear];
    }

}
