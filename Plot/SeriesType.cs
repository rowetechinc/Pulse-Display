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
 * 11/28/2012      RC          2.17       Initial coding.
 * 12/07/2012      RC          2.17       Added BaseSeriesType.
 * 07/16/2013      RC          3.0.4      Changed SeriesType.
 * 10/08/2014      RC          4.1.0      Added Base_Velocity to eBaseSeriesType.
 * 03/02/2015      RC          4.1.0      Added SystemSetup and Range Tracking.
 * 11/24/2015      RC          4.3.1      Added Magnitude and Direction and Speed.
 * 11/25/2015      RC          4.3.1      Added NMEA Heading and speed.
 * 12/04/2015      RC          4.4.0      Added DVL data to TimeSeries.  This includes Ship Velocity.
 * 
 */

namespace RTI
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    #region DataSource

    /// <summary>
    /// This is a list of all the data sources available.
    /// </summary>
    public class DataSource
    {
        #region Variables

        #region Titles

        /// <summary>
        /// The title for Profile
        /// </summary>
        public const string TITLE_PROFILE = "Profile";

        /// <summary>
        /// The title for Bottom Track.
        /// </summary>
        public const string TITLE_BT = "Bottom Track";

        /// <summary>
        /// The title for Water Track
        /// </summary>
        public const string TITLE_WT = "Water Track";

        /// <summary>
        /// The title for Ancillary Profile.
        /// </summary>
        public const string TITLE_ANCILLARY_PROFILE = "Ancillary Profile";

        /// <summary>
        /// The title for Ancillary Bottom Track.
        /// </summary>
        public const string TITLE_ANCILLARY_BT = "Ancillary Bottom Track";

        /// <summary>
        /// The title for the Range Tracking.
        /// </summary>
        public const string TITLE_RANGETRACKING = "Range Tracking";

        /// <summary>
        /// The title for the System Setup.
        /// </summary>
        public const string TITLE_SYSTEMSETUP = "System Setup";

        /// <summary>
        /// The title for the Waves.
        /// </summary>
        public const string TITLE_WAVES = "Waves";

        /// <summary>
        /// The title for the NMEA.
        /// </summary>
        public const string TITLE_NMEA = "NMEA";

        /// <summary>
        /// The title for the DVL.
        /// </summary>
        public const string TITLE_DVL = "DVL";

        #endregion

        #endregion

        #region Enum

        /// <summary>
        /// Enum of all the different types of base series.
        /// This will also give a code for the series type.
        /// </summary>
        public enum eSource
        {
            /// <summary>
            /// Profile data.
            /// </summary>
            WaterProfile,

            /// <summary>
            /// Bottom Track data.
            /// </summary>
            BottomTrack,

            /// <summary>
            /// Water Track data.
            /// </summary>
            WaterTrack,

            /// <summary>
            /// Ancillary Data for Profile data.
            /// </summary>
            AncillaryWaterProfile,

            /// <summary>
            /// Ancillary Data for Bottom Track data.
            /// </summary>
            AncillaryBottomTrack,

            /// <summary>
            /// Range Tracking data.
            /// </summary>
            RangeTracking,

            /// <summary>
            /// System Setup data.
            /// </summary>
            SystemSetup,

            /// <summary>
            /// Waves Calculations
            /// </summary>
            Waves,

            /// <summary>
            /// NMEA data.
            /// </summary>
            NMEA,

            /// <summary>
            /// DVL data.
            /// </summary>
            DVL,
            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Data source. This will be where the data is coming from.
        /// </summary>
        public eSource Source { get; set; }

        /// <summary>
        /// The Title for the source type.
        /// </summary>
        public string Title { get; set; }



        #endregion

        /// <summary>
        /// Initialize the Data Source.
        /// </summary>
        /// <param name="source">Base Series code.</param>
        public DataSource(eSource source)
        {
            // Set the values
            Source = source;
            Title = GetTitle(source);
        }

        #region Titles

        /// <summary>
        /// Get the description for the source type based off
        /// the enum value given.
        /// </summary>
        /// <param name="type">Enum value to determine the type.</param>
        /// <returns>Description of the source.</returns>
        public static string GetTitle(eSource type)
        {
            switch (type)
            {
                case eSource.WaterProfile:
                    return TITLE_PROFILE;
                case eSource.BottomTrack:
                    return TITLE_BT;
                case eSource.WaterTrack:
                    return TITLE_WT;
                case eSource.AncillaryWaterProfile:
                    return TITLE_ANCILLARY_PROFILE;
                case eSource.AncillaryBottomTrack:
                    return TITLE_ANCILLARY_BT;
                case eSource.RangeTracking:
                    return TITLE_RANGETRACKING;
                case eSource.SystemSetup:
                    return TITLE_SYSTEMSETUP;
                case eSource.Waves:
                    return TITLE_WAVES;
                case eSource.NMEA:
                    return TITLE_NMEA;
                case eSource.DVL:
                    return TITLE_DVL;
                default:
                    return "";
            }
        }

        #endregion

        #region List

        /// <summary>
        /// Create a list of all the data sources.
        /// </summary>
        /// <returns>List of all the data sources.</returns>
        public static BindingList<DataSource> GetDataSourceList()
        {
            var list = new BindingList<DataSource>();
            list.Add(new DataSource(eSource.WaterProfile));
            list.Add(new DataSource(eSource.BottomTrack));
            list.Add(new DataSource(eSource.WaterTrack));
            list.Add(new DataSource(eSource.AncillaryWaterProfile));
            list.Add(new DataSource(eSource.AncillaryBottomTrack));
            list.Add(new DataSource(eSource.RangeTracking));
            list.Add(new DataSource(eSource.SystemSetup));
            list.Add(new DataSource(eSource.NMEA));
            list.Add(new DataSource(eSource.Waves));
            list.Add(new DataSource(eSource.DVL));

            return list;
        }

        #endregion

        #region Override

        /// <summary>
        /// Return the description as the string for this object.
        /// </summary>
        /// <returns>Return the description as the string for this object.</returns>
        public override string ToString()
        {
            return Title;
        }

        /// <summary>
        /// Determine if the 2 series given are the equal.
        /// </summary>
        /// <param name="code1">First series to check.</param>
        /// <param name="code2">Series to check against.</param>
        /// <returns>True if there codes match.</returns>
        public static bool operator ==(DataSource code1, DataSource code2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(code1, code2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)code1 == null) || ((object)code2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (code1.Source == code2.Source);
        }

        /// <summary>
        /// Return the opposite of ==.
        /// </summary>
        /// <param name="code1">First series to check.</param>
        /// <param name="code2">Series to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(DataSource code1, DataSource code2)
        {
            return !(code1 == code2);
        }

        /// <summary>
        /// Create a hashcode based off the Code stored.
        /// </summary>
        /// <returns>Hash the Code.</returns>
        public override int GetHashCode()
        {
            return Source.GetHashCode();
        }

        /// <summary>
        /// Check if the given object is 
        /// equal to this object.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>If the codes are the same, then they are equal.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            DataSource p = (DataSource)obj;

            return (Source == p.Source);
        }

        #endregion
    }

    #endregion

    #region Base Series Type

    /// <summary>
    /// This describes the base series types.
    /// The difference between a base series and a series type is the
    /// base series does not differentiate between Water Profile, Bottom Track
    /// or Water Track.
    /// </summary>
    public class BaseSeriesType
    {
        #region Variables

        #region Base Series Types

        /// <summary>
        /// The title for the Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_VEL_BEAM = "Velocity - Beam";

        /// <summary>
        /// The title for the Velocity Instrument Coordiante system.
        /// </summary>
        public const string TITLE_VEL_XYZ = "Velocity - Instrument";

        /// <summary>
        /// The title for the Velocity Earth Coordiante system.
        /// </summary>
        public const string TITLE_VEL_ENU = "Velocity - Earth";

        /// <summary>
        /// The title for the Velocity Ship Coordiante system.
        /// </summary>
        public const string TITLE_VEL_SHIP = "Velocity - Ship";

        /// <summary>
        /// The title for the Amplitude data.
        /// </summary>
        public const string TITLE_AMPLITUDE = "Amplitude";

        /// <summary>
        /// The title for the Correlation data.
        /// </summary>
        public const string TITLE_CORRELATION = "Correlation";

        /// <summary>
        /// The title for the SNR data.
        /// </summary>
        public const string TITLE_SNR = "Signal to Noise Ratio (SNR)";

        /// <summary>
        /// The title for the Bottom Track Range data.
        /// </summary>
        public const string TITLE_RANGE = "Range";

        /// <summary>
        /// The title for the Heading data.
        /// </summary>
        public const string TITLE_HEADING = "Heading";

        /// <summary>
        /// The title for the Pitch data.
        /// </summary>
        public const string TITLE_PITCH = "Pitch";

        /// <summary>
        /// The title for the Roll data.
        /// </summary>
        public const string TITLE_ROLL = "Roll";

        /// <summary>
        /// The title for the Water Temperature data.
        /// </summary>
        public const string TITLE_TEMP_WATER = "Water Temperature";

        /// <summary>
        /// The title for the System Temperature data.
        /// </summary>
        public const string TITLE_TEMP_SYS = "System Temperature";

        /// <summary>
        /// The title for the Pressure data.
        /// </summary>
        public const string TITLE_PRESSURE = "Pressure";

        /// <summary>
        /// The title for the Transducer Depth Pressure data.
        /// </summary>
        public const string TITLE_TRANSDUCER_DEPTH = "Transducer Depth";

        /// <summary>
        /// The title for the BT Speed.
        /// </summary>
        public const string TITLE_SPEED = "Speed";

        /// <summary>
        /// The title for the Range Tracking Signal to noise ratio data.
        /// This will be used to measure vertical height in Waves
        /// system for vertical beams.
        /// </summary>
        public const string TITLE_RANGETRACKING_SNR = "SNR";

        /// <summary>
        /// The title for the Range Tracking Range data.
        /// This will be used to measure vertical height in Waves
        /// system for vertical beams.
        /// </summary>
        public const string TITLE_RANGETRACKING_RANGE = "Range";

        /// <summary>
        /// The title for the Range Tracking number of pings.
        /// This will be used to measure vertical height in Waves
        /// system for vertical beams.
        /// </summary>
        public const string TITLE_RANGETRACKING_PINGS = "Pings";

        /// <summary>
        /// The title for the System Setup voltage.
        /// </summary>
        public const string TITLE_SYSTEMSETUP_VOLTAGE = "Voltage";

        /// <summary>
        /// The title for the Water Velocity Magnitude.
        /// </summary>
        public const string TITLE_WATER_MAGNITUDE = "Water Velocity Magnitude";

        /// <summary>
        /// The title for the Water Velocity Direction.
        /// </summary>
        public const string TITLE_WATER_DIRECTION = "Water Velocity Direction";

        /// <summary>
        /// The title for the Water East Velocity.
        /// </summary>
        public const string TITLE_VEL_ENU_EAST = "Water East Velocity";

        /// <summary>
        /// The title for the Water North Velocity.
        /// </summary>
        public const string TITLE_VEL_ENU_NORTH = "Water North Velocity";

        /// <summary>
        /// The title for the Water Vertical Velocity.
        /// </summary>
        public const string TITLE_VEL_ENU_VERTICAL = "Water Vertical Velocity";

        /// <summary>
        /// The title for the Waves Frequency.
        /// </summary>
        public const string TITLE_WAVES_FREQUENCY = "Waves Frequency";

        /// <summary>
        /// The title for the Waves Period.
        /// </summary>
        public const string TITLE_WAVES_PERIOD = "Waves Period";

        /// <summary>
        /// The title for the Waves East Velocity.
        /// </summary>
        public const string TITLE_WAVES_EAST_VEL = "East Velocity";

        /// <summary>
        /// The title for the Waves North Velocity.
        /// </summary>
        public const string TITLE_WAVES_NORTH_VEL = "North Velocity";

        /// <summary>
        /// The title for the Waves Pressure and Height.
        /// </summary>
        public const string TITLE_WAVES_PRESSURE_HEIGHT = "Pressure and Height";

        /// <summary>
        /// The title for the Waves FFT.
        /// </summary>
        public const string TITLE_WAVES_FFT = "Waves FFT";

        /// <summary>
        /// The title for the Waves Spectrum.
        /// </summary>
        public const string TITLE_WAVES_SPECTRUM = "Uncorrected Subsurface Energy Spectrum";

        /// <summary>
        /// The title for the Waves Wave Set.
        /// </summary>
        public const string TITLE_WAVES_WAVE_SET = "Wave Set";

        /// <summary>
        /// The title for the Waves Sensor Set.
        /// </summary>
        public const string TITLE_WAVES_SENSOR_SET = "Sensor Set";

        /// <summary>
        /// The title for the Waves Velocity Series.
        /// </summary>
        public const string TITLE_WAVES_VELOCITY_SERIES = "Velocity Series";

        /// <summary>
        /// NMEA heading.
        /// </summary>
        public const string TITLE_NMEA_HEADING = "NMEA Heading";

        /// <summary>
        /// NMEA Speed.
        /// </summary>
        public const string TITLE_NMEA_SPEED = "NMEA Speed";



        #endregion

        #endregion

        #region Enum

        /// <summary>
        /// Enum of all the different types of base series.
        /// This will also give a code for the series type.
        /// </summary>
        public enum eBaseSeriesType
        {
            /// <summary>
            /// Base Series Type.
            /// Velocity in Beam Coordinate Transform.
            /// </summary>
            Base_Velocity_Beam,

            /// <summary>
            /// Base Series Type.
            /// Velocity in Instrument Coordinate Transform.
            /// </summary>
            Base_Velocity_XYZ,

            /// <summary>
            /// Base Series Type.
            /// Velocity in Earth Coordinate Transform.
            /// </summary>
            Base_Velocity_ENU,

            /// <summary>
            /// Base Series Type.
            /// Velocity in Ship Coordinate Transform.
            /// </summary>
            Base_Velocity_Ship,

            /// <summary>
            /// Base Series Type.
            /// Amplitude data.
            /// </summary>
            Base_Amplitude,

            /// <summary>
            /// Base Series Type.
            /// Correlation data.
            /// </summary>
            Base_Correlation,

            /// <summary>
            /// Base Series Type.
            /// Signal To Noise Ratio data.
            /// </summary>
            Base_SNR,

            /// <summary>
            /// Base Series Type.
            /// Range data.
            /// </summary>
            Base_Range,

            /// <summary>
            /// Base Series Type.
            /// Heading.
            /// </summary>
            Base_Heading,

            /// <summary>
            /// Base Series Type.
            /// Pitch.
            /// </summary>
            Base_Pitch,

            /// <summary>
            /// Base Series Type.
            /// Pitch.
            /// </summary>
            Base_Roll,

            /// <summary>
            /// Base Series Type.
            /// Water Temperature data.
            /// </summary>
            Base_Temperature_Water,

            /// <summary>
            /// Base Series Type.
            /// System Temperature data.
            /// </summary>
            Base_Temperature_Sys,

            /// <summary>
            /// Base Series Type.
            /// Pressure data.
            /// </summary>
            Base_Pressure,

            /// <summary>
            /// Base Series Type.
            /// Transducer Depth data.
            /// This is pressure data in meters.
            /// </summary>
            Base_TransducerDepth,

            /// <summary>
            /// Base Series Type.
            /// Speed of the boat data.
            /// </summary>
            Base_Speed,

            /// <summary>
            /// Base Series Type.
            /// Ranging Tracking Signal to noise ratio.
            /// </summary>
            Base_RangeTracking_SNR,

            /// <summary>
            /// Base Series Type.
            /// Ranging Tracking Range.
            /// </summary>
            Base_RangeTracking_Range,

            /// <summary>
            /// Base Series Type.
            /// Ranging Tracking number of pings.
            /// </summary>
            Base_RangeTracking_Pings,

            /// <summary>
            /// Base Series Type.
            /// System Setup Voltage.
            /// </summary>
            Base_SystemSetup_Voltage,

            /// <summary>
            /// Base Series Type.
            /// Water Earth Velocity magnitude.
            /// </summary>
            Base_Water_Magnitude,

            /// <summary>
            /// Base Series Type.
            /// Water Earth Velocity Direction.
            /// </summary>
            Base_Water_Direction,

            /// <summary>
            /// Base Series Type.
            /// Velocity in Earth Coordinate Transform East Velocity.
            /// </summary>
            Base_Velocity_ENU_East,

            /// <summary>
            /// Base Series Type.
            /// Velocity in Earth Coordinate Transform North Velocity.
            /// </summary>
            Base_Velocity_ENU_North,

            /// <summary>
            /// Base Series Type.
            /// Velocity in Earth Coordinate Transform Vertical Velocity.
            /// </summary>
            Base_Velocity_ENU_Vertical,

            /// <summary>
            /// Base Series Type.
            /// Waves frequency.
            /// </summary>
            Base_Waves_Frequency,

            /// <summary>
            /// Base Series Type.
            /// Waves Period.
            /// </summary>
            Base_Waves_Period,

            /// <summary>
            /// Base Series Type.
            /// Waves East Velocity.
            /// </summary>
            Base_Waves_East_Vel,

            /// <summary>
            /// Base Series Type.
            /// Waves North Velocity.
            /// </summary>
            Base_Waves_North_Vel,

            /// <summary>
            /// Base Series Type.
            /// Waves Pressure and Height.
            /// </summary>
            Base_Waves_Pressure_And_Height,

            /// <summary>
            /// Base Series Type.
            /// Waves FFT.
            /// </summary>
            Base_Waves_FFT,

            /// <summary>
            /// Base Series Type.
            /// Waves Spectrum.
            /// </summary>
            Base_Waves_Spectrum,

            /// <summary>
            /// Base Series Type.
            /// Waves Wave Set.
            /// </summary>
            Base_Waves_Wave_Set,

            /// <summary>
            /// Base Series Type.
            /// Waves Sensor Set.
            /// </summary>
            Base_Waves_Sensor_Set,

            /// <summary>
            /// Base Series Type.
            /// Waves Velocity series.
            /// </summary>
            Base_Waves_Velocity_Series,

            /// <summary>
            /// Base Series Type.
            /// NMEA Heading.
            /// </summary>
            Base_NMEA_Heading,

            /// <summary>
            /// Base Series Type.
            /// NMEA Speed.
            /// </summary>
            Base_NMEA_Speed,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Base Series type code.  This is a unique
        /// value to identify the type of series.
        /// </summary>
        public eBaseSeriesType Code { get; set; }

        /// <summary>
        /// The Title for the series type.
        /// </summary>
        public string Title { get; set; }



        #endregion

        /// <summary>
        /// Initialize the Base Series Type.
        /// </summary>
        /// <param name="baseType">Base Series code.</param>
        public BaseSeriesType(eBaseSeriesType baseType)
        {
            // Set the values
            Code = baseType;
            Title = GetTitle(baseType);
        }

        #region Titles
        
        /// <summary>
        /// Get the description for the series type based off
        /// the enum value given.
        /// </summary>
        /// <param name="type">Enum value to determine the type.</param>
        /// <returns>Description of the series.</returns>
        public static string GetTitle(eBaseSeriesType type)
        {
            switch (type)
            {
                case eBaseSeriesType.Base_Velocity_Beam:
                    return TITLE_VEL_BEAM;
                case eBaseSeriesType.Base_Velocity_XYZ:
                    return TITLE_VEL_XYZ;
                case eBaseSeriesType.Base_Velocity_ENU:
                    return TITLE_VEL_ENU;
                case eBaseSeriesType.Base_Velocity_Ship:
                    return TITLE_VEL_SHIP;
                case eBaseSeriesType.Base_Amplitude:
                    return TITLE_AMPLITUDE;
                case eBaseSeriesType.Base_Correlation:
                    return TITLE_CORRELATION;
                case eBaseSeriesType.Base_SNR:
                    return TITLE_SNR;
                case eBaseSeriesType.Base_Range:
                    return TITLE_RANGE;
                case eBaseSeriesType.Base_Heading:
                    return TITLE_HEADING;
                case eBaseSeriesType.Base_Pitch:
                    return TITLE_PITCH;
                case eBaseSeriesType.Base_Roll:
                    return TITLE_ROLL;
                case eBaseSeriesType.Base_Temperature_Sys:
                    return TITLE_TEMP_SYS;
                case eBaseSeriesType.Base_Temperature_Water:
                    return TITLE_TEMP_WATER;
                case eBaseSeriesType.Base_Pressure:
                    return TITLE_PRESSURE;
                case eBaseSeriesType.Base_TransducerDepth:
                    return TITLE_TRANSDUCER_DEPTH;
                case eBaseSeriesType.Base_RangeTracking_SNR:
                    return TITLE_RANGETRACKING_SNR;
                case eBaseSeriesType.Base_RangeTracking_Range:
                    return TITLE_RANGETRACKING_RANGE;
                case eBaseSeriesType.Base_RangeTracking_Pings:
                    return TITLE_RANGETRACKING_PINGS;
                case eBaseSeriesType.Base_SystemSetup_Voltage:
                    return TITLE_SYSTEMSETUP_VOLTAGE;
                case eBaseSeriesType.Base_Water_Magnitude:
                    return TITLE_WATER_MAGNITUDE;
                case eBaseSeriesType.Base_Water_Direction:
                    return TITLE_WATER_DIRECTION;
                case eBaseSeriesType.Base_Velocity_ENU_East:
                    return TITLE_VEL_ENU_EAST;
                case eBaseSeriesType.Base_Velocity_ENU_North:
                    return TITLE_VEL_ENU_NORTH;
                case eBaseSeriesType.Base_Velocity_ENU_Vertical:
                    return TITLE_VEL_ENU_VERTICAL;
                case eBaseSeriesType.Base_Waves_Frequency:
                    return TITLE_WAVES_FREQUENCY;
                case eBaseSeriesType.Base_Waves_Period:
                    return TITLE_WAVES_PERIOD;
                case eBaseSeriesType.Base_Waves_East_Vel:
                    return TITLE_WAVES_EAST_VEL;
                case eBaseSeriesType.Base_Waves_North_Vel:
                    return TITLE_WAVES_NORTH_VEL;
                case eBaseSeriesType.Base_Waves_Pressure_And_Height:
                    return TITLE_WAVES_PRESSURE_HEIGHT;
                case eBaseSeriesType.Base_Waves_FFT:
                    return TITLE_WAVES_FFT;
                case eBaseSeriesType.Base_Waves_Spectrum:
                    return TITLE_WAVES_SPECTRUM;
                case eBaseSeriesType.Base_Waves_Wave_Set:
                    return TITLE_WAVES_WAVE_SET;
                case eBaseSeriesType.Base_Waves_Sensor_Set:
                    return TITLE_WAVES_SENSOR_SET;
                case eBaseSeriesType.Base_Waves_Velocity_Series:
                    return TITLE_WAVES_VELOCITY_SERIES;
                case eBaseSeriesType.Base_Speed:
                    return TITLE_SPEED;
                case eBaseSeriesType.Base_NMEA_Heading:
                    return TITLE_NMEA_HEADING;
                case eBaseSeriesType.Base_NMEA_Speed:
                    return TITLE_NMEA_SPEED;
                default:
                    return "";
            }
        }

        #endregion

        #region List

        /// <summary>
        /// List of all the series type for the given DataSource.
        /// </summary>
        /// <returns>List of all the series types based off the DataSource given.</returns>
        public static BindingList<BaseSeriesType> GetTimeSeriesList(DataSource.eSource source)
        {
            var list = new BindingList<BaseSeriesType>();

            switch(source)
            {
                case DataSource.eSource.WaterProfile:
                case DataSource.eSource.WaterTrack:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_Beam));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_XYZ));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Amplitude));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Correlation));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Water_Magnitude));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Water_Direction));
                    return list;
                case DataSource.eSource.BottomTrack:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_Beam));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_XYZ));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Amplitude));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Correlation));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_SNR));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Range));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Speed));
                    return list;
                case DataSource.eSource.AncillaryWaterProfile:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pitch));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Roll));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Sys));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Water));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pressure));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_TransducerDepth));
                    return list;
                case DataSource.eSource.AncillaryBottomTrack:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pitch));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Roll));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Sys));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Water));
                    return list;
                case DataSource.eSource.RangeTracking:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_RangeTracking_Range));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_RangeTracking_SNR));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_RangeTracking_Pings));
                    return list;
                case DataSource.eSource.SystemSetup:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_SystemSetup_Voltage));
                    return list;
                case DataSource.eSource.NMEA:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_NMEA_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_NMEA_Speed));
                    return list;
                case DataSource.eSource.Waves:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Frequency));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Period));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_East_Vel));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_North_Vel));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Pressure_And_Height));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_FFT));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Spectrum));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Wave_Set));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Sensor_Set));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Velocity_Series));
                    return list;
                case DataSource.eSource.DVL:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_XYZ));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_Ship));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pitch));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Roll));
                    return list;
                default:
                    return list;

            }
        }

        /// <summary>
        /// List of all the series type for the given DataSource.
        /// </summary>
        /// <returns>List of all the series types based off the DataSource given.</returns>
        public static BindingList<BaseSeriesType> GetHeatmapSeriesList(DataSource.eSource source)
        {
            var list = new BindingList<BaseSeriesType>();

            switch (source)
            {
                case DataSource.eSource.WaterProfile:
                case DataSource.eSource.WaterTrack:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU_East));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU_North));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU_Vertical));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Amplitude));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Correlation));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Water_Magnitude));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Water_Direction));
                    return list;
                case DataSource.eSource.BottomTrack:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_Beam));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_XYZ));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Velocity_ENU));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Amplitude));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Correlation));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_SNR));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Range));
                    return list;
                case DataSource.eSource.AncillaryWaterProfile:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pitch));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Roll));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Sys));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Water));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pressure));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_TransducerDepth));
                    return list;
                case DataSource.eSource.AncillaryBottomTrack:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Pitch));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Roll));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Sys));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Temperature_Water));
                    return list;
                case DataSource.eSource.RangeTracking:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_RangeTracking_Range));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_RangeTracking_SNR));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_RangeTracking_Pings));
                    return list;
                case DataSource.eSource.SystemSetup:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_SystemSetup_Voltage));
                    return list;
                case DataSource.eSource.NMEA:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_NMEA_Heading));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_NMEA_Speed));
                    return list;
                case DataSource.eSource.Waves:
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Frequency));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Period));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_East_Vel));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_North_Vel));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Pressure_And_Height));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_FFT));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Spectrum));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Wave_Set));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Sensor_Set));
                    list.Add(new BaseSeriesType(eBaseSeriesType.Base_Waves_Velocity_Series));
                    return list;
                default:
                    return list;

            }
        }

        #endregion

        #region Override

        /// <summary>
        /// Return the description as the string for this object.
        /// </summary>
        /// <returns>Return the description as the string for this object.</returns>
        public override string ToString()
        {
            return Title;
        }

        /// <summary>
        /// Determine if the 2 series given are the equal.
        /// </summary>
        /// <param name="code1">First series to check.</param>
        /// <param name="code2">Series to check against.</param>
        /// <returns>True if there codes match.</returns>
        public static bool operator ==(BaseSeriesType code1, BaseSeriesType code2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(code1, code2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)code1 == null) || ((object)code2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (code1.Code == code2.Code);
        }

        /// <summary>
        /// Return the opposite of ==.
        /// </summary>
        /// <param name="code1">First series to check.</param>
        /// <param name="code2">Series to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(BaseSeriesType code1, BaseSeriesType code2)
        {
            return !(code1 == code2);
        }

        /// <summary>
        /// Create a hashcode based off the Code stored.
        /// </summary>
        /// <returns>Hash the Code.</returns>
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        /// <summary>
        /// Check if the given object is 
        /// equal to this object.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>If the codes are the same, then they are equal.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            BaseSeriesType p = (BaseSeriesType)obj;

            return (Code == p.Code);
        }

        #endregion
    }

    #endregion

    #region Series Type

    /// <summary>
    /// Describes all the possible series types.  This is used to give a
    /// description, a type code and additional imformation about the
    /// series like the min and max axis values and the plot title.
    /// </summary>
    public class SeriesType
    {
        #region Variables

        #region Water Profile

        /// <summary>
        /// The title for the Water Profile Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WP_VEL_BEAM = "Water Profile Velocity - Beam Coordinate";

        /// <summary>
        /// The title for the Water Profile Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WP_VEL_XYZ = "Water Profile Velocity - Instrument Coordinate";

        /// <summary>
        /// The title for the Water Profile Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WP_VEL_ENU = "Water Profile Velocity - Earth Coordinate";

        /// <summary>
        /// The title for the Water Profile Amplitude.
        /// </summary>
        public const string TITLE_WP_AMP = "Water Profile Amplitude";

        /// <summary>
        /// The title for the Water Profile Correlation.
        /// </summary>
        public const string TITLE_WP_CORR = "Water Profile Correlation";

        /// <summary>
        /// The title for the Water Profile Heading.
        /// </summary>
        public const string TITLE_WP_HEADING = "Water Profile Heading";

        /// <summary>
        /// The title for the Water Profile Pitch.
        /// </summary>
        public const string TITLE_WP_PITCH = "Water Profile Pitch";

        /// <summary>
        /// The title for the Water Profile Roll.
        /// </summary>
        public const string TITLE_WP_ROLL = "Water Profile Roll";

        /// <summary>
        /// The title for the Water Profile System Temperature.
        /// </summary>
        public const string TITLE_WP_TEMP_SYS = "Water Profile System Temperature";

        /// <summary>
        /// The title for the Water Profile Water Temperature.
        /// </summary>
        public const string TITLE_WP_TEMP_WATER = "Water Profile Water Temperature";

        /// <summary>
        /// The title for the Water Profile Pressure.
        /// </summary>
        public const string TITLE_WP_PRESSURE = "Water Profile Pressure";

        #endregion

        #region Bottom Track

        /// <summary>
        /// The title for the Bottom Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_BT_VEL_BEAM = "Bottom Track Velocity - Beam Coordinate";

        /// <summary>
        /// The title for the Bottom Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_BT_VEL_XYZ = "Bottom Track Velocity - Instrument Coordinate";

        /// <summary>
        /// The title for the Bottom Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_BT_VEL_ENU = "Bottom Track Velocity - Earth Coordinate";

        /// <summary>
        /// The title for the Bottom Track Correlation.
        /// </summary>
        public const string TITLE_BT_CORR = "Bottom Track Correlation";

        /// <summary>
        /// The title for the Bottom Track Amplitude.
        /// </summary>
        public const string TITLE_BT_AMP = "Bottom Track Amplitude";

        /// <summary>
        /// The title for the Bottom Track Heading.
        /// </summary>
        public const string TITLE_BT_HEADING = "Bottom Track Heading";

        /// <summary>
        /// The title for the Bottom Track Pitch.
        /// </summary>
        public const string TITLE_BT_PITCH = "Bottom Track Pitch";

        /// <summary>
        /// The title for the Bottom Track Roll.
        /// </summary>
        public const string TITLE_BT_ROLL = "Bottom Track Roll";

        /// <summary>
        /// The title for the Bottom Track System Temperature.
        /// </summary>
        public const string TITLE_BT_TEMP_SYS = "Bottom Track System Temperature";

        /// <summary>
        /// The title for the Bottom Track Water Temperature.
        /// </summary>
        public const string TITLE_BT_TEMP_WATER = "Bottom Track Water Temperature";

        /// <summary>
        /// The title for the Bottom Track Range.
        /// </summary>
        public const string TITLE_BT_RANGE = "Bottom Track Range";

        /// <summary>
        /// The title for the Bottom Track SNR.
        /// </summary>
        public const string TITLE_BT_SNR = "Bottom Track SNR";

        /// <summary>
        /// The title for the Bottom Track Speed.
        /// </summary>
        public const string TITLE_BT_SPEED = "Bottom Track Speed";

        #endregion

        #region Water Track

        /// <summary>
        /// The title for the Water Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WT_VEL_XYZ = "Water Track Velocity - Instrument Coordinate";

        /// <summary>
        /// The title for the Water Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WT_VEL_ENU = "Water Track Velocity - Earth Coordinate";

        #endregion

        #region Range Tracking

        /// <summary>
        /// The title for the Range Tracking Signal to Noise Ratio.
        /// </summary>
        public const string TITLE_RT_SNR = "Range Tracking SNR";

        /// <summary>
        /// The title for the Range Tracking Range.
        /// </summary>
        public const string TITLE_RT_RANGE = "Range Tracking Range";

        /// <summary>
        /// The title for the Range Tracking Pings.
        /// </summary>
        public const string TITLE_RT_PINGS = "Range Tracking Pings";

        #endregion

        #region System Setup

        /// <summary>
        /// The title for the System Setup Voltage.
        /// </summary>
        public const string TITLE_SS_VOLTAGE = "Voltage";

        #endregion

        #region NMEA

        /// <summary>
        /// The title for the NMEA Heading.
        /// </summary>
        public const string TITLE_NMEA_HEADING = "NMEA Heading";

        /// <summary>
        /// The title for the NMEA Speed.
        /// </summary>
        public const string TITLE_NMEA_SPEED = "NMEA Speed";

        #endregion

        #region DVL

        /// <summary>
        /// The title for the DVL Instrument Velocity.
        /// </summary>
        public const string TITLE_DVL_VEL_XYZ = "DVL Instrument Velocity";

        /// <summary>
        /// The title for the DVL Earth Velocity.
        /// </summary>
        public const string TITLE_DVL_VEL_ENU = "DVL Earth Velocity";

        /// <summary>
        /// The title for the DVL Ship Velocity.
        /// </summary>
        public const string TITLE_DVL_VEL_SHIP = "DVL Ship Velocity";

        /// <summary>
        /// The title for the DVL Heading.
        /// </summary>
        public const string TITLE_DVL_HEADING = "DVL Heading";

        /// <summary>
        /// The title for the DVL Pitch.
        /// </summary>
        public const string TITLE_DVL_PITCH = "DVL Pitch";

        /// <summary>
        /// The title for the DVL Roll.
        /// </summary>
        public const string TITLE_DVL_ROLL = "DVL Roll";


        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The source of the data.  This will determine if this will
        /// come from Water Profile, Bottom Track or Water Track data.
        /// </summary>
        public DataSource Source { get; set; }

        /// <summary>
        /// Base Series type.  This will determine what
        /// type of data will be displayed, velocity, amplitude, ...
        /// </summary>
        public BaseSeriesType Type { get; set; }

        /// <summary>
        /// The Title for the series type.
        /// </summary>
        public string Title { get; set; }

        #endregion


        /// <summary>
        /// Initialize the object based off the source and
        /// type given.  This will set the title.
        /// </summary>
        /// <param name="source">Data Source.</param>
        /// <param name="type">Base series type.</param>
        public SeriesType(DataSource source, BaseSeriesType type)
        {
            // Set the values
            Source = source;
            Type = type;
            Title = GetTitle(source, type);
        }

        #region Titles

        /// <summary>
        /// Get the description for the series type based off
        /// the values given.
        /// </summary>
        /// <param name="source">Source of the data.</param>
        /// <param name="type">Enum value to determine the type.</param>
        /// <returns>Description of the series.</returns>
        public static string GetTitle(DataSource source, BaseSeriesType type)
        {
            switch(source.Source)
            {
                case DataSource.eSource.WaterProfile:
                case DataSource.eSource.AncillaryWaterProfile:
                    return GetWaterProfileTitle(type);
                case DataSource.eSource.BottomTrack:
                case DataSource.eSource.AncillaryBottomTrack:
                    return GetBottomTrackTitle(type);
                case DataSource.eSource.WaterTrack:
                    return GetWaterTrackTitle(type);
                case DataSource.eSource.RangeTracking:
                    return GetRangeTrackingTitle(type);
                case DataSource.eSource.SystemSetup:
                    return GetSystemSetupTitle(type);
                case DataSource.eSource.NMEA:
                    return GetNmeaTitle(type);
                case DataSource.eSource.DVL:
                    return GetDvlTitle(type);
                default:
                    return "";
            }
        }

        /// <summary>
        /// Water Profile titles.
        /// </summary>
        /// <param name="type">Series type.</param>
        /// <returns>Title for the series.</returns>
        private static string GetWaterProfileTitle(BaseSeriesType type)
        {
            switch(type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                    return TITLE_WP_VEL_BEAM;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    return TITLE_WP_VEL_XYZ;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    return TITLE_WP_VEL_ENU;
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:
                    return TITLE_WP_AMP;
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:
                    return TITLE_WP_CORR;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:
                    return TITLE_WP_HEADING;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:
                    return TITLE_WP_PITCH;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:
                    return TITLE_WP_ROLL;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:
                    return TITLE_WP_TEMP_SYS;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:
                    return TITLE_WP_TEMP_WATER;
                case BaseSeriesType.eBaseSeriesType.Base_Pressure:
                    return TITLE_WP_PRESSURE;
                default:
                return "";
            }
        }

        /// <summary>
        /// Get the title based off the type given.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <returns>Title for the type.</returns>
        private static string GetBottomTrackTitle(BaseSeriesType type)
        {
            switch(type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                    return TITLE_BT_VEL_BEAM;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    return TITLE_BT_VEL_XYZ;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    return TITLE_BT_VEL_ENU;
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:
                    return TITLE_BT_AMP;
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:
                    return TITLE_BT_CORR;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:
                    return TITLE_BT_HEADING;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:
                    return TITLE_BT_PITCH;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:
                    return TITLE_BT_ROLL;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:
                    return TITLE_BT_TEMP_SYS;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:
                    return TITLE_BT_TEMP_WATER;
                case BaseSeriesType.eBaseSeriesType.Base_Range:
                    return TITLE_BT_RANGE;
                case BaseSeriesType.eBaseSeriesType.Base_SNR:
                    return TITLE_BT_SNR;
                case BaseSeriesType.eBaseSeriesType.Base_Speed:
                    return TITLE_BT_SPEED; 
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the title based off the type given.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <returns>Title for the type.</returns>
        private static string GetWaterTrackTitle(BaseSeriesType type)
        {
            switch(type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    return TITLE_WT_VEL_XYZ;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    return TITLE_WT_VEL_ENU;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the title based off the type given.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <returns>Title for the type.</returns>
        private static string GetRangeTrackingTitle(BaseSeriesType type)
        {
            if(type == null)
            {
                return "";
            }

            switch (type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_SNR:
                    return TITLE_RT_SNR;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Range:
                    return TITLE_RT_RANGE;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Pings:
                    return TITLE_RT_PINGS;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the title based off the type given.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <returns>Title for the type.</returns>
        private static string GetSystemSetupTitle(BaseSeriesType type)
        {
            if (type == null)
            {
                return "";
            }

            switch (type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_SystemSetup_Voltage:
                    return TITLE_SS_VOLTAGE;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the title based off the type given.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <returns>Title for the type.</returns>
        private static string GetNmeaTitle(BaseSeriesType type)
        {
            if (type == null)
            {
                return "";
            }

            switch (type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Heading:
                    return TITLE_NMEA_HEADING;
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Speed:
                    return TITLE_NMEA_SPEED;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the title based off the type given.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <returns>Title for the type.</returns>
        private static string GetDvlTitle(BaseSeriesType type)
        {
            if (type == null)
            {
                return "";
            }

            switch (type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    return TITLE_DVL_VEL_XYZ;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    return TITLE_DVL_VEL_ENU;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Ship:
                    return TITLE_DVL_VEL_SHIP;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:
                    return TITLE_DVL_HEADING;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:
                    return TITLE_DVL_PITCH;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:
                    return TITLE_DVL_ROLL;
                default:
                    return "";
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// Return the description as the string for this object.
        /// </summary>
        /// <returns>Return the description as the string for this object.</returns>
        public override string ToString()
        {
            return Title;
        }

        /// <summary>
        /// Determine if the 2 series given are the equal.
        /// </summary>
        /// <param name="code1">First series to check.</param>
        /// <param name="code2">Series to check against.</param>
        /// <returns>True if there codes match.</returns>
        public static bool operator ==(SeriesType code1, SeriesType code2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(code1, code2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)code1 == null) || ((object)code2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (code1.Source == code2.Source) && (code1.Type == code2.Type);
        }

        /// <summary>
        /// Return the opposite of ==.
        /// </summary>
        /// <param name="code1">First series to check.</param>
        /// <param name="code2">Series to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(SeriesType code1, SeriesType code2)
        {
            return !(code1 == code2);
        }

        /// <summary>
        /// Create a hashcode based off the Code stored.
        /// </summary>
        /// <returns>Hash the Code.</returns>
        public override int GetHashCode()
        {
            return Source.GetHashCode() + Type.GetHashCode();
        }

        /// <summary>
        /// Check if the given object is 
        /// equal to this object.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>If the codes are the same, then they are equal.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            SeriesType p = (SeriesType)obj;

            return (Source == p.Source) && (Type == p.Type);
        }

        #endregion

    }

    #endregion
}
