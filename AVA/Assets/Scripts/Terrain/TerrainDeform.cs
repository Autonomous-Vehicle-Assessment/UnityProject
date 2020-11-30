using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class TerrainStruct
{
    public Terrain terrain;
    public float[,] originalHeights;
    public float[,] modifiedHeights;
    public float[,] deltaHeights;
    public float resolution;
    public float mapSize;
    public int mapRes;
    public Vector3 position;
    public Vector2Int id;
    public static int tiles;
    public const int tilesWidth = 22;
    public bool active;
    public bool enabled;

    public TerrainStruct(Terrain _terrain, Material defaultMaterial)
    {
        terrain = _terrain;
        position = terrain.transform.position;

        mapRes = terrain.terrainData.heightmapResolution;
        
        // Store original terrain deformation and make copy for modified heights
        originalHeights = terrain.terrainData.GetHeights(0, 0, mapRes, mapRes);
        modifiedHeights = originalHeights;
        deltaHeights = originalHeights;

        // Get the terrain heightmap physical size resolution
        mapSize = terrain.terrainData.size[0];
        resolution = mapRes / mapSize;

        id = new Vector2Int(tiles - (tiles / tilesWidth) * tilesWidth, (tiles/ tilesWidth));
        tiles++;

        active = false;
        enabled = true;
        terrain.materialTemplate = new Material(defaultMaterial);
    }

    public TerrainStruct()
    {
        enabled = false;
        id = new Vector2Int(tiles - (tiles / tilesWidth) * tilesWidth, (tiles / tilesWidth));
        tiles++;
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
    [Range(1,10)]
    public float groundDamping;
    public float minDeformation;

    public bool deformTerrain;
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
        int index = 0;
        float[,] height = new float[65, 65];
        float heightConstant = 0.8f;
        for (int row = 0; row < 65; row++)
        {
            for (int coloumn = 0; coloumn < 65; coloumn++)
            {
                height[row, coloumn] = heightConstant;
            }
        }

        TerrainStruct.tiles = 0;

        foreach (Terrain terrain in tempTerrains)
        {
            if (terrain.GetComponent<Terrain>().isActiveAndEnabled)
            {
                TerrainData terrainData = new TerrainData();
                terrainData.name = "terrain" + index.ToString();
                terrainData.heightmapResolution = 65;
                terrainData.size = new Vector3(4, 10, 4);
                terrainData.SetHeights(0, 0, height);

                terrain.terrainData = terrainData;
                terrain.GetComponent<TerrainCollider>().terrainData = terrainData;
                terrains.Add(new TerrainStruct(terrain, defaultMaterial));
                index++;
            }
            else
            {
                terrains.Add(new TerrainStruct());
            }
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
            if(terrainStruct.enabled) terrainStruct.active = false;
        }

        foreach (WheelCollider wheelCollider in wheelColliders)
        {
            //WheelCollider wheelCollider = wheelColliders[1];

            // Find active terrain
            TerrainStruct activeTerrain = null;
            foreach (TerrainStruct terrainStruct in terrains)
            {
                if (terrainStruct.enabled) {
                    if (terrainStruct.LocationCheck(wheelCollider))
                    {
                        activeTerrain = terrainStruct;
                        terrainStruct.active = true;
                    }
                }
            }

            // Deformation of terrain vertices
            if (activeTerrain != null && deformTerrain)
            {
                if (wheelCollider.GetGroundHit(out WheelHit hit))
                {
                    deformationStrength = DeformationStrength(wheelCollider, hit, activeTerrain);

                    Vector3 wheelPos = wheelCollider.transform.position;
                    Vector3 relativePos = wheelPos - activeTerrain.position;
                    int xStart = (int)(relativePos[0] * activeTerrain.resolution) - brushSize / 2;
                    int yStart = (int)(relativePos[2] * activeTerrain.resolution) - brushSize / 2;                    

                    Vector2Int activeid = activeTerrain.id;
                    bool edgeCase = false;
                    bool cornerCase = false;

                    // ------------------------------------------------ //
                    //          Deformation on Active terrains          //
                    // ------------------------------------------------ //
                    if (deformationStrength > minDeformation)
                    {
                        for (int i = 0; i < brushSize; i++)
                        {
                            for (int j = 0; j < brushSize; j++)
                            {
                                int xPos = xStart + j;
                                int yPos = yStart + i;

                                float deformationAmount = tireMark[j, i] * deformationStrength;

                                // Deformation within active tile
                                if (xPos <= activeTerrain.mapRes - 1 && xPos >= 0 && yPos <= activeTerrain.mapRes - 1 && yPos >= 0)
                                {
                                    activeTerrain.modifiedHeights[yPos, xPos] -= deformationAmount;
                                }

                                // Set flag if on North/South/East/West edge or corner case
                                if (xPos >= activeTerrain.mapRes - 1)
                                {
                                    if (yPos >= activeTerrain.mapRes - 1)
                                    {
                                        // Terrain to the NE
                                        cornerCase = true;
                                    }
                                    else if (yPos <= 0)
                                    {
                                        // Terrain to the SE
                                        cornerCase = true;

                                    }
                                    else
                                    {
                                        // Terrain to the E
                                        edgeCase = true;

                                    }
                                }
                                else if (xPos <= 0)
                                {
                                    if (yPos >= activeTerrain.mapRes - 1)
                                    {
                                        // Terrain to the NW
                                        cornerCase = true;
                                    }
                                    else if (yPos <= 0)
                                    {
                                        // Terrain to the SW
                                        cornerCase = true;
                                    }
                                    else
                                    {
                                        // Terrain to the W
                                        edgeCase = true;
                                    }

                                }
                                else if (yPos >= activeTerrain.mapRes - 1)
                                {
                                    // Terrain N
                                    edgeCase = true;
                                }
                                else if (yPos <= 0)
                                {
                                    // Terrain S
                                    edgeCase = true;
                                }

                            }
                        }

                        activeTerrain.terrain.terrainData.SetHeightsDelayLOD(0, 0, activeTerrain.modifiedHeights);
                        activeTerrain.modifiedHeights = activeTerrain.terrain.terrainData.GetHeights(0, 0, activeTerrain.mapRes, activeTerrain.mapRes);
                    }



                    // ------------------------------------------------ //
                    //         Deformation on neighbour terrains        //
                    // ------------------------------------------------ //
                    if (deformationStrength > minDeformation)
                    {
                        // Edge neighbour terrain
                        if (edgeCase && !cornerCase)
                        {
                            TerrainStruct neighbourTerrain = null;
                            for (int i = 0; i < brushSize; i++)
                            {
                                for (int j = 0; j < brushSize; j++)
                                {
                                    int xPos = xStart + j;
                                    int yPos = yStart + i;
                                    bool neighbourFlag = false;

                                    float deformationAmount = tireMark[j, i] * deformationStrength;

                                    // Deformation on neighbour terrains
                                    int xPosNeighbor = 0;
                                    int yPosNeighbor = 0;

                                    // Current point East of the active terrain?
                                    if (xPos >= activeTerrain.mapRes - 1)
                                    {
                                        neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1]));
                                        xPosNeighbor = xPos - activeTerrain.mapRes + 1;
                                        yPosNeighbor = yPos;
                                        neighbourFlag = true;
                                    }

                                    // Current point West of the active terrain?
                                    else if (xPos <= 0)
                                    {
                                        neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1]));
                                        xPosNeighbor = xPos + activeTerrain.mapRes - 1;
                                        yPosNeighbor = yPos;
                                        neighbourFlag = true;
                                    }

                                    // Current point North of the active terrain?
                                    else if (yPos >= activeTerrain.mapRes - 1)
                                    {
                                        // Terrain N
                                        neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] + 1));
                                        xPosNeighbor = xPos;
                                        yPosNeighbor = yPos - activeTerrain.mapRes + 1;
                                        neighbourFlag = true;
                                    }

                                    // Current point South of the active terrain?
                                    else if (yPos <= 0)
                                    {
                                        // Terrain S
                                        neighbourTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] - 1));
                                        xPosNeighbor = xPos;
                                        yPosNeighbor = yPos + activeTerrain.mapRes - 1;
                                        neighbourFlag = true;
                                    }

                                    if (neighbourFlag)
                                    {
                                        if(neighbourTerrain.enabled) neighbourTerrain.modifiedHeights[yPosNeighbor, xPosNeighbor] -= deformationAmount;
                                    }
                                }
                            }


                            // Apply deformation
                            if (neighbourTerrain.enabled)
                            {
                                neighbourTerrain.terrain.terrainData.SetHeightsDelayLOD(0, 0, neighbourTerrain.modifiedHeights);
                                neighbourTerrain.modifiedHeights = neighbourTerrain.terrain.terrainData.GetHeights(0, 0, activeTerrain.mapRes, activeTerrain.mapRes);
                                neighbourTerrain.active = true;
                            }
                        }

                        // Corner neighbour terrain
                        else if (cornerCase)
                        {
                            // Corner
                            TerrainStruct cornerTerrain = null;
                            TerrainStruct northSouthTerrain = null;
                            TerrainStruct eastWestTerrain = null;

                            for (int i = 0; i < brushSize; i++)
                            {
                                for (int j = 0; j < brushSize; j++)
                                {
                                    int xPos = xStart + j;
                                    int yPos = yStart + i;

                                    bool cornerFlag = false;
                                    bool eastwestFlag = false;
                                    bool northsouthFlag = false;

                                    float deformationAmount = tireMark[j, i] * deformationStrength;

                                    // Deformation on neighbour terrains
                                    int xPosNeighbor = 0;
                                    int yPosNeighbor = 0;
                                    int xPosNeighborNS = 0;
                                    int yPosNeighborNS = 0;
                                    int xPosNeighborEW = 0;
                                    int yPosNeighborEW = 0;

                                    // Current point East of the active terrain
                                    if (xPos >= activeTerrain.mapRes - 1)
                                    {
                                        // Terrain to the North East
                                        if (yPos >= activeTerrain.mapRes - 1)
                                        {
                                            cornerTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1] + 1));
                                            xPosNeighbor = xPos - activeTerrain.mapRes + 1;
                                            yPosNeighbor = yPos - activeTerrain.mapRes + 1;
                                            cornerFlag = true;

                                            // Terrain to the North
                                            if (xPos == activeTerrain.mapRes - 1)
                                            {
                                                northSouthTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] + 1));
                                                xPosNeighborNS = xPos;
                                                yPosNeighborNS = yPos - activeTerrain.mapRes + 1;
                                                northsouthFlag = true;
                                            }

                                            // Terrain to the East
                                            if (yPos == activeTerrain.mapRes - 1)
                                            {
                                                eastWestTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1]));
                                                xPosNeighborEW = xPos - activeTerrain.mapRes + 1;
                                                yPosNeighborEW = yPos;
                                                eastwestFlag = true;
                                            }
                                        }

                                        // Terrain to the South East
                                        else if (yPos <= 0)
                                        {
                                            cornerTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1] - 1));
                                            xPosNeighbor = xPos - activeTerrain.mapRes + 1;
                                            yPosNeighbor = yPos + activeTerrain.mapRes - 1;
                                            cornerFlag = true;

                                            // Terrain to the South
                                            if (xPos == activeTerrain.mapRes - 1)
                                            {
                                                northSouthTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] - 1));
                                                xPosNeighborNS = xPos;
                                                yPosNeighborNS = yPos + activeTerrain.mapRes - 1;
                                                northsouthFlag = true;
                                            }

                                            // Terrain to the E
                                            if (yPos == 0)
                                            {
                                                eastWestTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1]));
                                                xPosNeighborEW = xPos - activeTerrain.mapRes + 1;
                                                yPosNeighborEW = yPos;
                                                eastwestFlag = true;
                                            }
                                        }

                                        // Terrain to the East
                                        else
                                        {
                                            eastWestTerrain = TerrainSelect(new Vector2Int(activeid[0] + 1, activeid[1]));
                                            xPosNeighborEW = xPos - activeTerrain.mapRes + 1;
                                            yPosNeighborEW = yPos;
                                            eastwestFlag = true;
                                        }
                                    }

                                    // Current point West of the active terrain
                                    else if (xPos <= 0)
                                    {
                                        // Terrain to the North West
                                        if (yPos >= activeTerrain.mapRes - 1)
                                        {
                                            cornerTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1] + 1));
                                            xPosNeighbor = xPos + activeTerrain.mapRes - 1;
                                            yPosNeighbor = yPos - activeTerrain.mapRes + 1;
                                            cornerFlag = true;

                                            // Terrain to the North
                                            if (xPos == 0)
                                            {
                                                northSouthTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] + 1));
                                                xPosNeighborNS = xPos;
                                                yPosNeighborNS = yPos - activeTerrain.mapRes + 1;
                                                northsouthFlag = true;
                                            }

                                            // Terrain to the west
                                            if (yPos == activeTerrain.mapRes - 1)
                                            {
                                                eastWestTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1]));
                                                xPosNeighborEW = xPos + activeTerrain.mapRes - 1;
                                                yPosNeighborEW = yPos;
                                                eastwestFlag = true;
                                            }
                                        }

                                        // Terrain to the South West
                                        else if (yPos <= 0)
                                        {
                                            cornerTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1] - 1));
                                            xPosNeighbor = xPos + activeTerrain.mapRes - 1;
                                            yPosNeighbor = yPos + activeTerrain.mapRes - 1;
                                            cornerFlag = true;

                                            // Terrain to the South
                                            if (xPos == 0)
                                            {
                                                northSouthTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] - 1));
                                                xPosNeighborNS = xPos;
                                                yPosNeighborNS = yPos + activeTerrain.mapRes - 1;
                                                northsouthFlag = true;
                                            }

                                            // Terrain to the West
                                            if (yPos == 0)
                                            {
                                                eastWestTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1]));
                                                xPosNeighborEW = xPos + activeTerrain.mapRes - 1;
                                                yPosNeighborEW = yPos;
                                                eastwestFlag = true;
                                            }
                                        }

                                        // Terrain to the west
                                        else
                                        {
                                            eastWestTerrain = TerrainSelect(new Vector2Int(activeid[0] - 1, activeid[1]));
                                            xPosNeighborEW = xPos + activeTerrain.mapRes - 1;
                                            yPosNeighborEW = yPos;
                                            eastwestFlag = true;
                                        }
                                    }

                                    // Current point North/South of active terrain
                                    else
                                    {
                                        // Terrain to the North
                                        if (yPos >= activeTerrain.mapRes - 1)
                                        {
                                            northSouthTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] + 1));
                                            xPosNeighborNS = xPos;
                                            yPosNeighborNS = yPos - activeTerrain.mapRes + 1;
                                            northsouthFlag = true;
                                        }
                                        // Terrain to the South
                                        else if (yPos <= 0)
                                        {
                                            northSouthTerrain = TerrainSelect(new Vector2Int(activeid[0], activeid[1] - 1));
                                            xPosNeighborNS = xPos;
                                            yPosNeighborNS = yPos + activeTerrain.mapRes - 1;
                                            northsouthFlag = true;
                                        }
                                    }

                                    // Update deformations
                                    if (cornerFlag)
                                    {
                                        if(cornerTerrain.enabled) cornerTerrain.modifiedHeights[yPosNeighbor, xPosNeighbor] -= deformationAmount;
                                    }

                                    if (northsouthFlag)
                                    {
                                        if(northSouthTerrain.enabled) northSouthTerrain.modifiedHeights[yPosNeighborNS, xPosNeighborNS] -= deformationAmount;
                                    }

                                    if (eastwestFlag)
                                    {
                                        if(eastWestTerrain.enabled) eastWestTerrain.modifiedHeights[yPosNeighborEW, xPosNeighborEW] -= deformationAmount;
                                    }
                                }
                            }

                            // Apply deformations
                            if (cornerTerrain.enabled)
                            {
                                cornerTerrain.terrain.terrainData.SetHeightsDelayLOD(0, 0, cornerTerrain.modifiedHeights);
                                cornerTerrain.modifiedHeights = cornerTerrain.terrain.terrainData.GetHeights(0, 0, activeTerrain.mapRes, activeTerrain.mapRes);
                                cornerTerrain.active = true;
                            }

                            if (northSouthTerrain.enabled)
                            {
                                northSouthTerrain.terrain.terrainData.SetHeightsDelayLOD(0, 0, northSouthTerrain.modifiedHeights);
                                northSouthTerrain.modifiedHeights = northSouthTerrain.terrain.terrainData.GetHeights(0, 0, activeTerrain.mapRes, activeTerrain.mapRes);
                                northSouthTerrain.active = true;
                            }

                            if (eastWestTerrain.enabled)
                            {
                                eastWestTerrain.terrain.terrainData.SetHeightsDelayLOD(0, 0, eastWestTerrain.modifiedHeights);
                                eastWestTerrain.modifiedHeights = eastWestTerrain.terrain.terrainData.GetHeights(0, 0, activeTerrain.mapRes, activeTerrain.mapRes);
                                eastWestTerrain.active = true;
                            }
                        }
                    }

                    UpdateTerrains();
                }
            }

        }

    }

    void OnApplicationQuit()
    {
        ResetTerrain();
    }

    private void UpdateTerrains()
    {
        foreach (TerrainStruct terrainStruct in terrains)
        {
            if (terrainStruct.enabled)
            {
                terrainStruct.terrain.terrainData.SyncHeightmap();
                terrainStruct.SetColor(debug);
            }
        }
    }

    private void ResetTerrain()
    {
        foreach (TerrainStruct terrainStruct in terrains)
        {
            if (terrainStruct.enabled)
            {
                terrainStruct.terrain.terrainData.SetHeights(0, 0, terrainStruct.originalHeights);
                terrainStruct.modifiedHeights = terrainStruct.terrain.terrainData.GetHeights(0, 0, terrainStruct.mapRes, terrainStruct.mapRes);
                terrainStruct.originalHeights = terrainStruct.terrain.terrainData.GetHeights(0, 0, terrainStruct.mapRes, terrainStruct.mapRes);
            }
        }
    }

    private TerrainStruct TerrainSelect(Vector2Int id)
    {
        int index = id[0] + id[1] * TerrainStruct.tilesWidth;
        return terrains[index];

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

                    // Todo: Average height take into account surrounding deformations.
                    if (neighbourTerrain.enabled)
                    {
                        originalHeight += neighbourTerrain.originalHeights[yPosNeighbor, xPosNeighbor];
                        modifiedHeight += neighbourTerrain.modifiedHeights[yPosNeighbor, xPosNeighbor];
                    }
                }
            }
        }

        originalHeight /= brushSize * brushSize;
        modifiedHeight /= brushSize * brushSize;

        float deltaModified = originalHeight - modifiedHeight;

        float impactForce = hit.force;
        float groundForce = deltaModified * groundStiffness;

        float deformationForce = impactForce - groundForce;

        return deformationStrength = Mathf.Max(0, (deformationForce / groundStiffness) * Time.deltaTime / (Time.deltaTime * groundDamping));
    }
}
