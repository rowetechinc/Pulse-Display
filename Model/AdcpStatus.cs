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
 * 01/22/2013      RC          2.17       Initial coding
 * 
 */



namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;

    /// <summary>
    /// Options for connection to the ADCP.
    /// Its either connected in ADCP or Compass mode,
    /// or not connected.
    /// </summary>
    public enum eAdcpStatus
    {
        /// <summary>
        /// Not connected to the ADCP.
        /// Unknown Issue with the ADCP,
        /// but no communication with the port.
        /// </summary>
        NotConnected = -1,

        /// <summary>
        /// Have not determined whether we
        /// are connected to the ADCP.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Connect to ADCP but in compass mode.
        /// </summary>
        Compass = 1,

        /// <summary>
        /// Connected to ADCP and in ADCP mode.
        /// </summary>
        Connected = 2,

        /// <summary>
        /// Downloading data from the ADCP.
        /// </summary>
        Downloading = 3,

        /// <summary>
        /// Uploading data to the ADCP.
        /// </summary>
        Uploading = 4,

        /// <summary>
        /// Importing data.
        /// </summary>
        Importing = 5
    }

    /// <summary>
    /// Status of the ADCP.  This is the output mode
    /// status of the ADCP.  If in downloading data, 
    /// importing data, or uploading data or ...
    /// </summary>
    public class AdcpStatus
    {

        #region Properties

        /// <summary>
        /// Status of this object.
        /// </summary>
        public eAdcpStatus Status { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object with the Unknown status.
        /// </summary>
        public AdcpStatus()
        {
            Status = eAdcpStatus.Unknown;
        }

        /// <summary>
        /// Initialize the object with the status.
        /// </summary>
        /// <param name="status">Current status.</param>
        public AdcpStatus(eAdcpStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Get the color for the status of this object.
        /// </summary>
        /// <returns>Color for this object.</returns>
        public Brush GetStatusColor()
        {
            return AdcpStatus.GetStatusColor(Status);
        }

        /// <summary>
        /// Get the string for the status of this object.
        /// </summary>
        /// <returns>String for this object.</returns>
        public string GetStatusString()
        {
            return AdcpStatus.GetStatusString(Status);
        }

        /// <summary>
        /// Get a color brush for the given status.
        /// This is used to give alert colors based off
        /// the status.
        /// </summary>
        /// <param name="status">Status to get the color.</param>
        /// <returns>Color brush based off status given.</returns>
        public static Brush GetStatusColor(eAdcpStatus status)
        {
            switch (status)
            {
                case eAdcpStatus.Unknown:
                    return new SolidColorBrush(Colors.Black);
                case eAdcpStatus.Compass:
                case eAdcpStatus.Downloading:
                case eAdcpStatus.Uploading:
                case eAdcpStatus.Importing:
                    return new SolidColorBrush(Colors.Yellow);
                case eAdcpStatus.NotConnected:
                    return new SolidColorBrush(Colors.Red);
                case eAdcpStatus.Connected:
                    return new SolidColorBrush(Colors.Green);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        /// <summary>
        /// Get the status string based off the status given.
        /// </summary>
        /// <param name="status">Status given.</param>
        /// <returns>String based off status given.</returns>
        public static string GetStatusString(eAdcpStatus status)
        {
            switch (status)
            {
                case eAdcpStatus.Unknown:
                    return "";
                case eAdcpStatus.Compass:
                    return "Compass Mode";
                case eAdcpStatus.NotConnected:
                    return "Not Connected";
                case eAdcpStatus.Connected:
                    return "Connected";
                case eAdcpStatus.Downloading:
                    return "Downloading";
                case eAdcpStatus.Uploading:
                    return "Uploading";
                case eAdcpStatus.Importing:
                    return "Importing";
                default:
                    return "";
            }
        }

        #region Override

        /// <summary>
        /// Return a string representing the
        /// status.
        /// </summary>
        /// <returns>Status value as a string.</returns>
        public override string ToString()
        {
            return GetStatusString();
        }

        /// <summary>
        /// Hashcode for the object.
        /// This will return the hashcode for the
        /// this object's string.
        /// </summary>
        /// <returns>Hashcode for the object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Determine if the given object is equal to this
        /// object.  This will check if the Status Value match.
        /// </summary>
        /// <param name="obj">Object to compare with this object.</param>
        /// <returns>TRUE = Status Value matched.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            AdcpStatus p = (AdcpStatus)obj;

            return Status == p.Status;
        }

        /// <summary>
        /// Determine if the two AdcpStatus Value given are the equal.
        /// </summary>
        /// <param name="stat1">First AdcpStatus to check.</param>
        /// <param name="stat2">AdcpStatus to check against.</param>
        /// <returns>True if there strings match.</returns>
        public static bool operator ==(AdcpStatus stat1, AdcpStatus stat2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(stat1, stat2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)stat1 == null) || ((object)stat2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return stat1.Status == stat2.Status;
        }

        /// <summary>
        /// Return the opposite of ==.
        /// </summary>
        /// <param name="stat1">First AdcpStatus to check.</param>
        /// <param name="stat2">AdcpStatus to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(AdcpStatus stat1, AdcpStatus stat2)
        {
            return !(stat1 == stat2);
        }

        #endregion

    }
}
