using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class drawCircleGizmo : MonoBehaviour
{
    public float radius;

    void OnDrawGizmos()
    {
        Color color = Color.red;
        color.a = 1f;
        Handles.color = color;
        
        Handles.DrawWireDisc(transform.position + new Vector3(-radius,0,0), transform.up, radius);
    }
}
