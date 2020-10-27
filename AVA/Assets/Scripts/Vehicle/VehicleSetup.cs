
public class VehicleSetup
{
    public Engine engine;
    public DriveTrain driveTrain;
    public Suspension suspension;
    public Wheels wheels;
    public Brakes brakes;
    public Steering steering;
    public AeroDynamics aeroDynamics;

    public VehicleSetup(Engine _engine, DriveTrain _driveTrain, Suspension _suspension, Wheels _wheels, Brakes _brakes, Steering _steering, AeroDynamics _aeroDynamics)
    {
        engine = _engine;
        driveTrain = _driveTrain;
        suspension = _suspension;
        wheels = _wheels;
        brakes = _brakes;
        steering = _steering;
        aeroDynamics = _aeroDynamics;
    }

    /// <summary>
    /// Updates all states of the vehicle.
    /// </summary>
    /// <param name="data">Vehicle databus</param>
    /// <returns></returns>
    public int[][] Update(int[][] data)
    {
        data = engine.Update(data);
        data = driveTrain.Update(data);
        engine.Equilibrium(data);

        return data;
    }
}
