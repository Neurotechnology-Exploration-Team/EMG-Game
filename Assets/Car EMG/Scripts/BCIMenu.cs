using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BCIMenu : MonoBehaviour, BCIMenuI
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

    public List<string> keybindNames = new List<string>();
    public Dictionary<int, string> channelKeybinds = new Dictionary<int, string>();
    private Dictionary<string, bool> keybindInputValues = new Dictionary<string, bool>();
    private Dictionary<string, double> keybindRawInputValues = new Dictionary<string, double>();
    
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

        for (int i = 0; i < channels.Length; i++)
        {
            channelKeybinds[i] = "";
        }

        foreach (string keybindName in keybindNames)
        {
            keybindInputValues[keybindName] = false;
            keybindRawInputValues[keybindName] = 0;
        }
        
        MakeKeybindDropdowns();
        SetAllKeybinds();
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

        switch (cytonConnected)
        {
            case OpenBCIReaderI.ConnectionStatus.Connected:
                cytonConnection.color = new Color(0, 1, 0, 1);
                break;
            case OpenBCIReaderI.ConnectionStatus.Connecting:
                cytonConnection.color = new Color(1, 1, 0, 1);
                break;
            case OpenBCIReaderI.ConnectionStatus.Disconnected:
                cytonConnection.color = new Color(1, 0, 0, 1);
                break;
            case OpenBCIReaderI.ConnectionStatus.Reconnecting:
                cytonConnection.color = new Color(1, 1, 0, 1);
                break;
        }
        
        networkConnection.color = networkConnected ? 
            new Color(0, 1, 0, 1) : 
            new Color(1, 0, 0, 1);

        foreach (string keybindName in keybindNames)
        {
            keybindInputValues[keybindName] = false;
            keybindRawInputValues[keybindName] = 0;
        }
        
        foreach (BCIMenuChannel channel in channels)
        {
            int channelIndex = Array.IndexOf(channels, channel);
            
            // update threshold bar
            channel.bar.value = (float) bciReader.GetNumericInput(channelIndex) / channel.barMax;

            if (!channelKeybinds[channelIndex].Equals(""))
            {
                keybindInputValues[channelKeybinds[channelIndex]] = bciReader.GetInput(channelIndex);
                keybindRawInputValues[channelKeybinds[channelIndex]] = bciReader.GetNumericInput(channelIndex);
            }
        
            // update debug values
            channel.debugOne.SetText("Value: " + Math.Round(bciReader.GetNumericInput(channelIndex)*1000000)/1000000);
            channel.debugTwo.SetText("T: " + Math.Round(channel.slider.value*channel.barMax*1000000)/1000000);
            channel.debugThree.SetText("Limit: " + channel.barMax);
        }
    }

    public void SetThresholdBar(int slider)
    {
        BCIMenuChannel channel = channels[slider];
        if (channel.slider.value >= 0 && channel.slider.value <= 1)
        {
            bciReader.SetThreshold(slider, channel.slider.value);
        }
        else if(bciReader.GetVerbose()){
            Debug.Log("Threshold outside of range");
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

    public List<string> GetKeybindNames()
    {
        return keybindNames;
    }

    public void SetKeybindNames(List<string> keybinds)
    {
        keybindNames = keybinds;

        MakeKeybindDropdowns();
    }

    public void MakeKeybindDropdowns()
    {
        channelKeybinds = new Dictionary<int, string>();
        for (int i = 0; i < channels.Length; i++)
        {
            channelKeybinds[i] = "";
            channels[i].keybind.options = new List<Dropdown.OptionData>();
            channels[i].keybind.options.Add(new Dropdown.OptionData(""));
            foreach (string keybind in keybindNames)
            {
                channels[i].keybind.options.Add(new Dropdown.OptionData(keybind));
            }
        }
    }

    public bool GetInputForKeybind(string keybind)
    {
        return keybindInputValues[keybind];
    }

    public double GetRawInputForKeybind(string keybind)
    {
        return keybindRawInputValues[keybind];
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

    public void SetAllKeybinds()
    {
        for (int i = 0; i < channels.Length; i++)
        {
            if (channels[i].keybind.value > 0)
            {
                channelKeybinds[i] = keybindNames[channels[i].keybind.value - 1];
            }
            else
            {
                channelKeybinds[i] = "";
            }
        }
    }

    public void ResetThresholdSliders()
    {
        for (int channel = 0; channel < channels.Length; channel++)
        {
            SetThresholdBar(channel);
        }
    }
}
