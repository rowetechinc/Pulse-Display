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
 * 08/09/2013      RC          3.0.7      Initial coding
 * 08/26/2013      RC          3.0.8      Added ExportDataOptions.
 * 12/09/2013      RC          3.2.0      Added ScreenOptions.
 * 02/12/2014      RC          3.2.3      Added VesselMountOptions.
 * 02/18/2014      RC          3.2.3      Fixed bug in AddConfiguration() to check if the key already exist.
 * 04/02/2014      RC          3.2.4      Changed ExportDataOptions to ExportOptions.
 * 03/16/2015      RC          
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using System.IO;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// All the options for a specific Subsystem Configuration.
    /// </summary>
    public class SubsystemOptions
    {
        #region Properties

        /// <summary>
        /// Text Subsystem Configuration options.
        /// </summary>
        public TextSubsystemConfigOptions TextOptions { get; set; }

        /// <summary>
        /// Graphical Subsystem Configuration options.
        /// </summary>
        public ViewDataGraphicalOptions GraphicalOptions { get; set; }

        /// <summary>
        /// Screening Subsystem Configuration options.
        /// </summary>
        public ScreenSubsystemConfigOptions ScreenOptions { get; set; }

        /// <summary>
        /// Averaging Subsystem Configuration options.
        /// </summary>
        public AverageSubsystemConfigOptions AverageOptions { get; set; }

        /// <summary>
        /// Validation View Subsystem Configuration options.
        /// </summary>
        public ValidationTestViewOptions ValidationViewOptions { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="ssConfig">Set the Subsystem Configuration.</param>
        public SubsystemOptions(SubsystemConfiguration ssConfig)
        {
            TextOptions = new TextSubsystemConfigOptions(ssConfig);
            GraphicalOptions = new ViewDataGraphicalOptions(ssConfig);
            ScreenOptions = new ScreenSubsystemConfigOptions(ssConfig);
            AverageOptions = new AverageSubsystemConfigOptions(ssConfig);
            ValidationViewOptions = new ValidationTestViewOptions(ssConfig);
        }
    }

    /// <summary>
    /// Class to hold all the Pulse configuration options.
    /// This will be stored with the project.
    /// </summary>
    //[JsonConverter(typeof(PulseConfigurationSerializer))]
    public class PulseConfiguration : IDisposable
    {

        #region Variables

        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Properties

        /// <summary>
        /// A dictionary to hold all the subsystem configuration and
        /// options for each subsystem configuration.
        /// </summary>
        public Dictionary<string, SubsystemOptions> SubsysOptions { get; set; }

        /// <summary>
        /// I used a HashSet here and not just store the SubsystemConfiguration in the
        /// dictionary, so i can easily convert between JSON.  JSON does not seem to like
        /// the key to be not be a string or int.  So now the key will be the IndexCodeString() 
        /// of the SubsystemConfiguration.
        /// </summary>
        public HashSet<SubsystemConfiguration> SubsystemConfigList { get; set; }

        /// <summary>
        /// Smart Page options.
        /// </summary>
        public SmartPageOptions SmartPageOptions { get; set; }

        /// <summary>
        /// Export Data Page Options.
        /// </summary>
        public ExportOptions ExportDataOptions { get; set; }

        /// <summary>
        /// Vessel Mount options.
        /// </summary>
        public VesselMountOptions VesselMountOptions { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public PulseConfiguration()
        {
            SubsysOptions = new Dictionary<string, SubsystemOptions>();
            SubsystemConfigList = new HashSet<SubsystemConfiguration>();
            SmartPageOptions = new SmartPageOptions();
            ExportDataOptions = new ExportOptions();
            VesselMountOptions = new VesselMountOptions();
        }

        /// <summary>
        /// Constructor used for JSON decoding.
        /// </summary>
        /// <param name="SubsysOptions">Subsystem options.</param>
        /// <param name="SubsystemConfigList">Subsystem Configuration list.</param>
        /// <param name="SmartPageOptions">SmartPage options.</param>
        /// <param name="ExportOptions">Export Data options.</param>
        /// <param name="VesselMountOptions">Vessel Mount Options.</param>
        [JsonConstructor]
        public PulseConfiguration(Dictionary<string, SubsystemOptions> SubsysOptions, HashSet<SubsystemConfiguration> SubsystemConfigList, SmartPageOptions SmartPageOptions, ExportDataOptions ExportOptions, VesselMountOptions VesselMountOptions)
        {
            this.SubsysOptions = SubsysOptions;
            this.SubsystemConfigList = SubsystemConfigList;
            this.SmartPageOptions = SmartPageOptions;
            this.ExportDataOptions = ExportDataOptions;
            this.VesselMountOptions = VesselMountOptions;

            // Check for Nulls
            CheckForNull();
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public void Dispose()
        {

        }

        #region Check for Null

        /// <summary>
        /// If the project is an older version, then an 
        /// object may not have existed in the project.
        /// This will ensure all the object are at least
        /// created with default values.
        /// </summary>
        private void CheckForNull()
        {
            if (SubsysOptions == null)
            {
                SubsysOptions = new Dictionary<string, SubsystemOptions>();
            }

            if (SubsystemConfigList == null)
            {
                SubsystemConfigList = new HashSet<SubsystemConfiguration>();
            }

            if (SmartPageOptions == null)
            {
                SmartPageOptions = new SmartPageOptions();
            }

            if (ExportDataOptions == null)
            {
                ExportDataOptions = new ExportOptions();
            }
        }

        #endregion

        #region Save Options

        /// <summary>
        /// Changes have been made.  Save the new options.
        /// This will publish an event to save this object.
        /// </summary>
        private void SaveOptions()
        {
            // Send event to store this object 
            PublishSaveOptionsEvent();
        }

        #endregion

        #region Add/Remove Configuration

        /// <summary>
        /// Add a configuration to the dictionary of subsystem configuraitons.
        /// </summary>
        /// <param name="ssConfig">Subystem Configuration</param>
        private void AddConfiguration(SubsystemConfiguration ssConfig)
        {
            // Add an entry to the dictionary
            if (!SubsysOptions.ContainsKey(ssConfig.IndexCodeString()))
            { 
                SubsysOptions.Add(ssConfig.IndexCodeString(), new SubsystemOptions(ssConfig));
                SubsystemConfigList.Add(ssConfig);
            }
        }

        #endregion

        #region Text Options

        /// <summary>
        /// Get the text options from the hashset.  This will
        /// check if the subsystem configuration is in the hashset.
        /// If it is in the hashset, it will get the text options.
        /// If it is not, it will return a default configuration.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration to check.</param>
        /// <returns>Text options.</returns>
        public TextSubsystemConfigOptions GetTextOptions(SubsystemConfiguration ssConfig)
        {
            TextSubsystemConfigOptions options = null;
            if (SubsystemConfigList.Contains(ssConfig))
            {
                options = SubsysOptions[ssConfig.IndexCodeString()].TextOptions;
            }
            else
            {
                options = new TextSubsystemConfigOptions();
            }

            return options;
        }

        /// <summary>
        /// Save the Text SubsystemConfiguration options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        /// <param name="options">Text SubsystemConfiguration options.</param>
        public void SaveTextOptions(SubsystemConfiguration ssConfig, TextSubsystemConfigOptions options)
        {
            // Check if the subsystem exist in the dictionary
            if (SubsystemConfigList.Contains(ssConfig))
            {
                // Store the text options to the SubsystemConfig entry
                SubsysOptions[ssConfig.IndexCodeString()].TextOptions = options;
            }
            else
            {
                // Add new subsystem configuration
                AddConfiguration(ssConfig);

                // Store the new options if the entry could be made
                if (SubsystemConfigList.Contains(ssConfig))
                {
                    // Store the text options to the SubsystemConfig entry
                    SubsysOptions[ssConfig.IndexCodeString()].TextOptions = options;
                }
            }

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region Graphical Options

        /// <summary>
        /// Get the Graphical options from the dictionary.  This will
        /// check if the subsystem configuration is in the hashset.
        /// If it is in the hashset, it will get the Graphical options.
        /// If it is not, it will return a default configuration.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration to check.</param>
        /// <returns>Graphical options.</returns>
        public ViewDataGraphicalOptions GetGraphicalOptions(SubsystemConfiguration ssConfig)
        {
            ViewDataGraphicalOptions options = null;
            if (SubsystemConfigList.Contains(ssConfig))
            {
                options = SubsysOptions[ssConfig.IndexCodeString()].GraphicalOptions;
            }
            else
            {
                options = new ViewDataGraphicalOptions();
            }

            return options;
        }

        /// <summary>
        /// Save the Graphical SubsystemConfiguration options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        /// <param name="options">Graphical SubsystemConfiguration options.</param>
        public void SaveGraphicalOptions(SubsystemConfiguration ssConfig, ViewDataGraphicalOptions options)
        {
            // Check if the subsystem exist in the dictionary
            if (SubsystemConfigList.Contains(ssConfig))
            {
                // Store the graphical options to the SubsystemConfig entry
                SubsysOptions[ssConfig.IndexCodeString()].GraphicalOptions = options;
            }
            else
            {
                // Add new subsystem configuration
                AddConfiguration(ssConfig);

                // Store the new options if the entry could be made
                if (SubsystemConfigList.Contains(ssConfig))
                {
                    // Store the graphical options to the SubsystemConfig entry
                    SubsysOptions[ssConfig.IndexCodeString()].GraphicalOptions = options;
                }
            }

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region Validation View Options

        /// <summary>
        /// Get the Validation View options from the dictionary.  This will
        /// check if the subsystem configuration is in the hashset.
        /// If it is in the hashset, it will get the Validation View options.
        /// If it is not, it will return a default configuration.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration to check.</param>
        /// <returns>Validation View options.</returns>
        public ValidationTestViewOptions GetValidationViewOptions(SubsystemConfiguration ssConfig)
        {
            ValidationTestViewOptions options = null;
            if (SubsystemConfigList.Contains(ssConfig))
            {
                options = SubsysOptions[ssConfig.IndexCodeString()].ValidationViewOptions;
            }
            else
            {
                // If there are any exist, use the first one
                if (SubsystemConfigList.Count > 0)
                {
                    options = SubsysOptions.First().Value.ValidationViewOptions;
                }
                else
                {
                    // No exist, so create a default option
                    options = new ValidationTestViewOptions();
                }
            }

            return options;
        }

        /// <summary>
        /// Save the Validation View SubsystemConfiguration options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        /// <param name="options">Validation View SubsystemConfiguration options.</param>
        public void SaveValidationViewOptions(SubsystemConfiguration ssConfig, ValidationTestViewOptions options)
        {
            // Check if the subsystem exist in the dictionary
            if (SubsystemConfigList.Contains(ssConfig))
            {
                // Store the Validation View options to the SubsystemConfig entry
                SubsysOptions[ssConfig.IndexCodeString()].ValidationViewOptions = options;
            }
            else
            {
                // Add new subsystem configuration
                AddConfiguration(ssConfig);

                // Store the new options if the entry could be made
                if (SubsystemConfigList.Contains(ssConfig))
                {
                    // Store the Validation View options to the SubsystemConfig entry
                    SubsysOptions[ssConfig.IndexCodeString()].ValidationViewOptions = options;
                }
            }

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region SmartPage Options

        /// <summary>
        /// Get the Smartpage options from the project.
        /// </summary>
        /// <returns>SmartPage options.</returns>
        public SmartPageOptions GetSmartPageOptions()
        {
            return SmartPageOptions;
        }

        /// <summary>
        /// Save the SmartPage options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="options">SmartPage options.</param>
        public void SaveSmartPageOptions(SmartPageOptions options)
        {
            // Set the options
            SmartPageOptions = options;

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region ExportData Options

        /// <summary>
        /// Get the ExportData options from the project.
        /// </summary>
        /// <returns>ExportData options.</returns>
        public ExportOptions GetExportDataOptions()
        {
            return ExportDataOptions;
        }

        /// <summary>
        /// Save the ExportData options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="options">ExportData options.</param>
        public void SaveExportDataOptions(ExportOptions options)
        {
            // Set the options
            ExportDataOptions = options;

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region VesselMount Options

        /// <summary>
        /// Get the Vessel Mount options from the project.
        /// </summary>
        /// <returns>Vessel Mount options.</returns>
        public VesselMountOptions GetVesselMountOptions()
        {
            return VesselMountOptions;
        }

        /// <summary>
        /// Save the Vessel Mount options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="options">ExportData options.</param>
        public void SaveVesselMountOptions(VesselMountOptions options)
        {
            // Set the options
            VesselMountOptions = options;

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region Screen Data Options

        /// <summary>
        /// Get the Screen options from the dictionary.  This will
        /// check if the subsystem configuration is in the hashset.
        /// If it is in the hashset, it will get the Screen options.
        /// If it is not, it will return a default configuration.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration to check.</param>
        /// <returns>Screen options.</returns>
        public ScreenSubsystemConfigOptions GetScreenOptions(SubsystemConfiguration ssConfig)
        {
            ScreenSubsystemConfigOptions options = null;
            if (SubsystemConfigList.Contains(ssConfig))
            {
                options = SubsysOptions[ssConfig.IndexCodeString()].ScreenOptions;
            }
            else
            {
                options = new ScreenSubsystemConfigOptions();
            }

            return options;
        }

        /// <summary>
        /// Save the Screen SubsystemConfiguration options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        /// <param name="options">Screen SubsystemConfiguration options.</param>
        public void SaveScreenOptions(SubsystemConfiguration ssConfig, ScreenSubsystemConfigOptions options)
        {
            // Check if the subsystem exist in the dictionary
            if (SubsystemConfigList.Contains(ssConfig))
            {
                // Store the graphical options to the SubsystemConfig entry
                SubsysOptions[ssConfig.IndexCodeString()].ScreenOptions = options;
            }
            else
            {
                // Add new subsystem configuration
                AddConfiguration(ssConfig);

                // Store the new options if the entry could be made
                if (SubsystemConfigList.Contains(ssConfig))
                {
                    // Store the graphical options to the SubsystemConfig entry
                    SubsysOptions[ssConfig.IndexCodeString()].ScreenOptions = options;
                }
            }

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region Average Data Options

        /// <summary>
        /// Get the Average options from the dictionary.  This will
        /// check if the subsystem configuration is in the hashset.
        /// If it is in the hashset, it will get the Average options.
        /// If it is not, it will return a default configuration.
        /// </summary>
        /// <param name="ssConfig">Subsystem configuration to check.</param>
        /// <returns>Screen options.</returns>
        public AverageSubsystemConfigOptions GetAverageOptions(SubsystemConfiguration ssConfig)
        {
            AverageSubsystemConfigOptions options = null;
            if (SubsystemConfigList.Contains(ssConfig))
            {
                options = SubsysOptions[ssConfig.IndexCodeString()].AverageOptions;
            }
            else
            {
                options = new AverageSubsystemConfigOptions();
            }

            return options;
        }

        /// <summary>
        /// Save the Average SubsystemConfiguration options to the project database.
        /// This will store the options so the user can retreive them when the project
        /// is loaded again.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        /// <param name="options">Average SubsystemConfiguration options.</param>
        public void SaveAverageOptions(SubsystemConfiguration ssConfig, AverageSubsystemConfigOptions options)
        {
            // Check if the subsystem exist in the dictionary
            if (SubsystemConfigList.Contains(ssConfig))
            {
                // Store the graphical options to the SubsystemConfig entry
                SubsysOptions[ssConfig.IndexCodeString()].AverageOptions = options;
            }
            else
            {
                // Add new subsystem configuration
                AddConfiguration(ssConfig);

                // Store the new options if the entry could be made
                if (SubsystemConfigList.Contains(ssConfig))
                {
                    // Store the graphical options to the SubsystemConfig entry
                    SubsysOptions[ssConfig.IndexCodeString()].AverageOptions = options;
                }
            }

            // Store the new options to the project DB
            SaveOptions();
        }

        #endregion

        #region Events

        #region Save Options

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void SaveOptionsDataEventHandler();

        /// <summary>
        /// Subscribe to receive event when this object needs to be stored to the project database.
        /// When changes have been made to the configuration, this event will be called.  It is up
        /// to the user if they want to save the options at the time.
        /// 
        /// To subscribe:
        /// pulseConfig.SaveOptionsDataEvent += new pulseConfig.SaveOptionsDataEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// pulseConfig.SaveOptionsDataEvent -= (method to call)
        /// </summary>
        public event SaveOptionsDataEventHandler SaveOptionsDataEvent;

        /// <summary>
        /// Publish the Save Options event.
        /// </summary>
        private void PublishSaveOptionsEvent()
        {
            if (SaveOptionsDataEvent != null)
            {
                SaveOptionsDataEvent();
            }
        }

        #endregion

        #endregion
    }
}
