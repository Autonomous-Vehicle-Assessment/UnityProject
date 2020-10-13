using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Engine))]
public class EngineEditor : Editor
{
    private bool e_EngineStats = true;
    private bool e_EngineFoldout;
    private bool e_MotorCurveFoldout;
    private bool e_WheelsFoldout;
    private bool e_GearFoldout;
    private bool e_GearRatioFoldout;


    public override void OnInspectorGUI()
    {
        /*
        // ----- Initialization ----- //
        //base.OnInspectorGUI();                        // Default inspector
        Engine engine = (Engine)target;       // Target object (Engine)

        
        // ----- Stats ----- //
        e_EngineStats = EditorGUILayout.BeginFoldoutHeaderGroup(e_EngineStats, "Engine Stats");

        if (e_EngineStats)
        {
            engine.m_EngineRPM = EditorGUILayout.Slider("Engine RPM", engine.m_EngineRPM, engine.m_MinRpm, engine.m_MaxRpm);
            engine.m_EngineTorque = EditorGUILayout.Slider("Engine Torque", engine.m_EngineTorque, 0f, (float)engine.e_TorqueCurveValues.Max());
        }


        EditorGUILayout.EndFoldoutHeaderGroup();

        // ----- Engine Data ----- //
        e_EngineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(e_EngineFoldout, "Engine Settings");

        if (e_EngineFoldout)
        {
            engine.m_MaxTorqueRpm = EditorGUILayout.IntSlider(new GUIContent("Max Torque RPM", "Engine speed at maximum torque - rounds per minute [rpm]"), engine.m_MaxTorqueRpm, 0, 10000);
            engine.m_MaxPowerRpm = EditorGUILayout.IntSlider(new GUIContent("Max Power RPM", "Engine speed at maximum power - rounds per minute [rpm]"), engine.m_MaxPowerRpm, 0, 10000);

            GUILayout.Space(10);

            engine.m_MinRpm = EditorGUILayout.IntSlider(new GUIContent("Min RPM", "Minimum engine speed"), engine.m_MinRpm, 0, 3000);
            engine.m_MaxRpm = EditorGUILayout.IntSlider(new GUIContent("Max RPM", "Maximum engine speed"), engine.m_MaxRpm, engine.m_MinRpm, 10000);

            GUILayout.Space(20);

            // ----- Motor Curve ----- //
            engine.m_MotorCurve = EditorGUILayout.CurveField("Motor Curve", engine.m_MotorCurve, GUILayout.Height(100));
            int len = engine.m_MotorCurve.length;

            // Clear Curve
            for (int i = 0; i < len; i++)
            {
                engine.m_MotorCurve.RemoveKey(len - i - 1);
            }

            // Plot Curve
            Keyframe[] ks = new Keyframe[engine.e_SpeedCurveValues.Count];
            for (int i = 0; i < engine.e_SpeedCurveValues.Count; i++)
            {
                ks[i] = new Keyframe(engine.e_SpeedCurveValues[i], engine.e_TorqueCurveValues[i]);
            }

            engine.m_MotorCurve.keys = ks;

            for (int i = 1; i < engine.e_SpeedCurveValues.Count - 1; i++)
            {
                if (engine.e_TorqueCurveValues[i] != engine.e_TorqueCurveValues[i + 1] && engine.e_TorqueCurveValues[i] != engine.e_TorqueCurveValues[i - 1])
                {
                    engine.m_MotorCurve.SmoothTangents(i, 1);
                }
            }
            engine.m_MotorCurve.SmoothTangents(0, 1);
            engine.m_MotorCurve.SmoothTangents(engine.e_SpeedCurveValues.Count - 1, 1);

            GUILayout.Space(10);

            // ----- Motor Cuve Points ----- //
            e_MotorCurveFoldout = EditorGUILayout.Foldout(e_MotorCurveFoldout, "Curve Points", true);

            if (e_MotorCurveFoldout)
            {
                engine.e_CurvePoints = EditorGUILayout.DelayedIntField("Points", engine.e_CurvePoints);
                engine.e_CurvePoints = Mathf.Max(0, engine.e_CurvePoints);

                // Increase array size
                if (engine.e_CurvePoints > engine.e_SpeedCurveValues.Count)
                {
                    len = engine.e_SpeedCurveValues.Count;
                    for (int k = 0; k < engine.e_CurvePoints - len; k++)
                    {
                        engine.e_SpeedCurveValues.Add(0);
                        engine.e_TorqueCurveValues.Add(0);
                    }
                }

                // Reduce array size
                if (engine.e_CurvePoints < engine.e_SpeedCurveValues.Count)
                {
                    for (int k = engine.e_SpeedCurveValues.Count - 1; k + 1 > engine.e_CurvePoints; k--)
                    {
                        engine.e_SpeedCurveValues.RemoveAt(k);
                        engine.e_TorqueCurveValues.RemoveAt(k);
                    }
                }

                // Input fields labels
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Speed [rpm]", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Torque [Nm]", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                // Input fields
                for (int i = 0; i < engine.e_CurvePoints; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    engine.e_SpeedCurveValues[i] = EditorGUILayout.IntField(engine.e_SpeedCurveValues[i]);
                    engine.e_TorqueCurveValues[i] = EditorGUILayout.IntField(engine.e_TorqueCurveValues[i]);
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(30);
            }

        }
        EditorGUILayout.EndFoldoutHeaderGroup();


        // ----- Gear Data ----- //
        e_GearFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(e_GearFoldout, "Gears");
        if (e_GearFoldout)
        {
            // Gear ratios
            e_GearRatioFoldout = EditorGUILayout.Foldout(e_GearRatioFoldout, "Gear Ratio's", true);
            if (e_GearRatioFoldout)
            {
                // Number of gears
                engine.m_NumberOfGears = EditorGUILayout.IntSlider("Number of Gears", engine.m_NumberOfGears, 1, 10);

                GUILayout.Space(10);

                // Gear Ratio's
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Gear", GUILayout.Width(120));
                EditorGUILayout.LabelField("Gear ratio", GUILayout.Width(150));
                EditorGUILayout.LabelField("Efficiency", GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < engine.m_NumberOfGears; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("{0}{1}", i + 1, GenericFunctions.ToOrdinal(i + 1)),GUILayout.Width(120));
                    engine.m_GearRatio[i] = EditorGUILayout.Slider(engine.m_GearRatio[i], 0, 10,GUILayout.Width(150));
                    engine.m_GearEff[i] = EditorGUILayout.Slider(engine.m_GearEff[i], 0, 1, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }

                // Reverse Gear Ratio
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Reverse", GUILayout.Width(120));
                engine.m_ReverseGearRatio = EditorGUILayout.Slider(engine.m_ReverseGearRatio, -5, 0, GUILayout.Width(150));
                engine.m_ReverseGearEff = EditorGUILayout.Slider(engine.m_ReverseGearEff, 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                // Final drive ratio (i_0)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Final drive (i\u2080)", GUILayout.Width(120));
                engine.m_FinalDriveRatio = EditorGUILayout.Slider(engine.m_FinalDriveRatio, 0, 10, GUILayout.Width(150));
                engine.m_FinalDriveEff = EditorGUILayout.Slider(engine.m_FinalDriveEff, 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Transfer Case (Low)", GUILayout.Width(120));
                engine.m_TransferCaseRatio[0] = EditorGUILayout.Slider(engine.m_TransferCaseRatio[0], 0, 10, GUILayout.Width(150));
                engine.m_TransferCaseEff[0] = EditorGUILayout.Slider(engine.m_TransferCaseEff[0], 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Transfer Case (High)", GUILayout.Width(120));
                engine.m_TransferCaseRatio[1] = EditorGUILayout.Slider(engine.m_TransferCaseRatio[1], 0, 10, GUILayout.Width(150));
                engine.m_TransferCaseEff[1] = EditorGUILayout.Slider(engine.m_TransferCaseEff[1], 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);
            }

            GUILayout.Space(20);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();



        // ----- Wheels ----- //
        e_WheelsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(e_WheelsFoldout, "Wheels");
        if (e_WheelsFoldout)
        {
            engine.NumberofWheels = EditorGUILayout.DelayedIntField("Number of wheels", engine.NumberofWheels);
            engine.NumberofWheels = Mathf.Abs(engine.NumberofWheels);

            GUILayout.Space(10);
            if (engine.m_Wheel == null)
            {
                engine.m_Wheel = new List<StandardWheel>();
            }
            if (engine.NumberofWheels > engine.m_Wheel.Count)
            {
                int ListSize = engine.m_Wheel.Count;
                for (int i = 0; i < engine.NumberofWheels - ListSize; i++)
                {
                    engine.m_Wheel.Add(new StandardWheel());
                }
            }
            if(engine.NumberofWheels < engine.m_Wheel.Count)
            {
                int ListSize = engine.m_Wheel.Count;
                for (int i = 0; i < ListSize - engine.NumberofWheels; i++)
                {
                    engine.m_Wheel.RemoveAt(engine.m_Wheel.Count - i - 1);
                }
            }


            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mesh", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Collider", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Steer", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Drive", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("SBrake", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("HBrake", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Side", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < engine.NumberofWheels; i++)
            {
                EditorGUILayout.BeginHorizontal();
                engine.m_Wheel[i].mesh = (GameObject)EditorGUILayout.ObjectField(engine.m_Wheel[i].mesh, typeof(GameObject), true, GUILayout.Width(80));
                engine.m_Wheel[i].m_collider = (WheelCollider)EditorGUILayout.ObjectField(engine.m_Wheel[i].m_collider, typeof(WheelCollider), true, GUILayout.Width(80));
                engine.m_Wheel[i].steering = EditorGUILayout.Toggle(engine.m_Wheel[i].steering, GUILayout.Width(50));
                engine.m_Wheel[i].drive = EditorGUILayout.Toggle(engine.m_Wheel[i].drive, GUILayout.Width(50));
                engine.m_Wheel[i].serviceBrake = EditorGUILayout.Toggle(engine.m_Wheel[i].serviceBrake, GUILayout.Width(50));
                engine.m_Wheel[i].handBrake = EditorGUILayout.Toggle(engine.m_Wheel[i].handBrake, GUILayout.Width(50));
                if (engine.m_Wheel[i].steering)
                {
                    engine.m_Wheel[i].wheelSide = (WheelSide)EditorGUILayout.EnumPopup(engine.m_Wheel[i].wheelSide, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(20);

            engine.m_MaximumInnerSteerAngle = EditorGUILayout.IntSlider("Maximum inside turn angle", (int)engine.m_MaximumInnerSteerAngle, 0, 90);
            engine.m_MaximumOuterSteerAngle = EditorGUILayout.IntSlider("Maximum outside turn angle", (int)engine.m_MaximumOuterSteerAngle, 0, 90);

            GUILayout.Space(20);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // ----- Anti Sway Bar ----- //
        engine.swayBarActive = EditorGUILayout.Toggle("Anti-Sway Bar: ", engine.swayBarActive);
        engine.AntiRoll = EditorGUILayout.FloatField("Anti Roll Value: ", engine.AntiRoll);

        engine.m_CenterofMass = (GameObject)EditorGUILayout.ObjectField("Center of Mass: ", engine.m_CenterofMass, typeof(GameObject), true);


        if (!UnityEditor.EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(engine);
            EditorSceneManager.MarkSceneDirty(engine.gameObject.scene);
        }
        */
    }
}

