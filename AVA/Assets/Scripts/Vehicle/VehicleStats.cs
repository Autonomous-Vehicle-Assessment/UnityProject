using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(DataLogging))]
public class VehicleStats : MonoBehaviour
{
    [Header("Vehicle State")]
    public SpeedType speedType;
    public float speed;
    public int gear;

    [Header("Path Tracker")]
    public bool trackPoints;
    public int renderTime = 3;
    private LineRenderer[] lineRenderers;
    private List<Vector3[]> points;

    [Header("Data Logging")]
    public bool active;
    private DataLogging dataLogger;

    // UI Elements
    private GameObject interfaceObject;
    private SpeedometerScript speedometer;
    private WindowGraph graphObject;
    private Text gearField;
    private EngineModel engine;
    private Graph graph;

    // Start is called before the first frame update
    private void Awake()
    {
        interfaceObject = GameObject.Find("UI");
        engine = GetComponent<EngineModel>();
        dataLogger = GetComponent<DataLogging>();

        if (interfaceObject != null)
        {
            speedometer = interfaceObject.GetComponent<SpeedometerScript>();
            graphObject = interfaceObject.transform.Find("Canvas").gameObject.transform.Find("WindowGraph").GetComponent<WindowGraph>();
            graph = interfaceObject.GetComponent<Graph>();
            gearField = interfaceObject.transform.Find("Canvas").gameObject.transform.Find("Gear").GetComponent<Text>();
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        engine.UpdateState();

        speed = engine.speed;
        gear = engine.currentGear + 1;

        if (interfaceObject != null)
        {
            graph.UpdateGraph(engine.speed, engine.engineRPM, engine.currentGear + 1);
            speedometer.UpdateDisplay(engine.speed, engine.engineRPM, GenericFunctions.SpeedTypeConverter(speedType).Item1);

            gearField.text = string.Format("{0}{1}", engine.currentGear + 1, GenericFunctions.ToOrdinal(engine.currentGear + 1));
        }

        if (active) dataLogger.UpdateLog();


        UpdateLineRenderer();
    }

    /// <summary>
    /// Creates Line Renderer master and sub line renderes to draw lines from wheel centers.
    /// </summary>
    private void UpdateLineRenderer()
    {
        if (lineRenderers == null || points == null)
        {
            Color[] ColorArray = { new Color(1, 0, 0), new Color(1, 0, 0), new Color(0, 0, 1), new Color(0, 0, 1) };
            GameObject lineRendererMaster = new GameObject("Line Renderer Master");
            lineRenderers = new LineRenderer[4];
            points = new List<Vector3[]>();
            for (int i = 0; i < 4; i++)
            {
                GameObject lineRenderer = new GameObject($"Line Renderer ({i})");
                lineRenderer.transform.parent = lineRendererMaster.transform;
                lineRenderers[i] = lineRenderer.AddComponent<LineRenderer>();
                lineRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
                lineRenderers[i].material.color = ColorArray[i];
                lineRenderers[i].widthMultiplier = .1f;//0.02f;
                lineRenderers[i].positionCount = (int)(renderTime / Time.fixedDeltaTime);

                points.Add(new Vector3[(int)(renderTime / Time.fixedDeltaTime)]);
                for (int k = 0; k < points[i].Length; k++)
                {
                    points[i][k] = engine.wheels[i].mesh.transform.position;
                }
                lineRenderers[i].SetPositions(points[i]);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            for (int k = 0; k < points[i].Length - 1; k++)
            {
                points[i][k] = points[i][k + 1];
            }
            points[i][points[i].Length - 1] = engine.wheels[i].mesh.transform.position;
            lineRenderers[i].SetPositions(points[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            lineRenderers[i].enabled = trackPoints;
        }
    }
}
