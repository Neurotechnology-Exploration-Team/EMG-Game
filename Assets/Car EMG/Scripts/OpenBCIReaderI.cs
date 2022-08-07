/// <summary>
/// An interface representing the functionality of a class that can interact with BCI hardware as input and provide
/// standard game input values as its output. Only ONE instance of this interface should exist at any one time.
/// </summary>
public interface OpenBCIReaderI
{
    public enum ThresholdType
    {
        /// <summary>Use the mean value of a number of recent input samples. This is usually the best option.
        /// Sensitivity is usually best around 200-500; increase sensitivity value for more stable results,
        /// decrease sensitivity value for better latency.</summary>
        Average,
        /// <summary>Use the absolute maximum value of a number of recent input samples. This is usually a decent option,
        /// though may have higher input latency than ThresholdType.Average. Try this if ThresholdType.Average
        /// isn't working well. Sensitivity value is usually best in a bit lower range than ThresholdType.Average,
        /// most of the time between 50 and 200. Increase sensitivity value for more stable results but
        /// significantly higher latency, decrease sensitivity value for better latency but more fluctuation.</summary>
        Max,
        /// <summary>Use the value of the last input sample. This is almost never the best option, and should be used
        /// purely for numerical testing to measure time between change in input and ensure that the connection is working.
        /// The sensitivity value has no effect on this ThresholdType.</summary>
        Last
    }
    public enum ConnectionStatus
    {
        /// <summary>
        /// There is no current connection.
        /// </summary>
        Disconnected,
        /// <summary>
        /// The program is attempting to connect to the board. The process should not be aborted if in this state.
        /// </summary>
        Connecting,
        /// <summary>
        /// The program is connected to the board and receiving valid data.
        /// </summary>
        Connected,
        /// <summary>
        /// The program is attempting to disconnect and reconnect to the board. The process should not be aborted if in this state.
        /// </summary>
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
    ///     for (int channel = 0; channel &lt; bci.GetNumChannels(); channel++) {
    ///         bool i = bci.GetInput(channel);
    ///         ...
    ///     }
    /// }
    /// 
    /// </code>
    /// </example>
    public int? GetNumChannels();

    /// <summary>
    /// Set whether or not the program should be allowed to use wifi to connect to the board.
    ///
    /// Note: turning this on may add a significant amount of time to reconnection delay
    /// </summary>
    /// <param name="allowWifi">Whether or not the program should be allowed to use wifi to connect to the board.</param>
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
    public void SetAllowWifi(bool allowWifi);
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
    /// Only use this if you know what you're doing.
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

    /// <summary>
    /// Set whether or not the program prints every message with debug info or just important ones.
    /// If true, program will print a lot of debug information
    /// If false, program will print only critical info such as whether or not the board has connected
    /// </summary>
    /// <param name="verbose">Whether or not the program prints every message with debug info or just important ones.</param>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// if (inDebugMode) {
    ///     bci.SetVerbose(true);
    /// } else {
    ///     bci.SetVerbose(false);
    /// }
    /// </code>
    /// </example>
    public void SetVerbose(bool verbose);
    /// <summary>
    /// Get whether or not the program prints every message or just important ones.
    /// If true, program will print a lot of debug information
    /// If false, program will print only critical info such as whether or not the board has connected
    /// </summary>
    /// <returns>Whether or not the program prints every message with debug info or just important ones.</returns>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// if (bci.GetVerbose()) {
    ///     Debug.Log("Extra debug message");
    /// } else {
    ///     Debug.Log("Important message");
    /// }
    /// </code>
    /// </example>
    public bool GetVerbose();

    /// <summary>
    /// Set the numerical threshold for a certain channel, in nanovolts. Only use this if you know what you're doing.
    /// See GetNumChannels() for the number of supported channels on the current device
    /// See GetNumericInput() for current nanovolt measurements.
    /// </summary>
    /// <param name="channel">The 0-indexed channel number</param>
    /// <param name="threshold">The threshold to set, in nanovolts</param>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// int channelCount = OpenBCIReaderI.GetNumChannels();
    ///
    /// for (int i = 0; i &lt; channelCount; i++) {
    ///     bci.SetThreshold(i, 0);
    /// }
    /// </code>
    /// </example>
    public void SetThreshold(int channel, double threshold);
    /// <summary>
    /// Sets the threshold for the specified channel so that the current state is interpreted as resting.
    ///
    /// This method is often called with no arguments when the game starts up for calibration;
    /// see instructions below.
    /// 
    /// If input becomes unreliable for a certain muscle, instruct the user to rest/relax the muscle,
    /// wait a couple seconds, then call this function with the bad channel. If all input becomes unreliable,
    /// call with no arguments to reset all channels so that the current input is interpreted as resting.
    /// </summary>
    /// <param name="channel">The channel whose threshold should be reset.</param>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    /// int badChannel = 4;
    ///
    /// int channelCount = bci.GetNumChannels();
    ///
    /// if (badChannel &lt; channelCount) {
    ///     bci.AutoRestingThreshold(badChannel);
    /// }
    /// </code>
    /// </example>
    public void AutoRestingThreshold(int channel);
    /// <summary>
    /// Sets the threshold for all channels so that the current state is interpreted as resting.
    ///
    /// This method is often called with no arguments when the game starts up for calibration;
    /// see instructions below.
    /// 
    /// If input becomes unreliable, instruct the user to rest/relax all muscles,
    /// wait a couple seconds, then call this function. If only one muscle is acting up, call this method
    /// with that muscle as the argument to reset the threshold for only that channel.
    /// </summary>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// bci.AutoRestingThreshold();
    /// </code>
    /// </example>
    public void AutoRestingThreshold();
    /// <summary>
    /// Get the current threshold for the specified channel.
    /// </summary>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// double x = bci.GetThreshold(0);
    /// </code>
    /// </example>
    /// <returns></returns>
    public double GetThreshold(int channel);

    /// <summary>
    /// Sets the way input is measured for a specific channel. Usually ThresholdType.Average or ThresholdType.Max
    ///
    /// If changed, you should also recalibrate with AutoRestingThreshold();
    ///
    /// <see cref="ThresholdType"/>
    /// <see cref="SetThresholdSensitivity(int,double)"/>
    ///
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><code>ThresholdType.Average</code></term>
    /// <description>Use the mean value of a number of recent input samples. This is usually the best option.
    /// Sensitivity is usually best around 200-500; increase sensitivity value for more stable results,
    /// decrease sensitivity value for better latency.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Max</code></term>
    /// <description>Use the absolute maximum value of a number of recent input samples. This is usually a decent option,
    /// though may have higher input latency than ThresholdType.Average. Try this if ThresholdType.Average
    /// isn't working well. Sensitivity value is usually best in a bit lower range than ThresholdType.Average,
    /// most of the time between 50 and 200. Increase sensitivity value for more stable results but
    /// significantly higher latency, decrease sensitivity value for better latency but more fluctuation.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Last</code></term>
    /// <description>Use the value of the last input sample. This is almost never the best option, and should be used
    /// purely for numerical testing to measure time between change in input and ensure that the connection is working.
    /// The sensitivity value has no effect on this ThresholdType.</description>
    /// </item>
    /// </list>
    /// 
    /// </summary>
    /// <param name="channel">The channel whose threshold type to set</param>
    /// <param name="thresholdType">The ThresholdType to use with the specified channel</param>
    ///
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    /// int testChannel = 4;
    /// int newSensitivity = 300;
    /// 
    /// int channelCount = bci.GetNumChannels();
    ///
    /// if (testChannel &lt; channelCount) {
    ///     bci.SetThresholdType(testChannel, ThresholdType.Max);
    ///     bci.SetThresholdSensitivity(testChannel, newSensitivity);
    /// }
    /// </code>
    /// </example>
    public void SetThresholdType(int channel, ThresholdType thresholdType);
    /// <summary>
    /// Sets the way input is measured for all channels. Usually ThresholdType.Average or ThresholdType.Max
    ///
    /// If changed, you should also recalibrate with AutoRestingThreshold();
    ///
    /// <see cref="ThresholdType"/>
    /// <see cref="SetThresholdSensitivity(int,double)"/>
    ///
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><code>ThresholdType.Average</code></term>
    /// <description>Use the mean value of a number of recent input samples. This is usually the best option.
    /// Sensitivity is usually best around 200-500; increase sensitivity value for more stable results,
    /// decrease sensitivity value for better latency.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Max</code></term>
    /// <description>Use the absolute maximum value of a number of recent input samples. This is usually a decent option,
    /// though may have higher input latency than ThresholdType.Average. Try this if ThresholdType.Average
    /// isn't working well. Sensitivity value is usually best in a bit lower range than ThresholdType.Average,
    /// most of the time between 50 and 200. Increase sensitivity value for more stable results but
    /// significantly higher latency, decrease sensitivity value for better latency but more fluctuation.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Last</code></term>
    /// <description>Use the value of the last input sample. This is almost never the best option, and should be used
    /// purely for numerical testing to measure time between change in input and ensure that the connection is working.
    /// The sensitivity value has no effect on this ThresholdType.</description>
    /// </item>
    /// </list>
    /// 
    /// </summary>
    /// <param name="thresholdType">The ThresholdType to use with all channels</param>
    ///
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    /// int newSensitivity = 300;
    ///
    /// bci.SetThresholdType(ThresholdType.Max);
    /// bci.SetThresholdSensitivity(newSensitivity);
    /// </code>
    /// </example>
    public void SetThresholdType(ThresholdType thresholdType);
    /// <summary>
    /// Sets the sensitivity for a certain channel. Usually, lower sensitivity values are MORE sensitive.
    /// See table for more information on what values are appropriate.
    /// Due to the way samples are collected, floating point values have no significance, so only integers
    /// are accepted.
    ///
    /// <see cref="ThresholdType"/>
    /// <see cref="SetThresholdType(int,OpenBCIReaderI.ThresholdType)"/>
    ///
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><code>ThresholdType.Average</code></term>
    /// <description>Use the mean value of a number of recent input samples. This is usually the best option.
    /// Sensitivity is usually best around 200-500; increase sensitivity value for more stable results,
    /// decrease sensitivity value for better latency.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Max</code></term>
    /// <description>Use the absolute maximum value of a number of recent input samples. This is usually a decent option,
    /// though may have higher input latency than ThresholdType.Average. Try this if ThresholdType.Average
    /// isn't working well. Sensitivity value is usually best in a bit lower range than ThresholdType.Average,
    /// most of the time between 50 and 200. Increase sensitivity value for more stable results but
    /// significantly higher latency, decrease sensitivity value for better latency but more fluctuation.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Last</code></term>
    /// <description>Use the value of the last input sample. This is almost never the best option, and should be used
    /// purely for numerical testing to measure time between change in input and ensure that the connection is working.
    /// The sensitivity value has no effect on this ThresholdType.</description>
    /// </item>
    /// </list>
    /// 
    /// </summary>
    /// <param name="channel">The channel whose sensitivity should be set</param>
    /// <param name="sensitivity">The new sensitivity value</param>
    /// 
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    /// int testChannel = 4;
    /// int newSensitivity = 300;
    /// 
    /// int channelCount = bci.GetNumChannels();
    ///
    /// if (testChannel &lt; channelCount) {
    ///     bci.SetThresholdSensitivity(testChannel, newSensitivity);
    /// }
    /// </code>
    /// </example>
    public void SetThresholdSensitivity(int channel, int sensitivity);
    /// <summary>
    /// Sets the sensitivity for all channels. Usually, lower sensitivity values are MORE sensitive.
    /// See table for more information on what values are appropriate.
    /// Due to the way samples are collected, floating point values have no significance, so only integers
    /// are accepted.
    ///
    /// <see cref="ThresholdType"/>
    /// <see cref="SetThresholdType(OpenBCIReaderI.ThresholdType)"/>
    ///
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><code>ThresholdType.Average</code></term>
    /// <description>Use the mean value of a number of recent input samples. This is usually the best option.
    /// Sensitivity is usually best around 200-500; increase sensitivity value for more stable results,
    /// decrease sensitivity value for better latency.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Max</code></term>
    /// <description>Use the absolute maximum value of a number of recent input samples. This is usually a decent option,
    /// though may have higher input latency than ThresholdType.Average. Try this if ThresholdType.Average
    /// isn't working well. Sensitivity value is usually best in a bit lower range than ThresholdType.Average,
    /// most of the time between 50 and 200. Increase sensitivity value for more stable results but
    /// significantly higher latency, decrease sensitivity value for better latency but more fluctuation.</description>
    /// </item>
    /// <item>
    /// <term><code>ThresholdType.Last</code></term>
    /// <description>Use the value of the last input sample. This is almost never the best option, and should be used
    /// purely for numerical testing to measure time between change in input and ensure that the connection is working.
    /// The sensitivity value has no effect on this ThresholdType.</description>
    /// </item>
    /// </list>
    /// 
    /// </summary>
    /// <param name="sensitivity">The new sensitivity value</param>
    /// 
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    /// int newSensitivity = 300;
    ///
    /// bci.SetThresholdSensitivity(newSensitivity);
    /// </code>
    /// </example>
    public void SetThresholdSensitivity(int sensitivity);

    /// <summary>
    /// Returns the boolean input of whether or not the user is flexing this muscle.
    /// </summary>
    /// <param name="channel">The muscle to measure input from</param>
    /// <returns>The boolean input of whether or not the user is flexing this muscle</returns>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// int channelCount = bci.GetNumChannels();
    ///
    /// for (int i = 0; i &lt; channelCount; i++) {
    ///     if (bci.GetInput(i)) {
    ///         // do something in-game that was waiting for this input
    ///     }
    /// }
    /// </code>
    /// </example>
    public bool GetInput(int channel);
    /// <summary>
    /// Returns the current numeric input, in nanovolts, for the specified channel. Only use this if you know
    /// what you're doing. This is the value after the calculation determined by the channel's ThresholdType.
    /// </summary>
    /// <param name="channel">The channel to measure input from</param>
    /// <returns>The current numeric input, in nanovolts, for the specified channel</returns>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// int channelCount = bci.GetNumChannels();
    ///
    /// for (int i = 0; i &lt; channelCount; i++) {
    ///     double nanovolts = bci.GetNumericInput(i);
    ///     ...;
    /// }
    /// </code>
    /// </example>
    public double GetNumericInput(int channel);

    /// <summary>
    /// Disconnect the currently connected BCI board, if one is connected. Do not attempt to reconnect.
    /// </summary>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// bci.Disconnect();
    /// </code>
    /// </example>
    public void Disconnect();
    /// <summary>
    /// Attempt to reconnect to a BCI board. Will first disconnect the currently connected BCI board if one is connected.
    /// </summary>
    /// <example>
    /// <code>
    /// OpenBCIReaderI bci = ...;
    ///
    /// bci.Reconnect();
    /// </code>
    /// </example>
    public void Reconnect();
}
