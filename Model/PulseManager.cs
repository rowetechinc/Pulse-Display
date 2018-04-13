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
 * 05/07/2013      RC          3.0.0      Initial coding
 * 06/26/2013      RC          3.0.2      Changed name to PulseManager.
 * 07/26/2013      RC          3.0.6      Subscribe to the selected project to know when files are written to the project and binary file.
 * 08/12/2013      RC          3.0.7      Save Pulse options to the Project db.  Remove all methods that saved the options to the Pulse db.
 * 09/03/2013      RC          3.0.9      Display the projects is descending order in GetProjectList(). 
 * 10/15/2013      RC          3.2.0      Create methods to save and get Pulse options from the database.
 * 12/02/2013      RC          3.2.0      Added GetProject, GetProjectID and RemoveProject that take just the project name.
 * 12/27/2013      RC          3.2.1      Added method to get and set the last AdcpPredictorUserInput value.
 * 01/02/2014      RC          3.2.2      Get and set the Selected Project ID to the PulseOptions.
 * 01/16/2014      RC          3.2.3      Put a try/catch in GetPulseConfig() so if the configuration could not be properly converted from JSON, the default configuration would be used.
 * 08/15/2014      RC          4.0.0      Removed all the function calls associated with tblAdcpSubsystemConfigOptions.
 * 09/09/2014      RC          4.0.3      Added SelectedPlayback to pulsemanager to allow many sources of playback.
 * 04/15/2015      RC          4.1.2      Save AverageOptions to pulse db.
 * 06/23/2015      RC          4.1.3      Added TankTestOptions.
 * 08/13/2015      RC          4.2.0      Added Waves Options.
 * 12/02/2015      RC          4.4.0      Added, DataBit, Parity and Stop Bit to ADCP serial port.
 * 10/18/2017      RC          4.4.7      Added DataOutputViewOptions.
 * 03/28/2018      RC          4.8.1      Retreieve and save the DataFormatOptions.
 * 
 * 
 */

using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using ReactiveUI;
using log4net;
using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace RTI
{


    /// <summary>
    /// Get a list of all the projects.
    /// Select, add and remove projects 
    /// from the list.  The list of projects
    /// is stored in the Pulse Database.
    /// Store the application settings to the Pulse Database.
    /// </summary>
    public class PulseManager : ReactiveObject, IDeactivate, IDisposable
    {
        #region Variables

        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Current revision of the database.
        /// </summary>
        public const string PULSE_TABLE_REVISION = "G";

        /// <summary>
        /// Database connection to the Pulse database.
        /// </summary>
        private string _dbMainConnection;

        /// <summary>
        /// Event Aggregator.
        /// </summary>
        private IEventAggregator _events;

        #endregion

        #region Properties

        #region Selected Project

        /// <summary>
        /// Selected Project.
        /// </summary>
        private Project _SelectedProject;
        /// <summary>
        /// Selected Project.
        /// </summary>
        public Project SelectedProject
        {
            get { return _SelectedProject; }
            set 
            {
                if (_SelectedProject != value)
                {
                    // Unsubscribe from the events
                    if (_SelectedProject != null)
                    {
                        _SelectedProject.ProjectEnsembleWriteEvent -= SelectedProject_ProjectEnsembleWriteEvent;
                        _SelectedProject.BinaryEnsembleWriteEvent -= _SelectedProject_BinaryEnsembleWriteEvent;
                        AppConfiguration.SaveOptionsDataEvent -= AppConfiguration_SaveOptionsDataEvent;
                        _SelectedProject.Dispose();
                    }

                    // Set the new Selected project
                    this.RaiseAndSetIfChanged(ref _SelectedProject, value);

                    if (_SelectedProject != null)
                    {
                        // Subscribe to receive event when an ensemble has been written to the project
                        _SelectedProject.ProjectEnsembleWriteEvent += new Project.ProjectEnsembleWriteEventHandler(SelectedProject_ProjectEnsembleWriteEvent);
                        _SelectedProject.BinaryEnsembleWriteEvent += new Project.BinaryEnsembleWriteEventHandler(_SelectedProject_BinaryEnsembleWriteEvent);

                        GetPulseConfig();
                        AppConfiguration.SaveOptionsDataEvent += new PulseConfiguration.SaveOptionsDataEventHandler(AppConfiguration_SaveOptionsDataEvent);

                        // Set the selected project ID
                        UpdateSelectedProjectID(_SelectedProject.ProjectID);
                    }

                    // Publish the new project
                    _events.PublishOnUIThread(new ProjectEvent(_SelectedProject));
                }
            }
        }

        /// <summary>
        /// Determine if a project is selected.
        /// </summary>
        public bool IsProjectSelected
        {
            get 
            {
                return _SelectedProject != null;
            }
        }

        #endregion

        #region Selected Playback

        /// <summary>
        /// Selected Playback.
        /// </summary>
        private IPlayback _SelectedPlayback;
        /// <summary>
        /// Selected Playback.
        /// </summary>
        public IPlayback SelectedPlayback 
        {
            get { return _SelectedPlayback; } 
            set
            {
                // Close out the previous playback
                if(_SelectedPlayback != null)
                {
                    _SelectedPlayback.Dispose();
                    _SelectedPlayback = null;
                }

                _SelectedPlayback = value;

                // Publish the new project
                _events.PublishOnUIThread(new PlaybackEvent(_SelectedPlayback));
            }
        }

        /// <summary>
        /// Determine if a project is selected.
        /// </summary>
        public bool IsPlaybackSelected
        {
            get { return _SelectedPlayback != null; }
        }

        #endregion

        #region App Configuration

        /// <summary>
        /// Configuration for the selected project.
        /// When the project changes, this will also change.
        /// </summary>
        private PulseConfiguration _AppConfiguration;
        /// <summary>
        /// Configuration for the selected project.
        /// When the project changes, this will also change.
        /// </summary>
        public PulseConfiguration AppConfiguration
        {
            get { return _AppConfiguration; }
            set
            {
                _AppConfiguration = value;
                this.RaiseAndSetIfChanged(ref _AppConfiguration, value);
            }
        }

        #endregion

        #region Pulse Options

        /// <summary>
        /// Pulse options.  These are the generic
        /// options for the software.  These options
        /// are not particular to a project and are generally 
        /// loaded at the start of the application to get the
        /// last best good options.
        /// </summary>
        private PulseOptions _PulseOptions;

        #endregion

        #region Display ViewModels 

        /// <summary>
        /// List of all Display ViewModels.
        /// Ensembles will be passed to all the display VMs.  
        /// So register to receive ensemble data.
        /// </summary>
        List<DisplayViewModel> DisplayVmList { get; set; }

        #endregion

        #endregion

        #region Commands



        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public PulseManager()
        {
            _events = IoC.Get<IEventAggregator>();

            // Init list
            DisplayVmList = new List<DisplayViewModel>();

            // Create the Pulse database if it does not exist
            // and set the database connection
            CheckPulseDatabase();

            // Initialize the values
            _SelectedProject = null;
            SelectedPlayback = null;
            GetPulseConfig();

            // Get the Pulse options
            _PulseOptions = GetPulseOptions();

            // Deactivate
            AttemptingDeactivation = new EventHandler<DeactivationEventArgs>(PulseManager_AttemptingDeactivation);
            Deactivated = new EventHandler<DeactivationEventArgs>(PulseManager_Deactivated);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public void Dispose()
        {
            if (_SelectedProject != null)
            {
                // Unsubscribe
                _SelectedProject.ProjectEnsembleWriteEvent -= SelectedProject_ProjectEnsembleWriteEvent;
                _SelectedProject.BinaryEnsembleWriteEvent -= _SelectedProject_BinaryEnsembleWriteEvent;

                AppConfiguration.SaveOptionsDataEvent -= AppConfiguration_SaveOptionsDataEvent;

                // Dispose
                _SelectedProject.Dispose();
            }

            if (IsPlaybackSelected)
            {
                SelectedPlayback.Dispose();
            }
        }

        #region Pulse Database

        /// <summary>
        /// Check if the Pulse database exist.  If it does not exist
        /// create the database.   Also set the database connection.
        /// </summary>
        private void CheckPulseDatabase()
        {
            // Get the directory to store application data.
            string dir = Pulse.Commons.GetAppStorageDir();

            // Create full path to the main database
            string dbMainFullPath = dir + @"\" + Pulse.DbCommon.PULSE_DB_NAME;

            // Create the database connection
            _dbMainConnection = "Data Source=" + dbMainFullPath;

            // Check if the database exist
            // If it does not, create it
            if (!File.Exists(dbMainFullPath))
            {
                CreatePulseDatabase();
            }
        }

        /// <summary>
        /// Create the main table that will hold
        /// the projects and settings.
        /// </summary>
        private void CreatePulseDatabase()
        {
            // All the possible tables
            var commands = new[]
            {
                "CREATE TABLE tblProjects (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Dir TEXT NOT NULL, DateTimeCreated DATETIME NOT NULL, DateTimeLastModified DATETIME NOT NULL, SerialNumber TEXT, Misc TEXT)",
                "CREATE TABLE tblPulseSettings (ID INTEGER PRIMARY KEY AUTOINCREMENT, PulseOptions TEXT, Misc TEXT)",
                //"CREATE TABLE tblAdcpSubsystemConfigOptions (ID INTEGER PRIMARY KEY AUTOINCREMENT, ProjectID INTEGER, SubsystemConfig TEXT, AverageOptions TEXT, ScreenOptions TEXT, PlotOptions TEXT, TextOptions TEXT, TimeSeriesOptions TEXT, Misc TEXT, FOREIGN KEY(ProjectID) REFERENCES tblProjects(ID))",
                //"CREATE INDEX idxAdcpSubsystemSetting ON tblAdcpSubsystemConfigOptions(ProjectID, SubsystemConfig)",
                string.Format("INSERT INTO {0} ({1}) VALUES ({2});", Pulse.DbCommon.TBL_PULSE_SETTINGS, Pulse.DbCommon.COL_PULSE_OPTIONS, "''"),   // Put at least 1 entry so an insert does not have to be done later
            };

            // Create the tables using array above
            using (DbConnection cnn = new SQLiteConnection(_dbMainConnection))
            {
                // Open connection
                cnn.Open();

                foreach (var cmd in commands)
                {
                    using (DbCommand c = cnn.CreateCommand())
                    {
                        c.CommandText = cmd;
                        c.CommandType = System.Data.CommandType.Text;
                        c.ExecuteNonQuery();
                    }
                }

                // Close connection
                cnn.Close();
            }
        }

        #region Project

        /// <summary>
        /// Add the new project to the database.
        /// </summary>
        /// <param name="prj">Project to add.</param>
        /// <returns>Project object with ranges set.</returns>
        public void AddNewProject(Project prj)
        {
            // Get the ID for the project added
            // Insert the new project into the database
            // and return its ID
            int id = InsertNewProject(prj);

            // Create a new project object
            prj.ProjectID = id;
        }

        /// <summary>
        /// Remove the project from the main database.
        /// This will create a command to remove the project
        /// from the main database.  It will then run the
        /// command on the datase.
        /// </summary>
        /// <param name="projectName">Project name of project to remove.</param>
        public void RemoveProject(string projectName)
        {
            int projectID = GetProjectId(projectName);

            // Create the command to delete the project and ADCP settings
            string command = string.Format("DELETE FROM {0} WHERE {1}={2};",
                            Pulse.DbCommon.TBL_PULSE_PROJECTS, Pulse.DbCommon.COL_PRJ_ID, projectID.ToString());

            // Run the command on the main database
            Pulse.DbCommon.RunQueryOnPulseDb(_dbMainConnection, command);
        }

        /// <summary>
        /// Get a list of all the projects.
        /// </summary>
        /// <returns>List of all the projects.</returns>
        public List<Project> GetProjectList()
        {
            var list = new List<Project>();

            // Create a list of projects
            string query = string.Format("SELECT * FROM {0} ORDER BY datetime({1}) DESC;", Pulse.DbCommon.TBL_PULSE_PROJECTS, Pulse.DbCommon.COL_PRJ_DT_LAST_MOD);

            // Read in all the projects in the tb_Projects
            try
            {
                // Query the database for all the projects
                DataTable dt = Pulse.DbCommon.GetDataTableFromPulseDb(_dbMainConnection, query);

                // Go through the result, creating Project objects
                // for all projects found
                foreach (DataRow r in dt.Rows)
                {
                    // Get the Project info
                    int id = Convert.ToInt32(r[Pulse.DbCommon.COL_PRJ_ID]);
                    string name = r[Pulse.DbCommon.COL_PRJ_NAME].ToString();
                    string dir = r[Pulse.DbCommon.COL_PRJ_DIR].ToString();
                    DateTime dateCreated = (DateTime)r[Pulse.DbCommon.COL_PRJ_DT_CREATED];
                    DateTime dateMod = (DateTime)r[Pulse.DbCommon.COL_PRJ_DT_LAST_MOD];
                    string serial = r[Pulse.DbCommon.COL_PRJ_SERIALNUMBER].ToString();

                    Project prj = new Project(id, name, dir, dateCreated, dateMod, serial);

                    // Add the Project to the list
                    list.Add(prj);
                }

            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error adding project to database.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error adding project to database.", ex);
            }

            return list;
        }

        /// <summary>
        /// Insert a new project to the table of projects.
        /// It will set the creation and modified date to the
        /// current date and time.
        /// </summary>
        /// <param name="prj">Project to add to the database.</param>
        /// <returns>ID of the project row.</returns>
        private int InsertNewProject(Project prj)
        {
            int projectID = 0;
            try
            {
                // Open a connection to the database
                SQLiteConnection cnn = Pulse.DbCommon.OpenPulseDB(_dbMainConnection);

                using (DbTransaction dbTrans = cnn.BeginTransaction())
                {
                    using (DbCommand cmd = cnn.CreateCommand())
                    {
                        // Create the statement
                        cmd.CommandText = "INSERT INTO tblProjects(Name, Dir, DateTimeCreated, DateTimeLastModified, SerialNumber) VALUES(@name, @dir, @dateCreated, @dateMod, @serialNumber); SELECT last_insert_rowid();";

                        // Add all the parameters
                        cmd.Parameters.Add(new SQLiteParameter("@name", System.Data.DbType.String) { Value = prj.ProjectName });
                        cmd.Parameters.Add(new SQLiteParameter("@dir", System.Data.DbType.String) { Value = prj.ProjectDir });
                        cmd.Parameters.Add(new SQLiteParameter("@dateCreated", System.Data.DbType.DateTime) { Value = prj.DateCreated });
                        cmd.Parameters.Add(new SQLiteParameter("@dateMod", System.Data.DbType.DateTime) { Value = prj.LastDateModified });
                        cmd.Parameters.Add(new SQLiteParameter("@serialNumber", System.Data.DbType.String) { Value = prj.SerialNumber.ToString() });

                        //cmd.ExecuteNonQuery();
                        // After insert, request last row id
                        projectID = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    // Add all the data
                    dbTrans.Commit();
                }
                // Close the connection to the database
                cnn.Close();
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error adding project to database.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error adding project to database.", ex);
            }

            return projectID;
        }

        /// <summary>
        /// When the project is modified or played, change
        /// the Modify date for the project.
        /// </summary>
        /// <param name="project">Project to update modify time.</param>
        public void UpdateProjectModifyDate(Project project)
        {
            try
            {
                // Open a connection to the database
                SQLiteConnection cnn = Pulse.DbCommon.OpenPulseDB(_dbMainConnection);

                using (DbTransaction dbTrans = cnn.BeginTransaction())
                {
                    using (DbCommand cmd = cnn.CreateCommand())
                    {
                        // Create the statement
                        cmd.CommandText = String.Format("UPDATE {0} SET {1}=@dateMod WHERE ID={2};", Pulse.DbCommon.TBL_PULSE_PROJECTS, Pulse.DbCommon.COL_PRJ_DT_LAST_MOD, project.ProjectID.ToString());

                        // Add all the parameters
                        cmd.Parameters.Add(new SQLiteParameter("@dateMod", System.Data.DbType.DateTime) { Value = DateTime.Now });

                        cmd.ExecuteNonQuery();
                    }
                    // Add all the data
                    dbTrans.Commit();
                }
                // Close the connection to the database
                cnn.Close();
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error setting Modify date to project database.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error setting Modify date to project database.", ex);
            }
        }

        /// <summary>
        /// When a serial number is found for the project, this will
        /// add the serial number to the project.
        /// </summary>
        /// <param name="project">Project to update modify time.</param>
        public void UpdateProjectModifySerialNumber(Project project)
        {
            try
            {
                // Open a connection to the database
                SQLiteConnection cnn = Pulse.DbCommon.OpenPulseDB(_dbMainConnection);

                using (DbTransaction dbTrans = cnn.BeginTransaction())
                {
                    using (DbCommand cmd = cnn.CreateCommand())
                    {
                        // Create the statement
                        cmd.CommandText = String.Format("UPDATE {0} SET {1}=@serialNum WHERE ID={2};", Pulse.DbCommon.TBL_PULSE_PROJECTS, Pulse.DbCommon.COL_PRJ_SERIALNUMBER, project.ProjectID.ToString());

                        // Add all the parameters
                        cmd.Parameters.Add(new SQLiteParameter("@serialNum", System.Data.DbType.String) { Value = project.SerialNumber.ToString() });

                        cmd.ExecuteNonQuery();
                    }
                    // Add all the data
                    dbTrans.Commit();
                }
                // Close the connection to the database
                cnn.Close();
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error setting serial number to project database.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error setting serial number to project database.", ex);
            }
        }

        /// <summary>
        /// Get the project from the database based off the project name given.
        /// </summary>
        /// <param name="projectName">Project name.</param>
        /// <returns></returns>
        public Project GetProject(string projectName)
        {
            // Create a list of projects
            string query = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\";", Pulse.DbCommon.TBL_PULSE_PROJECTS, Pulse.DbCommon.COL_PRJ_NAME, projectName);

            // Read in all the projects in the tb_Projects
            Project prj = null;
            try
            {
                // Query the database for all the projects
                DataTable dt = Pulse.DbCommon.GetDataTableFromPulseDb(_dbMainConnection, query);

                // Go through the result, creating Project objects
                // for all projects found
                foreach (DataRow r in dt.Rows)
                {
                    // Get the Project info
                    int id = Convert.ToInt32(r[Pulse.DbCommon.COL_PRJ_ID]);
                    string name = r[Pulse.DbCommon.COL_PRJ_NAME].ToString();
                    string dir = r[Pulse.DbCommon.COL_PRJ_DIR].ToString();
                    DateTime dateCreated = (DateTime)r[Pulse.DbCommon.COL_PRJ_DT_CREATED];
                    DateTime dateMod = (DateTime)r[Pulse.DbCommon.COL_PRJ_DT_LAST_MOD];
                    string serial = r[Pulse.DbCommon.COL_PRJ_SERIALNUMBER].ToString();

                    prj = new Project(id, name, dir, dateCreated, dateMod, serial);
                }

            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error finding project in database.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error finding project in database.", ex);
            }

            return prj;
        }

        /// <summary>
        /// Get the project ID based off the project name given.
        /// </summary>
        /// <param name="projectName">Project Name.</param>
        /// <returns>Project ID.  0 means a project could not be found.</returns>
        public int GetProjectId(string projectName)
        {
            int projectID = 0;
            try
            {
                // Open a connection to the database
                SQLiteConnection cnn = Pulse.DbCommon.OpenPulseDB(_dbMainConnection);

                using (DbTransaction dbTrans = cnn.BeginTransaction())
                {
                    using (DbCommand cmd = cnn.CreateCommand())
                    {
                        // Create the statement
                        cmd.CommandText = String.Format("SELECT {0} FROM {1} WHERE {2}=\"{3}\";", Pulse.DbCommon.COL_PRJ_ID, Pulse.DbCommon.TBL_PULSE_PROJECTS, Pulse.DbCommon.COL_PRJ_NAME, projectName);

                        projectID = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    // Add all the data
                    dbTrans.Commit();
                }
                // Close the connection to the database
                cnn.Close();
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error setting serial number to project database.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error setting serial number to project database.", ex);
            }

            return projectID;
        }

        #endregion

        #region Text Options

        ///// <summary>
        ///// Update the Text Options for the given SubsystemConfiguration and project.
        ///// This will determine if the row already exist for a given Project and SubystemConfiguration.
        ///// If it does not exist, it will create a new row.  If the row does exist, it will update
        ///// the row with the latest average option.
        ///// </summary>
        ///// <param name="options">Options to update.</param>
        //public void UpdateTextSubsystemConfigurationOptions(TextSubsystemConfigOptions options)
        //{
        //    if (IsProjectSelected)
        //    {
        //        int rowId = GetAdcpSubsystemConfigOptionsRowId(_SelectedProject, options.SubsystemConfig);
        //        if (rowId > 0)
        //        {
        //            UpdateTextSubsystemConfigurationOptions(rowId, options);
        //        }
        //        else
        //        {
        //            AddTextSubsystemConfigOptions(_SelectedProject, options);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Add the Text options to the database for the given SubsystemConfiguration.
        ///// This will create a new row based off the project and SubsystemConfiguration.  It will
        ///// then add the new option data to the table.  If the table already contains a row for the
        ///// given Project and SubsystemConfiguration, this method should not be used, but the UpdateXXX()
        ///// method should be used.
        ///// </summary>
        ///// <param name="project">Project being updated.</param>
        ///// <param name="options">Options to update.</param>
        //private void AddTextSubsystemConfigOptions(Project project, TextSubsystemConfigOptions options)
        //{
        //    string ssConfigJSON = Newtonsoft.Json.JsonConvert.SerializeObject(options.SubsystemConfig);             // Serialize SubsystemConfiguration to JSON
        //    string optionsJSON = Newtonsoft.Json.JsonConvert.SerializeObject(options);                              // Serialize Options to JSON

        //    // Add a new row to the database with given Project ID, SubsystemConfiguration and option
        //    AddSubsystemConfigOptions(project, ssConfigJSON, optionsJSON, Pulse.DbCommon.COL_ADCP_SUB_CFG_TEXT_OPTIONS);
        //}

        ///// <summary>
        ///// If a settings row already exist for the project and SubsystemConfiguration,
        ///// update the row with the latest options.
        ///// </summary>
        ///// <param name="rowID">Row ID to update.</param>
        ///// <param name="options">Text SubsystemConfiguration options.</param>
        //private void UpdateTextSubsystemConfigurationOptions(int rowID, TextSubsystemConfigOptions options)
        //{
        //    // Convert the options to a JSON string
        //    string optionsJSON = Newtonsoft.Json.JsonConvert.SerializeObject(options);                                     // Serialize Options to JSON

        //    // Update the column with given json string
        //    UpdateSubsystemConfigurationOptions(rowID, optionsJSON, Pulse.DbCommon.COL_ADCP_SUB_CFG_TEXT_OPTIONS);
        //}

        ///// <summary>
        ///// Get the Text SubsystemConfiguration options from the Pulse Database.
        ///// This will use the Project and the SubsystemConfiguration to find the
        ///// Average options.  If the options are currently not stored in the 
        ///// database, then the default options will be returned.
        ///// </summary>
        ///// <param name="ssConfig">SubsystemConfiguration selected.</param>
        ///// <returns>Text options for the selected Project and SubsystemConfiguration.</returns>
        //public TextSubsystemConfigOptions GetTextSubsystemConfigurationOptions(SubsystemConfiguration ssConfig)
        //{
        //    if (IsProjectSelected)
        //    {
        //        // Get the options from the database
        //        string result = GetSubsystemConfigurationOptions(_SelectedProject, ssConfig, Pulse.DbCommon.COL_ADCP_SUB_CFG_TEXT_OPTIONS);

        //        // If the string returned was not empty, then an options was found in the database in JSON format
        //        // Convert the JSON to the object and return the object
        //        if (!string.IsNullOrEmpty(result))
        //        {
        //            return Newtonsoft.Json.JsonConvert.DeserializeObject<TextSubsystemConfigOptions>(result);
        //        }
        //    }

        //    // Nothing was found in the database so return a default object
        //    return new TextSubsystemConfigOptions(ssConfig);
        //}

        #endregion

        #endregion

        #region Pulse Configuration

        /// <summary>
        /// Get the Pulse configuration from the project.
        /// This will first check if a project exist.  If it does exist
        /// it will check if the project contains a configuration.  If
        /// it contains a configuration, it will read it as a JSON string
        /// and deserialize it.  It will then set the configuration.  
        /// 
        /// If no project is selected or no configuration found, it will
        /// use the default values.
        /// </summary>
        private void GetPulseConfig()
        {
            try
            {
                // When a project is selected
                // check if there is options already in the project
                if (SelectedProject != null)
                {
                    // Get the App Config from the project as a JSON string
                    string json = SelectedProject.GetAppConfigurationFromDb();

                    // Verify a string exist
                    if (!string.IsNullOrEmpty(json))
                    {
                        // Parse the JSON to an object
                        AppConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<PulseConfiguration>(json);
                    }
                    // Project did not contain a config
                    else
                    {
                        // Create a default config
                        AppConfiguration = new PulseConfiguration();
                    }
                }
                else
                {
                    // Create a default config
                    AppConfiguration = new PulseConfiguration();
                }
            }
            catch (Exception e)
            {
                log.Error("Error getting Application Configuration", e);
                AppConfiguration = new PulseConfiguration();
            }
        }

        /// <summary>
        /// Save the PulseConfig to the project database file.
        /// This will store the config if a project is selected.
        /// </summary>
        private void SavePulseConfig()
        {
            // Verify a project is selected
            if (SelectedProject != null && AppConfiguration != null)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(AppConfiguration);        // Serialize object to JSON
                SelectedProject.WriteAppConfiguration(json);                                        // Write to the project db
            }
        }

        #endregion

        #region Pulse Options

        #region ADCP COMM type

        /// <summary>
        /// Update the Pulse database with the latest Adcp COMM type.
        /// </summary>
        /// <param name="type">ADCP COMM type.</param>
        public void UpdateAdcpCommType(AdcpConnection.AdcpCommTypes type)
        {
            // Update the address and then update the DB
            _PulseOptions.AdcpCommType = type;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP COMM Type.
        /// </summary>
        /// <returns>Last ADCP COMM type.</returns>
        public AdcpConnection.AdcpCommTypes GetAdcpCommType()
        {
            return _PulseOptions.AdcpCommType;
        }

        #endregion

        #region Update Database

        /// <summary>
        /// Update teh pulse options with the latest options.
        /// The pulse options are the options that generic to the
        /// entire application.  Things like the last serial port used for the
        /// ADCP.
        /// </summary>
        /// <param name="options">Latest Pulse options.</param>
        public void UpdatePulseOptions(PulseOptions options)
        {
            try
            {
                // Open a connection to the database
                SQLiteConnection cnn = Pulse.DbCommon.OpenPulseDB(_dbMainConnection);

                using (DbTransaction dbTrans = cnn.BeginTransaction())
                {
                    using (DbCommand cmd = cnn.CreateCommand())
                    {
                        // Create the statement
                        cmd.CommandText = String.Format("UPDATE {0} SET {1}=@pulseOptions WHERE ID={2};", Pulse.DbCommon.TBL_PULSE_SETTINGS, Pulse.DbCommon.COL_PULSE_OPTIONS, 1);

                        // Set the parameter
                        cmd.Parameters.Add(new SQLiteParameter("@pulseOptions", System.Data.DbType.String) { Value = Newtonsoft.Json.JsonConvert.SerializeObject(options) });

                        cmd.ExecuteNonQuery();
                    }
                    // Add all the data
                    dbTrans.Commit();
                }
                // Close the connection to the database
                cnn.Close();
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error setting Modify pulse options.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error setting Modify pulse options.", ex);
            }
        }

        /// <summary>
        /// Get the Pulse options from the database.  This will read the database
        /// table for the Pulse options.  If one exist, it will pass it as a JSON string.
        /// If one does not exist, it will return an empty string.
        /// </summary>
        /// <returns>Pulse Options found in the Pulse DB file.</returns>
        private string GetPulseOptionsFromDb()
        {
            string options = "";

            string query = String.Format("SELECT * FROM {0} WHERE ID=1;", Pulse.DbCommon.TBL_PULSE_SETTINGS);
            try
            {
                // Query the database for the ADCP settings
                DataTable dt = Pulse.DbCommon.GetDataTableFromPulseDb(_dbMainConnection, query);

                // Go through the result settings the settings
                // If more than 1 result is found, return the first one found
                foreach (DataRow r in dt.Rows)
                {
                    // Check if there is data
                    if (r[Pulse.DbCommon.COL_PULSE_OPTIONS] == DBNull.Value)
                    {
                        break;
                    }

                    // Get the JSON string from the project
                    options = Convert.ToString(r[Pulse.DbCommon.COL_PULSE_OPTIONS]);


                    // Only read the first row
                    break;
                }
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error getting ADCP Configuration from the project.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error getting ADCP Configuration from the project.", ex);
            }

            return options;
        }

        /// <summary>
        /// Get the Pulse options from the database.
        /// If none exist, a default one will be created.
        /// </summary>
        public PulseOptions GetPulseOptions()
        {
            // Get the Pulse options as a JSON string
            string json = GetPulseOptionsFromDb();

            // Verify a string exist
            if (!string.IsNullOrEmpty(json))
            {
                // Parse the JSON to an object
                _PulseOptions = Newtonsoft.Json.JsonConvert.DeserializeObject<PulseOptions>(json);
            }
            // Database did not contain any options
            else
            {
                // Create a default options
                _PulseOptions = new PulseOptions();
            }

            return _PulseOptions;
        }

        #endregion

        #region ADCP Serial Port

        /// <summary>
        /// Update the Pulse database with the latest Adcp Serial port port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateAdcpSerialCommPort(string port)
        {
            // Update the port and then update the DB
            _PulseOptions.AdcpSerialOptions.Port = port;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP serial port comm port.
        /// </summary>
        /// <returns>Last ADCP Comm port used.</returns>
        public string GetAdcpSerialCommPort()
        {
            return _PulseOptions.AdcpSerialOptions.Port;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp Serial port baud rate.
        /// </summary>
        /// <param name="baud">Baudrate to update.</param>
        public void UpdateAdcpSerialBaudRate(int baud)
        {
            // Update the port and then update the DB
            _PulseOptions.AdcpSerialOptions.BaudRate = baud;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP serial port baud rate.
        /// </summary>
        /// <returns>Last used ADCP baud rate.</returns>
        public int GetAdcpSerialBaudRate()
        {
            return _PulseOptions.AdcpSerialOptions.BaudRate;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp Serial port Data bit.
        /// </summary>
        /// <param name="bit">Data Bit to update.</param>
        public void UpdateAdcpSerialDataBit(int bit)
        {
            // Update the port and then update the DB
            _PulseOptions.AdcpSerialOptions.DataBits = bit;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP serial port data bit.
        /// </summary>
        /// <returns>Last used ADCP data bit.</returns>
        public int GetAdcpSerialDataBit()
        {
            return _PulseOptions.AdcpSerialOptions.DataBits;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp Serial port Parity.
        /// </summary>
        /// <param name="parity">Parity to update.</param>
        public void UpdateAdcpSerialParity(System.IO.Ports.Parity parity)
        {
            // Update the port and then update the DB
            _PulseOptions.AdcpSerialOptions.Parity = parity;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP serial port Parity.
        /// </summary>
        /// <returns>Last used ADCP Parity.</returns>
        public System.IO.Ports.Parity GetAdcpSerialParity()
        {
            return _PulseOptions.AdcpSerialOptions.Parity;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp Serial port Stop bit.
        /// </summary>
        /// <param name="bit">Stop bit to update.</param>
        public void UpdateAdcpSerialStopBits(System.IO.Ports.StopBits bit)
        {
            // Update the port and then update the DB
            _PulseOptions.AdcpSerialOptions.StopBits = bit;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP serial port Stop bit.
        /// </summary>
        /// <returns>Last used ADCP Stop bit.</returns>
        public System.IO.Ports.StopBits GetAdcpSerialStopBits()
        {
            return _PulseOptions.AdcpSerialOptions.StopBits;
        }

        #endregion

        #region GPS 1

        /// <summary>
        /// Update the Pulse database with the latest GPS1 Serial port port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateGps1SerialCommPort(string port)
        {
            // Update the port and then update the DB
            _PulseOptions.Gps1SerialOptions.Port = port;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used GPS 1 serial port comm port.
        /// </summary>
        /// <returns>Last GPS 1 Comm port used.</returns>
        public string GetGps1SerialCommPort()
        {
            return _PulseOptions.Gps1SerialOptions.Port;
        }

        /// <summary>
        /// Update the Pulse database with the latest GPS1 Serial port baud rate.
        /// </summary>
        /// <param name="baud">Baudrate to update.</param>
        public void UpdateGps1SerialBaudRate(int baud)
        {
            // Update the port and then update the DB
            _PulseOptions.Gps1SerialOptions.BaudRate = baud;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used GPS1 serial port baud rate.
        /// </summary>
        /// <returns>Last used GPS1 baud rate.</returns>
        public int GetGps1SerialBaudRate()
        {
            return _PulseOptions.Gps1SerialOptions.BaudRate;
        }

        /// <summary>
        /// Update the Pulse options with the latest
        /// IsGps1SerialEnabled value.
        /// </summary>
        /// <param name="flag">Latest IsGps1SerialEnabled value.</param>
        public void UpdateIsGps1SerialEnabled(bool flag)
        {
            _PulseOptions.IsGps1SerialEnabled = flag;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used IsGps1SerialEnabled option.
        /// </summary>
        /// <returns>Last used IsGps1SerialEnabled value.</returns>
        public bool GetIsGps1SerialEnabled()
        {
            return _PulseOptions.IsGps1SerialEnabled;
        }

        #endregion

        #region GPS 2

        /// <summary>
        /// Update the Pulse database with the latest GPS2 Serial port port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateGps2SerialCommPort(string port)
        {
            // Update the port and then update the DB
            _PulseOptions.Gps2SerialOptions.Port = port;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used GPS 2 serial port comm port.
        /// </summary>
        /// <returns>Last GPS 2 Comm port used.</returns>
        public string GetGps2SerialCommPort()
        {
            return _PulseOptions.Gps2SerialOptions.Port;
        }

        /// <summary>
        /// Update the Pulse database with the latest GPS2 Serial port baud rate.
        /// </summary>
        /// <param name="baud">Baudrate to update.</param>
        public void UpdateGps2SerialBaudRate(int baud)
        {
            // Update the port and then update the DB
            _PulseOptions.Gps2SerialOptions.BaudRate = baud;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used GPS2 serial port baud rate.
        /// </summary>
        /// <returns>Last used GPS2 baud rate.</returns>
        public int GetGps2SerialBaudRate()
        {
            return _PulseOptions.Gps2SerialOptions.BaudRate;
        }

        /// <summary>
        /// Update the Pulse options with the latest
        /// IsGps2SerialEnabled value.
        /// </summary>
        /// <param name="flag">Latest IsGps2SerialEnabled value.</param>
        public void UpdateIsGps2SerialEnabled(bool flag)
        {
            _PulseOptions.IsGps2SerialEnabled = flag;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used IsGps2SerialEnabled option.
        /// </summary>
        /// <returns>Last used IsGps2SerialEnabled value.</returns>
        public bool GetIsGps2SerialEnabled()
        {
            return _PulseOptions.IsGps2SerialEnabled;
        }

        #endregion

        #region NMEA 1

        /// <summary>
        /// Update the Pulse database with the latest NMEA 1 Serial port port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateNmea1SerialCommPort(string port)
        {
            // Update the port and then update the DB
            _PulseOptions.Nmea1SerialOptions.Port = port;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used NMEA 1 serial port comm port.
        /// </summary>
        /// <returns>Last NMEA 1 Comm port used.</returns>
        public string GetNmea1SerialCommPort()
        {
            return _PulseOptions.Nmea1SerialOptions.Port;
        }

        /// <summary>
        /// Update the Pulse database with the latest NMEA 1 Serial port baud rate.
        /// </summary>
        /// <param name="baud">Baudrate to update.</param>
        public void UpdateNmea1SerialBaudRate(int baud)
        {
            // Update the port and then update the DB
            _PulseOptions.Nmea1SerialOptions.BaudRate = baud;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used NMEA 1 serial port baud rate.
        /// </summary>
        /// <returns>Last used NMEA 1 baud rate.</returns>
        public int GetNmea1SerialBaudRate()
        {
            return _PulseOptions.Nmea1SerialOptions.BaudRate;
        }

        /// <summary>
        /// Update the Pulse options with the latest
        /// IsNmea1SerialEnabled value.
        /// </summary>
        /// <param name="flag">Latest IsNmea1SerialEnabled value.</param>
        public void UpdateIsNmea1SerialEnabled(bool flag)
        {
            _PulseOptions.IsNmea1SerialEnabled = flag;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used IsNmea1SerialEnabled option.
        /// </summary>
        /// <returns>Last used IsNmea1SerialEnabled value.</returns>
        public bool GetIsNmea1SerialEnabled()
        {
            return _PulseOptions.IsNmea1SerialEnabled;
        }

        #endregion

        #region NMEA 2

        /// <summary>
        /// Update the Pulse database with the latest NMEA 2 Serial port port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateNmea2SerialCommPort(string port)
        {
            // Update the port and then update the DB
            _PulseOptions.Nmea2SerialOptions.Port = port;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used NMEA 2 serial port comm port.
        /// </summary>
        /// <returns>Last NMEA 2 Comm port used.</returns>
        public string GetNmea2SerialCommPort()
        {
            return _PulseOptions.Nmea2SerialOptions.Port;
        }

        /// <summary>
        /// Update the Pulse database with the latest NMEA 2 Serial port baud rate.
        /// </summary>
        /// <param name="baud">Baudrate to update.</param>
        public void UpdateNmea2SerialBaudRate(int baud)
        {
            // Update the port and then update the DB
            _PulseOptions.Nmea2SerialOptions.BaudRate = baud;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used NMEA 2 serial port baud rate.
        /// </summary>
        /// <returns>Last used NMEA 2 baud rate.</returns>
        public int GetNmea2SerialBaudRate()
        {
            return _PulseOptions.Nmea2SerialOptions.BaudRate;
        }

        /// <summary>
        /// Update the Pulse options with the latest
        /// IsNmea2SerialEnabled value.
        /// </summary>
        /// <param name="flag">Latest IsNmea2SerialEnabled value.</param>
        public void UpdateIsNmea2SerialEnabled(bool flag)
        {
            _PulseOptions.IsNmea2SerialEnabled = flag;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used IsNmea2SerialEnabled option.
        /// </summary>
        /// <returns>Last used IsNmea2SerialEnabled value.</returns>
        public bool GetIsNmea2SerialEnabled()
        {
            return _PulseOptions.IsNmea2SerialEnabled;
        }

        #endregion

        #region ADCP Ethernet

        /// <summary>
        /// Update the Pulse database with the latest Adcp ethernet address A.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressA(uint addr)
        {
            // Update the address and then update the DB
            _PulseOptions.EthernetOptions.IpAddrA = addr;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address A.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address A used.</returns>
        public uint GetEthernetIpAddressA()
        {
            return _PulseOptions.EthernetOptions.IpAddrA;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp ethernet address B.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressB(uint addr)
        {
            // Update the address and then update the DB
            _PulseOptions.EthernetOptions.IpAddrB = addr;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address B.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address B used.</returns>
        public uint GetEthernetIpAddressB()
        {
            return _PulseOptions.EthernetOptions.IpAddrB;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp ethernet address C.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressC(uint addr)
        {
            // Update the address and then update the DB
            _PulseOptions.EthernetOptions.IpAddrC = addr;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address C.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address C used.</returns>
        public uint GetEthernetIpAddressC()
        {
            return _PulseOptions.EthernetOptions.IpAddrC;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp ethernet address D.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressD(uint addr)
        {
            // Update the address and then update the DB
            _PulseOptions.EthernetOptions.IpAddrD = addr;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address D.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address D used.</returns>
        public uint GetEthernetIpAddressD()
        {
            return _PulseOptions.EthernetOptions.IpAddrD;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp ethernet port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateEthernetPort(uint port)
        {
            // Update the address and then update the DB
            _PulseOptions.EthernetOptions.Port = port;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port.
        /// </summary>
        /// <returns>Last ADCP ethernet port.</returns>
        public uint GetEthernetPort()
        {
            return _PulseOptions.EthernetOptions.Port;
        }

        #endregion

        #region ADCP Predictor User Input

        /// <summary>
        /// Update the Pulse database with the latest Adcp Predictor User Input.
        /// </summary>
        /// <param name="input">Adcp Predictor User Input.</param>
        public void UpdatePredictionModelInput(PredictionModelInput input)
        {
            // Update the ADCP Predictor User Input
            _PulseOptions.PredictorUserInput = input;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP Predictor User Input.
        /// </summary>
        /// <returns>Last ADCP Predictor User Input.</returns>
        public PredictionModelInput GetPredictionModelInput()
        {
            return _PulseOptions.PredictorUserInput;
        }

        #endregion

        #region Selected Project ID

        /// <summary>
        /// Update the Pulse database with the latest selected project ID.
        /// </summary>
        /// <param name="selectedProjectID">Selected Project ID.</param>
        public void UpdateSelectedProjectID(int selectedProjectID)
        {
            // Update the selected project ID
            _PulseOptions.SelectedProjectID = selectedProjectID;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last selected Project ID.
        /// </summary>
        /// <returns>Last selected Project ID.</returns>
        public int GetSelectedProjectID()
        {
            return _PulseOptions.SelectedProjectID;
        }

        #endregion

        #region Validation Test View Options

        /// <summary>
        /// Update the Pulse database with the latest Validation Test View Option.
        /// </summary>
        /// <param name="selectedProjectID">Validation Test View Option.</param>
        public void UpdateValidationViewOptions(ValidationTestViewOptions options)
        {
            // Update the selected project ID
            _PulseOptions.ValidationViewOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Validation Test View Option.
        /// </summary>
        /// <returns>Last Validation Test View Option.</returns>
        public ValidationTestViewOptions GetValidationViewOptions()
        {
            return _PulseOptions.ValidationViewOptions;
        }

        #endregion

        #region Graphical View Options

        /// <summary>
        /// Update the Pulse database with the latest Graphical View Option.
        /// </summary>
        /// <param name="selectedProjectID">Graphical View Option.</param>
        public void UpdateGraphicalViewOptions(ViewDataGraphicalOptions options)
        {
            // Update the selected project ID
            _PulseOptions.GraphicalViewOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Graphical View Option.
        /// </summary>
        /// <returns>Last Graphical View Option.</returns>
        public ViewDataGraphicalOptions GetGraphicalViewOptions()
        {
            return _PulseOptions.GraphicalViewOptions;
        }

        #endregion

        #region Backscatter Options

        /// <summary>
        /// Update the Pulse database with the latest Backscatter Options.
        /// </summary>
        /// <param name="options">Backscatter Options.</param>
        public void UpdateBackscatterOptions(BackscatterOptions options)
        {
            // Update the selected project ID
            _PulseOptions.BackscatterOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Backscatter Option.
        /// </summary>
        /// <returns>Last Backscatter Option.</returns>
        public BackscatterOptions GetBackscatterOptions()
        {
            return _PulseOptions.BackscatterOptions;
        }

        #endregion

        #region Average Options

        /// <summary>
        /// Update the Pulse database with the latest Average Options.
        /// </summary>
        /// <param name="options">Average Options.</param>
        public void UpdateAverageOptions(AverageOptions options)
        {
            // Update the selected project ID
            _PulseOptions.AverageOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Average Option.
        /// </summary>
        /// <returns>Last Average Option.</returns>
        public AverageOptions GetAverageOptions()
        {
            return _PulseOptions.AverageOptions;
        }

        #endregion

        #region Tank Test View Options

        /// <summary>
        /// Update the Pulse database with the latest Tank Test View Option.
        /// </summary>
        /// <param name="selectedProjectID">Tank Test View Option.</param>
        public void UpdateTankTestOptions(TankTestOptions options)
        {
            // Update the selected project ID
            _PulseOptions.TankTestOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Tank Test View Option.
        /// </summary>
        /// <returns>Last Tank Test View Option.</returns>
        public TankTestOptions GetTestTankOptions()
        {
            return _PulseOptions.TankTestOptions;
        }

        #endregion

        #region View Data Waves Options

        /// <summary>
        /// Update the option for the View Data Waves View Model.
        /// </summary>
        /// <param name="options">View Data Waves Options.</param>
        public void UpdateViewDataWavesOptions(ViewDataWavesOptions options)
        {
            // Update the ADCP Config
            _PulseOptions.ViewDataWavesOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used View Data Waves Options.
        /// </summary>
        /// <returns>Last used View Data Waves Options.</returns>
        public ViewDataWavesOptions GetViewDataWavesOptions()
        {
            return _PulseOptions.ViewDataWavesOptions;
        }

        #endregion

        #region ADCP Config

        /// <summary>
        /// Update the option with the latest config.
        /// </summary>
        /// <param name="baud">ADCP Config.</param>
        public void UpdateAdcpConfig(AdcpConfiguration config)
        {
            // Update the ADCP Config
            _PulseOptions.AdcpConfig = config;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used ADCP Configuration.
        /// </summary>
        /// <returns>Last used ADCP Config.</returns>
        public AdcpConfiguration GetAdcpConfig()
        {
            return _PulseOptions.AdcpConfig;
        }

        #endregion

        #region Last Viewed Page

        /// <summary>
        /// Update the option for the Last Viewed Page.
        /// </summary>
        /// <param name="id">Last Viewed Page ID.</param>
        public void UpdateLastViewedPage(ViewNavEvent.ViewId id)
        {
            // Update the ADCP Config
            _PulseOptions.LastViewedPage = id;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used Viewed Page ID.
        /// </summary>
        /// <returns>Last used Viewed Page ID.</returns>
        public ViewNavEvent.ViewId GetLastViewedPage()
        {
            return _PulseOptions.LastViewedPage;
        }

        #endregion

        #region Recover Data Options

        /// <summary>
        /// Update the option for the Recover Data View Model.
        /// </summary>
        /// <param name="options">Recover Data Options.</param>
        public void UpdateRecoverDataOptions(RecoverDataOptions options)
        {
            // Update the ADCP Config
            _PulseOptions.RecoverDataOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last used Recover Data Options.
        /// </summary>
        /// <returns>Last used Recover Data Options.</returns>
        public RecoverDataOptions GetRecoverDataOptions()
        {
            return _PulseOptions.RecoverDataOptions;
        }

        #endregion

        #region DataOutput View Options

        /// <summary>
        /// Update the Pulse database with the latest Graphical View Option.
        /// </summary>
        /// <param name="selectedProjectID">Graphical View Option.</param>
        public void UpdateDataOutputViewOptions(DataOutputViewOptions options)
        {
            // Update the selected project ID
            _PulseOptions.DataOutputOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Graphical View Option.
        /// </summary>
        /// <returns>Last Graphical View Option.</returns>
        public DataOutputViewOptions GetDataOutputViewOptions()
        {
            return _PulseOptions.DataOutputOptions;
        }

        #endregion

        #region WP Magnitude and Direction View Options

        /// <summary>
        /// Update the Pulse database with the latest Water Profile Magnitude and Direction View Option.
        /// </summary>
        /// <param name="options">Water Profile Magnitude and Direction View Option.</param>
        public void UpdateWpMagDirOutputViewOptions(WpMagDirOutputViewOptions options)
        {
            // Update the selected project ID
            _PulseOptions.WpMagDirOutputOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Graphical View Option.
        /// </summary>
        /// <returns>Last Graphical View Option.</returns>
        public WpMagDirOutputViewOptions GetWpMagDirOutputViewOptions()
        {
            return _PulseOptions.WpMagDirOutputOptions;
        }

        #endregion

        #region Data Format Options

        /// <summary>
        /// Update the Pulse database with the latest Data Format Option.
        /// </summary>
        /// <param name="options">Data Format Option.</param>
        public void UpdateDataFormatOptions(DataFormatOptions options)
        {
            // Update the selected project ID
            _PulseOptions.DataFormatOptions = options;
            UpdatePulseOptions(_PulseOptions);
        }

        /// <summary>
        /// Get the last Data Format Option.
        /// </summary>
        /// <returns>Last Data Format Option.</returns>
        public DataFormatOptions GetDataFormatOptions()
        {
            return _PulseOptions.DataFormatOptions;
        }

        #endregion

        #endregion

        #region Project Image

        /// <summary>
        /// Reproduce the project image.
        /// </summary>
        /// <param name="projectName">Project Name.</param>
        public void RefreshProjectImage(string projectName)
        {
            // Find the project in the database
            Project prj = GetProject(projectName);

            // Produce the image
            ProjectImage img = new ProjectImage(prj, 0.0, 2.0);
            img.GenerateImage(prj, 0.0, 2.0);
            img.Dispose();

            if (prj != null)
            {
                prj.Dispose();
            }
        }


        #endregion

        #region Set Selected Project

        /// <summary>
        /// Set the selected project.
        /// </summary>
        /// <param name="projectName">Project name for the project.</param>
        public void SetSelectedProject(string projectName)
        {
            // Set the selected project
            Project prj = GetProject(projectName);
            SelectedProject = prj;
            prj.Dispose();
        }

        #endregion

        #region VM Display List

        /// <summary>
        /// Register a display.
        /// </summary>
        /// <param name="vm"></param>
        public void RegisterDisplayVM(DisplayViewModel vm)
        {
            DisplayVmList.Add(vm);
        }

        /// <summary>
        /// Display the ensemble to all the View Models.
        /// </summary>
        /// <param name="ensEvent"></param>
        public void DisplayEnsemble(EnsembleEvent ensEvent)
        {
            for(int x =0; x < DisplayVmList.Count; x++)
            {
                DisplayVmList[x].Handle(ensEvent);
            }
        }

        /// <summary>
        /// Display all the bulk ensemble batches to all the view models.
        /// </summary>
        /// <param name="ensEvent"></param>
        public void DisplayEnsembleBulk(BulkEnsembleEvent ensEvent)
        {
            for (int x = 0; x < DisplayVmList.Count; x++)
            {
                DisplayVmList[x].Handle(ensEvent);
            }
        }

        #endregion

        #region Deactivate

        /// <summary>
        /// Deactivate Interface
        /// </summary>
        public event EventHandler<DeactivationEventArgs> AttemptingDeactivation;
        
        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PulseManager_AttemptingDeactivation(object sender, DeactivationEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        /// <param name="close"></param>
        public void Deactivate(bool close)
        {
            Dispose();
        }

        /// <summary>
        /// Deactivate Interface
        /// </summary>
        public event EventHandler<DeactivationEventArgs> Deactivated;

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PulseManager_Deactivated(object sender, DeactivationEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region EventHandler

        /// <summary>
        /// EventHandler when an ensemble has been written to a project.
        /// 
        /// Publish an event.
        /// </summary>
        /// <param name="count">Number of ensembles in the project.</param>
        void SelectedProject_ProjectEnsembleWriteEvent(long count)
        {
            // Publish the new project
            _events.PublishOnUIThread(new EnsembleWriteEvent(EnsembleWriteEvent.WriteLocation.Project, count));
        }

        /// <summary>
        /// EventHandler when an ensemble has been written to the binary file.
        /// 
        /// Publish an event.
        /// </summary>
        /// <param name="count">File size of the binary file in bytes.</param>
        void _SelectedProject_BinaryEnsembleWriteEvent(long count)
        {
            // Publish the new project
            _events.PublishOnUIThread(new EnsembleWriteEvent(EnsembleWriteEvent.WriteLocation.Binary, count));
        }

        /// <summary>
        /// When the SaveOptions event is received, save the
        /// PulseConfiguration to the project db.
        /// </summary>
        void AppConfiguration_SaveOptionsDataEvent()
        {
            SavePulseConfig();
        }

        #endregion
    }
}
