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
 * 08/30/2015      RC          4.2.0      Initial coding
 * 
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Validation Test view options.
    /// </summary>
    public class ValidationTestViewOptions
    {
        #region Variables

        /// <summary>
        /// Default number of ensembles that will be displayed.
        /// </summary>
        private const int DEFAULT_MAX_ENSEMBLES = 250;

        #endregion

        #region Properties

        /// <summary>
        /// Settings are associated with this subsystem.
        /// </summary>
        [JsonIgnore]
        public SubsystemConfiguration SubsystemConfig { get; set; }

        /// <summary>
        /// Maximum number of ensembles to display.
        /// </summary>
        public int MaxEnsembles { get; set; }

        /// <summary>
        /// Calculate Distance Made Good data.
        /// </summary>
        public bool IsCalculateDmg { get; set; }

        /// <summary>
        /// Should the Correlation and Amplitude plot be displayed as
        /// single ping data or average data.
        /// </summary>
        public bool IsCorrAmpAverage { get; set; }

        /// <summary>
        /// Flag wheter to filter the data for bad data before plotting.
        /// </summary>
        public bool IsFilteringData { get; set; }

        /// <summary>
        /// Declination value.
        /// </summary>
        public float Declination { get; set; }

        #endregion

        /// <summary>
        /// Validation view options.
        /// </summary>
        public ValidationTestViewOptions()
        {
            // Set the subsystem
            SubsystemConfig = new SubsystemConfiguration();

            // Set default values
            SetDefaults();
        }

        /// <summary>
        /// Validation view options.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        public ValidationTestViewOptions(SubsystemConfiguration ssConfig)
        {
            // Set the subsystem
            SubsystemConfig = ssConfig;

            // Set default values
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            MaxEnsembles = DEFAULT_MAX_ENSEMBLES;
            IsCalculateDmg = true;
            IsCorrAmpAverage = true;
            IsFilteringData = true;
            Declination = 0.0f;
        }
    }
}
