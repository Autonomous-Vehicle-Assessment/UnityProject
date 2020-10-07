using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    private Vehicle vehicle;
    // Start is called before the first frame update
    void Start()
    {
        vehicle = new Vehicle
        {
            data = new int[2, 2] {
                { 1, 1 }, 
                { 1, 1 } 
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
