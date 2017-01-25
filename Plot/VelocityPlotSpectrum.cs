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
 * 03/11/2013      RC          2.18       Improved the performance of GetBrush().
 *       
 * 
 */

using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;


namespace RTI
{
    /// <summary>
    /// Display the velocity plot as a spectrum.  This will
    /// create 3D rectangles where there height and color
    /// depict the water velocity at the bin.
    /// 
    /// Performance is not great with this plot, so 
    /// only a limited number of ensembles are displayed.
    /// </summary>
    public class VelocityPlotSpectrum : ModelVisual3D
    {

        #region Variables

        /// <summary>
        /// Default color to use when no color or a
        /// bad value is given.
        /// </summary>
        private SolidColorBrush DEFAULT_EMPTY_COLOR = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Maximum number of datasets in the plot.
        /// </summary>
        private int _maxEnsembles;

        /// <summary>
        /// List of all the vectors for the velocity
        /// plot.
        /// </summary>
        private List<DataSet.EnsembleVelocityVectors> _vectors;

        /// <summary>
        /// Object to hold the empty color.
        /// </summary>
        private SolidColorBrush _emptyColor;

        private GeometryModel3D[,] Models;
        private ScaleTransform3D[,] ScaleTransforms;

        #endregion

        #region Properties

        /// <summary>
        /// Color scheme chosen.
        /// </summary>
        private ColormapBrush _colormap;
        /// <summary>
        /// Colormap property.
        /// </summary>
        public ColormapBrush Colormap
        {
            get { return _colormap; }
            set
            {
                _colormap = value;
                //this.RaisePropertyChanged(() => this.Colormap);

                //// Update the scale
                //Application.Current.Dispatcher.BeginInvoke(new Action(() => CreateScale()));

                //// Property Change the Scale text values
                //PropertyChangeVelocityScales();

                //// Update the plot
                //Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
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
        /// Minimum velocity property.
        /// </summary>
        public double MinVelocity
        {
            get { return _minVelocity; }
            set
            {
                _minVelocity = value;
                //this.RaisePropertyChanged(() => this.MinVelocity);

                //// Update the scale
                //Application.Current.Dispatcher.BeginInvoke(new Action(() => CreateScale()));

                //// Property Change the Scale text values
                //PropertyChangeVelocityScales();

                //// Update the plot
                //Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
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
        /// Max velocity property.
        /// </summary>
        public double MaxVelocity
        {
            get { return _maxVelocity; }
            set
            {
                _maxVelocity = value;
                //this.RaisePropertyChanged(() => this.MaxVelocity);

                //// Update the scale
                //Application.Current.Dispatcher.BeginInvoke(new Action(() => CreateScale()));

                //// Property Change the Scale text values
                //PropertyChangeVelocityScales();

                //// Update the plot
                //Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawEntirePlot()));
            }
        }

        /// <summary>
        /// Used to scale the display.
        /// </summary>
        private double _distance;
        /// <summary>
        /// Used to scale the display.
        /// </summary>
        public double Distance
        {
            get { return _distance; }
            set
            {
                _distance = value;
                //this.RaisePropertyChanged(() => this.Distance);
            }
        }

        /// <summary>
        /// Used to determine the shape of a bin.
        /// This will be a cube.
        /// </summary>
        private Geometry3D _geometry;
        /// <summary>
        /// Used to determine the shape of a bin.
        /// This will be a cube.
        /// </summary>
        public Geometry3D Geometry
        {
            get { return _geometry; }
            set
            {
                _geometry = value;
                //this.RaisePropertyChanged(() => this.Geometry);
            }
        }

        #endregion

        //public int FrequencyColumns { get; set; }
        //public int TimeColumns { get; set; }
        //private int updateCount = 0;
        //private bool ShowIntensity;
        //private bool ScaleHeightOnly;
        private int _ens;
        private bool _isNewPlot;

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public VelocityPlotSpectrum()
        {
            // Intialize values
            _maxEnsembles = 10;

            _vectors = new List<DataSet.EnsembleVelocityVectors>();

            _colormap = new ColormapBrush();
            _colormap.ColormapBrushType = ColormapBrush.ColormapBrushEnum.Winter;
            Distance = 1.0;
            //ShowIntensity = false;
            //ScaleHeightOnly = true;
            _minVelocity = -1;
            _maxVelocity = 2;
            _emptyColor = DEFAULT_EMPTY_COLOR;

            //FrequencyColumns = _maxEnsembles;
            //TimeColumns = 16;

            Geometry = GetDefaultGeometry();
            _ens = 0;
            _isNewPlot = true;
        }

        #region Data

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
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ClearList()));
            //Application.Current.Dispatcher.BeginInvoke(new Action(() => Update()));
        }

        /// <summary>
        /// Clear all the data.  This will
        /// clear the vector list.
        /// </summary>
        private void ClearList()
        {
            _vectors.Clear();

            // Reset the flag
            _isNewPlot = true;

            // Remove all the elements in the model
            ((Model3DGroup)Content).Children.Clear();
        }

        #endregion

        #region Models

        /// <summary>
        /// Create the shape of a bin.  The bin will be displayed as a cube.
        /// Each cube will be touching each other.
        /// </summary>
        /// <returns>Shape of a bin.</returns>
        private static Geometry3D GetDefaultGeometry()
        {
            // The default geometry is a box
            var mb = new MeshBuilder(false, false);
            mb.AddBox(new Point3D(0, 0, 0.5), 1, 1, 1);
            return mb.ToMesh();
        }

        private void AddVectors(DataSet.EnsembleVelocityVectors vectors)
        {
            // Add the vectors to the list
            _vectors.Add(vectors);

            // Limit the size
            if (_vectors.Count > _maxEnsembles)
            {
                _vectors.RemoveAt(0);
            }
        }

        /// <summary>
        /// Create the arrays used to produce the plots.
        /// This cannot be done in the constructor because
        /// we do not know the height which is the number
        /// of bins in an ensemble.  We must wait for an
        /// ensemble to get this value.
        /// </summary>
        /// <param name="width">Number of ensembles to display.</param>
        /// <param name="height">Number of bins to display in the ensemble.</param>
        private void CreatePlot(int width, int height)
        {
            Models = new GeometryModel3D[width, height];
            ScaleTransforms = new ScaleTransform3D[width, height];
            Content = new Model3DGroup();

            // Reset flag
            _isNewPlot = false;
        }

        /// <summary>
        /// Add the given vector to the list.  Then redraw the entire
        /// plot.   This will recreate the entire plot.
        /// </summary>
        private void DrawPlot()
        {
            int width = _maxEnsembles;                          // Max number of ensembles
            int height = _vectors[0].Vectors.Length;                // Number of bins in the vector array
            
            // If this is the first entry
            // The plot needs to be created
            // Need to know the number of bins to create the arrays
            if (_isNewPlot)
            {
                CreatePlot(width, height);
            }

            var group = new Model3DGroup();

            // Plot each ensemble in the list
            for(int ens = 0; ens < _vectors.Count; ens++)
            {
                // Create a 3d point based off each magnitude in the vector
                for (int bin = 0; bin < _vectors[ens].Vectors.Length; bin++)
                {
                    // Get the magnitude
                    double mag = _vectors[ens].Vectors[bin].Magnitude;

                    // Set the color based off the magnitude
                    Material material = MaterialHelper.CreateMaterial(GenerateColor(mag));

                    // Check if the magnitude is bad
                    // If its bad, set the height to 0
                    if (mag != DataSet.Ensemble.BAD_VELOCITY)
                    {
                        ScaleTransforms[ens, bin] = new ScaleTransform3D(1, 1, mag);
                    }
                    else
                    {
                        ScaleTransforms[ens, bin] = new ScaleTransform3D(1, 1, 0);
                    }

                    var translation = new TranslateTransform3D((bin - (height - 1) * 0.5) * Distance, ens * Distance, 0);
                    var tg = new Transform3DGroup();
                    tg.Children.Add(ScaleTransforms[ens, bin]);
                    tg.Children.Add(translation);
                    Models[ens, bin] = new GeometryModel3D(Geometry, material) { Transform = tg };
                    group.Children.Add(Models[ens, bin]);
                }
            }

            Content = group;
        }

        private void Update(DataSet.EnsembleVelocityVectors vectors)
        {
            int width = _maxEnsembles;                          // Max number of ensembles
            int height = vectors.Vectors.Length;                // Number of bins in the vector array
            
            // If this is the first entry
            // The plot needs to be created
            // Need to know the number of bins to create the arrays
            if (_isNewPlot)
            {
                CreatePlot(width, height);
            }

            // Create a 3d point based off each magnitude in the vector
            for (int bin = 0; bin < vectors.Vectors.Length; bin++)
            {
                // Get the magnitude
                double mag = vectors.Vectors[bin].Magnitude;

                // Set the color based off the magnitude
                Material material = MaterialHelper.CreateMaterial(GenerateColor(mag));

                // Check if the magnitude is bad
                // If its bad, set the height to 0
                if (mag != DataSet.Ensemble.BAD_VELOCITY)
                {
                    ScaleTransforms[_ens, bin] = new ScaleTransform3D(1, 1, mag);
                }
                else
                {
                    ScaleTransforms[_ens, bin] = new ScaleTransform3D(1, 1, 0);
                }

                var translation = new TranslateTransform3D((bin - (height - 1) * 0.5) * Distance, _ens * Distance, 0);
                var tg = new Transform3DGroup();
                tg.Children.Add(ScaleTransforms[_ens, bin]);
                tg.Children.Add(translation);
                Models[_ens, bin] = new GeometryModel3D(Geometry, material) { Transform = tg };
                ((Model3DGroup)Content).Children.Add(Models[_ens, bin]);
            }

            if (_ens + 1 < _maxEnsembles)
            {
                _ens++;
            }
            else
            {
                ShiftPlot(width, height);
                RemoveFirstEnsemble(height);
            }
        }

        private void ShiftPlot(int width, int height)
        {
            for(int ens = 0; ens < _maxEnsembles-1; ens++)
            {
                for (int bin = 0; bin < height; bin++)
                {
                    ScaleTransforms[ens, bin] = ScaleTransforms[ens + 1, bin];

                    var translation = new TranslateTransform3D((bin - (height - 1) * 0.5) * Distance, (ens+1) * Distance, 0);
                    var tg = new Transform3DGroup();
                    tg.Children.Add(ScaleTransforms[ens + 1, bin]);
                    tg.Children.Add(translation);
                    Models[ens+1, bin].Transform = tg;
                }
            }
        }

        /// <summary>
        /// Remove the first ensemble in the group.
        /// This will remove each element that made up
        /// the first ensemble.  Each element is a bin.
        /// We will need to know the number of bins in
        /// the first element.
        /// </summary>
        /// <param name="numBins">Number of bins in the first ensemble in the group.</param>
        private void RemoveFirstEnsemble(int numBins)
        {
            // Remove the first entrys
            // Need to remove each bin for the first ensemble
            for (int x = 0; x < numBins; x++)
            {
                ((Model3DGroup)Content).Children.RemoveAt(0);
            }
        }



        #endregion

        #region Color

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

            return _colormap.GetColormapColor(value, _minVelocity, _maxVelocity);
        }

        #endregion
    }

}