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
 * 11/02/2015      RC          4.1.0      Initial coding
 * 
 */

using Caliburn.Micro;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    public class DiagnosticsViewModel : PulseViewModel, IHandle<EnsembleEvent>, IHandle<ProjectEvent>, IHandle<SelectedEnsembleEvent>
    {

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Event aggregator.
        /// </summary>
        private IEventAggregator _events;

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<DataSet.Ensemble> _buffer;

        /// <summary>
        /// Flag to know if processing the buffer.
        /// </summary>
        private bool _isProcessingBuffer;

        /// <summary>
        /// Timer to reduce the number of update calls the terminal window.
        /// </summary>
        private System.Timers.Timer _displayTimer;

        /// <summary>
        /// Ensemble to display.
        /// </summary>
        private DataSet.Ensemble _ensemble;

        /// <summary>
        /// Last ping time.
        /// </summary>
        private float _lastPingTime;

        #endregion

        #region Properties

        #region Configuration

        /// <summary>
        /// Subsystem Data Configuration for this view.
        /// </summary>
        private SubsystemDataConfig _Config;
        /// <summary>
        /// Subsystem Data Configuration for this view.
        /// </summary>
        public SubsystemDataConfig Config
        {
            get { return _Config; }
            set
            {
                _Config = value;
                this.NotifyOfPropertyChange(() => this.Config);
                this.NotifyOfPropertyChange(() => this.IsPlayback);
            }
        }



        #endregion

        #region Display

        /// <summary>
        /// Display the CEPO index to describe this view model.
        /// </summary>
        public string Display
        {
            get
            {
                return _Config.IndexCodeString();
            }
        }

        /// <summary>
        /// Display the CEPO index to describe this view model.
        /// </summary>
        public string Title
        {
            get
            {
                return string.Format("[{0}]{1}", _Config.CepoIndex.ToString(), _Config.SubSystem.CodedDescString());
            }
        }

        /// <summary>
        /// Flag if this view will display playback or live data.
        /// TRUE = Playback Data
        /// </summary>
        public bool IsPlayback
        {
            get
            {
                if (_Config.Source == EnsembleSource.Playback)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Flag if the data came from the serial port.
        /// </summary>
        public bool IsSerial
        {
            get
            {
                if (_Config.Source == EnsembleSource.Serial)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Flag if the data came from the Long Term Average.
        /// </summary>
        public bool IsLta
        {
            get
            {
                if (_Config.Source == EnsembleSource.LTA)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Flag if the data came from the Short Term Average.
        /// </summary>
        public bool IsSta
        {
            get
            {
                if (_Config.Source == EnsembleSource.STA)
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Ensemble Data

        /// <summary>
        /// Ensemble date and time.
        /// </summary>
        public string EnsembleDateTime
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.EnsDateString + " " + _ensemble.EnsembleData.EnsTimeString;
                }
                return "";
            }
        }

        /// <summary>
        /// Ensemble number.
        /// </summary>
        public string EnsembleNumber
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.EnsembleNumber.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Firmware.
        /// </summary>
        public string Firmware
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.SysFirmware.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Serial Number.
        /// </summary>
        public string SerialNumber
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.SysSerialNumber.SystemSerialNumber.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Subsystems.
        /// </summary>
        public string Subsystems
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.SysSerialNumber.SubSystems.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Number of beams.
        /// </summary>
        public string NumBeams
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.NumBeams.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Number of bins.
        /// </summary>
        public string NumBins
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.NumBins.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Actual Ping Count.
        /// </summary>
        public string ActualPingCount
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.ActualPingCount.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Desired Ping Count.
        /// </summary>
        public string DesiredPingCount
        {
            get
            {
                if (_ensemble.IsEnsembleAvail)
                {
                    return _ensemble.EnsembleData.DesiredPingCount.ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// First Ping Time.
        /// </summary>
        public string FirstPingTime
        {
            get
            {
                if (_ensemble.IsAncillaryAvail)
                {
                    return _ensemble.AncillaryData.FirstPingTime.ToString() + " s";
                }
                return "";
            }
        }

        /// <summary>
        /// Last Ping Time.
        /// </summary>
        public string LastPingTime
        {
            get
            {
                if (_ensemble.IsAncillaryAvail)
                {
                    return _ensemble.AncillaryData.LastPingTime.ToString() + " s";
                }
                return "";
            }
        }

        /// <summary>
        /// Blank.
        /// </summary>
        public string Blank
        {
            get
            {
                if (_ensemble.IsAncillaryAvail)
                {
                    return _ensemble.AncillaryData.FirstBinRange.ToString("0.0") + " m";
                }
                return "";
            }
        }

        /// <summary>
        /// Blank.
        /// </summary>
        public string BinSize
        {
            get
            {
                if (_ensemble.IsAncillaryAvail)
                {
                    return _ensemble.AncillaryData.BinSize.ToString("0.0") + " m";
                }
                return "";
            }
        }

        #endregion

        #region Available

        /// <summary>
        /// Water Profile Available.
        /// </summary>
        public bool IsWpAvail
        {
            get
            {
                return _ensemble.IsEarthVelocityAvail;
            }
        }

        /// <summary>
        /// Bottom Track Available.
        /// </summary>
        public bool IsBtAvail
        {
            get
            {
                return _ensemble.IsBottomTrackAvail;
            }
        }

        /// <summary>
        /// Water Track Available.
        /// </summary>
        public bool IsWtAvail
        {
            get
            {
                return _ensemble.IsEarthWaterMassAvail;
            }
        }

        #endregion

        #region Nmea Properties

        /// <summary>
        /// Determine if there is a GPS fix.
        /// </summary>
        private string _gpsFix;
        /// <summary>
        /// GPS fix property.
        /// </summary>
        public string GpsFix
        {
            get { return _gpsFix; }
            set
            {
                _gpsFix = value;
                this.NotifyOfPropertyChange(() => this.GpsFix);
            }
        }

        /// <summary>
        /// Determine if there is a good GPS fix.
        /// </summary>
        private bool _IsGoodGps;
        /// <summary>
        /// GPS fix property.
        /// </summary>
        public bool IsGoodGps
        {
            get { return _IsGoodGps; }
            set
            {
                _IsGoodGps = value;
                this.NotifyOfPropertyChange(() => this.IsGoodGps);
            }
        }

        /// <summary>
        /// Latitude based off GPS data.
        /// </summary>
        private string _gpsLatitude;
        /// <summary>
        /// Latitude property.
        /// </summary>
        public string GpsLatitude
        {
            get { return _gpsLatitude; }
            set
            {
                _gpsLatitude = value;
                this.NotifyOfPropertyChange(() => this.GpsLatitude);
            }
        }

        /// <summary>
        /// GPS Longitude based off GPS data.
        /// </summary>
        private string _gpsLongitude;
        /// <summary>
        /// Longitude property.
        /// </summary>
        public string GpsLongitude
        {
            get { return _gpsLongitude; }
            set
            {
                _gpsLongitude = value;
                this.NotifyOfPropertyChange(() => this.GpsLongitude);
            }
        }

        /// <summary>
        /// Altitude based off GPS data.
        /// </summary>
        private string _gpsAltitude;
        /// <summary>
        /// Alitude property.
        /// </summary>
        public string GpsAltitude
        {
            get { return _gpsAltitude; }
            set
            {
                _gpsAltitude = value;
                this.NotifyOfPropertyChange(() => this.GpsAltitude);
            }
        }

        /// <summary>
        /// Speed based off GPS data.
        /// </summary>
        private string _gpsSpeed;
        /// <summary>
        /// Speed property.
        /// </summary>
        public string GpsSpeed
        {
            get { return _gpsSpeed; }
            set
            {
                _gpsSpeed = value;
                this.NotifyOfPropertyChange(() => this.GpsSpeed);
            }
        }

        #endregion

        #region Temp Data

        /// <summary>
        /// Determine if there is a good temperature.
        /// </summary>
        private bool _IsGoodTemp;
        /// <summary>
        /// Determine if there is a good temperature.
        /// </summary>
        public bool IsGoodTemp
        {
            get { return _IsGoodTemp; }
            set
            {
                _IsGoodTemp = value;
                this.NotifyOfPropertyChange(() => this.IsGoodTemp);
            }
        }

        /// <summary>
        /// System Temperature.
        /// </summary>
        private string _SystemTemp;
        /// <summary>
        /// System Temperature.
        /// </summary>
        public string SystemTemp
        {
            get { return _SystemTemp; }
            set
            {
                _SystemTemp = value;
                this.NotifyOfPropertyChange(() => this.SystemTemp);
            }
        }

        /// <summary>
        /// Water Temperature.
        /// </summary>
        private string _WaterTemp;
        /// <summary>
        /// Water Temperature.
        /// </summary>
        public string WaterTemp
        {
            get { return _WaterTemp; }
            set
            {
                _WaterTemp = value;
                this.NotifyOfPropertyChange(() => this.WaterTemp);
            }
        }

        #endregion

        #region Voltage Data

        /// <summary>
        /// Determine if there is a good voltage.
        /// </summary>
        private bool _IsGoodVoltage;
        /// <summary>
        /// Determine if there is a good voltage.
        /// </summary>
        public bool IsGoodVoltage
        {
            get { return _IsGoodVoltage; }
            set
            {
                _IsGoodVoltage = value;
                this.NotifyOfPropertyChange(() => this.IsGoodVoltage);
            }
        }

        /// <summary>
        /// System Temperature.
        /// </summary>
        private string _Voltage;
        /// <summary>
        /// System Temperature.
        /// </summary>
        public string Voltage
        {
            get { return _Voltage; }
            set
            {
                _Voltage = value;
                this.NotifyOfPropertyChange(() => this.Voltage);
            }
        }

        #endregion

        #region Pressure Data

        /// <summary>
        /// Determine if there is a good Pressure.
        /// </summary>
        private bool _IsGoodPressure;
        /// <summary>
        /// Determine if there is a good Pressure.
        /// </summary>
        public bool IsGoodPressure
        {
            get { return _IsGoodPressure; }
            set
            {
                _IsGoodPressure = value;
                this.NotifyOfPropertyChange(() => this.IsGoodPressure);
            }
        }

        /// <summary>
        /// Pressure.
        /// </summary>
        private string _Pressure;
        /// <summary>
        /// Pressure.
        /// </summary>
        public string Pressure
        {
            get { return _Pressure; }
            set
            {
                _Pressure = value;
                this.NotifyOfPropertyChange(() => this.Pressure);
            }
        }

        /// <summary>
        /// Transducer Depth.
        /// </summary>
        private string _TransducerDepth;
        /// <summary>
        /// Transducer Depth.
        /// </summary>
        public string TransducerDepth
        {
            get { return _TransducerDepth; }
            set
            {
                _TransducerDepth = value;
                this.NotifyOfPropertyChange(() => this.TransducerDepth);
            }
        }

        #endregion

        #region Salinity Data

        /// <summary>
        /// Determine the water type.
        /// </summary>
        private string _WaterType;
        /// <summary>
        /// Determine the water type.
        /// </summary>
        public string WaterType
        {
            get { return _WaterType; }
            set
            {
                _WaterType = value;
                this.NotifyOfPropertyChange(() => this.WaterType);
            }
        }

        /// <summary>
        /// Salinity.
        /// </summary>
        private string _Salinity;
        /// <summary>
        /// Salinity.
        /// </summary>
        public string Salinity
        {
            get { return _Salinity; }
            set
            {
                _Salinity = value;
                this.NotifyOfPropertyChange(() => this.Salinity);
            }
        }

        #endregion

        #region Status Data

        /// <summary>
        /// Determine if ADCP is good.
        /// </summary>
        private bool _IsGoodWpStatus;
        /// <summary>
        /// Determine if ADCP is good.
        /// </summary>
        public bool IsGoodWpStatus
        {
            get { return _IsGoodWpStatus; }
            set
            {
                _IsGoodWpStatus = value;
                this.NotifyOfPropertyChange(() => this.IsGoodWpStatus);
            }
        }

        /// <summary>
        /// ADCP Status
        /// </summary>
        private string _WpStatus;
        /// <summary>
        /// ADCP Status
        /// </summary>
        public string WpStatus
        {
            get { return _WpStatus; }
            set
            {
                _WpStatus = value;
                this.NotifyOfPropertyChange(() => this.WpStatus);
            }
        }

        /// <summary>
        /// Determine if BT is good.
        /// </summary>
        private bool _IsGoodBtStatus;
        /// <summary>
        /// Determine if BT is good.
        /// </summary>
        public bool IsGoodBtStatus
        {
            get { return _IsGoodBtStatus; }
            set
            {
                _IsGoodBtStatus = value;
                this.NotifyOfPropertyChange(() => this.IsGoodBtStatus);
            }
        }

        /// <summary>
        /// BT Status
        /// </summary>
        private string _BtStatus;
        /// <summary>
        /// BT Status
        /// </summary>
        public string BtStatus
        {
            get { return _BtStatus; }
            set
            {
                _BtStatus = value;
                this.NotifyOfPropertyChange(() => this.BtStatus);
            }
        }

        #endregion

        #region Ensemble Data

        /// <summary>
        /// Ping Time.
        /// </summary>
        private string _PingTime;
        /// <summary>
        /// Ping Time.
        /// </summary>
        public string PingTime
        {
            get { return _PingTime; }
            set
            {
                _PingTime = value;
                this.NotifyOfPropertyChange(() => this.PingTime);
            }
        }

        /// <summary>
        /// Profile Range.
        /// </summary>
        private string _ProfileRange;
        /// <summary>
        /// Profile Range.
        /// </summary>
        public string ProfileRange
        {
            get { return _ProfileRange; }
            set
            {
                _ProfileRange = value;
                this.NotifyOfPropertyChange(() => this.ProfileRange);
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to close this VM.
        /// </summary>
        public ReactiveCommand<object> CloseVMCommand { get; protected set; }

        #endregion

        public DiagnosticsViewModel(SubsystemDataConfig config)
            : base("DiagnosticsViewModel")
        {
            // Set Subsystem 
            _Config = config;

            // Get the Event Aggregator
            _events = IoC.Get<IEventAggregator>();

            _isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<DataSet.Ensemble>();

            _ensemble = new DataSet.Ensemble();

            // Close the VM
            CloseVMCommand = ReactiveCommand.Create();
            CloseVMCommand.Subscribe(_ => _events.PublishOnUIThread(new CloseVmEvent(_Config)));

            // Update the display
            _displayTimer = new System.Timers.Timer(250);
            _displayTimer.Elapsed += _displayTimer_Elapsed;
            _displayTimer.AutoReset = true;
            _displayTimer.Enabled = true;
            _displayTimer.Start();

            Init();

            _events.Subscribe(this);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            _displayTimer.Stop();
        }

        #region Update Display Timer

        /// <summary>
        /// Reduce the number of times the display is updated.
        /// This will update the display based off the timer and not
        /// based off when data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _displayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.NotifyOfPropertyChange(null);
        }

        #endregion

        #region Init

        private void Init()
        {
            SystemTemp = "0.0 °F";
            WaterTemp = "0.0 °F";
            IsGoodTemp = false;

            Voltage = "0.0v";
            IsGoodVoltage = false;

            Salinity = "0.0 ppt";
            WaterType = "FRESH";

            TransducerDepth = "0.0 m";
            Pressure = "0.0 Pa";
            IsGoodPressure = false;

            WpStatus = "";
            IsGoodWpStatus = false;
            BtStatus = "";
            IsGoodBtStatus = false;

            ProfileRange = "";
            PingTime = "";

            _lastPingTime = 0.0f;

            SetGpsDataDefault();
        }

        #endregion

        #region Display Data

        /// <summary>
        /// Display the given ensemble.
        /// </summary>
        /// <param name="ensemble">Ensemble to display.</param>
        public async Task DisplayData(DataSet.Ensemble ensemble)
        {
            _buffer.Enqueue(ensemble);

            // Execute async
            if (!_isProcessingBuffer)
            {
                // Execute async
                await Task.Run(() => DisplayDataExecute());
            }
        }

        /// <summary>
        /// Execute the displaying of the data async.
        /// </summary>
        private void DisplayDataExecute()
        {
            while (!_buffer.IsEmpty)
            {
                _isProcessingBuffer = true;

                // Get the latest data from the buffer
                DataSet.Ensemble ensemble = null;
                if (_buffer.TryDequeue(out ensemble))
                {
                    // Verify the ensemble is good
                    if (ensemble == null || ensemble.EnsembleData == null || !ensemble.IsEnsembleAvail)
                    {
                        _isProcessingBuffer = false;
                        continue;
                    }

                    // If no subsystem is given, then a project is not selected
                    // So receive all data and display
                    // If the serial number is not set, this may be an old ensemble
                    // Try to display it anyway
                    if (!_Config.SubSystem.IsEmpty() && !ensemble.EnsembleData.SysSerialNumber.IsEmpty())
                    {
                        // Verify the subsystem matches this viewmodel's subystem.
                        if ((_Config.SubSystem != ensemble.EnsembleData.GetSubSystem())        // Check if Subsystem matches 
                                || (_Config != ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
                        {
                            _isProcessingBuffer = false;
                            continue;
                        }
                    }

                    try
                    {
                        _ensemble = ensemble;
                        //this.NotifyOfPropertyChange(null);

                        // Set GPS Data
                        SetGpsData(ensemble);

                        // Set Voltage Data
                        SetVoltageData(ensemble);

                        // Set Temp Data
                        SetTempData(ensemble);

                        // Set Salinity Data
                        SetSalinityData(ensemble);

                        // Set Pressure Data
                        SetPressureData(ensemble);

                        // Set Status Data
                        SetStatusData(ensemble);

                        // Set Ensemble Data
                        SetEnsembleData(ensemble);
                    }
                    catch (Exception e)
                    {
                        log.Error("Error adding ensemble to plots.", e);
                    }
                }
            }

            _isProcessingBuffer = false;

            return;
        }

        /// <summary>
        /// Only update the contour plot and timeseries.  This will need each ensemble.
        /// The profile plots only need the last ensemble. 
        /// </summary>
        /// <param name="ensemble">Ensemble to display.</param>
        public void DisplayBulkData(DataSet.Ensemble ensemble)
        {
            Task.Run(() => DisplayData(ensemble));
        }

        #endregion

        #region NMEA data

        /// <summary>
        /// Set the default value for all the GPS values.
        /// This will set all values to empty.
        /// </summary>
        private void SetGpsDataDefault()
        {
            GpsFix = DotSpatial.Positioning.FixQuality.NoFix.ToString();
            GpsLatitude = DotSpatial.Positioning.Latitude.Empty.ToString();
            GpsLongitude = DotSpatial.Positioning.Longitude.Empty.ToString();
            GpsAltitude = DotSpatial.Positioning.Distance.Empty.ToString();
            GpsSpeed = DotSpatial.Positioning.Speed.Empty.ToString();
            _IsGoodGps = false;
        }

        /// <summary>
        /// Set the GPS data based off the NMEA data.
        /// </summary>
        /// <param name="adcpData">Ensemble containing NMEA data.</param>
        private void SetGpsData(DataSet.Ensemble adcpData)
        {
            // Check for NMEA data
            if (adcpData != null && adcpData.IsNmeaAvail)
            {
                // Check for GGA data
                if (adcpData.NmeaData.IsGpggaAvail())
                {
                    GpsFix = adcpData.NmeaData.GPGGA.FixQuality.ToString();
                    _IsGoodGps = true;

                    if (GpsFix == DotSpatial.Positioning.FixQuality.NoFix.ToString())
                    {
                        // No fix so set place holder, values would be NaN
                        GpsLatitude = "-";
                        GpsLongitude = "-";
                        GpsAltitude = "-";
                        _IsGoodGps = false;
                    }
                    else
                    {
                        // Set actual values
                        GpsLatitude = adcpData.NmeaData.GPGGA.Position.Latitude.ToString();
                        GpsLongitude = adcpData.NmeaData.GPGGA.Position.Longitude.ToString();
                        GpsAltitude = adcpData.NmeaData.GPGGA.Altitude.ToString();
                    }
                }
                else
                {
                    GpsFix = DotSpatial.Positioning.FixQuality.NoFix.ToString();
                    GpsLatitude = DotSpatial.Positioning.Latitude.Empty.ToString();
                    GpsLongitude = DotSpatial.Positioning.Longitude.Empty.ToString();
                    GpsAltitude = DotSpatial.Positioning.Distance.Empty.ToString();
                    _IsGoodGps = false;
                }

                // Check for VTG data
                if (adcpData.NmeaData.IsGpvtgAvail())
                {
                    //if (MeasurementStandard == Core.Commons.MeasurementStandards.METRIC)
                    //{
                    GpsSpeed = adcpData.NmeaData.GPVTG.Speed.ToMetersPerSecond().ToString();
                    //}
                    //else
                    //{
                    //    GpsSpeed = adcpData.NmeaData.GPVTG.Speed.ToFeetPerSecond().ToString();
                    //}

                    // Check if the value is good
                    if (GpsSpeed == Double.NaN.ToString())
                    {
                        GpsSpeed = "-";
                    }
                }
                else
                {
                    GpsSpeed = DotSpatial.Positioning.Speed.Empty.ToString();
                }
            }
            else
            {
                SetGpsDataDefault();
            }
        }

        #endregion

        #region Temp Data

        /// <summary>
        /// Set the temperature data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        private void SetTempData(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsAncillaryAvail)
            {
                SystemTemp = ensemble.AncillaryData.SystemTemp.ToString("0.0") + " °F";
                WaterTemp = ensemble.AncillaryData.WaterTemp.ToString("0.0") + " °F";

                // This is the default value if the temperature sensor is not working
                if (ensemble.AncillaryData.WaterTemp != 15.0f)
                {
                    IsGoodTemp = true;
                }
            }
        }

        #endregion

        #region Voltage

        /// <summary>
        /// Set the voltage data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        private void SetVoltageData(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsSystemSetupAvail)
            {
                Voltage = ensemble.SystemSetupData.Voltage.ToString("0.0") + " v";

                // This is the default value if the temperature sensor is not working
                if (ensemble.SystemSetupData.Voltage != 0.0f)
                {
                    IsGoodVoltage = true;
                }
            }
        }

        #endregion

        #region Pressure

        /// <summary>
        /// Set the voltage data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        private void SetPressureData(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsAncillaryAvail)
            {
                Pressure = ensemble.AncillaryData.Pressure.ToString("0.0") + " Pa";
                TransducerDepth = ensemble.AncillaryData.TransducerDepth.ToString("0.0") + " m";

                // No Pressure sensor installed
                if(ensemble.AncillaryData.TransducerDepth == 0.0f)
                {
                    IsGoodPressure = true;
                }
                // This is the default value if the pressure sensor is not working
                else if (ensemble.AncillaryData.TransducerDepth > 1.0f)
                {
                    IsGoodPressure = true;
                }
            }
        }

        #endregion

        #region Salinity

        /// <summary>
        /// Set the Salinity data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        private void SetSalinityData(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsAncillaryAvail)
            {
                Salinity = ensemble.AncillaryData.Salinity.ToString("0.0") + " ppt";

                // This is the default value if the temperature sensor is not working
                if (ensemble.AncillaryData.Salinity < 15.0f)
                {
                    WaterType = "FRESH";
                }
                else if(ensemble.AncillaryData.Salinity > 15.0f && ensemble.AncillaryData.Salinity < 35.0f)
                {
                    WaterType = "ESTURAY";
                }
                else
                {
                    WaterType = "OCEAN";
                }
            }
        }

        #endregion

        #region Status

        /// <summary>
        /// Set the voltage data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        private void SetStatusData(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsEnsembleAvail)
            {
                WpStatus = ensemble.EnsembleData.Status.ToString();

                // This is the default value if the temperature sensor is not working
                if (ensemble.EnsembleData.Status.Value == 0)
                {
                    IsGoodWpStatus = true;
                }
            }

            if (ensemble.IsBottomTrackAvail)
            {
                BtStatus = ensemble.BottomTrackData.Status.ToString();

                // This is the default value if the temperature sensor is not working
                if (ensemble.BottomTrackData.Status.Value == 0)
                {
                    IsGoodBtStatus = true;
                }
            }
        }

        #endregion

        #region Ensemble

        /// <summary>
        /// Set the voltage data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        private void SetEnsembleData(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsEnsembleAvail)
            {
                PingTime = ensemble.AncillaryData.LastPingTime - _lastPingTime + " s";
                ProfileRange = (ensemble.AncillaryData.FirstBinRange + (ensemble.AncillaryData.BinSize * ensemble.EnsembleData.NumBins)).ToString("0.0") + " m";

                // Store the last ping time
                _lastPingTime = ensemble.AncillaryData.LastPingTime;
            }
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Eventhandler for the latest ensemble data.
        /// This will filter the ensembles based off the subsystem type.
        /// It will set the max ensemble and then check the data and display the data.
        /// </summary>
        /// <param name="ensEvent">Ensemble event.</param>
        public void Handle(EnsembleEvent ensEvent)
        {
            // Check if source matches this display
            if (_Config.Source != ensEvent.Source || ensEvent.Ensemble == null)
            {
                return;
            }

            // Display the data
            Task.Run(() => DisplayData(ensEvent.Ensemble));
        }

        /// <summary>
        /// Update the velocity, correlaton and amplitude plot when
        /// an ensemble is selected.
        /// </summary>
        /// <param name="ensEvent">Selected Ensemble event.</param>
        public void Handle(SelectedEnsembleEvent ensEvent)
        {
            // Verify the ensemble is good
            if (ensEvent.Ensemble == null || ensEvent.Ensemble.EnsembleData == null || !ensEvent.Ensemble.IsEnsembleAvail)
            {
                return;
            }

            // If no subsystem is given, then a project is not selected
            // So receive all data and display
            // If the serial number is not set, this may be an old ensemble
            // Try to display it anyway
            if (!_Config.SubSystem.IsEmpty() && !ensEvent.Ensemble.EnsembleData.SysSerialNumber.IsEmpty())
            {
                // Verify the subsystem matches this viewmodel's subystem.
                if ((_Config.SubSystem != ensEvent.Ensemble.EnsembleData.GetSubSystem())        // Check if Subsystem matches 
                        || (_Config != ensEvent.Ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
                {
                    return;
                }

                // Display the data
                Task.Run(() => DisplayData(ensEvent.Ensemble));

            }
        }

        /// <summary>
        /// Receive event when a new project has been selected.
        /// Then clear all the data in the view.
        /// </summary>
        /// <param name="prjEvent">Project Event received.</param>
        public void Handle(ProjectEvent prjEvent)
        {
            // Get the new options for this project from the database
            //GetOptionsFromDatabase();
        }

        #endregion
    }
}
