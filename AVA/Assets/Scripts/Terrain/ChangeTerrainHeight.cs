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

    private static float[,] tireMark = { { .0025f, .005f, .0025f }, { .005f, .01f, .005f }, { .0025f, .005f, .0025f } };

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
            Vector3 wheelPos = wheelCollider.transform.position;
            Vector3 relativePos = wheelPos - terrainPos;

            Debug.Log($"Terrain Position: {terrainPos},  Wheel Position: {wheelPos},  Relative Position: {relativePos}");

            int xStart = (int)(relativePos[0] * stepSize);
            int yStart = (int)(relativePos[2] * stepSize);

            Debug.Log($"xStart: {xStart},  yStart: {yStart}");
            float[,] deltaDeform = new float[3,3];
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int xPos = xStart + j;
                    int yPos = yStart + i;
                    deltaDeform[j, i] = modifiedHeights[xPos, yPos] - tireMark[j, i];
                    Debug.Log(modifiedHeights[xPos, yPos] - tireMark[j, i]);
                }
            }




            terrainMain.terrainData.SetHeights(xStart, yStart, deltaDeform);

            modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
        }

        if (GUI.Button(new Rect(30, 60, 200, 30), "Reset Terrain"))
        {
            ResetTerrain();
        }
    }

    private void FixedUpdate()
    {
        Vector3 wheelPos = wheelCollider.transform.position;
        Vector3 relativePos = wheelPos - terrainPos;

        int xStart = (int)(relativePos[0] * stepSize);
        int yStart = (int)(relativePos[2] * stepSize);

        float[,] deltaDeform = new float[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int xPos = xStart + j;
                int yPos = yStart + i;
                deltaDeform[j, i] = modifiedHeights[xPos, yPos] - tireMark[j, i];
            }
        }

        terrainMain.terrainData.SetHeights(xStart, yStart, deltaDeform);

        modifiedHeights = terrainMain.terrainData.GetHeights(0, 0, xRes, yRes);
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
