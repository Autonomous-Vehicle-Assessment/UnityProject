using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UAVThreat : MonoBehaviour
{
    private Vector3[] points;

    void OnDrawGizmos()
    {
        List<Vector3> pointList = new List<Vector3>();
        foreach (Transform transform in transform.GetComponentInChildren<Transform>())
        {
            pointList.Add(transform.position);
        }
        points = pointList.ToArray();

        //Handles.matrix = transform.localToWorldMatrix;
        Color color = Color.red;
        color.a = .25f;
        Handles.color = color;

        Handles.DrawAAConvexPolygon(points);
    }
}
