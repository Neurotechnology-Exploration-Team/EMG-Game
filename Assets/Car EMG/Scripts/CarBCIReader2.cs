using System;
using UnityEngine;

using brainflow;

public class CarBCIReader2 : MonoBehaviour, OpenBCIReaderI
{
    
    public bool verbose;
    public bool attemptConnectionOnStartup;
    public bool allowWifi;
    
    private const int NumSamplesPerInput = 500;
    private const int CytonBoardID = 0;
    private const int WifiCytonBoardID = 5;
    
    private string serialPort;
    private BoardShim boardShim;
    private OpenBCIReaderI.ConnectionStatus connectionStatus = OpenBCIReaderI.ConnectionStatus.Disconnected;

    private double lastVal;
    private DateTime lastValTime = DateTime.UtcNow;

    // Start is called before the first frame update
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
        throw new NotImplementedException();
    }

    public int GetNumChannels()
    {
        throw new NotImplementedException();
    }

    public void SetAllowWifi()
    {
        throw new NotImplementedException();
    }

    public bool GetAllowWifi()
    {
        throw new NotImplementedException();
    }

    public void SetWifiBoardName(string name)
    {
        throw new NotImplementedException();
    }

    public string GetWifiBoardName()
    {
        throw new NotImplementedException();
    }

    public void SetDefaultSerialPort(string port)
    {
        throw new NotImplementedException();
    }

    public string GetDefaultSerialPort()
    {
        throw new NotImplementedException();
    }

    public void SetVerbose()
    {
        throw new NotImplementedException();
    }

    public bool GetVerbose()
    {
        throw new NotImplementedException();
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

    public void SetThresholdType(int channel, OpenBCIReaderI.ThresholdType thresholdType)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdType(OpenBCIReaderI.ThresholdType thresholdType)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdSensitivity(int channel, double sensitivity)
    {
        throw new NotImplementedException();
    }

    public void SetThresholdSensitivity(double sensitivity)
    {
        throw new NotImplementedException();
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
