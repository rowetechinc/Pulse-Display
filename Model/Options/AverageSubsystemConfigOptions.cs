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
 * 01/15/2013      RC          2.17       Initial coding.
 * 04/15/2015      RC          4.1.2      Moved the LTA and STA options to a seperate class.
 * 
 */

namespace RTI
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AverageOptions
    {
        #region Properties

        /// <summary>
        /// Enable or disable LTA averaging.
        /// </summary>
        public bool IsLtaEnabled { get; set; }

        /// <summary>
        /// Enable or disable STA averaging.
        /// </summary>
        public bool IsStaEnabled { get; set; }

        /// <summary>
        /// Long Term Average manager options.
        /// </summary>
        public AverageManagerOptions LtaAvgMgrOptions { get; set; }

        /// <summary>
        /// Short Term Average manager options.
        /// </summary>
        public AverageManagerOptions StaAvgMgrOptions { get; set; }

        #endregion

        /// <summary>
        /// Initialize the options.
        /// </summary>
        public AverageOptions()
        {
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            IsLtaEnabled = false;
            IsStaEnabled = false;
            LtaAvgMgrOptions = new AverageManagerOptions();
            StaAvgMgrOptions = new AverageManagerOptions();
        }

    }

    /// <summary>
    /// Average options for a subsystem configuration.
    /// </summary>
    public class AverageSubsystemConfigOptions
    {
        #region Variables

        #region Defaults

        /// <summary>
        /// Default value to enable LTA averaging.
        /// </summary>
        private const bool DEFAULT_IS_LTA_ENABLED = false;

        /// <summary>
        /// Default value to enable STA averaging.
        /// </summary>
        private const bool DEFAULT_IS_STA_ENABLED = false;

        /// <summary>
        /// Default value for averaging Correlation data.
        /// </summary>
        private const bool DEFAULT_IS_CORR_AVG = false;

        /// <summary>
        /// Default value for averaging Amplitude data.
        /// </summary>
        private const bool DEFAULT_IS_AMP_AVG = false;

        /// <summary>
        /// Default value for a running average.
        /// </summary>
        private const bool DEFAULT_RUNNING_AVG = false;

        /// <summary>
        /// Default value for number of samples to average
        /// together.
        /// </summary>
        private const int DEFAULT_NUM_SAMPLES = 5;

        /// <summary>
        /// Default value for Reference Layer Averaging.
        /// </summary>
        private const bool DEFAULT_IS_REF_LAYER_AVG = false;

        /// <summary>
        /// Default Minimum Reference Layer bin.
        /// </summary>
        private const int DEFAULT_REFLAYER_MIN = 1;

        /// <summary>
        /// Default Maximum Reference Layer Bin.
        /// </summary>
        private const int DEFAULT_REFLAYER_MAX = 5;

        /// <summary>
        /// Default flag for Beam Velocity averaging.
        /// </summary>
        public const bool DEFAULT_IS_BEAM_VEL_AVG = false;

        /// <summary>
        /// Default flag for Instrument Velocity averaging.
        /// </summary>
        public const bool DEFAULT_IS_INSTRUMENT_VEL_AVG = false;

        /// <summary>
        /// Default flag for Earth Velocity averaging.
        /// </summary>
        public const bool DEFAULT_IS_EARTH_VEL_AVG = false;

        /// <summary>
        /// Default flag for Bottom Track averaging.
        /// </summary>
        public const bool DEFAULT_IS_BT_AVG = false;

        /// <summary>
        /// Default scale.
        /// </summary>
        public const float DEFAULT_SCALE = 1.0f;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Enable or disable LTA averaging.
        /// </summary>
        [JsonIgnore]
        public bool IsLtaEnabled 
        {
            get { return AvgOptions.IsLtaEnabled; } 
            set
            {
                AvgOptions.IsLtaEnabled = value;
            }
        }

        /// <summary>
        /// Enable or disable STA averaging.
        /// </summary>
        [JsonIgnore]
        public bool IsStaEnabled 
        {
            get { return AvgOptions.IsStaEnabled; } 
            set
            {
                AvgOptions.IsStaEnabled = value;
            }
        }

        /// <summary>
        /// Average options for both LTA and STA.
        /// </summary>
        public AverageOptions AvgOptions { get; set; }

        /// <summary>
        /// Long Term Average manager options.
        /// </summary>
        [JsonIgnore]
        public AverageManagerOptions LtaAvgMgrOptions 
        {
            get { return AvgOptions.LtaAvgMgrOptions; } 
            protected set
            {
                AvgOptions.LtaAvgMgrOptions = value;
            }
        }

        /// <summary>
        /// Short Term Average manager options.
        /// </summary>
        [JsonIgnore]
        public AverageManagerOptions StaAvgMgrOptions 
        {
            get { return AvgOptions.StaAvgMgrOptions; } 
            set
            {
                AvgOptions.StaAvgMgrOptions = value;
            }
        }

        /// <summary>
        /// Subsystem Configuration for these options.
        /// </summary>
        public SubsystemConfiguration SubsystemConfig { get; protected set; }

        #endregion

        /// <summary>
        /// Default options with an empty SubsystemConfiguration.
        /// 
        /// The default constructor is needed when converting
        /// to and from JSON format.
        /// </summary>
        public AverageSubsystemConfigOptions()
        {
            // Initialize values
            SubsystemConfig = new SubsystemConfiguration();

            // Set the default values
            AvgOptions = new AverageOptions();
        }

        /// <summary>
        /// Initialize the values and set the default values.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration for these options.</param>
        public AverageSubsystemConfigOptions(SubsystemConfiguration ssConfig)
        {
            // Initialize values
            SubsystemConfig = ssConfig;

            // Set the default values
            AvgOptions = new AverageOptions();
        }
    }
}
