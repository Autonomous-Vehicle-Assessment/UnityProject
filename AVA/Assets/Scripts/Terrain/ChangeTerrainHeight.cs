using System;
using UnityEngine;

public class ChangeTerrainHeight : MonoBehaviour
{
    public Terrain terrainMain;
    private float[,] originalHeights;
    private float[,] modifiedHeights;
    private int xRes;
    private int yRes;
    private float width;
    private float length;
    private float stepSize;
    private Vector3 terrainPos;

    public WheelCollider wheelCollider;

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

    private void Awake()
    {
        // Get the terrain heightmap resolution width and length
        xRes = terrainMain.terrainData.heightmapResolution;
        yRes = terrainMain.terrainData.heightmapResolution;

        // Get the terrain heightmap physical width and length
        width = terrainMain.terrainData.size[0];
        length = terrainMain.terrainData.size[2];

        terrainPos = terrainMain.transform.position;

        stepSize = xRes / width;

        // get the heightmap points of the terrain, store values in a float array.
        
        originalHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
        modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
    }
    private void OnGUI()
    {
        if(GUI.Button(new Rect(30,30,200,30),"Change Terrain Height"))
        {
            float[,] deltaDeform = new float[3,3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int xPos = 0 + j;
                    int yPos = 0 + i;
                    deltaDeform[j, i] = modifiedHeights[xPos, yPos] - tireMark[j, i] * 0.04f;
                }
            }

            terrainMain.terrainData.SetHeights(0, 0, deltaDeform);

            modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
        }

        if (GUI.Button(new Rect(30, 70, 200, 30), "Deform under Tire"))
        {
            Vector3 wheelPos = wheelCollider.transform.position;
            Vector3 relativePos = wheelPos - terrainPos;

            int xStart = (int)(relativePos[0] * stepSize) - 1;
            int yStart = (int)(relativePos[2] * stepSize) - 1;

            float[,] deltaDeform = new float[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int xPos = xStart + j;
                    int yPos = yStart + i;
                    deltaDeform[j, i] = modifiedHeights[xPos, yPos] - tireMark[j, i] * 0.04f;
                }
            }

            terrainMain.terrainData.SetHeights(xStart, yStart, deltaDeform);

            modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);

        }

        if (GUI.Button(new Rect(30, 110, 200, 30), "Reset Terrain"))
        {
            ResetTerrain();
        }
    }

    private void FixedUpdate()
    {
        Vector3 wheelPos = wheelCollider.transform.position;
        Vector3 relativePos = wheelPos - terrainPos;

        if (wheelCollider.GetGroundHit(out WheelHit hit))
        {
            int xStart = (int)(relativePos[0] * stepSize) - 3;
            int yStart = (int)(relativePos[2] * stepSize) - 3;

            float originalHeight = 0;
            float modifiedHeight = 0;

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    int xPos = xStart + j;
                    int yPos = yStart + i;
                    originalHeight += originalHeights[yPos, xPos];
                    modifiedHeight += modifiedHeights[yPos, xPos];
                }
            }
            originalHeight /= 49;
            modifiedHeight /= 49;

            float deltaModified = originalHeight - modifiedHeight;

            float impactForce = hit.force;
            float groundForce = deltaModified * groundStiffness;

            float deformationForce = impactForce - groundForce;

            deformationStrength = Mathf.Max(0, (deformationForce / groundStiffness) * Time.deltaTime * groundDamping);

            if(deformationStrength > 0.00001f)
            {
                float[,] deltaDeform = new float[7, 7];
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        int xPos = xStart + j;
                        int yPos = yStart + i;

                        float deformationAmount = tireMark[j, i] * deformationStrength;
                        deltaDeform[j, i] = modifiedHeights[yPos, xPos] - deformationAmount;
                    }
                }

                
                terrainMain.terrainData.SetHeights(xStart, yStart, deltaDeform);
                
                
                

                modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
            }
        }        
    }

    private void LateUpdate()
    {

    }

    void OnApplicationQuit()
    {
        ResetTerrain();
    }

    private void ResetTerrain()
    {
        terrainMain.terrainData.SetHeights(0, 0, originalHeights);
        modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
    }

    // private void 
}
