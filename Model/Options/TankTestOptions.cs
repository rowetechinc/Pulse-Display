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
 * 06/23/2015      RC          0.0.5      Initial coding
 * 
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
    /// Tank Test Options.
    /// </summary>
    public class TankTestOptions
    {

        #region Properties

        /// <summary>
        /// Flag to filter data.
        /// </summary>
        public bool IsFilteringData { get; set; }

        /// <summary>
        /// Flag to average the data.
        /// </summary>
        public bool IsCorrAmpAverage { get; set; }

        /// <summary>
        /// Maximum number of ensembles to display.
        /// </summary>
        public int MaxEnsembles { get; set; }

        /// <summary>
        /// Flag to display statistics.
        /// </summary>
        public bool IsStats { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public TankTestOptions()
        {
            Init();
        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public void Init()
        {
            IsFilteringData = false;
            IsCorrAmpAverage = true;
            MaxEnsembles = 100;
            IsStats = false;
        }
    }
}
