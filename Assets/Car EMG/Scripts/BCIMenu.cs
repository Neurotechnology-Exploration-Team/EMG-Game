using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BCIMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject bciReaderObject;
    private OpenBCIReaderI bciReader;

    public GameObject pauseMenuUI;
    public Toggle advanceToggle;

    public Image networkConnection;
    public Image cytonConnection;

    public bool networkConnected;
    public OpenBCIReaderI.ConnectionStatus cytonConnected;

    public Dropdown boardType;

    public GameObject boardName;

    // BCI Menu Components

    // Buttons

    // Thresholds
    [Header("Threshold UI Settings")]

    // 0
    public Slider zeroSlider;
    public Slider zeroBar;
    public float zeroBarMax;
    public GameObject zeroAdvance;
    public TextMeshProUGUI zeroDebugOne;
    public TextMeshProUGUI zeroDebugTwo;
    public TextMeshProUGUI zeroDebugThree;
    public GameObject zeroParameter;

    // Start is called before the first frame update
    private void Start()
    {
        bciReader = bciReaderObject.GetComponent<OpenBCIReaderI>();
    }

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
        
        cytonConnected = bciReader.GetConnectionStatus();

        if (cytonConnected == OpenBCIReaderI.ConnectionStatus.Connected)
        {
            cytonConnection.color = new Color(0, 1, 0, 1);
        }
        else if (cytonConnected == OpenBCIReaderI.ConnectionStatus.Connecting)
        {
            cytonConnection.color = new Color(1, 1, 0, 1);
        }
        else if (cytonConnected == OpenBCIReaderI.ConnectionStatus.Disconnected)
        {
            cytonConnection.color = new Color(1, 0, 0, 1);
        } else if (cytonConnected == OpenBCIReaderI.ConnectionStatus.Reconnecting)
        {
            cytonConnection.color = new Color(1, 1, 0, 1);
        }

        if (networkConnected)
        {
            networkConnection.color = new Color(0, 1, 0, 1);
        }
        else
        {
            networkConnection.color = new Color(1, 0, 0, 1);
        }
        
        // update threshold bar
        zeroBar.value = (float) bciReader.GetNumericInput(0) / zeroBarMax;
        
        // update debug values
        zeroDebugOne.SetText("Value: " + Math.Round(bciReader.GetNumericInput(0)*1000000)/1000000);
        zeroDebugTwo.SetText("T: " + Math.Round(zeroSlider.value*zeroBarMax*1000000)/1000000);
        zeroDebugThree.SetText("Limit: " + zeroBarMax);
    }

    public void SetThresholdBar(int slider)
    {
        bciReader.SetThreshold(slider, zeroSlider.value);
    }
    
    public void SetThresholdParameter(int slider)
    {
        bciReader.SetThresholdSensitivity(slider, Int32.Parse(zeroParameter.GetComponent<TMP_InputField>().text));
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
        bciReader.Disconnect();
    }

    public void Reconnect_Board()
    {
        bciReader.Reconnect();
    }
}
