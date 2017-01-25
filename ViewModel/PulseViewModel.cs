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
 * 05/10/2013      RC          3.0.0      Initial coding
 * 09/13/2013      RC          3.1.1      Changed in constructor setting DisplayName to setting base.DisplayName.
 * 08/08/2014      RC          4.0.0      Removed _reactiveHelper.  It is not needed for ReactiveUI 6.0.
 * 
 */

using System.ComponentModel;
using Caliburn.Micro;
using ReactiveUI;

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;

    /// <summary>
    /// Create a base View model.
    /// This will include all the property changes and
    /// be a base class of Screen.
    /// </summary>
    public abstract class PulseViewModel : Screen, IDisposable
    {

        #region Variables


        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public PulseViewModel(string name)
        {
            // Set the display name used by Caliburn.Micro
            base.DisplayName = name;
        }

        /// <summary>
        /// Method to handle the shutdown process
        /// of the viewmodel.
        /// </summary>
        public abstract void Dispose();

    }
}
