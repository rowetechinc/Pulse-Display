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
 * 12/31/2014      RC          0.0.1      Initial coding
 * 
 * 
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Waves options.
    /// </summary>
    public class ViewDataWavesOptions
    {
        #region Default

        /// <summary>
        /// Default number of wave bands.
        /// </summary>
        protected const int DEFAULT_WAVES_MAX_BANDS = 100;

        /// <summary>
        /// Default minimum Frequency.
        /// </summary>
        protected const double DEFAULT_MIN_FREQ = 0.035;

        /// <summary>
        /// Default Max scale factor.
        /// </summary>
        protected const double DEFAULT_MAX_SCALE_FACTOR = 200;

        /// <summary>
        /// Default minimum height.
        /// </summary>
        protected const double DEFAULT_MIN_HEIGHT = 0.1;

        /// <summary>
        /// Default is Height sensor beam.
        /// </summary>
        protected const bool DEFAULT_IS_HEIGHT_SENSOR_BEAM = true;

        #endregion

        #region Properties

        /// <summary>
        /// Number of Waves bands to try and display.
        /// </summary>
        public int NumWavesBands { get; set; }

        /// <summary>
        /// Waves minimum frequency.
        /// </summary>
        public double WavesMinFreq { get; set; }

        /// <summary>
        /// Waves Max Scale Factor.
        /// </summary>
        public double WavesMaxScaleFactor { get; set; }

        /// <summary>
        /// Waves minimum height.
        /// </summary>
        public double WavesMinHeight { get; set; }

        /// <summary>
        /// Set a flag if using Height sensor beam.
        /// </summary>
        public bool IsHeightSensorBeam { get; set; }

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public ViewDataWavesOptions()
        {
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            NumWavesBands = DEFAULT_WAVES_MAX_BANDS;
            WavesMinFreq = DEFAULT_MIN_FREQ;
            WavesMaxScaleFactor = DEFAULT_MAX_SCALE_FACTOR;
            WavesMinHeight = DEFAULT_MIN_HEIGHT;
            IsHeightSensorBeam = DEFAULT_IS_HEIGHT_SENSOR_BEAM;
        }

    }
}
