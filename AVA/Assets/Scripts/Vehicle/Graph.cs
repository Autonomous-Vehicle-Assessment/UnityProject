using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;

/// <summary>
/// Draws a basic oscilloscope type graph in a GUI.Window()
/// Michael Hutton May 2020
/// This is just a basic 'as is' do as you wish...
/// Let me know if you use it as I'd be interested if people find it useful.
/// I'm going to keep experimenting wih the GL calls...eg GL.LINES etc 
/// </summary>
public class Graph : MonoBehaviour
{
    Material mat;
    private Rect windowRect;

    // A list of random values to draw
    private List<float>[] values;
    public TextAnchor textAnchor;
    public int labelWidth;
    public int labelHeight;
    public int labelX;
    public int labelY;

    public int FontSize;

    // List of Windows
    private bool showWindow0 = false;

    private Color[] colorList = new Color[] { Color.green, Color.red, Color.blue, Color.magenta, Color.cyan };

    private GUIStyle myGUIStyle;
    private GUIStyle rpmGUIStyle;
    private GUIStyle speedGUIStyle;
    private GUIStyle gearGUIStyle;
    private GUIStyle speedLabelGUIStyle;
    private GUIStyle rpmLabelGUIStyle;
    private GUIStyle gearLabelGUIStyle;
    private GUIStyle vVelocityGUIStyle;
    private GUIStyle vVelocityLabelGUIStyle;

    
    private bool initDone = false;

    // Start is called before the first frame update
    private void Start()
    {
        mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        // Should check for material but I'll leave that to you..

        // Initialize list
        values = new List<float>[4];

        for (int i = 0; i < 4; i++)
        {
            values[i] = new List<float>(1);
        }
    }

    // Update is called once per frame
    public void UpdateGraph(float Speed, float RPM, int CurrentGear, float VerticalVelocity)
    {
        // Keep adding values
        values[0].Add(Speed);
        values[1].Add(RPM);
        values[2].Add(CurrentGear);
        values[3].Add(VerticalVelocity);

    }

    private void Update()
    {
        // Create a GUI.toggle to show graph window
        //showWindow0 = GUI.Toggle(new Rect(10, 10, 100, 20), showWindow0, "Show Graph");
        if (Input.GetButtonUp("Infotab"))
        {
            showWindow0 = !showWindow0;
        }
    }

    private void OnGUI()
    {
        if (!initDone)
        {
            myGUIStyle = new GUIStyle(GUI.skin.label);
            myGUIStyle.alignment = textAnchor;

            // rpm Style
            rpmGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 11
            };

            rpmGUIStyle.normal.textColor = Color.red;

            rpmLabelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11
            };

            rpmLabelGUIStyle.normal.textColor = Color.red;

            // speed Style
            speedGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 11
            };

            speedGUIStyle.normal.textColor = Color.green;


            // speed Style
            speedLabelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11
            };

            speedLabelGUIStyle.normal.textColor = Color.green;

            // gear Style
            gearGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 11
            };

            gearGUIStyle.normal.textColor = Color.blue;

            gearLabelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11
            };

            gearLabelGUIStyle.normal.textColor = Color.blue;

            // Vertical Velocity
            vVelocityGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 11
            };

            vVelocityGUIStyle.normal.textColor = colorList[3];

            vVelocityLabelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11
            };

            vVelocityLabelGUIStyle.normal.textColor = colorList[3];


            

            // Graph window
            windowRect = new Rect(Screen.width - 20 - 512, 20, 512, 300);
        }


        if (showWindow0)
        {
            // Set out drawValue list equal to the values list
            //drawValues = values;
            windowRect = GUI.Window(0, windowRect, DrawGraph, "Vehicle State");
        }
    }


    void DrawGraph(int windowID)
    {       
        // Draw the graph in the repaint cycle
        if (Event.current.type == EventType.Repaint)
        {
            GL.PushMatrix();

            GL.Clear(true, false, Color.black);
            mat.SetPass(0);

            // Draw a black back ground Quad 
            GL.Begin(GL.QUADS);
            GL.Color(new Color(0,0,0,0.5f));
            GL.Vertex3(2, 15, 0);
            GL.Vertex3(windowRect.width - 2, 15, 0);
            GL.Vertex3(windowRect.width - 2, windowRect.height - 2, 0);
            GL.Vertex3(2, windowRect.height - 2, 0);
            GL.End();

            //for (int i = 0; i < values.Length; i++)
            //{
            // Draw the lines of the graph


            // Speed
            float maxScale = 30f / 100f;
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);

            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 - 2, 0);
            GL.Vertex3(50, windowRect.height / 5 - 2, 0);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 1 - 2 + 35 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 1 - 2 + 35 * maxScale + 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 1 - 2 + 35 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 1 - 2 - 110 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 1 - 2 - 110 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 1 - 2 - 110 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 1 - 2 - 110 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 1 - 2 + 35 * maxScale + 1, 0);
            GL.End();

            GL.Begin(GL.LINES);

            GL.Color(colorList[0]);

            int valueIndex = values[0].Count - 1;
            for (int k = (int)windowRect.width - 50; k > 50; k--)
            {
                float y1 = 0;
                float y2 = 0;

                if (valueIndex > 0)
                {
                    y2 = values[0][valueIndex] * maxScale;
                    y1 = values[0][valueIndex - 1] * maxScale;
                }
                GL.Vertex3(k, windowRect.height/5 - 2 - y2, 0);
                GL.Vertex3((k - 1), windowRect.height/5 - 2 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();


            // RPM
            maxScale = 30f / 2500f;
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 2 - 2 + 0 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 2 - 2 + 0 * maxScale + 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 2 - 2 + 0 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 2 - 2 - 2850 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 2 - 2 - 2850 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 2 - 2 - 2850 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 2 - 2 - 2850 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 2 - 2 + 0 * maxScale + 1, 0);
            GL.End();


            GL.Begin(GL.LINES);
            GL.Color(colorList[1]);

            valueIndex = values[1].Count - 1;
            for (int k = (int)windowRect.width - 50; k > 50; k--)
            {
                float y1 = 0;
                float y2 = 0;

                if (valueIndex > 0)
                {
                    y2 = values[1][valueIndex] * maxScale;
                    y1 = values[1][valueIndex - 1] * maxScale;
                }
                GL.Vertex3(k, windowRect.height / 5 * 2 - 2 - y2, 0);
                GL.Vertex3((k - 1), windowRect.height / 5 * 2 - 2 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();

            // Gear
            maxScale = 40f / 6f;
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 3 - 2 + 1 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 3 - 2 + 1 * maxScale + 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 3 - 2 + 1 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 3 - 2 - 6 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 3 - 2 - 6 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 3 - 2 - 6 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 3 - 2 - 6 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 3 - 2 + 1 * maxScale + 1, 0);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(colorList[2]);

            valueIndex = values[2].Count - 1;
            for (int k = (int)windowRect.width - 50; k > 50; k--)
            {
                float y1 = 0;
                float y2 = 0;

                if (valueIndex > 0)
                {
                    y2 = values[2][valueIndex] * maxScale;
                    y1 = values[2][valueIndex - 1] * maxScale;
                }
                GL.Vertex3(k, windowRect.height / 5 * 3 - 2 - y2, 0);
                GL.Vertex3((k - 1), windowRect.height / 5 * 3 - 2 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();


            // Vertical Velocity
            maxScale = 40f / 3f;

            GL.Begin(GL.LINES);
            GL.Color(Color.grey);

            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2, 0);
            GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2, 0);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2 + 3 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2 + 3 * maxScale + 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2 + 3 * maxScale + 1, 0);
            GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2 - 4 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2 - 4 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2 - 4 * maxScale - 1, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2 - 4 * maxScale - 1, 0);
            GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2 + 3 * maxScale + 1, 0);
            GL.End();

            for (int i = 0; i < 4; i++)
            {
                GL.Begin(GL.LINES);
                Color lineColor = Color.grey;
                lineColor.a = .5f;
                GL.Color(lineColor);

                GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2 - i * maxScale, 0);
                GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2 - i * maxScale, 0);
                GL.End();
            }

            for (int i = 0; i < 3; i++)
            {
                GL.Begin(GL.LINES);
                Color lineColor = Color.grey;
                lineColor.a = .5f;
                GL.Color(lineColor);

                GL.Vertex3(windowRect.width - 50, windowRect.height / 5 * 4.2f - 2 + i * maxScale, 0);
                GL.Vertex3(50, windowRect.height / 5 * 4.2f - 2 + i * maxScale, 0);
                GL.End();
            }

            GL.Begin(GL.LINES);
            GL.Color(colorList[3]);

            valueIndex = values[3].Count - 1;
            for (int k = (int)windowRect.width - 50; k > 50; k--)
            {
                float y1 = 0;
                float y2 = 0;

                if (valueIndex > 0)
                {
                    y2 = values[3][valueIndex] * maxScale;
                    y1 = values[3][valueIndex - 1] * maxScale;
                }
                GL.Vertex3(k, windowRect.height / 5 * 4.2f - 2 - y2, 0);
                GL.Vertex3((k - 1), windowRect.height / 5 * 4.2f - 2 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();


            //


            //GL.Color(colorList[i]);

            //valueIndex = values[i].Count - 1;
            //for (int k = (int)windowRect.width - 100; k > 3; k--)
            //{
            //    float y1 = 0;
            //    float y2 = 0;
            //    if (valueIndex > 0)
            //    {
            //        y2 = values[i][valueIndex];
            //        y1 = values[i][valueIndex - 1];
            //    }
            //    GL.Vertex3(k, windowRect.height - 2 - y2, 0);
            //    GL.Vertex3((k - 1), windowRect.height - 2 - y1, 0);
            //    valueIndex -= 1;
            //}
            // GL.End();
            //}
            GL.PopMatrix();
        }


        // Populate graph with text
        // Speed
        Rect speedRect = new Rect(windowRect.width - 83, windowRect.height / 5 - 10, 80, 50);
        Rect speedRectLabel = new Rect(5, windowRect.height / 5 - 10, 80, 50);
        string speedValue = ((int)(values[0][values[0].Count - 1])).ToString() + " km/hr";
        GUI.Label(speedRect, speedValue, speedGUIStyle);
        GUI.Label(speedRectLabel, "Speed", speedLabelGUIStyle);

        // RPM
        Rect rpmRect = new Rect(windowRect.width - 83, windowRect.height / 5 * 2 - 10, 80, 50);
        Rect rpmRectLabel = new Rect(5, windowRect.height / 5 * 2 - 10, 80, 50);
        string rpmValue = ((int)(values[1][values[1].Count - 1])).ToString() + " rpm";
        GUI.Label(rpmRect, rpmValue, rpmGUIStyle);
        GUI.Label(rpmRectLabel, "RPM", rpmLabelGUIStyle);

        // Gear
        Rect gearRect = new Rect(windowRect.width - 83, windowRect.height / 5 * 3 - 10, 80, 50);
        Rect gearRectLabel = new Rect(5, windowRect.height / 5 * 3 - 10, 80, 50);
        string gearValue = ((int)(values[2][values[2].Count - 1])).ToString() + GenericFunctions.ToOrdinal((int)(values[2][values[2].Count - 1]) ) + " gear";
        GUI.Label(gearRect, gearValue, gearGUIStyle);
        GUI.Label(gearRectLabel, "Gear", gearLabelGUIStyle);

        // Vertical Velocity
        Rect vVelocityRect = new Rect(windowRect.width - 83, windowRect.height / 5 * 4.2f - 10, 80, 50);
        Rect vVelocityRectLabel = new Rect(5, windowRect.height / 5 * 4.2f - 20, 50, 50);
        string vVelocityValue = ((values[3][values[3].Count - 1])).ToString("F") + " m/s";
        GUI.Label(vVelocityRect, vVelocityValue, vVelocityGUIStyle);
        GUI.Label(vVelocityRectLabel, "Vertical Velocity", vVelocityLabelGUIStyle);

        GUI.Label(new Rect(windowRect.width - labelX, labelY, labelWidth, labelHeight), "Text Position", myGUIStyle);

        initDone = false;
    }
}