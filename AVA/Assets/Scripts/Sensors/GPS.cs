using UnityEngine;

public class GPS
{
    // Variables of GPS class //
    private Vector3     coordinates;    // Coordinate vector of position
    private float       accuracy;       // Accuracy constant in all directions
    private Transform   transform;      // Transform of body
    private float       time_real;      // Time variable

    // private int[,] signalMap;        // signalMap should describe how many satelites is expected to be available at a given location in the current level.
    // private float sateliteStrength;  // sateliteStrength should list the expected signal strength from the satelites resulting in an accuracy.
    // private float updateFrequency;   // Currently handeled in the SensorController due to the MonoBehaviour of coroutines for timed execution.
    // private Quaternion orientation;  // Could add orientation based on previous position(s).
    // private Vector3 velocity;        // Could add velocity based on previous position(s).

    public GPS(Transform _transform)
    {
        coordinates = new Vector3();    // Initialization of coordinate vector.
        accuracy = 0.2f;                // Should to be dependant on signalMap/sateliteStrength parameters.
        transform = _transform;         // Gets transform for determining the position.
        time_real = Time.time;          // Not currently used.
    }

    public Vector3 GetGPSData()
    {
        Vector3 coordinate = UpdateState();
        return coordinate;           // Returns the updated GPS data
    }

    private Vector3 UpdateState()
    {
        Vector3 coordinates = transform.position + new Vector3(GetNoise(), GetNoise(), GetNoise()); // Updates the position in xyz and adds noise.
        float time_real = Time.time;  // Updates current time from start of program. Not yet returned for further use.
        return coordinates;
    }

    private float GetNoise()
    {
        float noise = Random.Range(-accuracy / 2f, accuracy / 2f);  // Adds RANDOM noise to a signal within given limits.
        // the normal of the noise can be bigger due to same in all directions. Fix this.
        // Add noise distribution, gaussian or similar.
        return noise;
    }
    public string GetGPSString()
    {
        // Method for appending GPS data to realistic string format. Not used as it would make using the data more complicated.
        string data = "X: " + coordinates.x.ToString() + "m Y: " + coordinates.y.ToString() + "m Z: " + coordinates.z.ToString() + "m Time: " + time_real.ToString() + "s";
        return data;
    }
}


