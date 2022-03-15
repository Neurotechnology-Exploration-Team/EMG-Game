using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBCIReaderDummy : MonoBehaviour, OpenBCIReaderI
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public OpenBCIReaderI.ConnectionStatus GetConnectionStatus()
    {
        throw new System.NotImplementedException();
    }

    public int GetNumChannels()
    {
        throw new System.NotImplementedException();
    }

    public void SetAllowWifi(bool allowWifi)
    {
        throw new System.NotImplementedException();
    }

    public bool GetAllowWifi()
    {
        throw new System.NotImplementedException();
    }

    public void SetWifiBoardName(string name)
    {
        throw new System.NotImplementedException();
    }

    public string GetWifiBoardName()
    {
        throw new System.NotImplementedException();
    }

    public void SetDefaultSerialPort(string port)
    {
        throw new System.NotImplementedException();
    }

    public string GetDefaultSerialPort()
    {
        throw new System.NotImplementedException();
    }

    public void SetVerbose(bool verbose)
    {
        throw new System.NotImplementedException();
    }

    public bool GetVerbose()
    {
        throw new System.NotImplementedException();
    }

    public void SetThreshold(int channel, double threshold)
    {
        throw new System.NotImplementedException();
    }

    public void AutoRestingThreshold(int channel)
    {
        throw new System.NotImplementedException();
    }

    public void AutoRestingThreshold()
    {
        throw new System.NotImplementedException();
    }

    public void SetThresholdType(int channel, OpenBCIReaderI.ThresholdType thresholdType)
    {
        throw new System.NotImplementedException();
    }

    public void SetThresholdType(OpenBCIReaderI.ThresholdType thresholdType)
    {
        throw new System.NotImplementedException();
    }

    public void SetThresholdSensitivity(int channel, int sensitivity)
    {
        throw new System.NotImplementedException();
    }

    public void SetThresholdSensitivity(int sensitivity)
    {
        throw new System.NotImplementedException();
    }

    public bool GetInput(int channel)
    {
        throw new System.NotImplementedException();
    }

    public double GetNumericInput(int channel)
    {
        throw new System.NotImplementedException();
    }

    public void Disconnect()
    {
        throw new System.NotImplementedException();
    }

    public void Reconnect()
    {
        throw new System.NotImplementedException();
    }
}
