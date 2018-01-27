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
 * 01/23/2015      RC          4.1.0      Initial coding.
 * 01/27/2015      RC          4.1.0      Added standard deviation.
 * 
 */

using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Minimum, maximum, Standard Deviation and Average series.
    /// </summary>
    public class MinMaxAvgStdSeries
    {
        #region Variables

        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Set a maximum bin.  If this value is used, it means use the
        /// maximum allowed based off the ensemble.
        /// </summary>
        public const int MAX_BIN = DataSet.Ensemble.MAX_NUM_BINS;

        /// <summary>
        /// Store the minimum, maximum and average value.
        /// </summary>
        private class MinMaxAvgStdVals
        {
            /// <summary>
            /// Minimum value.
            /// </summary>
            public float Min { get; set; }

            /// <summary>
            /// Maximum value.
            /// </summary>
            public float Max { get; set; }

            /// <summary>
            /// Average value.
            /// </summary>
            public float Avg { get; set; }

            /// <summary>
            /// Average count value.
            /// </summary>
            public int AvgCount { get; set; }

            /// <summary>
            /// Standard Deviation Old Measurement value.
            /// </summary>
            public float Std_OldM { get; set; }

            /// <summary>
            /// Standard Deviation New Measurement value.
            /// </summary>
            public float Std_NewM { get; set; }

            /// <summary>
            /// Standard Deviation Old Size value.
            /// </summary>
            public float Std_OldS { get; set; }

            /// <summary>
            /// Standard Deviation New Size value.
            /// </summary>
            public float Std_NewS { get; set; }

            /// <summary>
            /// Standard Deviation Count value.
            /// </summary>
            public int StdCount { get; set; }

            /// <summary>
            /// Velocity Difference Sum for the Bin to Bin standard deviation.
            /// </summary>
            public double StdB2BVelDiffSum { get; set; }

            /// <summary>
            /// Velocity Difference Sum Squared for the Bin to Bin standard deviation.
            /// </summary>
            public double StdB2BVelDiffSumSqr { get; set; }

            /// <summary>
            /// Velocity Difference Sum count for the Bin to Bin standard deviation.
            /// </summary>
            public int StdB2BVelDiffN { get; set; }

            /// <summary>
            /// Initialize the values.
            /// </summary>
            public MinMaxAvgStdVals()
            {
                Min = float.MaxValue;
                Max = float.MinValue;
                Avg = 0.0f;
                Std_NewM = 0.0f;
                Std_OldM = 0.0f;
                Std_NewS = 0.0f;
                Std_OldS = 0.0f;
                StdCount = 0;
                Avg = 0.0f;
                AvgCount = 0;
                StdB2BVelDiffSum = 0.0;
                StdB2BVelDiffSumSqr = 0.0;
                StdB2BVelDiffN = 0;
            }
        }

        /// <summary>
        /// Dictionary for the minimum, maximum and average value.
        /// </summary>
        private Dictionary<int, MinMaxAvgStdVals> _minMaxAvgStdDict { get; set; }

        #endregion

        #region Properities

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

        /// <summary>
        /// Plot color.
        /// </summary>
        public OxyColor Color { get; protected set; }

        /// <summary>
        /// Average points.  Used instead of Points if the user wants to display the average points.
        /// </summary>
        public LineSeriesWithToString AvgPoints { get; protected set; }

        /// <summary>
        /// Standard Deviation Ping to Ping points.
        /// </summary>
        public LineSeriesWithToString StdP2PPoints { get; protected set; }

        /// <summary>
        /// Standard Deviation Bin to Bin points.
        /// This standard deviation is used for the velocity data to remove the difference in boat
        /// speed and the boat moving around.
        /// </summary>
        public LineSeriesWithToString StdB2BPoints { get; protected set; }

        /// <summary>
        /// Minimum points.  This will display the minimum value for each bin.
        /// </summary>
        public ScatterSeriesWithToString MinPoints { get; protected set; }

        /// <summary>
        /// Maximum Points.  This will display the maximum value for each bin.
        /// </summary>
        public ScatterSeriesWithToString MaxPoints { get; protected set; }

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
        public MinMaxAvgStdSeries(ProfileType type, int beam, OxyColor color, bool isFilterData = true, int maxBins = MAX_BIN, List<DataSet.Ensemble> list = null)
        {
            // Initialize the values.
            Type = type;
            Beam = beam;
            MaxBins = maxBins;
            Color = color;

            _minMaxAvgStdDict = new Dictionary<int, MinMaxAvgStdVals>();

            MinPoints = new ScatterSeriesWithToString("Min " + beam.ToString(), color, 2);

            MaxPoints = new ScatterSeriesWithToString("Max " + beam.ToString(), color, 2);
            MaxPoints.MarkerType = MarkerType.Triangle;

            AvgPoints = new LineSeriesWithToString("Avg " + beam.ToString(), color);

            StdP2PPoints = new LineSeriesWithToString("Std P2P " + beam.ToString(), color);

            StdB2BPoints = new LineSeriesWithToString("Std B2B " + beam.ToString(), color);
        }

        /// <summary>
        /// Clear the series.
        /// </summary>
        public void ClearSeries()
        {
            if (_minMaxAvgStdDict != null)
            {
                _minMaxAvgStdDict.Clear();
            }

            if (AvgPoints != null)
            {
                AvgPoints.Points.Clear();
            }

            if (StdP2PPoints != null)
            {
                StdP2PPoints.Points.Clear();
            }

            if (StdB2BPoints != null)
            {
                StdB2BPoints.Points.Clear();
            }

            if (MinPoints != null)
            {
                MinPoints.Points.Clear();
            }

            if (MaxPoints != null)
            {
                MaxPoints.Points.Clear();
            }
        }

        #region Update Series Type


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

            // Update the line series
            UpdateLineSeries(ens, maxBins, isFilterData);
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

            // Clear the old points and update the new points
            MinPoints.Points.Clear();
            MaxPoints.Points.Clear();
            AvgPoints.Points.Clear();
            StdP2PPoints.Points.Clear();
            StdB2BPoints.Points.Clear();

            if (ensemble.IsAncillaryAvail)
            {
                // Convert the bin to a depth
                float binSize = ensemble.AncillaryData.BinSize;
                float firstBin = ensemble.AncillaryData.FirstBinRange;
                foreach (var pt in _minMaxAvgStdDict)
                {
                    // Set the bin
                    // Correlation uses depth instead of bin
                    float bin = pt.Key;
                    if (Type.Code == ProfileType.eProfileType.WP_Correlation)
                    {
                        bin = firstBin + (bin * binSize);
                    }

                    // Min Points
                    MinPoints.Points.Add(new ScatterPoint(pt.Value.Min, bin));

                    // Max points
                    MaxPoints.Points.Add(new ScatterPoint(pt.Value.Max, bin));

                    // Avg Points
                    float avg = 0.0f;
                    if (pt.Value.AvgCount > 0)
                    {
                        avg = pt.Value.Avg / pt.Value.AvgCount;
                    }
                    AvgPoints.Points.Add(new DataPoint(avg, bin));

                    // Standard Deviation Ping to Ping Points
                    StdP2PPoints.Points.Add(new DataPoint(StandardDeviation(pt.Key), bin));

                    // Standard Deviation Bin to Bin Points
                    if (pt.Value.StdB2BVelDiffN > 0)
                    {
                        var vds = pt.Value.StdB2BVelDiffSum;
                        var vdssqr = pt.Value.StdB2BVelDiffSumSqr;
                        var count = pt.Value.StdB2BVelDiffN;
                        var std = 0.707106781 * Math.Sqrt(vdssqr - (((vds * vds) / count) / count - 1));
                        StdB2BPoints.Points.Add(new DataPoint(std, bin));
                    }
                    else
                    {
                        if (isFilterData)
                        {
                            StdB2BPoints.Points.Add(new DataPoint(0.0f, bin));
                        }
                    }
                }
            }
        }

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
                    //// Check for bad velocity
                    //// If we are filtering data and the data is bad, then return and do not add the point
                    //if (isFilterData && ensemble.BeamVelocityData.BeamVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    //{
                    //    continue;
                    //}

                    // Check Min value
                    CheckMinValue(ensemble.BeamVelocityData.BeamVelocityData[bin, beam], bin);

                    // Check Max value
                    CheckMaxValue(ensemble.BeamVelocityData.BeamVelocityData[bin, beam], bin);

                    // Check Avg Value
                    CheckAvgValue(ensemble.BeamVelocityData.BeamVelocityData[bin, beam], bin);

                    // Check Ping to Ping Std Value
                    CheckP2PStdValue(ensemble.BeamVelocityData.BeamVelocityData[bin, beam], bin);

                    if((bin + 1) <= maxBins)
                    {
                        // Check Bin to Bin Std Value
                        CheckB2BStdValue(ensemble.BeamVelocityData.BeamVelocityData[bin, beam], ensemble.BeamVelocityData.BeamVelocityData[bin+1, beam], bin);
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
                    //// Check for bad velocity
                    //// If we are filtering data and the data is bad, then return and do not add the point
                    //if (isFilterData && ensemble.EarthVelocityData.EarthVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    //{
                    //    continue;
                    //}

                    // Check Min value
                    CheckMinValue(ensemble.EarthVelocityData.EarthVelocityData[bin, beam], bin);

                    // Check Max value
                    CheckMaxValue(ensemble.EarthVelocityData.EarthVelocityData[bin, beam], bin);

                    // Check Avg Value
                    CheckAvgValue(ensemble.EarthVelocityData.EarthVelocityData[bin, beam], bin);

                    if ((bin + 1) <= maxBins)
                    {
                        // Check Bin to Bin Std Value
                        CheckB2BStdValue(ensemble.EarthVelocityData.EarthVelocityData[bin, beam], ensemble.EarthVelocityData.EarthVelocityData[bin + 1, beam], bin);
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
                    //// Check for bad velocity
                    //// If we are filtering data and the data is bad, then return and do not add the point
                    //if (isFilterData && ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    //{
                    //    continue;
                    //}

                    // Check Min value
                    CheckMinValue(ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam], bin);

                    // Check Max value
                    CheckMaxValue(ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam], bin);

                    // Check Avg Value
                    CheckAvgValue(ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam], bin);

                    if ((bin + 1) <= maxBins)
                    {
                        // Check Bin to Bin Std Value
                        CheckB2BStdValue(ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam], ensemble.InstrumentVelocityData.InstrumentVelocityData[bin + 1, beam], bin);
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
                        // Check Min value
                        CheckMinValue(ensemble.AmplitudeData.AmplitudeData[bin, beam], bin);

                        // Check Max value
                        CheckMaxValue(ensemble.AmplitudeData.AmplitudeData[bin, beam], bin);

                        // Check Avg Value
                        CheckAvgValue(ensemble.AmplitudeData.AmplitudeData[bin, beam], bin);

                        // Check Std Value
                        CheckP2PStdValue(ensemble.AmplitudeData.AmplitudeData[bin, beam], bin);

                        if ((bin + 1) <= maxBins)
                        {
                            // Check Bin to Bin Std Value
                            CheckB2BStdValue(ensemble.AmplitudeData.AmplitudeData[bin, beam], ensemble.AmplitudeData.AmplitudeData[bin + 1, beam], bin);
                        }
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

                    // Check Min value
                    CheckMinValue(ensemble.CorrelationData.CorrelationData[bin, beam] * 100.0f, bin);

                    // Check Max value
                    CheckMaxValue(ensemble.CorrelationData.CorrelationData[bin, beam] * 100.0f, bin);

                    // Check Avg value
                    CheckAvgValue(ensemble.CorrelationData.CorrelationData[bin, beam] * 100.0f, bin);

                    // Check Std value
                    CheckP2PStdValue(ensemble.CorrelationData.CorrelationData[bin, beam] * 100.0f, bin);

                    if ((bin + 1) <= maxBins)
                    {
                        // Check Bin to Bin Std Value
                        CheckB2BStdValue(ensemble.CorrelationData.CorrelationData[bin, beam] * 100.0f, ensemble.CorrelationData.CorrelationData[bin + 1, beam] * 100.0f, bin);
                    }
                }
            }
        }

        #endregion

        #region Min Value

        /// <summary>
        /// Check the minimum value.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="bin">Bin to check.</param>
        private void CheckMinValue(float value, int bin)
        {
            // Check if the value is created
            if (!_minMaxAvgStdDict.ContainsKey(bin))
            {
                _minMaxAvgStdDict.Add(bin, new MinMaxAvgStdVals());
            }

            // Check for bad velocity
            if(value == DataSet.Ensemble.BAD_VELOCITY)
            {
                _minMaxAvgStdDict[bin].Min = 0.0f;
            }
            // Check if the value is less
            else if (_minMaxAvgStdDict[bin].Min > value)
            {
                _minMaxAvgStdDict[bin].Min = value;
            }
        }

        #endregion

        #region Max Value

        /// <summary>
        /// Check the maximum value.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="bin">Bin to check.</param>
        private void CheckMaxValue(float value, int bin)
        {
            // Check if the value is created
            if (!_minMaxAvgStdDict.ContainsKey(bin))
            {
                _minMaxAvgStdDict.Add(bin, new MinMaxAvgStdVals());
            }

            // Check for bad velocity
            if (value == DataSet.Ensemble.BAD_VELOCITY)
            {
                _minMaxAvgStdDict[bin].Max = 0.0f;
            }
            // Check if the value is less
            else if (_minMaxAvgStdDict[bin].Max < value)
            {
                _minMaxAvgStdDict[bin].Max = value;
            }
        }

        #endregion

        #region Avg Value

        /// <summary>
        /// Check the average value.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="bin">Bin to check.</param>
        private void CheckAvgValue(float value, int bin)
        {
            // Check if the value is created
            if (!_minMaxAvgStdDict.ContainsKey(bin))
            {
                _minMaxAvgStdDict.Add(bin, new MinMaxAvgStdVals());
            }

            // Check if the value is less
            if (value != DataSet.Ensemble.BAD_VELOCITY)
            {
                _minMaxAvgStdDict[bin].Avg += value;
                _minMaxAvgStdDict[bin].AvgCount++;
            }
        }

        #endregion

        #region Ping to Ping Std Value

        /// <summary>
        /// Check the ping to ping standard deviation value.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="bin">Bin to check.</param>
        private void CheckP2PStdValue(float value, int bin)
        {
            // Check if the value is created
            if (!_minMaxAvgStdDict.ContainsKey(bin))
            {
                _minMaxAvgStdDict.Add(bin, new MinMaxAvgStdVals());
            }

            // Check if the value is less
            if (value != DataSet.Ensemble.BAD_VELOCITY)
            {
                PushStdValue(value, bin);
            }
        }

        /// <summary>
        /// Store the new value to the bin.
        /// </summary>
        /// <param name="value">Value to store.</param>
        /// <param name="bin">Bin to store the value.</param>
        private void PushStdValue(float value, int bin)
        {
            _minMaxAvgStdDict[bin].StdCount++;

            // See Knuth TAOCP vol 2, 3rd edition, page 232
            if (_minMaxAvgStdDict[bin].StdCount == 1)
            {
                _minMaxAvgStdDict[bin].Std_OldM = _minMaxAvgStdDict[bin].Std_NewM = value;
                _minMaxAvgStdDict[bin].Std_OldS = 0.0f;
            }
            else
            {
                _minMaxAvgStdDict[bin].Std_NewM = _minMaxAvgStdDict[bin].Std_OldM + (value - _minMaxAvgStdDict[bin].Std_OldM) / _minMaxAvgStdDict[bin].StdCount;
                _minMaxAvgStdDict[bin].Std_NewS = _minMaxAvgStdDict[bin].Std_OldS + (value - _minMaxAvgStdDict[bin].Std_OldM) * (value - _minMaxAvgStdDict[bin].Std_NewM);
    
                // set up for next iteration
                _minMaxAvgStdDict[bin].Std_OldM = _minMaxAvgStdDict[bin].Std_NewM;
                _minMaxAvgStdDict[bin].Std_OldS = _minMaxAvgStdDict[bin].Std_NewS;
            }
        }

        /// <summary>
        /// Caluculate the mean value.
        /// </summary>
        /// <param name="bin">Bin number.</param>
        /// <returns>Mean value for the given bin.</returns>
        private double StdMean(int bin)
        {
            return (_minMaxAvgStdDict[bin].StdCount > 0) ? _minMaxAvgStdDict[bin].Std_NewM : 0.0;
        }

        /// <summary>
        /// Calculate the variance value.
        /// </summary>
        /// <param name="bin">Bin number.</param>
        /// <returns>Variance value for the given bin.</returns>
        private double StdVariance(int bin)
        {
            return ((_minMaxAvgStdDict[bin].StdCount > 1) ? _minMaxAvgStdDict[bin].Std_NewS / (_minMaxAvgStdDict[bin].StdCount - 1) : 0.0);
        }

        /// <summary>
        /// Calculate the standard deviation value.
        /// </summary>
        /// <param name="bin">Bin number.</param>
        /// <returns>Standard deviation value for the given bin.</returns>
        private double StandardDeviation(int bin)
        {
            return Math.Sqrt( StdVariance(bin) );
        }

        #endregion

        #region Bin to Bin Std Value

        /// <summary>
        /// Check the bin to bin standard deviation value.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="nextValue">Next Value to check.</param>
        /// <param name="bin">Bin to check.</param>
        private void CheckB2BStdValue(float value, float nextValue, int bin)
        {
            // Check if the value is created
            if (!_minMaxAvgStdDict.ContainsKey(bin))
            {
                _minMaxAvgStdDict.Add(bin, new MinMaxAvgStdVals());
            }

            // Check if the value is less
            if (value != DataSet.Ensemble.BAD_VELOCITY && nextValue != DataSet.Ensemble.BAD_VELOCITY)
            {
                var vds = nextValue - value;
                _minMaxAvgStdDict[bin].StdB2BVelDiffSum += vds;
                _minMaxAvgStdDict[bin].StdB2BVelDiffSumSqr += vds * vds;
                _minMaxAvgStdDict[bin].StdB2BVelDiffN++;
            }
        }

        #endregion

        #endregion

    }
}
