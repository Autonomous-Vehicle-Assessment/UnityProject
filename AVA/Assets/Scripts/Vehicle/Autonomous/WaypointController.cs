using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum GizmoMode
{
    Non,
    driverD,
    turningRadiusTR,
    obstacleAvoidanceOA,
    frontDistanceFD,
    dTR,
    dOA,
    dTROA
}
public class WaypointController : MonoBehaviour
{
    private EngineModel engine;
    private VehicleStats vehicleStats;
    
    [Header("Autonomous Driver")]
    public bool active;
    public GizmoMode gizmo;

    [Header("Vehicle State")]
    public float vehicleSpeed;
    public bool park;

    private float turningRadiusMin;
    private Vector3 turningRadiusCenter;
    private Vector3 offsetMin;

    [Header("Driver")]
    public float targetVelocity;
    public float distanceProGain = .01f;      // km/h / m
    public float velocityDepDist = .1f;      // km/h / m
    public float planRange = 30f;
    public float driverRange = 10f;
    public float waypointRange = 3f;

    [Header("Output")]
    [Range(-1, 1)]
    public float throttle;
    [Range(0, 1)]
    public float brake;
    [Range(-1, 1)]
    public float steer;

    // Controller
    private const float throttleCap = 1f;
    private const float reverseVel = 10f;
    private const float brakeVel = 10f;
    private const float reverseAngle = 75f;
    private const float proportionalGain = 0.2f;
    

    // Turning parameters
    private float wheelDistanceLength;
    private float wheelDistanceWidth;
    private float turningRadius;

    // Obstacle avoidance and planning
    private bool targetBehind;
    private bool withinTurning;
    private bool objectAhead;
    private bool reverse;
    private RaycastHit collisionHit;

    [Header("Pathfinder")]
    public int currentNode = 0;
    public int currentPath = 0;
    private float nodeDistance;
    private Vector3 wirePoint;
    private Vector3 linePoint;

    public PathLoader pathMaster;
    private GameObject wayPoint;

    [Header("Obstacle Avoidance")]
    [Range(3,100)]
    public int alternativeRoutes = 40;
    [Range(10,80)]
    public float routeAngles = 50;
    public float objectDistance = 5f;
    public LayerMask avoidanceMask;
    private static Vector3 boxDimensionsHalfs = new Vector3(2f, 1.2f, 2.6f);
    private static Vector3 boxDimensions = boxDimensionsHalfs * 2;


    private LayerMask vehicleAvoidanceLayer;
    private List<WaypointPath> paths;
    private List<PathNode> pathNodes;
    private Vector3 wayPointPathLong;


    // Start is called before the first frame update
    void Awake()
    {
        // get the controller
        engine = GetComponent<EngineModel>();
        vehicleStats = GetComponent<VehicleStats>();

        InitPath();

        vehicleAvoidanceLayer = LayerMask.GetMask("Vehicle");

        wheelDistanceLength = Vector3.Distance(engine.wheels[0].collider.transform.position, engine.wheels[2].collider.transform.position);
        wheelDistanceWidth = Vector3.Distance(engine.wheels[0].collider.transform.position, engine.wheels[1].collider.transform.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (park)
        {
            steer = 0;
            throttle = 0;
            brake = 1;
        }
        else
        {
            CheckWaypointDistance();
            Steer();
            Drive();
        }

        if (active) engine.Move(steer, throttle, brake, 0);

    }

    private void Steer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(wayPoint.transform.position);
        steer = relativeVector.x / relativeVector.magnitude;

        Vector2 targetPosition = new Vector2(wayPointPathLong.x, wayPointPathLong.z);

        turningRadiusMin = wheelDistanceLength / Mathf.Sin(engine.maximumInnerSteerAngle * Mathf.Deg2Rad);
        turningRadius = wheelDistanceLength / Mathf.Sin(Mathf.Abs(steer) * engine.maximumInnerSteerAngle * Mathf.Deg2Rad);

        offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(steer), 0, -wheelDistanceLength / 2);
        turningRadiusCenter = transform.position + transform.TransformVector(offsetMin);

        float distance = Vector2.Distance(new Vector2(turningRadiusCenter.x, turningRadiusCenter.z), targetPosition);

        withinTurning = distance < turningRadiusMin - wheelDistanceWidth / 2 && Mathf.Abs(relativeVector.x / relativeVector.magnitude) > Mathf.Sin(reverseAngle * Mathf.Deg2Rad) && relativeVector.magnitude > 3;
        targetBehind = transform.InverseTransformPoint(wayPointPathLong).z < -5;

        steer *= 1.5f;
        if (withinTurning || reverse || targetBehind)
        {
            reverse = true;
            if (vehicleSpeed < reverseVel)
            {
                steer = Mathf.Sign(steer) * -1f;
            }
        }
        if (reverse && !withinTurning && Mathf.Abs(relativeVector.x / relativeVector.magnitude) < Mathf.Sin(reverseAngle * Mathf.Deg2Rad) && relativeVector.z >= 0)
        {
            if (vehicleSpeed < reverseVel)
            {
                steer = Mathf.Sign(steer) * -1f;
            }
            reverse = false;
        }
        if (!reverse && vehicleSpeed < -reverseVel / 2)
        {
            steer *= -1f;
        }
        
        steer = Mathf.Min(1, Mathf.Max(-1, steer));
    }

    private void Drive()
    {
        vehicleSpeed = engine.speed;
        Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);
        float positionError = new Vector2(relativeVector.x, relativeVector.z).magnitude - (velocityDepDist * vehicleSpeed);

        targetVelocity = pathNodes[currentNode].targetVelocity * 1 / GenericFunctions.SpeedCoefficient(pathNodes[currentNode].speedType) * GenericFunctions.SpeedCoefficient(vehicleStats.speedType);   // Target velocity in m/s
        targetVelocity *= 1 + positionError * distanceProGain;

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
        ObstacleAvoidanceBoxcast();
        Vector2 currentPosition = new Vector2(wayPoint.transform.position.x, wayPoint.transform.position.z);
        Vector2 targetPosition = new Vector2(pathNodes[currentNode].transform.position.x, pathNodes[currentNode].transform.position.z);

        nodeDistance = Vector2.Distance(currentPosition, targetPosition);
        if (nodeDistance < .5f)
        {
            IncrementNode();
        }
    }

    private void ObstacleAvoidanceRaycast()
    {
        pathNodes = paths[currentPath].pathNodes;

        Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);
        wirePoint = transform.position + transform.TransformVector(relativeVector).normalized * Mathf.Min(relativeVector.magnitude, driverRange);
        Vector3 initialPath;
        if (currentNode == 0) initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[pathNodes.Count - 1].transform.position, pathNodes[currentNode].transform.position);
        else initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[currentNode - 1].transform.position, pathNodes[currentNode].transform.position);

        relativeVector = transform.TransformVector(transform.InverseTransformPoint(initialPath));
        initialPath = transform.position + relativeVector.normalized * Mathf.Min(relativeVector.magnitude, driverRange);

        linePoint = initialPath;

        float forwardRange = driverRange;
        float carWidth = 3f;

        Vector3 raycastDir = transform.TransformVector(transform.InverseTransformPoint(initialPath));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, raycastDir, out hit, planRange, avoidanceMask))
        {
            forwardRange = transform.InverseTransformPoint(hit.point).magnitude - objectDistance;
            collisionHit = hit;
            //Debug.DrawLine(transform.position, hit.point);
        }
        if (Physics.Raycast(transform.TransformPoint(new Vector3(-carWidth, 0)), raycastDir, out hit, planRange, avoidanceMask))
        {
            if (forwardRange > transform.InverseTransformPoint(hit.point).magnitude - objectDistance)
            {
                forwardRange = transform.InverseTransformPoint(hit.point).magnitude - objectDistance;
                collisionHit = hit;
            }
            //Debug.DrawLine(transform.TransformPoint(new Vector3(-carWidth, 0)), hit.point);
        }
        if (Physics.Raycast(transform.TransformPoint(new Vector3(carWidth, 0)), raycastDir, out hit, planRange, avoidanceMask))
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
                forwardRange = driverRange * 2;
                float angle = -90 + 180 / (anglechecks - 1) * i;
                Matrix4x4 rotMatrix = Matrix4x4.Rotate(transform.rotation * Quaternion.Euler(0, angle, 0));

                raycastDir = rotMatrix.MultiplyVector(transform.InverseTransformPoint(initialPath)).normalized;

                if (Physics.Raycast(transform.position, raycastDir, out hit, planRange, avoidanceMask))
                {
                    forwardRange = transform.InverseTransformPoint(hit.point).magnitude - 10;
                }
                if (Physics.Raycast(transform.TransformPoint(new Vector3(-carWidth, 0)), raycastDir, out hit, planRange, avoidanceMask))
                {
                    forwardRange = Mathf.Min(forwardRange, transform.InverseTransformPoint(hit.point).magnitude - 10);
                }
                if (Physics.Raycast(transform.TransformPoint(new Vector3(carWidth, 0)), raycastDir, out hit, planRange, avoidanceMask))
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

    private void ObstacleAvoidanceBoxcast()
    {
        Vector3 initialtDir = Vector3.forward;
        if (wayPoint != null)
        {
            pathNodes = paths[currentPath].pathNodes;
            Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);

            wirePoint = transform.position + transform.TransformVector(relativeVector).normalized * Mathf.Min(relativeVector.magnitude, driverRange);

            // Create initial path from closest point on waypoint path
            Vector3 initialPath;
            if (currentNode == 0) initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[pathNodes.Count - 1].transform.position, pathNodes[currentNode].transform.position);
            else initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[currentNode - 1].transform.position, pathNodes[currentNode].transform.position);
            linePoint = initialPath;

            // Relative vector to initial path and update to be capped by driverRange
            relativeVector = transform.TransformVector(transform.InverseTransformPoint(initialPath));
            initialPath = transform.position + relativeVector.normalized * Mathf.Min(relativeVector.magnitude, driverRange);


            float forwardRange = driverRange;

            initialtDir = transform.InverseTransformPoint(initialPath);

            relativeVector = transform.TransformVector(transform.InverseTransformPoint(initialPath));
            Vector3 finalPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));
            Vector3 wayPointPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, waypointRange));
            wayPointPathLong = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));
        
            // Check for objects in original (direct) path.
            Vector3 centerPos = transform.TransformPoint(0, .5f, -.2f);
            float startAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(initialtDir.x, 0, initialtDir.z), transform.up);
            Quaternion orientationInitial = transform.rotation * Quaternion.Euler(new Vector3(0, startAngle, 0));
            Matrix4x4 transformMatrixInitial = Matrix4x4.TRS(centerPos, orientationInitial, Vector3.one);

            if (ObjectInPath(transformMatrixInitial, orientationInitial, relativeVector))
            {
                float distanceToGoal = Mathf.Infinity;

                for (int i = 0; i < alternativeRoutes; i++)
                {
                    Vector3 rayRotation = new Vector3(0, startAngle - routeAngles + 2 * routeAngles / (alternativeRoutes - 1f) * i, 0);

                    Quaternion orientation = transform.rotation * Quaternion.Euler(rayRotation);
                    Matrix4x4 transformMatrix = Matrix4x4.TRS(centerPos, orientation, Vector3.one);

                    Vector3 direction = (transformMatrix.rotation * Vector3.forward).normalized;

                    float pathRange = planRange;
                    if (Physics.BoxCast(centerPos, boxDimensionsHalfs, direction, out RaycastHit hit, orientation, planRange, avoidanceMask))
                    {
                        float distance = transform.InverseTransformPoint(hit.point).magnitude;
                        pathRange = distance - boxDimensions.z * 1.5f;
                    }

                    Vector3 currentPath = transform.position + direction * Mathf.Min(pathRange, planRange);

                    // Find distance from finalPath to goal (linePoint)
                    Vector3 closestPoint = GetClosestPointOnFiniteLine(linePoint, transform.position, currentPath);
                    float currentDistance = (closestPoint - linePoint).magnitude;
                    if (currentDistance < distanceToGoal)
                    {
                        distanceToGoal = currentDistance;
                        finalPath = currentPath;
                        wayPointPath = transform.position + direction * Mathf.Min(relativeVector.magnitude, waypointRange);
                        wayPointPathLong = transform.position + direction * Mathf.Min(relativeVector.magnitude, driverRange);
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
            
            wayPoint.transform.position = wayPointPath;
        }
    }

    public bool ObjectInPath(Matrix4x4 transformMatrix, Quaternion orientationInitial, Vector3 relativeVector)
    {
        Vector3 centerPos = transform.TransformPoint(0, .5f, -.2f);

        Vector3 pathDirection = (transformMatrix.rotation * Vector3.forward).normalized;

        bool obstacleAhead = false;

        if (Physics.BoxCast(centerPos, boxDimensionsHalfs, pathDirection, out RaycastHit hit, orientationInitial, planRange, avoidanceMask))
        {
            float distance = transform.InverseTransformPoint(hit.point).magnitude;
            float pathRange = distance - boxDimensions.z * 1.5f;

            // If object between vehicle and target
            if (pathRange + boxDimensions.z * 1.5f < relativeVector.magnitude)
            {
                obstacleAhead = true;
            }
        }

        return obstacleAhead;
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
        switch (gizmo)
        {
            case GizmoMode.Non:
                break;
            case GizmoMode.driverD:
                DriverGizmo();
                break;
            case GizmoMode.turningRadiusTR:
                if (turningRadius < 100)
                {
                    TurningRadiusGizmo();
                }
                break;
            case GizmoMode.obstacleAvoidanceOA:
                BoxCastGizmo();
                break;
            case GizmoMode.frontDistanceFD:
                BoxCastGizmo();
                break;
            case GizmoMode.dTR:
                DriverGizmo();
                if (turningRadius < 100)
                {
                    TurningRadiusGizmo();
                }
                break;
            case GizmoMode.dOA:
                DriverGizmo();
                BoxCastGizmo();
                break;
            case GizmoMode.dTROA:
                DriverGizmo();
                if (turningRadius < 100)
                {
                    TurningRadiusGizmo();
                }
                BoxCastGizmo();
                break;
            default:
                break;
        }
    }

    private void DriverGizmo()
    {
        if (wayPoint != null)
        {
            Handles.color = Color.cyan;
            Handles.DrawLine(transform.position, wayPoint.transform.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wayPoint.transform.position, .25f);
        }
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(transform.position, transform.up, driverRange);
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, transform.up, waypointRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wirePoint, .25f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(linePoint, .25f);

        if (objectAhead) CollisionGizmo();

    }

    private void CollisionGizmo()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(collisionHit.point, .25f);
    }

    private void BoxCastGizmo() 
    {
        Vector3 initialtDir = Vector3.forward;

        if (wayPoint != null)
        {
            pathNodes = paths[currentPath].pathNodes;
            Vector3 relativeVector = transform.InverseTransformPoint(pathNodes[currentNode].transform.position);

            wirePoint = transform.position + transform.TransformVector(relativeVector).normalized * Mathf.Min(relativeVector.magnitude, driverRange);

            // Create initial path from closest point on waypoint path
            Vector3 initialPath;
            if (currentNode == 0) initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[pathNodes.Count - 1].transform.position, pathNodes[currentNode].transform.position);
            else initialPath = GetClosestPointOnFiniteLine(wirePoint, pathNodes[currentNode - 1].transform.position, pathNodes[currentNode].transform.position);
            linePoint = initialPath;

            // Relative vector to initial path and update to be capped by driverRange
            relativeVector = transform.TransformVector(transform.InverseTransformPoint(initialPath));
            initialPath = transform.position + relativeVector.normalized * Mathf.Min(relativeVector.magnitude, driverRange);


            float forwardRange = driverRange;

            initialtDir = transform.InverseTransformPoint(initialPath);

            relativeVector = transform.TransformVector(transform.InverseTransformPoint(initialPath));
            Vector3 finalPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));
            Vector3 wayPointPath = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, waypointRange));
            wayPointPathLong = transform.position + relativeVector.normalized * Mathf.Min(forwardRange, Mathf.Min(relativeVector.magnitude, driverRange));

            // Check for objects in original (direct) path.
            Vector3 centerPos = transform.TransformPoint(0, .5f, -.2f);
            float startAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(initialtDir.x, 0, initialtDir.z), transform.up);
            Quaternion orientationInitial = transform.rotation * Quaternion.Euler(new Vector3(0, startAngle, 0));
            Matrix4x4 transformMatrixInitial = Matrix4x4.TRS(centerPos, orientationInitial, Vector3.one);

            Color red = Color.red;
            red.a = .2f;
            Color cyan = Color.cyan;
            cyan.a = .2f;
            
            if (ObjectInPathGizmo(transformMatrixInitial, orientationInitial, relativeVector))
            {
                float distanceToGoal = Mathf.Infinity;
                int goalIndex = 0;

                for (int i = 0; i < alternativeRoutes; i++)
                {
                    Vector3 rayRotation = new Vector3(0, startAngle - routeAngles + 2 * routeAngles / (alternativeRoutes - 1f) * i, 0);

                    Quaternion orientation = transform.rotation * Quaternion.Euler(rayRotation);
                    Matrix4x4 transformMatrix = Matrix4x4.TRS(centerPos, orientation, Vector3.one);

                    Handles.matrix = transformMatrix;
                    Vector3 direction = (transformMatrix.rotation * Vector3.forward).normalized;

                    float pathRange = planRange;

                    if (Physics.BoxCast(centerPos, boxDimensionsHalfs, direction, out RaycastHit hit, orientation, planRange, avoidanceMask))
                    {
                        float distance = transform.InverseTransformPoint(hit.point).magnitude;
                        pathRange = distance - boxDimensionsHalfs.z;

                        Handles.color = Color.Lerp(red, cyan, distance / planRange - .5f);

                        Handles.DrawWireCube(new Vector3(0, 0, pathRange / 2 + boxDimensionsHalfs.z ), boxDimensions + new Vector3(0, 0, pathRange));
                    }
                    else
                    {
                        Handles.color = cyan;
                        Handles.DrawWireCube(new Vector3(0, 0, planRange / 2 + boxDimensionsHalfs.z), boxDimensions + new Vector3(0, 0, planRange));
                    }

                    Vector3 currentPath = transform.position + direction * Mathf.Min(pathRange, planRange);

                    // Find distance from finalPath to goal (linePoint)
                    Vector3 closestPoint = GetClosestPointOnFiniteLine(linePoint, transform.position, currentPath);
                    float currentDistance = (closestPoint - linePoint).magnitude;
                    if (currentDistance < distanceToGoal)
                    {
                        goalIndex = i;
                        distanceToGoal = currentDistance;
                        finalPath = currentPath;
                        wayPointPath = transform.position + direction * Mathf.Min(relativeVector.magnitude, waypointRange);
                        wayPointPathLong = transform.position + direction * Mathf.Min(relativeVector.magnitude, driverRange);
                    }
                }

                // Mark goal as green
                float waypointAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(initialtDir.x, 0, initialtDir.z), transform.up);
                Vector3 goalRayRotation = new Vector3(0, waypointAngle - routeAngles + 2 * routeAngles / (alternativeRoutes - 1f) * goalIndex, 0);

                Quaternion goalOrientation = transform.rotation * Quaternion.Euler(goalRayRotation);
                Matrix4x4 goalTransformMatrix = Matrix4x4.TRS(centerPos, goalOrientation, Vector3.one);

                Handles.matrix = goalTransformMatrix;
                Vector3 goalDirection = (goalTransformMatrix.rotation * Vector3.forward).normalized;
                Handles.color = Color.green;
                float goalRange = planRange;
                if (Physics.BoxCast(centerPos, boxDimensionsHalfs, goalDirection, out RaycastHit goalHit, goalOrientation, planRange, avoidanceMask))
                {
                    float distance = transform.InverseTransformPoint(goalHit.point).magnitude;
                    goalRange = distance - boxDimensionsHalfs.z;

                    Handles.DrawWireCube(new Vector3(0, 0, goalRange / 2 + boxDimensionsHalfs.z), boxDimensions + new Vector3(0, 0, goalRange));
                }
                else
                {
                    Handles.DrawWireCube(new Vector3(0, 0, planRange / 2 + boxDimensionsHalfs.z), boxDimensions + new Vector3(0, 0, planRange));
                }
            }            

            wayPoint.transform.position = wayPointPath;
        }
    }

    private bool ObjectInPathGizmo(Matrix4x4 transformMatrix, Quaternion orientationInitial, Vector3 relativeVector)
    {
        Vector3 centerPos = transform.TransformPoint(0, .5f, -.2f);

        Handles.matrix = transformMatrix;
        Handles.color = Color.green;

        bool obstacleAhead = false;
        Vector3 pathDirection = (transformMatrix.rotation * Vector3.forward).normalized;

        if (Physics.BoxCast(centerPos, boxDimensionsHalfs, pathDirection, out RaycastHit hit, orientationInitial, planRange, avoidanceMask))
        {
            float distance = transform.InverseTransformPoint(hit.point).magnitude;
            float pathRange = distance - boxDimensionsHalfs.z;

            // If object between vehicle and target
            if (pathRange < relativeVector.magnitude)
            {
                obstacleAhead = true;
            }
            else
            {
                float targetRange = Mathf.Min(pathRange, relativeVector.magnitude - boxDimensionsHalfs.z);
                Handles.DrawWireCube(new Vector3(0, 0, targetRange / 2 + boxDimensionsHalfs.z), boxDimensions + new Vector3(0, 0, targetRange));
            }
        }
        else
        {
            float targetRange = Mathf.Min(driverRange, relativeVector.magnitude - boxDimensionsHalfs.z);
            Handles.DrawWireCube(new Vector3(0, 0, targetRange / 2 + boxDimensionsHalfs.z), boxDimensions + new Vector3(0, 0, targetRange));
        }

        return obstacleAhead;
    }

    private void TurningRadiusGizmo()
    {
        Handles.color = Color.yellow;

        Vector3 offset = new Vector3(turningRadius * Mathf.Sign(steer), 0, -wheelDistanceLength / 2);

        Vector3 startPoint;

        // Right cornering
        if (steer >= 0)
        {
            startPoint = new Vector3(-1, 0, 0);

            if (vehicleSpeed < 0)
            {
                Handles.color = Color.red;
                Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
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

        // Left cornering
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

    public void InitPath()
    {
        paths = new List<WaypointPath>();
        foreach (WaypointPath path in pathMaster.GetComponentsInChildren<WaypointPath>())
        {
            paths.Add(path);
        }

        pathNodes = new List<PathNode>();
        pathNodes = paths[currentPath].pathNodes;

        wayPoint = new GameObject("TargetWaypoint");

        wayPoint.transform.parent = pathMaster.transform;
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
