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
 * 02/01/2013      RC          2.18       Initial coding
 * 07/16/2013      RC          3.0.4      Changed SeriesType.
 * 09/12/2013      RC          3.1.0      Removed backgroundworkers and replaced with ReactiveAsyncCommands.
 * 12/09/2013      RC          3.2.0      Added IntervalLength = 20 to the plot axis to display more axis values.
 * 01/14/2014      RC          3.2.3      Maded AddIncomingData() work async.  The data is buffered when it is recieved so the event handler can return immediately.
 * 01/16/2014      RC          3.2.3      Changed SetDefaultBeamColor() to convert the default colors to an OxyColor.
 * 05/23/2014      RC          3.2.4      Refresh the plot here and remove the event to tell the view to refresh the plot.  Added locks to the updating the plot.
 * 08/04/2014      RC          3.4.0      Fixed bug with refreshing the plot.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 01/23/2015      RC          4.1.0      Added MinMaxAvgStdSeries to take the min, max, avg and std of a profile plot.
 * 01/27/2015      RC          4.1.0      Added the option to turn on or off the profile plots.
 * 12/07/2015      RC          4.4.0      Added GenerateReport to create HTML files.
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
    public class ProfilePlotViewModel : PulseViewModel
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
        private ConcurrentQueue<EnsWithMax> _buffer;

        /// <summary>
        /// Flag to know if processing the buffer.
        /// </summary>
        private bool _isProcessingBuffer;

        /// <summary>
        /// Minimum, maxmium and average series.
        /// </summary>
        private List<MinMaxAvgStdSeries> _minMaxAvgStdSeriesList;

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

        #region Selection

        #region Bin

        /// <summary>
        /// The maximum bin found for the ensemble data.
        /// </summary>
        private int _maxBin;
        /// <summary>
        /// The maximum bin found for the ensemble data.
        /// </summary>
        public int MaxBin
        {
            get { return _maxBin; }
            set
            {
                _maxBin = value;
                this.NotifyOfPropertyChange(() => this.MaxBin);
            }
        }

        /// <summary>
        /// Selected bin for the plot.  If the plot data
        /// is dependent on the bin number, use this
        /// value to determine which bin was selected.
        /// </summary>
        private int _selectedMaxBins;
        /// <summary>
        /// Selected bin for the plot.  If the plot data
        /// is dependent on the bin number, use this
        /// value to determine which bin was selected.
        /// </summary>
        public int SelectedMaxBins
        {
            get { return _selectedMaxBins; }
            set
            {
                _selectedMaxBins = value;
                this.NotifyOfPropertyChange(() => this.SelectedMaxBins);

                // Update the add series command
                //((DelegateCommand<object>)AddSeriesCommand).RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Beam

        /// <summary>
        /// Maximum beam to select.  If an ensemble is receive,
        /// this will be set by the ensemble.  It will initially set
        /// by the default value.
        /// </summary>
        private int _maxBeam;
        /// <summary>
        /// Maximum beam to select.  If an ensemble is receive,
        /// this will be set by the ensemble.  It will initially set
        /// by the default value.
        /// </summary>
        public int MaxBeam
        {
            get { return _maxBeam; }
            set
            {
                _maxBeam = value;
                this.NotifyOfPropertyChange(() => this.MaxBeam);
            }
        }

        /// <summary>
        /// Selected beam number when creating 
        /// a new series.
        /// </summary>
        private int _selectedBeam;
        /// <summary>
        /// Selected beam number when creating 
        /// a new series.
        /// </summary>
        public int SelectedBeam
        {
            get { return _selectedBeam; }
            set
            {
                _selectedBeam = value;
                this.NotifyOfPropertyChange(() => this.SelectedBeam);

                // Change default color option for beam color
                SetDefaultBeamColor(SelectedBeam);
            }
        }

        #endregion

        #region DataSet Type

        /// <summary>
        /// List of all the data set types.
        /// This is based off the series type.
        /// </summary>
        private ObservableCollection<ProfileType> _dataSetTypeList;
        /// <summary>
        /// List of all the data set types.
        /// This is based off the series type.
        /// </summary>
        public ObservableCollection<ProfileType> DataSetTypeList
        {
            get { return _dataSetTypeList; }
            set
            {
                _dataSetTypeList = value;
                this.NotifyOfPropertyChange(() => this.DataSetTypeList);
            }
        }

        /// <summary>
        /// The selected data set type.
        /// </summary>
        private ProfileType _selectedDataSetType;
        /// <summary>
        /// The selected data set type.
        /// </summary>
        public ProfileType SelectedDataSetType
        {
            get { return _selectedDataSetType; }
            set
            {
                _selectedDataSetType = value;
                this.NotifyOfPropertyChange(() => this.SelectedDataSetType);

                // Update the add series command
                //((DelegateCommand<object>)AddSeriesCommand).RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Subsystem Configuration

        /// <summary>
        /// List of all the subsystem configurations.
        /// </summary>
        private ObservableCollection<AdcpSubsystemConfig> _subsystemConfigList;
        /// <summary>
        /// List of all the subsystem configurations.
        /// </summary>
        public ObservableCollection<AdcpSubsystemConfig> SubsystemConfigList
        {
            get { return _subsystemConfigList; }
            set
            {
                _subsystemConfigList = value;
                this.NotifyOfPropertyChange(() => this.SubsystemConfigList);
            }
        }

        /// <summary>
        /// Selected subsystem configuration.
        /// </summary>
        private AdcpSubsystemConfig _selectSubsystemConfig;
        /// <summary>
        /// Selected subsystem configuration.
        /// </summary>
        public AdcpSubsystemConfig SelectedSubsystemConfig
        {
            get { return _selectSubsystemConfig; }
            set
            {
                _selectSubsystemConfig = value;
                this.NotifyOfPropertyChange(() => this.SelectedSubsystemConfig);

                // Update the add series command
                //((DelegateCommand<object>)AddSeriesCommand).RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Color

        /// <summary>
        /// Color selected for the new series.
        /// </summary>
        private OxyColor _selectedSeriesColor;
        /// <summary>
        /// Color selected for the new series.
        /// </summary>
        public OxyColor SelectedSeriesColor
        {
            get { return _selectedSeriesColor; }
            set
            {
                _selectedSeriesColor = value;
                this.NotifyOfPropertyChange(() => this.SelectedSeriesColor);
            }
        }

        /// <summary>
        /// List of all the color options.
        /// </summary>
        public List<OxyColor> SeriesColorsList { get; protected set; }

        #endregion

        #region Series Remove

        /// <summary>
        /// Selected Series to remove from the plot.
        /// </summary>
        private Series _selectedRemoveSeries;
        /// <summary>
        /// Selected Series to remove from the plot.
        /// </summary>
        public Series SelectedRemoveSeries
        {
            get { return _selectedRemoveSeries; }
            set
            {
                _selectedRemoveSeries = value;
                this.NotifyOfPropertyChange(() => this.SelectedRemoveSeries);

                // Update the button
                //((DelegateCommand<object>)RemoveSeriesCommand).RaiseCanExecuteChanged();
            }
        }

        #endregion

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

                // Update all the plots with the new selection
                // Use UpdateBinSelection because it will do the samething
                // Use the EMPTY_BIN to refresh the plot
                //UpdateBinSelection(ProfileSeries.EMPTY_BIN);
            }
        }

        #endregion

        #region Plot Size

        /// <summary>
        /// Height of the plot.
        /// </summary>
        private int _plotHeight;
        /// <summary>
        /// Height of the plot.
        /// </summary>
        public int PlotHeight
        {
            get { return _plotHeight; }
            set
            {
                _plotHeight = value;
                this.NotifyOfPropertyChange(() => this.PlotHeight);
            }
        }

        /// <summary>
        /// Width of the plot.
        /// </summary>
        private int _plotWidth;
        /// <summary>
        /// Width of the plot.
        /// </summary>
        public int PlotWidth
        {
            get { return _plotWidth; }
            set
            {
                _plotWidth = value;
                this.NotifyOfPropertyChange(() => this.PlotWidth);
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
                UpdateSeriesVisibility(TAG_PROFILE_SERIES, value);
            }
        }

        /// <summary>
        /// This is a flag to turn on or off viewing the Minimum points series.
        /// </summary>
        private bool _IsMinSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Minimum points series.
        /// </summary>
        public bool IsMinSeriesOn
        {
            get { return _IsMinSeriesOn; }
            set
            {
                _IsMinSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsMinSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_MIN_SERIES, value);
            }
        }

        /// <summary>
        /// This is a flag to turn on or off viewing the Maximum points series.
        /// </summary>
        private bool _IsMaxSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Maximum points series.
        /// </summary>
        public bool IsMaxSeriesOn
        {
            get { return _IsMaxSeriesOn; }
            set
            {
                _IsMaxSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsMaxSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_MAX_SERIES, value);
            }
        }

        /// <summary>
        /// This is a flag to turn on or off viewing the Average series.
        /// </summary>
        private bool _IsAvgSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Average series.
        /// </summary>
        public bool IsAvgSeriesOn
        {
            get { return _IsAvgSeriesOn; }
            set
            {
                _IsAvgSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsAvgSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_AVG_SERIES, value);
            }
        }

        /// <summary>
        /// This is a flag to turn on or off viewing the Standard Deviation Ping to Ping series.
        /// </summary>
        private bool _IsStdP2PSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Standard Deviation Ping to Ping series.
        /// </summary>
        public bool IsStdP2PSeriesOn
        {
            get { return _IsStdP2PSeriesOn; }
            set
            {
                _IsStdP2PSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsStdP2PSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_STDP2P_SERIES, value);
            }
        }

        /// <summary>
        /// This is a flag to turn on or off viewing the Standard Deviation Bin to Bin series.
        /// This standard deviation is used for the velocity data to remove the difference in boat
        /// speed and the boat moving around.
        /// </summary>
        private bool _IsStdB2BSeriesOn;
        /// <summary>
        /// This is a flag to turn on or off viewing the Standard Deviation Bin to Bin series.
        /// This standard deviation is used for the velocity data to remove the difference in boat
        /// speed and the boat moving around.
        /// </summary>
        public bool IsStdB2BSeriesOn
        {
            get { return _IsStdB2BSeriesOn; }
            set
            {
                _IsStdB2BSeriesOn = value;
                this.NotifyOfPropertyChange(() => this.IsStdB2BSeriesOn);

                // Update the series visiblity
                UpdateSeriesVisibility(TAG_STDB2B_SERIES, value);
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
        public ProfilePlotViewModel(BaseSeriesType seriesType)
            : base(seriesType.Title)
        {
            // Initialize the plot
            //ID = DateTime.Now.ToString();                   // ID will the be the current date and time as a string
            ID = this.GetHashCode().ToString();
            _eventAggregator = IoC.Get<IEventAggregator>();
            _seriesType = seriesType;
            _plot = CreatePlot(_seriesType);
            PlotSeriesList = new BindingList<OxyPlot.Series.Series>();
            Title = _seriesType.ToString();
            _ensembleList = new LimitedList<DataSet.Ensemble>();
            _isFilterData = true;
            _isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<EnsWithMax>();
            _minMaxAvgStdSeriesList = new List<MinMaxAvgStdSeries>();
            IsProfileSeriesOn = true;
            IsMinSeriesOn = false;
            IsMaxSeriesOn = false;
            IsAvgSeriesOn = false;
            IsStdP2PSeriesOn = false;
            IsStdB2BSeriesOn = false;

            PlotHeight = DEFAULT_PLOT_HEIGHT;
            PlotWidth = DEFAULT_PLOT_WIDTH;

            // Commands
            // Create a command to remove this plot
            RemovePlotCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_RemovePlot()));                              // Remove this plot

            // Create a command to clear the plot
            ClearPlotCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_ClearPlot()));                                // Clear the plots

            // Create a command to add series
            AddSeriesCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_AddSeries()));                                // Add a new series to the plot 
            
            // Create a command to Remove series
            RemoveSeriesCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_RemoveSeries()));                          // Remove a series from the plot   

            // Create a command to generate a report of the plot.
            GenerateReportCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => GenerateReport()));                         // Generate a report for the plot 

            // Setup list options
            // MUST BE AFTER COMMANDS ARE CREATED (AddSeriesCommand)
            SetupListOptions(_seriesType);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {

        }

        #region Methods

        #region Incoming Data

        /// <summary>
        /// Add data to the plot to update the plot time series.
        /// </summary>
        /// <param name="ensemble">Latest data.</param>
        /// <param name="maxBins">Maximum number of bins to display.</param>
        public async Task AddIncomingData(DataSet.Ensemble ensemble, int maxBins)
        {
            EnsWithMax ewm = new EnsWithMax();
            ewm.Ensemble = ensemble;
            ewm.MaxBins = maxBins;

            _buffer.Enqueue(ewm);

            // Execute async
            if (!_isProcessingBuffer)
            {
                await Task.Run(() => AddIncomingDataExecute(ewm));
            }
        }

        /// <summary>
        /// Update the plot async.
        /// </summary>
        /// <param name="param">Ensemble and max bin.</param>
        private void AddIncomingDataExecute(object param)
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
                        UpdateEnsembleList(ewm.Ensemble, ewm.MaxBins);

                        // Update the list options
                        UpdateListOptions(ewm.Ensemble);

                        // Update the line series
                        UpdatePlot(ewm.Ensemble, ewm.MaxBins);
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
        /// <param name="seriesType">Series type.</param>
        /// <returns>Plot created based off the series type.</returns>
        private PlotModel CreatePlot(BaseSeriesType seriesType)
        {
            PlotModel temp = CreatePlot();

            // Set the plot title
            //temp.Title = seriesType.ToString();
            Title = _seriesType.ToString();
            temp.TitlePadding = 0;

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

            //temp.AutoAdjustPlotMargins = false;
            //temp.PlotMargins = new OxyThickness(0,0,0,20);
            //temp.Padding = new OxyThickness(10,0,10,0);

            //temp.Background = OxyColors.Black;
            temp.TextColor = OxyColors.White;
            temp.PlotAreaBorderColor = OxyColors.White;

            // Create line series and add to plot
            // No color for the series is specified
            //temp.Series.Add(new LineSeries(Beam0Color, 1, "Beam 1"));

            //temp.Title = "Time Series";

            // Set the legend position
            temp.IsLegendVisible = true;
            temp.LegendPosition = LegendPosition.BottomCenter;
            temp.LegendPlacement = LegendPlacement.Inside;
            temp.LegendOrientation = LegendOrientation.Horizontal;
            //temp.LegendSymbolPlacement = LegendSymbolPlacement.Right;
            temp.LegendFontSize = 10;
            temp.LegendItemSpacing = 8;

            // For the correlation plot, display the depth for the axis
            // For the rest of the plots use the bin.
            if (_seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Correlation)
            {
                // Setup the axis
                temp.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    StartPosition = 1,                                                  // This will invert the axis to start at the top with minimum value
                    EndPosition = 0,
                    TicklineColor = OxyColors.White,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
                    MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White),
                    TickStyle = OxyPlot.Axes.TickStyle.Inside,                               // Put tick lines inside the plot
                    //MinimumPadding = 0.1,                                                 // Start at axis edge   
                    //MaximumPadding = 0.1,                                                 // Start at axis edge
                    //IsAxisVisible = true,
                    //MajorStep = 1,
                    //Minimum = 0,
                    IntervalLength = 20,
                    Unit = "m"
                });
            }
            else
            {
                // Setup the axis
                temp.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    StartPosition = 1,                                                  // This will invert the axis to start at the top with minimum value
                    EndPosition = 0,
                    TicklineColor = OxyColors.White,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
                    MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White),
                    TickStyle = OxyPlot.Axes.TickStyle.Inside,                               // Put tick lines inside the plot
                    //MinimumPadding = 0.1,                                                 // Start at axis edge   
                    //MaximumPadding = 0.1,                                                 // Start at axis edge
                    //IsAxisVisible = true,
                    //MajorStep = 1,
                    Minimum = 0,
                    IntervalLength = 20,
                    Unit = "Bin"
                });
            }

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
            switch (_seriesType.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:                 // Beam Velocity data
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:                  // Instrument Velocity data
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:                  // Earth Velocity data
                    //temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 0.5, "m/s"));
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:                     // Amplitude data
                    LinearAxis axisAmp = CreatePlotAxis(AxisPosition.Bottom, 20, "dB");
                    axisAmp.Minimum = 0;
                    axisAmp.Maximum = 120;
                    temp.Axes.Add(axisAmp);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:                   // Correlation data
                    LinearAxis axisCorr = CreatePlotAxis(AxisPosition.Bottom, 20, "%");
                    axisCorr.Minimum = 0;
                    axisCorr.Maximum = 100;
                    temp.Axes.Add(axisCorr);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_SNR:                           // SNR data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 0.5, "dB"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Range:                         // Range data
                    LinearAxis axisRange = CreatePlotAxis(AxisPosition.Bottom, 5, "m");
                    axisRange.Minimum = 0;
                    axisRange.StartPosition = 1;                                        // This will invert the axis to start at the top with minimum value
                    axisRange.EndPosition = 0;                                          // This will invert the axis to start at the top with minimum value
                    temp.Axes.Add(axisRange);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:                       // Heading data
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:                         // Pitch data
                case BaseSeriesType.eBaseSeriesType.Base_Roll:                          // Roll data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "Deg"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pressure:                      // Pressure data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "Pa"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_TransducerDepth:               // Transducer Depth Pressure data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "m"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:               // System Temperature data
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:             // Water Temperature data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Bottom, 1, "Deg F"));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Create the plot axis.  Set the values for the plot axis.
        /// If you do not want to set a value, set the value to NULL.
        /// </summary>
        /// <param name="position">Position of the axis.</param>
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
            axis.MinimumPadding = 0;                                                 // Start at axis edge   
            axis.MaximumPadding = 0;                                                 // Start at axis edge

            // Axis position
            // Usually AxisPosition.Bottom or
            // Axis.Position.Left
            axis.Position = position;

            // Set the Minimum value for the axis
            //axis.Minimum = min;

            // Set the axis label
            axis.Unit = unit;

            return axis;
        }

        /// <summary>
        /// Create the plot axis.  Set the values for the plot axis.
        /// If you do not want to set a value, set the value to NULL.
        /// </summary>
        /// <param name="position">Position of the axis.</param>
        /// <param name="majorStep">Axis step.</param>
        /// <param name="unit">Label for the axis.</param>
        /// <returns>LinearAxis for the plot.</returns>
        private LinearAxis CreatePlotAxis(AxisPosition position, double majorStep, string unit)
        {
            // Create the axis
            LinearAxis axis = new LinearAxis();

            // Standard options
            axis.TicklineColor = OxyColors.White;
            axis.MajorGridlineStyle = LineStyle.Solid;
            axis.MinorGridlineStyle = LineStyle.Solid;
            axis.MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White);
            axis.MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White);
            axis.MinimumPadding = 0;                                                 // Start at axis edge   
            axis.MaximumPadding = 0;                                                 // Start at axis edge
            axis.MajorStep = majorStep;

            // Axis position
            // Usually AxisPosition.Bottom or
            // Axis.Position.Left
            axis.Position = position;

            // Set the Minimum value for the axis
            //axis.Minimum = min;

            // Set the axis label
            axis.Unit = unit;

            return axis;
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
            // Add the latest ensemble to the list
            _ensembleList.Add(ens);
        }

        /// <summary>
        /// Update the plot with the latest ensemble data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxBins">Maximum number of bins in the series.</param>
        private void UpdatePlot(DataSet.Ensemble ensemble, int maxBins)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Update the time series with the latest data
                foreach (var series in Plot.Series)
                {
                    // Verify the type of series
                    if (series.GetType() == typeof(ProfileSeries))
                    {
                         //Update the series
                        ((ProfileSeries)series).UpdateSeries(ensemble, maxBins, _isFilterData);

                    }

                    // Add the updated series to the plot
                    //Plot.Series.Add(series.LineSeries);
                }

                // Update min,max,avg,std
                foreach (var mSeries in _minMaxAvgStdSeriesList)
                {
                    mSeries.UpdateSeries(ensemble, maxBins, _isFilterData);
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
                // Clear the data for each series.
                foreach (var series in Plot.Series)
                {
                    if (series.GetType() == typeof(ProfileSeries))
                    {
                        ((ProfileSeries)series).ClearSeries();
                    }
                }

                foreach (var mSeries in _minMaxAvgStdSeriesList)
                {
                    mSeries.ClearSeries();
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region List Options

        /// <summary>
        /// Setup the list of options available to the user based off the
        /// series type.  This will enable and disable list and populate the
        /// list with the correct values based off the series type.
        /// </summary>
        /// <param name="seriesType">Series type.</param>
        private void SetupListOptions(BaseSeriesType seriesType)
        {
            // Set the DataSet List
            DataSetTypeList = ProfileType.GetDataSetTypeList(seriesType.Code);
            if (DataSetTypeList.Count > 0)
            {
                SelectedDataSetType = DataSetTypeList[0];
            }

            // Set the Subsystem config list
            SubsystemConfigList = new ObservableCollection<AdcpSubsystemConfig>();

            // Set the max bin
            MaxBin = DEFAULT_MAX_BIN;
            SelectedMaxBins = 0;
            MaxBeam = DEFAULT_MAX_BEAM - 1;
            SelectedBeam = 0;

            // Set the list of colors
            SeriesColorsList = BeamColor.GetBeamColorList();

            // Set the default selected color
            if (SeriesColorsList.Count > 0)
            {
                SelectedSeriesColor = SeriesColorsList[0];
            }
        }

        /// <summary>
        /// Update all the list based off the new ensemble data given.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        private void UpdateListOptions(DataSet.Ensemble ensemble)
        {
            // Update the max bin
            if (ensemble.EnsembleData.NumBins > _maxBin || _maxBin == DEFAULT_MAX_BIN)
            {
                // Subtract 1 because it is zero based
                MaxBin = ensemble.EnsembleData.NumBins - 1;
                SelectedMaxBins = ensemble.EnsembleData.NumBins - 1;
            }

            // Update the max beam
            if (ensemble.EnsembleData.NumBeams > _maxBeam)
            {
                // Subtract 1 because it is zero based
                MaxBeam = ensemble.EnsembleData.NumBeams - 1;
            }
        }

        /// <summary>
        /// Clear the list.  This will reset all the list 
        /// back to default.
        /// </summary>
        private void ClearListOptions()
        {
            SetupListOptions(_seriesType);
        }

        #endregion

        #region Selections

        /// <summary>
        /// Test whether all the selections have been made.
        /// Some selections are not necessary so this will
        /// validate only the necessary selections based 
        /// off the series type.
        /// </summary>
        /// <returns>TRUE =  All selections have been made.</returns>
        private bool TestSelections()
        {
            return true;
        }

        /// <summary>
        /// Based off the beam selected, this will help the
        /// user choose a default color for the beam number.
        /// The user can still change the selection, but this
        /// will be made automatically when the beam number changes.
        /// </summary>
        /// <param name="selectedBeam">Beam number selected.</param>
        private void SetDefaultBeamColor(int selectedBeam)
        {
            // Look for all the types that have beams as series
            if (_seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam ||
                _seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ ||
                _seriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU )
            {
                // Set the beam color for the line series
                switch (selectedBeam)
                {
                    case 0:
                        SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0);
                        break;
                    case 1:
                        SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1);
                        break;
                    case 2:
                        SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2);
                        break;
                    case 3:
                        SelectedSeriesColor = OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_3);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Add Series

        /// <summary>
        /// Add a series to the plot.
        /// Use ProfileSeries.MAX_BIN if you do not want to sent a maxBins.
        /// </summary>
        /// <param name="type">Series type.</param>
        /// <param name="beam">Beam selected.</param>
        /// <param name="maxBins">Maximum number of bins to display.</param>
        /// <param name="color">Color of the plot.</param>
        public void AddSeries(ProfileType type, int beam, OxyColor color, int maxBins = ProfileSeries.MAX_BIN)
        {
            // Create Min, Max, Avg, Std series
            // and add it to the list
            var minMaxAvgStdSeries = new MinMaxAvgStdSeries(type, beam, color, _isFilterData, maxBins, _ensembleList);
            _minMaxAvgStdSeriesList.Add(minMaxAvgStdSeries);

            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Add the series to the list
                // Create a series
                ProfileSeries series = new ProfileSeries(type, beam, color, _isFilterData, maxBins, _ensembleList);
                series.Tag = TAG_PROFILE_SERIES;
                series.IsVisible = _IsProfileSeriesOn;
                Plot.Series.Add(series);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(series)));

                minMaxAvgStdSeries.MinPoints.Tag = TAG_MIN_SERIES;
                minMaxAvgStdSeries.MinPoints.IsVisible = _IsMinSeriesOn;
                Plot.Series.Add(minMaxAvgStdSeries.MinPoints);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.MinPoints)));

                minMaxAvgStdSeries.MaxPoints.Tag = TAG_MAX_SERIES;
                minMaxAvgStdSeries.MaxPoints.IsVisible = _IsMaxSeriesOn;
                Plot.Series.Add(minMaxAvgStdSeries.MaxPoints);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.MaxPoints)));

                minMaxAvgStdSeries.AvgPoints.Tag = TAG_AVG_SERIES;
                minMaxAvgStdSeries.AvgPoints.IsVisible = _IsAvgSeriesOn;
                Plot.Series.Add(minMaxAvgStdSeries.AvgPoints);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.AvgPoints)));

                minMaxAvgStdSeries.StdP2PPoints.Tag = TAG_STDP2P_SERIES;
                minMaxAvgStdSeries.StdP2PPoints.IsVisible = _IsStdP2PSeriesOn;
                Plot.Series.Add(minMaxAvgStdSeries.StdP2PPoints);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.StdP2PPoints)));

                minMaxAvgStdSeries.StdB2BPoints.Tag = TAG_STDB2B_SERIES;
                minMaxAvgStdSeries.StdB2BPoints.IsVisible = _IsStdB2BSeriesOn;
                Plot.Series.Add(minMaxAvgStdSeries.StdB2BPoints);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(minMaxAvgStdSeries.StdB2BPoints)));
            }

            // Then refresh the plot
            Plot.InvalidatePlot(true);
        }

        #endregion

        #region Remove Series

        /// <summary>
        /// Remove the given time series from the plot.
        /// 
        /// This will remove the series from the plot.  It
        /// will then deselect the series.  It will then refresh
        /// the plot with the series removed.
        /// </summary>
        /// <param name="series">Series to remove.</param>
        public void RemoveSeries(OxyPlot.Series.Series series)
        {
            if(series == null)
            {
                return;
            }

            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Remove the series from the plot
                Plot.Series.Remove(series);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Remove(series)));

                // Remove the selection
                SelectedRemoveSeries = null;

                // Clear all the axis if there are no series
                if (Plot.Series.Count <= 0)
                {
                    for (int x = 0; x < Plot.Axes.Count; x++)
                    {
                        // If the axis is on the left side, this is
                        // the axis for the incoming data
                        // Remove it since there is no data associated 
                        // with the plot
                        if (Plot.Axes[x].Position == AxisPosition.Left)
                        {
                            Plot.Axes.RemoveAt(x);
                        }
                    }

                    // Remove the title
                    Plot.Title = "";
                }
            }

            // Then refresh the plot
            Plot.InvalidatePlot(true);
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
            foreach(var series in Plot.Series)
            {
                // Look for the series that matches
                if(series.Tag == tag)
                {
                    series.IsVisible = value;
                }
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// Determine if the 2 views given are the equal.
        /// </summary>
        /// <param name="view1">First series to check.</param>
        /// <param name="view2">Series to check against.</param>
        /// <returns>True if there codes match.</returns>
        public static bool operator ==(ProfilePlotViewModel view1, ProfilePlotViewModel view2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(view1, view2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)view1 == null) || ((object)view2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (view1.ID == view2.ID);
        }

        /// <summary>
        /// Return the opposite of ==.
        /// </summary>
        /// <param name="view1">First view to check.</param>
        /// <param name="view2">View to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(ProfilePlotViewModel view1, ProfilePlotViewModel view2)
        {
            return !(view1 == view2);
        }

        /// <summary>
        /// Create a hashcode based off the Code stored.
        /// </summary>
        /// <returns>Hash the Code.</returns>
        public override int GetHashCode()
        {
            // Use the date time and kind to represent a better hash code.
            // If it juse used the time, then if the 2 objects where created
            // close to each other, they would have the same hash code.
            // This gives a little more to the hash code then just datetime.
            DateTime when = DateTime.Now;
            ulong kind = (ulong)(int)when.Kind;
            return (int)((kind << 62) | (ulong)when.Ticks);
        }

        /// <summary>
        /// Check if the given object is 
        /// equal to this object.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>If the codes are the same, then they are equal.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            ProfilePlotViewModel p = (ProfilePlotViewModel)obj;

            return (ID == p.ID);
        }


        #endregion

        #region Report

        /// <summary>
        /// Generate a report of the plot.
        /// </summary>
        public void GenerateReport(string filename = "", bool isFiltered = true)
        {
            var reportStyle = new OxyPlot.Reporting.ReportStyle();

            // Make a copy of the plot for a white background
            PlotModel plot = new PlotModel();
            // Set new colors
            plot.TextColor = OxyColors.Black;
            plot.PlotAreaBorderColor = OxyColors.Black;

            // Copy data
            foreach (var series in _plot.Series)
            {
                // Verify the type of series
                if (series.GetType() == typeof(ProfileSeries))
                {
                    var newSeries = new ProfileSeries(((ProfileSeries)series).Type, ((ProfileSeries)series).Beam, ((ProfileSeries)series).Color, isFiltered, ((ProfileSeries)series).MaxBins, null);
                    newSeries.IsVisible = ((ProfileSeries)series).IsVisible;
                    foreach (var pt in ((ProfileSeries)series).Points)
                    {
                        newSeries.Points.Add(pt);
                    }

                    plot.Series.Add(newSeries);
                }
                if (series.GetType() == typeof(ScatterSeriesWithToString))
                {
                    var newSeries = new ScatterSeriesWithToString(((ScatterSeriesWithToString)series).Title, ((ScatterSeriesWithToString)series).MarkerFill, (int)((ScatterSeriesWithToString)series).MarkerSize);
                    newSeries.Tag = ((ScatterSeriesWithToString)series).Tag;
                    newSeries.IsVisible = ((ScatterSeriesWithToString)series).IsVisible;
                    foreach(var pt in ((ScatterSeriesWithToString)series).Points)
                    {
                        newSeries.Points.Add(pt);
                    }

                    plot.Series.Add(newSeries);
                }
                if (series.GetType() == typeof(LineSeriesWithToString))
                {
                    var newSeries = new LineSeriesWithToString(((LineSeriesWithToString)series).Title, ((LineSeriesWithToString)series).Color);
                    newSeries.Tag = ((LineSeriesWithToString)series).Tag;
                    newSeries.IsVisible = ((LineSeriesWithToString)series).IsVisible;
                    foreach (var pt in ((LineSeriesWithToString)series).Points)
                    {
                        newSeries.Points.Add(pt);
                    }

                    plot.Series.Add(newSeries);
                }
            }

            // Copy Axis but change axis color lines
            foreach (var axis in _plot.Axes)
            {
                var newAxis = new LinearAxis();
                newAxis.Position = axis.Position;
                newAxis.TickStyle = axis.TickStyle;
                newAxis.MinimumPadding = axis.MinimumPadding;
                newAxis.MaximumPadding = axis.MaximumPadding;
                newAxis.Unit = axis.Unit;
                newAxis.MajorGridlineStyle = axis.MajorGridlineStyle;
                newAxis.MinorGridlineStyle = axis.MinorGridlineStyle;
                newAxis.IntervalLength = axis.IntervalLength;
                newAxis.Minimum = axis.Minimum;
                newAxis.Maximum = axis.Maximum;
                newAxis.StartPosition = axis.StartPosition;
                newAxis.EndPosition = axis.EndPosition;
                newAxis.Key = axis.Key;
                newAxis.TitleColor = axis.TitleColor;
                newAxis.TextColor = axis.TextColor;
                newAxis.MinorStep = axis.MinorStep;
                newAxis.PositionTier = axis.PositionTier;

                // Set new colors
                newAxis.TicklineColor = OxyColors.LightGray;
                newAxis.MajorGridlineColor = OxyColors.Gray;
                newAxis.MinorGridlineColor = OxyColors.LightGray;

                plot.Axes.Add(newAxis);
            }

            // Create report
            var report = new OxyPlot.Reporting.Report();
            string title = string.Format("{0}", Title);
            report.AddPlot(plot, title, 800, 600);

            // Set file name
            DateTime currDateTime = DateTime.Now;
            if (string.IsNullOrEmpty(filename))
            {
                filename = string.Format("{0}\\{1}_{2:yyyyMMddHHmmss}.html", Pulse.Commons.DEFAULT_RECORD_DIR, title, currDateTime);
            }

            // Write report
            using (var s = File.Create(filename))
            {
                using (var w = new OxyPlot.Reporting.HtmlReportWriter(s))
                {
                    w.WriteReport(report, reportStyle);
                }
            }
        }

        #endregion

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
            ClearIncomingData();
        }

        #endregion

        #region Add Series Command

        /// <summary>
        /// Add a series to the plot.  This is a line
        /// for specific options selected.
        /// </summary>
        private void On_AddSeries()
        {
            // Add a series to the plot
            AddSeries(_selectedDataSetType, _selectedBeam, _selectedSeriesColor, _selectedMaxBins);

            // If there is only 1 axis
            // Then all the axis have been removed except the bottom one for the ensemble number
            // Add a new axis and set the new title
            if (Plot.Axes.Count == 1)
            {
                // Set the new plot axis for the new data
                SetPlotAxis(ref _plot);

                // Set the title for based off the last data added
                Plot.Title = _seriesType.ToString();
            }
        }

        #endregion

        #region Remove Series Command

        /// <summary>
        /// Remove a series frome the plot.  This is a line
        /// for specific options selected.
        /// </summary>
        private void On_RemoveSeries()
        {
            // Add a series to the plot
            RemoveSeries(_selectedRemoveSeries);
        }

        #endregion

        #endregion
    }
}
