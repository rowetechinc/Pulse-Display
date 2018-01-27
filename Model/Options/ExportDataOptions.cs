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
 * 08/26/2013      RC          3.0.8       Initial coding.
 * 
 */
namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using Newtonsoft.Json;

    /// <summary>
    /// Options for the Export data.
    /// </summary>
    public class ExportDataOptions
    {
        #region Defaults

        /// <summary>
        /// Default if Beam Velocity is selected for export.
        /// </summary>
        public const bool DEFAULT_IS_BEAM_SELECTED = true;

        /// <summary>
        /// Default if Instrument Velocity is selected for export.
        /// </summary>
        public const bool DEFAULT_IS_INSTRUMENT_SELECTED = true;

        /// <summary>
        /// Default if Earth Velocity is selected for export.
        /// </summary>
        public const bool DEFAULT_IS_EARTH_SELECTED = true;

        /// <summary>
        /// Default minimum number of bins.
        /// </summary>
        public const int DEFAULT_MIN_INDEX = 0;

        /// <summary>
        /// Default maximum number of bins.
        /// </summary>
        public const int DEFAULT_MAX_INDEX = DataSet.Ensemble.MAX_NUM_BINS;

        #endregion

        #region Properties

        #region Beam Velocity

        /// <summary>
        /// Flag if the Beam velocity is selected.
        /// </summary>
        public bool IsBeamVelSelected { get; set; }

        /// <summary>
        /// Beam Velocity Bin Minimum index.
        /// </summary>
        public int BeamMinIndex { get; set; }

        /// <summary>
        /// Beam Velcoity Bin Maximum index.
        /// </summary>
        public int BeamMaxIndex { get; set; }

        #endregion

        #region Instrument Velocity

        /// <summary>
        /// Flag if the Instrument velocity is selected.
        /// </summary>
        public bool IsInstrumentVelSelected { get; set; }

        /// <summary>
        /// Instrument Velocity Bin Minimum index.
        /// </summary>
        public int InstrumentMinIndex { get; set; }

        /// <summary>
        /// Instrument Velcoity Bin Maximum index.
        /// </summary>
        public int InstrumentMaxIndex { get; set; }

        #endregion

        #region Earth Velocity

        /// <summary>
        /// Flag if the Earth velocity is selected.
        /// </summary>
        public bool IsEarthVelSelected { get; set; }

        /// <summary>
        /// Earth Velocity Bin Minimum index.
        /// </summary>
        public int EarthMinIndex { get; set; }

        /// <summary>
        /// Earth Velcoity Bin Maximum index.
        /// </summary>
        public int EarthMaxIndex { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Initialize the options to default.
        /// </summary>
        public ExportDataOptions()
        {

        }

        /// <summary>
        /// Constructor for JSON conversion.
        /// </summary>
        /// <param name="IsBeamVelSelected">Is Beam Velocity selected.</param>
        /// <param name="BeamMinIndex">Mininum Beam Velocity Bin Index.</param>
        /// <param name="BeamMaxIndex">Maxinum Beam Velocity Bin Index.</param>
        /// <param name="IsInstrumentVelSelected">Is Instrument Velcity selected.</param>
        /// <param name="InstrumentMinIndex">Mininum Instrument Velocity Bin Index.</param>
        /// <param name="InstrumentMaxIndex">Maxinum Instrument Velocity Bin Index.</param>
        /// <param name="IsEarthVelSelected">Is Earth Velocity selected.</param>
        /// <param name="EarthMinIndex">Mininum Earth Velocity Bin Index.</param>
        /// <param name="EarthMaxIndex">Maxinum Earth Velocity Bin Index.</param>
        [JsonConstructor]
        public ExportDataOptions(bool IsBeamVelSelected, int BeamMinIndex, int BeamMaxIndex, 
                                    bool IsInstrumentVelSelected, int InstrumentMinIndex, int InstrumentMaxIndex,
                                    bool IsEarthVelSelected, int EarthMinIndex, int EarthMaxIndex)
        {
            this.IsBeamVelSelected = IsBeamVelSelected;
            this.BeamMinIndex = BeamMinIndex;
            this.BeamMaxIndex = BeamMaxIndex;
            this.IsInstrumentVelSelected = IsInstrumentVelSelected;
            this.InstrumentMinIndex = InstrumentMinIndex;
            this.InstrumentMaxIndex = InstrumentMaxIndex;
            this.IsEarthVelSelected = IsEarthVelSelected;
            this.EarthMinIndex = EarthMinIndex;
            this.EarthMaxIndex = EarthMaxIndex;
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            IsBeamVelSelected = DEFAULT_IS_BEAM_SELECTED;
            BeamMinIndex = DEFAULT_MIN_INDEX;
            BeamMaxIndex = DEFAULT_MAX_INDEX;
            IsInstrumentVelSelected = DEFAULT_IS_INSTRUMENT_SELECTED;
            InstrumentMinIndex = DEFAULT_MIN_INDEX;
            InstrumentMaxIndex = DEFAULT_MAX_INDEX;
            IsEarthVelSelected = DEFAULT_IS_EARTH_SELECTED;
            EarthMinIndex = DEFAULT_MIN_INDEX;
            EarthMaxIndex = DEFAULT_MAX_INDEX;
        }
    }
}
