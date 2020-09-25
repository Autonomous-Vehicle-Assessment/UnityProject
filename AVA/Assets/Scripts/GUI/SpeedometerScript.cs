using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerScript : MonoBehaviour
{
    private GameObject canvas;
    private GameObject Speedometer;
    private GameObject EngineRPM;
    private Transform Needle;
    private Text SpeedField;
    private Transform NeedleRPM;
    private Text SpeedFieldRPM;
    private Text SpeedTypeField;

    private float SpeedZero = 187.5f;
    private const float DeltaAngle = (-92.8f - 187.5f) / 280f;
    private const float DeltaAngleRPM = (-92.8f - 187.5f) / 7000f;

    // Start is called before the first frame update
    void Awake()
    {
        canvas = transform.Find("Canvas").gameObject;
        Speedometer = canvas.transform.Find("SpeedometerGroup").gameObject;
        EngineRPM = canvas.transform.Find("EngineRPMGroup").gameObject;

        Needle = Speedometer.transform.Find("Needle");
        SpeedField = Speedometer.transform.Find("VelocityField").GetComponent<Text>();
        SpeedTypeField = Speedometer.transform.Find("SpeedTypeField").GetComponent<Text>();

        NeedleRPM = EngineRPM.transform.Find("Needle");
        SpeedFieldRPM = EngineRPM.transform.Find("RPMField").GetComponent<Text>();


        SpeedField.text = "km/h";
        canvas.gameObject.SetActive(true);
    }

    public void UpdateDisplay(float Speed, float RPM, string s_SpeedType)
    {
        // Velocity
        float targetAngle = SpeedZero + DeltaAngle * Speed;
        float AnimationAngle = Mathf.LerpAngle(Needle.localEulerAngles[2], targetAngle, 10f * Time.deltaTime);

        Needle.localEulerAngles = new Vector3(0, 0, AnimationAngle);
        SpeedField.text = string.Format("{0}", (int)Speed);

        SpeedTypeField.text = s_SpeedType;

        // RPM
        targetAngle = SpeedZero + DeltaAngleRPM * RPM;
        AnimationAngle = Mathf.LerpAngle(NeedleRPM.localEulerAngles[2], targetAngle, 10f * Time.deltaTime);

        NeedleRPM.localEulerAngles = new Vector3(0, 0, AnimationAngle);
        SpeedFieldRPM.text = string.Format("{0}", (int)RPM);
    }
}
