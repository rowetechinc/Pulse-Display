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
 * Date            Initials    Vertion    Comments
 * -----------------------------------------------------------------
 * 12/07/2012      RC          2.17       Initial coding
 * 02/13/2013      RC          2.18       Changed GetTitle() to set the beam title to the coordinate transform description.
 * 07/16/2013      RC          3.0.4      Changed SeriesType.
 * 07/26/2013      RC          3.0.6      Fixed a bug for vertical beams in UpdateSeries().
 * 07/24/2014      RC          3.4.0      Fixed bug in UpdateSeries() when DVL data has no bins.
 * 10/07/2014      RC          4.1.0      Added Bottom Track speed and Water Track plots.
 * 03/02/2015      RC          4.1.0      Added SystemSetup and Range Tracking.
 * 10/15/2015      RC          4.3.0      Added new constructor.  Added Transducer Depth plot. 
 * 11/25/2015      RC          4.3.1      Added NMEA Heading and speed.
 * 12/04/2015      RC          4.4.0      Added DVL data to TimeSeries.  This includes Ship Velocity.
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
    /// TODO: Update summary.
    /// </summary>
    public class TimeSeries : LineSeries
    {
        #region Variables

        /// <summary>
        /// This is a bin number used to refresh the
        /// line series without giving a new bin number.
        /// This will allow the UpdateBinSelection() to 
        /// be used to refresh the line series for other
        /// reasons.
        /// </summary>
        public const int EMPTY_BIN = -1;

        #endregion

        #region Properties

        /// <summary>
        /// The type of line series this series is.
        /// </summary>
        public SeriesType Type { get; protected set; }

        /// <summary>
        /// Beam number for this series.
        /// </summary>
        public int Beam { get; protected set; }


        /// <summary>
        /// Bin number for this series.
        /// </summary>
        public int Bin { get; protected set; }

        #endregion

        /// <summary>
        /// Create a time series.  This will get the series
        /// type, beam number and bin number.  This will repersent
        /// one line on the TimeSeries plot.  Set the list to null
        /// if no initial data is available.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <param name="beam">Beam number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="color">Color for the plot.</param>
        /// <param name="isFilterData">Filter the data of all the bad values.</param>
        /// <param name="list">List of ensembles.  Set to NULL if no data will be given.</param>
        public TimeSeries(SeriesType type, int beam, int bin, OxyColor color, bool isFilterData = true, List<DataSet.Ensemble> list = null)
        {
            // Initialize the values.
            Type = type;
            Beam = beam;
            Bin = bin;
            Color = color;

            // Create a line series with the title as the type title
            //_lineSeries = new LineSeries(GetTitle(Bin, Beam));
            this.Title = GetTitle(Bin, Beam);

            if (list != null)
            {
                // Update the line series with the list of ensembles
                UpdateLineSeries(list, isFilterData);
            }
        }

        /// <summary>
        /// Create a time series.  This will get the series
        /// type, beam number and bin number.  This will repersent
        /// one line on the TimeSeries plot.  Set the list to null
        /// if no initial data is available.
        /// </summary>
        /// <param name="title">Title of series.</param>
        /// <param name="color">Color for the plot.</param>
        public TimeSeries(string title, OxyColor color)
        {
            // Initialize the values.
            Color = color;
            Title = title;
        }

        #region Types

        /// <summary>
        /// Based off the type, determine if the series
        /// will contain bin data or just beam data.  Bin data
        /// is data that has beam data for each beam.  Non Bin data
        /// is data like bottom track where bins do not exist.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IfSeriesHaveBins(SeriesType type)
        {
            // Only Water Profile has bins
            if (type.Source.Source != DataSource.eSource.WaterProfile)
            {
                return false;
            }

            // Based off the type
            switch(type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:
                    return true;
                default:
                    return false;
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
        /// </summary>
        /// <param name="ens">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        public void UpdateSeries(DataSet.Ensemble ens, int maxEnsembles, bool isFilterData = true)
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

                // DVL data has no bins, so do not check number of bins is bins = 0
                // Check if the bin number is valid
                if (ens.EnsembleData.NumBins > 0 && ens.EnsembleData.NumBins <= Bin)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            // Update the line series
            UpdateLineSeries(ens, maxEnsembles, isFilterData);
        }

        /// <summary>
        /// Update the bin selection.  If the bin selection has changed,
        /// update the line series with the new bin selection and new
        /// list of data.
        /// </summary>
        /// <param name="bin">New bin selected.</param>
        /// <param name="list">List of ensembles to update.</param>
        /// <param name="isFilterData">Filter the data of any error values.</param>
        public void UpdateBinSelection(int bin, List<DataSet.Ensemble> list, bool isFilterData = true)
        {
            // Clear the line series
            ClearSeries();

            // Update the bin selection
            // EMPTY_BIN is used to refresh the line series
            // Values like isFilterData could have changed
            if (bin != EMPTY_BIN)
            {
                Bin = bin;
                Title = GetTitle(Bin, Beam);
            }

            // Update the line series
            UpdateLineSeries(list, isFilterData);
        }

        /// <summary>
        /// Create the title for the line series.
        /// </summary>
        /// <param name="bin">Bin Number for the series.</param>
        /// <param name="beam">Beam Number for the series.</param>
        /// <returns>String for the series title.</returns>
        private string GetTitle(int bin, int beam)
        {
            // Determine the type
            string dataSetType = "";
            string beamType = "";
            string binType = "";
            switch (Type.Source.Source)
            {
                case DataSource.eSource.WaterProfile:
                    {
                        dataSetType = "WP";

                        switch (Type.Type.Code)
                        {
                            case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                binType = string.Format("Bin:{0}", bin);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                                beamType = DataSet.Ensemble.EarthBeamName(beam);
                                binType = string.Format("Bin:{0}", bin);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                                beamType = DataSet.Ensemble.InstrumentBeamName(beam);
                                binType = string.Format("Bin:{0}", bin);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Correlation:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                binType = string.Format("Bin:{0}", bin);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Amplitude:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                binType = string.Format("Bin:{0}", bin);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Water_Magnitude:
                                beamType = "Mag";
                                binType = string.Format("Bin:{0}", bin);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Water_Direction:
                                beamType = "Dir";
                                binType = string.Format("Bin:{0}", bin);
                                break;
                        }

                    }
                    break;
                case DataSource.eSource.BottomTrack:
                    {
                        dataSetType = "BT";

                        switch (Type.Type.Code)
                        {
                            case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                                beamType = DataSet.Ensemble.EarthBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                                beamType = DataSet.Ensemble.InstrumentBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Correlation:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Amplitude:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_SNR:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Range:
                                beamType = DataSet.Ensemble.BeamBeamName(beam);
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Speed:
                                beamType = "Speed";
                                break;
                        }
                    }
                    break;
                case DataSource.eSource.WaterTrack:
                    dataSetType = "WT";

                    switch (Type.Type.Code)
                    {
                        case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                            beamType = DataSet.Ensemble.EarthBeamName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                            beamType = DataSet.Ensemble.InstrumentBeamName(beam);
                            break;
                    }

                    break;
                case DataSource.eSource.AncillaryWaterProfile:
                    {
                        dataSetType = "WP";

                        switch (Type.Type.Code)
                        {
                            case BaseSeriesType.eBaseSeriesType.Base_Heading:
                                beamType = "Hdg";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Pitch:
                                beamType = "Pitch";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Roll:
                                beamType = "Roll";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:
                                beamType = "Sys Temp";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:
                                beamType = "Water Temp";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Pressure:
                                beamType = "Pressure";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_TransducerDepth:
                                beamType = "Transducer Depth";
                                break;
                        }
                    }
                    break;
                case DataSource.eSource.AncillaryBottomTrack:
                    {
                        dataSetType = "BT";

                        switch (Type.Type.Code)
                        {
                            case BaseSeriesType.eBaseSeriesType.Base_Heading:
                                beamType = "Hdg";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Pitch:
                                beamType = "Pitch";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Roll:
                                beamType = "Roll";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:
                                beamType = "Sys Temp";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:
                                beamType = "Water Temp";
                                break;
                            case BaseSeriesType.eBaseSeriesType.Base_Speed:
                                beamType = "Speed";
                                break;
                        }
                    }
                    break;
                case DataSource.eSource.RangeTracking:
                    dataSetType = "RT";

                    switch (Type.Type.Code)
                    {
                        case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Range:
                            beamType = DataSet.Ensemble.RangeTrackingName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_SNR:
                            beamType = DataSet.Ensemble.RangeTrackingName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Pings:
                            beamType = DataSet.Ensemble.RangeTrackingName(beam);
                            break;
                    }

                    break;
                case DataSource.eSource.SystemSetup:
                    dataSetType = "SS";

                    switch (Type.Type.Code)
                    {
                        case BaseSeriesType.eBaseSeriesType.Base_SystemSetup_Voltage:
                            beamType = "Voltage";
                            break;
                    }

                    break;

                case DataSource.eSource.NMEA:
                    dataSetType = "NMEA";

                    switch (Type.Type.Code)
                    {
                        case BaseSeriesType.eBaseSeriesType.Base_NMEA_Heading:
                            beamType = "Heading";
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_NMEA_Speed:
                            beamType = "Speed";
                            break;
                    }

                    break;

                case DataSource.eSource.DVL:
                    dataSetType = "DVL";

                    switch (Type.Type.Code)
                    {
                        case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                            beamType = DataSet.Ensemble.BeamBeamName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                            beamType = DataSet.Ensemble.EarthBeamName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                            beamType = DataSet.Ensemble.InstrumentBeamName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Velocity_Ship:
                            beamType = DataSet.Ensemble.ShipBeamName(beam);
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Heading:
                            beamType = "Hdg";
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Pitch:
                            beamType = "Pitch";
                            break;
                        case BaseSeriesType.eBaseSeriesType.Base_Roll:
                            beamType = "Roll";
                            break;
                    }

                    break;

                default:
                    break;
            }

            if (bin == EMPTY_BIN)
            {
                return string.Format("{0} {1}", dataSetType, beamType);
            }
            else
            {
                return string.Format("{0} {1} {2}", dataSetType, beamType, binType);
            }
        }



        /// <summary>
        /// Add all the given ensembles in the list to the
        /// line series.
        /// </summary>
        /// <param name="list">List of ensembles.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateLineSeries(List<DataSet.Ensemble> list, bool isFilterData)
        {
            // Update the line series
            for (int x = 0; x < list.Count; x++)
            {
                UpdateLineSeries(list[x], list.Count, isFilterData);
            }
        }

        /// <summary>
        /// Based off the code, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateLineSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch(Type.Source.Source)
            {
                case DataSource.eSource.WaterProfile:
                case DataSource.eSource.AncillaryWaterProfile:
                    UpdateWaterProfileSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                case DataSource.eSource.BottomTrack:
                case DataSource.eSource.AncillaryBottomTrack:
                    UpdateBottomTrackSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                case DataSource.eSource.WaterTrack:
                    UpdateWaterTrackSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                case DataSource.eSource.RangeTracking:
                    UpdateRangeTrackingSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                case DataSource.eSource.SystemSetup:
                    UpdateSystemSetupSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                case DataSource.eSource.NMEA:
                    UpdateNmeaSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                case DataSource.eSource.DVL:
                    UpdateDvlSeries(ensemble, maxEnsembles, isFilterData);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateWaterProfileSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch(Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:               // Water Profile Beam Velocity data
                    UpdateWPBeamVelocityPlot(ensemble, Beam, Bin, maxEnsembles, isFilterData);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:                // Water Profile Instrument Velocity data
                    UpdateWPInstrumentVelocityPlot(ensemble, Beam, Bin, maxEnsembles, isFilterData);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:                // Water Profile Earth Velocity data
                    UpdateWPEarthVelocityPlot(ensemble, Beam, Bin, maxEnsembles, isFilterData);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Amplitude:                   // Water Profile Amplitude data
                    UpdateWPAmplitudePlot(ensemble, Beam, Bin, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Correlation:                 // Water Profile Correlation data
                    UpdateWPCorrelationPlot(ensemble, Beam, Bin, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:                     // Water Profile Heading data
                    UpdateHeadingPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:                       // Water Profile Pitch data
                    UpdatePitchPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:                        // Water Profile Roll data
                    UpdateRollPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pressure:                    // Water Profile Pressure data
                    UpdatePressurePlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_TransducerDepth:             // Water Profile Transducer Depth data
                    UpdateTransducerDepthPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Sys:             // Water Profile System Temperature data
                    UpdateSystemTemperaturePlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Temperature_Water:           //Water Profile  Water Temperature data
                    UpdateWaterTemperaturePlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Water_Magnitude:             //Water Profile  Magnitude data
                    UpdateMagnitudePlot(ensemble, Bin, maxEnsembles, isFilterData);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Water_Direction:             //Water Profile  Direction data
                    UpdateDirectionPlot(ensemble, Bin, maxEnsembles, isFilterData);
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateBottomTrackSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch(Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_SNR:                         // Bottom Track SNR data
                    UpdateSnrPlot(ensemble, Beam, maxEnsembles, isFilterData);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Range:                       // Bottom Track Range data
                    UpdateRangePlot(ensemble, Beam, maxEnsembles, isFilterData);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Beam:
                    UpdateBTBeamVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);   // Bottom Track Beam Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    UpdateBTInstrumentVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);     // Bottom Track Instrument Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    UpdateBTEarthVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);  // Bottom Track Earth Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:                     // Bottom Track Heading data
                    UpdateBTHeadingPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:                       // Bottom Track Pitch data
                    UpdateBTPitchPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:                        // Bottom Track Roll data
                    UpdateBTRollPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Speed:                       // Bottom Track Speed data
                    UpdateBTSpeedPlot(ensemble, maxEnsembles);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateWaterTrackSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch (Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    UpdateWTInstrumentVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);     // Water Track Instrument Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    UpdateWTEarthVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);          // Water Track Earth Velocity
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateRangeTrackingSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch (Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Range:
                    UpdateRangeTrackingRangePlot(ensemble, Beam, maxEnsembles, isFilterData);     // Range Tracking Range
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_SNR:
                    UpdateRangeTrackingSnrPlot(ensemble, Beam, maxEnsembles, isFilterData);       // Range Tracking SNR
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_RangeTracking_Pings:
                    UpdateRangeTrackingPingsPlot(ensemble, Beam, maxEnsembles, isFilterData);       // Range Tracking Pings
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateSystemSetupSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch (Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_SystemSetup_Voltage:
                    UpdateSystemSetupVoltagePlot(ensemble, Beam, maxEnsembles, isFilterData);     // System Setup Voltage
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateNmeaSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch (Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Heading:
                    UpdateNmeaHeadingPlot(ensemble, Beam, maxEnsembles, isFilterData);      // NMEA Heading
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_NMEA_Speed:
                    UpdateNmeaSpeedPlot(ensemble, Beam, maxEnsembles, isFilterData);        // NMEA Speed
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based off the Series Type, call the apporiate update method.
        /// Each method will take the approriate data from the
        /// ensemble to update the line series.
        /// </summary>
        /// <param name="ensemble">Latest ensemble data.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        private void UpdateDvlSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData)
        {
            switch (Type.Type.Code)
            {
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_XYZ:
                    UpdateDvlInstrumentVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);    // DVL Instrument Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_ENU:
                    UpdateDvlEarthVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);         // DVL Earth Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Velocity_Ship:
                    UpdateDvlShipVelocityPlot(ensemble, Beam, maxEnsembles, isFilterData);          // DVL Ship Velocity
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Heading:                                   // DVL Heading data
                    UpdateDvlHeadingPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Pitch:                                     // DVL Pitch data
                    UpdateDvlPitchPlot(ensemble, maxEnsembles);
                    break;
                case BaseSeriesType.eBaseSeriesType.Base_Roll:                                      // DVL Roll data
                    UpdateDvlRollPlot(ensemble, maxEnsembles);
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
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateWPBeamVelocityPlot(DataSet.Ensemble ensemble, int beam, int bin, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBeamVelocityAvail)
            {
                // Ensure the bin and beam given are good
                if (beam > ensemble.EnsembleData.NumBeams || bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.BeamVelocityData.BeamVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        return;
                    }

                    //_lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(ensemble.EnsembleData.EnsDateTime), ensemble.BeamVelocityData.BeamVelocityData[bin, beam]));
                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BeamVelocityData.BeamVelocityData[bin, beam]));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Instrument Velocity Update

        /// <summary>
        /// Update the plot with the latest Instrument Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateWPInstrumentVelocityPlot(DataSet.Ensemble ensemble, int beam, int bin, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentVelocityAvail)
            {
                // Ensure the bin and beam given are good
                if (beam > ensemble.EnsembleData.NumBeams || bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        return;
                    }

                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, beam]));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Earth Velocity Update

        /// <summary>
        /// Update the plot with the latest Earth Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateWPEarthVelocityPlot(DataSet.Ensemble ensemble, int beam, int bin, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail)
            {
                // Ensure the bin and beam given are good
                if (beam > ensemble.EnsembleData.NumBeams || bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.EarthVelocityData.EarthVelocityData[bin, beam] == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        return;
                    }
                    
                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.EarthVelocityData.EarthVelocityData[bin, beam]));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Velocity Magnitude Update

        /// <summary>
        /// Update the plot with the latest Earth Velocity Magnitude.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateMagnitudePlot(DataSet.Ensemble ensemble, int bin, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {

                // Ensure the bin given is good
                if (bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        return;
                    }

                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Velocity Magnitude Update

        /// <summary>
        /// Update the plot with the latest Earth Velocity Magnitude.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        private void UpdateDirectionPlot(DataSet.Ensemble ensemble, int bin, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {

                // Ensure the bin given is good
                if (bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    // Check for bad velocity
                    // If we are filtering data and the data is bad, then return and do not add the point
                    if (isFilterData && ensemble.EarthVelocityData.VelocityVectors[bin].Magnitude == DataSet.Ensemble.BAD_VELOCITY)
                    {
                        return;
                    }

                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.EarthVelocityData.VelocityVectors[bin].DirectionXNorth));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Amplitude Update

        /// <summary>
        /// Update the plot with the latest Amplitude Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        private void UpdateWPAmplitudePlot(DataSet.Ensemble ensemble, int beam, int bin, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAmplitudeAvail)
            {
                // Ensure the bin and beam given are good
                if (beam > ensemble.EnsembleData.NumBeams || bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AmplitudeData.AmplitudeData[bin, beam]));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Correlation Update

        /// <summary>
        /// Update the plot with the latest Correlation Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Beam Number.</param>
        /// <param name="bin">Bin number.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        private void UpdateWPCorrelationPlot(DataSet.Ensemble ensemble, int beam, int bin, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsCorrelationAvail)
            {
                // Ensure the bin and beam given are good
                if (beam > ensemble.EnsembleData.NumBeams || bin > ensemble.EnsembleData.NumBins)
                {
                    return;
                }
                else
                {
                    Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, (ensemble.CorrelationData.CorrelationData[bin, beam] * 100)));
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Bottom Track Beam Velocity Update

        /// <summary>
        /// Update the plot with the latest Bottom Track Beam Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateBTBeamVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.NumBeams > beam)
            {
                // Check for bad velocity
                // If we are filtering data and the data is bad, then return and do not add the point
                if (isFilterData && ensemble.BottomTrackData.BeamVelocity[beam] == DataSet.Ensemble.BAD_VELOCITY)
                {
                    return;
                }

                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.BeamVelocity[beam]));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Bottom Track Instrument Velocity Update

        /// <summary>
        /// Update the plot with the latest Bottom Track Instrument Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateBTInstrumentVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.NumBeams > beam)
            {
                // Check for bad velocity
                // If we are filtering data and the data is bad, then return and do not add the point
                if (isFilterData && ensemble.BottomTrackData.InstrumentVelocity[beam] == DataSet.Ensemble.BAD_VELOCITY)
                {
                    return;
                }

                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.InstrumentVelocity[beam]));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Bottom Track Earth Velocity Update

        /// <summary>
        /// Update the plot with the latest Bottom Track Earth Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateBTEarthVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.NumBeams > beam)
            {
                // Check for bad velocity
                // If we are filtering data and the data is bad, then return and do not add the point
                if (isFilterData && ensemble.BottomTrackData.EarthVelocity[beam] == DataSet.Ensemble.BAD_VELOCITY)
                {
                    return;
                }

                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.EarthVelocity[beam]));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region SNR Update

        /// <summary>
        /// Update the plot with the latest SNR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateSnrPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.NumBeams > beam)
            {
                // Check for bad velocity
                // If we are filtering data and the data is bad, then return and do not add the point
                if (isFilterData && ensemble.BottomTrackData.SNR[beam] == DataSet.Ensemble.BAD_VELOCITY)
                {
                    return;
                }

                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.SNR[beam]));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Range Update

        /// <summary>
        /// Update the plot with the latest Range Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateRangePlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.NumBeams > beam)
            {
                // Check for bad velocity
                // If we are filtering data and the data is bad, then return and do not add the point
                if (isFilterData && ensemble.BottomTrackData.Range[beam] == DataSet.Ensemble.BAD_RANGE)
                {
                    return;
                }

                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.Range[beam]));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Water Profile Heading, Pitch and Roll Update

        /// <summary>
        /// Update the plot with the latest HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateHeadingPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.Heading));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdatePitchPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.Pitch));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateRollPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.Roll));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Bottom Track Heading, Pitch and Roll Update

        /// <summary>
        /// Update the plot with the latest BT HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateBTHeadingPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.Heading));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest BT HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateBTPitchPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.Pitch));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest BT HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateBTRollPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.Roll));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Bottom Track Speed

        /// <summary>
        /// Update the plot with the latest Bottom Track Speed Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateBTSpeedPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.BottomTrackData.GetVelocityMagnitude()));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Pressure Update

        /// <summary>
        /// Update the plot with the latest Pressure Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdatePressurePlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.Pressure));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Transducer Depth Pressure Update

        /// <summary>
        /// Update the plot with the latest Transducer Depth Pressure Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateTransducerDepthPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.TransducerDepth));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Temperature Update

        /// <summary>
        /// Update the plot with the latest Temperature Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points for the series.</param>
        private void UpdateWaterTemperaturePlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.WaterTemp));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest Temperature Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points for the series.</param>
        private void UpdateSystemTemperaturePlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.AncillaryData.SystemTemp));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Water Track Instrument Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Track Instrument Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateWTInstrumentVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentWaterMassAvail)
            {
                switch(beam)
                {
                    case DataSet.Ensemble.BEAM_X_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.InstrumentWaterMassData.VelocityX == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.InstrumentWaterMassData.VelocityX));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_Y_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.InstrumentWaterMassData.VelocityY == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.InstrumentWaterMassData.VelocityY));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_Z_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.InstrumentWaterMassData.VelocityZ == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.InstrumentWaterMassData.VelocityZ));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_Q_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.InstrumentWaterMassData.VelocityQ == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.InstrumentWaterMassData.VelocityQ));
                        }
                        break;
                }


            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Water Track Earth Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Track Earth Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateWTEarthVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthWaterMassAvail)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_EAST_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.EarthWaterMassData.VelocityEast == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.EarthWaterMassData.VelocityEast));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_NORTH_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.EarthWaterMassData.VelocityNorth == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.EarthWaterMassData.VelocityNorth));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_VERTICAL_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.EarthWaterMassData.VelocityVertical == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.EarthWaterMassData.VelocityVertical));
                        }
                        break;
                }


            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Range Tracking SNR Update

        /// <summary>
        /// Update the plot with the latest Range Tracking SNR.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateRangeTrackingSnrPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsRangeTrackingAvail)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_0_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_0_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_0_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_1_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_1_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_1_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_2_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_2_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_2_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_3_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_3_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.SNR[DataSet.Ensemble.BEAM_3_INDEX]));
                        }
                        break;
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Range Tracking Range Update

        /// <summary>
        /// Update the plot with the latest Range Tracking Range.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateRangeTrackingRangePlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsRangeTrackingAvail)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_0_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_0_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_0_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_1_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_1_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_1_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_2_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_2_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_2_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_3_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_3_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Range[DataSet.Ensemble.BEAM_3_INDEX]));
                        }
                        break;
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region Range Tracking Pings Update

        /// <summary>
        /// Update the plot with the latest Range Tracking Pings.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateRangeTrackingPingsPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsRangeTrackingAvail)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_0_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_0_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_0_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_1_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_1_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_1_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_2_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_2_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_2_INDEX]));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_3_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_3_INDEX] == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.RangeTrackingData.Pings[DataSet.Ensemble.BEAM_3_INDEX]));
                        }
                        break;
                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region System Setup Voltage Update

        /// <summary>
        /// Update the plot with the latest System Setup Voltage.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateSystemSetupVoltagePlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsSystemSetupAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.SystemSetupData.Voltage));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region NMEA Heading Update

        /// <summary>
        /// Update the plot with the latest NMEA Heading.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateNmeaHeadingPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsNmeaAvail && ensemble.NmeaData.IsGphdtAvail())
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.NmeaData.GPHDT.Heading.DecimalDegrees));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region NMEA Speed Update

        /// <summary>
        /// Update the plot with the latest NMEA Speed.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateNmeaSpeedPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsNmeaAvail && ensemble.NmeaData.IsGpvtgAvail())
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.NmeaData.GPVTG.Speed.ToMetersPerSecond().Value));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region DVL Instrument Velocity Update

        /// <summary>
        /// Update the plot with the latest DVL Instrument Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateDvlInstrumentVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsDvlDataAvail && ensemble.DvlData.BtInstrumentIsGoodVelocity)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_X_INDEX:
                    {
                        // Check for bad velocity
                        // If we are filtering data and the data is bad, then return and do not add the point
                        if (isFilterData && ensemble.DvlData.BtXVelocity == DataSet.Ensemble.BAD_VELOCITY)
                        {
                            return;
                        }

                        Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtXVelocity));
                    }
                    break;
                    case DataSet.Ensemble.BEAM_Y_INDEX:
                    {
                        // Check for bad velocity
                        // If we are filtering data and the data is bad, then return and do not add the point
                        if (isFilterData && ensemble.DvlData.BtYVelocity == DataSet.Ensemble.BAD_VELOCITY)
                        {
                            return;
                        }

                        Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtYVelocity));
                    }
                    break;
                    case DataSet.Ensemble.BEAM_Z_INDEX:
                    {
                        // Check for bad velocity
                        // If we are filtering data and the data is bad, then return and do not add the point
                        if (isFilterData && ensemble.DvlData.BtZVelocity == DataSet.Ensemble.BAD_VELOCITY)
                        {
                            return;
                        }

                        Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtZVelocity));
                    }
                    break;
                    case DataSet.Ensemble.BEAM_Q_INDEX:
                    {
                        // Check for bad velocity
                        // If we are filtering data and the data is bad, then return and do not add the point
                        if (isFilterData && ensemble.DvlData.BtErrorVelocity == DataSet.Ensemble.BAD_VELOCITY)
                        {
                            return;
                        }

                        Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtErrorVelocity));
                    }
                    break;

                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region DVL Earth Velocity Update

        /// <summary>
        /// Update the plot with the latest DVL Earth Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateDvlEarthVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsDvlDataAvail && ensemble.DvlData.BtEarthIsGoodVelocity)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_EAST_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtEastVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtEastVelocity));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_NORTH_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtNorthVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtNorthVelocity));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_VERTICAL_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtUpwardVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtUpwardVelocity));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_Q_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtErrorVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtErrorVelocity));
                        }
                        break;

                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region DVL Ship Velocity Update

        /// <summary>
        /// Update the plot with the latest DVL Ship Velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="beam">Which beam the series will represent.</param>
        /// <param name="maxEnsembles">Max number of ensembles for the series.</param>
        /// <param name="isFilterData">Filter the data for bad data.</param>
        private void UpdateDvlShipVelocityPlot(DataSet.Ensemble ensemble, int beam, int maxEnsembles, bool isFilterData)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsDvlDataAvail && ensemble.DvlData.BtShipIsGoodVelocity)
            {
                switch (beam)
                {
                    case DataSet.Ensemble.BEAM_FORWARD_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtTransverseVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtTransverseVelocity));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_PORT_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtLongitudinalVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtLongitudinalVelocity));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_UP_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtNormalVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtNormalVelocity));
                        }
                        break;
                    case DataSet.Ensemble.BEAM_Q_INDEX:
                        {
                            // Check for bad velocity
                            // If we are filtering data and the data is bad, then return and do not add the point
                            if (isFilterData && ensemble.DvlData.BtShipErrorVelocity == DataSet.Ensemble.BAD_VELOCITY)
                            {
                                return;
                            }

                            Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.BtShipErrorVelocity));
                        }
                        break;

                }
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        #endregion

        #region DVL Heading, Pitch and Roll Update

        /// <summary>
        /// Update the plot with the latest HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateDvlHeadingPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsDvlDataAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.Heading));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateDvlPitchPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsDvlDataAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.Pitch));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
            }
        }

        /// <summary>
        /// Update the plot with the latest HPR Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of data points for the series.</param>
        private void UpdateDvlRollPlot(DataSet.Ensemble ensemble, int maxEnsembles)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsDvlDataAvail)
            {
                Points.Add(new DataPoint(ensemble.EnsembleData.EnsembleNumber, ensemble.DvlData.Roll));
            }

            // Maintain the list size
            if (Points.Count > maxEnsembles)
            {
                Points.RemoveAt(0);
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
    }
}
