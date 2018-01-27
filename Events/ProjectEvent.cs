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
 * 06/26/2013      RC          3.0.2      Initial coding.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Event when the project is changed.
    /// If it added, deleted or changed.
    /// </summary>
    public class ProjectEvent
    {
        #region Enum

        /// <summary>
        /// Type of ensemble.
        /// Single ensemble or averaged
        /// ensemble.
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// A project has been selected.
            /// </summary>
            Select,

            /// <summary>
            /// A new project has been created.
            /// </summary>
            New,

            /// <summary>
            /// A project has been deleted.
            /// </summary>
            Delete
        }

        #endregion

        #region Properties

        /// <summary>
        /// Project that caused the event.
        /// Get the project from the PulseManager.
        /// This is used only for reference.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        ///  Type of event.
        /// </summary>
        public EventType Type { get; set; }

        #endregion


        /// <summary>
        /// Initialize the event.
        /// </summary>
        /// <param name="prj">Project that caused the event.</param>
        /// <param name="type">Type of event.  Default is SELECT.</param>
        public ProjectEvent(Project prj, EventType type = EventType.Select)
        {
            Project = prj;
            Type = type;
        }


    }
}
