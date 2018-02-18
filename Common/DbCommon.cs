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
 * 01/11/2012      RC          1.12       Initial coding
 * 01/27/2012      RC          1.14       Added SerialNumber column to Project database.
 * 02/06/2012      RC          2.00       Added Subsystem and ADCP User Guide Rev F changes.
 * 02/07/2012      RC          2.00       Added Pulse Settings variables.
 * 02/21/2012      RC          2.03       Added Screen and Average variables.
 * 02/22/2012      RC          2.03       Added Text and Plot options.
 * 02/23/2012      RC          2.03       Added Reference Layer Min/Max.
 * 02/24/2012      RC          2.03       Added CanUseBottomTrackVel and CanUseGpsVel.
 * 03/05/2012      RC          2.05       Added COL_ADCP_SUB_SCREEN_IS_SCREEN_VEL_THRESHOLD and COL_ADCP_SUB_SCREEN_SCREEN_VEL_THRESHOLD
 * 03/06/2012      RC          2.05       Added Mark Bad Below Bottom.
 * 03/20/2012      RC          2.06       Added PlotSize3D and BinPlotRadius.  Renamed PlotSize to PlotSize2D.  Renamed GoodBeamTransform to GoodPingTransform.
 * 03/21/2012      RC          2.07       Changed GoodPingTransform to IsGoodPingEarth.
 *                                         Added BinHistoryMaxCount and IsBinHistoryRealtime to the database.
 * 07/03/2012      RC          2.12       Added Last ADCP and GPS Comm settings to Pulse Table to load default settings at startup.
 *                                         Added Revision column to the Settings table
 * 09/06/2012      RC          2.15       Removed TBL_PULSE_ADCP_SETTINGS.
 * 09/17/2012      RC          2.15       Removed COL_ADCP_SUB_COMMANDS and COL_PRJ_ADCP_OPTIONS.
 * 12/03/2012      RC          2.17       Added COL_ADCP_SUB_TIMESERIES_OPTIONS.
 * 01/15/2013      RC          2.17       Added COL_ADCP_SUB_CFG_XXX variables.
 * 01/17/2013      RC          2.17       Removed the COL_ADCP_SUB_XXX variables and replaced them with COL_ADCP_SUB_CFG_XXX variables.
 * 08/15/2014      RC          4.0.0      Removed TBL_PULSE_ADCP_SUBSYSTEM_CONFIG_OPTIONS.
 * 
 */

using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using log4net;


namespace RTI
{
    namespace Pulse
    {
        /// <summary>
        /// Common Database commands used to 
        /// read and write to the database used
        /// by Pulse.
        /// </summary>
        public class DbCommon
        {

            #region Variables

            /// <summary>
            /// Logger for logging error messages.
            /// </summary>
            private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            #region Tables 

            /// <summary>
            /// Pulse Database:
            /// Database name.
            /// </summary>
            public const string PULSE_DB_NAME = "PulseDb.db";

            /// <summary>
            /// Pulse Main Database:
            /// Project table name.  Table to hold all the projects.  
            /// </summary>
            public const string TBL_PULSE_PROJECTS = "tblProjects";

            /// <summary>
            /// Pulse Database:
            /// Pulse Settings table name.  Table to hold all settings per project.
            /// </summary>
            public const string TBL_PULSE_SETTINGS = "tblPulseSettings";

            #endregion

            #region Columns

            #region Pulse Settings

            /// <summary>
            /// Pulse Settings Table
            /// Pulse Options as a JSON object.
            /// (TEXT)
            /// </summary>
            public const string COL_PULSE_OPTIONS = "PulseOptions";

            /// <summary>
            /// Pulse Settings Table
            /// Pulse Misc as a JSON object.
            /// (TEXT)
            /// </summary>
            public const string COL_PULSE_MISC = "Misc";

            /// <summary>
            /// Pulse Settings Table
            /// Revision of the database.
            /// (TEXT)
            /// 
            /// Revision of the database.
            /// </summary>
            public const string COL_PULSE_REVISION = "Revision";

            /// <summary>
            /// Pulse Settings Table
            /// Project Folder Path
            /// (TEXT)
            /// 
            /// Last folder path used by the user for a project.
            /// </summary>
            public const string COL_PULSE_PROJECT_DIR = "PrjFolderPath";

            /// <summary>
            /// Pulse Settings Table
            /// Project Folder Path
            /// (TEXT)
            /// 
            /// Font size used in the text display.
            /// </summary>
            public const string COL_PULSE_FONT_SIZE = "FontSize";

            /// <summary>
            /// Pulse Settings Table
            /// Max binary file size.
            /// (TEXT)
            /// 
            /// Maximum file size for a binary file.  When the file size
            /// reaches the maximum, a new file is created.
            /// </summary>
            public const string COL_PULSE_MAX_FILE_SIZE = "MaxFileSize";

            #region Last ADCP COMM Settings

            /// <summary>
            /// Pulse Setting Table:
            /// Last used ADCP Serial Port.
            /// (TEXT)
            /// </summary>
            public const string COL_PULSE_LAST_ADCP_PORT = "AdcpPort";

            /// <summary>
            /// Pulse Setting Table:
            /// Last used ADCP Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_PULSE_LAST_ADCP_BAUD = "AdcpBaudrate";

            /// <summary>
            /// Pulse Setting Table:
            /// Last used ADCP Data Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_PULSE_LAST_ADCP_DATABIT = "AdcpDataBits";

            /// <summary>
            /// ADCP Setting Table:
            /// Last used ADCP Parity.
            /// (TINYINT)
            /// </summary>
            public const string COL_PULSE_LAST_ADCP_PARITY = "AdcpParity";

            /// <summary>
            /// Pulse Setting Table:
            /// Last used ADCP Stop Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_PULSE_LAST_ADCP_STOPBIT = "AdcpStopBits";

            #endregion

            #region Last GPS COMM Settings

            /// <summary>
            /// Pulse Setting Table:
            /// GPS Serial Port Enabled.
            /// (BOOLEAN)
            /// </summary>
            public const string COL_PULSE_LAST_GPS_ENABLE = "GpsEnable";

            /// <summary>
            /// Pulse Setting Table:
            /// GPS Serial Port.
            /// (TEXT)
            /// </summary>
            public const string COL_PULSE_LAST_GPS_PORT = "GpsPort";

            /// <summary>
            /// Pulse Setting Table:
            /// GPS Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_PULSE_LAST_GPS_BAUD = "GpsBaudrate";

            /// <summary>
            /// Pulse Setting Table:
            /// GPS Data Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_PULSE_LAST_GPS_DATABIT = "GpsDataBits";

            /// <summary>
            /// Pulse Setting Table:
            /// GPS Parity.
            /// (TINYINT)
            /// </summary>
            public const string COL_PULSE_LAST_GPS_PARITY = "GpsParity";

            /// <summary>
            /// Pulse Setting Table:
            /// GPS Stop Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_PULSE_LAST_GPS_STOPBIT = "GpsStopBits";

            #endregion

            #endregion

            #region Project Settings

            /// <summary>
            /// Project Table:
            /// Project ID Column.
            /// (INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)
            /// </summary>
            public const string COL_PRJ_ID = "ID";

            /// <summary>
            /// Project Table:
            /// Project Name.
            /// (TEXT NOT NULL)
            /// </summary>
            public const string COL_PRJ_NAME = "Name";

            /// <summary>
            /// Project Table:
            /// Project Directory.
            /// (TEXT NOT NULL)
            /// </summary>
            public const string COL_PRJ_DIR = "Dir";

            /// <summary>
            /// Project Table:
            /// Project DateTime project was created.
            /// (DATETIME NOT NULL)
            /// </summary>
            public const string COL_PRJ_DT_CREATED = "DateTimeCreated";

            /// <summary>
            /// Project Table:
            /// Project DateTime project was last modified.
            /// (DATETIME NOT NULL)
            /// </summary>
            public const string COL_PRJ_DT_LAST_MOD = "DateTimeLastModified";

            /// <summary>
            /// Project Table:
            /// Project serial number.
            /// (TEXT NOT NULL)
            /// </summary>
            public const string COL_PRJ_SERIALNUMBER = "SerialNumber";

            /// <summary>
            /// Project Table:
            /// Misc JSON object.
            /// (TEXT)
            /// </summary>
            public const string COL_PRJ_ADCP_MISC = "Misc";

            #endregion

            #region ADCP Settings

            /// <summary>
            /// ADCP Setting Table:
            /// Table ID.  
            /// (INTEGER PRIMARY KEY AUTOINCREMENT) 
            /// </summary>
            public const string COL_ADCP_ID = "ID";

            /// <summary>
            /// ADCP Setting Table:
            /// Project ID associated with these settings.
            /// (INTEGER)
            /// FOREIGN KEY(ProjectID) REFERENCES tblProjects(ID)
            /// </summary>
            public const string COL_ADCP_PROJECT_ID = "ProjectID";

            #region Ensemble

            /// <summary>
            /// ADCP Setting Table:
            /// Frequency of the ADCP.
            /// (INTEGER)
            /// </summary>
            public const string COL_ADCP_FREQUENCY = "Frequency";

            /// <summary>
            /// ADCP Setting Table:
            /// ADCP Mode.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_MODE = "Mode";

            /// <summary>
            /// ADCP Setting Table:
            /// Ensemble interval Hour.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CEI_HOUR = "CEI_Hour";

            /// <summary>
            /// ADCP Setting Table:
            /// Ensemble interval Minute.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CEI_MINUTE = "CEI_Minute";

            /// <summary>
            /// ADCP Setting Table:
            /// Ensemble interval Second.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CEI_SECOND = "CEI_Second";

            /// <summary>
            /// ADCP Setting Table:
            /// Ensemble interval Hundredth of a second.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CEI_HUNSEC = "CEI_HunSec";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Year
            /// (SMALLINT)
            /// </summary>
            public const string COL_ADCP_CETFP_YEAR = "CETFP_Year";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Month
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CETFP_MONTH = "CETFP_Month";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Day
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CETFP_DAY = "CETFP_Day";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Hour
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CETFP_HOUR = "CETFP_Hour";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Minute
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CETFP_MINUTE = "CETFP_Minute";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Second
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CETFP_SEC = "CETFP_Second";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Time of First Ping. Hundredth of second.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CETFP_HUNSEC = "CETFP_HunSec";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Recording.
            /// (BOOLEAN)
            /// </summary>
            public const string COL_ADCP_CERECORD = "CERECORD";

            /// <summary>
            /// ADCP Settings Table
            /// Ensemble Ping Order.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_CEPO = "CEPO";

            #endregion

            #region Environmental

            /// <summary>
            /// ADCP Setting Table:
            /// Salinity.
            /// (FLOAT)
            /// </summary>
            public const string COL_ADCP_CWS = "CWS";

            /// <summary>
            /// ADCP Setting Table:
            /// Water Temp.
            /// (FLOAT)
            /// </summary>
            public const string COL_ADCP_CWT = "CWT";

            /// <summary>
            /// ADCP Setting Table:
            /// Transducer Depth.
            /// (FLOAT)
            /// </summary>
            public const string COL_ADCP_CTD = "CTD";

            /// <summary>
            /// ADCP Setting Table:
            /// Speed of Sound.
            /// (FLOAT)
            /// </summary>
            public const string COL_ADCP_CWSS = "CWSS";

            /// <summary>
            /// ADCP Setting Table:
            /// Heading Source.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_CHS = "CHS";

            /// <summary>
            /// ADCP Setting Table:
            /// Heading Offset.
            /// (FLOAT)
            /// </summary>
            public const string COL_ADCP_CHO = "CHO";

            #endregion

            #region COMM Port

            /// <summary>
            /// ADCP Setting Table:
            /// RS-232 Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_ADCP_C232B = "C232B";

            /// <summary>
            /// ADCP Setting Table:
            /// RS-485 Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_ADCP_C485B = "C485B";

            /// <summary>
            /// ADCP Setting Table:
            /// RS-422 Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_ADCP_C422B = "C422B";

            #endregion

            #region ADCP COMM Settings

            /// <summary>
            /// ADCP Setting Table:
            /// ADCP Serial Port.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_ADCP_PORT = "AdcpPort";

            /// <summary>
            /// ADCP Setting Table:
            /// ADCP Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_ADCP_ADCP_BAUD = "AdcpBaudrate";

            /// <summary>
            /// ADCP Setting Table:
            /// ADCP Data Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_ADCP_DATABIT = "AdcpDataBits";

            /// <summary>
            /// ADCP Setting Table:
            /// ADCP Parity.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_ADCP_PARITY = "AdcpParity";

            /// <summary>
            /// ADCP Setting Table:
            /// ADCP Stop Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_ADCP_STOPBIT = "AdcpStopBits";

            #endregion

            #region GPS COMM Settings

            /// <summary>
            /// ADCP Setting Table:
            /// GPS Serial Port Enabled.
            /// (BOOLEAN)
            /// </summary>
            public const string COL_ADCP_GPS_ENABLE = "GpsEnable";

            /// <summary>
            /// ADCP Setting Table:
            /// GPS Serial Port.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_GPS_PORT = "GpsPort";

            /// <summary>
            /// ADCP Setting Table:
            /// GPS Baudrate.
            /// (INTEGER)
            /// </summary>
            public const string COL_ADCP_GPS_BAUD = "GpsBaudrate";

            /// <summary>
            /// ADCP Setting Table:
            /// GPS Data Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_GPS_DATABIT = "GpsDataBits";

            /// <summary>
            /// ADCP Setting Table:
            /// GPS Parity.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_GPS_PARITY = "GpsParity";

            /// <summary>
            /// ADCP Setting Table:
            /// GPS Stop Bits.
            /// (TINYINT)
            /// </summary>
            public const string COL_ADCP_GPS_STOPBIT = "GpsStopBits";

            #endregion

            #endregion

            #region ADCP Subsystem Configuration Options

            /// <summary>
            /// ADCP Setting Table:
            /// Table ID.  
            /// (INTEGER PRIMARY KEY AUTOINCREMENT) 
            /// </summary>
            public const string COL_ADCP_SUB_CFG_ID = "ID";

            /// <summary>
            /// ADCP Subsystem Setting Table:
            /// Project ID associated with these settings.
            /// (INTEGER)
            /// FOREIGN KEY(ProjectID) REFERENCES tblProjects(ID)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_PROJECT_ID = "ProjectID";

            /// <summary>
            /// ADCP Subsystem Configuration Options Table:
            /// Column to hold JSON object for the Subsystem Configuration to identify the row.
            /// (BYTE)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_SUBSYSTEM_CONFIG = "SubsystemConfig";

            /// <summary>
            /// ADCP Subsystem Configuration Options Table:
            /// Column to hold JSON object for the subsystem configuration Plot View Options.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_PLOT_OPTIONS = "PlotOptions";

            /// <summary>
            /// ADCP Subsystem Configuration Options Table:
            /// Column to hold JSON object for the subsystem configuration Text View Options.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_TEXT_OPTIONS = "TextOptions";

            /// <summary>
            /// ADCP Subsystem Configuration Options Table:
            /// Column to hold JSON object for the subsystem configuration TimeSeries View Options.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_TIMESERIES_OPTIONS = "TimeSeriesOptions";

            /// <summary>
            /// ADCP Subsystem Configuration Options Table:
            /// Column to hold JSON object for the subsystem configuration Screen View Options.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_SCREEN_OPTIONS = "ScreenOptions";

            /// <summary>
            /// ADCP Configuration Options Table:
            /// Column to hold JSON object for the subsystem configuration Average View Options.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_AVG_OPTIONS = "AverageOptions";

            /// <summary>
            /// ADCP Subsystem Configuration Options Table:
            /// Column to hold JSON object for the subsystem configuration misc values.
            /// (TEXT)
            /// </summary>
            public const string COL_ADCP_SUB_CFG_MISC = "Misc";

            #endregion

            #endregion

            #endregion

            /// <summary>
            /// Constructor.
            /// Does nothing.  All methods static.
            /// </summary>
            public DbCommon()
            {
            
            }


            #region Open Database

            /// <summary>
            /// Open a connection to the database.  Check for any errors.
            /// </summary>
            /// <param name="dbConn">Database path.</param>
            /// <returns>A connection to the database.</returns>
            public static SQLiteConnection OpenPulseDB(string dbConn)
            {
                try
                {
                    var conn = new SQLiteConnection(dbConn);
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    return conn;
                }

                catch (SQLiteException e)
                {
                    log.Error("Error opening Main database.", e);
                    return null;
                }
                catch (Exception e)
                {
                    log.Error("Unknown Error opening Main database.", e);
                    return null;
                }
            }

            #endregion

            #region Query

            /// <summary>
            /// This query returns nothing.  It it will just run the query
            /// given as a string on the Pulse database.
            /// </summary>
            /// <param name="dbConnection">Database connection string.</param>
            /// <param name="query">Query to send to the database.</param>
            public static void RunQueryOnPulseDb(string dbConnection, string query)
            {
                try
                {
                    // Open a connection to the database
                    using (SQLiteConnection cnn = DbCommon.OpenPulseDB(dbConnection))
                    {
                        // If a connection could not be made, do nothing
                        if (cnn == null)
                        {
                            return;
                        }

                        using (DbTransaction dbTrans = cnn.BeginTransaction())
                        {
                            using (DbCommand cmd = cnn.CreateCommand())
                            {
                                // Create the statement
                                cmd.CommandText = query;

                                cmd.ExecuteNonQuery();
                            }
                            // Add all the data
                            dbTrans.Commit();
                        }
                        // Close the connection to the database
                        cnn.Close();
                    }
                }
                catch (SQLiteException e)
                {
                    log.Error(String.Format("Error running query on PulseDb: {0}", query), e);
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Unknown Error running query on PulseDb: {0}", query), e);
                }
            }

            /// <summary>
            /// This query will return a string.  Run the query and get a return value
            /// from the query.
            /// </summary>
            /// <param name="dbConnection">Database connection string.</param>
            /// <param name="query">Query to send to the database.</param>
            /// <returns>String value from the database.</returns>
            public static string GetValueOnPulseDb(string dbConnection, string query)
            {
                string result = "";
                try
                {
                    // Open a connection to the database
                    using (SQLiteConnection cnn = DbCommon.OpenPulseDB(dbConnection))
                    {
                        // If a connection could not be made, do nothing
                        if (cnn == null)
                        {
                            return result;
                        }

                        using (DbTransaction dbTrans = cnn.BeginTransaction())
                        {
                            using (DbCommand cmd = cnn.CreateCommand())
                            {
                                // Create the statement
                                cmd.CommandText = query;

                                result = Convert.ToString(cmd.ExecuteScalar());
                            }
                            // Add all the data
                            dbTrans.Commit();
                        }
                        // Close the connection to the database
                        cnn.Close();
                    }
                }
                catch (SQLiteException e)
                {
                    log.Error(String.Format("Error getting value on PulseDb: {0}", query), e);
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Unknown error getting value on PulseDb: {0}", query), e);
                }

                return result;
            }

            /// <summary>
            /// Allows the programmer to run a query against the Pulse Database
            /// and return a table with the result.
            /// </summary>
            /// <param name="dbConnection">Database connection to main database.</param>
            /// <param name="query">The SQL Query to run</param>
            /// <returns>A DataTable containing the result set.</returns>
            public static DataTable GetDataTableFromPulseDb(string dbConnection, string query)
            {
                DataTable dt = new DataTable();
                try
                {
                    // Open a connection to the database
                    using (SQLiteConnection cnn = Pulse.DbCommon.OpenPulseDB(dbConnection))
                    {
                        // If a connection could not be made, do nothing
                        if (cnn == null)
                        {
                            throw new Exception("Database Connection could not be made to PulseDb");
                        }

                        //using (DbTransaction dbTrans = cnn.BeginTransaction())
                        //{
                        //    using (DbCommand cmd = cnn.CreateCommand())
                        //    {
                        //        cmd.CommandText = query;
                        //        DbDataReader reader = cmd.ExecuteReader();

                        //        // Load the datatable with query result
                        //        dt.Load(reader);

                        //        // Close the connection
                        //        reader.Close();
                        //        cnn.Close();
                        //    }
                        //}

                        using (var command = new SQLiteCommand(query, cnn))
                        {
                            //command.Connection.Open();
                            SQLiteDataReader reader = command.ExecuteReader();

                            // Load the datatable with query result
                            dt.Load(reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                return dt;
            }

            #endregion
        }


    }
}
