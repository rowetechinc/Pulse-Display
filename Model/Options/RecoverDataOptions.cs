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
 * 12/31/2014      RC          0.0.1       Initial coding
 * 01/14/2015      RC          0.0.2       Changed Selected bins to a string to know if disabled.
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
    /// Options for the Recover Data View.
    /// </summary>
    public class RecoverDataOptions
    {

        #region Enum

        /// <summary>
        /// Height source.
        /// </summary>
        public enum HeightSource
        {
            /// <summary>
            /// Beam 0.
            /// </summary>
            Beam0 = 0,

            /// <summary>
            /// Beam 1.
            /// </summary>
            Beam1 = 1,

            /// <summary>
            /// Beam 2.
            /// </summary>
            Beam2 = 2,

            /// <summary>
            /// Beam 3.
            /// </summary>
            Beam3 = 3,

            /// <summary>
            /// Vertical Beam.
            /// </summary>
            Vertical = 4
        }

        #endregion

        #region Defaults

        /// <summary>
        /// Timeout for the download process.  This value
        /// is in minutes.
        /// IF THE DOWNLOAD TAKES LONGER THEN 10 MINUTES, THEN THE FILE
        /// WILL BE STOPPED EARLY.  THIS MAY NEED TO BE CONFIGURED BY THE
        /// USER.
        /// </summary>
        private const int DOWNLOAD_TIMEMOUT = 10;

        /// <summary>
        /// Value for a selected bin to be disabled.
        /// </summary>
        public const string DISABLE_BIN_SELECTION = "Disable";

        #endregion

        #region Properties

        /// <summary>
        /// Correlation threshold for data screening.
        /// </summary>
        public float CorrelationThreshold { get; set; }

        /// <summary>
        /// Pressure sensor offset.
        /// </summary>
        public float PressureOffset { get; set; }

        /// <summary>
        /// Beam height source.
        /// </summary>
        public HeightSource BeamHeightSource { get; set; }

        /// <summary>
        /// Selected bin 1.
        /// </summary>
        public string Bin1Selection { get; set; }

        /// <summary>
        /// Selected bin 2.
        /// </summary>
        public string Bin2Selection { get; set; }

        /// <summary>
        /// Selected bin 3.
        /// </summary>
        public string Bin3Selection { get; set; }

        /// <summary>
        /// Latitude.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Longitude.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Pressure sensor Height.
        /// </summary>
        public float PressureSensorHeight { get; set; }

        /// <summary>
        /// Period of time to wait for the file to be downloaded before
        /// moving on to the next file.   If the download process hangs,
        /// it will wait this long to download the file.  If the download
        /// is taking to long, it will only wait this amount of time to
        /// download a file.
        /// </summary>
        public int DownloadTimeout { get; set; }

        /// <summary>
        /// Directory to download the files
        /// from the ADCP to the user's computer.
        /// </summary>
        public string DownloadDirectory { get; set; }

        /// <summary>
        /// Flag to overwrite the file if it exist.
        /// TRUE = Overwrite the file.
        /// FALSE = Do not download the file, it already exist.
        /// </summary>
        public bool OverwriteDownloadFiles { get; set; }

        /// <summary>
        /// Set flag if the downloaded data should be parsed
        /// or just written to the file.
        /// TRUE = Parse data and write data to a file and database.
        /// FALSE  = Write data to file only.
        /// </summary>
        public bool ParseDownloadedData { get; set; }

        /// <summary>
        /// Heading offset.  Value added to the heading value.  Then data retransformed.
        /// </summary>
        public float HeadingOffset { get; set; }

        /// <summary>
        /// Pitch offset.  Value added to the pitch value.  Then data retransformed.
        /// </summary>
        public float PitchOffset { get; set; }

        /// <summary>
        /// Roll offset.  Value added to the roll value.  Then data retransformed.
        /// </summary>
        public float RollOffset { get; set; }

        /// <summary>
        /// Flag to replace the Pressure data with the vertical beam.
        /// In small waves environment, if the pressure sensor is not
        /// configured properly, the pressure data will be bad.  This
        /// will replace the pressure data with the vertical beam data
        /// so Wavector can still process the data.
        /// </summary>
        public bool IsReplacePressure { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public RecoverDataOptions()
        {
            SetDefaults();
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            Bin1Selection = "5";
            Bin2Selection = "6";
            Bin3Selection = "7";

            BeamHeightSource = HeightSource.Vertical;

            CorrelationThreshold = 0.25f;
            PressureOffset = 0.0f;

            Latitude = 0.0f;
            Longitude = 0.0f;
            PressureSensorHeight = 0.0f;

            DownloadTimeout = DOWNLOAD_TIMEMOUT;
            ParseDownloadedData = true;
            OverwriteDownloadFiles = true;
            DownloadDirectory = Pulse.Commons.DEFAULT_RECORD_DIR;

            HeadingOffset = 0.0f;
            PitchOffset = 0.0f;
            RollOffset = 0.0f;

            IsReplacePressure = false;
        }

        /// <summary>
        /// Return a list of selected bins by the user.
        /// </summary>
        /// <returns>List of selected bins.</returns>
        public List<int> GetSelectedBinList()
        {
            // Generate the list based off the selections
            var selectedBins = new List<int>();
            if (Bin1Selection != RecoverDataOptions.DISABLE_BIN_SELECTION)
            {
                selectedBins.Add(Convert.ToInt32(Bin1Selection));
            }
            if (Bin2Selection != RecoverDataOptions.DISABLE_BIN_SELECTION)
            {
                selectedBins.Add(Convert.ToInt32(Bin2Selection));
            }
            if (Bin3Selection != RecoverDataOptions.DISABLE_BIN_SELECTION)
            {
                selectedBins.Add(Convert.ToInt32(Bin3Selection));
            }

            return selectedBins;
        }

    }
}
