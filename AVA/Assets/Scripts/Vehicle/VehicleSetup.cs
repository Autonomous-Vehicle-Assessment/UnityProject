using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSetup
{
    private Engine engine;
    private Transmission transmission;
    private Suspension suspension;
    private Wheels wheels;
    private Brakes brakes;
    private Steering steering;
    private AeroDynamics aeroDynamics;

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
