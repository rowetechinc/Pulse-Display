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
 * 04/07/2015      RC          4.1.2       Initial coding.
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
    public class BackscatterOptions
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

        #region Class

        /// <summary>
        /// Time Series Options.
        /// </summary>
        public class TimeSeriesOptions
        {
            /// <summary>
            /// Data Source.
            /// </summary>
            public DataSource.eSource Source;
            
            /// <summary>
            /// Series Type.
            /// </summary>
            public BaseSeriesType.eBaseSeriesType Type;
            
            /// <summary>
            /// Beam number.
            /// </summary>
            public int Beam;

            /// <summary>
            /// Bin Number.
            /// </summary>
            public int Bin;

            /// <summary>
            /// Line color.
            /// </summary>
            public string Color;

            /// <summary>
            /// Initialize the values.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="type">Series type.</param>
            /// <param name="beam">Beam number.</param>
            /// <param name="bin">Bin number.</param>
            /// <param name="color">Series color.</param>
            public TimeSeriesOptions(DataSource.eSource source, BaseSeriesType.eBaseSeriesType type, int beam, int bin, string color)
            {
                Source = source;
                Type = type;
                Beam = beam;
                Bin = bin;
                Color = color;
            }

        }

        #endregion

        #region Properties

        /// <summary>
        /// Settings are associated with this subsystem.
        /// </summary>
        [JsonIgnore]
        public SubsystemConfiguration SubsystemConfig { get; set; }

        #region Max Ensembles

        /// <summary>
        /// East Velocity series options.
        /// </summary>
        public HeatmapSeriesOptions EastSeriesOptions { get; set; }

        /// <summary>
        /// North Velocity series options.
        /// </summary>
        public HeatmapSeriesOptions NorthSeriesOptions { get; set; }

        /// <summary>
        /// Vertical Velocity series options.
        /// </summary>
        public HeatmapSeriesOptions VerticalSeriesOptions { get; set; }

        /// <summary>
        /// Amplitude series options.
        /// </summary>
        public HeatmapSeriesOptions AmplitudeSeriesOptions { get; set; }

        /// <summary>
        /// Maximum number of ensembles to display.
        /// </summary>
        public int MaxEnsembles { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Use this constructor if all the settings are going to be
        /// set by the user.
        /// 
        /// Need to set the subsystem after contructing.  
        /// Subsystem will be set to empty when constructed.
        /// </summary>
        public BackscatterOptions()
        {
            // Set the subsystem
            SubsystemConfig = new SubsystemConfiguration();

            // Set default values
            SetDefaults();
        }

        /// <summary>
        /// Set the subsystem and set the
        /// default values.
        /// </summary>
        /// <param name="ssConfig">SubsystemConfiguration associated these options.</param>
        public BackscatterOptions(SubsystemConfiguration ssConfig)
        {
            // Set the subsystem
            SubsystemConfig = ssConfig;

            // Set default values
            SetDefaults();
        }

        /// <summary>
        /// Set the default values for the
        /// properties.
        /// </summary>
        public void SetDefaults()
        {
            MaxEnsembles = 250;

            EastSeriesOptions = new HeatmapSeriesOptions();
            EastSeriesOptions.Type = HeatmapPlotSeries.HeatmapPlotType.Earth_East_Vel;
            EastSeriesOptions.MinValue = -0.2;
            EastSeriesOptions.MaxValue = 0.2;
            EastSeriesOptions.ColorAxisMajorStep = 0.1;

            NorthSeriesOptions = new HeatmapSeriesOptions();
            NorthSeriesOptions.Type = HeatmapPlotSeries.HeatmapPlotType.Earth_North_Vel;
            NorthSeriesOptions.MinValue = -0.2;
            NorthSeriesOptions.MaxValue = 0.2;
            NorthSeriesOptions.ColorAxisMajorStep = 0.1;

            VerticalSeriesOptions = new HeatmapSeriesOptions();
            VerticalSeriesOptions.Type = HeatmapPlotSeries.HeatmapPlotType.Earth_Vertical_Vel;
            VerticalSeriesOptions.MinValue = -0.2;
            VerticalSeriesOptions.MaxValue = 0.2;
            VerticalSeriesOptions.ColorAxisMajorStep = 0.1;

            AmplitudeSeriesOptions = new HeatmapSeriesOptions();
            AmplitudeSeriesOptions.Type = HeatmapPlotSeries.HeatmapPlotType.Amplitude;
            AmplitudeSeriesOptions.MinValue = 0;
            AmplitudeSeriesOptions.MaxValue = 120;
            AmplitudeSeriesOptions.ColorAxisMajorStep = 30;

        }
    }
}