public interface OpenBCIReaderI
{
    public enum ThresholdType
    {
        Average,
        Max,
        Last
    }
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }
    /// <summary>
    /// Returns the current status of the connection with the Open BCI board
    /// </summary>
    /// <returns>
    /// <list type="table">
    /// <listheader>
    /// <term>Type</term>
    /// <description>Condition</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ConnectionStatus.Disconnected"/></term>
    /// <description>There is no current connection.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ConnectionStatus.Connecting"/></term>
    /// <description>The program is attempting to connect to the board. The process should not be aborted if in this state.</description>
    /// </item>
    /// </list>
    /// </returns>
    public ConnectionStatus GetConnectionStatus();
    public int GetNumChannels();

    public void SetAllowWifi();
    public bool GetAllowWifi();
    public void SetWifiBoardName(string name);
    public string GetWifiBoardName();
    public void SetDefaultSerialPort(string port);
    public string SetDefaultSerialPort();

    public void SetVerbose();
    public bool GetVerbose();

    public void SetThreshold(int channel, double threshold);
    public void AutoRestingThreshold(int channel);
    public void AutoRestingThreshold();

    public void SetThresholdType(int channel, ThresholdType thresholdType);
    public void SetThresholdType(ThresholdType thresholdType);
    public void SetThresholdSensitivity(int channel, double sensitivity);
    public void SetThresholdSensitivity(double sensitivity);

    public bool GetInput(int channel);
    public double GetNumericInput(int channel);

    public void Disconnect();
    public void Reconnect();
}
