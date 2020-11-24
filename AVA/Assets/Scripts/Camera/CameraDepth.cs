using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDepth : MonoBehaviour
{
    private Camera targetCamera;
    private GameObject targetVehicle;
    public float sensorRange;

    [Range(0f, 3f)]
    public float depthLevel;
    private Shader _shader;
    private Shader shader
    {
        get
        {
            return _shader != null ? _shader : (_shader = Shader.Find("Custom/RenderDepth"));
        }
    }

    private Material _material;
    private Material material
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            return _material;
        }
    }

    [Range(1,672)]
    public int width;
    [Range(1, 376)]
    public int height;
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


    // Start is called before the first frame update
    void Start()
    {
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

        foreach (GameObject vehicle in GameObject.FindGameObjectsWithTag("Vehicle"))
        {
            if (vehicle.name.Contains("Leader")) targetVehicle = vehicle;
        }
    }

    private void Update()
    {
        Vector3[,] temp = GetVehicleDepth();
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

            Vector3 topRight = transform.TransformVector(localTopRight);
            Vector3 topLeft = transform.TransformVector(localTopLeft);
            Vector3 bottomRight = transform.TransformVector(localBottomRight);
            Vector3 bottomLeft = transform.TransformVector(localBottomLeft);

            //Gizmos.color = Color.green;
            //Gizmos.DrawLine(transform.position, transform.position + transform.forward * sensorRange);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + topRight * sensorRange);
            Gizmos.DrawLine(transform.position, transform.position + topLeft * sensorRange);
            Gizmos.DrawLine(transform.position, transform.position + bottomRight * sensorRange);
            Gizmos.DrawLine(transform.position, transform.position + bottomLeft * sensorRange);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position + topLeft * sensorRange, transform.position + topRight * sensorRange);
            Gizmos.DrawLine(transform.position + topRight * sensorRange, transform.position + bottomRight * sensorRange);
            Gizmos.DrawLine(transform.position + bottomRight * sensorRange, transform.position + bottomLeft * sensorRange);
            Gizmos.DrawLine(transform.position + bottomLeft * sensorRange, transform.position + topLeft * sensorRange);

            if (targetVehicle != null)
            {
                Vector3 targetVector = transform.InverseTransformPoint(targetVehicle.transform.position).normalized;
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
                Vector3 direction = transform.TransformVector(localDirection);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + direction * sensorRange);
                Vector3 oldPoint = transform.position + direction * sensorRange;

                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * (vStepStart + height) + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * hStepStart + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = transform.TransformVector(localDirection);
                Gizmos.DrawLine(transform.position, transform.position + direction * sensorRange);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(oldPoint, transform.position + direction * sensorRange);
                oldPoint = transform.position + direction * sensorRange;

                Gizmos.color = Color.blue;
                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * (vStepStart + height) + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * (hStepStart + width) + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = transform.TransformVector(localDirection);
                Gizmos.DrawLine(transform.position, transform.position + direction * sensorRange);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(oldPoint, transform.position + direction * sensorRange);
                oldPoint = transform.position + direction * sensorRange;

                Gizmos.color = Color.blue;
                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * vStepStart + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * (hStepStart + width) + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = transform.TransformVector(localDirection);
                Gizmos.DrawLine(transform.position, transform.position + direction * sensorRange);

                //Gizmos.color = Color.green;
                //Gizmos.DrawLine(transform.position, transform.position + transform.TransformVector(targetVector) * sensorRange);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(oldPoint, transform.position + direction * sensorRange);
                oldPoint = transform.position + direction * sensorRange;

                yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * vStepStart + heightStepFull / 2;
                xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * hStepStart + widthStepFull / 2;

                localDirection = new Vector3(xVector, yVector, 1);
                direction = transform.TransformVector(localDirection);
                Gizmos.DrawLine(oldPoint, transform.position + direction * sensorRange);
            }
        }
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
                Vector3 direction = transform.TransformVector(localDirection);

                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, sensorRange, layerMask))
                {
                    depth[column, row] = transform.InverseTransformPoint(hit.point);
                }
                else
                {
                    depth[column, row] = new Vector3(float.NaN, float.NaN, float.NaN);
                }
            }
        }
        return depth;
    }

    public Vector3[,] GetVehicleDepth()
    {
        Vector3[,] depth = new Vector3[width, height];

        if(targetVehicle != null)
        {
            Vector3 targetVector = transform.InverseTransformPoint(targetVehicle.transform.position).normalized;
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
            int vStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelHeight - height - 1, vStepCenter - height / 2 - 1));
            int hStepStart = Mathf.Max(0, Mathf.Min(targetCamera.pixelWidth - width - 1, hStepCenter - width / 2 - 1));

            for (int row = 0; row < height; row++)
            {
                float yVector = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2) + heightStepFull * vStepStart + heightStepFull / 2;

                for (int column = 0; column < width; column++)
                {
                    // Setup unit vector
                    float xVector = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2) + widthStepFull * hStepStart + widthStepFull / 2;

                    Vector3 localDirection = new Vector3(xVector, yVector, 1);
                    Vector3 direction = transform.TransformVector(localDirection);

                    if (Physics.Raycast(transform.position, direction, out RaycastHit hit, sensorRange, layerMask))
                    {
                        depth[column, row] = transform.InverseTransformPoint(hit.point);
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
