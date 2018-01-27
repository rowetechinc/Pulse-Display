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
 * 07/26/2013      RC          3.0.5      Initial coding.
 * 
 */

namespace RTI
{

    /// <summary>
    /// Event when the project is changed.
    /// If it added, deleted or changed.
    /// </summary>
    public class EnsembleWriteEvent
    {
        #region Enum

        /// <summary>
        /// Write location.  This will be where the data was written to.
        /// Either the project or binary file.
        /// </summary>
        public enum WriteLocation
        {
            /// <summary>
            /// Ensemble written to the binary file.
            /// The count will be the file size in bytes.
            /// </summary>
            Binary,

            /// <summary>
            /// Ensemble written to the Project file.
            /// The count will be the number of ensembles in
            /// the project.
            /// </summary>
            Project
        }

        #endregion

        #region Properties

        /// <summary>
        /// Location where the ensemble was written.
        /// Either the binary file or the Project file.
        /// </summary>
        public WriteLocation Loc { get; set; }

        /// <summary>
        /// Binary: File size in bytes.
        ///  
        /// Project: Number of ensembles in the project.
        /// </summary>
        public long Count { get; set; }

        #endregion


        /// <summary>
        /// Initialize the event.
        /// </summary>
        /// <param name="loc">Where ensemble was written.</param>
        /// <param name="count">Either file size or number of ensembles.</param>
        public EnsembleWriteEvent(WriteLocation loc, long count)
        {
            Loc = loc;
            Count = count;
        }


    }
}
