/*
 * Copyright © 2013 
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
 * 10/03/2014      RC          4.1.0      Initial coding
 *
 */

using HelixToolkit.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RTI
{
    /// <summary>
    /// Compass cal scatter plot.
    /// </summary>
    public class CompassCalScatterPlot3D : ModelVisual3D
    {
        #region Variables

        /// <summary>
        /// Set the colormap for the plot.
        /// This will be the color spectrum of the plot.
        /// </summary>
        private ColormapBrush _colormap;

        /// <summary>
        /// List of all the points
        /// </summary>
        private List<Point3D> Points;

        /// <summary>
        /// Lock for the points list.
        /// </summary>
        public object PointsLock = new object();

        /// <summary>
        /// Sphere size.
        /// </summary>
        public double SphereSize { get; set; }

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<Point3D> _buffer;

        /// <summary>
        /// Flag for processing buffer.
        /// </summary>
        private bool _isProcessingBuffer;

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
                _colormap.ColormapBrushType = _colormapBrushSelection;
            }
        }

        /// <summary>
        /// Colors of the points.
        /// </summary>
        public Brush SurfaceBrush
        {
            get
            {
                 //return BrushHelper.CreateGradientBrush(Colors.White, Colors.Blue);
                 return GradientBrushes.RainbowStripes;
                // return GradientBrushes.BlueWhiteRed;
            }
        }

        /// <summary>
        /// Semi transparent brush.
        /// </summary>
        public Brush SemiTransBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromArgb(200, 32, 32, 32));;
            }
        }

        /// <summary>
        /// Average radius for the compass cal values.
        /// </summary>
        public double AverageRadius { get; set;}

        #endregion

        /// <summary>
        /// Create a scatter plot for compass cal.  This will
        /// create a sphere to get the border of where the
        /// average radius is.
        /// </summary>
        public CompassCalScatterPlot3D()
        {
            _colormap = new ColormapBrush();
            _colormap.ColormapBrushType = ColormapBrush.ColormapBrushEnum.Jet;

            _isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<Point3D>();

            Points = new List<Point3D>();

            SphereSize = 1.0;

            AverageRadius = 44.0;
        }

        /// <summary>
        /// Receive a list of vectors and add
        /// it to the list.  Limit the size of the list 
        /// of vectors.
        /// </summary>
        /// <param name="point3D">Point3D to add to the list.</param>
        public void AddIncomingData(Point3D point3D)
        {
            //_prevVectors = vectors;
            //await Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(vectors)));
            _buffer.Enqueue(point3D);

            // Process the buffer
            if(!_isProcessingBuffer)
            {
                AccumulateDataAndDrawPlot();
            }
        }

        /// <summary>
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {

            // Clear plot
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ClearPlot()));
            
            // Clear point
            Points.Clear();
            
            // Clear buffer
            while(_buffer.Count > 0)
            {
                Point3D point;
                _buffer.TryDequeue(out point);
            }
        }

        /// <summary>
        /// Accumulate the data in the buffer.
        /// Then draw the plot.
        /// </summary>
        private void AccumulateDataAndDrawPlot()
        {
            if (_buffer.Count > 0)
            {
                // Set flag
                _isProcessingBuffer = true;

                Point3D[] pointsArray = null;

                // Lock the points list
                lock (PointsLock)
                {
                    // Clean out the buffer
                    while (_buffer.Count > 0)
                    {
                        // Add all the points to the list
                        Point3D point;
                        if (_buffer.TryDequeue(out point))
                        {
                            Points.Add(point);
                        }
                    }

                    // Points array
                    pointsArray = Points.ToArray();
                }

                // If the points array exist, draw the plot
                if (pointsArray != null)
                {
                    // Draw the plot
                    DrawPlot(pointsArray);
                }
            }

            _isProcessingBuffer = false;
        }

        /// <summary>
        /// Draw the plot.
        /// </summary>
        /// <param name="pointsArray">Array of Point3D points.</param>
        public void DrawPlot(Point3D[] pointsArray)
        {
            // Verify data exist
            if (pointsArray == null) return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Create a new plotmodel
                var plotModel = new Model3DGroup();

                // Create a mesh
                var scatterMeshBuilder = new MeshBuilder(true, true);

                // Add spheres to the mesh
                for (var i = 0; i < pointsArray.Length; ++i)
                {
                    scatterMeshBuilder.AddSphere(pointsArray[i], SphereSize, 4, 4);
                }

                // Turn the mesh into a model
                var scatterModel = new GeometryModel3D(scatterMeshBuilder.ToMesh(), MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1, 0));
                scatterModel.BackMaterial = scatterModel.Material;

                // Add the mesh model to the plot
                plotModel.Children.Add(scatterModel);

                // Outline sphere
                plotModel.Children.Add(CreateOutline(AverageRadius));

                // Set the plot model so it will display
                Content = plotModel;
            }));
        }

        /// <summary>
        /// Create an outline sphere with transparent color.
        /// </summary>
        /// <param name="outlineRadius">Outline radius.</param>
        /// <returns>Sphere for the outline.</returns>
        private GeometryModel3D CreateOutline(double outlineRadius)
        {
            // Create a mesh
            var scatterMeshBuilder = new MeshBuilder(true, true);

            scatterMeshBuilder.AddSphere(new Point3D(0, 0, 0), outlineRadius);

            // Turn the mesh into a model
            var scatterModel = new GeometryModel3D(scatterMeshBuilder.ToMesh(), MaterialHelper.CreateMaterial(SemiTransBrush, null, null, 1, 0));
            scatterModel.BackMaterial = scatterModel.Material;

            return scatterModel;
        }

        /// <summary>
        /// Clear the plot by giving the Content
        /// an empty object.
        /// </summary>
        private void ClearPlot()
        {
            if (Content != null)
            {
                // Remove all the elements in the model
                ((Model3DGroup)Content).Children.Clear();
            }
            //SelectedBin = DEFAULT_SELECTED_BIN;
        }


    }
}
