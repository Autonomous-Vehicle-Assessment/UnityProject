
public class Differential
{
    public float propshaftEfficiency;
    public float axleEfficiency;
    public float finalDriveEfficiency;
    public float finalDriveRatio;
    public float efficiency;

    public Differential(float _propshaftEfficiency, float _axleEfficiency, float _finalDriveEfficiency, float _finalDriveRatio)
    {
        propshaftEfficiency = _propshaftEfficiency;
        axleEfficiency = _axleEfficiency;
        finalDriveRatio = _finalDriveRatio;
        finalDriveEfficiency = _finalDriveEfficiency;
        efficiency = propshaftEfficiency * axleEfficiency * finalDriveEfficiency;
    }

    public int[][] Update(int[][] data)
    {
        float transferCaseTorque = data[Channel.Vehicle][VehicleData.TransferCaseTorque] / 1000f;

        float differentialTorque = transferCaseTorque * finalDriveRatio * efficiency;

        data[Channel.Vehicle][VehicleData.DifferentialTorqueFront] = (int)(differentialTorque * 1000f);
        data[Channel.Vehicle][VehicleData.DifferentialTorqueRear] = (int)(differentialTorque * 1000f);

        data[Channel.Vehicle][VehicleData.FrontAxleLeftTorque] = (int)(differentialTorque / 2 * 1000f);
        data[Channel.Vehicle][VehicleData.FrontAxleRightTorque] = (int)(differentialTorque / 2 * 1000f);
        data[Channel.Vehicle][VehicleData.RearAxleLeftTorque] = (int)(differentialTorque / 2 * 1000f);
        data[Channel.Vehicle][VehicleData.RearAxleRightTorque] = (int)(differentialTorque / 2 * 1000f);

        return data;
    }
}
