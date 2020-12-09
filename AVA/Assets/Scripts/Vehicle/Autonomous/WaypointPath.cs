using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public Color lineColor;

    public List<PathNode> pathNodes = new List<PathNode>();

    //private void OnDrawGizmos()
    //{
    //    PathNode[] nodes = GetComponentsInChildren<PathNode>();
    //    pathNodes = new List<PathNode>();

    //    for (int i = 0; i < nodes.Length; i++)
    //    {
    //        if (nodes[i].transform != transform)
    //        {
    //            pathNodes.Add(nodes[i]);
    //            nodes[i].SetHeight();
    //        }
    //    }

    //    for (int i = 0; i < pathNodes.Count; i++)
    //    {
    //        Vector3 currentNode = pathNodes[i].transform.position;

    //        if (pathNodes[i].activeNode)
    //        {
    //            if (lineColor == Color.white)
    //            {
    //                Gizmos.color = Color.black;
    //            }
    //            else
    //            {
    //                Gizmos.color = Color.white;
    //            }

    //            Gizmos.DrawWireSphere(currentNode, .5f);
    //        }
    //    }
    //}
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
                nodes[i].SetHeight();
            }
        }

        Vector3 previousNode;

        for (int i = 0; i < pathNodes.Count; i++)
        {
            Vector3 currentNode = pathNodes[i].transform.position;
            if (i > 0)
            {
                previousNode = pathNodes[i - 1].transform.position;
                Gizmos.color = lineColor;
                Gizmos.DrawLine(previousNode, currentNode);
            }
            //else if(i == 0 && pathNodes.Count> 1)
            //{
            //    previousNode = pathNodes[pathNodes.Count - 1].transform.position;
            //}


            if (pathNodes[i].activeNode)
            {
                if (lineColor == Color.white)
                {
                    Gizmos.color = Color.black;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

            }
            else
            {
                Gizmos.color = lineColor;
            }
            
            Gizmos.DrawWireSphere(currentNode, .2f);
        }
    }
}
