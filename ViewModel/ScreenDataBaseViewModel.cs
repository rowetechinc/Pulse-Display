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
 * 07/23/2013      RC          3.0.4      Initial coding
 * 08/20/2014      RC          4.0.1      Added CloseVmEvent event.  Changed the list of VM to match ViewDataGraphicalViewModel.
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
    public class ScreenDataBaseViewModel : PulseViewModel, IHandle<EnsembleEvent>, IHandle<ProjectEvent>, IHandle<CloseVmEvent>
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
        /// This dictonary is to used to quickly search if the VM
        /// has already been created based off the SubsystemDataConfig
        /// used.  This must be kept in-sync with the ScreenDataVMList;
        /// </summary>
        private ConcurrentDictionary<SubsystemDataConfig, ScreenDataViewModel> _screenDataVMDict;

        /// <summary>
        /// The list of values from the VM dictionary.
        /// </summary>
        public List<ScreenDataViewModel> ScreenDataVMList
        {
            get
            {
                return _screenDataVMDict.Values.ToList();
            }
        }

        /// <summary>
        /// Selected Screen Data ViewModel.
        /// </summary>
        private ScreenDataViewModel _SelectedScreenDataVM;
        /// <summary>
        /// Selected Screen Data ViewModel.
        /// </summary>
        public ScreenDataViewModel SelectedScreenDataVM
        {
            get { return _SelectedScreenDataVM; }
            set
            {
                _SelectedScreenDataVM = value;
                this.NotifyOfPropertyChange(() => this.SelectedScreenDataVM);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Create a base view for all the ScreenData Views.
        /// </summary>
        public ScreenDataBaseViewModel()
            : base("ScreenDataBaseViewModel")
        {
            // Project Manager
            _pm = IoC.Get<PulseManager>();
            _events = IoC.Get<IEventAggregator>();
            _events.Subscribe(this);

            // Initialize the dict
            _screenDataVMDict = new ConcurrentDictionary<SubsystemDataConfig, ScreenDataViewModel>();

            // Create the ViewModels based off the AdcpConfiguration
            AddConfigurations();
        }

        /// <summary>
        /// Shutdown this object.
        /// </summary>
        public override void Dispose()
        {
            // Shutdown all the VMs created
            foreach (var vm in ScreenDataVMList)
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
            if (ScreenDataVMList.Count == 0)
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
            // Do not screen the averaged data, it has already been averaged
            if (!_screenDataVMDict.ContainsKey(config) && config.Source != EnsembleSource.STA && config.Source != EnsembleSource.LTA)
            {
                if (_screenDataVMDict.TryAdd(config, new ScreenDataViewModel(config)))
                {
                    this.NotifyOfPropertyChange(() => this.ScreenDataVMList);

                    // Select a tab is nothing is selected
                    if (_SelectedScreenDataVM == null)
                    {
                        if (ScreenDataVMList.Count > 0)
                        {
                            SelectedScreenDataVM = ScreenDataVMList[0];
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
            _SelectedScreenDataVM = null;

            foreach (var vm in _screenDataVMDict.Values)
            {
                vm.Dispose();
            }

            _screenDataVMDict.Clear();
            this.NotifyOfPropertyChange(() => this.ScreenDataVMList);
        }

        #endregion

        #region Screen Data

        /// <summary>
        /// Look for the view model for the screen options.
        /// Then pass the ensemble to the VM to be screened.
        /// </summary>
        /// <param name="ensemble">Ensemble to screen.</param>
        public void ScreenData(ref DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                try
                {
                    // Find the VM that matches the configuration
                    foreach (ScreenDataViewModel vm in ScreenDataVMList)
                    {
                        // Do not rescreen the averaged data
                        // it already has been screen
                        if (vm.Config == ensemble.EnsembleData.SubsystemConfig && vm.Config.Source != EnsembleSource.LTA && vm.Config.Source != EnsembleSource.STA)
                        {
                            // Screen the data based off the options in the VM
                            vm.ScreenEnsemble(ref ensemble);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Ususually an exception occurs here at startup when
                    // all the VMs are being created.
                    log.Error("Error screening the data.", e);
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

                // Check if the config exist in the table
                if (!_screenDataVMDict.ContainsKey(ssDataConfig))
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
            if (_screenDataVMDict.ContainsKey(closeVmEvent.SubsysDataConfig))
            {
                // Dispose the display then remove the display
                _screenDataVMDict[closeVmEvent.SubsysDataConfig].Dispose();
                ScreenDataViewModel vm = null;
                if(_screenDataVMDict.TryRemove(closeVmEvent.SubsysDataConfig, out vm))
                {
                    this.NotifyOfPropertyChange(() => this.ScreenDataVMList);

                    // Select a tab is nothing is selected
                    if (SelectedScreenDataVM == null)
                    {
                        if (ScreenDataVMList.Count > 0)
                        {
                            SelectedScreenDataVM = ScreenDataVMList[0];
                        }
                    }
                }
            }
        }

        #endregion

    }
}
