using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AIController : MonoBehaviour
{
    private EngineModel engine;               // Engine model
    private VehicleStats vehicleStats;               // Engine model

    public Transform pathMaster;
    private GameObject wayPoint;

    [Header("Driver")]
    public bool autonomousDriving;
    public float targetVelocity;
    public float throttleCap = 0.8f;
    public float reverseVel = 10f;
    public float reverseAngle = 30f;

    public float vehicleSpeed;
    private float speedError;
    private float proportionalGain = 0.2f;
    private float wheelDistanceLength;
    private float wheelDistanceWidth;

    private float turningRadius;
    public bool showTurningRadius;

    private bool targetBehind;
    private bool withinTurning;
    private bool reverse;
    private float turningRadiusMin;
    private float distance;
    private Vector3 turningRadiusCenter;
    private Vector3 offsetMin;

    [Header("Pathfinder")]
    public int currentNode = 0;
    public int currentPath = 0;
    public float driverRange = 10f;
    public bool showDriver;
    private float nodeDistance;
    private Vector3 wirePoint;
    private Vector3 linePoint;
    public LayerMask layerMask;
    private List<AIPath> paths;
    private Vector3 targetWaypoint;
    private List<PathNode> pathNodes;

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
        vehicleStats = GetComponent<VehicleStats>();
        
        paths = new List<AIPath>();
        foreach (AIPath path in pathMaster.GetComponentsInChildren<AIPath>())
        {
            paths.Add(path);
        }

        pathNodes = new List<PathNode>();
        pathNodes = paths[currentPath].pathNodes;

        wayPoint = new GameObject("Waypoint");
        
        wheelDistanceLength = Vector2.Distance(engine.wheels[0].collider.transform.position, engine.wheels[3].collider.transform.position);
        wheelDistanceWidth = Vector2.Distance(engine.wheels[0].collider.transform.position, engine.wheels[1].collider.transform.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckWaypointDistance();
        Steer();
        Drive();
        if (autonomousDriving) engine.Move(steer, throttle, brake, 0);

    }

    private void Steer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(wayPoint.transform.position);
        steer = relativeVector.x / relativeVector.magnitude;

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosition = new Vector2(wayPoint.transform.position.x, wayPoint.transform.position.z);

        nodeDistance = Vector2.Distance(currentPosition, targetPosition);

        turningRadiusMin = wheelDistanceLength / Mathf.Atan(engine.maximumInnerSteerAngle * Mathf.Deg2Rad) + wheelDistanceWidth / 2;
        turningRadius = wheelDistanceLength / Mathf.Atan(Mathf.Abs(steer) * engine.maximumInnerSteerAngle * Mathf.Deg2Rad) + wheelDistanceWidth / 2; ;

        offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(steer), 0, wheelDistanceLength / 2);
        turningRadiusCenter = transform.position + transform.TransformVector(offsetMin);       
        
        distance = Vector2.Distance(new Vector2(turningRadiusCenter.x,turningRadiusCenter.z), targetPosition);

        withinTurning = distance < turningRadiusMin && Mathf.Abs(relativeVector.x / relativeVector.magnitude) > Mathf.Sin(reverseAngle * Mathf.Deg2Rad) && relativeVector.magnitude > 1;
        targetBehind = relativeVector.z < -5;

        if (withinTurning || reverse || targetBehind)
        {
            reverse = true;
            if (vehicleSpeed < reverseVel)
            {
                steer = Mathf.Sign(steer) * -1;
            }

        }
        if (reverse && !withinTurning && Mathf.Abs(relativeVector.x / relativeVector.magnitude) < Mathf.Sin(reverseAngle * Mathf.Deg2Rad) && relativeVector.z >= 0)
        {
            if (vehicleSpeed < reverseVel)
            {
                steer = Mathf.Sign(steer) * -1;
            }
            reverse = false;
        }
        if (!reverse && vehicleSpeed < -reverseVel/2)
        {
            steer *= -1;
        }
        steer *= 2f;
    }

    private void Drive()
    {
        vehicleSpeed = engine.speed * 1 / GenericFunctions.SpeedCoefficient(vehicleStats.m_SpeedType);                                      // Vehicle velocity in m/s
        targetVelocity = pathNodes[currentNode].targetVelocity * 1 / GenericFunctions.SpeedCoefficient(pathNodes[currentNode].speedType);   // Target velocity in m/s

        if (reverse)
        {
            targetVelocity = -targetVelocity / 2f;
        }

        speedError = targetVelocity - vehicleSpeed;
        throttle = speedError * proportionalGain;

        if (reverse)
        {
            if (vehicleSpeed > reverseVel)
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
        UpdatePath();
        Vector2 currentPosition = new Vector2(wayPoint.transform.position.x, wayPoint.transform.position.z);
        Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);
        
        nodeDistance = Vector2.Distance(currentPosition, targetPosition);
        if ( nodeDistance < .5f)
        {
            if(currentNode == pathNodes.Count - 1)
            {
                currentNode = 0;
                if (currentPath == paths.Count - 1)
                {
                    currentPath = 0;
                }
                else
                {
                    currentPath++;

                    if (currentPath == 6)
                    {
                        currentPath++;
                    }
                }
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

    private void UpdatePath()
    {
        pathNodes = paths[currentPath].pathNodes;

        Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);
        wirePoint = transform.position + transform.TransformVector(relativeVector).normalized * Mathf.Min(relativeVector.magnitude, driverRange);
        Vector3 initialPath;
        if (currentNode == 0) initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[pathNodes.Count-1].transform.position, pathNodes[currentNode].transform.position);
        else initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[currentNode - 1].transform.position, pathNodes[currentNode].transform.position);
        
        relativeVector = transform.TransformVector(transform.InverseTransformPoint(initialPath));
        initialPath = transform.position + relativeVector.normalized * Mathf.Min(relativeVector.magnitude, driverRange);

        linePoint = initialPath;

        float forwardRange = driverRange;
        float carWidth = 3f;

        Vector3 raycastDir = transform.TransformVector(transform.InverseTransformPoint(initialPath));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, raycastDir, out hit, driverRange * 2, layerMask))
        {
            forwardRange = transform.InverseTransformPoint(hit.point).magnitude - 10;
            //Debug.DrawLine(transform.position, hit.point);
        }
        if (Physics.Raycast(transform.TransformPoint(new Vector3(-carWidth, 0)), raycastDir, out hit, driverRange * 3, layerMask))
        {
            forwardRange = Mathf.Min(forwardRange,transform.InverseTransformPoint(hit.point).magnitude - 10);
            //Debug.DrawLine(transform.TransformPoint(new Vector3(-carWidth, 0)), hit.point);
        }
        if (Physics.Raycast(transform.TransformPoint(new Vector3(carWidth, 0)), raycastDir, out hit, driverRange * 3, layerMask))
        {
            forwardRange = Mathf.Min(forwardRange, transform.InverseTransformPoint(hit.point).magnitude - 10);
            //Debug.DrawLine(transform.TransformPoint(new Vector3(carWidth, 0)), hit.point);
        }


        relativeVector = transform.TransformVector(transform.InverseTransformPoint(linePoint));
        Vector3 finalPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));
        Vector3 wayPointPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange * 1 / 3f));

        if (forwardRange < driverRange)
        {
            float distanceToGoal = Mathf.Infinity;
            // Check other angles.
            int anglechecks = 50;

            for (int i = 0; i < anglechecks; i++)
            {
                forwardRange = driverRange*2;
                float angle = -90 + 180 / (anglechecks-1) * i;
                Matrix4x4 rotMatrix = Matrix4x4.Rotate(transform.rotation * Quaternion.Euler(0, angle, 0));

                raycastDir = rotMatrix.MultiplyVector(transform.InverseTransformPoint(initialPath)).normalized;

                if (Physics.Raycast(transform.position, raycastDir, out hit, driverRange * 3, layerMask))
                {
                    forwardRange = transform.InverseTransformPoint(hit.point).magnitude - 10;
                }
                if (Physics.Raycast(transform.TransformPoint(new Vector3(-carWidth, 0)), raycastDir, out hit, driverRange * 3, layerMask))
                {
                    forwardRange = Mathf.Min(forwardRange, transform.InverseTransformPoint(hit.point).magnitude - 10);
                }
                if (Physics.Raycast(transform.TransformPoint(new Vector3(carWidth, 0)), raycastDir, out hit, driverRange * 3, layerMask))
                {
                    forwardRange = Mathf.Min(forwardRange, transform.InverseTransformPoint(hit.point).magnitude - 10);
                }
                Vector3 currentPath = transform.position + raycastDir * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange * 2));
                
                //Debug.DrawLine(currentPath, linePoint);

                // Find distance from finalPath to goal (linePoint)
                Vector3 closestPoint = GetClosestPointOnFiniteLine(linePoint, transform.position, currentPath);
                float currentDistance = (closestPoint - linePoint).magnitude;
                if (currentDistance < distanceToGoal)
                {
                    distanceToGoal = currentDistance;
                    finalPath = currentPath;
                    wayPointPath = transform.position + raycastDir * Mathf.Min(relativeVector.magnitude, driverRange * 1 / 3f);
                    //wayPointPath = transform.position + raycastDir * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange * 1 / 3f));
                }

            }

            Vector2 currentPosition = new Vector2(wirePoint.x, wirePoint.z);
            Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);

            nodeDistance = Vector2.Distance(currentPosition, targetPosition);
            if (nodeDistance < .5f)
            {
                if (currentNode == pathNodes.Count - 1)
                {
                    currentNode = 0;
                }
                else
                {
                    currentNode++;
                    pathNodes[currentNode].activeNode = true;
                    pathNodes[currentNode - 1].activeNode = false;
                }
            }

            pathNodes[currentNode].activeNode = true;
        }


        wayPoint.transform.position = wayPointPath;
    }

    private void OnDrawGizmos()
    {
        if (showDriver)
        {
            if(wayPoint != null)
            {
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(transform.position, transform.up, driverRange);
                Handles.DrawLine(transform.position, wayPoint.transform.position);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(wayPoint.transform.position, .25f);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(wirePoint, .25f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(linePoint, .25f);
            }
        }
        if (showTurningRadius)
        {
            if (turningRadius < 100)
            {
                Handles.color = Color.yellow;
                
                Vector3 offset = new Vector3(turningRadius * Mathf.Sign(steer), 0, -wheelDistanceLength/2 * 1.1f);
                //Vector3 offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(-steer), 0, wheelDistanceLength / 2);
                Vector3 startPoint;

                if (steer >= 0)
                {
                    startPoint = new Vector3(-1, 0, 0);
                    
                    if (vehicleSpeed < 0)
                    {
                        Handles.color = Color.red;
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius - wheelDistanceWidth/2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
                    }
                    else
                    {
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
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
                    Handles.color = Color.yellow;
                    if (vehicleSpeed < 0)
                    {
                        Handles.color = Color.red;
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
                    }
                    else
                    {
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
                        Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
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

    private Vector3 GetClosestPointOnInfiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        return line_start + Vector3.Project(point - line_start, line_end - line_start);
    }


    private Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return line_start + line_direction * project_length;
    }
}
