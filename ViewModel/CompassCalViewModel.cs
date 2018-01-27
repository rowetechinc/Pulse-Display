/*
 * Copyright © 2011 
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
 * 07/09/2012      RC          2.12       Initial coding
 * 07/20/2012      RC          2.12       Removed checking if _isReadingCompass is set when received kDataResponse from the compass.
 * 07/23/2012      RC          2.12       Made serial port a property so it can be set by AdcpSetupViewModel when settings change.
 * 08/16/2012      RC          2.12       Removed the need for the AdcpSetupViewModel.
 * 11/13/2012      RC          2.16       Added a complete compass cal process.
 *                                         Store the CompassCal results in the same folder as the error log.
 * 11/14/2012      RC          2.16       Send event for ADCP status when status changes.
 * 01/22/2013      RC          2.17       Made AdcpStatus an object and not an enum.
 * 07/12/2013      RC          3.0.4      Write result to maintence log.
 * 07/31/2013      RC          3.0.6      Added CompassUtilityViewModel.
 * 02/18/2014      RC          3.2.3      Fixed bug with getting Firmware property.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/15/2014      RC          4.0.0      Removed Vault and storing the data to database.
 * 02/03/2015      RC          4.1.0      Added an event to be sent when the compass cal is complete.
 * 07/10/2015      RC          4.1.3      Removed all the background workers.
 * 
 */

using System.ComponentModel.Composition;
using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.IO;
using log4net;
using Caliburn.Micro;
using ReactiveUI;
using System.Threading.Tasks;

namespace RTI
{

    /// <summary>
    /// Compass cal and configuration view model.
    /// </summary>
    [Export]
    public class CompassCalViewModel : PulseViewModel, IDataErrorInfo
    {
        #region Variables

        #region Defaults

        /// <summary>
        /// Default Point 1 location.
        /// </summary>
        public const double DEFAULT_POINT1_LOC = 0.0;

        /// <summary>
        /// Default Point 2 location.
        /// </summary>
        public const double DEFAULT_POINT2_LOC = 90.0;

        /// <summary>
        /// Default Point 3 location.
        /// </summary>
        public const double DEFAULT_POINT3_LOC = 180.0;

        /// <summary>
        /// Default Point 4 location.
        /// </summary>
        public const double DEFAULT_POINT4_LOC = 270.0;

        /// <summary>
        /// Default flag for validating the calibration score.
        /// </summary>
        public const bool DEFAULT_IS_VALIDATE_CAL_SCORE = false;

        /// <summary>
        /// Default result text file name.
        /// </summary>
        public const string DEFAULT_RESULT_TXT = "CompassCalResults.csv";

        #endregion

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
        private RTI.PniPrimeCompassBinaryCodec _compassCodec;

        /// <summary>
        /// Data from the PNI compass.  This includes the heading, pitch and
        /// roll.
        /// </summary>
        private PniPrimeCompassBinaryCodec.PniDataResponse _compassDataResponse;

        /// <summary>
        /// Flag set when Cal Score is set.
        /// </summary>
        private bool _isGoodCal;

        /// <summary>
        /// Flag to know if the compass calibration process should
        /// stop or continue running.
        /// </summary>
        private bool _IsCalibrationRunning;

        /// <summary>
        /// Count the number of points that have been collected.
        /// </summary>
        private int _pointCount;

        //// Compass calibration scores
        ///// <summary>
        ///// Maximum Magnetic Standard Deviation
        ///// Less than or equal to 0.1 per documentation
        ///// </summary>
        //private double MAX_MAG_STDEV = 0.1;

        ///// <summary>
        ///// Minimum Magnetic XY coverage.
        ///// 85% per documentation
        ///// </summary>
        //private int MIN_MAG_XY_COVERAGE = 85;

        ///// <summary>
        ///// Minimum Magnetic Z coverage.
        ///// 50% per documentation
        ///// </summary>
        //private int MIN_MAG_Z_COVERAGE = 50;

        ///// <summary>
        ///// Maximum accelearation Standard Deviation.
        ///// Less than or equal to 2 per documentation.
        ///// </summary>
        //private double MAX_ACCEL_STDEV = 2;

        ///// <summary>
        ///// Minimum acceleration XY Coverage.
        ///// 95% per documentation.
        ///// </summary>
        //private int MIN_ACCEL_XY_COVERAGE = 95;

        ///// <summary>
        ///// Minimum acceleration Z coverage.
        ///// 90% per documentation.
        ///// </summary>
        //private int MIN_ACCEL_Z_COVERAGE = 90;

        #endregion

        #region Properties

        #region Mag and Accel Cal Flag

        /// <summary>
        /// Flag set if the calibration will do both Mag and acceleration
        /// calibration.
        /// </summary>
        private bool _isMagAndAccelCalibration;
        /// <summary>
        /// Flag set if the calibration will do both Mag and acceleration calibration property.
        /// </summary>
        public bool IsMagAndAccelCalibration
        {
            get { return _isMagAndAccelCalibration; }
            set
            {
                _isMagAndAccelCalibration = value;
                this.NotifyOfPropertyChange(() => this.IsMagAndAccelCalibration);
            }
        }

        #endregion

        #region Compass Cal Buttons

        /// <summary>
        /// Use the same button to preform two different
        /// operations based off the state.
        /// </summary>
        private string _compassCalButtonLabel;
        /// <summary>
        /// Use the same button to preform two different 
        /// operations based off the state. 
        /// Compass Cal Button Label property.
        /// </summary>
        public string CompassCalButtonLabel
        {
            get { return _compassCalButtonLabel; }
            set
            {
                _compassCalButtonLabel = value;
                this.NotifyOfPropertyChange(() => this.CompassCalButtonLabel);
            }
        }

        /// <summary>
        /// Flag if in Compass Cal mode or not.
        /// This will also change the button label.
        /// </summary>
        private bool _isCompassCal;
        /// <summary>
        /// Flag if in Compass Cal mode property.
        /// </summary>
        public bool IsCompassCal
        {
            get { return _isCompassCal; }
            set
            {
                _isCompassCal = value;

                if (_isCompassCal)
                {
                    CompassCalButtonLabel = "Stop";
                }
                else
                {
                    CompassCalButtonLabel = "Start";
                }

                this.NotifyOfPropertyChange(() => this.IsCompassCal);
                this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
                this.NotifyOfPropertyChange(() => this.CanCompassTap);
                this.NotifyOfPropertyChange(() => this.CanReadCompass);
            }
        }

        /// <summary>
        /// Use the same button to preform two different operations
        /// based off the state.
        /// </summary>
        private string _compassDataButtonLabel;
        /// <summary>
        /// Use the same button to preform two different operations
        /// base off the state.  Compass Data Button label property.
        /// </summary>
        public string CompassDataButtonLabel
        {
            get { return _compassDataButtonLabel; }
            set
            {
                _compassDataButtonLabel = value;
                this.NotifyOfPropertyChange(() => this.CompassDataButtonLabel);
            }
        }

        #endregion

        #region Compass Connected Flag

        /// <summary>
        /// Flag if in Compass Data collecting mode.
        /// This will also change the button label.
        /// </summary>
        private bool _isCompassConnected;
        /// <summary>
        /// Flag if in Compass Data collecting mode property.
        /// </summary>
        public bool IsCompassConnected
        {
            get { return _isCompassConnected; }
            set
            {
                _isCompassConnected = value;

                if (_isCompassConnected)
                {
                    CompassDataButtonLabel = "Disconnect";
                }
                else
                {
                    CompassDataButtonLabel = "Connect";
                }

                this.NotifyOfPropertyChange(() => this.IsCompassConnected);
            }
        }

        #endregion

        #region Cal Samples

        /// <summary>
        /// Value representing the number
        /// of sample completed in the compass
        /// cal.
        /// </summary>
        private UInt32 _calSamples;
        /// <summary>
        /// Value representing the number of 
        /// samples completed in the compass cal property.
        /// </summary>
        public UInt32 CalSamples
        {
            get { return _calSamples; }
            set
            {
                _calSamples = value;
                this.NotifyOfPropertyChange(() => this.CalSamples);
            }
        }

        /// <summary>
        /// String of the position to go to 
        /// next based off the current sample.
        /// </summary>
        private string _CalPosition;
        /// <summary>
        /// String of the position to go to 
        /// next based off the current sample.
        /// </summary>
        public string CalPosition
        {
            get { return _CalPosition; }
            set
            {
                _CalPosition = value;
                this.NotifyOfPropertyChange(() => this.CalPosition);
            }
        }

        #endregion

        #region Cal Score

        /// <summary>
        /// Calibration Score Mag Std Dev Err.
        /// </summary>
        private float _calScore_StdDevErr;
        /// <summary>
        /// Calibration Score Mag Std Dev Err property.
        /// </summary>
        public float CalScore_StdDevErr
        {
            get { return _calScore_StdDevErr; }
            set
            {
                _calScore_StdDevErr = value;
                this.NotifyOfPropertyChange(() => this.CalScore_StdDevErr);
            }
        }

        /// <summary>
        /// Calibration Score Mag X Coverage.
        /// </summary>
        private float _calScore_xCoverage;
        /// <summary>
        /// Calibration Score Mag X Coverage property.
        /// </summary>
        public float CalScore_xCoverage
        {
            get { return _calScore_xCoverage; }
            set
            {
                _calScore_xCoverage = value;
                this.NotifyOfPropertyChange(() => this.CalScore_xCoverage);
            }
        }

        /// <summary>
        /// Calibration Score X Coverage color.
        /// Red is a bad score.
        /// </summary>
        private string _calScore_xCoverageColor;
        /// <summary>
        /// Calibration Score X Coverage color property.
        /// </summary>
        public string CalScore_xCoverageColor
        {
            get { return _calScore_xCoverageColor; }
            set
            {
                _calScore_xCoverageColor = value;
                this.NotifyOfPropertyChange(() => this.CalScore_xCoverageColor);
            }
        }

        /// <summary>
        /// Calibration Score Mag Y Coverage.
        /// </summary>
        private float _calScore_yCoverage;
        /// <summary>
        /// Calibration Score Mag Y Coverage property.
        /// </summary>
        public float CalScore_yCoverage
        {
            get { return _calScore_yCoverage; }
            set
            {
                _calScore_yCoverage = value;
                this.NotifyOfPropertyChange(() => this.CalScore_yCoverage);
            }
        }

        /// <summary>
        /// Calibration Score Mag Z Coverage.
        /// </summary>
        private float _calScore_zCoverage;
        /// <summary>
        /// Calibration Score Mag Z Coverage property.
        /// </summary>
        public float CalScore_zCoverage
        {
            get { return _calScore_zCoverage; }
            set
            {
                _calScore_zCoverage = value;
                this.NotifyOfPropertyChange(() => this.CalScore_zCoverage);
            }
        }

        /// <summary>
        /// Calibration Score X Accel Coverage.
        /// </summary>
        private UInt16 _calScore_xAccelCoverage;
        /// <summary>
        /// Calibration Score X Accel Coverage property.
        /// </summary>
        public UInt16 CalScore_xAccelCoverage
        {
            get { return _calScore_xAccelCoverage; }
            set
            {
                _calScore_xAccelCoverage = value;
                this.NotifyOfPropertyChange(() => this.CalScore_xAccelCoverage);
            }
        }

        /// <summary>
        /// Calibration Score Y Accel Coverage.
        /// </summary>
        private UInt16 _calScore_yAccelCoverage;
        /// <summary>
        /// Calibration Score Y Accel Coverage property.
        /// </summary>
        public UInt16 CalScore_yAccelCoverage
        {
            get { return _calScore_yAccelCoverage; }
            set
            {
                _calScore_yAccelCoverage = value;
                this.NotifyOfPropertyChange(() => this.CalScore_yAccelCoverage);
            }
        }

        /// <summary>
        /// Calibration Score Z Accel Coverage.
        /// </summary>
        private UInt16 _calScore_zAccelCoverage;
        /// <summary>
        /// Calibration Score Z Accel Coverage property.
        /// </summary>
        public UInt16 CalScore_zAccelCoverage
        {
            get { return _calScore_zAccelCoverage; }
            set
            {
                _calScore_zAccelCoverage = value;
                this.NotifyOfPropertyChange(() => this.CalScore_zAccelCoverage);
            }
        }

        /// <summary>
        /// Calibration Score Accel Std Dev Err.
        /// </summary>
        private float _calScore_accelStdDevErr;
        /// <summary>
        /// Calibration Score Accel Std Dev Err property.
        /// </summary>
        public float CalScore_accelStdDevErr
        {
            get { return _calScore_accelStdDevErr; }
            set
            {
                _calScore_accelStdDevErr = value;
                this.NotifyOfPropertyChange(() => this.CalScore_accelStdDevErr);
            }
        }

        #endregion

        #region Compass Data Response

        /// <summary>
        /// Compass Cal Heading output.
        /// This is gotten when the Read Compass command
        /// is sent and a response is given.
        /// </summary>
        public float CompassCalHeading
        {
            get { return _compassDataResponse.Heading; }
        }

        /// <summary>
        /// Compass Cal Pitch output.
        /// This is gotten when the Read Compass command
        /// is sent and a response is given.
        /// </summary>
        public float CompassCalPitch
        {
            get { return _compassDataResponse.Pitch; }
        }

        /// <summary>
        /// Compass Cal Roll output.
        /// This is gotten when the Read Compass command
        /// is sent and a response is given.
        /// </summary>
        public float CompassCalRoll
        {
            get { return _compassDataResponse.Roll; }
        }

        /// <summary>
        /// Compass Data Distortion.
        /// </summary>
        public string CompassCalDistortion
        {
            get
            {
                if (_compassDataResponse.Distortion)
                {
                    return "Yes";
                }

                return "No";
            }
        }

        /// <summary>
        /// Set the color based off the Distortion value.
        /// If there is a distortion, set color to 
        /// red.  If not, set to green.
        /// </summary>
        public Brush CompassCalDistortionColor
        {
            get
            {
                if (_compassDataResponse.Distortion)
                {
                    return new SolidColorBrush(Colors.Red);
                }

                return new SolidColorBrush(Colors.Green);
            }
        }

        /// <summary>
        /// Compass cal status.  Either calibrated
        /// or not calibrated.
        /// </summary>
        public string CompassCalStatus
        {
            get
            {
                if (_compassDataResponse.CalStatus)
                {
                    return "Calibrated";
                }

                return "Not Calibrated";
            }
        }

        /// <summary>
        /// Set the color based off the CalStatus value.
        /// If not calibrated, set color to 
        /// red.  If is, set to green.
        /// </summary>
        public Brush CompassCalStatusColor
        {
            get
            {
                if (_compassDataResponse.CalStatus)
                {
                    return new SolidColorBrush(Colors.Green);
                }

                return new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Compass Cal Magnetometer X score.
        /// </summary>
        public float CompassCalMagX
        {
            get { return _compassDataResponse.xAligned; }
        }

        /// <summary>
        /// Compass Cal Magnetometer Y score.
        /// </summary>
        public float CompassCalMagY
        {
            get { return _compassDataResponse.yAligned; }
        }

        /// <summary>
        /// Compass Cal Magnetometer Z score.
        /// </summary>
        public float CompassCalMagZ
        {
            get { return _compassDataResponse.zAligned; }
        }

        /// <summary>
        /// Compass Cal Acceleration Pitch score.
        /// </summary>
        public float CompassCalAccelPitch
        {
            get { return _compassDataResponse.pAligned; }
        }

        /// <summary>
        /// Compass Cal Acceleration Roll score.
        /// </summary>
        public float CompassCalAccelRoll
        {
            get { return _compassDataResponse.rAligned; }
        }

        /// <summary>
        /// Compass Cal Acceleration Z score.
        /// </summary>
        public float CompassCalAccelZ
        {
            get { return _compassDataResponse.izAligned; }
        }


        #endregion

        #region Compass Config Response

        /// <summary>
        /// The compass calibration declination value.  
        /// Format: Float32
        /// Range: -180 to 180
        /// Default Value: 0
        /// </summary>
        private float _compassCalDeclination;
        /// <summary>
        /// The compass calibration declination value.  
        /// Format: Float32
        /// Range: -180 to 180
        /// Default Value: 0
        /// </summary>
        public float CompassCalDeclination
        {
            get { return _compassCalDeclination; }
            set
            {
                // Validate the value
                if (value >= -180.0 && value <= 180.0)
                {
                    _compassCalDeclination = value;

                    // Send the configuration to the Compass
                    SendConfigCommand(PniPrimeCompassBinaryCodec.ID.kDeclination, _compassCalDeclination);
                }

                this.NotifyOfPropertyChange(() => this.CompassCalDeclination);
            }
        }

        /// <summary>
        /// The compass calibration Stable Check value.  
        /// Format: Boolean
        /// Range: 
        /// Default Value: True
        /// </summary>
        private bool _isCompassCalStableCheck;
        /// <summary>
        /// The compass calibration Stable Check value.  
        /// Format: Boolean
        /// Range: 
        /// Default Value: True
        /// </summary>
        public bool IsCompassCalStableCheck
        {
            get { return _isCompassCalStableCheck; }
            set
            {
                _isCompassCalStableCheck = value;

                // Send the configuration to the Compass
                SendConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalStableCheck, _isCompassCalStableCheck);

                this.NotifyOfPropertyChange(() => this.IsCompassCalStableCheck);
            }
        }

        /// <summary>
        /// The compass calibration Auto Sampling value.  
        /// Format: Boolean
        /// Range: 
        /// Default Value: True
        /// </summary>
        private bool _isCompassCalAutoSample;
        /// <summary>
        /// The compass calibration Auto Sampling value.  
        /// Format: Boolean
        /// Range: 
        /// Default Value: True
        /// </summary>
        public bool IsCompassCalAutoSample
        {
            get { return _isCompassCalAutoSample; }
            set
            {
                _isCompassCalAutoSample = value;

                // Send the configuration to the Compass
                SendConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling, _isCompassCalAutoSample);

                this.NotifyOfPropertyChange(() => this.IsCompassCalAutoSample);

                // Enable/Disable Sample button
                //((DelegateCommand<object>)TakeCompassCalSampleCommand).RaiseCanExecuteChanged();
                //this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            }
        }

        /// <summary>
        /// The compass calibration declination value.  
        /// Format: UInt32
        /// Range: 12 to 32
        /// Default Value: 12
        /// </summary>
        private UInt32 _compassCalNumSamples;
        /// <summary>
        /// The compass calibration declination value.  
        /// Format: UInt32
        /// Range: 12 to 32
        /// Default Value: 12
        /// </summary>
        public UInt32 CompassCalNumSamples
        {
            get { return _compassCalNumSamples; }
            set
            {
                // Validate the value
                if (value >= 12 && value <= 32)
                {
                    _compassCalNumSamples = value;

                    // Send the configuration to the Compass
                    SendConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalNumPoints, _compassCalNumSamples);
                }

                this.NotifyOfPropertyChange(() => this.CompassCalNumSamples);
            }
        }


        #endregion

        #region Compass Paramater Response

        /// <summary>
        /// Number of taps for the compass.
        /// </summary>
        private int _compassCalTaps;
        /// <summary>
        /// Number of taps for the compass.
        /// </summary>
        public int CompassCalTaps
        {
            get { return _compassCalTaps; }
            set
            {
                _compassCalTaps = value;
                this.NotifyOfPropertyChange(() => this.CompassCalTaps);
            }
        }

        #endregion

        #region Serial Number

        /// <summary>
        /// Serial number for the transducer.
        /// </summary>
        private RTI.SerialNumber _SerialNumber;
        /// <summary>
        /// Serial number for the transducer.
        /// </summary>
        public RTI.SerialNumber SerialNumber
        {
            get { return _SerialNumber; }
            set
            {
                _SerialNumber = value;
                this.NotifyOfPropertyChange(() => this.SerialNumber);

                if (_adcpConn != null)
                {
                    _adcpConn.SerialNumber = value;
                }
            }
        }

        /// <summary>
        /// Firmware version.
        /// </summary>
        private Firmware _Firmware;
        /// <summary>
        /// Firmware version.
        /// </summary>
        public Firmware Firmware
        {
            get 
            {
                return _Firmware;
            }
            set
            {
                _Firmware = value;
                this.NotifyOfPropertyChange(() => this.Firmware);

                if (_adcpConn != null)
                {
                    _adcpConn.Firmware = value;
                }
            }
        }

        /// <summary>
        /// Hardware type.
        /// This will list all the hardware types in the system.
        /// </summary>
        private string _Hardware;
        /// <summary>
        /// Hardware type.
        /// This will list all the hardware types in the system.
        /// </summary>
        public string Hardware
        {
            get { return _Hardware; }
            set
            {
                _Hardware = value;
                this.NotifyOfPropertyChange(() => this.Hardware);

                if (_adcpConn != null)
                {
                    _adcpConn.Hardware = value;
                }
            }
        }

        #endregion

        #region Test Results

        /// <summary>
        /// Test status.
        /// </summary>
        enum TestResultStatus
        {
            /// <summary>
            /// Fail.
            /// </summary>
            FAIL = 0,

            /// <summary>
            /// Pass.
            /// </summary>
            PASS = 1,

            /// <summary>
            /// Test in progress.
            /// </summary>
            IN_PROGRESS,

            /// <summary>
            /// Test not started.
            /// </summary>
            NOT_STARTED,

            /// <summary>
            /// Not testing this.
            /// </summary>
            NOT_TESTING
        }

        /// <summary>
        /// Test result for ADCP communication.
        /// Did we get a break and were able to
        /// set the serial number, firmware and hardware.
        /// </summary>
        private TestResultStatus _testResult_AdcpComm;
        /// <summary>
        /// Get the image to display based off
        /// the test has passed or failed.
        /// </summary>
        public string TestResult_AdcpComm
        {
            get
            {
                return TestResultImage(_testResult_AdcpComm);
            }
        }

        /// <summary>
        /// Test result for Compass communication.
        /// This will try to read the compass for data.
        /// If pass, this is set true.
        /// </summary>
        private TestResultStatus _testResult_CompassComm;
        /// <summary>
        /// Get the image to display based off
        /// if the test has passed or failed.
        /// </summary>
        public string TestResult_CompassComm
        {
            get
            {
                return TestResultImage(_testResult_CompassComm);
            }
        }

        /// <summary>
        /// Test result if the compass calibration
        /// is complete.  This will be set when a CalScore
        /// is received.
        /// </summary>
        private TestResultStatus _testResult_CompassCalComplete;
        /// <summary>
        /// Get the image to display based off
        /// if the test has passed or failed.
        /// </summary>
        public string TestResult_CompassCalComplete
        {
            get
            {
                return TestResultImage(_testResult_CompassCalComplete);
            }
        }

        /// <summary>
        /// Test result if the compass calibration
        /// is saved.  This will be set when a kSaveDone
        /// is received.
        /// </summary>
        private TestResultStatus _testResult_CompassCalSaved;
        /// <summary>
        /// Get the image to display based off
        /// if the test has passed or failed.
        /// </summary>
        public string TestResult_CompassCalSaved
        {
            get
            {
                return TestResultImage(_testResult_CompassCalSaved);
            }
        }

        /// <summary>
        /// Test result if the compass calibration
        /// is good.  The compass cal score will be compared
        /// against spec given by the manufacture of the compass.
        /// If they do not meet the specs of a good calibration, this
        /// will be set to false.
        /// </summary>
        private TestResultStatus _testResult_CompassCalGood;
        /// <summary>
        /// Get the image to display based off
        /// if the test has passed or failed.
        /// </summary>
        public string TestResult_CompassCalGood
        {
            get
            {
                return TestResultImage(_testResult_CompassCalGood);
            }
        }

        #endregion

        #region Test Points

        #region Point Locations

        /// <summary>
        /// Location of point 1 in degrees.
        /// </summary>
        private double point1_Loc;
        /// <summary>
        /// Location of point 1 in degrees.
        /// </summary>
        public double Point1_Loc
        {
            get { return point1_Loc; }
            set
            {
                point1_Loc = value;
                this.NotifyOfPropertyChange(() => this.Point1_Loc);
            }
        }

        /// <summary>
        /// Location of point 2 in degrees.
        /// </summary>
        private double point2_Loc;
        /// <summary>
        /// Location of point 2 in degrees.
        /// </summary>
        public double Point2_Loc
        {
            get { return point2_Loc; }
            set
            {
                point2_Loc = value;
                this.NotifyOfPropertyChange(() => this.Point2_Loc);
            }
        }

        /// <summary>
        /// Location of point 3 in degrees.
        /// </summary>
        private double point3_Loc;
        /// <summary>
        /// Location of point 3 in degrees.
        /// </summary>
        public double Point3_Loc
        {
            get { return point3_Loc; }
            set
            {
                point3_Loc = value;
                this.NotifyOfPropertyChange(() => this.Point3_Loc);
            }
        }

        /// <summary>
        /// Location of point 4 in degrees.
        /// </summary>
        private double point4_Loc;
        /// <summary>
        /// Location of point 4 in degrees.
        /// </summary>
        public double Point4_Loc
        {
            get { return point4_Loc; }
            set
            {
                point4_Loc = value;
                this.NotifyOfPropertyChange(() => this.Point4_Loc);
            }
        }

        #endregion

        #region Pre Points

        #region Point 1

        /// <summary>
        /// The heading for point 1 in degrees.
        /// </summary>
        private double point1_Pre_Hdg;
        /// <summary>
        /// The heading for point 1 in degrees.
        /// </summary>
        public double Point1_Pre_Hdg
        {
            get { return point1_Pre_Hdg; }
            set
            {
                point1_Pre_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point1_Pre_Hdg);
            }
        }

        /// <summary>
        /// The pitch for point 1 in degrees.
        /// </summary>
        private double point1_Pre_Ptch;
        /// <summary>
        /// The pitch for point 1 in degrees.
        /// </summary>
        public double Point1_Pre_Ptch
        {
            get { return point1_Pre_Ptch; }
            set
            {
                point1_Pre_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point1_Pre_Ptch);
            }
        }

        /// <summary>
        /// The Roll for point 1 in degrees.
        /// </summary>
        private double point1_Pre_Roll;
        /// <summary>
        /// The Roll for point 1 in degrees.
        /// </summary>
        public double Point1_Pre_Roll
        {
            get { return point1_Pre_Roll; }
            set
            {
                point1_Pre_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point1_Pre_Roll);
            }
        }

        #endregion

        #region Point 2

        /// <summary>
        /// The heading for point 2 in degrees.
        /// </summary>
        private double point2_Pre_Hdg;
        /// <summary>
        /// The heading for point 2 in degrees.
        /// </summary>
        public double Point2_Pre_Hdg
        {
            get { return point2_Pre_Hdg; }
            set
            {
                point2_Pre_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point2_Pre_Hdg);
            }
        }

        /// <summary>
        /// The pitch for point 2 in degrees.
        /// </summary>
        private double point2_Pre_Ptch;
        /// <summary>
        /// The pitch for point 2 in degrees.
        /// </summary>
        public double Point2_Pre_Ptch
        {
            get { return point2_Pre_Ptch; }
            set
            {
                point2_Pre_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point2_Pre_Ptch);
            }
        }

        /// <summary>
        /// The Roll for point 2 in degrees.
        /// </summary>
        private double point2_Pre_Roll;
        /// <summary>
        /// The Roll for point 2 in degrees.
        /// </summary>
        public double Point2_Pre_Roll
        {
            get { return point2_Pre_Roll; }
            set
            {
                point2_Pre_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point2_Pre_Roll);
            }
        }

        #endregion

        #region Point 3

        /// <summary>
        /// The heading for point 3 in degrees.
        /// </summary>
        private double point3_Pre_Hdg;
        /// <summary>
        /// The heading for point 3 in degrees.
        /// </summary>
        public double Point3_Pre_Hdg
        {
            get { return point3_Pre_Hdg; }
            set
            {
                point3_Pre_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point3_Pre_Hdg);
            }
        }

        /// <summary>
        /// The pitch for point 3 in degrees.
        /// </summary>
        private double point3_Pre_Ptch;
        /// <summary>
        /// The pitch for point 3 in degrees.
        /// </summary>
        public double Point3_Pre_Ptch
        {
            get { return point3_Pre_Ptch; }
            set
            {
                point3_Pre_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point3_Pre_Ptch);
            }
        }

        /// <summary>
        /// The Roll for point 3 in degrees.
        /// </summary>
        private double point3_Pre_Roll;
        /// <summary>
        /// The Roll for point 3 in degrees.
        /// </summary>
        public double Point3_Pre_Roll
        {
            get { return point3_Pre_Roll; }
            set
            {
                point3_Pre_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point3_Pre_Roll);
            }
        }

        #endregion

        #region Point 4

        /// <summary>
        /// The heading for point 4 in degrees.
        /// </summary>
        private double point4_Pre_Hdg;
        /// <summary>
        /// The heading for point 4 in degrees.
        /// </summary>
        public double Point4_Pre_Hdg
        {
            get { return point4_Pre_Hdg; }
            set
            {
                point4_Pre_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point4_Pre_Hdg);
            }
        }

        /// <summary>
        /// The pitch for point 4 in degrees.
        /// </summary>
        private double point4_Pre_Ptch;
        /// <summary>
        /// The pitch for point 4 in degrees.
        /// </summary>
        public double Point4_Pre_Ptch
        {
            get { return point4_Pre_Ptch; }
            set
            {
                point4_Pre_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point4_Pre_Ptch);
            }
        }

        /// <summary>
        /// The Roll for point 4 in degrees.
        /// </summary>
        private double point4_Pre_Roll;
        /// <summary>
        /// The Roll for point 4 in degrees.
        /// </summary>
        public double Point4_Pre_Roll
        {
            get { return point4_Pre_Roll; }
            set
            {
                point4_Pre_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point4_Pre_Roll);
            }
        }

        #endregion

        #endregion

        #region Post Points

        #region Point 1

        /// <summary>
        /// The Post heading for point 1 in degrees.
        /// </summary>
        private double point1_Post_Hdg;
        /// <summary>
        /// The Post heading for point 1 in degrees.
        /// </summary>
        public double Point1_Post_Hdg
        {
            get { return point1_Post_Hdg; }
            set
            {
                point1_Post_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point1_Post_Hdg);
            }
        }

        /// <summary>
        /// The Post pitch for point 1 in degrees.
        /// </summary>
        private double point1_Post_Ptch;
        /// <summary>
        /// The Post pitch for point 1 in degrees.
        /// </summary>
        public double Point1_Post_Ptch
        {
            get { return point1_Post_Ptch; }
            set
            {
                point1_Post_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point1_Post_Ptch);
            }
        }

        /// <summary>
        /// The Post Roll for point 1 in degrees.
        /// </summary>
        private double point1_Post_Roll;
        /// <summary>
        /// The Post Roll for point 1 in degrees.
        /// </summary>
        public double Point1_Post_Roll
        {
            get { return point1_Post_Roll; }
            set
            {
                point1_Post_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point1_Post_Roll);
            }
        }

        #endregion

        #region Point 2

        /// <summary>
        /// The Post heading for point 2 in degrees.
        /// </summary>
        private double point2_Post_Hdg;
        /// <summary>
        /// The Post heading for point 2 in degrees.
        /// </summary>
        public double Point2_Post_Hdg
        {
            get { return point2_Post_Hdg; }
            set
            {
                point2_Post_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point2_Post_Hdg);
            }
        }

        /// <summary>
        /// The Post pitch for point 2 in degrees.
        /// </summary>
        private double point2_Post_Ptch;
        /// <summary>
        /// The Post pitch for point 2 in degrees.
        /// </summary>
        public double Point2_Post_Ptch
        {
            get { return point2_Post_Ptch; }
            set
            {
                point2_Post_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point2_Post_Ptch);
            }
        }

        /// <summary>
        /// The Post Roll for point 2 in degrees.
        /// </summary>
        private double point2_Post_Roll;
        /// <summary>
        /// The Post Roll for point 2 in degrees.
        /// </summary>
        public double Point2_Post_Roll
        {
            get { return point2_Post_Roll; }
            set
            {
                point2_Post_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point2_Post_Roll);
            }
        }

        #endregion

        #region Point 3

        /// <summary>
        /// The Post heading for point 3 in degrees.
        /// </summary>
        private double point3_Post_Hdg;
        /// <summary>
        /// The Post heading for point 3 in degrees.
        /// </summary>
        public double Point3_Post_Hdg
        {
            get { return point3_Post_Hdg; }
            set
            {
                point3_Post_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point3_Post_Hdg);
            }
        }

        /// <summary>
        /// The Post pitch for point 3 in degrees.
        /// </summary>
        private double point3_Post_Ptch;
        /// <summary>
        /// The Post pitch for point 3 in degrees.
        /// </summary>
        public double Point3_Post_Ptch
        {
            get { return point3_Post_Ptch; }
            set
            {
                point3_Post_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point3_Post_Ptch);
            }
        }

        /// <summary>
        /// The Post Roll for point 3 in degrees.
        /// </summary>
        private double point3_Post_Roll;
        /// <summary>
        /// The Post Roll for point 3 in degrees.
        /// </summary>
        public double Point3_Post_Roll
        {
            get { return point3_Post_Roll; }
            set
            {
                point3_Post_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point3_Post_Roll);
            }
        }

        #endregion

        #region Point 4

        /// <summary>
        /// The Post heading for point 4 in degrees.
        /// </summary>
        private double point4_Post_Hdg;
        /// <summary>
        /// The Post heading for point 4 in degrees.
        /// </summary>
        public double Point4_Post_Hdg
        {
            get { return point4_Post_Hdg; }
            set
            {
                point4_Post_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point4_Post_Hdg);
            }
        }

        /// <summary>
        /// The Post pitch for point 4 in degrees.
        /// </summary>
        private double point4_Post_Ptch;
        /// <summary>
        /// The Post pitch for point 4 in degrees.
        /// </summary>
        public double Point4_Post_Ptch
        {
            get { return point4_Post_Ptch; }
            set
            {
                point4_Post_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point4_Post_Ptch);
            }
        }

        /// <summary>
        /// The Post Roll for point 4 in degrees.
        /// </summary>
        private double point4_Post_Roll;
        /// <summary>
        /// The Post Roll for point 4 in degrees.
        /// </summary>
        public double Point4_Post_Roll
        {
            get { return point4_Post_Roll; }
            set
            {
                point4_Post_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point4_Post_Roll);
            }
        }

        #endregion

        #endregion

        #region Diff Points

        #region Point 1

        /// <summary>
        /// The difference in heading for point 1 in degrees.
        /// </summary>
        private string point1_Diff_Hdg;
        /// <summary>
        /// The difference in heading for point 1 in degrees.
        /// </summary>
        public string Point1_Diff_Hdg
        {
            get { return point1_Diff_Hdg; }
            set
            {
                point1_Diff_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point1_Diff_Hdg);
            }
        }

        /// <summary>
        /// The difference in pitch for point 1 in degrees.
        /// </summary>
        private string point1_Diff_Ptch;
        /// <summary>
        /// The difference in pitch for point 1 in degrees.
        /// </summary>
        public string Point1_Diff_Ptch
        {
            get { return point1_Diff_Ptch; }
            set
            {
                point1_Diff_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point1_Diff_Ptch);
            }
        }

        /// <summary>
        /// The difference in Roll for point 1 in degrees.
        /// </summary>
        private string point1_Diff_Roll;
        /// <summary>
        /// The difference in Roll for point 1 in degrees.
        /// </summary>
        public string Point1_Diff_Roll
        {
            get { return point1_Diff_Roll; }
            set
            {
                point1_Diff_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point1_Diff_Roll);
            }
        }

        #endregion

        #region Point 2

        /// <summary>
        /// The difference in heading for point 2 in degrees.
        /// </summary>
        private string point2_Diff_Hdg;
        /// <summary>
        /// The difference in heading for point 2 in degrees.
        /// </summary>
        public string Point2_Diff_Hdg
        {
            get { return point2_Diff_Hdg; }
            set
            {
                point2_Diff_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point2_Diff_Hdg);
            }
        }

        /// <summary>
        /// The difference in pitch for point 2 in degrees.
        /// </summary>
        private string point2_Diff_Ptch;
        /// <summary>
        /// The difference in pitch for point 2 in degrees.
        /// </summary>
        public string Point2_Diff_Ptch
        {
            get { return point2_Diff_Ptch; }
            set
            {
                point2_Diff_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point2_Diff_Ptch);
            }
        }

        /// <summary>
        /// The difference in Roll for point 2 in degrees.
        /// </summary>
        private string point2_Diff_Roll;
        /// <summary>
        /// The difference in Roll for point 2 in degrees.
        /// </summary>
        public string Point2_Diff_Roll
        {
            get { return point2_Diff_Roll; }
            set
            {
                point2_Diff_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point2_Diff_Roll);
            }
        }

        #endregion

        #region Point 3

        /// <summary>
        /// The difference in heading for point 3 in degrees.
        /// </summary>
        private string point3_Diff_Hdg;
        /// <summary>
        /// The difference in heading for point 3 in degrees.
        /// </summary>
        public string Point3_Diff_Hdg
        {
            get { return point3_Diff_Hdg; }
            set
            {
                point3_Diff_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point3_Diff_Hdg);
            }
        }

        /// <summary>
        /// The difference in pitch for point 3 in degrees.
        /// </summary>
        private string point3_Diff_Ptch;
        /// <summary>
        /// The difference in pitch for point 3 in degrees.
        /// </summary>
        public string Point3_Diff_Ptch
        {
            get { return point3_Diff_Ptch; }
            set
            {
                point3_Diff_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point3_Diff_Ptch);
            }
        }

        /// <summary>
        /// The difference in Roll for point 3 in degrees.
        /// </summary>
        private string point3_Diff_Roll;
        /// <summary>
        /// The difference in Roll for point 3 in degrees.
        /// </summary>
        public string Point3_Diff_Roll
        {
            get { return point3_Diff_Roll; }
            set
            {
                point3_Diff_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point3_Diff_Roll);
            }
        }

        #endregion

        #region Point 4

        /// <summary>
        /// The difference in heading for point 4 in degrees.
        /// </summary>
        private string point4_Diff_Hdg;
        /// <summary>
        /// The difference in heading for point 4 in degrees.
        /// </summary>
        public string Point4_Diff_Hdg
        {
            get { return point4_Diff_Hdg; }
            set
            {
                point4_Diff_Hdg = value;
                this.NotifyOfPropertyChange(() => this.Point4_Diff_Hdg);
            }
        }

        /// <summary>
        /// The difference in pitch for point 4 in degrees.
        /// </summary>
        private string point4_Diff_Ptch;
        /// <summary>
        /// The difference in pitch for point 4 in degrees.
        /// </summary>
        public string Point4_Diff_Ptch
        {
            get { return point4_Diff_Ptch; }
            set
            {
                point4_Diff_Ptch = value;
                this.NotifyOfPropertyChange(() => this.Point4_Diff_Ptch);
            }
        }

        /// <summary>
        /// The difference in Roll for point 4 in degrees.
        /// </summary>
        private string point4_Diff_Roll;
        /// <summary>
        /// The difference in Roll for point 4 in degrees.
        /// </summary>
        public string Point4_Diff_Roll
        {
            get { return point4_Diff_Roll; }
            set
            {
                point4_Diff_Roll = value;
                this.NotifyOfPropertyChange(() => this.Point4_Diff_Roll);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Busy Indicator

        /// <summary>
        /// Flag used to tell the display a busy activity is
        /// occuring.
        /// </summary>
        private bool isBusyIndicator;
        /// <summary>
        /// Flag used to tell the display a busy activity is
        /// occuring.
        /// </summary>
        public bool IsBusyIndicator
        {
            get { return isBusyIndicator; }
            set
            {
                isBusyIndicator = value;
                this.NotifyOfPropertyChange(() => this.IsBusyIndicator);

                // Reset the busy status message
                if (!value)
                {
                    BusyStatus = "Loading...";
                }
            }
        }

        /// <summary>
        /// Status message to display when the
        /// busy indicator is shown.
        /// </summary>
        private string _busyStatus;
        /// <summary>
        /// Status message to display when the
        /// busy indicator is shown.
        /// </summary>
        public string BusyStatus
        {
            get { return _busyStatus; }
            set
            {
                _busyStatus = value;
                this.NotifyOfPropertyChange(() => this.BusyStatus);
            }
        }

        #endregion

        #region Status Bar

        /// <summary>
        /// Store event information.
        /// </summary>
        private StatusEvent _statusBarEvent;

        /// <summary>
        /// Text for the status bar.
        /// </summary>
        public string StatusBarText
        {
            get { return _statusBarEvent.Message; }
        }

        /// <summary>
        /// Background color of the status bar.
        /// </summary>
        public string StatusBarBackground
        {
            get { return _statusBarEvent.Color; }
        }

        /// <summary>
        /// The duration to show the status bar event.
        /// The duration is subtracted by 1 so the event
        /// can disappear for 1 second.
        /// </summary>
        public string StatusBarDurationStart
        {
            get
            {
                return "0:0:" + (_statusBarEvent.Duration - 1).ToString();
            }
        }

        /// <summary>
        /// The total duration of the event to be displayed.
        /// </summary>
        public string StatusBarDurationStop
        {
            get { return "0:0:" + (_statusBarEvent.Duration).ToString(); }
        }

        #endregion

        #region Validate

        /// <summary>
        /// Set flag to validate the Calibration score.
        /// It is very difficult to pass based off calibration score.
        /// This will turn off validiating the calibration score.
        /// </summary>
        private bool isValidateCalScore;
        /// <summary>
        /// Set flag if verify should be turned on or off.
        /// Turning on verify will cause initial and post
        /// points to be collected to verify the calibration 
        /// was good.
        /// </summary>
        public bool IsValidateCalScore
        {
            get { return isValidateCalScore; }
            set
            {
                isValidateCalScore = value;
                this.NotifyOfPropertyChange(() => this.IsValidateCalScore);

                //// Save the setting
                //CompassCal.Properties.Settings.Default.IsValidateCalScore = value;
                //CompassCal.Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region Result Text

        /// <summary>
        /// Result Text file.  The results will be stored to this
        /// file.
        /// </summary>
        private string _resultTxtFile;
        /// <summary>
        /// Result Text file.  The results will be stored to this
        /// file.
        /// </summary>
        public string ResultTextFile
        {
            get { return _resultTxtFile; }
            set
            {
                _resultTxtFile = value;
                this.NotifyOfPropertyChange(() => this.ResultTextFile);
            }
        }

        #endregion

        #region Diag Display

        /// <summary>
        /// Display the compass returns.
        /// </summary>
        private string _DiagDisplay;
        /// <summary>
        /// Display the compass returns.
        /// </summary>
        public string DiagDisplay
        {
            get { return _DiagDisplay; }
            set
            {
                _DiagDisplay = value;
                this.NotifyOfPropertyChange(() => this.DiagDisplay);
            }
        }

        #endregion

        #region Admin

        /// <summary>
        /// Set a flag if the user is an Admin.
        /// </summary>
        private bool _isAdmin;
        /// <summary>
        /// Set a flag if the user is an Admin.
        /// </summary>
        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                this.NotifyOfPropertyChange(() => this.IsAdmin);
            }
        }

        #endregion

        #region Button Flags

        /// <summary>
        /// Because interval mode will output data at any time,
        /// this flag is used to state when we want to read data
        /// and when we do not want to read data.
        /// </summary>
        private bool _IsReadingCompass;
        /// <summary>
        /// Because interval mode will output data at any time,
        /// this flag is used to state when we want to read data
        /// and when we do not want to read data.
        /// </summary>
        public bool IsReadingCompass
        {
            get { return _IsReadingCompass; }
            set
            {
                _IsReadingCompass = value;
                this.NotifyOfPropertyChange(() => this.IsReadingCompass);
                this.NotifyOfPropertyChange(() => this.CanReadCompass);
                this.NotifyOfPropertyChange(() => this.CanCompassTap);
            }
        }

        /// <summary>
        /// Set flag that we are collecting pre points.
        /// </summary>
        private bool _IsGettingPrePoints;
        /// <summary>
        /// Set flag that we are collecting pre points.
        /// </summary>
        public bool IsGettingPrePoints
        {
            get { return _IsGettingPrePoints; }
            set
            {
                _IsGettingPrePoints = value;
                this.NotifyOfPropertyChange(() => this.IsGettingPrePoints);
                this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            }
        }

        /// <summary>
        /// Set flag that we are collecting post points.
        /// </summary>
        private bool _IsGettingPostPoints;
        /// <summary>
        /// Set flag that we are collecting post points.
        /// </summary>
        public bool IsGettingPostPoints
        {
            get { return _IsGettingPostPoints; }
            set
            {
                _IsGettingPostPoints = value;
                this.NotifyOfPropertyChange(() => this.IsGettingPostPoints);
                this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            }
        }

        /// <summary>
        /// Enable or disable the compass cal button.
        /// </summary>
        private bool _CanStartCompassCal;
        /// <summary>
        /// Enable or disable the compass cal button.
        /// </summary>
        public bool CanStartCompassCal
        {
            get { return _CanStartCompassCal; }
            set
            {
                _CanStartCompassCal = value;
                this.NotifyOfPropertyChange(() => this.CanStartCompassCal);
            }
        }

        /// <summary>
        /// Flag if we can read the compass.
        /// This will check if we are doing a compass cal
        /// or already reading the compass.
        /// </summary>
        public bool CanReadCompass
        {
            get { return !IsCompassCal && !_IsReadingCompass; }
        }

        /// <summary>
        /// Flag if a sample can be taken.
        /// This will check if we are currently getting any points.
        /// </summary>
        public bool CanTakeCompassCalSample
        {
            get { return IsCompassCal || _IsGettingPrePoints || _IsGettingPostPoints; }
        }

        /// <summary>
        /// Flag if we can get the compass tap.
        /// This will check if we are doing a compass cal
        /// or already reading the compass. 
        /// </summary>
        public bool CanCompassTap
        {
            get { return !IsCompassCal && !_IsReadingCompass; }
        }

        #endregion

        #region Compass Utility VM

        /// <summary>
        /// Text Ensemble View Model.
        /// </summary>
        public CompassUtilityViewModel CompassUtilityVM { get; set; }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Start the compass calibration process.
        /// </summary>
        public ReactiveCommand<object> CompassCalCommand { get; protected set; }

        /// <summary>
        /// Connect the ADCP to the compass.
        /// </summary>
        public ReactiveCommand<object> CompassConnectCommand { get; protected set; }

        /// <summary>
        /// Disconnect the ADCP from the compass.
        /// </summary>
        public ReactiveCommand<object> CompassDisconnectCommand { get; protected set; }

        /// <summary>
        /// Save the compass calibration settings.
        /// </summary>
        public ReactiveCommand<object> SaveCompassCalCommand { get; protected set; }

        /// <summary>
        /// Read the compass in the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ReadCompassCommand { get; protected set; }

        /// <summary>
        /// Set the Compass Tap to 0.
        /// </summary>
        public ReactiveCommand<object> CompassTap0Command { get; protected set; }

        /// <summary>
        /// Set the Compass Tap to 4.
        /// </summary>
        public ReactiveCommand<object> CompassTap4Command { get; protected set; }

        /// <summary>
        /// Set the default values for the Compass calibration.
        /// </summary>
        public ReactiveCommand<object> SetDefaultCompassCalMagCommand { get; protected set; }

        /// <summary>
        /// Set the Default values for the Compass Acceleramator Calibration. 
        /// </summary>
        public ReactiveCommand<object> SetDefaultCompassCalAccelCommand { get; protected set; }

        /// <summary>
        /// Take a compass cal sample.
        /// </summary>
        public ReactiveCommand<object> TakeCompassCalSampleCommand { get; protected set; }

        /// <summary>
        /// Clear the diag display..
        /// </summary>
        public ReactiveCommand<object> ClearDiagDisplayCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Constructor
        /// 
        /// Initialize ranges.
        /// </summary>
        public CompassCalViewModel()
            : base("Compass Cal")
        {
            // Initialize ranges
            _eventAggregator = IoC.Get<IEventAggregator>();
            _adcpConn = IoC.Get<AdcpConnection>();

            //_adcpSerialPort = IoC.Get<AdcpConnection>().AdcpSerialPort;
            _compassCodec = new RTI.PniPrimeCompassBinaryCodec();
            _compassDataResponse = new PniPrimeCompassBinaryCodec.PniDataResponse();
            _isGoodCal = false;
            CompassCalButtonLabel = "Start";
            CompassDataButtonLabel = "Connect";
            ResultTextFile = RTI.Pulse.Commons.GetAppStorageDir() + @"\" + DEFAULT_RESULT_TXT;
            _statusBarEvent = new StatusEvent("");
            IsCompassCal = false;
            IsCompassConnected = false;
            IsMagAndAccelCalibration = false;
            DiagDisplay = "";

            // Set if the user is an admin
            IsAdmin = Pulse.Commons.IsAdmin();

            // Get the CompassUtility VM
            CompassUtilityVM = IoC.Get<CompassUtilityViewModel>();

            IsReadingCompass = false;
            _isGoodCal = false;
            IsGettingPrePoints = false;
            _pointCount = 0;
            CanStartCompassCal = true;

            SetAdcpStatus(new AdcpStatus(eAdcpStatus.Unknown));

            // Get all the settings
            GetSettings();

            // Initialize Pre/Post Points
            InitPrePostPoints();
            InitTestResults();

            _IsCalibrationRunning = false;

            InitializeCompassCalValues();
            InitializeCompassConfigValues();

            // Wait for incoming cal samples
            InitializeCompassCalValues();

            // Create a command to start Compass Cal
            CompassCalCommand = ReactiveCommand.Create(this.WhenAny(x => x.CanStartCompassCal, x => x.Value));
            CompassCalCommand.Subscribe(_ => OnCompassCal());

            // Create a command to start Compass Data
            CompassConnectCommand = ReactiveCommand.Create(this.WhenAny(x => x.IsCompassCal, x => !x.Value));
            CompassConnectCommand.Subscribe(_ => CompassConnect());

            // Create a command to start Compass Data
            CompassDisconnectCommand = ReactiveCommand.Create(this.WhenAny(x => x.IsCompassCal, x => !x.Value));
            CompassDisconnectCommand.Subscribe(_ => CompassDisconnect());

            // Create a command to Save Compass Cal
            SaveCompassCalCommand = ReactiveCommand.Create();
            SaveCompassCalCommand.Subscribe(_ => OnSaveCompassCal());

            // Create a command to Read the compass
            ReadCompassCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.CanReadCompass, x => x.Value), _ => Task.Run(() => OnReadCompass()));
            //ReadCompassCommand.Subscribe(_ => OnReadCompass());

            // Create a command to set the Compass to Tap 0
            CompassTap0Command = ReactiveCommand.Create(this.WhenAny(x => x.CanCompassTap, x => x.Value));
            CompassTap0Command.Subscribe(_ => Task.Run(() => OnCompassTap0()));

            // Create a command to set the Compass to Tap 4
            CompassTap4Command = ReactiveCommand.Create(this.WhenAny(x => x.CanCompassTap, x => x.Value));
            CompassTap4Command.Subscribe(_ => Task.Run(() => OnCompassTap4()));

            // Create a command to set the Default for Compass Cal Mag
            SetDefaultCompassCalMagCommand = ReactiveCommand.Create(this.WhenAny(x => x.IsCompassCal, x => !x.Value));
            SetDefaultCompassCalMagCommand.Subscribe(_ => OnSetDefaultCompassCalMag());

            // Create a command to set the Default for Compass Cal Accel
            SetDefaultCompassCalAccelCommand = ReactiveCommand.Create(this.WhenAny(x => x.IsCompassCal, x => !x.Value));
            SetDefaultCompassCalAccelCommand.Subscribe(_ => Task.Run(() => OnSetDefaultCompassCalAccel()));

            // Create a command to take a sample 
            TakeCompassCalSampleCommand = ReactiveCommand.Create(this.WhenAny(x => x.CanTakeCompassCalSample, x => x.Value));
            TakeCompassCalSampleCommand.Subscribe(_ => Task.Run(() => OnTakeCompassCalSample()));

            // Create a command to clear the diag display
            ClearDiagDisplayCommand = ReactiveCommand.Create();
            ClearDiagDisplayCommand.Subscribe(_ => DiagDisplay = "");

            // Setup the serial port to receive serial port events
            SetupAdcpEvents();
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            // Remove subscriptions
            UnsubscribeAdcpEvents();

            // Dispose Codec
            _compassCodec.Dispose();

            // Dispose Utility VM
            if (CompassUtilityVM != null)
            {
                CompassUtilityVM.Dispose();
            }

        }

        #region Methods

        #region Serial Port

        /// <summary>
        /// Create new serial ports with the options
        /// from the serial options.
        /// This will also subscribe to receive events
        /// from the serial ports and clear the buffers.
        /// </summary>
        private void SetupAdcpEvents()
        {
            // If the serial port was previous connected, 
            // Unsubscribe events.
            if (_adcpConn.AdcpSerialPort != null)
            {
                UnsubscribeAdcpEvents();

                //// Create serial ports
                //_adcpSerialPort = new RTI.AdcpSerialPort(_adcpSerialOptions);

                // Subscribe to receive event when data received
                _adcpConn.AdcpSerialPort.ReceiveAdcpSerialDataEvent += new RTI.AdcpSerialPort.ReceiveAdcpSerialDataEventHandler(On_ReceiveAdcpSerialDataEvent);
                _adcpConn.AdcpSerialPort.ReceiveCompassSerialDataEvent += new RTI.AdcpSerialPort.ReceiveCompassSerialDataEventHandler(On_ReceiveCompassSerialDataEvent);
                _adcpConn.AdcpSerialPort.ReceiveSerialData += new RTI.SerialConnection.ReceiveSerialDataEventHandler(On_AdcpReceiveSerialData);

                // Wait for incoming cal samples
                InitializeCompassCalValues();
                _compassCodec.CompassEvent += new RTI.PniPrimeCompassBinaryCodec.CompassEventHandler(CompassCodecEventHandler);

                // Connect the serial port if it is not already
                //_adcpConn.AdcpSerialPort.Connect();

            }
        }

        /// <summary>
        /// Unsubscribe from the ADCP serial port events.
        /// </summary>
        private void UnsubscribeAdcpEvents()
        {
            _adcpConn.AdcpSerialPort.ReceiveAdcpSerialDataEvent -= On_ReceiveAdcpSerialDataEvent;
            _adcpConn.AdcpSerialPort.ReceiveCompassSerialDataEvent -= On_ReceiveCompassSerialDataEvent;
            _compassCodec.CompassEvent -= CompassCodecEventHandler;
            _adcpConn.AdcpSerialPort.ReceiveSerialData -= On_AdcpReceiveSerialData;

            //_adcpSerialPort.Shutdown();
        }

        #endregion

        #region Settings

        /// <summary>
        /// Get the settings stored by the application.
        /// </summary>
        private void GetSettings()
        {
            // Set if we are verifying calibration
            IsValidateCalScore = DEFAULT_IS_VALIDATE_CAL_SCORE;

            // Set points
            Point1_Loc = DEFAULT_POINT1_LOC;
            Point2_Loc = DEFAULT_POINT2_LOC;
            Point3_Loc = DEFAULT_POINT3_LOC;
            Point4_Loc = DEFAULT_POINT4_LOC;
        }


        #endregion

        #region ADCP Status

        /// <summary>
        /// Update the status of the ADCP connection.
        /// </summary>
        /// <param name="status">Status of the ADCP.</param>
        private void SetAdcpStatus(AdcpStatus status)
        {
            // Turn off updating the serial data
            //_eventAggregator.GetEvent<AdcpStatusUpdateEvent>().Publish(status);
        }

        #endregion

        #region Compass

        /// <summary>
        /// Set the ADCP to compass mode.  If we are currently doing a compass 
        /// calibration, send a warning.  Set the ADCP to compass mode.  Set 
        /// the ADCP status to compass mode.
        /// </summary>
        /// <returns>TRUE = Compass connected.</returns>
        private bool CompassConnect()
        {
            // If we are currently doing a compass cal,
            // give a warning and do not continue
            if (IsCompassCal)
            {
                SetStatusBar(new StatusEvent("Compass calibration in progress.  Stop the calibration first.", MessageBoxImage.Error));
                return false;
            }

            // Check if we need to be put in compass mode
            if (!IsCompassConnected)
            {
                // Stop ping just in case it is pinging
                // Send command to stop pinging
                // If the command is not sent properly,
                // send a message to the user that a connection
                // is probably not made to the ADCP.
                if (!_adcpConn.AdcpSerialPort.StopPinging())
                {
                    // Try to fix the issue with the ADCP
                    // or give a warning to the user
                    //if (!SerialCommunicationIssue())
                    //{
                    // Do not continue trying to setup the ADCP for compass mode
                    SetStatusBar(new StatusEvent("Compass Issue.  Could not connect to compass.", MessageBoxImage.Error));
                    return false;
                    //}
                }

                IsCompassConnected = true;

                // Set status that ADCP is connected
                SetAdcpStatus(new AdcpStatus(eAdcpStatus.Compass));

                // Send the commands
                // Put ADCP in Compass mode
                // Set the serial port to COMPASS mode to decode compass data
                if (!_adcpConn.AdcpSerialPort.StartCompassMode())
                {
                    SetStatusBar(new StatusEvent("Compass Issue.  Could not connect to compass.", MessageBoxImage.Error));
                    return false;
                }

                Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);                  // Delay for 485 response

                // Clear the buffer of any data from last calibration
                _compassCodec.ClearIncomingData();
            }

            // Reset the compass buttons status
            ResetCompassButtonStatus();

            return true;
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

            // Set flag that disconnected
            IsCompassConnected = false;

            // Set status that ADCP is connected
            SetAdcpStatus(new AdcpStatus(eAdcpStatus.Connected));

            // Reset the compass buttons status
            ResetCompassButtonStatus();
        }

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

        /// <summary>
        /// Update the compass button status.  This will
        /// reset the enables and disables of the compass buttons.
        /// </summary>
        private void ResetCompassButtonStatus()
        {
            //// Set button disable or enable
            //this.NotifyOfPropertyChange(() => this.CanStartCompassCal);
            //this.NotifyOfPropertyChange(() => this.CanReadCompass);
            //this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            //this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            //this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            //((DelegateCommand<object>)CompassCalCommand).RaiseCanExecuteChanged();
            //((DelegateCommand<object>)CompassConnectCommand).RaiseCanExecuteChanged();
            //((DelegateCommand<object>)SetDefaultCompassCalMagCommand).RaiseCanExecuteChanged();
            //((DelegateCommand<object>)SetDefaultCompassCalAccelCommand).RaiseCanExecuteChanged();
            //((DelegateCommand<object>)ReadCompassCommand).RaiseCanExecuteChanged();
            //((DelegateCommand<object>)CompassDisconnectCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Send a command an value to the ADCP.  This will
        /// send the command and if required a value to the adcp
        /// for the compass to process.
        /// </summary>
        /// <param name="id">Command ID.</param>
        /// <param name="value">Value for command if required.</param>
        private void SendConfigCommand(PniPrimeCompassBinaryCodec.ID id, object value)
        {
            // Connect the compass
            CompassConnect();

            // Send command to Read Compass data
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.SetConfigCommand(id, value));

            CompassDisconnect();
        }

        /// <summary>
        /// Set the default values for the Compass Configuration.
        /// </summary>
        private void InitializeCompassConfigValues()
        {
            _compassCalDeclination = 0;
            this.NotifyOfPropertyChange(() => this.CompassCalDeclination);

            _isCompassCalStableCheck = true;
            this.NotifyOfPropertyChange(() => this.IsCompassCalStableCheck);

            _isCompassCalAutoSample = true;
            this.NotifyOfPropertyChange(() => this.IsCompassCalAutoSample);

            _compassCalNumSamples = 12;
            this.NotifyOfPropertyChange(() => this.CompassCalNumSamples);
        }

        /// <summary>
        /// Initialize all the Compass Calibration
        /// ranges.
        /// </summary>
        private void InitializeCompassCalValues()
        {
            _isGoodCal = false;
            CalSamples = 0;
            CalPosition = "";
            CalScore_StdDevErr = 0;
            CalScore_xCoverage = 0;
            CalScore_yCoverage = 0;
            CalScore_zCoverage = 0;
            CalScore_xAccelCoverage = 0;
            CalScore_yAccelCoverage = 0;
            CalScore_zAccelCoverage = 0;
            CalScore_accelStdDevErr = 0;
        }

        /// <summary>
        /// Validate the compass calibration scores against
        /// the min and max ranges.  If any are bad, then
        /// the calibration was bad.
        /// </summary>
        /// <returns></returns>
        private bool ValidateCalScore()
        {
            //bool MagResult = false;
            //bool AccelResult = false;

            //if (CalScore_xCoverage > MIN_MAG_XY_COVERAGE &&
            //    CalScore_yCoverage > MIN_MAG_XY_COVERAGE &&
            //    CalScore_zCoverage > MIN_MAG_Z_COVERAGE &&
            //    CalScore_StdDevErr < MAX_MAG_STDEV)
            //{
            //    MagResult = true;
            //}

            //if (CalScore_xAccelCoverage > MIN_ACCEL_XY_COVERAGE &&
            //    CalScore_yAccelCoverage > MIN_ACCEL_XY_COVERAGE &&
            //    CalScore_zAccelCoverage > MIN_ACCEL_Z_COVERAGE &&
            //    CalScore_accelStdDevErr < MAX_ACCEL_STDEV)
            //{
            //    AccelResult = true;
            //}

            //if (IsMagAndAccelCalibration)
            //{
            //    return MagResult && AccelResult;
            //}
            //else
            //{
            //    return MagResult;
            //}

            // If all the values are negative, then
            // a calibration could not be completed
            // Return false that a bad calibration was done.
            if (CalScore_xCoverage < 0 &&
                CalScore_yCoverage < 0 &&
                CalScore_zCoverage < 0 &&
                CalScore_StdDevErr < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Notify all the properties in _compassDataResponse.
        /// </summary>
        private void SetCompassDataResponse()
        {
            this.NotifyOfPropertyChange(() => this.CompassCalHeading);
            this.NotifyOfPropertyChange(() => this.CompassCalPitch);
            this.NotifyOfPropertyChange(() => this.CompassCalRoll);
            this.NotifyOfPropertyChange(() => this.CompassCalStatusColor);
            this.NotifyOfPropertyChange(() => this.CompassCalStatus);
            this.NotifyOfPropertyChange(() => this.CompassCalMagX);
            this.NotifyOfPropertyChange(() => this.CompassCalMagY);
            this.NotifyOfPropertyChange(() => this.CompassCalMagZ);
            this.NotifyOfPropertyChange(() => this.CompassCalAccelPitch);
            this.NotifyOfPropertyChange(() => this.CompassCalAccelRoll);
            this.NotifyOfPropertyChange(() => this.CompassCalAccelZ);
        }

        /// <summary>
        /// Read all the compass config settings.
        /// </summary>
        private void ReadCompassConfig()
        {
            // Use this flag to ensure data
            // is only read and displayed when
            // we want it.  When disconnecting
            // it goes back into interval mode,
            // we can get more data.
            IsReadingCompass = true;

            // Connect the compass
            CompassConnect();

            // Disable the button
            //((DelegateCommand<object>)ReadCompassCommand).RaiseCanExecuteChanged();
            //this.NotifyOfPropertyChange(() => this.CanReadCompass);

            // Set the Compass to output all the components
            //_adcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetAllDataComponentsCommand());

            // Because the compass is in interval mode, this command may be duplicated
            // Send command to Read Compass data
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetDataCommand());

            // Get the Number of points
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID.kUserCalNumPoints));

            // Get the Stable Check
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID.kUserCalStableCheck));

            // Get the Auto Sampling
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling));

            // Get the Declination
            _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID.kDeclination));

            // Stop reading compass data
            IsReadingCompass = false;

            // Enable the button
            //((DelegateCommand<object>)ReadCompassCommand).RaiseCanExecuteChanged();
            //this.NotifyOfPropertyChange(() => this.CanReadCompass);

            // Set the Compass to output only the default components
            // DOES NOT PUT THE COMPASS BACK INTO THE CORRECT MODE
            //_adcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetHPRDataComponentsCommands());

            // Disconnect from compass
            CompassDisconnect();
        }

        #endregion

        #region Compass Cal

        /// <summary>
        /// Work to do when the backgroundworker is started.  This will ask if a compass calibration wants to
        /// be started.  If the user presses OK, then the calibration will start.
        /// </summary>
        /// <param name="sender">Not Used.</param>
        /// <param name="e">Not used.</param>
        private void _workerCompassCal_DoWork(object sender, DoWorkEventArgs e)
        {
            // Ask if they want to start a calibration
            MessageBoxResult result = MessageBox.Show("Begin compass calibration?", "Compass Calibration", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                // Run the Compass Cal
                StartCalibration();
            }
        }

        /// <summary>
        /// Start the compass calibration process.
        /// </summary>
        private void StartCompassCalProcess()
        {
            // Ask if they want to start a calibration
            MessageBoxResult result = MessageBox.Show("Begin compass calibration?", "Compass Calibration", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                // Run the Compass Cal
                Task.Run(() => StartCalibration());
            }
        }

        /// <summary>
        /// Start the calibration process.
        /// This will get the system info, turn off
        /// auto sample and start to collect points.
        /// </summary>
        private void StartCalibration()
        {
            _IsCalibrationRunning = true;

            // Set button label
            CompassCalButtonLabel = "Stop";

            // Initialize test results
            InitTestResults();

            //// Test the database connection if the user is an admin
            //if (IsAdmin && !TestDatabaseConnection())
            //{
            //    // If fails, ask if the user wants to continue
            //    MessageBoxResult databaseTest = MessageBox.Show("Database Connection Failed.  Do you want to continue?", "Database Test", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //    if (databaseTest == MessageBoxResult.No)
            //    {
            //        // Stop the Compass Cal
            //        StopCompassCal();

            //        return;
            //    }
            //}

            // Set test status
            _testResult_CompassCalComplete = TestResultStatus.IN_PROGRESS;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassCalComplete);

            // Get the hardware, firmware and serial
            GetSystemInfo();

            // Read Current Settings of the compass
            //OnReadCompass(null);
            //ReadCompassConfig();

            // Turn off Auto Sample
            SendCompassConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling, false);

            // Check if compass cal cancel is called
            //if (_workerCompassCal.CancellationPending)
            if(!_IsCalibrationRunning)
            {
                return;
            }

            // Read current configuration
            ReadCompassConfig();

            // Check if compass cal cancel is called
            //if (_workerCompassCal.CancellationPending)
            if(!_IsCalibrationRunning)
            {
                return;
            }

            // Sleep to allow the auto sample to be set
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);

            // Verify Auto Sampling is turned off
            // If it is not turned off, then there was an issue communicating with the GPS
            if(IsCompassCalAutoSample)
            {
                SetStatusBar(new StatusEvent("Issue communicating with the compass. Could not change settings.", MessageBoxImage.Error, 30));
                StopCompassCal();
                return;
            }

            // Run the Initial Points
            //_pointCount = 0;
            //InitPrePostPoints();
            //_isGettingPrePoints = true;
            //((DelegateCommand<object>)TakeCompassCalSampleCommand).RaiseCanExecuteChanged();
            //this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
            //GetPoints(true, _pointCount);
            NextStep(CalSteps.StartPrePoint);

            // Check if compass cal cancel is called
            //if (_workerCompassCal.CancellationPending)
            if(!_IsCalibrationRunning)
            {
                return;
            }
        }

        /// <summary>
        /// Start a compass calibration.  This will start the
        /// calibration test.
        /// </summary>
        private void RunCompassCal()
        {
            // Set test status
            _testResult_CompassComm = TestResultStatus.IN_PROGRESS;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassComm);

            // Connect to the compass
            if (!CompassConnect())
            {
                // Set test status
                _testResult_CompassComm = TestResultStatus.FAIL;
                this.NotifyOfPropertyChange(() => this.TestResult_CompassComm);

                // Could not get into compass mode
                return;
            }

            // Set test status
            _testResult_CompassComm = TestResultStatus.PASS;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassComm);

            // Check if compass cal cancel is called
            //if (_workerCompassCal.CancellationPending)
            if (!_IsCalibrationRunning)
            {
                return;
            }

            // Reset the ranges
            InitializeCompassCalValues();

            // Start compass in calibration mode
            _adcpConn.AdcpSerialPort.StartCompassCal(IsMagAndAccelCalibration);

            // Set flag that doing a compass calibration
            IsCompassCal = true;
        }

        /// <summary>
        /// Stop a compass calibration.
        /// If the compass calibration is in process,
        /// this will stop the calibration process.
        /// </summary>
        private void StopCompassCal()
        {
            // Disable Compass Cal button
            CanStartCompassCal = false;

            // Set flag we are not compass calibrating
            IsCompassCal = false;
            IsGettingPrePoints = false;
            IsGettingPostPoints = false;

            // Stop Calibration mode
            _adcpConn.AdcpSerialPort.StopCompassCal();

            // Sleep to allow the compass cal to stop 
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);

            // Turn on Auto Sample
            SendCompassConfigCommand(RTI.PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling, true);

            // Sleep to allow the auto sample to be set
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);

            // Read current configuration
            ReadCompassConfig();

            // Sleep to allow the auto sample to be set
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);

            // Disconnect from the compass
            CompassDisconnect();

            // Enable compass cal button
            CanStartCompassCal = true;
        }

        /// <summary>
        /// Complete the compass cal.  If the compass cal was successful,
        /// call this method to wrap up the compass cal.  This will record
        /// the results, and display compass cal is complete.
        /// </summary>
        private void CompleteCompassCal()
        {
            // Set flag to stop running the compass cal process
            _IsCalibrationRunning = false;

            //if (IsAdmin)
            //{
                // Output Results to a file
                WriteResults();
            //}

            // Write results to maintence log
            WriteResultsToMaintenceLog();

            //// If is an Admin, then save the results to the database
            //if (IsAdmin)
            //{
            //    // Output results to database
            //    AddResultsToDb();
            //}

            // Turn off sample button
            IsGettingPostPoints = false;

            // Set test status
            _testResult_CompassCalComplete = TestResultStatus.PASS;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassCalComplete);

            SetStatusBar(new StatusEvent("Compass Calibration Complete."));
            CalSamples = 0;
            CalPosition = "Calibration Complete";

            // Set flag we are not compass calibrating
            IsCompassCal = false;
            IsGettingPrePoints = false;
            IsGettingPostPoints = false;
        }

        /// <summary>
        /// This will give the command ENGPNI to get
        /// the compass data.
        /// </summary>
        private RTI.Commands.HPR GetAdcpCompassReading()
        {
            _adcpConn.AdcpSerialPort.ReceiveBufferString = "";

            // Send a command for compass reading
            _adcpConn.AdcpSerialPort.SendData(RTI.Commands.AdcpCommands.CMD_ENGPNI);

            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Decode the the command results
            return RTI.Commands.AdcpCommands.DecodeEngPniResult(_adcpConn.AdcpSerialPort.ReceiveBufferString);
        }

        /// <summary>
        /// Send a break to get the serial number, firmware, and hardware.
        /// </summary>
        private void GetSystemInfo()
        {
            //// Set test status
            //_testResult_AdcpComm = TestResultStatus.IN_PROGRESS;
            //this.NotifyOfPropertyChange(() => this.TestResult_AdcpComm);

            // Clear buffer
            _adcpConn.AdcpSerialPort.ReceiveBufferString = "";

            //// Send a Break
            _adcpConn.AdcpSerialPort.SendBreak();

            // Wait for an output
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Get the buffer output
            string buffer = _adcpConn.AdcpSerialPort.ReceiveBufferString;

            // Decode break statement
            RTI.Commands.BreakStmt brk = RTI.Commands.AdcpCommands.DecodeBREAK(buffer);
            SerialNumber = brk.SerialNum;
            Firmware = brk.FirmwareVersion;
            Hardware = brk.Hardware;

            //// Set test status
            //_testResult_AdcpComm = TestResultStatus.PASS;
            //this.NotifyOfPropertyChange(() => this.TestResult_AdcpComm);
        }

        /// <summary>
        /// Get the Pre and post Points.  This will collect the 
        /// points used for validation.
        /// </summary>
        /// <param name="count">Which point are we currently processing.</param>
        /// <param name="isPre">Flag if the points are pre points or post points.</param>
        private void GetPoints(bool isPre, int count)
        {
            // Check if compass cal cancel is called
            //if (_workerCompassCal.CancellationPending)
            if (!_IsCalibrationRunning)
            {
                return;
            }

            // Temporarly turn off the buttons
            // This is to prevent double clicks on the button
            bool tempPre = _IsGettingPrePoints;
            bool tempPost = _IsGettingPostPoints;
            IsGettingPrePoints = false;
            IsGettingPostPoints = false;

            // Get a compass sample
            RTI.Commands.HPR result = GetAdcpCompassReading();

            if (count == 0)
            {
                CalSamples = 0;
                CalPosition = Point1_Loc + "°";
            }

            // Point 1
            if (count == 1)
            {
                if (isPre)
                {
                    Point1_Pre_Hdg = result.Heading;
                    Point1_Pre_Ptch = result.Pitch;
                    Point1_Pre_Roll = result.Roll;
                }
                else
                {
                    Point1_Post_Hdg = result.Heading;
                    Point1_Post_Ptch = result.Pitch;
                    Point1_Post_Roll = result.Roll;
                }
                CalSamples = 1;
                CalPosition = Point2_Loc + "°";
            }

            // Point 2
            if (count == 2)
            {
                if (isPre)
                {
                    Point2_Pre_Hdg = result.Heading;
                    Point2_Pre_Ptch = result.Pitch;
                    Point2_Pre_Roll = result.Roll;
                }
                else
                {
                    Point2_Post_Hdg = result.Heading;
                    Point2_Post_Ptch = result.Pitch;
                    Point2_Post_Roll = result.Roll;
                }
                CalSamples = 2;
                CalPosition = Point3_Loc + "°";
            }

            // Point 3
            if (count == 3)
            {
                if (isPre)
                {
                    Point3_Pre_Hdg = result.Heading;
                    Point3_Pre_Ptch = result.Pitch;
                    Point3_Pre_Roll = result.Roll;
                }
                else
                {
                    Point3_Post_Hdg = result.Heading;
                    Point3_Post_Ptch = result.Pitch;
                    Point3_Post_Roll = result.Roll;
                }
                CalSamples = 3;
                CalPosition = Point4_Loc + "°";
            }

            // Point 4
            // Start the compass cal after reading the value
            if (count == 4)
            {
                if (isPre)
                {
                    Point4_Pre_Hdg = result.Heading;
                    Point4_Pre_Ptch = result.Pitch;
                    Point4_Pre_Roll = result.Roll;

                    //_isGettingPrePoints = false;
                    //((DelegateCommand<object>)TakeCompassCalSampleCommand).RaiseCanExecuteChanged();
                    //this.NotifyOfPropertyChange(() => this.CanTakeCompassCalSample);
                    //RunCompassCal();
                    NextStep(CalSteps.StopPrePoint);
                }
                else
                {
                    Point4_Post_Hdg = result.Heading;
                    Point4_Post_Ptch = result.Pitch;
                    Point4_Post_Roll = result.Roll;

                    //// Check bad values
                    //CheckForBadPoints();

                    //// Calculate DIff
                    //CalculateDiff();
                    NextStep(CalSteps.StopPostPoint);

                    // Set the temp to false to complete
                    // the post point collection
                    tempPost = false;
                }
            }

            // Move to the next point
            _pointCount++;

            // Turn back on the flags
            IsGettingPrePoints = tempPre;
            IsGettingPostPoints = tempPost;
        }

        /// <summary>
        /// Calculate the difference between the initial and new point.
        /// </summary>
        private void CalculateDiff()
        {
            string ACURACY = "0.0000";

            // Point 1
            Point1_Diff_Hdg = (Point1_Post_Hdg - Point1_Loc).ToString(ACURACY);
            Point1_Diff_Ptch = (Point1_Post_Ptch - Point1_Pre_Ptch).ToString(ACURACY);
            double p1pre_roll = (Point1_Pre_Roll < 0) ? -Point1_Pre_Roll : Point1_Pre_Roll;
            double p1post_roll = (Point1_Post_Roll < 0) ? -Point1_Post_Roll : Point1_Post_Roll;
            Point1_Diff_Roll = (p1post_roll - p1pre_roll).ToString(ACURACY);

            // Point 2
            Point2_Diff_Hdg = (Point2_Post_Hdg - Point2_Loc).ToString(ACURACY);
            Point2_Diff_Ptch = (Point2_Post_Ptch - Point2_Pre_Ptch).ToString(ACURACY);
            double p2pre_roll = (Point2_Pre_Roll < 0) ? -Point2_Pre_Roll : Point2_Pre_Roll;
            double p2post_roll = (Point2_Post_Roll < 0) ? -Point2_Post_Roll : Point2_Post_Roll;
            Point2_Diff_Roll = (p2post_roll - p2pre_roll).ToString(ACURACY);

            // Point 3
            Point3_Diff_Hdg = (Point3_Post_Hdg - Point3_Loc).ToString(ACURACY);
            Point3_Diff_Ptch = (Point3_Post_Ptch - Point3_Pre_Ptch).ToString(ACURACY);
            double p3pre_roll = (Point3_Pre_Roll < 0) ? -Point3_Pre_Roll : Point3_Pre_Roll;
            double p3post_roll = (Point3_Post_Roll < 0) ? -Point3_Post_Roll : Point3_Post_Roll;
            Point3_Diff_Roll = (p3post_roll - p3pre_roll).ToString(ACURACY);

            // Point 4
            Point4_Diff_Hdg = (Point4_Post_Hdg - Point4_Loc).ToString(ACURACY);
            Point4_Diff_Ptch = (Point4_Post_Ptch - Point4_Pre_Ptch).ToString(ACURACY);
            double p4pre_roll = (Point4_Pre_Roll < 0) ? -Point4_Pre_Roll : Point4_Pre_Roll;
            double p4post_roll = (Point4_Post_Roll < 0) ? -Point4_Post_Roll : Point4_Post_Roll;
            Point4_Diff_Roll = (p4post_roll - p4pre_roll).ToString(ACURACY);
        }

        /// <summary>
        /// Check if any points taken were bad.
        /// A bad value will be a 0.  The point was not read.
        /// </summary>
        private void CheckForBadPoints()
        {
            if (Point1_Post_Hdg == 0 ||
                Point1_Pre_Hdg == 0 ||
                Point1_Post_Ptch == 0 ||
                Point1_Pre_Ptch == 0 ||
                Point1_Post_Roll == 0 ||
                Point1_Pre_Roll == 0 ||
                Point2_Post_Hdg == 0 ||
                Point2_Pre_Hdg == 0 ||
                Point2_Post_Ptch == 0 ||
                Point2_Pre_Ptch == 0 ||
                Point2_Post_Roll == 0 ||
                Point2_Pre_Roll == 0 ||
                Point3_Post_Hdg == 0 ||
                Point3_Pre_Hdg == 0 ||
                Point3_Post_Ptch == 0 ||
                Point3_Pre_Ptch == 0 ||
                Point3_Post_Roll == 0 ||
                Point3_Pre_Roll == 0 ||
                Point4_Post_Hdg == 0 ||
                Point4_Pre_Hdg == 0 ||
                Point4_Post_Ptch == 0 ||
                Point4_Pre_Ptch == 0 ||
                Point4_Post_Roll == 0 ||
                Point4_Pre_Roll == 0
                )
            {
                // Set test status
                _testResult_CompassCalGood = TestResultStatus.FAIL;
                this.NotifyOfPropertyChange(() => this.TestResult_CompassCalGood);
            }

        }

        #endregion

        #region Points

        /// <summary>
        /// Initialize the pre and post points to 0.
        /// </summary>
        private void InitPrePostPoints()
        {
            Point1_Pre_Hdg = 0;
            Point2_Pre_Hdg = 0;
            Point3_Pre_Hdg = 0;
            Point4_Pre_Hdg = 0;
            Point1_Post_Hdg = 0;
            Point2_Post_Hdg = 0;
            Point3_Post_Hdg = 0;
            Point4_Post_Hdg = 0;
            Point1_Diff_Hdg = "0";
            Point2_Diff_Hdg = "0";
            Point3_Diff_Hdg = "0";
            Point4_Diff_Hdg = "0";

            Point1_Pre_Ptch = 0;
            Point2_Pre_Ptch = 0;
            Point3_Pre_Ptch = 0;
            Point4_Pre_Ptch = 0;
            Point1_Post_Ptch = 0;
            Point2_Post_Ptch = 0;
            Point3_Post_Ptch = 0;
            Point4_Post_Ptch = 0;
            Point1_Diff_Ptch = "0";
            Point2_Diff_Ptch = "0";
            Point3_Diff_Ptch = "0";
            Point4_Diff_Ptch = "0";

            Point1_Pre_Roll = 0;
            Point2_Pre_Roll = 0;
            Point3_Pre_Roll = 0;
            Point4_Pre_Roll = 0;
            Point1_Post_Roll = 0;
            Point2_Post_Roll = 0;
            Point3_Post_Roll = 0;
            Point4_Post_Roll = 0;
            Point1_Diff_Roll = "0";
            Point2_Diff_Roll = "0";
            Point3_Diff_Roll = "0";
            Point4_Diff_Roll = "0";
        }

        #endregion

        #region Test Results

        /// <summary>
        /// If the test result is true, give the
        /// pass image.  If not give the fail image.
        /// </summary>
        /// <param name="status">Test status.</param>
        /// <returns>Image based off status.</returns>
        private string TestResultImage(TestResultStatus status)
        {
            switch (status)
            {
                case TestResultStatus.PASS:
                    return "../Images/test_pass.png";
                case TestResultStatus.FAIL:
                    return "../Images/test_fail.png";
                case TestResultStatus.IN_PROGRESS:
                    return "../Images/test_progress.png";
                case TestResultStatus.NOT_TESTING:
                    return "../Images/testnottesting.png";
                case TestResultStatus.NOT_STARTED:
                    return "../Images/test_notstarted.png";
                default:
                    return "";  // no image
            }
        }

        /// <summary>
        /// Initialize the test results.
        /// </summary>
        private void InitTestResults()
        {
            //_testResult_DatabaseComm = TestResultStatus.NOT_STARTED;
            //this.NotifyOfPropertyChange(() => this.TestResult_DatabaseComm);

            _testResult_AdcpComm = TestResultStatus.NOT_STARTED;
            this.NotifyOfPropertyChange(() => this.TestResult_AdcpComm);

            _testResult_CompassCalComplete = TestResultStatus.NOT_STARTED;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassCalComplete);

            _testResult_CompassCalGood = TestResultStatus.NOT_STARTED;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassCalGood);

            _testResult_CompassCalSaved = TestResultStatus.NOT_STARTED;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassCalSaved);

            _testResult_CompassComm = TestResultStatus.NOT_STARTED;
            this.NotifyOfPropertyChange(() => this.TestResult_CompassComm);
        }

        #endregion

        #region Status Bar

        /// <summary>
        /// Set the status bar with the latest message.
        /// </summary>
        /// <param name="statusEvent"></param>
        private void SetStatusBar(StatusEvent statusEvent)
        {
            _statusBarEvent = statusEvent;

            //this.NotifyOfPropertyChange(() => this.StatusBarBackground);
            //this.NotifyOfPropertyChange(() => this.StatusBarDurationStart);
            //this.NotifyOfPropertyChange(() => this.StatusBarDurationStop);
            this.NotifyOfPropertyChange(() => this.StatusBarText);
        }

        #endregion

        #region Write Results

        /// <summary>
        /// Create a string of all the results.
        /// </summary>
        /// <returns>String of the results of the calibration.</returns>
        private string ResultsString()
        {
            // Write the results
            var result = new StringBuilder();
            result.Append(SerialNumber + ", ");
            result.Append(Firmware + ", ");
            result.Append(CalScore_StdDevErr + ", ");
            result.Append(CalScore_xCoverage + ", ");
            result.Append(CalScore_yCoverage + ", ");
            result.Append(CalScore_zCoverage + ", ");
            result.Append(CalScore_accelStdDevErr + ", ");
            result.Append(CalScore_xAccelCoverage + ", ");
            result.Append(CalScore_yAccelCoverage + ", ");
            result.Append(CalScore_zAccelCoverage + ", ");
            result.Append(Point1_Pre_Hdg + ", ");
            result.Append(Point1_Pre_Ptch + ", ");
            result.Append(Point1_Pre_Roll + ", ");
            result.Append(Point2_Pre_Hdg + ", ");
            result.Append(Point2_Pre_Ptch + ", ");
            result.Append(Point2_Pre_Roll + ", ");
            result.Append(Point3_Pre_Hdg + ", ");
            result.Append(Point3_Pre_Ptch + ", ");
            result.Append(Point3_Pre_Roll + ", ");
            result.Append(Point4_Pre_Hdg + ", ");
            result.Append(Point4_Pre_Ptch + ", ");
            result.Append(Point4_Pre_Roll + ", ");
            result.Append(Point1_Post_Hdg + ", ");
            result.Append(Point1_Post_Ptch + ", ");
            result.Append(Point1_Post_Roll + ", ");
            result.Append(Point2_Post_Hdg + ", ");
            result.Append(Point2_Post_Ptch + ", ");
            result.Append(Point2_Post_Roll + ", ");
            result.Append(Point3_Post_Hdg + ", ");
            result.Append(Point3_Post_Ptch + ", ");
            result.Append(Point3_Post_Roll + ", ");
            result.Append(Point4_Post_Hdg + ", ");
            result.Append(Point4_Post_Ptch + ", ");
            result.Append(Point4_Post_Roll);

            return result.ToString();
        }

        /// <summary>
        /// Write the result to a file.
        /// </summary>
        private void WriteResults()
        {
            StringBuilder result;

            // If the file does not exist
            // Add the header to the file
            if (!File.Exists(ResultTextFile))
            {
                result = new StringBuilder();
                result.Append("SerialNumber" + ", ");
                result.Append("Firmware" + ", ");
                result.Append("CalScore_StdDevErr" + ", ");
                result.Append("CalScore_xCoverage" + ", ");
                result.Append("CalScore_yCoverage" + ", ");
                result.Append("CalScore_zCoverage" + ", ");
                result.Append("CalScore_accelStdDevErr" + ", ");
                result.Append("CalScore_xAccelCoverage" + ", ");
                result.Append("CalScore_yAccelCoverage" + ", ");
                result.Append("CalScore_zAccelCoverage" + ", ");
                result.Append("Point1_Pre_Hdg" + ", ");
                result.Append("Point1_Pre_Ptch" + ", ");
                result.Append("Point1_Pre_Roll" + ", ");
                result.Append("Point2_Pre_Hdg" + ", ");
                result.Append("Point2_Pre_Ptch" + ", ");
                result.Append("Point2_Pre_Roll" + ", ");
                result.Append("Point3_Pre_Hdg" + ", ");
                result.Append("Point3_Pre_Ptch" + ", ");
                result.Append("Point3_Pre_Roll" + ", ");
                result.Append("Point4_Pre_Hdg" + ", ");
                result.Append("Point4_Pre_Ptch" + ", ");
                result.Append("Point4_Pre_Roll" + ", ");
                result.Append("Point1_Post_Hdg" + ", ");
                result.Append("Point1_Post_Ptch" + ", ");
                result.Append("Point1_Post_Roll" + ", ");
                result.Append("Point2_Post_Hdg" + ", ");
                result.Append("Point2_Post_Ptch" + ", ");
                result.Append("Point2_Post_Roll" + ", ");
                result.Append("Point3_Post_Hdg" + ", ");
                result.Append("Point3_Post_Ptch" + ", ");
                result.Append("Point3_Post_Roll" + ", ");
                result.Append("Point4_Post_Hdg" + ", ");
                result.Append("Point4_Post_Ptch" + ", ");
                result.Append("Point4_Post_Roll");

                // Open write and write the line to the file
                using (StreamWriter w = File.AppendText(ResultTextFile))
                {
                    w.WriteLine(result.ToString());
                    w.Flush();
                    w.Close();
                }
            }

            // Open and write the line to the file
            using (StreamWriter w = File.AppendText(ResultTextFile))
            {
                w.WriteLine(ResultsString());
                w.Flush();
                w.Close();
            }
        }

        /// <summary>
        /// Create a maintence entry and add it to the
        /// maintence log.
        /// </summary>
        private void WriteResultsToMaintenceLog()
        {
            // Create an entry with the results
            // Add it to the list
            MaintenceEntry entry = new MaintenceEntry(MaintenceEntry.EntryId.UserCompassCal, ResultsString(), "");

            _adcpConn.AddMaintenceEntry(entry);
        }

        #endregion

        #region Send Complete Event

        /// <summary>
        /// Send the complete event with all the results
        /// to any subscribers.
        /// </summary>
        private void SendCompleteEvent()
        {
            // Populate the event
            CompassCalResultEvent ccEvent = new CompassCalResultEvent();
            ccEvent.SerialNumber = SerialNumber.SerialNumberString;
            ccEvent.Firmware = Firmware.ToString();
            ccEvent.CalScore_StdDevErr = CalScore_StdDevErr;
            ccEvent.CalScore_xCoverage = CalScore_xCoverage;
            ccEvent.CalScore_yCoverage = CalScore_yCoverage;
            ccEvent.CalScore_zCoverage = CalScore_zCoverage;
            ccEvent.CalScore_accelStdDevErr = CalScore_accelStdDevErr;
            ccEvent.CalScore_xAccelCoverage = CalScore_xAccelCoverage;
            ccEvent.CalScore_yAccelCoverage = CalScore_yAccelCoverage;
            ccEvent.CalScore_zAccelCoverage = CalScore_zAccelCoverage;
            ccEvent.Point1_Pre_Hdg = Point1_Pre_Hdg;
            ccEvent.Point1_Pre_Ptch = Point1_Pre_Ptch;
            ccEvent.Point1_Pre_Roll = Point1_Pre_Roll;
            ccEvent.Point2_Pre_Hdg = Point2_Pre_Hdg;
            ccEvent.Point2_Pre_Ptch = Point2_Pre_Ptch;
            ccEvent.Point2_Pre_Roll = Point2_Pre_Roll;
            ccEvent.Point3_Pre_Hdg = Point3_Pre_Hdg;
            ccEvent.Point3_Pre_Ptch = Point3_Pre_Ptch;
            ccEvent.Point3_Pre_Roll = Point3_Pre_Roll;
            ccEvent.Point4_Pre_Hdg = Point4_Pre_Hdg;
            ccEvent.Point4_Pre_Ptch = Point4_Pre_Ptch;
            ccEvent.Point4_Pre_Roll = Point4_Pre_Roll;
            ccEvent.Point1_Post_Hdg = Point1_Post_Hdg;
            ccEvent.Point1_Post_Ptch = Point1_Post_Ptch;
            ccEvent.Point1_Post_Roll = Point1_Post_Roll;
            ccEvent.Point2_Post_Hdg = Point2_Post_Hdg;
            ccEvent.Point2_Post_Ptch = Point2_Post_Ptch;
            ccEvent.Point2_Post_Roll = Point2_Post_Roll;
            ccEvent.Point3_Post_Hdg = Point3_Post_Hdg;
            ccEvent.Point3_Post_Ptch = Point3_Post_Ptch;
            ccEvent.Point3_Post_Roll = Point3_Post_Roll;
            ccEvent.Point4_Post_Hdg = Point4_Post_Hdg;
            ccEvent.Point4_Post_Ptch = Point4_Post_Ptch;
            ccEvent.Point4_Post_Roll = Point4_Post_Roll;

            // Send the event
            if(CompassCalCompleteEvent != null)
            {
                CompassCalCompleteEvent(ccEvent);
            }
        }

        #endregion

        #endregion

        #region Commands

        #region Start Compass Cal Command

        /// <summary>
        /// Send the command for Compass Cal.
        /// </summary>
        private void OnCompassCal()
        {
            // Check if a compass cal has already started
            // the worker thread will already be running
            //if (_workerCompassCal.IsBusy)
            if (_IsCalibrationRunning)
            {
                // Stop the worker thread
                //_workerCompassCal.CancelAsync();

                // If we are currently doing a compass cal, stop the calibration
                if (IsCompassCal || _IsGettingPrePoints || _IsGettingPostPoints)
                {
                    // Stop the Compass Cal
                    Task.Run(() => StopCompassCal());

                    // Set flag that calibration is not running because aborted
                    _IsCalibrationRunning = false;

                    // Set test status
                    _testResult_CompassCalComplete = TestResultStatus.FAIL;
                    this.NotifyOfPropertyChange(() => this.TestResult_CompassCalComplete);
                }
            }
            // Check if the compass cal is already in progress
            // or if we are getting pre or post points
            else if (IsCompassCal || _IsGettingPrePoints || _IsGettingPostPoints)
            {
                // Stop the Compass Cal
                Task.Run(() => StopCompassCal());

                // Set flag that calibration is not running because aborted
                _IsCalibrationRunning = false;

                // Set test status
                _testResult_CompassCalComplete = TestResultStatus.FAIL;
                this.NotifyOfPropertyChange(() => this.TestResult_CompassCalComplete);
            }
            // No compass cal running start the process
            else
            {
                //_workerCompassCal.RunWorkerAsync();
                StartCompassCalProcess();
            }
        }

        #endregion

        #region Save Compass Cal Command

        /// <summary>
        /// Send the command for Compass Cal.
        /// </summary>
        private void OnSaveCompassCal()
        {
            // Check if we are corrently connected
            if (!IsCompassCal)
            {
                // Connect to the compass if not connected
                CompassConnect();
            }

            // Save Calibration results
            _adcpConn.AdcpSerialPort.SaveCompassCal();

            CompassDisconnect();
        }

        #endregion

        #region Read Compass Data

        /// <summary>
        /// Read data from the compass. This will send the command
        /// to request data from the compass.  An event will then
        /// be received from the compass codec to display.
        /// </summary>
        private void OnReadCompass()
        {
            // Connect the compass
            CompassConnect();

            // Use this flag to ensure data
            // is only read and displayed when
            // we want it.  When disconnecting
            // it goes back into interval mode,
            // we can get more data.
            IsReadingCompass = true;

            // Because the compass is in interval mode, this command may be duplicated
            // Send command to Read Compass data
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetDataCommand());

            // Get the Parameters
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetParamCommand());

            // Get the Number of points
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalNumPoints));

            // Get the Stable Check
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalStableCheck));

            // Get the Auto Sampling
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling));

            // Get the Declination
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetConfigCommand(PniPrimeCompassBinaryCodec.ID.kDeclination));

            // Stop reading compass data
            IsReadingCompass = false;

            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Disconnect from compass
            CompassDisconnect();
        }

        #endregion

        #region Compass Tap 0 Command

        /// <summary>
        /// Put the ADCP into compass mode.  Then send the command for
        /// the compass to have 0 taps.  Then disconnect from compass mode.
        /// </summary>
        private void OnCompassTap0()
        {
            // Connect the compass
            CompassConnect();

            // Use this flag to ensure data
            // is only read and displayed when
            // we want it.  When disconnecting
            // it goes back into interval mode,
            // we can get more data.
            IsReadingCompass = true;

            // Get the X and P Axis command
            byte[] xAxis = null;
            byte[] pAxis = null;
            PniPrimeCompassBinaryCodec.SetTaps0Commands(out xAxis, out pAxis);
            _adcpConn.AdcpSerialPort.SendCompassCommand(xAxis);
            _adcpConn.AdcpSerialPort.SendCompassCommand(pAxis);

            // Stop reading compass data
            IsReadingCompass = false;

            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Disconnect from compass
            CompassDisconnect();
        }

        #endregion

        #region Compass Tap 4 Command

        /// <summary>
        /// Put the ADCP into compass mode.  Then send the command for
        /// the compass to have 4 taps.  This will send the 4 tap values.
        /// Then disconnect from compass mode.
        /// </summary>
        private void OnCompassTap4()
        {
            // Connect the compass
            CompassConnect();

            // Use this flag to ensure data
            // is only read and displayed when
            // we want it.  When disconnecting
            // it goes back into interval mode,
            // we can get more data.
            IsReadingCompass = true;

            // Get the X and P Axis command
            byte[] xAxis = null;
            byte[] pAxis = null;
            PniPrimeCompassBinaryCodec.SetTaps4Commands(out xAxis, out pAxis);
            _adcpConn.AdcpSerialPort.SendCompassCommand(xAxis);
            _adcpConn.AdcpSerialPort.SendCompassCommand(pAxis);

            // Stop reading compass data
            IsReadingCompass = false;

            Thread.Sleep(AdcpSerialPort.WAIT_STATE);

            // Disconnect from compass
            CompassDisconnect();
        }

        #endregion

        #region Set Compass Cal Mag Default

        /// <summary>
        /// Set the Compass Calibration Accelerator default settings.
        /// This will use the factory calibration.
        /// </summary>
        private void OnSetDefaultCompassCalMag()
        {
            // Connect the compass
            CompassConnect();

            // Send command to Read Compass data
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetDefaultCompassCalMagCommand());

            // Now wait for the event that the command is complete
            // Then save the compass cal settings using kSave
            // When kSaveDone is received, disconnect from the Compass back to ADCP mode
        }

        #endregion

        #region Set Compass Cal Accel Default

        /// <summary>
        /// Set the Compass Calibration Accelerator default settings.
        /// This will use the factory calibration.
        /// </summary>
        private void OnSetDefaultCompassCalAccel()
        {
            // Connect the compass
            CompassConnect();

            // Send command to Read Compass data
            _adcpConn.AdcpSerialPort.SendCompassCommand(PniPrimeCompassBinaryCodec.GetDefaultCompassCalAccelCommand());

            // Now wait for the event that the command is complete
            // Then save the compass cal settings using kSave
            // When kSaveDone is received, disconnect from the Compass back to ADCP mode
        }

        #endregion

        #region Take compass cal sample

        /// <summary>
        /// Take a compass cal sample.
        /// </summary>
        private void OnTakeCompassCalSample()
        {
            if (IsCompassCal)
            {
                // Take a sample
                NextStep(CalSteps.CalPoint);
            }
            else if (_IsGettingPrePoints)
            {
                NextStep(CalSteps.PrePoint);
            }
            else if (_IsGettingPostPoints)
            {
                NextStep(CalSteps.PostPoint);
            }
            else
            {
                SetStatusBar(new StatusEvent("Compass Cal has not started.  Start the compass cal first.", MessageBoxImage.Error));

            }
        }

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Pass data from the ADCP serial port to the
        /// RecorderManager to be parsed.
        /// </summary>
        /// <param name="data">Data received from the ADCP serial port in ADCP mode.</param>
        private void On_ReceiveAdcpSerialDataEvent(byte[] data)
        {

        }

        /// <summary>
        /// Pass data from the ADCP serial port to the 
        /// compass codec to be parsed.
        /// </summary>
        /// <param name="data">Data received from the ADCP serial port in Compass mode.</param>
        private void On_ReceiveCompassSerialDataEvent(byte[] data)
        {
            Task.Run(() => _compassCodec.AddIncomingData(data));
        }

        /// <summary>
        /// When data is received from the ADCP serial port, update the buffer display.
        /// </summary>
        /// <param name="data">Not Used.</param>
        public void On_AdcpReceiveSerialData(string data)
        {

        }

        /// <summary>
        /// Send a list of commands to the ADCP serial port.  This will
        /// take the list of commands and pass it to the serial port.  The
        /// commands are received from an event from the event aggregator.
        /// </summary>
        /// <param name="commands">List of commands to send.</param>
        private void On_AdcpSerialCommandsEvent(List<string> commands)
        {
            // Make sure we are not in compass mode
            if (!IsCompassCal && !IsCompassConnected)
            {
                // Send a list of commands
                Task.Run(() => _adcpConn.AdcpSerialPort.SendCommands(commands));
            }
        }

        /// <summary>
        /// When the serial port receives a new cal sample
        /// from the Compass calibration, set the new sample value.
        /// </summary>
        /// <param name="data">Data from the compass.</param>
        public void CompassCodecEventHandler(RTI.PniPrimeCompassBinaryCodec.CompassEventArgs data)
        {
            // Sample received
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kUserCalSampCount)
            {
                CalSamples = (UInt32)data.Value;
                CalPosition = RTI.PniPrimeCompassBinaryCodec.MagCalibrationPosition((UInt32)data.Value);
                DiagDisplay = "kUserCalSampCount " + CalSamples + "\n" + _DiagDisplay;
            }

            // Calibration Score received
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kUserCalScore)
            {
                DiagDisplay = "kUserCalScore\n" + _DiagDisplay;
                CalScore_StdDevErr = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).stdDevErr;
                CalScore_xCoverage = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).xCoverage;
                CalScore_yCoverage = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).yCoverage;
                CalScore_zCoverage = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).zCoverage;
                CalScore_xAccelCoverage = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).xAccelCoverage;
                CalScore_yAccelCoverage = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).yAccelCoverage;
                CalScore_zAccelCoverage = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).zAccelCoverage;
                CalScore_accelStdDevErr = ((RTI.PniPrimeCompassBinaryCodec.PniCalScore)data.Value).accelStdDevErr;

                // Set test status
                _testResult_CompassCalGood = TestResultStatus.IN_PROGRESS;
                this.NotifyOfPropertyChange(() => this.TestResult_CompassCalGood);

                // Validate if the score is good
                _isGoodCal = ValidateCalScore();

                if (_isGoodCal)
                {
                    // Set test status
                    _testResult_CompassCalSaved = TestResultStatus.IN_PROGRESS;
                    this.NotifyOfPropertyChange(() => this.TestResult_CompassCalSaved);

                    // Save the compass cal automatically after getting a score
                    _adcpConn.AdcpSerialPort.SaveCompassCal();
                    DiagDisplay = "Good Cal\n" + _DiagDisplay;
                    DiagDisplay = "Save Compass Cal\n" + _DiagDisplay;

                    // Set test status
                    _testResult_CompassCalGood = TestResultStatus.PASS;
                    this.NotifyOfPropertyChange(() => this.TestResult_CompassCalGood);
                }
                else
                {
                    // Set test status
                    _testResult_CompassCalSaved = TestResultStatus.FAIL;
                    this.NotifyOfPropertyChange(() => this.TestResult_CompassCalSaved);

                    // Set test status
                    _testResult_CompassCalGood = TestResultStatus.FAIL;
                    this.NotifyOfPropertyChange(() => this.TestResult_CompassCalGood);

                    DiagDisplay = "Bad Cal\n" + _DiagDisplay;
                }

                //
                // Wait for kSaveDone event to be received to move on
                //
            }

            // Data Response Received
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kDataResp)
            {
                DiagDisplay = "kDataResp\n" + _DiagDisplay;
                if (_IsReadingCompass)
                {
                    _compassDataResponse = (RTI.PniPrimeCompassBinaryCodec.PniDataResponse)data.Value;

                    // Notify a property change for the Compass Data Response
                    SetCompassDataResponse();
                    DiagDisplay = "kDataResp " + _compassDataResponse.ToString() + "\n" + _DiagDisplay;
                }
            }

            // Default Compass Cal Mag complete
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kFactoryUserCalDone)
            {
                DiagDisplay = "kFactoryUserCalDone\n" + _DiagDisplay;
                // Send command to save the Compass cal
                _adcpConn.AdcpSerialPort.SaveCompassCal();
                DiagDisplay = "Save Compass Cal\n" + _DiagDisplay;
            }

            // Default Compass Cal Accel complete
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kFactoryInclCalDone)
            {
                DiagDisplay = "kFactoryInclCalDone\n" + _DiagDisplay;
                // Send command to save the Compass cal
                _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.SaveCompassCalCommand());
            }

            // Save complete
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kSaveDone)
            {
                DiagDisplay = "kSaveDone\n" + _DiagDisplay;
                // Disconnect from compass
                //CompassDisconnect();

                SetStatusBar(new StatusEvent("Compass Calibration Value saved."));

                // Set test status
                _testResult_CompassCalSaved = TestResultStatus.PASS;
                this.NotifyOfPropertyChange(() => this.TestResult_CompassCalSaved);

                // Turn off compass cal
                // BY CALLING STOP HERE, THE START/STOP BUTTON WILL DISPLAY START EVEN
                // THOUGH THE POST POINTS STILL NEED TO BE COLLECTED
                StopCompassCal();
                DiagDisplay = "Stop Compass Cal\n" + _DiagDisplay;

                // Get Post Points
                NextStep(CalSteps.StartPostPoint);
                DiagDisplay = "Start Post Point\n" + _DiagDisplay;
            }

            // Compass Cal Number of Samples
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kUserCalNumPoints)
            {
                DiagDisplay = "kUserCalNumPoints\n" + _DiagDisplay;
                _compassCalNumSamples = (UInt32)data.Value;
                this.NotifyOfPropertyChange(() => this.CompassCalNumSamples);
                DiagDisplay = "kUserCalNumPoints " + _compassCalNumSamples + "\n" + _DiagDisplay;
            }

            // Compass Cal Declination
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kDeclination)
            {
                DiagDisplay = "kDeclination\n" + _DiagDisplay;
                _compassCalDeclination = (float)data.Value;
                this.NotifyOfPropertyChange(() => this.CompassCalDeclination);
                DiagDisplay = "kDeclination " + _compassCalDeclination + "\n" + _DiagDisplay;
            }

            // Compass Cal Stable Check
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kUserCalStableCheck)
            {
                DiagDisplay = "kUserCalStableCheck\n" + _DiagDisplay;
                _isCompassCalStableCheck = (bool)data.Value;
                this.NotifyOfPropertyChange(() => this.IsCompassCalStableCheck);
                DiagDisplay = "kUserCalStableCheck " + _isCompassCalStableCheck + "\n" + _DiagDisplay;
            }

            // Compass Cal Auto Sampling
            if (data.EventType == RTI.PniPrimeCompassBinaryCodec.ID.kUserCalAutoSampling)
            {
                DiagDisplay = "kUserCalAutoSampling\n" + _DiagDisplay;
                _isCompassCalAutoSample = (bool)data.Value;
                this.NotifyOfPropertyChange(() => this.IsCompassCalAutoSample);
                DiagDisplay = "kUserCalAutoSampling " + _isCompassCalAutoSample + "\n" + _DiagDisplay;
            }
        }

        /// <summary>
        /// Filter if we can send the commands to the serial port.
        /// </summary>
        /// <param name="commands">Commands to send to the serial port.</param>
        /// <returns>True = Commands will be send to the serial port.</returns>
        private bool CanReceiveAdcpSerialCommandsEvent(List<string> commands)
        {
            return true;
        }

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Not implemented.  Used with validation.
        /// </summary>
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Used to Validate entries in the textboxes.
        /// This will determine if the value is good
        /// or return an error messate
        /// </summary>
        /// <param name="columnName">Column to validate.</param>
        /// <returns>Empty string means no error.  Any other string is an error with an error message</returns>
        public string this[string columnName]
        {
            get
            {
                string result = null;

                if (columnName == "CompassCalDeclination")
                {
                    if (CompassCalDeclination < 180.0 || CompassCalDeclination > 180.0)
                    {
                        result = "Value must between -180 and 180";
                    }
                }

                // Return the result
                // An empty string means no error
                return result;
            }
        }

        #endregion // IDataErrorInfo Members

        #region Automata

        private enum CalSteps
        {
            /// <summary>
            /// Start collecting the pre points.
            /// </summary>
            StartPrePoint,

            /// <summary>
            /// Collect a Pre Point.
            /// This will read the compass for a Heading, Pitch and Roll.
            /// </summary>
            PrePoint,

            /// <summary>
            /// Stop getting the pre points and move to the next step.
            /// </summary>
            StopPrePoint,

            /// <summary>
            /// Start collecting the post points.
            /// </summary>
            StartPostPoint,

            /// <summary>
            /// Collect a Post Point.
            /// This will read the compass for a Heading, Pitch and Roll.
            /// </summary>
            PostPoint,

            /// <summary>
            /// Stop getting the post points and move to the next step.
            /// </summary>
            StopPostPoint,

            /// <summary>
            /// This will tell the compass to take a calibration sample.
            /// </summary>
            CalPoint,

        }

        /// <summary>
        /// Handle all the steps in the calibration process.
        /// Give the step type and this will handle the process.
        /// </summary>
        /// <param name="step">Step to process.</param>
        private void NextStep(CalSteps step)
        {
            switch (step)
            {
                case CalSteps.StartPrePoint:
                    DiagDisplay = "StartPrePoint\n" + _DiagDisplay;
                    _pointCount = 0;                                                                        // Initialize the point count
                    InitPrePostPoints();                                                                    // Initialize the pre and post points
                    IsGettingPrePoints = true;                                                              // Set flag to start getting pre points
                    Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);                                        // Wait for button to be updated, the next step will reset _isGettingPrePoints
                    NextStep(CalSteps.PrePoint);                                                            // Get the initial point
                    break;
                case CalSteps.PrePoint:
                    DiagDisplay = "PrePoint\n" + _DiagDisplay;
                    GetPoints(true, _pointCount);                                                           // Get a pre point
                    break;
                case CalSteps.StopPrePoint:
                    DiagDisplay = "StopPrePoint\n" + _DiagDisplay;
                    IsGettingPrePoints = false;                                                             // Set the flag that we have completed getting the pre points
                    Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);                                        // Wait for button to be updated, the next step will reset _isGettingPostPoints
                    RunCompassCal();                                                                        // Start Compass Cal
                    DiagDisplay = "Start Compass Cal\n" + _DiagDisplay;
                    break;
                case CalSteps.StartPostPoint:
                    DiagDisplay = "StartPostPoint\n" + _DiagDisplay;
                    _pointCount = 0;                                                                        // Initialize the point count
                    IsGettingPostPoints = true;                                                             // Set flag to start getting post points
                    Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE * 2);                                        // Wait for button to be updated, the next step will reset _isGettingPostPoints
                    NextStep(CalSteps.PostPoint);                                                           // Get the initial point
                    break;
                case CalSteps.PostPoint:
                    DiagDisplay = "PostPoint\n" + _DiagDisplay;
                    GetPoints(false, _pointCount);                                                          // Get a post point
                    break;
                case CalSteps.StopPostPoint:
                    DiagDisplay = "StopPostPoint\n" + _DiagDisplay;
                    CheckForBadPoints();                                                                    // Check bad values
                    CalculateDiff();                                                                        // Calculate Diff
                    SendCompleteEvent();                                                                    // Send the complete event
                    CompleteCompassCal();                                                                   // Complete the compass cal
                    DiagDisplay = "Compass Cal Complete\n" + _DiagDisplay;
                    break;
                case CalSteps.CalPoint:
                    DiagDisplay = "CalPoint\n" + _DiagDisplay;
                    _adcpConn.AdcpSerialPort.SendCompassCommand(RTI.PniPrimeCompassBinaryCodec.GetTakeUserCalSampleCommand());   // Get a calibration sample
                    break;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        /// <param name="ccEvent">Compass Cal result.</param>
        public delegate void CompassCalCompleteEventHandler(CompassCalResultEvent ccEvent);

        /// <summary>
        /// Subscribe to receive event when data has been successfully
        /// processed.  This can be used to tell if data is in this format
        /// and is being processed or is not in this format.
        /// Subscribe to this event.  This will hold all subscribers.
        /// 
        /// To subscribe:
        /// compassCalVM.CompassCalCompleteEvent += new adcpBinaryCodec.CompassCalCompleteEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// compassCalVM.CompassCalCompleteEvent -= (method to call)
        /// </summary>
        public event CompassCalCompleteEventHandler CompassCalCompleteEvent;

        #endregion
    }
}
