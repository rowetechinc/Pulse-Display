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
 * 08/07/2013      RC          3.0.7      Initial coding
 * 08/09/2013      RC          3.0.7      Added GpsSendEscCommand.
 * 01/17/2014      RC          3.2.3      Display all the NMEA terminals.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ReactiveUI;
    using System.Collections.ObjectModel;
    using Caliburn.Micro;
    using System.Collections;

    /// <summary>
    /// Terminal to communicate with the Navigation sources (NMEA).
    /// </summary>
    public class TerminalNavViewModel : PulseViewModel
    {
        #region Variables

        #endregion

        #region Properties

        /// <summary>
        /// GPS 1 terminal.
        /// </summary>
        public TerminalNmeaViewModel TerminalGps1VM { get; set; }

        /// <summary>
        /// GPS 2 terminal.
        /// </summary>
        public TerminalNmeaViewModel TerminalGps2VM { get; set; }

        /// <summary>
        /// NMEA 1 terminal.
        /// </summary>
        public TerminalNmeaViewModel TerminalNmea1VM { get; set; }

        /// <summary>
        /// NMEA 2 terminal.
        /// </summary>
        public TerminalNmeaViewModel TerminalNmea2VM { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public TerminalNavViewModel(AdcpConnection adcpConn)
            : base("Navigation Terminal")
        {
            // GPS 1
            TerminalGps1VM = new TerminalNmeaViewModel(TerminalNmeaViewModel.TerminalNavType.GPS1, "GPS 1 Serial Port", adcpConn);

            // GPS 2
            TerminalGps2VM = new TerminalNmeaViewModel(TerminalNmeaViewModel.TerminalNavType.GPS2, "GPS 2 Serial Port", adcpConn);

            // NMEA 1
            TerminalNmea1VM = new TerminalNmeaViewModel(TerminalNmeaViewModel.TerminalNavType.NMEA1, "NMEA 1 Serial Port", adcpConn);

            // NMEA 2
            TerminalNmea2VM = new TerminalNmeaViewModel(TerminalNmeaViewModel.TerminalNavType.NMEA2, "NMEA 2 Serial Port", adcpConn);
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            TerminalGps1VM.Dispose();

            TerminalGps2VM.Dispose();

            TerminalNmea1VM.Dispose();

            TerminalNmea2VM.Dispose();
        }

        
    }
}
