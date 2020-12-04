using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(EngineModel))]
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
            Engine.engineRPM = EditorGUILayout.Slider("Engine RPM", Engine.engineRPM, Engine.minRpm, Engine.maxRpm);
            Engine.engineTorque = EditorGUILayout.Slider("Engine Torque", Engine.engineTorque, 0f, (float)Engine.torqueCurveValues.Max());
        }


        EditorGUILayout.EndFoldoutHeaderGroup();

        // ----- Engine Data ----- //
        e_EngineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(e_EngineFoldout, "Engine Settings");

        if (e_EngineFoldout)
        {
            Engine.maxTorqueRpm = EditorGUILayout.IntSlider(new GUIContent("Max Torque RPM", "Engine speed at maximum torque - rounds per minute [rpm]"), Engine.maxTorqueRpm, 0, 10000);
            Engine.maxPowerRpm = EditorGUILayout.IntSlider(new GUIContent("Max Power RPM", "Engine speed at maximum power - rounds per minute [rpm]"), Engine.maxPowerRpm, 0, 10000);

            GUILayout.Space(10);

            Engine.minRpm = EditorGUILayout.IntSlider(new GUIContent("Min RPM", "Minimum engine speed"), Engine.minRpm, 0, 3000);
            Engine.maxRpm = EditorGUILayout.IntSlider(new GUIContent("Max RPM", "Maximum engine speed"), Engine.maxRpm, Engine.minRpm, 10000);

            GUILayout.Space(20);

            // ----- Motor Curve ----- //
            Engine.motorCurve = EditorGUILayout.CurveField("Motor Curve", Engine.motorCurve, GUILayout.Height(100));
            int len = Engine.motorCurve.length;

            // Clear Curve
            for (int i = 0; i < len; i++)
            {
                Engine.motorCurve.RemoveKey(len - i - 1);
            }

            // Plot Curve
            Keyframe[] ks = new Keyframe[Engine.speedCurveValues.Count];
            for (int i = 0; i < Engine.speedCurveValues.Count; i++)
            {
                ks[i] = new Keyframe(Engine.speedCurveValues[i], Engine.torqueCurveValues[i]);
            }

            Engine.motorCurve.keys = ks;

            for (int i = 1; i < Engine.speedCurveValues.Count - 1; i++)
            {
                if (Engine.torqueCurveValues[i] != Engine.torqueCurveValues[i + 1] && Engine.torqueCurveValues[i] != Engine.torqueCurveValues[i - 1])
                {
                    Engine.motorCurve.SmoothTangents(i, 1);
                }
            }
            Engine.motorCurve.SmoothTangents(0, 1);
            Engine.motorCurve.SmoothTangents(Engine.speedCurveValues.Count - 1, 1);

            GUILayout.Space(10);

            // ----- Motor Cuve Points ----- //
            e_MotorCurveFoldout = EditorGUILayout.Foldout(e_MotorCurveFoldout, "Curve Points", true);

            if (e_MotorCurveFoldout)
            {
                Engine.curvePoints = EditorGUILayout.DelayedIntField("Points", Engine.curvePoints);
                Engine.curvePoints = Mathf.Max(0, Engine.curvePoints);

                // Increase array size
                if (Engine.curvePoints > Engine.speedCurveValues.Count)
                {
                    len = Engine.speedCurveValues.Count;
                    for (int k = 0; k < Engine.curvePoints - len; k++)
                    {
                        Engine.speedCurveValues.Add(0);
                        Engine.torqueCurveValues.Add(0);
                    }
                }

                // Reduce array size
                if (Engine.curvePoints < Engine.speedCurveValues.Count)
                {
                    for (int k = Engine.speedCurveValues.Count - 1; k + 1 > Engine.curvePoints; k--)
                    {
                        Engine.speedCurveValues.RemoveAt(k);
                        Engine.torqueCurveValues.RemoveAt(k);
                    }
                }

                // Input fields labels
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Speed [rpm]", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Torque [Nm]", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                // Input fields
                for (int i = 0; i < Engine.curvePoints; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Engine.speedCurveValues[i] = EditorGUILayout.IntField(Engine.speedCurveValues[i]);
                    Engine.torqueCurveValues[i] = EditorGUILayout.IntField(Engine.torqueCurveValues[i]);
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
                Engine.numberOfGears = EditorGUILayout.IntSlider("Number of Gears", Engine.numberOfGears, 1, 10);

                GUILayout.Space(10);

                // Gear Ratio's
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Gear", GUILayout.Width(120));
                EditorGUILayout.LabelField("Gear ratio", GUILayout.Width(150));
                EditorGUILayout.LabelField("Efficiency", GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < Engine.numberOfGears; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("{0}{1}", i + 1, GenericFunctions.ToOrdinal(i + 1)),GUILayout.Width(120));
                    Engine.gearRatio[i] = EditorGUILayout.Slider(Engine.gearRatio[i], 0, 10,GUILayout.Width(150));
                    Engine.gearEff[i] = EditorGUILayout.Slider(Engine.gearEff[i], 0, 1, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }

                // Reverse Gear Ratio
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Reverse", GUILayout.Width(120));
                Engine.reverseGearRatio = EditorGUILayout.Slider(Engine.reverseGearRatio, -5, 0, GUILayout.Width(150));
                Engine.reverseGearEff = EditorGUILayout.Slider(Engine.reverseGearEff, 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                // Final drive ratio (i_0)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Final drive (i\u2080)", GUILayout.Width(120));
                Engine.finalDriveRatio = EditorGUILayout.Slider(Engine.finalDriveRatio, 0, 10, GUILayout.Width(150));
                Engine.finalDriveEff = EditorGUILayout.Slider(Engine.finalDriveEff, 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Transfer Case (Low)", GUILayout.Width(120));
                Engine.transferCaseRatio[0] = EditorGUILayout.Slider(Engine.transferCaseRatio[0], 0, 10);
                Engine.transferCaseEff[0] = EditorGUILayout.Slider(Engine.transferCaseEff[0], 0, 1, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Transfer Case (High)", GUILayout.Width(120));
                Engine.transferCaseRatio[1] = EditorGUILayout.Slider(Engine.transferCaseRatio[1], 0, 10);
                Engine.transferCaseEff[1] = EditorGUILayout.Slider(Engine.transferCaseEff[1], 0, 1, GUILayout.Width(150));
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
            Engine.numberofWheels = EditorGUILayout.DelayedIntField("Number of wheels", Engine.numberofWheels);
            Engine.numberofWheels = Mathf.Abs(Engine.numberofWheels);

            GUILayout.Space(10);
            if (Engine.wheels == null)
            {
                Engine.wheels = new List<StandardWheel>();
            }
            if (Engine.numberofWheels > Engine.wheels.Count)
            {
                int ListSize = Engine.wheels.Count;
                for (int i = 0; i < Engine.numberofWheels - ListSize; i++)
                {
                    Engine.wheels.Add(new StandardWheel());
                }
            }
            if(Engine.numberofWheels < Engine.wheels.Count)
            {
                int ListSize = Engine.wheels.Count;
                for (int i = 0; i < ListSize - Engine.numberofWheels; i++)
                {
                    Engine.wheels.RemoveAt(Engine.wheels.Count - i - 1);
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

            for (int i = 0; i < Engine.numberofWheels; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Engine.wheels[i].mesh = (GameObject)EditorGUILayout.ObjectField(Engine.wheels[i].mesh, typeof(GameObject), true, GUILayout.Width(80));
                Engine.wheels[i].collider = (WheelCollider)EditorGUILayout.ObjectField(Engine.wheels[i].collider, typeof(WheelCollider), true, GUILayout.Width(80));
                Engine.wheels[i].steering = EditorGUILayout.Toggle(Engine.wheels[i].steering, GUILayout.Width(50));
                Engine.wheels[i].drive = EditorGUILayout.Toggle(Engine.wheels[i].drive, GUILayout.Width(50));
                Engine.wheels[i].serviceBrake = EditorGUILayout.Toggle(Engine.wheels[i].serviceBrake, GUILayout.Width(50));
                Engine.wheels[i].handBrake = EditorGUILayout.Toggle(Engine.wheels[i].handBrake, GUILayout.Width(50));
                if (Engine.wheels[i].steering)
                {
                    Engine.wheels[i].wheelSide = (WheelSide)EditorGUILayout.EnumPopup(Engine.wheels[i].wheelSide, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(20);

            Engine.maximumInnerSteerAngle = EditorGUILayout.IntSlider("Maximum inside turn angle", (int)Engine.maximumInnerSteerAngle, 0, 90);
            Engine.maximumOuterSteerAngle = EditorGUILayout.IntSlider("Maximum outside turn angle", (int)Engine.maximumOuterSteerAngle, 0, 90);

            GUILayout.Space(20);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // ----- Anti Sway Bar ----- //
        Engine.swayBarActive = EditorGUILayout.Toggle("Anti-Sway Bar: ", Engine.swayBarActive);
        Engine.antiRoll = EditorGUILayout.FloatField("Anti Roll Value: ", Engine.antiRoll);

        Engine.centerofMass = (GameObject)EditorGUILayout.ObjectField("Center of Mass: ", Engine.centerofMass, typeof(GameObject), true);


        if (!UnityEditor.EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(Engine);
            EditorSceneManager.MarkSceneDirty(Engine.gameObject.scene);
        }
    }

}

