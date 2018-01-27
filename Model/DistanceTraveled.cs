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
 * 06/20/2012      RC          2.11       Initial coding
 * 06/22/2012      RC          2.12       Added Percent Error.
 * 06/28/2012      RC          2.12       Added Gps, Bottom Track and Water Profile Line Series.
 * 12/11/2012      RC          2.17       Added Direction Error.
 * 07/03/2013      RC          3.0.2      Fixed bug plot gps and bt distanced traveled plots.
 * 08/02/2013      RC          3.0.7      In AccumulateData(), nnsure bottom track and gps data exist to take accumulate data.
 * 04/15/2014      RC          3.2.4      Set default declination to 0.  
 * 05/01/2014      RC          3.2.4      Check for any NaN position in AccumulateGps().
 * 10/29/2015      RC          4.3.0      Fixed AccumulateWpEarth() to remove the boat speed.  Changed Direction error to an angle and not a percentage.
 * 01/14/2015      RC          4.4.2      Allow 3 beams systems when using bottom track data.
 * 06/20/2016      RC          4.4.3      Added vertical velocity to distanced traveled.
 * 04/26/2017      RC          4.4.6      Test for NaN for Latitude or Longitude in AccumulateGps().
 * 10/06/2017      RC          4.4.7      In AccumulateGps() verify a good _FirstGpsPos is set.
 * 
 */


using System;
using System.Text;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using System.Diagnostics;


namespace RTI
{

    /// <summary>
    /// Measure the distance traveled based off the GPS,
    /// the ensemble Bottom Track and ensemble Water Profile.
    /// </summary>
    public class DistanceTraveled
    {

        #region Variables

        /// <summary>
        /// Default Navigation Bin.
        /// </summary>
        public const int DEFAULT_NAV_BIN = 1;

        /// <summary>
        /// Default Declination for san diego.
        /// </summary>
        public const double DEFAULT_DECLINATION = 0;

        /// <summary>
        /// Default color for the GPS line series.
        /// </summary>
        public readonly OxyColor DEFAULT_GPS_COLOR = OxyColors.Chartreuse;

        /// <summary>
        /// Default color for the Bottom Track Earth line series.
        /// </summary>
        public readonly OxyColor DEFAULT_BT_EARTH_COLOR = OxyColors.DeepPink;

        /// <summary>
        /// Default color for the Bottom Track Instrument line series.
        /// </summary>
        public readonly OxyColor DEFAULT_BT_INSTRUMENT_COLOR = OxyColors.DarkSlateGray;

        /// <summary>
        /// Default color for the Water Profile Earth line series.
        /// </summary>
        public readonly OxyColor DEFAULT_WP_EARTH_COLOR = OxyColors.DarkTurquoise;

        /// <summary>
        /// Default color for the Water Profile Instrument line series.
        /// </summary>
        public readonly OxyColor DEFAULT_WP_INSTRUMENT_COLOR = OxyColors.DeepPink;

        #region Previous Values

        #region Previous Gps

        /// <summary>
        /// Previous GPS position.
        /// </summary>
        private DotSpatial.Positioning.Position _firstGpsPos;

        #endregion

        #region Previous Bottom Track

        /// <summary>
        /// Previous Bottom Track Earth First Ping time in seconds.
        /// </summary>
        private float _prevBtEarthTime;

        /// <summary>
        /// Previous Bottom Track Earth East velocity.
        /// </summary>
        private float _prevBtE;

        /// <summary>
        /// Previous Bottom Track Earth North Velocity.
        /// </summary>
        private float _prevBtN;

        /// <summary>
        /// Previous Bottom Track Earth Up Velocity.
        /// </summary>
        private float _prevBtU;

        /// <summary>
        /// Previous Bottom Track Instrument First Ping time in seconds.
        /// </summary>
        private float _prevBtInstrumentTime;

        /// <summary>
        /// Previous Bottom Track Instrument X velocity.
        /// </summary>
        private float _prevBtX;

        /// <summary>
        /// Previous Bottom Track Instrument Y Velocity.
        /// </summary>
        private float _prevBtY;

        /// <summary>
        /// Previous Bottom Track Instrument Z Velocity.
        /// </summary>
        private float _prevBtZ;
        

        #endregion

        #region Previous Water Profile

        /// <summary>
        /// Previous Water Profile Earth First Ping time in seconds.
        /// </summary>
        private float _prevWpEarthTime;

        /// <summary>
        /// Previous Water Profile Earth East velocity.
        /// </summary>
        private float _prevWpE;

        /// <summary>
        /// Previous Water Profile Earth North Velocity.
        /// </summary>
        private float _prevWpN;

        /// <summary>
        /// Previous Water Profile Earth Up Velocity.
        /// </summary>
        private float _prevWpU;

        /// <summary>
        /// Previous Water Profile Earth East velocity.
        /// </summary>
        private float _prevWpBtE;

        /// <summary>
        /// Previous Water Profile Earth North Velocity.
        /// </summary>
        private float _prevWpBtN;

        /// <summary>
        /// Previous Water Profile Earth Up Velocity.
        /// </summary>
        private float _prevWpBtU;

        /// <summary>
        /// Previous Water Profile Instrument First Ping time in seconds.
        /// </summary>
        private float _prevWpInstrumentTime;

        /// <summary>
        /// Previous Water Profile Instrument X velocity.
        /// </summary>
        private float _prevWpX;

        /// <summary>
        /// Previous Water Profile Instrument Y Velocity.
        /// </summary>
        private float _prevWpY;

        /// <summary>
        /// Previous Water Profile Instrument Z Velocity.
        /// </summary>
        private float _prevWpZ;

        #endregion

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Bin to use in Water Profile to calculate the
        /// distance and direction.  This bin is chosen by
        /// the user to represent best the navigation of
        /// the system.
        /// </summary>
        public int NavBin { get; set; }

        /// <summary>
        /// Offset used when creating the Lineseries for the points.  This
        /// will offset the X value.
        /// </summary>
        public double XOffset { get; set; }

        /// <summary>
        /// Offset used when creating the Lineseries for the points.  This
        /// will offset the Y value.
        /// </summary>
        public double YOffset { get; set; }

        /// <summary>
        /// Declination to add to the bottom track earth direction.
        /// Use as declination or heading offset.
        /// </summary>
        public double Declination { get; set; }

        #region GPS

        /// <summary>
        /// GPS maginitude.
        /// </summary>
        public double GpsMag { get; set; }

        /// <summary>
        /// GPS direction.
        /// </summary>
        public double GpsDir { get; set; }

        /// <summary>
        /// Gps Points.  X,Y based off initial point of 0,0.
        /// </summary>
        public LineSeries GpsPoints { get; set; }

        #endregion

        #region Bottom Track Earth

        /// <summary>
        /// Accumulated Bottom Track Earth East velocity.
        /// </summary>
        public double BtE { get; set; }

        /// <summary>
        ///  Accumulated Bottom Track Earth North velocity.
        /// </summary>
        public double BtN { get; set; }

        /// <summary>
        /// Accumulated Bottom Track Earth Up velocity.
        /// </summary>
        public double BtU { get; set; }

        /// <summary>
        /// Bottom Track Earth mangitude.
        /// </summary>
        public double BtEarthMag { get; set; }

        /// <summary>
        /// Bottom Track Earth direction.
        /// </summary>
        public double BtEarthDir { get; set; }

        /// <summary>
        /// Bottom Track Earth Points.  X,Y based off initial point of 0,0.
        /// </summary>
        public LineSeries BtEarthPoints { get; set; }

        #endregion

        #region Bottom Track Instrument

        /// <summary>
        /// Accumulated Bottom Track Instrument X velocity.
        /// </summary>
        public double BtX { get; set; }

        /// <summary>
        ///  Accumulated Bottom Track Instrument Y velocity.
        /// </summary>
        public double BtY { get; set; }

        /// <summary>
        /// Accumulated Bottom Track Instrument Z velocity.
        /// </summary>
        public double BtZ { get; set; }

        /// <summary>
        /// Bottom Track Instrument magnitude.
        /// </summary>
        public double BtInstrumentMag { get; set; }

        /// <summary>
        /// Bottom Track Instrument direction.
        /// </summary>
        public double BtInstrumentDir { get; set; }

        /// <summary>
        /// Bottom Track Instrument Points.  X,Y based off initial point of 0,0.
        /// </summary>
        public LineSeries BtInstrumentPoints { get; set; }

        #endregion

        #region Water Profile Earth

        /// <summary>
        /// Accumulated Water Profile Earth East velocity.
        /// </summary>
        public double WpE { get; set; }

        /// <summary>
        ///  Accumulated Water Profile Earth North velocity.
        /// </summary>
        public double WpN { get; set; }

        /// <summary>
        /// Accumulated Water Profile Earth Up velocity.
        /// </summary>
        public double WpU { get; set; }

        /// <summary>
        /// Water Profile Earth mangitude.
        /// </summary>
        public double WpEarthMag { get; set; }

        /// <summary>
        /// Water Profile Earth direction.
        /// </summary>
        public double WpEarthDir { get; set; }

        /// <summary>
        /// Water Profile Earth Points.  X,Y based off initial point of 0,0.
        /// </summary>
        public LineSeries WpEarthPoints { get; set; }

        #endregion

        #region Water Profile Instrument

        /// <summary>
        /// Accumulated Water Profile Instrument X velocity.
        /// </summary>
        public double WpX { get; set; }

        /// <summary>
        ///  Accumulated Water Profile Instrument Y velocity.
        /// </summary>
        public double WpY { get; set; }

        /// <summary>
        /// Accumulated Water Profile Instrument Z velocity.
        /// </summary>
        public double WpZ { get; set; }

        /// <summary>
        /// Water Profile Instrument magnitude.
        /// </summary>
        public double WpInstrumentMag { get; set; }

        /// <summary>
        /// Water Profile Instrument direction.
        /// </summary>
        public double WpInstrumentDir { get; set; }

        /// <summary>
        /// Water Profile Instrument Points.  X,Y based off initial point of 0,0.
        /// </summary>
        public LineSeries WpInstrumentPoints { get; set; }

        #endregion

        #region Percent Error

        /// <summary>
        /// Percent Error for Bottom Track Earth
        /// compared against GPS.
        /// </summary>
        public double BtEarthPercentError { get; set; }

        /// <summary>
        /// Percent Error for Bottom Track Instrument
        /// compared against GPS.
        /// </summary>
        public double BtInstrumentPercentError { get; set; }

        /// <summary>
        /// Percent Error for Water Profile Earth
        /// compared against GPS.
        /// </summary>
        public double WpEarthPercentError { get; set; }

        /// <summary>
        /// Percent Error for Water Profile Instrument
        /// compared against GPS.
        /// </summary>
        public double WpInstrumentPercentError { get; set; }

        #endregion

        #region Dir Error

        /// <summary>
        /// Direction Error for Bottom Track Earth
        /// compared against GPS.
        /// </summary>
        public double BtEarthDirError { get; set; }

        /// <summary>
        /// Direction Error for Bottom Track Instrument
        /// compared against GPS.
        /// </summary>
        public double BtInstrumentDirError { get; set; }

        /// <summary>
        /// Direction Error for Water Profile Earth
        /// compared against GPS.
        /// </summary>
        public double WpEarthDirError { get; set; }

        /// <summary>
        /// Direction Error for Water Profile Instrument
        /// compared against GPS.
        /// </summary>
        public double WpInstrumentDirError { get; set; }

        #endregion

        #region Colors

        /// <summary>
        /// Color for the GPS line series.
        /// </summary>
        public OxyColor GpsColor { get; set; }

        /// <summary>
        /// Color for the Bottom Track Earth line series.
        /// </summary>
        public OxyColor BtEarthColor { get; set; }

        /// <summary>
        /// Color for the Bottom Track Instrument line series.
        /// </summary>
        public OxyColor BtInstrumentColor { get; set; }

        /// <summary>
        /// Color for the Water Profile Earth line series.
        /// </summary>
        public OxyColor WpEarthColor { get; set; }

        /// <summary>
        /// Color for the Water Profile Instrument line series.
        /// </summary>
        public OxyColor WpInstrumentColor { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        /// <param name="navBin">Bin to calculate the Water Profile data.</param>
        /// <param name="xOffset">Start location for the line series on the X axis. Default = 0.</param>
        /// <param name="yOffset">Start location for the line series on the Y axis. Default = 0.</param>
        public DistanceTraveled(int navBin = DEFAULT_NAV_BIN, double xOffset = 0.0, double yOffset = 0.0)
        {
            // Initialize values
            NavBin = navBin;
            XOffset = xOffset;
            YOffset = yOffset;
            Declination = DEFAULT_DECLINATION;

            GpsColor = DEFAULT_GPS_COLOR;
            BtEarthColor = DEFAULT_BT_EARTH_COLOR;
            BtInstrumentColor = DEFAULT_BT_INSTRUMENT_COLOR;
            WpEarthColor = DEFAULT_WP_EARTH_COLOR;
            WpInstrumentColor = DEFAULT_WP_INSTRUMENT_COLOR;

            InitValues();
        }

        /// <summary>
        /// Load the latest ensemble.  This will recalculate all the
        /// maginitude and directions.  
        /// 
        /// This assumes that we are always moving
        /// forward through the data.  If you move back in the data, it will be
        /// added to the distance and direction and mess up the calculation.
        /// </summary>
        /// <param name="ensemble">Ensemble to calculate.</param>
        public void AddIncomingData(DataSet.Ensemble ensemble)
        {
            // Accumulate the values
            AccumulateData(ensemble);
        }

        /// <summary>
        /// Calculate all the data at once.
        /// </summary>
        /// <param name="cache"></param>
        public void Calculate(Cache<long, DataSet.Ensemble> cache, Subsystem subsystem, SubsystemDataConfig ssConfig)
        {
            for(int x = 0; x < cache.Count(); x++)
            {
                DataSet.Ensemble ensemble = cache.Get(x);
                if (ensemble != null)
                {
                    // Verify the subsystem matches this viewmodel's subystem.
                    if ((subsystem == ensemble.EnsembleData.GetSubSystem())                 // Check if Subsystem matches 
                            && (ssConfig == ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
                    {
                        AccumulateData(ensemble);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all the data.
        /// </summary>
        public void Clear()
        {
            InitValues();
        }

        #region Initialize

        /// <summary>
        /// Initialize all the values.
        /// </summary>
        private void InitValues()
        {
            _firstGpsPos = DotSpatial.Positioning.Position.Empty;
            GpsDir = 0.0;
            GpsMag = 0.0;
            GpsPoints = new LineSeries() { Color = GpsColor, StrokeThickness = 1, Title = "GPS" };

            BtEarthDir = 0.0;
            BtEarthMag = 0.0;
            BtEarthPoints = new LineSeries() { Color = BtEarthColor, StrokeThickness = 1, Title = "BT ENU" };
            BtE = 0.0;
            BtN = 0.0;
            BtU = 0.0;
            _prevBtEarthTime = -1.0f;
            _prevBtE = 0.0f;
            _prevBtN = 0.0f;
            _prevBtU = 0.0f;

            BtInstrumentDir = 0.0;
            BtInstrumentMag = 0.0;
            BtInstrumentPoints = new LineSeries() { Color = BtInstrumentColor, StrokeThickness = 1, Title = "BT XYZ" };
            BtX = 0.0;
            BtY = 0.0;
            BtZ = 0.0;
            _prevBtInstrumentTime = -1.0f;
            _prevBtX = 0.0f;
            _prevBtY = 0.0f;
            _prevBtZ = 0.0f;

            WpEarthDir = 0.0;
            WpEarthMag = 0.0;
            WpEarthPoints = new LineSeries() { Color = WpEarthColor, StrokeThickness = 1, Title = "WP ENU" };
            WpE = 0.0;
            WpN = 0.0;
            WpU = 0.0;
            _prevWpEarthTime = -1.0f;
            _prevWpE = 0.0f;
            _prevWpN = 0.0f;
            _prevWpU = 0.0f;

            WpInstrumentDir = 0.0;
            WpInstrumentMag = 0.0;
            WpInstrumentPoints = new LineSeries() { Color = WpInstrumentColor, StrokeThickness = 1, Title = "WP XYZ" };
            WpX = 0.0;
            WpY = 0.0;
            WpZ = 0.0;
            _prevWpInstrumentTime = -1.0f;
            _prevWpX = 0.0f;
            _prevWpY = 0.0f;
            _prevWpZ = 0.0f;

            BtEarthPercentError = 0;
            BtInstrumentPercentError = 0;
            WpEarthPercentError = 0;
            WpInstrumentPercentError = 0;

            BtEarthDirError = 0;
            BtInstrumentDirError = 0;
            WpEarthDirError = 0;
            WpInstrumentDirError = 0;
        }

        #endregion

        #region Accumulate

        /// <summary>
        /// Accumulate all the data based off the new ensemble added.
        /// </summary>
        /// <param name="ensemble">New ensemble.</param>
        private void AccumulateData(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                // Ensure bottom track and gps data exist to take a sample
                //if (ensemble.IsNmeaAvail && ensemble.NmeaData.IsGpggaAvail() && ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.IsEarthVelocityGood())
                //{
                    AccumulateGps(ensemble);                    // GPS
                    AccumulateBtEarth(ensemble);                // Bottom Track Earth Velocity
                    AccumulateBtInstrument(ensemble);           // Bottom Track Instrument Velocity
                    AccumulateWpEarth(ensemble, NavBin);        // Water Profile Earth Velocity
                    AccumulateWpInstrument(ensemble, NavBin);   // Water Profile Instrument Velocity

                    CalculatePercentError();                    // Calculate the percent error

                    //Debug.WriteLine(this.ToString());
                    //Debug.WriteLine(string.Format("Ens: {0}, {1}", ensemble.EnsembleData.EnsembleNumber, ensemble.EnsembleData.EnsDateTime.ToString()));
                    //Debug.WriteLine(string.Format("GPS           {0}, {1}", GpsMag, GpsDir));
                    //Debug.WriteLine(string.Format("BT Earth      {0}, {1} - {2}, {3}, {4}", BtEarthMag, BtEarthDir, BtE, BtN, BtU));
                    //Debug.WriteLine(string.Format("BT Instrument {0}, {1} - {2}, {3}, {4}", BtInstrumentMag, BtInstrumentDir, BtX, BtY, BtZ));
                    //Debug.WriteLine(string.Format("WP Earth      {0}, {1} - {2}, {3}, {4}", WpEarthMag, WpEarthDir, WpE, WpN, WpU));
                    //Debug.WriteLine(string.Format("WP Instrument {0}, {1} - {2}, {3}, {4}", WpInstrumentMag, WpInstrumentDir, WpX, WpY, WpZ));
                    //Debug.WriteLine(string.Format("--------------------------------------------------------"));
                //}
            }
        }

        #region GPS

        /// <summary>
        /// If GGA NMEA data is available, calculate the magnitude and direction
        /// of the current position to the start.
        /// </summary>
        /// <param name="ensemble"></param>
        private void AccumulateGps(DataSet.Ensemble ensemble)
        {
            // If the previous postion has not been set, we cannot calculate yet
            if (_firstGpsPos == DotSpatial.Positioning.Position.Empty)
            {
                if (ensemble.IsNmeaAvail)
                {
                    if (ensemble.NmeaData.IsGpggaAvail() && !ensemble.NmeaData.GPGGA.Position.IsInvalid && ensemble.NmeaData.GPGGA.Position.Longitude.DecimalDegrees != Double.NaN)
                    {
                        _firstGpsPos = ensemble.NmeaData.GPGGA.Position;

                        GpsPoints.Points.Add(new DataPoint(0 + XOffset, 0 + YOffset));
                    }
                }

                // Nothing to calculate yet
                return;
            }

            // Calculate the magnitude and direction
            // Use the first position and the current position
            // If the previous postion has not been set, we cannot calculate yet
            if (_firstGpsPos != DotSpatial.Positioning.Position.Empty && !_firstGpsPos.IsInvalid)
            {
                if (ensemble.IsNmeaAvail)
                {
                    if (ensemble.NmeaData.IsGpggaAvail() && !ensemble.NmeaData.GPGGA.Position.IsInvalid && ensemble.NmeaData.GPGGA.Position.Longitude.DecimalDegrees != Double.NaN )
                    {
                        GpsMag = _firstGpsPos.DistanceTo(ensemble.NmeaData.GPGGA.Position).ToMeters().Value;     // Meters           Distance Made Good
                        GpsDir = _firstGpsPos.BearingTo(ensemble.NmeaData.GPGGA.Position).DecimalDegrees;        // Decimal Degrees  Course Made Good
                        //GpsDir = ensemble.NmeaData.GPGGA.Position.BearingTo(_firstGpsPos).DecimalDegrees;       // Decimal Degrees  Course Over Ground

                        // Generate X,Y point
                        double x = YOffset + (GpsMag * Math.Sin(MathHelper.DegreeToRadian(GpsDir)));
                        double y = XOffset + (GpsMag * Math.Cos(MathHelper.DegreeToRadian(GpsDir)));

                        // Add the point to the line series
                        GpsPoints.Points.Add(new DataPoint(x, y));
                    }
                }
            }
        }

        #endregion

        #region Bottom Track

        /// <summary>
        /// Accumulate the Bottom Track Earth velocity to get the distance made good and course made good.
        /// This will calculate the time from the previous and current ensemble.  It will then calculate based
        /// off time and speed the distance and direction traveled.  It will then store the previous values for
        /// the calculation next time.
        /// </summary>
        /// <param name="ensemble">Current ensemble.</param>
        private void AccumulateBtEarth(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsBottomTrackAvail)
            {
                // Ensure at least a 3 beam system
                // Cannot calculate ENU with a vertical beam
                if (ensemble.BottomTrackData.NumBeams >= 3)
                {
                    // Is Earth data is good, then calculate
                    if (ensemble.BottomTrackData.IsEarthVelocityGood())
                    {
                        // If the previous values have not been set, they must be set first
                        if (_prevBtEarthTime < 0)
                        {
                            // Set the previous values
                            _prevBtEarthTime = ensemble.BottomTrackData.FirstPingTime;
                            _prevBtE = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                            _prevBtN = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                            _prevBtU = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
                            BtEarthPoints.Points.Add(new DataPoint(0 + XOffset, 0 + YOffset));
                            return;
                        }

                        double dT = ensemble.BottomTrackData.FirstPingTime - _prevBtEarthTime;

                        BtE += 0.5 * dT * (ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX] + _prevBtE);
                        BtN += 0.5 * dT * (ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX] + _prevBtN);
                        BtU += 0.5 * dT * (ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX] + _prevBtU);

                        BtEarthMag = Math.Sqrt((BtE * BtE) + (BtN * BtN) + (BtU * BtU));
                        BtEarthDir = Math.Atan2(BtE, BtN) * (180.0 / Math.PI) + Declination;
                        if (BtEarthDir < 0.0)
                        {
                            BtEarthDir = 360.0 + BtEarthDir;
                        }

                        // Generate X,Y point
                        double x = XOffset + (BtEarthMag * Math.Sin(MathHelper.DegreeToRadian(BtEarthDir)));
                        double y = YOffset + (BtEarthMag * Math.Cos(MathHelper.DegreeToRadian(BtEarthDir)));

                        // Add the point to the line series
                        BtEarthPoints.Points.Add(new DataPoint(x, y));

                        // Set the previous values
                        _prevBtEarthTime = ensemble.BottomTrackData.FirstPingTime;
                        _prevBtE = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                        _prevBtN = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                        _prevBtU = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
                    }
                }
            }
        }


        /// <summary>
        /// Accumulate the Bottom Track Instrument velocity to get the distance made good and course made good.
        /// This will calculate the time from the previous and current ensemble.  It will then calculate based
        /// off time and speed the distance and direction traveled.  It will then store the previous values for
        /// the calculation next time.
        /// </summary>
        /// <param name="ensemble">Current ensemble.</param>
        private void AccumulateBtInstrument(DataSet.Ensemble ensemble)
        {
            if (ensemble.IsBottomTrackAvail)
            {
                // Ensure at least a 4 beam system
                if (ensemble.BottomTrackData.NumBeams >= 3)
                {
                    // Is Earth data is good, then calculate
                    if (ensemble.BottomTrackData.IsInstrumentVelocityGood())
                    {
                        // If the previous values have not been set, they must be set first
                        if (_prevBtInstrumentTime < 0)
                        {
                            // Set the previous values
                            _prevBtInstrumentTime = ensemble.BottomTrackData.FirstPingTime;
                            _prevBtX = ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_X_INDEX];
                            _prevBtY = ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_Y_INDEX];
                            _prevBtZ = ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_Z_INDEX];
                            BtInstrumentPoints.Points.Add(new DataPoint(0 + XOffset, 0 + YOffset));
                            return;
                        }

                        double dT = ensemble.BottomTrackData.FirstPingTime - _prevBtInstrumentTime;

                        BtX += 0.5 * dT * (ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_X_INDEX] + _prevBtX);
                        BtY += 0.5 * dT * (ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_Y_INDEX] + _prevBtY);
                        BtZ += 0.5 * dT * (ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_Z_INDEX] + _prevBtZ);

                        BtInstrumentMag = Math.Sqrt((BtX * BtX) + (BtY * BtY) + (BtZ * BtZ));
                        BtInstrumentDir = Math.Atan2(BtX, BtY) * (180 / Math.PI) + Declination;
                        if (BtInstrumentDir < 0.0)
                        {
                            BtInstrumentDir = 360.0 + BtInstrumentDir;
                        }

                        // Generate X,Y point
                        double x = XOffset + (BtInstrumentMag * Math.Cos(MathHelper.DegreeToRadian(BtInstrumentDir)));
                        double y = YOffset + (BtInstrumentMag * Math.Sin(MathHelper.DegreeToRadian(BtInstrumentDir)));

                        // Inverse the signs so it matches the Water Profile Instrument
                        x *= -1;
                        y *= -1;

                        // Add the point to the line series
                        BtInstrumentPoints.Points.Add(new DataPoint(x, y));

                        // Set the previous values
                        _prevBtInstrumentTime = ensemble.BottomTrackData.FirstPingTime;
                        _prevBtX = ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_X_INDEX];
                        _prevBtY = ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_Y_INDEX];
                        _prevBtZ = ensemble.BottomTrackData.InstrumentVelocity[DataSet.Ensemble.BEAM_Z_INDEX];
                    }
                }
            }
        }

        #endregion

        #region Water Profile

        /// <summary>
        /// Accumulate the Water Profile Earth velocity to get the distance made good and course made good.
        /// This will calculate the time from the previous and current ensemble.  It will then calculate based
        /// off time and speed the distance and direction traveled.  It will then store the previous values for
        /// the calculation next time.
        /// 
        /// The user will give a bin to used to calculate the distance and direction.  This bin is selected based
        /// off the best bin that represents coherent data.  Data that is not distored due to wind or other factors.
        /// </summary>
        /// <param name="ensemble">Current ensemble.</param>
        /// <param name="navBin">Bin selected for the velocity to use.</param>
        private void AccumulateWpEarth(DataSet.Ensemble ensemble, int navBin)
        {
            if (ensemble.IsEarthVelocityAvail && ensemble.IsAncillaryAvail)
            {
                // Verify the navBin
                navBin = VerifyNavBin(navBin, ensemble);

                // Is Earth data is good, then calculate
                if (ensemble.EarthVelocityData.IsBinGood(navBin))
                {
                    // If the previous values have not been set, they must be set first
                    if (_prevWpEarthTime < 0)
                    {
                        _prevWpEarthTime = ensemble.AncillaryData.FirstPingTime;
                        _prevWpE = ensemble.EarthVelocityData.EarthVelocityData[navBin, DataSet.Ensemble.BEAM_EAST_INDEX];
                        _prevWpN = ensemble.EarthVelocityData.EarthVelocityData[navBin, DataSet.Ensemble.BEAM_NORTH_INDEX];
                        _prevWpU = ensemble.EarthVelocityData.EarthVelocityData[navBin, DataSet.Ensemble.BEAM_VERTICAL_INDEX];

                        if (ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.IsEarthVelocityGood())
                        {
                            _prevWpBtE = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                            _prevWpBtN = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                            _prevWpBtU = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
                        }
                        WpEarthPoints.Points.Add(new DataPoint(0 + XOffset, 0 + YOffset));
                        return;
                    }

                    double dT = ensemble.AncillaryData.FirstPingTime - _prevWpEarthTime;

                    // Put the ship speed back into the data
                    float btEast = 0;
                    float btNorth = 0;
                    float btVertical = 0;
                    if (ensemble.IsBottomTrackAvail)
                    {
                        // Is Earth data is good, then calculate
                        if (ensemble.BottomTrackData.IsEarthVelocityGood())
                        {
                            btEast = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                            btNorth = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                            btVertical = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];

                        }
                        else if (_prevWpBtE != DataSet.Ensemble.BAD_VELOCITY && _prevWpBtN != DataSet.Ensemble.BAD_VELOCITY && _prevWpBtU != DataSet.Ensemble.BAD_VELOCITY)
                        {
                            btEast = _prevWpBtE;
                            btNorth = _prevWpBtN;
                            btVertical = _prevWpBtU;
                        }
                    }

                    // Put the ship speed back in
                    // Refer to the class RemoveShipSpeed.cs to see how it was removed previously
                    float east = btEast - ensemble.EarthVelocityData.EarthVelocityData[navBin, DataSet.Ensemble.BEAM_EAST_INDEX];
                    float north = btNorth - ensemble.EarthVelocityData.EarthVelocityData[navBin, DataSet.Ensemble.BEAM_NORTH_INDEX];
                    float up = btVertical - ensemble.EarthVelocityData.EarthVelocityData[navBin, DataSet.Ensemble.BEAM_VERTICAL_INDEX];

                    WpE += 0.5 * dT * (east + _prevWpE);
                    WpN += 0.5 * dT * (north + _prevWpN);
                    WpU += 0.5 * dT * (up + _prevWpU);

                    WpEarthMag = Math.Sqrt((WpE * WpE) + (WpN * WpN) + (WpU * WpU));
                    WpEarthDir = Math.Atan2(WpE, WpN) * (180 / Math.PI) + Declination;
                    if (WpEarthDir < 0.0)
                    {
                        WpEarthDir = 360.0 + WpEarthDir;
                    }

                    // Generate X,Y point
                    double x = XOffset + (WpEarthMag * Math.Cos(MathHelper.DegreeToRadian(WpEarthDir)));
                    double y = YOffset + (WpEarthMag * Math.Sin(MathHelper.DegreeToRadian(WpEarthDir)));

                    // Add the point to the line series
                    WpEarthPoints.Points.Add(new DataPoint(x, y));

                    // Set the previous values
                    _prevWpEarthTime = ensemble.AncillaryData.FirstPingTime;
                    _prevWpE = east;
                    _prevWpN = north;
                    _prevWpU = up;

                    if (ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.IsEarthVelocityGood())
                    {
                        _prevWpBtE = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                        _prevWpBtN = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                        _prevWpBtU = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
                    }
                }
            }
        }

        /// <summary>
        /// Accumulate the Water Profile Instrument velocity to get the distance made good and course made good.
        /// This will calculate the time from the previous and current ensemble.  It will then calculate based
        /// off time and speed the distance and direction traveled.  It will then store the previous values for
        /// the calculation next time.
        /// 
        /// The user will give a bin to used to calculate the distance and direction.  This bin is selected based
        /// off the best bin that represents coherent data.  Data that is not distored due to wind or other factors.
        /// </summary>
        /// <param name="ensemble">Current ensemble.</param>
        /// <param name="navBin">Bin selected for the velocity to use.</param>
        private void AccumulateWpInstrument(DataSet.Ensemble ensemble, int navBin)
        {
            if (ensemble.IsInstrumentVelocityAvail && ensemble.IsAncillaryAvail)
            {
                // Verify the navBin
                navBin = VerifyNavBin(navBin, ensemble);

                // Is Earth data is good, then calculate
                if (ensemble.InstrumentVelocityData.IsBinGood(navBin))
                {
                    // If the previous values have not been set, they must be set first
                    if (_prevWpInstrumentTime < 0)
                    {
                        _prevWpInstrumentTime = ensemble.AncillaryData.FirstPingTime;
                        _prevWpX = ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_X_INDEX];
                        _prevWpY = ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_Y_INDEX];
                        _prevWpZ = ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_Z_INDEX];
                        WpInstrumentPoints.Points.Add(new DataPoint(0 + XOffset, 0 + YOffset));
                        return;
                    }

                    double dT = ensemble.AncillaryData.FirstPingTime - _prevWpInstrumentTime;

                    WpX += 0.5 * dT * (ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_X_INDEX] + _prevWpX);
                    WpY += 0.5 * dT * (ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_Y_INDEX] + _prevWpY);
                    WpZ += 0.5 * dT * (ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_Z_INDEX] + _prevWpZ);

                    WpInstrumentMag = Math.Sqrt((WpX * WpX) + (WpY * WpY) + (WpZ * WpZ));
                    WpInstrumentDir = Math.Atan2(WpX, WpY) * (180 / Math.PI) + Declination;
                    if (WpInstrumentDir < 0.0)
                    {
                        WpInstrumentDir = 360.0 + WpInstrumentDir;
                    }

                    // Generate X,Y point
                    double x = XOffset + (WpInstrumentMag * Math.Cos(MathHelper.DegreeToRadian(WpInstrumentDir)));
                    double y = YOffset + (WpInstrumentMag * Math.Sin(MathHelper.DegreeToRadian(WpInstrumentDir)));

                    // Add the point to the line series
                    WpInstrumentPoints.Points.Add(new DataPoint(x, y));

                    // Set the previous values
                    _prevWpInstrumentTime = ensemble.AncillaryData.FirstPingTime;
                    _prevWpX = ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_X_INDEX];
                    _prevWpY = ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_Y_INDEX];
                    _prevWpZ = ensemble.InstrumentVelocityData.InstrumentVelocityData[navBin, DataSet.Ensemble.BEAM_Z_INDEX];
                }
            }
        }

        #endregion

        #region Percent Error

        /// <summary>
        /// Calculate the percent error for the maginitude against
        /// the GPS value.
        /// </summary>
        private void CalculatePercentError()
        {
            if (GpsMag != 0)
            {
                // Mag Error
                BtEarthPercentError = MathHelper.PercentError(GpsMag, BtEarthMag);
                BtInstrumentPercentError = MathHelper.PercentError(GpsMag, BtInstrumentMag);
                WpEarthPercentError = MathHelper.PercentError(GpsMag, WpEarthMag);
                WpInstrumentPercentError = MathHelper.PercentError(GpsMag, WpInstrumentMag);

                // Dir Error
                //BtEarthDirError = MathHelper.PercentError(GpsDir, BtEarthDir);
                //BtInstrumentDirError = MathHelper.PercentError(GpsDir, BtInstrumentDir);
                //WpEarthDirError = MathHelper.PercentError(GpsDir, WpEarthDir);
                //WpInstrumentDirError = MathHelper.PercentError(GpsDir, WpInstrumentDir);

                // Dir Error
                BtEarthDirError = MathHelper.AngleDiff(GpsDir,BtEarthDir);
                BtInstrumentDirError = MathHelper.AngleDiff(GpsDir, BtInstrumentDir);
                WpEarthDirError = MathHelper.AngleDiff(GpsDir, WpEarthDir);
                WpInstrumentDirError = MathHelper.AngleDiff(GpsDir, WpInstrumentDir);
                
            }
        }

        #endregion

        /// <summary>
        /// Verify the Navigation bin is good.  This will check if the value
        /// is greater then the number of bins.  It will check if the value is
        /// less then 0.  If it is neither of these, then it will return the original
        /// value given.  If it is greater, it will return the max number of bins in
        /// the ensemble.  If it is less then 0, it will return 0.
        /// </summary>
        /// <param name="navBin">Navigation bin it is trying to use.</param>
        /// <param name="ensemble">Ensemble to use the bin.</param>
        /// <returns>A good Navigation bin.</returns>
        private int VerifyNavBin(int navBin, DataSet.Ensemble ensemble)
        {
            if (ensemble.IsEnsembleAvail)
            {
                // Verify a good navBin
                if (navBin >= ensemble.EnsembleData.NumBins)
                {
                    return ensemble.EnsembleData.NumBins - 1;
                }
                if (navBin < 0)
                {
                    return 0;
                }
            }

            return navBin;
        }

        #endregion

        /// <summary>
        /// Output the object values as a string.
        /// </summary>
        /// <returns>String of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("GpsMag={0} GpsDir={1} \n", GpsMag.ToString("0.000"), GpsDir.ToString("0.000")));
            sb.Append(string.Format("BT E={0} N={1} U={2} Mag={3} Dir={4} PE={5} \n", BtE.ToString("0.000"), BtN.ToString("0.000"), BtU.ToString("0.000"), BtEarthMag.ToString("0.000"), BtEarthDir.ToString("0.000"), BtEarthPercentError.ToString("0.000")));
            sb.Append(string.Format("BT X={0} Y={1} Z={2} Mag={3} Dir={4} PE={5} \n", BtX.ToString("0.000"), BtY.ToString("0.000"), BtZ.ToString("0.000"), BtInstrumentMag.ToString("0.000"), BtInstrumentDir.ToString("0.000"), BtInstrumentPercentError.ToString("0.000")));
            sb.Append(string.Format("WP E={0} N={1} U={2} Mag={3} Dir={4} PE={5} \n", WpE.ToString("0.000"), WpN.ToString("0.000"), WpU.ToString("0.000"), WpEarthMag.ToString("0.000"), WpEarthDir.ToString("0.000"), WpEarthPercentError.ToString("0.000")));
            sb.Append(string.Format("WP X={0} Y={1} Z={2} Mag={3} Dir={4} PE={5} \n", WpX.ToString("0.000"), WpY.ToString("0.000"), WpZ.ToString("0.000"), WpInstrumentMag.ToString("0.000"), WpInstrumentDir.ToString("0.000"), WpInstrumentPercentError.ToString("0.000")));

            return sb.ToString();
        }

    }
}
