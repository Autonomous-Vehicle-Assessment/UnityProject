using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class TerrainStruct
{
    public Terrain terrain;
    public float[,] originalHeights;
    public float[,] modifiedHeights;
    public float resolution;
    public float mapSize;
    public int mapRes;
    public Vector3 position;
    public Vector2Int id;
    public static int tiles;
    public const int tilesWidth = 10;
    public bool active;

    public TerrainStruct(Terrain _terrain, Material defaultMaterial)
    {
        terrain = _terrain;
        position = terrain.transform.position;

        mapRes = terrain.terrainData.heightmapResolution;
        
        // Store original terrain deformation and make copy for modified heights
        originalHeights = terrain.terrainData.GetHeights(0, 0, mapRes, mapRes);
        modifiedHeights = terrain.terrainData.GetHeights(0, 0, mapRes, mapRes);

        // Get the terrain heightmap physical size resolution
        mapSize = terrain.terrainData.size[0];
        resolution = mapRes / mapSize;

        id = new Vector2Int(tiles - (tiles / tilesWidth) * tilesWidth, (tiles/ tilesWidth));
        tiles++;

        active = false;
        terrain.materialTemplate = new Material(defaultMaterial);
    }

    public void SetColor(bool debug)
    {
        if (debug)
        {
            if (active)
            {
                if (terrain.materialTemplate.GetColor("_Color") != Color.green)
                {
                    terrain.materialTemplate.SetColor("_Color", Color.green);
                }
            }
            else
            {
                if (terrain.materialTemplate.GetColor("_Color") != Color.red)
                {
                    terrain.materialTemplate.SetColor("_Color", Color.red);
                }
            }
        }
        else
        {
            if (terrain.materialTemplate.GetColor("_Color") != Color.white)
            {
                terrain.materialTemplate.SetColor("_Color", Color.white);
            }
        }
    }

    public bool LocationCheck(WheelCollider wheelCollider)
    {
        Vector3 wheelPos = wheelCollider.transform.position;

        bool withinTerrain = (wheelPos.x <= position.x + mapSize && wheelPos.x >= position.x && wheelPos.z <= position.z + mapSize && wheelPos.z >= position.z);

        return withinTerrain;
    }
}


public class TerrainDeform : MonoBehaviour
{
    public GameObject terrainMaster;
    public List<TerrainStruct> terrains;

    public Material defaultMaterial;
    public bool debug;

    public GameObject wheelColliderMaster;
    private WheelCollider[] wheelColliders;

    [Space(10)]
    public float groundStiffness;
    [Range(0,5)]
    public float groundDamping;
    public float deformationStrength;

    private static float[,] tireMark = {    { .75f, .80f, .85f, .90f, .85f, .80f, .75f },
                                            { .80f, .85f, .90f, .95f, .90f, .85f, .80f },
                                            { .85f, .90f, .95f,  01f, .95f, .90f, .85f },
                                            { .90f, .95f,  01f,  01f,  01f, .95f, .90f },
                                            { .85f, .90f, .95f,  01f, .95f, .90f, .85f },
                                            { .80f, .85f, .90f, .95f, .90f, .85f, .80f },
                                            { .75f, .80f, .85f, .90f, .85f, .80f, .75f }, };
    private static float[,] tireMarkFull = {    
                                            { .95f,  01f, .95f},
                                            {  01f,  01f,  01f},
                                            { .95f,  01f, .95f}, };
    private static int brushSize = 7;

    private void Awake()
    {
        // Load all terrains from master
        Terrain[] tempTerrains = terrainMaster.GetComponentsInChildren<Terrain>();
        terrains = new List<TerrainStruct>();
        // get the heightmap points of the terrain, store values in a float array.
        foreach (Terrain terrain in tempTerrains)
        {
            terrains.Add(new TerrainStruct(terrain, defaultMaterial));
        }

        // Load all wheelcolliders from master
        wheelColliders = wheelColliderMaster.GetComponentsInChildren<WheelCollider>();

    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(30, 30, 200, 30), "Reset Terrain"))
        {
            ResetTerrain();
        }
    }

    private void FixedUpdate()
    {
        foreach (TerrainStruct terrainStruct in terrains)
        {
            terrainStruct.active = false;
        }

        foreach (WheelCollider wheelCollider in wheelColliders)
        {
            //WheelCollider wheelCollider = wheelColliders[0];
            // Find active terrain
            TerrainStruct activeTerrain = null;
            foreach (TerrainStruct terrainStruct in terrains)
            {
                if (terrainStruct.LocationCheck(wheelCollider))
                {
                    activeTerrain = terrainStruct;
                    terrainStruct.active = true;
                }
            }

            if (activeTerrain != null)
            {
                if (wheelCollider.GetGroundHit(out WheelHit hit))
                {
                    deformationStrength = DeformationStrength(wheelCollider, hit, activeTerrain);

                    Vector3 wheelPos = wheelCollider.transform.position;
                    Vector3 relativePos = wheelPos - activeTerrain.position;
                    int xStart = (int)(relativePos[0] * activeTerrain.resolution) - brushSize / 2;
                    int yStart = (int)(relativePos[2] * activeTerrain.resolution) - brushSize / 2;

                    
                    if (deformationStrength > 0.00001f)
                    {
                        float[,] deltaDeform = new float[brushSize, brushSize];
                        int xSize = 0;
                        int ySize = 0;

                        for (int i = 0; i < brushSize; i++)
                        {
                            for (int j = 0; j < brushSize; j++)
                            {
                                int xPos = xStart + j;
                                int yPos = yStart + i;

                                float deformationAmount = tireMark[j, i] * deformationStrength;

                                if (xPos <= activeTerrain.mapRes - 1 && xPos >= 0 && yPos <= activeTerrain.mapRes - 1 && yPos >= 0)
                                {
                                    deltaDeform[j, i] = activeTerrain.modifiedHeights[yPos, xPos] - deformationAmount;
                                    xSize++;
                                }
                            }
                        }


                        // activeTerrain.terrain.terrainData.SetHeightsDelayLOD(xStart, yStart, deltaDeform);

                        activeTerrain.modifiedHeights = activeTerrain.terrain.terrainData.GetHeights(0, 0, activeTerrain.mapRes, activeTerrain.mapRes);
                    }
                    
                }
            }
            
            
        }

        UpdateTerrains();
    }

    void OnApplicationQuit()
    {
        ResetTerrain();
    }

    private void UpdateTerrains()
    {
        foreach (TerrainStruct terrainStruct in terrains)
        {
            terrainStruct.terrain.terrainData.SyncHeightmap();
            terrainStruct.SetColor(debug);
        }
    }

    private void ResetTerrain()
    {
        foreach (TerrainStruct terrainStruct in terrains)
        {
            terrainStruct.terrain.terrainData.SetHeights(0, 0, terrainStruct.originalHeights);
        }
    }

    private TerrainStruct TerrainSelect(Vector2Int id)
    {
        int index = id[0] + id[1] * TerrainStruct.tilesWidth;
        return terrains[index];

    }


    public static T[,] SubArray<T>(this T[,] values, int row_min, int row_max, int col_min, int col_max)
    {
        // Allocate the result array.
        int num_rows = row_max - row_min + 1;
        int num_cols = col_max - col_min + 1;
        T[,] result = new T[num_rows, num_cols];

        // Get the number of columns in the values array.
        int total_cols = values.GetUpperBound(1) + 1;
        int from_index = row_min * total_cols + col_min;
        int to_index = 0;
        for (int row = 0; row <= num_rows - 1; row++)
        {
            Array.Copy(values, from_index, result, to_index, num_cols);
            from_index += total_cols;
            to_index += num_cols;
        }

        return result;
    }

    private float DeformationStrength(WheelCollider wheelCollider, WheelHit hit, TerrainStruct activeTerrain)
    {
        Vector3 wheelPos = wheelCollider.transform.position;
        Vector3 relativePos = wheelPos - activeTerrain.position;
        int xStart = (int)(relativePos[0] * activeTerrain.resolution) - brushSize / 2;
        int yStart = (int)(relativePos[2] * activeTerrain.resolution) - brushSize / 2;

        float originalHeight = 0;
        float modifiedHeight = 0;

        for (int i = 0; i < brushSize; i++)
        {
            for (int j = 0; j < brushSize; j++)
            {
                int xPos = xStart + j;
                int yPos = yStart + i;

                //Check if inside or on bounds
                if (xPos <= activeTerrain.mapRes - 1 && xPos >= 0 && yPos <= activeTerrain.mapRes - 1 && yPos >= 0)
                {
                    originalHeight += activeTerrain.originalHeights[yPos, xPos];
                    modifiedHeight += activeTerrain.modifiedHeights[yPos, xPos];
                }

                // Outside bounds
                else
                {
                    TerrainStruct neighbourTerrain;
                    Vector2Int activeid = activeTerrain.id;
                    int xPosNeighbor, yPosNeighbor;

                    // Current point East of the active terrain?
                    if (xPos > activeTerrain.mapRes - 1)
                    {

                        if (yPos > activeTerrain.mapRes - 1)
                        {
                            // Terrain to the NE
                            neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1] + 1));
                            xPosNeighbor = xPos - activeTerrain.mapRes + 1;
                            yPosNeighbor = yPos - activeTerrain.mapRes + 1;

                        }
                        else if (yPos < 0)
                        {
                            // Terrain to the SE
                            neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1] - 1));
                            xPosNeighbor = xPos - activeTerrain.mapRes + 1;
                            yPosNeighbor = yPos + activeTerrain.mapRes - 1;
                        }
                        else
                        {
                            // Terrain to the E
                            neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1]));
                            xPosNeighbor = xPos - activeTerrain.mapRes + 1;
                            yPosNeighbor = yPos;

                        }
                    }

                    // Current point West of the active terrain?
                    else if (xPos < 0)
                    {
                        if (yPos > activeTerrain.mapRes - 1)
                        {
                            // Terrain to the NW
                            neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1] + 1));
                            xPosNeighbor = xPos + activeTerrain.mapRes - 1;
                            yPosNeighbor = yPos - activeTerrain.mapRes + 1;

                        }
                        else if (yPos < 0)
                        {
                            // Terrain to the SW
                            neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1] - 1));
                            xPosNeighbor = xPos + activeTerrain.mapRes - 1;
                            yPosNeighbor = yPos + activeTerrain.mapRes - 1;
                        }
                        else
                        {
                            // Terrain to the W
                            neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1]));
                            xPosNeighbor = xPos + activeTerrain.mapRes - 1;
                            yPosNeighbor = yPos;

                        }

                    }

                    // Current point North of the active terrain?
                    else if (yPos > activeTerrain.mapRes - 1)
                    {
                        // Terrain N
                        neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] + 1));
                        xPosNeighbor = xPos;
                        yPosNeighbor = yPos - activeTerrain.mapRes + 1;
                    }

                    // Current point South of the active terrain?
                    else
                    {
                        // Terrain S
                        neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] - 1));
                        xPosNeighbor = xPos;
                        yPosNeighbor = yPos + activeTerrain.mapRes - 1;

                    }

                    neighbourTerrain.active = true;

                    originalHeight += neighbourTerrain.originalHeights[yPosNeighbor, xPosNeighbor];
                    modifiedHeight += neighbourTerrain.modifiedHeights[yPosNeighbor, xPosNeighbor];
                }
            }
        }

        originalHeight /= brushSize * brushSize;
        modifiedHeight /= brushSize * brushSize;

        float deltaModified = originalHeight - modifiedHeight;

        float impactForce = hit.force;
        float groundForce = deltaModified * groundStiffness;

        float deformationForce = impactForce - groundForce;

        return deformationStrength = Mathf.Max(0, (deformationForce / groundStiffness) * Time.deltaTime * groundDamping);
    }
}
