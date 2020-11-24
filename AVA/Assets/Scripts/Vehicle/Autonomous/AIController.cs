using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    private float proportionalGain = 1;
    private float wheelDistanceLength;
    private float wheelDistanceWidth;

    public float turningRadius;
    public bool showTurningRadius;

    private bool targetBehind;
    private bool withinTurning;
    private bool reverse;
    private float turningRadiusMin;
    public float distance;
    private Vector3 turningRadiusCenter;
    private Vector3 offsetMin;

    private List<PathNode> pathNodes = new List<PathNode>();

    [Header("Nodes")]
    public int currentNode = 0;
    public float nodeDistance;

    [Header("Output")]
    [Range(0,1)]
    public float throttle;
    [Range(0,1)]
    public float brake;
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

        wheelDistanceLength = Vector2.Distance(engine.wheels[0].collider.transform.position, engine.wheels[3].collider.transform.position);
        wheelDistanceWidth = Vector2.Distance(engine.wheels[0].collider.transform.position, engine.wheels[1].collider.transform.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (autonomousDriving)
        {
            Steer();
            Drive();

            engine.Move(steer, throttle, brake, 0);
            CheckWaypointDistance();
        }
    }

    private void Steer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);
        steer = relativeVector.x / relativeVector.magnitude;

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);

        nodeDistance = Vector2.Distance(currentPosition, targetPosition);

        turningRadiusMin = wheelDistanceLength / Mathf.Atan(engine.maximumInnerSteerAngle * Mathf.Deg2Rad) + wheelDistanceWidth / 2;
        turningRadius = wheelDistanceLength / Mathf.Atan(Mathf.Abs(steer) * engine.maximumInnerSteerAngle * Mathf.Deg2Rad) + wheelDistanceWidth / 2; ;

        offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(steer), 0, wheelDistanceLength / 2);
        turningRadiusCenter = transform.position + transform.TransformVector(offsetMin);       
        
        distance = Vector2.Distance(new Vector2(turningRadiusCenter.x,turningRadiusCenter.z), targetPosition);

        withinTurning = distance < turningRadiusMin && Mathf.Abs(relativeVector.x / relativeVector.magnitude) > Mathf.Sin(30 * Mathf.Deg2Rad) && relativeVector.magnitude > 1;
        targetBehind = relativeVector.z < -5;

        if (withinTurning || reverse || targetBehind)
        {
            reverse = true;
            if (vehicleSpeed < 5)
            {
                steer = Mathf.Sign(steer) * -1;
            }

        }
        if (reverse && !withinTurning && Mathf.Abs(relativeVector.x / relativeVector.magnitude) < Mathf.Sin(30 * Mathf.Deg2Rad) && relativeVector.z >= 0)
        {
            if (vehicleSpeed < 5)
            {
                steer = Mathf.Sign(steer) * -1;
            }
            reverse = false;
        }
        if (!reverse && vehicleSpeed < -2)
        {
            steer *= -1;
        }
        steer *= 3f;
        //offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(-steer), 0, wheelDistanceLength / 2);
        //turningRadiusCenter = transform.position + transform.TransformVector(offsetMin);       
    }

    private void Drive()
    {
        vehicleSpeed = engine.speed;
        targetVelocity = pathNodes[currentNode].targetVelocity;
        if (reverse)
        {
            targetVelocity = -targetVelocity / 2f;
        }

        speedError = targetVelocity - vehicleSpeed;
        throttle = speedError * proportionalGain;

        if (reverse)
        {
            if (vehicleSpeed > 5)
            {
                brake = Mathf.Abs(Mathf.Max(-1, Mathf.Min(0, throttle)));
            }
            else
            {
                brake = 0;
                throttle = Mathf.Min(throttleCap, throttle);
            }            
        }
        else
        {
            brake = Mathf.Abs(Mathf.Max(-1, Mathf.Min(0, throttle)));

            throttle = Mathf.Max(0, Mathf.Min(throttleCap, throttle));
        }
    }

    private void CheckWaypointDistance()
    {
        Vector2 currentPosition = new Vector2(transform.position.x,transform.position.z);
        Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);
        
        nodeDistance = Vector2.Distance(currentPosition, targetPosition);
        if ( nodeDistance < 1.5f)
        {
            if(currentNode == pathNodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
                pathNodes[currentNode].activeNode = true;
                pathNodes[currentNode-1].activeNode = false;
            }
        }

        pathNodes[currentNode].activeNode = true;
    }

    private void OnDrawGizmos()
    {
        if (showTurningRadius)
        {
            if (turningRadius < 50)
            {
                Handles.color = Color.cyan;
                
                Vector3 offset = new Vector3(turningRadius * Mathf.Sign(steer), 0, -wheelDistanceLength/2 * 1.1f);
                //Vector3 offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(-steer), 0, wheelDistanceLength / 2);
                Vector3 startPoint;

                if (steer >= 0)
                {
                    startPoint = new Vector3(-1, 0, 0);
                    
                    if (vehicleSpeed < 0)
                    {
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -(140 - Mathf.Min(110, 110 / 50 * turningRadius)), turningRadius - wheelDistanceWidth/2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -(140 - Mathf.Min(110, 110 / 50 * turningRadius)), turningRadius + wheelDistanceWidth / 2);
                    }
                    else
                    {
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 140 - Mathf.Min(110, 110 / 50 * turningRadius), turningRadius - wheelDistanceWidth / 2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 140 - Mathf.Min(110, 110 / 50 * turningRadius), turningRadius + wheelDistanceWidth / 2);
                    }
                    if (withinTurning)
                    {
                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.green;
                    }

                    if (withinTurning)
                    {
                        Handles.DrawWireDisc(transform.position + transform.TransformVector(offsetMin), transform.up, turningRadiusMin - wheelDistanceWidth / 2);
                        Gizmos.DrawWireSphere(turningRadiusCenter, .5f);
                    }
                }
                else
                {
                    startPoint = new Vector3(1, 0, 0);
                    Handles.color = Color.cyan;
                    if (vehicleSpeed < 0)
                    {
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -(-140 + Mathf.Min(110, 110 / 50 * turningRadius)), turningRadius - wheelDistanceWidth / 2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -(-140 + Mathf.Min(110, 110 / 50 * turningRadius)), turningRadius + wheelDistanceWidth / 2);
                    }
                    else
                    {
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -140 + Mathf.Min(110, 110 / 50 * turningRadius), turningRadius - wheelDistanceWidth / 2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -140 + Mathf.Min(110, 110 / 50 * turningRadius), turningRadius + wheelDistanceWidth / 2);
                    }

                    if (withinTurning)
                    {
                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.green;
                    }
                    if (withinTurning)
                    {
                        Handles.DrawWireDisc(transform.position + transform.TransformVector(offsetMin), transform.up, turningRadiusMin - wheelDistanceWidth / 2);
                        Gizmos.DrawWireSphere(turningRadiusCenter, .5f);
                    }
                }
            }
        }
    }
}
