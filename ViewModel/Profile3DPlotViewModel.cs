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
 * 10/29/2019      RC          4.12.0      Initial coding
 * 
 */

namespace RTI
{
    using System.ComponentModel.Composition;
    using OxyPlot;
    using System.Windows.Input;
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using OxyPlot.Axes;
    using Caliburn.Micro;
    using ReactiveUI;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using OxyPlot.Series;
    using System.IO;

    /// <summary>
    /// Create ProfileSeries Plot control to display time series plots.
    /// </summary>
    [Export]
    public class Profile3DPlotViewModel : PulseViewModel
    {

        #region Variable

        #region Defaults

        /// <summary>
        /// Default height of the plot in pixels.
        /// </summary>
        private int DEFAULT_PLOT_HEIGHT = 425;

        /// <summary>
        /// Default width of the plot in pixels.
        /// </summary>
        private int DEFAULT_PLOT_WIDTH = 200;       // 420

        /// <summary>
        /// Default number for maximum number of beams.
        /// </summary>
        private int DEFAULT_MAX_BEAM = 4;

        /// <summary>
        /// Default number of maximum bins.
        /// </summary>
        private int DEFAULT_MAX_BIN = DataSet.Ensemble.MAX_NUM_BINS;

        /// <summary>
        /// Profile series tag.
        /// </summary>
        public const string TAG_PROFILE_SERIES = "Profile";

        /// <summary>
        /// Min series tag.
        /// </summary>
        public const string TAG_MIN_SERIES = "Min";

        /// <summary>
        /// Max series tag.
        /// </summary>
        public const string TAG_MAX_SERIES = "Max";

        /// <summary>
        /// Avg series tag.
        /// </summary>
        public const string TAG_AVG_SERIES = "Avg";

        /// <summary>
        /// Standard Deviation Ping to Ping series tag.
        /// </summary>
        public const string TAG_STDP2P_SERIES = "STD P2P";

        /// <summary>
        /// Standard Deviation Bin to Bin series tag.
        /// </summary>
        public const string TAG_STDB2B_SERIES = "STD B2B";

        #endregion

        /// <summary>
        /// Bin or Depth Axis Key.
        /// </summary>
        private string BIN_OR_DEPTH_AXIS = "BIN_AXIS";

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
        private LimitedList<DataSet.Ensemble> _ensembleList;

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<DataSet.Ensemble> _buffer;

        /// <summary>
        /// Flag to know if processing the buffer.
        /// </summary>
        private bool _isProcessingBuffer;


        /// <summary>
        /// Options for this ViewModel.
        /// </summary>
        private ViewDataGraphicalOptions _options;

        /// <summary>
        /// Pulse manager to manage the application.
        /// </summary>
        private PulseManager _pm;

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
            /// Maximum number of bins to display.
            /// </summary>
            public int MaxBins { get; set; }
        }

        #endregion

        #region Properties

        #region ID

        /// <summary>
        /// ID to identify this specific view model.
        /// </summary>
        public string ID { get; private set; }

        #endregion

        #region Type

        /// <summary>
        /// The series type of this plot.  This will determine
        /// what the axis labels and axis scales will be.
        /// </summary>
        private BaseSeriesType _seriesType;
        /// <summary>
        /// The series type of this plot.  This will determine
        /// what the axis labels and axis scales will be.
        /// </summary>
        public BaseSeriesType SeriesType
        {
            get { return _seriesType; }
            set
            {
                _seriesType = value;
                this.NotifyOfPropertyChange(() => this.SeriesType);
            }
        }

        #endregion

        #region Plot

        /// <summary>
        /// The plot for the view model.  This will be the plot
        /// that will be updated by the user.
        /// </summary>
        private BinPlot3D _plot;
        /// <summary>
        /// The plot for the view model.  This will be the plot
        /// that will be updated by the user.
        /// </summary>
        public BinPlot3D Plot
        {
            get { return _plot; }
            set
            {
                _plot = value;
                this.NotifyOfPropertyChange(() => this.Plot);
            }
        }

        /// <summary>
        /// A reference to the plot series for binding.
        /// </summary>
        private BindingList<OxyPlot.Series.Series> _PlotSeriesList;
        /// <summary>
        /// A reference to the plot series for binding.
        /// </summary>
        public BindingList<OxyPlot.Series.Series> PlotSeriesList
        {
            get { return _PlotSeriesList; }
            set
            {
                _PlotSeriesList = value;
                this.NotifyOfPropertyChange(() => this.PlotSeriesList);
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

        #region Options

        /// <summary>
        /// Colormap brush chosen.
        /// </summary>
        public ColormapBrush.ColormapBrushEnum ColormapBrushSelection
        {
            get { return _options.PlotColorMap; }
            set
            {
                _options.PlotColorMap = value;
                Plot.ColormapBrushSelection = value;

                this.NotifyOfPropertyChange(() => this.ColormapBrushSelection);

                // Update the database
                UpdateDatabaseOptions();
            }
        }

        /// <summary>
        /// Min velocity displayed in the Velocity ListBox.
        /// </summary>
        public double ContourMinValue
        {
            get { return _options.ContourMinimumValue; }
            set
            {
                _options.ContourMinimumValue = value;
                Plot.MinVelocity = value;

                this.NotifyOfPropertyChange(() => this.ContourMinValue);

                // Update the database
                UpdateDatabaseOptions();
            }
        }

        /// <summary>
        /// Max velocity displayed in the Velocity ListBox.
        /// </summary>
        public double ContourMaxValue
        {
            get { return _options.ContourMaximumValue; }
            set
            {
                _options.ContourMaximumValue = value;
                Plot.MaxVelocity = value;

                this.NotifyOfPropertyChange(() => this.ContourMaxValue);

                // Update the database
                UpdateDatabaseOptions();
            }
        }

        #endregion


        #endregion

        #region Commands

        /// <summary>
        /// Command to Remove the plot from the display.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> RemovePlotCommand { get; protected set; }

        /// <summary>
        /// Command to clear the plot.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearPlotCommand { get; protected set; }

        /// <summary>
        /// Command to add a series to the plot.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> AddSeriesCommand { get; protected set; }

        /// <summary>
        /// Command to remove a series from the plot.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> RemoveSeriesCommand { get; protected set; }

        /// <summary>
        /// Command to generate a report for the plot.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GenerateReportCommand { get; protected set; }
        
        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public Profile3DPlotViewModel()
            : base("3D Profile Plot VM")
        {
            // Initialize the plot
            //ID = DateTime.Now.ToString();                   // ID will the be the current date and time as a string
            ID = this.GetHashCode().ToString();
            _eventAggregator = IoC.Get<IEventAggregator>();
            //_seriesType = seriesType;
            //_plot = CreatePlot(_seriesType);
            //PlotSeriesList = new BindingList<OxyPlot.Series.Series>();
            //Title = _seriesType.ToString();
            //_ensembleList = new LimitedList<DataSet.Ensemble>();
            //_isFilterData = true;
            //_isProcessingBuffer = false;
            //_buffer = new ConcurrentQueue<EnsWithMax>();
            //_minMaxAvgStdSeriesList = new List<MinMaxAvgStdSeries>();
            //IsProfileSeriesOn = true;
            //IsMinSeriesOn = false;
            //IsMaxSeriesOn = false;
            //IsAvgSeriesOn = false;
            //IsStdP2PSeriesOn = false;
            //IsStdB2BSeriesOn = false;

            //PlotHeight = DEFAULT_PLOT_HEIGHT;
            //PlotWidth = DEFAULT_PLOT_WIDTH;

            _pm = IoC.Get<PulseManager>();

            // Get the options from the database
            GetOptionsFromDatabase();

            _plot = new BinPlot3D();
            _plot.CylinderRadius = 0;
            _plot.ColormapBrushSelection = ColormapBrushSelection;
            _plot.MinVelocity = ContourMinValue;
            _plot.MaxVelocity = ContourMaxValue;



        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {

        }

        #region Methods

        #region Options

        /// <summary>
        /// Get the options for this subsystem display
        /// from the database.  If the options have not
        /// been set to the database yet, default values 
        /// will be used.
        /// </summary>
        private void GetOptionsFromDatabase()
        {
            //var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);
            //_options = _pm.AppConfiguration.GetGraphicalOptions(ssConfig);

            //// Notify all the properties
            //NotifyOptionPropertyChange();

            _options = _pm.GetGraphicalViewOptions();

            // Notify all the properties
            NotifyOptionPropertyChange();
        }

        /// <summary>
        /// Notify all the properties of a change
        /// when a new option object is set.
        /// </summary>
        private void NotifyOptionPropertyChange()
        {
            //// Pass the Contour Plot options to the contour plot
            //if (_ContourPlot != null)
            //{
            //    _ContourPlot.ColormapBrushSelection = _options.PlotColorMap;
            //    _ContourPlot.MaxEnsembles = _options.DisplayMaxEnsembles;
            //    _ContourPlot.MinValue = _options.ContourMinimumValue;
            //    _ContourPlot.MaxValue = _options.ContourMaximumValue;
            //}
        }

        /// <summary>
        /// Update the database with the latest options.
        /// </summary>
        private void UpdateDatabaseOptions()
        {
            // SubsystemDataConfig needs to be converted to a SubsystemConfiguration
            // because the SubsystemConfig will be compared in AppConfiguration to determine
            // where to save the settings.  Because SubsystemDataConfig and SubsystemConfiguration
            // are not the same type, it will not pass Equal()
            //_pm.UpdateBackscatterOptions(_options);
        }

        #endregion


        public async void AddIncomingData(DataSet.EnsembleVelocityVectors vv)
        {
            await Plot.AddIncomingData(vv);
        }

        public void ClearIncomingData()
        {
            Plot.ClearIncomingData();
        }

        //#region Incoming Data

        ///// <summary>
        ///// Add data to the plot to update the plot time series.
        ///// </summary>
        ///// <param name="ensemble">Latest data.</param>
        ///// <param name="maxBins">Maximum number of bins to display.</param>
        //public async Task AddIncomingData(DataSet.Ensemble ensemble, int maxBins)
        //{
        //    EnsWithMax ewm = new EnsWithMax();
        //    ewm.Ensemble = ensemble;
        //    ewm.MaxBins = maxBins;

        //    _buffer.Enqueue(ewm);

        //    // Execute async
        //    if (!_isProcessingBuffer)
        //    {
        //        await Task.Run(() => AddIncomingDataExecute(ewm));
        //    }
        //}

        ///// <summary>
        ///// Add the bulk data to the plot to update the plot time series.
        ///// </summary>
        ///// <param name="ensemble">Latest data.</param>
        ///// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        //public void AddIncomingDataBulk(Cache<long, DataSet.Ensemble> ensembles, Subsystem subsystem, SubsystemDataConfig ssConfig)
        //{
        //    for (int x = 0; x < ensembles.Count(); x++)
        //    {
        //        EnsWithMax ewm = new EnsWithMax();
        //        ewm.Ensemble = ensembles.IndexValue(x);

        //        // Verify the subsystem matches this viewmodel's subystem.
        //        if ((subsystem == ewm.Ensemble.EnsembleData.GetSubSystem())                 // Check if Subsystem matches 
        //                && (ssConfig == ewm.Ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
        //        {
        //            if (ewm != null)
        //            {
        //                // Get the max number of bins
        //                ewm.MaxBins = DataSet.Ensemble.MAX_NUM_BINS;
        //                if (ewm.Ensemble.IsEnsembleAvail)
        //                {
        //                    ewm.MaxBins = ewm.Ensemble.EnsembleData.NumBins;
        //                }

        //                // Update the list with the latest ensemble
        //                UpdateEnsembleList(ewm.Ensemble, ewm.MaxBins);

        //                // Update the list options
        //                UpdateListOptions(ewm.Ensemble);

        //                // Flip the Axis for upward or downward ADCP
        //                SetUpwardOrDownwardPlotAxis(ewm.Ensemble);

        //                // Lock the plot for an update
        //                lock (Plot.SyncRoot)
        //                {
        //                    // Update the time series with the latest data
        //                    foreach (var series in Plot.Series)
        //                    {
        //                        // Verify the type of series
        //                        if (series.GetType() == typeof(ProfileSeries))
        //                        {
        //                            //Update the series
        //                            ((ProfileSeries)series).UpdateSeries(ewm.Ensemble, ewm.MaxBins, _isFilterData);
        //                        }
        //                    }

        //                    // Update min,max,avg,std
        //                    foreach (var mSeries in _minMaxAvgStdSeriesList)
        //                    {
        //                        mSeries.UpdateSeries(ewm.Ensemble, ewm.MaxBins, _isFilterData);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // After the line series have been updated
        //    // Refresh the plot with the latest data.
        //    Plot.InvalidatePlot(true);
        //}


        ///// <summary>
        ///// Update the plot async.
        ///// </summary>
        ///// <param name="param">Ensemble and max bin.</param>
        //private void AddIncomingDataExecute(object param)
        //{
        //    while (_buffer.Count > 0)
        //    {
        //        _isProcessingBuffer = true;

        //        EnsWithMax ewm = null;
        //        if (_buffer.TryDequeue(out ewm))
        //        {
        //            if (ewm != null)
        //            {
        //                // Update the list with the latest ensemble
        //                UpdateEnsembleList(ewm.Ensemble, ewm.MaxBins);

        //                // Update the list options
        //                UpdateListOptions(ewm.Ensemble);

        //                // Flip the Axis for upward or downward ADCP
        //                SetUpwardOrDownwardPlotAxis(ewm.Ensemble);

        //                // Update the line series
        //                UpdatePlot(ewm.Ensemble, ewm.MaxBins);
        //            }
        //        }
        //    }

        //    _isProcessingBuffer = false;
        //}

        ///// <summary>
        ///// Clear all the line series.
        ///// </summary>
        //public void ClearIncomingData()
        //{
        //    // Clear the plot
        //    ClearPlot();

        //    // Clear the list options
        //    ClearListOptions();

        //    // Clear the list of ensembles
        //    _ensembleList.Clear();
        //}

        //#endregion

        //#region Create Plot

        ///// <summary>
        ///// Create the plot.  This will create a base plot.
        ///// Then based off the series type, it will add the
        ///// series type specifics to the plot.  This includes
        ///// the axis labels and min and max values.
        ///// </summary>
        ///// <param name="seriesType">Series type.</param>
        ///// <returns>Plot created based off the series type.</returns>
        //private PlotModel CreatePlot(BaseSeriesType seriesType)
        //{
        //    PlotModel temp = CreatePlot();

        //    // Set the plot title
        //    //temp.Title = seriesType.ToString();
        //    Title = _seriesType.ToString();
        //    temp.TitlePadding = 0;

        //    // Set the axis
        //    SetPlotAxis(ref temp);

        //    return temp;
        //}

        //#region Create Plot

        ///// <summary>
        ///// Create the plot.  Set the settings for the plot.
        ///// </summary>
        ///// <returns>Plot created.</returns>
        //private PlotModel CreatePlot()
        //{
        //    PlotModel temp = new PlotModel();

        //    //temp.AutoAdjustPlotMargins = false;
        //    //temp.PlotMargins = new OxyThickness(0,0,0,20);
        //    //temp.Padding = new OxyThickness(10,0,10,0);

        //    //temp.Background = OxyColors.Black;
        //    temp.TextColor = OxyColors.White;
        //    temp.PlotAreaBorderColor = OxyColors.White;

        //    // Create line series and add to plot
        //    // No color for the series is specified
        //    //temp.Series.Add(new LineSeries(Beam0Color, 1, "Beam 1"));

        //    //temp.Title = "Time Series";

        //    // Set the legend position
        //    temp.IsLegendVisible = true;
        //    temp.LegendPosition = LegendPosition.BottomCenter;
        //    temp.LegendPlacement = LegendPlacement.Inside;
        //    temp.LegendOrientation = LegendOrientation.Horizontal;
        //    //temp.LegendSymbolPlacement = LegendSymbolPlacement.Right;
        //    temp.LegendFontSize = 10;
        //    temp.LegendItemSpacing = 8;

        //    // For the correlation plot, display the depth for the axis
        //    // For the rest of the plots use the bin.
        //    if (_seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Correlation)
        //    {
        //        // Setup the axis
        //        temp.Axes.Add(new LinearAxis
        //        {
        //            Position = AxisPosition.Left,
        //            StartPosition = 1,                                                  // This will invert the axis to start at the top with minimum value
        //            EndPosition = 0,
        //            TicklineColor = OxyColors.White,
        //            MajorGridlineStyle = LineStyle.Solid,
        //            MinorGridlineStyle = LineStyle.Solid,
        //            MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
        //            MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White),
        //            TickStyle = OxyPlot.Axes.TickStyle.Inside,                               // Put tick lines inside the plot
        //            //MinimumPadding = 0.1,                                                 // Start at axis edge   
        //            //MaximumPadding = 0.1,                                                 // Start at axis edge
        //            //IsAxisVisible = true,
        //            //MajorStep = 1,
        //            //Minimum = 0,
        //            IntervalLength = 20,
        //            Unit = "m",
        //            Key = BIN_OR_DEPTH_AXIS

        //        });
        //    }
        //    else
        //    {
        //        // Setup the axis
        //        temp.Axes.Add(new LinearAxis
        //        {
        //            Position = AxisPosition.Left,
        //            StartPosition = 1,                                                  // This will invert the axis to start at the top with minimum value
        //            EndPosition = 0,
        //            TicklineColor = OxyColors.White,
        //            MajorGridlineStyle = LineStyle.Solid,
        //            MinorGridlineStyle = LineStyle.Solid,
        //            MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
        //            MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White),
        //            TickStyle = OxyPlot.Axes.TickStyle.Inside,                               // Put tick lines inside the plot
        //            //MinimumPadding = 0.1,                                                 // Start at axis edge   
        //            //MaximumPadding = 0.1,                                                 // Start at axis edge
        //            //IsAxisVisible = true,
        //            //MajorStep = 1,
        //            Minimum = 0,
        //            IntervalLength = 20,
        //            Unit = "Bin",
        //            Key = BIN_OR_DEPTH_AXIS
        //        });
        //    }

        //    return temp;
        //}

        //#endregion

        //#region Axis

        ///// <summary>
        ///// Set the plot axis values (label and min/max) based
        ///// off the series type.
        ///// </summary>
        ///// <param name="temp">Plot Model to set the axis.</param>
        //private void SetPlotAxis(ref PlotModel temp)
        //{
        //    switch (_seriesType.Code)
        //    {
        //        case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:                 // Beam Velocity data
        //        case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:                  // Instrument Velocity data
        //        case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:                  // Earth Velocity data
        //            //temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 0.5, "m/s"));
        //            temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, "m/s"));
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_Amplitude:                     // Amplitude data
        //            LinearAxis axisAmp = CreatePlotAxis(AxisPosition.Bottom, 20, "dB");
        //            axisAmp.Minimum = 0;
        //            axisAmp.Maximum = 120;
        //            temp.Axes.Add(axisAmp);
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_Correlation:                   // Correlation data
        //            LinearAxis axisCorr = CreatePlotAxis(AxisPosition.Bottom, 20, "%");
        //            axisCorr.Minimum = 0;
        //            axisCorr.Maximum = 100;
        //            temp.Axes.Add(axisCorr);
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_SNR:                           // SNR data
        //            temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 0.5, "dB"));
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_Range:                         // Range data
        //            LinearAxis axisRange = CreatePlotAxis(AxisPosition.Bottom, 5, "m");
        //            axisRange.Minimum = 0;
        //            axisRange.StartPosition = 1;                                        // This will invert the axis to start at the top with minimum value
        //            axisRange.EndPosition = 0;                                          // This will invert the axis to start at the top with minimum value
        //            temp.Axes.Add(axisRange);
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_Heading:                       // Heading data
        //        case BaseSeriesType.eBaseSeriesType.Base_Pitch:                         // Pitch data
        //        case BaseSeriesType.eBaseSeriesType.Base_Roll:                          // Roll data
        //            temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "Deg"));
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_Pressure:                      // Pressure data
        //            temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "Pa"));
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_TransducerDepth:               // Transducer Depth Pressure data
        //            temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "m"));
        //            break;
        //        case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:               // System Temperature data
        //        case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:             // Water Temperature data
        //            temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "Deg F"));
        //            break;
        //        default:
        //            break;
        //    }
        //}

        ///// <summary>
        ///// Create the plot axis.  Set the values for the plot axis.
        ///// If you do not want to set a value, set the value to NULL.
        ///// </summary>
        ///// <param name="position">Position of the axis.</param>
        ///// <param name="unit">Label for the axis.</param>
        ///// <returns>LinearAxis for the plot.</returns>
        //private LinearAxis CreatePlotAxis(AxisPosition position, string unit)
        //{
        //    // Create the axis
        //    LinearAxis axis = new LinearAxis();

        //    // Standard options
        //    axis.TicklineColor = OxyColors.White;
        //    axis.MajorGridlineStyle = LineStyle.Solid;
        //    axis.MinorGridlineStyle = LineStyle.Solid;
        //    axis.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
        //    axis.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
        //    axis.MinimumPadding = 0;                                                 // Start at axis edge   
        //    axis.MaximumPadding = 0;                                                 // Start at axis edge

        //    // Axis position
        //    // Usually AxisPosition.Bottom or
        //    // Axis.Position.Left
        //    axis.Position = position;

        //    // Set the Minimum value for the axis
        //    //axis.Minimum = min;

        //    // Set the axis label
        //    axis.Unit = unit;

        //    return axis;
        //}

        ///// <summary>
        ///// Create the plot axis.  Set the values for the plot axis.
        ///// If you do not want to set a value, set the value to NULL.
        ///// </summary>
        ///// <param name="position">Position of the axis.</param>
        ///// <param name="majorStep">Axis step.</param>
        ///// <param name="unit">Label for the axis.</param>
        ///// <returns>LinearAxis for the plot.</returns>
        //private LinearAxis CreatePlotAxis(AxisPosition position, double majorStep, string unit)
        //{
        //    // Create the axis
        //    LinearAxis axis = new LinearAxis();

        //    // Standard options
        //    axis.TicklineColor = OxyColors.White;
        //    axis.MajorGridlineStyle = LineStyle.Solid;
        //    axis.MinorGridlineStyle = LineStyle.Solid;
        //    axis.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
        //    axis.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
        //    axis.MinimumPadding = 0;                                                 // Start at axis edge   
        //    axis.MaximumPadding = 0;                                                 // Start at axis edge
        //    axis.MajorStep = majorStep;

        //    // Axis position
        //    // Usually AxisPosition.Bottom or
        //    // Axis.Position.Left
        //    axis.Position = position;

        //    // Set the Minimum value for the axis
        //    //axis.Minimum = min;

        //    // Set the axis label
        //    axis.Unit = unit;

        //    return axis;
        //}

        //#endregion

        //#endregion

        //#region Update Plot

        ///// <summary>
        ///// Update the list with the latest ensemble.  Also ensure the list is the correct size.
        ///// </summary>
        ///// <param name="ens">Latest ensemble.</param>
        ///// <param name="maxEnsembles">Maximum number of points for a series.</param>
        //private void UpdateEnsembleList(DataSet.Ensemble ens, int maxEnsembles)
        //{
        //    // Add the latest ensemble to the list
        //    _ensembleList.Add(ens);
        //}

        ///// <summary>
        ///// Update the plot with the latest ensemble data.
        ///// </summary>
        ///// <param name="ensemble">Latest ensemble data.</param>
        ///// <param name="maxBins">Maximum number of bins in the series.</param>
        //private void UpdatePlot(DataSet.Ensemble ensemble, int maxBins)
        //{
        //    // Lock the plot for an update
        //    lock (Plot.SyncRoot)
        //    {
        //        // Update the time series with the latest data
        //        foreach (var series in Plot.Series)
        //        {
        //            // Verify the type of series
        //            if (series.GetType() == typeof(ProfileSeries))
        //            {
        //                 //Update the series
        //                ((ProfileSeries)series).UpdateSeries(ensemble, maxBins, _isFilterData);

        //            }

        //            // Add the updated series to the plot
        //            //Plot.Series.Add(series.LineSeries);
        //        }

        //        // Update min,max,avg,std
        //        foreach (var mSeries in _minMaxAvgStdSeriesList)
        //        {
        //            mSeries.UpdateSeries(ensemble, maxBins, _isFilterData);
        //        }
        //    }

        //    // After the line series have been updated
        //    // Refresh the plot with the latest data.
        //    Plot.InvalidatePlot(true);
        //}

        ///// <summary>
        ///// Update the plot with the latest ensemble data.
        ///// </summary>
        ///// <param name="ensemble">Latest ensemble data.</param>
        ///// <param name="maxBins">Maximum number of bins in the series.</param>
        //private void UpdatePlotBulk(Cache<long, DataSet.Ensemble> ensembles, Subsystem subsystem, SubsystemDataConfig ssConfig)
        //{
        //    // Lock the plot for an update
        //    lock (Plot.SyncRoot)
        //    {
        //        for (int x = 0; x < ensembles.Count(); x++)
        //        {
        //            var ensemble = ensembles.IndexValue(x);

        //            // Verify the subsystem matches this viewmodel's subystem.
        //            if ((subsystem == ensemble.EnsembleData.GetSubSystem())                 // Check if Subsystem matches 
        //                    && (ssConfig == ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
        //            {

        //                // Get the number of bins
        //                int maxBins = DataSet.Ensemble.MAX_NUM_BINS;
        //                if (ensemble.IsEnsembleAvail)
        //                {
        //                    maxBins = ensemble.EnsembleData.NumBins;
        //                }

        //                // Update the time series with the latest data
        //                foreach (var series in Plot.Series)
        //                {
        //                    // Verify the type of series
        //                    if (series.GetType() == typeof(ProfileSeries))
        //                    {
        //                        //Update the series
        //                        ((ProfileSeries)series).UpdateSeries(ensemble, maxBins, _isFilterData);

        //                    }
        //                }

        //                // Update min,max,avg,std
        //                foreach (var mSeries in _minMaxAvgStdSeriesList)
        //                {
        //                    mSeries.UpdateSeries(ensemble, maxBins, _isFilterData);
        //                }
        //            }
        //        }
        //    }

        //    // After the line series have been updated
        //    // Refresh the plot with the latest data.
        //    Plot.InvalidatePlot(true);
        //}

        //#endregion

        //#region Clear Plot

        ///// <summary>
        ///// Clear the plot of all the data.
        ///// </summary>
        //private void ClearPlot()
        //{
        //    // Lock the plot for an update
        //    lock (Plot.SyncRoot)
        //    {
        //        // Clear the data for each series.
        //        foreach (var series in Plot.Series)
        //        {
        //            if (series.GetType() == typeof(ProfileSeries))
        //            {
        //                ((ProfileSeries)series).ClearSeries();
        //            }
        //        }

        //        foreach (var mSeries in _minMaxAvgStdSeriesList)
        //        {
        //            mSeries.ClearSeries();
        //        }
        //    }

        //    // After the line series have been updated
        //    // Refresh the plot with the latest data.
        //    Plot.InvalidatePlot(true);
        //}

        //#endregion

        //#region List Options

        ///// <summary>
        ///// Setup the list of options available to the user based off the
        ///// series type.  This will enable and disable list and populate the
        ///// list with the correct values based off the series type.
        ///// </summary>
        ///// <param name="seriesType">Series type.</param>
        //private void SetupListOptions(BaseSeriesType seriesType)
        //{
        //    // Set the DataSet List
        //    DataSetTypeList = ProfileType.GetDataSetTypeList(seriesType.Code);
        //    if (DataSetTypeList.Count > 0)
        //    {
        //        SelectedDataSetType = DataSetTypeList[0];
        //    }

        //    // Set the Subsystem config list
        //    SubsystemConfigList = new ObservableCollection<AdcpSubsystemConfig>();

        //    // Set the max bin
        //    MaxBin = DEFAULT_MAX_BIN;
        //    SelectedMaxBins = 0;
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
        //    // Update the max bin
        //    if (ensemble.EnsembleData.NumBins > _maxBin || _maxBin == DEFAULT_MAX_BIN)
        //    {
        //        // Subtract 1 because it is zero based
        //        MaxBin = ensemble.EnsembleData.NumBins - 1;
        //        SelectedMaxBins = ensemble.EnsembleData.NumBins - 1;
        //    }

        //    // Update the max beam
        //    if (ensemble.EnsembleData.NumBeams > _maxBeam)
        //    {
        //        // Subtract 1 because it is zero based
        //        MaxBeam = ensemble.EnsembleData.NumBeams - 1;
        //    }
        //}

        ///// <summary>
        ///// Clear the list.  This will reset all the list 
        ///// back to default.
        ///// </summary>
        //private void ClearListOptions()
        //{
        //    SetupListOptions(_seriesType);
        //}

        //#endregion

        //#region Selections

        ///// <summary>
        ///// Test whether all the selections have been made.
        ///// Some selections are not necessary so this will
        ///// validate only the necessary selections based 
        ///// off the series type.
        ///// </summary>
        ///// <returns>TRUE =  All selections have been made.</returns>
        //private bool TestSelections()
        //{
        //    return true;
        //}

        ///// <summary>
        ///// Based off the beam selected, this will help the
        ///// user choose a default color for the beam number.
        ///// The user can still change the selection, but this
        ///// will be made automatically when the beam number changes.
        ///// </summary>
        ///// <param name="selectedBeam">Beam number selected.</param>
        //private void SetDefaultBeamColor(int selectedBeam)
        //{
        //    // Look for all the types that have beams as series
        //    if (_seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam ||
        //        _seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ ||
        //        _seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU )
        //    {
        //        // Set the beam color for the line series
        //        switch (selectedBeam)
        //        {
        //            case 0:
        //                SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0);
        //                break;
        //            case 1:
        //                SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1);
        //                break;
        //            case 2:
        //                SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2);
        //                break;
        //            case 3:
        //                SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_3);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}

        //#endregion

        //#region Add Series

        ///// <summary>
        ///// Add a series to the plot.
        ///// Use ProfileSeries.MAX_BIN if you do not want to sent a maxBins.
        ///// </summary>
        ///// <param name="type">Series type.</param>
        ///// <param name="beam">Beam selected.</param>
        ///// <param name="maxBins">Maximum number of bins to display.</param>
        ///// <param name="color">Color of the plot.</param>
        //public void AddSeries(ProfileType type, int beam, OxyColor color, int maxBins = ProfileSeries.MAX_BIN)
        //{
        //    // Create Min, Max, Avg, Std series
        //    // and add it to the list
        //    var minMaxAvgStdSeries = new MinMaxAvgStdSeries(type, beam, color, _isFilterData, maxBins, _ensembleList);
        //    _minMaxAvgStdSeriesList.Add(minMaxAvgStdSeries);

        //    // Lock the plot for an update
        //    lock (Plot.SyncRoot)
        //    {
        //        // Add the series to the list
        //        // Create a series
        //        ProfileSeries series = new ProfileSeries(type, beam, color, _isFilterData, maxBins, _ensembleList);
        //        series.Tag = TAG_PROFILE_SERIES;
        //        series.IsVisible = _IsProfileSeriesOn;
        //        Plot.Series.Add(series);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(series)));

        //        minMaxAvgStdSeries.MinPoints.Tag = TAG_MIN_SERIES;
        //        minMaxAvgStdSeries.MinPoints.IsVisible = _IsMinSeriesOn;
        //        Plot.Series.Add(minMaxAvgStdSeries.MinPoints);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.MinPoints)));

        //        minMaxAvgStdSeries.MaxPoints.Tag = TAG_MAX_SERIES;
        //        minMaxAvgStdSeries.MaxPoints.IsVisible = _IsMaxSeriesOn;
        //        Plot.Series.Add(minMaxAvgStdSeries.MaxPoints);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.MaxPoints)));

        //        minMaxAvgStdSeries.AvgPoints.Tag = TAG_AVG_SERIES;
        //        minMaxAvgStdSeries.AvgPoints.IsVisible = _IsAvgSeriesOn;
        //        Plot.Series.Add(minMaxAvgStdSeries.AvgPoints);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.AvgPoints)));

        //        minMaxAvgStdSeries.StdP2PPoints.Tag = TAG_STDP2P_SERIES;
        //        minMaxAvgStdSeries.StdP2PPoints.IsVisible = _IsStdP2PSeriesOn;
        //        Plot.Series.Add(minMaxAvgStdSeries.StdP2PPoints);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.StdP2PPoints)));

        //        minMaxAvgStdSeries.StdB2BPoints.Tag = TAG_STDB2B_SERIES;
        //        minMaxAvgStdSeries.StdB2BPoints.IsVisible = _IsStdB2BSeriesOn;
        //        Plot.Series.Add(minMaxAvgStdSeries.StdB2BPoints);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.StdB2BPoints)));
        //    }

        //    // Then refresh the plot
        //    Plot.InvalidatePlot(true);
        //}

        //#endregion

        //#region Remove Series

        ///// <summary>
        ///// Remove the given time series from the plot.
        ///// 
        ///// This will remove the series from the plot.  It
        ///// will then deselect the series.  It will then refresh
        ///// the plot with the series removed.
        ///// </summary>
        ///// <param name="series">Series to remove.</param>
        //public void RemoveSeries(OxyPlot.Series.Series series)
        //{
        //    if(series == null)
        //    {
        //        return;
        //    }

        //    // Lock the plot for an update
        //    lock (Plot.SyncRoot)
        //    {
        //        // Remove the series from the plot
        //        Plot.Series.Remove(series);
        //        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Remove(series)));

        //        // Remove the selection
        //        SelectedRemoveSeries = null;

        //        // Clear all the axis if there are no series
        //        if (Plot.Series.Count <= 0)
        //        {
        //            for (int x = 0; x < Plot.Axes.Count; x++)
        //            {
        //                // If the axis is on the left side, this is
        //                // the axis for the incoming data
        //                // Remove it since there is no data associated 
        //                // with the plot
        //                if (Plot.Axes[x].Position == AxisPosition.Left)
        //                {
        //                    Plot.Axes.RemoveAt(x);
        //                }
        //            }

        //            // Remove the title
        //            Plot.Title = "";
        //        }
        //    }

        //    // Then refresh the plot
        //    Plot.InvalidatePlot(true);
        //}

        //#endregion

        //#region Update Series Visibility

        ///// <summary>
        ///// Update the series visibilty.  The given tag is the
        ///// series tag.  The value will turn on or off the 
        ///// visibility.
        ///// </summary>
        ///// <param name="tag">The tag of the series.</param>
        ///// <param name="value">TRUE = On, FALSE = OFF.</param>
        //private void UpdateSeriesVisibility(object tag, bool value)
        //{
        //    foreach(var series in Plot.Series)
        //    {
        //        // Look for the series that matches
        //        if(series.Tag == tag)
        //        {
        //            series.IsVisible = value;
        //        }
        //    }

        //    // Then refresh the plot
        //    Plot.InvalidatePlot(true);
        //}

        //#endregion

        //#region Override

        ///// <summary>
        ///// Determine if the 2 views given are the equal.
        ///// </summary>
        ///// <param name="view1">First series to check.</param>
        ///// <param name="view2">Series to check against.</param>
        ///// <returns>True if there codes match.</returns>
        //public static bool operator ==(ProfilePlotViewModel view1, ProfilePlotViewModel view2)
        //{
        //    // If both are null, or both are same instance, return true.
        //    if (System.Object.ReferenceEquals(view1, view2))
        //    {
        //        return true;
        //    }

        //    // If one is null, but not both, return false.
        //    if (((object)view1 == null) || ((object)view2 == null))
        //    {
        //        return false;
        //    }

        //    // Return true if the fields match:
        //    return (view1.ID == view2.ID);
        //}

        ///// <summary>
        ///// Return the opposite of ==.
        ///// </summary>
        ///// <param name="view1">First view to check.</param>
        ///// <param name="view2">View to check against.</param>
        ///// <returns>Return the opposite of ==.</returns>
        //public static bool operator !=(ProfilePlotViewModel view1, ProfilePlotViewModel view2)
        //{
        //    return !(view1 == view2);
        //}

        ///// <summary>
        ///// Create a hashcode based off the Code stored.
        ///// </summary>
        ///// <returns>Hash the Code.</returns>
        //public override int GetHashCode()
        //{
        //    // Use the date time and kind to represent a better hash code.
        //    // If it juse used the time, then if the 2 objects where created
        //    // close to each other, they would have the same hash code.
        //    // This gives a little more to the hash code then just datetime.
        //    DateTime when = DateTime.Now;
        //    ulong kind = (ulong)(int)when.Kind;
        //    return (int)((kind << 62) | (ulong)when.Ticks);
        //}

        ///// <summary>
        ///// Check if the given object is 
        ///// equal to this object.
        ///// </summary>
        ///// <param name="obj">Object to check.</param>
        ///// <returns>If the codes are the same, then they are equal.</returns>
        //public override bool Equals(object obj)
        //{
        //    //Check for null and compare run-time types.
        //    if (obj == null || GetType() != obj.GetType()) return false;

        //    ProfilePlotViewModel p = (ProfilePlotViewModel)obj;

        //    return (ID == p.ID);
        //}


        //#endregion

        //#region Report

        ///// <summary>
        ///// Generate a report of the plot.
        ///// </summary>
        //public void GenerateReport(string filename = "", bool isFiltered = true)
        //{
        //    var reportStyle = new OxyPlot.Reporting.ReportStyle();

        //    // Make a copy of the plot for a white background
        //    PlotModel plot = new PlotModel();
        //    // Set new colors
        //    plot.TextColor = OxyColors.Black;
        //    plot.PlotAreaBorderColor = OxyColors.Black;

        //    // Copy data
        //    foreach (var series in _plot.Series)
        //    {
        //        // Verify the type of series
        //        if (series.GetType() == typeof(ProfileSeries))
        //        {
        //            var newSeries = new ProfileSeries(((ProfileSeries)series).Type, ((ProfileSeries)series).Beam, ((ProfileSeries)series).Color, isFiltered, ((ProfileSeries)series).MaxBins, null);
        //            newSeries.IsVisible = ((ProfileSeries)series).IsVisible;
        //            foreach (var pt in ((ProfileSeries)series).Points)
        //            {
        //                newSeries.Points.Add(pt);
        //            }

        //            plot.Series.Add(newSeries);
        //        }
        //        if (series.GetType() == typeof(ScatterSeriesWithToString))
        //        {
        //            var newSeries = new ScatterSeriesWithToString(((ScatterSeriesWithToString)series).Title, ((ScatterSeriesWithToString)series).MarkerFill, (int)((ScatterSeriesWithToString)series).MarkerSize);
        //            newSeries.Tag = ((ScatterSeriesWithToString)series).Tag;
        //            newSeries.IsVisible = ((ScatterSeriesWithToString)series).IsVisible;
        //            foreach(var pt in ((ScatterSeriesWithToString)series).Points)
        //            {
        //                newSeries.Points.Add(pt);
        //            }

        //            plot.Series.Add(newSeries);
        //        }
        //        if (series.GetType() == typeof(LineSeriesWithToString))
        //        {
        //            var newSeries = new LineSeriesWithToString(((LineSeriesWithToString)series).Title, ((LineSeriesWithToString)series).Color);
        //            newSeries.Tag = ((LineSeriesWithToString)series).Tag;
        //            newSeries.IsVisible = ((LineSeriesWithToString)series).IsVisible;
        //            foreach (var pt in ((LineSeriesWithToString)series).Points)
        //            {
        //                newSeries.Points.Add(pt);
        //            }

        //            plot.Series.Add(newSeries);
        //        }
        //    }

        //    // Copy Axis but change axis color lines
        //    foreach (var axis in _plot.Axes)
        //    {
        //        var newAxis = new LinearAxis();
        //        newAxis.Position = axis.Position;
        //        newAxis.TickStyle = axis.TickStyle;
        //        newAxis.MinimumPadding = axis.MinimumPadding;
        //        newAxis.MaximumPadding = axis.MaximumPadding;
        //        newAxis.Unit = axis.Unit;
        //        newAxis.MajorGridlineStyle = axis.MajorGridlineStyle;
        //        newAxis.MinorGridlineStyle = axis.MinorGridlineStyle;
        //        newAxis.IntervalLength = axis.IntervalLength;
        //        newAxis.Minimum = axis.Minimum;
        //        newAxis.Maximum = axis.Maximum;
        //        newAxis.StartPosition = axis.StartPosition;
        //        newAxis.EndPosition = axis.EndPosition;
        //        newAxis.Key = axis.Key;
        //        newAxis.TitleColor = axis.TitleColor;
        //        newAxis.TextColor = axis.TextColor;
        //        newAxis.MinorStep = axis.MinorStep;
        //        newAxis.PositionTier = axis.PositionTier;

        //        // Set new colors
        //        newAxis.TicklineColor = OxyColors.LightGray;
        //        newAxis.MajorGridlineColor = OxyColors.Gray;
        //        newAxis.MinorGridlineColor = OxyColors.LightGray;

        //        plot.Axes.Add(newAxis);
        //    }

        //    // Create report
        //    var report = new OxyPlot.Reporting.Report();
        //    string title = string.Format("{0}", Title);
        //    report.AddPlot(plot, title, 800, 600);

        //    // Set file name
        //    DateTime currDateTime = DateTime.Now;
        //    if (string.IsNullOrEmpty(filename))
        //    {
        //        filename = string.Format("{0}\\{1}_{2:yyyyMMddHHmmss}.html", Pulse.Commons.DEFAULT_RECORD_DIR, title, currDateTime);
        //    }

        //    // Write report
        //    using (var s = File.Create(filename))
        //    {
        //        using (var w = new OxyPlot.Reporting.HtmlReportWriter(s))
        //        {
        //            w.WriteReport(report, reportStyle);
        //        }
        //    }
        //}

        //#endregion

        //#region Check Upward or Downward

        ///// <summary>
        ///// Check if the plot axis label needs to be reset for Upward or Downward.
        ///// Set the axis StartPosition and EndPosition for upward or downward.
        ///// Set both the Bin Axis and Meter Axis
        ///// 
        ///// Downward:
        ///// StartPosition = 1
        ///// EndPosition = 0
        ///// 
        ///// Upward: 
        ///// StartPosition = 0
        ///// EndPosition = 1
        ///// </summary>
        ///// <param name="ens"></param>
        //private void SetUpwardOrDownwardPlotAxis(DataSet.Ensemble ens)
        //{
        //    // Upward Facing
        //    if (ens.AncillaryData.IsUpwardFacing())
        //    {
        //        // Find the Plot axes for the bin and meter 
        //        for (int x = 0; x < Plot.Axes.Count; x++)
        //        {
        //            // Bin Axis
        //            // Upward should be 0 so reset
        //            if (Plot.Axes[x].Key == BIN_OR_DEPTH_AXIS && Plot.Axes[x].StartPosition == 1)
        //            {
        //                Plot.Axes[x].StartPosition = 0;
        //                Plot.Axes[x].EndPosition = 1;
        //            }

        //            // Meters Axis
        //            // Upward should be 0 so reset
        //            if (Plot.Axes[x].Key == BIN_OR_DEPTH_AXIS && Plot.Axes[x].StartPosition == 1)
        //            {
        //                Plot.Axes[x].StartPosition = 0;
        //                Plot.Axes[x].EndPosition = 1;
        //            }

        //        }
        //    }
        //    // Downward Facing
        //    else
        //    {
        //        // Find the Plot axes for the bin and meter 
        //        for (int x = 0; x < Plot.Axes.Count; x++)
        //        {
        //            // Downward StartPosition should be 1 so reset
        //            if (Plot.Axes[x].Key == BIN_OR_DEPTH_AXIS && Plot.Axes[x].StartPosition == 0)
        //            {
        //                Plot.Axes[x].StartPosition = 1;
        //                Plot.Axes[x].EndPosition = 0;
        //            }

        //            // Downward StartPosition should be 1 so reset
        //            if (Plot.Axes[x].Key == BIN_OR_DEPTH_AXIS && Plot.Axes[x].StartPosition == 0)
        //            {
        //                Plot.Axes[x].StartPosition = 1;
        //                Plot.Axes[x].EndPosition = 0;
        //            }
        //        }
        //    }

        //}

        //#endregion

        #endregion

        #region Commands

        #region Remove Plot Command

        /// <summary>
        /// Remove this plot.
        /// </summary>
        private void On_RemovePlot()
        {

        }

        #endregion

        #region Clear Plot Command

        /// <summary>
        /// Clear this plot.
        /// </summary>
        private void On_ClearPlot()
        {
            // Clear the plot
            //ClearIncomingData();
        }

        #endregion


        #endregion
    }
}
