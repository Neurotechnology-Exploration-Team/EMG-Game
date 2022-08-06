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
    public Toggle advancedToggle;

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
    public BCIMenuChannel[] channels;
    // public BCIMenuChannel zero;

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

        foreach (BCIMenuChannel channel in channels)
        {
            // update threshold bar
            channel.bar.value = (float) bciReader.GetNumericInput(0) / channel.barMax;
        
            // update debug values
            channel.debugOne.SetText("Value: " + Math.Round(bciReader.GetNumericInput(0)*1000000)/1000000);
            channel.debugTwo.SetText("T: " + Math.Round(channel.slider.value*channel.barMax*1000000)/1000000);
            channel.debugThree.SetText("Limit: " + channel.barMax);
        }
    }

    public void SetThresholdBar(int slider)
    {
        foreach (BCIMenuChannel channel in channels)
        {
            if (channel.slider.value >= 0 && channel.slider.value <= 1)
            {
                bciReader.SetThreshold(slider, channel.slider.value);
            }
            else if(bciReader.GetVerbose()){
                Debug.Log("Threshold outside of range");
            }
        }
    }
    
    public void SetThresholdParameter(int slider)
    {
        if (slider < channels.Length)
        {
            if (Int32.TryParse(channels[slider].parameter.GetComponent<TMP_InputField>().text, out int value))
            {
                if (value >= 1 && value <= 1000)
                {
                    bciReader.SetThresholdSensitivity(slider, value);
                }
                else if (bciReader.GetVerbose())
                {
                    Debug.Log("Threshold Parameter outside of range");
                }
            }
            else if (bciReader.GetVerbose())
            {
                Debug.Log("Invalid Threshold Parameter, unable to parse");
            }
        }
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

    public void Advanced()
    {
        if (advancedToggle.isOn)
        {
            foreach (BCIMenuChannel channel in channels)
            {
                channel.advanced.SetActive(true);
            }
        }
        else
        {
            foreach (BCIMenuChannel channel in channels)
            {
                channel.advanced.SetActive(false);
            }
        }
    }

    public void Board_Type()
    {
        if (boardType.value == 1 || boardType.value == 0)
        {
            bciReader.SetAllowWifi(boardType.value == 1);
            if (boardType.value == 1)
            {
                boardName.SetActive(true);
            }
            else
            {
                boardName.SetActive(false);
            }
            bciReader.Reconnect();
        }
    }

    public void WifiBoardName()
    {
        name = boardName.GetComponent<TMP_InputField>().text;
        if (name.Length == 12 && name.Substring(0, 8).Equals("OpenBCI-")) 
        {
            bool valid = true;
            foreach(char c in name.Substring(8))
            {
                if (!Char.IsLetterOrDigit(c))
                {
                    valid = false;
                }
            }
            if (valid)
            {
                bciReader.SetWifiBoardName(name);
            }
            else if (bciReader.GetVerbose())
            {
                Debug.Log("Board code contains non alphanumeric character");
            }
        }
        else if (bciReader.GetVerbose())
        {
            Debug.Log("Invalid board name, board name should follow \"OpenBCI-XXXX\" format");
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
