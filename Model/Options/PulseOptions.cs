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
 * 08/30/2012      RC          2.15       Initial coding
 * 12/23/2013      RC          3.2.1      Added PredictorUserInput.
 * 01/02/2014      RC          3.2.2      Added SelectedProjectID to know the last selected project.
 * 01/17/2014      RC          3.2.3      Added GPS1, GPS2, NMEA1 and NMEA2 serial options.
 * 04/07/2015      RC          4.1.2      Added BackscatterOptions.
 * 04/15/2015      RC          4.1.2      Added AverageOptions.
 * 06/23/2015      RC          4.1.3      Added TankTestOptions.
 * 08/13/2015      RC          4.2.0      Added ViewDataWavesOptions.
 * 
 */

using Newtonsoft.Json;
namespace RTI
{

    /// <summary>
    /// Store the Pulse options.  This object is used to be
    /// serialized and stored in the Pulse database.
    /// 
    /// These are the generic
    /// options for the software.  These options
    /// are not particular to a project and are generally 
    /// loaded at the start of the application to get the
    /// last best good options. 
    /// 
    /// </summary>
    public class PulseOptions
    {
        #region Defaults

        /// <summary>
        /// Default font size.
        /// </summary>
        private const int DEFAULT_FONT_SIZE = 12;

        #endregion

        #region Properties

        /// <summary>
        /// Revision of the Pulse database.
        /// </summary>
        public string Revision { get; set; }

        /// <summary>
        /// Project folder path for the Pulse software.
        /// </summary>
        public string PrjFolderPath { get; set; }

        /// <summary>
        /// Font size for the Text display.
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// Maximum file size when saving the binary data
        /// to file.  This value is stored in bytes.
        /// </summary>
        public long MaxFileSize { get; set; }

        /// <summary>
        /// Adcp Communication type.
        /// </summary>
        public AdcpConnection.AdcpCommTypes AdcpCommType { get; set; }

        /// <summary>
        /// Last serial port options for the ADCP.
        /// This is used to set the previous settings when starting the application
        /// or creating a new project.
        /// </summary>
        public SerialOptions AdcpSerialOptions { get; set; }

        /// <summary>
        /// Flag if the the previous settings had the GPS 1 serial
        /// port enabled.
        /// </summary>
        public bool IsGps1SerialEnabled { get; set; }

        /// <summary>
        /// Flag if the the previous settings had the GPS 2 serial
        /// port enabled.
        /// </summary>
        public bool IsGps2SerialEnabled { get; set; }

        /// <summary>
        /// Flag if the the previous settings had the NMEA 1 serial
        /// port enabled.
        /// </summary>
        public bool IsNmea1SerialEnabled { get; set; }

        /// <summary>
        /// Flag if the the previous settings had the NMEA 2 serial
        /// port enabled.
        /// </summary>
        public bool IsNmea2SerialEnabled { get; set; }

        /// <summary>
        /// Last serial port options for the GPS 1.
        /// This is used to set the previous settings when starting the application
        /// or creating a new project.
        /// </summary>
        public SerialOptions Gps1SerialOptions { get; set; }

        /// <summary>
        /// Last serial port options for the GPS 2.
        /// This is used to set the previous settings when starting the application
        /// or creating a new project.
        /// </summary>
        public SerialOptions Gps2SerialOptions { get; set; }

        /// <summary>
        /// Last serial port options for the NMEA 1.
        /// This is used to set the previous settings when starting the application
        /// or creating a new project.
        /// </summary>
        public SerialOptions Nmea1SerialOptions { get; set; }

        /// <summary>
        /// Last serial port options for the NMEA 2.
        /// This is used to set the previous settings when starting the application
        /// or creating a new project.
        /// </summary>
        public SerialOptions Nmea2SerialOptions { get; set; }

        /// <summary>
        /// Last ethernet port options for the ADCP.
        /// This is used to set the previous settings when starting the application
        /// or creating a new project.
        /// </summary>
        public AdcpEthernetOptions EthernetOptions { get; set; }

        /// <summary>
        /// ADCP Prediction Model User Input.
        /// </summary>
        public PredictionModelInput PredictorUserInput { get; set; }

        /// <summary>
        /// The selected project ID.  This will store the
        /// last selected project's ID.
        /// </summary>
        public int SelectedProjectID { get; set; }

        /// <summary>
        /// Validation View Options.
        /// </summary>
        public ValidationTestViewOptions ValidationViewOptions { get; set; }

        /// <summary>
        /// Graphical View Options.
        /// </summary>
        public ViewDataGraphicalOptions GraphicalViewOptions { get; set; }

        /// <summary>
        /// Backscatter View Options.
        /// </summary>
        public BackscatterOptions BackscatterOptions { get; set; }

        /// <summary>
        /// Average View options.
        /// </summary>
        public AverageOptions AverageOptions { get; set; }

        /// <summary>
        /// Tank Test View options.
        /// </summary>
        public TankTestOptions TankTestOptions { get; set; }

        /// <summary>
        /// View Data Waves Options.
        /// </summary>
        public ViewDataWavesOptions ViewDataWavesOptions { get; set; }

        /// <summary>
        /// Last ADCP configuration.
        /// </summary>
        public AdcpConfiguration AdcpConfig { get; set; }

        /// <summary>
        /// Last Viewed page.
        /// </summary>
        public ViewNavEvent.ViewId LastViewedPage { get; set; }

        /// <summary>
        /// Recover Data View options.s
        /// </summary>
        public RecoverDataOptions RecoverDataOptions { get; set; }

        #endregion

        /// <summary>
        /// Default constructor.
        /// 
        /// The user must set all the values.
        /// </summary>
        public PulseOptions()
        {
            // Set the default values
            SetDefaults();
        }

        /// <summary>
        /// Set the Pulse options.
        /// </summary>
        /// <param name="prjFolderPath">Project folder path.</param>
        /// <param name="fontSize">Font Size.</param>
        /// <param name="maxFileSize">Max file size.</param>
        /// <param name="adcpCommType">ADCP Communication type.</param>
        /// <param name="adcpOptions">Adcp Serial Options.</param>
        /// <param name="isGps1Enabled">Flag if GPS 1 serial port is enabled.</param>
        /// <param name="isGps2Enabled">Flag if GPS 2 serial port is enabled.</param>
        /// <param name="isNmea1Enabled">Flag if NMEA 1 serial port is enabled.</param>
        /// <param name="isNmea2Enabled">Flag if NMEA 2 serial port is enabled.</param>
        /// <param name="gps1Options">Gps 1 Serial options.</param>
        /// <param name="gps2Options">Gps 2 Serial options.</param>
        /// <param name="nmea1Options">NMEA 1 Serial options.</param>
        /// <param name="nmea2Options">NMEA 2 Serial options.</param>
        /// <param name="ethernetOption">Ethernet options.</param>
        /// <param name="predictorUserInput">ADCP Predictor User Input.</param>
        /// <param name="selectedProjectID">Selected Project ID.</param>
        /// <param name="validationViewOptions">Validation View Options.</param>
        /// <param name="graphicalViewOptions">Graphical View Options.</param>
        /// <param name="backscatterOptions">Backscatter options.</param>
        /// <param name="averageOptions">Average Options.</param>
        /// <param name="tankTestOptions">Tank Test Options.</param>
        /// <param name="viewDataWavesOptions">Waves options.</param>
        /// <param name="adcpConfig">ADCP Config.</param>
        /// <param name="recoverDataOptions">Recover Data Options.</param>
        /// <param name="lastViewedPage">Last paged view.</param>
        public PulseOptions(string prjFolderPath, int fontSize, int maxFileSize, AdcpConnection.AdcpCommTypes adcpCommType, SerialOptions adcpOptions,
                                bool isGps1Enabled, bool isGps2Enabled, bool isNmea1Enabled, bool isNmea2Enabled, 
                                SerialOptions gps1Options, SerialOptions gps2Options, SerialOptions nmea1Options, SerialOptions nmea2Options, 
                                AdcpEthernetOptions ethernetOption, 
                                PredictionModelInput predictorUserInput, 
                                int selectedProjectID, ValidationTestViewOptions validationViewOptions, ViewDataGraphicalOptions graphicalViewOptions,
                                BackscatterOptions backscatterOptions, AverageOptions averageOptions, TankTestOptions tankTestOptions, ViewDataWavesOptions viewDataWavesOptions,
                                AdcpConfiguration adcpConfig, RecoverDataOptions recoverDataOptions,
                                ViewNavEvent.ViewId lastViewedPage)
        {
            PrjFolderPath = prjFolderPath;
            FontSize = fontSize;
            MaxFileSize = maxFileSize;
            AdcpCommType = adcpCommType;
            AdcpSerialOptions = adcpOptions;
            IsGps1SerialEnabled = isGps1Enabled;
            IsGps2SerialEnabled = isGps2Enabled;
            IsNmea1SerialEnabled = isNmea1Enabled;
            IsNmea2SerialEnabled = isNmea2Enabled;
            Gps1SerialOptions = gps1Options;
            Gps2SerialOptions = gps2Options;
            Nmea1SerialOptions = nmea1Options;
            Nmea2SerialOptions = nmea2Options;
            EthernetOptions = ethernetOption;
            PredictorUserInput = predictorUserInput;
            SelectedProjectID = selectedProjectID;
            ValidationViewOptions = validationViewOptions;
            GraphicalViewOptions = graphicalViewOptions;
            BackscatterOptions = backscatterOptions;
            AverageOptions = averageOptions;
            TankTestOptions = tankTestOptions;
            ViewDataWavesOptions = viewDataWavesOptions;
            AdcpConfig = adcpConfig;
            RecoverDataOptions = recoverDataOptions;
            LastViewedPage = lastViewedPage;

        }

        /// <summary>
        /// Set the default values for all the options.
        /// 
        /// Try to use the last ADCP/GPS serial option.  If the
        /// last option was never set, then the default settings will
        /// still be used for the serial options.
        /// </summary>
        private void SetDefaults()
        {
            Revision = ProjectManagerDatabaseWriter.PULSE_TABLE_REVISION;
            PrjFolderPath = Pulse.Commons.GetProjectDefaultFolderPath();
            FontSize = DEFAULT_FONT_SIZE;
            MaxFileSize = 1048576 * 50; // 50 MegaBytes
            AdcpCommType = AdcpConnection.AdcpCommTypes.Serial;
            AdcpSerialOptions = new SerialOptions();
            Gps1SerialOptions = new SerialOptions();
            Gps2SerialOptions = new SerialOptions();
            Nmea1SerialOptions = new SerialOptions();
            Nmea2SerialOptions = new SerialOptions();
            IsGps1SerialEnabled = false;
            IsGps2SerialEnabled = false;
            IsNmea1SerialEnabled = false;
            IsNmea2SerialEnabled = false;
            EthernetOptions = new AdcpEthernetOptions();
            PredictorUserInput = new PredictionModelInput();
            SelectedProjectID = 0;
            ValidationViewOptions = new ValidationTestViewOptions();
            GraphicalViewOptions = new ViewDataGraphicalOptions();
            BackscatterOptions = new BackscatterOptions();
            AverageOptions = new AverageOptions();
            TankTestOptions = new TankTestOptions();
            ViewDataWavesOptions = new ViewDataWavesOptions();
            AdcpConfig = new AdcpConfiguration();
            RecoverDataOptions = new RecoverDataOptions();
            LastViewedPage = ViewNavEvent.ViewId.HomeView;
        }

    }
}
