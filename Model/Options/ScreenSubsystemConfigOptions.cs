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
 * 01/16/2013      RC          2.17       Initial coding.
 * 08/16/2013      RC          2.19.4     Added GpsHeadingOffset for GPS heading offset to ADCP.
 *                                         Changed default DEFAULT_IS_REMOVE_SHIP_SPEED to true.
 *                                         Changed default DEFAULT_MARK_BAD_BELOW_BOTTOM to true.
 * 05/07/2014      RC          3.2.4       Added Correlation Threshold and SNR threshold for retransforming the data.
 * 01/06/2016      RC          3.3.0      Added IsRetransformUseGpsHeading and RetransformHeadingOffset.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Average options for a subsystem configuration.
    /// </summary>
    public class ScreenSubsystemConfigOptions
    {
        #region Variables

        #region Defaults

        /// <summary>
        /// Screen Mark Bad Below bottom default;
        /// </summary>
        private const bool DEFAULT_MARK_BAD_BELOW_BOTTOM = true;

        /// <summary>
        /// Screen bottom track range default
        /// </summary>
        private const bool DEFAULT_IS_SCREEN_BT_RANGE = false;

        /// <summary>
        /// Screen Bottom Track Range Standard Deviation Mulitiplier default.
        /// </summary>
        private const double DEFAULT_SCREEN_BT_RANGE_STD_MULTIPLER = 2.0;

        /// <summary>
        /// Default Minimum Reference Layer bin.
        /// </summary>
        private const int DEFAULT_REFLAYER_MIN = 1;

        /// <summary>
        /// Default Maximum Reference Layer Bin.
        /// </summary>
        private const int DEFAULT_REFLAYER_MAX = 5;

        /// <summary>
        /// Default flag to Remove the ship speed.
        /// </summary>
        private const bool DEFAULT_IS_REMOVE_SHIP_SPEED = true;

        /// <summary>
        /// Default flag to use Bottom Track velocity
        /// to remove Ship speed.
        /// </summary>
        private const bool DEFAULT_CAN_USE_BT_VEL = true;

        /// <summary>
        /// Default flag to use GPS velocity
        /// to remove Ship speed.
        /// </summary>
        private const bool DEFAULT_CAN_USE_GPS_VEL = true;

        /// <summary>
        /// Default GPS heading offset.
        /// </summary>
        private const double DEFAULT_GPS_HEADING_OFFSET = 0.0;

        /// <summary>
        /// Default flag to screen the velocity error value with a 
        /// threshold value.
        /// </summary>
        private const bool DEFAULT_IS_SCREEN_VELOCITY_THRESHOLD = false;

        /// <summary>
        /// Default value for the velocity screen threshold.
        /// </summary>
        private const float DEFAULT_SCREEN_VELOCITY_THRESHOLD = 0.25f;

        /// <summary>
        /// Default flag to mark a beam bad for 3 beam solution.
        /// </summary>
        private const bool DEFAULT_IS_FORCE_3_BEAM_SOLUTION = false;

        /// <summary>
        /// Default value for ForceBeamBad.
        /// </summary>
        private const int DEFAULT_FORCE_BAD_BEAM = 0;

        /// <summary>
        /// Flag to retransform the data.
        /// </summary>
        private const bool DEFAULT_IS_RETRANSFORM_DATA = false;

        /// <summary>
        /// Default flag to mark a beam bad for 3 Bottom Track beam solution.
        /// </summary>
        private const bool DEFAULT_IS_FORCE_3_BT_BEAM_SOLUTION = false;

        /// <summary>
        /// Default value for Bottom Track  ForceBeamBad.
        /// </summary>
        private const int DEFAULT_FORCE_BAD_BT_BEAM = 0;

        /// <summary>
        /// Flag to retransform the Bottom Track data.
        /// </summary>
        private const bool DEFAULT_IS_RETRANSFORM_BT_DATA = false;

        /// <summary>
        /// Heading source.
        /// </summary>
        private const Transform.HeadingSource DEFAULT_HEADING_SOURCE = Transform.HeadingSource.ADCP;

        /// <summary>
        /// Heading offset when retransforming the data.
        /// </summary>
        private const float DEFAULT_RETRANSFORM_HEADING_OFFSET = 0.0f;

        /// <summary>
        /// Default Water Profile Correlation Threshold for BB.
        /// </summary>
        private const float DEFAULT_WP_CORR_THRESH = 0.25f;

        /// <summary>
        /// Default Bottom Track Correlation Threshold for BB.
        /// </summary>
        private const float DEFAULT_BT_CORR_THRESH = 0.90f;

        /// <summary>
        /// Default Bottom Track SNR Threshold for BB.
        /// </summary>
        private const float DEFAULT_BT_SNR_THRESH = 10.0f;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Screen the velocity data for any velocities
        /// below the bottom.  If any velocity is below the
        /// bottom, mark it bad.
        /// </summary>
        public bool IsMarkBadBelowBottom { get; set; }

        /// <summary>
        /// Screen the Bottom Track Range values.
        /// </summary>
        public bool IsScreenBottomTrackRange { get; set; }

        /// <summary>
        /// Multiplier used with the Standard Deviation calculated for the 
        /// Bottom Track Range.
        /// </summary>
        public double ScreenBottomTrackRangeMultiplier { get; set; }

        /// <summary>
        /// Screen the Error (Q) velocity value using
        /// a threshold value.
        /// </summary>
        public bool IsScreenErrorVelocityThreshold { get; set; }

        /// <summary>
        /// Threshold value for screening the Error (Q) velocity
        /// value.
        /// </summary>
        public float ScreenErrorVelocityThreshold { get; set; }

        /// <summary>
        /// Screen the velocity error value using
        /// a threshold value.
        /// </summary>
        public bool IsScreenVerticalVelocityThreshold { get; set; }

        /// <summary>
        /// Threshold value for screening the Vertical velocity
        /// value.
        /// </summary>
        public float ScreenVerticalVelocityThreshold { get; set; }

        /// <summary>
        /// Flag used to determine if Remove Ship Speed
        /// should be done to the ensemble.  This will
        /// remove the ship speed from the water velocity data.
        /// </summary>
        public bool IsRemoveShipSpeed { get; set; }

        /// <summary>
        /// Set flag whether the Bottom Track Velocity
        /// can be used to remove the Ship speed.
        /// </summary>
        public bool CanUseBottomTrackVel { get; set; }

        /// <summary>
        /// Set flag whether the GPS Velocity
        /// can be used to remove the Ship speed.
        /// </summary>
        public bool CanUseGpsVel { get; set; }

        /// <summary>
        /// The GPS heading may not match with the ADCP heading, 
        /// so a gpsHeadingOffset will also have to be
        /// set to account for the GPS not aligned with the ADCP heading.
        /// </summary>
        public double GpsHeadingOffset { get; set; }

        #region ReTransfrom Data

        /// <summary>
        /// Flag to retransform the data.
        /// This will transform the data using the Beam Velocity data.
        /// </summary>
        public bool IsRetransformData { get; set; }

        /// <summary>
        /// Heading source.
        /// </summary>
        public Transform.HeadingSource RetransformHeadingSource { get; set; }

        /// <summary>
        /// Heading offset to use with heading.
        /// </summary>
        public float RetransformHeadingOffset { get; set; }

        /// <summary>
        /// Water Profile Correlation Threshold.
        /// Anything less than this value will be considered bad.
        /// </summary>
        public float WpCorrThresh { get; set; }

        /// <summary>
        /// Bottom Track Correlation Threshold.
        /// Anything less than this value will be considered bad.
        /// </summary>
        public float BtCorrThresh { get; set; }

        /// <summary>
        /// Bottom Track SNR Threshold.
        /// Anything less than this value will be considered bad.
        /// </summary>
        public float BtSnrThresh { get; set; }

        #endregion

        #region Force 3 Beam

        /// <summary>
        /// Flag to force a 3 beam solution.  This will be used with the
        /// beam selected to force bad.
        /// </summary>
        public bool IsForce3BeamSolution { get; set; }

        /// <summary>
        /// Beam to force bad.  This has to be in the range
        /// of the number of beams available for the subsystem.
        /// </summary>
        public int ForceBeamBad { get; set; }

        #endregion

        #region BT Force 3 Beam

        /// <summary>
        /// Flag to force a 3 beam solution.  This will be used with the
        /// beam selected to force bad.
        /// </summary>
        public bool IsForce3BottomTrackBeamSolution { get; set; }

        /// <summary>
        /// Beam to force bad.  This has to be in the range
        /// of the number of beams available for the subsystem.
        /// </summary>
        public int ForceBottomTrackBeamBad { get; set; }

        #endregion

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
        public ScreenSubsystemConfigOptions()
        {
            // Initialize values
            SubsystemConfig = new SubsystemConfiguration();

            // Set the default values
            SetDefaults();
        }

        /// <summary>
        /// Initialize the values and set the default values.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration for these options.</param>
        public ScreenSubsystemConfigOptions(SubsystemConfiguration ssConfig)
        {
            // Initialize values
            SubsystemConfig = ssConfig;

            // Set the default values
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            IsMarkBadBelowBottom = DEFAULT_MARK_BAD_BELOW_BOTTOM;
            IsScreenBottomTrackRange = DEFAULT_IS_SCREEN_BT_RANGE;
            IsRemoveShipSpeed = DEFAULT_IS_REMOVE_SHIP_SPEED;
            CanUseBottomTrackVel = DEFAULT_CAN_USE_BT_VEL;
            CanUseGpsVel = DEFAULT_CAN_USE_GPS_VEL;
            GpsHeadingOffset = DEFAULT_GPS_HEADING_OFFSET;
            IsScreenErrorVelocityThreshold = DEFAULT_IS_SCREEN_VELOCITY_THRESHOLD;
            ScreenErrorVelocityThreshold = DEFAULT_SCREEN_VELOCITY_THRESHOLD;
            IsScreenVerticalVelocityThreshold = DEFAULT_IS_SCREEN_VELOCITY_THRESHOLD;
            ScreenVerticalVelocityThreshold = DEFAULT_SCREEN_VELOCITY_THRESHOLD;
            ScreenBottomTrackRangeMultiplier = DEFAULT_SCREEN_BT_RANGE_STD_MULTIPLER;
            IsForce3BeamSolution = DEFAULT_IS_FORCE_3_BEAM_SOLUTION;
            ForceBeamBad = DEFAULT_FORCE_BAD_BEAM;
            IsForce3BottomTrackBeamSolution = DEFAULT_IS_FORCE_3_BT_BEAM_SOLUTION;
            ForceBottomTrackBeamBad = DEFAULT_FORCE_BAD_BT_BEAM;
            IsRetransformData = DEFAULT_IS_RETRANSFORM_DATA;
            WpCorrThresh = DEFAULT_WP_CORR_THRESH;
            BtCorrThresh = DEFAULT_BT_CORR_THRESH;
            BtSnrThresh = DEFAULT_BT_SNR_THRESH;
            RetransformHeadingSource = DEFAULT_HEADING_SOURCE;
            RetransformHeadingOffset = DEFAULT_RETRANSFORM_HEADING_OFFSET;

        }
    }
}
