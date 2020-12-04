using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CameraDepth : MonoBehaviour
{
    private Camera targetCamera;
    public Transform targetVehicle;
    public float sensorRange = 100f;

    //[Range(0f, 3f)]
    //public float depthLevel;

    [Range(1,672)]
    public int width = 100;
    [Range(1, 376)]
    public int height = 4;
    public bool showFieldOfView;
    public LayerMask layerMask;

    private float vFoV;
    private float hFoV;
    private float hDistance;
    private float vDistance;

    private float heightStep;
    private float widthStep;
    private float heightStepFull;
    private float widthStepFull;
    [HideInInspector]
    public int vStepStart;
    [HideInInspector]
    public int hStepStart;
    public bool debugMode;


    // Start is called before the first frame update
    void Start()
    {
        // Transform camerasObject = transform.Find("Cameras");
        // Transform depthCameraObject = camerasObject.Find("Depth Camera");
        targetCamera = GetComponent<Camera>();

        targetCamera.depthTextureMode = DepthTextureMode.Depth;

        vFoV = targetCamera.fieldOfView;
        hFoV = Camera.VerticalToHorizontalFieldOfView(vFoV, targetCamera.aspect);

        hDistance = Mathf.Tan(hFoV * Mathf.Deg2Rad / 2) * 2;
        vDistance = Mathf.Tan(vFoV * Mathf.Deg2Rad / 2) * 2;

        widthStep = hDistance / width;
        heightStep = vDistance / height;

        widthStepFull = hDistance / targetCamera.pixelWidth;
        heightStepFull = vDistance / targetCamera.pixelHeight;

        //foreach (GameObject vehicle in GameObject.FindGameObjectsWithTag("Vehicle"))
        //{
        //    if (vehicle.name.Contains("Leader"))
        //    {
        //        targetVehicle = vehicle.transform.Find("Vehicle Mesh").Find("ColliderMesh");
        //    }
        //}
    }

    private void Update()
    {
        if (debugMode)
        {
            Vector3[,] temp = GetVehicleDepth();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showFieldOfView)
        {
            vFoV = targetCamera.fieldOfView;
            hFoV = Camera.VerticalToHorizontalFieldOfView(vFoV, targetCamera.aspect);

            hDistance = Mathf.Tan(hFoV * Mathf.Deg2Rad / 2) * 2;
            vDistance = Mathf.Tan(vFoV * Mathf.Deg2Rad / 2) * 2;

            Vector3 localTopRight = new Vector3(hDistance / 2, vDistance / 2, 1);
            Vector3 localTopLeft = new Vector3(-hDistance / 2, vDistance / 2, 1);
            Vector3 localBottomRight = new Vector3(hDistance / 2, -vDistance / 2, 1);
            Vector3 localBottomLeft = new Vector3(-hDistance / 2, -vDistance / 2, 1);

            Vector3 topRight = targetCamera.transform.TransformVector(localTopRight);
            Vector3 topLeft = targetCamera.transform.TransformVector(localTopLeft);
            Vector3 bottomRight = targetCamera.transform.TransformVector(localBottomRight);
            Vector3 bottomLeft = targetCamera.transform.TransformVector(localBottomLeft);

            //Gizmos.color = Color.green;
            //Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + targetCamera.transform.forward * sensorRange);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + topRight * sensorRange);
            Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + topLeft * sensorRange);
            Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + bottomRight * sensorRange);
            Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + bottomLeft * sensorRange);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(targetCamera.transform.position + topLeft * sensorRange, targetCamera.transform.position + topRight * sensorRange);
            Gizmos.DrawLine(targetCamera.transform.position + topRight * sensorRange, targetCamera.transform.position + bottomRight * sensorRange);
            Gizmos.DrawLine(targetCamera.transform.position + bottomRight * sensorRange, targetCamera.transform.position + bottomLeft * sensorRange);
            Gizmos.DrawLine(targetCamera.transform.position + bottomLeft * sensorRange, targetCamera.transform.position + topLeft * sensorRange);

            if (targetVehicle != null)
            {
                Vector3 targetVector = targetCamera.transform.InverseTransformPoint(targetVehicle.position).normalized;
                float vAngle = Mathf.Asin(targetVector.y);
                float hAngle = Mathf.Asin(targetVector.x);

                // Find closest pixel to given direction
                float vCenter = Mathf.Tan(vAngle);
                float hCenter = Mathf.Tan(hAngle);

                float vOffsetCenter = vCenter - -vDistance / 2;
                float hOffsetCenter = hCenter - -hDistance / 2;

                int vStepCenter = (int)(vOffsetCenter / heightStepFull);
                int hStepCenter = (int)(hOffsetCenter / widthStepFull);

                // Find offset from edge
                int vStepStart = Mathf.Max(0,Mathf.Min(targetCamera.pixelHeight - height - 1, vStepCenter - height / 2 - 1));
                int hStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelWidth - width - 1, hStepCenter - width / 2 - 1));

                float yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2 ) + heightStepFull* vStepStart + heightStepFull / 2;
                float xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2 ) + widthStepFull * hStepStart + widthStepFull / 2;

                Vector3 localDirection = new Vector3(xVector, yVector, 1);
                Vector3 direction = targetCamera.transform.TransformVector(localDirection);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + direction * sensorRange);
                Vector3 oldPoint = targetCamera.transform.position + direction * sensorRange;

                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * (vStepStart + height) + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * hStepStart + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = targetCamera.transform.TransformVector(localDirection);
                Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + direction * sensorRange);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(oldPoint, targetCamera.transform.position + direction * sensorRange);
                oldPoint = targetCamera.transform.position + direction * sensorRange;

                Gizmos.color = Color.blue;
                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * (vStepStart + height) + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * (hStepStart + width) + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = targetCamera.transform.TransformVector(localDirection);
                Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + direction * sensorRange);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(oldPoint, targetCamera.transform.position + direction * sensorRange);
                oldPoint = targetCamera.transform.position + direction * sensorRange;

                Gizmos.color = Color.blue;
                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * vStepStart + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * (hStepStart + width) + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = targetCamera.transform.TransformVector(localDirection);
                Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + direction * sensorRange);

                //Gizmos.color = Color.green;
                //Gizmos.DrawLine(targetCamera.transform.position, targetCamera.transform.position + targetCamera.transform.TransformVector(targetVector) * sensorRange);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(oldPoint, targetCamera.transform.position + direction * sensorRange);
                oldPoint = targetCamera.transform.position + direction * sensorRange;

                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * vStepStart + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * hStepStart + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = targetCamera.transform.TransformVector(localDirection);
                Gizmos.DrawLine(oldPoint, targetCamera.transform.position + direction * sensorRange);
            }
        }
    }


    public Vector3[] GetDepthArray()
    {
        List<Vector3> depth = new List<Vector3>();
        for (int row = 0; row < height; row++)
        {
            float yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2 + heightStep * row + heightStep / 2);

            for (int column = 0; column < width; column++)
            {
                // Setup unit vector
                float xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2 + widthStep * column + widthStep / 2);

                Vector3 localDirection = new Vector3(xVector, yVector, 1);
                Vector3 direction = targetCamera.transform.TransformVector(localDirection);

                if (Physics.Raycast(targetCamera.transform.position, direction, out RaycastHit hit, sensorRange, layerMask))
                {
                    depth.Add(transform.InverseTransformPoint(hit.point));
                }
                else
                {
                    depth.Add(new Vector3(float.NaN, float.NaN, float.NaN));
                }
            }
        }
        return depth.ToArray();
    }
    public Vector3[,] GetDepth()
    {
        Vector3[,] depth = new Vector3[width, height];
        for (int row = 0; row < height; row++)
        {
            float yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2 + heightStep * row + heightStep / 2);

            for (int column = 0; column < width; column++)
            {
                // Setup unit vector
                float xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2 + widthStep * column + widthStep / 2);

                Vector3 localDirection = new Vector3(xVector, yVector, 1);
                Vector3 direction = targetCamera.transform.TransformVector(localDirection);

                if (Physics.Raycast(targetCamera.transform.position, direction, out RaycastHit hit, sensorRange, layerMask))
                {
                    depth[column, row] = targetCamera.transform.InverseTransformPoint(hit.point);
                }
                else
                {
                    depth[column, row] = new Vector3(float.NaN, float.NaN, float.NaN);
                }
            }
        }
        return depth;
    }

    public Vector3[] GetVehicleDepthArray()
    {
        List<Vector3> depth = new List<Vector3>();

        vStepStart = 0;
        hStepStart = 0;

        if (targetVehicle != null)
        {
            Vector3 targetVector = targetCamera.transform.InverseTransformPoint(targetVehicle.position).normalized;
            float vAngle = Mathf.Asin(targetVector.y);
            float hAngle = Mathf.Asin(targetVector.x);

            // Find closest pixel to given direction
            float vCenter = Mathf.Tan(vAngle);
            float hCenter = Mathf.Tan(hAngle);

            float vOffsetCenter = vCenter - -vDistance / 2;
            float hOffsetCenter = hCenter - -hDistance / 2;

            int vStepCenter = (int)(vOffsetCenter / heightStepFull);
            int hStepCenter = (int)(hOffsetCenter / widthStepFull);

            // Find offset from edge
            vStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelHeight - height - 1, vStepCenter - height / 2 - 1));
            hStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelWidth - width - 1, hStepCenter - width / 2 - 1));

            for (int row = 0; row < height; row++)
            {
                float yVector = Mathf.Tan(vFoV * Mathf.Deg2Rad / 2) - heightStepFull * (vStepStart + row) + heightStepFull / 2;

                for (int column = 0; column < width; column++)
                {
                    // Setup unit vector
                    float xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * (hStepStart + column) + widthStepFull / 2;

                    Vector3 localDirection = new Vector3(xVector, yVector, 1);
                    Vector3 direction = targetCamera.transform.TransformVector(localDirection);
                    RaycastHit hit;

                    if (Physics.Raycast(targetCamera.transform.position, direction, out hit, sensorRange, layerMask))
                    {
                        depth.Add(targetCamera.transform.InverseTransformPoint(hit.point));

                        if (debugMode) Debug.DrawLine(targetCamera.transform.position, hit.point, Color.red);
                    }
                    else
                    {
                        depth.Add(new Vector3(float.NaN, float.NaN, float.NaN));
                    }
                }
            }
        }

        return depth.ToArray();
    }

    public Vector3[,] GetVehicleDepth()
    {
        Vector3[,] depth = new Vector3[width, height];

        vStepStart = 0;
        hStepStart = 0;

        if (targetVehicle != null)
        {
            Vector3 targetVector = targetCamera.transform.InverseTransformPoint(targetVehicle.position).normalized;
            float vAngle = Mathf.Asin(targetVector.y);
            float hAngle = Mathf.Asin(targetVector.x);

            // Find closest pixel to given direction
            float vCenter = Mathf.Tan(vAngle);
            float hCenter = Mathf.Tan(hAngle);

            float vOffsetCenter = vCenter - -vDistance / 2;
            float hOffsetCenter = hCenter - -hDistance / 2;

            int vStepCenter = (int)(vOffsetCenter / heightStepFull);
            int hStepCenter = (int)(hOffsetCenter / widthStepFull);

            // Find offset from edge
            vStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelHeight - height - 1, vStepCenter - height / 2 - 1));
            hStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelWidth - width - 1, hStepCenter - width / 2 - 1));

            for (int row = 0; row < height; row++)
            {
                float yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * (vStepStart + row) + heightStepFull / 2;

                for (int column = 0; column < width; column++)
                {
                    // Setup unit vector
                    float xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * (hStepStart + column) + widthStepFull / 2;

                    Vector3 localDirection = new Vector3(xVector, yVector, 1);
                    Vector3 direction = targetCamera.transform.TransformVector(localDirection);
                    RaycastHit hit;

                    if (Physics.Raycast(targetCamera.transform.position, direction, out hit, sensorRange, layerMask))
                    {
                        depth[column, row] = targetCamera.transform.InverseTransformPoint(hit.point);

                        if (debugMode) Debug.DrawLine(targetCamera.transform.position, hit.point, Color.red);
                    }
                    else
                    {
                        depth[column, row] = new Vector3(float.NaN, float.NaN, float.NaN);
                    }
                }
            }
        }
        
        return depth;
    }
}
