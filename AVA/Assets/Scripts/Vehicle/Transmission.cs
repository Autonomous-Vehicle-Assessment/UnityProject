using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transmission
{

    // ----- Gearbox - STATUS ----- //
    private int m_CurrentGear;
    private int m_CurrentTransferCaseIndex;
    private TransferCase m_CurrentTransferCase;

    // ----- Gearbox - SETUP ----- //
    private int numberOfGears;
    private float[] gearRatio = new float[10];
    private float[] gearEff = new float[10];
    private float[] transferCaseRatio = { 2.72f, 1.0f };
    private float[] transferCaseEff = new float[2];
    private float reverseGearRatio = -3.0f;
    private float reverseGearEff = 1.0f;
    private float finalDriveRatio = 1.0f;
    private float finalDriveEff = 1.0f;

    // ----- Differential ----- //
    private float diffSlipLimitFront = 1.0f;
    private float diffSlipLimitRear = 1.0f;
    private float diffTransferLimitFront = 1.0f;
    private float diffTransferLimitRear = 1.0f;

    
    public Transmission(GearBox gearBox)
    {

    }

    /// <summary>
    /// Calculates transmission slip.
    /// </summary>
    /// <param name="EngineRpm">Engine output RPM.</param>
    /// <param name="TransmissionRpm">Transmission RPM on engine side.</param>
    /// <returns>Slipvalue 
    /// (   1: EngineRpm >> TransmissionRpm) 
    /// (  -1: EngineRpm << TransmissionRpm) 
    /// (   0: EngineRpm == TransmissionRpm
    /// </returns>
    public float Slip(float EngineRpm, float TransmissionRpm)
    {
        float slip;
        if (EngineRpm >= TransmissionRpm)
        {
            slip = 1 - TransmissionRpm / EngineRpm;
        }
        else
        {
            slip = EngineRpm / TransmissionRpm;
        }
        return slip;
    }
}
