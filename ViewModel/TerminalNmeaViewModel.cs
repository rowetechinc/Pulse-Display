/*
 * Copyright © 2013 
 * Rowe Technology Inc.
 * All rights reserved.
 * http://www.rowetechinc.com
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification is NOT permitted.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 * 
 * HISTORY
 * -----------------------------------------------------------------
 * Date            Initials    Version    Comments
 * -----------------------------------------------------------------
 * 08/07/2013      RC          3.0.7      Initial coding
 * 08/09/2013      RC          3.0.7      Added GpsSendEscCommand.
 * 01/31/2014      RC          3.2.3      Save the GPS options to the selected project.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/22/2014      RC          4.0.2      Added raw recording.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ReactiveUI;
    using System.Collections.ObjectModel;
    using Caliburn.Micro;
    using System.Collections;
    using System.Threading.Tasks;

    /// <summary>
    /// GPS Terminal to communicate with the GPS.
    /// </summary>
    public class TerminalNmeaViewModel : PulseViewModel
    {
        #region Enum and Classes

        /// <summary>
        /// All the types of Navigation Terminals.
        /// </summary>
        public enum TerminalNavType
        {
            /// <summary>
            /// GPS 1.
            /// </summary>
            GPS1,

            /// <summary>
            /// GPS 2.
            /// </summary>
            GPS2,

            /// <summary>
            /// NMEA 1.
            /// </summary>
            NMEA1,

            /// <summary>
            /// NMEA 2.
            /// </summary>
            NMEA2
        }

        #endregion

        #region Variables

        /// <summary>
        /// Singleton object to communication with the ADCP.
        /// </summary>
        private AdcpConnection _adcpConnection;

        /// <summary>
        /// Pulse Manager.
        /// </summary>
        private PulseManager _pm;

        /// <summary>
        /// Serial options for the GPS serial port.
        /// </summary>
        private SerialOptions _gpsSerialOptions;

        /// <summary>
        /// The type of terminal.
        /// </summary>
        private TerminalNavType _terminalType;

        #endregion

        #region Properties

        /// <summary>
        /// Title.
        /// </summary>
        private string _Title;
        /// <summary>
        /// Title.
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

        #region GPS Serial Port

        /// <summary>
        /// Flag if the GPS port is enabled.
        /// This will turn on or off the GPS port.
        /// </summary>
        public bool IsGpsSerialPortEnabled
        {
            get 
            {
                return GetIsGpsSerialPortEnabled();
            }
            set
            {
                // Set the value
                SetIsGpsSerialPortEnabled(value);
                this.NotifyOfPropertyChange(() => this.IsGpsSerialPortEnabled);

                if (value)
                {
                    // Reconnect the GPS serial port
                    ConnectSerialGps();
                }
                else
                {
                    DisconnectSerialGps();
                }
            }
        }

        #region Buffer

        /// <summary>
        /// Display the receive buffer from the connected GPS serial port.
        /// </summary>
        public string GpsReceiveBuffer
        {
            get
            {
                if (SerialPort() != null)
                {
                    return SerialPort().ReceiveBufferString;
                }

                return "";
            }
        }

        #endregion

        #region GPS Send Commands

        /// <summary>
        /// History of all the previous GPS commands.
        /// </summary>
        private ObservableCollection<string> _GpsCommandHistory;
        /// <summary>
        /// History of all the previous GPS commands.
        /// </summary>
        public IEnumerable GpsCommandHistory
        {
            get { return _GpsCommandHistory; }
        }

        /// <summary>
        /// Command currently selected for GPS.
        /// </summary>
        private string _SelectedGpsCommand;
        /// <summary>
        /// Command currently selected for GPS.
        /// </summary>
        public string SelectedGpsCommand
        {
            get { return _SelectedGpsCommand; }
            set
            {
                _SelectedGpsCommand = value;
                this.NotifyOfPropertyChange(() => this.SelectedGpsCommand);
                this.NotifyOfPropertyChange(() => this.NewGpsCommand);
            }
        }


        /// <summary>
        /// New GPS command entered by the user.
        /// This will be called when the user enters
        /// in a new command to send to the GPS.
        /// It will update the list and set the SelectedGpsCommand.
        /// </summary>
        public string NewGpsCommand
        {
            get { return _SelectedGpsCommand; }
            set
            {
                //if (_SelectedGpsCommand != null)
                //{
                //    return;
                //}
                if (!string.IsNullOrEmpty(value))
                {
                    _GpsCommandHistory.Insert(0, value);
                    SelectedGpsCommand = value;
                }
            }
        }

        #endregion

        #region GPS Serial Options

        private List<string> _gpsPortOptions;
        /// <summary>
        /// Get the GPS COMM port options.
        /// </summary>
        public List<string> GpsPortOptions
        {
            get { return _gpsPortOptions; }
            set
            {
                _gpsPortOptions = value;
                this.NotifyOfPropertyChange(() => this.GpsPortOptions);
            }
        }

        /// <summary>
        /// Store the COMM Port value as a string for the GPS.
        /// </summary>
        public string GpsPort
        {
            get 
            {
                return GetGpsCommPort();
            }
            set
            {
                SetGpsCommPort(value);
            }
        }

        /// <summary>
        /// Get the GPS Baud Rate options.
        /// </summary>
        public List<int> GpsBaudRateOptions
        {
            get { return SerialOptions.BaudRateOptions; }
        }

        /// <summary>
        /// Store the serial port baud rate as an int for the GPS.
        /// </summary>
        public int GpsBaudRate
        {
            get
            {
                return GetGpsBaudRate();
            }
            set
            {
                SetGpsBaudRate(value);
            }
        }

        ///// <summary>
        ///// Get the GPS Data Bit options.
        ///// </summary>
        //public List<int> GpsDataBitsOptions
        //{
        //    get { return SerialOptions.DataBitsOptions; }
        //}

        ///// <summary>
        ///// Store the serial port data bits as an int for the GPS.
        ///// </summary>
        //public int GpsDataBits
        //{
        //    get { return _gpsSerialOptions.DataBits; }
        //    set
        //    {
        //        _gpsSerialOptions.DataBits = value;
        //        this.NotifyOfPropertyChange(() => this.GpsDataBits);

        //        // Reconnect the GPS if conected
        //        if (IsGpsSerialPortEnabled)
        //        {
        //            ReconnectSerialGps();
        //        }

        //        // Save to database
        //        SaveSettingsToDb();
        //    }
        //}

        ///// <summary>
        ///// Get the GPS Parity options.
        ///// </summary>
        //public List<System.IO.Ports.Parity> GpsParityOptions
        //{
        //    get { return SerialOptions.ParityOptions; }
        //}

        ///// <summary>
        ///// Store the serial port _parity as a System.IO.Ports.Parity 
        ///// for the GPS.
        ///// </summary>
        //public System.IO.Ports.Parity GpsParity
        //{
        //    get { return _gpsSerialOptions.Parity; }
        //    set
        //    {
        //        _gpsSerialOptions.Parity = value;
        //        this.NotifyOfPropertyChange(() => this.GpsParity);

        //        // Reconnect the GPS if conected
        //        if (IsGpsSerialPortEnabled)
        //        {
        //            ReconnectSerialGps();
        //        }

        //        // Save to database
        //        SaveSettingsToDb();
        //    }
        //}

        ///// <summary>
        ///// Get the GPS Stop bit options.
        ///// </summary>
        //public List<System.IO.Ports.StopBits> GpsStopBitsOptions
        //{
        //    get { return SerialOptions.StopBitsOptions; }
        //}

        ///// <summary>
        ///// Store the serial port Stop bits as a System.IO.Ports.StopBits
        ///// for the GPS.
        ///// </summary>
        //public System.IO.Ports.StopBits GpsStopBits
        //{
        //    get { return _gpsSerialOptions.StopBits; }
        //    set
        //    {
        //        _gpsSerialOptions.StopBits = value;
        //        this.NotifyOfPropertyChange(() => this.GpsStopBits);

        //        // Reconnect the GPS if conected
        //        if (IsGpsSerialPortEnabled)
        //        {
        //            ReconnectSerialGps();
        //        }

        //        // Save to database
        //        //SaveSettingsToDb();
        //    }
        //}

        #endregion

        #endregion

        #region Record

        /// <summary>
        /// Turn on or off recording raw ADCP.
        /// </summary>
        public bool IsRawGpsRecording
        {
            get
            {
                switch(_terminalType)
                {
                    case TerminalNavType.GPS1:
                    default:
                        return _adcpConnection.IsRawGps1Recording;
                    case TerminalNavType.GPS2:
                        return _adcpConnection.IsRawGps2Recording;
                    case TerminalNavType.NMEA1:
                        return _adcpConnection.IsRawNmea1Recording;
                    case TerminalNavType.NMEA2:
                        return _adcpConnection.IsRawNmea2Recording;
                }
            }
            set
            {
                if (value)
                {
                    switch (_terminalType)
                    {
                        case TerminalNavType.GPS1:
                        default:
                            _adcpConnection.IsRawGps1Recording = value;
                            _adcpConnection.StartRawGps1Record(Pulse.Commons.DEFAULT_RECORD_DIR);
                            break;
                        case TerminalNavType.GPS2:
                            _adcpConnection.IsRawGps2Recording = value;
                            _adcpConnection.StartRawGps2Record(Pulse.Commons.DEFAULT_RECORD_DIR);
                            break;
                        case TerminalNavType.NMEA1:
                            _adcpConnection.IsRawNmea1Recording = value;
                            _adcpConnection.StartRawNmea1Record(Pulse.Commons.DEFAULT_RECORD_DIR);
                            break;
                        case TerminalNavType.NMEA2:
                            _adcpConnection.IsRawNmea2Recording = value;
                            _adcpConnection.StartRawNmea2Record(Pulse.Commons.DEFAULT_RECORD_DIR);
                            break;
                    }
                }
                else
                {
                    switch (_terminalType)
                    {
                        case TerminalNavType.GPS1:
                        default:
                            _adcpConnection.IsRawGps1Recording = value;
                            _adcpConnection.StopRawGps1Record();
                            break;
                        case TerminalNavType.GPS2:
                            _adcpConnection.IsRawGps2Recording = value;
                            _adcpConnection.StopRawGps2Record();
                            break;
                        case TerminalNavType.NMEA1:
                            _adcpConnection.IsRawNmea1Recording = value;
                            _adcpConnection.StopRawNmea1Record();
                            break;
                        case TerminalNavType.NMEA2:
                            _adcpConnection.IsRawNmea2Recording = value;
                            _adcpConnection.StopRawNmea2Record();
                            break;
                    }
                }

                this.NotifyOfPropertyChange(() => this.IsRawGpsRecording);
                this.NotifyOfPropertyChange(() => this.RawGpsBytesWritten);
                this.NotifyOfPropertyChange(() => this.RawGpsFileName);
            }
        }

        /// <summary>
        /// File name for the raw ADCP file.
        /// </summary>
        public string RawGpsFileName
        {
            get
            {
                switch (_terminalType)
                {
                    case TerminalNavType.GPS1:
                    default:
                        return _adcpConnection.RawGps1RecordFileName;
                    case TerminalNavType.GPS2:
                        return _adcpConnection.RawGps2RecordFileName;
                    case TerminalNavType.NMEA1:
                        return _adcpConnection.RawNmea1RecordFileName;
                    case TerminalNavType.NMEA2:
                        return _adcpConnection.RawNmea2RecordFileName;
                }
            }
        }

        /// <summary>
        /// Bytes written to the raw ADCP file.
        /// </summary>
        public string RawGpsBytesWritten
        {
            get
            {
                switch (_terminalType)
                {
                    case TerminalNavType.GPS1:
                    default:
                        return MathHelper.MemorySizeString(_adcpConnection.RawGps1BytesWritten);
                    case TerminalNavType.GPS2:
                        return MathHelper.MemorySizeString(_adcpConnection.RawGps2BytesWritten);
                    case TerminalNavType.NMEA1:
                        return MathHelper.MemorySizeString(_adcpConnection.RawNmea1BytesWritten);
                    case TerminalNavType.NMEA2:
                        return MathHelper.MemorySizeString(_adcpConnection.RawNmea2BytesWritten);
                }
            }
        }

        #endregion

        #endregion

        #region Command

        /// <summary>
        /// Send a BREAK to the GPS serial port.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GpsBreakCommand { get; protected set; }

        /// <summary>
        /// Clear the GPS receive buffer.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GpsClearCommand { get; protected set; }

        /// <summary>
        /// Command to send commands to the GPS.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GpsSendCommand { get; protected set; }

        /// <summary>
        /// Command to send the ESC command to the GPS.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GpsSendEscCommand { get; protected set; }

        /// <summary>
        /// Scan for all available comm ports.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> RescanCommPortCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public TerminalNmeaViewModel(TerminalNavType type, string title, AdcpConnection adcpConn)
            : base("NMEA Terminal")
        {
            _terminalType = type;
            Title = title;

            // Pulse Manager
            _pm = IoC.Get<PulseManager>();

            // Get the singleton ADCP connection
            _adcpConnection = adcpConn;

            // Create eventhandlers for serial connection/disconnection
            SubscribeSerialConnectionEvents();

            // Initialize values
            _GpsCommandHistory = new ObservableCollection<string>();
            _gpsSerialOptions = new SerialOptions();
            ScanForAvailablePorts();

            // Subscribe to receive events when data is available from the serial port
            if (SerialPort() != null && SerialPort().IsAvailable())
            {
                SerialPort().ReceiveGpsSerialDataEvent += new GpsSerialPort.ReceiveGpsSerialDataEventHandler(GpsSerialPort_ReceiveGpsSerialDataEvent);
            }

            // Send a BREAK to the GPS command
            GpsBreakCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                      // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist 
                                                                        _ => Task.Run(() => SendBreak()));                                              // Send BREAK to the GPS

            // Clear GPS Receive Buffer command
            GpsClearCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                      // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => ClearGpsReceiveBuffer()));                                  // Clear GPS Receive Buffer

            // Send a command to the GPS
            GpsSendCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                       // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SendCommandToGps()));                                       // Send command to GPS

            // Send a command to the GPS
            GpsSendEscCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                    // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SendEscCommandToGps()));                                    // Send command to GPS

            RescanCommPortCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => ScanForAvailablePorts()));                                  // Scan for all available ports

            // Connect if settings have it set to connect
            IsGpsSerialPortEnabled = GetIsGpsSerialPortEnabled();

            // Update all the serial port options based off previous settings
            //this.NotifyOfPropertyChange(() => this.IsGpsSerialPortEnabled);
            this.NotifyOfPropertyChange(() => this.GpsPort);
            this.NotifyOfPropertyChange(() => this.GpsBaudRate);

            this.NotifyOfPropertyChange(() => this.IsRawGpsRecording);
            this.NotifyOfPropertyChange(() => this.RawGpsBytesWritten);
            this.NotifyOfPropertyChange(() => this.RawGpsFileName);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            // Unsubscribe
            UnsubscribeSerialConnectionEvents();

            // Unsubscribe to receive events when data is available from the serial port
            DisconnectSerialGps();
        }

        #region Serial Connection

        /// <summary>
        /// Return the serial port connection from AdcpConnection
        /// based off the terminal type.
        /// </summary>
        /// <returns>Serial port from AdcpConnection.</returns>
        private GpsSerialPort SerialPort()
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    return _adcpConnection.Gps1SerialPort;
                case TerminalNavType.GPS2:
                    return _adcpConnection.Gps2SerialPort;
                case TerminalNavType.NMEA1:
                    return _adcpConnection.Nmea1SerialPort;
                case TerminalNavType.NMEA2:
                    return _adcpConnection.Nmea2SerialPort;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Check if the serial port is available.
        /// Check if the AdcpConnection is set.  Check
        /// if the serial port exist.  Check if the serial
        /// port is open.
        /// </summary>
        /// <returns>TRUE = SerialPort is available.  /  False = SerialPort is not available.</returns>
        private bool IsSerialPortAvail()
        {
            if (_adcpConnection != null)
            {
                // Subscribe to receive events when data is available from the serial port
                if ( SerialPort() != null && SerialPort().IsOpen())
                {
                    return true;
                }

            }

            return false;
        }

        #endregion

        #region Serial Connection Events

        /// <summary>
        /// Establish the connection for knowing
        /// when the serial connetions are connected
        /// and disconnected.
        /// </summary>
        private void SubscribeSerialConnectionEvents()
        {
            switch (_terminalType)
            {
                case TerminalNavType.GPS1:
                    _adcpConnection.Gps1SerialConnectEvent += new AdcpConnection.Gps1SerialConnectEventHandler(_adcpConnection_GpsSerialConnectEvent);
                    _adcpConnection.Gps1SerialDisconnectEvent += new AdcpConnection.Gps1SerialDisconnectEventHandler(_adcpConnection_GpsSerialDisconnectEvent);
                    break;
                case TerminalNavType.GPS2:
                    _adcpConnection.Gps2SerialConnectEvent += new AdcpConnection.Gps2SerialConnectEventHandler(_adcpConnection_GpsSerialConnectEvent);
                    _adcpConnection.Gps2SerialDisconnectEvent += new AdcpConnection.Gps2SerialDisconnectEventHandler(_adcpConnection_GpsSerialDisconnectEvent);
                    break;
                case TerminalNavType.NMEA1:
                    _adcpConnection.Nmea1SerialConnectEvent += new AdcpConnection.Nmea1SerialConnectEventHandler(_adcpConnection_GpsSerialConnectEvent);
                    _adcpConnection.Nmea1SerialDisconnectEvent += new AdcpConnection.Nmea1SerialDisconnectEventHandler(_adcpConnection_GpsSerialDisconnectEvent);
                    break;
                case TerminalNavType.NMEA2:
                    _adcpConnection.Nmea2SerialConnectEvent += new AdcpConnection.Nmea2SerialConnectEventHandler(_adcpConnection_GpsSerialConnectEvent);
                    _adcpConnection.Nmea2SerialDisconnectEvent += new AdcpConnection.Nmea2SerialDisconnectEventHandler(_adcpConnection_GpsSerialDisconnectEvent);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Unsubscribe from the serial connection events.
        /// </summary>
        private void UnsubscribeSerialConnectionEvents()
        {
            switch (_terminalType)
            {
                case TerminalNavType.GPS1:
                    _adcpConnection.Gps1SerialConnectEvent -= _adcpConnection_GpsSerialConnectEvent;
                    _adcpConnection.Gps1SerialDisconnectEvent -= _adcpConnection_GpsSerialDisconnectEvent;
                    break;
                case TerminalNavType.GPS2:
                    _adcpConnection.Gps2SerialConnectEvent -= _adcpConnection_GpsSerialConnectEvent;
                    _adcpConnection.Gps2SerialDisconnectEvent -= _adcpConnection_GpsSerialDisconnectEvent;
                    break;
                case TerminalNavType.NMEA1:
                    _adcpConnection.Nmea1SerialConnectEvent -= _adcpConnection_GpsSerialConnectEvent;
                    _adcpConnection.Nmea1SerialDisconnectEvent -= _adcpConnection_GpsSerialDisconnectEvent;
                    break;
                case TerminalNavType.NMEA2:
                    _adcpConnection.Nmea2SerialConnectEvent -= _adcpConnection_GpsSerialConnectEvent;
                    _adcpConnection.Nmea2SerialDisconnectEvent -= _adcpConnection_GpsSerialDisconnectEvent;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region GPS Serial Port

        /// <summary>
        /// Reconnect the Serial GPS connection.
        /// </summary>
        private void ConnectSerialGps()
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    if (_adcpConnection.Gps1SerialPort != null && _adcpConnection.Gps1SerialPort.SerialOptions != null)
                    {
                        _adcpConnection.ConnectGps1Serial(_adcpConnection.Gps1SerialPort.SerialOptions);
                    }
                    else if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                    {
                        _adcpConnection.ConnectGps1Serial(new SerialOptions() { Port = GpsPort, BaudRate = GpsBaudRate });
                    }

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Gps1SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }
                    break;
                case TerminalNavType.GPS2:
                    if (_adcpConnection.Gps2SerialPort != null && _adcpConnection.Gps2SerialPort.SerialOptions != null)
                    {
                        _adcpConnection.ConnectGps2Serial(_adcpConnection.Gps2SerialPort.SerialOptions);
                    }
                    else if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                    {
                        _adcpConnection.ConnectGps2Serial(new SerialOptions() { Port = GpsPort, BaudRate = GpsBaudRate });
                    }

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Gps2SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }
                    break;
                case TerminalNavType.NMEA1:
                    if (_adcpConnection.Nmea1SerialPort != null && _adcpConnection.Nmea1SerialPort.SerialOptions != null)
                    {
                        _adcpConnection.ConnectNmea1Serial(_adcpConnection.Nmea1SerialPort.SerialOptions);
                    }
                    else if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                    {
                        _adcpConnection.ConnectNmea1Serial(new SerialOptions() { Port = GpsPort, BaudRate = GpsBaudRate });
                    }

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Nmea1SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }
                    break;
                case TerminalNavType.NMEA2:
                    if (_adcpConnection.Nmea2SerialPort != null && _adcpConnection.Nmea2SerialPort.SerialOptions != null)
                    {
                        _adcpConnection.ConnectNmea2Serial(_adcpConnection.Nmea2SerialPort.SerialOptions);
                    }
                    else if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                    {
                        _adcpConnection.ConnectNmea2Serial(new SerialOptions() { Port = GpsPort, BaudRate = GpsBaudRate });
                    }

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Nmea2SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }
                    break;
                default:
                    break;
            }

            if (IsSerialPortAvail())
            {
                SerialPort().ReceiveGpsSerialDataEvent += new GpsSerialPort.ReceiveGpsSerialDataEventHandler(GpsSerialPort_ReceiveGpsSerialDataEvent);
            }

            ClearGpsReceiveBuffer();
        }

        /// <summary>
        /// Unsubscribe and disconnect the GPS serial port.
        /// </summary>
        private void DisconnectSerialGps()
        {
            if (IsSerialPortAvail())
            {
                // Unsubscribe
                SerialPort().ReceiveGpsSerialDataEvent -= GpsSerialPort_ReceiveGpsSerialDataEvent;

                switch (_terminalType)
                {
                    case TerminalNavType.GPS1:
                        _adcpConnection.DisconnectGps1Serial();
                        break;
                    case TerminalNavType.GPS2:
                        _adcpConnection.DisconnectGps2Serial();
                        break;
                    case TerminalNavType.NMEA1:
                        _adcpConnection.DisconnectNmea1Serial();
                        break;
                    case TerminalNavType.NMEA2:
                        _adcpConnection.DisconnectNmea2Serial();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Disconnect then connect the serial GPS.
        /// This will ensure all the events are handled properly
        /// to reconnect.
        /// </summary>
        private void ReconnectSerialGps()
        {
            // Disconnect
            DisconnectSerialGps();

            // Connect
            ConnectSerialGps();
        }

        /// <summary>
        /// Event handler when receiving GPS serial data.
        /// </summary>
        /// <param name="data">Data received from the GPS serial port.</param>
        private void GpsSerialPort_ReceiveGpsSerialDataEvent(string data)
        {
            this.NotifyOfPropertyChange(() => this.GpsReceiveBuffer);

            this.NotifyOfPropertyChange(() => this.IsRawGpsRecording);
            this.NotifyOfPropertyChange(() => this.RawGpsBytesWritten);
            this.NotifyOfPropertyChange(() => this.RawGpsFileName);
        }

        /// <summary>
        /// Clear the GPS serial port Receive buffer.
        /// </summary>
        private void ClearGpsReceiveBuffer()
        {
            //if (IsSerialPortAvail())
            if(SerialPort() != null)
            {
                SerialPort().ReceiveBufferString = "";
                NotifyOfPropertyChange(() => this.GpsReceiveBuffer);
            }
        }

        /// <summary>
        /// Display a serial port error message.
        /// </summary>
        /// <param name="error">Error message.</param>
        private void DisplaySerialPortError(string error)
        {
            if (SerialPort() != null)
            {
                SerialPort().ReceiveBufferString = "";
                SerialPort().ReceiveBufferString = error;
                NotifyOfPropertyChange(() => this.GpsReceiveBuffer);
            }
        }

        /// <summary>
        /// Display the connection error message.
        /// </summary>
        private void DisplayConnectionError()
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    if (_adcpConnection.Gps1SerialPort != null)
                    {
                        DisplaySerialPortError(string.Format("GPS1 COULD NOT CONNECT: Port: {0}, Baud: {1}", _adcpConnection.Gps1SerialPort.SerialOptions.Port, _adcpConnection.Gps1SerialPort.SerialOptions.BaudRate));
                    }
                    else
                    {
                        DisplaySerialPortError(string.Format("GPS1 NOT CONNECTED"));
                    }
                    break;
                case TerminalNavType.GPS2:
                    if (_adcpConnection.Gps2SerialPort != null)
                    {
                        DisplaySerialPortError(string.Format("GPS2 COULD NOT CONNECT: Port: {0}, Baud: {1}", _adcpConnection.Gps2SerialPort.SerialOptions.Port, _adcpConnection.Gps2SerialPort.SerialOptions.BaudRate));
                    }
                    else
                    {
                        DisplaySerialPortError(string.Format("GPS2 NOT CONNECTED"));
                    }
                    break;
                case TerminalNavType.NMEA1:
                    if (_adcpConnection.Nmea1SerialPort != null)
                    {
                        DisplaySerialPortError(string.Format("NMEA1 COULD NOT CONNECT: Port: {0}, Baud: {1}", _adcpConnection.Nmea1SerialPort.SerialOptions.Port, _adcpConnection.Nmea1SerialPort.SerialOptions.BaudRate));
                    }
                    else
                    {
                        DisplaySerialPortError(string.Format("NMEA1 NOT CONNECTED"));
                    }
                    break;
                case TerminalNavType.NMEA2:
                    if (_adcpConnection.Nmea2SerialPort != null)
                    {
                        DisplaySerialPortError(string.Format("NMEA2 COULD NOT CONNECT: Port: {0}, Baud: {1}", _adcpConnection.Nmea2SerialPort.SerialOptions.Port, _adcpConnection.Nmea2SerialPort.SerialOptions.BaudRate));
                    }
                    else
                    {
                        DisplaySerialPortError(string.Format("NMEA2 NOT CONNECTED"));
                    }
                    break;
            
            }
        }

        /// <summary>
        /// Send a BREAK to the GPS.
        /// </summary>
        private void SendBreak()
        {
            if (IsSerialPortAvail())
            {
                SerialPort().SendBreak();
            }
            else
            {
                DisplayConnectionError();
            }
        }

        /// <summary>
        /// Send a command to the GPS.  This will get the
        /// command given by the user.
        /// </summary>
        private void SendCommandToGps()
        {
            if (IsSerialPortAvail())
            {
                // Send the command within the buffer
                // Try sending the command.  If it fails try one more time
                if (!SerialPort().SendDataWaitReply(SelectedGpsCommand, 1000))
                {
                    // Try again if failed first time
                    SerialPort().SendDataWaitReply(SelectedGpsCommand, 1000);
                }

                SelectedGpsCommand = "";
            }
            else
            {
                DisplayConnectionError();
            }
        }

        /// <summary>
        /// Send the ESC command to the GPS serial port.
        /// This will take the hex value for ESC and store it to an 
        /// array.  It will then send the command to the serial port.
        /// </summary>
        private void SendEscCommandToGps()
        {
            if (IsSerialPortAvail())
            {
                byte[] data = new byte[1];
                data[0] = 0x1B;
                SerialPort().SendData(data, 0, 1);
            }
            else
            {
                DisplayConnectionError();
            }
        }

        #endregion

        #region Serial Port Options

        /// <summary>
        /// Scan for all the avaiable serial ports on the system.
        /// Set then to the list.
        /// </summary>
        private void ScanForAvailablePorts()
        {
            GpsPortOptions = SerialOptions.PortOptions;
        }

        #region IsGpsSerialPortEnabled

        /// <summary>
        /// Get the flag if the GPS serial port is enabled.
        /// </summary>
        /// <returns>Value for enabled flag based off terminal type.</returns>
        private bool GetIsGpsSerialPortEnabled()
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    // Get the options from the connection
                    if (_adcpConnection.Gps1SerialPort != null && _adcpConnection.Gps1SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.IsGps1SerialPortEnabled;
                    }

                    return _pm.GetIsGps1SerialEnabled(); 
                case TerminalNavType.GPS2:
                    // Get the options from the connection
                    if (_adcpConnection.Gps2SerialPort != null && _adcpConnection.Gps2SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.IsGps2SerialPortEnabled;
                    }

                    return _pm.GetIsGps2SerialEnabled(); 
                case TerminalNavType.NMEA1:
                    // Get the options from the connection
                    if (_adcpConnection.Nmea1SerialPort != null && _adcpConnection.Nmea1SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.IsNmea1SerialPortEnabled;
                    }

                    return _pm.GetIsNmea1SerialEnabled(); 
                case TerminalNavType.NMEA2:
                    // Get the options from the connection
                    if (_adcpConnection.Nmea2SerialPort != null && _adcpConnection.Nmea2SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.IsNmea2SerialPortEnabled;
                    }

                    return _pm.GetIsNmea2SerialEnabled(); 
                default:
                    return false;
            }
        }

        /// <summary>
        /// Set the IsGpsSerialPortEnabled value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        private void SetIsGpsSerialPortEnabled(bool value)
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    _adcpConnection.IsGps1SerialPortEnabled = value;
                    _pm.UpdateIsGps1SerialEnabled(value);
                    break;
                case TerminalNavType.GPS2:
                    _adcpConnection.IsGps2SerialPortEnabled = value;
                    _pm.UpdateIsGps2SerialEnabled(value);
                    break;
                case TerminalNavType.NMEA1:
                    _adcpConnection.IsNmea1SerialPortEnabled = value;
                    _pm.UpdateIsNmea1SerialEnabled(value);
                    break;
                case TerminalNavType.NMEA2:
                    _adcpConnection.IsNmea2SerialPortEnabled = value;
                    _pm.UpdateIsNmea2SerialEnabled(value);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Comm Port

        /// <summary>
        /// Get the GPS serial comm port.
        /// It will determine the type to fine the correct value.
        /// </summary>
        /// <returns>Comm port.</returns>
        private string GetGpsCommPort()
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Gps1SerialOptions.Port;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Gps1SerialPort != null && _adcpConnection.Gps1SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Gps1SerialPort.SerialOptions.Port;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetGps1SerialCommPort();
                case TerminalNavType.GPS2:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Gps2SerialOptions.Port;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Gps2SerialPort != null && _adcpConnection.Gps2SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Gps2SerialPort.SerialOptions.Port;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetGps2SerialCommPort();
                case TerminalNavType.NMEA1:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Nmea1SerialOptions.Port;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Nmea1SerialPort != null && _adcpConnection.Nmea1SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Nmea1SerialPort.SerialOptions.Port;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetNmea1SerialCommPort();
                case TerminalNavType.NMEA2:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Nmea2SerialOptions.Port;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Nmea2SerialPort != null && _adcpConnection.Nmea2SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Nmea2SerialPort.SerialOptions.Port;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetNmea2SerialCommPort();
                default:
                    return "";
            }
        }

        /// <summary>
        /// Set the GPS Comm Port.
        /// </summary>
        /// <param name="value">Comm Port to set.</param>
        private void SetGpsCommPort(string value)
        {
            switch(_terminalType)
            {
                case TerminalNavType.GPS1:
                    this.NotifyOfPropertyChange(() => this.GpsPort);

                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Gps1SerialOptions.Port = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectGps1Serial(_pm.SelectedProject.Configuration.Gps1SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectGps1Serial(new SerialOptions() { Port = value, BaudRate = GpsBaudRate });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateGps1SerialCommPort(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Gps1SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
                case TerminalNavType.GPS2:
                    this.NotifyOfPropertyChange(() => this.GpsPort);

                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Gps2SerialOptions.Port = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectGps2Serial(_pm.SelectedProject.Configuration.Gps2SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectGps2Serial(new SerialOptions() { Port = value, BaudRate = GpsBaudRate });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateGps2SerialCommPort(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Gps2SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
                case TerminalNavType.NMEA1:
                    this.NotifyOfPropertyChange(() => this.GpsPort);

                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Nmea1SerialOptions.Port = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectNmea1Serial(_pm.SelectedProject.Configuration.Nmea1SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectNmea1Serial(new SerialOptions() { Port = value, BaudRate = GpsBaudRate });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateNmea1SerialCommPort(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Nmea1SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
                case TerminalNavType.NMEA2:
                    this.NotifyOfPropertyChange(() => this.GpsPort);

                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Nmea2SerialOptions.Port = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectNmea2Serial(_pm.SelectedProject.Configuration.Nmea2SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectNmea2Serial(new SerialOptions() { Port = value, BaudRate = GpsBaudRate });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateNmea2SerialCommPort(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Nmea2SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
            }
        }

        #endregion

        #region Baud Rate

        /// <summary>
        /// Get the GPS serial baud rate.
        /// It will determine the type to fine the correct value.
        /// </summary>
        /// <returns>Baud Rate.</returns>
        private int GetGpsBaudRate()
        {
            switch (_terminalType)
            {
                case TerminalNavType.GPS1:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Gps1SerialOptions.BaudRate;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Gps1SerialPort != null && _adcpConnection.Gps1SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Gps1SerialPort.SerialOptions.BaudRate;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetGps1SerialBaudRate();
                case TerminalNavType.GPS2:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Gps2SerialOptions.BaudRate;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Gps2SerialPort != null && _adcpConnection.Gps2SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Gps2SerialPort.SerialOptions.BaudRate;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetGps2SerialBaudRate();
                case TerminalNavType.NMEA1:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Nmea1SerialOptions.BaudRate;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Nmea1SerialPort != null && _adcpConnection.Nmea1SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Nmea1SerialPort.SerialOptions.BaudRate;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetNmea1SerialBaudRate();
                case TerminalNavType.NMEA2:
                    // If a project is selected, get to the options from the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        return _pm.SelectedProject.Configuration.Nmea2SerialOptions.BaudRate;
                    }

                    // Get the options from the connection
                    if (_adcpConnection.Nmea2SerialPort != null && _adcpConnection.Nmea2SerialPort.SerialOptions != null)
                    {
                        return _adcpConnection.Nmea2SerialPort.SerialOptions.BaudRate;
                    }

                    // Get the options from the PulseManager's last settings
                    return _pm.GetNmea2SerialBaudRate();
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Set the GPS Baud Rate.
        /// </summary>
        /// <param name="value">Baud Rate to set.</param>
        private void SetGpsBaudRate(int value)
        {
            // Property change
            this.NotifyOfPropertyChange(() => this.GpsBaudRate);

            switch (_terminalType)
            {
                case TerminalNavType.GPS1:
                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Gps1SerialOptions.BaudRate = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectGps1Serial(_pm.SelectedProject.Configuration.Gps1SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectGps1Serial(new SerialOptions() { Port = GpsPort, BaudRate = value });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateGps1SerialBaudRate(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Gps1SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
                case TerminalNavType.GPS2:
                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Gps2SerialOptions.BaudRate = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectGps2Serial(_pm.SelectedProject.Configuration.Gps2SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectGps2Serial(new SerialOptions() { Port = GpsPort, BaudRate = value });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateGps2SerialBaudRate(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Gps2SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
                case TerminalNavType.NMEA1:
                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Nmea1SerialOptions.BaudRate = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectNmea1Serial(_pm.SelectedProject.Configuration.Nmea1SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectNmea1Serial(new SerialOptions() { Port = GpsPort, BaudRate = value });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateNmea1SerialBaudRate(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Nmea1SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
                case TerminalNavType.NMEA2:
                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        _pm.SelectedProject.Configuration.Nmea2SerialOptions.BaudRate = value;

                        // Save the new configuration settings
                        _pm.SelectedProject.Save();

                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectNmea2Serial(_pm.SelectedProject.Configuration.Nmea2SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (!string.IsNullOrEmpty(GpsPort) && GpsBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectNmea2Serial(new SerialOptions() { Port = GpsPort, BaudRate = value });
                        }
                    }

                    // Set the PM with the latest option
                    _pm.UpdateNmea2SerialBaudRate(value);

                    // Check if a conneciton could be made
                    if (!_adcpConnection.Nmea2SerialPort.IsAvailable())
                    {
                        DisplayConnectionError();
                    }

                    break;
            }
        }

        #endregion

        #endregion

        #region EventHandler

        /// <summary>
        /// Serial Connection event received.
        /// </summary>
        private void _adcpConnection_GpsSerialConnectEvent()
        {
            //ConnectSerialGps();
            if (IsSerialPortAvail())
            {
                SerialPort().ReceiveGpsSerialDataEvent += new GpsSerialPort.ReceiveGpsSerialDataEventHandler(GpsSerialPort_ReceiveGpsSerialDataEvent);
            }
        }

        /// <summary>
        /// Disconnect event received for the serial port.
        /// </summary>
        private void _adcpConnection_GpsSerialDisconnectEvent()
        {
            //DisconnectSerialGps();
            // Unsubscribe
            SerialPort().ReceiveGpsSerialDataEvent -= GpsSerialPort_ReceiveGpsSerialDataEvent;
        }

        #endregion
    }
}
