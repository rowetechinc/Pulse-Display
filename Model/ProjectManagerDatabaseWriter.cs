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
 * 09/30/2011      RC                     Initial coding
 * 10/04/2011      RC                     Changed STATUS in BT to INTEGER
 * 10/05/2011      RC                     Changed Depth to TransducerDepth
 * 10/12/2011      RC                     Changed REAL to FLOAT
 *                                         Added AdcpSetting ranges
 * 10/13/2011      RC                     Can now read and write to AdcpSettings
 * 10/18/2011      RC                     Adding Shutdown method
 *                                         Fixed bug saving settings.  Had quotes around ID.
 * 11/11/2011      RC                     Changed directory to use Environment.SpecialFolder.CommonApplicationData.
 *                                         Added saving and reading C232B and C485B to ADCP settings.
 * 11/15/2011      RC                     Removed IsDirectReading from Project table.
 *                                         Added UpdateProjectModifyDate().
 *                                         Removed GetCommSettings().
 * 12/09/2011      RC          1.09       Removed Nmea Table and added NmeaData to Ensemble table.
 * 12/14/2011      RC          1.09       Convert to Bool instead of cast in GetAdcpSettings().   
 * 12/19/2011      RC          1.10       Adding System Serial Number and Firmware to database.
 * 12/21/2011      RC          1.10       System.Data.SQLite 1.0.77 takes DateTime instead of DateTime.ToString().
 * 12/22/2011      RC          1.10       Create the Project folder in the ProjectViewModel.
 * 12/28/2011      RC          1.10       Changed EnsNum to EnsId in tblBeam and tblBottomTrack to ensure unique id for ensemble.
 * 12/29/2011      RC          1.11       Adding log and removing RecorderManager.
 * 12/30/2011      RC          1.11       Delete the ADCP settings when deleting the project.
 *                                         Create the project database in the project constructor now.
 * 01/03/2012      RC          1.11       Create the project in the ViewModel.  Add the Project ID to the project.
 *                                         Remove the Image blob from the project table.
 *                                         Added FontSize to settings.
 * 01/27/2012      RC          1.14        Added SerialNumber to Project database.
 * 02/06/2012      RC          2.00       Changed large string construction to string builder.
 *                                         Create method to write subsystem settings to database.
 *                                         Stop writting Setting row id to the project.
 * 02/07/2012      RC          2.00       Create a method to update the serial number for the project.
 *                                         Added C422B and Ensemble commands to the AdcpSettings table.
 *                                         Removed Frequency from AdcpSetting table.
 * 02/17/2012      RC          2.03       Add reading and writing Average and Screen options to the database.
 * 02/23/2012      RC          2.03       Add reading and writing Plot and Text options to database.
 * 02/24/2012      RC          2.03       Add CanUseBottomTrackVel and CanUseGpsVel to AverageScreen options.
 * 02/27/2012      RC          2.04       Set My Documents as default folder path for projects.
 * 03/05/2012      RC          2.05       Added IsScreenVelocityThreshold and ScreenVelocityThreshold to AverageScreenSubsystemOptions.
 * 03/06/2012      RC          2.05       Added Mark Bad Below Bottom.
 * 03/20/2012      RC          2.06       Added PlotSize3D and BinPlotRadius.  Renamed PlotSize to PlotSize2D.  Renamed GoodBeamTransform to GoodPingTransform.
 * 03/21/2012      RC          2.07       Changed GoodPingTransform to IsGoodPingEarth.
 *                                         Added BinHistoryMaxCount and IsBinHistoryRealtime to the database.
 * 05/21/2012      RC          2.11       Verify the serial port option is possible.
 * 07/03/2012      RC          2.12       Added last ADCP and GPS Comm settings to Settings table.
 *                                         Added methods to set and get the last ADCP and GPS Comm settings.
 * 08/16/2012      RC          2.13       For GenerateUpdateAdcpSettingString() and WriteNewAdcpSettings(), allow null values, so only parts of the settings can be added or set.
 * 08/21/2012      RC          2.13       In WriteNewAdcpSettings() if any options are null, create the option with the default values.
 * 08/28/2012      RC          2.13       Fixed bug in GenerateUpdateAdcpSettingString() where a comma was in the wrong spot.
 * 08/29/2012      RC          2.15       Added 2 columns to Project table in CreateMainDbTables().
 * 08/30/2012      RC          2.15       Changed the options for Adcp Settings and Subsystem Settings to a JSON object to store in the database.  Removed the AdcpSettings Table.  
 *                                         Changed the revision to G.
 * 09/04/2012      RC          2.15       In GetAdcpSettings(), use the last good ADCP/GPS serial options by default.  They are overwritten if options have already been saved.
 * 09/06/2012      RC          2.15       Removed deleting the project id row from the AdcpSettings table in RemoveProject().  The table does not exist anymore.
 * 09/07/2012      RC          2.15       Added DeploymentOptions to WriteAdcpSettings().
 * 09/13/2012      RC          2.15       In WriteAdcpSettings(), remove saving the serial port options for a project.
 * 09/17/2012      RC          2.15       Moved the AdcpCommands and AdcpSubsystemCommands to the project database.  Updated CreateMainDbTables().
 * 12/03/2012      RC          2.17       Added TimeSeriesOptions column to the Settings table.
 *                                         Added reading and writing settings to the TimeSeriesOptions column.
 *  01/15/2013     RC          2.17       Added Get/Set AverageSubsystemConfigurationOptions.
 *  01/16/2013     RC          2.17       Added Get/Set ScreenSubsystemConfigurationOptions.
 *                                         Added Get/Set PlotSubsystemConfigurationOptions.
 *                                         Added Get/Set TextSubsystemConfigurationOptions.
 *                                         Added Get/Set TimeSeriesSubsystemConfigurationOptions.
 *  01/17/2013     RC          2.17       Changed the DB index for the new SubsystemConfig table.
 *  08/15/2014     RC          4.0.0      Removed all the function calls associated with tblAdcpSubsystemConfigOptions.
 * 
 */

using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using RTI.Commands;
using log4net;
using System.Text;
using System.Collections.Generic;

namespace RTI
{
    /// <summary>
    /// Write and read to the application database.
    /// This is used to store projects and settings.
    /// </summary>
    public class ProjectManagerDatabaseWriter
    {
        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Current revision of the database.
        /// </summary>
        public const string PULSE_TABLE_REVISION = "G";

        /// <summary>
        /// Database connection to the application database.
        /// </summary>
        private string _dbMainConnection;

        /// <summary>
        /// Constructor
        /// 
        ///  Set the Directory for the application.
        ///  Then create the string to open the Main database.
        ///  Check if the main database exist.  If it does not,
        ///  then create the main database.
        /// </summary>
        public ProjectManagerDatabaseWriter()
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
                CreateMainDatabase();
            }
        }

        /// <summary>
        /// Close any objects created.
        /// </summary>
        public void Shutdown()
        {

        }

        #region Tables

        /// <summary>
        /// Create the main database.  This database
        /// will store all the projects and settings.
        /// </summary>
        private void CreateMainDatabase( )
        {
            // Create all the tables for the database
            CreateMainDbTables();
        }

        /// <summary>
        /// Create the main table that will hold
        /// the projects and settings.
        /// </summary>
        private void CreateMainDbTables()
        {
            // All the possible tables
            var commands = new[]
            {
                "CREATE TABLE tblProjects (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Dir TEXT NOT NULL, DateTimeCreated DATETIME NOT NULL, DateTimeLastModified DATETIME NOT NULL, SerialNumber TEXT, Misc TEXT)",
                "CREATE TABLE tblPulseSettings (ID INTEGER PRIMARY KEY AUTOINCREMENT, PulseOptions TEXT, Misc TEXT)",
                "CREATE TABLE tblAdcpSubsystemConfigOptions (ID INTEGER PRIMARY KEY AUTOINCREMENT, ProjectID INTEGER, SubsystemConfig TEXT, AverageOptions TEXT, ScreenOptions TEXT, PlotOptions TEXT, TextOptions TEXT, TimeSeriesOptions TEXT, Misc TEXT, FOREIGN KEY(ProjectID) REFERENCES tblProjects(ID))",
                "CREATE INDEX idxAdcpSubsystemSetting ON tblAdcpSubsystemConfigOptions(ProjectID, SubsystemConfig)",
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

        #endregion

        #region Project

        /// <summary>
        /// Add the new project to the database.  Create a project object and 
        /// return the object.  
        /// </summary>
        /// <param name="prj">Project to add.</param>
        /// <returns>Project object with ranges set.</returns>
        public void AddNewProject(ref Project prj)
        {
            // Get the ID for the project added
            // Insert the new project into the database
            // and return its ID
            int id = InsertNewProject(prj);

            // Create a new project object
            prj.ProjectID = id;
        }

        /// <summary>
        /// Return a list of projects in the database.
        /// </summary>
        /// <returns>A list of projects in the database.</returns>
        public ObservableCollectionEx<Project> GetProjectList()
        {
            // Create a list of projects
            ObservableCollectionEx<Project> list = new ObservableCollectionEx<Project>();
            string query = "SELECT * FROM tblProjects;";

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

        #endregion

        #region Pulse Options

        /// <summary>
        /// Set the pulse options.
        /// This will serialize the Pulse options into a JSON object.
        /// Then write the JSON object to the database.
        /// </summary>
        /// <param name="options">Pule Options.</param>
        public void SetPulseOptions(PulseOptions options)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(options);                                                                         // Serialize object to JSON
            string jsonCmd = String.Format("{0} = '{1}'", Pulse.DbCommon.COL_PULSE_OPTIONS, json);
            string query = String.Format("UPDATE {0} SET {1} WHERE ID=1;", Pulse.DbCommon.TBL_PULSE_SETTINGS, jsonCmd);                                 // Create query string.  ID should always equal 1, only the first row will be used

            Pulse.DbCommon.RunQueryOnPulseDb(_dbMainConnection, query);
        }

        /// <summary>
        /// Get the Pulse options from the database.
        /// This will read in the pulse options from the database.
        /// The options are stored as a JSON object.  The JSON object
        /// is then deserialized into a PulseOption object.
        /// 
        /// There should be only 1 row in the Pulse Options table, so read
        /// in all the rows and decode the first row if it contains any options.
        /// 
        /// If there are no options set yet in the database, then default options will
        /// be returned.
        /// </summary>
        /// <returns>Pulse Options from the database or default options.</returns>
        public PulseOptions GetPulseOptions()
        {
            PulseOptions options = new PulseOptions();

            string query = String.Format("SELECT * FROM {0};", Pulse.DbCommon.TBL_PULSE_SETTINGS);
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

                    // This will call the default constructor or pass to the constructor parameter a null
                    // The constructor parameter must be set after creating the object.
                    string json = Convert.ToString(r[Pulse.DbCommon.COL_PULSE_OPTIONS]);
                    if (!String.IsNullOrEmpty(json))
                    {
                        options = Newtonsoft.Json.JsonConvert.DeserializeObject<PulseOptions>(json);
                    }


                    // Only read the first row
                    break;
                }
            }
            catch (SQLiteException e)
            {
                log.Error("SQL Error getting Pulse Options.", e);
            }
            catch (Exception ex)
            {
                log.Error("Error getting Pulse Options.", ex);
            }

            return options;
        }

        #region Last Comm Settings

        #region ADCP

        /// <summary>
        /// Get the last ADCP Serial port options from the database.
        /// This is the last used settings in Pulse.
        /// </summary>
        /// <returns>Last used ADCP serial port options.</returns>
        public SerialOptions GetLastPulseAdcpSerialOptions()
        {
            // Get the pulse options from the database
            PulseOptions options = GetPulseOptions();

            return options.AdcpSerialOptions;            // Return ADCP serial options
        }

        /// <summary>
        /// Create a string of all the parameters and values for serial options.  Then set them in the database.
        /// </summary>
        /// <param name="adcpSerialOptions">Serial port options.</param>
        public void SetLastPulseAdcpSerialOptions(SerialOptions adcpSerialOptions)
        {
            // Get the current options from the database
            // Then replace the ADCP serial options with the new options.
            PulseOptions options = GetPulseOptions();
            if (adcpSerialOptions != null)
            {
                options.AdcpSerialOptions = adcpSerialOptions;
            }

            // Set the pulse options
            SetPulseOptions(options);
        }

        #endregion

        #region GPS 1

        /// <summary>
        /// Get the last GPS 1 Serial port options from the database.
        /// This is the last used settings in Pulse.
        /// </summary>
        /// <param name="isEnabled">Get flag if the port was enabled.</param>
        /// <returns>Last used GPS 1 serial port options.</returns>
        public SerialOptions GetLastPulseGps1SerialOptions(out bool isEnabled)
        {
            // Get the pulse options from the database
            PulseOptions options = GetPulseOptions();


            isEnabled = options.IsGps1SerialEnabled;     // Set the IsGpsEnabled
            return options.Gps1SerialOptions;            // Return GPS serial options

        }

        /// <summary>
        /// Create a string of all the parameters and values for GPS 1 serial options.  Then set them in the database.
        /// </summary>
        /// <param name="enableGpsSerial">Flag is GPS 1 serial port is enabled.</param>
        /// <param name="gpsSerialOptions">GPS 1 serial port options.</param>
        public void SetLastPulseGps1SerialOptions(bool enableGpsSerial, SerialOptions gpsSerialOptions)
        {
            // Get the current options from the database
            // Then replace the GPS serial options with the new options.
            PulseOptions options = GetPulseOptions();
            if (gpsSerialOptions != null)
            {
                options.IsGps1SerialEnabled = enableGpsSerial;
                options.Gps1SerialOptions = gpsSerialOptions;
            }

            // Set the pulse options
            SetPulseOptions(options);
        }

        #endregion

        #region GPS 2

        /// <summary>
        /// Get the last GPS 2 Serial port options from the database.
        /// This is the last used settings in Pulse.
        /// </summary>
        /// <param name="isEnabled">Get flag if the port was enabled.</param>
        /// <returns>Last used GPS 2 serial port options.</returns>
        public SerialOptions GetLastPulseGps2SerialOptions(out bool isEnabled)
        {
            // Get the pulse options from the database
            PulseOptions options = GetPulseOptions();


            isEnabled = options.IsGps2SerialEnabled;     // Set the IsGpsEnabled
            return options.Gps2SerialOptions;            // Return GPS serial options

        }

        /// <summary>
        /// Create a string of all the parameters and values for GPS 2 serial options.  Then set them in the database.
        /// </summary>
        /// <param name="enableGpsSerial">Flag is GPS 2 serial port is enabled.</param>
        /// <param name="gpsSerialOptions">GPS 2 serial port options.</param>
        public void SetLastPulseGps2SerialOptions(bool enableGpsSerial, SerialOptions gpsSerialOptions)
        {
            // Get the current options from the database
            // Then replace the GPS serial options with the new options.
            PulseOptions options = GetPulseOptions();
            if (gpsSerialOptions != null)
            {
                options.IsGps2SerialEnabled = enableGpsSerial;
                options.Gps2SerialOptions = gpsSerialOptions;
            }

            // Set the pulse options
            SetPulseOptions(options);
        }

        #endregion

        #region NMEA 1

        /// <summary>
        /// Get the last NMEA 1 Serial port options from the database.
        /// This is the last used settings in Pulse.
        /// </summary>
        /// <param name="isEnabled">Get flag if the port was enabled.</param>
        /// <returns>Last used NMEA 1 serial port options.</returns>
        public SerialOptions GetLastPulseNmea1SerialOptions(out bool isEnabled)
        {
            // Get the pulse options from the database
            PulseOptions options = GetPulseOptions();


            isEnabled = options.IsNmea1SerialEnabled;     // Set the IsNmea1SerialEnabled
            return options.Nmea1SerialOptions;            // Return NMEA 1 serial options

        }

        /// <summary>
        /// Create a string of all the parameters and values for NMEA 1 serial options.  Then set them in the database.
        /// </summary>
        /// <param name="enableNmeaSerial">Flag is NMEA 1 serial port is enabled.</param>
        /// <param name="nmeaSerialOptions">NMEA 1 serial port options.</param>
        public void SetLastPulseNmea1SerialOptions(bool enableNmeaSerial, SerialOptions nmeaSerialOptions)
        {
            // Get the current options from the database
            // Then replace the NMEA serial options with the new options.
            PulseOptions options = GetPulseOptions();
            if (nmeaSerialOptions != null)
            {
                options.IsNmea1SerialEnabled = enableNmeaSerial;
                options.Nmea1SerialOptions = nmeaSerialOptions;
            }

            // Set the pulse options
            SetPulseOptions(options);
        }

        #endregion

        #region NMEA 2

        /// <summary>
        /// Get the last NMEA 2 Serial port options from the database.
        /// This is the last used settings in Pulse.
        /// </summary>
        /// <param name="isEnabled">Get flag if the port was enabled.</param>
        /// <returns>Last used NMEA 2 serial port options.</returns>
        public SerialOptions GetLastPulseNmea2SerialOptions(out bool isEnabled)
        {
            // Get the pulse options from the database
            PulseOptions options = GetPulseOptions();


            isEnabled = options.IsNmea2SerialEnabled;     // Set the IsNmea2SerialEnabled
            return options.Nmea2SerialOptions;            // Return NMEA 2 serial options

        }

        /// <summary>
        /// Create a string of all the parameters and values for NMEA 2 serial options.  Then set them in the database.
        /// </summary>
        /// <param name="enableNmeaSerial">Flag is NMEA 2 serial port is enabled.</param>
        /// <param name="nmeaSerialOptions">NMEA 2 serial port options.</param>
        public void SetLastPulseNmea2SerialOptions(bool enableNmeaSerial, SerialOptions nmeaSerialOptions)
        {
            // Get the current options from the database
            // Then replace the NMEA serial options with the new options.
            PulseOptions options = GetPulseOptions();
            if (nmeaSerialOptions != null)
            {
                options.IsNmea2SerialEnabled = enableNmeaSerial;
                options.Nmea2SerialOptions = nmeaSerialOptions;
            }

            // Set the pulse options
            SetPulseOptions(options);
        }

        #endregion

        #endregion

        #endregion
    }
}