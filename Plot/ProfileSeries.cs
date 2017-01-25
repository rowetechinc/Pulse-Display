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
 * 02/01/2013      RC          2.18       Initial coding
 * 02/13/2013      RC          2.18       Changed GetTitle() to set the beam title to the coordinate transform description.
 * 07/09/2013      RC          3.0.3      Fixed bug when updating plot and checking for valid beam and bin.
 * 12/09/2013      RC          3.2.0      Changed UpdateWPCorrelationPlot() axis from bin to depth.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using OxyPlot;
    using OxyPlot.Series;

    /// <summary>
    /// Create a Profile object.  THis will keep track of the profile
    /// data based off the profile type given in the constructor.
    /// </summary>
    public class ProfileSeries : LineSeries
    {

        #region Variables

        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This is a bin number used to refresh the
        /// line series without giving a new bin number.
        /// This will allow the UpdateBinSelection() to 
        /// be used to refresh the line series for other
        /// reasons.
        /// </summary>
        public const int EMPTY_BIN = -1;

        /// <summary>
        /// This is a beam number used to refresh the
        /// line series without giving a new beam number.
        /// This will allow the UpdateBeamSelection() to 
        /// be used to refresh the line series for other
        /// reasons.
        /// </summary>
        public const int EMPTY_BEAM = -1;

        /// <summary>
        /// Set a maximum bin.  If this value is used, it means use the
        /// maximum allowed based off the ensemble.
        /// </summary>
        public const int MAX_BIN = DataSet.Ensemble.MAX_NUM_BINS;

        #endregion

        #region Properties

        /// <summary>
        /// The type of profile this object.
        /// </summary>
        public ProfileType Type { get; protected set; }

        /// <summary>
        /// Number of Beams for this profile.
        /// </summary>
        public int Beam { get; protected set; }


        /// <summary>
        /// Maximum number of Bin for this profile to display.
        /// </summary>
        public int MaxBins { get; protected set; }

        #endregion

        /// <summary>
        /// Create a Profile object.  THis will keep track of the profile
        /// data based off the profile type given in the constructor.
        /// </summary>
        /// <param name="type">Profile Type.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="maxBins">Number of bins to display.</param>
        /// <param name="color">Color of the profile line.</param>
        /// <param name="isFilterData">Flag for filtering the data for bad values.</param>
        /// <param name="list">Initial list of data to plot.</param>
        public ProfileSeries(ProfileType type, int beam, OxyColor color, bool isFilterData = true, int maxBins = MAX_BIN, List<DataSet.Ensemble> list = null)
        {
            // Initialize the values.
            Type = type;
            Beam = beam;
            MaxBins = maxBins;
            Color = color;

            // Create a line series with the title as the type title
            this.Title = GetTitle(Beam);

            if (list != null)
            {
                // Update the line series with the list of ensembles
                UpdateLineSeries(list, maxBins, isFilterData);
            }
        }

        #region Methods

        #region Title

        /// <summary>
        /// Create the title for the line series.
        /// </summary>
        /// <param name="beam">Beam Number for the series.</param>
        /// <returns>String for the series title.</returns>
        private string GetTitle(int beam)
        {
            // Determine the type
            string dataSetType = "";
            string beamType = "";
            switch (Type.Code)
            {
                case ProfileType.eProfileType.WP_Velocity_BEAM:
                    dataSetType = "WP";
                    beamType = DataSet.Ensemble.BeamBeamName(beam);
                    break;
                case ProfileType.eProfileType.WP_Velocity_ENU:
                    dataSetType = "WP";
                    beamType = DataSet.Ensemble.EarthBeamName(beam);
                    break;
                case ProfileType.eProfileType.WP_Velocity_XYZ:
                    dataSetType = "WP";
                    beamType = DataSet.Ensemble.InstrumentBeamName(beam);
                    break;
                case ProfileType.eProfileType.WP_Correlation:
                    dataSetType = "Corr";
                    beamType = DataSet.Ensemble.BeamBeamName(beam);
                    break;
                case ProfileType.eProfileType.WP_Amplitude:
                    dataSetType = "Amp";
                    beamType = DataSet.Ensemble.BeamBeamName(beam);
                    break;
                case ProfileType.eProfileType.WT_Velocity_BEAM:
                    dataSetType = "WT";
                    beamType = DataSet.Ensemble.BeamBeamName(beam);
                    break;
                case ProfileType.eProfileType.WT_Velocity_ENU:
                    dataSetType = "WT";
                    beamType = DataSet.Ensemble.EarthBeamName(beam);
                    break;
                case ProfileType.eProfileType.WT_Velocity_XYZ:
                    dataSetType = "WT";
                    beamType = DataSet.Ensemble.InstrumentBeamName(beam);
                    break;
                case ProfileType.eProfileType.WT_Correlation:
                    dataSetType = "WT";
                    beamType = DataSet.Ensemble.BeamBeamName(beam);
                    break;
                case ProfileType.eProfileType.WT_Amplitude:
                    dataSetType = "WT";
                    beamType = DataSet.Ensemble.BeamBeamName(beam);
                    break;
                default:
                    dataSetType = "";
                    break;
            }

            if (beam != EMPTY_BEAM)
            {
                return string.Format("{0} {1}", dataSetType, beamType);
            }
            else
            {
                return string.Format("{0}", dataSetType);
            }
        }

        #endregion

        #region Line Series

        /// <summary>
        /// Clear the series.
        /// </summary>
        public void ClearSeries()
        {
            // Clear the line series
            Points.Clear();
        }

        /// <summary>
        /// Update the series with the latest ensemble data.
        /// If MAX_BIN is used for maxBins, it will use the number of bins in the ensemble.
        /// </summary>
        /// <param name="ens">Latest ensemble.</param>
        /// <param name="maxBins">Maximum number of bins to display.  If MAX_BIN is used, it will use the number of bins in the ensemble.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        public void UpdateSeries(DataSet.Ensemble ens, int maxBins = MAX_BIN, bool isFilterData = true)
        {
            // Ensure the Beam and Bin is within the ensemble
            // If this is a vertical beam, it only has a single beam
            // even though generically the plot could have created 4+ beams
            if (ens.IsEnsembleAvail)
            {
                // Check if the Beam and Bin for this series is within the ensemble
                // Check if NumBeams is less then Beam for this series or
                // NumBins is less then Bin
                if (ens.EnsembleData.NumBeams <= Beam)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            // Clear series
            ClearSeries();

            // Update the line series
            UpdateLineSeries(ens, maxBins, isFilterData);
        }

        /// <summary>
        /// Add all the given ensembles in the list to the
        /// line series.
        /// </summary>
        /// <param name="list">List of ensembles.</param>
        /// <param name="maxBins">Maximum number of bins to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateLineSeries(List<DataSet.Ensemble> list, int maxBins, bool isFilterData)
        {
            // Clear the series
            ClearSeries();

            // Update the line series
            for (int x = 0; x < list.Count; x++)
            {
                UpdateLineSeries(list[x], maxBins, isFilterData);
            }
        }

        /// <summary>
        /// Based off the code, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxBin">Maximum number of bins to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateLineSeries(DataSet.Ensemble ensemble, int maxBin, bool isFilterData)
        {
            switch (Type.Code)
            {
                case ProfileType.eProfileType.WP_Velocity_BEAM:               // Water Profile Beam Velocity data
                    UpdateWPBeamVelocityPlot(ensemble, Beam, maxBin, isFilterData);
                    break;
                case ProfileType.eProfileType.WP_Velocity_XYZ:                // Water Profile Instrument Velocity data
                    UpdateWPInstrumentVelocityPlot(ensemble, Beam, maxBin, isFilterData);
                    break;
                case ProfileType.eProfileType.WP_Velocity_ENU:                // Water Profile Earth Velocity data
                    UpdateWPEarthVelocityPlot(ensemble, Beam, maxBin, isFilterData);
                    break;
                case ProfileType.eProfileType.WP_Amplitude:                   // Water Profile Amplitude data
                    UpdateWPAmplitudePlot(ensemble, Beam, maxBin);
                    break;
                case ProfileType.eProfileType.WP_Correlation:                 // Water Profile Correlation data
                    UpdateWPCorrelationPlot(ensemble, Beam, maxBin);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Update Series Type

        #region Beam Velocity Update

        /// <summary>
        /// Update the plot with the latest Beam Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="maxBins">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateWPBeamVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxBins, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBeamVelocityAvail)
            {
                // Check if MaxBins is set to max, if it is set to max, then use the number
                // number of bins in the ensemble
                if (maxBins == MAX_BIN)
                {
                    maxBins = ensemble.EnsembleData.NumBins;
                }

                // Ensure the bin and beam given are good
                if (beam >= ensemble.EnsembleData.NumBeams)
                {
                    beam = ensemble.EnsembleData.NumBeams - 1;
                }

                if (maxBins >= ensemble.EnsembleData.NumBins)
                {
                    maxBins = ensemble.EnsembleData.NumBins - 1;
                }

                // Add all the bin's beam data
                // Go only to maxBins
                for (int bin = 0; bin < maxBins; bin++)
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.BeamVelocityData.BeamVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        // Add the point to the series
                        Points.Add(new DataPoint(0.0f, bin));
                    }
                    else
                    {
                        // Add the point to the series
                        Points.Add(new DataPoint(ensemble.BeamVelocityData.BeamVelocityData[bin, beam], bin));
                    }
                }
            }
        }

        #endregion

        #region Earth Velocity Update

        /// <summary>
        /// Update the plot with the latest Earth Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="maxBins">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateWPEarthVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxBins, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail)
            {
                // Check if MaxBins is set to max, if it is set to max, then use the number
                // number of bins in the ensemble
                if (maxBins == MAX_BIN)
                {
                    maxBins = ensemble.EnsembleData.NumBins;
                }

                // Ensure the bin and beam given are good
                if (beam >= ensemble.EnsembleData.NumBeams)
                {
                    beam = ensemble.EnsembleData.NumBeams-1;
                }

                if (maxBins >= ensemble.EnsembleData.NumBins)
                {
                    maxBins = ensemble.EnsembleData.NumBins - 1;
                }

                // Add all the bin's beam data
                // Go only to maxBins
                for (int bin = 0; bin < maxBins; bin++)
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.EarthVelocityData.EarthVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        Points.Add(new DataPoint(0.0f, bin));
                    }
                    else
                    {
                        Points.Add(new DataPoint(ensemble.EarthVelocityData.EarthVelocityData[bin, beam], bin));
                    }
                }
            }
        }

        #endregion

        #region Instrument Velocity Update

        /// <summary>
        /// Update the plot with the latest Instrument Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="maxBins">Max number of Bins to display.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateWPInstrumentVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxBins, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentVelocityAvail)
            {
                // Check if MaxBins is set to max, if it is set to max, then use the number
                // number of bins in the ensemble
                if (maxBins == MAX_BIN)
                {
                    maxBins = ensemble.EnsembleData.NumBins;
                }

                // Ensure the bin and beam given are good
                if (beam >= ensemble.EnsembleData.NumBeams)
                {
                    beam = ensemble.EnsembleData.NumBeams - 1;
                }

                if (maxBins >= ensemble.EnsembleData.NumBins)
                {
                    maxBins = ensemble.EnsembleData.NumBins - 1;
                }

                // Add all the bin's beam data
                // Go only to maxBins
                for (int bin = 0; bin < maxBins; bin++)
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        Points.Add(new DataPoint(0.0f, bin));
                    }
                    else
                    {
                        Points.Add(new DataPoint(ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam], bin));
                    }
                }
            }
        }

        #endregion

        #region Amplitude Update

        /// <summary>
        /// Update the plot with the latest Amplitude Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="maxBins">Bin number.</param>
        private void UpdateWPAmplitudePlot(DataSet.Ensemble ensemble, int beam, int maxBins)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAmplitudeAvail && ensemble.AmplitudeData.AmplitudeData != null)
            {
                // Check if MaxBins is set to max, if it is set to max, then use the number
                // number of bins in the ensemble
                if (maxBins == MAX_BIN)
                {
                    maxBins = ensemble.EnsembleData.NumBins;
                }

                // Ensure the bin and beam given are good
                if (beam >= ensemble.EnsembleData.NumBeams)
                {
                    beam = ensemble.EnsembleData.NumBeams - 1;
                }

                if (maxBins >= ensemble.EnsembleData.NumBins)
                {
                    maxBins = ensemble.EnsembleData.NumBins - 1;
                }

                // Some times i get an index out of range error
                // Not sure whye
                try
                {
                    // Add all the bin's beam data
                    // Go only to maxBins
                    for (int bin = 0; bin < maxBins; bin++)
                    {
                        Points.Add(new DataPoint(ensemble.AmplitudeData.AmplitudeData[bin, beam], bin));
                    }
                }
                catch (Exception e)
                {
                    log.Error("Error adding points to profile series.", e);
                }
            }
        }

        #endregion

        #region Correlation Update

        /// <summary>
        /// Update the plot with the latest Correlation Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="maxBins">Max number of Bins to display.</param>
        private void UpdateWPCorrelationPlot(DataSet.Ensemble ensemble, int beam, int maxBins)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsCorrelationAvail && ensemble.CorrelationData.CorrelationData != null)
            {
                // Check if MaxBins is set to max, if it is set to max, then use the number
                // number of bins in the ensemble
                if (maxBins == MAX_BIN)
                {
                    maxBins = ensemble.EnsembleData.NumBins;
                }

                // Ensure the bin and beam given are good
                if (beam >= ensemble.EnsembleData.NumBeams)
                {
                    beam = ensemble.EnsembleData.NumBeams - 1;
                }

                if (maxBins >= ensemble.EnsembleData.NumBins)
                {
                    maxBins = ensemble.EnsembleData.NumBins - 1;
                }

                // Add all the bin's beam data
                // Go only to maxBins
                for (int bin = 0; bin < maxBins; bin++)
                {
                    // Convert the bin to a depth
                    float binSize = ensemble.AncillaryData.BinSize;
                    float firstBin = ensemble.AncillaryData.FirstBinRange;
                    float depth = firstBin + (bin * binSize);

                    // Turn the value into a percentage by multiplying by 100
                    Points.Add(new DataPoint(ensemble.CorrelationData.CorrelationData[bin, beam] * 100, depth));
                }
            }
        }

        #endregion

        #endregion

        #region Override

        /// <summary>
        /// Return the description as the string for this object.
        /// </summary>
        /// <returns>Return the description as the string for this object.</returns>
        public override string ToString()
        {
            return Title;
        }

        ///// <summary>
        ///// Determine if the 2 series given are the equal.
        ///// </summary>
        ///// <param name="series1">First series to check.</param>
        ///// <param name="series2">Series to check against.</param>
        ///// <returns>True if there codes match.</returns>
        //public static bool operator ==(TimeSeries series1, TimeSeries series2)
        //{
        //    // If both are null, or both are same instance, return true.
        //    if (System.Object.ReferenceEquals(series1, series2))
        //    {
        //        return true;
        //    }

        //    // If one is null, but not both, return false.
        //    if (((object)series1 == null) || ((object)series2 == null))
        //    {
        //        return false;
        //    }

        //    // Return true if the fields match:
        //    return (series1.Type.Code == series2.Type.Code && series1.Beam == series2.Beam && series1.Title == series2.Title);
        //}

        ///// <summary>
        ///// Return the opposite of ==.
        ///// </summary>
        ///// <param name="code1">First series to check.</param>
        ///// <param name="code2">Series to check against.</param>
        ///// <returns>Return the opposite of ==.</returns>
        //public static bool operator !=(SeriesType code1, SeriesType code2)
        //{
        //    return !(code1 == code2);
        //}

        ///// <summary>
        ///// Create a hashcode based off the Title stored.
        ///// </summary>
        ///// <returns>Hash the Code.</returns>
        //public override int GetHashCode()
        //{
        //    return Title.GetHashCode();
        //}

        ///// <summary>
        ///// Check if the given object is 
        ///// equal to this object.
        ///// </summary>
        ///// <param name="obj">Object to check.</param>
        ///// <returns>If the codes are the same, then they are equal.</returns>
        //public override bool Equals(object obj)
        //{
        //    //Check for null and compare run-time types.
        //    if (obj == null || GetType() != obj.GetType()) return false;

        //    TimeSeries p = (TimeSeries)obj;

        //    return (Type.Code == p.Type.Code && Beam == p.Beam && Title == p.Title);
        //}

        #endregion

        #endregion
    }
}
