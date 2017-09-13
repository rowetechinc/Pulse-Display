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
 * 11/15/2012      RC          2.16       Initial coding
 * 01/23/2013      RC          2.17       In SerialNumValue, allow the value to be 0.
 * 10/04/2013      RC          3.2.0      Added AddSubsystem() so a subsystem can be added externally.
 * 07/15/2014      RC          3.4.0      Changed the default serial number.  Also i have the subsystem type expanded by default.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/01/2015      RC          4.1.3      Added an event to know when the serial number changes. 
 * 09/13/2017      RC          4.4.7      Added UpdateSerialNumber() to add a new serial number.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using ReactiveUI;

namespace RTI
{

    /// <summary>
    /// Generate a serial number.  This will have all the enteries for the
    /// serial number.
    /// </summary>
    public class SerialNumberGeneratorViewModel : PulseViewModel
    {

        #region Variables



        #endregion

        #region Properties

        /// <summary>
        /// Serial number to generate.
        /// </summary>
        private SerialNumber _SerialNumber;
        /// <summary>
        /// Serial number to generate.
        /// </summary>
        public SerialNumber SerialNumber
        {
            get { return _SerialNumber; }
            set
            {
                _SerialNumber = value;
                this.NotifyOfPropertyChange(() => this.SerialNumber);

                // Send event to update Serial number
                UpdateSerialNumberEvent();
            }
        }

        /// <summary>
        /// Serial Number as a string.
        /// </summary>
        public string SerialNumStr
        {
            get { return _SerialNumber.SerialNumberString; }
        }

        #region Spare

        /// <summary>
        /// 
        /// </summary>
        public string Spare
        {
            get { return _SerialNumber.Spare.PadRight(SerialNumber.SPARE_NUM_BYTES, '0'); }
            set
            {
                SerialNumber.Spare = value;
                this.NotifyOfPropertyChange(() => this.Spare);
                this.NotifyOfPropertyChange(() => this.SerialNumStr);

                // Send event to update Serial number
                UpdateSerialNumberEvent();
            }
        }

        /// <summary>
        /// Flag to display the Spare value changer.
        /// </summary>
        private bool _isSpareValueVis;
        /// <summary>
        /// Flag to display the Spare value changer.
        /// </summary>
        public bool IsSpareValueVis
        {
            get { return _isSpareValueVis; }
            set
            {
                _isSpareValueVis = value;
                this.NotifyOfPropertyChange(() => this.IsSpareValueVis);
            }
        }

        #endregion

        #region Serial Number

        /// <summary>
        /// System Serial number as a string.  This will
        /// include 6 digits with the serial number.
        /// </summary>
        public string SerialNum
        {
            get { return _SerialNumber.SystemSerialNumber.ToString("000000"); }
            set
            {
                uint result = 0;
                if (uint.TryParse(value, out result))
                {
                    SerialNumber.SystemSerialNumber = result;
                }

                this.NotifyOfPropertyChange(() => this.SerialNum);
                this.NotifyOfPropertyChange(() => this.SerialNumStr);

                // Send event to update Serial number
                UpdateSerialNumberEvent();
            }
        }

        /// <summary>
        /// Flag to display the Serial Number value changer.
        /// </summary>
        private bool _isSerialNumValueVis;
        /// <summary>
        /// Flag to display the Serial Number value changer.
        /// </summary>
        public bool IsSerialNumValueVis
        {
            get { return _isSerialNumValueVis; }
            set
            {
                _isSerialNumValueVis = value;
                this.NotifyOfPropertyChange(() => this.IsSerialNumValueVis);
            }
        }

        /// <summary>
        /// Serial Number value set in the changer.
        /// This will allow the user to change the serial 
        /// nummber.  The serial number can only be a 6
        /// digit number between 0 and 999999.
        /// </summary>
        public int SerialNumValue
        {
            get { return (int)_SerialNumber.SystemSerialNumber; }
            set
            {
                this.NotifyOfPropertyChange(() => this.SerialNumValue);

                if (value >= 0 && value <= 999999)
                {
                    SerialNumber.SystemSerialNumber = (uint)value;

                    this.NotifyOfPropertyChange(() => this.SerialNum);
                    this.NotifyOfPropertyChange(() => this.SerialNumStr);
                }

                // Send event to update Serial number
                UpdateSerialNumberEvent();
            }
        }

        #endregion

        #region Subsystem

        /// <summary>
        /// Flag to display the Subsystem value changer.
        /// </summary>
        private bool _isSubsystemValueVis;
        /// <summary>
        /// Flag to display the Subsystem value changer.
        /// </summary>
        public bool IsSubsystemValueVis
        {
            get { return _isSubsystemValueVis; }
            set
            {
                _isSubsystemValueVis = value;
                this.NotifyOfPropertyChange(() => this.IsSubsystemValueVis);
            }
        }

        /// <summary>
        /// List of all the subsystems with a description.
        /// Used to populate the combobox.
        /// </summary>
        public SubsystemList ListOfSubsystems { get; set; }

        /// <summary>
        /// Selected Subsystem from the combobox.
        /// </summary>
        private RTI.SubsystemList.SubsystemCodeDesc _selectedSubsystem;
        /// <summary>
        /// Selected Subsystem from the combobox.
        /// </summary>
        public RTI.SubsystemList.SubsystemCodeDesc SelectedSubsystem
        {
            get { return _selectedSubsystem; }
            set
            {
                _selectedSubsystem = value;
                this.NotifyOfPropertyChange(() => this.SelectedSubsystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Subsystems
        {
            get { return _SerialNumber.SubSystems; }
        }

        #endregion

        #region Base Electronic Hardware

        /// <summary>
        /// Flag to display the Subsystem value changer.
        /// </summary>
        private bool _isBaseElectTypeValueVis;
        /// <summary>
        /// Flag to display the Subsystem value changer.
        /// </summary>
        public bool IsBaseElectTypeValueVis
        {
            get { return _isBaseElectTypeValueVis; }
            set
            {
                _isBaseElectTypeValueVis = value;
                this.NotifyOfPropertyChange(() => this.IsBaseElectTypeValueVis);
            }
        }

        /// <summary>
        /// List of all the base electronic hardware types.
        /// </summary>
        public ReactiveList<string> ListOfBaseElecType { get; set; }

        /// <summary>
        /// The Selected Base Elec Type.
        /// </summary>
        public string SelectedBaseElecType
        {
            get { return _SerialNumber.BaseHardware; }
            set
            {
                SerialNumber.BaseHardware = value;
                this.NotifyOfPropertyChange(() => this.SelectedBaseElecType);
                this.NotifyOfPropertyChange(() => this.SerialNumStr);

                // Send event to update Serial number
                UpdateSerialNumberEvent();
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to disable/Enable the Serial Number value view. 
        /// </summary>
        public ReactiveCommand<object> SerialNumValueViewCommand { get; protected set; }

        /// <summary>
        /// Command to disable/Enable the Spare value view. 
        /// </summary>
        public ReactiveCommand<object> SpareValueViewCommand { get; protected set; }

        /// <summary>
        /// Command to disable/Enable the Subsystem value view. 
        /// </summary>
        public ReactiveCommand<object> SubsystemValueViewCommand { get; protected set; }

        /// <summary>
        /// Command to disable/Enable the Serial Number value view. 
        /// </summary>
        public ReactiveCommand<object> AddSubsystemCommand { get; protected set; }

        /// <summary>
        /// Command to remove a subsystem from the serial number.
        /// </summary>
        public ReactiveCommand<object> RemoveSubsystemCommand { get; protected set; }

        /// <summary>
        /// Command to disable/Enable the Base Electronic Type value view. 
        /// </summary>
        public ReactiveCommand<object> BaseElecTypeValueViewCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public SerialNumberGeneratorViewModel()
            : base("Serial Number Generator")
        {
            // Initialize the value
            _SerialNumber = new SerialNumber("01000000000000000000000000000001");

            // Populate the subsystem list
            PopulateSubsystemList();

            // Populate Base Electronic Type List
            PopulateBaseElectronicTypeList();

            _isSerialNumValueVis = false;
            _isSpareValueVis = false;
            _isSubsystemValueVis = true;
            _isBaseElectTypeValueVis = false;

            UpdateProperties();

            // Commands
            SerialNumValueViewCommand = ReactiveCommand.Create();
            SerialNumValueViewCommand.Subscribe(_ => On_SerialNumValueViewCommand());

            SpareValueViewCommand = ReactiveCommand.Create();
            SpareValueViewCommand.Subscribe(_ => On_SpareValueViewCommand());

            SubsystemValueViewCommand = ReactiveCommand.Create();
            SubsystemValueViewCommand.Subscribe(_ => On_SubsystemValueViewCommand());

            AddSubsystemCommand = ReactiveCommand.Create();
            AddSubsystemCommand.Subscribe(_ => On_AddSubsystemCommand());

            RemoveSubsystemCommand = ReactiveCommand.Create();
            RemoveSubsystemCommand.Subscribe(_ => On_RemoveSubsystemCommand());

            BaseElecTypeValueViewCommand = ReactiveCommand.Create();
            BaseElecTypeValueViewCommand.Subscribe(_ => On_BaseElecTypeValueViewCommand());
        }

        /// <summary>
        /// Shutdown the view model.
        /// </summary>
        public override void Dispose()
        {

        }

        /// <summary>
        /// Update the serial number with a given serial number.
        /// </summary>
        /// <param name="serialNumber">New serial number.</param>
        public void UpdateSerialNumber(SerialNumber serialNumber)
        {
            _SerialNumber = serialNumber;

            UpdateProperties();
        }

        /// <summary>
        /// Update the properties.
        /// </summary>
        private void UpdateProperties()
        {
            this.NotifyOfPropertyChange(() => this.SerialNum);
            this.NotifyOfPropertyChange(() => this.SerialNumStr);
            this.NotifyOfPropertyChange(() => this.SerialNumValue);
            this.NotifyOfPropertyChange(() => this.Spare);
            this.NotifyOfPropertyChange(() => this.Subsystems);
            this.NotifyOfPropertyChange(() => this.SelectedBaseElecType);
        }

        /// <summary>
        /// Create a list of all the subsystem types.
        /// Then select the first first subsystem.
        /// </summary>
        private void PopulateSubsystemList()
        {
            // Create the list
            ListOfSubsystems = new SubsystemList();

            // Set something new to the selected subsystem
            if (ListOfSubsystems.Count > 0)
            {
                SelectedSubsystem = ListOfSubsystems.First();
            }
        }

        /// <summary>
        /// Create a list of all the Base Electronic Hardware types.
        /// Then select the first type
        /// </summary>
        private void PopulateBaseElectronicTypeList()
        {
            // Create the list
            ListOfBaseElecType = new ReactiveList<string>();

            // Add all Base Electronic Types
            foreach (var hw in SerialNumber.BaseHardwareList)
            {
                ListOfBaseElecType.Add(hw);        // ADCP1
            }

            // Set something new to the selected Selected Base Electronic type
            if (ListOfBaseElecType.Count > 1)
            {
                SelectedBaseElecType = ListOfBaseElecType[1];
            }
        }

        #region Add Subsystem

        /// <summary>
        /// Add a subsystem to the serial number.  Then 
        /// update the removed the selected subsystem from the list
        /// and select a new default.
        /// </summary>
        /// <param name="ss">Subsystem to add.</param>
        public void AddSubsystem(Subsystem ss)
        {
            _SerialNumber.AddSubsystem(ss);

            RTI.SubsystemList.SubsystemCodeDesc subsysDesc = new RTI.SubsystemList.SubsystemCodeDesc(ss.Code, Subsystem.DescString(ss.Code));
            if(ListOfSubsystems.Contains(subsysDesc))
            {
                ListOfSubsystems.Remove(subsysDesc);
            }

            // Set something new to the selected subsystem
            if (ListOfSubsystems.Count > 0)
            {
                SelectedSubsystem = ListOfSubsystems.First();
            }

            // Update the Subsystem and Serial number string
            this.NotifyOfPropertyChange(() => this.Subsystems);
            this.NotifyOfPropertyChange(() => this.SerialNumStr);

            // Send event to update Serial number
            UpdateSerialNumberEvent();
        }

        #endregion

        #region Commands

        #region SerialNumValueViewCommand

        /// <summary>
        /// Disable/Enable the Serial Number value view. 
        /// </summary>
        private void On_SerialNumValueViewCommand( )
        {
            IsSerialNumValueVis = !IsSerialNumValueVis;
        }

        #endregion

        #region SpareValueViewCommand

        /// <summary>
        /// Disable/Enable the Spare value view. 
        /// </summary>
        private void On_SpareValueViewCommand()
        {
            IsSpareValueVis = !IsSpareValueVis;
        }

        #endregion

        #region SubsystemValueViewCommand

        /// <summary>
        /// Disable/Enable the Subsystem value view. 
        /// </summary>
        private void On_SubsystemValueViewCommand()
        {
            IsSubsystemValueVis = !IsSubsystemValueVis;
        }

        #endregion

        #region AddSubsystemCommand

        /// <summary>
        /// Add a subsystem to the serial number.
        /// </summary>
        private void On_AddSubsystemCommand()
        {
            if (SelectedSubsystem != null)
            {
                // Create a subsystem based off the selected values
                // and set it to the serial number
                Subsystem ss = new Subsystem(SelectedSubsystem.Code, (ushort)_SerialNumber.SubSystemsList.Count);
                AddSubsystem(ss);
            }
        }

        /// <summary>
        /// Disable the button if the number of subsystems is full.
        /// </summary>
        /// <returns>TRUE = Can add another Subsystem.</returns>
        private bool Can_AddSubsystemCommand()
        {
            if (_SerialNumber.SubSystemsList.Count >= SerialNumber.SUBSYSTEM_NUM_BYTES)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region RemoveSubsystemCommand

        /// <summary>
        /// Remove a subsystem from the serial number.
        /// </summary>
        private void On_RemoveSubsystemCommand()
        {
            if (_SerialNumber.SubSystemsList.Count > 0)
            {
                // Remove the last subsystem in the serial number
                Subsystem ss = _SerialNumber.SubSystemsList.Last();
                _SerialNumber.RemoveSubsystem(ss);

                // Add the Subsystem type back to the list
                RTI.SubsystemList.SubsystemCodeDesc subsysDesc = new RTI.SubsystemList.SubsystemCodeDesc(ss.Code, Subsystem.DescString(ss.Code));
                if (!ListOfSubsystems.Contains(subsysDesc))
                {
                    ListOfSubsystems.Add(subsysDesc);

                    // Set something new to the selected subsystem
                    if (ListOfSubsystems.Count > 0)
                    {
                        SelectedSubsystem = ListOfSubsystems.First();
                    }
                }
            }

            // Update the Subsystem and Serial number string
            this.NotifyOfPropertyChange(() => this.Subsystems);
            this.NotifyOfPropertyChange(() => this.SerialNumStr);

            //// Call this to update the button if should be disabled
            //((DelegateCommand<object>)AddSubsystemCommand).RaiseCanExecuteChanged();
            //((DelegateCommand<object>)RemoveSubsystemCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Disable the button if there are no subsystems.
        /// </summary>
        /// <returns>TRUE = Can add another Subsystem.</returns>
        private bool Can_RemoveSubsystemCommand()
        {
            if (_SerialNumber.SubSystemsList.Count > 0)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region BaseElecTypeValueViewCommand

        /// <summary>
        /// Disable/Enable the Base Electronic Type value view. 
        /// </summary>
        private void On_BaseElecTypeValueViewCommand()
        {
            IsBaseElectTypeValueVis = !IsBaseElectTypeValueVis;
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void UpdateEventHandler();

        /// <summary>
        /// Subscribe to know when the serial number has been updated.
        /// To subscribe:
        /// vm.UpdateEvent += new vm.UpdateEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// vm.UpdateEvent -= (method to call)
        /// </summary>
        public event UpdateEventHandler UpdateEvent;

        /// <summary>
        /// Update the serial number event.
        /// </summary>
        private void UpdateSerialNumberEvent()
        {
            if(UpdateEvent != null)
            {
                UpdateEvent();
            }
        }

        #endregion
    }
}
