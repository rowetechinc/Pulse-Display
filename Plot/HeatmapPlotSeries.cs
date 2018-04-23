using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Heatmap plot series.
    /// </summary>
    public class HeatmapPlotSeries : HeatMapSeries
    {

        #region Variable

        /// <summary>
        /// Heatmap Series data.
        /// </summary>
        public struct HeatMapSeriesData
        {
            /// <summary>
            /// Data.
            /// </summary>
            public double[] Data;

            /// <summary>
            /// Ensemble number.
            /// </summary>
            public int EnsNum;

            /// <summary>
            /// Ensemble date and time.
            /// </summary>
            public DateTime EnsDateTime;

            /// <summary>
            /// The bin for the range found in bottom track.
            /// This is used to draw the bottom track line.
            /// </summary>
            public int RangeBin;

        }

        /// <summary>
        /// List of data for the series.
        /// </summary>
        private List<HeatMapSeriesData> _dataList;

        #endregion

        #region Plot Type

        /// <summary>
        /// Plot types.
        /// </summary>
        public enum HeatmapPlotType
        {
            /// <summary>
            /// Beam velocity data.
            /// Beam 0 velocity.
            /// </summary>
            Beam_0_Vel,

            /// <summary>
            /// Beam velocity data.
            /// Beam 1 velocity.
            /// </summary>
            Beam_1_Vel,

            /// <summary>
            /// Beam velocity data.
            /// Beam 2 velocity.
            /// </summary>
            Beam_2_Vel,

            /// <summary>
            /// Beam velocity data.
            /// Beam 3 velocity.
            /// </summary>
            Beam_3_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// X velocity.
            /// </summary>
            Instr_X_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// Y velocity.
            /// </summary>
            Instr_Y_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// Z velocity.
            /// </summary>
            Instr_Z_Vel,

            /// <summary>
            /// Instrument velocity data.
            /// Error velocity.
            /// </summary>
            Instr_Error_Vel,

            /// <summary>
            /// Earth velocity data.
            /// East velocity.
            /// </summary>
            Earth_East_Vel,

            /// <summary>
            /// Earth velocity data.
            /// North velocity.
            /// </summary>
            Earth_North_Vel,

            /// <summary>
            /// Earth velocity data.
            /// Vertical velocity.
            /// </summary>
            Earth_Vertical_Vel,

            /// <summary>
            /// Earth velocity data.
            /// Error velocity.
            /// </summary>
            Earth_Error_Vel,

            /// <summary>
            /// Earth velocity data.
            /// This will display the magnitude of the velocity.
            /// </summary>
            Earth_Velocity_Magnitude,

            /// <summary>
            /// Earth velocity data.
            /// This will display the direction of the velocity.
            /// </summary>
            Earth_Velocity_Direction,

            /// <summary>
            /// Amplitude data.
            /// This will the display the average amplitude for the bin.
            /// </summary>
            Amplitude,

            /// <summary>
            /// Correlation.
            /// This will display the average correlation for the bin.
            /// </summary>
            Correlation
        }

        /// <summary>
        /// List of all the plot types.
        /// </summary>
        public static BindingList<HeatmapPlotType> PlotTypeList
        {
            get
            {
                var list = new BindingList<HeatmapPlotType>();
                list.Add(HeatmapPlotType.Earth_Velocity_Magnitude);
                list.Add(HeatmapPlotType.Earth_Velocity_Direction);
                list.Add(HeatmapPlotType.Amplitude);
                list.Add(HeatmapPlotType.Correlation);
                list.Add(HeatmapPlotType.Beam_0_Vel);
                list.Add(HeatmapPlotType.Beam_1_Vel);
                list.Add(HeatmapPlotType.Beam_2_Vel);
                list.Add(HeatmapPlotType.Beam_3_Vel);
                list.Add(HeatmapPlotType.Instr_X_Vel);
                list.Add(HeatmapPlotType.Instr_Y_Vel);
                list.Add(HeatmapPlotType.Instr_Z_Vel);
                list.Add(HeatmapPlotType.Instr_Error_Vel);
                list.Add(HeatmapPlotType.Earth_East_Vel);
                list.Add(HeatmapPlotType.Earth_North_Vel);
                list.Add(HeatmapPlotType.Earth_Vertical_Vel);
                list.Add(HeatmapPlotType.Earth_Error_Vel);

                return list;
            }

        }

        #endregion

        #region Properties

        /// <summary>
        /// The type of series.  Which data from the dataset to display.
        /// </summary>
        public HeatmapPlotType Type { get; protected set; }

        /// <summary>
        /// Minimum bin.
        /// </summary>
        public int MinBin { get; protected set; }

        /// <summary>
        /// Maximum bin.
        /// </summary>
        public int MaxBin { get; protected set; }

        /// <summary>
        /// Bin size in meters.
        /// </summary>
        public float BinSize { get; set; }

        /// <summary>
        /// Depth of the first bin.
        /// </summary>
        public float FirstBinRange { get; set; }

        /// <summary>
        /// First time in the series.
        /// </summary>
        public DateTime FirstTime { get; set; }

        /// <summary>
        /// Get the interval between each ensemble.
        /// </summary>
        public double EnsembleInterval { get; set; }

        /// <summary>
        /// Set flag to interperlate the data to blend.
        /// </summary>
        public bool Interperlate { get; set; }

        #endregion

        /// <summary>
        /// Create a time series.  This will get the series
        /// type, beam number and bin number.  This will repersent
        /// one line on the TimeSeries plot.  Set the list to null
        /// if no initial data is available.
        /// </summary>
        /// <param name="type">Type of series.</param>
        /// <param name="minBin">Minimum bin.</param>
        /// <param name="maxBin">Maximum bin.</param>
        /// <param name="isFilterData">Filter the data of all the bad values.</param>
        /// <param name="isBottomTrackLine">Flag if bottom track line should be include.</param>
        /// <param name="list">List of ensembles.  Set to NULL if no data will be given.</param>
        /// <param name="interperlate">Flag to interplate the data to blend.</param>
        public HeatmapPlotSeries(HeatmapPlotType type, int minBin, int maxBin, bool isFilterData = true, bool isBottomTrackLine = true, List<DataSet.Ensemble> list = null, bool interperlate = true)
        {
            // Initialize the values.
            Type = type;
            MinBin = minBin;
            MaxBin = maxBin;

            // Initialize ensemble options
            BinSize = float.NaN;
            FirstBinRange = float.NaN;
            FirstTime = DateTime.MinValue;
            EnsembleInterval = Double.NaN;

            //TrackerFormatString = "{1}: {2}\n{3}: {4}\n{5}: {6}\n";

            // Interpolate the data to blend it
            Interpolate = interperlate;

            // Initialize values
            ClearSeries();

            // Create a line series with the title as the type title
            //_lineSeries = new LineSeries(GetTitle(Bin, Beam));
            //this.Title = GetTitle(Bin, Beam);
            //Title = GetTitle();

            if (list != null)
            {
                foreach (var ens in list)
                {
                    // Update the line series with the list of ensembles
                    UpdateSeries(ens, list.Count, minBin, maxBin, isFilterData, isBottomTrackLine);
                }
            }
        }

        /// <summary>
        /// Update the series with the latest ensemble data.
        /// </summary>
        /// <param name="ens">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of ensembles to display.</param>
        /// <param name="minBin">Minimum bin.</param>
        /// <param name="maxBin">Maximum bin.</param>
        /// <param name="isFilterData">Filter the data for bad values.</param>
        /// <param name="isBottomTrackLine">Flag Bottom track line.</param>
        public void UpdateSeries(DataSet.Ensemble ens, int maxEnsembles, int minBin, int maxBin, bool isFilterData = true, bool isBottomTrackLine = true)
        {
            if(ens == null)
            {
                return;
            }

            MinBin = minBin;
            MaxBin = maxBin;

            // Set the bin size and depth of first bin
            SetEnsOptions(ens);

            // Update the line series
            UpdateLineSeries(ens, maxEnsembles, isFilterData, isBottomTrackLine);
        }

        /// <summary>
        /// Get the ensemble options.
        /// </summary>
        /// <param name="ens">Ensemble to get options.</param>
        private void SetEnsOptions(DataSet.Ensemble ens)
        {
            if(!ens.IsEnsembleAvail || !ens.IsAncillaryAvail)
            {
                return;
            }

            if(float.IsNaN(BinSize))
            {
                BinSize = ens.AncillaryData.BinSize;
            }

            if(float.IsNaN(FirstBinRange))
            {
                FirstBinRange = ens.AncillaryData.FirstBinRange;
            }

            // Must be before FirstTime is set
            if (Double.IsNaN(EnsembleInterval) && FirstTime != DateTime.MinValue)
            {
                var ts = ens.EnsembleData.EnsDateTime - FirstTime;
                EnsembleInterval = ts.TotalMilliseconds;
            }

            if (FirstTime == DateTime.MinValue)
            {
                FirstTime = ens.EnsembleData.EnsDateTime;
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
        /// <param name="isBottomTrackLine">Flag for bottom track line.</param>
        private void UpdateLineSeries(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine)
        {
            switch (Type)
            {
                case HeatmapPlotType.Earth_Velocity_Magnitude:                                                  // Water Profile Magnitude Velocity data
                    UpdateWPMagnitudePlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Earth_Velocity_Direction:                                                  // Water Profile Direction data
                    UpdateWPDirectionPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Amplitude:                                                                 // Water Profile Amplitude data
                    UpdateWPAmplitudePlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Correlation:                                                               // Water Profile Correlation data
                    UpdateWPCorrelationPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Beam_0_Vel:                                                                // Water Profile Beam 0 Velocity data
                    UpdateWPBeam0Plot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Beam_1_Vel:                                                                // Water Profile Beam 1 Velocity data
                    UpdateWPBeam1Plot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Beam_2_Vel:                                                                // Water Profile Beam 2 Velocity data
                    UpdateWPBeam2Plot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Beam_3_Vel:                                                                // Water Profile Beam 3 Velocity data
                    UpdateWPBeam3Plot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Earth_East_Vel:                                                            // Water Profile Earth East Velocity data
                    UpdateWPEnuEastPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Earth_North_Vel:                                                           // Water Profile Earth North Velocity data
                    UpdateWPEnuNorthPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Earth_Vertical_Vel:                                                        // Water Profile Earth Vertical Velocity data
                    UpdateWPEnuVerticalPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotType.Earth_Error_Vel:                                                           // Error Velocity
                    UpdateWPEnuErrorPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Instr_X_Vel:                                             // X Velocity
                    UpdateWPInstrXPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Y_Vel:                                             // Y Velocity
                    UpdateWPInstrYPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Z_Vel:                                             // Z Velocity
                    UpdateWPInstrZPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;
                case HeatmapPlotSeries.HeatmapPlotType.Instr_Error_Vel:                                         // Error Velocity
                    UpdateWPInstrQPlot(ensemble, maxEnsembles, isFilterData, isBottomTrackLine);
                    break;

            }
        }

        #region Update Series Type

        #region WP Magnitude Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity Magnitude Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPMagnitudePlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {
                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.EarthVelocityData.VelocityVectors.Length || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.EarthVelocityData.VelocityVectors.Length)
                {
                    maxBin = ensemble.EarthVelocityData.VelocityVectors.Length;
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data from the Earth Velocity Vector
                var data = new double[dataCount];
                for (int x = minBin; x < dataCount; x++)
                {
                    if (x < ensemble.EarthVelocityData.VelocityVectors.Length)
                    {
                        data[x] = ensemble.EarthVelocityData.VelocityVectors[x].Magnitude;
                    }
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while(_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                for(int x = 0; x < _dataList.Count; x++)
                {
                    for(int y = 0; y < _dataList[x].Data.Count(); y++)
                    {
                        Data[ensCount,y] = _dataList[x].Data[y];
                    }

                    //// If we are drawing the bottom track line
                    //// Set the new value
                    //if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    //{
                    //    Data[ensCount, ens.RangeBin] = MinValue;
                    //}

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Direction Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity Direction Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPDirectionPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {
                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.EarthVelocityData.VelocityVectors.Length || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.EarthVelocityData.VelocityVectors.Length)
                {
                    maxBin = ensemble.EarthVelocityData.VelocityVectors.Length;
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data from the Earth Velocity Vector
                var data = new double[dataCount];
                for (int x = minBin; x < dataCount; x++)
                {
                    data[x] = ensemble.EarthVelocityData.VelocityVectors[x].DirectionXNorth;
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        if (ens.Data[y] == DataSet.Ensemble.BAD_VELOCITY)
                        {
                            Data[ensCount, y] = 370;
                        }
                        else
                        {
                            Data[ensCount, y] = ens.Data[y];
                        }
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = 370;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Amplitude Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity Magnitude Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag if the bottom track line should be drawn.</param>
        private void UpdateWPAmplitudePlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsAmplitudeAvail)
            {
                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.AmplitudeData.AmplitudeData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.AmplitudeData.AmplitudeData.GetLength(0))
                {
                    maxBin = ensemble.AmplitudeData.AmplitudeData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data Amplitude data
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    int count = 0;
                    float avg = 0.0f;
                    for (int beam = 0; beam < ensemble.AmplitudeData.AmplitudeData.GetLength(1); beam++ )
                    { 
                        if(ensemble.AmplitudeData.AmplitudeData[bin, beam] != DataSet.Ensemble.BAD_VELOCITY)
                        {
                            avg += ensemble.AmplitudeData.AmplitudeData[bin, beam];
                            count++;
                        }
                    }

                    if(count > 0)
                    {
                        data[bin] = avg / count;                            // Avg Amplitude
                    }
                    else
                    {
                        data[bin] = DataSet.Ensemble.BAD_VELOCITY;          // Bad Values
                    }
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    // Go through all the ensemble data
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Correlation Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity Magnitude Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag if the bottom track line should be drawn.</param>
        private void UpdateWPCorrelationPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsCorrelationAvail)
            {
                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.CorrelationData.CorrelationData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.CorrelationData.CorrelationData.GetLength(0))
                {
                    maxBin = ensemble.CorrelationData.CorrelationData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data Correlation data
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    int count = 0;
                    float avg = 0.0f;
                    for (int beam = 0; beam < ensemble.CorrelationData.CorrelationData.GetLength(1); beam++)
                    {
                        if (ensemble.CorrelationData.CorrelationData[bin, beam] != DataSet.Ensemble.BAD_VELOCITY)
                        {
                            avg += ensemble.CorrelationData.CorrelationData[bin, beam];
                            count++;
                        }
                    }

                    if (count > 0)
                    {
                        data[bin] = (avg / count) * 100.0;                  // Avg Amplitude
                    }
                    else
                    {
                        data[bin] = -1;          // Bad Values
                    }
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    // Go through all the ensemble data
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = -1;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Beam 0 Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Beam 0 velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPBeam0Plot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBeamVelocityAvail)
            {
                // Check for the number of beams
                if(ensemble.EnsembleData.NumBeams < 1)
                {
                    return;
                }
                
                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0))
                {
                    maxBin = ensemble.BeamVelocityData.BeamVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data for Velocity
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.BeamVelocityData.BeamVelocityData[bin, DataSet.Ensemble.BEAM_0_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Beam 1 Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Beam 1 velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPBeam1Plot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBeamVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 2)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0))
                {
                    maxBin = ensemble.BeamVelocityData.BeamVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data for Velocity
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.BeamVelocityData.BeamVelocityData[bin, DataSet.Ensemble.BEAM_1_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Beam 2 Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Beam 2 velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPBeam2Plot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBeamVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 3)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0))
                {
                    maxBin = ensemble.BeamVelocityData.BeamVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data for Velocity
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.BeamVelocityData.BeamVelocityData[bin, DataSet.Ensemble.BEAM_2_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Beam 3 Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Beam 3 velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPBeam3Plot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsBeamVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 4)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.BeamVelocityData.BeamVelocityData.GetLength(0))
                {
                    maxBin = ensemble.BeamVelocityData.BeamVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data for Velocity
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.BeamVelocityData.BeamVelocityData[bin, DataSet.Ensemble.BEAM_3_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Earth East Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity East Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPEnuEastPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 1)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0))
                {
                    maxBin = ensemble.EarthVelocityData.EarthVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                var data = new double[dataCount];
                if (ensemble.EnsembleData.NumBeams == 1)
                {
                    Title = "Vertical Beam Velocity";

                    if (ensemble.IsBeamVelocityAvail)
                    {
                        // Get the data for Beam 0 Velocity
                        for (int bin = minBin; bin < dataCount; bin++)
                        {
                            data[bin] = ensemble.BeamVelocityData.BeamVelocityData[bin, DataSet.Ensemble.BEAM_0_INDEX];
                        }
                    }
                }
                else
                {
                    // Get the data for Earth East Velocity
                    for (int bin = minBin; bin < dataCount; bin++)
                    {
                        data[bin] = ensemble.EarthVelocityData.EarthVelocityData[bin, DataSet.Ensemble.BEAM_EAST_INDEX];
                    }
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                hmsd.EnsDateTime = ensemble.EnsembleData.EnsDateTime;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Earth North Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity North Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPEnuNorthPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 2)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0))
                {
                    maxBin = ensemble.EarthVelocityData.EarthVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.EarthVelocityData.EarthVelocityData[bin, DataSet.Ensemble.BEAM_NORTH_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // and it will be in the plot area
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Earth Vertical Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity Vertical Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPEnuVerticalPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 3)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0))
                {
                    maxBin = ensemble.EarthVelocityData.EarthVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data for Earth East Velocity
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.EarthVelocityData.EarthVelocityData[bin, DataSet.Ensemble.BEAM_VERTICAL_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Earth Error Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Earth velocity Error Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPEnuErrorPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsEarthVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 4)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.EarthVelocityData.EarthVelocityData.GetLength(0))
                {
                    maxBin = ensemble.EarthVelocityData.EarthVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                // Get the data for Earth East Velocity
                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.EarthVelocityData.EarthVelocityData[bin, DataSet.Ensemble.BEAM_Q_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Instrument X Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Instrument X velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPInstrXPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 1)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0))
                {
                    maxBin = ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                var data = new double[dataCount];
                if (ensemble.EnsembleData.NumBeams == 1)
                {
                    Title = "Vertical Beam Velocity";

                    if (ensemble.IsBeamVelocityAvail)
                    {
                        // Get the data for Beam 0 Velocity
                        for (int bin = minBin; bin < dataCount; bin++)
                        {
                            data[bin] = ensemble.BeamVelocityData.BeamVelocityData[bin, DataSet.Ensemble.BEAM_X_INDEX];
                        }
                    }
                }
                else
                {
                    // Get the data for Velocity
                    for (int bin = minBin; bin < dataCount; bin++)
                    {
                        data[bin] = ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, DataSet.Ensemble.BEAM_EAST_INDEX];
                    }
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                hmsd.EnsDateTime = ensemble.EnsembleData.EnsDateTime;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // Set the new value
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Instrument Y Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Instrument Y velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPInstrYPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 2)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0))
                {
                    maxBin = ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, DataSet.Ensemble.BEAM_Y_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // and it will be in the plot area
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Instrument Z Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Instrument Z velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPInstrZPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 2)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0))
                {
                    maxBin = ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, DataSet.Ensemble.BEAM_Z_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // and it will be in the plot area
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #region WP Instrument Q Velocity Update

        /// <summary>
        /// Update the plot with the latest Water Profile Instrument Q velocity Data.
        /// </summary>
        /// <param name="ensemble">Latest ensemble.</param>
        /// <param name="maxEnsembles">Maximum number of points in the series.</param>
        /// <param name="isFilterData">Filter the data for bad velocity.</param>
        /// <param name="isBottomTrackLine">Flag to Display the bottom track line.</param>
        private void UpdateWPInstrQPlot(DataSet.Ensemble ensemble, int maxEnsembles, bool isFilterData, bool isBottomTrackLine = true)
        {
            // Check if the ensemble contains data
            if (ensemble.IsEnsembleAvail && ensemble.IsInstrumentVelocityAvail)
            {
                // Check for the number of beams
                if (ensemble.EnsembleData.NumBeams < 2)
                {
                    return;
                }

                int minBin = MinBin;
                int maxBin = MaxBin;
                if (MinBin < 0 || MinBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0) || MinBin > MaxBin)
                {
                    minBin = 0;
                }
                if (MaxBin < MinBin || MaxBin > ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0))
                {
                    maxBin = ensemble.InstrumentVelocityData.InstrumentVelocityData.GetLength(0);
                }

                // Data Count
                var dataCount = maxBin - minBin;

                X0 = 0;
                X1 = _dataList.Count;
                Y0 = 0;
                Y1 = dataCount;

                var data = new double[dataCount];
                for (int bin = minBin; bin < dataCount; bin++)
                {
                    data[bin] = ensemble.InstrumentVelocityData.InstrumentVelocityData[bin, DataSet.Ensemble.BEAM_Q_INDEX];
                }

                // Add the data
                HeatMapSeriesData hmsd = new HeatMapSeriesData();
                hmsd.Data = data;
                hmsd.EnsNum = ensemble.EnsembleData.EnsembleNumber;
                if (ensemble.IsBottomTrackAvail && ensemble.IsAncillaryAvail)
                {
                    hmsd.RangeBin = ensemble.BottomTrackData.GetRangeBin(ensemble.AncillaryData.BinSize, ensemble.AncillaryData.FirstBinRange);
                }
                _dataList.Add(hmsd);

                // Ensure not exceed the max ensembles
                while (_dataList.Count > maxEnsembles)
                {
                    _dataList.RemoveAt(0);
                }

                // Combine all the data
                int ensCount = 0;
                Data = new double[_dataList.Count, dataCount];
                foreach (var ens in _dataList)
                {
                    for (int y = 0; y < ens.Data.Count(); y++)
                    {
                        Data[ensCount, y] = ens.Data[y];
                    }

                    // If we are drawing the bottom track line
                    // and it will be in the plot area
                    if (isBottomTrackLine && ens.RangeBin > 0 && ens.RangeBin < ens.Data.Count())
                    {
                        Data[ensCount, ens.RangeBin] = MaxValue;
                    }

                    ensCount++;
                }
            }
        }

        #endregion

        #endregion

        #region Clear Series

        /// <summary>
        /// Clear the series.
        /// </summary>
        public void ClearSeries()
        {
            // Initialize the values
            X0 = 0;
            X1 = 2;
            Y0 = 0;
            Y1 = 2;

            // Added 100, because amplitude max goes to 120 possibliy which is larger than bad_vel
            Data = new double[2, 2] { { DataSet.Ensemble.BAD_VELOCITY + 100, DataSet.Ensemble.BAD_VELOCITY + 100 }, { DataSet.Ensemble.BAD_VELOCITY + 100, DataSet.Ensemble.BAD_VELOCITY + 100 } }; 

            // A list of incoming data
            _dataList = new List<HeatMapSeriesData>();
        }

        #endregion

        #region Title

        /// <summary>
        /// Title for the heatmap.
        /// </summary>
        /// <param name="type">Plot type.</param>
        /// <returns>Heatmap plot title.</returns>
        public static string GetTitle(HeatmapPlotType type)
        {
            switch (type)
            {
                // For the velocity value, use the set values.
                case HeatmapPlotType.Earth_Velocity_Magnitude:
                default:
                    return "Earth Velocity Magnitude";
                case HeatmapPlotType.Earth_Velocity_Direction:
                    return "Earth Velocity Direction";
                case HeatmapPlotType.Beam_0_Vel:
                    return "Beam 0 Velocity Direction";
                case HeatmapPlotType.Beam_1_Vel:
                    return "Beam 1 Velocity Direction";
                case HeatmapPlotType.Beam_2_Vel:
                    return "Beam 2 Velocity Direction";
                case HeatmapPlotType.Beam_3_Vel:
                    return "Beam 3 Velocity Direction";
                case HeatmapPlotType.Instr_X_Vel:
                    return "Instrument X Velocity Direction";
                case HeatmapPlotType.Instr_Y_Vel:
                    return "Instrument Y Velocity Direction";
                case HeatmapPlotType.Instr_Z_Vel:
                    return "Instrument Z Velocity Direction";
                case HeatmapPlotType.Instr_Error_Vel:
                    return "Instrument Error Velocity Direction";
                case HeatmapPlotType.Earth_East_Vel:
                    return "Earth East Velocity Direction";
                case HeatmapPlotType.Earth_North_Vel:
                    return "Earth North Velocity Direction";
                case HeatmapPlotType.Earth_Vertical_Vel:
                    return "Earth Vertical Velocity Direction";
                case HeatmapPlotType.Earth_Error_Vel:
                    return "Earth Error Velocity Direction";
                case HeatmapPlotType.Correlation:
                    return "Correlation";
                case HeatmapPlotType.Amplitude:
                    return "Amplitude";
            }
        }

        #endregion


        ///// <summary>
        ///// Gets the point on the series that is nearest the specified point.
        ///// </summary>
        ///// <param name="point">The point.</param>
        ///// <param name="interpolate">Interpolate the series if this flag is set to <c>true</c>.</param>
        ///// <returns>A TrackerHitResult for the current hit.</returns>
        //public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        //{
        //    //foreach (var v in this.Values)
        //    //{
        //    //    if (double.IsNaN(v) || v < this.XAxis.ActualMinimum || v > this.XAxis.ActualMaximum)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    double x = this.XAxis.Transform(v);
        //    //    var r = new OxyRect(x - (this.symbolSize.Width / 2), this.symbolPosition - this.symbolSize.Height, this.symbolSize.Width, this.symbolSize.Height);
        //    //    if (r.Contains(point))
        //    //    {
        //    //        return new TrackerHitResult
        //    //        {
        //    //            Series = this,
        //    //            DataPoint = new DataPoint(v, double.NaN),
        //    //            Position = new ScreenPoint(x, this.symbolPosition - this.symbolSize.Height),
        //    //            Text = this.Format(this.TrackerFormatString, null, this.Title, v)
        //    //        };
        //    //    }
        //    //}

        //    if(float.IsNaN(BinSize) || float.IsNaN(FirstBinRange) || FirstTime == DateTime.MinValue || Double.IsNaN(EnsembleInterval))
        //    {
        //        return null;
        //    }

        //    double XValue = this.XAxis.Transform(point.X);
        //    double YValue = this.YAxis.Transform(point.Y);
        //    double depth = FirstBinRange + (point.Y * BinSize);

            

        //    return new TrackerHitResult
        //    {
        //        Series = this,
        //        //Position = point,
        //        Text = string.Format("X {0} Y {1} TranX {2} TranY{3}", point.X, point.Y, XValue, YValue)
        //        //Text = string.Format("Bin: {0} Ens Count: {1}\nDepth: {2}", YValue, XValue, depth) 
        //    };
        //}


        #region Override

        /// <summary>
        /// Return the description as the string for this object.
        /// </summary>
        /// <returns>Return the description as the string for this object.</returns>
        public override string ToString()
        {
            return GetTitle(Type);
        }

        #endregion
    }
}
