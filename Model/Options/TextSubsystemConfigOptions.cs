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
 * 02/22/2012      RC          2.03       Initial coding.
 * 08/29/2012      RC          2.15       Added Minimum and maximum bin.
 *                                         Created a default constructor.
 * 12/27/2012      RC          2.17       Replaced Subsystem.Empty with Subsystem.IsEmpty().
 * 01/16/2013      RC          2.17       Changed from Subsystem to SubsystemConfiguration.
 * 
 */

using System.ComponentModel;
using Newtonsoft.Json;

namespace RTI
{
    /// <summary>
    /// This class will hold all the settings for the 
    /// average and screen subsystems.
    /// </summary>
    public class TextSubsystemConfigOptions
    {

        #region Defaults

        /// <summary>
        /// Default font size for the data.
        /// </summary>
        public const int DEFAULT_FONT_SIZE = 12;

        /// <summary>
        /// Default measurement standard.
        /// </summary>
        public const Core.Commons.MeasurementStandards DEFAULT_MEASUREMENT_STANDARD = Core.Commons.MeasurementStandards.METRIC;

        /// <summary>
        /// Default coordinate transform.
        /// </summary>
        public const Core.Commons.Transforms DEFAULT_TRANSFORM = Core.Commons.Transforms.EARTH;

        #endregion

        #region Properties

        /// <summary>
        /// Size of the font for the data.
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// Measurement standard set for displaying.
        /// </summary>
        public Core.Commons.MeasurementStandards MeasurementStandard { get; set; }

        /// <summary>
        /// Coordinate transform to display.
        /// </summary>
        public Core.Commons.Transforms Transform { get; set; }

        /// <summary>
        /// Minimum Bin to display.
        /// </summary>
        public int MinimumBin { get; set; }

        /// <summary>
        /// Maximum Bin to display.
        /// </summary>
        public int MaximumBin { get; set; }

        /// <summary>
        /// Settings are associated with this SubsystemConfiguration.
        /// </summary>
        [JsonIgnore]
        public SubsystemConfiguration SubsystemConfig { get; set; }

        #endregion

        /// <summary>
        /// Set the subsystem and set the
        /// default values.
        /// </summary>
        /// <param name="ssConfig">SubsystemConfiguration associated these options.</param>
        public TextSubsystemConfigOptions(SubsystemConfiguration ssConfig)
        {
            // Set the subsystem
            SubsystemConfig = ssConfig;

            // Set default values
            SetDefaults();
        }

        /// <summary>
        /// Use this constructor if all the settings are going to be
        /// set by the user.
        /// 
        /// Need to set the subsystem after contructing.  
        /// Subsystem will be set to empty when constructed.
        /// </summary>
        public TextSubsystemConfigOptions()
        {
            // Set empty subsystem.
            SubsystemConfig = new SubsystemConfiguration();

            // Set default values
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            FontSize = DEFAULT_FONT_SIZE;
            MinimumBin = 0;
            MeasurementStandard = DEFAULT_MEASUREMENT_STANDARD;
            Transform = DEFAULT_TRANSFORM;
        }
    }
}