
/// <summary>
/// Base Vehicle Class
/// </summary>
public class Vehicle
{
    public int[][] data;                    // Data array (Jagged)
    public VehicleSetup vehicleSetup;      // Vehicle parameters 
    
    public Vehicle(VehicleSetup _vehicleSetup)
    {
        data = new int[3][];
        data[0] = new int[10];
        data[1] = new int[23];
        data[2] = new int[8];

        vehicleSetup = _vehicleSetup;
    }

    public void Update()
    {
        data = vehicleSetup.Update(data);
    }

    public void Set(int Channel, int DataChannel, int Data)
    {
        data[Channel][DataChannel] = Data;
    }

    public int Get(int Channel, int DataChannel)
    {
        return data[Channel][DataChannel];
    }

}
