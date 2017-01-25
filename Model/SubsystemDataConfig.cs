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
 * 08/15/2013      RC          3.0.7      Initial coding.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Used to determine where the data came from and where it should go.
    /// Base VM's create VMs based off the Subsystem configuration.  This will
    /// also give where the data came from, so multiple displays can be created
    /// for a SubsystemConfiguration and each VM will now differ based off
    /// SubsystemConfiguration and DataSource.
    /// </summary>
    public class SubsystemDataConfig : SubsystemConfiguration
    {
        #region Variables


        #endregion

        #region Properties

        /// <summary>
        /// Ensemble Source.
        /// </summary>
        public EnsembleSource Source { get; set; }

        #endregion

        /// <summary>
        /// Configuration of the Subsystem data.
        /// This will determine where the data comes from
        /// and where it should go based off the SubsystemConfiguration.
        /// </summary>
        /// <param name="ss">Subsystem for the configuration.</param>
        /// <param name="cepoIndex">CEPO index.</param>
        /// <param name="ssConfigIndex">SubsystemConfiguration Index.</param>
        /// <param name="source">Source of the data.</param>
        public SubsystemDataConfig(Subsystem ss, byte cepoIndex, byte ssConfigIndex, EnsembleSource source)
            : base(ss, cepoIndex, ssConfigIndex)
        {
            // Initialize values
            Source = source;
        }

        /// <summary>
        /// Configuration of the Subsystem data.
        /// This will determine where the data comes from
        /// and where it should go based off the SubsystemConfiguration.
        /// </summary>
        /// <param name="ssConfig">Subsystem Configuration.</param>
        /// <param name="source">Ensemble source.</param>
        public SubsystemDataConfig(SubsystemConfiguration ssConfig, EnsembleSource source)
            : base(ssConfig.SubSystem, ssConfig.CepoIndex, ssConfig.SubsystemConfigIndex)
        {
            Source = source;
        }

        ///// <summary>
        ///// Get the Index and Code from the base class.
        ///// </summary>
        ///// <returns>Index and code string.</returns>
        //public string IndexCodeString()
        //{
        //    return base.IndexCodeString();
        //}

        #region Overrides

        /// <summary>
        /// Determine if the 2 SubsystemConfigurations given are the equal.
        /// </summary>
        /// <param name="config1">First SubsystemDataConfig to check.</param>
        /// <param name="config2">SubsystemDataConfig to check against.</param>
        /// <returns>True if there CommandSetup match.</returns>
        public static bool operator ==(SubsystemDataConfig config1, SubsystemDataConfig config2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(config1, config2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)config1 == null) || ((object)config2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (config1.SubSystem == config2.SubSystem && config1.CepoIndex == config2.CepoIndex && config1.SubsystemConfigIndex == config2.SubsystemConfigIndex && config1.Source == config2.Source);
        }

        /// <summary>
        /// Return the opposite of ==.
        /// </summary>
        /// <param name="config1">First SubsystemDataConfig to check.</param>
        /// <param name="config2">SubsystemDataConfig to check against.</param>
        /// <returns>Return the opposite of ==.</returns>
        public static bool operator !=(SubsystemDataConfig config1, SubsystemDataConfig config2)
        {
            return !(config1 == config2);
        }

        /// <summary>
        /// Create a hashcode based off the CommandSetup stored.
        /// </summary>
        /// <returns>Hash the Code.</returns>
        public override int GetHashCode()
        {
            return CepoIndex.GetHashCode() + Source.GetHashCode();
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

            SubsystemDataConfig p = (SubsystemDataConfig)obj;

            return (SubSystem == p.SubSystem && CepoIndex == p.CepoIndex && SubsystemConfigIndex == p.SubsystemConfigIndex && Source == p.Source);
        }

        #endregion
    }
}
