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
 * 02/27/2012      RC          2.04       Initial coding
 * 02/24/2012      RC          2.04       Made all the method calls to a SimpleBitmap use the UI thread.
 * 03/02/2012      RC          2.04       Changed VelocityPlotSelection::EnsembleNumber to VelocityPlotSelection::Index.  It is the index within the list.
 * 03/12/2012      RC          2.06       Take the absolute value of the magnitude.
 *                                         Renamed VelocityPlotSelection to MouseSelection and moved to AdcpPlotOutputViewModel.
 * 03/16/2012      RC          2.06       Absolute value of magnitude is done when created now.
 * 05/07/2012      RC          2.11       Added DrawPlot() to draw a large amount of data quickly.
 * 05/09/2012      RC          2.11       Allow the numbers of bins to be set in the constructor.
 * 05/23/2012      RC          2.11       Passed the min/max velocity and colormap to the DrawPlot().
 * 05/24/2012      RC          2.11       Resize the plot if the MaxEnsemble changes.
 * 05/25/2012      RC          2.11       Fixed how the plot is resized based off MaxEnsemble and number of ensembles in the list.
 * 05/29/2012      RC          2.11       Fixed bug with updating the legend.
 * 06/05/2012      RC          2.11       Call event when the plot has completed being drawn.
 * 06/14/2012      RC          2.11       Display plot based off Min and Max bin set.
 * 06/15/2012      RC          2.11       Made ClearIncomingData() not call dispatcher to clear the list.  Fixed a timing issue when clearing the list and loading a new project.
 * 06/29/2012      RC          2.12       Check for a null _velocityBitmap in ShiftPlot().
 * 08/29/2012      RC          2.14       Fixed bug when playing realtime and playback data at the same time with different settings in ShiftPlot().
 * 12/03/2012      RC          2.17       Added MouseSelection to this object.
 * 12/05/2012      RC          2.17       Set the MouseSelection.Id if it can be set when a selection is made.
 * 12/19/2012      RC          2.17       Moved the legend to its own ItemControl.
 * 12/28/2012      RC          2.17       Update the legend color using UpdateColor().
 * 02/21/2013      RC          2.18       In AddIncomingData(), check for null vectors.Vectors when receiving DVL data.
 * 03/11/2013      RC          2.18       Improved the performance of GetBrush().
 * 06/19/2013      RC          3.0.1      Renamed to ContourPlot.
 * 06/28/2013      RC          3.0.1      Replaced Shutdown() with IDisposable.
 * 08/13/2013      RC          3.0.7      Renamed _contourPlotVectors to _ensemblesList and made it a LimitedList.
 *                                         Take an Ensemble for incoming data so all the data is available.
 *                                         Plot the Bottom Track Range.
 * 08/15/2013      RC          3.0.7      Fixed bug setting _ensemblesList.Limit in MaxEnsemble, was using the previous value.
 * 12/06/2013      RC          3.2.0      Added LeftMouseButtonEvent.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/18/2014      RC          4.0.0      Removed clearing the plot in CheckPlotHeight().
 * 01/19/2015      RC          4.1.0      Added plot types to Contour plot.
 * 01/20/2015      RC          4.1.0      Added all the plot types.
 * 01/22/2015      RC          4.1.0      Fixed bug in DrawPlot() checking for the max bin.
 * 
 */

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using ReactiveUI;
using log4net;
using LogManager = log4net.LogManager;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace RTI
{

    /// <summary>
    /// Selection on the VelocityPlot.
    /// By clicking anywhere in the VelocityPlot,
    /// the Ensemble number and Bin number will
    /// be selected.
    /// </summary>
    public class ContourPlotMouseSelection
    {
        /// <summary>
        /// Index within the list of the ensemble.
        /// This is used to get the ensemble from the
        /// list by giving the index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Selected bin from the VelocityPlot.
        /// </summary>
        public int BinNumber { get; set; }

        /// <summary>
        /// Unique ID for selected ensemble.
        /// </summary>
        public DataSet.UniqueID Id { get; set; }
    }

    /// <summary>
    /// Plot using Listbox and rectangles of velocity data.
    /// Any method call that modifies a SimpleBitmap must be called
    /// using the UI thread using the Dispatcher.  This includes the
    /// constructor the SimpleBitmap object.  If you do not use the
    /// UI thread for all the SimpleBitmap methods, you will get the 
    /// error: "Must create DependencySource on same Thread as the DependencyObject".
    /// </summary>
    public class ContourPlot : ReactiveObject
    {

        #region Variables

        /// <summary>
        /// Setup logger
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Default Values

        /// <summary>
        /// Maximum number of bins to display.
        /// </summary>
        private const int MAX_BINS = 200;

        /// <summary>
        /// Width of the plot area in pixels.
        /// Use this value / rect_size to determine
        /// the number of rectangles that can fit in
        /// the display across the width.
        /// </summary>
        private const int PLOT_WIDTH = 400;

        /// <summary>
        /// Height of the plot area in pixels.
        /// Use this value/rect_size to determine
        /// the number rectangles that can fit in 
        /// the display down the height.
        /// </summary>
        private const int PLOT_HEIGHT = 200;

        /// <summary>
        /// Minimum rectangle size in pixels.  A rectangle
        /// is one box containing the color that 
        /// is displayed as the plot.
        /// </summary>
        private const int MIN_RECT_SIZE = 5;

        /// <summary>
        /// Maximum rectangle size in pixels.
        /// </summary>
        private const int MAX_RECT_SIZE = 200;

        ///// <summary>
        ///// Default scale label is meters per second.
        ///// </summary>
        ////private const string DEFAULT_SCALE_LABEL = "m/s";

        /// <summary>
        /// Default size of a rectangle in pixels.
        /// </summary>
        private const int DEFAULT_RECT_WIDTH = 8;

        /// <summary>
        /// Default size for the Bottom Track line thickness in pixels.
        /// </summary>
        private const int DEFAULT_BT_RECT_HEIGHT = 3;

        /// <summary>
        /// Default color to use when no color or a
        /// bad value is given.
        /// </summary>
        private SolidColorBrush DEFAULT_EMPTY_COLOR = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Default color to use to color the bottom track line.
        /// </summary>
        private SolidColorBrush DEFAULT_BT_COLOR = new SolidColorBrush(Colors.White);

        #endregion

        /// <summary>
        /// On the first pass of the plot, 
        /// we must wait for all the columns to
        /// be filled in before we start to shift
        /// the plot.  This flag is used to state 
        /// that all the columns have been filled and
        /// shifting the plot can begin.
        /// </summary>
        private bool _firstPass;

        /// <summary>
        /// This is the current number of bins in the plot.
        /// This number will set the height of the plot.
        /// When this number is changed, the entire plot needs
        /// to be updated.
        /// </summary>
        private int _numBins;

        /// <summary>
        /// Object to hold the empty color.
        /// </summary>
        private SolidColorBrush _emptyColor;

        /// <summary>
        /// Color scheme chosen.
        /// </summary>
        private ColormapBrush _colormap;

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<DataSet.Ensemble> _buffer;

        /// <summary>
        /// Thread to decode incoming data.
        /// </summary>
        private Thread _processDataThread;

        /// <summary>
        /// Flag used to stop the thread.
        /// </summary>
        private bool _continue;

        /// <summary>
        /// Event to cause the thread
        /// to go to sleep or wakeup.
        /// </summary>
        private EventWaitHandle _eventWaitData;

        /// <summary>
        /// Limit how many times the display is refreshed.
        /// </summary>
        private int _displayCounter;

        #endregion

        #region Properties

        
        /// <summary>
        /// Title of the plot.
        /// </summary>
        private string _Title;
        /// <summary>
        /// Title of the plot.
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                this.RaiseAndSetIfChanged(ref _Title, value);
            }

        }

        #region Min / Max Values

        /// <summary>
        /// Maximum number of dataset to display.
        /// </summary>
        private int _maxEnsembles;
        /// <summary>
        /// Maximum number of dataset to display.
        /// </summary>
        public int MaxEnsembles
        {
            get { return _maxEnsembles; }
            set
            {
                // Only change the if it is different
                if (value != _maxEnsembles)
                {
                    // Reset the list limit
                    _ensemblesList.Limit = value;

                    // Since the Resize function is called by the dispatcher,
                    // the values will change possibly by the time they are used
                    // So store temporary values so they do not change in the future
                    int prvMax = _maxEnsembles;
                    int newMax = value;
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => ResizePlot(prvMax, newMax)));

                    this.RaiseAndSetIfChanged(ref _maxEnsembles, value);
                }
            }
        }

        /// <summary>
        /// Minimum value.
        /// This will represent to lowest value in the 
        /// color spectrum.  Anything with this value or
        /// lower will have the lowest color in the 
        /// color map.
        /// </summary>
        private double _minValue;
        /// <summary>
        /// Minimum value property.
        /// </summary>
        public double MinValue
        {
            get { return _minValue; }
            set
            {
                this.RaiseAndSetIfChanged(ref _minValue, value);

                // Update the legend
                Legend.ViewModel.MinVelocity = value;

                // Update the plot
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
            }
        }

        /// <summary>
        /// Max Value.  This represents the greatest
        /// value in the color spectrum.  Anything with 
        /// this value or greater will have the greatest
        /// color in the color map.
        /// </summary>
        private double _maxValue;
        /// <summary>
        /// Max value property.
        /// </summary>
        public double MaxValue
        {
            get { return _maxValue; }
            set
            {
                this.RaiseAndSetIfChanged(ref _maxValue, value);

                // Update the legend
                Legend.ViewModel.MaxVelocity = value;

                // Update the plot
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
            }
        }

        /// <summary>
        /// Minimum bin to display.
        /// </summary>
        private int _minBin;
        /// <summary>
        /// Minimum bin to display.
        /// </summary>
        public int MinBin
        {
            get { return _minBin; }
            set
            {
                // Verify a valid value
                if (value >= 0 && value < _maxBin)
                {
                    this.RaiseAndSetIfChanged(ref _minBin, value);

                    // Redraw the plot
                    RecreatePlot();
                }
            }
        }

        /// <summary>
        /// Maximum bin to display.
        /// </summary>
        private int _maxBin;
        /// <summary>
        /// Maximum bin to display.
        /// </summary>
        public int MaxBin
        {
            get { return _maxBin; }
            set
            {
                if (value > _minBin && value <= DataSet.Ensemble.MAX_NUM_BINS)
                {
                    this.RaiseAndSetIfChanged(ref _maxBin, value);

                    // Redraw the plot
                    RecreatePlot();
                }
            }
        }

        #endregion

        #region Display Selection

        /// <summary>
        /// Plot types.
        /// </summary>
        public enum PlotType
        {
            /// <summary>
            /// Beam velocity data.
            /// Beam 0 velocity.
            /// </summary>
            Beam_0_Vel,

            /// <summary>
            /// Beam velocity data.
            /// Beam 1 velocity.
            /// </summary>
            Beam_1_Vel,

            /// <summary>
            /// Beam velocity data.
            /// Beam 2 velocity.
            /// </summary>
            Beam_2_Vel,

            /// <summary>
            /// Beam velocity data.
            /// Beam 3 velocity.
            /// </summary>
            Beam_3_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// X velocity.
            /// </summary>
            Instr_X_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// Y velocity.
            /// </summary>
            Instr_Y_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// Z velocity.
            /// </summary>
            Instr_Z_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// Error velocity.
            /// </summary>
            Instr_Error_Vel,

            /// <summary>
            /// Earth velocity data.
            /// East velocity.
            /// </summary>
            Earth_East_Vel,

            /// <summary>
            /// Earth velocity data.
            /// North velocity.
            /// </summary>
            Earth_North_Vel,

            /// <summary>
            /// Earth velocity data.
            /// Vertical velocity.
            /// </summary>
            Earth_Vertical_Vel,

            /// <summary>
            /// Earth velocity data.
            /// Error velocity.
            /// </summary>
            Earth_Error_Vel,

            /// <summary>
            /// Earth velocity data.
            /// This will display the magnitude of the velocity.
            /// </summary>
            Earth_Velocity_Magnitude,

            /// <summary>
            /// Earth velocity data.
            /// This will display the direction of the velocity.
            /// </summary>
            Earth_Velocity_Direction,

            /// <summary>
            /// Amplitude data.
            /// This will the display the average amplitude for the bin.
            /// </summary>
            Amplitude,

            /// <summary>
            /// Correlation.
            /// This will display the average correlation for the bin.
            /// </summary>
            Correlation
        }

        /// <summary>
        /// List of all the plot types.
        /// </summary>
        public static List<PlotType> PlotTypeList
        {
            get
            {
                var list = new List<RTI.ContourPlot.PlotType>();
                list.Add(ContourPlot.PlotType.Earth_Velocity_Magnitude);
                list.Add(ContourPlot.PlotType.Earth_Velocity_Direction);
                list.Add(ContourPlot.PlotType.Amplitude);
                list.Add(ContourPlot.PlotType.Correlation);
                list.Add(ContourPlot.PlotType.Beam_0_Vel);
                list.Add(ContourPlot.PlotType.Beam_1_Vel);
                list.Add(ContourPlot.PlotType.Beam_2_Vel);
                list.Add(ContourPlot.PlotType.Beam_3_Vel);
                list.Add(ContourPlot.PlotType.Instr_X_Vel);
                list.Add(ContourPlot.PlotType.Instr_Y_Vel);
                list.Add(ContourPlot.PlotType.Instr_Z_Vel);
                list.Add(ContourPlot.PlotType.Instr_Error_Vel);
                list.Add(ContourPlot.PlotType.Earth_East_Vel);
                list.Add(ContourPlot.PlotType.Earth_North_Vel);
                list.Add(ContourPlot.PlotType.Earth_Vertical_Vel);
                list.Add(ContourPlot.PlotType.Earth_Error_Vel);

                return list;
            }

        }

        /// <summary>
        /// Minimum bin to display.
        /// </summary>
        private PlotType _SelectedPlotType;
        /// <summary>
        /// Minimum bin to display.
        /// </summary>
        public PlotType SelectedPlotType
        {
            get { return _SelectedPlotType; }
            set
            {
                switch(value)
                {
                    // For the velocity value, use the set values.
                    case PlotType.Earth_Velocity_Magnitude:
                    default:
                        Title = "Earth Velocity Magnitude";
                        _legend.ViewModel.ScaleLabel = "m/s";
                        break;
                    case PlotType.Earth_Velocity_Direction:
                        _legend.ViewModel.ScaleLabel = "deg";
                        Title = "Earth Velocity Direction";
                        break;
                    case PlotType.Beam_0_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Beam 0 Velocity Direction";
                        break;
                    case PlotType.Beam_1_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Beam 1 Velocity Direction";
                        break;
                    case PlotType.Beam_2_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Beam 2 Velocity Direction";
                        break;
                    case PlotType.Beam_3_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Beam 3 Velocity Direction";
                        break;
                    case PlotType.Instr_X_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Instrument X Velocity Direction";
                        break;
                    case PlotType.Instr_Y_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Instrument Y Velocity Direction";
                        break;
                    case PlotType.Instr_Z_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Instrument Z Velocity Direction";
                        break;
                    case PlotType.Instr_Error_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Instrument Error Velocity Direction";
                        break;
                    case PlotType.Earth_East_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Earth East Velocity Direction";
                        break;
                    case PlotType.Earth_North_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Earth North Velocity Direction";
                        break;
                    case PlotType.Earth_Vertical_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Earth Vertical Velocity Direction";
                        break;
                    case PlotType.Earth_Error_Vel:
                        _legend.ViewModel.ScaleLabel = "m/s";
                        Title = "Earth Error Velocity Direction";
                        break;
                    case PlotType.Correlation:
                        Title = "Correlation";
                        _legend.ViewModel.ScaleLabel = "";
                        break;
                    case PlotType.Amplitude:
                        Title = "Amplitude";
                        _legend.ViewModel.ScaleLabel = "dB";
                        break;
                }

                this.RaiseAndSetIfChanged(ref _SelectedPlotType, value);
            }
        }

        #endregion

        #region Plot

        /// <summary>
        /// List of all the ensembles to plot.
        /// </summary>
        private LimitedList<DataSet.Ensemble> _ensemblesList;

        /// <summary>
        /// Bitmap to store the velocities at the
        /// different bin ranges.  This will be
        /// an image of all the bins in the viewable area.
        /// </summary>
        private WriteableBitmap _ContourBitmap;

        /// <summary>
        /// Use this as the image source for the
        /// view.  This will be the base of the
        /// _velocityBitmap SimpleBitmap.
        /// </summary>
        public WriteableBitmap ContourImage
        {
            get
            {
                // This may be null if the Dispatcher UI thread
                // has not created the bitmap yet.
                if (_ContourBitmap != null)
                {
                    return _ContourBitmap;
                }

                return null;
            }
        }

        /// <summary>
        /// Size of the rectangle in the plot.
        /// </summary>
        private int _rectSize;
        /// <summary>
        /// Size of the rectangle in the plot.
        /// This handles the Height of the rectangle.
        /// The width is fixed.
        /// </summary>
        public int RectSize
        {
            get { return _rectSize; }
            set
            {
                // Check min and max size
                if (value > MAX_RECT_SIZE)
                {
                    //_rectSize = MAX_RECT_SIZE;
                    this.RaiseAndSetIfChanged(ref _rectSize, MAX_RECT_SIZE);
                }
                else if (value < MIN_RECT_SIZE)
                {
                    //_rectSize = MIN_RECT_SIZE;
                    this.RaiseAndSetIfChanged(ref _rectSize, MIN_RECT_SIZE);
                }
                else
                {
                    this.RaiseAndSetIfChanged(ref _rectSize, value);
                }


                // Draw a new plot
                Application.Current.Dispatcher.BeginInvoke(new Action(() => CreatePlot()));

                // Update the plot
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
            }
        }

        /// <summary>
        /// Selected Colormap.  This is the color spectrum
        /// used to display the min to max values.
        /// </summary>
        private ColormapBrush.ColormapBrushEnum _colormapBrushSelection;
        /// <summary>
        /// Colormap brush chosen.
        /// </summary>
        public ColormapBrush.ColormapBrushEnum ColormapBrushSelection
        {
            get { return _colormapBrushSelection; }
            set
            {
                this.RaiseAndSetIfChanged(ref _colormapBrushSelection, value);

                _colormap.ColormapBrushType = value;

                // Update the legend
                Legend.ViewModel.UpdateColor(value);

                // Update the plot
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
            }
        }

        /// <summary>
        /// List of all available color schemes.
        /// </summary>
        public List<ColormapBrush.ColormapBrushEnum> ColormapList { get; set; }

        #endregion

        #region Legend Properties


        /// <summary>
        /// Legend for the plot.  This will keep track of the
        /// legend image and min to max values for the legend.
        /// </summary>
        private ContourPlotLegendView _legend;
        /// <summary>
        /// Legend for the plot.  This will keep track of the
        /// legend image and min to max values for the legend.
        /// </summary>
        public ContourPlotLegendView Legend
        {
            get { return _legend; }
            set
            {
                this.RaiseAndSetIfChanged(ref _legend, value);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Constructor
        /// 
        /// Initialize the ranges and subscribe
        /// to receive data.
        /// </summary>
        public ContourPlot(int maxEnsembles, int numBins = RTI.Commands.AdcpSubsystemCommands.MAX_CWPBN)
        {
            // Initialize values
            _maxEnsembles = maxEnsembles;
            _minValue = 0;
            _maxValue = 2;
            _minBin = 0;
            _maxBin = -1;
            _ensemblesList = new LimitedList<DataSet.Ensemble>(_maxEnsembles);
            _buffer = new ConcurrentQueue<DataSet.Ensemble>();
            _displayCounter = 0;

            // Initialize the thread
            _continue = true;
            _eventWaitData = new EventWaitHandle(false, EventResetMode.AutoReset);
            _processDataThread = new Thread(ProcessDataThread);
            _processDataThread.Name = string.Format("Contour Plot");
            _processDataThread.Start();

            // Initialize ranges
            _emptyColor = DEFAULT_EMPTY_COLOR;
            _rectSize = DEFAULT_RECT_WIDTH;

            _colormap = new ColormapBrush();
            ColormapList = ColormapHelper.GetColorList();

            // Create the legend
            _legend = new ContourPlotLegendView(new ContourPlotLegendViewModel(_colormap, _minValue, _maxValue));

            // SET AFTER THE LEGEND IS CREATED
            SelectedPlotType = PlotType.Earth_Velocity_Magnitude;

            _numBins = numBins;

            //_ContourBitmap = BitmapFactory.New(GenerateWidth(), GenerateHeight());

            // Create the plot
            CreatePlot();
        }

        /// <summary>
        /// Receive the ensemble and add it to the plot.
        /// This will check if the data has changed.  It will then
        /// update the plot.
        /// </summary>
        /// <param name="ensemble">Ensemble to update the plot.</param>
        public void AddIncomingData(DataSet.Ensemble ensemble)
        {
            _buffer.Enqueue(ensemble);

            //// Execute async
            //if (!_isProcessingBuffer)
            //{
            //    // Execute async
            //    await Task.Run(() => AddIncomingDataExecute());
            //}
            // Limit how often the dsiplay updates
            if ((++_displayCounter % 5) == 0)
            {
                // Wake up the thread to process data
                _eventWaitData.Set();

                _displayCounter = 0;
            }
        }

        /// <summary>
        /// Add the incoming data async.
        /// </summary>
        private void ProcessDataThread()
        {
            while (_continue)
            {
                // Wakeup the thread with a signal
                // Have a 2 second timeout to see if we need to shutdown the thread
                _eventWaitData.WaitOne(2000);

                while (_buffer.Count > 0)
                {
                    //_isProcessingBuffer = true;

                    // Remove the ensemble from the buffer
                    DataSet.Ensemble ensemble = null;
                    if (_buffer.TryDequeue(out ensemble))
                    {
                        if (ensemble != null)
                        {
                            //Debug.WriteLine("ContourPlot1::UpdatePlot " + _ensemblesList.Count + " " + ensemble.EnsembleData.EnsembleNumber);

                            try
                            {
                                if (ensemble != null)
                                {
                                    // Check Plot Height for changes
                                    CheckPlotHeight(ensemble);

                                    // Update the plot
                                    UpdatePlot(ensemble);
                                }

                            }
                            catch (Exception e)
                            {
                                // Null pointer reference when shutting down the application
                                log.Error("Error Adding Incoming Data to ContourPlot.", e);
                            }
                        }
                    }

                }
            }
        }


        /// <summary>
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {
            ClearPlot();
        }

        #region Generate Height/Width

        /// <summary>
        /// Generate the width of the bitmap.
        /// This will be the number of ensembles to display in the 
        /// plot times the number of pixels per ensemble.
        /// 
        ///            Width
        ///   |--------------------|
        ///    --- --- ---      ---
        ///   |ENS|ENS|ENS|....|ENS|
        ///   |   |   |   |    |   |
        ///   |   |   |   |    |   |
        ///         ...       
        ///   |   |   |   |    |   |
        ///    --- --- ---      ---
        /// 
        /// The width of 1 ensemble is DEFAULT_RECT_WIDTH;
        /// The Height of 1 ensemble is (_numBins * _rectSize).   
        /// 
        /// </summary>
        /// <returns>Number of pixels for the width.</returns>
        private int GenerateWidth()
        {
            return _maxEnsembles * DEFAULT_RECT_WIDTH;
        }

        /// <summary>
        /// Generate the height of the bitmap.
        /// This is number of bins in the ensemble times
        /// the size of one bin in pixels.
        ///                    
        ///   --- ---     ---  ===
        ///  |bin|bin|...|bin|  | 
        ///   --- ---     ---   | 
        ///  |bin|bin|   |bin|  |
        ///   --- ---     ---   | Height
        ///   ...               |
        ///   --- ---     ---   |
        ///  |bin|bin|   |bin|  |
        ///   --- ---     ---  ===
        /// 
        /// The width of 1 bin is DEFAULT_RECT_WIDTH;
        /// The Height of 1 bin is _rectSize.
        /// 
        /// </summary>
        /// <returns></returns>
        private int GenerateHeight()
        {
            return _numBins * _rectSize;
        }


        /// <summary>
        /// Verify the Plot height does not need to change
        /// This will check what the plot height should be verse
        /// what the actual plot height is.
        /// </summary>
        /// <param name="ensemble">Ensemble to check.</param>
        private void CheckPlotHeight(DataSet.Ensemble ensemble)
        {
            // Vectors is null if getting DVL data

            // Check number of bins in Vectors
            // If it has changed, then new settings have been loaded
            // Clear the old data and create a new plot
            if (ensemble != null && ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {
                if (_numBins != ensemble.EarthVelocityData.VelocityVectors.Length)
                {
                    // Set the new number of bins
                    _numBins = ensemble.EarthVelocityData.VelocityVectors.Length;

                    // Clear the list of all the old data, the new data may not work with the new settings
                    //ClearIncomingData();

                    // Recreate the plot
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => RecreatePlot()));
                }
            }


        }

        #endregion

        /// <summary>
        /// If you are not accumulating data and you just want to draw all the data in the list given,
        /// use this method.  This will draw the data given in the list.
        /// </summary>
        /// <param name="ensembles">List of ensemble to draw.</param>
        /// <param name="minVel">Minimum Velocity in scale.</param>
        /// <param name="maxVel">Maximum Velocity in scale.</param>
        /// <param name="colorMap">Color scheme.</param>
        public void DrawPlot(List<DataSet.Ensemble> ensembles, double minVel, double maxVel, ColormapBrush.ColormapBrushEnum colorMap)
        {
            // Set the vector list
            _ensemblesList.AddRange(ensembles);
            _maxEnsembles = ensembles.Count;
            _minValue = minVel;
            _maxValue = maxVel;
            _colormapBrushSelection = colorMap;
            _colormap.ColormapBrushType = colorMap;

            // Set the number of bins
            if (ensembles.Count > 0 && ensembles[0].IsEarthVelocityAvail && ensembles[0].EarthVelocityData.IsVelocityVectorAvail)
            {
                _numBins = ensembles[0].EarthVelocityData.VelocityVectors.Length;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => CreatePlot()));
            }

            // Draw the plot
            Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
        }

        #region Add Data

        /// <summary>
        /// Clear the list of data.
        /// Clear the plot.
        /// </summary>
        private void ClearPlot()
        {
            _ensemblesList.Clear();

            // Reset the max ensembles, 1 is given instead of
            // zero because the bitmap cannot be size 0
            //_maxEnsembles = 1;
            //_ContourBitmap = null;
            _numBins = RTI.Commands.AdcpSubsystemCommands.MAX_CWPBN;
            //_maxEnsembles = 1;

            // Update the plot
            //CreatePlot();
            //Application.Current.Dispatcher.BeginInvoke(new Action(() => CreatePlot()));

            Application.Current.Dispatcher.BeginInvoke(new Action(() => _ContourBitmap.Clear()));
        }

        /// <summary>
        /// Update the plot.
        /// This will determine if we need to shift the plot for the new
        /// entry because we have exceed the number of ensembles to display
        /// on the screen or add the ensemble to the plot.
        /// </summary>
        /// <param name="ensemble">New ensemble to add to plot.</param>
        private void UpdatePlot(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                // Add the ensemble to the list
                _ensemblesList.Add(ensemble);

                // Get the current ensemble count
                // List is 0 based, so subtract 1. 
                int ensCount = _ensemblesList.Count - 1; 

                // Check if we need to shift the plot
                // The plot is shifted to the left to allow a new
                // entry to the plot.  This will reduce the number
                // of draws from _maxEnsembles draws to 2 draws (shift and new entry)
                if (_ensemblesList.Count >= _maxEnsembles)
                {
                    // On the first pass through, a shift should not occur, because
                    // no data exist in the last column.  After every column has been
                    // filled, a shift can occur.
                    if (_firstPass)
                    {
                        // Set flag that a first pass has been done
                        _firstPass = false;

                        // Draw the column only
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(ensemble, ensCount)));
                    }
                    else
                    {
                        // Shift the plot then draw the new column
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => ShiftPlotAndDraw(ensemble, ensCount)));
                    }
                }
                else
                {
                    // Draw the column only
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(ensemble, ensCount)));
                }
            }
        }

        #endregion

        #region Plot

        /// <summary>
        /// Recreate the bitmap image.  The settings will be 
        /// X = MaxNumDataSets * RectangleWidth
        /// Y = NumBins * RectSize
        /// 
        /// RectSize is configurable by the user.  It will adjust the
        /// height of the rectangles.
        /// 
        /// The rectangle width is fixed.
        /// 
        /// If we are switching project, a new _velocityBitmap could be created and
        /// not be ready to draw before DrawPlot() is called.  If this happens, we will lose an ensemble
        /// from the image.  This will usually only happen when the project is first selected.
        /// 
        /// Must be called by a Dispatcher to run on the UI thread.
        /// Application.Current.Dispatcher.BeginInvoke(new Action(() => CreatePlot()));
        /// </summary>
        private void CreatePlot()
        {
            try
            {
                //_ContourBitmap = new SimpleBitmap(GenerateWidth(), GenerateHeight());

                int height = GenerateHeight();
                int width = GenerateWidth();

                if (_ContourBitmap == null)
                {
                    //_ContourBitmap = null;
                    _ContourBitmap = BitmapFactory.New(width, height);
                }

                // Check if the size changed
                if (_ContourBitmap.Height != height || _ContourBitmap.Width != width)
                {
                    _ContourBitmap = _ContourBitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.Bilinear);
                }
                //_ContourBitmap.Resize(GenerateWidth(), GenerateHeight(), WriteableBitmapExtensions.Interpolation.Bilinear);

                // Set flag that this is first pass
                // This is needed because on the first pass
                // the plot should not be shifted
                _firstPass = true;

                this.RaisePropertyChanged("ContourImage");
            }
            catch(Exception e)
            {
                log.Error("Error creating contour plot.", e);
            }
        }

        /// <summary>
        /// If any settings have changed,
        /// the entire plot needs to be recreated.
        /// This will create a new bitmap then
        /// redraw everything.
        /// Must be called by a Dispatcher to run on the UI thread.
        /// Application.Current.Dispatcher.BeginInvoke(new Action(() => RecreatePlot()));
        /// </summary>
        private void RecreatePlot()
        {
            // Create the plot
            // Then draw the entire plot with the new settings
            CreatePlot();
            DrawEntirePlot();
        }

        /// <summary>
        /// This method is created so that both methods can
        /// be called at the same time within the dispatcher thread.
        /// This will prevent any race conditions of shifting not finishing
        /// before drawing occurs.
        /// 
        /// Must be called by a Dispatcher to run on the UI thread.
        /// Application.Current.Dispatcher.BeginInvoke(new Action(() => ShiftPlotAndDraw()));
        /// </summary>
        /// <param name="ensemble">New ensemble to append to the plot.</param>
        /// <param name="ensCount">Location to append the plot.</param>
        private void ShiftPlotAndDraw(DataSet.Ensemble ensemble, int ensCount)
        {
            // First shift the plot
            ShiftPlot();

            // Then draw the new column
            DrawPlot(ensemble, ensCount);
        }

        /// <summary>
        /// Draw the given ensemble to the plot.
        /// This will append to the plot the ensemble.
        /// The ensCount will tell where to locate the 
        /// ensemble within the plot.  ensCount
        /// cannot exceed _maxEnsembles.
        /// 
        /// Use the Absolute value of the magnitude.  The
        /// negative and positive magnitude will be the same magnitude
        /// just different directions.
        /// 
        /// Must be called by a Dispatcher to run on the UI thread.
        /// Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
        /// </summary>
        /// <param name="ensemble">Ensemble to append to the image.</param>
        /// <param name="ensCount">Location to append the plot.</param>
        private void DrawPlot(DataSet.Ensemble ensemble, int ensCount)
        {
            // If we are switching project, a new _velocityBitmap could be created and
            // not ready before this is called.  If this happens, we will lose an ensemble
            // from the image
            if (_ContourBitmap != null && ensemble != null)
            {
                // Set Min and Max bin
                int minBin = _minBin;
                int maxBin = ensemble.EnsembleData.NumBins;
                if (_maxBin > 0 && _maxBin <= ensemble.EnsembleData.NumBins)
                {
                    maxBin = _maxBin;
                }

                // Go through each vector
                for (int bin = minBin; bin < maxBin; bin++)
                {

                    SolidColorBrush colorBrush = _emptyColor;

                    switch(_SelectedPlotType)
                    {
                        case PlotType.Earth_Velocity_Magnitude:
                        default:
                            colorBrush = VelocityColorBrush(ensemble, bin);
                            break;
                        case PlotType.Earth_Velocity_Direction:
                            colorBrush = DirectionColorBrush(ensemble, bin);
                            break;
                        case PlotType.Beam_0_Vel:
                            colorBrush = BeamVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_0_INDEX);
                            break;
                        case PlotType.Beam_1_Vel:
                            colorBrush = BeamVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_0_INDEX);
                            break;
                        case PlotType.Beam_2_Vel:
                            colorBrush = BeamVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_0_INDEX);
                            break;
                        case PlotType.Beam_3_Vel:
                            colorBrush = BeamVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_0_INDEX);
                            break;
                        case PlotType.Instr_X_Vel:
                            colorBrush = InstrumentVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_X_INDEX);
                            break;
                        case PlotType.Instr_Y_Vel:
                            colorBrush = InstrumentVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_Y_INDEX);
                            break;
                        case PlotType.Instr_Z_Vel:
                            colorBrush = InstrumentVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_Z_INDEX);
                            break;
                        case PlotType.Instr_Error_Vel:
                            colorBrush = InstrumentVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_Q_INDEX);
                            break;
                        case PlotType.Earth_East_Vel:
                            colorBrush = EarthVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_EAST_INDEX);
                            break;
                        case PlotType.Earth_North_Vel:
                            colorBrush = EarthVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_NORTH_INDEX);
                            break;
                        case PlotType.Earth_Vertical_Vel:
                            colorBrush = EarthVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_VERTICAL_INDEX);
                            break;
                        case PlotType.Earth_Error_Vel:
                            colorBrush = EarthVelColorBrush(ensemble, bin, DataSet.Ensemble.BEAM_Q_INDEX);
                            break;
                        case PlotType.Correlation:
                            colorBrush = CorrelationColorBrush(ensemble, bin);
                            break;
                        case PlotType.Amplitude:
                            colorBrush = AmplitudeColorBrush(ensemble, bin);
                            break;
                    }

                    int startX = ensCount * DEFAULT_RECT_WIDTH;
                    int startY = bin * _rectSize;
                    int endX = startX + DEFAULT_RECT_WIDTH;
                    int endY = startY + _rectSize;

                    _ContourBitmap.FillRectangle(startX,                                // X start
                                                    startY,                             // Y start
                                                    endX,                               // X end
                                                    endY,                               // Y end
                                                    colorBrush.Color);                  // color

                    // Draw the Bottom Track line
                    DrawBottomTrack(ensemble, ensCount);
                }

                // Set a property change for the VelocityImage
                this.RaisePropertyChanged("ContourImage");
            }
        }

        /// <summary>
        /// Determine the color to use based off the mangitude found in the velocity vector.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <returns>Color value based off the magnitude and the min and max value.</returns>
        private SolidColorBrush VelocityColorBrush(DataSet.Ensemble ensemble, int bin)
        {
            // Get the color for the magnitude
            // Check for bad velocities
            SolidColorBrush colorBrush;
            if(!ensemble.IsEarthVelocityAvail)
            {
                colorBrush = _emptyColor;
            }
            else if (ensemble.EarthVelocityData.VelocityVectors == null || (ensemble.EarthVelocityData.VelocityVectors[bin] != null && (ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude == DataSet.Ensemble.BAD_VELOCITY || ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude == 0)))
            {
                colorBrush = _emptyColor;
            }
            else
            {
                try
                {
                    colorBrush = GenerateColor(ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude);
                }
                catch (NullReferenceException)
                {
                    colorBrush = _emptyColor;
                }
            }

            return colorBrush;
        }

        /// <summary>
        /// Determine the color to use based off the direction found in the velocity vector.
        /// This uses Y direction is the north direction.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <returns>Color value based off the direction and the min and max value.</returns>
        private SolidColorBrush DirectionColorBrush(DataSet.Ensemble ensemble, int bin)
        {
            // Get the color for the direction
            // Check for bad velocities
            SolidColorBrush colorBrush;
            if(!ensemble.IsEarthVelocityAvail)
            {
                colorBrush = _emptyColor;
            }
            else if (ensemble.EarthVelocityData.VelocityVectors == null || (ensemble.EarthVelocityData.VelocityVectors[bin] != null && (ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude == DataSet.Ensemble.BAD_VELOCITY || ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude == 0)))
            {
                colorBrush = _emptyColor;
            }
            else
            {
                try
                {
                    colorBrush = GenerateColor(ensemble.EarthVelocityData.VelocityVectors[bin].DirectionXNorth);
                }
                catch (NullReferenceException)
                {
                    colorBrush = _emptyColor;
                }
            }

            return colorBrush;
        }

        /// <summary>
        /// Determine the color to use based off the Beam Velocity data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <param name="beam">Beam Number</param>
        /// <returns>Color value based off the Beam Velocity and the min and max value.</returns>
        private SolidColorBrush BeamVelColorBrush(DataSet.Ensemble ensemble, int bin, int beam)
        {
            // Get the color for the direction
            // Check for bad velocities
            SolidColorBrush colorBrush;
            if (!ensemble.IsBeamVelocityAvail || ensemble.BeamVelocityData.BeamVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
            {
                colorBrush = _emptyColor;
            }
            else
            {
                try
                {
                    colorBrush = GenerateColor(ensemble.BeamVelocityData.BeamVelocityData[bin, beam]);
                }
                catch (NullReferenceException)
                {
                    colorBrush = _emptyColor;
                }
            }

            return colorBrush;
        }

        /// <summary>
        /// Determine the color to use based off the Beam 1 Velocity data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <param name="beam">Beam number to get the data.</param>
        /// <returns>Color value based off the Beam 1 Velocity and the min and max value.</returns>
        private SolidColorBrush InstrumentVelColorBrush(DataSet.Ensemble ensemble, int bin, int beam)
        {
            // Get the color for the direction
            // Check for bad velocities
            SolidColorBrush colorBrush;
            if (!ensemble.IsInstrumentVelocityAvail || ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
            {
                colorBrush = _emptyColor;
            }
            else
            {
                try
                {
                    colorBrush = GenerateColor(ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam]);
                }
                catch (NullReferenceException)
                {
                    colorBrush = _emptyColor;
                }
            }

            return colorBrush;
        }

        /// <summary>
        /// Determine the color to use based off the Earth Velocity data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <param name="beam">Beam to get the data.</param>
        /// <returns>Color value based off the Earth Velocity and the min and max value.</returns>
        private SolidColorBrush EarthVelColorBrush(DataSet.Ensemble ensemble, int bin, int beam)
        {
            // Get the color for the direction
            // Check for bad velocities
            SolidColorBrush colorBrush;
            if (!ensemble.IsEarthVelocityAvail || ensemble.EarthVelocityData.EarthVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
            {
                colorBrush = _emptyColor;
            }
            else
            {
                try
                {
                    colorBrush = GenerateColor(ensemble.EarthVelocityData.EarthVelocityData[bin, beam]);
                }
                catch (NullReferenceException)
                {
                    colorBrush = _emptyColor;
                }
            }

            return colorBrush;
        }

        /// <summary>
        /// Determine the color to use based off the average amplitude data.
        /// This uses Y direction is the north direction.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <returns>Color value based off the amplitude and the min and max value.</returns>
        private SolidColorBrush AmplitudeColorBrush(DataSet.Ensemble ensemble, int bin)
        {
            // Get the color for average amplitude
            SolidColorBrush colorBrush = _emptyColor;

            // Get the average amplitude
            int numBeams = ensemble.EnsembleData.NumBeams;
            float avg = 0;
            for (int beam = 0; beam < numBeams ; beam++ )
            {
                avg += ensemble.AmplitudeData.AmplitudeData[bin, beam];
            }

            // Get the average
            avg /= numBeams;

            // Get the color
            try
            {
                colorBrush = GenerateColor(avg);
            }
            catch (NullReferenceException)
            {
                colorBrush = _emptyColor;
            }

            return colorBrush;
        }

        /// <summary>
        /// Determine the color to use based off the average Correlation data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        /// <param name="bin">Bin to get the data.</param>
        /// <returns>Color value based off the Correlation and the min and max value.</returns>
        private SolidColorBrush CorrelationColorBrush(DataSet.Ensemble ensemble, int bin)
        {
            // Get the color for average correlation
            SolidColorBrush colorBrush = _emptyColor;

            // Get the average correlaton
            int numBeams = ensemble.EnsembleData.NumBeams;
            float avg = 0;
            for (int beam = 0; beam < numBeams; beam++)
            {
                avg += ensemble.CorrelationData.CorrelationData[bin, beam];
            }

            // Get the average
            avg /= numBeams;

            // Get the color
            try
            {
                colorBrush = GenerateColor(avg);
            }
            catch (NullReferenceException)
            {
                colorBrush = _emptyColor;
            }

            return colorBrush;
        }

        /// <summary>
        /// If the bottom Track range will fit on the plot, then plot it.
        /// This will determine the range to the bottom and convert it to pixels.
        /// It will then check the overall size of the bitmap image.  If the 
        /// depth fits on the plot, then display it.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the Bottom Track range.</param>
        /// <param name="ensCount">Location to plot the range.</param>
        private void DrawBottomTrack(DataSet.Ensemble ensemble, int ensCount)
        {
            if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
            {
                // Get the bottom track depth
                double depth = ensemble.BottomTrackData.GetAverageRange();
                
                // Ensure a depth is given
                if (depth > 0.0)
                {
                    double binDepth = depth / ensemble.AncillaryData.BinSize;
                    int pixelDepth = (int)Math.Round(binDepth * _rectSize);

                    // Verify the depth will fit on the plot
                    // If the depth is beyond the plot depth, do not plot it
                    if (pixelDepth < GenerateHeight())
                    {

                        SolidColorBrush colorBrush = DEFAULT_BT_COLOR;

                        int startX = ensCount * DEFAULT_RECT_WIDTH;
                        int startY = pixelDepth;
                        int endX = startX + DEFAULT_RECT_WIDTH;
                        int endY = startY + DEFAULT_BT_RECT_HEIGHT;

                        _ContourBitmap.FillRectangle(startX,                                // X start
                                                        startY,                             // Y start
                                                        endX,                               // X end
                                                        endY,                               // Y end
                                                        colorBrush.Color);                  // color

                        //// Draw the bin with the color set to the magnitude
                        //_ContourBitmap.SetPixelRect(
                        //                            (ensCount * DEFAULT_RECT_WIDTH),    // X start
                        //                            pixelDepth,                         // Y start
                        //                            DEFAULT_RECT_WIDTH,                 // width
                        //                            DEFAULT_BT_RECT_HEIGHT,             // height
                        //                            colorBrush                          // color
                        //                            );
                    }
                }

            }
        }

        /// <summary>
        /// Redraw the entire plot.  This is used
        /// when setting have changed and the entire plot 
        /// needs to be redrawn.
        /// 
        /// Must be called by a Dispatcher to run on the UI thread.
        /// Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
        /// </summary>
        private void DrawEntirePlot()
        {
            // Go through each vector array
            //for (int ens = 0; ens < _ensembleVectors.Count; ens++)
            for (int ens = 0; ens < _ensemblesList.Count; ens++)
            {
                DrawPlot(_ensemblesList[ens], ens);
            }

            // Publish that the draw is complete
            DrawCompleteUpdated();
        }

        /// <summary>
        /// Shift the entire bitmap by 1 column to the left.  This will copy everything
        /// except the left most column and store it to an array.  It will then
        /// copy it back to the bitmap starting at 0,0.  This will leave the right most
        /// column ready for a new entry.
        /// 
        /// If this is called and the data changes (number of bins), because of playing 
        /// realtime and playback at the same time, an IndexOutOfRangeException will be 
        /// thrown.  The exception handling will do nothing and wait for the next set of
        /// data to display.
        /// </summary>
        private void ShiftPlot()
        {
            try
            {
                if (_ContourBitmap != null)
                {
                    int width = (_maxEnsembles - 1) * DEFAULT_RECT_WIDTH;                        // 1 less then maxDataSet to not include the first column on left
                    int height = (int)_ContourBitmap.PixelHeight;                   // Height of the entire bitmap
                    int stride = width * _ContourBitmap.Format.BitsPerPixel / 8;      // Stride of the bitmap (width * bytes per pixel)

                    // Array to hold the copied bitmap
                    //byte[] copiedBitmap = new byte[width * height * (_ContourBitmap.Format.BitsPerPixel / 8)];

                    //// Copy the entire plot except the last column
                    //// The last column will be dropped off and a new
                    //// column will be added to the right
                    //_ContourBitmap.BaseBitmap.CopyPixels(new Int32Rect(
                    //                                        DEFAULT_RECT_WIDTH,                         // X Start (shifted 1 column)
                    //                                        0,                                          // Y Start
                    //                                        width,                                      // Width
                    //                                        height),                                    // Height
                    //                                        copiedBitmap,                               // Array to stored copied data
                    //                                        stride,                                     // Stride
                    //                                        0                                           // Offset
                    //                                        );

                    //// Write the copied bitmap back 
                    //// starting at 0,0
                    //_ContourBitmap.BaseBitmap.WritePixels(new Int32Rect(
                    //                                        0,
                    //                                        0,
                    //                                        width,
                    //                                        height),
                    //                                        copiedBitmap,
                    //                                        stride,
                    //                                        0);


                    // Clone the bitmap
                    // Crop out the last column
                    // A new column will be added to the front
                    var clonedBitmap = _ContourBitmap.Clone();
                    //clonedBitmap.Crop(DEFAULT_RECT_WIDTH,           // Start X
                    //                    0,                          // Start Y
                    //                    width,                      // Width
                    //                    height);                    // Height

                    _ContourBitmap.Blit(new Rect(0, 0, clonedBitmap.Width, clonedBitmap.Height ),                               // Destination location
                                        clonedBitmap,                                                                           // Source image
                                        new Rect(DEFAULT_RECT_WIDTH, 0, clonedBitmap.Width, clonedBitmap.Height));              // Source location

                }
            }
            catch (Exception e)
            {
                // This exception happens when the incoming data changes 
                // Usually when playing back data and displaying realtime data at the same time
                // The number of bins did not match, so the shift will fail.
                // The exception is usually IndexOutOfRangeException
                // But if you put a break point here, then ArgumentException will be thrown
                // So this will catch all exceptions
                // DO NOTHING
                log.Debug("Error Shifting Plot.  Usually happens when plot settings change from a new project.", e);
            }
        }

        /// <summary>
        /// If the number of max ensembles has changed, then the plot needs to be
        /// resized.  This will resize the bitmap and copy the old image back to the
        /// new image.
        /// </summary>
        /// <param name="prevMaxEnsembles">Previous max ensembles.</param>
        /// <param name="newMaxEnsembles">New max ensembles.</param>
        private void ResizePlot(int prevMaxEnsembles, int newMaxEnsembles)
        {
            try
            {
                if (_ContourBitmap != null)
                {
                //    int width = prevMaxEnsembles * DEFAULT_RECT_WIDTH;                            // Width of the entire bitmap
                //    int height = (int)_ContourBitmap.PixelHeight;                       // Height of the entire bitmap
                //    int stride = width * _ContourBitmap.Format.BitsPerPixel / 8;        // Stride of the bitmap (width * bytes per pixel)

                //    // Array to hold the copied bitmap
                //   // byte[] copiedBitmap = new byte[width * height * (_ContourBitmap.BaseBitmap.Format.BitsPerPixel / 8)];

                //    //// Copy the entire plot
                //    //_ContourBitmap.BaseBitmap.CopyPixels(new Int32Rect(
                //    //                                        0,                                          // X Start
                //    //                                        0,                                          // Y Start
                //    //                                        width,                                      // Width
                //    //                                        height),                                    // Height
                //    //                                        copiedBitmap,                               // Array to stored copied data
                //    //                                        stride,                                     // Stride
                //    //                                        0                                           // Offset
                //    //                                        );

                //    //// Create a new bitmap with new maxEnsembles
                //    //_ContourBitmap = new SimpleBitmap(newMaxEnsembles * DEFAULT_RECT_WIDTH, _numBins * _rectSize);

                //    //// Check if the new image size can fit all the old image
                //    //int newWidth = newMaxEnsembles * DEFAULT_RECT_WIDTH;
                //    //if (width > newWidth)
                //    //{
                //    //    width = newWidth;
                //    //}
                //    //int newHeight = _numBins * _rectSize;
                //    //if (height > newHeight)
                //    //{
                //    //    height = newHeight;
                //    //}

                //    // Check when we copy back the old image if we start the old image at the beginning or
                //    // end of the image.  When enlarging the image, the end of the image will have a blank spot.
                //    // Reducing the size of the image will have no blank spot.
                //    //
                //    // Start at the beginning of the image if the previous image was larger then we have now so
                //    // there was no blank spot.  
                //    // Also start at the beginning if there is not enough data to fill the entire image.
                //    if (prevMaxEnsembles > newMaxEnsembles || _ensemblesList.Count < _maxEnsembles)
                //    {
                //        // Write the copied bitmap back 
                //        // starting at 0,0
                //        _ContourBitmap.BaseBitmap.WritePixels(new Int32Rect(
                //                                                0,
                //                                                0,
                //                                                width,
                //                                                height),
                //                                                copiedBitmap,
                //                                                stride,
                //                                                0);
                //    }
                //    // Start one column forward because the end will have a blank spot.  This will make the blank
                //    // spot at the beginning of the image instead of at the end of the image.
                //    else
                //    {
                //        // Write the copied bitmap back 
                //        // starting at 1 column forward.
                //        _ContourBitmap.BaseBitmap.WritePixels(new Int32Rect(
                //                                                DEFAULT_RECT_WIDTH,
                //                                                0,
                //                                                width,
                //                                                height),
                //                                                copiedBitmap,
                //                                                stride,
                //                                                0);
                //    }

                    // Resize the bitmap
                    _ContourBitmap.Resize(newMaxEnsembles * DEFAULT_RECT_WIDTH, _numBins * _rectSize, WriteableBitmapExtensions.Interpolation.Bilinear);
                }
            }
            catch (System.NullReferenceException ex)
            {
                // Sometimes when changing projects or receiving
                // live data and changing projects, the Bitmap is 
                // lost
                log.Error("NullReference for the Bitmap", ex);
            }
        }

        #endregion

        #region Bottom Track

        private void SetPlotHeight()
        {

        }

        #endregion

        #region Mouse Event

        /// <summary>
        /// This method is called when the mouse is over the
        /// VelocityPlot.  Decode the location of the mouse to 
        /// determine which ensemble and bin is selected.
        /// </summary>
        /// <param name="sender">Object that has the mouse event.</param>
        /// <param name="e">Mouse event arguments.</param>
        public void On_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //int x = (int)e.GetPosition((Image)sender).X;
            //int y = (int)e.GetPosition((Image)sender).Y;

            //Debug.WriteLine(string.Format("VelocityPlot:: X: {0}  Y: {1}", x, y));
        }

        /// <summary>
        /// This method is called when the user clicks the left mouse button is over the
        /// ContourPlot.  Decode the location of the mouse to 
        /// determine which ensemble and bin is selected.
        /// </summary>
        /// <param name="sender">Object that has the mouse event.</param>
        /// <param name="e">Mouse event arguments.</param>
        public ContourPlotMouseSelection On_MousLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int x = (int)e.GetPosition((Image)sender).X;
            int y = (int)e.GetPosition((Image)sender).Y;

            int ens = (int)(x / DEFAULT_RECT_WIDTH);
            int bin = (int)(y / _rectSize);

            //Debug.WriteLine(string.Format("VelocityPlot:: X: {0}  Y: {1} DefaultWidth: {2}  RectSize: {3}  Ens: {4}  Bin: {5}", x, y, DEFAULT_RECT_WIDTH, _rectSize, ens, bin));

            ContourPlotMouseSelection contourPlotMouseSelection = new ContourPlotMouseSelection();
            contourPlotMouseSelection.BinNumber = bin;                        // Bin selected
            contourPlotMouseSelection.Index = ens;                            // Ensemble from the list selected.  This is the index within the list

            // Set the Unique ID for the ensemble if
            // the vector at least contains that number of ensembles
            if (_ensemblesList.Count > ens)
            {
                contourPlotMouseSelection.Id = _ensemblesList[ens].EnsembleData.UniqueId;
            }

            // Publish an event of the selection
            PublishLeftMouseButtonEvent(contourPlotMouseSelection);

            return contourPlotMouseSelection;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// This method calculates the color that
        /// represents the interpolation of the two specified color ranges using
        /// the specified normalized value.
        /// </summary>
        /// <param name="minColor">The color representing the normalized value 0.0</param>
        /// <param name="maxColor">The color representing the normalized value 1.0</param>
        /// <param name="normalized_value">a value between 0.0 and 1.0 representing
        ///                                 where on the color scale between c1 and c2
        ///                                 the returned color should be.
        /// </param>
        /// <returns>RGB of the value.</returns>
        private SolidColorBrush GenerateColor(SolidColorBrush minColor, SolidColorBrush maxColor, double normalized_value)
        {
            // Bad Values get an empty color
            if (normalized_value > 1)
            {
                return _emptyColor;
            }

            if (normalized_value <= 0.0)
            {
                return minColor;
            }
            if (normalized_value >= 1.0)
            {
                return maxColor;
            }

            byte red = (byte)((1.0 - normalized_value) * (minColor.Color.R + normalized_value * maxColor.Color.R));
            byte green = (byte)((1.0 - normalized_value) * (minColor.Color.G + normalized_value * maxColor.Color.G));
            byte blue = (byte)((1.0 - normalized_value) * (minColor.Color.B + normalized_value * maxColor.Color.B));

            SolidColorBrush color = new SolidColorBrush();
            color.Color = Color.FromArgb(255, red, green, blue);
            return color;
        }

        /// <summary>
        /// Create a brush based off the value given.
        /// The value will be based against the min and max velocity value.
        /// </summary>
        /// <param name="value">Value to convert to a color brush.</param>
        /// <returns>Color brush with the color based off the value, min and max velocity.</returns>
        private SolidColorBrush GenerateColor(double value)
        {
            // Bad Values get an empty color
            if (value == DataSet.Ensemble.BAD_VELOCITY)
            {
                return _emptyColor;
            }

            return _colormap.GetColormapColor(value, _minValue, _maxValue);
        }

        #endregion

        #region Events

        #region Draw Complete Event
        /// <summary>
        /// Event To subscribe to.  This event takes no parameter.
        /// Event called when the draw is complete.
        /// </summary>
        public delegate void DrawCompleteEventHandler();

        /// <summary>
        /// Subscribe to this event.  This will hold all subscribers.
        /// 
        /// To subscribe:
        /// velocityPlot.DrawCompleteEvent += new adcpGraphOutputViewModel.DrawCompleteEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// velocityPlot.DrawCompleteEvent -= (method to call)
        /// </summary>
        public event DrawCompleteEventHandler DrawCompleteEvent;

        /// <summary>
        /// Verify there is a subscriber before calling the
        /// subscribers with the new event.
        /// </summary>
        private void DrawCompleteUpdated()
        {
            if (DrawCompleteEvent != null)
            {
                DrawCompleteEvent();
            }
        }

        #endregion

        #region Left Mouse Button Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        /// <param name="ms">Mouse selection.</param>
        public delegate void LeftMouseButtonEventHandler(ContourPlotMouseSelection ms);

        /// <summary>
        /// Subscribe to receive event when data has been successfully
        /// processed.  This can be used to tell if data is in this format
        /// and is being processed or is not in this format.
        /// Subscribe to this event.  This will hold all subscribers.
        /// 
        /// To subscribe:
        /// contourPlot.LeftMouseButtonEvent += new adcpBinaryCodec.LeftMouseButtonEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// contourPlot.LeftMouseButtonEvent -= (method to call)
        /// </summary>
        public event LeftMouseButtonEventHandler LeftMouseButtonEvent;

        /// <summary>
        /// Publish the event.
        /// </summary>
        /// <param name="ms">Mouse Selection.</param>
        private void PublishLeftMouseButtonEvent(ContourPlotMouseSelection ms)
        {
            if (LeftMouseButtonEvent != null)
            {
                LeftMouseButtonEvent(ms);
            }
        }

        #endregion

        #endregion
    }
}