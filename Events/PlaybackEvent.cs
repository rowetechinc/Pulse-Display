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
 * 09/10/2014      RC          4.0.3      Initial coding.
 * 06/18/2019      RC          4.11.1     Added DisplayAllEvent.
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
    /// Event published when a new Playback is set.
    /// </summary>
    public class PlaybackEvent
    {
        #region Properties

        /// <summary>
        /// Selected playback.
        /// </summary>
        public IPlayback SelectedPlayback { get; set; }

        #endregion

        /// <summary>
        /// Set the selected playback.
        /// </summary>
        /// <param name="selectedPlayback">Selected playback.</param>
        public PlaybackEvent(IPlayback selectedPlayback)
        {
            SelectedPlayback = selectedPlayback;
        }
    }

    /// <summary>
    /// Display all the data.
    /// </summary>
    public class DisplayAllEvent
    {
        /// <summary>
        /// Do nothing.
        /// </summary>
        public DisplayAllEvent()
        {
        }
    }
}
