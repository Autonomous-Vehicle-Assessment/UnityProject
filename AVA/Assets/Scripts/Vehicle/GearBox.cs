using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearBox
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

}
