using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class TerrainMechanics : MonoBehaviour
{
    private List<SoilType> SoilList;
    private float tyreRadius = 0.505000000000000f;
    private float tyreWidth = 0.25f;
    private float pressurePlateMainDimension = 0.25f;
    private float omega = 22f;
    private float velocity = 10f;
    private float slipRatio = 0.099909990999100f;


    /*
    public static float FixedUnloading(float angleOfDeparture, float wheelRadius)
    {
        return wheelRadius - Mathf.Cos(angleOfDeparture * Mathf.Deg2Rad) * wheelRadius;
        
    }
    public static float[] GetTerrainParameters(int terrainType)
    {

    }*/

    public void Start()
    {
        SoilList = new List<SoilType>();
        SoilList.Add(new SoilType(TerrainType.SandyBrendan));
        //Debug.Log(SoilList[0].terrainName);
        //Debug.Log("thetaN");
        //Debug.Log(SoilList[0].MaxRadialStressAngle(slipRatio)*Mathf.Rad2Deg);
        //Debug.Log("RadialStress");
        //Debug.Log(SoilList[0].RadialStress(SoilList[0].EntryAngle()-0.01f, slipRatio, tyreRadius, pressurePlateMainDimension));
        //Debug.Log(SoilList[0].RadialStress(-0.01f, slipRatio, tyreRadius, pressurePlateMainDimension));

        //Debug.Log("Vertical stress");
        //Debug.Log(SoilList[0].VerticalStress(SoilList[0].EntryAngle() - 0.01f, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio));

        Debug.Log($"Entry angle = {Mathf.PI / 4}[rad]");
        float W45 = SoilList[0].VerticalStress(Mathf.PI/4, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        Debug.Log($"Total Vertical stress at 45deg = {W45}");
        //float W30 = SoilList[0].VerticalStress(Mathf.PI / 6, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        //Debug.Log($"Total Vertical stress at 30deg = {W30}");
        //float W20 = SoilList[0].VerticalStress(Mathf.PI / 9, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        //Debug.Log($"Total Vertical stress at 20deg = {W20}");

        //float thetaN = SoilList[0].MaxRadialStressAngle(slipRatio);
        //Debug.Log($"Max radial Stress angle {thetaN} [rad], {thetaN * Mathf.Rad2Deg} [deg]");
        //float entryAngle = SoilList[0].EntryAngle(tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        //Debug.Log($"Entry angle {entryAngle} [rad], {entryAngle * Mathf.Rad2Deg} [deg]");

        //float sigmaN_30deg = SoilList[0].RadialStressFront(Mathf.PI / 6, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        //Debug.Log($"Radial Stress at 30deg {sigmaN_30deg} [Pa]");
        ////float sigmaN_m2deg = SoilList[0].RadialStress(-2f*Mathf.Deg2Rad, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        ////Debug.Log($"Radial Stress at -2deg {sigmaN_m2deg} [Pa]");
        //float s_max30deg = SoilList[0].MaxShear(sigmaN_30deg);
        //Debug.Log($"Max Shear stress at 30deg {s_max30deg} [Pa]");
        //float j_x30deg = SoilList[0].ShearDisplacement(Mathf.PI / 6, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        //Debug.Log($"Shear Displacement at 30deg {j_x30deg} [m]");
        //float tau_x30deg = SoilList[0].ShearStress(Mathf.PI/6, sigmaN_30deg,tyreWidth,tyreRadius,pressurePlateMainDimension,slipRatio);
        //Debug.Log($"Shear Stress at 30deg {tau_x30deg} [Pa]");

        //Debug.Log($"Test of cosine to pi/4 - 45deg = {Mathf.Cos(Mathf.PI / 4)}");
    }

    public void FixedUpdate()
    {
        //Debug.Log(SoilList[0].VerticalStress(20f*Mathf.Deg2Rad, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio));
        //float result = SoilList[0].VerticalStress(SoilList[0].EntryAngle() - 0.01f, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);

    }
}


