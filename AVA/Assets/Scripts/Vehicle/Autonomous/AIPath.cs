using System.Collections.Generic;
using UnityEngine;

public class AIPath : MonoBehaviour
{
    public Color lineColor;

    //private List<Transform> nodes = new List<Transform>();
    private List<PathNode> pathNodes = new List<PathNode>();

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;

        PathNode[] nodes = GetComponentsInChildren<PathNode>();
        pathNodes = new List<PathNode>();

        for (int i = 0; i < nodes.Length; i++)
        {
            if(nodes[i].transform != transform)
            {
                pathNodes.Add(nodes[i]);
            }
        }

        for (int i = 0; i < pathNodes.Count; i++)
        {
            Vector3 currentNode = pathNodes[i].transform.position;
            Vector3 previousNode = Vector3.zero;

            if (i > 0)
            {
                previousNode = pathNodes[i - 1].transform.position;
            }
            else if(i == 0 && pathNodes.Count> 1)
            {
                previousNode = pathNodes[pathNodes.Count - 1].transform.position;
            }

            if (pathNodes[i].activeNode)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            
            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawWireSphere(currentNode, .2f);
        }
    }
}
