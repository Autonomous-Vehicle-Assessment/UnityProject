using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LiDAR : MonoBehaviour
{
    [Header("Sensors")]
    public float sensorLength = 3;
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
    public float sensorAngle = 30f;
    [Range(.5f, 30f)]
    public float angleChange = 180;
    [Range(1,20)]
    public int numberOfRays = 3;
    public bool showSensor;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position + frontSensorPosition;

        // Front center sensor
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);

        }

        // Front right angle sensor
        if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);

        }

        // Front Left angle sensor
        if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);

        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showSensor)
        {
            Gizmos.color = Color.cyan;
            Handles.color = Color.red;

            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(frontSensorPosition), transform.rotation, Vector3.one);

            Vector3 offsetUp = new Vector3(0, Mathf.Sin(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength, Mathf.Cos(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength - sensorLength);
            Vector3 offsetDown = new Vector3(0, -Mathf.Sin(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength, Mathf.Cos(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength - sensorLength);

            Vector3 startPosition = Vector3.zero;
            int angleSteps = (int)(360f / angleChange);

            for (int i = 0; i < angleSteps; i++)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(frontSensorPosition), Quaternion.Euler(0, (360f / angleSteps) * i, 0), Vector3.one);
                Gizmos.DrawLine(Vector3.zero, Vector3.zero + transform.forward * sensorLength);
                Gizmos.DrawLine(Vector3.zero, Vector3.zero + transform.forward * sensorLength + offsetUp);
                Gizmos.DrawLine(Vector3.zero, Vector3.zero + transform.forward * sensorLength + offsetDown);
            }

            Handles.matrix = Gizmos.matrix;
            Handles.DrawWireDisc(startPosition, transform.up, sensorLength);
            Handles.DrawWireDisc(startPosition + new Vector3(0, Mathf.Sin(sensorAngle / 2 * Mathf.Deg2Rad) * sensorLength), transform.up, Mathf.Cos(sensorAngle / 2 * Mathf.Deg2Rad) * sensorLength);
            Handles.DrawWireDisc(startPosition + new Vector3(0, -Mathf.Sin(sensorAngle / 2 * Mathf.Deg2Rad) * sensorLength), transform.up, Mathf.Cos(sensorAngle / 2 * Mathf.Deg2Rad) * sensorLength);
        }
    }

    private void GetDepth()
    {
        Vector3[,] depthArray;

        for (int i = 0; i < angleChange; i++)
        {

        }
    }
}
