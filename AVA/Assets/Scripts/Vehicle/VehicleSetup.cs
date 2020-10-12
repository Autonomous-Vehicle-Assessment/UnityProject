using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSetup
{
    public Engine engine;
    public Transmission transmission;
    public Suspension suspension;
    public Wheels wheels;
    public Brakes brakes;
    public Steering steering;
    public AeroDynamics aeroDynamics;

    public VehicleSetup(Engine _engine, Transmission _transmission, Suspension _suspension, Wheels _wheels, Brakes _brakes, Steering _steering, AeroDynamics _aeroDynamics)
    {
        engine = _engine;
        transmission = _transmission;
        suspension = _suspension;
        wheels = _wheels;
        brakes = _brakes;
        steering = _steering;
        aeroDynamics = _aeroDynamics;
    }
}
