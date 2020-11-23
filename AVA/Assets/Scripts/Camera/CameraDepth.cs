using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDepth : MonoBehaviour
{
    private Camera targetCamera;
    public float sensorRange;

    //[Range(0f, 3f)]
    //public float depthLevel;
    //private Shader _shader;
    //private Shader shader
    //{
    //    get { 
    //        return _shader != null ? _shader : (_shader = Shader.Find("Custom/RenderDepth"));
    //    }
    //}

    //private Material _material;
    //private Material material
    //{
    //    get
    //    {
    //        if(_material == null)
    //        {
    //            _material = new Material(shader);
    //            _material.hideFlags = HideFlags.HideAndDontSave;
    //        }
    //        return _material;
    //    }
    //}


    public int width;
    public int height;
    public bool showFieldOfView;
    public LayerMask layerMask;

    private float vFoV;
    private float hFoV;

    private float heightStep;
    private float widthStep;


    // Start is called before the first frame update
    void Start()
    {
        targetCamera = GetComponent<Camera>();

        // targetCamera.depthTextureMode = DepthTextureMode.Depth;

        vFoV = targetCamera.fieldOfView;
        hFoV = Camera.VerticalToHorizontalFieldOfView(vFoV, targetCamera.aspect);

        float hDistance = Mathf.Tan(hFoV * Mathf.Deg2Rad / 2);
        float vDistance = Mathf.Tan(vFoV * Mathf.Deg2Rad / 2);

        widthStep = hDistance / width;
        heightStep = vDistance / height;
    }

    private void Update()
    {
        float[,] depthValues = GetDepth();
    }
    //private void OnDisable()
    //{
    //    if (_material != null)
    //    {
    //        DestroyImmediate(_material);
    //    }
    //}

    //private void OnRenderImage(RenderTexture src, RenderTexture dest)
    //{
    //    if(shader != null)
    //    {
    //        material.SetFloat("_DepthLevel", depthLevel);
    //        Graphics.Blit(src, dest, material);
    //    }
    //    else
    //    {
    //        Graphics.Blit(src, dest);
    //    }
    //}

    private void OnDrawGizmos()
    {
        if (showFieldOfView)
        {
            float vFoV = targetCamera.fieldOfView;

            float hDistance = Mathf.Tan(Camera.VerticalToHorizontalFieldOfView(vFoV, targetCamera.aspect) * Mathf.Deg2Rad / 2);
            float vDistance = Mathf.Tan(vFoV * Mathf.Deg2Rad / 2);

            Vector3 localTopRight = new Vector3(hDistance, vDistance, 1);
            Vector3 localTopLeft = new Vector3(-hDistance, vDistance, 1);
            Vector3 localBottomRight = new Vector3(hDistance, -vDistance, 1);
            Vector3 localBottomLeft = new Vector3(-hDistance, -vDistance, 1);

            Vector3 topRight = transform.TransformVector(localTopRight);
            Vector3 topLeft = transform.TransformVector(localTopLeft);
            Vector3 bottomRight = transform.TransformVector(localBottomRight);
            Vector3 bottomLeft = transform.TransformVector(localBottomLeft);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * sensorRange);
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
        }
    }
    public float[,] GetDepth()
    {
        float[,] depth = new float[width, height];
        for (int row = 0; row < height; row++)
        {
            float vDistance = Mathf.Tan(-vFoV * Mathf.Deg2Rad / 2 + heightStep * row + heightStep / 2);

            for (int column = 0; column < width; column++)
            {
                // Setup unit vector
                float hDistance = Mathf.Tan(-hFoV * Mathf.Deg2Rad / 2 + widthStep * column + widthStep / 2);

                Vector3 localDirection = new Vector3(hDistance, vDistance, 1);
                Vector3 direction = transform.TransformVector(localDirection);

                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, sensorRange, layerMask))
                {
                    float distance = Vector3.Distance(transform.position, hit.point);
                    depth[column, row] = distance;
                }
                else
                {
                    //Debug.DrawRay(transform.position, direction, Color.white);
                    depth[column, row] = Mathf.Infinity;
                }
            }
        }
        return depth;
    }
}
