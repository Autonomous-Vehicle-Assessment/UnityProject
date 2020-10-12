using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    private Vehicle vehicle;

    // Start is called before the first frame update
    void Awake()
    {
        vehicle = new Vehicle(VehicleSetupAssemble());
    }

    // Update is called once per frame
    void Update()
    {
        vehicle.Update();
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
        return new Engine(new AnimationCurve(), new AnimationCurve());
    }

    public Transmission TransmissionSetup()
    {
        GearBox gearBox = new GearBox();
        return new Transmission(gearBox);
    }
}
