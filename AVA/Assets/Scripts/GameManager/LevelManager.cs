using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private bool Paused = false;
    private bool Slowed = false;
    public float SlowMotionValue = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (Paused)
            {
                ResumeGame();
                Paused = false;
            }
            else
            {
                PauseGame();
                Paused = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            if (Slowed)
            {
                Slowmotion(SlowMotionValue);
                Slowed = false;
            }
            else
            {
                ResumeGame();
                Slowed = true;
            }
        }

        if (Input.GetAxis("Restart")!= 0)
        {
            ReloadScene();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public void Slowmotion(float Scale)
    {
        Time.timeScale = Scale;
    }

}
