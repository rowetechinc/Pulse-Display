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
 * 10/18/2017      RC          4.5.0      Initial coding
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
    /// Options for the Water Profile Magnitude and Direction View options.
    /// </summary>
    public class WpMagDirOutputViewOptions
    {
        #region Variables

        /// <summary>
        /// Transform type, Ship.
        /// </summary>
        public const string TRANSFORM_SHIP = "SHIP";

        /// <summary>
        /// Transform type, Earth.
        /// </summary>
        public const string TRANSFORM_EARTH = "EARTH";

        /// <summary>
        /// Transform type, Instrument.
        /// </summary>
        public const string TRANSFORM_INSTRUMENT = "INSTRUMENT";

        #endregion

        #region Properties

        /// <summary>
        /// Flag to enable outputing Water Profile magintidue and direction.
        /// </summary>
        public bool IsOutputEnabled { get; set; }

        /// <summary>
        /// Water Profile Magnitude and Direction output Comm Port.
        /// </summary>
        public string SelectedCommPort { get; set; }

        /// <summary>
        /// Water Profile Magnitude and Direction output Baud rate.
        /// </summary>
        public int SelectedBaud { get; set; }

        /// <summary>
        /// Selected bins to output WP Magnitude and direction.
        /// </summary>
        public string SelectedBins { get; set; }

        /// <summary>
        /// Flag to enable retransforming Magnitude and direction.
        /// </summary>
        public bool IsRetransformData { get; set; }

        /// <summary>
        /// Velocity transformation selected for Magnitude and Direction output.
        /// </summary>
        public string SelectedTransform { get; set; }

        /// <summary>
        /// Can use GPS heading to replace compass heading.
        /// </summary>
        public bool IsUseGpsHeading { get; set; }

        /// <summary>
        /// Offset of the ship heading versus magnetic compass heading in degrees for WP Mag and Dir output.
        /// </summary>
        public float ShipXdcrOffset { get; set; }

        /// <summary>
        /// Heading offset versus the magnetic compas for WP Mag and Dir output.
        /// </summary>
        public float HeadingOffset { get; set; }

        /// <summary>
        /// Heading source for WP Mag and Dir output.
        /// </summary>
        public Transform.HeadingSource SelectedHeadingSource { get; set; }

        /// <summary>
        /// Flag to enable removing the ship speed in the WP Mag and Dir output.
        /// </summary>
        public bool IsRemoveShipSpeed { get; set; }

        /// <summary>
        /// Flag to enable using Bottom Track to remove the ship speed in WP Mag and Dir output.
        /// </summary>
        public bool CanUseBottomTrackVel { get; set; }

        /// <summary>
        /// Flag to enable using GPS to remove the ship speed in WP Mag and Dir output.
        /// </summary>
        public bool CanUseGpsVel { get; set; }

        /// <summary>
        /// Turn on or off recording.
        /// </summary>
        public bool IsRecording { get; set; }

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public WpMagDirOutputViewOptions()
        {
            SetDefaults();
        }

        /// <summary>
        /// Set default options.
        /// </summary>
        public void SetDefaults()
        {
            IsOutputEnabled = false;
            SelectedCommPort = "";
            SelectedBaud = 115200;
            SelectedBins = "4,8,12";
            IsRetransformData = true;
            IsUseGpsHeading = true;
            SelectedTransform = TRANSFORM_SHIP;
            ShipXdcrOffset = 0.0f;
            HeadingOffset = 0.0f;
            SelectedHeadingSource = Transform.HeadingSource.GPS1;
            IsRemoveShipSpeed = true;
            CanUseBottomTrackVel = true;
            CanUseGpsVel = true;
            IsRecording = false;
        }
    }
}
