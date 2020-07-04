using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTerrainHeight : MonoBehaviour
{
    public Terrain TerrainMain;
    // Start is called before the first frame update
    private void OnGUI()
    {
        if(GUI.Button(new Rect(30,30,200,30),"Change Terrain Height"))
        {

            // Get the terrain heightmap width and height
            int xRes = TerrainMain.terrainData.heightmapResolution;
            int yRes = TerrainMain.terrainData.heightmapResolution;

            // get the heightmap points of the terrain, store values in a float array.
            float[,] heights = TerrainMain.terrainData.GetHeights(0, 0, xRes, yRes);

        }
    }
}
