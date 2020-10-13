using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class TerrainMechanics : MonoBehaviour
{
    private List<SoilType> SoilList;

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
        Debug.Log(SoilList[0].terrainName);
    }
}


