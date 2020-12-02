using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TwistController : MonoBehaviour
{
    private EngineModel engine;               // Engine model
    private VehicleStats vehicleStats;

    [Header("TwistSubscriber")]
    public RosSharp.RosBridgeClient.TwistSubscriber twistSubscriber;
    public float targetLinearVelocity;
    public float targetAngularVelocity;
    public float targetTurningRadius;

    [Header("Driver")]
    public bool autonomousDriving;
    public float linearVelocity;
    public float angularVelocity;
    public float turningRadius;
    private float proportionalGain = .4f;
    private float brakeVel = 3;
    private float throttleCap = 1;

    // Vehicle parameters;
    private float wheelDistanceLength;
    private float wheelDistanceWidth;
    private float turningRadiusMin;

    [Header("Output")]
    [Range(-1,1)]
    public float throttle;
    [Range(0,1)]
    public float brake;
    [Range(-1,1)]
    public float steer;

    void Awake()
    {
        engine = GetComponent<EngineModel>();
        vehicleStats = GetComponent<VehicleStats>();

        wheelDistanceLength = Vector3.Distance(engine.wheels[0].collider.transform.position, engine.wheels[2].collider.transform.position);
        wheelDistanceWidth = Vector3.Distance(engine.wheels[0].collider.transform.position, engine.wheels[1].collider.transform.position);

        turningRadiusMin = wheelDistanceLength / Mathf.Atan(engine.maximumInnerSteerAngle * Mathf.Deg2Rad) + wheelDistanceWidth / 2;
    }

    void FixedUpdate()
    {
        linearVelocity = engine.speed * 1 / GenericFunctions.SpeedTypeConverterFloat(vehicleStats.m_SpeedType);

        Steer();
        Drive();

        if (autonomousDriving) engine.Move(steer, throttle, brake, 0);
        //else engine.Move(0, 0, 1, 0);
        // Debug.Log(engine.wheels[1].collider.steerAngle);
    }

    private void Steer()
    {
        targetAngularVelocity = twistSubscriber.angularVelocity.z;

        if (targetAngularVelocity == 0) 
        { 
            steer = 0;
            angularVelocity = 0;
            targetTurningRadius = 0;
            turningRadius = 0;
        }
        else
        {
            turningRadius = wheelDistanceLength / Mathf.Sin(steer * engine.maximumInnerSteerAngle * Mathf.Deg2Rad);

            angularVelocity = linearVelocity / turningRadius;

            targetTurningRadius = linearVelocity / targetAngularVelocity;

            if(Mathf.Abs(targetTurningRadius) < 6)
            {
                if (Mathf.Abs(targetTurningRadius) < 1) steer = 0;
                else steer = Mathf.Sign(targetTurningRadius);
            }
            else steer = Mathf.Asin(wheelDistanceLength / targetTurningRadius) * 1 / (engine.maximumInnerSteerAngle * Mathf.Deg2Rad);

                // Mathf.Sign(targetTurningRadius) * Mathf.Tan(wheelDistanceLength / (Mathf.Abs(targetTurningRadius) - wheelDistanceWidth / 2)) / (engine.maximumInnerSteerAngle * Mathf.Deg2Rad);
        }

        steer = Mathf.Min(1, Mathf.Max(-1, steer));
    }

    private void Drive()
    {
        targetLinearVelocity = twistSubscriber.linearVelocity.x;

        float speedError = targetLinearVelocity - linearVelocity;
        throttle = speedError * proportionalGain;

        if (speedError < 0)
        {
            if (linearVelocity > brakeVel)
            {
                brake = Mathf.Min(1,Mathf.Abs(throttle));
                throttle = 0;
            }
            else
            {
                brake = 0;
                throttle = Mathf.Max(-throttleCap, throttle);
            }            
        }

        else
        {
            if (linearVelocity < -brakeVel)
            {
                brake = Mathf.Min(1,Mathf.Abs(throttle));
                throttle = 0;
            }
            else
            {
                brake = 0;
                throttle = Mathf.Min(throttleCap, throttle);
            }
        }
    }


    //private void OnDrawGizmos()
    //{
    //    if (showDriver)
    //    {
    //        if(wayPoint != null)
    //        {
    //            Handles.color = Color.cyan;
    //            Handles.DrawWireDisc(transform.position, transform.up, driverRange);
    //            Handles.DrawLine(transform.position, wayPoint.transform.position);
    //            Gizmos.color = Color.red;
    //            Gizmos.DrawWireSphere(wayPoint.transform.position, .25f);
    //            Gizmos.color = Color.blue;
    //            Gizmos.DrawWireSphere(wirePoint, .25f);
    //            Gizmos.color = Color.yellow;
    //            Gizmos.DrawWireSphere(linePoint, .25f);
    //        }
    //    }
    //    if (showTurningRadius)
    //    {
    //        if (turningRadius < 100)
    //        {
    //            Handles.color = Color.yellow;
                
    //            Vector3 offset = new Vector3(turningRadius * Mathf.Sign(steer), 0, -wheelDistanceLength/2 * 1.1f);
    //            //Vector3 offsetMin = new Vector3(turningRadiusMin * Mathf.Sign(-steer), 0, wheelDistanceLength / 2);
    //            Vector3 startPoint;

    //            if (steer >= 0)
    //            {
    //                startPoint = new Vector3(-1, 0, 0);
                    
    //                if (vehicleSpeed < 0)
    //                {
    //                    Handles.color = Color.red;
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius - wheelDistanceWidth/2);
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
    //                }
    //                else
    //                {
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
    //                }
    //                if (withinTurning)
    //                {
    //                    Handles.color = Color.red;
    //                }
    //                else
    //                {
    //                    Handles.color = Color.green;
    //                }

    //                if (withinTurning)
    //                {
    //                    Handles.DrawWireDisc(transform.position + transform.TransformVector(offsetMin), transform.up, turningRadiusMin - wheelDistanceWidth / 2);
    //                    Gizmos.DrawWireSphere(turningRadiusCenter, .5f);
    //                }
    //            }
    //            else
    //            {
    //                startPoint = new Vector3(1, 0, 0);
    //                Handles.color = Color.yellow;
    //                if (vehicleSpeed < 0)
    //                {
    //                    Handles.color = Color.red;
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), 800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
    //                }
    //                else
    //                {
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius - wheelDistanceWidth / 2);
    //                    Handles.DrawWireArc(transform.position + transform.TransformVector(offset), transform.up, transform.TransformVector(startPoint), -800 / turningRadius, turningRadius + wheelDistanceWidth / 2);
    //                }

    //                if (withinTurning)
    //                {
    //                    Handles.color = Color.red;
    //                }
    //                else
    //                {
    //                    Handles.color = Color.green;
    //                }
    //                if (withinTurning)
    //                {
    //                    Handles.DrawWireDisc(transform.position + transform.TransformVector(offsetMin), transform.up, turningRadiusMin - wheelDistanceWidth / 2);
    //                    Gizmos.DrawWireSphere(turningRadiusCenter, .5f);
    //                }
    //            }
    //        }
    //    }
    //}

}
