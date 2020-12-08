using UnityEngine;

public class WaypointGenerator : MonoBehaviour
{
    // public Transform PublishedTransform;
    public PathNode pathNode;
    public EngineModel leader;
    private Transform leaderCoG;
    public Vector3 offset;
    public bool active = true;

    private void Update()
    {
        if (active)
        {
            Vector3 leaderPos = leader.transform.TransformPoint(offset);
            Vector3 currentPos = pathNode.transform.position;
            pathNode.transform.position = Vector3.Lerp(currentPos,leaderPos, 10 * Time.deltaTime);
            pathNode.targetVelocity = leader.speed;
            pathNode.SetHeight();
            pathNode.Activate();
        }
    }

    private void OnDrawGizmosSelected()
    {
        pathNode.transform.position = leader.transform.TransformPoint(offset);
        pathNode.targetVelocity = leader.speed;
        pathNode.SetHeight();
    }
}