using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    private Vehicle vehicle;
    public AnimationCurve TorqueCurve;
    public AnimationCurve PowerCurve;
    public AnimationCurve ThrottleCurve;
    public AnimationCurve EngineFriction;
    public AnimationCurve ClutchCurve;
    public float EngineRpm;
    public float EngineTorque;
    public float TransmissionRpm;

    // Start is called before the first frame update
    void Awake()
    {
        vehicle = new Vehicle(VehicleSetupAssemble());
        vehicle.data[Channel.Vehicle][VehicleData.EngineRpm] = 700 * 10000;
        vehicle.data[Channel.Vehicle][VehicleData.EngineWorking] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        vehicle.data[Channel.Input][InputData.Throttle] = (int)(ThrottleCurve.Evaluate(Input.GetAxis("Throttle")) * 10000);
        vehicle.data[Channel.Input][InputData.Clutch] = (int)(Input.GetAxis("Clutch") * 10000);
        vehicle.data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(ClutchCurve.Evaluate(Mathf.Max(0,Input.GetAxis("Clutch"))) * 10000);
        vehicle.Update();
        EngineRpm = vehicle.data[Channel.Vehicle][VehicleData.EngineRpm] / 10000.0f;
        EngineTorque = vehicle.data[Channel.Vehicle][VehicleData.EngineTorque] / 10000.0f;
        TransmissionRpm = vehicle.data[Channel.Vehicle][VehicleData.TransmissionRpm] / 10000.0f;
    }


    public VehicleSetup VehicleSetupAssemble()
    {
        Engine engine = EngineSetup();
        Transmission transmission = TransmissionSetup();
        Suspension suspension = new Suspension();
        Wheels wheels = new Wheels();
        Brakes brakes = new Brakes();
        Steering steering = new Steering();
        AeroDynamics aeroDynamics = new AeroDynamics();

        return new VehicleSetup(engine, transmission, suspension, wheels, brakes, steering, aeroDynamics);
    }

    public Engine EngineSetup()
    {
        return new Engine(TorqueCurve, PowerCurve, EngineFriction, 0.15f);
    }

    public Transmission TransmissionSetup()
    {
        GearBox gearBox = new GearBox();
        return new Transmission(gearBox, ClutchCurve);
    }
}
