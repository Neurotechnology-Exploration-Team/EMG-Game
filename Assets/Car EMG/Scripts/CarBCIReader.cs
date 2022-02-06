using System;
using System.Collections.Generic;
using UnityEngine;

using brainflow;
using UnityEditor.SceneManagement;

public class CarBCIReader : MonoBehaviour
{
    private const string SerialPort = "COM7";
    private const int BoardID = 0;
    private static double _determinedBoardSampleRefreshTime = 0.5;
    private static double[] _calibrationThresholds = Array.Empty<double>();
    private static double[] _calibrationAccuracies = Array.Empty<double>();
    private const int NumSamplesPerInput = 20;
    private static int[] _eegChannels;
    
    private static BoardShim _boardShim;

    private static float _elapsedSinceInput;

    private static bool[] _inputs = Array.Empty<bool>();

    public int leftSteeringInput = 2;
    public int rightSteeringInput = 1;
    public int forwardsAcclInput = 0;
    public int backwardsAcclInput = 3;

    private enum Stages
    {
        Sleep = 0,
        CalibrationRefresh = 1,
        CalibrationSensitivity = 2,
        Play = 3
    }

    private Stages stage = Stages.Sleep;

    // Start is called before the first frame update
    private void Start()
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

        _eegChannels = BoardShim.get_exg_channels(BoardID);
        _inputs = new bool[_eegChannels.Length];

        Debug.Log("OpenBCI initialization complete");
    }

    // Update is called once per frame
    private void Update()
    {
        _elapsedSinceInput += Time.deltaTime;
        if (stage == Stages.Sleep)
        {
            if (_elapsedSinceInput > 5)
            {
                stage = Stages.CalibrationRefresh;
                _elapsedSinceInput = 0;
            }
        }
        if (stage == Stages.CalibrationRefresh)
        {
            var progress = CalibrateSampleUpdateRate();
            if (Math.Abs(progress - 1) < 0.01)
            {
                PrintCalibrationRefreshStatus();
                stage = Stages.CalibrationSensitivity;
            }
        }
        if (stage == Stages.CalibrationSensitivity)
        {
            var progress = CalibrateSensitivity();
            if (Math.Abs(progress - 1) < 0.01)
            {
                stage = Stages.Play;
            }
        }
        if (stage == Stages.Play)
        {
            if (_elapsedSinceInput > 0.2)
            {
                _elapsedSinceInput = 0;
                OncePerSecond();
            }
        }
    }

    private double[,] beforeTest;

    private double CalibrateSampleUpdateRate()
    {
        if (beforeTest == null)
        {
            beforeTest = GetRawData();
            _elapsedSinceInput = 0;
            return 0;
        }
        else
        {
            // we are trying to figure out how often the buffer is updated by the board
            var afterTest = GetRawData();
            var differences = 0;
            // Debug.Log("UHOH " + beforeTest.Length);
            // Debug.Log("BIG UHOH IF 0 " + afterTest.Length);
            for (var i = 0; i < NumSamplesPerInput; i++)
            {
                try
                {
                    if (beforeTest[0, i] - afterTest[0, i] > 0.01 && afterTest[0, i] - beforeTest[0, i] > 0.01)
                        differences++;
                } catch (IndexOutOfRangeException e)
                {
                    Debug.Log(i);
                }
                
            }
                
            if (differences == NumSamplesPerInput)
            {
                _determinedBoardSampleRefreshTime = _elapsedSinceInput;
                return 1;
            }
            Debug.Log("Progress (percent): " + ((double) differences / NumSamplesPerInput));
            return 1;
            return (double) differences / NumSamplesPerInput;
        }
    }

    private static void PrintCalibrationRefreshStatus()
    {
        var framesLag = (int) Mathf.Floor((float) _determinedBoardSampleRefreshTime * 30);
        switch (framesLag)
        {
            case 0:
                Debug.Log("Refresh rate: UNREAL.");
                break;
            case 1:
                Debug.Log("Refresh rate: VERY GOOD.");
                break;
            case 2:
                Debug.Log("Refresh rate: GOOD.");
                break;
            case 3:
            case 4:
            case 5:
                Debug.Log("Refresh rate: ACCEPTABLE.");
                break;
            default:
                if (framesLag < 10) Debug.Log("Refresh rate: UNCOMFORTABLE.");
                else if (framesLag < 15) Debug.Log("Refresh rate: NOTICEABLE.");
                else if (framesLag < 30) Debug.Log("Refresh rate: TERRIBLE.");
                else Debug.Log("Refresh rate: ABYSMAL.");
                break;
        }
        Debug.Log("Runs with less than " + framesLag + " frames input lag at 30fps.");
    }
    
    private enum CalibrationStage
    {
        Preface = 0,
        InitialRelaxed = 1,
        InitialFlexed = 2,
        SecondaryRelaxed = 3,
        SecondaryFlexed = 4,
        Calculation = 5
    }

    private CalibrationStage calibrationStage = CalibrationStage.Preface;

    private List<double>[] relaxedReadings;
    private List<double>[] flexedReadings;
    
    double CalibrateSensitivity()
    {
        switch (calibrationStage)
        {
            case CalibrationStage.Preface:
                Debug.Log("We will now calibrate our signals to tell when you are flexing.");
                Debug.Log("Please relax your muscle.");
                _elapsedSinceInput = 0;
                calibrationStage = CalibrationStage.InitialRelaxed;
                relaxedReadings = new List<double>[_eegChannels.Length];
                flexedReadings = new List<double>[_eegChannels.Length];
                for (var c = 0; c < _eegChannels.Length; c++)
                {
                    relaxedReadings[c] = new List<double>();
                    flexedReadings[c] = new List<double>();
                }

                return 0;
            
            case CalibrationStage.InitialRelaxed:
                if (_elapsedSinceInput < 3) {
                    // user is still reading the directions to relax
                }
                else if (_elapsedSinceInput < 6)
                {
                    // user is relaxed, accept input
                    var readings = GetReadings();
                    for (var c = 0; c < _eegChannels.Length; c++) relaxedReadings[c].Add(readings[c]);
                }
                else
                {
                    // readings complete, move on
                    Debug.Log("Please flex your muscle for about 10 seconds, until your are told otherwise. Do not overdo it, just a firm flex.");
                    _elapsedSinceInput = 0;
                    calibrationStage = CalibrationStage.InitialFlexed;
                    return 0.2;
                }

                return (_elapsedSinceInput / 6) / 5;
            
            case CalibrationStage.InitialFlexed:
                if (_elapsedSinceInput < 3)
                {
                    // user is still reading the directions to flex
                }
                else if (_elapsedSinceInput < 6)
                {
                    // user is flexed, accept input
                    var readings = GetReadings();
                    for (var c = 0; c < _eegChannels.Length; c++) flexedReadings[c].Add(readings[c]);
                }
                else
                {
                    // readings complete, move on
                    Debug.Log("Please relax your muscle.");
                    _elapsedSinceInput = 0;
                    calibrationStage = CalibrationStage.SecondaryRelaxed;
                    return 0.4;
                }
                
                return (_elapsedSinceInput / 6) / 5 + 0.2;
            
            case CalibrationStage.SecondaryRelaxed:
                if (_elapsedSinceInput < 3) {
                    // user is still reading the directions to relax
                }
                else if (_elapsedSinceInput < 6)
                {
                    // user is relaxed, accept input
                    var readings = GetReadings();
                    for (var c = 0; c < _eegChannels.Length; c++) relaxedReadings[c].Add(readings[c]);
                }
                else
                {
                    // readings complete, move on
                    Debug.Log("Please flex your muscle for about 10 seconds, until your are told otherwise. Do not overdo it, just a firm flex.");
                    _elapsedSinceInput = 0;
                    calibrationStage = CalibrationStage.SecondaryFlexed;
                    return 0.6;
                }
                
                return (_elapsedSinceInput / 6) / 5 + 0.4;
            
            case CalibrationStage.SecondaryFlexed:
                if (_elapsedSinceInput < 3)
                {
                    // user is still reading the directions to flex
                }
                else if (_elapsedSinceInput < 6)
                {
                    // user is flexed, accept input
                    var readings = GetReadings();
                    for (var c = 0; c < _eegChannels.Length; c++) flexedReadings[c].Add(readings[c]);
                    Debug.Log("FLEXYYXYXYXYXYX");
                }
                else
                {
                    // readings complete, move on
                    Debug.Log("Calculating accuracy, please wait...");
                    _elapsedSinceInput = 0;
                    calibrationStage = CalibrationStage.Calculation;
                    return 0.8;
                }
                
                return (_elapsedSinceInput / 6) / 5 + 0.6;
            case CalibrationStage.Calculation:

                var lowestSureFlex = new double[_eegChannels.Length];
                for (var c = 0; c < _eegChannels.Length; c++) lowestSureFlex[c] = flexedReadings[c][0];

                for (var c = 0; c < _eegChannels.Length; c++)
                {
                    for (var i = 0; i < flexedReadings.Length; i++)
                        if (flexedReadings[c][i] > lowestSureFlex[c])
                        {
                            lowestSureFlex[c] = flexedReadings[c][i];
                        }
                }
                
                var highestSureRelaxed = new double[_eegChannels.Length];
                for (var c = 0; c < _eegChannels.Length; c++) highestSureRelaxed[c] = relaxedReadings[c][0];
                
                for (var c = 0; c < _eegChannels.Length; c++)
                {
                    for (var i = 0; i < relaxedReadings.Length; i++)
                        if (relaxedReadings[c][i] > highestSureRelaxed[c])
                        {
                            highestSureRelaxed[c] = relaxedReadings[c][i];
                        }
                }

                // at this point, lowestSureFlex and highestSureRelaxed are near the middle of our scale
                // we need to choose a threshold number near these two numbers that effectively splits the two
                _calibrationThresholds = new double[_eegChannels.Length];
                _calibrationAccuracies = new double[_eegChannels.Length];
                for (var c = 0; c < _eegChannels.Length; c++)
                {
                    double lowest, highest;
                    if (lowestSureFlex[c] < highestSureRelaxed[c])
                    {
                        lowest = lowestSureFlex[c];
                        highest = highestSureRelaxed[c];
                    }
                    else
                    {
                        highest = lowestSureFlex[c];
                        lowest = highestSureRelaxed[c];
                    }
                    // binary search for best option
                    double threshold;
                    while (true)
                    {
                        threshold = (lowest + highest) / 2;
                        int failsRelaxed = 0, failsFlexed = 0;
                        for (var i = 0; i < relaxedReadings[c].Count; i++)
                        {
                            if (relaxedReadings[c][i] > threshold) failsRelaxed++;
                        }
                        for (var i = 0; i < flexedReadings[c].Count; i++)
                        {
                            if (flexedReadings[c][i] < threshold) failsFlexed++;
                        }

                        if (Math.Abs(highest - lowest) < 0.0000000001)
                        {
                            // this is the best model we are gonna get
                            break;
                        }
                        if (failsFlexed > failsRelaxed)
                        {
                            // a lot of flexed numbers appear not flexed in this model, lower threshold
                            highest = threshold;
                        }
                        else
                        {
                            // a lot of not flexed numbers appear flexed in this model, increase threshold
                            lowest = threshold;
                        }
                    }
                    // threshold now holds the optimum split value; calculate its accuracy
                    _calibrationThresholds[c] = threshold;
                    var possibleTests = 0;
                    var successes = 0;
                    for (var i = 0; i < flexedReadings[c].Count; i++)
                    {
                        possibleTests++;
                        if (flexedReadings[c][i] > threshold) successes++;
                    }
                    for (var i = 0; i < relaxedReadings[c].Count; i++)
                    {
                        possibleTests++;
                        if (relaxedReadings[c][i] < threshold) successes++;
                    }

                    var accuracy = successes / (float) possibleTests;

                    _calibrationAccuracies[c] = accuracy;
                    
                    Debug.Log("Accuracy for channel " + c + " is " + (accuracy*100) + " %");
                }

                return 1;
        }
        return 0;
    }
    
    void OncePerSecond()
    {
        var averages = GetReadings();

        for (var c = 0; c < _eegChannels.Length; c++)
        {
            _inputs[c] = averages[c] > _calibrationThresholds[c];
        }
    }

    private double[] GetReadings()
    {
        // Debug.Log("-----");

        var unprocessedData = GetRawData();

        // var filtered = new double[_eegChannels.Length][];
        //
        // for (var i = 0; i < _eegChannels.Length; i++)
        // {
        //     filtered[i] = DataFilter.perform_bandpass(unprocessedData.GetRow(_eegChannels[i]), BoardShim.get_sampling_rate(BoardID), 51.0, 4.0, 2, (int)FilterTypes.BUTTERWORTH, 0);
        //     /*filtered[i] = DataFilter.perform_bandpass(filtered[i], BoardShim.get_sampling_rate(board_id), 51.0, 100.0, 2, (int)FilterTypes.BUTTERWORTH, 0);
        //     filtered[i] = DataFilter.perform_bandpass(filtered[i], BoardShim.get_sampling_rate(board_id), 50.0, 4.0, 2, (int)FilterTypes.BUTTERWORTH, 0);
        //     filtered[i] = DataFilter.perform_bandpass(filtered[i], BoardShim.get_sampling_rate(board_id), 60.0, 4.0, 2, (int)FilterTypes.BUTTERWORTH, 0);*/
        // }

        var averages = new double[_eegChannels.Length];

        for (var j = 0; j < _eegChannels.Length; j++)
        {
            double avg = 0;
            for (var i = 0; i > unprocessedData.GetLength(1); i++)
            {
                double thisReading = Mathf.Abs((float) unprocessedData[j,i]);
                if (thisReading > avg) avg = thisReading; // take absolute max instead of average
                // avg += Mathf.Abs((float)filtered[j][i]);
            }
            averages[j] = avg;
            // Debug.Log(j + ":" + avg);
        }

        return averages;
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

    public static bool GetInput(int num)
    {
        return num > 0 && num < _inputs.Length && _inputs[num];
    }

    public int GetAxis(String s)
    {
        if (stage == Stages.Play) Debug.Log("INPUT: " + GetInput(0));
        if (s.Equals("Horizontal"))
        {
            if (GetInput(leftSteeringInput) == GetInput(rightSteeringInput)) return 0;
            return (GetInput(leftSteeringInput) ? -1 : 1);
        } else if (s.Equals("Vertical"))
        {
            if (GetInput(backwardsAcclInput) == GetInput(forwardsAcclInput)) return 0;
            return (GetInput(backwardsAcclInput) ? -1 : 1);
        }
        else return 0;
    }

    private void OnDestroy()
    {
        _boardShim.stop_stream();
        _boardShim.release_session();
    }
}
