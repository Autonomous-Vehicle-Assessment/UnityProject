using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EngineModel))]
public class CarUserControl : MonoBehaviour
{
    private EngineModel engine;               // Engine model
    private DataLogging dataLogger;

    [SerializeField] public TransferCase currentTransfercase;
    [SerializeField] public bool trackPoints;
    [SerializeField] public bool active;
    [SerializeField] public int renderTime = 1;

    private LineRenderer[] lineRenderers;
    private List<Vector3[]> points;

    private void Awake()
    {
        // get the controller
        engine = GetComponent<EngineModel>();

        // DataLogger
        dataLogger = GetComponent<DataLogging>();
    }


    private void LateUpdate()
    {
        // pass the input to the car!
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float footbrake = Input.GetAxis("FootBrake");
        float handbrake = Input.GetAxis("HandBrake");

        if (Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (currentTransfercase == TransferCase.High)
            {
                currentTransfercase = TransferCase.Low;
            }
            else
            {
                currentTransfercase = TransferCase.High;
            }
        }
            
        engine.currentTransferCase = currentTransfercase;

        engine.UpdateState();

        if (engine != null)
        {
            if (active) engine.Move(h, v, footbrake, handbrake);
        }


        dataLogger.Stamp(engine);

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
                lineRenderers[i].widthMultiplier = 0.02f;
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