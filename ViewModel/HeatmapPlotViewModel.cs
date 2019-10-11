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
 * 02/02/2018      RC          4.7.2       Added new default for plot as OxyPalettes.Jet(64).
 * 02/07/2018      RC          4.7.2       Added MaxEnsemble to AddIncomingDataBulk() to allow a greater number then in cache.
 * 03/23/2018      RC          4.8.0       Updated Heatmap plot with bottom track line and shade under bottom track line. 
 * 08/10/2018      RC          4.10.2      Fixed InterPlote flag to only change HeatmapPlotSeries.
 * 10/10/2019      RC          4.11.3      Fixed UpdateMeterAxis() max value.
 *                                         Added SetUpwardOrDownwardPlotAxis() to flip axis based on upward or downward looking.
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
        /// Axis label in meters.
        /// </summary>
        public const string AXIS_LABEL_METERS = "meters";

        /// <summary>
        /// Axis label in bins.
        /// </summary>
        public const string AXIS_LABEL_BINS = "bins";

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
        /// <param name="ensembles">Latest data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display. If set to 0, use cache size.</param>
        /// <param name="ssConfig">Subsystem config.</param>
        /// <param name="subsystem">Subsystem type.</param>
        public void AddIncomingDataBulk(Cache<long, DataSet.Ensemble> ensembles, Subsystem subsystem, SubsystemDataConfig ssConfig, int maxEnsembles = 0)
        {
            for (int y = 0; y < ensembles.Count(); y++)
            {
                EnsWithMax ewm = new EnsWithMax();
                ewm.Ensemble = ensembles.IndexValue(y);

                // Max ensemble set by user or use the cache size
                if (maxEnsembles == 0)
                {
                    ewm.MaxEnsembles = ensembles.Count();
                }
                else
                {
                    ewm.MaxEnsembles = maxEnsembles;
                }

                if (ewm != null)
                {
                    // Verify the subsystem matches this viewmodel's subystem.
                    if ((subsystem == ewm.Ensemble.EnsembleData.GetSubSystem())                 // Check if Subsystem matches 
                            && (ssConfig == ewm.Ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
                    {
                        // Update the list with the latest ensemble
                        //UpdateEnsembleList(ewm.Ensemble, ewm.MaxEnsembles);

                        // Update the plots in the dispatcher thread
                        try
                        {
                            // Lock the plot for an update
                            lock (Plot.SyncRoot)
                            {
                                // Check if plot is upward or downward ADCP
                                SetUpwardOrDownwardPlotAxis(ewm.Ensemble);

                                // Update the meter axis
                                UpdateMeterAxis(ewm.Ensemble);

                                int ensPlotCount = 0;

                                // Update the time series with the latest data
                                for (int x = 0; x < Plot.Series.Count; x++)
                                {
                                    // Heatmap Plot Series
                                    if (Plot.Series[x].GetType() == typeof(HeatmapPlotSeries))
                                    {
                                        // Update the series
                                        ((HeatmapPlotSeries)Plot.Series[x]).UpdateSeries(ewm.Ensemble, ewm.MaxEnsembles, MinBin, MaxBin, _isFilterData, IsBottomTrackLine);

                                        // Get the total number of ensembles plotted
                                        // Zero based, so subtract 1
                                        ensPlotCount = ((HeatmapPlotSeries)Plot.Series[x]).Data.GetLength(0) - 1;
                                    }

                                    // Add Bottom Track line
                                    if (Plot.Series[x].GetType() == typeof(AreaSeries))
                                    {
                                        // Add the Bottom Track line data
                                        AddBottomTrackData(x, ewm.Ensemble, ensPlotCount, ewm.MaxEnsembles);
                                        //((LineSeries)Plot.Series[x]).YAxisKey = AXIS_LABEL_METERS;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // When shutting down, can get a null reference
                            log.Debug("Error updating Heatmap Plot", ex);
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

            // Color option
            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.HighColor = OxyColors.Black;
            linearColorAxis1.LowColor = OxyColors.Black;
            linearColorAxis1.Palette = OxyPalettes.Jet(64);
            linearColorAxis1.Position = AxisPosition.Right;
            linearColorAxis1.Minimum = 0.0;
            linearColorAxis1.Tag = TAG_COLOR_AXIS;
            temp.Axes.Add(linearColorAxis1);

            // Bottom Axis 
            // Ensembles 
            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
            linearAxis2.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
            linearAxis2.Unit = "Ensembles";
            //linearAxis2.Key = AxisPosition.Bottom.ToString();
            linearAxis2.Key = "Ensembles";
            temp.Axes.Add(linearAxis2);

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
            // Left axis in Bins
            temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, AXIS_LABEL_BINS));

            // Right axis in Meters
            temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, AXIS_LABEL_METERS, 2));

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
        private LinearAxis CreatePlotAxis(AxisPosition position, string unit, int positionTier = 0)
        {
            // Create the axis
            LinearAxis axis = new LinearAxis();

            // Standard options
            axis.TicklineColor = OxyColors.White;
            axis.MajorGridlineStyle = LineStyle.Solid;
            axis.MinorGridlineStyle = LineStyle.Solid;
            axis.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
            axis.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
            axis.EndPosition = 0;                                                   // 0 = Downward looking
            axis.StartPosition = 1;                                                 // 1 = Downward looking
            axis.Position = position;
            axis.Key = unit;
            axis.PositionTier = positionTier;

            // Set the axis label
            axis.Unit = unit;

            return axis;
        }

        /// <summary>
        /// Update the Meter axis with the latest ensemble data.
        /// Set the minimum to the blank.
        /// Set the maximum to either the max number of bins or the max depth.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the latest settings.</param>
        private void UpdateMeterAxis(DataSet.Ensemble ensemble)
        {
            float min = 0.0f;
            float max = 1.0f;

            if(ensemble.IsAncillaryAvail && ensemble.IsEnsembleAvail)
            {
                // Use blank for min
                min = ensemble.AncillaryData.FirstBinRange;

                // Max is blank + NumBins * BinSize
                max = ensemble.AncillaryData.FirstBinRange + (ensemble.AncillaryData.BinSize * ensemble.EnsembleData.NumBins);

            }

            //if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail && ensemble.IsEnsembleAvail)
            //{
            //    // Update the bottom track line series
            //    int rangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);

            //    // Update the meter axis with the largest depth
            //    if (rangeBin > ensemble.EnsembleData.NumBins)
            //    {

            //        // Use the bottom track depth for max
            //        max = ensemble.BottomTrackData.GetAverageRange();
            //    }
            //    else
            //    {
            //        // Use the total number of bins for the max
            //        max = ensemble.AncillaryData.GetBinToDepth(ensemble.EnsembleData.NumBins);
            //    }
            //}


            // Set the min and max
            for (int x = 0; x < Plot.Axes.Count; x++)
            {
                // Set the Minimum and Maxmimum for the axis
                //if(AXIS_LABEL_METERS.Equals(Plot.Axes[x].Key))
                if(Plot.Axes[x].Key == AXIS_LABEL_METERS)
                {
                    Plot.Axes[x].Minimum = min;
                    Plot.Axes[x].Maximum = max;
                }
            }

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
                // Update the meter axis
                UpdateMeterAxis(ensemble);

                // Check if Upward or Downward and set plot axis
                SetUpwardOrDownwardPlotAxis(ensemble);

                int plotEnsCount = 0;

                // Update the time series with the latest data
                for (int x = 0; x < Plot.Series.Count; x++)
                {
                    // Heatmap Plot Series
                    if (Plot.Series[x].GetType() == typeof(HeatmapPlotSeries))
                    {
                        // Update the series
                        ((HeatmapPlotSeries)Plot.Series[x]).UpdateSeries(ensemble, maxEnsembles, MinBin, MaxBin, _isFilterData, IsBottomTrackLine);
                        //((HeatmapPlotSeries)Plot.Series[x]).YAxisKey = AXIS_LABEL_METERS;

                        // Get the total number of ensembles plotted
                        // Zero based so subtract 1
                        plotEnsCount = ((HeatmapPlotSeries)Plot.Series[x]).Data.GetLength(0) - 1;
                    }

                    // Add Bottom Track line
                    if (Plot.Series[x].GetType() == typeof(AreaSeries))
                    {
                        // Add the Bottom Track line data
                        AddBottomTrackData(x, ensemble, plotEnsCount, maxEnsembles);
                        //((LineSeries)Plot.Series[x]).YAxisKey = AXIS_LABEL_METERS;
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
                foreach(var series in Plot.Series)
                {
                    if (series.GetType() == typeof(HeatmapPlotSeries))
                    {
                        ((HeatmapPlotSeries)series).ClearSeries();
                    }
                    else if(series.GetType() == typeof(AreaSeries))
                    {
                        ((AreaSeries)series).Points.Clear();
                        ((AreaSeries)series).Points2.Clear();
                    }
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region List Options

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

        /// <summary>
        /// Add Heatmap series.  This will use the Ensemble number and the bins to populate the series.
        /// </summary>
        /// <param name="options">Heatmap series options.</param>
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

        /// <summary>
        /// Add Bottom Track line series.  This will be a line to mark the bottom.
        /// </summary>
        public void AddBtSeries()
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Add the series to the list
                // Create a series
                AreaSeries series = new AreaSeries()
                {
                    Color = OxyColors.Red,
                    Color2 = OxyColors.Transparent,
                    Fill = OxyColor.FromAColor(40, OxyColors.LightGray),
                //MarkerType = MarkerType.Circle,
                //MarkerSize = 3,
                //MarkerStroke = OxyColors.White,
                //MarkerFill = OxyColors.SkyBlue,
                //MarkerStrokeThickness = 1.5
            };
                series.Tag = "Bottom Track";
                //series.XAxisKey = AxisPosition.Left.ToString();
                //series.YAxisKey = AxisPosition.Bottom.ToString();
                Plot.Series.Add(series);
            }

            // Then refresh the plot
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region Update Bottom Track Data

        /// <summary>
        /// Add the Bottom Track data line.
        /// </summary>
        /// <param name="lineSeriesIndex">Line series index in the plot series list.</param>
        /// <param name="ensemble">Ensemble to add data.</param>
        /// <param name="ensembleCount">Number of profile ensembles displayed.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display in the plot.</param>
        private void AddBottomTrackData(int lineSeriesIndex, DataSet.Ensemble ensemble, int ensembleCount, int maxEnsembles)
        {
            if(ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail && ensemble.IsEnsembleAvail)
            {
                // Update the bottom track line series
                int rangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);

                // Only plot the range if it is found
                if (rangeBin > 0)
                {
                    // Create a new data point for the bottom track line
                    // This will be the (ensemble count, range bin)
                    ((AreaSeries)Plot.Series[lineSeriesIndex]).Points.Add(new DataPoint(ensembleCount, rangeBin));

                    // Add the second point for the shaded area
                    if (rangeBin < ensemble.EnsembleData.NumBins)
                    {
                        // Less then the number of bins, so go to the end of the number of bins
                        ((AreaSeries)Plot.Series[lineSeriesIndex]).Points2.Add(new DataPoint(ensembleCount, ensemble.EnsembleData.NumBins-1));
                    }
                    else
                    {
                        // This is the deepest point
                        ((AreaSeries)Plot.Series[lineSeriesIndex]).Points2.Add(new DataPoint(ensembleCount, rangeBin));
                    }

                    // Add 1 because zero based and include the max number
                    while (((AreaSeries)Plot.Series[lineSeriesIndex]).Points.Count > maxEnsembles + 1)
                    {
                        // Shift Points
                        ShiftBottomTrackLineSeries(lineSeriesIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Shift the bottom Track line points.  The plot is zero at the beginning always.  
        /// Remove the first entry.  Then adjust all the index values.
        /// This will adjust the X value (index).
        /// </summary>
        /// <param name="lineSeriesIndex"></param>
        private void ShiftBottomTrackLineSeries(int lineSeriesIndex)
        {
            // Remove the first point (0)
            ((AreaSeries)Plot.Series[lineSeriesIndex]).Points.RemoveAt(0);

            // Copy all the points
            List<DataPoint> cloneDP = new List<DataPoint>(((AreaSeries)Plot.Series[lineSeriesIndex]).Points);

            // Copy all the points2
            List<DataPoint> cloneDP2 = new List<DataPoint>(((AreaSeries)Plot.Series[lineSeriesIndex]).Points2);

            // Clear the original list
            ((AreaSeries)Plot.Series[lineSeriesIndex]).Points.Clear();
            ((AreaSeries)Plot.Series[lineSeriesIndex]).Points2.Clear();

            // Update the index for each point
            for (int x = 0; x < cloneDP.Count; x++)
            {
                DataPoint dp = cloneDP[x];                                                      // Get the data point
                DataPoint dp2 = cloneDP2[x];                                                    // Get the data point2
                dp.X = x;                                                                       // Change the index
                ((AreaSeries)Plot.Series[lineSeriesIndex]).Points.Add(dp);                      // Replace the point
                ((AreaSeries)Plot.Series[lineSeriesIndex]).Points2.Add(dp2);                      // Replace the point2
            }
        }

        #endregion

        #region Check Upward or Downward

        /// <summary>
        /// Check if the plot axis label needs to be reset for Upward or Downward.
        /// Set the axis StartPosition and EndPosition for upward or downward.
        /// 
        /// Downward:
        /// StartPosition = 1
        /// EndPosition = 0
        /// 
        /// Upward: 
        /// StartPosition = 0
        /// EndPosition = 1
        /// 
        /// 
        /// </summary>
        /// <param name="ens"></param>
        private void SetUpwardOrDownwardPlotAxis(DataSet.Ensemble ens)
        {
            if(ens.AncillaryData.IsUpwardFacing())
            {
                // Find the Plot axes for the bin and meter 
                for(int x = 0; x < Plot.Axes.Count; x++)
                {
                    // Upward should be 0 so reset
                    if(Plot.Axes[x].Key == AXIS_LABEL_BINS && Plot.Axes[x].StartPosition == 1)
                    {
                        Plot.Axes[x].StartPosition = 0;
                        Plot.Axes[x].EndPosition = 1;
                    }

                    // Upward should be 0 so reset
                    if (Plot.Axes[x].Key == AXIS_LABEL_METERS && Plot.Axes[x].StartPosition == 1)
                    {
                        Plot.Axes[x].StartPosition = 0;
                        Plot.Axes[x].EndPosition = 1;
                    }

                }
            }
            else
            {
                // Find the Plot axes for the bin and meter 
                for (int x = 0; x < Plot.Axes.Count; x++)
                {
                    // Downward StartPosition should be 1 so reset
                    if (Plot.Axes[x].Key == AXIS_LABEL_BINS && Plot.Axes[x].StartPosition == 0)
                    {
                        Plot.Axes[x].StartPosition = 1;
                        Plot.Axes[x].EndPosition = 0;
                    }

                    // Downward StartPosition should be 1 so reset
                    if (Plot.Axes[x].Key == AXIS_LABEL_METERS && Plot.Axes[x].StartPosition == 0)
                    {
                        Plot.Axes[x].StartPosition = 1;
                        Plot.Axes[x].EndPosition = 0;
                    }
                }
            }

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
                foreach(var series in Plot.Series)
                {
                    if (series.GetType() == typeof(HeatmapPlotSeries))
                    {
                        ((HeatmapPlotSeries)series).Interpolate = flag;
                    }
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
                SelectedPalette = OxyPalettes.Jet(64);
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
                    SelectedPalette = OxyPalettes.Jet(64);
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
                    MaxValue = 2;
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
