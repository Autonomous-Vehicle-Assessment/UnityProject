/// <summary>
/// Vehicle data Channels.
/// </summary>
public static class Channel
{
    /// <summary>
    /// Vehicle Input channels (Throttle, Steering, etc.)
    /// </summary>
    public const int Input      = 0;
    /// <summary>
    /// Vehicle data channels (EngineRpm, Transmission Rpm, GearboxMode, etc.)
    /// </summary>
    public const int Vehicle    = 1;
    /// <summary>
    /// Vehicle settings channels (DifferentialLock, AbsOverride, AutoshiftOverride, etc.)
    /// </summary>
    public const int Settings   = 2;
}

/// <summary>
/// Vehicle Input channel data.
/// </summary>
public static class InputData
{
    /// <summary>
    /// Steering wheel position (%)
    /// <para>Resolution 10000</para>
    /// <para>-10000 = full left</para>
    /// <para>     0 = center</para>
    /// <para> 10000 = full right</para>
    /// </summary>
    public const int Steer          = 0;
    /// <summary>
    /// Throttle pedal position (%)
    /// <para>Resolution 10000</para>
    /// <para>5000 = 0.5 = 50%</para>
    /// </summary>
    public const int Throttle       = 1;
    /// <summary>
    /// Brake pedal position (%)
    /// <para>Resolution 10000</para>
    /// <para>5000 = 0.5 = 50%</para>
    /// </summary>
    public const int Brake          = 2;
    /// <summary>
    /// Handbrake position (%)
    /// <para>Resolution: 10000</para>
    /// <para>5000 = 0.5 = 50%</para>
    /// </summary>
    public const int Handbrake      = 3;
    /// <summary>
    /// Clutch pedal position (%)
    /// <para>Resolution 10000</para>
    /// <para>5000 = 0.5 = 50%</para>
    /// </summary>
    public const int Clutch         = 4;
    /// <summary>
    /// Manual gear level position (gear number)
    /// <para>-1 (reverse), 0 (neutral), 1, 2, 3, ...</para>
    /// </summary>
    public const int ManualGear = 5;
    /// <summary>
    /// Automatic transmission mode (gear mode): 0, 1, 2, 3, 4, 5 = M, P, R, N, D, L
    /// <para>M (0): Manual: do not automatically engage gears. Use manual gear shifting.</para>
    /// <para>P (1): Park</para>
    /// <para>R (2): Reverse</para>
    /// <para>N (3): Neutral</para>
    /// <para>D (4): Drive: automatically engage forward gears.</para>
    /// <para>L (5): Low: first gear only.</para>
    /// </summary>
    public const int AutomaticGear  = 6;
    /// <summary>
    /// Incremental gear shifting value (gear increment)
    /// <para>Add +1 for gear up or -1 for gear down</para>
    /// </summary>
    public const int GearShift      = 7;
    /// <summary>
    /// Transfer case selector switch
    /// <para> 0 = Low</para>
    /// <para> 1 = High</para>
    /// </summary>
    public const int TransferCase   = 8;
    /// <summary>
    /// Retarder brake stick position (retarder level)
    /// <para>0 (off), 1, 2, 3, ...</para>
    /// </summary>
    public const int Retarder       = 9;
    /// <summary>
    /// Ignition key position
    /// <para>-1 = off</para>
    /// <para> 0 = acc-on</para>
    /// <para> 1 = start</para>
    /// </summary>
    public const int Key            = 10;
}

/// <summary>
/// Vehicle channel data.
/// </summary>
public static class VehicleData
{
    /// <summary>
    /// Vehicle aboslute velocity (m/s)
    /// <para>Resolution: 1000</para>
    /// <para>14500 = 14.5 m/s</para>
    /// </summary>
    public const int Speed              = 0;
    /// <summary>
    /// Engine RPMs (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>1200000 = 1200 rpm</para>
    /// </summary>
    public const int EngineRpm              = 1;
    /// <summary>
    /// Is the engine stalled?
    /// <para>0 = normal operation or switched off</para>
    /// <para>1 = engine stalled</para>
    /// </summary>
    public const int EngineStalled          = 2;
    /// <summary>
    /// Is the engine up and running?
    /// <para>0 = no</para>
    /// <para>1 = yes</para>
    /// </summary>
    public const int EngineWorking          = 3;
    /// <summary>
    /// Is the engine starting as per the ignition input?
    /// <para>0 = no</para>
    /// <para>1 = yes</para>
    /// </summary>
    public const int EngineStarting         = 4;
    /// <summary>
    /// Is the rpm limiter cutting engine power?
    /// <para>0 = yes</para>
    /// <para>1 = yes</para>
    /// </summary>
    public const int EngineLimiter          = 5;
    /// <summary>
    /// How much load is demanded (%)
    /// <para>Resolution: 1000</para>
    /// <para>200 = 0.2 = 20%</para>
    /// </summary>
    public const int EngineLoad             = 6;
    /// <summary>
    /// Torque at the output of the engine (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>200000 = 200 Nm</para>
    /// </summary>
    public const int EngineTorque           = 7;
    /// <summary>
    /// Power developed by the engine (kW)
    /// <para>Resolution: 1000</para>
    /// <para>10000 = 10 kW</para>
    /// </summary>
    public const int EnginePower            = 8;
    /// <summary>
    /// Instant fuel consumption (L/s)
    /// <para>Resolution: 1000</para>
    /// <para>20100 = 20.1 L/s</para>
    /// </summary>
    public const int EngineFuelRate         = 9;
    /// <summary>
    /// Torque at the output of the clutch (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int ClutchTorque           = 10;
    /// <summary>
    /// Torque at the output of the clutch from slip(Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int ClutchSlipTorque       = 11;
    /// <summary>
    /// Clutch RPMs (rpm), transmission engine-side RPMs.
    /// <para>Resolution: 1000</para>
    /// <para>1200000 = 1200 rpm</para>
    /// </summary>
    public const int ClutchRpm              = 12;
    /// <summary>
    /// Lock ratio of the clutch (%)
    /// <para>Resolution: 1000</para>
    /// <para>800 = 0.8 = 80%</para>
    /// </summary>
    public const int ClutchLock             = 13;
    /// <summary>
    /// Engaged gear (gear number)
    /// <para>Negative = reverse</para>
    /// <para>       0 = Neutral or Park</para>
    /// <para>Positive = forward</para>
    /// </summary>
    public const int GearboxGear            = 14;
    /// <summary>
    /// Actual transmission mode (gear mode): 0, 1, 2, 3, 4, 5 = M, P, R, N, D, L
    /// <para>M (0): Manual: do not automatically engage gears. Use manual gear shifting.</para>
    /// <para>P (1): Park</para>
    /// <para>R (2): Reverse</para>
    /// <para>N (3): Neutral</para>
    /// <para>D (4): Drive: automatically engage forward gears.</para>
    /// <para>L (5): Low: first gear only.</para>
    /// </summary>
    public const int GearboxMode            = 15;
    /// <summary>
    /// Is the gearbox in the middle of a gear shift operation?
    /// <para>0 = no</para>
    /// <para>1 = yes</para>
    /// </summary>
    public const int GearboxShifting        = 16;
    /// <summary>
    /// Brake torque injected in the driveline by the retarder (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>2000000 = 2000 Nm</para>
    /// </summary>
    public const int RetarderTorque         = 17;
    /// <summary>
    /// Rpms at the output of the transmission (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>100000 = 100 rpm</para>
    /// </summary>
    public const int TransmissionRpm        = 18;
    /// <summary>
    /// Torque at the output of the transmission (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int TransmissionTorque     = 19;
    /// <summary>
    /// Rpms at the output of the transfercase (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>100000 = 100 rpm</para>
    /// </summary>
    public const int TransferCaseRpm        = 20;
    /// <summary>
    /// Torque at the output of the transfercase (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int TransferCaseTorque     = 21;
    /// <summary>
    /// Rpms at the output of the front left axle (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>100000 = 100 rpm</para>
    /// </summary>
    public const int FrontAxleLeftRpm       = 22;
    /// <summary>
    /// Rpms at the output of the front right axle (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>100000 = 100 rpm</para>
    /// </summary>
    public const int FrontAxleLeftRight     = 23;
    /// <summary>
    /// Torque at the output of the front left axle (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int FrontAxleLeftTorque    = 24;
    /// <summary>
    /// Torque at the output of the front right axle (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int FrontAxleRightTorque   = 25;
    /// <summary>
    /// Rpms at the output of the rear left axle (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>100000 = 100 rpm</para>
    /// </summary>
    public const int RearAxleLeftRpm        = 26;
    /// <summary>
    /// Rpms at the output of the rear right axle (rpm)
    /// <para>Resolution: 1000</para>
    /// <para>100000 = 100 rpm</para>
    /// </summary>
    public const int RearAxleLeftRight      = 27;
    /// <summary>
    /// Torque at the output of the rear left axle (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int RearAxleLeftTorque     = 28;
    /// <summary>
    /// Torque at the output of the rear right axle (Nm)
    /// <para>Resolution: 1000</para>
    /// <para>150000 = 150 Nm</para>
    /// </summary>
    public const int RearAxleRightTorque    = 29;
    /// <summary>
    /// Is the ABS being engaged in any wheel?
    /// <para>       0 = no</para>
    /// <para>non-zero = yes</para>
    /// </summary>
    public const int AbsEngaged             = 30;
    /// <summary>
    /// Is the TCS limiting the engine throttle?
    /// <para>       0 = no</para>
    /// <para>non-zero = yes</para>
    /// </summary>
    public const int TcsEngaged             = 31;
    /// <summary>
    /// Is the ESC applying brakes for keeping stability?
    /// <para>       0 = no</para>
    /// <para>non-zero = yes</para>
    /// </summary>
    public const int EscEngaged             = 32;
    /// <summary>
    /// Is the ASR applying brakes for reducing wheel slip?
    /// <para>       0 = no</para>
    /// <para>non-zero = yes</para>
    /// </summary>
    public const int AsrEngaged             = 33;
    /// <summary>
    /// Steering wheel position after steering aids (%)
    /// <para>Resolution 10000</para>
    /// <para>-10000 = full left</para>
    /// <para>     0 = center</para>
    /// <para> 10000 = full right</para>
    /// </summary>
    public const int AidedSteer             = 34;
    /// <summary>
    /// Overall fuel consumption (L/100km)
    /// <para>Resolution: 1000</para>
    /// <para>20230 = 20.23 L/100km</para>
    /// </summary>
    public const int FuelConsumption        = 35;
}

/// <summary>
/// Vehicle Settings channel data.
/// </summary>
public static class SettingsData
{
    /// <summary>
    /// Override lock settings at the differential
    /// <para>0 = no override</para>
    /// <para>1 = force locked differential</para>
    /// <para>2 = force open differential</para>
    /// </summary>
    public const int DifferentialLock       = 0;
    /// <summary>
    /// Override lock settings at the driveline
    /// <para>0 = no override</para>
    /// <para>1 = force locked driveline</para>
    /// <para>2 = force unlocked / open driveline</para>
    /// </summary>
    public const int DrivelineLock          = 1;
    /// <summary>
    /// Auto-shifting override setting
    /// <para>0 = no override</para>
    /// <para>1 = force auto shift</para>
    /// <para>2 = force manual shift</para>
    /// </summary>
    public const int AutoShiftOverride      = 2;
    /// <summary>
    /// ABS override setting
    /// <para>0 = no override</para>
    /// <para>1 = force ABS enabled</para>
    /// <para>2 = force ABS disabled</para>
    /// </summary>
    public const int AbsOverride            = 3;
    /// <summary>
    /// ESC override setting
    /// <para>0 = no override</para>
    /// <para>1 = force ESC enabled</para>
    /// <para>2 = force ESC disabled</para>
    /// </summary>
    public const int EscOverride            = 4;
    /// <summary>
    /// TSC override setting
    /// <para>0 = no override</para>
    /// <para>1 = force TSC enabled</para>
    /// <para>2 = force TSC disabled</para>
    /// </summary>
    public const int TscOverride            = 5;
    /// <summary>
    /// ASR override setting
    /// <para>0 = no override</para>
    /// <para>1 = force ASR enabled</para>
    /// <para>2 = force ASR disabled</para>
    /// </summary>
    public const int AsrOverride            = 6;
    /// <summary>
    /// Steering aids override setting
    /// <para>0 = no override</para>
    /// <para>2 = force all steering aids disabled</para>
    /// </summary>
    public const int SteeringAidsOverride   = 7;
}