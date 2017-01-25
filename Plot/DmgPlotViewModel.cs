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
 * 07/03/2014      RC          3.4.0       Initial coding
 * 08/04/2014      RC          3.4.0       Fixed bug with refreshing the plot.
 * 08/07/2014      RC          4.0.0       Updated ReactiveCommand to 6.0.
 * 10/20/2014      RC          4.1.0       Added DmgPlotData to fix a bug updating the plot.
 * 10/23/2014      RC          4.1.0       Fixed clearing the plot.
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using OxyPlot;
    using OxyPlot.Axes;
    using System.Collections.Concurrent;
    using ReactiveUI;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using OxyPlot.Series;

    /// <summary>
    /// Distance Made Good plot.
    /// This will add, remove and update the plot.
    /// </summary>
    public class DmgPlotViewModel : PulseViewModel
    {
        #region Variable

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Distance made good plot buffer.
        /// </summary>
        private ConcurrentQueue<DmgPlotData> _DmgPlotBuffer;

        /// <summary>
        /// Flag if the DMG plot is processing.
        /// </summary>
        private bool _isProcessDmgPlot;

        #endregion

        #region Classes

        /// <summary>
        /// Data to pass the DMG plot.
        /// </summary>
        public class DmgPlotData
        {
            #region Properties

            /// <summary>
            /// GPS Lineseries.
            /// </summary>
            public LineSeries GpsLs { get; set; }

            /// <summary>
            /// Bottom Track Earth Line series.
            /// </summary>
            public LineSeries BtEarthLs { get; set; }

            #endregion

            /// <summary>
            /// Initialize the class.
            /// </summary>
            public DmgPlotData()
            {
                GpsLs = new LineSeries() { Color = OxyColors.Chartreuse, StrokeThickness = 1, Title = "GPS" };
                BtEarthLs = new LineSeries() { Color = OxyColors.DeepPink, StrokeThickness = 1, Title = "BT ENU" };
            }

            /// <summary>
            /// Add data to the object.  This will make a deep copy so a reference will not be held.
            /// A shallow copy was causing an exception with the list being modified while also plotting the points.
            /// </summary>
            /// <param name="gpsLs">GPS line series.</param>
            /// <param name="btEarthLs">Bottom Track Earth line series.</param>
            public void AddData(LineSeries gpsLs, LineSeries btEarthLs)
            {
                // Deep Copy the points
                GpsLs.Points.Clear();
                for (int x = 0; x < gpsLs.Points.Count; x++)
                {
                    GpsLs.Points.Add(gpsLs.Points[x]);
                }

                BtEarthLs.Points.Clear();
                for (int x = 0; x < btEarthLs.Points.Count; x++)
                {
                    BtEarthLs.Points.Add(btEarthLs.Points[x]);
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Title.
        /// </summary>
        private string _Title;
        /// <summary>
        /// Title.
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

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

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public DmgPlotViewModel()
            : base("Distance Made Good Plot")
        {
            Title = "Distance Made Good Plot";

            // Initialize the values
            _DmgPlotBuffer = new ConcurrentQueue<DmgPlotData>();

            // Create the plot
            Plot = CreateDistanceMadeGoodPlot();
        }

        /// <summary>
        /// Dispose the view model.
        /// </summary>
        public override void Dispose()
        {

        }

        #region Incoming Data

        /// <summary>
        /// Add the Distance Made Good values to the plot.
        /// This will keep a list of the last MAX_DATASETS ranges
        /// and plot them.
        /// </summary>
        /// <param name="data">Get the latest data.</param>
        public async Task AddIncomingData(DmgPlotData data)
        {
            // Update the plot
            try
            {
                // Enqueue the data to the buffer
                _DmgPlotBuffer.Enqueue(data);

                if (!_isProcessDmgPlot)
                {
                    //DisplayDmgPlotCommand.Execute(null);
                    await Task.Run(() => DisplayDmgPlogExecute());
                }
            }
            catch (Exception e)
            {
                // When shutting down, can get a null reference
                Debug.WriteLine(e.ToString());
            }
        }

        #endregion

        #region Create Plot

        /// <summary>
        /// Create the Distance Made Good Plot.
        /// </summary>
        /// <returns>Plot Model for Distance Made Good.</returns>
        private PlotModel CreateDistanceMadeGoodPlot()
        {
            PlotModel temp = new PlotModel();

            temp.IsLegendVisible = true;

            //temp.AutoAdjustPlotMargins = false;
            //temp.PlotMargins = new OxyThickness(0, 0, 0, 0);
            //temp.Padding = new OxyThickness(0,10,00,0);

            temp.Background = OxyColors.Black;
            temp.TextColor = OxyColors.White;
            temp.PlotAreaBorderColor = OxyColors.White;

            temp.Title = "Distance Made Good";

            // Setup the axis
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
                //IntervalLength = 5,
                MinimumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                MaximumPadding = 0.1,                                               // Pad the top and bottom of the plot so min/max lines can be seen
                Unit = "m"
            });
            temp.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //MajorStep = 1
                //Minimum = 0,
                //Maximum = _maxDataSets,
                TicklineColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                //IntervalLength = 5,
                TickStyle = OxyPlot.Axes.TickStyle.None,
                //IsAxisVisible = false,
                Unit = "m"
            });

            temp.Series.Add(new LineSeries() { Color = OxyColors.Chartreuse, StrokeThickness = 1, Title = "GPS" });
            temp.Series.Add(new LineSeries() { Color = OxyColors.DeepPink, StrokeThickness = 1, Title = "BT ENU" });

            return temp;
        }

        #endregion

        #region Display Plot

        /// <summary>
        /// Update the DMG PlotModel with the latest data.
        /// </summary>
        private void DisplayDmgPlogExecute()
        {
            try
            {
                // Process the all the data in the buffer
                while (!_DmgPlotBuffer.IsEmpty)
                {
                    // Set the flag
                    _isProcessDmgPlot = true;

                    // Remove the data from the buffer
                    DmgPlotData prt = null;
                    if (_DmgPlotBuffer.TryDequeue(out prt))
                    {
                        // Lock the plot for an update
                        lock (Plot.SyncRoot)
                        {
                            if (prt != null)
                            {
                                // GPS 
                                // Verify points exist
                                if (prt.GpsLs.Points.Count > 0)
                                {
                                    _plot.Series[0] = prt.GpsLs;
                                }

                                // Bottom Track Earth
                                // Verify points exist
                                if (prt.BtEarthLs.Points.Count > 0)
                                {
                                    _plot.Series[1] = prt.BtEarthLs;
                                }
                            }
                        }

                        // Display plot
                        _plot.InvalidatePlot(true);
                    }
                }

                // Reset the flag
                _isProcessDmgPlot = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                log.Error("Error updating DMG plot.", e);
            }
        }

        #endregion

        #region Clear Plot

        /// <summary>
        /// Clear the plot.
        /// </summary>
        public void ClearPlot()
        {
            // Lock the plot for an update
            lock (Plot.SyncRoot)
            {
                // Clear the series out
                ((LineSeries)_plot.Series[0]).Points.Clear();
                ((LineSeries)_plot.Series[1]).Points.Clear();
            }



            _plot.InvalidatePlot(true);
        }

        #endregion

    }
}
