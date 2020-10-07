using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Soil class containing relevant soil parameters and all relevant terramechanical functions.
/// </summary>
/// 

public enum TerrainType
{
    LETESand,
    UplandSandyLoam,
    Custom,
}

public class SoilType
{
    public string terrainName;
    private TerrainType terrainType;  // Teerrain Type Name

    // Bekker Parameters
    private float n;        // Pressure-Sinkage Parameter []
    private float k_c;      // Pressure-Sinkage Parameter [kN/m^(n+1)]
    private float k_phi;    // Pressure-Sinkage Parameter [kN/m^(n+2)]

    private float k_o;      // Unloading/Reloading Terrain Parameter [kN/m^3]
    private float A_u;      // Unloading/Reloading Terrain Parameter [kN/m^4]

    // Reece Parameters
    private float k_1;
    private float k_2;
    private float gamma_s;

    private float c;        // Cohesion [kPa]
    private float phi;      // Angle of Shear Resistance [degrees]

    public SoilType(TerrainType _terrainType)
    {
        terrainType = _terrainType;


        switch (_terrainType)
        {
            case TerrainType.LETESand:
                terrainName = _terrainType.ToString();
                n = 0.7f;
                k_c = 1f;
                k_phi = 1f;
                k_1 = 1f;
                k_2 = 1f;
                gamma_s = 1f;
                c = 1f;
                phi = 1f * Mathf.Deg2Rad;
                break;
            case TerrainType.UplandSandyLoam:

                break;
        }
    }

    public SoilType(string _terrainType, float _n, float _k_c, float _k_phi, float _c, float _phi)
    {
        terrainType = TerrainType.Custom;
        terrainName = _terrainType;
        n = _n;
        k_c = _k_c;
        k_phi = _k_phi;
        c = _c;
        phi = _phi * Mathf.Deg2Rad;


    }


    public float MaxShear(float pressure)
    {
        return c + pressure * Mathf.Tan(phi);
    }

    public float BekkerPressureSinkage(float sinkage, float pressurePlateDimension)
    {
        return (k_c / pressurePlateDimension + k_phi) * Mathf.Pow(sinkage, n);
    }

    public float BekkerUnloading(float sinkage, float unloadingSinkage, float unloadingPressure)
    {
        float k_u = k_o + A_u * unloadingSinkage;
        return unloadingPressure - k_u * (unloadingSinkage - sinkage);
    }

    public float ReecePressureSinkage(float sinkage, float pressurePlateDimension)
    {
        return (c * k_1 + gamma_s * pressurePlateDimension * k_2) * Mathf.Pow((sinkage / pressurePlateDimension), n);
    }

    public float RadialShearStressFront(float angle, float entryAngle, float WheelRadiusUndeformed, float pressurePlateDimension)
    {
        return (c * k_1 + gamma_s * pressurePlateDimension * k_2) * Mathf.Pow((WheelRadiusUndeformed / pressurePlateDimension), n) * Mathf.Pow((Mathf.Cos(angle) - Mathf.Cos(entryAngle)), n);
    }
}
