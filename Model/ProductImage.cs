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
 * 11/15/2013      RC          3.2.0      Initial coding
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Load the product image based off the serial number given.
    /// </summary>
    public class ProductImage
    {

        #region Image

        /// <summary>
        /// 300 kHz 5.25" Self Contained ADCP.
        /// </summary>
        public const string IMG_300SC_525 = "../Images/300SC_525.png";

        /// <summary>
        /// 1200 kHz Direct Reading ADCP.
        /// </summary>
        public const string IMG_1200DR = "../Images/1200DR.png";

        /// <summary>
        /// 1200 kHz Self Contained ADCP.
        /// </summary>
        public const string IMG_1200SC = "../Images/1200SC.png";

        /// <summary>
        /// 300 or 600 kHz Direct Reading ADCP.
        /// </summary>
        public const string IMG_300600DR = "../Images/300600DR.png";

        /// <summary>
        /// 300 or 600 kHz Self Contained ADCP.
        /// </summary>
        public const string IMG_300600SC = "../Images/300600SC.png";

        /// <summary>
        /// Dual frequency Direct Reading ADCP.
        /// </summary>
        public const string IMG_DUALDR = "../Images/dualDR.png";

        /// <summary>
        /// Dual freuquency Self Contained ADCP.
        /// </summary>
        public const string IMG_DUALSC = "../Images/dualSC.png";

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public ProductImage()
        {

        }

        #region Product Image

        /// <summary>
        /// Determine the Product image based off the mode type and frequency.
        /// </summary>
        /// <param name="project">Project to determine the project image.</param>
        /// <returns>The string for the product image.</returns>
        public static string GetProductImage(Project project)
        {
            if (project != null)
            {
                switch (project.Configuration.DeploymentOptions.DeploymentMode)
                {
                    case DeploymentOptions.AdcpDeploymentMode.DirectReading:
                    case DeploymentOptions.AdcpDeploymentMode.River:
                    case DeploymentOptions.AdcpDeploymentMode.VM:
                    case DeploymentOptions.AdcpDeploymentMode.Dvl:
                        return DirectReadingImage(project);
                    case DeploymentOptions.AdcpDeploymentMode.SelfContained:
                    case DeploymentOptions.AdcpDeploymentMode.Waves:
                        return SelfContainedImage(project);

                }
            }

            return IMG_300600DR;
        }

        /// <summary>
        /// Determine which frequency the Direct Reading ADCP to display.
        /// </summary>
        /// <param name="project">Project to determine the project image.</param>
        /// <returns>The string for the product image.</returns>
        private static string DirectReadingImage(Project project)
        {
            if (project.Configuration.SerialNumber.SubSystemsList.Count > 0)
            {
                // Check if its a dual frequency system
                if (project.Configuration.SerialNumber.SubSystemsList.Count > 1)
                {
                    return IMG_DUALDR;
                }
                else
                {
                    // Determine the frequency of the system
                    switch (project.Configuration.SerialNumber.SubSystemsList[0].Code)
                    {
                        case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_2:
                        case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_45OFFSET_6:
                            return IMG_1200DR;
                        case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_3:
                        case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_45OFFSET_7:
                        case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4:
                        case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_45OFFSET_8:
                        default:
                            return IMG_300600DR;
                    }
                }
            }
            else
            {
                return IMG_300600DR;
            }
        }

        /// <summary>
        /// Determine which frequency the Self Contained ADCP to display.
        /// </summary>
        /// <param name="project">Project to determine the project image.</param>
        /// <returns>The string for the product image.</returns>
        private static string SelfContainedImage(Project project)
        {
            if (project.Configuration.SerialNumber.SubSystemsList.Count > 0)
            {
                // Check if its a dual frequency system
                if (project.Configuration.SerialNumber.SubSystemsList.Count > 1)
                {
                    return IMG_DUALSC;
                }
                else
                {
                    // Determine the frequency of the system
                    switch (project.Configuration.SerialNumber.SubSystemsList[0].Code)
                    {
                        case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_2:
                        case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_45OFFSET_6:
                            return IMG_1200SC;
                        case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_3:
                        case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_45OFFSET_7:
                        case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4:
                        case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_45OFFSET_8:
                        default:
                            return IMG_300600SC;
                    }
                }
            }
            else
            {
                return IMG_300600SC;
            }
        }

        #endregion

    }
}
