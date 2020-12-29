using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVMovement : MonoBehaviour
{
    public float angularSpeed = 1f;
    public float circleRad = 1f;

    private Vector3 fixedPoint;
    private Vector3 fixedAngle;
    private float currentAngle;

    void Start()
    {
        fixedPoint = transform.position;
        fixedAngle = transform.eulerAngles;
    }

    void Update()
    {
        currentAngle += angularSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle)) * circleRad;
        transform.position = fixedPoint + offset;
        transform.eulerAngles = fixedAngle + new Vector3(0, currentAngle * Mathf.Rad2Deg, 0);
    }
}
