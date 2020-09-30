
using UnityEngine;

public class Noise_Test : MonoBehaviour
{
    public float prev_val;
    // Start is called before the first frame update
    void Start()
    {
        float prev_val = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        float val = GenericFunctions.GetGaussian(0.1f);
        
        if (Mathf.Abs(val) > Mathf.Abs(prev_val))
        {
            Debug.Log(val);
            prev_val = val;
        }
    }
}
