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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Time Series Options.
    /// </summary>
    public class TimeSeriesOptions
    {
        /// <summary>
        /// Data Source.
        /// </summary>
        public DataSource.eSource Source { get; set; }

        /// <summary>
        /// Series Type.
        /// </summary>
        public BaseSeriesType.eBaseSeriesType Type { get; set; }

        /// <summary>
        /// Beam number.
        /// </summary>
        public int Beam { get; set; }

        /// <summary>
        /// Bin Number.
        /// </summary>
        public int Bin { get; set; }

        /// <summary>
        /// Line color.
        /// </summary>
        public string Color { get; set; }

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

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public TimeSeriesOptions()
        {
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            Source = DataSource.eSource.BottomTrack;
            Type = BaseSeriesType.eBaseSeriesType.Base_Range;
            Beam = 0;
            Bin = 0;
            Color = BeamColor.DEFAULT_COLOR_BEAM_0;
        }

    }
}
