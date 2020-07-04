using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ConstraintBody : MonoBehaviour
{
    public Rigidbody m_Rigidbody;


    // Start is called before the first frame update
    void Awake()
    {
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }
}
