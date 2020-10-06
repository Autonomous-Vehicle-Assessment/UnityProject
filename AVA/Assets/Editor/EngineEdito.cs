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
        // ----- Initialization ----- //
        //base.OnInspectorGUI();                        // Default inspector
        EngineModel Engine = (EngineModel)target;       // Target object (Engine)

        
        // ----- Stats ----- //
        e_EngineStats = EditorGUILayout.BeginFoldoutHeaderGroup(e_EngineStats, "Engine Stats");

        if (e_EngineStats)
        {
            Engine.m_EngineRPM = EditorGUILayout.Slider("Engine RPM", Engine.m_EngineRPM, Engine.m_MinRpm, Engine.m_MaxRpm);
            Engine.m_EngineTorque = EditorGUILayout.Slider("Engine Torque", Engine.m_EngineTorque, 0f, (float)Engine.e_TorqueCurveValues.Max());
        }


        EditorGUILayout.EndFoldoutHeaderGroup();

        // ----- Engine Data ----- //
        e_EngineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(e_EngineFoldout, "Engine Settings");

        if (e_EngineFoldout)
        {
            Engine.m_MaxTorqueRpm = EditorGUILayout.IntSlider(new GUIContent("Max Torque RPM", "Engine speed at maximum torque - rounds per minute [rpm]"), Engine.m_MaxTorqueRpm, 0, 10000);
            Engine.m_MaxPowerRpm = EditorGUILayout.IntSlider(new GUIContent("Max Power RPM", "Engine speed at maximum power - rounds per minute [rpm]"), Engine.m_MaxPowerRpm, 0, 10000);

            GUILayout.Space(10);

            Engine.m_MinRpm = EditorGUILayout.IntSlider(new GUIContent("Min RPM", "Minimum engine speed"), Engine.m_MinRpm, 0, 3000);
            Engine.m_MaxRpm = EditorGUILayout.IntSlider(new GUIContent("Max RPM", "Maximum engine speed"), Engine.m_MaxRpm, Engine.m_MinRpm, 10000);

            GUILayout.Space(20);

            // ----- Motor Curve ----- //
            Engine.m_MotorCurve = EditorGUILayout.CurveField("Motor Curve", Engine.m_MotorCurve, GUILayout.Height(100));
            int len = Engine.m_MotorCurve.length;

            // Clear Curve
            for (int i = 0; i < len; i++)
            {
                Engine.m_MotorCurve.RemoveKey(len - i - 1);
            }

            // Plot Curve
            Keyframe[] ks = new Keyframe[Engine.e_SpeedCurveValues.Count];
            for (int i = 0; i < Engine.e_SpeedCurveValues.Count; i++)
            {
                ks[i] = new Keyframe(Engine.e_SpeedCurveValues[i], Engine.e_TorqueCurveValues[i]);
            }

            Engine.m_MotorCurve.keys = ks;

            for (int i = 1; i < Engine.e_SpeedCurveValues.Count - 1; i++)
            {
                if (Engine.e_TorqueCurveValues[i] != Engine.e_TorqueCurveValues[i + 1] && Engine.e_TorqueCurveValues[i] != Engine.e_TorqueCurveValues[i - 1])
                {
                    Engine.m_MotorCurve.SmoothTangents(i, 1);
                }
            }
            Engine.m_MotorCurve.SmoothTangents(0, 1);
            Engine.m_MotorCurve.SmoothTangents(Engine.e_SpeedCurveValues.Count - 1, 1);

            GUILayout.Space(10);

            // ----- Motor Cuve Points ----- //
            e_MotorCurveFoldout = EditorGUILayout.Foldout(e_MotorCurveFoldout, "Curve Points", true);

            if (e_MotorCurveFoldout)
            {
                Engine.e_CurvePoints = EditorGUILayout.DelayedIntField("Points", Engine.e_CurvePoints);
                Engine.e_CurvePoints = Mathf.Max(0, Engine.e_CurvePoints);

                // Increase array size
                if (Engine.e_CurvePoints > Engine.e_SpeedCurveValues.Count)
                {
                    len = Engine.e_SpeedCurveValues.Count;
                    for (int k = 0; k < Engine.e_CurvePoints - len; k++)
                    {
                        Engine.e_SpeedCurveValues.Add(0);
                        Engine.e_TorqueCurveValues.Add(0);
                    }
                }

                // Reduce array size
                if (Engine.e_CurvePoints < Engine.e_SpeedCurveValues.Count)
                {
                    for (int k = Engine.e_SpeedCurveValues.Count - 1; k + 1 > Engine.e_CurvePoints; k--)
                    {
                        Engine.e_SpeedCurveValues.RemoveAt(k);
                        Engine.e_TorqueCurveValues.RemoveAt(k);
                    }
                }

                // Input fields labels
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Speed [rpm]", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Torque [Nm]", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                // Input fields
                for (int i = 0; i < Engine.e_CurvePoints; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Engine.e_SpeedCurveValues[i] = EditorGUILayout.IntField(Engine.e_SpeedCurveValues[i]);
                    Engine.e_TorqueCurveValues[i] = EditorGUILayout.IntField(Engine.e_TorqueCurveValues[i]);
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
                Engine.m_NumberOfGears = EditorGUILayout.IntSlider("Number of Gears", Engine.m_NumberOfGears, 1, 10);

                GUILayout.Space(10);

                // Gear Ratio's
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Gear", GUILayout.Width(120));
                EditorGUILayout.LabelField("Gear ratio", GUILayout.Width(150));
                EditorGUILayout.LabelField("Efficiency", GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < Engine.m_NumberOfGears; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("{0}{1}", i + 1, GenericFunctions.ToOrdinal(i + 1)),GUILayout.Width(120));
                    Engine.m_GearRatio[i] = EditorGUILayout.Slider(Engine.m_GearRatio[i], 0, 10,GUILayout.Width(150));
                    Engine.m_GearEff[i] = EditorGUILayout.Slider(Engine.m_GearEff[i], 0, 1, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }

                // Reverse Gear Ratio
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Reverse", GUILayout.Width(120));
                Engine.m_ReverseGearRatio = EditorGUILayout.Slider(Engine.m_ReverseGearRatio, -5, 0, GUILayout.Width(150));
                Engine.m_ReverseGearEff = EditorGUILayout.Slider(Engine.m_ReverseGearEff, 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                // Final drive ratio (i_0)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Final drive (i\u2080)", GUILayout.Width(120));
                Engine.m_FinalDriveRatio = EditorGUILayout.Slider(Engine.m_FinalDriveRatio, 0, 10, GUILayout.Width(150));
                Engine.m_FinalDriveEff = EditorGUILayout.Slider(Engine.m_FinalDriveEff, 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Transfer Case (Low)", GUILayout.Width(120));
                Engine.m_TransferCaseRatio[0] = EditorGUILayout.Slider(Engine.m_TransferCaseRatio[0], 0, 10);
                Engine.m_TransferCaseEff[0] = EditorGUILayout.Slider(Engine.m_TransferCaseEff[0], 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Transfer Case (High)", GUILayout.Width(120));
                Engine.m_TransferCaseRatio[1] = EditorGUILayout.Slider(Engine.m_TransferCaseRatio[1], 0, 10);
                Engine.m_TransferCaseEff[1] = EditorGUILayout.Slider(Engine.m_TransferCaseEff[1], 0, 1, GUILayout.Width(150));
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
            Engine.NumberofWheels = EditorGUILayout.DelayedIntField("Number of wheels", Engine.NumberofWheels);
            Engine.NumberofWheels = Mathf.Abs(Engine.NumberofWheels);

            GUILayout.Space(10);
            if (Engine.m_Wheel == null)
            {
                Engine.m_Wheel = new List<StandardWheel>();
            }
            if (Engine.NumberofWheels > Engine.m_Wheel.Count)
            {
                int ListSize = Engine.m_Wheel.Count;
                for (int i = 0; i < Engine.NumberofWheels - ListSize; i++)
                {
                    Engine.m_Wheel.Add(new StandardWheel());
                }
            }
            if(Engine.NumberofWheels < Engine.m_Wheel.Count)
            {
                int ListSize = Engine.m_Wheel.Count;
                for (int i = 0; i < ListSize - Engine.NumberofWheels; i++)
                {
                    Engine.m_Wheel.RemoveAt(Engine.m_Wheel.Count - i - 1);
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

            for (int i = 0; i < Engine.NumberofWheels; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Engine.m_Wheel[i].mesh = (GameObject)EditorGUILayout.ObjectField(Engine.m_Wheel[i].mesh, typeof(GameObject), true, GUILayout.Width(80));
                Engine.m_Wheel[i].m_collider = (WheelCollider)EditorGUILayout.ObjectField(Engine.m_Wheel[i].m_collider, typeof(WheelCollider), true, GUILayout.Width(80));
                Engine.m_Wheel[i].steering = EditorGUILayout.Toggle(Engine.m_Wheel[i].steering, GUILayout.Width(50));
                Engine.m_Wheel[i].drive = EditorGUILayout.Toggle(Engine.m_Wheel[i].drive, GUILayout.Width(50));
                Engine.m_Wheel[i].serviceBrake = EditorGUILayout.Toggle(Engine.m_Wheel[i].serviceBrake, GUILayout.Width(50));
                Engine.m_Wheel[i].handBrake = EditorGUILayout.Toggle(Engine.m_Wheel[i].handBrake, GUILayout.Width(50));
                if (Engine.m_Wheel[i].steering)
                {
                    Engine.m_Wheel[i].wheelSide = (WheelSide)EditorGUILayout.EnumPopup(Engine.m_Wheel[i].wheelSide, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(20);

            Engine.m_MaximumInnerSteerAngle = EditorGUILayout.IntSlider("Maximum inside turn angle", (int)Engine.m_MaximumInnerSteerAngle, 0, 90);
            Engine.m_MaximumOuterSteerAngle = EditorGUILayout.IntSlider("Maximum outside turn angle", (int)Engine.m_MaximumOuterSteerAngle, 0, 90);

            GUILayout.Space(20);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // ----- Anti Sway Bar ----- //
        Engine.swayBarActive = EditorGUILayout.Toggle("Anti-Sway Bar: ", Engine.swayBarActive);
        Engine.AntiRoll = EditorGUILayout.FloatField("Anti Roll Value: ", Engine.AntiRoll);

        Engine.m_CenterofMass = (GameObject)EditorGUILayout.ObjectField("Center of Mass: ", Engine.m_CenterofMass, typeof(GameObject), true);


        if (!UnityEditor.EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(Engine);
            EditorSceneManager.MarkSceneDirty(Engine.gameObject.scene);
        }
    }

}

