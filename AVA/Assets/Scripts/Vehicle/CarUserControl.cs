using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(Engine))]
    public class CarUserControl : MonoBehaviour
    {
        private Engine engine;               // Engine model
        private DataLogging dataLogger;


        [SerializeField] public TransferCase m_CurrentTransfercase;
        [SerializeField] public bool TrackPoints;
        [SerializeField] public int RenderTime = 1;
        private LineRenderer[] lineRenderers;
        private List<Vector3[]> points;

        private void Awake()
        {
            // get the controller
            engine = GetComponent<Engine>();

            // DataLogger
            dataLogger = GetComponent<DataLogging>();
        }


        private void LateUpdate()
        {
            // pass the input to the car!
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            float footbrake = Input.GetAxis("FootBrake");
            float handbrake = Input.GetAxis("HandBrake");

            if (Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                if (m_CurrentTransfercase == TransferCase.High)
                {
                    m_CurrentTransfercase = TransferCase.Low;
                }
                else
                {
                    m_CurrentTransfercase = TransferCase.High;
                }
            }
            
            engine.m_CurrentTransferCase = m_CurrentTransfercase;

            engine.UpdateState();

            if (engine != null)
            {
                engine.Move(h, v, footbrake, handbrake);
            }

            string s_Time = Time.time.ToString();
            string s_Velocity = engine.m_Speed.ToString();
            string s_EngineRPM = engine.m_EngineRPM.ToString();
            string s_EngineTorque = engine.m_EngineTorque.ToString();

            float m_TransmissionTorque = 0;
            float m_WheelForce = 0;

            for (int i = 0; i < engine.m_Wheel.Count; i++)
            {
                m_TransmissionTorque += engine.m_Wheel[i].m_collider.motorTorque;
                m_WheelForce += engine.m_Wheel[i].m_collider.motorTorque * engine.m_Wheel[i].m_collider.radius;
            }

            string s_WheelForce = m_WheelForce.ToString();
            string s_TransmissionTorque = m_TransmissionTorque.ToString();

            string s_CurrentGear = (engine.m_CurrentGear + 1).ToString();

            // Log data
            dataLogger.WriteToFile(s_Time + ";" + s_Velocity + ";" + s_WheelForce + ";" + s_TransmissionTorque + ";" + s_CurrentGear + ";" + s_EngineRPM + ";" + s_EngineTorque + "\n");


            // Draw line at wheels
            if (lineRenderers == null || points == null)
            {
                Color[] ColorArray = { new Color(1, 0, 0), new Color(1, 0, 0), new Color(0, 0, 1), new Color(0, 0, 1) };
                lineRenderers = new LineRenderer[4];
                points = new List<Vector3[]>();
                for (int i = 0; i < 4; i++)
                {
                    lineRenderers[i] = engine.m_Wheel[i].m_collider.gameObject.AddComponent<LineRenderer>();
                    lineRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderers[i].material.color = ColorArray[i];
                    lineRenderers[i].widthMultiplier = 0.02f;
                    lineRenderers[i].positionCount = (int)(RenderTime / Time.fixedDeltaTime);

                    points.Add(new Vector3[(int)(RenderTime / Time.fixedDeltaTime)]);
                    for (int k = 0; k < points[i].Length; k++)
                    {
                        points[i][k] = engine.m_Wheel[i].mesh.transform.position;
                    }
                    lineRenderers[i].SetPositions(points[i]);
                }

            }
            for (int i = 0; i < 4; i++)
            {
                for (int k = 0; k < points[i].Length - 1; k++)
                {
                    points[i][k] = points[i][k + 1];
                }
                points[i][points[i].Length - 1] = engine.m_Wheel[i].mesh.transform.position;
                lineRenderers[i].SetPositions(points[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                lineRenderers[i].enabled = TrackPoints;
            }
        }
    }
}