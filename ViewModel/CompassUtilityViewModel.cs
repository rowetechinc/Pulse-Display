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
 * 07/30/2013      RC          3.0.6      Initial coding
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 10/01/2014      RC          4.1.0      Added ability to record the compass data and set what output.
 * 01/12/2017      RC          4.4.4      Added all the compass mounting reference options.
 *
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using System.Threading;
    using System.Windows;
    using ReactiveUI;
    using System.Threading.Tasks;
    using System.IO;
    using System.Collections.Concurrent;
    using System.ComponentModel;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CompassUtilityViewModel : PulseViewModel, IDeactivate
    {
        #region Variables

        // Setup logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// EventAggregator to receive global events.
        /// This will be used to get ensembles for a serial
        /// number.
        /// </summary>
        private IEventAggregator _eventAggregator;

        /// <summary>
        /// Adcp Connection.
        /// </summary>
        private AdcpConnection _adcpConn;

        /// <summary>
        /// Codec to decode compass data.
        /// </summary>
        private PniPrimeCompassBinaryCodec _compassCodec;

        /// <summary>
        /// Binary writer for raw Compass data.
        /// </summary>
        private StreamWriter _rawCompassRecordWriter;

        /// <summary>
        /// Lock for the raw ADCP file.
        /// </summary>
        private object _rawCompassRecordFileLock = new object();

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<RTI.PniPrimeCompassBinaryCodec.PniDataResponse> _buffer;

        /// <summary>
        /// Flag for processing buffer.
        /// </summary>
        private bool _isProcessingBuffer;

        #endregion

        #region Properties

        #region Connection

        /// <summary>
        /// Flag if the ADCP is connected in compass mode.
        /// 
        /// This is mainly used to set the buttons on and off.
        /// The mode should be checked in the AdcpSerialPort if it
        /// is set correctly.
        /// AdcpSerialPort.IsCompassMode
        /// </summary>
        public bool IsCompassConnected
        {
            get 
            {
                if (_adcpConn == null)
                {
                    return false;
                }

                if (_adcpConn.AdcpSerialPort == null)
                {
                    return false;
                }

                return _adcpConn.AdcpSerialPort.IsCompassMode; 
            }
        }

        #endregion

        #region Show Config

        /// <summary>
        /// Result from sending the ShowConfig command.
        /// </summary>
        private string _ShowConfig;
        /// <summary>
        /// Result from sending the ShowConfig command.
        /// </summary>
        public string ShowConfig
        {
            get { return _ShowConfig; }
            set
            {
                _ShowConfig = value;
                this.NotifyOfPropertyChange(() => this.ShowConfig);
            }
        }

        #endregion

        #region Show Param

        /// <summary>
        /// Result from sending the ShowParam command.
        /// </summary>
        private string _ShowParam;
        /// <summary>
        /// Result from sending the ShowParam command.
        /// </summary>
        public string ShowParam
        {
            get { return _ShowParam; }
            set
            {
                _ShowParam = value;
                this.NotifyOfPropertyChange(() => this.ShowParam);
            }
        }

        #endregion

        #region Show Acq Param

        /// <summary>
        /// String for Acq Params.
        /// </summary>
        private string _ShowAcqParam;
        /// <summary>
        /// String for Acq Params.
        /// </summary>
        public string ShowAcqParam
        {
            get { return _ShowAcqParam; }
            set
            {
                _ShowAcqParam = value;
                this.NotifyOfPropertyChange(() => this.ShowAcqParam);
            }
        }

        #endregion

        #region Show Mod Info

        /// <summary>
        /// String for Mod Info.
        /// </summary>
        private string _ShowModInfo;
        /// <summary>
        /// String for Mod Info.
        /// </summary>
        public string ShowModInfo
        {
            get { return _ShowModInfo; }
            set
            {
                _ShowModInfo = value;
                this.NotifyOfPropertyChange(() => this.ShowModInfo);
            }
        }

        #endregion

        #region Sample Data

        /// <summary>
        /// Sample data from the ADCP compass.  This will be the
        /// heading, pitch and roll data.
        /// </summary>
        private string _SampleData;

        /// <summary>
        /// Sample data from the ADCP compass.  This will be the
        /// heading, pitch and roll data.
        /// </summary>
        public string SampleData
        {
            get { return _SampleData; }
            set
            {
                _SampleData = value;
                this.NotifyOfPropertyChange(() => this.SampleData);
            }
        }

        #endregion

        #region Record Data

        /// <summary>
        /// Turn on or off the compass recording.
        /// </summary>
        private bool _IsCompassRecording;
        /// <summary>
        /// Turn on or off the compass recording.
        /// </summary>
        public bool IsCompassRecording
        {
            get { return _IsCompassRecording; }
            set
            {
                _IsCompassRecording = value;
                this.NotifyOfPropertyChange(() => this.IsCompassRecording);

                // Turn on or off recording
                RecordData();
            }
        }

        /// <summary>
        /// Bytes written to the recording.
        /// </summary>
        private long _RawCompassBytesWritten;
        /// <summary>
        /// Bytes written to the recording.
        /// </summary>
        public long RawCompassBytesWritten
        {
            get { return _RawCompassBytesWritten; }
            set
            {
                _RawCompassBytesWritten = value;
                this.NotifyOfPropertyChange(() => this.RawCompassBytesWritten);
                this.NotifyOfPropertyChange(() => this.RawCompassByteWrittenStr);
            }
        }

        /// <summary>
        /// Compass bytes written to a string.
        /// </summary>
        public string RawCompassByteWrittenStr
        {
            get
            {
                return MathHelper.MemorySizeString(RawCompassBytesWritten);
            }
        }

        /// <summary>
        /// File name for the compass record.
        /// </summary>
        private string _RawCompassRecordFileName;
        /// <summary>
        /// File name for the compass record.
        /// </summary>
        public string RawCompassRecordFileName
        {
            get { return _RawCompassRecordFileName; }
            set
            {
                _RawCompassRecordFileName = value;
                this.NotifyOfPropertyChange(() => this.RawCompassRecordFileName);
            }
        }

        #endregion

        #region Mounting Reference

        /// <summary>
        /// Get a list for all mounting refernces..
        /// </summary>
        /// <returns>Get a list of all the mounting references.</returns>
        public BindingList<string> GetMountingRefList()
        {
            BindingList<string> MountingRefList = new BindingList<string>();
            //foreach (var pniRef in PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef)
            foreach(string pniRef in Enum.GetNames(typeof(PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef)))
            {
                MountingRefList.Add(pniRef);
            }

            return MountingRefList;
        }

        /// <summary>
        /// Selected mounting reference.
        /// </summary>
        private string _SelectedMountingRef;
        /// <summary>
        /// Set the selected mounting reference.
        /// </summary>
        public string SelectedMountingRef
        {
            get { return _SelectedMountingRef; }

            set
            {
                _SelectedMountingRef = value;
                this.NotifyOfPropertyChange(() => this.RawCompassRecordFileName);

            }
        }
        
        /// <summary>
        /// List of the Mounting references.
        /// </summary>
        public BindingList<string> MountingRefList
        {
            get
            {
                return GetMountingRefList();
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Connect the ADCP as Compass mode.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CompassConnectCommand { get; protected set; }

        /// <summary>
        /// Connect the ADCP as Compass mode.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CompassDisconnectCommand { get; protected set; }

        /// <summary>
        /// Command to show the config of the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ShowConfigCommand { get; protected set; }

        /// <summary>
        /// Command to show the paramaters of the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ShowParamCommand { get; protected set; }

        /// <summary>
        /// Command to show the AcqParam of the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ShowAcqParamCommand { get; protected set; }

        /// <summary>
        /// Command to show the Mod Info of the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ShowModInfoCommand { get; protected set; }

        /// <summary>
        /// Command to set the Mounting Ref.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> MountingRefCommand { get; protected set; }

        /// <summary>
        /// Command to set the Mounting Reference that is selected.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> MountingRefSelectedCommand { get; protected set; }

        /// <summary>
        /// Command to set the Taps.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> TapsCommand { get; protected set; }

        /// <summary>
        /// Command to set the polling command.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> PollingCommand { get; protected set; }

        /// <summary>
        /// Command to save the Compass calibration and configuration.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SaveCalConfigCommand { get; protected set; }

        /// <summary>
        /// Command to start or stop the Sample data.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SampleDataCommand { get; protected set; }

        /// <summary>
        /// Command to get data from the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GetDataCommand { get; protected set; }

        /// <summary>
        /// Command to power the compass on or off based off the param.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CompassPowerCommand { get; protected set; }

        /// <summary>
        /// Command to set all the default values for the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> DefaultCompassSettingsCommand { get; protected set; }

        /// <summary>
        /// Command to set data output to all values for the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SetAllDataOutputCommand { get; protected set; }

        /// <summary>
        /// Command to set data output to Heading, Pitch and Roll values for the compass.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SetHprDataOutputCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize object.
        /// </summary>
        public CompassUtilityViewModel()
            : base("Compass Utility")
        {
            // Initialize ranges
            _eventAggregator = IoC.Get<IEventAggregator>();
            _adcpConn = IoC.Get<AdcpConnection>();
            _compassCodec = new RTI.PniPrimeCompassBinaryCodec();
            IsCompassRecording = false;
            RawCompassBytesWritten = 0;
            RawCompassRecordFileName = "";
            _isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<RTI.PniPrimeCompassBinaryCodec.PniDataResponse>();

            SelectedMountingRef = MountingRefList[0];

            // Setup the compass event subscriptions
            SubscribeCompassEvents();

            // Commands
            // Command to connect to compass mode
            CompassConnectCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => !x.Value),
                                                                                _ => Task.Run(() => CompassConnect()));

            // Command to disconnect from compass mode
            CompassDisconnectCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => CompassDisconnect()));

            ShowConfigCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => ExecuteShowConfig()));

            ShowParamCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => ExecuteShowParam()));

            ShowAcqParamCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => ExecuteShowAcqParam()));

            ShowModInfoCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => ExecuteShowModInfo()));

            MountingRefCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                param => Task.Run(() => SetMountingRef(param)));

            MountingRefSelectedCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => SetMountingRefFromSelection()));

            TapsCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                param => Task.Run(() => SetTaps(param)));

            PollingCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                param => Task.Run(() => SetPolling(param)));

            SaveCalConfigCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => SaveCalAndSettings()));

            SampleDataCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                param => Task.Run(() => SetSampleData(param)));

            GetDataCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => Task.Run(() => GetData()));

            CompassPowerCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                param => Task.Run(() => SetCompassPower(param)));

            DefaultCompassSettingsCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => SetCompassDefaults());

            SetAllDataOutputCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => SetAllData());

            SetHprDataOutputCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsCompassConnected, x => x.Value),
                                                                                _ => SetHPRData());

            // Set compass connected for button activation
            this.NotifyOfPropertyChange(() => this.IsCompassConnected);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            // Unsubscribe
            UnsubscribeCompassEvents();

            // Stop Recording
            StopRecording();

            // Dispose codec
            _compassCodec.Dispose();
        }

        #region Connect Disconnect Compass

        /// <summary>
        /// Make the ADCP go into compasss mode.
        /// The ADCP will then only work for compass commands.
        /// The ADCP must be disconnected from the compass when complete.
        /// </summary>
        /// <returns>TRUE = Connected.</returns>
        private void CompassConnect()
        {
            // Send the commands
            // Put ADCP in Compass mode
            // Set the serial port to COMPASS mode to decode compass data
            if (!_adcpConn.AdcpSerialPort.StartCompassMode())
            {
                //SetStatusBar(new StatusEvent("Compass Issue.  Could not connect to compass.", MessageBoxImage.Error));
            }

            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);                  // Delay for 485 response

            // Clear the buffer of any data from last calibration
            _compassCodec.ClearIncomingData();

            // Set compass connected for button activation
            this.NotifyOfPropertyChange(() => this.IsCompassConnected);
        }

                /// <summary>
        /// Disconnect the ADCP from Compass mode.  This will
        /// send the command to disconnect the compass.  IT
        /// will then set the ADCP status to ADCP.
        /// </summary>
        private void CompassDisconnect()
        {
            // Turn on compass interval outputing
            // Stop ADCP from compass mode
            // Set the serial port to ADCP mode to decode ADCP data
            _adcpConn.AdcpSerialPort.StopCompassMode();

            // Set status that ADCP is connected
            //SetAdcpStatus(new AdcpStatus(eAdcpStatus.Connected));

            // Set compass connected for button activation
            this.NotifyOfPropertyChange(() => this.IsCompassConnected);
        }

        #endregion

        #region Send Compass Command

        /// <summary>
        /// Send a command an value to the ADCP.  This will
        /// send the command and if required a value to the adcp
        /// for the compass to process.
        /// </summary>
        /// <param name="id">Command ID.</param>
        /// <param name="value">Value for command if required.</param>
        private void SendCompassConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID id, object value)
        {
            // Connect the compass
            CompassConnect();

            // Send command to Read Compass data
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.SetConfigCommand(id, value));

            CompassDisconnect();
        }

        #endregion

        #region Show Config

        /// <summary>
        /// Send all the configuration commands to get the configuration of the compass.
        /// </summary>
        private void ExecuteShowConfig()
        {
            // Clear the current ShowConfig
            ShowConfig = "";

            // Get Declination
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kDeclination));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get TrueNorth
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kTrueNorth));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get BigEndian
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kBigEndian));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get MountingRef
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kMountingRef));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get UserCalStableCheck
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalStableCheck));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get UserCalNumPoints
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalNumPoints));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get UserCalAutoSampling
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Get kBaudRate
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kBaudRate));
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);
        }

        #endregion

        #region Show Parameters

        /// <summary>
        /// Send command to get the Param commands.
        /// </summary>
        private void ExecuteShowParam()
        {
            // Clear Param Results
            ShowParam = "";

            // Get Params
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetParamCommand());
            Thread.Sleep(AdcpSerialPort.WAIT_STATE * 2);
        }

        /// <summary>
        /// Display the PNI Parameter result.
        /// </summary>
        /// <param name="param">Parameters from the ADCP.</param>
        private void DisplayParamResponse(PniPrimeCompassBinaryCodec.PniParameters param)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Filter: Taps =");
            sb.AppendLine(param.Count.ToString());
            foreach (var tap in param.Taps)
            {
                sb.AppendFormat("{0}, ", tap.ToString("0.0000000000"));
            }

            ShowParam = sb.ToString();
        }

        #endregion

        #region Show AcqParams

        /// <summary>
        /// Clear the ShowAcqParam, then request it from the ADCP compass.
        /// </summary>
        private void ExecuteShowAcqParam()
        {
            ShowAcqParam = "";

            // Send command for Acq Params
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetAcqParamsCommand());
        }

        /// <summary>
        /// Display the AcqParams.
        /// </summary>
        /// <param name="param">Acq Params.</param>
        private void DisplayAcqParam(PniPrimeCompassBinaryCodec.PniAcqParam param)
        {
            ShowAcqParam += string.Format("Polling Mode: {0} \n", MathHelper.BoolToOnOffStr(param.PollingMode));
            ShowAcqParam += string.Format("Flush Filter: {0} \n", MathHelper.BoolToOnOffStr(param.FlushFilter));
            ShowAcqParam += string.Format("Sensor Acq Time: {0} \n", param.SensorAcqTime.ToString("0.00"));
            ShowAcqParam += string.Format("Interval Rsp Time: {0} \n", param.IntervalRespTime.ToString("0.00"));
        }

        #endregion

        #region Show ModInfo

        /// <summary>
        /// Clear the ShowModInfo, then request it from the ADCP compass.
        /// </summary>
        private void ExecuteShowModInfo()
        {
            ShowModInfo = "";

            // Send command for Acq Params
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetModInfoCommand());
        }

        /// <summary>
        /// Display the ModInfo.
        /// </summary>
        /// <param name="info">Info to display.</param>
        private void DisplayModInfo(PniPrimeCompassBinaryCodec.PniModInfo info)
        {
            ShowModInfo += string.Format("Type: {0} \n", info.Type);
            ShowModInfo += string.Format("Revision: {0} \n", info.Revision);
        }

        #endregion

        #region Set Mounting Ref

        /// <summary>
        /// Send the command to set the Mounting reference.
        /// This will cast the parameter to a string.
        /// This it will parse the string to an int.
        /// If that all works, then it will determine which angle as set the 
        /// mounting reference to the compass.
        /// </summary>
        /// <param name="param">Parameter of the command.</param>
        private void SetMountingRef(object param)
        {
            var paramStr = param as string;
            if (paramStr != null)
            {
                int result = 0;
                if (int.TryParse(paramStr, out result))
                {
                    switch(result)
                    {
                        case 0:
                            var cmd0 = PniPrimeCompassBinaryCodec.SetConfigCommand(PniPrimeCompassBinaryCodec.ID.kMountingRef, (byte)PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef.Standard);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(cmd0);
                            break;
                        case 90:
                            var cmd90 = PniPrimeCompassBinaryCodec.SetConfigCommand(PniPrimeCompassBinaryCodec.ID.kMountingRef, (byte)PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef.NEG_90);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(cmd90);
                            break;
                        case 180:
                            var cmd180 = PniPrimeCompassBinaryCodec.SetConfigCommand(PniPrimeCompassBinaryCodec.ID.kMountingRef, (byte)PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef.NEG_180);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(cmd180);
                            break;
                        case 270:
                            var cmd270 = PniPrimeCompassBinaryCodec.SetConfigCommand(PniPrimeCompassBinaryCodec.ID.kMountingRef, (byte)PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef.NEG_270);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(cmd270);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Send the command for the mounting reference for the given selected mounting reference.
        /// </summary>
        private void SetMountingRefFromSelection()
        {
            PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef choice;
            if (Enum.TryParse(SelectedMountingRef, out choice))
            {
                var cmd = PniPrimeCompassBinaryCodec.SetConfigCommand(PniPrimeCompassBinaryCodec.ID.kMountingRef, (byte)choice);
                _adcpConn.AdcpSerialPort.SendCompassCommand(cmd);
            }
        }

        #endregion

        #region Set Taps

        /// <summary>
        /// Send a command to the ADCP compass to set the Taps value.
        /// </summary>
        /// <param name="param">Paramater from the button for which taps.</param>
        private void SetTaps(object param)
        {
            var paramStr = param as string;
            if (paramStr != null)
            {
                int result = 0;
                if (int.TryParse(paramStr, out result))
                {
                    switch(result)
                    {
                        case 0:
                            // Get the X and P Axis command
                            byte[] xAxis = null;
                            byte[] pAxis = null;
                            PniPrimeCompassBinaryCodec.SetTaps0Commands(out xAxis, out pAxis);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(xAxis);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(pAxis);
                            break;
                        case 4:
                            // Get the X and P Axis command
                            byte[] xAxis_4 = null;
                            byte[] pAxis_4 = null;
                            PniPrimeCompassBinaryCodec.SetTaps4Commands(out xAxis_4, out pAxis_4);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(xAxis_4);
                            Thread.Sleep(AdcpSerialPort.WAIT_STATE);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(pAxis_4);
                            break;
                        case 8:
                            // Get the X and P Axis command
                            byte[] xAxis_8 = null;
                            byte[] pAxis_8 = null;
                            PniPrimeCompassBinaryCodec.SetTaps8Commands(out xAxis_8, out pAxis_8);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(xAxis_8);
                            Thread.Sleep(AdcpSerialPort.WAIT_STATE);
                            _adcpConn.AdcpSerialPort.SendCompassCommand(pAxis_8);
                            break;
                        default:
                            break;
                    }

                }
            }
        }

        #endregion

        #region Set Polling

        /// <summary>
        /// Set the polling value based off the param given.
        /// The param will be converted to a string.
        /// </summary>
        /// <param name="param">Param either "on" or "off".</param>
        private void SetPolling(object param)
        {
            var paramStr = param as string;
            if (paramStr != null)
            {
                switch(paramStr)
                {
                    case "on":
                        var paramOn = new PniPrimeCompassBinaryCodec.PniAcqParam();
                        paramOn.PollingMode = true;
                        var paramOnCmd = PniPrimeCompassBinaryCodec.SetAcqParamsCommand(paramOn);
                        _adcpConn.AdcpSerialPort.SendCompassCommand(paramOnCmd);
                        break;
                    case "off":
                        var paramOff = new PniPrimeCompassBinaryCodec.PniAcqParam();
                        paramOff.PollingMode = false;
                        var paramOffCmd = PniPrimeCompassBinaryCodec.SetAcqParamsCommand(paramOff);
                        _adcpConn.AdcpSerialPort.SendCompassCommand(paramOffCmd);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Save Compass Cal and Settings

        /// <summary>
        /// Send the Save Compass Cal and Settings command.
        /// </summary>
        private void SaveCalAndSettings()
        {
            byte[] SaveCalDataCmd = PniPrimeCompassBinaryCodec.SaveCompassCalCommand();
            _adcpConn.AdcpSerialPort.SendCompassCommand(SaveCalDataCmd);
        }


        #endregion

        #region Sample Data

        /// <summary>
        /// Send the command to stop or stop the interval mode
        /// based off the param given.
        /// The params will be a string of either "start" or
        /// "stop".
        /// </summary>
        /// <param name="param">Param to start or stop the interval mode.</param>
        private void SetSampleData(object param)
        {
            var paramStr = param as string;
            if (paramStr != null)
            {
                switch(paramStr)
                {
                    case "start":
                        _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.StartIntervalModeCommand());
                        break;
                    case "stop":
                        _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.StopIntervalModeCommand());
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Get Data

        /// <summary>
        ///  Send a command to get Data from the ADCP compass.
        /// </summary>
        private void GetData()
        {
            // Clear buffer
            SampleData = "";

            // Send command to get data
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetDataCommand());
        }

        /// <summary>
        /// Send a command to get all Data from the ADCP compass.
        /// This will get all the possible values from the compass.
        ///  
        /// kHeading
        /// kPAngle
        /// kRAngle
        /// 
        /// </summary>
        private async Task SetHPRData()
        {
            // Send command to get data
            await Task.Run(() => _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.SetHPRDataComponentsCommands()));
        }

        /// <summary>
        /// Send a command to get all Data from the ADCP compass.
        /// This will get all the possible values from the compass.
        ///  
        /// kHeading
        /// kPAngle
        /// kRAngle
        /// kDistortion
        /// kCalStatus
        /// kPAligned
        /// kRAligned
        /// kIZAligned
        /// kXAligned
        /// kYAligned
        /// kZAligned
        /// 
        /// </summary>
        private async Task SetAllData()
        {
            // Send command to get data
            await Task.Run(() => _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.SetAllDataComponentsCommand()));
        }

        #endregion

        #region Power Up Down

        /// <summary>
        /// Send the power command based off the param given.
        /// The param will be a string eith either "up" or "down".
        /// </summary>
        /// <param name="param">Param from the power command.</param>
        private void SetCompassPower(object param)
        {
            var paramStr = param as string;
            if (paramStr != null)
            {
                switch(paramStr)
                {
                    case "up":
                        _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.PowerUpCommand());
                        break;
                    case "down":
                        _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.PowerDownCommand());
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Set Defaults

        /// <summary>
        /// Set the default values for the compass.
        /// This will set:
        /// Mounting Ref = Standard
        /// Taps = 4
        /// Polling = Off
        /// Start Sampling data
        /// Save settings.
        /// </summary>
        private async Task SetCompassDefaults()
        {
            await Task.Run(() => SetMountingRef("0"));            // Mounting Ref = Standard
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            await Task.Run(() => SetTaps("4"));                   // Taps = 4
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            await Task.Run(() => SetPolling("off"));              // Polling = Off
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            await Task.Run(() => SetSampleData("start"));         // Start Sampling data
            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            await Task.Run(() => SaveCalAndSettings());           // Save the settings

            // Show Results
            await Task.Run(() => ExecuteShowConfig());
            await Task.Run(() => ExecuteShowParam());
            await Task.Run(() => ExecuteShowAcqParam());
            await Task.Run(() => ExecuteShowModInfo());
        }

        #endregion

        #region Event Subscriptions

        /// <summary>
        /// Create new serial ports with the options
        /// from the serial options.
        /// This will also subscribe to receive events
        /// from the serial ports and clear the buffers.
        /// </summary>
        private void SubscribeCompassEvents()
        {
            // If the serial port was previous connected, 
            // Unsubscribe events.
            if (_adcpConn.AdcpSerialPort != null)
            {
                UnsubscribeCompassEvents();

                // Subscribe to receive event when compass data received
                _adcpConn.AdcpSerialPort.ReceiveCompassSerialDataEvent += new RTI.AdcpSerialPort.ReceiveCompassSerialDataEventHandler(On_ReceiveCompassSerialDataEvent);

                // Wait for incoming cal samples
                _compassCodec.CompassEvent += new RTI.PniPrimeCompassBinaryCodec.CompassEventHandler(CompassCodecEventHandler);
            }
        }

        /// <summary>
        /// Unsubscribe from the ADCP serial port events.
        /// </summary>
        private void UnsubscribeCompassEvents()
        {
            _adcpConn.AdcpSerialPort.ReceiveCompassSerialDataEvent -= On_ReceiveCompassSerialDataEvent;

            _compassCodec.CompassEvent -= CompassCodecEventHandler;
        }

        #endregion

        #region Record Data

        /// <summary>
        /// Turn on recording.
        /// This will create the binary writer.
        /// </summary>
        private void RecordData()
        {
            if (_IsCompassRecording)
            {
                StartRecording(Pulse.Commons.DEFAULT_RECORD_DIR);
            }
            else
            {
                StopRecording();
            }
        }

        /// <summary>
        /// Create the recorder.
        /// </summary>
        /// <param name="dir">Directory to write the data to.</param>
        private void StartRecording(string dir)
        {
            // Create the writer if it does not exist
            if (_rawCompassRecordWriter == null)
            {
                // Create a file name
                DateTime currDateTime = DateTime.Now;

                string filename = string.Format("RawCompass_{0:yyyyMMddHHmmss}.csv", currDateTime);
                string filePath = string.Format("{0}\\{1}", dir, filename);

                try
                {
                    // Open the binary writer
                    _rawCompassRecordWriter = new StreamWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                    // Set the raw ADCP file name
                    RawCompassRecordFileName = filePath;

                    // Reset the number of bytes written
                    RawCompassBytesWritten = 0;

                    // Write the data to the file
                    _rawCompassRecordWriter.Write(GetCompassDataHeader());
                }
                catch (Exception e)
                {
                    log.Error("Error creating the raw ADCP file.", e);
                }
            }
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        private void StopRecording()
        {
            try
            {
                if (_rawCompassRecordWriter != null)
                {
                    // Flush and close the writer
                    _rawCompassRecordWriter.Flush();
                    _rawCompassRecordWriter.Close();
                    _rawCompassRecordWriter.Dispose();
                    _rawCompassRecordWriter = null;
                }
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Raw Compass Record.", e);
            }
        }

        /// <summary>
        /// Record compass data.
        /// </summary>
        /// <param name="data">PNI Compass data.</param>
        public async Task RecordCompassData(RTI.PniPrimeCompassBinaryCodec.PniDataResponse data)
        {
             // Verify recording is turned on
            if (_IsCompassRecording)
            {
                _buffer.Enqueue(data);

                // Execute async
                if (!_isProcessingBuffer)
                {
                    // Execute async
                    await Task.Run(() => RecordCompassDataExecute());
                }
            }
        }

        /// <summary>
        /// Record the compass data.
        /// </summary>
        private void RecordCompassDataExecute()
        {
            while (!_buffer.IsEmpty)
            {
                _isProcessingBuffer = true;

                // Verify writer is created
                if (_rawCompassRecordWriter != null)
                {
                    try
                    {
                        // Get the latest data from the buffer
                        RTI.PniPrimeCompassBinaryCodec.PniDataResponse data = null;
                        if (_buffer.TryDequeue(out data))
                        {
                            // Seen thread exceptions for trying to have
                            // multiple threads write at the same time.
                            // The serial data is coming in and it is not writing fast enough
                            lock (_rawCompassRecordFileLock)
                            {
                                string compassData = GetCompassData(data);

                                // Write the data to the file
                                _rawCompassRecordWriter.Write(compassData);

                                // Accumulate the number of bytes written
                                RawCompassBytesWritten += compassData.Length;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing lake test data
                        log.Error("Error raw ADCP data..", e);
                    }
                }
            }

            // Turn off processing
            _isProcessingBuffer = false;
        }

        /// <summary>
        /// Get the compass data header.
        /// </summary>
        /// <returns>Header for the data file.</returns>
        private string GetCompassDataHeader()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Heading (Deg),");
            sb.Append("Pitch (Deg),");
            sb.Append("Roll (Deg),");
            sb.Append("pAligned (G),");
            sb.Append("rAligned (G),");
            sb.Append("izAligned (G),");
            sb.Append("xAligned (uT),");
            sb.Append("yAligned (uT),");
            sb.Append("zAligned (uT),");
            sb.Append("CalStatus,");
            sb.Append("Distortion,");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Create a CSV of the compass data.
        /// </summary>
        /// <param name="data">Data to decode.</param>
        private string GetCompassData(RTI.PniPrimeCompassBinaryCodec.PniDataResponse data)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(data.Heading + ",");
            sb.Append(data.Pitch + ",");
            sb.Append(data.Roll + ",");
            sb.Append(data.pAligned + ",");
            sb.Append(data.rAligned + ",");
            sb.Append(data.izAligned + ",");
            sb.Append(data.xAligned + ",");
            sb.Append(data.yAligned + ",");
            sb.Append(data.zAligned + ",");

            if (data.CalStatus)
            {
                sb.Append(1 + ",");
            }
            else
            {
                sb.Append(0 + ",");
            }

            if (data.Distortion)
            {
                sb.Append(1);
            }
            else
            {
                sb.Append(0);
            }

            sb.AppendLine();

            return sb.ToString();
        }

        #endregion

        #region Display Data

        /// <summary>
        /// Display the compass data response.
        /// </summary>
        /// <param name="compassDataResponse">Compass data result.</param>
        private void DisplayCompassResponse(RTI.PniPrimeCompassBinaryCodec.PniDataResponse compassDataResponse)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Heading: {0} Deg", compassDataResponse.Heading));
            sb.AppendLine(string.Format("Pitch:   {0} Deg", compassDataResponse.Pitch));
            sb.AppendLine(string.Format("Roll:    {0} Deg", compassDataResponse.Roll));

            if (compassDataResponse.CalStatus)
            {
                sb.AppendLine(string.Format("Cal Status: Calibrated"));
            }
            else
            {
                sb.AppendLine(string.Format("Cal Status: Not Calibrated"));
            }

            if (compassDataResponse.Distortion)
            {
                sb.AppendLine(string.Format("Distoriation: Distoriation"));
            }
            else
            {
                sb.AppendLine(string.Format("Distoriation: No Distoriation"));
            }

            sb.AppendLine(string.Format("pAligned:  {0} G", compassDataResponse.pAligned));
            sb.AppendLine(string.Format("rAligned:  {0} G", compassDataResponse.rAligned));
            sb.AppendLine(string.Format("izAligned: {0} G", compassDataResponse.izAligned));
            sb.AppendLine(string.Format("xAligned:  {0} uT", compassDataResponse.xAligned));
            sb.AppendLine(string.Format("yAligned:  {0} uT", compassDataResponse.yAligned));
            sb.AppendLine(string.Format("zAligned:  {0} uT", compassDataResponse.zAligned));

            SampleData = sb.ToString();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Pass data from the ADCP serial port to the 
        /// compass codec to be parsed.
        /// </summary>
        /// <param name="data">Data received from the ADCP serial port in Compass mode.</param>
        private void On_ReceiveCompassSerialDataEvent(byte[] data)
        {
            _compassCodec.AddIncomingData(data);
        }

        /// <summary>
        /// When the serial port receives a new cal sample
        /// from the Compass calibration, set the new sample value.
        /// </summary>
        /// <param name="data">Data from the compass.</param>
        public void CompassCodecEventHandler(PniPrimeCompassBinaryCodec.CompassEventArgs data)
        {
            // Declination
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kDeclination)
            {
                var val = (float)data.Value;
                ShowConfig += "Declination: " + val + "\n";
            }

            // True North
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kTrueNorth || data.EventType == PniPrimeCompassBinaryCodec.ID.kModInfoResp)
            {
                var param = data.Value as PniPrimeCompassBinaryCodec.PniModInfo;
                if (param != null)
                {
                    DisplayModInfo(param);
                    return;
                }

                try
                {
                    var val = (bool)data.Value;
                    ShowConfig += "True North: " + MathHelper.BoolToOnOffStr(val) + "\n";
                }
                catch (Exception) { }
            }

            // kBigEndian
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kBigEndian)
            {
                var val = (bool)data.Value;
                ShowConfig += "Big Endian: " + MathHelper.BoolToOnOffStr(val) + "\n";
            }

            // kMountingRef
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kMountingRef)
            {
                var val = (int)data.Value;
                ShowConfig += "Mounting Ref: " + PniPrimeCompassBinaryCodec.PniConfiguration.MountingRefToString((PniPrimeCompassBinaryCodec.PniConfiguration.PniMountingRef)val) + "\n";
            }

            // kUserCalStableCheck
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kUserCalStableCheck)
            {
                var val = (bool)data.Value;
                ShowConfig += "User Calibration Stable Check: " + MathHelper.BoolToOnOffStr(val) + "\n";
            }

            // kUserCalNumPoints
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kUserCalNumPoints)
            {
                var val = (uint)data.Value;
                ShowConfig += "User Calibration Number of Samples: " + val + "\n";
            }

            // kUserCalAutoSampling
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling)
            {
                var val = (bool)data.Value;
                ShowConfig += "User Calibration Auto Sample: " + MathHelper.BoolToOnOffStr(val) + "\n";
            }

            // kParamResp or kBaudRate
            // They have the same value
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kBaudRate || data.EventType == PniPrimeCompassBinaryCodec.ID.kParamResp)
            {
                // kParamResp
                // Try as kParamResp
                var param = data.Value as PniPrimeCompassBinaryCodec.PniParameters;
                if (param != null)
                {
                    DisplayParamResponse(param);
                    return; // Stop here
                }

                // kBaudRate
                // Try as kBaudRate
                try
                {
                    var val = (int)data.Value;
                    ShowConfig += "BaudRate: " + PniPrimeCompassBinaryCodec.PniConfiguration.BaudRateToString(val) + "\n";
                }
                catch (InvalidCastException) { return; }    // If fails do nothing
            }

            // kAcqParamsResp
            if (data.EventType == PniPrimeCompassBinaryCodec.ID.kAcqParamsResp)
            {
                var param = data.Value as PniPrimeCompassBinaryCodec.PniAcqParam;
                if (param != null)
                {
                    DisplayAcqParam(param);
                }
            }

            // kDataResp
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kDataResp)
            {
                RTI.PniPrimeCompassBinaryCodec.PniDataResponse compassDataResponse = data.Value as RTI.PniPrimeCompassBinaryCodec.PniDataResponse;
                if (compassDataResponse != null)
                {
                    // Display the data
                    DisplayCompassResponse(compassDataResponse);

                    //Record the data
                    Task.Run(() => RecordCompassData(compassDataResponse));
                }
            }


        }

        #endregion

        #region IDeactivate

        /// <summary>
        /// Attemp to deactivate.
        /// </summary>
        event EventHandler<DeactivationEventArgs> IDeactivate.AttemptingDeactivation
        {
            add {  }
            remove {  }
        }

        /// <summary>
        /// Deactivate.
        /// </summary>
        /// <param name="close">Flag if closing.</param>
        void IDeactivate.Deactivate(bool close)
        {
            // Just in case the user forgot to disconnect the compass
            if (close)
            {
                CompassDisconnect();
            }
        }

        /// <summary>
        /// Event for deactivating.
        /// </summary>
        event EventHandler<DeactivationEventArgs> IDeactivate.Deactivated
        {
            add {  }
            remove {  }
        }

        #endregion
    }
}
