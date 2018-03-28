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
 * 03/28/2018      RC          4.8.1      Initial coding.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Class to store the Data Format options to decode in the codec.
    /// </summary>
    public class DataFormatOptions
    {

        #region Properties

        /// <summary>
        /// Is the Binary Format turned on.
        /// </summary>
        public bool IsBinaryFormat { get; set; }

        /// <summary>
        /// Is the DVL Format turned on.
        /// </summary>
        public bool IsDvlFormat { get; set; }

        /// <summary>
        /// Is the PD0 Format turned on.
        /// </summary>
        public bool IsPd0Format { get; set; }

        /// <summary>
        /// Is the PD6/PD13 Format turned on.
        /// </summary>
        public bool IsPd6_13Format { get; set; }

        /// <summary>
        /// Is the PD4/PD5 Format turned on.
        /// </summary>
        public bool IsPd4_5Format { get; set; }

        #endregion

        /// <summary>
        /// Initialize the options.
        /// </summary>
        public DataFormatOptions()
        {
            SetDefault();
        }

        /// <summary>
        /// Set the default options.
        /// </summary>
        private void SetDefault()
        {
            IsBinaryFormat = true;
            IsDvlFormat = true;
            IsPd0Format = true;
            IsPd6_13Format = true;
            IsPd4_5Format = true;
        }
    }
}
