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
 * 05/06/2013      RC          3.0.0      Initial coding
 * 05/23/2013      RC          3.0.0      Added Send command.
 * 07/31/2013      RC          3.0.6      Added CompassDisconnectCommand.
 * 08/07/2013      RC          3.0.7      Moved the ADCP Terminal to TerminalAdcpViewModel.
 * 
 */

using System.ComponentModel;
using System.Diagnostics;
using Caliburn.Micro;

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.Collections;

    /// <summary>
    /// Terminal to communicate with the ADCP and GPS.
    /// </summary>
    public class TerminalViewModel : PulseViewModel, IDeactivate
    {

        #region Variables

        #endregion

        #region Properties

        /// <summary>
        /// Terminal for the ADCP.
        /// </summary>
        public TerminalAdcpViewModel AdcpTerminalVM { get; set; }

        /// <summary>
        /// Terminal for the GPS.
        /// </summary>
        public TerminalNavViewModel NavTerminalVM { get; set; }

        #endregion


        /// <summary>
        /// Set the ADCP and GPS Terminal Viewmodels.
        /// </summary>
        public TerminalViewModel(AdcpConnection adcpConn) 
            : base("Terminal")
        {
            AdcpTerminalVM = new TerminalAdcpViewModel(adcpConn);
            NavTerminalVM = new TerminalNavViewModel(adcpConn);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            AdcpTerminalVM.Dispose();
            NavTerminalVM.Dispose();
        }
    }
}
