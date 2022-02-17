using UnityEngine;

using brainflow;

public class CarBCIReader2 : MonoBehaviour
{
    public bool verbose;
    
    private enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected
    }
    
    private const int NumSamplesPerInput = 500;
    private const int BoardID = 0;
    
    private string serialPort;
    private BoardShim boardShim;
    private ConnectionStatus connectionStatus = ConnectionStatus.Disconnected;

    // Start is called before the first frame update
    void Start()
    {
        AttemptConnect();
        
        if (connectionStatus == ConnectionStatus.Disconnected) Debug.Log("No board detected.");
    }

    // Update is called once per frame
    void Update()
    {
        if (connectionStatus == ConnectionStatus.Connected)
        {
            double[,] data = GetRawData();
        }
    }

    private bool AttemptConnect()
    {
        // enable debug info
        BoardShim.enable_dev_board_logger();

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

            return false;
        }
        else
        {
            return AttemptConnectSerial(serialPort);
        }
    }

    private bool AttemptConnectSerial(string attemptSerialPort)
    {
        connectionStatus = ConnectionStatus.Connecting;
        
        var inputParams = new BrainFlowInputParams
        {
            serial_port = serialPort
        };

        boardShim = new BoardShim(BoardID, inputParams);
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
                    connectionStatus = ConnectionStatus.Disconnected;
                    return false;
                case "GENERAL_ERROR:17":
                    if (verbose) Debug.LogWarning("Warning, board not available on " + attemptSerialPort + ": GENERAL_ERROR:17");
                    connectionStatus = ConnectionStatus.Disconnected;
                    return false;
                default:
                    Debug.Log("Unknown message: " + e.Message);
                    throw;
            }
        }
    }
    private double[,] GetRawData()
    {
        if (connectionStatus != ConnectionStatus.Connected)
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
                connectionStatus = ConnectionStatus.Disconnected;
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
        if (connectionStatus == ConnectionStatus.Disconnected) return;
        boardShim.stop_stream();
        boardShim.release_session();
    }
}
