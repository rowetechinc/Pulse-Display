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
 * 06/18/2013      RC          3.0.1       Initial coding.
 * 01/16/2014      RC          3.2.3       Changed the beam colors to a string.
 * 12/07/2015      RC          4.4.0       Added MeasurementStandard.
 * 
 */

using OxyPlot;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
namespace RTI
{
    /// <summary>
    /// This class will hold all the settings for the 
    /// average and screen subsystems.
    /// </summary>
    public class ViewDataGraphicalOptions
    {

        #region Defaults

        #region Velocity Plot

        /// <summary>
        /// Default color map to used for the ListBox plot.
        /// </summary>
        public const ColormapBrush.ColormapBrushEnum DEFAULT_COLORMAP = ColormapBrush.ColormapBrushEnum.Jet;

        /// <summary>
        /// Default minimum velocity in m/s.
        /// </summary>
        private const double DEFAULT_MIN_VELOCITY = 0;

        /// <summary>
        /// Default maximum velocity in m/s.
        /// </summary>
        private const double DEFAULT_MAX_VELOCITY = 2;

        #endregion

        #region Screen Size

        /// <summary>
        /// Default size for the plot area in pixels.
        /// </summary>
        public const int DEFAULT_PLOT_SIZE_2D = 250;

        /// <summary>
        /// Default size for the plot area for the 3D velocity plot.
        /// </summary>
        public const int DEFAULT_PLOT_SIZE_3D = 200;

        /// <summary>
        /// Default radius for the cylinder in the bin plot.
        /// </summary>
        private const double DEFAULT_BIN_PLOT_RAD = 0.0;

        #endregion

        #region Good Beam

        /// <summary>
        /// Default value for Good Ping.  Display Earth data.
        /// </summary>
        private const bool DEFAULT_GOOD_PING = true;

        #endregion

        #region Bin Plot

        /// <summary>
        /// Default flag if Bin History is Realtime.
        /// </summary>
        private const bool DEFAULT_IS_BIN_HISTORY_REALTIME = false;

        /// <summary>
        /// Default number of bins to display in the Bin History plot.
        /// </summary>
        private const int DEFAULT_MAX_BIN_HISTORY_COUNT = 15;

        #endregion

        #region Display Max Ensembles

        /// <summary>
        /// Default maximum number of ensembles to display.
        /// </summary>
        public const int DEFAULT_DISPLAY_MAX_ENS = 50;

        #endregion

        #region Transform

        /// <summary>
        /// Default coordinate transform.
        /// </summary>
        public const Core.Commons.Transforms DEFAULT_TRANSFORM = Core.Commons.Transforms.EARTH;

        #endregion

        #endregion

        #region Properties

        #region Contour Plot

        /// <summary>
        /// Minimum velocity for the listbox plot.
        /// </summary>
        public double ContourMinimumValue { get; set; }

        /// <summary>
        /// Maximum velocity for the listbox plot.
        /// </summary>
        public double ContourMaximumValue { get; set; }

        /// <summary>
        /// Color scheme to use for the listbox plot.
        /// </summary>
        public ColormapBrush.ColormapBrushEnum PlotColorMap { get; set; }

        /// <summary>
        /// Selected contour plot type.
        /// </summary>
        public ContourPlot.PlotType SelectedContourPlotType { get; set; }

        #endregion

        #region Heatmap Plot

        /// <summary>
        /// Options for the heatmap plot.
        /// </summary>
        public HeatmapSeriesOptions HeatmapOptions { get; set; }

        #endregion

        #region Beam Colors

        /// <summary>
        /// Beam 0 color for the line plots.
        /// </summary>
        public string Beam0Color { get; set; }

        /// <summary>
        /// Beam 1 color for the line plots.
        /// </summary>
        public string Beam1Color { get; set; }

        /// <summary>
        /// Beam 2 color for the line plots.
        /// </summary>
        public string Beam2Color { get; set; }

        /// <summary>
        /// Beam 3 color for the line plots.
        /// </summary>
        public string Beam3Color { get; set; }

        #endregion

        #region Filter Data

        /// <summary>
        /// Filter the data of bad values.
        /// </summary>
        public bool FilterData { get; set; }

        /// <summary>
        /// Selected Transform for the Velocity Plot.
        /// </summary>
        public Core.Commons.Transforms SelectedVelocityPlotTransform { get; set; }

        #endregion

        #region Screen Size

        /// <summary>
        /// Size of the plot area in pixels for the 2D Velocity plot.
        /// </summary>
        public int PlotSize2D { get; set; }

        /// <summary>
        /// Size of the plot area in pixels for the 3D Velocity plot.
        /// </summary>
        public int PlotSize3D { get; set; }

        #endregion

        #region Good Beam

        /// <summary>
        /// Set which coordinate transform to display
        /// the good ping data.  
        /// TRUE = Display Earth Data.
        /// FALSE = Display Beam Data.
        /// </summary>
        public bool IsGoodPingEarth { get; set; }

        #endregion

        #region Bin Plot

        /// <summary>
        /// Maximum number of history bins to show in the plot.
        /// </summary>
        public int BinHistoryMaxCount { get; set; }

        /// <summary>
        /// Flag if the Bin History plot should be realtime
        /// or updated when a user clicks.
        /// </summary>
        public bool IsBinHistoryRealtime { get; set; }

        /// <summary>
        /// Size of the Bin Plot cylinder radius.  This value
        /// is in m/s.
        /// </summary>
        public double BinPlotRadius { get; set; }

        #endregion

        #region Display Max Ensembles

        /// <summary>
        /// Maximum number of ensembles to display.
        /// </summary>
        public int DisplayMaxEnsembles { get; set; }

        #endregion

        #region Velocity Plot

        /// <summary>
        /// Display the 3D Velocity display.
        /// Set this to true will display the 3D plot.
        /// </summary>
        public bool IsDisplay3DVelocity { get; set; }

        #endregion

        #region Time Series Plot

        /// <summary>
        /// Time Series 1 options.
        /// </summary>
        public List<TimeSeriesOptions> TimeSeries1Options { get; set; }

        /// <summary>
        /// Time Series 2 options.
        /// </summary>
        public List<TimeSeriesOptions> TimeSeries2Options { get; set; }

        /// <summary>
        /// Time Series 3 options.
        /// </summary>
        public List<TimeSeriesOptions> TimeSeries3Options { get; set; } 

        #endregion

        #region Measurement Standards

        /// <summary>
        /// Measurement standards.  Metric or standard.
        /// </summary>
        public Core.Commons.MeasurementStandards MeasurementStandard { get; set; }

        #endregion

        /// <summary>
        /// Settings are associated with this subsystem.
        /// </summary>
        [JsonIgnore]
        public SubsystemConfiguration SubsystemConfig { get; set; }

        #endregion

        /// <summary>
        /// Use this constructor if all the settings are going to be
        /// set by the user.
        /// 
        /// Need to set the subsystem after contructing.  
        /// Subsystem will be set to empty when constructed.
        /// </summary>
        public ViewDataGraphicalOptions()
        {
            // Set the subsystem
            SubsystemConfig = new SubsystemConfiguration();

            // Set default values
            SetDefaults();

            // Create a list of all the color options.
            //SetupColorOptions();
        }

        /// <summary>
        /// Set the subsystem and set the
        /// default values.
        /// </summary>
        /// <param name="ssConfig">SubsystemConfiguration associated these options.</param>
        public ViewDataGraphicalOptions(SubsystemConfiguration ssConfig)
        {
            // Set the subsystem
            SubsystemConfig = ssConfig;

            // Set default values
            SetDefaults();

            // Create a list of all the color options.
            //SetupColorOptions();
        }

        /// <summary>
        /// Set the default values for the
        /// properties.
        /// </summary>
        public void SetDefaults()
        {
            PlotSize2D = DEFAULT_PLOT_SIZE_2D;
            PlotSize3D = DEFAULT_PLOT_SIZE_3D;
            BinPlotRadius = DEFAULT_BIN_PLOT_RAD;
            Beam0Color = BeamColor.DEFAULT_COLOR_BEAM_0;
            Beam1Color = BeamColor.DEFAULT_COLOR_BEAM_1;
            Beam2Color = BeamColor.DEFAULT_COLOR_BEAM_2;
            Beam3Color = BeamColor.DEFAULT_COLOR_BEAM_3;
            PlotColorMap = DEFAULT_COLORMAP;
            ContourMinimumValue = DEFAULT_MIN_VELOCITY;
            ContourMaximumValue = DEFAULT_MAX_VELOCITY;
            SelectedContourPlotType = ContourPlot.PlotType.Earth_Velocity_Magnitude;
            FilterData = true;
            IsGoodPingEarth = DEFAULT_GOOD_PING;
            BinHistoryMaxCount = DEFAULT_MAX_BIN_HISTORY_COUNT;
            IsBinHistoryRealtime = DEFAULT_IS_BIN_HISTORY_REALTIME;
            DisplayMaxEnsembles = DEFAULT_DISPLAY_MAX_ENS;
            SelectedVelocityPlotTransform = DEFAULT_TRANSFORM;
            IsDisplay3DVelocity = false;
            MeasurementStandard = Core.Commons.MeasurementStandards.METRIC;
            HeatmapOptions = new HeatmapSeriesOptions();

            TimeSeries1Options = new List<TimeSeriesOptions>();
            //TimeSeries1Options.Add( new TimeSeriesOptions
            //(
            //    DataSource.eSource.WaterProfile,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    0,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0)
            //));
            //TimeSeries1Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.WaterProfile,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    1,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1)
            //));
            //TimeSeries1Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.WaterProfile,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    2,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2)
            //));
            //TimeSeries1Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.WaterProfile,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    3,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_3)
            //));

            TimeSeries2Options = new List<TimeSeriesOptions>();
            //TimeSeries2Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    0,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0)
            //));
            //TimeSeries2Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    1,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1)
            //));
            //TimeSeries2Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    2,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2)
            //));
            //TimeSeries2Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam,
            //    3,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_3)
            //));

            TimeSeries3Options = new List<TimeSeriesOptions>();
            //TimeSeries3Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Range,
            //    0,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_0)
            //));
            //TimeSeries3Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Range,
            //    1,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_1)
            //));
            //TimeSeries3Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Range,
            //    2,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_2)
            //));
            //TimeSeries3Options.Add(new TimeSeriesOptions
            //(
            //    DataSource.eSource.BottomTrack,
            //    BaseSeriesType.eBaseSeriesType.Base_Range,
            //    3,
            //    0,
            //    OxyColor.Parse(BeamColor.DEFAULT_COLOR_BEAM_3)
            //));
        }
    }
}