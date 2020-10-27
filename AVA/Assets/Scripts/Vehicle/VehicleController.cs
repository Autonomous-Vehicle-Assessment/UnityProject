using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    private Vehicle vehicle;

    // ----- Engine Curves ----- //
    public AnimationCurve torqueCurve;
    public List<int> torqueCurveRpm;
    public List<int> torqueCurveValues;

    public AnimationCurve powerCurve;
    public List<int> powerCurveRpm;
    public List<int> powerCurveValues;

    public AnimationCurve torqueConverterCurve;
    public List<float> torqueConverterCurveSpeedRatio;
    public List<float> torqueConverterCurveValues;

    public AnimationCurve torqueConverterEfficiencyCurve;
    public List<float> torqueConverterEfficiencyCurveSpeedRatio;
    public List<float> torqueConverterEfficiencyCurveValues;

    public AnimationCurve throttleCurve;
    public AnimationCurve engineFriction;
    public AnimationCurve clutchPedalCurve;
    public float engineRpm;
    public float engineTorque;
    public float transmissionRpm;

    public int CurrentGear;
    public GearMode gearMode;

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
        SetupCurves();
        vehicle = new Vehicle(VehicleSetupAssemble());
        vehicle.data[Channel.Vehicle][VehicleData.EngineRpm] = 700 * 1000;
        vehicle.data[Channel.Vehicle][VehicleData.EngineWorking] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        vehicle.data[Channel.Input][InputData.Throttle] = (int)(throttleCurve.Evaluate(Input.GetAxis("Throttle")) * 10000);
        vehicle.data[Channel.Input][InputData.Clutch] = (int)(Input.GetAxis("Clutch") * 10000);
        
        vehicle.data[Channel.Input][InputData.Clutch] = 1 * 10000;
        vehicle.data[Channel.Input][InputData.AutomaticGear] = (int)gearMode;

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
        engineRpm = vehicle.data[Channel.Vehicle][VehicleData.EngineRpm] / 1000.0f;
        engineTorque = vehicle.data[Channel.Vehicle][VehicleData.EngineTorque] / 1000.0f;
        transmissionRpm = vehicle.data[Channel.Vehicle][VehicleData.TransmissionRpm] / 1000.0f;

        CurrentGear = vehicle.data[Channel.Vehicle][VehicleData.GearboxGear];

    }


    public VehicleSetup VehicleSetupAssemble()
    {
        Engine engine = EngineSetup();
        DriveTrain driveTrain = DriveTrainSetup();
        Suspension suspension = new Suspension();
        Wheels wheels = new Wheels();
        Brakes brakes = new Brakes();
        Steering steering = new Steering();
        AeroDynamics aeroDynamics = new AeroDynamics();

        return new VehicleSetup(engine, driveTrain, suspension, wheels, brakes, steering, aeroDynamics);
    }

    public Engine EngineSetup()
    {
        return new Engine(torqueCurve, powerCurve, engineFriction, 0.15f);
    }

    public DriveTrain DriveTrainSetup()
    {
        GearBox gearBox = new GearBox(GearRatio, GearEff, ReverseGearRatio, ReverseGearEff);
        TransferCase transferCase = new TransferCase(TransferCaseRatio, TransferCaseEff);
        return new DriveTrain(gearBox, transferCase, clutchPedalCurve, torqueConverterCurve);
    }
    
    public void SetupCurves()
    {
        torqueCurve = new AnimationCurve();
        powerCurve = new AnimationCurve();
        torqueConverterCurve = new AnimationCurve();
        torqueConverterEfficiencyCurve = new AnimationCurve();

        torqueCurve.keys = ListToKeyframes(torqueCurveRpm, torqueCurveValues);
        powerCurve.keys = ListToKeyframes(powerCurveRpm, powerCurveValues);
        torqueConverterCurve.keys = ListToKeyframes(torqueConverterCurveSpeedRatio, torqueConverterCurveValues);
        torqueConverterEfficiencyCurve.keys = ListToKeyframes(torqueConverterEfficiencyCurveSpeedRatio, torqueConverterEfficiencyCurveValues);

        torqueCurve = SmoothCurve(torqueCurve);
        powerCurve = SmoothCurve(powerCurve);
        torqueConverterCurve = SmoothCurve(torqueConverterCurve);
        torqueConverterEfficiencyCurve = SmoothCurve(torqueConverterEfficiencyCurve);
    }

    /// <summary>
    /// Converts two lists into an array of keyframes.
    /// </summary>
    /// <param name="time">x - values of the graph.</param>
    /// <param name="values">y - values of the graph.</param>
    /// <returns>Array of keyframes for constructing </returns>
    public Keyframe[] ListToKeyframes(List<int> time,List<int> values)
    {
        Keyframe[] keyframes = new Keyframe[time.Count];

        for (int i = 0; i < time.Count; i++)
        {
            keyframes[i] = new Keyframe(time[i], values[i]);
        }

        return keyframes;
    }

    /// <summary>
    /// Converts two lists into an array of keyframes.
    /// </summary>
    /// <param name="time">x - values of the graph.</param>
    /// <param name="values">y - values of the graph.</param>
    /// <returns>Array of keyframes for constructing </returns>
    public Keyframe[] ListToKeyframes(List<float> time, List<float> values)
    {
        Keyframe[] keyframes = new Keyframe[time.Count];

        for (int i = 0; i < time.Count; i++)
        {
            keyframes[i] = new Keyframe(time[i], values[i]);
        }

        return keyframes;
    }

    /// <summary>
    /// Smoothen animation curves, neighboring equal values sets tangents to 0.
    /// </summary>
    /// <param name="curve">Animation Curve to be smoothed</param>
    /// <returns>Smoothed animation curve</returns>
    public AnimationCurve SmoothCurve(AnimationCurve curve)
    {
        for (int i = 0; i < curve.keys.Length; i++)
        {

            if (i != 0 && i != curve.keys.Length-1)
            {
                float current = curve.keys[i].value;
                float previous = curve.keys[i - 1].value;
                float next = curve.keys[i + 1].value;

                if (current == next || current == previous)
                {
                    curve.keys[0].inTangent = 0;
                    curve.keys[0].outTangent = 0;
                }
                else
                {
                    curve.SmoothTangents(i, 0);
                }
            }
            else
            {
                curve.SmoothTangents(i, 0);
            }            
        }

        return curve;
    }
}
