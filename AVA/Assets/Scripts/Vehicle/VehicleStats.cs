using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UnityStandardAssets.Vehicles.Car
{
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

            Engine.m_Speed = VehicleRigidBody.velocity.magnitude * m_SpeedCoefficient;

            graph.UpdateGraph(Engine.m_Speed,Engine.m_EngineRPM,Engine.m_CurrentGear + 1);
            Speedometer.UpdateDisplay(Engine.m_Speed, Engine.m_EngineRPM, s_SpeedType);

            GearField.text = string.Format("{0}{1}", Engine.m_CurrentGear+1,GenericFunctions.ToOrdinal(Engine.m_CurrentGear + 1));            
        }
    }

}