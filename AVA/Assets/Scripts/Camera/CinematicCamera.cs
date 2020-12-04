using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    private Rigidbody cinematicRigidbody;
    public Vector3 worldVelocity;
    public Vector3 worldAngularVelocity;

    // Start is called before the first frame update
    void Start()
    {
        cinematicRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        cinematicRigidbody.velocity = worldVelocity;
        cinematicRigidbody.angularVelocity = worldAngularVelocity;
    }
}
