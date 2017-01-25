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
 * 03/05/2012      RC          2.05       Initial coding
 * 03/06/2012      RC          2.05       Add ability to clear the plot.
 * 03/16/2012      RC          2.06       Absolute value of magnitude is done when created now.
 * 03/21/2012      RC          2.07       Renamed DEFAULT_MAX_DATASET to DEFAULT_MAX_ENSEMBLE.
 * 05/10/2012      RC          2.11       Added AddIncomingData() that takes a list.
 * 05/30/2012      RC          2.11       Added ScaleLabel property to change the scale label for the legend.
 * 06/05/2012      RC          2.11       Call event when the plot has completed being drawn.
 * 06/08/2012      RC          2.11       Set the max ensemble in the constructor.
 * 06/13/2012      RC          2.11       Redraw the plot when the colormap is changed.
 *                                         Check if the vector is null when trying to draw the plot.
 * 06/14/2012      RC          2.11       Display the plot based off Min and Max bin set.
 * 07/23/2012      RC          2.12       Call ResetMinMaxVelocity() when clearing the plot.
 * 12/19/2012      RC          2.17       Changed how the Legend is updated and moved the legend to its own ItemControl.
 * 01/23/2013      RC          2.17       In DrawPlot(), check if the number of bins is correct.  If we have a project and live data with different number of bins.
 * 
 */

using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;


namespace RTI
{
    /// <summary>
    /// Produce a 3D plot of the velocity data.
    /// The data will displayed as a surface where the peaks can be seen.
    /// The color for the plot is given by the brush chosen.  The plot auto scales
    /// so the color will always show the minimum to maximum value and color for the
    /// scene currently displayed.
    /// </summary>
    public class VelocityPlotSurface : ModelVisual3D
    {

        #region Variables

        /// <summary>
        /// Default brush.
        /// </summary>
        public const ColormapBrush.ColormapBrushEnum DEFAULT_BRUSH = ColormapBrush.ColormapBrushEnum.Jet;

        /// <summary>
        /// Default maximum number of ensembles to plot.
        /// </summary>
        public const int DEFAULT_MAX_ENSEMBLES = 100;

        /// <summary>
        /// Default minimum velocity.
        /// </summary>
        public const double DEFAULT_MIN_VELOCITY = -1.0;

        /// <summary>
        /// Default maximum velocity.
        /// </summary>
        public const double DEFAULT_MAX_VELOCITY = 2.0;

        /// <summary>
        /// List of all the vectors for the velocity
        /// plot.
        /// </summary>
        private List<DataSet.EnsembleVelocityVectors> _vectors;

        /// <summary>
        /// Color to use on the plot.
        /// </summary>
        private Brush _surfaceBrush;

        /// <summary>
        /// Default label for the scale values.
        /// </summary>
        private const string DEFAULT_SCALE_LABEL = "m/s";

        /// <summary>
        /// Default width of a rectangle for the scale.
        /// </summary>
        private const int DEFAULT_SCALE_WIDTH = 5;

        /// <summary>
        /// Default height of a rectangle for the scale.
        /// </summary>
        private const int DEFAULT_SCALE_HEIGHT = 30;

        /// <summary>
        /// Default color to use when no color or a
        /// bad value is given.
        /// </summary>
        private SolidColorBrush DEFAULT_EMPTY_COLOR = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Color scheme chosen.
        /// </summary>
        private ColormapBrush _colormap;

        /// <summary>
        /// Object to hold the empty color.
        /// </summary>
        private SolidColorBrush _emptyColor;

        #endregion

        #region Properties

        /// <summary>
        /// Color selection chosen by the user for the plot.
        /// </summary>
        private RTI.ColormapBrush.ColormapBrushEnum _colormapBrushSelection;
        /// <summary>
        /// Color selection chosen by the user for the plot.
        /// </summary>
        public RTI.ColormapBrush.ColormapBrushEnum ColormapBrushSelection
        {
            get { return _colormapBrushSelection; }
            set
            {
                _colormapBrushSelection = value;
                _surfaceBrush = ColormapBrush.GetBrush(_colormapBrushSelection);
                _colormap.ColormapBrushType = value;

                // Redraw the plot
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
            }
        }

        /// <summary>
        /// Maximum number of ensembles to plot.
        /// </summary>
        public int MaxEnsembles { get; set; }

        /// <summary>
        /// Scale label for the values.  (Default is m/s).
        /// </summary>
        public string ScaleLabel { get; set; }

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
                    _minBin = value;

                    // Reset the min and max velocity for the legend.
                    ResetMinMaxVelocity();

                    // Redraw the plot
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
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
                    _maxBin = value;

                    // Reset the min and max velocity for the legend.
                    ResetMinMaxVelocity();

                    // Redraw the plot
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
                }
            }
        }

        /// <summary>
        /// Minimum Velocity.
        /// This will represent to lowest value in the 
        /// color spectrum.  Anything with this value or
        /// lower will have the lowest color in the 
        /// color map.
        /// </summary>
        private double _minVelocity;
        /// <summary>
        /// Minimum Velocity.
        /// This will represent to lowest value in the 
        /// color spectrum.  Anything with this value or
        /// lower will have the lowest color in the 
        /// color map.
        /// </summary>
        public double MinVelocity
        {
            get { return _minVelocity; }
            set
            {
                _minVelocity = value;

                // Publish that legend changed
                PublishLegendUpdated();
            }
        }

        /// <summary>
        /// Max Velocities.  This represents the greatest
        /// value in the color spectrum.  Anything with 
        /// this value or greater will have the greatest
        /// color in the color map.
        /// </summary>
        private double _maxVelocity;
        /// <summary>
        /// Max Velocities.  This represents the greatest
        /// value in the color spectrum.  Anything with 
        /// this value or greater will have the greatest
        /// color in the color map.
        /// </summary>
        public double MaxVelocity
        {
            get { return _maxVelocity; }
            set
            {
                _maxVelocity = value;

                // Publish that legend changed
                PublishLegendUpdated();
            }
        }

        #region Legend

        ///// <summary>
        ///// Bitmap to store the scale based off
        ///// the current options.  
        ///// </summary>
        //private SimpleBitmap _scaleBitmap;
        ///// <summary>
        ///// Use this as the image source for the
        ///// view.  This will be the base of the
        ///// _scaleBitmap SimpleBitmap.
        ///// </summary>
        //public WriteableBitmap ScaleImage
        //{
        //    get
        //    {
        //        if (_scaleBitmap != null)
        //        {
        //            return _scaleBitmap.BaseBitmap;
        //        }
        //        else
        //        {
        //            // Create the legend
        //            CreateLegend();
        //            return _scaleBitmap.BaseBitmap;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Used for scale to display the minimum velocity.
        ///// </summary>
        //public string MinVelocityScale
        //{
        //    get { return _minVelocity.ToString("0.00") + ScaleLabel; }
        //}

        ///// <summary>
        ///// Used for scale to display the velocity between center and minimum.
        ///// </summary>
        //public string HalfMinVelocityScale
        //{
        //    get { return (_minVelocity + ((_maxVelocity - _minVelocity) * 0.25)).ToString("0.00") + ScaleLabel; }
        //}

        ///// <summary>
        ///// Used for scale to display the center velocity.
        ///// </summary>
        //public string HalfVelocityScale
        //{
        //    get { return (_minVelocity + ((_maxVelocity - _minVelocity) * 0.50)).ToString("0.00") + ScaleLabel; }
        //}

        ///// <summary>
        ///// Used for scale to display the velocity between center and maximum.
        ///// </summary>
        //public string HalfMaxVelocityScale
        //{
        //    get { return (_minVelocity + ((_maxVelocity - _minVelocity) * 0.75)).ToString("0.00") + ScaleLabel; }
        //}

        ///// <summary>
        ///// Used for scale to display the velocity the maximum velocity.
        ///// </summary>
        //public string MaxVelocityScale
        //{
        //    get { return _maxVelocity.ToString("0.00") + ScaleLabel; }
        //}

        #endregion

        #endregion


        /// <summary>
        /// Initialize the values.
        /// </summary>
        public VelocityPlotSurface()
        {
            // Intialize values
            MaxEnsembles = DEFAULT_MAX_ENSEMBLES;
            _minBin = 0;
            _maxBin = -1;

            // Set the color brush
            _colormap = new ColormapBrush();
            _colormapBrushSelection = DEFAULT_BRUSH;
            _surfaceBrush = ColormapBrush.GetBrush(_colormapBrushSelection);
            _colormap.ColormapBrushType = _colormapBrushSelection;
            _emptyColor = DEFAULT_EMPTY_COLOR;

            ResetMinMaxVelocity();
            ScaleLabel = DEFAULT_SCALE_LABEL;

            // Create a list to hold the vectors
            _vectors = new List<DataSet.EnsembleVelocityVectors>();
        }


        /// <summary>
        /// Receive a list of vectors and add
        /// it to the list.  Limit the size of the list 
        /// of vectors.
        /// </summary>
        /// <param name="vectors">Vectors to add to the list.</param>
        public void AddIncomingData(DataSet.EnsembleVelocityVectors vectors)
        {
            //Application.Current.Dispatcher.BeginInvoke(new Action(() => Update(vectors)));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => AddVectors(vectors)));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
        }

        /// <summary>
        /// Add an entire list to the plot.  This is used to add bulk data to the
        /// plot.
        /// </summary>
        /// <param name="vectors">List of Vectors.</param>
        public void DrawPlot(List<DataSet.EnsembleVelocityVectors> vectors)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => _vectors = vectors));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
        }

        /// <summary>
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ClearList()));
        }

        #region Add / Remove Data

        /// <summary>
        /// Clear all the data.  This will
        /// clear the vector list.
        /// </summary>
        private void ClearList()
        {
            // Clear the list
            _vectors.Clear();

            // Clear the model
            if (Content != null)
            {
                // Remove all the elements in the model
                ((Model3DGroup)Content).Children.Clear();
            }

            // Reset the min and max value
            ResetMinMaxVelocity();
        }

        /// <summary>
        /// Reset the minimum and maximum velocity for the legend.
        /// </summary>
        private void ResetMinMaxVelocity()
        {
            _minVelocity = RTI.DataSet.Ensemble.BAD_VELOCITY;
            _maxVelocity = 0;
        }

        /// <summary>
        /// Add a new vector to the list.
        /// This must be called by the dispatcher because
        /// if we are plotting immediately after.  This will ensure
        /// the thread stay in sync with data being added and plotted.
        /// 
        /// Limit the size of the list to MaxEnsembles.
        /// </summary>
        /// <param name="vectors">Vector to add to the list.</param>
        private void AddVectors(DataSet.EnsembleVelocityVectors vectors)
        {
            // Add the vectors to the list
            _vectors.Add(vectors);

            // Limit the size
            while (_vectors.Count > MaxEnsembles)
            {
                _vectors.RemoveAt(0);
            }
        }

        #endregion

        #region Draw 

        /// <summary>
        /// Draw the plot.  This will get all the magnitudes from each and ensemble and
        /// draw it as the Z value in the 3D point.  All the bins in one ensemble will form
        /// a column.  Bad magnitudes (BAD_VELOCITY) will show 0 for Z.  
        /// 
        /// The color for the Z value is based off the brush selected.  The Z value will
        /// be associated with color by getting the location in the spectrum of the brush
        /// when setting the Min and Max for the color spectrum.  The color is
        /// stored to a TextCoord array.
        /// 
        /// The color will auto scale where the minimum value
        /// found will be the minimum color in the brush and the maximum value will
        /// be the maximum color in the brush
        /// 
        /// This will redraw the entire plot when calling.
        /// 
        /// Must be called by a Dispatcher to run on the UI thread.
        /// Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot()));
        /// </summary>
        public void DrawPlot()
        {
            var plotModel = new Model3DGroup();

            if (_vectors != null && _vectors.Count > 1)
            {
                int numEns = _vectors.Count;
                int numBins = _vectors[0].Vectors.Length;
                if (MaxBin > 0 && MaxBin <= _vectors[0].Vectors.Length)
                {
                    numBins = MaxBin;
                }

                Point3D[,] Points = new Point3D[numEns, numBins];

                // Set the texture coordinates by z-value (magnitude)
                // This coordinate is the location within the texture of the color
                // to display for the given magnitude.  The Texture is based off
                // the brush chosen.
                var texcoords = new Point[numEns, numBins];
                for (int ens = 0; ens < numEns; ens++)
                {
                    for (int bin = MinBin; bin < numBins; bin++)
                    {
                        // This check is done if we are switching between live and playback data and
                        // number of bins are not the same
                        if (bin < _vectors[ens].Vectors.Length)
                        {
                            // Get the magnitude from the vector
                            // If the magnitude is bad, the display a 0 magnitude
                            if (_vectors[ens].Vectors[bin].Magnitude != DataSet.Ensemble.BAD_VELOCITY)
                            {
                                // Set the point
                                Points[ens, bin] = new Point3D(-ens, bin, _vectors[ens].Vectors[bin].Magnitude);

                                // Set Min and Max velocity
                                SetMinMaxVelocity(_vectors[ens].Vectors[bin].Magnitude);
                            }
                            else
                            {
                                Points[ens, bin] = new Point3D(-ens, bin, 0);
                            }

                            double u = (Points[ens, bin].Z);
                            texcoords[ens, bin] = new Point(u, u);
                        }
                    }
                }

                // Create a mesh based off the points created from the magnitude
                // The texture color will be obtained from the texcoords values set.
                // This will also cause the color to auto scale where the minimum value
                // found will be the minimum color in the brush and the maximum value will
                // be the maximum color in the brush
                var surfaceMeshBuilder = new MeshBuilder();
                surfaceMeshBuilder.AddRectangularMesh(Points, texcoords);

                var surfaceModel = new GeometryModel3D(surfaceMeshBuilder.ToMesh(),
                                            MaterialHelper.CreateMaterial(_surfaceBrush, null, null, 1, 0));
                surfaceModel.Material.Freeze();
                surfaceModel.BackMaterial = surfaceModel.Material;

                plotModel.Children.Add(surfaceModel);
            }

            Content = plotModel;
            
            // Create the legend for the plot
            //CreateLegend();

            // Send the event that the draw is complete
            DrawCompleteUpdated();
        }

        #endregion

        #region Legend

        /// <summary>
        /// Set the minimum and maximum velocity for the plot.
        /// If the value changes, update the legend.
        /// </summary>
        /// <param name="vel">Velocity to compare against min and max.</param>
        private void SetMinMaxVelocity(double vel)
        {
            if (_minVelocity > vel)
            {
                // Set new Min velocity
                MinVelocity = vel;
            }
            if (_maxVelocity < vel)
            {
                // Set new Max value
                MaxVelocity = vel;
            }
        }

        ///// <summary>
        ///// Create a legend for the plot.
        ///// This will create a bitmap of 
        ///// rectangles that gradually change
        ///// color based off the min and max 
        ///// velocity value and the min and
        ///// max color set.
        ///// 
        ///// Must be called by a Dispatcher to run on the UI thread.
        ///// Application.Current.Dispatcher.BeginInvoke(new Action(() => CreateLegend()));
        ///// </summary>
        //private void CreateLegend()
        //{
        //    int SCALE_COUNT = _colormap.ColormapLength;
        //    double increment = (_maxVelocity - _minVelocity) / SCALE_COUNT;

        //    // Create the bitmap
        //    _scaleBitmap = new SimpleBitmap(SCALE_COUNT * DEFAULT_SCALE_WIDTH, DEFAULT_SCALE_HEIGHT);

        //    double value = _minVelocity;
        //    for (int x = 0; x < SCALE_COUNT; x++)
        //    {
        //        _scaleBitmap.SetPixelRect((x * DEFAULT_SCALE_WIDTH), 0, DEFAULT_SCALE_WIDTH, DEFAULT_SCALE_HEIGHT, GenerateColor(value));

        //        // Move further in the color spectrum
        //        value += increment;
        //    }

        //    // Publish that legend changed
        //    PublishLegendUpdated();
        //}

        ///// <summary>
        ///// Create a brush based off the value given.
        ///// The value will be based against the min and max velocity value.
        ///// </summary>
        ///// <param name="value">Value to convert to a color brush.</param>
        ///// <returns>Color brush with the color based off the value, min and max velocity.</returns>
        //private SolidColorBrush GenerateColor(double value)
        //{
        //    // Bad Values get an empty color
        //    if (value == DataSet.Ensemble.BAD_VELOCITY)
        //    {
        //        return _emptyColor;
        //    }

        //    return GetBrush(value, _minVelocity, _maxVelocity);
        //}

        ///// <summary>
        ///// Create a brush based off the value given and a min and max value.
        ///// The value will be based against the min and max value given.
        ///// </summary>
        ///// <param name="z">Value to generate a color.</param>
        ///// <param name="zmin">Min value.</param>
        ///// <param name="zmax">Max Value.</param>
        ///// <returns>Color brush based off the value, min and max.</returns>
        //private SolidColorBrush GetBrush(double z, double zmin, double zmax)
        //{
        //    SolidColorBrush brush = new SolidColorBrush();
        //    //_colormap.Ydivisions = (int)((zmax - zmin) / (_colormap.ColormapLength - 1));
        //    _colormap.Ymin = zmin;
        //    _colormap.Ymax = zmax;
        //    _colormap.Ydivisions = _colormap.ColormapLength;
        //    int colorIndex = (int)(((_colormap.ColormapLength - 1) * (z - zmin) + zmax - z) / (zmax - zmin));
        //    if (colorIndex < 0)
        //        colorIndex = 0;
        //    if (colorIndex >= _colormap.ColormapLength)
        //        colorIndex = _colormap.ColormapLength - 1;
        //    brush = _colormap.ColormapBrushes()[colorIndex];

        //    // Freeze the brush for performance
        //    brush.Freeze();
        //    return brush;
        //}

        #endregion

        #region Event

        #region Legend Changed Event
        /// <summary>
        /// Event To subscribe to.  This event takes no parameter.
        /// </summary>
        public delegate void LegendUpdatedEventHandler( );

        /// <summary>
        /// Subscribe to this event.  This will hold all subscribers.
        /// 
        /// To subscribe:
        /// velocityPlotSurface.LegendUpdatedEvent += new adcpGraphOutputViewModel.LegendUpdatedEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// velocityPlotSurface.LegendUpdatedEvent -= (method to call)
        /// </summary>
        public event LegendUpdatedEventHandler LegendUpdatedEvent;

        /// <summary>
        /// Verify there is a subscriber before calling the
        /// subscribers with the new event.
        /// </summary>
        private void PublishLegendUpdated( )
        {
            if (LegendUpdatedEvent != null)
            {
                LegendUpdatedEvent();
            }
        }

        #endregion

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
        /// velocityPlotSurface.DrawCompleteEvent += new adcpGraphOutputViewModel.DrawCompleteEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// velocityPlotSurface.DrawCompleteEvent -= (method to call)
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

        #endregion


    }

}