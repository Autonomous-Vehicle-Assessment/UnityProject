using UnityEngine;
using System;

public enum LeaderFollowerMode
{
    Column,
    Diamond,
    SideBySide
}

public class PathNode : MonoBehaviour
{
    public float targetVelocity;
    [HideInInspector]
    public bool activeNode;
    [HideInInspector]
    public float targetHeight = 1;
    private float deltaHeight;
    public SpeedType speedType;

    [Header("Events")]
    public Transform eventObject;
    public bool objectState = true;
    public Vector3 movePosition;
    public Vector3 moveOrientation;


    [Header("Leader Follower TBD")]
    public WaypointGenerator[] leaderFollowers;
    public EngineModel[] leaders;
    public bool leaderState;
    public LeaderFollowerMode leaderFollowerMode;

    [Header("Waypoint Driver TBD")]
    public WaypointController[] driver;
    public PathLoader[] pathMasters;
    public float driverRange;
    public bool driverActive;
    public bool skipPath;
    public bool uAVThreat;

    public void SetHeight()
    {
        LayerMask mask = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 250, mask))
        {
            deltaHeight = targetHeight + transform.InverseTransformPoint(hit.point).y;

            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + deltaHeight, transform.position.z);
            transform.position = newPos;

            //Gizmos.DrawLine(transform.position, hit.point);
        }
    }

    public void RecalculateSpeed()
    {
        targetVelocity = targetVelocity * GenericFunctions.SpeedCoefficient(speedType);
    }

    public void WayPointEvent()
    {
        if (eventObject != null)
        {
            eventObject.gameObject.SetActive(objectState);
            eventObject.position = eventObject.TransformPoint(movePosition);
            eventObject.rotation = eventObject.rotation * Quaternion.Euler(moveOrientation);
        }

        int index = 0;

        if (leaderFollowers != null)
        {
            if(leaders.Length != 0)
            {
                // Set leader follower state and mode
                switch (leaderFollowerMode)
                {
                    case LeaderFollowerMode.Column:
                        Vector3 columnFormation = new Vector3(0, 0, -4);
                        foreach (WaypointGenerator waypointGenerator in leaderFollowers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = columnFormation;
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.Diamond:
                        Vector3[] diamondFormation = { new Vector3(-5, 0, 4), new Vector3(5, 0, 4), new Vector3(0, 0, -4) };
                        foreach (WaypointGenerator waypointGenerator in leaderFollowers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = diamondFormation[index];
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.SideBySide:
                        Vector3[] SideBySide = { new Vector3(0, 0, 0), new Vector3(5, 0, 11), new Vector3(5, 0, 0) };
                        foreach (WaypointGenerator waypointGenerator in leaderFollowers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = SideBySide[index];
                            index++;
                        }
                        break;
                    default:
                        break;
                }
            }
            

            foreach (WaypointGenerator waypointGenerator in leaderFollowers)
            {
                waypointGenerator.active = leaderState;
                waypointGenerator.pathNode.activeNode = leaderState;
            }

        }
        index = 0;

        if (driver != null)
        {
            foreach (WaypointController waypointController in driver)
            {
                if(driverRange != 0) waypointController.driverRange = driverRange;
                if (pathMasters.Length == driver.Length) 
                {
                    waypointController.pathMaster = pathMasters[index];
                    waypointController.InitPath();
                    waypointController.currentNode = 0;
                    waypointController.currentPath = 0;
                }

                index++;
                
            }
            if (uAVThreat)
            {
                // Skip to UAV alternate return path   
            }

            if (skipPath)
            {
                // Skip to next path
                // Set current node to 0
                // Update active waypoint
                // Deactivate previous point
            }
        }


    }

    public void Activate()
    {
        activeNode = true;
        WayPointEvent();
    }

}
