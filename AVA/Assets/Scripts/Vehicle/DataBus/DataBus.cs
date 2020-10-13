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
    public const int Steer          = 0;
    public const int Throttle       = 1;
    public const int Brake          = 2;
    public const int Handbrake      = 3;
    public const int Clutch         = 4;
    public const int ManualGear     = 5;
    public const int AutomaticGear  = 6;
    public const int GearShift      = 7;
    public const int Retarder       = 8;
    public const int Key            = 9;
}

/// <summary>
/// Vehicle channel data.
/// </summary>
public static class VehicleData
{
    public const int Speed              = 0;
    public const int EngineRpm          = 1;
    public const int EngineStalled      = 2;
    public const int EngineWorking      = 3;
    public const int EngineStarting     = 4;
    public const int EngineLimiter      = 5;
    public const int EngineLoad         = 6;
    public const int EngineTorque       = 7;
    public const int EnginePower        = 8;
    public const int EngineFuelRate     = 9;
    public const int ClutchTorque       = 10;
    public const int ClutchLock         = 11;
    public const int GearboxGear        = 12;
    public const int GearboxMode        = 13;
    public const int GearboxShifting    = 14;
    public const int RetarderTorque     = 15;
    public const int TransmissionRpm    = 16;
    public const int AbsEngaged         = 17;
    public const int TcsEngaged         = 18;
    public const int EscEngaged         = 19;
    public const int AsrEngaged         = 20;
    public const int AidedSteer         = 21;
    public const int FuelConsumption    = 22;
}

/// <summary>
/// Vehicle Settings channel data.
/// </summary>
public static class SettingsData
{
    public const int DifferentialLock       = 0;
    public const int DrivelineLock          = 1;
    public const int AutoShiftOverride      = 2;
    public const int AbsOverride            = 3;
    public const int EscOverride            = 4;
    public const int TscOverride            = 5;
    public const int AsrOverride            = 6;
    public const int SteeringAidsOverride   = 7;
}