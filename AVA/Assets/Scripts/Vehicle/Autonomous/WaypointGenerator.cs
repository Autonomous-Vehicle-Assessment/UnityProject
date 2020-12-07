using UnityEngine;

public class WaypointGenerator : MonoBehaviour
{
    // public Transform PublishedTransform;
    public PathNode pathNode;
    public EngineModel leader;
    private Transform leaderCoG;
    public Vector3 offset;

    private void Update()
    {
        pathNode.transform.position = leader.transform.TransformPoint(offset);
        pathNode.targetVelocity = leader.speed;
        pathNode.SetHeight();
    }

    private void OnDrawGizmosSelected()
    {
        pathNode.transform.position = leader.transform.TransformPoint(offset);
        pathNode.targetVelocity = leader.speed;
        pathNode.SetHeight();
    }
}