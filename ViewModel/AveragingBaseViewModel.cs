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
 * 07/07/2014      RC          3.4.0      Initial coding
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/21/2014      RC          4.0.1      Clear all the VM properly when changing projects.  On activate, make sure the VM will display.
 * 10/07/2015      RC          4.3.0      Changed dictionary to ConcurrentDicitionary.
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
    using System.Collections.Concurrent;

    /// <summary>
    /// This will create a VM for each Subsystem configuration.
    /// </summary>
    public class AveragingBaseViewModel : PulseViewModel, IHandle<EnsembleEvent>, IHandle<ProjectEvent>, IHandle<CloseVmEvent>
    {
        #region Variables

        /// <summary>
        /// Setup logger to report errors.
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

        #endregion

        #region Properties

        #region Configurations

        /// <summary>
        /// This dictionary is to used to quickly search if the VM
        /// has already been created based off the SubsystemConfiguration
        /// used.  This must be kept in-sync with the AveragingVMList;
        /// </summary>
        private ConcurrentDictionary<SubsystemDataConfig, AveragingViewModel> _averagingVMDict;

        /// <summary>
        /// List of all the Average ViewModels.
        /// </summary>
        public List<AveragingViewModel> AveragingVMList
        {
            get
            {
                return _averagingVMDict.Values.ToList();
            }
        }

        /// <summary>
        /// Selected Averaging ViewModel.
        /// </summary>
        private AveragingViewModel _SelectedAveragingVM;
        /// <summary>
        /// Selected Averaging ViewModel.
        /// </summary>
        public AveragingViewModel SelectedAveragingVM
        {
            get { return _SelectedAveragingVM; }
            set
            {
                _SelectedAveragingVM = value;
                this.NotifyOfPropertyChange(() => this.SelectedAveragingVM);
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Move to the next screen.
        /// </summary>
        public ReactiveCommand<object> NextCommand { get; protected set; }

        /// <summary>
        /// Go back a screen.
        /// </summary>
        public ReactiveCommand<object> BackCommand { get; protected set; }

        /// <summary>
        /// Exit the wizard.
        /// </summary>
        public ReactiveCommand<object> ExitCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Create a base view for all the Averaging Views.
        /// </summary>
        public AveragingBaseViewModel()
            : base("AvergingBaseViewModel")
        {
            // Project Manager
            _pm = IoC.Get<PulseManager>();
            _events = IoC.Get<IEventAggregator>();
            _events.Subscribe(this);

            // Initialize the dict
            _averagingVMDict = new ConcurrentDictionary<SubsystemDataConfig, AveragingViewModel>();

            // Create the ViewModels based off the AdcpConfiguration
            AddConfigurations();

            // Next command
            NextCommand = ReactiveCommand.Create();
            NextCommand.Subscribe(_ => _events.PublishOnUIThread(new ViewNavEvent(ViewNavEvent.ViewId.SimpleCompassCalWizardView)));

            // Back coommand
            BackCommand = ReactiveCommand.Create();
            BackCommand.Subscribe(_ => _events.PublishOnUIThread(new ViewNavEvent(ViewNavEvent.ViewId.Back)));

            // Exit coommand
            ExitCommand = ReactiveCommand.Create();
            ExitCommand.Subscribe(_ => _events.PublishOnUIThread(new ViewNavEvent(ViewNavEvent.ViewId.HomeView)));
        }

        /// <summary>
        /// Shutdown this object.
        /// </summary>
        public override void Dispose()
        {
            // Shutdown all the VMs created
            foreach (var vm in AveragingVMList)
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
            if (AveragingVMList.Count == 0)
            {
                AddConfigurations();
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
            //_averagingVMTable.Add(config);
            //AveragingVMList.Add(new AveragingViewModel(config));
            if (!_averagingVMDict.ContainsKey(config))
            {
                if (_averagingVMDict.TryAdd(config, new AveragingViewModel(config)))
                {
                    this.NotifyOfPropertyChange(() => this.AveragingVMList);

                    // Select a tab is nothing is selected
                    if (_SelectedAveragingVM == null)
                    {
                        if (AveragingVMList.Count > 0)
                        {
                            SelectedAveragingVM = AveragingVMList[0];
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
            _SelectedAveragingVM = null;

            foreach (var vm in _averagingVMDict.Values)
            {
                vm.Dispose();
            }

            _averagingVMDict.Clear();
            this.NotifyOfPropertyChange(() => this.AveragingVMList);
        }

        #endregion

        #region Screen Data

        /// <summary>
        /// Look for the view model for the average options.
        /// Then pass the ensemble to the VM to be averaged.
        /// </summary>
        /// <param name="ensemble">Ensemble to average.</param>
        public void AverageEnsemble(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                try
                {
                    // Find the VM that matches the configuration
                    foreach (AveragingViewModel vm in AveragingVMList)
                    {
                        if (vm.Config == ensemble.EnsembleData.SubsystemConfig)
                        {
                            // Average the data based off the options in the VM
                            vm.AverageEnsemble(ensemble);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Ususually an exception occurs here at startup when
                    // all the VMs are being created.
                    log.Error("Error averaging the data.", e);
                }
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
                
                // Don't add a STA or LTA config to average averaged data
                if (ensEvent.Source != EnsembleSource.STA && ensEvent.Source != EnsembleSource.LTA)
                {
                    // Check if the config exist in the table
                    if (!_averagingVMDict.ContainsKey(ssDataConfig))
                    {
                        Application.Current.Dispatcher.BeginInvoke(new System.Action(() => AddConfig(ssDataConfig)));
                    }
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
            if (_averagingVMDict.ContainsKey(closeVmEvent.SubsysDataConfig))
            {
                // Dispose the display then remove the display
                _averagingVMDict[closeVmEvent.SubsysDataConfig].Dispose();
                AveragingViewModel vm = null;
                if (_averagingVMDict.TryRemove(closeVmEvent.SubsysDataConfig, out vm))
                {
                    this.NotifyOfPropertyChange(() => this.AveragingVMList);

                    // Select a tab is nothing is selected
                    if (_SelectedAveragingVM == null)
                    {
                        if (AveragingVMList.Count > 0)
                        {
                            SelectedAveragingVM = AveragingVMList[0];
                        }
                    }
                }
            }
        }

        #endregion

    }
}
