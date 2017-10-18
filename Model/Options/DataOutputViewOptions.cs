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
    /// Options for the Data Output view.
    /// </summary>
    public class DataOutputViewOptions
    {
        #region Variables

        /// <summary>
        /// Encoding type, PD6 and PD13.
        /// </summary>
        public const string ENCODING_PD6_PD13 = "PD6 and PD13";

        /// <summary>
        /// Encoding type, VmDas.
        /// </summary>
        public const string ENCODING_VMDAS = "VmDas";

        /// <summary>
        /// Encoding type, Binary Ensemble.
        /// </summary>
        public const string ENCODING_Binary_ENS = "Binary Ensemble";

        /// <summary>
        /// Encoding type, PD0.
        /// </summary>
        public const string ENCODING_PD0 = "PD0";

        /// <summary>
        /// Encoding type, Retransform PD6.
        /// </summary>
        public const string ENCODING_RETRANSFORM_PD6 = "Retransform PD6";

        #endregion

        #region Properties

        /// <summary>
        /// Flag to enable this feature.
        /// </summary>
        public bool IsOutputEnabled { get; set; }

        /// <summary>
        /// Data Output comm port.
        /// </summary>
        public string SelectedCommPort { get; set; }

        /// <summary>
        /// Data Output baud rate.
        /// </summary>
        public int SelectedBaud { get; set; }

        /// <summary>
        /// Output format to output to the comm port.s
        /// </summary>
        public string SelectedFormat { get; set; }

        /// <summary>
        /// Flag to retransform the data.
        /// </summary>
        public bool IsRetransformData { get; set; }

        /// <summary>
        /// Use GPS heading when retransforming the data.
        /// </summary>
        public bool IsUseGpsHeading { get; set; }

        /// <summary>
        /// Ship offset in degrees.
        /// </summary>
        public float ShipXdcrOffset { get; set; }

        /// <summary>
        /// Heading Offset in degrees.
        /// </summary>
        public float HeadingOffset { get; set; }

        /// <summary>
        /// Source for the heading.
        /// </summary>
        public Transform.HeadingSource SelectedHeadingSource { get; set; }

        /// <summary>
        /// Flag to enable removing ship speed.
        /// </summary>
        public bool IsRemoveShipSpeed { get; set; }

        /// <summary>
        /// Flag to enable using BT to remove ship speed.
        /// </summary>
        public bool CanUseBottomTrackVel { get; set; }

        /// <summary>
        /// Flag to enable using GPS to remove ship speed.
        /// </summary>
        public bool CanUseGpsVel { get; set; }

        /// <summary>
        /// Flag to enable Water Track.
        /// </summary>
        public bool IsCalculateWaterTrack { get; set; }

        /// <summary>
        /// Mininum bin for Water Track.
        /// </summary>
        public int WtMinBin { get; set; }

        /// <summary>
        /// Maximum bin for Water Track.
        /// </summary>
        public int WtMaxBin { get; set; }

        /// <summary>
        /// Minimum bin to output in VmDas output format.
        /// </summary>
        public int VmDasMinBin { get; set; }

        /// <summary>
        /// Maximum bin to output in VmDas output format.
        /// </summary>
        public int VmDasMaxBin { get; set; }

        /// <summary>
        /// Coordinate transform for PD0.
        /// </summary>
        public PD0.CoordinateTransforms SelectedCoordTransform { get; set; }

        /// <summary>
        /// Flag to enable PD0 output.
        /// </summary>
        public bool IsPd0Selected { get; set; }

        /// <summary>
        /// Turn on or off recording.
        /// </summary>
        public bool IsRecording { get; set; }

        #endregion

        /// <summary>
        /// Initialize the options.
        /// </summary>
        public DataOutputViewOptions()
        {
            // Set default values
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
            SelectedFormat = ENCODING_PD6_PD13;
            IsRetransformData = true;
            IsUseGpsHeading = true;
            ShipXdcrOffset = 0.0f;
            HeadingOffset = 0.0f;
            SelectedHeadingSource = Transform.HeadingSource.GPS1;
            IsPd0Selected = true;
            IsRemoveShipSpeed = true;
            CanUseBottomTrackVel = true;
            CanUseGpsVel = true;
            IsCalculateWaterTrack = true;
            WtMinBin = 3;
            WtMaxBin = 4;
            VmDasMinBin = 1;
            VmDasMaxBin = 200;
            SelectedCoordTransform = PD0.CoordinateTransforms.Coord_Earth;
            IsRecording = false;
        }
    }
}
