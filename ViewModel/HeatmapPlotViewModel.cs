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
 * 03/26/2015      RC          4.1.3       Initial coding
 * 07/26/2016      RC          4.4.3       Set the min and max options for each selected plot type.
 * 08/02/2016      RC          4.4.4       Added Interperlate option to the plot to blend data.
 * 
 */

using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RTI
{
    public class HeatmapPlotViewModel : PulseViewModel
    {
        #region Defaults

        ///// <summary>
        ///// Default number for maximum number of beams.
        ///// </summary>
        //private int DEFAULT_MAX_BEAM = 4;

        ///// <summary>
        ///// Default number of maximum bins.
        ///// </summary>
        //private int DEFAULT_MAX_BIN = DataSet.Ensemble.MAX_NUM_BINS;

        private const string TAG_COLOR_AXIS = "color";

        /// <summary>
        /// Heatmap series tag.
        /// </summary>
        private const string TAG_HEATMAP_SERIES = "Heatmap";

        /// <summary>
        /// Bottom Track Range series tag.
        /// </summary>
        private const string TAG_BT_SERIES = "BT";

        /// <summary>
        /// Min series tag.
        /// </summary>
        private const string TAG_MIN_SERIES = "Min";

        /// <summary>
        /// Max series tag.
        /// </summary>
        private const string TAG_MAX_SERIES = "Max";

        /// <summary>
        /// Avg series tag.
        /// </summary>
        private const string TAG_AVG_SERIES = "Avg";

        /// <summary>
        /// Standard Deviation Ping to Ping series tag.
        /// </summary>
        private const string TAG_STDP2P_SERIES = "STD P2P";

        /// <summary>
        /// Standard Deviation Bin to Bin series tag.
        /// </summary>
        private const string TAG_STDB2B_SERIES = "STD B2B";

        #endregion

        #region Variable

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Defaults

        ///// <summary>
        ///// Default height of the plot in pixels.
        ///// </summary>
        //private int DEFAULT_PLOT_HEIGHT = 180;

        ///// <summary>
        ///// Default width of the plot in pixels.
        ///// </summary>
        //private int DEFAULT_PLOT_WIDTH = 825;       // 420


        /// <summary>
        /// Default number of maximum bins.
        /// </summary>
        //private int DEFAULT_MAX_BIN = DataSet.Ensemble.MAX_NUM_BINS;

        #endregion

        /// <summary>
        /// Receive global events from the EventAggregator.
        /// </summary>
        private IEventAggregator _eventAggregator;

        /// <summary>
        /// Use as a list of all the ensembles that will be displayed.
        /// This list will be passed as a reference to the plot time
        /// series to know which data is available if the plots need
        /// to be updated if a new bin selection is made.
        /// </summary>
        private List<DataSet.Ensemble> _ensembleList;

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<EnsWithMax> _buffer;

        /// <summary>
        /// Minimum, maxmium and average series.
        /// </summary>
        //private List<MinMaxAvgStdSeries> _minMaxAvgStdSeriesList;

        /// <summary>
        /// Flag to know if processing the buffer.
        /// </summary>
        private bool _isProcessingBuffer;

        /// <summary>
        /// Lock when updating the series.
        /// </summary>
        private object UpdateSeriesLock = new object();

        /// <summary>
        /// Lock for updating the plot.
        /// </summary>
        private object _UpdatePlotLock = new object();

        /// <summary>
        /// Heatmap Options.
        /// </summary>
        private HeatmapSeriesOptions _options;

        #endregion

        #region Enum and Structs

        /// <summary>
        /// Object used to pass to a threadworker argument.
        /// </summary>
        private class EnsWithMax
        {
            /// <summary>
            /// Ensemble.
            /// </summary>
            public DataSet.Ensemble Ensemble { get; set; }

            /// <summary>
            /// Maximum number of ensembles to display.
            /// </summary>
            public int MaxEnsembles { get; set; }
        }

        #endregion

        #region Properties

        #region ID

        /// <summary>
        /// ID to identify this specific view model.
        /// </summary>
        public string ID { get; private set; }

        #endregion

        #region Plot

        /// <summary>
        /// The plot for the view model.  This will be the plot
        /// that will be updated by the user.
        /// </summary>
        private PlotModel _plot;
        /// <summary>
        /// The plot for the view model.  This will be the plot
        /// that will be updated by the user.
        /// </summary>
        public PlotModel Plot
        {
            get { return _plot; }
            set
            {
                _plot = value;
                this.NotifyOfPropertyChange(() => this.Plot);
            }
        }

        /// <summary>
        /// Title for the plot.
        /// </summary>
        private string _title;
        /// <summary>
        /// Title for the plot.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

        #endregion

        #region Base Series Type

        /// <summary>
        /// List of all the base series.
        /// </summary>
        public BindingList<HeatmapPlotSeries.HeatmapPlotType> SeriesTypeList { get; set; }

        /// <summary>
        /// The series type of this plot.  This will determine
        /// what the axis labels and axis scales will be.
        /// Used to add a new series.  To get the current
        /// data BaseSeriesType, use SelectedSeriesType.Type.
        /// </summary>
        private HeatmapPlotSeries.HeatmapPlotType _SelectedSeriesType;
        /// <summary>
        /// The series type of this plot.  This will determine
        /// what the axis labels and axis scales will be.
        /// Used to add a new series.  To get the current
        /// data BaseSeriesType, use SelectedSeriesType.Type.
        /// </summary>
        public HeatmapPlotSeries.HeatmapPlotType SelectedSeriesType
        {
            get { return _SelectedSeriesType; }
            set
            {
                _SelectedSeriesType = value;
                this.NotifyOfPropertyChange(() => this.SelectedSeriesType);

                // Set the plot title
                //temp.Title = seriesType.ToString();
                Title = _SelectedSeriesType.ToString();
                Plot.Title = Title;

                // Set the new options and update the series
                _options.Type = value;
                UpdateOptions();

                // Clear the plot and reset the series
                ClearPlot();
                AddSeries(_options);
            }
        }

        #endregion

        #region Filter Data

        /// <summary>
        /// This is a flag used to determine if the
        /// data should be filtered or not.  By filtering
        /// the data, any Error values are not plotted.
        /// By setting this to true, the plot will also
        /// show all error values, which will cause a spike
        /// in the data.
        /// </summary>
        private bool _isFilterData;
        /// <summary>
        /// This is a flag used to determine if the
        /// data should be filtered or not.  By filtering
        /// the data, any Error values are not plotted.
        /// By setting this to true, the plot will also
        /// show all error values, which will cause a spike
        /// in the data.
        /// </summary>
        public bool IsFilterData
        {
            get { return _isFilterData; }
            set
            {
                _isFilterData = value;
                this.NotifyOfPropertyChange(() => this.IsFilterData);
            }
        }

        #endregion

        #region Plot On/Off Switch

        /// <summary>
        /// This is a flag to turn on or off viewing the Profile series.
        /// </summary>
        private bool _IsProfileSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Profile series.
        /// </summary>
        public bool IsProfileSeriesOn
        {
            get { return _IsProfileSeriesOn; }
            set
            {
                _IsProfileSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsProfileSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_HEATMAP_SERIES, value);
            }
        }

        /// <summary>
        /// This is a flag to turn on or off viewing the Bottom Track Range series.
        /// </summary>
        private bool _IsBtRangeSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Bottom Track Range series.
        /// </summary>
        public bool IsBtRangeSeriesOn
        {
            get { return _IsBtRangeSeriesOn; }
            set
            {
                _IsBtRangeSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsBtRangeSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_BT_SERIES, value);
            }
        }

        #endregion

        #region Palette

        /// <summary>
        /// List of palettes.
        /// </summary>
        public BindingList<OxyPalette> PaletteList { get; set; }

        /// <summary>
        /// Palette for the plot.  This will be
        /// the color range of the plot.
        /// </summary>
        private OxyPalette _SelectedPalette;
        /// <summary>
        /// Palette for the plot.  This will be
        /// the color range of the plot.
        /// </summary>
        public OxyPalette SelectedPalette
        {
            get { return _SelectedPalette; }
            set
            {
                _SelectedPalette = value;
                this.NotifyOfPropertyChange(() => this.SelectedPalette);

                // Set the new color palette
                SetColorPlotAxis();

                // Set the options
                _options.Palette = HeatmapSeriesOptions.PaletteToList(_SelectedPalette);
                UpdateOptions();
            }
        }

        #endregion

        #region Min and Max Value

        /// <summary>
        /// Minimum value for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        private double _MinValue;
        /// <summary>
        /// Minimum value for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        public double MinValue
        {
            get { return _MinValue; }
            set
            {
                _MinValue = value;
                this.NotifyOfPropertyChange(() => this.MinValue);

                // Set the new color palette
                SetColorPlotAxis();

                // Send an event that the options were updated
                _options.MinValue = value;
                UpdateOptions();
            }
        }

        /// <summary>
        /// Maximum value for the plot.
        /// Anything above this value will be HighColor.
        /// </summary>
        private double _MaxValue;
        /// <summary>
        /// Maximum value for the plot.
        /// Anything above this value will be HighColor.
        /// </summary>
        public double MaxValue
        {
            get { return _MaxValue; }
            set
            {
                _MaxValue = value;
                this.NotifyOfPropertyChange(() => this.MaxValue);

                // Set the new color palette
                SetColorPlotAxis();

                // Send an event that the options were updated
                _options.MaxValue = value;
                UpdateOptions();
            }
        }

        #endregion

        #region Color Axis Major Step

        /// <summary>
        /// Minimum value for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        private double _ColorAxisMajorStep;
        /// <summary>
        /// Minimum value for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        public double ColorAxisMajorStep
        {
            get { return _ColorAxisMajorStep; }
            set
            {
                _ColorAxisMajorStep = value;
                this.NotifyOfPropertyChange(() => this.ColorAxisMajorStep);

                // Set the new color palette
                SetColorPlotAxis();

                // Send an event that the options were updated
                _options.ColorAxisMajorStep = value;
                UpdateOptions();
            }
        }

        #endregion

        #region Bottom Track Line

        /// <summary>
        /// This is a flag used to determine if we should display the 
        /// bottom track line.
        /// </summary>
        private bool _IsBottomTrackLine;
        /// <summary>
        /// This is a flag used to determine if we should display the 
        /// bottom track line.
        /// </summary>
        public bool IsBottomTrackLine
        {
            get { return _IsBottomTrackLine; }
            set
            {
                _IsBottomTrackLine = value;
                this.NotifyOfPropertyChange(() => this.IsBottomTrackLine);

                _options.IsBottomTrackLine = value;
                UpdateOptions();
            }
        }

        #endregion

        #region Min and Max Bin

        /// <summary>
        /// Minimum bin for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        private int _MinBin;
        /// <summary>
        /// Minimum bin for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        public int MinBin
        {
            get { return _MinBin; }
            set
            {
                _MinBin = value;
                this.NotifyOfPropertyChange(() => this.MinBin);

                // This will change the size of the plot
                // So remove the plot
                ClearPlot();

                // Send an event that the options were updated
                _options.MinBin = value;
                UpdateOptions();

                // Add the plot back
                AddSeries(_options);
            }
        }

        /// <summary>
        /// Minimum bin for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        private int _MaxBin;
        /// <summary>
        /// Minimum bin for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        public int MaxBin
        {
            get { return _MaxBin; }
            set
            {
                _MaxBin = value;
                this.NotifyOfPropertyChange(() => this.MaxBin);

                // This will change the size of the plot
                // So remove the plot
                ClearPlot();

                // Send an event that the options were updated
                _options.MaxBin = value;
                UpdateOptions();

                // Add the plot back
                AddSeries(_options);
            }
        }

        #endregion

        #region Interperlate

        /// <summary>
        /// Minimum bin for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        private bool _Interperlate;
        /// <summary>
        /// Minimum bin for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        public bool Interperlate
        {
            get { return _Interperlate; }
            set
            {
                _Interperlate = value;
                this.NotifyOfPropertyChange(() => this.Interperlate);

                // This will change the size of the plot
                // So remove the plot
                //ClearPlot();

                // Send an event that the options were updated
                _options.Interperlate = value;
                UpdateOptions();

                // Add the plot back
                //AddSeries(_options);

                // Set the flag to all the series
                SetInterperlate(value);
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to clear the plot.
        /// </summary>
        public ReactiveCommand<object> ClearPlotCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public HeatmapPlotViewModel(HeatmapPlotSeries.HeatmapPlotType type, HeatmapSeriesOptions options = null)
            : base(HeatmapPlotSeries.GetTitle(type))
        {
            // Initialize the plot
            ID = this.GetHashCode().ToString();
            _eventAggregator = IoC.Get<IEventAggregator>();
            _SelectedSeriesType = type;

            // Set the options
            SetOptions(options);

            // Create the plot
            _plot = CreatePlot(_SelectedSeriesType);
            SetColorPlotAxis();

            // Create List
            CreateLists();

            _isFilterData = true;
            _isProcessingBuffer = false;
            IsProfileSeriesOn = true;
            IsBtRangeSeriesOn = true;

            // Close the VM
            ClearPlotCommand = ReactiveCommand.Create();
            ClearPlotCommand.Subscribe(_ => ClearPlot());
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {

        }

        #region Incoming Data

        /// <summary>
        /// Add data to the plot to update the plot time series.
        /// </summary>
        /// <param name="ensemble">Latest data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        public async Task AddIncomingData(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            EnsWithMax ewm = new EnsWithMax();
            ewm.Ensemble = ensemble;
            ewm.MaxEnsembles = maxEnsembles;

            _buffer.Enqueue(ewm);

            // Execute async
            if (!_isProcessingBuffer)
            {
                // Execute async
                await Task.Run(() => AddIncomingDataExecute(ewm));
            }
        }

        /// <summary>
        /// Add the bulk data to the plot to update the plot time series.
        /// </summary>
        /// <param name="ensemble">Latest data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        public void AddIncomingDataBulk(Cache<long, DataSet.Ensemble> ensembles, Subsystem subsystem, SubsystemDataConfig ssConfig)
        {
            for (int y = 0; y < ensembles.Count(); y++)
            {
                EnsWithMax ewm = new EnsWithMax();
                ewm.Ensemble = ensembles.IndexValue(y);
                ewm.MaxEnsembles = ensembles.Count();

                if (ewm != null)
                {
                    // Verify the subsystem matches this viewmodel's subystem.
                    if ((subsystem == ewm.Ensemble.EnsembleData.GetSubSystem())                 // Check if Subsystem matches 
                            && (ssConfig == ewm.Ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
                    {
                        // Update the list with the latest ensemble
                        UpdateEnsembleList(ewm.Ensemble, ewm.MaxEnsembles);

                        // Update the plots in the dispatcher thread
                        try
                        {
                            // Lock the plot for an update
                            lock (Plot.SyncRoot)
                            {
                                // Update the time series with the latest data
                                for (int x = 0; x < Plot.Series.Count; x++)
                                {
                                    // Heatmap Plot Series
                                    if (Plot.Series[x].GetType() == typeof(HeatmapPlotSeries))
                                    {
                                        // Update the series
                                        ((HeatmapPlotSeries)Plot.Series[x]).UpdateSeries(ewm.Ensemble, ewm.MaxEnsembles, MinBin, MaxBin, _isFilterData, IsBottomTrackLine);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // When shutting down, can get a null reference
                            log.Debug("Error updating Time Series Plot", ex);
                        }
                    }
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }


        /// <summary>
        /// Add the incoming data async.
        /// </summary>
        /// <param name="param">Ensemble and Max ensembles.</param>
        private void AddIncomingDataExecute(object param)
        {
            lock (_UpdatePlotLock)
            {
                while (_buffer.Count > 0)
                {
                    _isProcessingBuffer = true;

                    EnsWithMax ewm = null;
                    if (_buffer.TryDequeue(out ewm))
                    {
                        if (ewm != null)
                        {
                            // Update the list with the latest ensemble
                            UpdateEnsembleList(ewm.Ensemble, ewm.MaxEnsembles);

                            // Update the plots in the dispatcher thread
                            try
                            {
                                //Application.Current.Dispatcher.BeginInvoke(new Action(() => UpdateSeries(ensemble, maxEnsembles)));
                                UpdateSeries(ewm.Ensemble, ewm.MaxEnsembles);
                            }
                            catch (Exception ex)
                            {
                                // When shutting down, can get a null reference
                                log.Debug("Error updating HeatMap Series Plot", ex);
                            }
                        }
                    }
                }
            }

            _isProcessingBuffer = false;
        }

        /// <summary>
        /// Clear all the line series.
        /// </summary>
        public void ClearIncomingData()
        {
            // Clear the plot
            ClearPlot();

            // Clear the list options
            ClearListOptions();

            // Clear the list of ensembles
            _ensembleList.Clear();
        }

        #endregion

        #region Create Plot

        /// <summary>
        /// Create the plot.  This will create a base plot.
        /// Then based off the series type, it will add the
        /// series type specifics to the plot.  This includes
        /// the axis labels and min and max values.
        /// </summary>
        /// <param name="type">Series type.</param>
        /// <returns>Plot created based off the series type.</returns>
        private PlotModel CreatePlot(HeatmapPlotSeries.HeatmapPlotType type)
        {
            PlotModel temp = CreatePlot();

            // Set the plot title
            temp.Title = _SelectedSeriesType.ToString();
            Title = _SelectedSeriesType.ToString();

            // Set the axis
            SetPlotAxis(ref temp);

            return temp;
        }

        #region Create Plot

        /// <summary>
        /// Create the plot.  Set the settings for the plot.
        /// </summary>
        /// <returns>Plot created.</returns>
        private PlotModel CreatePlot()
        {
            PlotModel temp = new PlotModel();

            temp.TextColor = OxyColors.White;
            temp.Background = OxyColors.Black;

            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.HighColor = OxyColors.Black;
            linearColorAxis1.LowColor = OxyColors.Black;
            linearColorAxis1.Palette = OxyPalettes.Rainbow(64);
            linearColorAxis1.Position = AxisPosition.Right;
            linearColorAxis1.Minimum = 0.0;
            linearColorAxis1.Tag = TAG_COLOR_AXIS;
            temp.Axes.Add(linearColorAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
            linearAxis2.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
            linearAxis2.Unit = "Ensembles";
            linearAxis2.Key = AxisPosition.Bottom.ToString();
            temp.Axes.Add(linearAxis2);

            temp.LegendPosition = LegendPosition.BottomCenter;

            return temp;
        }

        #endregion

        #region Axis

        /// <summary>
        /// Set the plot axis values (label and min/max) based
        /// off the series type.
        /// </summary>
        /// <param name="temp">Plot Model to set the axis.</param>
        private void SetPlotAxis(ref PlotModel temp)
        {
            // Left axis
            temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, "bin"));

            switch (_SelectedSeriesType)
            {
                case HeatmapPlotSeries.HeatmapPlotType.Amplitude:                 // Amplitude data
                    if (_options == null)
                    {
                        MinValue = 0.0;
                        MaxValue = 120.0;
                    }          
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Correlation:               // Correlation data
                    if (_options == null)
                    {
                        MinValue = 0;
                        MaxValue = 100;
                    }
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Magnitude:                // Magnitude
                case HeatmapPlotSeries.HeatmapPlotType.Beam_0_Vel:                              // Beam 0
                case HeatmapPlotSeries.HeatmapPlotType.Beam_1_Vel:                              // Beam 1
                case HeatmapPlotSeries.HeatmapPlotType.Beam_2_Vel:                              // Beam 2
                case HeatmapPlotSeries.HeatmapPlotType.Beam_3_Vel:                              // Beam 3
                case HeatmapPlotSeries.HeatmapPlotType.Earth_East_Vel:                          // East Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Earth_North_Vel:                         // North Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Vertical_Vel:                      // Vertical Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Error_Vel:                         // Error Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_X_Vel:                             // X Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Y_Vel:                             // Y Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Z_Vel:                             // Z Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Error_Vel:                         // Error Velocity
                    if (_options == null)
                    {
                        MinValue = 0.0;
                        MaxValue = 2.0;
                    }
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Direction:                // Direction
                    if (_options == null)
                    {
                        MinValue = -180.0;
                        MaxValue = 180.0;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set the color plot axis values.
        /// </summary>
        private void SetColorPlotAxis()
        {
            if(Plot == null)
            {
                return;
            }

            switch (_SelectedSeriesType)
            {
                case HeatmapPlotSeries.HeatmapPlotType.Amplitude:                               // Amplitude
                    Plot.Axes.Add(CreateColorAxis(AxisPosition.Right, SelectedPalette, MinValue, MaxValue, "dB", ColorAxisMajorStep));
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Correlation:                             // Correlation
                    Plot.Axes.Add(CreateColorAxis(AxisPosition.Right, SelectedPalette, MinValue, MaxValue, "%", ColorAxisMajorStep));
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Magnitude:                // Magnitude
                case HeatmapPlotSeries.HeatmapPlotType.Beam_0_Vel:                              // Beam 0
                case HeatmapPlotSeries.HeatmapPlotType.Beam_1_Vel:                              // Beam 1
                case HeatmapPlotSeries.HeatmapPlotType.Beam_2_Vel:                              // Beam 2
                case HeatmapPlotSeries.HeatmapPlotType.Beam_3_Vel:                              // Beam 3
                case HeatmapPlotSeries.HeatmapPlotType.Earth_East_Vel:                          // East Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Earth_North_Vel:                         // North Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Vertical_Vel:                      // Vertical Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Error_Vel:                         // Error Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_X_Vel:                             // X Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Y_Vel:                             // Y Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Z_Vel:                             // Z Velocity
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Error_Vel:                         // Error Velocity
                    Plot.Axes.Add(CreateColorAxis(AxisPosition.Right, SelectedPalette, MinValue, MaxValue, "m/s", ColorAxisMajorStep));
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Direction:                // Direction
                    Plot.Axes.Add(CreateColorAxis(AxisPosition.Right, SelectedPalette, MinValue, MaxValue, "deg", ColorAxisMajorStep));
                    break;
                default:
                    break; ;
            }


            // After the Palette have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

        /// <summary>
        /// Create the plot axis.  Set the values for the plot axis.
        /// If you do not want to set a value, set the value to NULL.
        /// </summary>
        /// <param name="position">Position of the axis.</param>
        /// <param name="majorStep">Minimum value.</param>
        /// <param name="unit">Label for the axis.</param>
        /// <returns>LinearAxis for the plot.</returns>
        private LinearAxis CreatePlotAxis(AxisPosition position, string unit)
        {
            // Create the axis
            LinearAxis axis = new LinearAxis();

            // Standard options
            axis.TicklineColor = OxyColors.White;
            axis.MajorGridlineStyle = LineStyle.Solid;
            axis.MinorGridlineStyle = LineStyle.Solid;
            axis.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
            axis.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
            axis.EndPosition = 0;
            axis.StartPosition = 1;
            axis.Position = position;
            axis.Key = position.ToString();

            // Set the axis label
            axis.Unit = unit;

            return axis;
        }

        /// <summary>
        /// Create the plot axis.  Set the values for the plot axis.
        /// If you do not want to set a value, set the value to NULL.
        /// </summary>
        /// <param name="position">Position of the axis.</param>
        /// <param name="palette">Color Palette.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        /// <param name="unit">Label for the axis.</param>
        /// <returns>Color linearAxis for the plot.</returns>
        private LinearAxis CreateColorAxis(AxisPosition position, OxyPalette palette, double minValue, double maxValue, string unit, double majorStep)
        {
            // Remove the old color axis
            if (Plot != null)
            {
                int index = -1;
                for (int x = 0; x < Plot.Axes.Count; x++)
                {
                    string tag = (string)Plot.Axes[x].Tag;
                    if (tag != null && tag.Equals(TAG_COLOR_AXIS))
                    {
                        index = x;
                    }
                }
                if (index >= 0)
                {
                    Plot.Axes.RemoveAt(index);
                }
            }

            // Create the axis
            var axis = new LinearColorAxis();

            axis.Palette = palette;
            axis.HighColor = OxyColors.Black;
            axis.LowColor = OxyColors.Black;
            axis.Minimum = minValue;
            axis.Maximum = MaxValue;
            axis.Position = position;
            axis.Tag = TAG_COLOR_AXIS;
            axis.Unit = unit;
            axis.MajorStep = majorStep;
            

            return axis;
        }

        /// <summary>
        /// Check if the plot series contains any Heading,
        /// Pitch or Roll series.  If it does, then the plot
        /// axis will be screwed up, so allow the plot to generate
        /// the axis labels.
        /// </summary>
        /// <returns>TRUE = Plot contains a Heading, Pitch or Roll series.</returns>
        private bool HeadingPitchOrRollSeriesExist()
        {
            foreach (TimeSeries series in Plot.Series)
            {
                if (series.Type.Type.Code == BaseSeriesType.eBaseSeriesType.Base_Heading ||
                   series.Type.Type.Code == BaseSeriesType.eBaseSeriesType.Base_Pitch ||
                    series.Type.Type.Code == BaseSeriesType.eBaseSeriesType.Base_Roll)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region Update Plot

        /// <summary>
        /// Update the list with the latest ensemble.  Also ensure the list is the correct size.
        /// </summary>
        /// <param name="ens">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points for a series.</param>
        private void UpdateEnsembleList(DataSet.Ensemble ens, int maxEnsembles)
        {
            // Keep the list at the correct size
            if (_ensembleList.Count > 0 && _ensembleList.Count > maxEnsembles)
            {
                _ensembleList.RemoveAt(0);
            }

            // Add the latest ensemble to the list
            _ensembleList.Add(ens);
        }

        /// <summary>
        /// Update the plot with the latest ensemble data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of points in the line series.</param>
        private void UpdateSeries(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Update the time series with the latest data
                for (int x = 0; x < Plot.Series.Count; x++ )
                {
                    // Heatmap Plot Series
                    if (Plot.Series[x].GetType() == typeof(HeatmapPlotSeries))
                    {
                        // Update the series
                        ((HeatmapPlotSeries)Plot.Series[x]).UpdateSeries(ensemble, maxEnsembles, MinBin, MaxBin, _isFilterData, IsBottomTrackLine);
                    }
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region Clear Plot

        /// <summary>
        /// Clear the plot of all the data.
        /// </summary>
        private void ClearPlot()
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                foreach(HeatmapPlotSeries series in Plot.Series)
                {
                    series.ClearSeries();
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region List Options

        ///// <summary>
        ///// Setup the list of options available to the user based off the
        ///// series type.  This will enable and disable list and populate the
        ///// list with the correct values based off the series type.
        ///// </summary>
        ///// <param name="seriesType">Series type.</param>
        //private void SetupListOptions(BaseSeriesType seriesType)
        //{
        //    // Setup the Data Source list
        //    DataSourceList = DataSource.GetDataSourceList();

        //    // Setup the Coordinate Transform list
        //    CoordinateTransformList = Core.Commons.GetTransformList();

        //    //// Set the DataSet List
        //    //DataSetTypeList = BaseSeriesType.GetDataSetTypeList(seriesType.Code);
        //    //if (DataSetTypeList.Count > 0)
        //    //{
        //    //    SelectedDataSetType = DataSetTypeList[0];
        //    //}

        //    // Set the Subsystem config list
        //    SubsystemConfigList = new ObservableCollection<AdcpSubsystemConfig>();

        //    // Set the max bin
        //    MaxBin = DEFAULT_MAX_BIN;
        //    SelectedBin = 0;
        //    MaxBeam = DEFAULT_MAX_BEAM - 1;
        //    SelectedBeam = 0;

        //    // Set the list of colors
        //    SeriesColorsList = BeamColor.GetBeamColorList();

        //    // Set the default selected color
        //    if (SeriesColorsList.Count > 0)
        //    {
        //        SelectedSeriesColor = SeriesColorsList[0];
        //    }
        //}

        ///// <summary>
        ///// Update all the list based off the new ensemble data given.
        ///// </summary>
        ///// <param name="ensemble">Latest ensemble data.</param>
        //private void UpdateListOptions(DataSet.Ensemble ensemble)
        //{
        //    // Update the Subsystem Configuration list
        //    // Create a new AdcpSubsystemConfig and check if we need to add it to the list
        //    // Set the CEPO index to 0
        //    AdcpSubsystemConfig config = new AdcpSubsystemConfig(ensemble.EnsembleData.SubsystemConfig);
        //    if (!SubsystemConfigList.Contains(config))
        //    {
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => SubsystemConfigList.Add(config)));
        //    }

        //    // Update the max bin
        //    if (ensemble.EnsembleData.NumBins > _maxBin)
        //    {
        //        // Subtract 1 because it is zero based
        //        MaxBin = ensemble.EnsembleData.NumBins - 1;
        //    }

        //    // Update the max beam
        //    if (ensemble.EnsembleData.NumBeams > _maxBeam)
        //    {
        //        // Subtract 1 because it is zero based
        //        MaxBeam = ensemble.EnsembleData.NumBeams - 1;
        //    }
        //}

        /// <summary>
        /// Clear the list.  This will reset all the list 
        /// back to default.
        /// </summary>
        private void ClearListOptions()
        {
            //SetupListOptions(_SelectedSeriesType.Type);
        }

        #endregion

        #region Add Series

        public void AddSeries(HeatmapSeriesOptions options)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Add the series to the list
                // Create a series
                HeatmapPlotSeries series = new HeatmapPlotSeries(options.Type, options.MinBin, options.MaxBin, options.IsFilterData, options.IsBottomTrackLine, _ensembleList, options.Interperlate);
                series.Tag = TAG_HEATMAP_SERIES;
                series.IsVisible = _IsProfileSeriesOn;
                //series.XAxisKey = AxisPosition.Left.ToString();
                //series.YAxisKey = AxisPosition.Bottom.ToString();
                Plot.Series.Add(series);
            }

            // Then refresh the plot
            Plot.InvalidatePlot(true);

            // Set the options
            SetOptions(options);

            // Set Default series option types
            SetDefaultSeriesOptions();
        }

        #endregion

        #region Interperlate

        /// <summary>
        /// Set the interperlate flag based off the user settings.
        /// </summary>
        /// <param name="flag">Flag to interperlate to blend.</param>
        public void SetInterperlate(bool flag)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                foreach(HeatmapPlotSeries series in Plot.Series)
                {
                    series.Interpolate = flag;
                }
            }

            // Then refresh the plot
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region Lists

        private void CreateLists()
        {
            _ensembleList = new LimitedList<DataSet.Ensemble>();
            _buffer = new ConcurrentQueue<EnsWithMax>();

            PaletteList = new BindingList<OxyPalette>();
            PaletteList.Add(OxyPalettes.BlackWhiteRed(64));
            PaletteList.Add(OxyPalettes.BlueWhiteRed(64));
            PaletteList.Add(OxyPalettes.BlueWhiteRed31);
            PaletteList.Add(OxyPalettes.Cool(64));
            PaletteList.Add(OxyPalettes.Gray(64));
            PaletteList.Add(OxyPalettes.Hot(64));
            PaletteList.Add(OxyPalettes.Hue64);
            PaletteList.Add(OxyPalettes.HueDistinct(64));
            PaletteList.Add(OxyPalettes.Jet(64));
            PaletteList.Add(OxyPalettes.Rainbow(64));
        }

        #endregion

        #region Options

        /// <summary>
        /// Set the options.  If no options are given, set the default options.
        /// </summary>
        /// <param name="options">Options to set.</param>
        private void SetOptions(HeatmapSeriesOptions options)
        {
            if (options == null)
            {
                _options = new HeatmapSeriesOptions();
                SelectedPalette = OxyPalettes.Hue64;
                MinValue = _options.MinValue;
                MaxValue = _options.MaxValue;
            }
            else
            {
                // Set the new options
                _options = options;

                // Set the color palette
                if (options.Palette.Count > 0)
                {
                    SelectedPalette = HeatmapSeriesOptions.ListToPalette(options.Palette);
                }
                else
                {
                    SelectedPalette = OxyPalettes.Hue64;
                }

                // Set after the series has been added
                IsFilterData = options.IsFilterData;
                MinValue = options.MinValue;
                MaxValue = options.MaxValue;
                _MinBin = options.MinBin;
                _MaxBin = options.MaxBin;
                ColorAxisMajorStep = options.ColorAxisMajorStep;
                IsBottomTrackLine = options.IsBottomTrackLine;
            }

            _SelectedSeriesType = _options.Type;
            SeriesTypeList = HeatmapPlotSeries.PlotTypeList;
        }

        private void SetDefaultSeriesOptions()
        {
            switch (_SelectedSeriesType)
            {
                case HeatmapPlotSeries.HeatmapPlotType.Amplitude:
                    MinValue = 0;
                    MaxValue = 120;
                    ColorAxisMajorStep = 20;
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Beam_0_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Beam_1_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Beam_2_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Beam_3_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Earth_East_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Earth_North_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Vertical_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Instr_X_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Y_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Z_Vel:
                    MinValue = -2.0;
                    MaxValue = 2.0;
                    ColorAxisMajorStep = 0.2;
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Error_Vel:
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Error_Vel:
                    MinValue = -0.2;
                    MaxValue = 0.2;
                    ColorAxisMajorStep = 0.1;
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Correlation:
                    MinValue = 0;
                    MaxValue = 100;
                    ColorAxisMajorStep = 20;
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Direction:
                    MinValue = 0;
                    MaxValue = 360;
                    ColorAxisMajorStep = 20;
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Magnitude:
                    MinValue = 0;
                    MaxValue = 5;
                    ColorAxisMajorStep = 0.5;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Update Series Visibility

        /// <summary>
        /// Update the series visibilty.  The given tag is the
        /// series tag.  The value will turn on or off the 
        /// visibility.
        /// </summary>
        /// <param name="tag">The tag of the series.</param>
        /// <param name="value">TRUE = On, FALSE = OFF.</param>
        private void UpdateSeriesVisibility(object tag, bool value)
        {
            foreach (var series in Plot.Series)
            {
                // Look for the series that matches
                if (series.Tag == tag)
                {
                    series.IsVisible = value;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void UpdateOptionsEventHandler(HeatmapSeriesOptions options);

        /// <summary>
        /// Subscribe to when options are updated.
        /// 
        /// To subscribe:
        /// tsVM.UpdateOptionsEvent += new tsVM.UpdateOptionsEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// tsVM.UpdateOptionsEvent -= (method to call)
        /// </summary>
        public event UpdateOptionsEventHandler UpdateOptionsEvent;

        private void UpdateOptions()
        {
            if(UpdateOptionsEvent != null)
            {
                UpdateOptionsEvent(_options);
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// Return the description as the string for this object.
        /// </summary>
        /// <returns>Return the description as the string for this object.</returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }
}
