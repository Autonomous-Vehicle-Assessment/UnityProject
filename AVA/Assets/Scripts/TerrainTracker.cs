using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTracker : MonoBehaviour
{
    public int surfaceIndex = 0;
    public float[] surfaceValues;

    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPos;

    // Use this for initialization
    void Start()
    {

        terrain = Terrain.activeTerrain;
        terrainData = terrain.terrainData;
        terrainPos = terrain.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        surfaceIndex = GetMainTexture(transform.position);
        surfaceValues = GetTextureMix(transform.position);
    }

    private float[] GetTextureMix(Vector3 WorldPos)
    {
        // returns an array containing the relative mix of textures
        // on the main terrain at this world position.

        // The number of values in the array will equal the number
        // of textures added to the terrain.

        // calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // extract the 3D array data to a 1D array:
        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

        for (int n = 0; n < cellMix.Length; n++)
        {
            cellMix[n] = splatmapData[0, 0, n];
        }
        return cellMix;
    }

    private int GetMainTexture(Vector3 WorldPos)
    {
        // returns the zero-based index of the most dominant texture
        // on the main terrain at this world position.
        float[] mix = GetTextureMix(WorldPos);

        float maxMix = 0;
        int maxIndex = 0;

        // loop through each mix value and find the maximum
        for (int n = 0; n < mix.Length; n++)
        {
            if (mix[n] > maxMix)
            {
                maxIndex = n;
                maxMix = mix[n];
            }
        }
        return maxIndex;
    }
    /*private void Update()
    {
        // Bit shift the index of the layer (11) to get a bit mask
        int layerMask = 1 << 11;

        // This would cast rays only against colliders in layer 11.
        // But instead we want to collide against everything except layer 11. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            Debug.Log(hit.collider.gameObject.GetComponent<Terrain>().);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
        
    }
    */
}
