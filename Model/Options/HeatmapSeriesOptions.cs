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
 * 08/02/2016      RC          4.4.12      Added Interperlate option.
 * 
 */

using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Heatmap Series Options.
    /// </summary>
    public class HeatmapSeriesOptions
    {
        /// <summary>
        /// Series Type.
        /// </summary>
        public HeatmapPlotSeries.HeatmapPlotType Type { get; set; }

        /// <summary>
        /// Minimum bin to display.
        /// </summary>
        public int MinBin { get; set; }

        /// <summary>
        /// Maximum bin to display.
        /// </summary>
        public int MaxBin { get; set; }

        /// <summary>
        /// Flag if we are filtering the data.
        /// </summary>
        public bool IsFilterData { get; set; }

        /// <summary>
        /// Flag if a bottom track line should be included with the plot.
        /// </summary>
        public bool IsBottomTrackLine { get; set; }

        /// <summary>
        /// Minimum value for the plot.
        /// Anything below this value will be LowColor.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Maximum value for the plot.
        /// Anything above this value will be HighColor.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Color Axis MajorStep for the plot.
        /// This will be the number of divides in the color axis legend.
        /// </summary>
        public double ColorAxisMajorStep { get; set; }

        /// <summary>
        /// Palette for the plot.  This will be
        /// the color range of the plot.
        /// It will be a list of strings.  Each
        /// string will be a color.
        /// </summary>
        public List<string> Palette { get; set; }

        /// <summary>
        /// Flag to interplate the data to blend.
        /// </summary>
        public bool Interperlate { get; set; }

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public HeatmapSeriesOptions()
        {
            SetDefaults();
        }

        /// <summary>
        /// Initialize the values.
        /// </summary>
        /// <param name="type">Series type.</param>
        /// <param name="minBin">Minimum bin.</param>
        /// <param name="maxBin">Maximum Bin.</param>
        /// <param name="isFilterData">Flag to filter data.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        /// <param name="palette">Color palette.</param>
        /// <param name="colorAxisMajorStep">Color Axis Major Step</param>
        /// <param name="isBottomTrackLine">Flag to show the bottom track line.</param>
        /// <param name="interperlate">Interperlate the data.</param>
        public HeatmapSeriesOptions(HeatmapPlotSeries.HeatmapPlotType type, 
                                        int minBin, int maxBin, 
                                        bool isFilterData, bool isBottomTrackLine,
                                        double minValue, double maxValue, 
                                        List<string> palette,
                                        double colorAxisMajorStep,
                                        bool interperlate)
        {
            Type = type;
            MinBin = minBin;
            MaxBin = maxBin;
            IsFilterData = isFilterData;
            IsBottomTrackLine = isBottomTrackLine;
            MinValue = minValue;
            MaxValue = maxValue;
            Palette = palette;
            ColorAxisMajorStep = colorAxisMajorStep;
            Interperlate = interperlate;
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            Type = HeatmapPlotSeries.HeatmapPlotType.Earth_Velocity_Magnitude;
            MinBin = 0;
            MaxBin = 100;
            IsFilterData = true;
            MinValue = 0.0;
            MaxValue = 2.0;
            Palette = new List<string>();
            ColorAxisMajorStep = 5;
            IsBottomTrackLine = true;
            Interperlate = false;
        }

        /// <summary>
        /// Oxypalette to list of color strings.
        /// </summary>
        /// <param name="palette">OxyPalette.</param>
        /// <returns>List of colors.</returns>
        public static List<string> PaletteToList(OxyPalette palette)
        {
            var list = new List<string>();
            foreach(var color in palette.Colors)
            {
                list.Add(color.ToString());
            }

            return list;
        }

        /// <summary>
        /// Convert a list of color strings to a OxyPalette.
        /// </summary>
        /// <param name="list">List of colors.</param>
        /// <returns>OxyPalette.</returns>
        public static OxyPalette ListToPalette(List<string> list)
        {
            var colorList = new List<OxyColor>();
            foreach(var color in list)
            {
                colorList.Add(OxyColor.Parse(color));
            }

            return new OxyPalette(colorList);
        }

    }
}
