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
 * 07/25/2013      RC          3.0.4      Initial coding
 * 09/12/2013      RC          3.1.0      Added SelectedValidationTestVM to know which view to display.
 * 09/18/2013      RC          3.1.3      Added San Diego declination (CHO) to On_ConfigureAdcpSalt() and On_ConfigureAdcpFresh() command sets.
 * 04/15/2014      RC          3.2.4      In On_ConfigureAdcpFresh() and On_ConfigureAdcpSalt() removed setting CHO.
 * 05/07/2014      RC          3.2.4      In On_ConfigureAdcpSalt(), set the CHS (heading source) to use the GPS.
 * 06/10/2014      RC          3.3.0      Added a warning if not recording live data.
 * 06/13/2014      RC          3.3.1      Added a button to import commands and added a flag to say if tank testing to store results in a different folder.
 * 06/23/2014      RC          3.3.1      Removed using the dispatcher in the EnsembleEvent eventhandler.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/20/2014      RC          4.0.1      Changed the list of VM to match ViewDataGraphicalViewModel.
 * 08/21/2014      RC          4.0.1      Clear all the VM properly when changing projects.  On activate, make sure the VM will display.
 * 08/22/2014      RC          4.0.2      Moved DEFAULT_RECORD_DIR_TANK and DEFAULT_RECORD_DIR to Pulse.Common.
 * 02/18/2015      RC          4.1.0      Check for a configuration file for fresh and salt or use default commands.
 * 10/07/2015      RC          4.3.0      Changed dictionary to ConcurrentDicitionary.
 * 12/03/2015      RC          4.4.0      Added timer to monitor recording button.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using ReactiveUI;
    using System.Windows;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    /// <summary>
    /// Base viewmodel for the Validation test.  This will create a VM for each
    /// subsystem configuration.
    /// </summary>
    public class ValidationTestBaseViewModel : PulseViewModel, IHandle<EnsembleEvent>, IHandle<ProjectEvent>, IHandle<CloseVmEvent>, IHandle<BulkEnsembleEvent>
    {
        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Project manager.
        /// </summary>
        private PulseManager _pm;

        /// <summary>
        /// EventsAggregator.
        /// </summary>
        private IEventAggregator _events;

        /// <summary>
        /// Connection to the ADCP.
        /// </summary>
        private AdcpConnection _adcpConn;

        /// <summary>
        /// Frequency string value for 1200 kHz.
        /// </summary>
        private const string FREQ_1200 = "1200 kHz";

        /// <summary>
        /// Frequency string value for 600 kHz.
        /// </summary>
        private const string FREQ_600 = "600 kHz";

        /// <summary>
        /// Frequency string value for 300 kHz.
        /// </summary>
        private const string FREQ_300 = "300 kHz";

        /// <summary>
        /// Timer to display a warning about recording.
        /// </summary>
        private System.Timers.Timer _warningRecordTimer;

        #endregion

        #region Properties

        #region Configurations

        /// <summary>
        /// This dictionary is to used to quickly search if the VM
        /// has already been created based off the SubsystemConfiguration
        /// used.  This must be kept in-sync with the ValidationDataVMList;
        /// </summary>
        private ConcurrentDictionary<SubsystemDataConfig, ValidationTestViewModel> _validationTestVMDict;

        /// <summary>
        /// List of all the Validation Test ViewModels.
        /// </summary>
        public List<ValidationTestViewModel> ValidationTestVMList
        {
            get
            {
                return _validationTestVMDict.Values.ToList();
            }
        }


        /// <summary>
        /// The selected Validation Test ViewModel to display.
        /// </summary>
        private ValidationTestViewModel _SelectedValidationTestVM;
        /// <summary>
        /// The selected Validation Test ViewModel to display.
        /// </summary>
        public ValidationTestViewModel SelectedValidationTestVM
        {
            get { return _SelectedValidationTestVM; }
            set
            {
                _SelectedValidationTestVM = value;
                this.NotifyOfPropertyChange(() => this.SelectedValidationTestVM);
            }
        }

        #endregion

        #region Test Flag

        /// <summary>
        /// Flag True when testing is in progress.
        /// This start to calculate the DMG.
        /// </summary>
        private bool _isTesting;
        /// <summary>
        /// Flag True when testing is in progress.
        /// This start to calculate the DMG.
        /// </summary>
        public bool IsTesting
        {
            get { return _isTesting; }
            set
            {
                _isTesting = value;
                this.NotifyOfPropertyChange(() => this.IsTesting);

                // Pass the flag to all the VM
                foreach (var vm in ValidationTestVMList)
                {
                    vm.IsTesting = value;
                }

            }
        }

        #endregion

        #region Admin Buttons

        /// <summary>
        /// A flag to know if the user is an admin.
        /// </summary>
        private bool _isAdmin;
        /// <summary>
        /// A flag to know if the user is an admin.
        /// </summary>
        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                this.NotifyOfPropertyChange(() => this.IsAdmin);
            }
        }

        /// <summary>
        /// String for the selected frequency.
        /// </summary>
        private string _selectedFrequency;
        /// <summary>
        /// String for the selected frequency.
        /// </summary>
        public string SelectedFrequency
        {
            get { return _selectedFrequency; }
            set
            {
                _selectedFrequency = value;
                this.NotifyOfPropertyChange(() => this.SelectedFrequency);
            }
        }

        /// <summary>
        /// A list of all the frequency options.
        /// </summary>
        public ObservableCollection<string> FrequencyOptions { get; private set; }

        #endregion

        #region Result Directory

        /// <summary>
        /// Directory to store the Validation test files to.
        /// </summary>
        private string _ValidationTestDirectory;
        /// <summary>
        /// Directory to store the Validation test files to.
        /// </summary>
        public string ValidationTestDirectory
        {
            get { return _ValidationTestDirectory; }
            set
            {
                _ValidationTestDirectory = value;
                this.NotifyOfPropertyChange(() => this.ValidationTestDirectory);

                CreateTestResultDirectory(_ValidationTestDirectory);
            }
        }

        #endregion

        #region Recording Warning

        /// <summary>
        /// A warning if data is not recording but is being received.
        /// </summary>
        private bool _IsDisplayRecordingWarning;
        /// <summary>
        /// A warning if data is not recording but is being received.
        /// </summary>
        public bool IsDisplayRecordingWarning
        {
            get { return _IsDisplayRecordingWarning; }
            set
            {
                _IsDisplayRecordingWarning = value;
                this.NotifyOfPropertyChange(() => this.IsDisplayRecordingWarning);
            }
        }

        #endregion

        #region Tank Test Warning

        /// <summary>
        /// Flag if the testing is the tank test.  This will recorded file name.
        /// </summary>
        private bool _IsTankTesting;
        /// <summary>
        /// Flag if the testing is the tank test.  This will recorded file name.
        /// </summary>
        public bool IsTankTesting
        {
            get { return _IsTankTesting; }
            set
            {
                _IsTankTesting = value;
                this.NotifyOfPropertyChange(() => this.IsTankTesting);

                // Set the correct directory
                if (IsTankTesting)
                {
                    ValidationTestDirectory = Pulse.Commons.DEFAULT_RECORD_DIR_TANK;
                }
                else
                {
                    ValidationTestDirectory = Pulse.Commons.DEFAULT_RECORD_DIR;
                }
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to start the testing.
        /// This will begin the recording process.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StartTestingCommand { get; protected set; }

        /// <summary>
        /// Command to stop the testing.
        /// This will stop the recording process.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StopTestingCommand { get; protected set; }

        /// <summary>
        /// Command to configure the ADCP for fresh water.
        /// This will set the salinity and the baud rate for 232b.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ConfigureAdcpFreshCommand { get; protected set; }

        /// <summary>
        /// Command to configure the ADCP for salt water.
        /// This will set the salinity and the baud rate for 232b.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ConfigureAdcpSaltCommand { get; protected set; }

        /// <summary>
        /// Command to configure the ADCP tank test.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ConfigureAdcpTankCommand { get; protected set; }

        /// <summary>
        /// Command to configure the ADCP tank ringing test.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ConfigureAdcpRingingTankCommand { get; protected set; }

        /// <summary>
        /// Command to start pinging the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StartPingingCommand { get; protected set; }

        /// <summary>
        /// Command to stop pinging the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StopPingingCommand { get; protected set; }

        /// <summary>
        /// Command to import ADCP commands to the ADCP.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ImportAdcpScriptCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Create a base view for all the ScreenData Views.
        /// </summary>
        public ValidationTestBaseViewModel()
            : base("ValidationTestBaseViewModel")
        {
            // Project Manager
            _pm = IoC.Get<PulseManager>();
            _events = IoC.Get<IEventAggregator>();
            _events.Subscribe(this);
            _adcpConn = IoC.Get<AdcpConnection>();

            // Set if the user is an admin
            IsAdmin = Pulse.Commons.IsAdmin();
            //LakeTestDirectory = RTI.Pulse.Commons.GetProjectDefaultFolderPath() + @"\LakeTest";
            ValidationTestDirectory = Pulse.Commons.DEFAULT_RECORD_DIR;

            // Initialize the list
            _validationTestVMDict = new ConcurrentDictionary<SubsystemDataConfig, ValidationTestViewModel>();

            IsTesting = _adcpConn.IsValidationTestRecording;
            IsTankTesting = false;

            // Setup Admin values
            SetupAdminValues();

            // Initialize for warning when not recording live data
            IsDisplayRecordingWarning = false;

            // Warning timer
            _warningRecordTimer = new System.Timers.Timer();
            _warningRecordTimer.Interval = 5000;                // 5 seconds.
            _warningRecordTimer.Elapsed += _warningRecordTimer_Elapsed;
            _warningRecordTimer.AutoReset = true;

            // Set the Clock time to Local System time on the ADCP
            StartTestingCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsTesting, x => !x.Value),
                                                                    _ => Task.Run(() => On_StartTesting()));

            // Create a command to stop testing
            StopTestingCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(x => x.IsTesting, x => x.Value),
                                                                _ => Task.Run(() => On_StopTesting()));

            // Create a command to configure the ADCP for Fresh Water Lake Test
            ConfigureAdcpFreshCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_ConfigureAdcpFresh()));

            // Create a command to configure the ADCP for Salt Water Ocean Test
            ConfigureAdcpSaltCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_ConfigureAdcpSalt()));

            // Create a command to configure the ADCP for Tank Test
            ConfigureAdcpTankCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_ConfigureAdcpTank()));

            // Create a command to configure the ADCP for Ringing Test in tank
            ConfigureAdcpRingingTankCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => On_ConfigureAdcpRingingTank()));

            // Create a command to Start Pinging
            StartPingingCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => StartPingingCommandExec()));

            // Create a command to Stop Pinging
            StopPingingCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => StopPingingCommandExec()));

            // Import commands
            ImportAdcpScriptCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConn,                                                // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                      // Ensure the connection exist
                                                                        _ => ImportAdcpScript());                                                   // Import the ADCP Script
                                                  

            // Create the ViewModels based off the AdcpConfiguration
            AddConfigurations();
        }

        /// <summary>
        /// Shutdown this object.
        /// </summary>
        public override void Dispose()
        {
            // Stop testing
            if (_isTesting)
            {
                RecordTestResults();
            }

            // Shutdown all the VMs created
            foreach (var vm in ValidationTestVMList)
            {
                vm.Dispose();
            }
        }

        /// <summary>
        /// Activate all the sub displayes when the display is activated.
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();

            // Check if record button was already pressed
            if (IsTesting != _adcpConn.IsValidationTestRecording)
            {
                IsTesting = _adcpConn.IsValidationTestRecording;
            }
            //this.NotifyOfPropertyChange(() => this.IsTesting);

            // If no configurations are loaded, get them
            if (ValidationTestVMList.Count == 0)
            {
                AddConfigurations();
            }

            foreach (var vm in ValidationTestVMList)
            {
                vm.ActivateVm(true);
            }
        }

        /// <summary>
        /// Deactivate all the sud displayes when the display is deactivated.
        /// </summary>
        /// <param name="close">Flag to close display.</param>
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            foreach (var vm in ValidationTestVMList)
            {
                vm.ActivateVm(false);
            }
        }

        #region Configuration

        /// <summary>
        /// Create all the ViewModels based off the Adcp Configuration in the selected project.
        /// </summary>
        private void AddConfigurations()
        {
            if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration != null)
            {
                // Create a viewmodel for every configuration
                foreach (var config in _pm.SelectedProject.Configuration.SubsystemConfigDict.Values)
                {
                    AddConfig(new SubsystemDataConfig(config.SubsystemConfig, EnsembleSource.Playback));
                }
            }
        }

        /// <summary>
        /// Add a configuration.  This will create the ViewModel based
        /// off the configuration given.
        /// </summary>
        /// <param name="config">Configuration to use to create the ViewModel.</param>
        private void AddConfig(SubsystemDataConfig config)
        {
            if (!_validationTestVMDict.ContainsKey(config))
            {
                if (_validationTestVMDict.TryAdd(config, new ValidationTestViewModel(config)))
                {
                    this.NotifyOfPropertyChange(() => this.ValidationTestVMList);

                    // Select a tab is nothing is selected
                    if (SelectedValidationTestVM == null)
                    {
                        if (ValidationTestVMList.Count > 0)
                        {
                            SelectedValidationTestVM = ValidationTestVMList[0];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear all the configurations.
        /// This is done when a new project is selected.
        /// </summary>
        private void ClearConfig()
        {
            _SelectedValidationTestVM = null;

            foreach (var vm in _validationTestVMDict.Values)
            {
                vm.Dispose();
            }

            _validationTestVMDict.Clear();
            this.NotifyOfPropertyChange(() => this.ValidationTestVMList);

        }

        #endregion

        #region Admin

        /// <summary>
        /// Setup all the admin values.  These
        /// are values used for the admin section.
        /// </summary>
        private void SetupAdminValues()
        {
            // Set the frequency list
            FrequencyOptions = new ObservableCollection<string>();
            FrequencyOptions.Add(FREQ_1200);
            FrequencyOptions.Add(FREQ_600);
            FrequencyOptions.Add(FREQ_300);

            SelectedFrequency = FrequencyOptions[0];
        }

        #endregion

        #region Test Results

        /// <summary>
        /// Create the Lake Test directory if it does not exist.
        /// </summary>
        /// <param name="dir">Directory to create.</param>
        private void CreateTestResultDirectory(string dir)
        {
            // Check if the Company folders exist
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// Record the test results for all the VM.
        /// </summary>
        private void RecordTestResults()
        {
            foreach (var vm in ValidationTestVMList)
            {
                // Write the test results to file
                // The lake test file is closed in the shutdown method or the recorder manager
                vm.RecordTestResults(_ValidationTestDirectory, _adcpConn.ValidationTestFileName);
            }
        }

        #endregion

        #region Recording Warning

        /// <summary>
        /// Display the warning that the user is not recording.  This is enabled based off the incoming ensemble data state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _warningRecordTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                IsDisplayRecordingWarning = true;
                System.Threading.Thread.Sleep(1000);
                IsDisplayRecordingWarning = false;
            });
        }

        #endregion

        #region Bulk Ensemble

        /// <summary>
        /// Display the bulk ensemble event async.
        /// </summary>
        private void BulkEnsembleDisplayExecute(BulkEnsembleEvent ensEvent)
        {

            // Find all the configurations
            for (int x = 0; x < ensEvent.Ensembles.Count(); x++)
            {
                // There can only be 12 configurations
                if(x > 12)
                {
                    break;
                }

                // Get the ensemble
                DataSet.Ensemble ens = ensEvent.Ensembles.IndexValue(x);

                // Verify the ensemble
                if (ens != null && ens.IsEnsembleAvail)
                {
                    // Create the config
                    var ssDataConfig = new SubsystemDataConfig(ens.EnsembleData.SubsystemConfig, ensEvent.Source);

                    // Check if the config exist in the table
                    if (!_validationTestVMDict.ContainsKey(ssDataConfig))
                    {
                        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => AddConfig(ssDataConfig)));
                    }

                    //Wait for the dispatcher to add the config
                    // Monitor for any timeouts
                    int timeout = 0;
                    while (!_validationTestVMDict.ContainsKey(ssDataConfig))
                    {
                        // Set a timeout and wait for the config
                        timeout++;
                        if (timeout > 10)
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(250);
                    }

                    //_events.PublishOnUIThread(new EnsembleEvent(ens, EnsembleSource.Playback));
                }
            }

            // Pass the ensembles to the displays
            foreach (var vm in _validationTestVMDict.Values)
            {
                vm.DisplayBulkData(ensEvent.Ensembles);
            }
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Handle event when EnsembleEvent is received.
        /// This will create the displays for each config
        /// if it has not been created already.  It will also
        /// display the latest ensemble.
        /// </summary>
        /// <param name="ensEvent">Ensemble event.</param>
        public void Handle(EnsembleEvent ensEvent)
        {
            if (ensEvent.Ensemble != null && ensEvent.Ensemble.IsEnsembleAvail)
            {
                // Create the config
                var ssDataConfig = new SubsystemDataConfig(ensEvent.Ensemble.EnsembleData.SubsystemConfig, ensEvent.Source);

                if (!_validationTestVMDict.ContainsKey(ssDataConfig))
                {
                    Application.Current.Dispatcher.BeginInvoke(new System.Action(() => AddConfig(ssDataConfig)));
                }
            }

            // Check if record button was already pressed
            if(IsTesting != _adcpConn.IsValidationTestRecording)
            {
                IsTesting = _adcpConn.IsValidationTestRecording;
            }

            // Update timer
            if (!_adcpConn.IsRecording &&                                           // Not recording
                !_adcpConn.IsValidationTestRecording &&                             // Not Validation test recording
                ensEvent.Source != EnsembleSource.Playback)                         // Not playing back data
            {
                _warningRecordTimer.Enabled = true;
            }
            else
            {
                _warningRecordTimer.Enabled = false;
            }
        }

        /// <summary>
        /// New project is selected.
        /// </summary>
        /// <param name="prjEvent">Project event.</param>
        public void Handle(ProjectEvent prjEvent)
        {
            // Clear all the configs
            ClearConfig();

            // Add known configuration
            AddConfigurations();
        }

        /// <summary>
        /// Remove the display based off the SubsystemDataConfig
        /// given in the event.
        /// </summary>
        /// <param name="closeVmEvent">Contains the SubsystemDataConfig to remove the display.</param>
        public void Handle(CloseVmEvent closeVmEvent)
        {
            // Check if the display exist
            if (_validationTestVMDict.ContainsKey(closeVmEvent.SubsysDataConfig))
            {
                // Dispose the display then remove the display
                _validationTestVMDict[closeVmEvent.SubsysDataConfig].Dispose();
                ValidationTestViewModel vm = null;
                if (_validationTestVMDict.TryRemove(closeVmEvent.SubsysDataConfig, out vm))
                {
                    this.NotifyOfPropertyChange(() => this.ValidationTestVMList);

                    // Select a tab is nothing is selected
                    if (SelectedValidationTestVM == null)
                    {
                        if (ValidationTestVMList.Count > 0)
                        {
                            SelectedValidationTestVM = ValidationTestVMList[0];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle event when BulkEnsembleEvent is received.
        /// This will create the displays for each config
        /// if it has not been created already.  It will also
        /// display the latest ensemble.
        /// </summary>
        /// <param name="ensEvent">Ensemble event.</param>
        public void Handle(BulkEnsembleEvent ensEvent)
        {
            // Execute async
            Task.Run(() => BulkEnsembleDisplayExecute(ensEvent));
        }

        #endregion

        #region Commands

        #region Start Stop Pinging

        /// <summary>
        /// Send the command to Start Pinging.
        /// </summary>
        /// <returns></returns>
        private void StartPingingCommandExec()
        {
            _adcpConn.AdcpSerialPort.StartPinging(true);
        }

        /// <summary>
        /// Send the command to Stop Pinging.
        /// </summary>
        /// <returns></returns>
        private void StopPingingCommandExec()
        {
            _adcpConn.AdcpSerialPort.StopPinging();
        }

        #endregion

        #region Start Testing

        /// <summary>
        /// Start Testing.
        /// </summary>
        private void On_StartTesting()
        {
            // Set flag to start recording
            IsTesting = true;

            // Clear the plots
            foreach (var vm in ValidationTestVMList)
            {
                vm.Clear();

                // Turn on calculating DMG
                vm.IsCalculateDmg = true;
            }

            // Start recording
            _adcpConn.StartValidationTest(_ValidationTestDirectory);
        }



        #endregion

        #region Stop Testing

        /// <summary>
        /// Stop Testing.
        /// Record the results.
        /// </summary>
        private void On_StopTesting()
        {
            // Set flag that we are recording
            IsTesting = false;

            // Stop recording
            _adcpConn.StopValidationTest();

            // Write the test results to file
            RecordTestResults();
        }

        #endregion

        #region Configure ADCP Fresh Command

        /// <summary>
        /// Configure the ADCP with the proper commands to do a fresh water lake test.
        /// CDEFAULT            Set default settings
        /// C232B 19200         Change GPS serial port to 19200
        /// CSAVE               Save all settings to SD card
        /// </summary>
        private void On_ConfigureAdcpFresh()
        {
            // Ensure the serial port is open
            if (_adcpConn.IsOpen())
            {
                if (File.Exists(RTI.Pulse.Commons.DEFAULT_CONFIG_DIR + @"\ConfigureFresh.txt"))
                {
                    string[] commands = File.ReadAllLines(RTI.Pulse.Commons.DEFAULT_CONFIG_DIR + @"\ConfigureFresh.txt");

                    _adcpConn.SendCommands(commands.ToList());
                }
                else
                {
                    // List of commands to send to the ADCP
                    List<string> commands = new List<string>();
                    commands.Add(RTI.Commands.AdcpCommands.CMD_STOP_PINGING);                                                           // Stop pinging
                    commands.Add(RTI.Commands.AdcpCommands.CMD_CDEFAULT);                                                               // Set default settings
                    commands.Add(RTI.Commands.AdcpCommands.CMD_C232B + " 19200");                                                       // Set GPS serial port to 19200 baud
                    //commands.Add(RTI.Commands.AdcpCommands.CMD_CHO + " " + Commands.AdcpCommands.DEFAULT_SAN_DIEGO_DECLINATION);        // Add in San Diego Declination
                    commands.Add(RTI.Commands.AdcpCommands.GetLocalSystemTimeCommand());                                                // Set Local Time
                    commands.Add(RTI.Commands.AdcpCommands.CMD_CWS + " 0");                                                             // Salinity set to 0
                    commands.Add(RTI.Commands.AdcpCommands.CMD_CSAVE);                                                                  // Save settings to the SD card
                    commands.Add(RTI.Commands.AdcpCommands.CMD_START_PINGING);                                                          // Start pinging

                    // Send these commands to the ADCP serial port 
                    _adcpConn.SendCommands(commands);
                }
            }
        }

        #endregion

        #region Configure ADCP Salt Command

        /// <summary>
        /// Configure the ADCP with the proper commands to do a salt water ocean test.
        /// CDEFAULT            Set default settings
        /// C232B 19200         Change GPS serial port to 19200
        /// CWS 35              Set the salinity to 35 ppm
        /// CSAVE               Save all settings to SD card
        /// </summary>
        private void On_ConfigureAdcpSalt()
        {
            // Ensure the serial port is open
            if (_adcpConn.IsOpen())
            {

                if (File.Exists(RTI.Pulse.Commons.DEFAULT_CONFIG_DIR + @"\ConfigureSalt.txt"))
                {
                    string[] commands = File.ReadAllLines(RTI.Pulse.Commons.DEFAULT_CONFIG_DIR + @"\ConfigureSalt.txt");

                    _adcpConn.SendCommands(commands.ToList());
                }
                else
                {
                    // List of commands to send to the ADCP
                    List<string> commands = new List<string>();
                    commands.Add(RTI.Commands.AdcpCommands.CMD_STOP_PINGING);                                                           // Stop pinging
                    commands.Add(RTI.Commands.AdcpCommands.CMD_CDEFAULT);                                                               // Set default settings
                    commands.Add(RTI.Commands.AdcpCommands.CMD_C232B + " 19200");                                                       // Set GPS serial port to 19200 baud
                    commands.Add(RTI.Commands.AdcpCommands.CMD_CWS + " 35");                                                            // Set salinity to 35 ppm
                    //commands.Add(RTI.Commands.AdcpCommands.CMD_CHS + " 2");                                                             // Set the Heading source to the GPS
                    //commands.Add(RTI.Commands.AdcpCommands.CMD_CHO + " " + Commands.AdcpCommands.DEFAULT_SAN_DIEGO_DECLINATION);        // Add in San Diego Declination
                    commands.Add(RTI.Commands.AdcpCommands.GetLocalSystemTimeCommand());                                                // Set Local Time
                    commands.Add(RTI.Commands.AdcpCommands.CMD_CSAVE);                                                                  // Save settings to the SD card
                    commands.Add(RTI.Commands.AdcpCommands.CMD_START_PINGING);                                                          // Start pinging

                    // Send these commands to the ADCP serial port 
                    _adcpConn.SendCommands(commands);
                }
            }
        }


        #endregion

        #region Configure ADCP Tank Command

        /// <summary>
        /// Configure the ADCP with the proper commands to do a tank test.
        /// CDEFAULT            Set default settings
        /// CEI 00:00:00.25     Set the Ensemble interval
        /// ENGTRGAINO
        /// CWPBB 0,0.042       Set the Water Profile broadband mode and lag
        /// CWPBL 0.01          Set the Water Profile blank to 0.01 meters
        /// CWPBS 0.05          Set the Water Profile bin size to 0.05 meters
        /// CWPX 0.20           Set the Water Profile transmit size to 0.20 meters
        /// CBTBB 4,2.000,30.00 Set the Bottom Track Broadband mode, lag and depth for 1200khz
        ///     or
        /// CBTBB 4,4.000,30.00 Set the Bottom Track Broadband mode, lag and depth for 600khz
        ///     or
        /// CBTBB 4,8.000,30.00 Set the Bottom Track Broadband mode, lag and depth for 300khz
        /// CSAVE               Save all settings to SD card
        /// </summary>
        private void On_ConfigureAdcpTank()
        {
            // Ensure the serial port is open
            if (_adcpConn.AdcpSerialPort.IsOpen())
            {
                // List of commands to send to the ADCP
                List<string> commands = new List<string>();
                commands.Add(RTI.Commands.AdcpCommands.CMD_STOP_PINGING);                   // Stop pinging
                commands.Add(RTI.Commands.AdcpCommands.CMD_CDEFAULT);                       // Set default settings
                commands.Add(RTI.Commands.AdcpCommands.CMD_CEI + " 00:00:00.25");           // Set the ensemble interval
                commands.Add("ENGTRAGAINO");
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBB + " 0,0.042");    // Set the Water Profile Broadband mode and lag
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBL + "0.01");        // Set the Water Profile blank
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBS + "0.05");        // Set the Water Profile bin size
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPX + "0.20");         // Set the Water Profile transmit size

                if (SelectedFrequency == FREQ_1200)
                {
                    commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CBTBB + "4,2.00,30.00");    // Set the Bottom Track Broadband mode, lag and depth for 1200 kHz
                }
                else if (SelectedFrequency == FREQ_600)
                {
                    commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CBTBB + "4,4.00,30.00");    // Set the Bottom Track Broadband mode, lag and depth for 600 kHz
                }
                else if (SelectedFrequency == FREQ_300)
                {
                    commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CBTBB + "4,8.00,30.00");    // Set the Bottom Track Broadband mode, lag and depth for 300 kHz
                }

                commands.Add(RTI.Commands.AdcpCommands.GetLocalSystemTimeCommand());// Set Local Time
                commands.Add(RTI.Commands.AdcpCommands.CMD_CSAVE);                          // Save settings to the SD card
                commands.Add(RTI.Commands.AdcpCommands.CMD_START_PINGING);                  // Start pinging

                // Publish the event to send these commands to the ADCP serial port 
                _adcpConn.AdcpSerialPort.SendCommands(commands);
            }
        }

        #endregion

        #region Configure ADCP Ringing Tank Command

        /// <summary>
        /// Configure the ADCP with the proper commands to do a tank test.
        /// CDEFAULT            Set default settings
        /// ENGTRGAIN1
        /// CWPBB 1,0.042       Set the Water Profile broadband mode and lag
        /// CWPBP 100,0.1       Set the Water Profile Base Ping: number of pings and time between pings
        /// CWPBL 0.01          Set the Water Profile blank to 0.01 meters
        /// CWPBS 0.02          Set the Water Profile bin size for 1200 kHz
        ///     or
        /// CWPBS 0.04          Set the Water Profile bin size for 600 kHz
        ///     or
        /// CWPBS 0.08          Set the Water Profile bin size for 300 kHz
        /// CBTON 0             Set Bottom Track OFF
        /// CSAVE               Save all settings to SD card
        /// </summary>
        private void On_ConfigureAdcpRingingTank()
        {
            // Ensure the serial port is open
            if (_adcpConn.AdcpSerialPort.IsOpen())
            {
                // List of commands to send to the ADCP
                List<string> commands = new List<string>();
                commands.Add(RTI.Commands.AdcpCommands.CMD_STOP_PINGING);                   // Stop pinging
                commands.Add(RTI.Commands.AdcpCommands.CMD_CDEFAULT);                       // Set default settings
                commands.Add("ENGTRAGAIN1");
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBB + " 1,0.042");    // Set the Water Profile Broadband mode and lag
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBP + "100,0.1");     // Set the Water Profile Base Ping
                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBL + "0.01");        // Set the Water Profile Blank

                if (SelectedFrequency == FREQ_1200)
                {
                    commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBS + "0.02");    // Set the Water Profile bin size for 1200 kHz
                }
                else if (SelectedFrequency == FREQ_600)
                {
                    commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBS + "0.04");    // Set the Water Profile bin size for 600 kHz
                }
                else if (SelectedFrequency == FREQ_300)
                {
                    commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CWPBS + "0.08");    // Set the Water Profile bin size for 300 kHz
                }

                commands.Add(RTI.Commands.AdcpSubsystemCommands.CMD_CBTON + "0");           // Set Bottom Track Off

                commands.Add(RTI.Commands.AdcpCommands.GetLocalSystemTimeCommand());// Set Local Time
                commands.Add(RTI.Commands.AdcpCommands.CMD_CSAVE);                          // Save settings to the SD card
                commands.Add(RTI.Commands.AdcpCommands.CMD_START_PINGING);                  // Start pinging

                // Publish the event to send these commands to the ADCP serial port 
                _adcpConn.AdcpSerialPort.SendCommands(commands);
            }
        }

        #endregion

        #region Import ADCP Script

        /// <summary>
        /// Import an ADCP script file and send it to the ADCP.
        /// </summary>
        public async Task ImportAdcpScript()
        {
            string fileName = "";
            try
            {
                // Show the FolderBrowserDialog.
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "All files (*.*)|*.*";
                dialog.Multiselect = false;

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Get the files selected
                    fileName = dialog.FileName;

                    // Set the command set
                    string adcpCommandSet = File.ReadAllText(fileName);

                    // Run the command async
                    await Task.Run(() => SendCommandSetToAdcp(adcpCommandSet));
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error reading command set from {0}", fileName), e);
            }
        }

        /// <summary>
        /// Send the given command set to the ADCP.
        /// </summary>
        /// <param name="cmdSet">String of commands.</param>
        private void SendCommandSetToAdcp(object cmdSet)
        {
            // Conver the object to a string
            string commandSet = (string)cmdSet;

            // Verify there are any commands
            if (!string.IsNullOrEmpty(commandSet))
            {
                string[] result = commandSet.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Remove all line feed, carrage returns, new lines and tabs
                for (int x = 0; x < result.Length; x++)
                {
                    result[x] = result[x].Replace("\n", String.Empty);
                    result[x] = result[x].Replace("\r", String.Empty);
                    result[x] = result[x].Replace("\t", String.Empty);
                }

                _adcpConn.SendCommands(result.ToList());
            }
        }

        #endregion

        #endregion

    }
}
