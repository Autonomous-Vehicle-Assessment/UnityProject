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

    public int CurrentGear;

    // ----- Gearbox - SETUP ----- //
    /// <summary>
    /// Array of Gear Ratio's
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para>
    /// </summary>
    public float[] GearRatio = new float[10];
    /// <summary>
    /// Array of Gear Efficiencies
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para>
    /// </summary>
    public float[] GearEff = new float[10];
    /// <summary>
    /// Array of reverse Gear ratios
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para>
    /// </summary>
    public float[] ReverseGearRatio = new float[10];
    /// <summary>
    /// Array of reverse Gear Efficiencies
    /// <para>[0] = 1st</para>
    /// <para>[1] = 2nd</para>
    /// <para>[2] = 3rd</para>
    /// <para>...</para>
    /// </summary>
    public float[] ReverseGearEff = new float[10];
    /// <summary>
    /// Array of Transfer Case ratios
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para>
    /// </summary>
    public float[] TransferCaseRatio = { 2.72f, 1.0f };
    /// <summary>
    /// Array of Transfer Case efficiencies
    /// <para>[0] = Low</para>
    /// <para>[1] = High</para>
    /// </summary>
    public float[] TransferCaseEff = new float[2];
    /// <summary>
    /// Gearing ratio of the final drive
    /// </summary>
    public float FinalDriveRatio = 1.0f;
    /// <summary>
    /// Gearing efficiency of the final drive
    /// </summary>
    public float FinalDriveEff = 1.0f;


    // Start is called before the first frame update
    void Awake()
    {
        vehicle = new Vehicle(VehicleSetupAssemble());
        vehicle.data[Channel.Vehicle][VehicleData.EngineRpm] = 700 * 1000;
        vehicle.data[Channel.Vehicle][VehicleData.EngineWorking] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        vehicle.data[Channel.Input][InputData.Throttle] = (int)(ThrottleCurve.Evaluate(Input.GetAxis("Throttle")) * 10000);
        vehicle.data[Channel.Input][InputData.Clutch] = (int)(Input.GetAxis("Clutch") * 10000);
        vehicle.data[Channel.Vehicle][VehicleData.ClutchLock] = (int)(ClutchCurve.Evaluate(Mathf.Max(0,Input.GetAxis("Clutch"))) * 1000);
        int GearShift = 0;
        if (Input.GetButtonUp("ShiftUp"))
        {
            GearShift = 1;
        }
        if (Input.GetButtonUp("ShiftDown"))
        {
            GearShift = -1;
        }
        vehicle.data[Channel.Input][InputData.GearShift] = GearShift;
        vehicle.Update();
        EngineRpm = vehicle.data[Channel.Vehicle][VehicleData.EngineRpm] / 1000.0f;
        EngineTorque = vehicle.data[Channel.Vehicle][VehicleData.EngineTorque] / 1000.0f;
        TransmissionRpm = vehicle.data[Channel.Vehicle][VehicleData.TransmissionRpm] / 1000.0f;

        CurrentGear = vehicle.data[Channel.Vehicle][VehicleData.GearboxGear];

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
        GearBox gearBox = new GearBox(GearRatio,GearEff,ReverseGearRatio,ReverseGearEff,TransferCaseRatio,TransferCaseEff,FinalDriveRatio,FinalDriveEff);
        return new Transmission(gearBox, ClutchCurve);
    }
}
