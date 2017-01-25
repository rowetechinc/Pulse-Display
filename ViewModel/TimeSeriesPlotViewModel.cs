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
 * 11/27/2012      RC          2.17       Initial coding
 * 12/12/2012      RC          2.17       Added IsFilterData to allow the user to choose to filter the data.
 * 12/28/2012      RC          2.17       Moved AdcpSubsystemConfig.Subsystem into AdcpSubsystemConfig.SubsystemConfig.Subsystem.
 * 02/13/2013      RC          2.18       Added property, IsFixBin to not take bin selections if set true.  Now check for IsFixBin in UpdateBinSelection().
 * 02/22/2013      RC          2.18       Added a Try/Catch in UpdatePlot() to check if the BackgroundWorker is busy when trying to use it.  This happens when update is fast from DVL data.
 * 03/11/2013      RC          2.18       Removed the background workers and used the Dispatcher thread to update the plots.
 * 07/16/2013      RC          3.0.4      Changed SeriesType.
 * 09/03/2013      RC          3.0.9      Added RemoveAllSeriesCommand command to remove all the series at once.
 * 09/12/2013      RC          3.1.0      Removed backgroundworkers and replaced with ReactiveAsyncCommands.
 * 12/09/2013      RC          3.2.0      Added IntervalLength = 20 to data axis to ensure the axis display values.
 * 01/14/2014      RC          3.2.3      Maded AddIncomingData() work async.  The data is buffered when it is recieved so the event handler can return immediately.
 * 01/16/2014      RC          3.2.3      Changed SetDefaultBeamColor() to convert the default colors to an OxyColor.
 * 08/04/2014      RC          3.4.0      Fixed bug with refreshing the plot.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 10/07/2014      RC          4.1.0      Added Bottom Track speed and Water Track plots.
 * 03/02/2015      RC          4.1.0      Added SystemSetup and Range Tracking.
 * 11/24/2015      RC          4.3.1      Added Magnitude and Direction and Speed.
 * 11/25/2015      RC          4.3.1      Added NMEA Heading and speed.
 * 12/04/2015      RC          4.4.0      Added DVL data to TimeSeries.  This includes Ship Velocity.
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
    using log4net;
    using OxyPlot.Axes;
    using ReactiveUI;
    using Caliburn.Micro;
    using System.Linq;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.IO;

    /// <summary>
    /// Create TimeSeries Plot control to display time series plots.
    /// </summary>
    [Export]
    public class TimeSeriesPlotViewModel : PulseViewModel
    {

        #region Variable

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Defaults

        /// <summary>
        /// Default height of the plot in pixels.
        /// </summary>
        private int DEFAULT_PLOT_HEIGHT = 180;

        /// <summary>
        /// Default width of the plot in pixels.
        /// </summary>
        private int DEFAULT_PLOT_WIDTH = 825;       // 420

        /// <summary>
        /// Default number for maximum number of beams.
        /// </summary>
        private int DEFAULT_MAX_BEAM = 4;

        /// <summary>
        /// Default number of maximum bins.
        /// </summary>
        private int DEFAULT_MAX_BIN = DataSet.Ensemble.MAX_NUM_BINS;

        #endregion

        /// <summary>
        /// Receive global events from the EventAggregator.
        /// </summary>
        private IEventAggregator _eventAggregator;

        /// <summary>
        /// List of all the series for this plot.
        /// </summary>
        private List<TimeSeries> _seriesList;

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

        #endregion

        #region Selection

        #region Bin

        /// <summary>
        /// Set flag if the Bin selection should be visible.
        /// TRUE = visible.
        /// </summary>
        private bool _IsBinSelectionVisible;
        /// <summary>
        /// Set flag if the Bin selection should be visible.
        /// TRUE = visible.
        /// </summary>
        public bool IsBinSelectionVisible
        {
            get { return _IsBinSelectionVisible; }
            set
            {
                _IsBinSelectionVisible = value;
                this.NotifyOfPropertyChange(() => this.IsBinSelectionVisible);
            }
        }

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
        private int _selectedBin;
        /// <summary>
        /// Selected bin for the plot.  If the plot data
        /// is dependent on the bin number, use this
        /// value to determine which bin was selected.
        /// </summary>
        public int SelectedBin
        {
            get { return _selectedBin; }
            set
            {
                _selectedBin = value;
                this.NotifyOfPropertyChange(() => this.SelectedBin);

                // Update the add series command
                //((DelegateCommand<object>)AddSeriesCommand).RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Beam

        /// <summary>
        /// Set flag if the Beam selection should be visible.
        /// TRUE = visible.
        /// </summary>
        private bool _IsBeamSelectionVisible;
        /// <summary>
        /// Set flag if the Beam selection should be visible.
        /// TRUE = visible.
        /// </summary>
        public bool IsBeamSelectionVisible
        {
            get { return _IsBeamSelectionVisible; }
            set
            {
                _IsBeamSelectionVisible = value;
                this.NotifyOfPropertyChange(() => this.IsBeamSelectionVisible);
            }
        }

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

        #region Series Type

        /// <summary>
        /// Series type for this plot.
        /// This will be the current series to use for the plot.
        /// </summary>
        private SeriesType _SelectedSeriesType;
        /// <summary>
        /// Series type for this plot.
        /// </summary>
        public SeriesType SelectedSeriesType
        {
            get { return _SelectedSeriesType; }
            set
            {
                _SelectedSeriesType = value;
                this.NotifyOfPropertyChange(() => this.SelectedSeriesType);
            }
        }

        #endregion

        #region Data Source

        /// <summary>
        /// List of all the data sources.
        /// </summary>
        private BindingList<DataSource> _DataSourceList;
        /// <summary>
        /// List of all the data sources.
        /// </summary>
        public BindingList<DataSource> DataSourceList
        {
            get { return _DataSourceList; }
            set
            {
                _DataSourceList = value;
                this.NotifyOfPropertyChange(() => this.DataSourceList);
            }
        }

        /// <summary>
        /// The selected data source.
        /// Used to add a new series.  To get the current
        /// data source, use SelectedSeriesType.Source.
        /// </summary>
        private DataSource _SelectedDataSource;
        /// <summary>
        /// The selected data source.
        /// Used to add a new series.  To get the current
        /// data source, use SelectedSeriesType.Source.
        /// </summary>
        public DataSource SelectedDataSource
        {
            get { return _SelectedDataSource; }
            set
            {
                _SelectedDataSource = value;
                this.NotifyOfPropertyChange(() => this.SelectedDataSource);

                // Create the Base Series type based off the source choosen
                BaseSeriesTypeList = BaseSeriesType.GetTimeSeriesList(_SelectedDataSource.Source);
                this.NotifyOfPropertyChange(() => this.BaseSeriesTypeList);

                // Set flag if Beam and Bin selection is Visible
                SetFlagBeamBinSelectionVisible();
            }
        }

        #endregion

        #region Base Series Type

        /// <summary>
        /// List of all the base series.
        /// </summary>
        public BindingList<BaseSeriesType> BaseSeriesTypeList { get; set; }

        /// <summary>
        /// The series type of this plot.  This will determine
        /// what the axis labels and axis scales will be.
        /// Used to add a new series.  To get the current
        /// data BaseSeriesType, use SelectedSeriesType.Type.
        /// </summary>
        private BaseSeriesType _SelectedBaseSeriesType;
        /// <summary>
        /// The series type of this plot.  This will determine
        /// what the axis labels and axis scales will be.
        /// Used to add a new series.  To get the current
        /// data BaseSeriesType, use SelectedSeriesType.Type.
        /// </summary>
        public BaseSeriesType SelectedBaseSeriesType
        {
            get { return _SelectedBaseSeriesType; }
            set
            {
                _SelectedBaseSeriesType = value;
                this.NotifyOfPropertyChange(() => this.SelectedBaseSeriesType);

                // Set flag if Beam and Bin selection is Visible
                SetFlagBeamBinSelectionVisible();
            }
        }

        #endregion

        #region Coordinate Transform

        /// <summary>
        /// List of all the coordinate transform.
        /// </summary>
        private BindingList<Core.Commons.Transforms> _CoordinateTransformList;
        /// <summary>
        /// List of all the coordinate transform.
        /// </summary>
        public BindingList<Core.Commons.Transforms> CoordinateTransformList
        {
            get { return _CoordinateTransformList; }
            set
            {
                _CoordinateTransformList = value;
                this.NotifyOfPropertyChange(() => this.CoordinateTransformList);
            }
        }

        /// <summary>
        /// The selected coordinate transform.
        /// </summary>
        private Core.Commons.Transforms _SelectedCoordinateTransform;
        /// <summary>
        /// The selected coordinate transform.
        /// </summary>
        public Core.Commons.Transforms SelectedCoordinateTransform
        {
            get { return _SelectedCoordinateTransform; }
            set
            {
                _SelectedCoordinateTransform = value;
                this.NotifyOfPropertyChange(() => this.SelectedCoordinateTransform);

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
        private TimeSeries _selectedRemoveSeries;
        /// <summary>
        /// Selected Series to remove from the plot.
        /// </summary>
        public TimeSeries SelectedRemoveSeries
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
                UpdateBinSelection(TimeSeries.EMPTY_BIN);
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

        #endregion

        #region Commands

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
        /// Command to remove all the series from the plot.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> RemoveAllSeriesCommand { get; protected set; }

        /// <summary>
        /// Command to generate a report..
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> GenerateReportCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public TimeSeriesPlotViewModel(SeriesType type)
            : base(type.Title)
        {
            // Initialize the plot
            ID = this.GetHashCode().ToString();
            _eventAggregator = IoC.Get<IEventAggregator>();

            SelectedSeriesType = type;
            SelectedDataSource = type.Source;
            SelectedBaseSeriesType = type.Type;
            PlotSeriesList = new BindingList<OxyPlot.Series.Series>();
            _plot = CreatePlot(type);
            _seriesList = new List<TimeSeries>();
            _ensembleList = new List<DataSet.Ensemble>();
            _isFilterData = true;
            _isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<EnsWithMax>();

            PlotHeight = DEFAULT_PLOT_HEIGHT;
            PlotWidth = DEFAULT_PLOT_WIDTH;

            // Commands
            // Create a command to clear the plot
            ClearPlotCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_ClearPlot()));                                // Clear the plots

            // Create a command to add series
            AddSeriesCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_AddSeries()));                                // Add a new series to the plot   

            // Create a command to Remove series
            RemoveSeriesCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_RemoveSeries()));                          // Remove a series from the plot                                                                           

            // Create a command to Remove series
            RemoveAllSeriesCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_RemoveAllSeries()));                    // Remove a series from the plot  

            // Create a command to generate a report of the plot.
            GenerateReportCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => GenerateReport()));                         // Generate a report for the plot  

            // Setup list options
            // MUST BE AFTER COMMANDS ARE CREATED (AddSeriesCommand)
            SetupListOptions(_SelectedSeriesType.Type);
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
        /// Add the series data.  This is used to add data already processed and 
        /// not live data.
        /// </summary>
        /// <param name="series">Series data to add to the plot.</param>
        public void AddSeries(OxyPlot.Series.LineSeries series)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Add the series
                Plot.Series.Add(series);
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

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
                                log.Debug("Error updating Time Series Plot", ex);
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

        /// <summary>
        /// Update the bin selection for all the 
        /// series.
        /// </summary>
        /// <param name="bin">New bin selected.</param>
        public void UpdateBinSelection(int bin)
        {
            // Update the line series
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => UpdateSeriesBinSelection(bin)));
            }
            catch (Exception ex)
            {
                // When shutting down, can get a null reference
                log.Debug("Error updating the bin selection for the Time Series Plot", ex);
            }

        }

        /// <summary>
        /// Update all the series with the latest bin selection.
        /// </summary>
        /// <param name="bin">Bin Selected.</param>
        private void UpdateSeriesBinSelection(int bin)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Update the time series with the latest data
                foreach (TimeSeries series in Plot.Series)
                {
                    if (TimeSeries.IfSeriesHaveBins(series.Type))
                    {
                        // Update the series with the new bin and ensemble list
                        series.UpdateBinSelection(bin, _ensembleList, _isFilterData);
                    }
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
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
        private PlotModel CreatePlot(SeriesType seriesType)
        {
            PlotModel temp = CreatePlot();

            // Set the plot title
            temp.Title = seriesType.ToString();

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
            //temp.PlotMargins = new OxyThickness(0, 0, 0, 0);
            //temp.Padding = new OxyThickness(10,0,10,0);

            temp.TitleFontSize = 10.667;

            //temp.Background = OxyColors.Black;
            temp.TextColor = OxyColors.White;
            temp.PlotAreaBorderColor = OxyColors.White;

            // Set the legend position
            temp.IsLegendVisible = true;
            //temp.LegendPosition = LegendPosition.RightTop;
            //temp.LegendPlacement = LegendPlacement.Outside;
            temp.LegendOrientation = LegendOrientation.Vertical;
            //temp.LegendSymbolPlacement = LegendSymbolPlacement.Right;
            temp.LegendFontSize = 8;   // 10
            //temp.LegendItemSpacing = 8;
            
            // Setup the axis
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //MajorStep = 1,
                //Minimum = 0,
                //Maximum = _maxDataSets,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
                MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White),
                TickStyle = OxyPlot.Axes.TickStyle.Inside,                               // Put tick lines inside the plot
                MinimumPadding = 0,                                                 // Start at axis edge   
                MaximumPadding = 0,                                                 // Start at axis edge
                //IsAxisVisible = true,
                //MajorStep = 1,
                Unit = "ENS"

            });

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
            switch (_SelectedSeriesType.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:             // Beam Velocity data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.4, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:              // Instrument Velocity data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.4, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Ship:              // Ship Velocity data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.4, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:              // Earth Velocity data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:                 // Amplitude data
                    LinearAxis axisAmp = CreatePlotAxis(AxisPosition.Left, 20, "dB");
                    axisAmp.Minimum = 0;
                    axisAmp.Maximum = 120;
                    temp.Axes.Add(axisAmp);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:               // Correlation data
                    LinearAxis axisCorr = CreatePlotAxis(AxisPosition.Left, 20, "%");
                    axisCorr.Minimum = 0;
                    axisCorr.Maximum = 100;
                    temp.Axes.Add(axisCorr);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_SNR:                       // SNR data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.5, "dB"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Range:                     // Range data
                    LinearAxis axisRange = CreatePlotAxis(AxisPosition.Left, 5, "m");
                    axisRange.Minimum = 0;
                    axisRange.StartPosition = 1;                                    // This will invert the axis to start at the top with minimum value
                    axisRange.EndPosition = 0;                                      // This will invert the axis to start at the top with minimum value
                    temp.Axes.Add(axisRange);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:                     // Pitch
                case BaseSeriesType.eBaseSeriesType.Base_Roll:                      // Roll
                    LinearAxis hprAxis = CreatePlotAxis(AxisPosition.Left, 90, "Deg");  // Intervals at 90
                    hprAxis.Minimum = -180;
                    hprAxis.Maximum = 180;
                    temp.Axes.Add(hprAxis);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:                       // Heading
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Heading:                  // NMEA Heading
                case BaseSeriesType.eBaseSeriesType.Base_Water_Direction:               // Water Direction
                    LinearAxis dirAxis = CreatePlotAxis(AxisPosition.Left, 90, "Deg");  // Intervals at 90
                    dirAxis.Minimum = 0;
                    dirAxis.Maximum = 360;
                    temp.Axes.Add(dirAxis);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pressure:                  // Pressure data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1, "Pa"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_TransducerDepth:           // Transducer Depth Pressure data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1, "m"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:           // System Temperature data
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:         // Water Temperature data
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1, "Deg C"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Speed:                     // Speed
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.25, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Pings:       // Pings
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "pings"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Range:       // Vertical Range
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_SNR:         // SNR
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "SNR"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_SystemSetup_Voltage:       // Voltage
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "volt"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_East_Vel:            // Waves East Velocity
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_North_Vel:           // Waves East Velocity
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Pressure_And_Height: // Waves Pressure and Height
                    temp.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.25, "m"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_FFT:                 // Waves FFT
                    temp.Axes.Clear();
                    var fft = CreatePlotAxis(AxisPosition.Left, 1.0, "m^2/Hz");
                    fft.Maximum = 100;
                    temp.Axes.Add(fft);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Frequency:           // Waves Frequency
                    temp.Axes.Clear();
                    var btA = CreatePlotAxis(AxisPosition.Bottom, 1.0, "Frequency (Hz)");
                    btA.Minimum = 0;
                    temp.Axes.Add(btA);
                    var r1A = CreatePlotAxis(AxisPosition.Right, 90.0, "deg");
                    r1A.Minimum = 0;
                    r1A.Maximum = 360;
                    r1A.Key = "dir";
                    r1A.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    r1A.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    temp.Axes.Add(r1A);
                    var r2A = CreatePlotAxis(AxisPosition.Right, 20.0, "deg");
                    r2A.PositionTier = 1;
                    r2A.Minimum = 0;
                    r2A.Maximum = 80;
                    r2A.Key = "spr";
                    r2A.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    r2A.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    temp.Axes.Add(r2A);
                    var l1A = CreatePlotAxis(AxisPosition.Left, 20.0, "m");
                    l1A.PositionTier = 1;
                    l1A.Minimum = 0;
                    l1A.Maximum = 4;
                    l1A.Key = "sp";
                    l1A.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    l1A.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    temp.Axes.Add(l1A);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Period:           // Waves Period
                    temp.Axes.Clear();
                    var wpB = CreatePlotAxis(AxisPosition.Bottom, 1.0, "period (sec)");
                    wpB.Minimum = 0;
                    wpB.Maximum = 30;
                    wpB.StartPosition = 1;
                    wpB.EndPosition = 0;
                    temp.Axes.Add(wpB);
                    var wpR = CreatePlotAxis(AxisPosition.Right, 90.0, "deg");
                    wpR.Minimum = 0;
                    wpR.Maximum = 360;
                    wpR.Key = "dir";
                    wpR.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    wpR.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    temp.Axes.Add(wpR);
                    var wpR1 = CreatePlotAxis(AxisPosition.Right, 20.0, "deg");
                    wpR1.PositionTier = 1;
                    wpR1.Minimum = 0;
                    wpR1.Maximum = 80;
                    wpR1.MajorStep = 20;
                    wpR1.Key = "spr";
                    wpR1.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    wpR1.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    temp.Axes.Add(wpR1);
                    var wpL = CreatePlotAxis(AxisPosition.Left, 20.0, "m");
                    wpL.PositionTier = 1;
                    wpL.Minimum = 0;
                    wpL.Maximum = 4;
                    wpL.Key = "sp";
                    wpL.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    wpL.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    temp.Axes.Add(wpL);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Spectrum:           // Waves Spectrum
                    temp.Axes.Clear();
                    var wsB = CreatePlotAxis(AxisPosition.Bottom, 1.0, "Frequency (Hz)");
                    wsB.Minimum = 0;
                    temp.Axes.Add(wsB);
                    var wsR = CreatePlotAxis(AxisPosition.Left, 1.0, "m^2/Hz");
                    wsR.Minimum = 0;
                    wsR.Maximum = 4;
                    wsR.MinorStep = 0.5;
                    temp.Axes.Add(wsR);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Wave_Set:           // Waves Wave Set
                    temp.Axes.Clear();
                    var wwsB = CreatePlotAxis(AxisPosition.Bottom, 1.0, "Days");
                    wwsB.Minimum = 0;
                    wwsB.MinorStep = 0.5;
                    temp.Axes.Add(wwsB);
                    var wwsL = CreatePlotAxis(AxisPosition.Left, 1.0, "m");
                    wwsL.Minimum = 0;
                    wwsL.Key = "Hs";
                    wwsL.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    wwsL.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    temp.Axes.Add(wwsL);
                    var wwsL1 = CreatePlotAxis(AxisPosition.Left, 1.0, "s");
                    wwsL1.PositionTier = 1;
                    wwsL1.Minimum = 0;
                    wwsL1.Key = "period";
                    wwsL1.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    wwsL1.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    temp.Axes.Add(wwsL1);
                    var wwsR = CreatePlotAxis(AxisPosition.Right, 90.0, "deg");
                    wwsR.PositionTier = 1;
                    wwsR.Minimum = 0;
                    wwsR.Maximum = 360;
                    wwsR.Key = "dir";
                    wwsR.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    wwsR.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    temp.Axes.Add(wwsR);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Sensor_Set:           // Waves Sensor Set
                    temp.Axes.Clear();
                    var wssB = CreatePlotAxis(AxisPosition.Bottom, 1.0, "Days");
                    wssB.Minimum = 0;
                    wssB.MinorStep = 0.5;
                    temp.Axes.Add(wssB);
                    var wssL = CreatePlotAxis(AxisPosition.Left, 1.0, "m");
                    wssL.Minimum = 0;
                    wssL.Key = "pressure";
                    wssL.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    wssL.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    temp.Axes.Add(wssL);
                    var wssR = CreatePlotAxis(AxisPosition.Right, 1.0, "deg C");
                    wssR.Minimum = 0;
                    wssR.Key = "temp";
                    wssR.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    wssR.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    temp.Axes.Add(wssR);
                    var wssL1 = CreatePlotAxis(AxisPosition.Left, 10.0, "m");
                    wssL1.PositionTier = 1;
                    wssL1.Minimum = 0;
                    wssL1.Key = "vh";
                    wssL1.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    wssL1.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2));
                    temp.Axes.Add(wssL1);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Waves_Velocity_Series:           // Waves Velocity Series
                    temp.Axes.Clear();
                    var wvsB = CreatePlotAxis(AxisPosition.Bottom, 1.0, "Days");
                    wvsB.Minimum = 0;
                    wvsB.MinorStep = 0.5;
                    temp.Axes.Add(wvsB);
                    var wvsL = CreatePlotAxis(AxisPosition.Left, 1.0, "m/s");
                    wvsL.Minimum = 0;
                    wvsL.Key = "uvMag";
                    wvsL.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    wvsL.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0));
                    temp.Axes.Add(wvsL);
                    var wvsR = CreatePlotAxis(AxisPosition.Right, 90.0, "Deg");
                    wvsR.Minimum = -180.0;
                    wvsR.Maximum = 180.0;
                    wvsR.Key = "uvDir";
                    wvsR.TitleColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    wvsR.TextColor = OxyColor.FromAColor(220, OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1));
                    temp.Axes.Add(wvsR);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set the plot axis values (label and min/max) based
        /// off the series type.
        /// </summary>
        private void SetPlotAxis()
        {
            // Clear the old axis
            Plot.Axes.Clear();

            // Add Default Bottom axis
            // Setup the axis
            Plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //MajorStep = 1,
                //Minimum = 0,
                //Maximum = _maxDataSets,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, OxyColors.White),
                MinorGridlineColor = OxyColor.FromAColor(20, OxyColors.White),
                TickStyle = OxyPlot.Axes.TickStyle.Inside,                               // Put tick lines inside the plot
                MinimumPadding = 0,                                                 // Start at axis edge   
                MaximumPadding = 0,                                                 // Start at axis edge
                //IsAxisVisible = true,
                //MajorStep = 5,
                //MinorStep = 1,
                Unit = "ENS"

            });

            switch (_SelectedSeriesType.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:             // Beam Velocity data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.4, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:              // Instrument Velocity data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.4, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Ship:              // Ship Velocity data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.4, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:              // Earth Velocity data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:                 // Amplitude data
                    LinearAxis axisAmp = CreatePlotAxis(AxisPosition.Left, 20, "dB");
                    axisAmp.Minimum = 0;
                    axisAmp.Maximum = 120;
                    Plot.Axes.Add(axisAmp);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:               // Correlation data
                    LinearAxis axisCorr = CreatePlotAxis(AxisPosition.Left, 20, "%");
                    axisCorr.Minimum = 0;
                    axisCorr.Maximum = 100;
                    Plot.Axes.Add(axisCorr);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_SNR:                       // SNR data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.5, "dB"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Range:                     // Range data
                    LinearAxis axisRange = CreatePlotAxis(AxisPosition.Left, 5, "m");
                    axisRange.Minimum = 0;
                    axisRange.StartPosition = 1;                                    // This will invert the axis to start at the top with minimum value
                    axisRange.EndPosition = 0;                                      // This will invert the axis to start at the top with minimum value
                    Plot.Axes.Add(axisRange);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Heading:              // NMEA Heading
                case BaseSeriesType.eBaseSeriesType.Base_Heading:                   // Heading
                    LinearAxis headingAxis = CreatePlotAxis(AxisPosition.Left, 30, "Deg");
                    //if (!HeadingPitchOrRollSeriesExist())
                    //{
                    //    headingAxis.Minimum = 0;
                    //    headingAxis.Maximum = 360;
                    //}
                    Plot.Axes.Add(headingAxis);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:                     // Pitch
                    LinearAxis pitchAxis = CreatePlotAxis(AxisPosition.Left, 5, "Deg");
                    //if (!HeadingPitchOrRollSeriesExist())
                    //{
                    //    pitchAxis.Minimum = -90.0;
                    //    pitchAxis.Maximum = 90.0;
                    //}
                    Plot.Axes.Add(pitchAxis);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:                      // Roll
                    LinearAxis hprAxis = CreatePlotAxis(AxisPosition.Left, 90, "Deg");
                    //if (!HeadingPitchOrRollSeriesExist())
                    //{
                    //    hprAxis.Minimum = -180.0;
                    //    hprAxis.Maximum = 180.0;
                    //}
                    Plot.Axes.Add(hprAxis);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Water_Direction:                      // Direction
                    LinearAxis dirAxis = CreatePlotAxis(AxisPosition.Left, 90, "Deg");
                    //if (!HeadingPitchOrRollSeriesExist())
                    //{
                    //    hprAxis.Minimum = -180.0;
                    //    hprAxis.Maximum = 180.0;
                    //}
                    Plot.Axes.Add(dirAxis);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Water_Magnitude:              // Earth Velocity Magnitude
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Speed:              // Earth Velocity Magnitude
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pressure:                  // Pressure data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1, "Pa"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_TransducerDepth:           // Transduer Depth Pressure data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1, "m"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:           // System Temperature data
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:         // Water Temperature data
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1, "Deg C"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Speed:                     // Speed
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 0.25, "m/s"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Pings:       // Pings
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "pings"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Range:       // Range
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "m"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_SNR:         // SNR
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "dB"));
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_SystemSetup_Voltage:       // Voltage
                    Plot.Axes.Add(CreatePlotAxis(AxisPosition.Left, 1.0, "volts"));
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
        /// <param name="majorStep">Minimum value.</param>
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
            axis.IntervalLength = 20;
            
            //axis.MajorStep = majorStep;

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
        /// Check if the plot series contains any Heading,
        /// Pitch or Roll series.  If it does, then the plot
        /// axis will be screwed up, so allow the plot to generate
        /// the axis labels.
        /// </summary>
        /// <returns>TRUE = Plot contains a Heading, Pitch or Roll series.</returns>
        private bool HeadingPitchOrRollSeriesExist()
        {
            foreach(TimeSeries series in Plot.Series)
            {
                if(series.Type.Type.Code == BaseSeriesType.eBaseSeriesType.Base_Heading || 
                   series.Type.Type.Code == BaseSeriesType.eBaseSeriesType.Base_Pitch || 
                    series.Type.Type.Code == BaseSeriesType.eBaseSeriesType.Base_Roll )
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
                foreach (TimeSeries series in Plot.Series)
                {
                    // Update the series
                    series.UpdateSeries(ensemble, maxEnsembles, _isFilterData);
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
                foreach (TimeSeries series in Plot.Series)
                {
                    series.ClearSeries();
                }
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);
        }

        /// <summary>
        /// Remove all the series from the plots.
        /// </summary>
        public void ClearSeries()
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Clear the series from the plot.
                Plot.Series.Clear();
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
            // Setup the Data Source list
            DataSourceList = DataSource.GetDataSourceList();

            // Setup the Coordinate Transform list
            CoordinateTransformList = Core.Commons.GetTransformList();

            //// Set the DataSet List
            //DataSetTypeList = BaseSeriesType.GetDataSetTypeList(seriesType.Code);
            //if (DataSetTypeList.Count > 0)
            //{
            //    SelectedDataSetType = DataSetTypeList[0];
            //}

            // Set the Subsystem config list
            SubsystemConfigList = new ObservableCollection<AdcpSubsystemConfig>();

            // Set the max bin
            MaxBin = DEFAULT_MAX_BIN;
            SelectedBin = 0;
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
            // Update the Subsystem Configuration list
            // Create a new AdcpSubsystemConfig and check if we need to add it to the list
            // Set the CEPO index to 0
            AdcpSubsystemConfig config = new AdcpSubsystemConfig(ensemble.EnsembleData.SubsystemConfig);
            if (!SubsystemConfigList.Contains(config))
            {
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => SubsystemConfigList.Add(config)));
            }

            // Update the max bin
            if (ensemble.EnsembleData.NumBins > _maxBin)
            {
                // Subtract 1 because it is zero based
                MaxBin = ensemble.EnsembleData.NumBins - 1;
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
            SetupListOptions(_SelectedSeriesType.Type);
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
        /// Set flag if the selected data source and
        /// base series type will need to set the 
        /// bin and beam options.
        /// 
        /// Bin data is only in Water Profile data.
        /// 
        /// Beam data is only in Water Profile, Bottom Track
        /// and Water Track data.
        /// </summary>
        private void SetFlagBeamBinSelectionVisible()
        {
            // Only water profile data has bins
            if (SelectedDataSource.Source == DataSource.eSource.WaterProfile)
            {
                IsBinSelectionVisible = true;
            }
            else
            {
                IsBinSelectionVisible = false;
            }

            // Determine if we are looking at data with beams
            if (SelectedDataSource.Source == DataSource.eSource.WaterProfile ||
                SelectedDataSource.Source == DataSource.eSource.BottomTrack ||
                SelectedDataSource.Source == DataSource.eSource.WaterTrack ||
                SelectedDataSource.Source == DataSource.eSource.RangeTracking ||
                SelectedDataSource.Source == DataSource.eSource.DVL)
            {
                IsBeamSelectionVisible = true;
            }
            else
            {
                IsBeamSelectionVisible = false;
            }

            // Mag and Dir do not need Beam selection
            if (SelectedBaseSeriesType != null && (SelectedBaseSeriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Water_Magnitude || SelectedBaseSeriesType.Code == BaseSeriesType.eBaseSeriesType.Base_Water_Direction))
            {
                IsBeamSelectionVisible = false;
            }

            //switch(SelectedBaseSeriesType.Code)
            //{
            //    case BaseSeriesType.eBaseSeriesType.Base_Amplitude:
            //        break;
            //}
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

        #endregion

        #region Add Series

        /// <summary>
        /// Add a series to the plot.
        /// </summary>
        /// <param name="source">Data Source</param>
        /// <param name="type">Series type.</param>
        /// <param name="beam">Beam selected.</param>
        /// <param name="bin">Bin selected.</param>
        /// <param name="color">Color of the plot.</param>
        public void AddSeries(DataSource source, BaseSeriesType type, int beam, int bin, OxyColor color)
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Create a series
                SelectedSeriesType = new SeriesType(source, type);
                TimeSeries series = new TimeSeries(SelectedSeriesType, beam, bin, color, _isFilterData, _ensembleList);

                // Add the series to the list
                //_seriesList.Add(series);
                Plot.Series.Add(series);
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => PlotSeriesList.Add(series)));

                // Reset the plot axis labels
                SetPlotAxis();
            }

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);

            // Send an event that a new series was added
            if(AddSeriesEvent != null)
            {
                AddSeriesEvent(source.Source, type.Code, beam, bin, color.ToString());
            }
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
        public void RemoveSeries(TimeSeries series)
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

            // After the line series have been updated
            // Refresh the plot with the latest data.
            Plot.InvalidatePlot(true);

            // Send event
            if(RemoveSeriesEvent != null)
            {
                RemoveSeriesEvent(series.Type.Type.Code, series.Beam, series.Bin, series.Color.ToString());
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
        public static bool operator ==(TimeSeriesPlotViewModel view1, TimeSeriesPlotViewModel view2)
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
        public static bool operator !=(TimeSeriesPlotViewModel view1, TimeSeriesPlotViewModel view2)
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

            TimeSeriesPlotViewModel p = (TimeSeriesPlotViewModel)obj;

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
            foreach(TimeSeries series in _plot.Series)
            {
                var newSeries = new TimeSeries(series.Type, series.Beam, series.Bin, series.Color, isFiltered, null);
                foreach(var pt in series.Points)
                {
                    newSeries.Points.Add(pt);
                }
                plot.Series.Add(newSeries);
            }

            // Copy Axis but change axis color lines
            foreach(var axis in _plot.Axes)
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
            string title = string.Format("{0} {1} {2}", SelectedSeriesType, SelectedDataSource, SelectedBaseSeriesType);
            report.AddPlot(plot, title, 800, 600);

            // Set file name
            DateTime currDateTime = DateTime.Now;
            if (string.IsNullOrEmpty(filename))
            {
                filename = string.Format("{0}\\{1}_{2:yyyyMMddHHmmss}.html", Pulse.Commons.DEFAULT_RECORD_DIR, title, currDateTime);
            }

            // Write report
            using(var s = File.Create(filename))
            {
                using(var w = new OxyPlot.Reporting.HtmlReportWriter(s))
                {
                    w.WriteReport(report, reportStyle);
                }
            }
        }

        #endregion

        #endregion

        #region Commands

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
            AddSeries(_SelectedDataSource, _SelectedBaseSeriesType, _selectedBeam, _selectedBin, _selectedSeriesColor);

            // If there is only 1 axis
            // Then all the axis have been removed except the bottom one for the ensemble number
            // Add a new axis and set the new title
            if (Plot.Axes.Count == 1)
            {
                // Set the new plot axis for the new data
                SetPlotAxis(ref _plot);

                // Set the title for based off the last data added
                Plot.Title = SelectedSeriesType.ToString();
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

        #region Remove All Series Command

        /// <summary>
        /// Remove all the series from the plot.
        /// </summary>
        private void On_RemoveAllSeries()
        {
            // Remove all the series in the plot
            while (PlotSeriesList.Count > 0)
            {
                // Convert to Timeseris
                TimeSeries series = PlotSeriesList.Last() as TimeSeries;

                if (series != null)
                {
                    // Add a series to the plot
                    RemoveSeries(series);
                }
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="type">Series type.</param>
        /// <param name="beam">Beam number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="color">Series colors.</param>
        public delegate void AddSeriesEventHandler(DataSource.eSource source, BaseSeriesType.eBaseSeriesType type, int beam, int bin, string color);

        /// <summary>
        /// Subscribe to when a new series was added.
        /// 
        /// To subscribe:
        /// tsVM.AddSeriesEvent += new tsVM.AddSeriesEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// tsVM.AddSeriesEvent -= (method to call)
        /// </summary>
        public event AddSeriesEventHandler AddSeriesEvent;

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        /// <param name="type">Series type.</param>
        /// <param name="beam">Beam number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="color">Series colors.</param>
        public delegate void RemoveSeriesEventHandler(BaseSeriesType.eBaseSeriesType type, int beam, int bin, string color);

        /// <summary>
        /// Subscribe to when a series is removed.
        /// 
        /// To subscribe:
        /// tsVM.RemoveSeriesEvent += new tsVM.RemoveSeriesEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// tsVM.RemoveSeriesEvent -= (method to call)
        /// </summary>
        public event RemoveSeriesEventHandler RemoveSeriesEvent;

        #endregion 
    }
}
