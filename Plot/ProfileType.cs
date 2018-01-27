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
 * Date            Initials    Vertion    Comments
 * -----------------------------------------------------------------
 * 02/01/2013      RC          2.18       Initial coding
 * 
 */


namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;

    /// <summary>
    /// All the different types of Profile Plots.
    /// </summary>
    public class ProfileType
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

        #endregion

        #region Water Track

        /// <summary>
        /// The title for the Water Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WT_VEL_BEAM = "Water Track Velocity - Beam Coordinate";

        /// <summary>
        /// The title for the Water Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WT_VEL_XYZ = "Water Track Velocity - Instrument Coordinate";

        /// <summary>
        /// The title for the Water Track Velocity Beam Coordiante system.
        /// </summary>
        public const string TITLE_WT_VEL_ENU = "Water Track Velocity - Earth Coordinate";

        /// <summary>
        /// The title for the Water Track Amplitude.
        /// </summary>
        public const string TITLE_WT_AMP = "Water Track Amplitude";

        /// <summary>
        /// The title for the Water Track Correlation.
        /// </summary>
        public const string TITLE_WT_CORR = "Water Track Correlation";

        #endregion

        #endregion

        #region Enum

        /// <summary>
        /// Enum of all the different types of profiles.
        /// This will also give a code for the profile type.
        /// </summary>
        public enum eProfileType
        {
            /// <summary>
            /// Water Profile Velocity in Beam Coordinate Transform.
            /// </summary>
            WP_Velocity_BEAM,

            /// <summary>
            /// Water Profile Velocity in Instrument Coordinate Transform.
            /// </summary>
            WP_Velocity_XYZ,

            /// <summary>
            /// Water Profile Velocity in Earth Coordinate Transform.
            /// </summary>
            WP_Velocity_ENU,

            /// <summary>
            /// Water Profile Amplitude.
            /// </summary>
            WP_Amplitude,

            /// <summary>
            /// Water Profile Correlation.
            /// </summary>
            WP_Correlation,

            /// <summary>
            /// Water Track Velocity in Beam Coordinate Transform.
            /// </summary>
            WT_Velocity_BEAM,

            /// <summary>
            /// Water Track Velocity in Instrument Coordinate Transform.
            /// </summary>
            WT_Velocity_XYZ,

            /// <summary>
            /// Water Track Velocity in Earth Coordinate Transform.
            /// </summary>
            WT_Velocity_ENU,

            /// <summary>
            /// Water Track Amplitude.
            /// </summary>
            WT_Amplitude,

            /// <summary>
            /// Water Track Correlation.
            /// </summary>
            WT_Correlation
        }

        #endregion

        #region Properties

        /// <summary>
        /// Series type code.  This is a unique
        /// value to identify the type of series.
        /// </summary>
        public eProfileType Code { get; set; }

        /// <summary>
        /// The Title for the series type.
        /// </summary>
        public string Title { get; set; }

        #endregion

        /// <summary>
        /// Create a profile type with the given
        /// profile enum.  This will create an object
        /// with a code and title.
        /// </summary>
        /// <param name="type">Profile type.</param>
        public ProfileType(eProfileType type)
        {
            // Set the values
            Code = type;
            Title = GetTitle(type);
        }

        /// <summary>
        /// Get the description for the profile type based off
        /// the enum value given.
        /// </summary>
        /// <param name="type">Enum value to determine the type.</param>
        /// <returns>Description of the profile.</returns>
        public static string GetTitle(eProfileType type)
        {
            switch (type)
            {
                case eProfileType.WP_Velocity_BEAM:
                    return TITLE_WP_VEL_BEAM;
                case eProfileType.WP_Velocity_XYZ:
                    return TITLE_WP_VEL_XYZ;
                case eProfileType.WP_Velocity_ENU:
                    return TITLE_WP_VEL_ENU;
                case eProfileType.WP_Amplitude:
                    return TITLE_WP_AMP;
                case eProfileType.WP_Correlation:
                    return TITLE_WP_CORR;
                case eProfileType.WT_Velocity_BEAM:
                    return TITLE_WT_VEL_BEAM;
                case eProfileType.WT_Velocity_XYZ:
                    return TITLE_WT_VEL_XYZ;
                case eProfileType.WT_Velocity_ENU:
                    return TITLE_WT_VEL_ENU;
                case eProfileType.WT_Amplitude:
                    return TITLE_WT_AMP;
                case eProfileType.WT_Correlation:
                    return TITLE_WT_CORR;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Create a list of the DataSet types.  The list will be based off
        /// the profile type given.  The is list generated and then returned with 
        /// only the data set types available for the given series type.
        /// </summary>
        /// <param name="type">Series type.</param>
        /// <returns>List with all the possible profile data set types.</returns>
        public static ObservableCollection<ProfileType> GetDataSetTypeList(RTI.BaseSeriesType.eBaseSeriesType type)
        {
            ObservableCollection<ProfileType> list = new ObservableCollection<ProfileType>();

            switch (type)
            {
                case RTI.BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                    list.Add(new ProfileType(ProfileType.eProfileType.WP_Velocity_BEAM));
                    list.Add(new ProfileType(ProfileType.eProfileType.WT_Velocity_BEAM));
                    break;
                case RTI.BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    list.Add(new ProfileType(ProfileType.eProfileType.WP_Velocity_XYZ));
                    list.Add(new ProfileType(ProfileType.eProfileType.WT_Velocity_XYZ));
                    break;
                case RTI.BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    list.Add(new ProfileType(ProfileType.eProfileType.WP_Velocity_ENU));
                    list.Add(new ProfileType(ProfileType.eProfileType.WT_Velocity_ENU));
                    break;
                case RTI.BaseSeriesType.eBaseSeriesType.Base_Amplitude:
                    list.Add(new ProfileType(ProfileType.eProfileType.WP_Amplitude));
                    list.Add(new ProfileType(ProfileType.eProfileType.WT_Amplitude));
                    break;
                case RTI.BaseSeriesType.eBaseSeriesType.Base_Correlation:
                    list.Add(new ProfileType(ProfileType.eProfileType.WP_Correlation));
                    list.Add(new ProfileType(ProfileType.eProfileType.WT_Correlation));
                    break;
                default:
                    break;
            }

            return list;
        }

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
        /// Determine if the 2 profile given are the equal.
        /// </summary>
        /// <param name="code1">First profile to check.</param>
        /// <param name="code2">Profile to check against.</param>
        /// <returns>True if there codes match.</returns>
        public static bool operator ==(ProfileType code1, ProfileType code2)
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
        /// <param name="code1">First profile to check.</param>
        /// <param name="code2">Profile to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(ProfileType code1, ProfileType code2)
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

            ProfileType p = (ProfileType)obj;

            return (Code == p.Code);
        }

        #endregion
    }
}
