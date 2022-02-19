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
    /// 
    /// <returns>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Condition</description>
    /// </listheader>
    /// <item>
    /// <term><code>ConnectionStatus.Disconnected</code></term>
    /// <description>There is no current connection.</description>
    /// </item>
    /// <item>
    /// <term><code>ConnectionStatus.Connecting</code></term>
    /// <description>The program is attempting to connect to the board. The process should not be aborted if in this state.</description>
    /// </item>
    /// <item>
    /// <term><code>ConnectionStatus.Connected</code></term>
    /// <description>The program is connected to the board and receiving valid data.</description>
    /// </item>
    /// </list>
    /// </returns>
    ///
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    /// 
    /// switch (bci.GetConnectionStatus()) {
    ///     case OpenBCIReaderI.ConnectionStatus.Disconnected:
    ///         bci.Reconnect();
    ///         break;
    ///     case OpenBCIReaderI.ConnectionStatus.Connecting:
    ///         Debug.Log("Please wait!");
    ///         break;
    ///     case OpenBCIReaderI.ConnectionStatus.Connected:
    ///         bool i = bci.GetInput(2);
    ///         ...
    ///         break;
    /// }
    /// </code>
    /// </example>
    public ConnectionStatus GetConnectionStatus();
    /// <summary>
    /// Returns the number of channels in the connected device, or null if no device is connected
    /// </summary>
    /// <returns>The number of channels in the connected device, or null if no device is connected</returns>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// if (bci.GetConnectionStatus() == OpenBCIReaderI.ConnectionStatus.Connected) {
    ///     for (int channel = 0; channel < bci.GetNumChannels(); channel++) {
    ///         bool i = bci.GetInput(channel);
    ///         ...
    ///     }
    /// }
    /// 
    /// </code>
    /// </example>
    public int GetNumChannels();

    /// <summary>
    /// Set whether or not the program should be allowed to use wifi to connect to the board.
    ///
    /// Note: turning this on may add a significant amount of time to reconnection delay
    /// </summary>
    ///
    /// <example>
    /// <code>
    ///
    /// OpenBCIReaderI bci = ...;
    ///
    /// if (usingWifiShield) {
    ///     bci.SetAllowWifi(true);
    /// } else {
    ///     bci.SetAllowWifi(false);
    /// }
    ///
    /// </code>
    /// </example>
    public void SetAllowWifi();
    /// <summary>
    /// Get whether or not the program should be allowed to use wifi to connect to the board.
    ///
    /// Note: Enabling this on may add a significant amount of time to reconnection delay
    /// </summary>
    /// <returns>Whether or not the program should be allowed to use wifi to connect to the board.</returns>
    ///
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// if (bci.GetAllowWifi()) {
    ///     ...
    /// } else {
    ///     ...
    /// }
    /// </code>
    /// </example>
    public bool GetAllowWifi();
    /// <summary>
    /// Set the name of the wifi shield used with the board.
    /// Similar to a serial number, different for each board.
    /// Should be similar to "OpenBCI-XXXX"
    /// </summary>
    /// <param name="name">The name of the Wifi shield</param>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// bci.SetAllowWifi(true);
    /// bci.SetWifiBoardName("OpenBCI-XXXX");
    /// bci.Reconnect();
    /// </code>
    /// </example>
    public void SetWifiBoardName(string name);
    /// <summary>
    /// Get the name of the wifi shield used with the board.
    /// Similar to a serial number, different for each board.
    /// Should be similar to "OpenBCI-XXXX"
    /// </summary>
    /// <returns>The name of the Wifi shield</returns>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// string boardName = bci.GetWifiBoardName();
    /// </code>
    /// </example>
    public string GetWifiBoardName();
    /// <summary>
    /// Set the default Bluetooth dongle serial port for Cyton dongles.
    /// Not necessary to be able to use dongles, but may improve connection delays.
    /// Similar to "COMX", where X is an integer from 1 to 9.
    ///
    /// In Windows, see device manager for serial port list.
    /// </summary>
    /// <param name="port">The name of the serial port where the Dongle is plugged into.</param>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// bci.SetDefaultSerialPort("COM6");
    /// </code>
    /// </example>
    public void SetDefaultSerialPort(string port);
    /// <summary>
    /// Get the default Bluetooth dongle serial port for Cyton dongles.
    /// Returns null if no default has been explicitly set.
    /// If null, the program will scan all available ports when reconnection is requested.
    /// </summary>
    /// <returns>The name of the serial port where the program expects to find the Cyton dongle.</returns>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// string serialPort = bci.GetDefaultSerialPort();
    /// </code>
    /// </example>
    public string GetDefaultSerialPort();

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
