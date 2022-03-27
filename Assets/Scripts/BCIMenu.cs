using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BCIMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    // BCI Menu Components

    // Buttons

    // Thresholds
    [Header("Threshold UI Settings")]

    // 0
    public Slider zeroSlider;
    public Slider zeroBar;



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }


    }

    public void SetThresholdBar(Slider threshold, float thresholdValue)
    {

    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void Disconnect_Board()
    {
        // Code Here
    }

    public void Reconnect_Board()
    {
        // Code Here
    }

    public void Terminate_Program()
    {
        // Code Here
    }
}
