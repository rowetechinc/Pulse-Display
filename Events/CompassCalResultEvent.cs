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
 * 02/03/2015      RC          4.1.0      Initial coding
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    public class CompassCalResultEvent
    {
        /// <summary>
        /// IsSelected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Date and time the compass cal was done.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// User name who ran the test.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// ADCP Serial Number.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// ADCP Firmware version.
        /// </summary>
        public string Firmware { get; set; }

        #region Magnitude

        /// <summary>
        /// Calibration score magnitude standard deviation.
        /// </summary>
        public float CalScore_StdDevErr { get; set; }

        /// <summary>
        /// Calibration score X magnitude  coverage.
        /// </summary>
        public float CalScore_xCoverage { get; set; }

        /// <summary>
        /// Calibration score Y magnitude  coverage.
        /// </summary>
        public float CalScore_yCoverage { get; set; }

        /// <summary>
        /// Calibration score Z magnitude coverage.
        /// </summary>
        public float CalScore_zCoverage { get; set; }

        #endregion

        #region Acceleration

        /// <summary>
        /// Calibration score acceleration standard deviation.
        /// </summary>
        public float CalScore_accelStdDevErr { get; set; }

        /// <summary>
        /// Calibration score X acceleration coverage.
        /// </summary>
        public UInt16 CalScore_xAccelCoverage { get; set; }

        /// <summary>
        /// Calibration score Y acceleration coverage.
        /// </summary>
        public UInt16 CalScore_yAccelCoverage { get; set; }

        /// <summary>
        /// Calibration score Z acceleration coverage.
        /// </summary>
        public UInt16 CalScore_zAccelCoverage { get; set; }

        #endregion

        #region PrePoints

        #region Point 1

        /// <summary>
        /// PrePoint 1 Heading.
        /// </summary>
        public double Point1_Pre_Hdg { get; set; }

        /// <summary>
        /// PrePoint 1 Pitch.
        /// </summary>
        public double Point1_Pre_Ptch { get; set; }

        /// <summary>
        /// PrePoint 1 Roll.
        /// </summary>
        public double Point1_Pre_Roll { get; set; }

        #endregion

        #region Point 2

        /// <summary>
        /// PrePoint 2 Heading.
        /// </summary>
        public double Point2_Pre_Hdg { get; set; }

        /// <summary>
        /// PrePoint 2 Pitch.
        /// </summary>
        public double Point2_Pre_Ptch { get; set; }

        /// <summary>
        /// PrePoint 2 Roll.
        /// </summary>
        public double Point2_Pre_Roll { get; set; }

        #endregion

        #region Point 3

        /// <summary>
        /// PrePoint 3 Heading.
        /// </summary>
        public double Point3_Pre_Hdg { get; set; }

        /// <summary>
        /// PrePoint 3 Pitch.
        /// </summary>
        public double Point3_Pre_Ptch { get; set; }

        /// <summary>
        /// PrePoint 3 Roll.
        /// </summary>
        public double Point3_Pre_Roll { get; set; }

        #endregion

        #region Point 4

        /// <summary>
        /// PrePoint 4 Heading.
        /// </summary>
        public double Point4_Pre_Hdg { get; set; }

        /// <summary>
        /// PrePoint 4 Pitch.
        /// </summary>
        public double Point4_Pre_Ptch { get; set; }

        /// <summary>
        /// PrePoint 4 Roll.
        /// </summary>
        public double Point4_Pre_Roll { get; set; }

        #endregion

        #endregion

        #region PostPoints

        #region Point 1

        /// <summary>
        /// PostPoint 1 Heading.
        /// </summary>
        public double Point1_Post_Hdg { get; set; }

        /// <summary>
        /// PostPoint 1 Pitch.
        /// </summary>
        public double Point1_Post_Ptch { get; set; }

        /// <summary>
        /// PostPoint 1 Roll.
        /// </summary>
        public double Point1_Post_Roll { get; set; }

        #endregion

        #region Point 2

        /// <summary>
        /// PostPoint 2 Heading.
        /// </summary>
        public double Point2_Post_Hdg { get; set; }

        /// <summary>
        /// PostPoint 2 Pitch.
        /// </summary>
        public double Point2_Post_Ptch { get; set; }

        /// <summary>
        /// PostPoint 2 Roll.
        /// </summary>
        public double Point2_Post_Roll { get; set; }

        #endregion

        #region Point 3

        /// <summary>
        /// PostPoint 3 Heading.
        /// </summary>
        public double Point3_Post_Hdg { get; set; }

        /// <summary>
        /// PostPoint 3 Pitch.
        /// </summary>
        public double Point3_Post_Ptch { get; set; }

        /// <summary>
        /// PostPoint 3 Roll.
        /// </summary>
        public double Point3_Post_Roll { get; set; }

        #endregion

        #region Point 4

        /// <summary>
        /// PostPoint 4 Heading.
        /// </summary>
        public double Point4_Post_Hdg { get; set; }

        /// <summary>
        /// PostPoint 4 Pitch.
        /// </summary>
        public double Point4_Post_Ptch { get; set; }

        /// <summary>
        /// PostPoint 4 Roll.
        /// </summary>
        public double Point4_Post_Roll { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public CompassCalResultEvent()
        {
            IsSelected = true;
            Created = DateTime.Now;
            SerialNumber = "";
            Firmware = "";
            CalScore_StdDevErr = 0;
            CalScore_xCoverage = 0;
            CalScore_yCoverage = 0;
            CalScore_zCoverage = 0;
            CalScore_accelStdDevErr = 0;
            CalScore_xAccelCoverage = 0;
            CalScore_yAccelCoverage = 0;
            CalScore_zAccelCoverage = 0;
            Point1_Post_Hdg = 0;
            Point1_Pre_Hdg = 0;
            Point1_Post_Ptch = 0;
            Point1_Pre_Ptch = 0;
            Point1_Post_Roll = 0;
            Point1_Pre_Roll = 0;
            Point2_Post_Hdg = 0;
            Point2_Pre_Hdg = 0;
            Point2_Post_Ptch = 0;
            Point2_Pre_Ptch = 0;
            Point2_Post_Roll = 0;
            Point2_Pre_Roll = 0;
            Point3_Post_Hdg = 0;
            Point3_Pre_Hdg = 0;
            Point3_Post_Ptch = 0;
            Point3_Pre_Ptch = 0;
            Point3_Post_Roll = 0;
            Point3_Pre_Roll = 0;
            Point4_Post_Hdg = 0;
            Point4_Pre_Hdg = 0;
            Point4_Post_Ptch = 0;
            Point4_Pre_Ptch = 0;
            Point4_Post_Roll = 0;
            Point4_Pre_Roll = 0;

            UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }


    }
}
