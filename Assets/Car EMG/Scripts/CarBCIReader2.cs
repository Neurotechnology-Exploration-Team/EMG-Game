using System;
using UnityEngine;

using brainflow;
public class CarBCIReader2 : MonoBehaviour, OpenBCIReaderI
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
    /// Also known as sensitivity
    /// </summary>
    /// <see cref="SetThresholdSensitivity(int,double)"/>
    private int NumSamplesPerInput = 500;
    
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
    
    /// <summary>
    /// Variable that represents the current board connection
    /// </summary>
    private BoardShim boardShim;
    
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
                try { boardShim.stop_stream(); } 
                catch (BrainFlowException e) {Debug.LogError(e);}
                try { boardShim.release_session(); } 
                catch (BrainFlowException e) {Debug.LogError(e);}
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
        
        var inputParams = new BrainFlowInputParams
        {
            serial_port = attemptSerialPort
        };

        boardShim = new BoardShim(CytonBoardID, inputParams);
        try
        {
            boardShim.prepare_session();
            
            boardShim.start_stream(450000, "file://file_stream.csv:w");

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
        
        var inputParams = new BrainFlowInputParams
        {
            ip_address = "192.168.4.1",
            ip_port = emptyPort
        };

        boardShim = new BoardShim(WifiCytonBoardID, inputParams);
        try
        {
            boardShim.prepare_session();
            
            boardShim.start_stream(450000, "file://file_stream.csv:w");

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
            return boardShim.get_current_board_data(NumSamplesPerInput);
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
        boardShim.stop_stream();
        boardShim.release_session();
    }

    public OpenBCIReaderI.ConnectionStatus GetConnectionStatus()
    {
        return connectionStatus;
    }

    public int? GetNumChannels()
    {
        throw new NotImplementedException();
    }

    public void SetAllowWifi(bool shouldAllowWifi)
    {
        allowWifi = shouldAllowWifi;
    }

    public bool GetAllowWifi()
    {
        return allowWifi;
    }

    public void SetWifiBoardName(string newWifiBoardName)
    {
        wifiBoardName = newWifiBoardName;
    }

    public string GetWifiBoardName()
    {
        return wifiBoardName;
    }

    public void SetDefaultSerialPort(string port)
    {
        serialPort = port;
    }

    public string GetDefaultSerialPort()
    {
        return serialPort;
    }

    public void SetVerbose(bool newVerbose)
    {
        verbose = newVerbose;
    }

    public bool GetVerbose()
    {
        return verbose;
    }

    public void SetThreshold(int channel, double threshold)
    {
        throw new NotImplementedException();
    }

    public void AutoRestingThreshold(int channel)
    {
        throw new NotImplementedException();
    }

    public void AutoRestingThreshold()
    {
        throw new NotImplementedException();
    }

    public double GetThreshold(int channel)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdType(int channel, OpenBCIReaderI.ThresholdType thresholdType)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdType(OpenBCIReaderI.ThresholdType thresholdType)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdSensitivity(int channel, int sensitivity)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdSensitivity(int sensitivity)
    {
        NumSamplesPerInput = sensitivity;
    }

    public bool GetInput(int channel)
    {
        throw new NotImplementedException();
    }

    public double GetNumericInput(int channel)
    {
        throw new NotImplementedException();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }

    public void Reconnect()
    {
        throw new NotImplementedException();
    }
}
