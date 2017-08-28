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
 * 05/06/2013      RC          3.0.0      Initial coding
 * 05/23/2013      RC          3.0.0      Added Send command.
 * 07/31/2013      RC          3.0.6      Added CompassDisconnectCommand.
 * 09/17/2013      RC          3.1.2      Added SendCommandSet, ClearCommandSet and ImportCommandSet to send a bulk set of commands.
 * 10/15/2013      RC          3.2.0      Save the ADCP serial port options to the Pulse Database.
 * 11/25/2013      RC          3.2.0      Added Ethernet options.  Made the terminal work with serial and ethernet by passing all commands to AdcpConnection.
 * 12/05/2013      RC          3.2.0      Added Scan button to scan for new serial ports.
 * 01/20/2014      RC          3.2.3      Added warning message if not connected.
 * 06/13/2014      RC          3.3.1      Make a connection after the scan in ScanSerialPorts().
 * 07/01/2014      RC          3.4.0      Fixed a bug in ScanSerialPorts() where if no port is found, do not set SelectedSerialPort.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/22/2014      RC          4.0.2      Added raw recording.
 * 12/02/2015      RC          4.4.0      Added, DataBit, Parity and Stop Bit.
 * 05/24/2017      RC          4.4.6      Added some additional command buttons.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using System.Collections.ObjectModel;
    using System.Collections;
    using ReactiveUI;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading.Tasks;

    /// <summary>
    /// ADCP Terminal to communicate with the ADCP.
    /// </summary>
    public class TerminalAdcpViewModel : PulseViewModel
    {

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Singleton object to communication with the ADCP.
        /// </summary>
        private AdcpConnection _adcpConnection;

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        /// <summary>
        /// Timer to reduce the number of update calls the terminal window.
        /// </summary>
        private System.Timers.Timer _displayTimer;


        #endregion

        #region Class and Enums

        /// <summary>
        /// Class to describe the types of ADCP communication.
        /// </summary>
        public class  AdcpCommOptions
        {
            /// <summary>
            /// Type of ADCP communication.
            /// </summary>
            public AdcpConnection.AdcpCommTypes AdcpCommType { get; set;}

            /// <summary>
            /// Name of the type.
            /// </summary>
            public string Name { get; set;}
            
            /// <summary>
            /// Initialize the object.
            /// </summary>
            /// <param name="type">Adcp Comm Type.</param>
            /// <param name="name">Name of the type.</param>
            public AdcpCommOptions(AdcpConnection.AdcpCommTypes type, string name)
            {
                AdcpCommType = type;
                Name = name;
            }
        }

        #endregion

        #region Properties

        #region ADCP Receive Buffer

        /// <summary>
        /// Display the receive buffer from the connected ADCP serial port.
        /// </summary>
        public string AdcpReceiveBuffer
        {
            get { return _adcpConnection.ReceiveBufferString; }
        }

        #endregion

        #region ADCP Send Commands

        /// <summary>
        /// History of all the previous ADCP commands.
        /// </summary>
        private ObservableCollection<string> _AdcpCommandHistory;
        /// <summary>
        /// History of all the previous ADCP commands.
        /// </summary>
        public IEnumerable AdcpCommandHistory
        {
            get { return _AdcpCommandHistory; }
        }

        /// <summary>
        /// Command currently selected.
        /// </summary>
        private string _SelectedAdcpCommand;
        /// <summary>
        /// Command currently selected.
        /// </summary>
        public string SelectedAdcpCommand
        {
            get { return _SelectedAdcpCommand; }
            set
            {
                _SelectedAdcpCommand = value;
                this.NotifyOfPropertyChange(() => this.SelectedAdcpCommand);
                this.NotifyOfPropertyChange(() => this.NewAdcpCommand);
            }
        }

        /// <summary>
        /// New command entered by the user.
        /// This will be called when the user enters
        /// in a new command to send to the ADCP.
        /// It will update the list and set the SelectedCommand.
        /// </summary>
        public string NewAdcpCommand
        {
            get { return _SelectedAdcpCommand; }
            set
            {
                //if (_SelectedAdcpCommand != null)
                //{
                //    return;
                //}
                if (!string.IsNullOrEmpty(value))
                {
                    _AdcpCommandHistory.Insert(0, value);
                    SelectedAdcpCommand = value;
                }
            }
        }


        /// <summary>
        /// A list of commands to send to the ADCP.  This will
        /// usually include multiple commands listed one on each line.
        /// </summary>
        private string _AdcpCommandSet;
        /// <summary>
        /// A list of commands to send to the ADCP.  This will
        /// usually include multiple commands listed one on each line.
        /// </summary>
        public string AdcpCommandSet
        {
            get { return _AdcpCommandSet; }
            set
            {
                _AdcpCommandSet = value;
                this.NotifyOfPropertyChange(() => this.AdcpCommandSet);
            }
        }

        #endregion

        #region Select Connection

        /// <summary>
        /// List of all the ADCP Communication options.
        /// </summary>
        public ReactiveList<AdcpCommOptions> AdcpCommOptionsList { get; protected set; }

        /// <summary>
        /// ADCP Connection Type.
        /// </summary>
        private AdcpCommOptions _SelectedAdcpConnOption;
        /// <summary>
        /// ADCP Connection Type.
        /// </summary>
        public AdcpCommOptions SelectedAdcpConnOption
        {
            get { return _SelectedAdcpConnOption; }
            set
            {
                _SelectedAdcpConnOption = value;
                this.NotifyOfPropertyChange(() => this.SelectedAdcpConnOption);
                this.NotifyOfPropertyChange(() => this.CanFindAdcp);

                SetSelectedAdcpConn(value);
            }
        }

        #endregion

        #region ADCP Serial Port Options

        #region Lists

        /// <summary>
        /// List of all the comm ports on the computer.
        /// </summary>
        private List<string> _CommPortList;
        /// <summary>
        /// List of all the comm ports on the computer.
        /// </summary>
        public List<string> CommPortList
        {
            get { return _CommPortList; }
            set
            {
                _CommPortList = value;
                this.NotifyOfPropertyChange(() => this.CommPortList);
            }
        }

        /// <summary>
        /// List of all the baud rate options.
        /// </summary>
        public List<int> BaudRateList { get; set; }

        /// <summary>
        /// List of all the Data Bit options.
        /// </summary>
        public List<int> DataBitList { get; set; }

        /// <summary>
        /// List of all the Parity options.
        /// </summary>
        public List<System.IO.Ports.Parity> ParityList { get; set; }

        /// <summary>
        /// List of all the Stop Bit options.
        /// </summary>
        public List<System.IO.Ports.StopBits> StopBitList { get; set; }

        #endregion

        /// <summary>
        /// Selected ADCP Comm Port.
        /// </summary>
        public string SelectedAdcpCommPort
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.Port;
                }

                // Get the options from the connection
                if (_adcpConnection.AdcpSerialPort != null && _adcpConnection.AdcpSerialPort.SerialOptions != null)
                {
                    return _adcpConnection.GetAdcpSerialCommPort();
                }

                return _pm.GetAdcpSerialCommPort();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.SelectedAdcpCommPort);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.Port = value;

                    // Automatcially reconnect when the settings change
                    _adcpConnection.ReconnectAdcpSerial(_pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions);
                }
                else
                {
                    // No project selected so create a serial options
                    // Make sure both options have been set
                    if (!string.IsNullOrEmpty(value) && SelectedAdcpBaudRate > 0)
                    {
                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectAdcpSerial(new SerialOptions() { Port = value, BaudRate = SelectedAdcpBaudRate, DataBits = SelectedDataBit, Parity = SelectedParity, StopBits = SelectedStopBit });
                    }
                }

                // Set the PM with the latest option
                _pm.UpdateAdcpSerialCommPort(value);

                // Clear the buffer
                ClearReceiveBuffer();

                // Check if a conneciton could be made
                if (!_adcpConnection.AdcpSerialPort.IsAvailable())
                {
                    DisplayConnectionError();
                }
            }
        }

        /// <summary>
        /// Selected ADCP Baud Rate.
        /// </summary>
        public int SelectedAdcpBaudRate
        {
            get 
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.BaudRate;
                }

                // Get the options from the connection
                if (_adcpConnection.AdcpSerialPort != null && _adcpConnection.AdcpSerialPort.SerialOptions != null)
                {
                    return _adcpConnection.GetAdcpSerialBaudRate();
                }

                return _pm.GetAdcpSerialBaudRate();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.SelectedAdcpBaudRate);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.BaudRate = value;

                    // Automatcially reconnect when the settings change
                    _adcpConnection.ReconnectAdcpSerial(_pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions);
                }
                else
                {
                    // No project selected so create a serial options
                    // Make sure both options have been set
                    if (SelectedAdcpCommPort != null && value > 0)
                    {
                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectAdcpSerial(new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = value, DataBits = SelectedDataBit, Parity = SelectedParity, StopBits = SelectedStopBit });
                    }
                }

                // Set the PM with the latest option
                _pm.UpdateAdcpSerialBaudRate(value);

                // Clear the buffer
                ClearReceiveBuffer();

                // Check if a conneciton could be made
                if (!_adcpConnection.AdcpSerialPort.IsAvailable())
                {
                    DisplayConnectionError();
                }
            }
        }

        /// <summary>
        /// Selected ADCP Data Bit.
        /// </summary>
        public int SelectedDataBit
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.DataBits;
                }

                // Get the options from the connection
                if (_adcpConnection.AdcpSerialPort != null && _adcpConnection.AdcpSerialPort.SerialOptions != null)
                {
                    return _adcpConnection.GetAdcpSerialDataBit();
                }

                return _pm.GetAdcpSerialDataBit();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.SelectedDataBit);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.DataBits = value;

                    // Automatcially reconnect when the settings change
                    _adcpConnection.ReconnectAdcpSerial(_pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions);
                }
                else
                {
                    // No project selected so create a serial options
                    // Make sure both options have been set
                    if (value > 0)
                    {
                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectAdcpSerial(new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = SelectedAdcpBaudRate, DataBits = value, Parity = SelectedParity, StopBits = SelectedStopBit });
                    }
                }

                // Set the PM with the latest option
                _pm.UpdateAdcpSerialDataBit(value);

                // Clear the buffer
                ClearReceiveBuffer();

                // Check if a conneciton could be made
                if (!_adcpConnection.AdcpSerialPort.IsAvailable())
                {
                    DisplayConnectionError();
                }
            }
        }

        /// <summary>
        /// Selected ADCP Parity.
        /// </summary>
        public System.IO.Ports.Parity SelectedParity
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.Parity;
                }

                // Get the options from the connection
                if (_adcpConnection.AdcpSerialPort != null && _adcpConnection.AdcpSerialPort.SerialOptions != null)
                {
                    return _adcpConnection.GetAdcpSerialParity();
                }

                return _pm.GetAdcpSerialParity();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.SelectedParity);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.Parity = value;

                    // Automatcially reconnect when the settings change
                    _adcpConnection.ReconnectAdcpSerial(_pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions);
                }
                else
                {
                    // No project selected so create a serial options
                    // Make sure both options have been set
                    if (value > 0)
                    {
                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectAdcpSerial(new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = SelectedAdcpBaudRate, DataBits = SelectedDataBit, Parity = value, StopBits = SelectedStopBit });
                    }
                }

                // Set the PM with the latest option
                _pm.UpdateAdcpSerialParity(value);

                // Clear the buffer
                ClearReceiveBuffer();

                // Check if a conneciton could be made
                if (!_adcpConnection.AdcpSerialPort.IsAvailable())
                {
                    DisplayConnectionError();
                }
            }
        }

        /// <summary>
        /// Selected ADCP Stop Bit.
        /// </summary>
        public System.IO.Ports.StopBits SelectedStopBit
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.StopBits;
                }

                // Get the options from the connection
                if (_adcpConnection.AdcpSerialPort != null && _adcpConnection.AdcpSerialPort.SerialOptions != null)
                {
                    return _adcpConnection.GetAdcpSerialStopBits();
                }

                return _pm.GetAdcpSerialStopBits();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.SelectedStopBit);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions.StopBits = value;

                    // Automatcially reconnect when the settings change
                    _adcpConnection.ReconnectAdcpSerial(_pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions);
                }
                else
                {
                    // No project selected so create a serial options
                    // Make sure both options have been set
                    if (value > 0)
                    {
                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectAdcpSerial(new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = SelectedAdcpBaudRate, DataBits = SelectedDataBit, Parity = SelectedParity,  StopBits = value });
                    }
                }

                // Set the PM with the latest option
                _pm.UpdateAdcpSerialStopBits(value);

                // Clear the buffer
                ClearReceiveBuffer();

                // Check if a conneciton could be made
                if (!_adcpConnection.AdcpSerialPort.IsAvailable())
                {
                    DisplayConnectionError();
                }
            }
        }

        #endregion

        #region ADCP Ethernet Options

        /// <summary>
        /// Selected ADCP Ethernet Address A.
        /// </summary>
        public uint EtherAddressA
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.EthernetOptions.IpAddrA;
                }

                return _adcpConnection.GetEthernetIpAddressA();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.EtherAddressA);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.EthernetOptions.IpAddrA = value;
                }

                // Also set it to the _adcpConnection
                // Make sure both options have been set
                _adcpConnection.UpdateEthernetIpAddressA(value);
            }
        }

        /// <summary>
        /// Selected ADCP Ethernet Address B.
        /// </summary>
        public uint EtherAddressB
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.EthernetOptions.IpAddrB;
                }

                return _adcpConnection.GetEthernetIpAddressB();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.EtherAddressB);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.EthernetOptions.IpAddrB = value;
                }

                // Also set it to the _adcpConnection
                // Make sure both options have been set
                _adcpConnection.UpdateEthernetIpAddressB(value);
            }
        }

        /// <summary>
        /// Selected ADCP Ethernet Address C.
        /// </summary>
        public uint EtherAddressC
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.EthernetOptions.IpAddrC;
                }

                return _adcpConnection.GetEthernetIpAddressC();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.EtherAddressC);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.EthernetOptions.IpAddrC = value;
                }

                // Also set it to the _adcpConnection
                // Make sure both options have been set
                _adcpConnection.UpdateEthernetIpAddressC(value);
            }
        }

        /// <summary>
        /// Selected ADCP Ethernet Address D.
        /// </summary>
        public uint EtherAddressD
        {
            get
            {
                // If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.EthernetOptions.IpAddrD;
                }

                return _adcpConnection.GetEthernetIpAddressD();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.EtherAddressD);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.EthernetOptions.IpAddrD = value;
                }

                // Also set it to the _adcpConnection
                // Make sure both options have been set
                _adcpConnection.UpdateEthernetIpAddressD(value);
            }
        }

        /// <summary>
        /// Ethernet Port
        /// </summary>
        public uint EtherPort
        {
            get
            {
                //If a project is selected, get to the options from the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    return _pm.SelectedProject.Configuration.EthernetOptions.Port;
                }

                return _adcpConnection.GetEthernetPort();
            }
            set
            {
                this.NotifyOfPropertyChange(() => this.EtherPort);

                // Set the value to the project
                if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                {
                    _pm.SelectedProject.Configuration.EthernetOptions.Port = value;
                }

                // Also set it to the _adcpConnection
                // Make sure both options have been set
                _adcpConnection.UpdateEthernetPort(value);
            }
        }

        #endregion

        #region Record

        /// <summary>
        /// Turn on or off recording raw ADCP.
        /// </summary>
        public bool IsRawAdcpRecording
        {
            get
            {
                return _adcpConnection.IsRawAdcpRecording;
            }
            set
            {
                _adcpConnection.IsRawAdcpRecording = value;

                if(value)
                {
                    _adcpConnection.StartRawAdcpRecord(Pulse.Commons.DEFAULT_RECORD_DIR);
                }
                else
                {
                    _adcpConnection.StopRawAdcpRecord();
                }

                this.NotifyOfPropertyChange(() => this.IsRawAdcpRecording);
                this.NotifyOfPropertyChange(() => this.RawAdcpBytesWritten);
                this.NotifyOfPropertyChange(() => this.RawAdcpFileName);
            }
        }

        /// <summary>
        /// File name for the raw ADCP file.
        /// </summary>
        public string RawAdcpFileName
        {
            get
            {
                return _adcpConnection.RawAdcpRecordFileName;
            }
        }

        /// <summary>
        /// Bytes written to the raw ADCP file.
        /// </summary>
        public string RawAdcpBytesWritten
        {
            get
            {
                return MathHelper.MemorySizeString(_adcpConnection.RawAdcpBytesWritten);
            }
        }

        #endregion

        #region Find ADCP

        /// <summary>
        /// Flag if we can find the ADCP.
        /// </summary>
        public bool CanFindAdcp
        {
            get
            {
                // Make sure its a serial connection
                if (_SelectedAdcpConnOption.AdcpCommType == AdcpConnection.AdcpCommTypes.Ethernet || _SelectedAdcpConnOption.AdcpCommType == AdcpConnection.AdcpCommTypes.TCP)
                {
                    return false;
                }

                // Make sure it has not already been pressed
                if(_IsFindingAdcp)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Flag if we are find the ADCP.
        /// </summary>
        private bool _IsFindingAdcp;
        /// <summary>
        /// Flag if we are find the ADCP.
        /// </summary>
        public bool IsFindingAdcp
        {
            get
            {
                return _IsFindingAdcp;
            }
            set
            {
                _IsFindingAdcp = value;
                this.NotifyOfPropertyChange(() => this.IsFindingAdcp);
                this.NotifyOfPropertyChange(() => this.CanFindAdcp);
            }
        }
        

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Send to the command to start pinging the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StartPingCommand { get; protected set; }

        /// <summary>
        /// Send to the command to stop pinging the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StopPingCommand { get; protected set; }

        /// <summary>
        /// Send a BREAK to the serial port.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> BreakCommand { get; protected set; }

        /// <summary>
        /// Send a Force BREAK to the serial port.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ForceBreakCommand { get; protected set; }

        /// <summary>
        /// Clear the receive buffer.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearCommand { get; protected set; }

        /// <summary>
        /// Command to send commands to the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SendCommand { get; protected set; }

        /// <summary>
        /// Command to disconnect the ADCP from the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CompassDisconnectCommand { get; protected set; }

        /// <summary>
        /// Command to Connect the ADCP from the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CompassConnectCommand { get; protected set; }

        /// <summary>
        /// Command to send the command CSHOW to see all the settings.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CSHOWCommand { get; protected set; }

        /// <summary>
        /// Command to send the command SLEEP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SLEEPCommand { get; protected set; }

        /// <summary>
        /// Command to send the command set the time.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SetTimeCommand { get; protected set; }

        /// <summary>
        /// Command to send the command zero the pressure sensor.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ZeroPressureSensorCommand { get; protected set; }

        /// <summary>
        /// Command to send a list of commands found in AdcpCommandSet.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SendCommandSetCommand { get; protected set; }

        /// <summary>
        /// Command to Clear the list of commands found in AdcpCommandSet.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearCommandSetCommand { get; protected set; }

        /// <summary>
        /// Command to import a file with a command set and set to AdcpCommandSet.
        /// </summary>
        public ReactiveCommand<object> ImportCommandSetCommand { get; protected set; }

        /// <summary>
        /// Command to scan for available serial ports.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ScanSerialPortsCommand { get; protected set; }

        /// <summary>
        /// Command to find the ADCP.
        /// </summary>
        public ReactiveCommand<object> FindAdcpCommand { get; protected set; }

        /// <summary>
        /// Command to send a test ping on the ethernet port.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> TestPingEthernetCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the values.
        /// Get the AdcpConnection from the container.
        /// </summary>
        public TerminalAdcpViewModel(AdcpConnection adcpConn) 
            : base("ADCP Terminal")
        {
            // Pulse Manager
            _pm = IoC.Get<PulseManager>();

            IsFindingAdcp = false;

            // Update the display
            _displayTimer = new System.Timers.Timer(500);
            _displayTimer.Elapsed += _displayTimer_Elapsed;
            _displayTimer.AutoReset = true;
            _displayTimer.Enabled = true;

            // Set the list
            InitList();
            AdcpCommOptionsList = new ReactiveList<AdcpCommOptions>();
            AdcpCommOptionsList.Add(new AdcpCommOptions(AdcpConnection.AdcpCommTypes.Serial, "Serial"));
            AdcpCommOptionsList.Add(new AdcpCommOptions(AdcpConnection.AdcpCommTypes.Ethernet, "ADCP Ethernet"));
            AdcpCommOptionsList.Add(new AdcpCommOptions(AdcpConnection.AdcpCommTypes.TCP, "TCP"));

            // Get the singleton ADCP connection
            _adcpConnection = adcpConn;
            _adcpConnection.ReceiveDataEvent += new AdcpConnection.ReceiveDataEventHandler(_adcpConnection_ReceiveDataEvent);

            // Initialize values
            _AdcpCommandHistory = new ObservableCollection<string>();

            // Start Pinging ADCP command
            StartPingCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                     // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port is open 
                                                                        _ => Task.Run(() => StartPinging()));                                           // Start pinging

            // Stop Pinging ADCP command
            StopPingCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                      // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port is open 
                                                                        _ => Task.Run(() => StopPinging()));                                            // Stop pinging

            // Send a BREAK to the ADCP command
            BreakCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                         // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port is open 
                                                                        _ => Task.Run(() => SendBreak()));                                              // Send BREAK to the ADCP


            // Send a FORCE BREAK to the ADCP command
            ForceBreakCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                    // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port is open 
                                                                        _ => Task.Run(() => SendForceBreak()));                                         // Send Force BREAK to the ADCP

            // Clear ADCP Receive Buffer command
            ClearCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                         // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => ClearReceiveBuffer()));                                     // Clear Receive Buffer                                                                            

            // Send a command to the ADCP
            SendCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                          // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SendCommandToAdcp()));                                      // Send command to ADCP                                                                              

            // Compass Disconnect
            CompassDisconnectCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                             // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => CompassDisconnectCommandExec()));                           // Stop Compass command  

            // CSHOW command
            CSHOWCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                         // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => CshowCommandExec()));                                       // CSHOW command        

            // SLEEP command
            SLEEPCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                         // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SleepCommandExec()));                                       // SLEEP command  

            // Set time command
            SetTimeCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                       // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SetTimeCommandExec()));                                     // Set Time command  

            // Zero Pressure Sensor command
            ZeroPressureSensorCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                            // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => ZeroPressureCommandExec()));                                // Zero Pressure Sensor command  

            // Connect Compass command
            CompassConnectCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => CompassConnectCommandExec()));                              // Compass Connect command  

            // Send Command Set command
            SendCommandSetCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                                // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SendCommandSetToAdcp()));                                   // Send the command set command                                                                 

            // Clear Command Set command
            ClearCommandSetCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                               // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => ClearCommandSetCommandExec()));                             // Send the command set command   

            // Test Ethernet Ping Command
            TestPingEthernetCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConnection,                                               // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                          // Verify the Serial port object exist
                                                                        _ => Task.Run(() => SendTestEthernetPing()));                                   // Send the command set command                                  

            // Import Command Set command
            ImportCommandSetCommand = ReactiveCommand.Create(this.WhenAny(_ => _._adcpConnection,                                                       // Pass the AdcpConnection
                                                                        x => x.Value != null));                                                         // Verify the Serial port object exist
            ImportCommandSetCommand.Subscribe(_ => ImportCommandSet());                                                                                 // Import the command set command

            // Scan for Serial Port command
            ScanSerialPortsCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => ScanSerialPorts()));                                           // Scan for the serial ports

            // Find any ADCP command
            FindAdcpCommand = ReactiveCommand.Create(this.WhenAny(_ => _.CanFindAdcp, x => x.Value));
            FindAdcpCommand.Subscribe(_ => Task.Run(() => FindAdcp()));


            if (_adcpConnection.AdcpCommType == AdcpConnection.AdcpCommTypes.Serial)
            {
                _SelectedAdcpConnOption = AdcpCommOptionsList[0];
            }
            else if (_adcpConnection.AdcpCommType == AdcpConnection.AdcpCommTypes.Ethernet)
            {
                _SelectedAdcpConnOption = AdcpCommOptionsList[1];
            }
            else if (_adcpConnection.AdcpCommType == AdcpConnection.AdcpCommTypes.TCP)
            {
                _SelectedAdcpConnOption = AdcpCommOptionsList[2];
            }

            // Set the connection
            SetSelectedAdcpConn(SelectedAdcpConnOption);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            if (_adcpConnection != null)
            {
                _adcpConnection.ReceiveDataEvent -= _adcpConnection_ReceiveDataEvent;
            }
        }

        #region Init

        private void InitList()
        {
            CommPortList = SerialOptions.PortOptions;
            BaudRateList = SerialOptions.BaudRateOptions;
            DataBitList = SerialOptions.DataBitsOptions;

            ParityList = new List<System.IO.Ports.Parity>();
            ParityList.Add(System.IO.Ports.Parity.None);
            ParityList.Add(System.IO.Ports.Parity.Odd);
            ParityList.Add(System.IO.Ports.Parity.Even);

            StopBitList = new List<System.IO.Ports.StopBits>();
            StopBitList.Add(System.IO.Ports.StopBits.One);
            StopBitList.Add(System.IO.Ports.StopBits.Two);
            StopBitList.Add(System.IO.Ports.StopBits.OnePointFive);
        }

        #endregion

        #region Update Display

        /// <summary>
        /// Reduce the number of times the display is updated.
        /// This will update the display based off the timer and not
        /// based off when data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _displayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);

            // Recording
            this.NotifyOfPropertyChange(() => this.IsRawAdcpRecording);
            this.NotifyOfPropertyChange(() => this.RawAdcpBytesWritten);
            this.NotifyOfPropertyChange(() => this.RawAdcpFileName);
        }

        #endregion

        #region Database Settings

        /// <summary>
        /// Save all the settings to the Pulse DB.
        /// </summary>
        private void SaveSettingsToDb()
        {

        }

        #endregion

        #region ADCP Serial Port

        /// <summary>
        /// Clear the ADCP serial port Receive buffer.
        /// </summary>
        private void ClearReceiveBuffer()
        {
            _adcpConnection.ReceiveBufferString = "";
            NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);
        }

        /// <summary>
        /// Send a command to the ADCP.  This will get the
        /// command given by the user.
        /// </summary>
        private void SendCommandToAdcp()
        {
            // Send the command within the buffer
            // Try sending the command.  If it fails try one more time
            if (!_adcpConnection.SendDataWaitReply(SelectedAdcpCommand, 1000))
            {
                // Try again if failed first time
                _adcpConnection.SendDataWaitReply(SelectedAdcpCommand, 1000);
            }

            SelectedAdcpCommand = "";

            // Check if a conneciton could be made
            if (_adcpConnection.GetAdcpCommType() == AdcpConnection.AdcpCommTypes.Serial && !_adcpConnection.AdcpSerialPort.IsAvailable())
            {
                DisplayConnectionError();
            }
        }

        /// <summary>
        /// Display a serial port error message.
        /// </summary>
        /// <param name="error">Error message.</param>
        private void DisplaySerialPortError(string error)
        {
            _adcpConnection.ReceiveBufferString = "";
            _adcpConnection.ReceiveBufferString = error;
            NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);
        }

        /// <summary>
        /// Display a connection error message.
        /// </summary>
        private void DisplayConnectionError()
        {
            DisplaySerialPortError(string.Format("ADCP COULD NOT CONNECT: Port: {0}, Baud: {1}", _adcpConnection.AdcpSerialPort.SerialOptions.Port, _adcpConnection.AdcpSerialPort.SerialOptions.BaudRate));
        }


        #endregion

        #region Commands

        /// <summary>
        /// Send the command to disconnect the compass.
        /// </summary>
        /// <returns></returns>
        private void CompassDisconnectCommandExec()
        {
            _adcpConnection.SendDataWaitReply(RTI.Commands.AdcpCommands.CMD_DIAGCPT_DISCONNECT);
        }

        /// <summary>
        /// Send the command to Connect the compass.
        /// </summary>
        /// <returns></returns>
        private void CompassConnectCommandExec()
        {
            _adcpConnection.AdcpSerialPort.StartCompassMode();
        }

        /// <summary>
        /// Send the command to CSHOW.
        /// </summary>
        /// <returns></returns>
        private void CshowCommandExec()
        {
            _adcpConnection.SendDataWaitReply(RTI.Commands.AdcpCommands.CMD_CSHOW);
        }

        /// <summary>
        /// Send the command to SLEEP.
        /// </summary>
        /// <returns></returns>
        private void SleepCommandExec()
        {
            _adcpConnection.SendDataWaitReply(RTI.Commands.AdcpCommands.CMD_SLEEP);
        }

        /// <summary>
        /// Send the command to Set the time.
        /// </summary>
        /// <returns></returns>
        private void SetTimeCommandExec()
        {
            _adcpConnection.SetLocalSystemDateTime();
        }

        /// <summary>
        /// Send the command to zero the pressure sensor.
        /// </summary>
        /// <returns></returns>
        private void ZeroPressureCommandExec()
        {
            _adcpConnection.SendDataWaitReply(RTI.Commands.AdcpCommands.CMD_CPZ);
        }

        /// <summary>
        /// Send the command to clear the command set.
        /// </summary>
        /// <returns></returns>
        private void ClearCommandSetCommandExec()
        {
            AdcpCommandSet = String.Empty;
        }

        #endregion

        #region Buttons

        /// <summary>
        /// Send the break command.
        /// </summary>
        private void SendBreak()
        {
            _adcpConnection.SendBreak();
        }

        /// <summary>
        /// Set the baud rate to 115200.
        /// Send the Force break command.
        /// </summary>
        private void SendForceBreak()
        {
            // Set the baud rate to 115200
            SelectedAdcpBaudRate = 115200;
            this.NotifyOfPropertyChange(() => this.SelectedAdcpBaudRate);

            // Send a force break
            _adcpConnection.SendForceBreak();
        }

        /// <summary>
        /// Send a command to start pinging.
        /// Also set the Date and Time.
        /// </summary>
        private void StartPinging()
        {
            bool result = _adcpConnection.StartPinging(true);

            // Check if a conneciton could be made
            if (_adcpConnection.GetAdcpCommType() == AdcpConnection.AdcpCommTypes.Serial && _adcpConnection.AdcpSerialPort.IsAvailable() && !result)
            {
                DisplayConnectionError();
            };
        }

        /// <summary>
        /// Send a command to stop pinging.
        /// Also set the Date and Time.
        /// </summary>
        private void StopPinging()
        {
            bool result = _adcpConnection.StopPinging();

            // Check if a conneciton could be made
            if (_adcpConnection.GetAdcpCommType() == AdcpConnection.AdcpCommTypes.Serial && _adcpConnection.AdcpSerialPort.IsAvailable() && !result)
            {
                DisplayConnectionError();
            }
        }

        #endregion

        #region Command Set

        /// <summary>
        /// Get all the commands from the Command set.  Then convert it to an array by spliting
        /// on the new lines.  Each line should be a single command.
        /// Then send the command array to the ADCP serial port.
        /// </summary>
        private void SendCommandSetToAdcp()
        {
            // Verify there are any commands
            if (!string.IsNullOrEmpty(_AdcpCommandSet))
            {
                string[] result = _AdcpCommandSet.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Remove all line feed, carrage returns, new lines and tabs
                for (int x = 0; x < result.Length; x++)
                {
                    result[x] = result[x].Replace("\n", String.Empty);
                    result[x] = result[x].Replace("\r", String.Empty);
                    result[x] = result[x].Replace("\t", String.Empty);
                }

                _adcpConnection.SendCommands(result.ToList());
            }

            // Check if a conneciton could be made
            if (!_adcpConnection.AdcpSerialPort.IsAvailable())
            {
                DisplayConnectionError();
            }
        }

        /// <summary>
        /// Import a command set from a file.
        /// </summary>
        private void ImportCommandSet()
        {
            string fileName = "";
            try
            {
                // Show the FolderBrowserDialog.
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "All files (*.*)|*.*";
                dialog.Multiselect = false;

                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Get the files selected
                    fileName = dialog.FileName;

                    // Set the command set
                    AdcpCommandSet = File.ReadAllText(fileName);
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error reading command set from {0}", fileName), e);
            }
        }

        #endregion

        #region Adcp Comm Option

        /// <summary>
        /// Update the list with the latest serial ports available.
        /// </summary>
        private void ScanSerialPorts()
        {
            // Clear the list
            CommPortList.Clear();

            // Update the lsit with the latest available ports
            CommPortList = SerialOptions.PortOptions;

            // If the list contains any ports
            // Check if the last selected port is one of them
            if (CommPortList.Count > 0)
            {
                if (CommPortList.Contains(_pm.GetAdcpSerialCommPort()))
                {
                    SelectedAdcpCommPort = _pm.GetAdcpSerialCommPort();
                }
                else
                {
                    SelectedAdcpCommPort = CommPortList[0];
                }
            }
        }

        /// <summary>
        /// Set the selected ADCP Conn option.
        /// </summary>
        /// <param name="option">Option to set.</param>
        private void SetSelectedAdcpConn(AdcpCommOptions option)
        {
            // Check to see if it changed
            // If no change do nothing
            //if (_adcpConnection.AdcpCommType != option.AdcpCommType)
            //{
                // Set the new type
                //_adcpConnection.AdcpCommType = option.AdcpCommType;
                _adcpConnection.UpdateAdcpCommType(option.AdcpCommType);

                if (option.AdcpCommType == AdcpConnection.AdcpCommTypes.Serial)
                {
                    // Set the value to the project
                    if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
                    {
                        // Automatcially reconnect when the settings change
                        _adcpConnection.ReconnectAdcpSerial(_pm.SelectedProject.Configuration.AdcpSerialOptions.SerialOptions);
                    }
                    else
                    {
                        // No project selected so create a serial options
                        // Make sure both options have been set
                        if (SelectedAdcpCommPort != null && SelectedAdcpBaudRate > 0)
                        {
                            // Automatcially reconnect when the settings change
                            _adcpConnection.ReconnectAdcpSerial(new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = SelectedAdcpBaudRate, DataBits = SelectedDataBit, Parity = SelectedParity, StopBits = SelectedStopBit });
                        }
                    }

                    // Disconnect the ethernet port
                    _adcpConnection.DisconnectAdcpEthernet();
                    _adcpConnection.DisconnectTcp();
                }
                // ADCP Ethernet port
                // This is the ethernet port within the ADCP
                else if(option.AdcpCommType == AdcpConnection.AdcpCommTypes.Ethernet)
                {
                    // Disconnect all previous connections
                    _adcpConnection.DisconnectAdcpSerial();
                    _adcpConnection.DisconnectAdcpEthernet();
                    _adcpConnection.DisconnectTcp();

                    // Connect to the ethernet port
                    _adcpConnection.ConnectAdcpEthernet();

                }
                // Connect to a TCP server 
                // sending data
                else if (option.AdcpCommType == AdcpConnection.AdcpCommTypes.TCP)
                {
                    // Disconnect all previous connections
                    _adcpConnection.DisconnectAdcpSerial();
                    _adcpConnection.DisconnectAdcpEthernet();
                    _adcpConnection.DisconnectTcp();

                    // Connect to TCP server
                    _adcpConnection.ConnectTcp();
                }
            //}
        }

        #endregion

        #region Ethernet Test Ping

        /// <summary>
        /// Test the Ethernet connection by sending a ping
        /// and getting a response back.
        /// </summary>
        private void SendTestEthernetPing()
        {
            _adcpConnection.TestEthernetConnection();
            this.NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);
        }

        #endregion

        #region Find ADCP

        private async Task<List<AdcpSerialPort.AdcpSerialOptions>> FindAdcp()
        {
            List<AdcpSerialPort.AdcpSerialOptions> serialConnOptions = new List<AdcpSerialPort.AdcpSerialOptions>();

            IsFindingAdcp = true;

            if (_adcpConnection.AdcpSerialPort != null)
            {
                _adcpConnection.DisconnectAdcpSerial();
                // Scan all the serial ports
                await Task.Run(() => serialConnOptions = _adcpConnection.ScanSerialConnection());

                // If any good serial ports were found, use the first serial port
                if (serialConnOptions.Count > 0)
                {
                    // Set the selected ports
                    SelectedAdcpCommPort = serialConnOptions.First().SerialOptions.Port;
                    //_serialOptions.Port = serialConnOptions.First().SerialOptions.Port;
                    SelectedAdcpBaudRate = serialConnOptions.First().SerialOptions.BaudRate;
                    //_serialOptions.BaudRate = serialConnOptions.First().SerialOptions.BaudRate;
                    //this.NotifyOfPropertyChange(() => this.SelectedCommPort);
                    //this.NotifyOfPropertyChange(() => this.SelectedBaud);

                    // Reconnect the ADCP serial connection
                    //ReconnectAdcpSerial(_serialOptions);
                }
            }

            IsFindingAdcp = false;

            return serialConnOptions;
        }

        #endregion

        #region EventHandler

        /// <summary>
        /// Event handler when receiving serial data.
        /// </summary>
        /// <param name="data">Data received from the serial port.</param>
        private void _adcpConnection_ReceiveDataEvent(byte[] data)
        {
            //this.NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);

            //// Recording
            //this.NotifyOfPropertyChange(() => this.IsRawAdcpRecording);
            //this.NotifyOfPropertyChange(() => this.RawAdcpBytesWritten);
            //this.NotifyOfPropertyChange(() => this.RawAdcpFileName);
        }

        #endregion

    }
}
