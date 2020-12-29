using UnityEngine;
using System;

public enum LeaderFollowerMode
{
    Column,
    Diamond,
    DiamondLong,
    SideBySide,
    LeapFrog,
    EchelonRight,
    EchelonLeft
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
    public WaypointGenerator[] followers;
    public bool followerState;
    public EngineModel[] leaders;
    public LeaderFollowerMode leaderFollowerMode;

    [Header("Waypoint Driver TBD")]
    public WaypointController[] driver;
    public PathLoader[] pathMasters;
    public float driverRange;
    public bool driverActive;
    public bool park;
    public bool returnPath;
    public bool surveyPath;
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

        if (followers != null)
        {
            if(leaders.Length != 0)
            {
                // Set leader follower state and mode
                switch (leaderFollowerMode)
                {
                    case LeaderFollowerMode.Column:
                        Vector3 columnFormation = new Vector3(0, 0, -4);
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = columnFormation;
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.Diamond:
                        Vector3[] diamondFormation = { new Vector3(-10, 0, -3), new Vector3(10, 0, -3), new Vector3(0, 0, -15) };
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = diamondFormation[index];
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.DiamondLong:
                        Vector3[] diamondLongFormation = { new Vector3(-10, 0, -5), new Vector3(10, 0, -15), new Vector3(0, 0, -15) };
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = diamondLongFormation[index];
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.SideBySide:
                        Vector3[] SideBySide = { new Vector3(5, 0, 3), new Vector3(2.5f, 0, -10), new Vector3(2.5f, 0, -20) };
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = SideBySide[index];
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.LeapFrog:
                        Vector3[] LeapFrog = { new Vector3(5, 0, 0), new Vector3(0, 0, -5), new Vector3(5, 0, -5) };
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = LeapFrog[index];
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.EchelonRight:
                        Vector3 EchelonRight = new Vector3(1, 0, -7);
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = EchelonRight;
                            index++;
                        }
                        break;
                    case LeaderFollowerMode.EchelonLeft:
                        Vector3 EchelonLeft = new Vector3(-1, 0, -4);
                        foreach (WaypointGenerator waypointGenerator in followers)
                        {
                            waypointGenerator.leader = leaders[index];
                            waypointGenerator.offset = EchelonLeft;
                            index++;
                        }
                        break;
                    default:
                        break;
                }
            }
            

            foreach (WaypointGenerator waypointGenerator in followers)
            {
                waypointGenerator.active = followerState;
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

                waypointController.park = park;

                index++;
                
            }
            if (uAVThreat)
            {
                // Skip to UAV alternate return path
                driver[0].currentPath++;
                driver[0].currentNode = 0;
            }
            if (skipPath)
            {
                driver[0].currentPath += 2;
                driver[0].currentNode = 0;
            }
            if (surveyPath)
            {
                driver[0].currentPath++;
                driver[0].storedNode = driver[0].currentNode;
                driver[0].currentNode = 0;                
            }
            if (returnPath)
            {
                driver[0].currentPath--;
                driver[0].currentNode = driver[0].storedNode;
            }
        }


    }

    public void Activate()
    {
        activeNode = true;
        WayPointEvent();
    }

}
