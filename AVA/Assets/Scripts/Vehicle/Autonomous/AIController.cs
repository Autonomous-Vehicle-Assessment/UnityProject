using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private EngineModel engine;               // Engine model
    
    public Transform path;

    [Header("Driver")]
    public bool autonomousDriving;
    public float targetVelocity;
    public float throttleCap;
    
    public float vehicleSpeed;
    private float speedError;
    public float proportionalGain;

    private List<PathNode> pathNodes = new List<PathNode>();

    [Header("Nodes")]
    public int currentNode = 0;
    public float nodeDistance;

    [Header("Output")]
    [Range(0,1)]
    public float throttle;
    [Range(-1,1)]
    public float steer;

    // Start is called before the first frame update
    void Awake()
    {
        // get the controller
        engine = GetComponent<EngineModel>();

        PathNode[] nodes = path.GetComponentsInChildren<PathNode>();
        pathNodes = new List<PathNode>();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].transform != transform)
            {
                pathNodes.Add(nodes[i]);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (autonomousDriving)
        {
            Steer();
            Drive();

            engine.Move(steer, throttle, 0, 0);
            CheckWaypointDistance();
        }
    }

    private void Steer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);
        steer = (relativeVector.x / relativeVector.magnitude);        
    }

    private void Drive()
    {
        vehicleSpeed = engine.speed;
        targetVelocity = pathNodes[currentNode].targetVelocity;
        speedError = targetVelocity - vehicleSpeed;
        throttle = speedError * proportionalGain;

        throttle = Mathf.Min(throttleCap,throttle);
    }

    private void CheckWaypointDistance()
    {
        Vector2 currentPosition = new Vector2(transform.position.x,transform.position.z);
        Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);
        
        nodeDistance = Vector2.Distance(currentPosition, targetPosition);
        if ( nodeDistance < 1f)
        {
            if(currentNode == pathNodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }
}
