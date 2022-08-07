using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using brainflow;
using Random = System.Random;

public class OpenBCIReaderDummy : MonoBehaviour, OpenBCIReaderI
{
    /// <summary>
    /// Get whether or not the program prints every message or just important ones.
    /// If true, program will print a lot of debug information
    /// If false, program will print only critical info such as whether or not the board has connected
    /// </summary>
    /// <see cref="SetVerbose"/>
    /// <see cref="GetVerbose"/>
    public bool verbose;
    /// <summary>
    /// Whether the class should try to connect to the board when it is instantiated
    /// In production, should be false
    /// Otherwise, set via Unity editor
    /// </summary>
    public bool attemptConnectionOnStartup;
    /// <summary>
    /// Set whether or not the program should be allowed to use wifi to connect to the board.
    ///
    /// Note: turning this on may add a significant amount of time to reconnection delay
    /// </summary>
    /// <see cref="SetAllowWifi(bool)"/>
    /// <see cref="GetAllowWifi"/>
    public bool allowWifi;

    /// <summary>
    /// The name of the wifi shield
    /// Should be similar to "OpenBCI-XXXX"
    /// </summary>
    private string wifiBoardName;
    
    /// <summary>
    /// Constant board id for bluetooth Cyton connections
    /// <see href="https://brainflow.readthedocs.io/en/stable/SupportedBoards.html#openbci"/>
    /// </summary>
    private const int CytonBoardID = 0;
    /// <summary>
    /// Constant board id for wifi Cyton connections
    /// <see href="https://brainflow.readthedocs.io/en/stable/SupportedBoards.html#openbci"/>
    /// </summary>
    private const int WifiCytonBoardID = 5;
    
    /// <summary>
    /// Similar to "COMX", where X is an integer from 1 to 9.
    ///
    /// In Windows, see device manager for serial port list.
    /// </summary>
    /// <see cref="SetDefaultSerialPort"/>
    /// <see cref="GetDefaultSerialPort"/>
    private string serialPort;
    
    // /// <summary>
    // /// Variable that represents the current board connection
    // /// </summary>
    // private BoardShim boardShim;

    /// <summary>
    /// Number of channels in the BCI device, usually determined by boardShim
    /// </summary>
    private int numChannels = 8;

    /// <summary>
    /// Current averaged measured nanovolt values
    /// </summary>
    private double[] nanovoltAverages = new double[8];

    /// <summary>
    /// Thresholds, in nanovolts, for each channel
    /// </summary>
    private double[] thresholds = new Double[8];

    /// <summary>
    /// Threshold types for each channel
    /// </summary>
    private OpenBCIReaderI.ThresholdType[] thresholdTypes = new OpenBCIReaderI.ThresholdType[8];

    /// <summary>
    /// Threshold sensivitities (number of samples per average) for each channel
    /// </summary>
    /// <see cref="SetThresholdSensitivity(int,double)"/>
    private int[] thresholdSensitivities = new int[8];
    
    /// <summary>
    /// Current connection status
    /// </summary>
    /// <see cref="OpenBCIReaderI.ConnectionStatus"/>
    private OpenBCIReaderI.ConnectionStatus connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;

    /// <summary>
    /// The last recorded value, used to test if board connection is stable
    /// </summary>
    /// <see cref="lastValTime"/>
    private double lastVal;
    /// <summary>
    /// The timestamp of the last recorded value, used to test if board connection is stable
    /// </summary>
    /// <see cref="lastVal"/>
    private DateTime lastValTime = DateTime.UtcNow;

    /// <summary>
    /// Called before the first frame update.
    /// Attempts to start the connection if attemptConnectionOnStartup
    /// </summary>
    private void Start()
    {
        SetThresholdSensitivity(500);
        
        if (!attemptConnectionOnStartup) return;
        
        AttemptConnect();
        Debug.Log(connectionStatus == OpenBCIReaderI.ConnectionStatus.Disconnected
            ? "No OpenBCI board connection could be made."
            : "OpenBCI board connecting...");
    }

    // Update is called once per frame
    private void Update()
    {
        if (connectionStatus == OpenBCIReaderI.ConnectionStatus.Disconnected) return;
        
        var data = GetRawData();
        for (int channel = 0; channel < numChannels; channel++)
        {
            double avg = 0;
            for (int sample = 0; sample < thresholdSensitivities[channel]; sample++)
            {
                avg += data[channel, sample];
            }

            avg /= thresholdSensitivities[channel];
            nanovoltAverages[channel] = avg;
        }
        if (data == null || data.Length <= 0) return;
        switch (connectionStatus)
        {
            case OpenBCIReaderI.ConnectionStatus.Connecting when Math.Abs(data[0, 0] - lastVal) >= .01:
                Debug.Log("OpenBCI board connected.");
                connectionStatus = OpenBCIReaderI.ConnectionStatus.Connected;
                break;
            case OpenBCIReaderI.ConnectionStatus.Connected when Math.Abs(data[0, 0] - lastVal) < .01 && 
                                                 (DateTime.Now - lastValTime).TotalSeconds > 5:
            {
                Debug.LogWarning("Board connection faulty... please do not close the program...");
                connectionStatus = OpenBCIReaderI.ConnectionStatus.Reconnecting;
                break;
            }
            case OpenBCIReaderI.ConnectionStatus.Reconnecting when Math.Abs(data[0, 0] - lastVal) < .01 &&
                                                    (DateTime.Now - lastValTime).TotalSeconds > 10:
            {
                Debug.LogError("Board connection failed. Please wait for brainflow to close safely.");
                connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                Thread.Sleep(500);
                // try { boardShim.stop_stream(); } 
                // catch (BrainFlowException e) {Debug.LogError(e);}
                // try { boardShim.release_session(); } 
                // catch (BrainFlowException e) {Debug.LogError(e);}
                Debug.Log("It is now safe to stop the game.");
                break;
            }
        }

        if (Math.Abs(data[0, 0] - lastVal) > .01)
        {
            lastVal = data[0, 0];
            lastValTime = DateTime.Now;
        }
        // Debug.Log(data[0, 0]);
    }
    
    private bool AttemptConnect()
    {
        // enable debug info
        BoardShim.enable_dev_board_logger();
        
        if (verbose) Debug.Log("Attempting bluetooth connection...");

        if (serialPort == null)
        {
            if (verbose) Debug.LogWarning("Warning: No serial port detected. Attempting to search for board...");
            for (int i = 0; i < 10; i++)
            {
                if (AttemptConnectSerial("COM" + i))
                {
                    serialPort = "COM" + i;
                    return true;
                }
            }
        }
        else
        {
            AttemptConnectSerial(serialPort);
        }

        if (connectionStatus != OpenBCIReaderI.ConnectionStatus.Disconnected) return true;
        if (allowWifi) return AttemptConnectWifi(4000);
        if (verbose) Debug.Log("Wifi not allowed.");
        return false;
    }

    private bool AttemptConnectSerial(string attemptSerialPort)
    {
        connectionStatus = OpenBCIReaderI.ConnectionStatus.Connecting;
        
        try
        {
            Thread.Sleep(100);
            if (false) throw new Exception("UNABLE_TO_OPEN_PORT_ERROR:2");

            Debug.Log("OpenBCI initialization complete on " + attemptSerialPort);
            return true;
        }
        catch (BrainFlowException e)
        {
            switch (e.Message)
            {
                case "UNABLE_TO_OPEN_PORT_ERROR:2":
                    if (verbose) Debug.LogWarning("Warning, board not available on " + attemptSerialPort + ": UNABLE_TO_OPEN_PORT_ERROR:2");
                    connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                    return false;
                case "GENERAL_ERROR:17":
                    if (verbose) Debug.LogWarning("Warning, board not available on " + attemptSerialPort + ": GENERAL_ERROR:17");
                    connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                    return false;
                case "BOARD_WRITE_ERROR:4":
                    if (verbose) Debug.LogWarning("Warning, board not available on " + attemptSerialPort + ": BOARD_WRITE_ERROR:4");
                    connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                    return false;
                case "ANOTHER_BOARD_IS_CREATED_ERROR:16":
                    Debug.LogWarning("Another process is using the board on " + attemptSerialPort + ": ANOTHER_BOARD_IS_CREATED_ERROR:16\n" +
                                     "You may need to restart Unity.");
                    connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                    return false;
                case "BOARD_NOT_READY_ERROR:7":
                    Debug.LogWarning("Warning, board not ready on " + attemptSerialPort + ": BOARD_NOT_READY_ERROR:7\n" +
                                     "Please try again, and make sure the actual cyton board is turned on.");
                    connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                    return false;
                default:
                    Debug.Log("Unknown message: " + e.Message);
                    throw;
            }
        }
    }
    
    private bool AttemptConnectWifi(int emptyPort)
    {
        if (verbose) Debug.Log("Attempting wifi connect...");
        connectionStatus = OpenBCIReaderI.ConnectionStatus.Connecting;
        
        try
        {
            if (false) throw new Exception("BOARD_WRITE_ERROR:4");
            
            Thread.Sleep(10_000);

            Debug.Log("OpenBCI initialization complete on wifi");
            return true;
        }
        catch (BrainFlowException e)
        {
            switch (e.Message)
            {
                case "BOARD_WRITE_ERROR:4":
                    if (verbose) Debug.LogWarning("Warning, board not available on wifi: BOARD_WRITE_ERROR:4");
                    connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                    return false;
                default:
                    Debug.Log("Unknown message: " + e.Message);
                    throw;
            }
        }
    }
    private double[,] GetRawData()
    {
        if (connectionStatus == OpenBCIReaderI.ConnectionStatus.Disconnected)
        {
            Debug.LogWarning("Warning: attempt to collect data when board is not connected");
            return null;
        }
        try
        {
            if (false) throw new Exception("BOARD_NOT_CREATED_ERROR:15");
            
            Random r = new Random();
            double[,] rawData = new double[numChannels, thresholdSensitivities.Max()];
            for (int channel = 0; channel < numChannels; channel++)
            {
                for (int val = 0; val < thresholdSensitivities[0]; val++)
                {
                    rawData[channel, val] = r.NextDouble() * (flexedChannels[channel] ? 2 : 1);
                }
            }

            return rawData;
        }
        catch (BrainFlowException e)
        {
            if (e.Message.Equals("BOARD_NOT_CREATED_ERROR:15"))
            {
                Debug.Log("Warning: attempt to collect data when board is believed to be connected but actually is not.");
                connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
                return null;
            }
            else
            {
                Debug.LogError("Error: unknown. Terminating session and throwing error: " + e.Message);
                Destroy(gameObject);
                throw;
            }
        }
    }

    private void OnDestroy()
    {
        if (connectionStatus == OpenBCIReaderI.ConnectionStatus.Disconnected) return;
        // boardShim.stop_stream();
        // boardShim.release_session();
    }

    public OpenBCIReaderI.ConnectionStatus GetConnectionStatus()
    {
        return connectionStatus;
    }

    public int? GetNumChannels()
    {
        switch (connectionStatus)
        {
            case OpenBCIReaderI.ConnectionStatus.Connected:
                return numChannels;
            case OpenBCIReaderI.ConnectionStatus.Disconnected:
            case OpenBCIReaderI.ConnectionStatus.Connecting:
            case OpenBCIReaderI.ConnectionStatus.Reconnecting:
            default:
                return null;
        }
    }

    public void SetAllowWifi(bool allowWifi)
    {
        if (verbose) Debug.Log("Setting allowWifi to " + allowWifi);
        this.allowWifi = allowWifi;
    }

    public bool GetAllowWifi()
    {
        return allowWifi;
    }

    public void SetWifiBoardName(string name)
    {
        if (verbose) Debug.Log("Setting wifiBoardName to " + name);
        wifiBoardName = name;
    }

    public string GetWifiBoardName()
    {
        return wifiBoardName;
    }

    public void SetDefaultSerialPort(string port)
    {
        if (verbose) Debug.Log("Setting defaultSerialPort to " + port);
        serialPort = port;
    }

    public string GetDefaultSerialPort()
    {
        return serialPort;
    }

    public void SetVerbose(bool verbose)
    {
        if (verbose) Debug.Log("Setting verbose to " + verbose);
        this.verbose = verbose;
    }

    public bool GetVerbose()
    {
        return verbose;
    }

    public void SetThreshold(int channel, double threshold)
    {
        if (verbose) Debug.Log("Setting threshold for channel " + channel + " to " + threshold);
        thresholds[channel] = threshold;
    }

    public void AutoRestingThreshold(int channel)
    {
        if (verbose) Debug.Log("Auto-setting resting threshold for channel " + channel);
        thresholds[channel] = nanovoltAverages[channel] * 1.2;
    }

    public void AutoRestingThreshold()
    {
        if (verbose) Debug.Log("Auto-setting resting threshold for all channels");
        for (int channel = 0; channel < thresholds.Length; channel++)
        {
            thresholds[channel] = nanovoltAverages[channel] * 1.2;
            if (verbose) Debug.Log("Auto channel " + channel + " = " + thresholds[channel]);
        }
    }

    public double GetThreshold(int channel)
    {
        return thresholds[channel];
    }

    public void SetThresholdType(int channel, OpenBCIReaderI.ThresholdType thresholdType)
    {
        if (verbose) Debug.Log("Setting threshold type for channel " + channel + " to " + thresholdType);
        thresholdTypes[channel] = thresholdType;
    }

    public void SetThresholdType(OpenBCIReaderI.ThresholdType thresholdType)
    {
        if (verbose) Debug.Log("Setting threshold type for all channels to " + thresholdType);
        for (int channel = 0; channel < thresholdTypes.Length; channel++) thresholdTypes[channel] = thresholdType;
    }

    public void SetThresholdSensitivity(int channel, int sensitivity)
    {
        if (verbose) Debug.Log("Setting threshold sensitivity for channel " + channel + " to " + sensitivity);
        thresholdSensitivities[channel] = sensitivity;
    }

    public void SetThresholdSensitivity(int sensitivity)
    {
        if (verbose) Debug.Log("Setting threshold sensitivity for all channels to " + sensitivity);
        for (int channel = 0; channel < thresholdSensitivities.Length; channel++) 
            thresholdSensitivities[channel] = sensitivity;
    }

    public bool GetInput(int channel)
    {
        return nanovoltAverages[channel] > thresholds[channel];
    }

    public double GetNumericInput(int channel)
    {
        return nanovoltAverages[channel];
    }

    public void Disconnect()
    {
        if (verbose) Debug.Log("Disconnecting");
        connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;
    }

    public void Reconnect()
    {
        if (verbose) Debug.Log("Reconnecting");
        Disconnect();

        AttemptConnect();
        Debug.Log(connectionStatus == OpenBCIReaderI.ConnectionStatus.Disconnected
            ? "No OpenBCI board connection could be made."
            : "OpenBCI board connecting...");
    }

    /// <summary>
    /// Array representing whether the fake user is currently flexing the muscle
    /// </summary>
    public bool[] flexedChannels = new bool[8];

    /// <summary>
    /// Set all simulated muscles to flexing or not flexing
    /// </summary>
    /// <param name="flexed">If true, all simulated muscles will flex. If false, all simulated muscles will relax.</param>
    public void Flexed(bool flexed)
    {
        for (int channel = 0; channel < numChannels; channel++)
        {
            flexedChannels[channel] = flexed;
        }
    }

    /// <summary>
    /// Set a certain channel to flexing or not flexing
    /// </summary>
    /// <param name="flexed">If true, the simulated muscle will flex. If false, the simulated muscle will relax.</param>
    /// <param name="channel">Which simulated muscle to change</param>
    public void Flexed(bool flexed, int channel)
    {
        flexedChannels[channel] = flexed;
    }

    /// <summary>
    /// Returns whether or not a simulated muscle is currently flexing
    /// </summary>
    /// <param name="channel">Which simulated muscle to return</param>
    /// <returns>Whether or not the specified simulated muscle is currently flexing</returns>
    public bool Flexed(int channel)
    {
        return flexedChannels[channel];
    }
}
