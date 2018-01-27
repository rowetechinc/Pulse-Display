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
 * 10/26/2015      RC          4.3.1       Initial coding
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
    using System.Threading.Tasks;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Collections.Concurrent;

    /// <summary>
    /// Allow the user to test out settings in the prediction model.
    /// </summary>
    public class DiagnosticsBaseViewModel : PulseViewModel, IHandle<EnsembleEvent>, IHandle<ProjectEvent>, IHandle<CloseVmEvent>
    {
        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// EventsAggregator.
        /// </summary>
        private IEventAggregator _events;

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        /// <summary>
        /// Singleton object to communication with the ADCP.
        /// </summary>
        private AdcpConnection _adcpConn;

        ///// <summary>
        ///// Timer to reduce the number of update calls the terminal window.
        ///// </summary>
        ////private System.Timers.Timer _displayTimer;

        #endregion

        #region Properties

        #region ADCPs

        /// <summary>
        /// Adcp List.
        /// </summary>
        public class Adcps
        {
            /// <summary>
            /// Singleton object to communication with the ADCP.
            /// </summary>
            private AdcpConnection _adcpConn;

            /// <summary>
            /// Baud rate.
            /// </summary>
            public string Baud { get; set; }

            /// <summary>
            /// Port.
            /// </summary>
            public string Port { get; set; }

            /// <summary>
            /// Serial Number.
            /// </summary>
            public string SerialNumber { get; set; }

            /// <summary>
            /// Firmware Version.
            /// </summary>
            public string Firmware { get; set; }

            /// <summary>
            /// Hardware type.
            /// </summary>
            public string Hardware { get; set; }

            /// <summary>
            /// Serial Options.
            /// </summary>
            public SerialOptions SerialOptions {get; set;}

            /// <summary>
            /// Connect to serial port.
            /// </summary>
            public ReactiveCommand<System.Reactive.Unit> ConnectCommand { get; protected set; }

            /// <summary>
            /// Initialize
            /// </summary>
            public Adcps()
            {
                // ADCP Connection
                _adcpConn = IoC.Get<AdcpConnection>(); ;

                Baud = "";
                Port = "";
                SerialNumber = "";
                Firmware = "";
                Hardware = "";

                // Find ADCP command
                ConnectCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConn,                                                     // Pass the AdcpConnection
                                                                            x => x.Value.IsOpen()),                                                  // Verify the Serial port is open 
                                                                            _ => Task.Run(() => Connect()));                                        // Start pinging
            }

            /// <summary>
            /// Connect the serial port.
            /// </summary>
            private void Connect()
            {
                _adcpConn.ReconnectAdcpSerial(SerialOptions);
            }
        }

        /// <summary>
        /// List of ADCPs and the COMM port they were found on.
        /// </summary>
        public ObservableCollection<Adcps> AdcpList { get; set; }

        #endregion

        #region Configurations

        /// <summary>
        /// This dictonary is to used to quickly search if the VM
        /// has already been created based off the SubsystemDataConfig
        /// used.  This must be kept in-sync with the VMList;
        /// </summary>
        private ConcurrentDictionary<SubsystemDataConfig, DiagnosticsViewModel> _diagVMDict;

        /// <summary>
        /// The list of values from the VM dictionary.
        /// </summary>
        public List<DiagnosticsViewModel> DiagVMList
        {
            get
            {
                return _diagVMDict.Values.ToList();
            }
        }

        /// <summary>
        /// Selected  ViewModel.
        /// </summary>
        private DiagnosticsViewModel _SelectedDiagVM;
        /// <summary>
        /// Selected ViewModel.
        /// </summary>
        public DiagnosticsViewModel SelectedDiagVM
        {
            get { return _SelectedDiagVM; }
            set
            {
                _SelectedDiagVM = value;
                this.NotifyOfPropertyChange(() => this.SelectedDiagVM);
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Set the default values for the selected subsystem.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> FindAdcpCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public DiagnosticsBaseViewModel()
            : base("Diagnostics Model")
        {
            // Initialize the values
            _pm = IoC.Get<PulseManager>();
            _adcpConn = IoC.Get<AdcpConnection>();
            _events = IoC.Get<IEventAggregator>();
            _events.Subscribe(this);

            // Initialize the dict
            _diagVMDict = new ConcurrentDictionary<SubsystemDataConfig, DiagnosticsViewModel>();

            // Create the ViewModels based off the AdcpConfiguration
            AddConfigurations();

            Init();

            // Find ADCP command
            FindAdcpCommand = ReactiveCommand.CreateAsyncTask(this.WhenAny(_ => _._adcpConn,                                                     // Pass the AdcpConnection
                                                                        x => x.Value != null),                                                   // Verify the Serial port is open 
                                                                        _ => Task.Run(() => FindAdcp()));                                        // Start pinging
        }

        /// <summary>
        /// Shutdown this object.
        /// </summary>
        public override void Dispose()
        {
            // Shutdown all the VMs created
            foreach (var vm in DiagVMList)
            {
                vm.Dispose();
            }
        }

        /// <summary>
        /// The list of configurations could have been cleared when
        /// loading a new project.  This will refresh the list if
        /// it needs to be.
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();

            // If no configurations are loaded, get them
            if (DiagVMList.Count == 0)
            {
                AddConfigurations();
            }
        }

        #region Init

        /// <summary>
        /// Initialize the values.
        /// </summary>
        private void Init()
        {
            AdcpList = new ObservableCollection<Adcps>();
        }

        #endregion

        #region Find ADCP

        /// <summary>
        /// Find the ADCP connected.
        /// </summary>
        private void FindAdcp()
        {
            // Clear the current list
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => AdcpList.Clear()));

            // Find any ADCP
            List<AdcpSerialPort.AdcpSerialOptions> list = _adcpConn.ScanSerialConnection();
            foreach(var adcp in list)
            {
                Adcps item = new Adcps();
                item.SerialOptions = adcp.SerialOptions;
                item.Port = adcp.SerialOptions.Port;
                item.Baud = adcp.SerialOptions.BaudRate.ToString();
                item.SerialNumber = adcp.SerialNumber.ToString();
                item.Firmware = adcp.Firmware.ToString();
                item.Hardware = adcp.Hardware;
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() => 
                    {
                        AdcpList.Add(item);
                        AdcpList.Last().ConnectCommand.Execute(null);
                    }));
            }
        }

        #endregion

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
            if (!_diagVMDict.ContainsKey(config))
            {
                if (_diagVMDict.TryAdd(config, new DiagnosticsViewModel(config)))
                {
                    this.NotifyOfPropertyChange(() => this.DiagVMList);

                    // Select a tab is nothing is selected
                    if (_SelectedDiagVM == null)
                    {
                        if (DiagVMList.Count > 0)
                        {
                            SelectedDiagVM = DiagVMList[0];
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
            _SelectedDiagVM = null;

            foreach (var vm in _diagVMDict.Values)
            {
                vm.Dispose();
            }

            _diagVMDict.Clear();
            this.NotifyOfPropertyChange(() => this.DiagVMList);
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

                // Check if the config exist in the table
                if (!_diagVMDict.ContainsKey(ssDataConfig))
                {
                    Application.Current.Dispatcher.BeginInvoke(new System.Action(() => AddConfig(ssDataConfig)));
                }
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
            if (_diagVMDict.ContainsKey(closeVmEvent.SubsysDataConfig))
            {
                // Dispose the display then remove the display
                _diagVMDict[closeVmEvent.SubsysDataConfig].Dispose();
                DiagnosticsViewModel vm = null;
                if (_diagVMDict.TryRemove(closeVmEvent.SubsysDataConfig, out vm))
                {
                    this.NotifyOfPropertyChange(() => this.DiagVMList);

                    // Select a tab is nothing is selected
                    if (_SelectedDiagVM == null)
                    {
                        if (DiagVMList.Count > 0)
                        {
                            SelectedDiagVM = DiagVMList[0];
                        }
                    }
                }
            }
        }

        #endregion

    }
}
