using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroDynamics : MonoBehaviour
{
    [SerializeField] private float frontalArea = 0.7f;          // Front area [m^2]
    [SerializeField] private float cd = 0.30f;                  // Drag Coefficient (Sign control, Z+ is car forward)
    [SerializeField] private float clFront = -0.05f;            // Lift front (negative = downforce)
    [SerializeField] private float clRear = -0.05f;             // Lift rear (negative = downforce)

    private float velDependentLiftFront;
    private float velDependentLiftRear;
    private float velDependentDrag;
    private Rigidbody rB;
    private WheelCollider[] wC;

    private const float airDensity = 1.2041f;                   // Air Density at 20° and 1 atm.

    void Start()
    {
        rB = GetComponent<Rigidbody>();                                 // Vehicle Rigidbody
        wC = gameObject.GetComponentsInChildren<WheelCollider>();       // Array of Wheel Colliders

        float tempDragVar = airDensity * frontalArea * 0.5f;

        velDependentLiftFront = clFront * tempDragVar / 4.0f;           // Velocity dependent lift (Front), divided by 4 
        velDependentLiftRear = clRear * tempDragVar / 4.0f;             // Velocity dependent lift (Rear), divided by 4 
        
        velDependentDrag = cd * tempDragVar;                            // Velocity dependent drag (Front)
    }

    public void ApplyAeroDrag(float vel)
    {
        float drag = velDependentDrag * vel * Mathf.Abs(vel);
        rB.AddRelativeForce(0.0f, 0.0f, -drag, ForceMode.Force);
    }

    public void ApplyAeroLift(float vel)
    {
        float velSq = vel * vel;
        float liftFront = velDependentLiftFront * velSq;
        float liftRear = velDependentLiftRear * velSq;

        //Apply lift at Wheels for stability
        for (int i = 0; i < 2; i++)
        {
            rB.AddForceAtPosition(wC[i].transform.up * liftRear, wC[i].transform.position);
            rB.AddForceAtPosition(wC[i + 2].transform.up * liftFront, wC[i + 2].transform.position);
        }
    }
}
