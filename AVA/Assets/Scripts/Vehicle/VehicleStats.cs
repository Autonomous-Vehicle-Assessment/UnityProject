using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VehicleStats : MonoBehaviour
{
    private GameObject InterfaceObject;
    private SpeedometerScript Speedometer;
    private WindowGraph GraphObject;
    private Text GearField;
    private Rigidbody VehicleRigidBody;
    private EngineModel Engine;
    private Graph graph;

    public SpeedType m_SpeedType;
    private string s_SpeedType;
    private float m_SpeedCoefficient;
    private List<int> SpeedCurve;

    // Start is called before the first frame update
    private void Awake()
    {
        InterfaceObject = GameObject.Find("UI");
        Engine = GetComponent<EngineModel>();
        VehicleRigidBody = GetComponent<Rigidbody>();
        Speedometer = InterfaceObject.GetComponent<SpeedometerScript>();
        GraphObject = InterfaceObject.transform.Find("Canvas").gameObject.transform.Find("WindowGraph").GetComponent<WindowGraph>();
        graph = InterfaceObject.GetComponent<Graph>();
        GearField = InterfaceObject.transform.Find("Canvas").gameObject.transform.Find("Gear").GetComponent<Text>();

        (s_SpeedType, m_SpeedCoefficient) = GenericFunctions.SpeedTypeConverter(m_SpeedType);

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (m_SpeedType)
        {
            case SpeedType.MPH:
                s_SpeedType = " MPH";
                m_SpeedCoefficient = 2.23693629f;
                break;
            case SpeedType.KPH:
                s_SpeedType = " km/h";
                m_SpeedCoefficient = 3.6f;
                break;
            case SpeedType.MPS:
                s_SpeedType = " m/s";
                m_SpeedCoefficient = 1.0f;
                break;
        }

        Engine.speed = VehicleRigidBody.velocity.magnitude * m_SpeedCoefficient * Mathf.Sign(transform.InverseTransformDirection(VehicleRigidBody.velocity).z);

        graph.UpdateGraph(Engine.speed,Engine.engineRPM,Engine.currentGear + 1);
        Speedometer.UpdateDisplay(Engine.speed, Engine.engineRPM, s_SpeedType);

        GearField.text = string.Format("{0}{1}", Engine.currentGear+1,GenericFunctions.ToOrdinal(Engine.currentGear + 1));            
    }
}