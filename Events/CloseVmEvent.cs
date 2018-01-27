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
 * 08/20/2014      RC          4.0.1      Initial coding.
 * 
 */


namespace RTI
{
    /// <summary>
    /// Close the given VM.
    /// The VM that will be closed will 
    /// be based off the SubsystemConfig given.
    /// </summary>
    public class CloseVmEvent
    {
        #region Properties

        /// <summary>
        /// Subsystem Data Configuration to select the configuration to close.
        /// </summary>
        public SubsystemDataConfig SubsysDataConfig { get; set; }

        #endregion

        /// <summary>
        /// Give the SubsystemDataConfig to close.
        /// </summary>
        /// <param name="ssConfig">Subsystem config to close.</param>
        public CloseVmEvent(SubsystemDataConfig ssConfig)
        {
            SubsysDataConfig = ssConfig; 
        }
    }
}
