/*
 * Copyright © 2013 
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
 * 04/23/2013      RC          3.0.0      Initial coding
 * 05/06/2013      RC          3.0.0      Added Param property. 
 * 05/13/2013      RC          3.0.0      Added Back View ID.
 * 07/03/2013      RC          3.0.2      Added Validation Test view.
 * 07/11/2013      RC          3.0.4      Added Compass Cal view.
 * 07/23/2013      RC          3.0.4      Added ScreenDataView.
 * 07/29/2013      RC          3.0.6      Added DownloadDataView.
 * 07/30/2013      RC          3.0.6      Added UpdateFirmwareView.
 * 07/31/2013      RC          3.0.6      Added CompassUtilityView.
 * 08/23/2013      RC          3.0.8      Added AboutView.
 * 08/26/2013      RC          3.0.8      Added ExportDataView.
 * 10/03/2013      RC          3.2.0      Added ModeView, ProjectView, NewProjectView and LoadProjectView.
 * 10/15/2013      RC          3.2.0      Added LoadProjectsView.
 * 10/29/2013      RC          3.2.0      Added AdcpConfigurationView.
 * 10/31/2013      RC          3.2.0      Added AvergingView and BottomTrackOnView. 
 * 11/01/2013      RC          3.2.0      Added BinsView.
 * 12/05/2013      RC          3.2.0      Added BurstModeView.
 * 12/20/2013      RC          3.2.1      Added AdcpPredicitionModelView.
 * 09/03/2014      RC          4.0.3      Added SelectPlaybackView.
 * 09/17/2014      RC          4.1.0      Added DvlSetupView.
 * 10/02/2014      RC          4.1.0      Added RtiCompassCal.
 * 06/19/2015      RC          4.1.3      Added TestTankView.
 * 01/15/2015      RC          4.4.2      Added BurnInTestView.
 * 08/28/2017      RC          4.5.2      Added DataOutputView.
 * 01/25/2018      RC          4.7.2      Added ViewDataGraphicalView and ViewDataTextView and ViewData3dView.
 * 
 */

namespace RTI
{
    /// <summary>
    /// Event for the page navigation.  This will
    /// pass an ID for which page to load and 
    /// any parameters for the page.
    /// </summary>
    public class ViewNavEvent
    {
        /// <summary>
        /// Ids for all the views available
        /// to navigate to.
        /// </summary>
        public enum ViewId
        {
            /// <summary>
            /// Page Event for back.
            /// This is used to move back in the 
            /// application.
            /// </summary>
            Back,

            /// <summary>
            /// Home View.
            /// </summary>
            HomeView,

            /// <summary>
            /// Smart View to configure the ADCP
            /// </summary>
            SmartPageView,

            /// <summary>
            /// Terminal View to communicate with the ADCP
            /// </summary>
            TerminalView,

            /// <summary>
            /// Settings view for the Pulse application.
            /// </summary>
            SettingsView,

            /// <summary>
            /// View Data to view the incoming data from the ADCP.
            /// </summary>
            ViewDataView,

            /// <summary>
            /// Validation Test view.
            /// </summary>
            ValidationTestView,

            /// <summary>
            /// Compass Cal view.
            /// </summary>
            CompassCalView,

            /// <summary>
            /// Screen Data view.
            /// </summary>
            ScreenDataView,

            /// <summary>
            /// Download Data view.
            /// </summary>
            DownloadDataView,

            /// <summary>
            /// Update Firmware view.
            /// </summary>
            UpdateFirmwareView,

            /// <summary>
            /// Compass Utility view.
            /// </summary>
            CompassUtilityView,

            /// <summary>
            /// About View.
            /// </summary>
            AboutView,

            /// <summary>
            /// Export Data View.
            /// </summary>
            ExportDataView,

            /// <summary>
            /// Mode View.
            /// </summary>
            ModeView,

            /// <summary>
            /// Project View.
            /// </summary>
            ProjectView,

            /// <summary>
            /// New Project View.
            /// </summary>
            NewProjectView,

            /// <summary>
            /// Communications View.
            /// </summary>
            CommunicationsView,

            /// <summary>
            /// Storage View.
            /// </summary>
            StorageView,

            /// <summary>
            /// Load projects view.
            /// </summary>
            LoadProjectsView,

            /// <summary>
            /// Adcp Configuration view.
            /// </summary>
            AdcpConfigurationView,

            /// <summary>
            /// Averaging Options view.
            /// </summary>
            AveragingView,

            /// <summary>
            /// Bottom Track On view.
            /// </summary>
            BottomTrackOnView,

            /// <summary>
            /// Bins view.
            /// </summary>
            BinsView,

            /// <summary>
            /// Ping Timing View.
            /// </summary>
            PingTimingView,

            /// <summary>
            /// Frequency View.
            /// </summary>
            FrequencyView,

            /// <summary>
            /// Broadband View.
            /// </summary>
            BroadbandModeView,

            /// <summary>
            /// Navigation Sources View.
            /// </summary>
            NavSourcesView,

            /// <summary>
            /// Save ADCP Configuration View.
            /// </summary>
            SaveAdcpConfigurationView,

            /// <summary>
            /// Set the Salinity view.
            /// </summary>
            SalinityView,

            /// <summary>
            /// Time view.
            /// </summary>
            TimeView,

            /// <summary>
            /// Ensemble Interval view.
            /// </summary>
            EnsembleIntervalView,

            /// <summary>
            /// Simple Compass Cal view.
            /// </summary>
            SimpleCompassCalView,

            /// <summary>
            /// Simple Compass Cal Wizard view.
            /// </summary>
            SimpleCompassCalWizardView,

            /// <summary>
            /// Zero the pressure sensor view.
            /// </summary>
            ZeroPressureSensorView,

            /// <summary>
            /// Deploy the ADCP view.
            /// </summary>
            DeployAdcpView,

            /// <summary>
            /// Scan the ADCP.
            /// </summary>
            ScanAdcpView,

            /// <summary>
            /// ADCP Utilities view.
            /// </summary>
            AdcpUtilitiesView,

            /// <summary>
            /// Burst Mode View.
            /// </summary>
            BurstModeView,

            /// <summary>
            /// ADCP Predicition View.
            /// </summary>
            AdcpPredictionModelView,

            /// <summary>
            /// ADCP Predicition View.
            /// </summary>
            VesselMountOptionsView,

            /// <summary>
            /// Select Playback.
            /// </summary>
            SelectPlaybackView,

            /// <summary>
            /// DVL Setup.
            /// </summary>
            DvlSetupView,

            /// <summary>
            /// RTI Compass Cal
            /// </summary>
            RtiCompassCalView,

            /// <summary>
            /// Waves View.
            /// </summary>
            WavesView,

            /// <summary>
            /// Water Test View.
            /// </summary>
            WaterTestView,

            /// <summary>
            /// Variance Calculation View.
            /// </summary>
            VarianceCalcView,

            /// <summary>
            /// Backscatter View.
            /// </summary>
            BackscatterView,

            /// <summary>
            /// Final Check View.
            /// </summary>
            FinalCheckView,

            /// <summary>
            /// Test Tank View.
            /// </summary>
            TankTestView,

            /// <summary>
            /// Board Serial Number.
            /// </summary>
            BoardSerialNumber,

            /// <summary>
            /// Diagnostic View.
            /// </summary>
            DiagnosticView,

            /// <summary>
            /// Waves Setup.
            /// </summary>
            WavesSetupView,

            /// <summary>
            /// Burn In Test View.
            /// </summary>
            BurnInTestView,

            /// <summary>
            /// Processor Board Test View.
            /// </summary>
            ProcessorBoardTestView,

            /// <summary>
            /// Data Output View.
            /// </summary>
            DataOutputView,

            /// <summary>
            /// Tabular Data Output View.
            /// </summary>
            ViewDataTextView,

            /// <summary>
            /// Graphical Data Output View.
            /// </summary>
            ViewDataGraphicalView,

            /// <summary>
            /// 3D Output View.
            /// </summary>
            ViewData3dView
        }

        #region Properties

        /// <summary>
        /// ID for the page.
        /// </summary>
        public ViewId ID { get; protected set; }

        /// <summary>
        /// A parameter for the page.
        /// </summary>
        public object Param { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the ID.
        /// </summary>
        /// <param name="id">View ID.</param>
        public ViewNavEvent(ViewId id)
        {
            ID = id;
        }

        /// <summary>
        /// Initialize the object with the ID
        /// and the parameter.
        /// </summary>
        /// <param name="id">View ID.</param>
        /// <param name="param">Parameter for the view.</param>
        public ViewNavEvent(ViewId id, object param)
        {
            ID = id;
            Param = param;
        }

    }

}
