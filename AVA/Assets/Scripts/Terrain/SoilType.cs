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
    private const float DeltaT_desired = 0.001f; //[rad]

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

    private float VerticalStress(float theta_e, float tyreWidth, float tyreRadius, float pressurePlateMainDimension, float slipRatio)
    {
        float sum = 0;
        float theta_tot = theta_e - ExitAngle();
        int Steps = (int)(theta_tot / DeltaT_desired + 1);
        float DeltaT = theta_tot / (Steps - 1);

        for (int i = 0; i < Steps; i++)
        {
            float theta = ExitAngle() + DeltaT * i;
            float normalPressure = RadialStress(theta, slipRatio, tyreRadius, pressurePlateMainDimension);
            if (theta < 0)
            {
                sum -= (normalPressure * Mathf.Cos(theta) + ShearStress(normalPressure, theta, tyreRadius, slipRatio) * Mathf.Sin(theta)) * DeltaT;
            }
            else
            {
                sum += (normalPressure * Mathf.Cos(theta) - ShearStress(normalPressure, theta, tyreRadius, slipRatio) * Mathf.Sin(theta)) * DeltaT;
            }
        }
        return (tyreWidth * tyreRadius) / 2 * sum;
    }

    private float LineSearch(float delta, int kmax, float tol, float tyreWidth, float tyreRadius, float pressurePlateMainDimension, float slipRatio)
    {
        int n = 0; 
        float step = delta;
        while (VerticalStress(0f, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) < VerticalStress(step, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) && n <kmax)
        {
            step /= 2f;
            n++;
        }


        /// Phase 1 - Golden Section Search ///
        // Golden section search values for first iteration (0 as lower bracket)
        int q = 0;  // Golden section search exponent
        float alpha_l = 0;
        float alpha_i = Alpha(q, step);
        float alpha_u = Alpha(q + 1, step);

        // Golden section loop for bracketing of the optimum point
        n = 1;
        float VerticalStress0 = VerticalStress(Alpha(q, step), tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        float VerticalStress1 = VerticalStress(Alpha(q + 1, step), tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        while (n < kmax && VerticalStress0  > VerticalStress1)
        {
            q++;
            alpha_l = Alpha(q - 1, step);
            alpha_i = Alpha(q, step);
            alpha_u = Alpha(q + 1, step);
            n++;
        }

        // If flat optimum, make one more iteration
        VerticalStress0 = VerticalStress(Alpha(q, step), tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        VerticalStress1 = VerticalStress(Alpha(q + 1, step), tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        if (VerticalStress0 == VerticalStress1)
        {
            q++;
            alpha_l = Alpha(q - 1, step);
            alpha_i = Alpha(q, step);
            alpha_u = Alpha(q + 1, step);
        }

        if (n == kmax)
        {
            Debug.Log("No Brackets found in Linesearch");
        }

        /// Phase 2 - Polynomial Interpolation ///
        n = 1;

        // Check initial polynomial point
        // Calculates top point of interpolated polynomial based on a1 and a2
        float alpha_bar = -A1(alpha_l, alpha_i, alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) 
                    / (2 * A2(alpha_l, alpha_i, alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio));

        // Deviation between top point and intermediate point
        float dev = Mathf.Abs(alpha_bar - alpha_i);

        if (VerticalStress(alpha_l, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) == VerticalStress(alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio))
        {
            alpha_bar = alpha_l; // Returns first minimum point
        }
        else
        {
            while (n < kmax && dev > tol)
            {
                n++;
                alpha_bar = -A1(alpha_l, alpha_i, alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio)
                / (2 * A2(alpha_l, alpha_i, alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio));
                dev = Mathf.Abs(alpha_bar - alpha_i);

                if (alpha_i < alpha_bar)   // Step 4
                {
                    if (VerticalStress(alpha_i, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) < VerticalStress(alpha_bar, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio))   // Step 4a
                    {
                        alpha_u = alpha_bar;
                    }
                    else        // Step 4b
                    {
                        alpha_l = alpha_i;
                        alpha_i = alpha_bar;
                    }
                }
                else    // Step 5
                {
                    if (VerticalStress(alpha_i, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) < VerticalStress(alpha_bar, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio))   // Step 5a
                    {
                        alpha_l = alpha_bar;
                    }
                    else        // Step 5b
                    {
                        alpha_u = alpha_i;
                        alpha_i = alpha_bar;
                    }
                }
            }
        }

        float finalStepSize = alpha_bar;


        float theta_e = 20f * Mathf.Deg2Rad;
        return theta_e;
    }

    private float Alpha(float q, float delta)
    {
        float alpha_q = delta;
        for (int i = 0; i < q; i++)
        {
            alpha_q += delta * Mathf.Pow((1 + Mathf.Sqrt(5))/2f,i);
        }
        return alpha_q;
    }

    private float A1(float alpha_l, float alpha_i, float alpha_u, float tyreWidth, float tyreRadius, float pressurePlateMainDimension, float slipRatio)
    {
        float f_l = VerticalStress(alpha_l, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        float f_i = VerticalStress(alpha_i, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        float f_u = VerticalStress(alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        return (f_i - f_l) / (alpha_i - alpha_l) 
            - A2(alpha_l, alpha_i, alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio) 
            * (alpha_l + alpha_i);
    }

    private float A2(float alpha_l, float alpha_i, float alpha_u, float tyreWidth, float tyreRadius, float pressurePlateMainDimension, float slipRatio)
    {
        float f_l = VerticalStress(alpha_l, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        float f_i = VerticalStress(alpha_i, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        float f_u = VerticalStress(alpha_u, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);
        return 1 / (alpha_u - alpha_i) * ((f_u - f_l) / (alpha_u - alpha_l) - (f_i - f_l) / (alpha_i - alpha_l));
    }










    public float EntryAngle(float delta, int kmax, float tol, float tyreWidth, float tyreRadius, float pressurePlateMainDimension, float slipRatio)
    {
        // Insert fminsearch here, currently fixed.
        //return Mathf.PI / 6;

        float theta_e = LineSearch(delta, kmax, tol, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);

        //float theta_e = 20f * Mathf.Deg2Rad;// FindMinimum();
        return theta_e;
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

        float y = -(Mathf.Pow(k, 2f) - Mathf.Sqrt(Mathf.Pow(k, 4f) * Mathf.Pow(slipRatio, 2f) - 2 * Mathf.Pow(k, 4f) 
            * slipRatio + Mathf.Pow(k, 4f) + Mathf.Pow(k, 2f) * Mathf.Pow(slipRatio, 2) - 2 * Mathf.Pow(k, 2f) 
            * slipRatio)) / (k * (slipRatio - 1) * (Mathf.Pow(k, 2f) + 1));
        float x = -(Mathf.Sqrt(Mathf.Pow(k,4f) * Mathf.Pow(slipRatio,2) - 2 * Mathf.Pow(k,4f) * slipRatio 
            + Mathf.Pow(k,4f) + Mathf.Pow(k,2f) * Mathf.Pow(slipRatio, 2) - 2 * Mathf.Pow(k,2f) * slipRatio) + 1) 
            / ((slipRatio - 1) * (Mathf.Pow(k,2) + 1));

        float angle = Mathf.Abs(Mathf.Atan2(Mathf.Abs(y), Mathf.Abs(x)));

        return Mathf.Min(Mathf.Max(angle,0f),(1f / 3f) * phi);
    }


    private float RadialStressFront(float angle, float tyreRadius, float pressurePlateMainDimension)
    {
        return (c * k_1 + gamma_s * pressurePlateMainDimension * k_2) * Mathf.Pow((tyreRadius / pressurePlateMainDimension), n) 
            * Mathf.Pow((Mathf.Cos(angle) - Mathf.Cos(EntryAngle())), n);
    }

    private float RadialStressRear(float angle, float slipRatio, float tyreRadius, float pressurePlateMainDimension)
    {
        float sigma_nr = (c * k_1 + gamma_s * pressurePlateMainDimension * k_2) 
            * Mathf.Pow(tyreRadius / pressurePlateMainDimension, n) 
            * Mathf.Pow(Mathf.Cos(EntryAngle() - (angle - ExitAngle()) / (MaxRadialStressAngle(slipRatio) - ExitAngle()) 
            * (EntryAngle() - MaxRadialStressAngle(slipRatio))) - Mathf.Cos(EntryAngle()), n);

        return sigma_nr;
    }

    public float RadialStress(float angle, float slipRatio, float tyreRadius, float pressurePlateMainDimension)
    {
        float sigma_n = 0f;

        if (angle <= EntryAngle() && angle >= ExitAngle())
        {
            //Debug.Log("Inside angles");
            if (angle >= MaxRadialStressAngle(slipRatio))
            {
                //Debug.Log("Angle is in front of Max");
                sigma_n = RadialStressFront(angle, tyreRadius, pressurePlateMainDimension);
            }
            else
            {
                //Debug.Log("Angle is in behind of Max");
                sigma_n = RadialStressRear(angle, slipRatio, tyreRadius, pressurePlateMainDimension);
            }
        }
        return sigma_n;
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
