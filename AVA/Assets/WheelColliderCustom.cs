using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelColliderCustom : MonoBehaviour
{
    public float WheelRadius;
    public bool aboutToCollide;
    public float distanceToCollision;
    private Transform wheel;

    void Start()
    {
        wheel = GetComponent<Transform>();
    }

    public void OnTriggerStay(Collider collider)
    {        
        Vector3 ContactPoint = collider.ClosestPoint(wheel.transform.position);
        //Vector3 WheelCenter = wheel.transform.position;

        Debug.Log("Contact Point: ");
        Debug.Log(ContactPoint);
        Debug.Log("Wheel Center: ");
        Debug.Log(wheel.transform.position);
    }
}
