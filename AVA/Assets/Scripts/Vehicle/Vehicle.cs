using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Enumerator for selecting Data Channel.
/// </summary>
public enum Channel : int
{
    Input,
    Vehicle,
    Settings
}

/// <summary>
/// Enumerator for selecting Data in the Input Channel.
/// </summary>
public enum InputData : int
{
    Steer,
    Throttle,
    Brake,
    Handbrake,
    Clutch,
    ManualGear,
    AutomaticGear,
    GearShift,
    Retarder,
    Key
}

/// <summary>
/// Enumerator for selecting Data in the Vehicle Channel.
/// </summary>
public enum VehicleData : int
{
    Speed,
    EngineRpm,
    EngineStalled,
    EngineWorking,
    EntingStarting,
    EngineLimiter,
    EngineLoad,
    EngineTorque,
    EnginePower,
    EngineFuelRate,
    ClutchTorque,
    ClutchLock,
    GearboxGear,
    GearboxMode,
    GearboxShifting,
    RetarderTorque,
    TransmissionRpm,
    AbsEngaged,
    TcsEngaged,
    EscEngaged,
    AsrEngaged,
    AidedSteer,
    FuelConsumption
}

/// <summary>
/// Enumerator for selecting Data in the Settings Channel
/// </summary>
public enum SettingsData : int
{
    DifferentialLock,
    DrivelineLock,
    AutoShiftOverride,
    AbsOverride,
    EscOverride,
    TscOverride,
    AsrOverride,
    SteeringAidsOverride
}

/// <summary>
/// Base Vehicle Class
/// </summary>
public class Vehicle
{
    public int[][] data;                    // Data array (Jagged)
    private VehicleSetup vehicleSetup;      // Vehicle parameters 
    
    public Vehicle(VehicleSetup _vehicleSetup)
    {
        data = new int[3][];
        data[0] = new int[10];
        data[1] = new int[23];
        data[2] = new int[8];

        vehicleSetup = _vehicleSetup;
    }

    public void Set(Channel channel, InputData inputData, int Data)
    {
        data[(int)channel][(int)inputData] = Data;
    }
    public void Set(Channel channel, VehicleData vehicleData, int Data)
    {
        data[(int)channel][(int)vehicleData] = Data;
    }
    public void Set(Channel channel, SettingsData settingsData, int Data)
    {
        data[(int)channel][(int)settingsData] = Data;
    }

    public int Get(Channel channel, InputData inputData)
    {
        return data[(int)channel][(int)inputData];
    }
    public int Get(Channel channel, VehicleData vehicleData)
    {
        return data[(int)channel][(int)vehicleData];
    }
    public int Get(Channel channel, SettingsData settingsData)
    {
        return data[(int)channel][(int)settingsData];
    }
}
