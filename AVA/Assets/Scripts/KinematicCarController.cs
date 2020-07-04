using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KinematicCarController : MonoBehaviour
{
    [SerializeField] private Rigidbody m_Rigidbody;
    [SerializeField] private WheelCollider m_WheelCollider;
    [SerializeField] private GameObject m_WheelMesh;
    [SerializeField] private SpeedType m_SpeedType;
    [SerializeField] private float m_Speed = 1;
    [SerializeField] private float m_SlipLimit;

    private Quaternion m_WheelMeshLocalRotations;

    // Start is called before the first frame update
    private void Start()
    {
        m_WheelMeshLocalRotations = m_WheelMesh.transform.localRotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Quaternion quat;
            Quaternion OffSet = new Quaternion(1, 1, 1, 1);
            Vector3 position;
            m_WheelCollider.GetWorldPose(out position, out quat);
            m_WheelMesh.transform.position = position;
            m_WheelMesh.transform.rotation = quat * OffSet;

            m_Rigidbody.velocity = transform.forward * m_Speed;
            Debug.Log(m_Rigidbody.velocity);
        }
    }
}
