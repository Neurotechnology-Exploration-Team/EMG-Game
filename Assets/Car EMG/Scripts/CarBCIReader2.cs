using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using brainflow;

public class CarBCIReader2 : MonoBehaviour
{
    private const string SerialPort = "COM7";
    private const int NumSamplesPerInput = 500;
    private const int BoardID = 0;
    
    private static BoardShim _boardShim;

    // Start is called before the first frame update
    void Start()
    {
        BoardShim.enable_dev_board_logger();

        var inputParams = new BrainFlowInputParams
        {
            serial_port = SerialPort
        };

        _boardShim = new BoardShim(BoardID, inputParams);
        try
        {
            _boardShim.prepare_session();
        }
        catch (BrainFlowException e)
        {
            Debug.Log("Do you have the wrong port, or is the device plugged in, or could the device be taken up by other programs?");
            Debug.LogError(e);
        }
        _boardShim.start_stream(450000, "file://file_stream.csv:w");

        Debug.Log("OpenBCI initialization complete");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private double[,] GetRawData()
    {
        try
        {
            return _boardShim.get_current_board_data(NumSamplesPerInput);
        }
        catch (BrainFlowException)
        {
            Debug.Log("Do you have the wrong port, or is the device plugged in, or could the device be taken up by other programs?");
            Destroy(gameObject);
            throw;
        }
    }

    private void OnDestroy()
    {
        _boardShim.stop_stream();
        _boardShim.release_session();
    }
}
