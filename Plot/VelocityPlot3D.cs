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
using Caliburn.Micro;


namespace RTI
{
    /// <summary>
    /// Create a 3D plot of the velocity data.
    /// This will plot the velocity magnitude data in 3D.
    /// </summary>
    public class VelocityPlot3D : PropertyChangedBase
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
        /// Mininimum magnitude found
        /// in all the vectors.
        /// Because the terrain displays as a sheet with either showing
        /// everything on the top or bottom, but not both, i will drive
        /// the negatives number up so 0, the bottom is now the minimum value
        /// </summary>
        private double _minMag;

        #endregion

        #region Properties

        /// <summary>
        /// The 3D model for the object.
        /// This is a helix object to display.
        /// </summary>
        private GeometryModel3D _model;
        /// <summary>
        /// The 3D model for the object.
        /// This is a helix object to display.
        /// </summary>
        public GeometryModel3D Model
        {
            get { return _model; }
            set
            {
                _model = value;
                this.NotifyOfPropertyChange(() => this.Model);
            }
        }

        /// <summary>
        /// Texture for the plot.
        /// </summary>
        private TerrainTexture _texture;
        /// <summary>
        /// Texture used with the plot.
        /// </summary>
        public TerrainTexture Texture
        {
            get { return _texture; }
            set
            {
                _texture = value;
                this.NotifyOfPropertyChange(() => this.Texture);
            }
        }

        /// <summary>
        /// The color used for the texture.
        /// </summary>
        private int _plotColor3D;
        /// <summary>
        /// The color used for the texture.
        /// </summary>
        public int PlotColor3D
        {
            get { return _plotColor3D; }
            set
            {
                _plotColor3D = value;
                this.NotifyOfPropertyChange(() => this.PlotColor3D);

                Texture = new SlopeTexture(PlotColor3D);
            }
        }
        
        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        /// <param name="maxEnsembles">Maximum number of ensembles.</param>
        public VelocityPlot3D(int maxEnsembles)
        {
            // Intialize values
            _maxEnsembles = maxEnsembles;

            _vectors = new List<DataSet.EnsembleVelocityVectors>();

            _minMag = 0.0;

            PlotColor3D = 8;

            // Set the texture
            //Texture = new SlopeTexture(PlotColor3D);
            //Texture = new SlopeDirectionTexture(0);
            //Texture = new MapTexture(@"../Images/companylogo.png") { Left = 0, Right = 0, Top = 0, Bottom = 0 };

        }

        /// <summary>
        /// Receive a list of vectors and add
        /// it to the list.  Limit the size of the list 
        /// of vectors.
        /// </summary>
        /// <param name="vectors">Vectors to add to the list.</param>
        public void AddIncomingData(DataSet.EnsembleVelocityVectors vectors)
        {
            // Add the vectors to the list
            _vectors.Add(vectors);

            // Set Minimum value
            // Because the terrain displays as a sheet with either showing
            // everything on the top or bottom, but not both, i will drive
            // the negatives number up so 0, the bottom is now the minimum value
            //FindMinimum(vectors);

            // Limit the size
            if (_vectors.Count > _maxEnsembles)
            {
                _vectors.RemoveAt(0);
            }

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => CreatePoints()));
        }

        /// <summary>
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => ClearList()));
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => CreatePoints()));
        }


        /// <summary>
        /// Create a GeometryModel3D based off the vectors
        /// stored in the list.  This will take the magnitude
        /// as the Z point and create a mesh of all the magnitudes.
        /// It will then use this mesh a 3D model.
        /// </summary>
        public void CreatePoints()
        {
            var pts = new List<Point3D>();

            // The width of the plot will the number of ensembles stored in the list
            int width = _maxEnsembles;

            // Get each vector from the list
            for (int ens = 0; ens < _vectors.Count; ens++ )
            {
                // Create a 3d point based off each magnitude in the vector
                for (int bin = 0; bin < _vectors[ens].Vectors.Length; bin++)
                {
                    double x = ens;
                    double y = bin;
                    double z = _vectors[ens].Vectors[bin].Magnitude;

                    // Check for bad velocities
                    if (z == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        z = 0.0;
                    }
                    else
                    {

                        pts.Add(new Point3D(x, y, z));
                    }
                }
            }

            var material = Materials.Gold;

            var mb = new MeshBuilder(false, false);
            mb.AddRectangularMesh(pts, width);
            var mesh = mb.ToMesh();

            if (_texture != null)
            {
                _texture.Calculate(null, mesh);
                material = Texture.Material;
                mesh.TextureCoordinates = Texture.TextureCoordinates;
            }

            var model = new GeometryModel3D();
            model.Geometry = mesh;
            model.Material = material;
            model.BackMaterial = material;
            
            Model = model;
        }

        /// <summary>
        /// Clear all the data.  This will
        /// clear the vector list.
        /// </summary>
        private void ClearList()
        {
            _vectors.Clear();
        }

        private void FindMinimum(DataSet.EnsembleVelocityVectors vectors)
        {
            // Create a 3d point based off each magnitude in the vector
            for (int bin = 0; bin < vectors.Vectors.Length; bin++)
            {
                if (vectors.Vectors[bin].Magnitude < _minMag)
                {
                    _minMag = vectors.Vectors[bin].Magnitude;
                }

            }
        }
    }
}