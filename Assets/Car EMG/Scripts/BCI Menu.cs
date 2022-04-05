using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BCIMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public Toggle advanceToggle;

    public Image networkConnection;
    public Image cytonConnection;

    public bool networkConnected;
    public bool cytonConnected;

    public Dropdown boardType;

    public GameObject boardName;

    // BCI Menu Components

    // Buttons

    // Thresholds
    [Header("Threshold UI Settings")]

    // 0
    public Slider zeroSlider;
    public Slider zeroBar;
    public GameObject zeroAdvance;
    public TextMeshProUGUI zeroDebugOne;
    public TextMeshProUGUI zeroDebugTwo;
    public TextMeshProUGUI zeroDebugThree;



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

        if (cytonConnected)
        {
            cytonConnection.color = new Color(0, 1, 0, 1);
        }
        else
        {
            cytonConnection.color = new Color(1, 0, 0, 1);
        }

        if (networkConnected)
        {
            networkConnection.color = new Color(0, 1, 0, 1);
        }
        else
        {
            networkConnection.color = new Color(1, 0, 0, 1);
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

    public void Advance()
    {
        if (advanceToggle.isOn)
        {
            zeroAdvance.SetActive(true);
        }
        else
        {
            zeroAdvance.SetActive(false);
        }
    }

    public void Board_Type()
    {
        if (boardType.value == 1)
        {
            boardName.SetActive(true);
        }
        else
        {
            boardName.SetActive(false);
        }
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
