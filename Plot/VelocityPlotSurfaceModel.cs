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
 * 05/01/2012      RC          2.11       Initial coding
 * 05/23/2012      RC          2.11       Passed colormap to DrawPlot().
 * 05/30/2012      RC          2.11       Added ScaleLabel property to change the scale label for the legend.
 * 06/08/2012      RC          2.11       Set the max ensemble in the constructor.
 * 06/13/2012      RC          2.11       Added ColormapBrushSelection cause property change to plot when changed.
 * 06/14/2012      RC          2.11       Added properties for the Min and Max bins.
 * 07/23/2012      RC          2.12       Added the property MaxEnsembles.
 * 12/19/2012      RC          2.17       Moved the Legend to its own ItemControl.
 * 
 */

namespace RTI
{
    using Caliburn.Micro;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Use this version of the VelocityPlotSurface if you need a legend for the plot.
    /// The plot is not a notificationObject, so if you bind to the object, you will not
    /// get notification if the legend changes.  This will give a legend you can bind to
    /// for the plot.
    /// </summary>
    public class VelocityPlotSurfaceModel : PropertyChangedBase
    {
        #region Properties

        /// <summary>
        /// 3D surface plot.
        /// </summary>
        private VelocityPlotSurface _plot;
        /// <summary>
        /// 3D surface plot.
        /// </summary>
        public VelocityPlotSurface Plot
        {
            get { return _plot; }
            set
            {
                _plot = value;
                this.NotifyOfPropertyChange(() => this.Plot);
            }
        }

        /// <summary>
        /// Color selection chosen by the user for the plot.
        /// </summary>
        public RTI.ColormapBrush.ColormapBrushEnum ColormapBrushSelection
        {
            get { return Plot.ColormapBrushSelection; }
            set
            {
                Plot.ColormapBrushSelection = value;
                this.NotifyOfPropertyChange(() => this.Plot);
                //this.RaisePropertyChanged(() => this.ScaleImage);

                Legend.ViewModel.Colormap.ColormapBrushType = value;
            }
        }

        #region Legend

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
                _legend = value;
                this.NotifyOfPropertyChange(() => this.Legend);
            }
        }

        #endregion

        #region Min / Max Values

        /// <summary>
        /// Minimum bin to display
        /// </summary>
        public int MinBin
        {
            get { return Plot.MinBin; }
            set
            {
                Plot.MinBin = value;
                this.NotifyOfPropertyChange(() => this.MinBin);
                this.NotifyOfPropertyChange(() => this.Plot);
            }
        }

        /// <summary>
        /// Maximum bin to display
        /// </summary>
        public int MaxBin
        {
            get { return Plot.MaxBin; }
            set
            {
                Plot.MaxBin = value;
                this.NotifyOfPropertyChange(() => this.MaxBin);
                this.NotifyOfPropertyChange(() => this.Plot);
            }
        }

        /// <summary>
        /// Maximum ensembles to display.
        /// </summary>
        public int MaxEnsembles
        {
            get { return Plot.MaxEnsembles; }
            set
            {
                Plot.MaxEnsembles = MaxEnsembles;
                this.NotifyOfPropertyChange(() => this.MaxEnsembles);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Create the plot.
        /// </summary>
        /// <param name="maxEnsembles">Maximum ensembles to display.</param>
        public VelocityPlotSurfaceModel(int maxEnsembles = VelocityPlotSurface.DEFAULT_MAX_ENSEMBLES)
        {
            _plot = new VelocityPlotSurface();
            _plot.MaxEnsembles = maxEnsembles;
            _plot.LegendUpdatedEvent += new VelocityPlotSurface.LegendUpdatedEventHandler(On_LegendUpdatedEvent);

            // Create the legend
            Legend = new ContourPlotLegendView(new ContourPlotLegendViewModel(VelocityPlotSurface.DEFAULT_BRUSH, VelocityPlotSurface.DEFAULT_MIN_VELOCITY, VelocityPlotSurface.DEFAULT_MAX_VELOCITY));
        }



        /// <summary>
        /// Receive a vector and add
        /// it to the list.  Limit the size of the list 
        /// of vectors.
        /// </summary>
        /// <param name="vectors">Vectors to add to the list.</param>
        public void AddIncomingData(DataSet.EnsembleVelocityVectors vectors)
        {
            _plot.AddIncomingData(vectors);
            //PropertyChangeVelocityScales();
        }

        /// <summary>
        /// Receive a list of vectors and add
        /// it to the list.  Limit the size of the list 
        /// of vectors.
        /// </summary>
        /// <param name="vectors">Vectors to add to the list.</param>
        /// <param name="colorMap">Color Scheme.</param>
        public void DrawPlot(List<DataSet.EnsembleVelocityVectors> vectors, ColormapBrush.ColormapBrushEnum colorMap)
        {
            _plot.MaxEnsembles = vectors.Count;
            _plot.ColormapBrushSelection = colorMap;
            _plot.DrawPlot(vectors);
            //PropertyChangeVelocityScales();
        }

        /// <summary>
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {
            _plot.ClearIncomingData();
        }

        /// <summary>
        /// Draw the plot.  This will take whatever
        /// is in the buffer and draw it.
        /// </summary>
        public void DrawPlot()
        {
            _plot.DrawPlot();
        }

        #region EventHandler

        /// <summary>
        /// Event handled when the plot's legend has been updated.
        /// This is when a new minimum or maximum velocity has
        /// been found.  Update the legend.
        /// </summary>
        private void On_LegendUpdatedEvent()
        {
            Legend.ViewModel.MinVelocity = _plot.MinVelocity;
            Legend.ViewModel.MaxVelocity = _plot.MaxVelocity;
            //Legend.ViewModel.Colormap.ColormapBrushType = _plot.ColormapBrushSelection;
        }

        #endregion
    }
}
