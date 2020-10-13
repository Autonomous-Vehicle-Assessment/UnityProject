using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Soil class containing relevant soil parameters and all relevant terramechanical functions.
/// </summary>
/// 

public enum TerrainType
{
    SandyBrendan,
    YoloLoamBrendan,
    Custom,
}

public class SoilType
{
    public string terrainName;
    private TerrainType terrainType;  // Teerrain Type Name

    //// OBSOLETE ///Bekker Parameters 
    //private float n;        // Pressure-Sinkage Parameter []
    //private float k_c;      // Pressure-Sinkage Parameter [kN/m^(n+1)]
    //private float k_phi;    // Pressure-Sinkage Parameter [kN/m^(n+2)]

    //private float k_o;      // Unloading/Reloading Terrain Parameter [kN/m^3]
    //private float A_u;      // Unloading/Reloading Terrain Parameter [kN/m^4]

    private float k_1;      //[]
    private float k_2;      //[]
    private float n;        //[]
    private float c;        //Cohesion [Pa]
    private float phi;      //Angle of Shear Resistance [rad]
    private float Kx;       //[m]
    private float Ky;       //[m]
    private float gamma_s;  //Density [kg/m^3]

    public SoilType(TerrainType _terrainType)
    {
        terrainType = _terrainType;


        switch (_terrainType)
        {
            case TerrainType.SandyBrendan:
                terrainName = _terrainType.ToString();
                k_1 = 2f;
                k_2 = 17659.75f;
                n = 0.77f;
                c = 130f;
                phi = 31.1f * Mathf.Deg2Rad;
                Kx = 0.038f;
                Ky = 0.038f;
                gamma_s = 1600f;
                break;
            case TerrainType.YoloLoamBrendan:
                terrainName = _terrainType.ToString();
                k_1 = 3.25f;
                k_2 = 4600f;
                n = 0.99f;
                c = 22670f;
                phi = 22f * Mathf.Deg2Rad;
                Kx = 0.015f;
                Ky = 0.015f;
                gamma_s = 1258f;
                break;
        }
    }

    public SoilType(string _terrainType, float _k1, float _k2, float _n, float _c, float _phi, float _KxKy, float _gamma_s)
    {
        terrainType = TerrainType.Custom;
        terrainName = _terrainType;
        k_1 = _k1;
        k_2 = _k2;
        n = _n;
        c = _c;
        phi = _phi * Mathf.Deg2Rad;
        Kx = _KxKy;
        Ky = _KxKy;
        gamma_s = _gamma_s;
    }

    public float EntryAngle()
    {
        // Insert fminsearch here, currently fixed.
        return Mathf.PI / 6;
    }
    public float ExitAngle()
    {
        // Fixed according to Brendan
        return -5f * Mathf.Deg2Rad;
    }


    public float ShearDisplacement(float angle, float tyreRadius, float slipRatio)
    {
        return tyreRadius * ((EntryAngle() - angle) - (1f - slipRatio) * (Mathf.Sin(EntryAngle()) - Mathf.Sin(angle)));
    }

    public float MaxShear(float normalPressure)
    {
        return c + normalPressure * Mathf.Tan(phi);
    }

    public float ShearStress(float normalPressure, float angle, float tyreRadius, float slipRatio)
    {
        return MaxShear(normalPressure) * (1f - Mathf.Exp(-ShearDisplacement(angle, tyreRadius, slipRatio) / Kx));
    }
 

    public float MaxRadialStressAngle(float slipRatio)
    {
        float k = Mathf.Tan(Mathf.PI / 4f - phi / 2f); // kappa/my

        float y = -(Mathf.Pow(k,2f) - Mathf.Sqrt(Mathf.Pow(k,4f) * Mathf.Pow(slipRatio, 2f) - 2 * Mathf.Pow(k,4f) * slipRatio + Mathf.Pow(k,4f) + Mathf.Pow(k,2f) * Mathf.Pow(slipRatio, 2) - 2 * Mathf.Pow(k,2f) * slipRatio)) / (k * (slipRatio - 1) * (Mathf.Pow(k,2f) + 1));
        float x = -(Mathf.Sqrt(Mathf.Pow(k,4f) * Mathf.Pow(slipRatio,2) - 2 * Mathf.Pow(k,4f) * slipRatio + Mathf.Pow(k,4f) + Mathf.Pow(k,2f) * Mathf.Pow(slipRatio, 2) - 2 * Mathf.Pow(k,2f) * slipRatio) + 1) / ((slipRatio - 1) * (Mathf.Pow(k,2) + 1));
        float angle = Mathf.Abs(Mathf.Atan2(Mathf.Abs(y), Mathf.Abs(x)));

        return Mathf.Min(Mathf.Max(angle,0f),(1f / 3f) * phi);
    }


    public float RadialShearStressFront(float angle, float entryAngle, float tyreRadius, float pressurePlateDimension)
    {
        return (c * k_1 + gamma_s * pressurePlateDimension * k_2) * Mathf.Pow((tyreRadius / pressurePlateDimension), n) * Mathf.Pow((Mathf.Cos(angle) - Mathf.Cos(entryAngle)), n);
    }

    public float RadialShearStressRear(float angle, float slipRatio, float tyreRadius, float pressurePlateDimension)
    {
        float var1 = c * k_1 + gamma_s * pressurePlateDimension * k_2;
        float var2 = Mathf.Pow((tyreRadius / pressurePlateDimension), n);
        float var3 = EntryAngle() - ((angle - EntryAngle()) / (MaxRadialStressAngle(slipRatio) - ExitAngle())) * (ExitAngle() - MaxRadialStressAngle(slipRatio));
        float var4 = Mathf.Pow((Mathf.Cos(var3) - Mathf.Cos(EntryAngle())), n);
        float sigma_nr = var1 * var2 * var4;
        return sigma_nr;

    }






    //public float BekkerPressureSinkage(float sinkage, float pressurePlateDimension)
    //{
    //    return (k_c / pressurePlateDimension + k_phi) * Mathf.Pow(sinkage, n);
    //}

    //public float BekkerUnloading(float sinkage, float unloadingSinkage, float unloadingPressure)
    //{
    //    float k_u = k_o + A_u * unloadingSinkage;
    //    return unloadingPressure - k_u * (unloadingSinkage - sinkage);
    //}

    //public float ReecePressureSinkage(float sinkage, float pressurePlateDimension)
    //{
    //    return (c * k_1 + gamma_s * pressurePlateDimension * k_2) * Mathf.Pow((sinkage / pressurePlateDimension), n);
    //}
}
