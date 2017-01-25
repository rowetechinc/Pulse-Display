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
 * 06/21/2012      RC          2.12       Initial coding  
 * 06/25/2012      RC          2.12       Remove recording to a project only.
 * 06/26/2012      RC          2.12       Display average amplitude and correlation plot.
 *                                         Clear the correlation, amplitude and good ping plot when ClearPlot() called.
 *                                         Send AdcpSerialCommandsEvent to configure the ADCP for a lake test.
 *                                         Added a Start and Stop Ping button.
 * 06/27/2012      RC          2.12       Display the beam colors.
 * 08/21/2012      RC          2.13       Record test results to a file in CSV format.
 *                                         Display test orientation.
 *                                         Get shutdown event to know when shutting down.  Close all files when shutting down.
 *                                         Turn on IsCalculateDmg when starting the test.
 * 08/24/2012      RC          2.13       Added Lat/Lon so the value can be recorded seperately.  
 *                                         Changed the name of the results files to ValidationTestResults.txt.
 * 08/28/2012      RC          2.13       In AccumulateCorrelationData(), changed the loop to check the length from the parameter to using the array _accumCurr to prevent a miss match in length.
 * 08/29/2012      RC          2.14       In AccumulateCorrelationData() and AccumulateAmplitudeData(), fixed a bug when playing realtime and playback data with different settings.
 * 09/07/2012      RC          2.15       Calculate the percentage of good ensembles.  
 *                                         Do not pass any ensembles that are not good to the DMG calculator.
 * 10/01/2012      RC          2.15       Set TestOrientationColor to a default color so at startup it is not null.
 * 12/11/2012      RC          2.17       Output Direction Error to test results file.
 * 12/18/2012      RC          2.17       Added IsAdmin to determine if the user is an admin and can use additional buttons.
 *                                         Added a fresh and salt water configure button.
 *                                         Added Tank test and Ringing test configure buttons.
 * 01/24/2013      RC          2.18       Count the bad status.
 * 05/01/2013      RC          2.19       Ignore Single beam ensembles.
 * 07/03/2013      RC          3.0.2      Updated to use ReactiveUI and Caliburn.Micro.
 * 12/11/2013      RC          3.2.0      Removed the background worker and replaced it with a ReactiveAsyncCommand for DisplayData.
 * 01/16/2014      RC          3.2.3      Changed the default colors to strings so need to parse them back to OxyColor.
 * 02/13/2014      RC          3.2.3      Allow DVL data to be recorded.
 * 04/15/2014      RC          3.2.4      Allow display of Single beam ADCPs.
 * 05/14/2014      RC          3.2.4      Removed Small Plots and the UpdatePlot event.
 * 05/15/2014      RC          3.2.4      Update the plots after the series are updated.
 * 05/23/2014      RC          3.2.4      Added syncrhonization to the updating of the plots.  Optimized the plot updates.
 * 05/28/2014      RC          3.2.4      Created global line series for each plot and update the points in the line series only.
 * 06/12/2014      RC          3.3.1      Added Profile Range and Signal to Noise results.
 * 06/27/2014      RC          3.4.0      Check for new sources of data from averaging.
 * 07/03/2014      RC          3.4.0      Moved Dmg Plot to DmgPlotViewModel.
 * 07/11/2014      RC          3.4.0      Added ActualPingCount, FirstPingTime and LastPingTime.
 * 08/04/2014      RC          3.4.0      Fixed bug with refreshing the plot.
 * 08/06/2014      RC          4.0.0      Added missing properties.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/20/2014      RC          4.0.1      Added CloseVMCommand.
 * 08/21/2014      RC          4.0.1      Await on Clear() in DisplayBulkDataExecute().
 * 05/19/2016      RC          4.4.3      Added GPS Heading.
 * 
 */

using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.ComponentModel;
using OxyPlot;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Text;
using log4net;
using System.Windows.Media;
using OxyPlot.Series;
using OxyPlot.Axes;
using Caliburn.Micro;
using ReactiveUI;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Plots and data needed to do a lake test.
    /// </summary>
    public class ValidationTestViewModel : PulseViewModel, IDisposable, IHandle<EnsembleEvent>, IHandle<ProjectEvent>, IHandle<BulkEnsembleEvent>
    {

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test file name.
        /// </summary>
        private const string VALIDATION_TEST_FILE_NAME = @"\ValidationTestResults.csv";

        /// <summary>
        /// Maximum number of points to display on the small plots.
        /// </summary>
        private const int MAX_GRAPH_POINTS = 100;

        /// <summary>
        /// Color of the line for plots.
        /// </summary>
        private OxyColor LINE_COLOR = OxyColors.Orange;

        /// <summary>
        /// Default color for the test orientation.
        /// This color was found in Styles/Defaults.xmal PulseBackBorder2Color.
        /// </summary>
        private string DEFAULT_TEST_ORIENTATION_COLOR = "#FF141414";

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        /// <summary>
        /// List of latest Bottom Track
        /// Ranges.
        /// </summary>
        private List<float[]> _bottomTrackRangeList;

        /// <summary>
        /// List of latest Bottom Track velocities.
        /// </summary>
        private List<double> _bottomTrackVelocityList;

        /// <summary>
        /// List of latest Bottom Track
        /// Velocity series.
        /// </summary>
        private List<float[]> _bottomTrackVelSeriesList;

        /// <summary>
        /// Event Aggregator to receive the latest ensembles.
        /// </summary>
        private IEventAggregator _eventAggregator;

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<DataSet.Ensemble> _buffer;

        /// <summary>
        /// Flag to know if processing the buffer.
        /// </summary>
        private bool _isProcessingBuffer;

        /// <summary>
        /// Is the display active.  Active
        /// means that it is currently 
        /// displayed.
        /// </summary>
        private bool _isActive;

        #region Line Series

        #region Bottom Track Range

        /// <summary>
        /// Bottom Track Range Line Series Beam 0.
        /// </summary>
        private LineSeries _btRangeLsBeam0;

        /// <summary>
        /// Bottom Track Range Line Series Beam 1.
        /// </summary>
        private LineSeries _btRangeLsBeam1;

        /// <summary>
        /// Bottom Track Range Line Series Beam 2.
        /// </summary>
        private LineSeries _btRangeLsBeam2;

        /// <summary>
        /// Bottom Track Range Line Series Beam 3.
        /// </summary>
        private LineSeries _btRangeLsBeam3;

        #endregion

        #region Bottom Track Speed

        /// <summary>
        /// Bottom Track Speed Line Series Beam 0.
        /// </summary>
        private LineSeries _btSpeedLs;

        #endregion

        #region Bottom Track Beam Velocity

        /// <summary>
        /// Bottom Track Beam Velocity Line Series Beam 0.
        /// </summary>
        private LineSeries _btBeamVelLsBeam0;

        /// <summary>
        /// Bottom Track Beam Velocity Line Series Beam 1.
        /// </summary>
        private LineSeries _btBeamVelLsBeam1;

        /// <summary>
        /// Bottom Track Beam Velocity Line Series Beam 2.
        /// </summary>
        private LineSeries _btBeamVelLsBeam2;

        /// <summary>
        /// Bottom Track Beam Velocity Line Series Beam 3.
        /// </summary>
        private LineSeries _btBeamVelLsBeam3;

        #endregion

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Options to store for averaging the data.
        /// </summary>
        private ValidationTestViewOptions _Options;

        #region Configuration

        /// <summary>
        /// Subsystem Configuration for this view.
        /// </summary>
        private SubsystemDataConfig _Config;
        /// <summary>
        /// Subsystem Configuration for this view.
        /// </summary>
        public SubsystemDataConfig Config
        {
            get { return _Config; }
            set
            {
                _Config = value;
                this.NotifyOfPropertyChange(() => this.Config);
            }
        }

        #endregion

        #region Display

        /// <summary>
        /// Display the configuration CEPO index to indentify the 
        /// configuration.
        /// </summary>
        public string Display { get { return _Config.IndexCodeString(); } }

        /// <summary>
        /// Display the configuration desc to indentify the 
        /// configuration.
        /// </summary>
        public string Desc { get { return _Config.DescString(); } }

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

        #region Ensembles

        /// <summary>
        /// Maximum number of ensembles to display.
        /// </summary>
        public int MaxEnsembles
        {
            get { return _Options.MaxEnsembles; }
            set
            {
                _Options.MaxEnsembles = value;
                this.NotifyOfPropertyChange(() => this.MaxEnsembles);

                SaveOptions();
            }
        }

        /// <summary>
        /// Total number of ensembles received for processing.
        /// This value is used to calculate the percentage of good
        /// ensembles processed.
        /// </summary>
        private long _ensembleCount;
        /// <summary>
        /// Total number of ensembles received for processing.
        /// This value is used to calculate the percentage of good
        /// ensembles processed.
        /// </summary>
        public long EnsembleCount
        {
            get { return _ensembleCount; }
            set
            {
                _ensembleCount = value;
                this.NotifyOfPropertyChange(() => this.EnsembleCount);
            }
        }

        #region Good Ensemble Count

        /// <summary>
        /// Number of good ensembles counted.  This value is used to calculate the
        /// percentage of good ensembles processed.
        /// </summary>
        private long _goodEnsembleCount;
        /// <summary>
        /// Number of good ensembles counted.  This value is used to calculate the
        /// percentage of good ensembles processed.
        /// </summary>
        public long GoodEnsembleCount
        {
            get { return _goodEnsembleCount; }
            set
            {
                _goodEnsembleCount = value;
                this.NotifyOfPropertyChange(() => this.GoodEnsembleCount);
            }
        }

        /// <summary>
        /// Percentage of good ensembles.  This will get the total number
        /// of good ensembles against the total ensemble that were received for
        /// processing.
        /// </summary>
        private string _goodEnsemblePercentage;
        /// <summary>
        /// Percentage of good ensembles.  This will get the total number
        /// of good ensembles against the total ensemble that were received for
        /// processing.
        /// </summary>
        public string GoodEnsemblePercentage
        {
            get { return _goodEnsemblePercentage; }
            set
            {
                _goodEnsemblePercentage = value;
                this.NotifyOfPropertyChange(() => this.GoodEnsemblePercentage);
            }
        }

        #endregion

        #region Bad Status Count

        /// <summary>
        /// Number of bad status ensembles counted.  This value is used to calculate the
        /// percentage of bad status ensembles processed.
        /// </summary>
        private long _badStatusCount;
        /// <summary>
        /// Number of bad status ensembles counted.  This value is used to calculate the
        /// percentage of bad status ensembles processed.
        /// </summary>
        public long BadStatusCount
        {
            get { return _badStatusCount; }
            set
            {
                _badStatusCount = value;
                this.NotifyOfPropertyChange(() => this.BadStatusCount);
                this.NotifyOfPropertyChange(() => this.IsBadStatusCount);
            }
        }
        /// <summary>
        /// Flag if the number of bad status ensembles counted is incremented.
        /// </summary>
        public bool IsBadStatusCount
        {
            get { return _badStatusCount > 0; }
        }

        /// <summary>
        /// Percentage of bad status.  This will get the total number
        /// of good ensembles against the total ensemble with a status 
        /// that was not good.
        /// </summary>
        private string _badStatusPercentage;
        /// <summary>
        /// Percentage of bad status.  This will get the total number
        /// of good ensembles against the total ensemble with a status 
        /// that was not good.
        /// </summary>
        public string BadStatusPercentage
        {
            get { return _badStatusPercentage; }
            set
            {
                _badStatusPercentage = value;
                this.NotifyOfPropertyChange(() => this.BadStatusPercentage);
            }
        }

        #endregion

        #region Plots

        /// <summary>
        /// Flag wheter to filter the data for bad data before plotting.
        /// </summary>
        public bool IsFilteringData
        {
            get { return _Options.IsFilteringData; }
            set
            {
                _Options.IsFilteringData = value;
                this.NotifyOfPropertyChange(() => this.IsFilteringData);

                CorrPlot.IsFilterData = value;
                AmpPlot.IsFilterData = value;

                SaveOptions();
            }
        }

        /// <summary>
        /// Correlation plot.
        /// </summary>
        public ProfilePlotViewModel CorrPlot { get; set; }

        /// <summary>
        /// Amplitude plot.
        /// </summary>
        public ProfilePlotViewModel AmpPlot { get; set; }

        /// <summary>
        /// Bottom Track Range plot model.
        /// </summary>
        private PlotModel _bottomTrackRangePlot;
        /// <summary>
        /// Bottom Track Range plot model property.
        /// </summary>
        public PlotModel BottomTrackRangePlot
        {
            get
            {
                return _bottomTrackRangePlot;
            }
            private set
            {
                _bottomTrackRangePlot = value;
                this.NotifyOfPropertyChange(() => this.BottomTrackRangePlot);
            }
        }

        /// <summary>
        /// Bottom Track Speed plot model.
        /// </summary>
        private PlotModel _bottomTrackSpeedPlot;
        /// <summary>
        /// Bottom Track Speed plot model property.
        /// </summary>
        public PlotModel BottomTrackSpeedPlot
        {
            get
            {
                return _bottomTrackSpeedPlot;
            }
            private set
            {
                _bottomTrackSpeedPlot = value;
                this.NotifyOfPropertyChange(() => this.BottomTrackSpeedPlot);
            }
        }

        /// <summary>
        /// Bottom Track Velocity Series plot model.
        /// </summary>
        private PlotModel _bottomTrackVelSeriesPLot;
        /// <summary>
        /// Bottom Track Velocity Series plot model.
        /// </summary>
        public PlotModel BottomTrackVelSeriesPlot
        {
            get { return _bottomTrackVelSeriesPLot; }
            set
            {
                _bottomTrackVelSeriesPLot = value;
                this.NotifyOfPropertyChange(() => this.BottomTrackVelSeriesPlot);
            }
        }

        ///// <summary>
        ///// Set flag if should use Earth or Beam for Good Ping data.
        ///// </summary>
        //private bool _isGoodPingEarth;
        ///// <summary>
        ///// Set flag if should use Earth or Beam for Good Ping data.
        ///// </summary>
        //public bool IsGoodPingEarth
        //{
        //    get { return _isGoodPingEarth; }
        //    set
        //    {
        //        _isGoodPingEarth = value;
        //        this.NotifyOfPropertyChange(() => this.IsGoodPingEarth);
        //    }
        //}

        #endregion

        #region DMG

        /// <summary>
        /// Calculate Distance Made Good data.
        /// </summary>
        public bool IsCalculateDmg
        {
            get { return _Options.IsCalculateDmg; }
            set
            {
                _Options.IsCalculateDmg = value;
                this.NotifyOfPropertyChange(() => this.IsCalculateDmg);

                if (IsCalculateDmg)
                {
                    ReportText.Clear();
                }

                SaveOptions();
            }
        }


        /// <summary>
        /// Flag True when testing is in progress.
        /// This start to calculate the DMG.
        /// </summary>
        private bool _isTesting;
        /// <summary>
        /// Flag True when testing is in progress.
        /// This start to calculate the DMG.
        /// </summary>
        public bool IsTesting
        {
            get { return _isTesting; }
            set
            {
                _isTesting = value;
                this.NotifyOfPropertyChange(() => this.IsTesting);

                if (_isTesting)
                {
                    // Clear the plots
                    Clear();
                }
            }
        }

        ///// <summary>
        ///// Set flag if we should also create a project
        ///// when testing starts.  If true, this will create
        ///// a project with the file name as the project name.
        ///// </summary>
        //private bool _isCreatingProject;
        ///// <summary>
        ///// Set flag if we should also create a project
        ///// when testing starts.  If true, this will create
        ///// a project with the file name as the project name.
        ///// </summary>
        //public bool IsCreatingProject
        //{
        //    get { return _isCreatingProject; }
        //    set
        //    {
        //        _isCreatingProject = value;
        //        this.NotifyOfPropertyChange(() => this.IsCreatingProject);
        //    }
        //}

        /// <summary>
        /// Text from the calculation of
        /// Distance made good and Course made good.
        /// </summary>
        private ProjectReportText _reportText;
        /// <summary>
        /// Text from the calculation of
        /// Distance made good and Course made good.
        /// </summary>
        public ProjectReportText ReportText
        {
            get { return _reportText; }
            set
            {
                _reportText = value;
                this.NotifyOfPropertyChange(() => this.ReportText);
            }
        }

        #endregion

        #region Colors

        /// <summary>
        /// List of all the color options.
        /// </summary>
        private List<OxyColor> _beamColorList;
        /// <summary>
        /// List of all the color options.
        /// </summary>
        public List<OxyColor> BeamColorList
        {
            get { return _beamColorList; }
            set
            {
                _beamColorList = value;
                this.NotifyOfPropertyChange(() => this.BeamColorList);
            }
        }

        /// <summary>
        /// Color for Beam 0 plots property.
        /// </summary>
        private OxyColor _beam0Color;
        /// <summary>
        /// Color for Beam 0 plots property.
        /// </summary>
        public OxyColor Beam0Color
        {
            get { return _beam0Color; }
            set
            {
                _beam0Color = value;
                this.NotifyOfPropertyChange(() => this.Beam0Color);
                this.NotifyOfPropertyChange(() => this.Beam0ColorStr);
            }
        }

        /// <summary>
        /// String for the Beam 0 Color to display as a background color.
        /// </summary>
        public string Beam0ColorStr
        {
            get { return "#" + BeamColor.ColorValue(_beam0Color); }
        }

        /// <summary>
        /// Color for Beam 1 plots property.
        /// </summary>
        private OxyColor _beam1Color;
        /// <summary>
        /// Color for Beam 1 plots property.
        /// </summary>
        public OxyColor Beam1Color
        {
            get { return _beam1Color; }
            set
            {
                _beam1Color = value;
                this.NotifyOfPropertyChange(() => this.Beam1Color);
                this.NotifyOfPropertyChange(() => this.Beam1ColorStr);
            }
        }

        /// <summary>
        /// String for the Beam 1 Color to display as a background color.
        /// </summary>
        public string Beam1ColorStr
        {
            get { return "#" + BeamColor.ColorValue(_beam1Color); }
        }

        /// <summary>
        /// Color for Beam 2 plots property.
        /// </summary>
        private OxyColor _beam2Color;
        /// <summary>
        /// Color for Beam 2 plots property.
        /// </summary>
        public OxyColor Beam2Color
        {
            get { return _beam2Color; }
            set
            {
                _beam2Color = value;
                this.NotifyOfPropertyChange(() => this.Beam2Color);
                this.NotifyOfPropertyChange(() => this.Beam2ColorStr);
            }
        }

        /// <summary>
        /// String for the Beam 2 Color to display as a background color.
        /// </summary>
        public string Beam2ColorStr
        {
            get { return "#" + BeamColor.ColorValue(_beam2Color); }
        }

        /// <summary>
        /// Color for Beam 4 plots property.
        /// </summary>
        private OxyColor _beam3Color;
        /// <summary>
        /// Color for Beam 3 plots property.
        /// </summary>
        public OxyColor Beam3Color
        {
            get { return _beam3Color; }
            set
            {
                _beam3Color = value;
                this.NotifyOfPropertyChange(() => this.Beam3Color);
                this.NotifyOfPropertyChange(() => this.Beam3ColorStr);
            }
        }

        /// <summary>
        /// String for the Beam 3 Color to display as a background color.
        /// </summary>
        public string Beam3ColorStr
        {
            get { return "#" + BeamColor.ColorValue(_beam3Color); }
        }

        #endregion

        #region Text Output

        /// <summary>
        /// Serial number of the system.
        /// </summary>
        private string _sysSerialNumber;
        /// <summary>
        /// Serial number of the system.
        /// </summary>
        public string SysSerialNumber
        {
            get { return _sysSerialNumber; }
            set
            {
                _sysSerialNumber = value;
                this.NotifyOfPropertyChange(() => this.SysSerialNumber);
            }
        }

        /// <summary>
        /// Latitude and Longitude of current ensemble.
        /// </summary>
        private string _latLon;
        /// <summary>
        /// Latitude and Longitude of current ensemble.
        /// </summary>
        public string LatLon
        {
            get { return _latLon; }
            set
            {
                _latLon = value;
                this.NotifyOfPropertyChange(() => this.LatLon);
            }
        }

        /// <summary>
        /// Last Latitude position.
        /// </summary>
        private DotSpatial.Positioning.Latitude _lat;
        /// <summary>
        /// Last Latitude position.
        /// </summary>
        public DotSpatial.Positioning.Latitude Lat
        {
            get { return _lat; }
            set
            {
                _lat = value;
                this.NotifyOfPropertyChange(() => this.Lat);
            }
        }

        /// <summary>
        /// Last longitude position.
        /// </summary>
        private DotSpatial.Positioning.Longitude _lon;
        /// <summary>
        /// Last longitude position.
        /// </summary>
        public DotSpatial.Positioning.Longitude Lon
        {
            get { return _lon; }
            set
            {
                _lon = value;
                this.NotifyOfPropertyChange(() => this.Lon);
            }
        }

        /// <summary>
        /// GPS heading.
        /// </summary>
        private string _GpsHeading;
        /// <summary>
        /// GPS heading.
        /// </summary>
        public string GpsHeading
        {
            get { return _GpsHeading; }
            set
            {
                _GpsHeading = value;
                this.NotifyOfPropertyChange(() => this.GpsHeading);
            }
        }

        /// <summary>
        /// ADCP heading.
        /// </summary>
        private string _heading;
        /// <summary>
        /// ADCP heading.
        /// </summary>
        public string Heading
        {
            get { return _heading; }
            set
            {
                _heading = value;
                this.NotifyOfPropertyChange(() => this.Heading);
            }
        }



        /// <summary>
        /// ADCP Pitch.
        /// </summary>
        private string _pitch;
        /// <summary>
        /// ADCP Pitch.
        /// </summary>
        public string Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                this.NotifyOfPropertyChange(() => this.Pitch);
            }
        }

        /// <summary>
        /// ADCP Roll.
        /// </summary>
        private string _roll;
        /// <summary>
        /// ADCP Roll.
        /// </summary>
        public string Roll
        {
            get { return _roll; }
            set
            {
                _roll = value;
                this.NotifyOfPropertyChange(() => this.Roll);
            }
        }

        /// <summary>
        /// Status of the system.
        /// </summary>
        private string _status;
        /// <summary>
        /// Status of the system.
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.NotifyOfPropertyChange(() => this.Status);
            }
        }

        /// <summary>
        /// Pressue under water.
        /// </summary>
        private string _pressure;
        /// <summary>
        /// Pressure under water.
        /// </summary>
        public string Pressure
        {
            get { return _pressure; }
            set
            {
                _pressure = value;
                this.NotifyOfPropertyChange(() => this.Pressure);
            }
        }

        /// <summary>
        /// Water Temperature.
        /// </summary>
        private string _waterTemp;
        /// <summary>
        /// Water Temperature.
        /// </summary>
        public string WaterTemp
        {
            get { return _waterTemp; }
            set
            {
                _waterTemp = value;
                this.NotifyOfPropertyChange(() => this.WaterTemp);
            }
        }

        /// <summary>
        /// System temperature.
        /// </summary>
        private string _systemTemp;
        /// <summary>
        /// System temperature.
        /// </summary>
        public string SystemTemp
        {
            get { return _systemTemp; }
            set
            {
                _systemTemp = value;
                this.NotifyOfPropertyChange(() => this.SystemTemp);
            }
        }

        /// <summary>
        /// Bin Size.
        /// </summary>
        private string _BinSize;
        /// <summary>
        /// Bin Size.
        /// </summary>
        public string BinSize
        {
            get { return _BinSize; }
            set
            {
                _BinSize = value;
                this.NotifyOfPropertyChange(() => this.BinSize);
            }
        }

        /// <summary>
        /// Blank.
        /// </summary>
        private string _Blank;
        /// <summary>
        /// Blank.
        /// </summary>
        public string Blank
        {
            get { return _Blank; }
            set
            {
                _Blank = value;
                this.NotifyOfPropertyChange(() => this.Blank);
            }
        }

        /// <summary>
        /// Actual Ping Count.
        /// </summary>
        private string _ActualPingCount;
        /// <summary>
        /// Actual Ping Count.
        /// </summary>
        public string ActualPingCount
        {
            get { return _ActualPingCount; }
            set
            {
                _ActualPingCount = value;
                this.NotifyOfPropertyChange(() => this.ActualPingCount);
            }
        }

        /// <summary>
        /// First Ping Time.
        /// </summary>
        private string _FirstPingTime;
        /// <summary>
        /// First Ping Time.
        /// </summary>
        public string FirstPingTime
        {
            get { return _FirstPingTime; }
            set
            {
                _FirstPingTime = value;
                this.NotifyOfPropertyChange(() => this.FirstPingTime);
            }
        }

        /// <summary>
        /// Last Ping Time.
        /// </summary>
        private string _LastPingTime;
        /// <summary>
        /// Last Ping Time.
        /// </summary>
        public string LastPingTime
        {
            get { return _LastPingTime; }
            set
            {
                _LastPingTime = value;
                this.NotifyOfPropertyChange(() => this.LastPingTime);
            }
        }

        /// <summary>
        /// GPS status.
        /// </summary>
        private string _gpsStatus;
        /// <summary>
        /// GPS status.
        /// </summary>
        public string GpsStatus
        {
            get { return _gpsStatus; }
            set
            {
                _gpsStatus = value;
                this.NotifyOfPropertyChange(() => this.GpsStatus);
            }
        }

        #endregion

        #region Average

        /// <summary>
        /// Should the Correlation and Amplitude plot be displayed as
        /// single ping data or average data.
        /// </summary>
        public bool IsCorrAmpAverage
        {
            get { return _Options.IsCorrAmpAverage; }
            set
            {
                _Options.IsCorrAmpAverage = value;
                this.NotifyOfPropertyChange(() => this.IsCorrAmpAverage);

                AmpPlot.IsAvgSeriesOn = value;
                CorrPlot.IsAvgSeriesOn = value;
                AmpPlot.IsProfileSeriesOn = !value;
                CorrPlot.IsProfileSeriesOn = !value;


                SaveOptions();
            }
        }

        #endregion

        #region Record Test Reslts

        /// <summary>
        /// Flag to turn on and off recording the CSV test results.
        /// </summary>
        private bool _isRecordTestResults;
        /// <summary>
        /// Flag to turn on and off recording the CSV test results.
        /// </summary>
        public bool IsRecordTestResults
        {
            get { return _isRecordTestResults; }
            set
            {
                _isRecordTestResults = value;
                this.NotifyOfPropertyChange(() => this.IsRecordTestResults);
            }
        }

        #endregion

        #region Test orientation

        /// <summary>
        /// Set which beams are being tested.  Beams 0 and 1 or Beams 2 and 3.
        /// This is determined by which beams are around 0 and which beams are inverses
        /// of each other.
        /// Which beam is forward.
        /// </summary>
        private string _testOrientation;
        /// <summary>
        /// Set which beams are being tested.  Beams 0 and 1 or Beams 2 and 3.
        /// This is determined by which beams are around 0 and which beams are inverses
        /// of each other.
        /// Which beam is forward.
        /// </summary>
        public string TestOrientation
        {
            get { return _testOrientation; }
            set
            {
                _testOrientation = value;
                this.NotifyOfPropertyChange(() => this.TestOrientation);
            }
        }

        /// <summary>
        /// Color to represent the beam that is forward in the test.
        /// This is based off the TestOrientation value.
        /// </summary>
        private string _testOrientationColor;
        /// <summary>
        /// Color to represent the beam that is forward in the test.
        /// This is based off the TestOrientation value.
        /// </summary>
        public string TestOrientationColor
        {
            get { return _testOrientationColor; }
            set
            {
                _testOrientationColor = value;
                this.NotifyOfPropertyChange(() => this.TestOrientationColor);
            }
        }

        #endregion

        #region Declination 

        /// <summary>
        /// Declination value.
        /// </summary>
        public float Declination
        {
            get { return _Options.Declination; }
            set
            {
                _Options.Declination = value;
                this.NotifyOfPropertyChange(() => this.Declination);

                _reportText.Declination = _Options.Declination;

                SaveOptions();
            }
        }

        #endregion

        #endregion

        #region Plot

        /// <summary>
        /// Distance Made Good Plot.
        /// </summary>
        private DmgPlotViewModel _DmgPlot;
        /// <summary>
        /// Distance Made Good Plot.
        /// </summary>
        public DmgPlotViewModel DmgPlot
        {
            get { return _DmgPlot; }
            set
            {
                _DmgPlot = value;
                this.NotifyOfPropertyChange(() => this.DmgPlot);
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to clear the plots.
        /// This will clear all the buffers.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearPlotsCommand { get; protected set; }

        /// <summary>
        /// Command to close this VM.
        /// </summary>
        public ReactiveCommand<object> CloseVMCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public ValidationTestViewModel(SubsystemDataConfig config)
            : base("ValidationTest")
        {
            // Initialize values
            Config = config;
            _pm = IoC.Get<PulseManager>();
            _eventAggregator = IoC.Get<IEventAggregator>();
            _isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<DataSet.Ensemble>();

            _beamColorList = BeamColor.GetBeamColorList();

            // Distance Made Good Plot
            DmgPlot = new DmgPlotViewModel();

            IsRecordTestResults = true;
            //_maxEnsembles = DEFAULT_MAX_ENSEMBLES;
            //IsGoodPingEarth = true;
            //IsFilteringData = true;
            _isTesting = false;
            //_isCalculateDmg = true;
            //IsCreatingProject = false;
            //IsCorrAmpAverage = true;
            Beam0Color = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0);
            Beam1Color = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1);
            Beam2Color = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2);
            Beam3Color = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_3);
            TestOrientationColor = DEFAULT_TEST_ORIENTATION_COLOR;
            GoodEnsembleCount = 0;
            EnsembleCount = 0;
            BadStatusCount = 0;
            GoodEnsemblePercentage = 0.ToString("0.00") + "%";
            BadStatusPercentage = 0.ToString("0.00") + "%";

            GetOptionsFromDatabase();

            _reportText = new ProjectReportText();
            if (_Options != null)
            {
                _reportText.Declination = _Options.Declination;
            }

            // Create the plots
            SetupPlots();

            // Create a command to clear plots
            ClearPlotsCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => Clear()));

            // Close the VM
            CloseVMCommand = ReactiveCommand.Create();
            CloseVMCommand.Subscribe(_ => _eventAggregator.PublishOnUIThread(new CloseVmEvent(_Config)));

            // Subscribe to receive raw ensembles and selected projects
            _eventAggregator.Subscribe(this);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            // Unsubscribe
            _eventAggregator.Unsubscribe(this);
        }

        /// <summary>
        /// Clear the object.
        /// </summary>
        public void Clear()
        {
            // Clear the Report text
            ReportText.Clear();

            // Clear plots
            ClearPlots();

            // Clear text
            SysSerialNumber = "";
            LatLon = "";
            Lat = new DotSpatial.Positioning.Latitude();
            Lon = new DotSpatial.Positioning.Longitude();
            GpsHeading = "";
            Heading = "";
            Pitch = "";
            Roll = "";
            Status = "";
            GpsStatus = "";
            WaterTemp = "";
            SystemTemp = "";
            Pressure = "";
            TestOrientation = "";
            BinSize = "";
            Blank = "";
            ActualPingCount = "";
            FirstPingTime = "";
            LastPingTime = "";

            EnsembleCount = 0;
            GoodEnsembleCount = 0;
            GoodEnsemblePercentage = "";
            BadStatusCount = 0;
            BadStatusPercentage = "";

            TestOrientationColor = DEFAULT_TEST_ORIENTATION_COLOR;
        }

        #region Options

        /// <summary>
        /// Get the options for this subsystem display
        /// from the database.  If the options have not
        /// been set to the database yet, default values 
        /// will be used.
        /// </summary>
        private void GetOptionsFromDatabase()
        {
            //if (_Config != null)
            //{
            //    var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);
            //    _Options = _pm.AppConfiguration.GetValidationViewOptions(ssConfig);

            //    // Notify all the properties
            //    NotifyResultsProperties();
            //}
            _Options = _pm.GetValidationViewOptions();

            // Notify all the properties
            NotifyResultsProperties();
        }

        /// <summary>
        /// Save the options to the project.
        /// </summary>
        private void SaveOptions()
        {
            //// SubsystemDataConfig needs to be converted to a SubsystemConfiguration
            //// because the SubsystemConfig will be compared in AppConfiguration to determine
            //// where to save the settings.  Because SubsystemDataConfig and SubsystemConfiguration
            //// are not the same type, it will not pass Equal()
            //var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);

            //_pm.AppConfiguration.SaveValidationViewOptions(ssConfig, _Options);

            _pm.UpdateValidationViewOptions(_Options);
        }

        /// <summary>
        /// Update all the properties.
        /// </summary>
        private void NotifyResultsProperties()
        {
            // Notify all the properties
            this.NotifyOfPropertyChange();

            this.NotifyOfPropertyChange(() => this.MaxEnsembles);
            this.NotifyOfPropertyChange(() => this.EnsembleCount);
            this.NotifyOfPropertyChange(() => this.IsCalculateDmg);
            this.NotifyOfPropertyChange(() => this.IsCorrAmpAverage);
            this.NotifyOfPropertyChange(() => this.IsFilteringData);
            this.NotifyOfPropertyChange(() => this.Declination);
        }

        #endregion

        #region Plots

        #region Setup Plots

        /// <summary>
        /// Setup all the plots.
        /// </summary>
        private void SetupPlots()
        {
            CorrPlot = new ProfilePlotViewModel(new BaseSeriesType(BaseSeriesType.eBaseSeriesType.Base_Correlation));
            CorrPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Correlation), 0, Beam0Color);
            CorrPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Correlation), 1, Beam1Color);
            CorrPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Correlation), 2, Beam2Color);
            CorrPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Correlation), 3, Beam3Color);
            CorrPlot.Plot.IsLegendVisible = false;
            CorrPlot.IsAvgSeriesOn = IsCorrAmpAverage;
            CorrPlot.IsProfileSeriesOn = !IsCorrAmpAverage;
            CorrPlot.IsFilterData = IsFilteringData;

            AmpPlot = new ProfilePlotViewModel(new BaseSeriesType(BaseSeriesType.eBaseSeriesType.Base_Amplitude));
            AmpPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Amplitude), 0, Beam0Color);
            AmpPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Amplitude), 1, Beam1Color);
            AmpPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Amplitude), 2, Beam2Color);
            AmpPlot.AddSeries(new ProfileType(ProfileType.eProfileType.WP_Amplitude), 3, Beam3Color);
            AmpPlot.Plot.IsLegendVisible = false;
            AmpPlot.IsAvgSeriesOn = IsCorrAmpAverage;
            AmpPlot.IsProfileSeriesOn = !IsCorrAmpAverage;
            AmpPlot.IsFilterData = IsFilteringData;

            BottomTrackRangePlot = CreateBottomTrackRangePlot();
            _bottomTrackRangeList = new List<float[]>();

            BottomTrackSpeedPlot = CreateBottomTrackSpeedPlot();
            _bottomTrackVelocityList = new List<double>();

            BottomTrackVelSeriesPlot = CreateBottomTrackVelSeriesPlot();
            _bottomTrackVelSeriesList = new List<float[]>();
        }

        /// <summary>
        /// Create a small line plot.
        /// </summary>
        /// <param name="minValue">Minimum value in the plot.</param>
        /// <param name="maxValue">Maximum value in the plot.</param>
        /// <returns>Plot model.</returns>
        private PlotModel CreateSmallPlot(double minValue, double maxValue)
        {
            // Create a temp variable
            // to update without continously
            // calling property change
            var temp = new PlotModel();

            //// Create a line series and points
            LineSeries series = new LineSeries();
            series.LineStyle = LineStyle.Solid;
            series.Color = LINE_COLOR;

            // Add series to the plot
            temp.Series.Add(series);

            // Do not change margins
            //temp.AutoAdjustPlotMargins = false;

            // No legend
            temp.IsLegendVisible = false;

            // No spacing around graph
            temp.PlotMargins = new OxyThickness(0, 0, 0, 0);

            // Make the border transparent
            temp.PlotAreaBorderColor = OxyColor.FromAColor(0, OxyColors.Black);

            // Set axis to not visiable
            LinearAxis leftAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false,
                //Minimum = minValue,
                //Maximum = maxValue
            };
            LinearAxis bottomAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,
            };
            temp.Axes.Add(leftAxis);
            temp.Axes.Add(bottomAxis);

            // Set the plot
            return temp;
        }

        /// <summary>
        /// Create a plotmodel with the following settings.
        /// </summary>
        /// <param name="name">Name of the plot.</param>
        /// <returns>A plotmodel setup.</returns>
        private PlotModel CreatePlot(string name)
        {
            var temp = new PlotModel() { Title = name };

            // Create line series and add to plot
            LineSeries beam1 = new LineSeries();
            LineSeries beam2 = new LineSeries();
            LineSeries beam3 = new LineSeries();
            LineSeries beam4 = new LineSeries();

            //add series to plot
            temp.Series.Add(beam1);
            temp.Series.Add(beam2);
            temp.Series.Add(beam3);
            temp.Series.Add(beam4);

            // Do not change margins
            //temp.AutoAdjustPlotMargins = false;

            // No legend
            temp.IsLegendVisible = false;

            // No spacing around graph
            //temp.PlotMargins = new OxyThickness(0);
            //temp.Padding = new OxyThickness(40, 10, 5, 45);
            temp.Background = OxyColors.Black;
            temp.TextColor = OxyColors.White;
            temp.PlotAreaBorderColor = OxyColors.White;

            return temp;
        }

        /// <summary>
        /// Create a PlotModel for Bottom Track plots.
        /// </summary>
        /// <returns>PlotModel setup for 4 beams of data.</returns>
        private PlotModel CreateBottomTrackPlot()
        {
            var temp = new PlotModel();

            temp.IsLegendVisible = false;

            //temp.AutoAdjustPlotMargins = false;
            //temp.PlotMargins = new OxyThickness(25, 0, 0, 0);
            //temp.Padding = new OxyThickness(10,0,10,0);

            temp.Background = OxyColors.Black;
            temp.TextColor = OxyColors.White;
            temp.PlotAreaBorderColor = OxyColors.White;

            return temp;
        }

        /// <summary>
        /// PlotModel for the Bottom Track Range.  This plot
        /// differs from the Bottom Track Range in that
        /// the Y axis is inverted and has 4 series.
        /// </summary>
        /// <returns>PlotModel for the Bottom Track Range.</returns>
        public PlotModel CreateBottomTrackRangePlot()
        {
            PlotModel temp = CreateBottomTrackPlot();

            // Create line series and add to plot
            // Color is specified for each series
            temp.Series.Add(new LineSeries() { Color = Beam0Color });
            temp.Series.Add(new LineSeries() { Color = Beam1Color });
            temp.Series.Add(new LineSeries() { Color = Beam2Color });
            temp.Series.Add(new LineSeries() { Color = Beam3Color });

            temp.Title = "Bottom Track Range";

            //// Setup the axis
            var c = OxyColors.White;
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                //Minimum = 0,
                StartPosition = 1,                                              // This will invert the axis to start at the top with minimum value
                EndPosition = 0,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                IntervalLength = 10,
                MinimumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                MaximumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be se
                Unit = "m"
            });
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //MajorStep = 1
                Minimum = 0,
                //Maximum = _maxDataSets,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = OxyPlot.Axes.TickStyle.None,
                IsAxisVisible = false,
                Unit = "Bin"
            });

            return temp;
        }

        /// <summary>
        /// Create a plot for the Bottom Track Speed.
        /// This plots is different from the Bottom Track
        /// Range because it uses 1 series and the Y axis is not inverted
        /// </summary>
        /// <returns>PlotModel for Bottom Track Speed.</returns>
        public PlotModel CreateBottomTrackSpeedPlot()
        {
            PlotModel temp = CreateBottomTrackPlot();

            // Create line series and add to plot
            // No color for the series is specified
            temp.Series.Add(new LineSeries());

            temp.Title = "Bottom Track Speed";

            //// Setup the axis
            var c = OxyColors.White;
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                //Minimum = 0,
                //StartPosition = 1,                                              // This will invert the axis to start at the top with minimum value
                //EndPosition = 0
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                IntervalLength = 10,
                MinimumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                MaximumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                Unit = "m/s"
            });
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //MajorStep = 1
                Minimum = 0,
                //Maximum = _maxDataSets,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = OxyPlot.Axes.TickStyle.None,
                IsAxisVisible = false,
                Unit = "Bin"
            });

            return temp;
        }

        /// <summary>
        /// Create a plot for the Bottom Track Speed.
        /// This plots is different from the Bottom Track
        /// Range because it uses 1 series and the Y axis is not inverted
        /// </summary>
        /// <returns>PlotModel for Bottom Track Speed.</returns>
        public PlotModel CreateBottomTrackVelSeriesPlot()
        {
            PlotModel temp = CreateBottomTrackPlot();

            // Create line series and add to plot
            // No color for the series is specified
            temp.Series.Add(new LineSeries() { Color = Beam0Color });
            temp.Series.Add(new LineSeries() { Color = Beam1Color });
            temp.Series.Add(new LineSeries() { Color = Beam2Color });
            temp.Series.Add(new LineSeries() { Color = Beam3Color });

            temp.Title = "Bottom Track Velocity Series";

            //// Setup the axis
            var c = OxyColors.White;
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                //Minimum = 0,
                //StartPosition = 1,                                              // This will invert the axis to start at the top with minimum value
                //EndPosition = 0
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                IntervalLength = 10,
                MinimumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                MaximumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                Unit = "m/s"
            });
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //MajorStep = 1
                Minimum = 0,
                //Maximum = _maxDataSets,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = OxyPlot.Axes.TickStyle.None,
                IsAxisVisible = false,
                Unit = "Bin"
            });

            return temp;
        }

        #endregion

        #region Update Plots

        ///<summary>
        /// Update all the series to display
        /// the graph.
        ///</summary>
        private void AddSeries(DataSet.Ensemble adcpData)
        {
            // Verify the ensemble is good
            if (adcpData != null)
            {
                // Verify the data is available before trying to draw
                if (adcpData.IsCorrelationAvail)
                {
                    // Correlation plots
                    Task.Run(() => CorrPlot.AddIncomingData(adcpData, adcpData.EnsembleData.NumBins));
                }

                if (adcpData.IsAmplitudeAvail)
                {
                    // Amplitude plots
                    Task.Run(() => AmpPlot.AddIncomingData(adcpData, adcpData.EnsembleData.NumBins));
                }

                if (adcpData.IsBottomTrackAvail) // Bottom Track plots
                {
                    // Bottom Track Range
                    AddBottomTrackRangeSeries(adcpData);

                    // Bottom Track Speed
                    AddBottomTrackSpeedSeries(adcpData);

                    // Bottom Track Beam Series
                    AddBottomTrackVelSeriesSeries(adcpData);

                    // Set the orientation of the test based off bottom track velocities
                    SetTestOrientation(adcpData);
                }
            }
        }

        #region Bottom Track Range

        /// <summary>
        /// Add the Bottom Track Range values to the plot.
        /// This will keep a list of the last MAX_DATASETS ranges
        /// and plot them.
        /// </summary>
        /// <param name="adcpData">Get the latest data.</param>
        private void AddBottomTrackRangeSeries(DataSet.Ensemble adcpData)
        {
            try
            {
                _bottomTrackRangeList.Add(adcpData.BottomTrackData.Range);
                if (_bottomTrackRangeList.Count > MaxEnsembles)
                {
                    _bottomTrackRangeList.RemoveAt(0);
                }

                // Update the plot
                UpdateBottomTrackRangePlot(_bottomTrackRangeList);
            }
            catch (Exception)
            {
                // When shutting down, can get a null reference
            }
        }

        /// <summary>
        /// Update the PlotModel with the latest data.
        /// </summary>
        /// <param name="rangeList">Latest data to update the plot.</param>
        private void UpdateBottomTrackRangePlot(List<float[]> rangeList)
        {
            lock (BottomTrackRangePlot.SyncRoot)
            {
                if (rangeList != null)
                {
                    // Create the Line Series if they do not exist
                    if (_btRangeLsBeam0 == null)
                    {
                        _btRangeLsBeam0 = new LineSeries() { Color = Beam0Color };
                    }
                    if (_btRangeLsBeam1 == null)
                    {
                        _btRangeLsBeam1 = new LineSeries() { Color = Beam1Color };
                    }
                    if (_btRangeLsBeam2 == null)
                    {
                        _btRangeLsBeam2 = new LineSeries() { Color = Beam2Color };
                    }
                    if (_btRangeLsBeam3 == null)
                    {
                        _btRangeLsBeam3 = new LineSeries() { Color = Beam3Color };
                    }

                    // Clear the last points
                    _btRangeLsBeam0.Points.Clear();
                    _btRangeLsBeam1.Points.Clear();
                    _btRangeLsBeam2.Points.Clear();
                    _btRangeLsBeam3.Points.Clear();

                    // Populate the series with new points
                    int index = 0;
                    for (int x = 0; x < rangeList.Count; x++)
                    {
                        if (rangeList[x] != null)
                        {
                            if (IsFilteringData)
                            {
                                // If the point is bad, do not plot it
                                if (rangeList[x].Length > 0 && rangeList[x][0] != DataSet.Ensemble.BAD_RANGE)
                                {
                                    _btRangeLsBeam0.Points.Add(new DataPoint(index, rangeList[x][0]));
                                }

                                if (rangeList[x].Length > 1 && rangeList[x][1] != DataSet.Ensemble.BAD_RANGE)
                                {
                                    _btRangeLsBeam1.Points.Add(new DataPoint(index, rangeList[x][1]));
                                }

                                if (rangeList[x].Length > 2 && rangeList[x][2] != DataSet.Ensemble.BAD_RANGE)
                                {
                                    _btRangeLsBeam2.Points.Add(new DataPoint(index, rangeList[x][2]));
                                }

                                if (rangeList[x].Length > 3 && rangeList[x][3] != DataSet.Ensemble.BAD_RANGE)
                                {
                                    _btRangeLsBeam3.Points.Add(new DataPoint(index, rangeList[x][3]));
                                }
                            }
                            else
                            {
                                if (rangeList[x].Length > 0)
                                {
                                    _btRangeLsBeam0.Points.Add(new DataPoint(index, rangeList[x][0]));
                                }

                                if (rangeList[x].Length > 1)
                                {
                                    _btRangeLsBeam1.Points.Add(new DataPoint(index, rangeList[x][1]));
                                }

                                if (rangeList[x].Length > 2)
                                {
                                    _btRangeLsBeam2.Points.Add(new DataPoint(index, rangeList[x][2]));
                                }

                                if (rangeList[x].Length > 3)
                                {
                                    _btRangeLsBeam3.Points.Add(new DataPoint(index, rangeList[x][3]));
                                }
                            }
                        }

                        index++;
                    }

                    // Update series
                    if (BottomTrackRangePlot.Series.Count < 4)
                    {
                        BottomTrackRangePlot.Series.Add(_btRangeLsBeam0);
                        BottomTrackRangePlot.Series.Add(_btRangeLsBeam1);
                        BottomTrackRangePlot.Series.Add(_btRangeLsBeam2);
                        BottomTrackRangePlot.Series.Add(_btRangeLsBeam3);
                    }
                    else
                    {
                        BottomTrackRangePlot.Series[0] = _btRangeLsBeam0;
                        BottomTrackRangePlot.Series[1] = _btRangeLsBeam1;
                        BottomTrackRangePlot.Series[2] = _btRangeLsBeam2;
                        BottomTrackRangePlot.Series[3] = _btRangeLsBeam3;
                    }
                }
                else
                {
                    // Just clear
                    BottomTrackRangePlot.Series.Clear();
                }     
            }

            // Update the plot
            BottomTrackRangePlot.InvalidatePlot(true);
        }



        #endregion

        #region Bottom Track Speed

        /// <summary>
        /// Add the Bottom Track Range values to the plot.
        /// This will keep a list of the last MAX_DATASETS ranges
        /// and plot them.
        /// </summary>
        /// <param name="adcpData">Get the latest data.</param>
        private void AddBottomTrackSpeedSeries(DataSet.Ensemble adcpData)
        {
            _bottomTrackVelocityList.Add(adcpData.BottomTrackData.GetVelocityMagnitude());
            if (_bottomTrackVelocityList.Count > MaxEnsembles)
            {
                _bottomTrackVelocityList.RemoveAt(0);
            }

            // Update the plot
            try
            {
                UpdateBottomTrackSpeedPlot(_bottomTrackVelocityList);
            }
            catch (Exception)
            {
                // When shutting down, can get a null reference
            }
        }

        /// <summary>
        /// Update the PlotModel with the latest data.
        /// If filtering is turned on and a bad speed is found, it will not
        /// plot the point.  On the next pass, it will move to the next good point.
        /// </summary>
        /// <param name="speedList">Latest data to update the plot.</param>
        private void UpdateBottomTrackSpeedPlot(List<double> speedList)
        {
            lock (BottomTrackSpeedPlot.SyncRoot)
            {
                if (speedList != null)
                {
                    if (_btSpeedLs == null)
                    {
                        _btSpeedLs = new LineSeries();
                    }

                    // Clear the last points
                    _btSpeedLs.Points.Clear();

                    // Populate the series with new points
                    int index = 0;
                    //foreach (double speed in speedList)
                    for (int x = 0; x < speedList.Count; x++)
                    {
                        double curSpeed = speedList[x];

                        // Check if data should be filtered
                        if (IsFilteringData)
                        {
                            // Filter for good speeds
                            if (curSpeed != 0)
                            {
                                _btSpeedLs.Points.Add(new DataPoint(index, curSpeed));
                            }
                        }
                        else
                        {
                            _btSpeedLs.Points.Add(new DataPoint(index, curSpeed));
                        }

                        index++;
                    }

                    // Update the series
                    if (BottomTrackSpeedPlot.Series.Count < 1)
                    {
                        BottomTrackSpeedPlot.Series.Add(_btSpeedLs);
                    }
                    else
                    {
                        BottomTrackSpeedPlot.Series[0] = _btSpeedLs;
                    }
                }
                else
                {
                    // Clear the plot
                    BottomTrackSpeedPlot.Series.Clear();
                }
            }

            // Update the plot
            BottomTrackSpeedPlot.InvalidatePlot(true);
        }

        #endregion

        #region Bottom Track Beam Series

        /// <summary>
        /// Add the latest Bottom Track Beam velocity data to the list.
        /// Then update the plot.
        /// </summary>
        /// <param name="adcpData">Latest ensemble.</param>
        private void AddBottomTrackVelSeriesSeries(DataSet.Ensemble adcpData)
        {
            _bottomTrackVelSeriesList.Add(adcpData.BottomTrackData.BeamVelocity);
            if (_bottomTrackVelSeriesList.Count > MaxEnsembles)
            {
                _bottomTrackVelSeriesList.RemoveAt(0);
            }

            // Update the plot
            try
            {
                UpdateBottomTrackVelSeriesPlot(_bottomTrackVelSeriesList);
            }
            catch (Exception)
            {
                // When shutting down, can get a null reference
            }
        }

        /// <summary>
        /// Update the Bottom Track Velocity series plot with the latest data.
        /// Then update the plot in the view.
        /// </summary>
        /// <param name="dataList">Latest data to plot.</param>
        private void UpdateBottomTrackVelSeriesPlot(List<float[]> dataList)
        {
            lock (BottomTrackVelSeriesPlot.SyncRoot)
            {
                if (dataList != null)
                {
                    // Make a copy just in case the datalist is modified
                    List<float[]> tempList = new List<float[]>(dataList);

                    // Create the Line series
                    if (_btBeamVelLsBeam0 == null)
                    {
                        _btBeamVelLsBeam0 = new LineSeries() { Color = Beam0Color };
                    }
                    if (_btBeamVelLsBeam1 == null)
                    {
                        _btBeamVelLsBeam1 = new LineSeries() { Color = Beam1Color };
                    }
                    if (_btBeamVelLsBeam2 == null)
                    {
                        _btBeamVelLsBeam2 = new LineSeries() { Color = Beam2Color };
                    }
                    if (_btBeamVelLsBeam3 == null)
                    {
                        _btBeamVelLsBeam3 = new LineSeries() { Color = Beam3Color };
                    }

                    // Clear the last points
                    _btBeamVelLsBeam0.Points.Clear();
                    _btBeamVelLsBeam1.Points.Clear();
                    _btBeamVelLsBeam2.Points.Clear();
                    _btBeamVelLsBeam3.Points.Clear();

                    // Populate the series with new points
                    int index = 0;
                    for (int x = 0; x < tempList.Count; x++)
                    {
                        if (tempList[x] != null)
                        {
                            if (IsFilteringData)
                            {
                                // If the point is bad, do not plot it
                                if (tempList[x].Length > 0 && tempList[x][0] != DataSet.Ensemble.BAD_VELOCITY)
                                {
                                    _btBeamVelLsBeam0.Points.Add(new DataPoint(index, tempList[x][0]));
                                }

                                if (tempList[x].Length > 1 && tempList[x][1] != DataSet.Ensemble.BAD_VELOCITY)
                                {
                                    _btBeamVelLsBeam1.Points.Add(new DataPoint(index, tempList[x][1]));
                                }

                                if (tempList[x].Length > 2 && tempList[x][2] != DataSet.Ensemble.BAD_VELOCITY)
                                {
                                    _btBeamVelLsBeam2.Points.Add(new DataPoint(index, tempList[x][2]));
                                }

                                if (tempList[x].Length > 3 && tempList[x][3] != DataSet.Ensemble.BAD_VELOCITY)
                                {
                                    _btBeamVelLsBeam3.Points.Add(new DataPoint(index, tempList[x][3]));
                                }
                            }
                            else
                            {
                                if (tempList[x].Length > 0)
                                {
                                    _btBeamVelLsBeam0.Points.Add(new DataPoint(index, tempList[x][0]));
                                }

                                if (tempList[x].Length > 1)
                                {
                                    _btBeamVelLsBeam1.Points.Add(new DataPoint(index, tempList[x][1]));
                                }

                                if (tempList[x].Length > 2)
                                {
                                    _btBeamVelLsBeam2.Points.Add(new DataPoint(index, tempList[x][2]));
                                }

                                if (tempList[x].Length > 3)
                                {
                                    _btBeamVelLsBeam3.Points.Add(new DataPoint(index, tempList[x][3]));
                                }
                            }
                        }

                        index++;
                    }

                    // Update the plot
                    if (BottomTrackVelSeriesPlot.Series.Count < 4)
                    {
                        BottomTrackVelSeriesPlot.Series.Add(_btBeamVelLsBeam0);
                        BottomTrackVelSeriesPlot.Series.Add(_btBeamVelLsBeam1);
                        BottomTrackVelSeriesPlot.Series.Add(_btBeamVelLsBeam2);
                        BottomTrackVelSeriesPlot.Series.Add(_btBeamVelLsBeam3);
                    }
                    else
                    {
                        BottomTrackVelSeriesPlot.Series[0] = _btBeamVelLsBeam0;
                        BottomTrackVelSeriesPlot.Series[1] = _btBeamVelLsBeam1;
                        BottomTrackVelSeriesPlot.Series[2] = _btBeamVelLsBeam2;
                        BottomTrackVelSeriesPlot.Series[3] = _btBeamVelLsBeam3;
                    }
                }
                else
                {
                    BottomTrackVelSeriesPlot.Series.Clear();
                }
            }

            // Update the plot
            BottomTrackVelSeriesPlot.InvalidatePlot(true);
        }

        #endregion

        #region Test Orientation

        /// <summary>
        /// This will determine the test orientation.  It looks at which bottom beam velocity
        /// is above 0.2 m/s.  The oppositie should be the inverse or close to it.  It will just verify
        /// the opposite is less than 0.  Which ever beam is greater then 0.2 is considered the beam that
        /// is forward.  This may jump around in bad test conditions or if the boat is moving to slow.
        /// </summary>
        /// <param name="adcpData">Ensemble data.</param>
        private void SetTestOrientation(DataSet.Ensemble adcpData)
        {
            if (adcpData.BottomTrackData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
            {
                float b0 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_0_INDEX];
                float b1 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_1_INDEX];
                float b2 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_2_INDEX];
                float b3 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_3_INDEX];

                // Make sure they are not bad velocity
                if (b0 != DataSet.Ensemble.BAD_VELOCITY && b1 != DataSet.Ensemble.BAD_VELOCITY && b2 != DataSet.Ensemble.BAD_VELOCITY && b3 != DataSet.Ensemble.BAD_VELOCITY)
                {
                    // Beam 0 is forward
                    if (b0 > 0.2 && b1 < 0)
                    {
                        TestOrientation = "Beam 0 Forward";
                        TestOrientationColor = Beam0ColorStr;
                    }
                    else if (b1 > 0.2 && b0 < 0)
                    {
                        TestOrientation = "Beam 1 Forward";
                        TestOrientationColor = Beam1ColorStr;
                    }
                    else if (b2 > 0.2 && b3 < 0)
                    {
                        TestOrientation = "Beam 2 Forward";
                        TestOrientationColor = Beam2ColorStr;
                    }
                    else if (b3 > 0.2 && b2 < 0)
                    {
                        TestOrientation = "Beam 3 Forward";
                        TestOrientationColor = Beam3ColorStr;
                    }
                    else
                    {
                        TestOrientation = "";
                    }
                }
            }
            // Vertical Beam
            else if (adcpData.BottomTrackData.NumBeams == 1)
            {
                TestOrientation = "Vertical BEAM";
                TestOrientationColor = Beam0ColorStr;
            }
        }

        #endregion

        #region Distance Made Good Plot

        /// <summary>
        /// Add the Distance Made Good values to the plot.
        /// This will keep a list of the last MAX_DATASETS ranges
        /// and plot them.
        /// </summary>
        /// <param name="prt">Get the latest data.</param>
        private async Task AddDistanceMadeGoodSeries(ProjectReportText prt)
        {
            // Copy the data needed by the DMG plot
            DmgPlotViewModel.DmgPlotData data = new DmgPlotViewModel.DmgPlotData();
            data.AddData(prt.GpsPoints, prt.BtEarthPoints);

            await DmgPlot.AddIncomingData(data);
        }

        #endregion

        #endregion

        #region Clear Plots

        /// <summary>
        /// Clear all the values for the plots.
        /// </summary>
        private void ClearPlots()
        {
            // Use a negative number
            // to know if the value has
            // changed or never been set
            //_maxEnsembles = DEFAULT_MAX_ENSEMBLES;

            // Clear bottom track Range plot
            _bottomTrackRangeList.Clear();
            UpdateBottomTrackRangePlot(null);

            // Clear bottom track Speed plot
            _bottomTrackVelocityList.Clear();
            UpdateBottomTrackSpeedPlot(null);

            // Clear Bottom Track Velocity series plot
            _bottomTrackVelSeriesList.Clear();
            UpdateBottomTrackVelSeriesPlot(null);

            // Clear the Amplitude plot
            AmpPlot.ClearIncomingData();

            // Clear the Correlation plot
            CorrPlot.ClearIncomingData();

            // Distance Made Good Plot
            DmgPlot.ClearPlot();

            GoodEnsembleCount = 0;
            EnsembleCount = 0;
            GoodEnsemblePercentage = 0.ToString("0.00") + "%";
            BadStatusCount = 0;
            BadStatusPercentage = 0.ToString("0.00") + "%";
        }

        #endregion

        #endregion

        #region Ensemble Text Output

        /// <summary>
        /// Output a text output of the latest ensemble data.
        /// </summary>
        private void SetEnsembleTextOutput(DataSet.Ensemble ensemble)
        {
            // Lat Lon
            if (ensemble.IsNmeaAvail)
            {
                if (ensemble.NmeaData.IsGpggaAvail())
                {
                    _latLon = ensemble.NmeaData.GPGGA.Position.ToString();
                    _lat = ensemble.NmeaData.GPGGA.Position.Latitude;
                    _lon = ensemble.NmeaData.GPGGA.Position.Longitude;
                    _gpsStatus = ensemble.NmeaData.GPGGA.FixQuality.ToString();
                }
                else
                {
                    _latLon = "";
                    _lat = new DotSpatial.Positioning.Latitude();
                    _lon = new DotSpatial.Positioning.Longitude();
                    _gpsStatus = "";
                }

                if(ensemble.NmeaData.IsGphdtAvail())
                {
                    _GpsHeading = ensemble.NmeaData.GPHDT.Heading.DecimalDegrees.ToString() + "°";
                }
                else
                {
                    _GpsHeading = "";
                }
            }
            else
            {
                _latLon = "";
                _lat = new DotSpatial.Positioning.Latitude();
                _lon = new DotSpatial.Positioning.Longitude();
                _gpsStatus = "";
                _GpsHeading = "";
            }

            // Heading, pitch, roll
            if (ensemble.IsAncillaryAvail)
            {
                _heading = ensemble.AncillaryData.Heading.ToString("0.000") + "°";
                _pitch = ensemble.AncillaryData.Pitch.ToString("0.000") + "°";
                _roll = ensemble.AncillaryData.Roll.ToString("0.000") + "°";
                _waterTemp = ensemble.AncillaryData.WaterTemp.ToString("0.000") + "° C";
                _pressure = ensemble.AncillaryData.Pressure.ToString("0.000") + " Pa";
                _systemTemp = ensemble.AncillaryData.SystemTemp.ToString("0.000") + "° C";
                _BinSize = ensemble.AncillaryData.BinSize.ToString("0.000") + "m";
                _Blank = ensemble.AncillaryData.FirstBinRange.ToString("0.000") + "m";
                if (ensemble.EnsembleData.ActualPingCount > 1)
                {
                    _ActualPingCount = ensemble.EnsembleData.ActualPingCount.ToString("0") + " pings";
                }
                else
                {
                    _ActualPingCount = ensemble.EnsembleData.ActualPingCount.ToString("0") + " ping";
                }
                _FirstPingTime = ensemble.AncillaryData.FirstPingTime.ToString("0.00") + " sec";
                _LastPingTime = ensemble.AncillaryData.LastPingTime.ToString("0.00") + " sec";
            }
            else
            {
                _heading = "";
                _pitch = "";
                _roll = "";
                _waterTemp = "";
                _pressure = "";
                _systemTemp = "";
                _BinSize = "";
                _Blank = "";
                _ActualPingCount = "";
                _FirstPingTime = "";
                _LastPingTime = "";
             }

            // Status
            if (ensemble.IsEnsembleAvail)
            {
                _status = ensemble.EnsembleData.Status.ToString();
                _sysSerialNumber = ensemble.EnsembleData.SysSerialNumber.ToString();
            }
            else
            {
                _status = "";
                _sysSerialNumber = "";
            }

            // Update all the displays
            this.NotifyOfPropertyChange(null);
        }


        #endregion

        #region Record Test Results

        /// <summary>
        /// Record the test results to a file in CSV format.
        /// </summary>
        /// <param name="dir">Directory to store the results.</param>
        /// <param name="testFileName">Test file name created for this test.</param>
        public void RecordTestResults(string dir, string testFileName)
        {
            // Verify the data should be recorded
            if (IsRecordTestResults)
            {
                // Verify a serial number has been set
                // If no serial number is set, then no data has been recorded
                if (!string.IsNullOrEmpty(SysSerialNumber))
                {
                    string resultFile = dir + VALIDATION_TEST_FILE_NAME;

                    // Set the header for the file
                    SetFileHeader(resultFile);                                                    // Set Header for the file if it needs

                    StringBuilder result = new StringBuilder();
                    result.Append(DateTime.Now.ToString() + ",");                                 // Date and Time
                    result.Append("SN" + SysSerialNumber + _Config.IndexCodeString() + ",");      // Serial Number [CEPO Index] Subsystem Code
                    result.Append(TestOrientation + ",");                                         // Test Orientation
                    result.Append(testFileName + ",");                                            // Validation Test file name
                    result.Append(Lat + ",");                                                     // Last Latitude
                    result.Append(Lon + ",");                                                     // Last Longitude
                    result.Append(Heading + ",");                                                 // Last Heading
                    result.Append(Pitch + ",");                                                   // Last Pitch
                    result.Append(Roll + ",");                                                    // Last Roll
                    result.Append(Status + ",");                                                  // Last Status
                    result.Append(GpsStatus + ",");                                               // Last Gps Status
                    result.Append(WaterTemp + ",");                                               // Last Water Temp
                    result.Append(SystemTemp + ",");                                              // Last System Temp
                    result.Append(BinSize + ",");                                                 // Bin Size
                    result.Append(Blank + ",");                                                   // Blank

                    // GPS
                    result.Append(ReportText._distanceTraveled.GpsMag + ",");                     // GPS Mag
                    result.Append(ReportText._distanceTraveled.GpsDir + ",");                     // GPS Dir

                    // BT Earth
                    result.Append(ReportText._distanceTraveled.BtE + ",");                        // BT East
                    result.Append(ReportText._distanceTraveled.BtN + ",");                        // BT North
                    result.Append(ReportText._distanceTraveled.BtU + ",");                        // BT Up
                    result.Append(ReportText._distanceTraveled.BtEarthMag + ",");                 // BT Earth Mag
                    result.Append(ReportText._distanceTraveled.BtEarthDir + ",");                 // BT Earth Dir
                    result.Append(ReportText._distanceTraveled.BtEarthPercentError + ",");        // BT Earth Percent Error

                    // BT Instrument
                    result.Append(ReportText._distanceTraveled.BtX + ",");                        // BT X
                    result.Append(ReportText._distanceTraveled.BtY + ",");                        // BT Y
                    result.Append(ReportText._distanceTraveled.BtZ + ",");                        // BT Z
                    result.Append(ReportText._distanceTraveled.BtInstrumentMag + ",");            // BT Instrument Mag
                    result.Append(ReportText._distanceTraveled.BtInstrumentDir + ",");            // BT Instrument Dir
                    result.Append(ReportText._distanceTraveled.BtInstrumentPercentError + ",");   // BT Instrument Percent Error

                    // WP Earth
                    result.Append(ReportText._distanceTraveled.WpE + ",");                        // WP East
                    result.Append(ReportText._distanceTraveled.WpN + ",");                        // WP North
                    result.Append(ReportText._distanceTraveled.WpU + ",");                        // WP Up
                    result.Append(ReportText._distanceTraveled.WpEarthMag + ",");                 // WP Earth Mag
                    result.Append(ReportText._distanceTraveled.WpEarthDir + ",");                 // WP Earth Dir
                    result.Append(ReportText._distanceTraveled.WpEarthPercentError + ",");        // WP Earth Percent Error

                    // Wp Instrument
                    result.Append(ReportText._distanceTraveled.WpX + ",");                        // WP X
                    result.Append(ReportText._distanceTraveled.WpY + ",");                        // WP Y
                    result.Append(ReportText._distanceTraveled.WpZ + ",");                        // WP Z
                    result.Append(ReportText._distanceTraveled.WpInstrumentMag + ",");            // WP Instrument Mag
                    result.Append(ReportText._distanceTraveled.WpInstrumentDir + ",");            // WP Instrument Dir
                    result.Append(ReportText._distanceTraveled.WpInstrumentPercentError + ",");   // WP Instrument Percent Error

                    // Average Amplitude
                    result.Append(ReportText.AvgAmpLake300B0 + ",");                              // Lake 300 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpLake300B1 + ",");                              // Lake 300 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpLake300B2 + ",");                              // Lake 300 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpLake300B3 + ",");                              // Lake 300 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpLake600B0 + ",");                              // Lake 600 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpLake600B1 + ",");                              // Lake 600 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpLake600B2 + ",");                              // Lake 600 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpLake600B3 + ",");                              // Lake 600 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpLake1200B0 + ",");                             // Lake 1200 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpLake1200B1 + ",");                             // Lake 1200 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpLake1200B2 + ",");                             // Lake 1200 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpLake1200B3 + ",");                             // Lake 1200 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpOcean300B0 + ",");                              // Ocean 300 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean300B1 + ",");                              // Ocean 300 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean300B2 + ",");                              // Ocean 300 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean300B3 + ",");                              // Ocean 300 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpOcean600B0 + ",");                              // Ocean 600 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean600B1 + ",");                              // Ocean 600 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean600B2 + ",");                              // Ocean 600 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean600B3 + ",");                              // Ocean 600 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpOcean1200B0 + ",");                             // Ocean 1200 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean1200B1 + ",");                             // Ocean 1200 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean1200B2 + ",");                             // Ocean 1200 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpOcean1200B3 + ",");                             // Ocean 1200 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpNoiseB0 + ",");                                 // Noise Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpNoiseB1 + ",");                                 // Noise Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpNoiseB2 + ",");                                 // Noise Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpNoiseB3 + ",");                                 // Noise Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpLakeSnr300B0 + ",");                              // Lake SNR 300 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr300B1 + ",");                              // Lake SNR 300 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr300B2 + ",");                              // Lake SNR 300 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr300B3 + ",");                              // Lake SNR 300 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpLakeSnr600B0 + ",");                              // Lake SNR 600 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr600B1 + ",");                              // Lake SNR 600 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr600B2 + ",");                              // Lake SNR 600 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr600B3 + ",");                              // Lake SNR 600 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpLakeSnr1200B0 + ",");                             // Lake SNR 1200 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr1200B1 + ",");                             // Lake SNR 1200 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr1200B2 + ",");                             // Lake SNR 1200 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpLakeSnr1200B3 + ",");                             // Lake SNR 1200 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpOceanSnr300B0 + ",");                              // Ocean SNR 300 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr300B1 + ",");                              // Ocean SNR 300 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr300B2 + ",");                              // Ocean SNR 300 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr300B3 + ",");                              // Ocean SNR 300 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpOceanSnr600B0 + ",");                              // Ocean SNR 600 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr600B1 + ",");                              // Ocean SNR 600 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr600B2 + ",");                              // Ocean SNR 600 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr600B3 + ",");                              // Ocean SNR 600 kHz Beam 3 Average Amplitude

                    result.Append(ReportText.AvgAmpOceanSnr1200B0 + ",");                             // Ocean SNR 1200 kHz Beam 0 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr1200B1 + ",");                             // Ocean SNR 1200 kHz Beam 1 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr1200B2 + ",");                             // Ocean SNR 1200 kHz Beam 2 Average Amplitude
                    result.Append(ReportText.AvgAmpOceanSnr1200B3 + ",");                             // Ocean SNR 1200 kHz Beam 3 Average Amplitude

                    // Profile Range
                    result.Append(ReportText.ProfileRangeBeam0 + ",");                                // Profile Range Beam 0
                    result.Append(ReportText.ProfileRangeBeam1 + ",");                                // Profile Range Beam 1
                    result.Append(ReportText.ProfileRangeBeam2 + ",");                                // Profile Range Beam 2
                    result.Append(ReportText.ProfileRangeBeam3 + ",");                                // Profile Range Beam 3

                    // Direction Error
                    result.Append(ReportText._distanceTraveled.BtEarthDirError + ",");                // BT Earth Percent Error
                    result.Append(ReportText._distanceTraveled.BtInstrumentDirError + ",");           // BT Instrument Percent Error
                    result.Append(ReportText._distanceTraveled.WpEarthDirError + ",");                // WP Earth Percent Error
                    result.Append(ReportText._distanceTraveled.WpInstrumentDirError + ",");           // WP Instrument Percent Error

                    try
                    {
                        // Open write and write the line to the file
                        using (StreamWriter w = File.AppendText(resultFile))
                        {
                            w.WriteLine(result.ToString());
                            w.Flush();
                            w.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        // Log any errors
                        log.Error("Error recording Validation Test Results.", e);
                    }
                }
            }
        }

        /// <summary>
        /// If the file does not exist, create the file
        /// with header as the first line.
        /// </summary>
        /// <param name="resultFile">File to record results.</param>
        private void SetFileHeader(string resultFile)
        {
            // If the file does not exist
            // create the file with the header as the first line
            if (!File.Exists(resultFile))
            {
                StringBuilder result = new StringBuilder();
                result.Append("DateTime" + ",");                                                // Date and Time
                result.Append("SysSerialNumber" + ",");                                         // Serial Number With Subsystem Config
                result.Append("TestOrientation" + ",");                                         // Test Orientation
                result.Append("FileName" + ",");                                                // Lake Test File Name
                result.Append("Lat" + ",");                                                     // Last Lat
                result.Append("Lon" + ",");                                                     // Last Lon
                result.Append("Heading" + ",");                                                 // Last Heading
                result.Append("Pitch" + ",");                                                   // Last Pitch
                result.Append("Roll" + ",");                                                    // Last Roll
                result.Append("Status" + ",");                                                  // Last Status
                result.Append("GpsStatus" + ",");                                               // Last Gps Status
                result.Append("WaterTemp" + ",");                                               // Last Water Temp
                result.Append("SystemTemp" + ",");                                              // Last System Temp
                result.Append("BinSize" + ",");                                                 // Bin Size
                result.Append("Blank" + ",");                                                   // Blank


                // GPS
                result.Append("GpsMag" + ",");                     // GPS Mag
                result.Append("GpsDir" + ",");                     // GPS Dir

                // BT Earth
                result.Append("BtE" + ",");                        // BT East
                result.Append("BtN" + ",");                        // BT North
                result.Append("BtU" + ",");                        // BT Up
                result.Append("BtEarthMag" + ",");                 // BT Earth Mag
                result.Append("BtEarthDir" + ",");                 // BT Earth Dir
                result.Append("BtEarthPercentError" + ",");        // BT Earth Percent Error

                // BT Instrument
                result.Append("BtX" + ",");                        // BT X
                result.Append("BtY" + ",");                        // BT Y
                result.Append("BtZ" + ",");                        // BT Z
                result.Append("BtInstrumentMag" + ",");            // BT Instrument Mag
                result.Append("BtInstrumentDir" + ",");            // BT Instrument Dir
                result.Append("BtInstrumentPercentError" + ",");   // BT Instrument Percent Error

                // WP Earth
                result.Append("WpE" + ",");                        // WP East
                result.Append("WpN" + ",");                        // WP North
                result.Append("WpU" + ",");                        // WP Up
                result.Append("WpEarthMag" + ",");                 // WP Earth Mag
                result.Append("WpEarthDir" + ",");                 // WP Earth Dir
                result.Append("WpEarthPercentError" + ",");        // WP Earth Percent Error

                // Wp Instrument
                result.Append("WpX" + ",");                        // WP X
                result.Append("WpY" + ",");                        // WP Y
                result.Append("WpZ" + ",");                        // WP Z
                result.Append("WpInstrumentMag" + ",");            // WP Instrument Mag
                result.Append("WpInstrumentDir" + ",");            // WP Instrument Dir
                result.Append("WpInstrumentPercentError" + ",");   // WP Instrument Percent Error

                // Average Amplitude
                // Lake Signal
                result.Append("AvgAmpLake300B0" + ",");                 // Lake 300 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpLake300B1" + ",");                 // Lake 300 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpLake300B2" + ",");                 // Lake 300 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpLake300B3" + ",");                 // Lake 300 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpLake600B0" + ",");                 // Lake 600 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpLake600B1" + ",");                 // Lake 600 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpLake600B2" + ",");                 // Lake 600 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpLake600B3" + ",");                 // Lake 600 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpLake1200B0" + ",");                 // Lake 1200 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpLake1200B1" + ",");                 // Lake 1200 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpLake1200B2" + ",");                 // Lake 1200 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpLake1200B3" + ",");                 // Lake 1200 kHz Beam 3 Average Amplitude

                // Ocean Signal
                result.Append("AvgAmpOcean300B0" + ",");                 // Ocean 300 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpOcean300B1" + ",");                 // Ocean 300 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpOcean300B2" + ",");                 // Ocean 300 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpOcean300B3" + ",");                 // Ocean 300 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpOcean600B0" + ",");                 // Ocean 600 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpOcean600B1" + ",");                 // Ocean 600 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpOcean600B2" + ",");                 // Ocean 600 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpOcean600B3" + ",");                 // Ocean 600 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpOcean1200B0" + ",");                 // Ocean 1200 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpOcean1200B1" + ",");                 // Ocean 1200 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpOcean1200B2" + ",");                 // Ocean 1200 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpOcean1200B3" + ",");                 // Ocean 1200 kHz Beam 3 Average Amplitude

                // Noise
                result.Append("AvgAmpNoiseB0" + ",");                     // Noise Beam 0 Average Amplitude
                result.Append("AvgAmpNoiseB1" + ",");                     // Noise Beam 1 Average Amplitude
                result.Append("AvgAmpNoiseB2" + ",");                     // Noise Beam 2 Average Amplitude
                result.Append("AvgAmpNoiseB3" + ",");                     // Noise Beam 3 Average Amplitude

                // Lake SNR
                result.Append("AvgAmpLakeSnr300B0" + ",");                // Lake SNR 300 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpLakeSnr300B1" + ",");                // Lake SNR 300 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpLakeSnr300B2" + ",");                // Lake SNR 300 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpLakeSnr300B3" + ",");                // Lake SNR 300 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpLakeSnr600B0" + ",");                // Lake SNR 600 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpLakeSnr600B1" + ",");                // Lake SNR 600 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpLakeSnr600B2" + ",");                // Lake SNR 600 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpLakeSnr600B3" + ",");                // Lake SNR 600 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpLakeSnr1200B0" + ",");                // Lake SNR 1200 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpLakeSnr1200B1" + ",");                // Lake SNR 1200 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpLakeSnr1200B2" + ",");                // Lake SNR 1200 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpLakeSnr1200B3" + ",");                // Lake SNR 1200 kHz Beam 3 Average Amplitude

                // Ocean SNR
                result.Append("AvgAmpOceanSnr300B0" + ",");                // Ocean SNR 300 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpOceanSnr300B1" + ",");                // Ocean SNR 300 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpOceanSnr300B2" + ",");                // Ocean SNR 300 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpOceanSnr300B3" + ",");                // Ocean SNR 300 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpOceanSnr600B0" + ",");                // Ocean SNR 600 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpOceanSnr600B1" + ",");                // Ocean SNR 600 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpOceanSnr600B2" + ",");                // Ocean SNR 600 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpOceanSnr600B3" + ",");                // Ocean SNR 600 kHz Beam 3 Average Amplitude

                result.Append("AvgAmpOceanSnr1200B0" + ",");                // Ocean SNR 1200 kHz Beam 0 Average Amplitude
                result.Append("AvgAmpOceanSnr1200B1" + ",");                // Ocean SNR 1200 kHz Beam 1 Average Amplitude
                result.Append("AvgAmpOceanSnr1200B2" + ",");                // Ocean SNR 1200 kHz Beam 2 Average Amplitude
                result.Append("AvgAmpOceanSnr1200B3" + ",");                // Ocean SNR 1200 kHz Beam 3 Average Amplitude

                // Profile Range
                result.Append("AvgProfileRangeB0" + ",");                   // Profile Range Beam 0
                result.Append("AvgProfileRangeB1" + ",");                   // Profile Range Beam 1
                result.Append("AvgProfileRangeB2" + ",");                   // Profile Range Beam 2
                result.Append("AvgProfileRangeB3" + ",");                   // Profile Range Beam 3

                // Direction Error
                result.Append("BtEarthDirError" + ",");                     // BT Earth Dir Error
                result.Append("BtInstrumentDirError" + ",");                // BT Instrument Dir Error
                result.Append("WpEarthDirError" + ",");                     // WP Earth Dir Error
                result.Append("WpInstrumentDirError");                      // WP Instrument Percent Error

                try
                {
                    // Open write and write the line to the file
                    using (StreamWriter w = File.AppendText(resultFile))
                    {
                        w.WriteLine(result.ToString());
                        w.Flush();
                        w.Close();
                    }
                }
                catch (Exception e)
                {
                    // Log any errors
                    log.Error("Error recording Validation Test Results.", e);
                }
            }
        }

        #endregion

        #region Display Data

        /// <summary>
        /// Buffer the incoming ensembles.  Then execute the display data async.
        /// </summary>
        /// <param name="ensemble">Ensemble to buffer.</param>
        private async Task DisplayData(DataSet.Ensemble ensemble)
        {
            // Buffer the data
            _buffer.Enqueue(ensemble);

            // Execute async
            if (!_isProcessingBuffer)
            {
                // Execute async
                await Task.Run(() => DisplayDataExecute());
            }
        }

        /// <summary>
        /// Display the data.  This should only be called from DisplayData(object) and that
        /// should be called from DisplayDataExecute.Execute(ensemble) to run in the background.
        /// </summary>
        private async void DisplayDataExecute()
        {
            try
            {
                // Continue processing until all the data has been displayed
                while (_buffer.Count > 0)
                {
                    _isProcessingBuffer = true;

                    // Get the data from the buffer
                    DataSet.Ensemble ensemble = null;
                    if (_buffer.TryDequeue(out ensemble))
                    {
                        // Increase the ensemble count
                        EnsembleCount++;

                        // Add data to plots
                        AddSeries(ensemble);

                        // Add Text output
                        SetEnsembleTextOutput(ensemble);

                        // Add data to Distance made good calculation
                        //if (_isCalculateDmg && IsDmgDataGood(ensemble))
                        if (IsCalculateDmg)
                        {
                            // Increase the good ensemble count
                            GoodEnsembleCount++;

                            // Calculate the DMG
                            _reportText.AddIncomingData(ensemble);

                            // Update the Distance Made Good Plot
                            await AddDistanceMadeGoodSeries(_reportText);
                        }
                        else
                        {
                            // Set the minimum values
                            if (ensemble.IsEnsembleAvail)
                            {
                                _reportText.NumBins = ensemble.EnsembleData.NumBins;
                                _reportText.DateAndTime = ensemble.EnsembleData.EnsDateTime;
                            }
                        }

                        if (ensemble.IsEnsembleAvail)
                        {
                            // Check if the status is not good
                            // If it is not good, increment the count
                            if (ensemble.EnsembleData.Status.Value != RTI.Status.GOOD)
                            {
                                BadStatusCount++;
                            }
                        }

                        // Set the Percentages
                        if (_ensembleCount > 0)
                        {
                            // Calculate the Good Ensemble Percentage
                            double resultGoodEnsemblePercentage = (((double)_goodEnsembleCount) / ((double)_ensembleCount)) * 100.0;
                            GoodEnsemblePercentage = resultGoodEnsemblePercentage.ToString("0.00") + "%";

                            // Calculate the Bad Status Percentage
                            double resultBadStatusPercentage = (((double)_goodEnsembleCount) / ((double)_ensembleCount)) * 100.0;
                            BadStatusPercentage = resultBadStatusPercentage.ToString("0.00") + "%";
                        }
                    }
                }
                _isProcessingBuffer = false;
            }
            catch (Exception e)
            {
                log.Error("Error Displaying data.", e);
                _isProcessingBuffer = false;
            }
        }

        /// <summary>
        /// Display the bulk data async.
        /// </summary>
        /// <param name="eventData">BulkEnsembleEvent.</param>
        private async Task DisplayBulkDataExecute(object eventData)
        {
            // Convert the object to BulkEnsembleEvent
            BulkEnsembleEvent ensEvent = eventData as BulkEnsembleEvent;
            if (ensEvent != null)
            {
                // Clear all the current data
                await Task.Run(() => Clear());

                for (int x = 0; x < ensEvent.Ensembles.Count(); x++)
                {
                    // If no subsystem is given, then a project is not selected
                    // So receive all data and display
                    // If the serial number is not set, this may be an old ensemble
                    // Try to display it anyway
                    if (!_Config.SubSystem.IsEmpty() && !ensEvent.Ensembles.IndexValue(x).EnsembleData.SysSerialNumber.IsEmpty())
                    {
                        // Verify the subsystem matches this viewmodel's subystem.
                        if ((_Config.SubSystem != ensEvent.Ensembles.IndexValue(x).EnsembleData.GetSubSystem())        // Check if Subsystem matches 
                                || (_Config != ensEvent.Ensembles.IndexValue(x).EnsembleData.SubsystemConfig)          // Check if Subsystem Config matches
                                || _Config.Source != ensEvent.Source)                                   // Check if source matches
                        {
                            return;
                        }
                    }

                    // Display the data
                    await DisplayData(ensEvent.Ensembles.IndexValue(x));
                }
            }
        }

        #endregion

        #region Activate

        /// <summary>
        /// Turn on or off refreshing the plots.
        /// </summary>
        /// <param name="isActive"></param>
        public void ActivateVm(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                //CorrPlot.InvalidatePlot(true);
                //AmpPlot.InvalidatePlot(true);
                BottomTrackRangePlot.InvalidatePlot(true);
                BottomTrackSpeedPlot.InvalidatePlot(true);
                BottomTrackVelSeriesPlot.InvalidatePlot(true);
            }
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Check if the dataset's subsystem matches this
        /// viewmodel's subystem.
        /// </summary>
        /// <param name="ensemble"></param>
        /// <returns></returns>
        public bool ReceiveCurrentDataSetFilter(DataSet.Ensemble ensemble)
        {
            // Verify the subsystem matches this viewmodel's subystem.
            //return _subsystem == ensemble.EnsembleData.GetSubSystem();
            return true;
        }

        /// <summary>
        /// Update the display with the latest ensemble information.
        /// Verify the ensemble given matches the subsystem of this VM.
        /// 
        /// Filter the incoming data for calculating the DMG.  If either the GPS or the 
        /// Bottom Track data is bad, then do not calculate the DMG for the ensemble.  The
        /// data will be inconsistent between GPS and Bottom Track.
        /// </summary>
        /// <param name="ensEvent">Ensemble event which contains the ensemble.</param>
        public void Handle(EnsembleEvent ensEvent)
        {
            if(ensEvent.Ensemble == null)
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
                        || (_Config != ensEvent.Ensemble.EnsembleData.SubsystemConfig)          // Check if Subsystem Config matches
                        || _Config.Source != ensEvent.Source)                                   // Check if source matches
                {
                    // If it is not DVL data, then the data does not belong to this display
                    if (ensEvent.Ensemble.EnsembleData.SysSerialNumber != SerialNumber.DVL)
                    {
                        return;
                    }
                    else
                    {
                        // If DVL, check everything but subsystem
                        if (_Config != ensEvent.Ensemble.EnsembleData.SubsystemConfig           // Check if Subsystem Config matches
                                    || _Config.Source != ensEvent.Source)                       // Check if source matches
                        {
                            return;
                        }
                    }
                }
            }

            // Display the data
            Task.Run(() => DisplayData(ensEvent.Ensemble));
        }

        /// <summary>
        /// Display a bulk set of ensembles.
        /// </summary>
        /// <param name="ensEvent">Bulk Ensemble event.</param>
        public void Handle(BulkEnsembleEvent ensEvent)
        {
            // Display the data async
            Task.Run(() => DisplayBulkDataExecute(ensEvent));
        }

        /// <summary>
        /// Receive event when a new project has been selected.
        /// Then clear all the data in the view.
        /// </summary>
        /// <param name="message">Project received.</param>
        public void Handle(ProjectEvent message)
        {
            Clear();
        }

        /// <summary>
        /// Determine if the DMG data should be used.
        /// First check if we are filtering data.  If we are not filtering data, then allow the data to always be used.
        /// 
        /// Then check if the Bottom Track Earth Velocity and GPS data is good.  If they are good, then allow the data to 
        /// be used.  If either is not good, then there calculations will not match.  One will move forward while the
        /// other is waiting for the next data point with good data.
        /// </summary>
        /// <param name="ensemble">Ensemble to check.</param>
        /// <returns>TRUE = If Filter and all good.  / False = If Filtering and any data is bad.</returns>
        private bool IsDmgDataGood(DataSet.Ensemble ensemble)
        {
            if (IsFilteringData)
            {
                // Ensure the GPS data is good and the bottom track velocity is good
                // If either is bad, then the data will be off in calculating so ignore the data
                // This will not use the data, but it will still be recorded because the recordermanager is not filtering out data.
                // The user must filter the record data as needed.
                if (ensemble.IsBottomTrackAvail &&                              // If Bottom Track is available 
                    ensemble.IsNmeaAvail &&                                     // If GPS data is available
                    ensemble.BottomTrackData.IsEarthVelocityGood() &&           // If Bottom Track Earth Velocity data is good
                    ensemble.NmeaData.IsGpggaAvail())                           // If GPS GPGGA data is available
                {
                    return true;
                }
                else
                {
                    // One of the above failed
                    return false;
                }
            }

            // Default is true.
            return true;
        }

        #endregion
    }
}
