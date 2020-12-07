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
    public float throttleCap = 1f;
    public float reverseVel = 10f;
    public float brakeVel = 10f;
    public float reverseAngle = .7f;

    public float vehicleSpeed;
    private float speedError;
    public float proportionalGain = 0.2f;
    public float distanceProportionalGain = 1f;      // km/h / m
    private float wheelDistanceLength;
    private float wheelDistanceWidth;

    private float turningRadius;
    public bool showTurningRadius;
    public bool showDriver;

    private bool targetBehind;
    private bool withinTurning;
    private bool reverse;
    private bool objectAhead;
    private RaycastHit collisionHit;
    private Vector3 alternativeWaypoint;


    private float turningRadiusMin;
    private float distance;
    private Vector3 turningRadiusCenter;
    private Vector3 offsetMin;

    [Header("Pathfinder")]
    public int currentNode = 0;
    public int currentPath = 0;
    public float planRange = 30f;
    public float driverRange = 10f;
    public float waypointRange = 3f;
    public float objectDistance = 10f;
    private float nodeDistance;
    private Vector3 wirePoint;
    private Vector3 linePoint;
    
    public LayerMask layerMask;
    private List<AIPath> paths;
    private Vector3 targetWaypoint;
    private List<PathNode> pathNodes;
    private Vector3 wayPointPathLong;

    [Header("Output")]
    [Range(-1,1)]
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

        wayPoint = new GameObject("TargetWaypoint");
        wayPoint.transform.parent = pathMaster;
        
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

        Vector2 targetPosition = new Vector2(wayPointPathLong.x, wayPointPathLong.z);

        turningRadiusMin = wheelDistanceLength / Mathf.Sin(engine.maximumInnerSteerAngle * Mathf.Deg2Rad); //wheelDistanceLength / Mathf.Atan(engine.maximumInnerSteerAngle * Mathf.Deg2Rad) + wheelDistanceWidth / 2;
        turningRadius = wheelDistanceLength / Mathf.Sin(Mathf.Abs(steer) * engine.maximumInnerSteerAngle * Mathf.Deg2Rad);

        offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(steer), 0, wheelDistanceLength / 2);
        turningRadiusCenter = transform.position + transform.TransformVector(offsetMin);       
        
        distance = Vector2.Distance(new Vector2(turningRadiusCenter.x,turningRadiusCenter.z), targetPosition);

        withinTurning = distance < turningRadiusMin && Mathf.Abs(relativeVector.x / relativeVector.magnitude) > Mathf.Sin(reverseAngle * Mathf.Deg2Rad) && relativeVector.magnitude > 1;
        targetBehind = transform.InverseTransformPoint(wayPointPathLong).z < -5;

        if (withinTurning || reverse || targetBehind)
        {
            reverse = true;
            if (vehicleSpeed < reverseVel)
            {
                steer = Mathf.Sign(steer) * -.5f;
            }

        }
        if (reverse && !withinTurning && Mathf.Abs(relativeVector.x / relativeVector.magnitude) < Mathf.Sin(reverseAngle * Mathf.Deg2Rad) && relativeVector.z >= 0)
        {
            if (vehicleSpeed < reverseVel)
            {
                steer = Mathf.Sign(steer) * -.5f;
            }
            reverse = false;
        }
        if (!reverse && vehicleSpeed < -reverseVel/2)
        {
            steer *= -.5f;
        }

        steer *= 2f;
    }

    private void Drive()
    {
        vehicleSpeed = engine.speed;        // Vehicle velocity in m/s

        Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);
        float positionError = new Vector2(relativeVector.x, relativeVector.z).magnitude;

        targetVelocity = pathNodes[currentNode].targetVelocity * 1 / GenericFunctions.SpeedCoefficient(pathNodes[currentNode].speedType) * GenericFunctions.SpeedCoefficient(vehicleStats.m_SpeedType);   // Target velocity in m/s
        targetVelocity += positionError * distanceProportionalGain;
        
        if (reverse) targetVelocity = -reverseVel;
        float speedError = targetVelocity - vehicleSpeed;

        throttle = speedError * proportionalGain;

        if (speedError < 0)
        {
            if (vehicleSpeed > brakeVel)
            {
                brake = Mathf.Min(1, Mathf.Abs(throttle));
                throttle = 0;
            }
            else
            {
                brake = 0;
                throttle = Mathf.Max(-throttleCap, throttle);
            }
        }

        else
        {
            if (vehicleSpeed < -brakeVel)
            {
                brake = Mathf.Min(1, Mathf.Abs(throttle));
                throttle = 0;
            }
            else
            {
                brake = 0;
                throttle = Mathf.Min(throttleCap, throttle);
            }
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
            IncrementNode();
        }
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
        if (Physics.Raycast(transform.position, raycastDir, out hit, planRange, layerMask))
        {
            forwardRange = transform.InverseTransformPoint(hit.point).magnitude - objectDistance;
            collisionHit = hit;
            //Debug.DrawLine(transform.position, hit.point);
        }
        if (Physics.Raycast(transform.TransformPoint(new Vector3(-carWidth, 0)), raycastDir, out hit, planRange, layerMask))
        {
            if(forwardRange > transform.InverseTransformPoint(hit.point).magnitude - objectDistance)
            {
                forwardRange = transform.InverseTransformPoint(hit.point).magnitude - objectDistance;
                collisionHit = hit;
            }
            //Debug.DrawLine(transform.TransformPoint(new Vector3(-carWidth, 0)), hit.point);
        }
        if (Physics.Raycast(transform.TransformPoint(new Vector3(carWidth, 0)), raycastDir, out hit, planRange, layerMask))
        {
            if (forwardRange > transform.InverseTransformPoint(hit.point).magnitude - objectDistance)
            {
                forwardRange = transform.InverseTransformPoint(hit.point).magnitude - objectDistance;
                collisionHit = hit;
            }
        }


        relativeVector = transform.TransformVector(transform.InverseTransformPoint(linePoint));
        Vector3 finalPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));
        Vector3 wayPointPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, waypointRange));
        wayPointPathLong = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));
        
        if (forwardRange < Mathf.Min(relativeVector.magnitude, driverRange))
        {
            objectAhead = true;

            float distanceToGoal = Mathf.Infinity;
            // Check other angles.
            int anglechecks = 50;

            for (int i = 0; i < anglechecks; i++)
            {
                forwardRange = driverRange*2;
                float angle = -90 + 180 / (anglechecks-1) * i;
                Matrix4x4 rotMatrix = Matrix4x4.Rotate(transform.rotation * Quaternion.Euler(0, angle, 0));

                raycastDir = rotMatrix.MultiplyVector(transform.InverseTransformPoint(initialPath)).normalized;

                if (Physics.Raycast(transform.position, raycastDir, out hit, planRange, layerMask))
                {
                    forwardRange = transform.InverseTransformPoint(hit.point).magnitude - 10;
                }
                if (Physics.Raycast(transform.TransformPoint(new Vector3(-carWidth, 0)), raycastDir, out hit, planRange, layerMask))
                {
                    forwardRange = Mathf.Min(forwardRange, transform.InverseTransformPoint(hit.point).magnitude - 10);
                }
                if (Physics.Raycast(transform.TransformPoint(new Vector3(carWidth, 0)), raycastDir, out hit, planRange, layerMask))
                {
                    forwardRange = Mathf.Min(forwardRange, transform.InverseTransformPoint(hit.point).magnitude - 10);
                }
                Vector3 currentPath = transform.position + raycastDir * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, planRange));
                

                // Find distance from finalPath to goal (linePoint)
                Vector3 closestPoint = GetClosestPointOnFiniteLine(linePoint, transform.position, currentPath);
                float currentDistance = (closestPoint - linePoint).magnitude;
                if (currentDistance < distanceToGoal)
                {
                    distanceToGoal = currentDistance;
                    finalPath = currentPath;
                    wayPointPath = transform.position + raycastDir * Mathf.Min(relativeVector.magnitude, waypointRange);
                    wayPointPathLong = transform.position + raycastDir * Mathf.Min(relativeVector.magnitude, driverRange);
                }
            }

            Vector2 currentPosition = new Vector2(wirePoint.x, wirePoint.z);
            Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);

            nodeDistance = Vector2.Distance(currentPosition, targetPosition);
            if (nodeDistance < .5f)
            {
                IncrementNode();
            }
        }
        else objectAhead = false;


        wayPoint.transform.position = wayPointPath;
    }

    private void IncrementNode()
    {
        int prevNode = currentNode;
        int prevPath = currentPath;

        // End of current path
        if (currentNode == pathNodes.Count - 1)
        {
            currentNode = 0;

            // End of paths
            if (currentPath == paths.Count - 1) currentPath = 0;
            else currentPath++;

            pathNodes = paths[currentPath].pathNodes;
        }
        else currentNode++;

        paths[prevPath].pathNodes[prevNode].activeNode = false;

        pathNodes[currentNode].Activate();
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
                Handles.color = Color.red;
                Gizmos.DrawWireSphere(wayPoint.transform.position, .25f);
                Handles.DrawWireDisc(transform.position, transform.up, waypointRange);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(wirePoint, .25f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(linePoint, .25f);

                if (objectAhead)
                {
                    // Handles.color = Color.blue;
                    // Handles.DrawWireDisc(transform.position, transform.up, planRange);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(collisionHit.point, .25f);
                }
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
