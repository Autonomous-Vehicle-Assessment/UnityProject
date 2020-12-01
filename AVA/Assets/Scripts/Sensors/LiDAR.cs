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
    [Range(.5f, 360f)]
    public float angleChange = 180;
    [Range(1,128)]
    public int numberOfRays = 3;
    private int angleSteps;
    public bool showLidarCollision;
    public bool showLidarRays;
    public bool colorRays;
    public bool active;
    [Range(1,10)]
    public int frequency;
    public float minDistance;

    private IEnumerator lidarRoutine;
    private bool routineActive;
    private Vector3[,] depthData;

    // Start is called before the first frame update
    void Awake()
    {
        angleSteps = (int)(360f / angleChange);
        lidarRoutine = LidarRoutine();
        routineActive = false;

        if (active)
        {
            depthData = new Vector3[numberOfRays, angleSteps];
            StartCoroutine(lidarRoutine);
            routineActive = true;
        }
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (active && !routineActive) 
        {
            depthData = new Vector3[numberOfRays, angleSteps];
            StartCoroutine(lidarRoutine);
            routineActive = true;
        }
        if (!active && routineActive) 
        {
            StopCoroutine(lidarRoutine);
            routineActive = false;
        }
        
    }

    private void OnApplicationQuit()
    {
        if (routineActive)
        {
            StopCoroutine(lidarRoutine);
            routineActive = false;
        }
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

    private void OnDrawGizmos()
    {
        if (showLidarRays || showLidarCollision)
        {
            Gizmos.color = Color.cyan;
            Handles.color = Color.red;
            int angleSteps = (int)(360f / angleChange);

            for (int i = 0; i < angleSteps; i++)
            {
                float yOffset = -Mathf.Sin(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength;
                float zOffset = Mathf.Cos(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength - sensorLength;
                Vector3 offsetDown = new Vector3(0, yOffset, zOffset);

                for (int j = 0; j < numberOfRays; j++)
                {
                    Quaternion angleRotation = transform.rotation * Quaternion.Euler(0, (360f / angleSteps) * i, 0);
                    Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(frontSensorPosition), angleRotation, Vector3.one);
                    float angle = -sensorAngle / 2f + sensorAngle / (numberOfRays - 1) * j;
                    float yOffsetPoint = Mathf.Sin(angle * Mathf.Deg2Rad) * sensorLength - yOffset;
                    float zOffsetPoint = Mathf.Cos(angle * Mathf.Deg2Rad) * sensorLength - zOffset - sensorLength;

                    Vector3 offsetUp = new Vector3(0, yOffsetPoint, zOffsetPoint);
                    Vector3 target = new Vector3(0, 0, 1) * sensorLength + offsetDown + offsetUp;

                    Color rayColor = Color.cyan;
                    rayColor.a = .05f;
                    Gizmos.color = rayColor;


                    Color rayColorHit = Color.cyan;

                    Vector3 pos = transform.TransformPoint(frontSensorPosition);
                    Vector3 dir = Gizmos.matrix.MultiplyVector(target);

                    if (Physics.Raycast(pos, dir, out RaycastHit hit, sensorLength))
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        if ((hit.point - pos).magnitude > minDistance)
                        {
                            if (showLidarCollision)
                            {
                                float normalAngle = Vector3.Angle(dir, hit.normal);
                                float normStrength = -1f + normalAngle * 1f / 70f;
                                rayColorHit = Color.Lerp(Color.magenta, Color.yellow, normStrength);
                                Gizmos.color = rayColorHit;
                                Gizmos.DrawWireCube(hit.point, Vector3.one * .05f);
                            }

                        }
                        rayColorHit.a = .05f;
                        if (colorRays) Gizmos.color = rayColorHit;
                        else Gizmos.color = rayColor;
                        if (showLidarRays) Gizmos.DrawLine(pos, hit.point);
                    }
                    else
                    {
                        rayColorHit.a = .05f;
                        Gizmos.color = rayColorHit;
                        if (showLidarRays) Gizmos.DrawLine(Vector3.zero, Vector3.zero + target);
                    }


                }
            }
        }
    }

    public Vector3[] GetDepthArray()
    {
        List<Vector3> depthList = new List<Vector3>();
        int index = 0;
        for (int angle = 0; angle < angleSteps; angle++)
        {
            float yOffset = -Mathf.Sin(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength;
            float zOffset = Mathf.Cos(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength - sensorLength;
            Vector3 offsetDown = new Vector3(0, yOffset, zOffset);

            for (int ray = 0; ray < numberOfRays; ray++)
            {
                Quaternion angleRotation = transform.rotation * Quaternion.Euler(0, (360f / angleSteps) * angle, 0);
                Handles.matrix = Matrix4x4.TRS(transform.TransformPoint(frontSensorPosition), angleRotation, Vector3.one);
                float rayAngle = -sensorAngle / 2f + sensorAngle / (numberOfRays - 1) * ray;
                float yOffsetPoint = Mathf.Sin(rayAngle * Mathf.Deg2Rad) * sensorLength - yOffset;
                float zOffsetPoint = Mathf.Cos(rayAngle * Mathf.Deg2Rad) * sensorLength - zOffset - sensorLength;

                Vector3 offsetUp = new Vector3(0, yOffsetPoint, zOffsetPoint);
                Vector3 target = new Vector3(0, 0, 1) * sensorLength + offsetDown + offsetUp;


                Vector3 pos = transform.TransformPoint(frontSensorPosition);
                Vector3 dir = Handles.matrix.MultiplyVector(target);

                if (Physics.Raycast(pos, dir, out RaycastHit hit, sensorLength))
                {
                    if ((hit.point - pos).magnitude > minDistance)
                    {
                        depthList.Add(transform.TransformVector(hit.point - pos));
                    }
                }
            }
        }

        //Vector3[] depthArray = new Vector3[depthList.Count];
        Vector3[] depthArray = depthList.ToArray();

        return depthArray;
    }

    public Vector3[,] GetDepthMatrix()
    {
        Vector3[,] depthArray = new Vector3[numberOfRays, angleSteps];

        for (int angle = 0; angle < angleSteps; angle++)
        {
            float yOffset = -Mathf.Sin(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength;
            float zOffset = Mathf.Cos(sensorAngle / 2f * Mathf.Deg2Rad) * sensorLength - sensorLength;
            Vector3 offsetDown = new Vector3(0, yOffset, zOffset);

            for (int ray = 0; ray < numberOfRays; ray++)
            {
                Quaternion angleRotation = transform.rotation * Quaternion.Euler(0, (360f / angleSteps) * angle, 0);
                Handles.matrix = Matrix4x4.TRS(transform.TransformPoint(frontSensorPosition), angleRotation, Vector3.one);
                float rayAngle = -sensorAngle / 2f + sensorAngle / (numberOfRays - 1) * ray;
                float yOffsetPoint = Mathf.Sin(rayAngle * Mathf.Deg2Rad) * sensorLength - yOffset;
                float zOffsetPoint = Mathf.Cos(rayAngle * Mathf.Deg2Rad) * sensorLength - zOffset - sensorLength;

                Vector3 offsetUp = new Vector3(0, yOffsetPoint, zOffsetPoint);
                Vector3 target = new Vector3(0, 0, 1) * sensorLength + offsetDown + offsetUp;


                Vector3 pos = transform.TransformPoint(frontSensorPosition);
                Vector3 dir = Handles.matrix.MultiplyVector(target);

                if (Physics.Raycast(pos, dir, out RaycastHit hit, sensorLength))
                {
                    if ((hit.point - pos).magnitude > minDistance)
                    {
                        depthArray[ray, angle] = transform.TransformVector(hit.point - pos);
                    }
                }
            }
        }

        return depthArray;
    }

    IEnumerator LidarRoutine()
    {
        while (active)
        {
            GetDepthMatrix();
            yield return new WaitForSeconds(1f / frequency);
        }
    }
}
