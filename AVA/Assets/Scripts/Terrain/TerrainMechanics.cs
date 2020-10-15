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

    }
    public void FixedUpdate()
    {
        //Debug.Log(SoilList[0].VerticalStress(20f*Mathf.Deg2Rad, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio));
        //float result = SoilList[0].VerticalStress(SoilList[0].EntryAngle() - 0.01f, tyreWidth, tyreRadius, pressurePlateMainDimension, slipRatio);

    }
}


