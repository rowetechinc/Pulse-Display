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
 * 12/06/2013      RC          3.2.0      Added BulkEnsembleEvent and SelectedEnsembleEvent.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// Source of the data for the ADCP.
    /// </summary>
    public enum EnsembleSource
    {
        /// <summary>
        /// Playback from a file.
        /// </summary>
        Playback,

        /// <summary>
        /// Data from the serial port.
        /// </summary>
        Serial,

        /// <summary>
        /// Data from the Ethernet port.
        /// </summary>
        Ethernet,

        /// <summary>
        /// Long term average.
        /// </summary>
        LTA,

        /// <summary>
        /// Short term average.
        /// </summary>
        STA
    }

    /// <summary>
    /// Type of ensemble.
    /// Single ensemble or averaged
    /// ensemble.
    /// </summary>
    public enum EnsembleType
    {
        /// <summary>
        /// A single ensemble.
        /// </summary>
        Single,

        /// <summary>
        /// Short term averaged ensemble.  This ensemble has
        /// multiple ensembles averaged together.
        /// </summary>
        STA,

        /// <summary>
        /// Long term averaged ensemble.  This ensemble has
        /// multiple ensembles averaged together.
        /// </summary>
        LTA
    }

    /// <summary>
    /// Event to pass around an ensemble.
    /// </summary>
    public class EnsembleEvent
    {
        #region Properties

        /// <summary>
        /// Set the source of the ensemble.  
        /// A live ensemble is an ensemble that is coming live from
        /// the ADCP.  It can come from the Serial or Ethnernt.  
        /// A playback ensemble is an ensemble that is playbacked
        /// from a project.
        /// </summary>
        public EnsembleSource Source { get; set; }

        /// <summary>
        /// Type of ensemble.
        /// Single or averaged ensemble.
        /// </summary>
        public EnsembleType Type { get; set; }

        /// <summary>
        /// Ensemble.
        /// </summary>
        public DataSet.Ensemble Ensemble { get; set; }

        #endregion

        /// <summary>
        /// Initialize the event.
        /// </summary>
        /// <param name="ensemble">Ensemble to send in event.</param>
        /// <param name="source">Source of the ensemble.</param>
        /// <param name="type">Type of ensemble: single or averaged.  Default is SINGLE.</param>
        public EnsembleEvent(DataSet.Ensemble ensemble, EnsembleSource source, EnsembleType type = EnsembleType.Single)
        {
            Ensemble = ensemble;
            Source = source;
            Type = type;
        }
    }

    /// <summary>
    /// An event to receive a bulk quanity of ensembles.
    /// </summary>
    public class BulkEnsembleEvent
    {
        #region Properties

        /// <summary>
        /// Set the source of the ensemble.  
        /// A live ensemble is an ensemble that is coming live from
        /// the ADCP.  It can come from the Serial or Ethnernt.  
        /// A playback ensemble is an ensemble that is playbacked
        /// from a project.
        /// </summary>
        public EnsembleSource Source { get; set; }

        /// <summary>
        /// Type of ensemble.
        /// Single or averaged ensemble.
        /// </summary>
        public EnsembleType Type { get; set; }

        /// <summary>
        /// Ensemble.
        /// </summary>
        public Cache<long, DataSet.Ensemble> Ensembles { get; set; }

        #endregion

        /// <summary>
        /// Receive a bulk package of ensembles. This is usually all the 
        /// ensembles in the project.
        /// The cache contains the row ID and the ensemble.
        /// </summary>
        /// <param name="ensembles">Cache of ensembles.</param>
        /// <param name="source">Source of the ensembles. Playback or live.</param>
        /// <param name="type">Type of ensembles: Averaged or not.</param>
        public BulkEnsembleEvent(Cache<long, DataSet.Ensemble> ensembles, EnsembleSource source, EnsembleType type = EnsembleType.Single)
        {
            Ensembles = ensembles;
            Source = source;
            Type = type;
        }

    }

    /// <summary>
    /// Display information about the selected ensemble.
    /// </summary>
    public class SelectedEnsembleEvent
    {
        #region Properties

        /// <summary>
        /// Set the source of the ensemble.  
        /// A live ensemble is an ensemble that is coming live from
        /// the ADCP.  It can come from the Serial or Ethnernt.  
        /// A playback ensemble is an ensemble that is playbacked
        /// from a project.
        /// </summary>
        public EnsembleSource Source { get; set; }

        /// <summary>
        /// Type of ensemble.
        /// Single or averaged ensemble.
        /// </summary>
        public EnsembleType Type { get; set; }

        /// <summary>
        /// Ensemble.
        /// </summary>
        public DataSet.Ensemble Ensemble { get; set; }

        #endregion

        /// <summary>
        /// Initialize the event.
        /// </summary>
        /// <param name="ensemble">Ensemble to send in event.</param>
        /// <param name="source">Source of the ensemble.</param>
        /// <param name="type">Type of ensemble: single or averaged.  Default is SINGLE.</param>
        /// <param name="ms">Mouse Selection.</param>
        public SelectedEnsembleEvent(DataSet.Ensemble ensemble, EnsembleSource source, EnsembleType type = EnsembleType.Single, ContourPlotMouseSelection ms = null)
        {
            Ensemble = ensemble;
            Source = source;
            Type = type;
        }
    }


}
