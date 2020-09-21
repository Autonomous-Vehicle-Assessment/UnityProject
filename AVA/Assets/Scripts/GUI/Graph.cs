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
    private bool initDone = false;

    // Start is called before the first frame update
    private void Start()
    {
        mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        // Should check for material but I'll leave that to you..

        // Initialize list
        values = new List<float>[3];

        for (int i = 0; i < 3; i++)
        {
            values[i] = new List<float>(1);
        }
    }

    // Update is called once per frame
    public void UpdateGraph(float Speed, float RPM, int CurrentGear)
    {
        // Keep adding values
        values[0].Add(Speed);
        values[1].Add(RPM/20f);
        values[2].Add(CurrentGear * 50);

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

            // Graph window
            windowRect = new Rect(Screen.width - 20 - 512, 20, 512, 256);
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
            GL.Color(new Color(0,0,0,0.2f));
            GL.Vertex3(2, 15, 0);
            GL.Vertex3(windowRect.width - 2, 15, 0);
            GL.Vertex3(windowRect.width - 2, windowRect.height - 2, 0);
            GL.Vertex3(2, windowRect.height - 2, 0);
            GL.End();

            for (int i = 0; i < values.Length; i++)
            {
                // Draw the lines of the graph
                GL.Begin(GL.LINES);
                GL.Color(colorList[i]);

                int valueIndex = values[i].Count - 1;
                for (int k = (int)windowRect.width - 100; k > 3; k--)
                {
                    float y1 = 0;
                    float y2 = 0;
                    if (valueIndex > 0)
                    {
                        y2 = values[i][valueIndex];
                        y1 = values[i][valueIndex - 1];
                    }
                    GL.Vertex3(k, windowRect.height - 2 - y2, 0);
                    GL.Vertex3((k - 1), windowRect.height - 2 - y1, 0);
                    valueIndex -= 1;
                }
                GL.End();
            }
            GL.PopMatrix();
        }

        // Populate graph with text
        Rect rpmRect = new Rect(windowRect.width - 104, 40, 100, 50);
        Rect rpmRectLabel = new Rect(windowRect.width - 104, 40, 100, 50);
        string rpmValue = ((int)(values[1][values[1].Count - 1] * 20)).ToString() + " rpm";
        GUI.Label(rpmRect, rpmValue, rpmGUIStyle);

        GUI.Label(new Rect(windowRect.width - labelX, labelY, labelWidth, labelHeight), "Text Position", myGUIStyle);

        initDone = false;
    }
}