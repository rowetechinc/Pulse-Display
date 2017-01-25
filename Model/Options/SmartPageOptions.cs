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
 * 05/28/2013      RC          3.0.0      Initial coding
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using DotSpatial.Positioning;
    using Newtonsoft.Json;
    using System.Windows;
    using ReactiveUI;
    using RTI.Commands;

    /// <summary>
    /// A list of all subsystems.  This is used
    /// to display the list as a string with the code
    /// and description.
    /// </summary>
    public class ListOfSubsystems : List<Subsystem>
    {

        /// <summary>
        /// Used to display the list.
        /// Set the DisplayMember of the combobox.
        /// http://stackoverflow.com/questions/3664956/c-sharp-combobox-overridden-tostring
        /// </summary>
        public string Display { get { return ToString(); } }

        /// <summary>
        /// Take the given list and set it to the base class.
        /// </summary>
        /// <param name="list">List to set to the base class.</param>
        public ListOfSubsystems(List<Subsystem> list)
            : base(list)
        {
            
        }

        /// <summary>
        /// Combine all the codes in the list to create the CEPO command.
        /// </summary>
        /// <returns>String of all the codes for the CEPO command.</returns>
        public string GetCEPO()
        {
            string cepo = "";

            // Combine all the codes to create the CEPO command
            foreach (var ss in this)
            {
                cepo += ss.CodeToString();
            }

            return cepo;
        }

        /// <summary>
        /// Overwrite the string for this list.
        /// Combine the code and description to 
        /// give a string for each entry.
        /// </summary>
        /// <returns>String for each entry.</returns>
        public override string ToString()
        {
            // Convert the list of subsystems to a string.
            string ssCode = "";
            string ssDesc = "";
            foreach (var ss in this)
            {
                ssCode += ss.CodeToString();
                ssDesc += ss.DescString() + " | ";
            }

            return ssCode + "\t" + ssDesc;
        }
    }

    /// <summary>
    /// All the options for the SmartPage viewmodel.
    /// This is used so the options can be stored.
    /// </summary>
    public class SmartPageOptions
    {

        #region Variable

        #region Accoustic Mode

        /// <summary>
        /// Profiler accoustic mode.
        /// </summary>
        public const string ACCOUSTIC_MODE_PROFILER = "Profiler";

        /// <summary>
        /// Self Contained accoustic mode.
        /// </summary>
        public const string ACCOUSTIC_MODE_SELF_CONTAINED = "Self Contained";

        /// <summary>
        /// DVL accoustic mode.
        /// </summary>
        public const string ACCOUSTIC_MODE_DVL = "DVL";

        /// <summary>
        /// River accoustic mode.
        /// </summary>
        public const string ACCOUSTIC_MODE_RIVER = "River";

        /// <summary>
        /// Waves accoustic mode.
        /// </summary>
        public const string ACCOUSTIC_MODE_WAVES = "Waves";

        #endregion

        #endregion

        #region Properties

        #region Configuration

        /// <summary>
        /// Configuration currently selected.
        /// </summary>
        public string SelectedConfiguration { get; set; }

        #endregion

        #region Accoustic Mode

        /// <summary>
        /// Accoustic Mode currently selected.
        /// </summary>
        public string SelectedAccousticMode { get; set; }

        #endregion

        #region ADCP

        /// <summary>
        /// Last clock reading of the ADCP.
        /// </summary>
        public string ClockStr { get; set; }

        /// <summary>
        /// Total bytes based off the deployment settings.
        /// </summary>
        public long MemoryCardRequired { get; set; }

        /// <summary>
        /// Size the memory card in the ADCP.
        /// In bytes.
        /// </summary>
        public long MemoryCardCapacity { get; set; }

        /// <summary>
        /// Memory used by the ADCP.
        /// In bytes.
        /// </summary>
        public long MemoryCardUsed { get; set; }

        /// <summary>
        /// Result of the compass calibration.
        /// This will be filled if a calibration has
        /// been completed.  It will be empty if no
        /// calibration has been done.
        /// </summary>
        public string CompassCalStr { get; set; }

        /// <summary>
        /// Display if there is a pressure sensor present.
        /// If there is one present, display the type.
        /// Also display to zero the pressrue sensor
        /// if the sensor is not reading near zero.
        /// </summary>
        public string PressureSensorStr { get; set; }

        #endregion

        #region Hardware

        /// <summary>
        /// Selected Subsystem string.  This is the subsystems
        /// selected and will be used to populate the CEPO command.
        /// </summary>
        //public ListOfSubsystems Subsystems { get; set; }
        public Subsystem SelectedSubsystem { get; set; }

        /// <summary>
        /// Set flag if the user wants the ensemble data output on the
        /// serial port.
        /// </summary>
        public Commands.AdcpCommands.AdcpOutputMode SerialOutput { get; set; }

        /// <summary>
        /// Set a flag if we are burst recording.
        /// </summary>
        public bool IsBurstRecording { get; set; }

        #endregion

        #region Environmental

        #endregion

        #region Ping Control

        #endregion

        #region Additional Commands

        /// <summary>
        /// String to hold all additional commands.
        /// </summary>
        public string AdditionalCommands { get; set; }

        #endregion

        #region EngConf

        /// <summary>
        /// ADCP Hardware configuration.
        /// </summary>
        public EngConf HdrwConfig { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public SmartPageOptions()
        {
            // Set the default values
            SetDefaults();
        }

        /// <summary>
        /// Constructor to set all the values for JSON constructor.
        /// </summary>
        /// <param name="ClockStr">Last Clock reading of the ADCP.</param>
        /// <param name="MemoryCardRequired">Total bytes required for deployment.</param>
        /// <param name="MemoryCardCapacity">Size of the memory card in bytes.</param>
        /// <param name="MemoryCardUsed">Amount of memory used on the ADCP in bytes.</param>
        /// <param name="CompassCalStr">String of the compass cal result.</param>
        /// <param name="PressureSensorStr">Result of the pressure sensor being present.</param>
        /// <param name="SelectedConfiguration">Selected configuration.</param>
        /// <param name="SelectedAccousticMode">Selected accoustic mode.</param>
        /// <param name="SelectedSubsystem">Selected subsystem.</param>
        /// <param name="IsBurstRecording">Flag if burst recording is on.</param>
        /// <param name="AdditionalCommands">String for additional commands.</param>
        /// <param name="HdrwConfig">ADCP Hardware Configuration.</param>
        [JsonConstructor]
        public SmartPageOptions(string ClockStr, long MemoryCardRequired, long MemoryCardCapacity, long MemoryCardUsed, string CompassCalStr, string PressureSensorStr, string SelectedConfiguration, string SelectedAccousticMode, Subsystem SelectedSubsystem, bool IsBurstRecording, string AdditionalCommands, EngConf HdrwConfig)
        {
            this.ClockStr = ClockStr;
            this.MemoryCardRequired = MemoryCardRequired;
            this.MemoryCardCapacity = MemoryCardCapacity;
            this.MemoryCardUsed = MemoryCardUsed;
            this.CompassCalStr = CompassCalStr;
            this.PressureSensorStr = PressureSensorStr;
            this.SelectedConfiguration = SelectedConfiguration;
            this.SelectedAccousticMode = SelectedAccousticMode;
            this.SelectedSubsystem = SelectedSubsystem;
            this.IsBurstRecording = IsBurstRecording;
            this.AdditionalCommands = AdditionalCommands;
            this.HdrwConfig = HdrwConfig;
            
            // If the HdrwConfig is null, then create a default 
            if (this.HdrwConfig == null)
            {
                this.HdrwConfig = new EngConf();
            }
        }

        #region Defaults

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            // Initialize the values
            SelectedConfiguration = null;                                           // By default select nothing
            SelectedAccousticMode = SmartPageOptions.ACCOUSTIC_MODE_PROFILER;       // By default select nothing

            // Hardware
            SelectedSubsystem = null;
            IsBurstRecording = false;

            // Additional Commands
            AdditionalCommands = "";

            // ADCP
            ClockStr = "";
            MemoryCardRequired = 0;
            MemoryCardCapacity = 0;
            MemoryCardUsed = 0;
            CompassCalStr = "";
            PressureSensorStr = "";

            // EngConf
            HdrwConfig = new EngConf();
        }

        #endregion
    }
}
