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
 * 06/20/2012      RC          2.12       Initial coding
 * 06/21/2012      RC          2.12       Initialize and clear data properly.
 * 06/22/2012      RC          2.12       Added Percent Error.
 *                                         Output the scale with the value.
 * 06/28/2012      RC          2.12       Added Gps, Bottom Track and Water Profile Line Series.
 * 08/20/2012      RC          2.13       Added Average amplitude calculations.
 * 08/21/2012      RC          2.13       Maded _distanceTraveled public so i can get the raw values and not the strings.
 * 12/11/2012      RC          2.17       Added Direction Error.
 * 01/22/2013      RC          2.17       Fixed Date/Time not being updated in SetReportInfo().
 * 06/12/2014      RC          3.3.1      Added Profile Range and Signal to Noise results.
 * 08/18/2014      RC          4.0.0      Removed setting NavBin in the constructor so it will use the proper default value.
 * 02/24/2015      RC          4.0.3      Added GpsDistance and BtEarthDistance.
 * 04/09/2015      RC          4.1.2      Added GlitchCheck().
 * 04/16/2015      RC          4.1.2      Check for the number of beams in ProfileRange().
 * 05/20/2015      RC          4.1.3      Clear all the values.
 * 06/23/2015      RC          4.1.3      Added Tank Testing.
 * 04/07/2016      RC          4.4.3      Added support for SeaSeven in ProjectReportText.
 * 03/15/2016      RC          4.4.3      Allow DMG to be calculated with no Water Profile data.
 * 02/13/2017      RC          4.4.5      Removed the await in LoadData().
 * 08/30/2018      RC          4.11.0     Added Ensemble SyncRoot in AddIncomingData().
 * 
 */

using System.ComponentModel;
using System;
using OxyPlot;
using System.Diagnostics;
using OxyPlot.Series;
using Caliburn.Micro;
using System.Threading.Tasks;
using System.Text;

namespace RTI
{

    /// <summary>
    /// Used to display a text output of a report of the
    /// project.  
    /// </summary>
    public class ProjectReportText : PropertyChangedBase 
    {
        #region Variables

        #region Defaults

        /// <summary>
        /// Default Ampltide depth of 1 meter.
        /// </summary>
        public const double DEFAULT_AMP_DEPTH_1M_TANK = 1.0;

        /// <summary>
        /// Default SNR Depth for 300 kHz Tank.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_300_TANK = 0.6;

        /// <summary>
        /// Default SNR Depth for 300 kHz tank.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_600_TANK = 0.4;

        /// <summary>
        /// Default SNR Depth for 300 kHz tank.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_1200_TANK = 0.2;

        /// <summary>
        /// Default SNR Depth for 300 kHz lake.
        /// In meters.
        /// 4m bin size.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_300_LAKE = 10.05;

        /// <summary>
        /// Default SNR Depth for 300 kHz lake.
        /// In meters.
        /// 2m bin size.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_600_LAKE = 10.8;

        /// <summary>
        /// Default SNR Depth for 300 kHz lake.
        /// In meters.
        /// 1m bin size.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_1200_LAKE = 5.5;

        /// <summary>
        /// Default SNR Depth for 300 kHz ocean.
        /// In meters.
        /// 8m bin size.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_300_OCEAN = 40.0;

        /// <summary>
        /// Default SNR Depth for 300 kHz ocean.
        /// In meters.
        /// 4m bin size.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_600_OCEAN = 20.0;

        /// <summary>
        /// Default SNR Depth for 300 kHz ocean.
        /// In meters.
        /// 2m bin size.
        /// </summary>
        public const double DEFAULT_SNR_DEPTH_1200_OCEAN = 10.0;

        #endregion

        /// <summary>
        /// Calculate the distance traveled in GPS, Bottom Track
        /// and Water Profile data.
        /// </summary>
        public DistanceTraveled _distanceTraveled;

        #region Average Amplitude

        /// <summary>
        ///  Number of beams to accumulate.
        /// </summary>
        private const int DEFAULT_NUM_BEAMS = 4;

        // Number of beams
        private int _numBeams;

        #region Tank

        /// <summary>
        /// Accumulation of amplitude values at Tank 1 meter.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpTank1mAccum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Tank 1 meter.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpTank1mCount;

        /// <summary>
        /// Accumulation of amplitude values at Tank 300 kHz.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpTank300Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Tank 300 kHz.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpTank300Count;

        /// <summary>
        /// Accumulation of amplitude values at Tank 600 kHz.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpTank600Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Tank 600 kHz.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpTank600Count;

        /// <summary>
        /// Accumulation of amplitude values at Tank 1200 kHz.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpTank1200Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Tank 1200 kHz.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpTank1200Count;

        #endregion

        #region Lake

        /// <summary>
        /// Accumulation of  amplitude values at Lake 300 kHz.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpLake300Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Lake 300 kHz.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpLake300Count;

        /// <summary>
        /// Accumulation of  amplitude values at Lake 600 kHz.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpLake600Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Lake 600 kHz.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpLake600Count;

        /// <summary>
        /// Accumulation of  amplitude values at Lake 1200 kHz.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpLake1200Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Lake 1200 kHz.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpLake1200Count;

        /// <summary>
        /// Accumulation of amplitude values at Noise.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpNoiseAccum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average at Noise.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpNoiseCount;

        #endregion

        #region Ocean

        /// <summary>
        /// Accumulation of  amplitude values for Ocean 300.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpOcean300Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average for Ocean 300.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpOcean300Count;

        /// <summary>
        /// Accumulation of  amplitude values for Ocean 600.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpOcean600Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the average for Ocean 600.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpOcean600Count;

        /// <summary>
        /// Accumulation of  amplitude values for Ocean 1200.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgAmpOcean1200Accum;

        /// <summary>
        /// Number of amplitude values that have been accumulated for the averagefor Ocean 1200.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgAmpOcean1200Count;

        #endregion

        #region Bottom Track Amplitude

        /// <summary>
        /// Accumulation of Bottom Track amplitude values.
        /// In dB.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgBtAmpAccum;

        /// <summary>
        /// Number of Bottom Track amplitude values that have been accumulated.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgBtAmpCount;

        #endregion

        #endregion

        #region Average Profile Range

        /// <summary>
        /// Accumulation of Profile range values.
        /// In meters.
        /// The array will hold all 4 beams.
        /// </summary>
        private double[] _avgProfileRangeAccum;

        /// <summary>
        /// Number of Profile Range values that have been accumulated for the average.
        /// In number of values.
        /// The array will hold all 4 beams.
        /// </summary>
        private int[] _avgProfileRangeCount;

        #endregion

        #endregion

        #region Enum

        /// <summary>
        /// Orientation of the beam, to know which beam was forward.
        /// </summary>
        public enum AdcpTestOrientation
        {
            /// <summary>
            /// Beam 0 Forward.
            /// </summary>
            BEAM_0_FORWARD,

            /// <summary>
            /// Beam 1 Forward.
            /// </summary>
            BEAM_1_FORWARD,

            /// <summary>
            /// Beam 2 Forward.
            /// </summary>
            BEAM_2_FORWARD,

            /// <summary>
            /// Beam 3 Forward.
            /// </summary>
            BEAM_3_FORWARD,

            /// <summary>
            /// Vertical Beam.
            /// </summary>
            VERTICAL_BEAM
        }

        #endregion

        #region Properties

        #region Distance Traveled

        /// <summary>
        /// Declination for the area in the world the testing is being done.
        /// </summary>
        public double Declination
        {
            get { return _distanceTraveled.Declination; }
            set
            {
                _distanceTraveled.Declination = value;
                this.NotifyOfPropertyChange(() => this.Declination);
            }
        }

        /// <summary>
        /// Bin selected to calculate distance traveled in
        /// Water Profile data.
        /// </summary>
        public int NavBin
        {
            get { return _distanceTraveled.NavBin; }
            set
            {
                _distanceTraveled.NavBin = value;
                this.NotifyOfPropertyChange(() => this.NavBin);
            }
        }

        /// <summary>
        /// XOffset for the Lineseries.
        /// </summary>
        public double XOffset
        {
            get { return _distanceTraveled.XOffset; }
            set
            {
                _distanceTraveled.XOffset = value;
                this.NotifyOfPropertyChange(() => this.XOffset);
            }
        }

        /// <summary>
        /// YOffset for the Lineseries.
        /// </summary>
        public double YOffset
        {
            get { return _distanceTraveled.YOffset; }
            set
            {
                _distanceTraveled.XOffset = value;
                this.NotifyOfPropertyChange(() => this.YOffset);
            }
        }

        #region GPS

        /// <summary>
        /// Gps Direction.
        /// </summary>
        public string GpsDir
        {
            get { return _distanceTraveled.GpsDir.ToString("0.000") + "°"; }
        }

        /// <summary>
        /// Gps Magnitude
        /// </summary>
        public string GpsMag
        {
            get { return _distanceTraveled.GpsMag.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Gps Magnitude
        /// </summary>
        public double GpsDistance
        {
            get { return _distanceTraveled.GpsMag; }
        }

        /// <summary>
        /// Gps Line series.
        /// </summary>
        public LineSeries GpsPoints
        {
            get { return _distanceTraveled.GpsPoints; }
        }

        #endregion

        #region Bottom Track

        #region Earth

        /// <summary>
        /// Bottom Track Earth East.
        /// </summary>
        public string BtE
        {
            get { return _distanceTraveled.BtE.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Earth North.
        /// </summary>
        public string BtN
        {
            get { return _distanceTraveled.BtN.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Earth Up.
        /// </summary>
        public string BtU
        {
            get { return _distanceTraveled.BtU.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Earth Direction.
        /// </summary>
        public string BtEarthDir
        {
            get { return _distanceTraveled.BtEarthDir.ToString("0.000") + "°"; }
        }

        /// <summary>
        /// Bottom Track Earth Magnitude.
        /// </summary>
        public string BtEarthMag
        {
            get { return _distanceTraveled.BtEarthMag.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Earth Magnitude.
        /// </summary>
        public double BtEarthDistance
        {
            get { return _distanceTraveled.BtEarthMag; }
        }

        /// <summary>
        /// Bottom Track Earth Line series.
        /// </summary>
        public LineSeries BtEarthPoints
        {
            get { return _distanceTraveled.BtEarthPoints; }
        }

        /// <summary>
        /// Bottom Track Earth Percent Error.
        /// </summary>
        public string BtEarthPercentError
        {
            get { return _distanceTraveled.BtEarthPercentError.ToString("0.000") + "%"; }
        }

        /// <summary>
        /// Bottom Track Earth Direction Error.
        /// </summary>
        public string BtEarthDirError
        {
            get { return _distanceTraveled.BtEarthDirError.ToString("0.000") + "°"; }
        }

        #endregion

        #region Instrument

        /// <summary>
        /// Bottom Track Instrument X.
        /// </summary>
        public string BtX
        {
            get { return _distanceTraveled.BtX.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Instrument Y.
        /// </summary>
        public string BtY
        {
            get { return _distanceTraveled.BtY.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Instrument Z.
        /// </summary>
        public string BtZ
        {
            get { return _distanceTraveled.BtZ.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Instrument Direction.
        /// </summary>
        public string BtInstrumentDir
        {
            get { return _distanceTraveled.BtInstrumentDir.ToString("0.000") + "°"; }
        }

        /// <summary>
        /// Bottom Track Instrument Magnitude.
        /// </summary>
        public string BtInstrumentMag
        {
            get { return _distanceTraveled.BtInstrumentMag.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Bottom Track Instrument Line series.
        /// </summary>
        public LineSeries BtInstrumentPoints
        {
            get { return _distanceTraveled.BtInstrumentPoints; }
        }

        /// <summary>
        /// Bottom Track Instrument Percent Error.
        /// </summary>
        public string BtInstrumentPercentError
        {
            get { return _distanceTraveled.BtInstrumentPercentError.ToString("0.000") + "%"; }
        }

        /// <summary>
        /// Bottom Track Instrument Direction Error.
        /// </summary>
        public string BtInstrumentDirError
        {
            get { return _distanceTraveled.BtInstrumentDirError.ToString("0.000") + "°"; }
        }

        #endregion

        #endregion

        #region Water Profile

        #region Earth

        /// <summary>
        /// Water Profile Earth East.
        /// </summary>
        public string WpE
        {
            get { return _distanceTraveled.WpE.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Earth North.
        /// </summary>
        public string WpN
        {
            get { return _distanceTraveled.WpN.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Earth Up.
        /// </summary>
        public string WpU
        {
            get { return _distanceTraveled.WpU.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Earth Direction.
        /// </summary>
        public string WpEarthDir
        {
            get { return _distanceTraveled.WpEarthDir.ToString("0.000") + "°"; }
        }

        /// <summary>
        /// Water Profile Earth Magnitude.
        /// </summary>
        public string WpEarthMag
        {
            get { return _distanceTraveled.WpEarthMag.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Earth Line series.
        /// </summary>
        public LineSeries WpEarthPoints
        {
            get { return _distanceTraveled.WpEarthPoints; }
        }

        /// <summary>
        /// Water Profile Earth Percent Error.
        /// </summary>
        public string WpEarthPercentError
        {
            get { return _distanceTraveled.WpEarthPercentError.ToString("0.000") + "%"; }
        }

        /// <summary>
        /// Water Profile Earth Direction Error.
        /// </summary>
        public string WpEarthDirError
        {
            get { return _distanceTraveled.WpEarthDirError.ToString("0.000") + "°"; }
        }

        #endregion

        #region Instrument

        /// <summary>
        /// Water Profile Instrument X.
        /// </summary>
        public string WpX
        {
            get { return _distanceTraveled.WpX.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Instrument Y.
        /// </summary>
        public string WpY
        {
            get { return _distanceTraveled.WpY.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Instrument Z.
        /// </summary>
        public string WpZ
        {
            get { return _distanceTraveled.WpZ.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Instrument Direction.
        /// </summary>
        public string WpInstrumentDir
        {
            get { return _distanceTraveled.WpInstrumentDir.ToString("0.000") + "°"; }
        }

        /// <summary>
        /// Water Profile Instrument Magnitude.
        /// </summary>
        public string WpInstrumentMag
        {
            get { return _distanceTraveled.WpInstrumentMag.ToString("0.000") + "m"; }
        }

        /// <summary>
        /// Water Profile Instrument Line series.
        /// </summary>
        public LineSeries WpInstrumentPoints
        {
            get { return _distanceTraveled.WpInstrumentPoints; }
        }

        /// <summary>
        /// Water Profile Instrument Percent Error.
        /// </summary>
        public string WpInstrumentPercentError
        {
            get { return _distanceTraveled.WpInstrumentPercentError.ToString("0.000") + "%"; }
        }

        /// <summary>
        /// Water Profile Instrument Direction Error.
        /// </summary>
        public string WpInstrumentDirError
        {
            get { return _distanceTraveled.WpInstrumentDirError.ToString("0.000") + "°"; }
        }

        #endregion

        #endregion

        #endregion

        #region Report

        /// <summary>
        /// Date and Time of the data collection.
        /// </summary>
        private DateTime _dateAndTime;
        /// <summary>
        /// Date and Time of the data collection.
        /// </summary>
        public DateTime DateAndTime
        {
            get { return _dateAndTime; }
            set
            {
                _dateAndTime = value;
                this.NotifyOfPropertyChange(() => this.DateAndTime);
            }
        }

        /// <summary>
        /// Number of ensembles.
        /// </summary>
        private int _numEnsembles;
        /// <summary>
        /// Number of ensembles.
        /// </summary>
        public int NumEnsembles
        {
            get { return _numEnsembles; }
            set
            {
                _numEnsembles = value;
                this.NotifyOfPropertyChange(() => this.NumEnsembles);
            }
        }

        /// <summary>
        /// Number of bins in the ensemble.
        /// </summary>
        private int _numBins;
        /// <summary>
        /// Number of bins in the ensemble.
        /// </summary>
        public int NumBins
        {
            get { return _numBins; }
            set
            {
                _numBins = value;
                this.NotifyOfPropertyChange(() => this.NumBins);
            }
        }

        /// <summary>
        /// Bin Size in the ensemble.
        /// </summary>
        private float _BinSize;
        /// <summary>
        /// Bin Size in the ensemble.
        /// </summary>
        public float BinSize
        {
            get { return _BinSize; }
            set
            {
                _BinSize = value;
                this.NotifyOfPropertyChange(() => this.BinSize);
            }
        }

        #endregion

        #region Average Amplitude

        #region Tank

        #region Depth Tank 1 meter Signal

        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1mB0;
        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1mB0
        {
            get { return _avgAmpTank1mB0; }
            set
            {
                _avgAmpTank1mB0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB0Str);
            }
        }

        /// <summary>
        /// Average 1m Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTank1mB0Str
        {
            get
            {
                return _avgAmpTank1mB0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1mB1;
        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1mB1
        {
            get { return _avgAmpTank1mB1; }
            set
            {
                _avgAmpTank1mB1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB1Str);
            }
        }

        /// <summary>
        /// Average 1m Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTank1mB1Str
        {
            get
            {
                return _avgAmpTank1mB1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1mB2;
        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1mB2
        {
            get { return _avgAmpTank1mB2; }
            set
            {
                _avgAmpTank1mB2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB2Str);
            }
        }

        /// <summary>
        /// Average 1m Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTank1mB2Str
        {
            get
            {
                return _avgAmpTank1mB2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1mB3;
        /// <summary>
        /// Average Amplitude Tank 1 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1mB3
        {
            get { return _avgAmpTank1mB3; }
            set
            {
                _avgAmpTank1mB3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1mB3Str);
            }
        }

        /// <summary>
        /// Average 1m Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTank1mB3Str
        {
            get
            {
                return _avgAmpTank1mB3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 1 meter SNR

        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1mB0;
        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1mB0
        {
            get { return _avgAmpTankSnr1mB0; }
            set
            {
                _avgAmpTankSnr1mB0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB0Str);
            }
        }

        /// <summary>
        /// Average SNR 1m Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1mB0Str
        {
            get
            {
                return _avgAmpTankSnr1mB0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1mB1;
        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1mB1
        {
            get { return _avgAmpTankSnr1mB1; }
            set
            {
                _avgAmpTankSnr1mB1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB1Str);
            }
        }

        /// <summary>
        /// Average SNR 1m Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1mB1Str
        {
            get
            {
                return _avgAmpTankSnr1mB1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1mB2;
        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1mB2
        {
            get { return _avgAmpTankSnr1mB2; }
            set
            {
                _avgAmpTankSnr1mB2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB2Str);
            }
        }

        /// <summary>
        /// Average SNR 1m Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1mB2Str
        {
            get
            {
                return _avgAmpTankSnr1mB2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1mB3;
        /// <summary>
        /// Average Amplitude Tank SNR 1 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1mB3
        {
            get { return _avgAmpTankSnr1mB3; }
            set
            {
                _avgAmpTankSnr1mB3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1mB3Str);
            }
        }

        /// <summary>
        /// Average SNR 1m Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1mB3Str
        {
            get
            {
                return _avgAmpTankSnr1mB3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 300 kHz meters Noise

        /// <summary>
        /// First depth to measure.  Default depth Tank 300 kHz.
        /// </summary>
        private double _depthTank300;
        /// <summary>
        /// First depth to measure.  Default depth Tank 300 kHz.
        /// </summary>
        public double DepthTank300
        {
            get { return _depthTank300; }
            set
            {
                _depthTank300 = value;
                this.NotifyOfPropertyChange(() => this.DepthTank300);
                this.NotifyOfPropertyChange(() => this.DepthTank300Str);
            }
        }

        /// <summary>
        /// Depth Tank 300 kHz to a single digit string.
        /// </summary>
        public string DepthTank300Str
        {
            get
            {
                return _depthTank300.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank300B0;
        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank300B0
        {
            get { return _avgAmpTank300B0; }
            set
            {
                _avgAmpTank300B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B0Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTank300B0Str
        {
            get
            {
                return _avgAmpTank300B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank300B1;
        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank300B1
        {
            get { return _avgAmpTank300B1; }
            set
            {
                _avgAmpTank300B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B1Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTank300B1Str
        {
            get
            {
                return _avgAmpTank300B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank300B2;
        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank300B2
        {
            get { return _avgAmpTank300B2; }
            set
            {
                _avgAmpTank300B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B2Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTank300B2Str
        {
            get
            {
                return _avgAmpTank300B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank300B3;
        /// <summary>
        /// Average Amplitude Tank 300 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank300B3
        {
            get { return _avgAmpTank300B3; }
            set
            {
                _avgAmpTank300B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank300B3Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTank300B3Str
        {
            get
            {
                return _avgAmpTank300B3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 300 kHz SNR

        /// <summary>
        /// Average Amplitude Tank SNR 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr300B0;
        /// <summary>
        /// Average Amplitude Tank SNR 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr300B0
        {
            get { return _avgAmpTankSnr300B0; }
            set
            {
                _avgAmpTankSnr300B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B0Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr300B0Str
        {
            get
            {
                return _avgAmpTankSnr300B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr300B1;
        /// <summary>
        /// Average Amplitude Tank SNR 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr300B1
        {
            get { return _avgAmpTankSnr300B1; }
            set
            {
                _avgAmpTankSnr300B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B1Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr300B1Str
        {
            get
            {
                return _avgAmpTankSnr300B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr300B2;
        /// <summary>
        /// Average Amplitude Tank SNR 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr300B2
        {
            get { return _avgAmpTankSnr300B2; }
            set
            {
                _avgAmpTankSnr300B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B2Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr300B2Str
        {
            get
            {
                return _avgAmpTankSnr300B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr300B3;
        /// <summary>
        /// Average Amplitude Tank SNR 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr300B3
        {
            get { return _avgAmpTankSnr300B3; }
            set
            {
                _avgAmpTankSnr300B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr300B3Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr300B3Str
        {
            get
            {
                return _avgAmpTankSnr300B3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 600 kHz meters Noise

        /// <summary>
        /// Second depth to measure.  Default depth Tank 600 kHz.
        /// </summary>
        private double _depthTank600;
        /// <summary>
        /// Second depth to measure.  Default depth Tank 600 kHz.
        /// </summary>
        public double DepthTank600
        {
            get { return _depthTank600; }
            set
            {
                _depthTank600 = value;
                this.NotifyOfPropertyChange(() => this.DepthTank600);
                this.NotifyOfPropertyChange(() => this.DepthTank600Str);
            }
        }

        /// <summary>
        /// Depth Tank 600 kHz to a single digit string.
        /// </summary>
        public string DepthTank600Str
        {
            get
            {
                return _depthTank600.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank600B0;
        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank600B0
        {
            get { return _avgAmpTank600B0; }
            set
            {
                _avgAmpTank600B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B0Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTank600B0Str
        {
            get
            {
                return _avgAmpTank600B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank600B1;
        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank600B1
        {
            get { return _avgAmpTank600B1; }
            set
            {
                _avgAmpTank600B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B1Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTank600B1Str
        {
            get
            {
                return _avgAmpTank600B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank600B2;
        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank600B2
        {
            get { return _avgAmpTank600B2; }
            set
            {
                _avgAmpTank600B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B2Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTank600B2Str
        {
            get
            {
                return _avgAmpTank600B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank600B3;
        /// <summary>
        /// Average Amplitude Tank 600 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank600B3
        {
            get { return _avgAmpTank600B3; }
            set
            {
                _avgAmpTank600B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank600B3Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTank600B3Str
        {
            get
            {
                return _avgAmpTank600B3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 600 kHz SNR

        /// <summary>
        /// Average Amplitude Tank SNR 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr600B0;
        /// <summary>
        /// Average Amplitude Tank SNR 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr600B0
        {
            get { return _avgAmpTankSnr600B0; }
            set
            {
                _avgAmpTankSnr600B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B0Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr600B0Str
        {
            get
            {
                return _avgAmpTankSnr600B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 600 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr600B1;
        /// <summary>
        /// Average Amplitude Tank SNR 600 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr600B1
        {
            get { return _avgAmpTankSnr600B1; }
            set
            {
                _avgAmpTankSnr600B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B1Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr600B1Str
        {
            get
            {
                return _avgAmpTankSnr600B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 600 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr600B2;
        /// <summary>
        /// Average Amplitude Tank SNR 600 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr600B2
        {
            get { return _avgAmpTankSnr600B2; }
            set
            {
                _avgAmpTankSnr600B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B2Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr600B2Str
        {
            get
            {
                return _avgAmpTankSnr600B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 600 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr600B3;
        /// <summary>
        /// Average Amplitude Tank SNR 600 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr600B3
        {
            get { return _avgAmpTankSnr600B3; }
            set
            {
                _avgAmpTankSnr600B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr600B3Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr600B3Str
        {
            get
            {
                return _avgAmpTankSnr600B3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 1200 kHz meters Noise

        /// <summary>
        /// Third depth to measure.  Default depth Tank 1200 kHz.
        /// </summary>
        private double _depthTank1200;
        /// <summary>
        /// Third depth to measure.  Default depth Tank 1200 kHz.
        /// </summary>
        public double DepthTank1200
        {
            get { return _depthTank1200; }
            set
            {
                _depthTank1200 = value;
                this.NotifyOfPropertyChange(() => this.DepthTank1200);
                this.NotifyOfPropertyChange(() => this.DepthTank1200Str);
            }
        }

        /// <summary>
        /// Depth Tank 1200 kHz to a single digit string.
        /// </summary>
        public string DepthTank1200Str
        {
            get
            {
                return _depthTank1200.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1200B0;
        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1200B0
        {
            get { return _avgAmpTank1200B0; }
            set
            {
                _avgAmpTank1200B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B0Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTank1200B0Str
        {
            get
            {
                return _avgAmpTank1200B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1200B1;
        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1200B1
        {
            get { return _avgAmpTank1200B1; }
            set
            {
                _avgAmpTank1200B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B1Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTank1200B1Str
        {
            get
            {
                return _avgAmpTank1200B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1200B2;
        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1200B2
        {
            get { return _avgAmpTank1200B2; }
            set
            {
                _avgAmpTank1200B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B2Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTank1200B2Str
        {
            get
            {
                return _avgAmpTank1200B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTank1200B3;
        /// <summary>
        /// Average Amplitude Tank 1200 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTank1200B3
        {
            get { return _avgAmpTank1200B3; }
            set
            {
                _avgAmpTank1200B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTank1200B3Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTank1200B3Str
        {
            get
            {
                return _avgAmpTank1200B3.ToString("0");
            }
        }

        #endregion

        #region Depth Tank 1200 kHz SNR

        /// <summary>
        /// Average Amplitude Tank SNR 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1200B0;
        /// <summary>
        /// Average Amplitude Tank SNR 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1200B0
        {
            get { return _avgAmpTankSnr1200B0; }
            set
            {
                _avgAmpTankSnr1200B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B0Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Tank Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1200B0Str
        {
            get
            {
                return _avgAmpTankSnr1200B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 1200 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1200B1;
        /// <summary>
        /// Average Amplitude Tank SNR 1200 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1200B1
        {
            get { return _avgAmpTankSnr1200B1; }
            set
            {
                _avgAmpTankSnr1200B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B1Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Tank Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1200B1Str
        {
            get
            {
                return _avgAmpTankSnr1200B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 1200 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1200B2;
        /// <summary>
        /// Average Amplitude Tank SNR 1200 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1200B2
        {
            get { return _avgAmpTankSnr1200B2; }
            set
            {
                _avgAmpTankSnr1200B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B2Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Tank Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1200B2Str
        {
            get
            {
                return _avgAmpTankSnr1200B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Tank SNR 1200 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpTankSnr1200B3;
        /// <summary>
        /// Average Amplitude Tank SNR 1200 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpTankSnr1200B3
        {
            get { return _avgAmpTankSnr1200B3; }
            set
            {
                _avgAmpTankSnr1200B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpTankSnr1200B3Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Tank Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpTankSnr1200B3Str
        {
            get
            {
                return _avgAmpTankSnr1200B3.ToString("0");
            }
        }

        #endregion

        #endregion

        #region Lake

        #region Depth Lake 300 kHz meters Noise

        /// <summary>
        /// First depth to measure.  Default depth Lake 300 kHz.
        /// </summary>
        private double _depthLake300;
        /// <summary>
        /// First depth to measure.  Default depth Lake 300 kHz.
        /// </summary>
        public double DepthLake300
        {
            get { return _depthLake300; }
            set
            {
                _depthLake300 = value;
                this.NotifyOfPropertyChange(() => this.DepthLake300);
                this.NotifyOfPropertyChange(() => this.DepthLake300Str);
            }
        }

        /// <summary>
        /// Depth Lake 300 kHz to a single digit string.
        /// </summary>
        public string DepthLake300Str
        {
            get
            {
                return _depthLake300.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake300B0;
        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake300B0
        {
            get { return _avgAmpLake300B0; }
            set
            {
                _avgAmpLake300B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B0Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Lake Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpLake300B0Str
        {
            get
            {
                return _avgAmpLake300B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake300B1;
        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake300B1
        {
            get { return _avgAmpLake300B1; }
            set
            {
                _avgAmpLake300B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B1Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Lake Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpLake300B1Str
        {
            get
            {
                return _avgAmpLake300B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake300B2;
        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake300B2
        {
            get { return _avgAmpLake300B2; }
            set
            {
                _avgAmpLake300B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B2Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Lake Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpLake300B2Str
        {
            get
            {
                return _avgAmpLake300B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake300B3;
        /// <summary>
        /// Average Amplitude Lake 300 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake300B3
        {
            get { return _avgAmpLake300B3; }
            set
            {
                _avgAmpLake300B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake300B3Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Lake Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpLake300B3Str
        {
            get
            {
                return _avgAmpLake300B3.ToString("0");
            }
        }

        #endregion

        #region Depth Lake 300 kHz SNR

        /// <summary>
        /// Average Amplitude Lake SNR 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr300B0;
        /// <summary>
        /// Average Amplitude Lake SNR 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr300B0
        {
            get { return _avgAmpLakeSnr300B0; }
            set
            {
                _avgAmpLakeSnr300B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B0Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Lake Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr300B0Str
        {
            get
            {
                return _avgAmpLakeSnr300B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr300B1;
        /// <summary>
        /// Average Amplitude Lake SNR 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr300B1
        {
            get { return _avgAmpLakeSnr300B1; }
            set
            {
                _avgAmpLakeSnr300B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B1Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Lake Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr300B1Str
        {
            get
            {
                return _avgAmpLakeSnr300B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr300B2;
        /// <summary>
        /// Average Amplitude Lake SNR 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr300B2
        {
            get { return _avgAmpLakeSnr300B2; }
            set
            {
                _avgAmpLakeSnr300B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B2Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Lake Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr300B2Str
        {
            get
            {
                return _avgAmpLakeSnr300B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr300B3;
        /// <summary>
        /// Average Amplitude Lake SNR 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr300B3
        {
            get { return _avgAmpLakeSnr300B3; }
            set
            {
                _avgAmpLakeSnr300B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr300B3Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Lake Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr300B3Str
        {
            get
            {
                return _avgAmpLakeSnr300B3.ToString("0");
            }
        }

        #endregion

        #region Depth Lake 600 kHz meters Noise

        /// <summary>
        /// Second depth to measure.  Default depth Lake 600 kHz.
        /// </summary>
        private double _depthLake600;
        /// <summary>
        /// Second depth to measure.  Default depth Lake 600 kHz.
        /// </summary>
        public double DepthLake600
        {
            get { return _depthLake600; }
            set
            {
                _depthLake600 = value;
                this.NotifyOfPropertyChange(() => this.DepthLake600);
                this.NotifyOfPropertyChange(() => this.DepthLake600Str);
            }
        }

        /// <summary>
        /// Depth Lake 600 kHz to a single digit string.
        /// </summary>
        public string DepthLake600Str
        {
            get
            {
                return _depthLake600.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake600B0;
        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake600B0
        {
            get { return _avgAmpLake600B0; }
            set
            {
                _avgAmpLake600B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B0Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Lake Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpLake600B0Str
        {
            get
            {
                return _avgAmpLake600B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake600B1;
        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake600B1
        {
            get { return _avgAmpLake600B1; }
            set
            {
                _avgAmpLake600B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B1Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Lake Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpLake600B1Str
        {
            get
            {
                return _avgAmpLake600B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake600B2;
        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake600B2
        {
            get { return _avgAmpLake600B2; }
            set
            {
                _avgAmpLake600B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B2Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Lake Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpLake600B2Str
        {
            get
            {
                return _avgAmpLake600B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake600B3;
        /// <summary>
        /// Average Amplitude Lake 600 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake600B3
        {
            get { return _avgAmpLake600B3; }
            set
            {
                _avgAmpLake600B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake600B3Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Lake Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpLake600B3Str
        {
            get
            {
                return _avgAmpLake600B3.ToString("0");
            }
        }

        #endregion

        #region Depth Lake 600 kHz SNR

        /// <summary>
        /// Average Amplitude Lake SNR 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr600B0;
        /// <summary>
        /// Average Amplitude Lake SNR 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr600B0
        {
            get { return _avgAmpLakeSnr600B0; }
            set
            {
                _avgAmpLakeSnr600B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B0Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Lake Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr600B0Str
        {
            get
            {
                return _avgAmpLakeSnr600B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 600 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr600B1;
        /// <summary>
        /// Average Amplitude Lake SNR 600 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr600B1
        {
            get { return _avgAmpLakeSnr600B1; }
            set
            {
                _avgAmpLakeSnr600B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B1Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Lake Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr600B1Str
        {
            get
            {
                return _avgAmpLakeSnr600B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 600 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr600B2;
        /// <summary>
        /// Average Amplitude Lake SNR 600 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr600B2
        {
            get { return _avgAmpLakeSnr600B2; }
            set
            {
                _avgAmpLakeSnr600B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B2Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Lake Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr600B2Str
        {
            get
            {
                return _avgAmpLakeSnr600B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 600 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr600B3;
        /// <summary>
        /// Average Amplitude Lake SNR 600 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr600B3
        {
            get { return _avgAmpLakeSnr600B3; }
            set
            {
                _avgAmpLakeSnr600B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr600B3Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Lake Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr600B3Str
        {
            get
            {
                return _avgAmpLakeSnr600B3.ToString("0");
            }
        }

        #endregion

        #region Depth Lake 1200 kHz meters Noise

        /// <summary>
        /// Third depth to measure.  Default depth Lake 1200 kHz.
        /// </summary>
        private double _depthLake1200;
        /// <summary>
        /// Third depth to measure.  Default depth Lake 1200 kHz.
        /// </summary>
        public double DepthLake1200
        {
            get { return _depthLake1200; }
            set
            {
                _depthLake1200 = value;
                this.NotifyOfPropertyChange(() => this.DepthLake1200);
                this.NotifyOfPropertyChange(() => this.DepthLake1200Str);
            }
        }

        /// <summary>
        /// Depth Lake 1200 kHz to a single digit string.
        /// </summary>
        public string DepthLake1200Str
        {
            get
            {
                return _depthLake1200.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake1200B0;
        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake1200B0
        {
            get { return _avgAmpLake1200B0; }
            set
            {
                _avgAmpLake1200B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B0Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Lake Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpLake1200B0Str
        {
            get
            {
                return _avgAmpLake1200B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake1200B1;
        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake1200B1
        {
            get { return _avgAmpLake1200B1; }
            set
            {
                _avgAmpLake1200B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B1Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Lake Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpLake1200B1Str
        {
            get
            {
                return _avgAmpLake1200B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake1200B2;
        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake1200B2
        {
            get { return _avgAmpLake1200B2; }
            set
            {
                _avgAmpLake1200B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B2Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Lake Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpLake1200B2Str
        {
            get
            {
                return _avgAmpLake1200B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLake1200B3;
        /// <summary>
        /// Average Amplitude Lake 1200 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLake1200B3
        {
            get { return _avgAmpLake1200B3; }
            set
            {
                _avgAmpLake1200B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpLake1200B3Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Lake Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpLake1200B3Str
        {
            get
            {
                return _avgAmpLake1200B3.ToString("0");
            }
        }

        #endregion

        #region Depth Lake 1200 kHz SNR

        /// <summary>
        /// Average Amplitude Lake SNR 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr1200B0;
        /// <summary>
        /// Average Amplitude Lake SNR 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr1200B0
        {
            get { return _avgAmpLakeSnr1200B0; }
            set
            {
                _avgAmpLakeSnr1200B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B0Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Lake Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr1200B0Str
        {
            get
            {
                return _avgAmpLakeSnr1200B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 1200 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr1200B1;
        /// <summary>
        /// Average Amplitude Lake SNR 1200 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr1200B1
        {
            get { return _avgAmpLakeSnr1200B1; }
            set
            {
                _avgAmpLakeSnr1200B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B1Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Lake Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr1200B1Str
        {
            get
            {
                return _avgAmpLakeSnr1200B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 1200 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr1200B2;
        /// <summary>
        /// Average Amplitude Lake SNR 1200 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr1200B2
        {
            get { return _avgAmpLakeSnr1200B2; }
            set
            {
                _avgAmpLakeSnr1200B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B2Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Lake Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr1200B2Str
        {
            get
            {
                return _avgAmpLakeSnr1200B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Lake SNR 1200 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpLakeSnr1200B3;
        /// <summary>
        /// Average Amplitude Lake SNR 1200 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpLakeSnr1200B3
        {
            get { return _avgAmpLakeSnr1200B3; }
            set
            {
                _avgAmpLakeSnr1200B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpLakeSnr1200B3Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Lake Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpLakeSnr1200B3Str
        {
            get
            {
                return _avgAmpLakeSnr1200B3.ToString("0");
            }
        }

        #endregion

        #endregion

        #region Ocean

        #region Depth Ocean 300 kHz Noise

        /// <summary>
        /// Ocean 300 kHz depth to measure.
        /// </summary>
        private double _depthOcean300;
        /// <summary>
        /// Ocean 300 kHz depth to measure.
        /// </summary>
        public double DepthOcean300
        {
            get { return _depthOcean300; }
            set
            {
                _depthOcean300 = value;
                this.NotifyOfPropertyChange(() => this.DepthOcean300);
                this.NotifyOfPropertyChange(() => this.DepthOcean300Str);
            }
        }

        /// <summary>
        /// Depth 300 Ocean to a single digit string.
        /// </summary>
        public string DepthOcean300Str
        {
            get
            {
                return _depthOcean300.ToString("0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean300B0;
        /// <summary>
        /// Average Amplitude Ocean 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean300B0
        {
            get { return _avgAmpOcean300B0; }
            set
            {
                _avgAmpOcean300B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B0Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpOcean300B0Str
        {
            get
            {
                return _avgAmpOcean300B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean300B1;
        /// <summary>
        /// Average Amplitude Ocean 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean300B1
        {
            get { return _avgAmpOcean300B1; }
            set
            {
                _avgAmpOcean300B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B1Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpOcean300B1Str
        {
            get
            {
                return _avgAmpOcean300B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean300B2;
        /// <summary>
        /// Average Amplitude Ocean 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean300B2
        {
            get { return _avgAmpOcean300B2; }
            set
            {
                _avgAmpOcean300B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B2Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpOcean300B2Str
        {
            get
            {
                return _avgAmpOcean300B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean300B3;
        /// <summary>
        /// Average Amplitude Ocean 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean300B3
        {
            get { return _avgAmpOcean300B3; }
            set
            {
                _avgAmpOcean300B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean300B3Str);
            }
        }

        /// <summary>
        /// Average 300 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpOcean300B3Str
        {
            get
            {
                return _avgAmpOcean300B3.ToString("0");
            }
        }

        #endregion

        #region Depth Ocean 300 kHz SNR

        /// <summary>
        /// Average Amplitude Ocean SNR 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr300B0;
        /// <summary>
        /// Average Amplitude Ocean SNR 300 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr300B0
        {
            get { return _avgAmpOceanSnr300B0; }
            set
            {
                _avgAmpOceanSnr300B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B0Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr300B0Str
        {
            get
            {
                return _avgAmpOceanSnr300B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr300B1;
        /// <summary>
        /// Average Amplitude Ocean SNR 300 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr300B1
        {
            get { return _avgAmpOceanSnr300B1; }
            set
            {
                _avgAmpOceanSnr300B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B1Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr300B1Str
        {
            get
            {
                return _avgAmpOceanSnr300B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr300B2;
        /// <summary>
        /// Average Amplitude Ocean SNR 300 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr300B2
        {
            get { return _avgAmpOceanSnr300B2; }
            set
            {
                _avgAmpOceanSnr300B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B2Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr300B2Str
        {
            get
            {
                return _avgAmpOceanSnr300B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr300B3;
        /// <summary>
        /// Average Amplitude Ocean SNR 300 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr300B3
        {
            get { return _avgAmpOceanSnr300B3; }
            set
            {
                _avgAmpOceanSnr300B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr300B3Str);
            }
        }

        /// <summary>
        /// Average SNR 300 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr300B3Str
        {
            get
            {
                return _avgAmpOceanSnr300B3.ToString("0");
            }
        }

        #endregion

        #region Depth Ocean 600 kHz Noise

        /// <summary>
        /// Ocean 600 kHz depth to measure.  Default depth is 10 meters.
        /// </summary>
        private double _depthOcean600;
        /// <summary>
        /// Ocean 600 kHz depth to measure.  Default depth is 10 meters.
        /// </summary>
        public double DepthOcean600
        {
            get { return _depthOcean600; }
            set
            {
                _depthOcean600 = value;
                this.NotifyOfPropertyChange(() => this.DepthOcean600);
                this.NotifyOfPropertyChange(() => this.DepthOcean600Str);
            }
        }

        /// <summary>
        /// Depth Ocean 600 kHz to a single digit string.
        /// </summary>
        public string DepthOcean600Str
        {
            get
            {
                return _depthOcean600.ToString("0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean600B0;
        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean600B0
        {
            get { return _avgAmpOcean600B0; }
            set
            {
                _avgAmpOcean600B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B0Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpOcean600B0Str
        {
            get
            {
                return _avgAmpOcean600B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean600B1;
        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean600B1
        {
            get { return _avgAmpOcean600B1; }
            set
            {
                _avgAmpOcean600B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B1Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpOcean600B1Str
        {
            get
            {
                return _avgAmpOcean600B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean600B2;
        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean600B2
        {
            get { return _avgAmpOcean600B2; }
            set
            {
                _avgAmpOcean600B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B2Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpOcean600B2Str
        {
            get
            {
                return _avgAmpOcean600B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean600B3;
        /// <summary>
        /// Average Amplitude Ocean 600 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean600B3
        {
            get { return _avgAmpOcean600B3; }
            set
            {
                _avgAmpOcean600B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean600B3Str);
            }
        }

        /// <summary>
        /// Average 600 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpOcean600B3Str
        {
            get
            {
                return _avgAmpOcean600B3.ToString("0");
            }
        }

        #endregion

        #region Depth Ocean 600 kHz SNR

        /// <summary>
        /// Average Amplitude Ocean SNR 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr600B0;
        /// <summary>
        /// Average Amplitude Ocean SNR 600 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr600B0
        {
            get { return _avgAmpOceanSnr600B0; }
            set
            {
                _avgAmpOceanSnr600B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B0Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr600B0Str
        {
            get
            {
                return _avgAmpOceanSnr600B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 600 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr600B1;
        /// <summary>
        /// Average Amplitude Ocean SNR 600 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr600B1
        {
            get { return _avgAmpOceanSnr600B1; }
            set
            {
                _avgAmpOceanSnr600B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B1Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr600B1Str
        {
            get
            {
                return _avgAmpOceanSnr600B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 600 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr600B2;
        /// <summary>
        /// Average Amplitude Ocean SNR 600 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr600B2
        {
            get { return _avgAmpOceanSnr600B2; }
            set
            {
                _avgAmpOceanSnr600B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B2Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr600B2Str
        {
            get
            {
                return _avgAmpOceanSnr600B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 600 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr600B3;
        /// <summary>
        /// Average Amplitude Ocean SNR 600 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr600B3
        {
            get { return _avgAmpOceanSnr600B3; }
            set
            {
                _avgAmpOceanSnr600B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr600B3Str);
            }
        }

        /// <summary>
        /// Average SNR 600 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr600B3Str
        {
            get
            {
                return _avgAmpOceanSnr600B3.ToString("0");
            }
        }

        #endregion

        #region Depth Ocean 1200 meters Noise

        /// <summary>
        /// Ocean 1200 kHz depth to measure.  Default depth is 30 meters.
        /// </summary>
        private double _depthOcean1200;
        /// <summary>
        /// Ocean 1200 kHz depth to measure.  Default depth is 30 meters.
        /// </summary>
        public double DepthOcean1200
        {
            get { return _depthOcean1200; }
            set
            {
                _depthOcean1200 = value;
                this.NotifyOfPropertyChange(() => this.DepthOcean1200);
                this.NotifyOfPropertyChange(() => this.DepthOcean1200Str);
            }
        }

        /// <summary>
        /// Depth Ocean 1200 kHz to a single digit string.
        /// </summary>
        public string DepthOcean1200Str
        {
            get
            {
                return _depthOcean1200.ToString("0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean1200B0;
        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean1200B0
        {
            get { return _avgAmpOcean1200B0; }
            set
            {
                _avgAmpOcean1200B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B0Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpOcean1200B0Str
        {
            get
            {
                return _avgAmpOcean1200B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean1200B1;
        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean1200B1
        {
            get { return _avgAmpOcean1200B1; }
            set
            {
                _avgAmpOcean1200B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B1Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpOcean1200B1Str
        {
            get
            {
                return _avgAmpOcean1200B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean1200B2;
        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean1200B2
        {
            get { return _avgAmpOcean1200B2; }
            set
            {
                _avgAmpOcean1200B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B2Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpOcean1200B2Str
        {
            get
            {
                return _avgAmpOcean1200B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOcean1200B3;
        /// <summary>
        /// Average Amplitude Ocean 1200 kHz meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOcean1200B3
        {
            get { return _avgAmpOcean1200B3; }
            set
            {
                _avgAmpOcean1200B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpOcean1200B3Str);
            }
        }

        /// <summary>
        /// Average 1200 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpOcean1200B3Str
        {
            get
            {
                return _avgAmpOcean1200B3.ToString("0");
            }
        }

        #endregion

        #region Depth Ocean 1200 kHz SNR

        /// <summary>
        /// Average Amplitude Ocean SNR 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr1200B0;
        /// <summary>
        /// Average Amplitude Ocean SNR 1200 kHz meter bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr1200B0
        {
            get { return _avgAmpOceanSnr1200B0; }
            set
            {
                _avgAmpOceanSnr1200B0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B0);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B0Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr1200B0Str
        {
            get
            {
                return _avgAmpOceanSnr1200B0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 1200 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr1200B1;
        /// <summary>
        /// Average Amplitude Ocean SNR 1200 meter bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr1200B1
        {
            get { return _avgAmpOceanSnr1200B1; }
            set
            {
                _avgAmpOceanSnr1200B1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B1);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B1Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr1200B1Str
        {
            get
            {
                return _avgAmpOceanSnr1200B1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 1200 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr1200B2;
        /// <summary>
        /// Average Amplitude Ocean SNR 1200 meter bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr1200B2
        {
            get { return _avgAmpOceanSnr1200B2; }
            set
            {
                _avgAmpOceanSnr1200B2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B2);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B2Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr1200B2Str
        {
            get
            {
                return _avgAmpOceanSnr1200B2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Ocean SNR 1200 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpOceanSnr1200B3;
        /// <summary>
        /// Average Amplitude Ocean SNR 1200 meter bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpOceanSnr1200B3
        {
            get { return _avgAmpOceanSnr1200B3; }
            set
            {
                _avgAmpOceanSnr1200B3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B3);
                this.NotifyOfPropertyChange(() => this.AvgAmpOceanSnr1200B3Str);
            }
        }

        /// <summary>
        /// Average SNR 1200 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpOceanSnr1200B3Str
        {
            get
            {
                return _avgAmpOceanSnr1200B3.ToString("0");
            }
        }

        #endregion

        #endregion

        #region Depth Noise

        /// <summary>
        /// Noise depth to measure.
        /// </summary>
        private double _depthNoise;
        /// <summary>
        /// Noise depth to measure.
        /// </summary>
        public double DepthNoise
        {
            get { return _depthNoise; }
            set
            {
                _depthNoise = value;
                this.NotifyOfPropertyChange(() => this.DepthNoise);
                this.NotifyOfPropertyChange(() => this.DepthNoiseStr);
            }
        }

        /// <summary>
        /// Depth noise to a single digit string.
        /// </summary>
        public string DepthNoiseStr
        {
            get
            {
                return _depthNoise.ToString("0") + "m";
            }
        }

        /// <summary>
        /// Average Amplitude Noise bin, Beam 0.
        /// Value in dB.
        /// </summary>
        private double _avgAmpNoiseB0;
        /// <summary>
        /// Average Amplitude Noise bin, Beam 0.
        /// Value in dB.
        /// </summary>
        public double AvgAmpNoiseB0
        {
            get { return _avgAmpNoiseB0; }
            set
            {
                _avgAmpNoiseB0 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB0);
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB0Str);
            }
        }

        /// <summary>
        /// Average Noise 300 kHz Ocean Beam 0 to a single digit string.
        /// </summary>
        public string AvgAmpNoiseB0Str
        {
            get
            {
                return _avgAmpNoiseB0.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Noise bin, Beam 1.
        /// Value in dB.
        /// </summary>
        private double _avgAmpNoiseB1;
        /// <summary>
        /// Average Amplitude Noise bin, Beam 1.
        /// Value in dB.
        /// </summary>
        public double AvgAmpNoiseB1
        {
            get { return _avgAmpNoiseB1; }
            set
            {
                _avgAmpNoiseB1 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB1);
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB1Str);
            }
        }

        /// <summary>
        /// Average Noise 300 kHz Ocean Beam 1 to a single digit string.
        /// </summary>
        public string AvgAmpNoiseB1Str
        {
            get
            {
                return _avgAmpNoiseB1.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Noise bin, Beam 2.
        /// Value in dB.
        /// </summary>
        private double _avgAmpNoiseB2;
        /// <summary>
        /// Average Amplitude Noise bin, Beam 2.
        /// Value in dB.
        /// </summary>
        public double AvgAmpNoiseB2
        {
            get { return _avgAmpNoiseB2; }
            set
            {
                _avgAmpNoiseB2 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB2);
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB2Str);
            }
        }

        /// <summary>
        /// Average Noise 300 kHz Ocean Beam 2 to a single digit string.
        /// </summary>
        public string AvgAmpNoiseB2Str
        {
            get
            {
                return _avgAmpNoiseB2.ToString("0");
            }
        }

        /// <summary>
        /// Average Amplitude Noise bin, Beam 3.
        /// Value in dB.
        /// </summary>
        private double _avgAmpNoiseB3;
        /// <summary>
        /// Average Amplitude Noise bin, Beam 3.
        /// Value in dB.
        /// </summary>
        public double AvgAmpNoiseB3
        {
            get { return _avgAmpNoiseB3; }
            set
            {
                _avgAmpNoiseB3 = value;
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB3);
                this.NotifyOfPropertyChange(() => this.AvgAmpNoiseB3Str);
            }
        }

        /// <summary>
        /// Average Noise 300 kHz Ocean Beam 3 to a single digit string.
        /// </summary>
        public string AvgAmpNoiseB3Str
        {
            get
            {
                return _avgAmpNoiseB3.ToString("0");
            }
        }

        #endregion

        #region Glitch Check

        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        private bool _IsGlitchB0;
        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        public bool IsGlitchB0
        {
            get { return _IsGlitchB0; }
            set
            {
                _IsGlitchB0 = value;
                this.NotifyOfPropertyChange(() => this.IsGlitchB0);
            }
        }

        /// <summary>
        /// Count the number of glitches on beam 0.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        private int _GlitchCountB0;
        /// <summary>
        /// Count the number of glitches on beam 0.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        public int GlitchCountB0
        {
            get { return _GlitchCountB0; }
            set
            {
                _GlitchCountB0 = value;
                this.NotifyOfPropertyChange(() => this.GlitchCountB0);
            }
        }

        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        private bool _IsGlitchB1;
        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        public bool IsGlitchB1
        {
            get { return _IsGlitchB1; }
            set
            {
                _IsGlitchB1 = value;
                this.NotifyOfPropertyChange(() => this.IsGlitchB1);
            }
        }

        /// <summary>
        /// Count the number of glitches on beam 1.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        private int _GlitchCountB1;
        /// <summary>
        /// Count the number of glitches on beam 1.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        public int GlitchCountB1
        {
            get { return _GlitchCountB1; }
            set
            {
                _GlitchCountB1 = value;
                this.NotifyOfPropertyChange(() => this.GlitchCountB1);
            }
        }

        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        private bool _IsGlitchB2;
        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        public bool IsGlitchB2
        {
            get { return _IsGlitchB2; }
            set
            {
                _IsGlitchB2 = value;
                this.NotifyOfPropertyChange(() => this.IsGlitchB2);
            }
        }

        /// <summary>
        /// Count the number of glitches on beam 2.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        private int _GlitchCountB2;
        /// <summary>
        /// Count the number of glitches on beam 2.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        public int GlitchCountB2
        {
            get { return _GlitchCountB2; }
            set
            {
                _GlitchCountB2 = value;
                this.NotifyOfPropertyChange(() => this.GlitchCountB2);
            }
        }

        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        private bool _IsGlitchB3;
        /// <summary>
        /// Flag if the glitch value is is greater than one.
        /// </summary>
        public bool IsGlitchB3
        {
            get { return _IsGlitchB3; }
            set
            {
                _IsGlitchB3 = value;
                this.NotifyOfPropertyChange(() => this.IsGlitchB3);
            }
        }

        /// <summary>
        /// Count the number of glitches on beam 3.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        private int _GlitchCountB3;
        /// <summary>
        /// Count the number of glitches on beam 3.
        /// A glitch occurs when the amplitude spikes.
        /// </summary>
        public int GlitchCountB3
        {
            get { return _GlitchCountB3; }
            set
            {
                _GlitchCountB3 = value;
                this.NotifyOfPropertyChange(() => this.GlitchCountB3);
            }
        }

        #endregion

        #region Bottom Track Amplitude

        /// <summary>
        /// Count the number of bottom track amplitude on beam 0.
        /// </summary>
        private double _BtAmpB0;
        /// <summary>
        /// Count the number of bottom track amplitude on beam 0.
        /// </summary>
        public double BtAmpB0
        {
            get { return _BtAmpB0; }
            set
            {
                _BtAmpB0 = value;
                this.NotifyOfPropertyChange(() => this.BtAmpB0);
            }
        }

        /// <summary>
        /// Count the number of bottom track amplitude on beam 1.
        /// </summary>
        private double _BtAmpB1;
        /// <summary>
        /// Count the number of bottom track amplitude on beam 1.
        /// </summary>
        public double BtAmpB1
        {
            get { return _BtAmpB1; }
            set
            {
                _BtAmpB1 = value;
                this.NotifyOfPropertyChange(() => this.BtAmpB1);
            }
        }

        /// <summary>
        /// Count the number of bottom track amplitude on beam 2.
        /// </summary>
        private double _BtAmpB2;
        /// <summary>
        /// Count the number of bottom track amplitude on beam 2.
        /// </summary>
        public double BtAmpB2
        {
            get { return _BtAmpB2; }
            set
            {
                _BtAmpB2 = value;
                this.NotifyOfPropertyChange(() => this.BtAmpB2);
            }
        }

        /// <summary>
        /// Count the number of bottom track amplitude on beam 3.
        /// </summary>
        private double _BtAmpB3;
        /// <summary>
        /// Count the number of bottom track amplitude on beam 3.
        /// </summary>
        public double BtAmpB3
        {
            get { return _BtAmpB3; }
            set
            {
                _BtAmpB3 = value;
                this.NotifyOfPropertyChange(() => this.BtAmpB3);
            }
        }

        #endregion

        #endregion

        #region Profile Range

        /// <summary>
        /// Profile Range Beam 0.  Use Correlation.
        /// </summary>
        private double _ProfileRangeBeam0;
        /// <summary>
        /// Profile Range Beam 0.  Use Correlation.
        /// </summary>
        public double ProfileRangeBeam0
        {
            get { return _ProfileRangeBeam0; }
            set
            {
                _ProfileRangeBeam0 = value;
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam0);
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam0Str);
            }
        }

        /// <summary>
        /// Profile Range Beam 0  to a single digit string.
        /// </summary>
        public string ProfileRangeBeam0Str
        {
            get
            {
                return _ProfileRangeBeam0.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Profile Range Beam 1.  Use Correlation.
        /// </summary>
        private double _ProfileRangeBeam1;
        /// <summary>
        /// Profile Range Beam 1.  Use Correlation.
        /// </summary>
        public double ProfileRangeBeam1
        {
            get { return _ProfileRangeBeam1; }
            set
            {
                _ProfileRangeBeam1 = value;
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam1);
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam1Str);
            }
        }

        /// <summary>
        /// Profile Range Beam 1  to a single digit string.
        /// </summary>
        public string ProfileRangeBeam1Str
        {
            get
            {
                return _ProfileRangeBeam1.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Profile Range Beam 2.  Use Correlation.
        /// </summary>
        private double _ProfileRangeBeam2;
        /// <summary>
        /// Profile Range Beam 2.  Use Correlation.
        /// </summary>
        public double ProfileRangeBeam2
        {
            get { return _ProfileRangeBeam2; }
            set
            {
                _ProfileRangeBeam2 = value;
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam2);
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam2Str);
            }
        }

        /// <summary>
        /// Profile Range Beam 2  to a single digit string.
        /// </summary>
        public string ProfileRangeBeam2Str
        {
            get
            {
                return _ProfileRangeBeam2.ToString("0.0") + "m";
            }
        }

        /// <summary>
        /// Profile Range Beam 3.  Use Correlation.
        /// </summary>
        private double _ProfileRangeBeam3;
        /// <summary>
        /// Profile Range Beam 3.  Use Correlation.
        /// </summary>
        public double ProfileRangeBeam3
        {
            get { return _ProfileRangeBeam3; }
            set
            {
                _ProfileRangeBeam3 = value;
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam3);
                this.NotifyOfPropertyChange(() => this.ProfileRangeBeam3Str);
            }
        }

        /// <summary>
        /// Profile Range Beam 3 to a single digit string.
        /// </summary>
        public string ProfileRangeBeam3Str
        {
            get
            {
                return _ProfileRangeBeam3.ToString("0.0") + "m";
            }
        }

        #endregion

        #region Test Orientation

        /// <summary>
        /// Test Orientation of the ADCP.
        /// </summary>
        private AdcpTestOrientation _TestOrientation;
        /// <summary>
        /// Test Orientation of the ADCP.
        /// </summary>
        public AdcpTestOrientation TestOrientation
        {
            get { return _TestOrientation; }
            set
            {
                _TestOrientation = value;
                this.NotifyOfPropertyChange(() => this.TestOrientation);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public ProjectReportText(int numBeams = DEFAULT_NUM_BEAMS)
        {
            _numBeams = numBeams;
            _distanceTraveled = new DistanceTraveled();
            _dateAndTime = new DateTime();
            NumEnsembles = 0;
            NumBins = 0;
            TestOrientation = AdcpTestOrientation.BEAM_0_FORWARD;

            // Depths for Lake to measure
            DepthTank300 = DEFAULT_SNR_DEPTH_300_TANK;                      // 300  kHz Tank
            DepthTank600 = DEFAULT_SNR_DEPTH_600_TANK;                      // 600  kHz Tank
            DepthTank1200 = DEFAULT_SNR_DEPTH_1200_TANK;                    // 1200 kHz Tank

            // Depths for Lake to measure
            DepthLake300 = DEFAULT_SNR_DEPTH_300_LAKE;                      // 300  kHz Lake
            DepthLake600 = DEFAULT_SNR_DEPTH_600_LAKE;                      // 600  kHz Lake
            DepthLake1200 = DEFAULT_SNR_DEPTH_1200_LAKE;                    // 1200 kHz Lake

            // Depths for Ocean to measure
            DepthOcean300 = DEFAULT_SNR_DEPTH_300_OCEAN;                    // 300  kHz Ocean
            DepthOcean600 = DEFAULT_SNR_DEPTH_600_OCEAN;                    // 600  kHz Ocean
            DepthOcean1200 = DEFAULT_SNR_DEPTH_1200_OCEAN;                  // 1200 kHz Ocean

            // Depth of Noise
            DepthNoise = 0.0;

            // Glitch Check
            GlitchCountB0 = 0;
            GlitchCountB1 = 0;
            GlitchCountB2 = 0;
            GlitchCountB3 = 0;

            // Bottom Track Amplitude
            BtAmpB0 = 0.0;
            BtAmpB1 = 0.0;
            BtAmpB2 = 0.0;
            BtAmpB3 = 0.0;

            IsGlitchB0 = false;
            IsGlitchB1 = false;
            IsGlitchB2 = false;
            IsGlitchB3 = false;

            // Average Tank data
            _avgAmpTank1mAccum = new double[numBeams];                     // Tank 1m Accum
            _avgAmpTank1mCount = new int[numBeams];                        // Tank 1m Count 
            _avgAmpTank300Accum = new double[numBeams];                     // Tank 300 Accum
            _avgAmpTank300Count = new int[numBeams];                        // Tank 300 Count 
            _avgAmpTank600Accum = new double[numBeams];                     // Tank 600 Accum
            _avgAmpTank600Count = new int[numBeams];                        // Tank 600 Count
            _avgAmpTank1200Accum = new double[numBeams];                    // Tank 1200 Accum
            _avgAmpTank1200Count = new int[numBeams];                       // Tank 1200 Count

            // Average Lake data
            _avgAmpLake300Accum = new double[numBeams];                     // Lake 300 Accum
            _avgAmpLake300Count = new int[numBeams];                        // Lake 300 Count 
            _avgAmpLake600Accum = new double[numBeams];                     // Lake 600 Accum
            _avgAmpLake600Count = new int[numBeams];                        // Lake 600 Count
            _avgAmpLake1200Accum = new double[numBeams];                    // Lake 1200 Accum
            _avgAmpLake1200Count = new int[numBeams];                       // Lake 1200 Count

            // Average Ocean data
            _avgAmpOcean300Accum = new double[numBeams];                    // Ocean 300 Accum
            _avgAmpOcean300Count = new int[numBeams];                       // Ocean 300 Count
            _avgAmpOcean600Accum = new double[numBeams];                    // Ocean 600 Accum
            _avgAmpOcean600Count = new int[numBeams];                       // Ocean 600 Count
            _avgAmpOcean1200Accum = new double[numBeams];                   // Ocean 1200 Accum
            _avgAmpOcean1200Count = new int[numBeams];                      // Ocean 1200 Count

            // Average Noise data
            _avgAmpNoiseAccum = new double[numBeams];                       // Noise Accum
            _avgAmpNoiseCount = new int[numBeams];                          // Noise Count

            // Profile Range Average
            _avgProfileRangeAccum = new double[numBeams];                    // Profile Accum
            _avgProfileRangeCount = new int[numBeams];                       // Profile Count

            // Bottom Track Amplitude
            _avgBtAmpAccum = new double[numBeams];                          // Bottom Track Amp Accum
            _avgBtAmpCount = new int[numBeams];                             // Bottom Track Amp Count

            // Profile Range
            ProfileRangeBeam0 = 0;
            ProfileRangeBeam1 = 0;
            ProfileRangeBeam2 = 0;
            ProfileRangeBeam3 = 0;

            ClearAverageAmplitude();
        }

        /// <summary>
        /// Clear the object.
        /// </summary>
        public void Clear()
        {
            // Clear DMG values
            _distanceTraveled.Clear();
            PropertyChangedDistanceTraveled();
            NumEnsembles = 0;
            _dateAndTime = new DateTime();
            NumBins = 0;

            // Depth of Noise
            DepthNoise = 0.0;

            // Glitch Check
            GlitchCountB0 = 0;
            GlitchCountB1 = 0;
            GlitchCountB2 = 0;
            GlitchCountB3 = 0;
            IsGlitchB0 = false;
            IsGlitchB1 = false;
            IsGlitchB2 = false;
            IsGlitchB3 = false;

            // Bottom Track Amplitude
            BtAmpB0 = 0.0;
            BtAmpB1 = 0.0;
            BtAmpB2 = 0.0;
            BtAmpB3 = 0.0;

            // Profile Range
            ProfileRangeBeam0 = 0;
            ProfileRangeBeam1 = 0;
            ProfileRangeBeam2 = 0;
            ProfileRangeBeam3 = 0;

            // Clear Average Amplitude values
            ClearAverageAmplitude();
        }

        /// <summary>
        /// Load an entire cache of data to be calcualted and displayed.
        /// </summary>
        /// <param name="cache"></param>
        public void LoadData(Cache<long, DataSet.Ensemble> cache, Subsystem subsystem, SubsystemDataConfig ssConfig)
        {
            // Distance Traveled
            //await Task.Run(() => _distanceTraveled.Calculate(cache));
            _distanceTraveled.Calculate(cache, subsystem, ssConfig);
            PropertyChangedDistanceTraveled();

            for (int x = 0; x < cache.Count(); x++ )
            {
                // Get the ensembles
                var ensemble = cache.IndexValue(x);

                // Verify the subsystem matches this viewmodel's subystem.
                if ((subsystem == ensemble.EnsembleData.GetSubSystem())                 // Check if Subsystem matches 
                        && (ssConfig == ensemble.EnsembleData.SubsystemConfig))         // Check if Subsystem Config matches
                {

                    // Average Amplitude
                    AverageAmplitude(ensemble);

                    // Glitch check
                    GlitchCheck(ensemble);

                    // Profile Range
                    ProfileRange(ensemble);

                    // Get report Info
                    SetReportInfo(ensemble);

                    // Set Test Orientation
                    SetTestOrientation(ensemble);

                    NumEnsembles++;
                }
            }
        }

        /// <summary>
        /// Load an ensemble to be calculated and displayed.
        /// </summary>
        /// <param name="ensemble">Ensemble data.</param>
        public void AddIncomingData(DataSet.Ensemble ensemble)
        {
            //if (ensemble != null && ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail && ensemble.IsAmplitudeAvail)
            if (ensemble != null)
            {
                lock (ensemble.SyncRoot)
                {
                    // Distance Traveled
                    _distanceTraveled.AddIncomingData(ensemble);
                    PropertyChangedDistanceTraveled();

                    // Average Amplitude
                    AverageAmplitude(ensemble);

                    // Glitch check
                    GlitchCheck(ensemble);

                    // Profile Range
                    ProfileRange(ensemble);

                    // Get report Info
                    SetReportInfo(ensemble);

                    // Set Test Orientation
                    SetTestOrientation(ensemble);
                }

                // Set number of ensembles
                NumEnsembles++;
            }
        }

        #region Distance Traveled

        /// <summary>
        /// Call property changed all the Distance traveled properties.
        /// </summary>
        private void PropertyChangedDistanceTraveled()
        {
            // GPS
            this.NotifyOfPropertyChange(() => this.GpsDir);
            this.NotifyOfPropertyChange(() => this.GpsMag);

            // Bottom Track Earth
            this.NotifyOfPropertyChange(() => this.BtEarthDir);
            this.NotifyOfPropertyChange(() => this.BtEarthMag);
            this.NotifyOfPropertyChange(() => this.BtEarthPercentError);
            this.NotifyOfPropertyChange(() => this.BtEarthDirError);
            this.NotifyOfPropertyChange(() => this.BtE);
            this.NotifyOfPropertyChange(() => this.BtN);
            this.NotifyOfPropertyChange(() => this.BtU);

            // Bottom Track Instrument
            this.NotifyOfPropertyChange(() => this.BtInstrumentDir);
            this.NotifyOfPropertyChange(() => this.BtInstrumentMag);
            this.NotifyOfPropertyChange(() => this.BtInstrumentPercentError);
            this.NotifyOfPropertyChange(() => this.BtInstrumentDirError);
            this.NotifyOfPropertyChange(() => this.BtX);
            this.NotifyOfPropertyChange(() => this.BtY);
            this.NotifyOfPropertyChange(() => this.BtZ);

            // Water Profile Earth
            this.NotifyOfPropertyChange(() => this.WpEarthDir);
            this.NotifyOfPropertyChange(() => this.WpEarthMag);
            this.NotifyOfPropertyChange(() => this.WpEarthPercentError);
            this.NotifyOfPropertyChange(() => this.WpEarthDirError);
            this.NotifyOfPropertyChange(() => this.WpE);
            this.NotifyOfPropertyChange(() => this.WpN);
            this.NotifyOfPropertyChange(() => this.WpU);

            // Water Profile Instrument
            this.NotifyOfPropertyChange(() => this.WpInstrumentDir);
            this.NotifyOfPropertyChange(() => this.WpInstrumentMag);
            this.NotifyOfPropertyChange(() => this.WpInstrumentPercentError);
            this.NotifyOfPropertyChange(() => this.WpInstrumentDirError);
            this.NotifyOfPropertyChange(() => this.WpX);
            this.NotifyOfPropertyChange(() => this.WpY);
            this.NotifyOfPropertyChange(() => this.WpZ);
        }

        #endregion

        #region Average Amplitude

        /// <summary>
        /// Accumulate the average for the Amplitude values at the different depths.
        /// If the bins do not go to the depth, then use the last bin.
        /// </summary>
        /// <param name="ensemble">Ensemble to accumulate.</param>
        private void AverageAmplitude(DataSet.Ensemble ensemble)
        {
            if (ensemble != null && ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail && ensemble.IsAmplitudeAvail)
            {
                // Noise must go first
                #region Noise

                //// Noise
                //int binLocNoise = ensemble.EnsembleData.NumBins - 1;                // Noise. Subtract because start with 0.
                //DepthNoise = binLocNoise * ensemble.AncillaryData.BinSize;          // Set the depth of the noise
                int binLocNoise = GetNoiseFloorLocation(ensemble);

                // Noise Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeNoise(ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeNoise(ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeNoise(ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }


                #endregion

                #region Tank

                int binLocTank1m = GetTank1mBinLocation(ensemble);

                int binLocTank300 = GetTank300BinLocation(ensemble);

                int binLocTank600 = GetTank600BinLocation(ensemble);

                int binLocTank1200 = GetLake1200BinLocation(ensemble);


                // Tank 1m Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeTank1m(ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeTank1m(ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeTank1m(ensemble.AmplitudeData.AmplitudeData[binLocTank1m, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Tank 300 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeTank300(ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeTank300(ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeTank300(ensemble.AmplitudeData.AmplitudeData[binLocTank300, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Tank 600 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeTank600(ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeTank600(ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeTank600(ensemble.AmplitudeData.AmplitudeData[binLocTank600, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Tank 1200 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeTank1200(ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeTank1200(ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeTank1200(ensemble.AmplitudeData.AmplitudeData[binLocTank1200, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                #endregion

                #region Lake

                // Lake
                //int binLocLake300 = (int)(DepthLake300 / binSize) - 1;                // Lake 300 meters. Let it truncate the value  Subtract because start with 0.
                //if (binLocLake300 < 0)
                //{
                //    binLocLake300 = 0;
                //}
                //if (binLocLake300 >= ensemble.EnsembleData.NumBins)
                //{
                //    binLocLake300 = ensemble.EnsembleData.NumBins - 1;
                //}
                int binLocLake300 = GetLake300BinLocation(ensemble);

                //int binLocLake600 = (int)(DepthLake600 / binSize) - 1;                // Lake 600 meters. Let it truncate the value  Subtract because start with 0.
                //if (binLocLake600 < 0)
                //{
                //    binLocLake600 = 0;
                //}
                //if (binLocLake600 >= ensemble.EnsembleData.NumBins)
                //{
                //    binLocLake600 = ensemble.EnsembleData.NumBins - 1;
                //}
                int binLocLake600 = GetLake600BinLocation(ensemble);

                //int binLocLake1200 = (int)(DepthLake1200 / binSize) - 1;                // Lake 1200 meters. Let it truncate the value.  Subtract because start with 0.
                //if (binLocLake1200 < 0)
                //{
                //    binLocLake1200 = 0;
                //}
                //if (binLocLake1200 >= ensemble.EnsembleData.NumBins)
                //{
                //    binLocLake1200 = ensemble.EnsembleData.NumBins - 1;
                //}
                int binLocLake1200 = GetLake1200BinLocation(ensemble);

                // Lake 300 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeLake300(ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeLake300(ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeLake300(ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Lake 600 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeLake600(ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeLake600(ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeLake600(ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Lake 1200 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeLake1200(ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeLake1200(ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeLake1200(ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                #endregion

                #region Ocean

                // Ocean
                //int binLocOcean300 = (int)(DepthOcean300 / binSize) - 1;                // Ocean 300 meters. Let it truncate the value  Subtract because start with 0.
                //if (binLocOcean300 < 0)
                //{
                //    binLocOcean300 = 0;
                //}
                //if (binLocOcean300 >= ensemble.EnsembleData.NumBins)
                //{
                //    binLocOcean300 = ensemble.EnsembleData.NumBins - 1;
                //}
                int binLocOcean300 = GetOcean300BinLocation(ensemble);

                //int binLocOcean600 = (int)(DepthOcean600 / binSize) - 1;                // Ocean 600 meters. Let it truncate the value  Subtract because start with 0.
                //if (binLocOcean600 < 0)
                //{
                //    binLocOcean600 = 0;
                //}
                //if (binLocOcean600 >= ensemble.EnsembleData.NumBins)
                //{
                //    binLocOcean600 = ensemble.EnsembleData.NumBins - 1;
                //}
                int binLocOcean600 = GetOcean600BinLocation(ensemble);

                //int binLocOcean1200 = (int)(DepthOcean1200 / binSize) - 1;                // Ocean 1200 meters. Let it truncate the value.  Subtract because start with 0.
                //if (binLocOcean1200 < 0)
                //{
                //    binLocOcean1200 = 0;
                //}
                //if (binLocOcean1200 >= ensemble.EnsembleData.NumBins)
                //{
                //    binLocOcean1200 = ensemble.EnsembleData.NumBins - 1;
                //}
                int binLocOcean1200 = GetOcean1200BinLocation(ensemble);

                // Ocean 300 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeOcean300(ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeOcean300(ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeOcean300(ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Ocean 600 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeOcean600(ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeOcean600(ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeOcean600(ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                // Ocean 1200 Average
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageAmplitudeOcean1200(ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageAmplitudeOcean1200(ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageAmplitudeOcean1200(ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }

                #endregion
            }

            #region Bottom Track Amplitude

            // Bottom Track Amplitude
            if (ensemble.IsEnsembleAvail && ensemble.IsBottomTrackAvail)
            {
                if (ensemble.EnsembleData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                {
                    AverageBottomTrackAmplitude(ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_2_INDEX],
                            ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_3_INDEX]);
                }
                // Noise Average
                // 3 beams to include 7beam systems
                else if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    AverageBottomTrackAmplitude(ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_0_INDEX],
                            ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_1_INDEX],
                            ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_2_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY);
                }
                else
                {
                    // Vertical Beam, so use the same value to get the average
                    AverageBottomTrackAmplitude(ensemble.BottomTrackData.Amplitude[DataSet.Ensemble.BEAM_0_INDEX],
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY,
                            DataSet.Ensemble.BAD_VELOCITY);
                }
            }

            #endregion

        }

        #region Bin Locations

        /// <summary>
        /// Get the Noise floor bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetNoiseFloorLocation(DataSet.Ensemble ensemble)
        {
            if(ensemble == null || !ensemble.IsAncillaryAvail || !ensemble.IsEnsembleAvail )
            {
                return 0;
            }

            // Noise
            int binLocNoise = ensemble.EnsembleData.NumBins - 1;                // Noise. Subtract because start with 0.
            DepthNoise = binLocNoise * ensemble.AncillaryData.BinSize;          // Set the depth of the noise

            return binLocNoise;
        }

        #region Tank

        /// <summary>
        /// Get the Tank Test 1m bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetTank1mBinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocTank1m = (int)(DEFAULT_AMP_DEPTH_1M_TANK / binSize) - 1;                // Tank 1 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocTank1m < 0)
            {
                binLocTank1m = 0;
            }
            if (binLocTank1m >= ensemble.EnsembleData.NumBins)
            {
                binLocTank1m = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocTank1m;
        }

        /// <summary>
        /// Get the Tank Test 300kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetTank300BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocTank300 = (int)(DepthTank300 / binSize) - 1;                // Tank 300 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocTank300 < 0)
            {
                binLocTank300 = 0;
            }
            if (binLocTank300 >= ensemble.EnsembleData.NumBins)
            {
                binLocTank300 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocTank300;
        }

        /// <summary>
        /// Get the Tank Test 600kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetTank600BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocTank600 = (int)(DepthLake600 / binSize) - 1;                // Tank 600 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocTank600 < 0)
            {
                binLocTank600 = 0;
            }
            if (binLocTank600 >= ensemble.EnsembleData.NumBins)
            {
                binLocTank600 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocTank600;
        }

        /// <summary>
        /// Get the Tank Test 1200kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetTank1200BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocTank1200 = (int)(DepthLake1200 / binSize) - 1;                // Tank 1200 meters. Let it truncate the value.  Subtract because start with 0.
            if (binLocTank1200 < 0)
            {
                binLocTank1200 = 0;
            }
            if (binLocTank1200 >= ensemble.EnsembleData.NumBins)
            {
                binLocTank1200 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocTank1200;
        }

        #endregion

        /// <summary>
        /// Get the Lake Test 300kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetLake300BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocLake300 = (int)(DepthLake300 / binSize) - 1;                // Lake 300 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocLake300 < 0)
            {
                binLocLake300 = 0;
            }
            if (binLocLake300 >= ensemble.EnsembleData.NumBins)
            {
                binLocLake300 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocLake300;
        }

        /// <summary>
        /// Get the Lake Test 600kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetLake600BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocLake600 = (int)(DepthLake600 / binSize) - 1;                // Lake 600 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocLake600 < 0)
            {
                binLocLake600 = 0;
            }
            if (binLocLake600 >= ensemble.EnsembleData.NumBins)
            {
                binLocLake600 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocLake600;
        }

        /// <summary>
        /// Get the Lake Test 1200kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetLake1200BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocLake1200 = (int)(DepthLake1200 / binSize) - 1;                // Lake 1200 meters. Let it truncate the value.  Subtract because start with 0.
            if (binLocLake1200 < 0)
            {
                binLocLake1200 = 0;
            }
            if (binLocLake1200 >= ensemble.EnsembleData.NumBins)
            {
                binLocLake1200 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocLake1200;
        }

        /// <summary>
        /// Get the Ocean Test 300kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetOcean300BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocOcean300 = (int)(DepthOcean300 / binSize) - 1;                // Ocean 300 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocOcean300 < 0)
            {
                binLocOcean300 = 0;
            }
            if (binLocOcean300 >= ensemble.EnsembleData.NumBins)
            {
                binLocOcean300 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocOcean300;
        }

        /// <summary>
        /// Get the Ocean Test 600kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetOcean600BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocOcean600 = (int)(DepthOcean600 / binSize) - 1;                // Ocean 600 meters. Let it truncate the value  Subtract because start with 0.
            if (binLocOcean600 < 0)
            {
                binLocOcean600 = 0;
            }
            if (binLocOcean600 >= ensemble.EnsembleData.NumBins)
            {
                binLocOcean600 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocOcean600;
        }

        /// <summary>
        /// Get the Ocean Test 1200kHz bin location.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the bin size and number of bins.</param>
        /// <returns>Bin location.</returns>
        private int GetOcean1200BinLocation(DataSet.Ensemble ensemble)
        {
            // Get the bin for the depth 5m, 10m and 30m
            float binSize = 1;                                                    // Default to 1 meter size bins
            if (ensemble.IsAncillaryAvail)
            {
                binSize = ensemble.AncillaryData.BinSize;
            }

            int binLocOcean1200 = (int)(DepthOcean1200 / binSize) - 1;                // Ocean 1200 meters. Let it truncate the value.  Subtract because start with 0.
            if (binLocOcean1200 < 0)
            {
                binLocOcean1200 = 0;
            }
            if (binLocOcean1200 >= ensemble.EnsembleData.NumBins)
            {
                binLocOcean1200 = ensemble.EnsembleData.NumBins - 1;
            }

            return binLocOcean1200;
        }

        #endregion

        #region Clear

        /// <summary>
        /// Clear the Average Amplitude values.  Because the average
        /// value is a division.  The count will be 0 so when calculating
        /// the average, there must be a check for 0.
        /// </summary>
        private void ClearAverageAmplitude()
        {
            for (int x = 0; x < _numBeams; x++)
            {
                // Tank
                _avgAmpTank1mAccum[x] = 0.0;
                _avgAmpTank1mCount[x] = 0;
                _avgAmpTank300Accum[x] = 0.0;
                _avgAmpTank300Count[x] = 0;
                _avgAmpTank600Accum[x] = 0.0;
                _avgAmpTank600Count[x] = 0;
                _avgAmpTank1200Accum[x] = 0.0;
                _avgAmpTank1200Count[x] = 0;

                // Lake
                _avgAmpLake300Accum[x] = 0.0;
                _avgAmpLake300Count[x] = 0;
                _avgAmpLake600Accum[x] = 0.0;
                _avgAmpLake600Count[x] = 0;
                _avgAmpLake1200Accum[x] = 0.0;
                _avgAmpLake1200Count[x] = 0;

                // Ocean
                _avgAmpOcean300Accum[x] = 0.0;
                _avgAmpOcean300Count[x] = 0;
                _avgAmpOcean600Accum[x] = 0.0;
                _avgAmpOcean600Count[x] = 0;
                _avgAmpOcean1200Accum[x] = 0.0;
                _avgAmpOcean1200Count[x] = 0;

                // Noise
                _avgAmpNoiseAccum[x] = 0.0;
                _avgAmpNoiseCount[x] = 0;

                // Profile Range
                _avgProfileRangeAccum[x] = 0.0;
                _avgProfileRangeCount[x] = 0;

                // Bottom Track Amp
                _avgBtAmpAccum[x] = 0.0;
                _avgBtAmpCount[x] = 0;
            }

            // Tank
            AvgAmpTank300B0 = 0;
            AvgAmpTank300B1 = 0;
            AvgAmpTank300B2 = 0;
            AvgAmpTank300B3 = 0;

            AvgAmpTank600B0 = 0;
            AvgAmpTank600B1 = 0;
            AvgAmpTank600B2 = 0;
            AvgAmpTank600B3 = 0;

            AvgAmpTank1200B0 = 0;
            AvgAmpTank1200B1 = 0;
            AvgAmpTank1200B2 = 0;
            AvgAmpTank1200B3 = 0;

            // Lake
            AvgAmpLake300B0 = 0;
            AvgAmpLake300B1 = 0;
            AvgAmpLake300B2 = 0;
            AvgAmpLake300B3 = 0;

            AvgAmpLake600B0 = 0;
            AvgAmpLake600B1 = 0;
            AvgAmpLake600B2 = 0;
            AvgAmpLake600B3 = 0;

            AvgAmpLake1200B0 = 0;
            AvgAmpLake1200B1 = 0;
            AvgAmpLake1200B2 = 0;
            AvgAmpLake1200B3 = 0;

            // Ocean
            AvgAmpOcean300B0 = 0;
            AvgAmpOcean300B1 = 0;
            AvgAmpOcean300B2 = 0;
            AvgAmpOcean300B3 = 0;

            AvgAmpOcean600B0 = 0;
            AvgAmpOcean600B1 = 0;
            AvgAmpOcean600B2 = 0;
            AvgAmpOcean600B3 = 0;

            AvgAmpOcean1200B0 = 0;
            AvgAmpOcean1200B1 = 0;
            AvgAmpOcean1200B2 = 0;
            AvgAmpOcean1200B3 = 0;

            // Noise
            AvgAmpNoiseB0 = 0;
            AvgAmpNoiseB1 = 0;
            AvgAmpNoiseB2 = 0;
            AvgAmpNoiseB3 = 0;

            // Tank SNR
            AvgAmpTankSnr300B0 = 0;
            AvgAmpTankSnr300B1 = 0;
            AvgAmpTankSnr300B2 = 0;
            AvgAmpTankSnr300B3 = 0;

            AvgAmpTankSnr600B0 = 0;
            AvgAmpTankSnr600B1 = 0;
            AvgAmpTankSnr600B2 = 0;
            AvgAmpTankSnr600B3 = 0;

            AvgAmpTankSnr1200B0 = 0;
            AvgAmpTankSnr1200B1 = 0;
            AvgAmpTankSnr1200B2 = 0;
            AvgAmpTankSnr1200B3 = 0;

            // Lake SNR
            AvgAmpLakeSnr300B0 = 0;
            AvgAmpLakeSnr300B1 = 0;
            AvgAmpLakeSnr300B2 = 0;
            AvgAmpLakeSnr300B3 = 0;

            AvgAmpLakeSnr600B0 = 0;
            AvgAmpLakeSnr600B1 = 0;
            AvgAmpLakeSnr600B2 = 0;
            AvgAmpLakeSnr600B3 = 0;

            AvgAmpLakeSnr1200B0 = 0;
            AvgAmpLakeSnr1200B1 = 0;
            AvgAmpLakeSnr1200B2 = 0;
            AvgAmpLakeSnr1200B3 = 0;

            // Ocean SNR
            AvgAmpOceanSnr300B0 = 0;
            AvgAmpOceanSnr300B1 = 0;
            AvgAmpOceanSnr300B2 = 0;
            AvgAmpOceanSnr300B3 = 0;

            AvgAmpOceanSnr600B0 = 0;
            AvgAmpOceanSnr600B1 = 0;
            AvgAmpOceanSnr600B2 = 0;
            AvgAmpOceanSnr600B3 = 0;

            AvgAmpOceanSnr1200B0 = 0;
            AvgAmpOceanSnr1200B1 = 0;
            AvgAmpOceanSnr1200B2 = 0;
            AvgAmpOceanSnr1200B3 = 0;

            // Profile Range
            ProfileRangeBeam0 = 0;
            ProfileRangeBeam1 = 0;
            ProfileRangeBeam2 = 0;
            ProfileRangeBeam3 = 0;

            // Glitch Check
            GlitchCountB0 = 0;
            GlitchCountB1 = 0;
            GlitchCountB2 = 0;
            GlitchCountB3 = 0;

            // Bottom Track Amplitude
            BtAmpB0 = 0.0;
            BtAmpB1 = 0.0;
            BtAmpB2 = 0.0;
            BtAmpB3 = 0.0;
        }

        #endregion

        #region Average Tank Data

        /// <summary>
        /// Average the amplitude data at the Tank 1m bin.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeTank1m(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpTank1mCount[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpTank1mB0 = _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpTank1mCount[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpTankSnr1mB0 = AvgAmpTank1mB0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpTank1mCount[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpTank1mB1 = _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpTank1mCount[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpTankSnr1mB1 = AvgAmpTank1mB1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpTank1mCount[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpTank1mB2 = _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpTank1mCount[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpTankSnr1mB2 = AvgAmpTank1mB2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpTank1mCount[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpTank1mB3 = _avgAmpTank1mAccum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpTank1mCount[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpTankSnr1mB3 = AvgAmpTank1mB3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Tank 300 kHz bin.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeTank300(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank300Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpTank300Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpTank300B0 = _avgAmpTank300Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpTank300Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpTankSnr300B0 = AvgAmpTank300B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank300Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpTank300Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpTank300B1 = _avgAmpTank300Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpTank300Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpTankSnr300B1 = AvgAmpTank300B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank300Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpTank300Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpTank300B2 = _avgAmpTank300Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpTank300Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpTankSnr300B2 = AvgAmpTank300B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank300Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpTank300Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpTank300B3 = _avgAmpTank300Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpTank300Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpTankSnr300B3 = AvgAmpTank300B3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Tank 600 kHz.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeTank600(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank600Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpTank600Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpTank600B0 = _avgAmpTank600Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpTank600Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpTankSnr600B0 = AvgAmpTank600B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank600Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpTank600Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpTank600B1 = _avgAmpTank600Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpTank600Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpTankSnr600B1 = AvgAmpTank600B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank600Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpTank600Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpTank600B2 = _avgAmpTank600Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpTank600Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpTankSnr600B2 = AvgAmpTank600B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank600Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpTank600Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpTank600B3 = _avgAmpTank600Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpTank600Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpTankSnr600B3 = AvgAmpTank600B3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Tank 1200 kHz.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeTank1200(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpTank1200Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpTank1200B0 = _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpTank1200Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpTankSnr1200B0 = AvgAmpTank1200B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpTank1200Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpTank1200B1 = _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpTank1200Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpTankSnr1200B1 = AvgAmpTank1200B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpTank1200Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpTank1200B2 = _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpTank1200Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpTankSnr1200B2 = AvgAmpTank1200B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpTank1200Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpTank1200B3 = _avgAmpTank1200Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpTank1200Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpTankSnr1200B3 = AvgAmpTank1200B3 - AvgAmpNoiseB3;
            }
        }

        #endregion

        #region Average Lake Data

        /// <summary>
        /// Average the amplitude data at the Lake 300 kHz bin.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeLake300(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake300Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpLake300Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpLake300B0 = _avgAmpLake300Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpLake300Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpLakeSnr300B0 = AvgAmpLake300B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake300Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpLake300Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpLake300B1 = _avgAmpLake300Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpLake300Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpLakeSnr300B1 = AvgAmpLake300B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake300Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpLake300Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpLake300B2 = _avgAmpLake300Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpLake300Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpLakeSnr300B2 = AvgAmpLake300B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake300Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpLake300Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpLake300B3 = _avgAmpLake300Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpLake300Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpLakeSnr300B3 = AvgAmpLake300B3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Lake 600 kHz.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeLake600(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake600Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpLake600Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpLake600B0 = _avgAmpLake600Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpLake600Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpLakeSnr600B0 = AvgAmpLake600B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake600Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpLake600Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpLake600B1 = _avgAmpLake600Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpLake600Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpLakeSnr600B1 = AvgAmpLake600B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake600Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpLake600Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpLake600B2 = _avgAmpLake600Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpLake600Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpLakeSnr600B2 = AvgAmpLake600B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake600Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpLake600Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpLake600B3 = _avgAmpLake600Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpLake600Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpLakeSnr600B3 = AvgAmpLake600B3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Lake 1200 kHz.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeLake1200(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpLake1200Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpLake1200B0 = _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpLake1200Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpLakeSnr1200B0 = AvgAmpLake1200B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpLake1200Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpLake1200B1 = _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpLake1200Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpLakeSnr1200B1 = AvgAmpLake1200B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpLake1200Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpLake1200B2 = _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpLake1200Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpLakeSnr1200B2 = AvgAmpLake1200B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpLake1200Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpLake1200B3 = _avgAmpLake1200Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpLake1200Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpLakeSnr1200B3 = AvgAmpLake1200B3 - AvgAmpNoiseB3;
            }
        }

        #endregion

        #region Average Ocean Data

        /// <summary>
        /// Average the amplitude data at the Ocean 300 kHz bin.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeOcean300(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpOcean300Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpOcean300B0 = _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpOcean300Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpOceanSnr300B0 = AvgAmpOcean300B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpOcean300Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpOcean300B1 = _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpOcean300Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpOceanSnr300B1 = AvgAmpOcean300B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpOcean300Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpOcean300B2 = _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpOcean300Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpOceanSnr300B2 = AvgAmpOcean300B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpOcean300Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpOcean300B3 = _avgAmpOcean300Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpOcean300Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpOceanSnr300B3 = AvgAmpOcean300B3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Ocean 600 kHz.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeOcean600(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpOcean600Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpOcean600B0 = _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpOcean600Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpOceanSnr600B0 = AvgAmpOcean600B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpOcean600Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpOcean600B1 = _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpOcean600Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpOceanSnr600B1 = AvgAmpOcean600B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpOcean600Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpOcean600B2 = _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpOcean600Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpOceanSnr600B2 = AvgAmpOcean600B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpOcean600Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpOcean600B3 = _avgAmpOcean600Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpOcean600Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpOceanSnr600B3 = AvgAmpOcean600B3 - AvgAmpNoiseB3;
            }
        }

        /// <summary>
        /// Average the amplitude data at the Ocean 1200 kHz.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeOcean1200(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpOcean1200B0 = _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_0_INDEX];
                AvgAmpOceanSnr1200B0 = AvgAmpOcean1200B0 - AvgAmpNoiseB0;
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpOcean1200B1 = _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_1_INDEX];
                AvgAmpOceanSnr1200B1 = AvgAmpOcean1200B1 - AvgAmpNoiseB1;
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpOcean1200B2 = _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_2_INDEX];
                AvgAmpOceanSnr1200B2 = AvgAmpOcean1200B2 - AvgAmpNoiseB2;
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpOcean1200B3 = _avgAmpOcean1200Accum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpOcean1200Count[DataSet.Ensemble.BEAM_3_INDEX];
                AvgAmpOceanSnr1200B3 = AvgAmpOcean1200B3 - AvgAmpNoiseB3;
            }
        }

        #endregion

        #region Noise

        /// <summary>
        /// Average the amplitude data at the Noise bin.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageAmplitudeNoise(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgAmpNoiseCount[DataSet.Ensemble.BEAM_0_INDEX]++;

                AvgAmpNoiseB0 = _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_0_INDEX] / _avgAmpNoiseCount[DataSet.Ensemble.BEAM_0_INDEX];
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgAmpNoiseCount[DataSet.Ensemble.BEAM_1_INDEX]++;

                AvgAmpNoiseB1 = _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_1_INDEX] / _avgAmpNoiseCount[DataSet.Ensemble.BEAM_1_INDEX];
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgAmpNoiseCount[DataSet.Ensemble.BEAM_2_INDEX]++;

                AvgAmpNoiseB2 = _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_2_INDEX] / _avgAmpNoiseCount[DataSet.Ensemble.BEAM_2_INDEX];
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgAmpNoiseCount[DataSet.Ensemble.BEAM_3_INDEX]++;

                AvgAmpNoiseB3 = _avgAmpNoiseAccum[DataSet.Ensemble.BEAM_3_INDEX] / _avgAmpNoiseCount[DataSet.Ensemble.BEAM_3_INDEX];
            }
        }

        #endregion

        #region Average Bottom Track Amplitude

        /// <summary>
        /// Average the bottom track amplitude data.
        /// Give the 4 amplitude values for each beam.  If the
        /// value is good, accumulate the data.
        /// </summary>
        /// <param name="b0">Beam 0 amplitude value in dB.</param>
        /// <param name="b1">Beam 1 amplitude value in dB.</param>
        /// <param name="b2">Beam 2 amplitude value in dB.</param>
        /// <param name="b3">Beam 3 amplitude value in dB.</param>
        private void AverageBottomTrackAmplitude(float b0, float b1, float b2, float b3)
        {
            // Beam 0
            if (b0 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgBtAmpAccum[DataSet.Ensemble.BEAM_0_INDEX] += b0;
                _avgBtAmpCount[DataSet.Ensemble.BEAM_0_INDEX]++;

                BtAmpB0 = _avgBtAmpAccum[DataSet.Ensemble.BEAM_0_INDEX] / _avgBtAmpCount[DataSet.Ensemble.BEAM_0_INDEX];
            }
            // Beam 1
            if (b1 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgBtAmpAccum[DataSet.Ensemble.BEAM_1_INDEX] += b1;
                _avgBtAmpCount[DataSet.Ensemble.BEAM_1_INDEX]++;

                BtAmpB1 = _avgBtAmpAccum[DataSet.Ensemble.BEAM_1_INDEX] / _avgBtAmpCount[DataSet.Ensemble.BEAM_1_INDEX];
            }
            // Beam 2
            if (b2 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgBtAmpAccum[DataSet.Ensemble.BEAM_2_INDEX] += b2;
                _avgBtAmpCount[DataSet.Ensemble.BEAM_2_INDEX]++;

                BtAmpB2 = _avgBtAmpAccum[DataSet.Ensemble.BEAM_2_INDEX] / _avgBtAmpCount[DataSet.Ensemble.BEAM_2_INDEX];
            }
            // Beam 3
            if (b3 != DataSet.Ensemble.BAD_VELOCITY)
            {
                _avgBtAmpAccum[DataSet.Ensemble.BEAM_3_INDEX] += b3;
                _avgBtAmpCount[DataSet.Ensemble.BEAM_3_INDEX]++;

                BtAmpB3 = _avgBtAmpAccum[DataSet.Ensemble.BEAM_3_INDEX] / _avgBtAmpCount[DataSet.Ensemble.BEAM_3_INDEX];
            }
        }

        #endregion

        #endregion

        #region Glitch Check

        /// <summary>
        /// Check if any of the beams see a glitch.  A glitch will look like
        /// a spike of the entire amplitude or correlation.
        /// This will look for spikes in values compared against the average value.
        /// </summary>
        /// <param name="ensemble">Ensemble to check.</param>
        private void GlitchCheck(DataSet.Ensemble ensemble)
        {
            if (ensemble != null && ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail && ensemble.IsAmplitudeAvail )
            {

                // Bin locations
                int binLocNoise = GetNoiseFloorLocation(ensemble);
                int binLocLake300 = GetLake300BinLocation(ensemble);
                int binLocLake600 = GetLake600BinLocation(ensemble);
                int binLocLake1200 = GetLake1200BinLocation(ensemble);
                int binLocOcean300 = GetOcean300BinLocation(ensemble);
                int binLocOcean600 = GetOcean600BinLocation(ensemble);
                int binLocOcean1200 = GetOcean1200BinLocation(ensemble);

                float threshold = 15.0f;
                float minGlitch = 5.0f;
                float maxGlitch = 130.0f;
                float step = 0.2f;

                #region Beam 0
                if (ensemble.EnsembleData.NumBeams >= 1)
                {
                    float b0Count = 0;
                    float b0Lake300 = ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpLake300B0 + threshold < b0Lake300 || AvgAmpLake300B0 - threshold > b0Lake300 || b0Lake300 < minGlitch || b0Lake300 > maxGlitch)
                    {
                        b0Count += step;
                    }

                    float b0Lake600 = ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpLake600B0 + threshold < b0Lake600 || AvgAmpLake600B0 - threshold > b0Lake600 || b0Lake600 < minGlitch || b0Lake600 > maxGlitch)
                    {
                        b0Count += step;
                    }

                    float b0Lake1200 = ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpLake1200B0 + threshold < b0Lake1200 || AvgAmpLake1200B0 - threshold > b0Lake1200 || b0Lake1200 < minGlitch || b0Lake1200 > maxGlitch)
                    {
                        b0Count += step;
                    }

                    float b0Ocean300 = ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpOcean300B0 + threshold < b0Ocean300 || AvgAmpOcean300B0 - threshold > b0Ocean300 || b0Ocean300 < minGlitch || b0Ocean300 > maxGlitch)
                    {
                        b0Count += step;
                    }

                    float b0Ocean600 = ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpOcean600B0 + threshold < b0Ocean600 || AvgAmpOcean600B0 - threshold > b0Ocean600 || b0Ocean600 < minGlitch || b0Ocean600 > maxGlitch)
                    {
                        b0Count += step;
                    }

                    float b0Ocean1200 = ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpOcean1200B0 + threshold < b0Ocean1200 || AvgAmpOcean1200B0 - threshold > b0Ocean1200 || b0Ocean1200 < minGlitch || b0Ocean1200 > maxGlitch)
                    {
                        b0Count += step;
                    }

                    float b0Noise = ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_0_INDEX];
                    if (AvgAmpNoiseB0 + threshold < b0Noise || AvgAmpNoiseB0 - threshold > b0Noise || b0Noise < minGlitch || b0Noise > maxGlitch)
                    {
                        b0Count += step;
                    }

                    // Check if a glitch occured
                    if (b0Count >= 1.0f)
                    {
                        GlitchCountB0++;
                        IsGlitchB0 = true;
                    }
                }
                #endregion

                #region Beam 1
                if (ensemble.EnsembleData.NumBeams >= 2)
                {
                    float b1Count = 0;
                    float b1Lake300 = ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpLake300B1 + threshold < b1Lake300 || AvgAmpLake300B1 - threshold > b1Lake300 || b1Lake300 < minGlitch || b1Lake300 > maxGlitch)
                    {
                        b1Count += step;
                    }

                    float b1Lake600 = ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpLake600B1 + threshold < b1Lake600 || AvgAmpLake600B1 - threshold > b1Lake600 || b1Lake600 < minGlitch || b1Lake600 > maxGlitch)
                    {
                        b1Count += step;
                    }

                    float b1Lake1200 = ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpLake1200B1 + threshold < b1Lake1200 || AvgAmpLake1200B1 - threshold > b1Lake1200 || b1Lake1200 < minGlitch || b1Lake1200 > maxGlitch)
                    {
                        b1Count += step;
                    }

                    float b1Ocean300 = ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpOcean300B1 + threshold < b1Ocean300 || AvgAmpOcean300B1 - threshold > b1Ocean300 || b1Ocean300 < minGlitch || b1Ocean300 > maxGlitch)
                    {
                        b1Count += step;
                    }

                    float b1Ocean600 = ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpOcean600B1 + threshold < b1Ocean600 || AvgAmpOcean600B1 - threshold > b1Ocean600 || b1Ocean600 < minGlitch || b1Ocean600 > maxGlitch)
                    {
                        b1Count += step;
                    }

                    float b1Ocean1200 = ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpOcean1200B1 + threshold < b1Ocean1200 || AvgAmpOcean1200B1 - threshold > b1Ocean1200 || b1Ocean1200 < minGlitch || b1Ocean1200 > maxGlitch)
                    {
                        b1Count += step;
                    }

                    float b1Noise = ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_1_INDEX];
                    if (AvgAmpNoiseB1 + threshold < b1Noise || AvgAmpNoiseB1 - threshold > b1Noise || b1Noise < minGlitch || b1Noise > maxGlitch)
                    {
                        b1Count += step;
                    }

                    // Check if a glitch occured
                    if (b1Count >= 1.0f)
                    {
                        GlitchCountB1++;
                        IsGlitchB1 = true;
                    }
                }
                #endregion

                #region Beam 2
                if (ensemble.EnsembleData.NumBeams >= 3)
                {
                    float b2Count = 0;
                    float b2Lake300 = ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpLake300B2 + threshold < b2Lake300 || AvgAmpLake300B2 - threshold > b2Lake300 || b2Lake300 < minGlitch || b2Lake300 > maxGlitch)
                    {
                        b2Count += step;
                    }

                    float b2Lake600 = ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpLake600B2 + threshold < b2Lake600 || AvgAmpLake600B2 - threshold > b2Lake600 || b2Lake600 < minGlitch || b2Lake600 > maxGlitch)
                    {
                        b2Count += step;
                    }

                    float b2Lake1200 = ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpLake1200B2 + threshold < b2Lake1200 || AvgAmpLake1200B2 - threshold > b2Lake1200 || b2Lake1200 < minGlitch || b2Lake1200 > maxGlitch)
                    {
                        b2Count += step;
                    }

                    float b2Ocean300 = ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpOcean300B2 + threshold < b2Ocean300 || AvgAmpOcean300B2 - threshold > b2Ocean300 || b2Ocean300 < minGlitch || b2Ocean300 > maxGlitch)
                    {
                        b2Count += step;
                    }

                    float b2Ocean600 = ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpOcean600B2 + threshold < b2Ocean600 || AvgAmpOcean600B2 - threshold > b2Ocean600 || b2Ocean600 < minGlitch || b2Ocean600 > maxGlitch)
                    {
                        b2Count += step;
                    }

                    float b2Ocean1200 = ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpOcean1200B2 + threshold < b2Ocean1200 || AvgAmpOcean1200B2 - threshold > b2Ocean1200 || b2Ocean1200 < minGlitch || b2Ocean1200 > maxGlitch)
                    {
                        b2Count += step;
                    }

                    float b2Noise = ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_2_INDEX];
                    if (AvgAmpNoiseB2 + threshold < b2Noise || AvgAmpNoiseB2 - threshold > b2Noise || b2Noise < minGlitch || b2Noise > maxGlitch)
                    {
                        b2Count += step;
                    }

                    // Check if a glitch occured
                    if (b2Count >= 1.0f)
                    {
                        GlitchCountB2++;
                        IsGlitchB2 = true;
                    }
                }
                #endregion

                #region Beam 3
                if (ensemble.EnsembleData.NumBeams >= 4)
                {
                    float b3Count = 0;
                    float b3Lake300 = ensemble.AmplitudeData.AmplitudeData[binLocLake300, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpLake300B3 + threshold < b3Lake300 || AvgAmpLake300B3 - threshold > b3Lake300 || b3Lake300 < minGlitch || b3Lake300 > maxGlitch)
                    {
                        b3Count += step;
                    }

                    float b3Lake600 = ensemble.AmplitudeData.AmplitudeData[binLocLake600, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpLake600B3 + threshold < b3Lake600 || AvgAmpLake600B3 - threshold > b3Lake600 || b3Lake600 < minGlitch || b3Lake600 > maxGlitch)
                    {
                        b3Count += step;
                    }

                    float b3Lake1200 = ensemble.AmplitudeData.AmplitudeData[binLocLake1200, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpLake1200B3 + threshold < b3Lake1200 || AvgAmpLake1200B3 - threshold > b3Lake1200 || b3Lake1200 < minGlitch || b3Lake1200 > maxGlitch)
                    {
                        b3Count += step;
                    }

                    float b3Ocean300 = ensemble.AmplitudeData.AmplitudeData[binLocOcean300, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpOcean300B3 + threshold < b3Ocean300 || AvgAmpOcean300B3 - threshold > b3Ocean300 || b3Ocean300 < minGlitch || b3Ocean300 > maxGlitch)
                    {
                        b3Count += step;
                    }

                    float b3Ocean600 = ensemble.AmplitudeData.AmplitudeData[binLocOcean600, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpOcean600B3 + threshold < b3Ocean600 || AvgAmpOcean600B3 - threshold > b3Ocean600 || b3Ocean600 < minGlitch || b3Ocean600 > maxGlitch)
                    {
                        b3Count += step;
                    }

                    float b3Ocean1200 = ensemble.AmplitudeData.AmplitudeData[binLocOcean1200, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpOcean1200B3 + threshold < b3Ocean1200 || AvgAmpOcean1200B3 - threshold > b3Ocean1200 || b3Ocean1200 < minGlitch || b3Ocean1200 > maxGlitch)
                    {
                        b3Count += step;
                    }

                    float b3Noise = ensemble.AmplitudeData.AmplitudeData[binLocNoise, DataSet.Ensemble.BEAM_3_INDEX];
                    if (AvgAmpNoiseB3 + threshold < b3Noise || AvgAmpNoiseB3 - threshold > b3Noise || b3Noise < minGlitch || b3Noise > maxGlitch)
                    {
                        b3Count += step;
                    }

                    // Check if a glitch occured
                    if (b3Count >= 1.0f)
                    {
                        GlitchCountB3++;
                        IsGlitchB3 = true;
                    }
                }
                #endregion
            }
        }

        #endregion

        #region Profile Range

        /// <summary>
        /// Profile range. Look for the 25% in correlaton.
        /// </summary>
        /// <param name="ensemble">Ensemble data.</param>
        private void ProfileRange(DataSet.Ensemble ensemble)
        {
            // Correlation value for max range
            float MAX_CORR_PROFILE_RANGE = 0.25f;

            // Flag when to stop looking
            bool beam0Found = false;
            bool beam1Found = false;
            bool beam2Found = false;
            bool beam3Found = false;

            if (ensemble.IsCorrelationAvail && ensemble.IsAncillaryAvail)
            {
                // 1 Beam system
                if (ensemble.EnsembleData.NumBeams == 1)
                {
                    // Find the profile range for each beam
                    // Start with the second bin
                    // Beam 0
                    for (int x = 1; x < ensemble.CorrelationData.CorrelationData.GetLength(0); x++)
                    {
                        // Beam 0
                        if (ensemble.CorrelationData.CorrelationData[x, DataSet.Ensemble.BEAM_0_INDEX] < MAX_CORR_PROFILE_RANGE && !beam0Found)
                        {
                            // Calculate the range
                            // Then store to accumulate and display the average
                            _avgProfileRangeAccum[DataSet.Ensemble.BEAM_0_INDEX] += (x * ensemble.AncillaryData.BinSize) + ensemble.AncillaryData.FirstBinRange;
                            _avgProfileRangeCount[DataSet.Ensemble.BEAM_0_INDEX]++;
                            ProfileRangeBeam0 = _avgProfileRangeAccum[DataSet.Ensemble.BEAM_0_INDEX] / _avgProfileRangeCount[DataSet.Ensemble.BEAM_0_INDEX];
                            beam0Found = true;
                        }
                    }
                }
                else
                {
                    // Find the profile range for each beam
                    // Start with the second bin
                    // Beam 0
                    for (int x = 1; x < ensemble.CorrelationData.CorrelationData.GetLength(0); x++)
                    {
                        // Beam 0
                        if (ensemble.CorrelationData.CorrelationData.GetLength(1) > 0 && ensemble.CorrelationData.CorrelationData[x, DataSet.Ensemble.BEAM_0_INDEX] < MAX_CORR_PROFILE_RANGE && !beam0Found)
                        {
                            // Calculate the range
                            // Then store to accumulate and display the average
                            _avgProfileRangeAccum[DataSet.Ensemble.BEAM_0_INDEX] += (x * ensemble.AncillaryData.BinSize) + ensemble.AncillaryData.FirstBinRange;
                            _avgProfileRangeCount[DataSet.Ensemble.BEAM_0_INDEX]++;
                            ProfileRangeBeam0 = _avgProfileRangeAccum[DataSet.Ensemble.BEAM_0_INDEX] / _avgProfileRangeCount[DataSet.Ensemble.BEAM_0_INDEX];
                            beam0Found = true;
                        }

                        // Beam 1
                        if (ensemble.CorrelationData.CorrelationData.GetLength(1) > 1 && ensemble.CorrelationData.CorrelationData[x, DataSet.Ensemble.BEAM_1_INDEX] < MAX_CORR_PROFILE_RANGE && !beam1Found)
                        {
                            // Calculate the range
                            // Then store to accumulate and display the average
                            _avgProfileRangeAccum[DataSet.Ensemble.BEAM_1_INDEX] += (x * ensemble.AncillaryData.BinSize) + ensemble.AncillaryData.FirstBinRange;
                            _avgProfileRangeCount[DataSet.Ensemble.BEAM_1_INDEX]++;
                            ProfileRangeBeam1 = _avgProfileRangeAccum[DataSet.Ensemble.BEAM_1_INDEX] / _avgProfileRangeCount[DataSet.Ensemble.BEAM_1_INDEX];
                            beam1Found = true;
                        }

                        // Beam 2
                        if (ensemble.CorrelationData.CorrelationData.GetLength(1) > 2 && ensemble.CorrelationData.CorrelationData[x, DataSet.Ensemble.BEAM_2_INDEX] < MAX_CORR_PROFILE_RANGE && !beam2Found)
                        {
                            // Calculate the range
                            // Then store to accumulate and display the average
                            _avgProfileRangeAccum[DataSet.Ensemble.BEAM_2_INDEX] += (x * ensemble.AncillaryData.BinSize) + ensemble.AncillaryData.FirstBinRange;
                            _avgProfileRangeCount[DataSet.Ensemble.BEAM_2_INDEX]++;
                            ProfileRangeBeam2 = _avgProfileRangeAccum[DataSet.Ensemble.BEAM_2_INDEX] / _avgProfileRangeCount[DataSet.Ensemble.BEAM_2_INDEX];
                            beam2Found = true;
                        }

                        // Beam 3
                        if (ensemble.CorrelationData.CorrelationData.GetLength(1) > 3 && ensemble.CorrelationData.CorrelationData[x, DataSet.Ensemble.BEAM_3_INDEX] < MAX_CORR_PROFILE_RANGE && !beam3Found)
                        {
                            // Calculate the range
                            // Then store to accumulate and display the average
                            _avgProfileRangeAccum[DataSet.Ensemble.BEAM_3_INDEX] += (x * ensemble.AncillaryData.BinSize) + ensemble.AncillaryData.FirstBinRange;
                            _avgProfileRangeCount[DataSet.Ensemble.BEAM_3_INDEX]++;
                            ProfileRangeBeam3 = _avgProfileRangeAccum[DataSet.Ensemble.BEAM_3_INDEX] / _avgProfileRangeCount[DataSet.Ensemble.BEAM_3_INDEX];
                            beam3Found = true;
                        }
                    }
                }
            }

        }

        #endregion

        #region Report Info

        /// <summary>
        /// Set the ensemble information.
        /// </summary>
        /// <param name="ensemble">Ensemble to get information.</param>
        private void SetReportInfo(DataSet.Ensemble ensemble)
        {
            // Set the ensemble data
            if (ensemble.IsEnsembleAvail)
            {
                // Set number of bins
                NumBins = ensemble.EnsembleData.NumBins;

                // Set Date and Time
                _dateAndTime = ensemble.EnsembleData.EnsDateTime;
                this.NotifyOfPropertyChange(() => this.DateAndTime);
            }

            if(ensemble.IsAncillaryAvail)
            {
                // Bin Size
                BinSize = ensemble.AncillaryData.BinSize;
            }
        }

        #endregion

        #region Test Orientation

        /// <summary>
        /// This will determine the test orientation.  It looks at which bottom beam velocity
        /// is above 0.2 m/s.  The oppositie should be the inverse or close to it.  It will just verify
        /// the opposite is less than 0.  Which ever beam is greater then 0.2 is considered the beam that
        /// is forward.  This may jump around in bad test conditions or if the boat is moving to slow.
        /// </summary>
        /// <param name="adcpData">Ensemble data.</param>
        private void SetTestOrientation(DataSet.Ensemble adcpData)
        {
            // No way to determine so return and set default
            if(!adcpData.IsBottomTrackAvail)
            {
                TestOrientation = AdcpTestOrientation.BEAM_0_FORWARD;
                return;
            }

            if (adcpData.BottomTrackData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
            {
                float b0 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_0_INDEX];
                float b1 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_1_INDEX];
                float b2 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_2_INDEX];
                float b3 = adcpData.BottomTrackData.BeamVelocity[DataSet.Ensemble.BEAM_3_INDEX];

                // Make sure they are not bad velocity
                if (b0 != DataSet.Ensemble.BAD_VELOCITY && b1 != DataSet.Ensemble.BAD_VELOCITY && b2 != DataSet.Ensemble.BAD_VELOCITY && b3 != DataSet.Ensemble.BAD_VELOCITY)
                {
                    // Beam 0 is forward
                    if (b0 > 0.2 && b1 < 0)
                    {
                        TestOrientation = AdcpTestOrientation.BEAM_0_FORWARD;
                    }
                    else if (b1 > 0.2 && b0 < 0)
                    {
                        TestOrientation = AdcpTestOrientation.BEAM_1_FORWARD;
                    }
                    else if (b2 > 0.2 && b3 < 0)
                    {
                        TestOrientation = AdcpTestOrientation.BEAM_2_FORWARD;
                    }
                    else if (b3 > 0.2 && b2 < 0)
                    {
                        TestOrientation = AdcpTestOrientation.BEAM_3_FORWARD;
                    }
                    else
                    {
                        TestOrientation = AdcpTestOrientation.BEAM_0_FORWARD;
                    }
                }
            }
            // Vertical Beam
            else if (adcpData.BottomTrackData.NumBeams == 1)
            {
                TestOrientation = AdcpTestOrientation.VERTICAL_BEAM;
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// Return a string representing the
        /// status.
        /// </summary>
        /// <returns>Status value as a string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Date and Time: {0}", _dateAndTime.ToString()));
            sb.AppendLine(string.Format("Num Ensembles: {0}", _numEnsembles.ToString()));
            sb.AppendLine(string.Format("Num Bins: {0}", _numBins.ToString()));
            sb.AppendLine(string.Format("Bin Size: {0}", _BinSize.ToString()));
            sb.AppendLine(string.Format("Test Orientation: {0}", _TestOrientation.ToString()));
            sb.AppendLine("Noise");
            sb.AppendLine(string.Format("Ocean 300kHz Noise Depth: {0}", _depthNoise));
            sb.AppendLine(string.Format("Ocean 300kHz Noise: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpNoiseB0.ToString("0.000"), _avgAmpNoiseB1.ToString("0.000"), _avgAmpNoiseB2.ToString("0.000"), _avgAmpNoiseB3.ToString("0.000")));
            sb.AppendLine("Tank");
            sb.AppendLine(string.Format("Tank 1m Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTank1mB0.ToString("0.000"), _avgAmpTank1mB1.ToString("0.000"), _avgAmpTank1mB2.ToString("0.000"), _avgAmpTank1mB3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 1m SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTankSnr1mB0.ToString("0.000"), _avgAmpTankSnr1mB1.ToString("0.000"), _avgAmpTankSnr1mB2.ToString("0.000"), _avgAmpTankSnr1mB3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 300kHz Signal Depth: {0}", _depthTank300));
            sb.AppendLine(string.Format("Tank 300kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTank300B0.ToString("0.000"), _avgAmpTank300B1.ToString("0.000"), _avgAmpTank300B2.ToString("0.000"), _avgAmpTank300B3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 300kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTankSnr300B0.ToString("0.000"), _avgAmpTankSnr300B1.ToString("0.000"), _avgAmpTankSnr300B2.ToString("0.000"), _avgAmpTankSnr300B3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 600kHz Depth: {0}", _depthTank600));
            sb.AppendLine(string.Format("Tank 600kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTank600B0.ToString("0.000"), _avgAmpTank600B1.ToString("0.000"), _avgAmpTank600B2.ToString("0.000"), _avgAmpTank600B3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 600kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTankSnr600B0.ToString("0.000"), _avgAmpTankSnr600B1.ToString("0.000"), _avgAmpTankSnr600B2.ToString("0.000"), _avgAmpTankSnr600B3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 1200kHz Depth: {0}", _depthTank1200));
            sb.AppendLine(string.Format("Tank 1200kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTank1200B0.ToString("0.000"), _avgAmpTank1200B1.ToString("0.000"), _avgAmpTank1200B2.ToString("0.000"), _avgAmpTank1200B3.ToString("0.000")));
            sb.AppendLine(string.Format("Tank 1200kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpTankSnr1200B0.ToString("0.000"), _avgAmpTankSnr1200B1.ToString("0.000"), _avgAmpTankSnr1200B2.ToString("0.000"), _avgAmpTankSnr1200B3.ToString("0.000")));
            sb.AppendLine("Lake");
            sb.AppendLine(string.Format("Lake 300kHz Signal Depth: {0}", _depthLake300));
            sb.AppendLine(string.Format("Lake 300kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpLake300B0.ToString("0.000"), _avgAmpLake300B1.ToString("0.000"), _avgAmpLake300B2.ToString("0.000"), _avgAmpLake300B3.ToString("0.000")));
            sb.AppendLine(string.Format("Lake 300kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpLakeSnr300B0.ToString("0.000"), _avgAmpLakeSnr300B1.ToString("0.000"), _avgAmpLakeSnr300B2.ToString("0.000"), _avgAmpLakeSnr300B3.ToString("0.000")));
            sb.AppendLine(string.Format("Lake 600kHz Depth: {0}", _depthLake600));
            sb.AppendLine(string.Format("Lake 600kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpLake600B0.ToString("0.000"), _avgAmpLake600B1.ToString("0.000"), _avgAmpLake600B2.ToString("0.000"), _avgAmpLake600B3.ToString("0.000")));
            sb.AppendLine(string.Format("Lake 600kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpLakeSnr600B0.ToString("0.000"), _avgAmpLakeSnr600B1.ToString("0.000"), _avgAmpLakeSnr600B2.ToString("0.000"), _avgAmpLakeSnr600B3.ToString("0.000")));
            sb.AppendLine(string.Format("Lake 1200kHz Depth: {0}", _depthLake1200));
            sb.AppendLine(string.Format("Lake 1200kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpLake1200B0.ToString("0.000"), _avgAmpLake1200B1.ToString("0.000"), _avgAmpLake1200B2.ToString("0.000"), _avgAmpLake1200B3.ToString("0.000")));
            sb.AppendLine(string.Format("Lake 1200kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpLakeSnr1200B0.ToString("0.000"), _avgAmpLakeSnr1200B1.ToString("0.000"), _avgAmpLakeSnr1200B2.ToString("0.000"), _avgAmpLakeSnr1200B3.ToString("0.000")));
            sb.AppendLine("Ocean");
            sb.AppendLine(string.Format("Ocean 300kHz Signal Depth: {0}", _depthOcean300));
            sb.AppendLine(string.Format("Ocean 300kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpOcean300B0.ToString("0.000"), _avgAmpOcean300B1.ToString("0.000"), _avgAmpOcean300B2.ToString("0.000"), _avgAmpOcean300B3.ToString("0.000")));
            sb.AppendLine(string.Format("Ocean 300kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpOceanSnr300B0.ToString("0.000"), _avgAmpOceanSnr300B1.ToString("0.000"), _avgAmpOceanSnr300B2.ToString("0.000"), _avgAmpOceanSnr300B3.ToString("0.000")));
            sb.AppendLine(string.Format("Ocean 600kHz Depth: {0}", _depthOcean600));
            sb.AppendLine(string.Format("Ocean 600kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpOcean600B0.ToString("0.000"), _avgAmpOcean600B1.ToString("0.000"), _avgAmpOcean600B2.ToString("0.000"), _avgAmpOcean600B3.ToString("0.000")));
            sb.AppendLine(string.Format("Ocean 600kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpOceanSnr600B0.ToString("0.000"), _avgAmpOceanSnr600B1.ToString("0.000"), _avgAmpOceanSnr600B2.ToString("0.000"), _avgAmpOceanSnr600B3.ToString("0.000")));
            sb.AppendLine(string.Format("Ocean 1200kHz Depth: {0}", _depthOcean1200));
            sb.AppendLine(string.Format("Ocean 1200kHz Signal: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpOcean1200B0.ToString("0.000"), _avgAmpOcean1200B1.ToString("0.000"), _avgAmpOcean1200B2.ToString("0.000"), _avgAmpOcean1200B3.ToString("0.000")));
            sb.AppendLine(string.Format("Ocean 1200kHz SNR: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _avgAmpOceanSnr1200B0.ToString("0.000"), _avgAmpOceanSnr1200B1.ToString("0.000"), _avgAmpOceanSnr1200B2.ToString("0.000"), _avgAmpOceanSnr1200B3.ToString("0.000")));
            sb.AppendLine("Glitch");
            sb.AppendLine(string.Format("Glitch Count: {0} | {1} | {2} | {3}", _GlitchCountB0.ToString("0"), _GlitchCountB1.ToString("0"), _GlitchCountB2.ToString("0"), _GlitchCountB3.ToString("0")));
            sb.AppendLine("BT Amplitude");
            sb.AppendLine(string.Format("Bottom Track Amplitude: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _BtAmpB0.ToString("0.000"), _BtAmpB1.ToString("0.000"), _BtAmpB2.ToString("0.000"), _BtAmpB3.ToString("0.000")));
            sb.AppendLine("Profile Range");
            sb.AppendLine(string.Format("Profile Range: {0,7:###.000} | {1,7:###.000} | {2,7:###.000} | {3,7:###.000}", _ProfileRangeBeam0.ToString("0.000"), _ProfileRangeBeam1.ToString("0.000"), _ProfileRangeBeam2.ToString("0.000"), _ProfileRangeBeam3.ToString("0.000")));

            sb.AppendLine(_distanceTraveled.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// Hashcode for the object.
        /// This will return the hashcode for the
        /// this object's string.
        /// </summary>
        /// <returns>Hashcode for the object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        ///// <summary>
        ///// Determine if the given object is equal to this
        ///// object.  This will check if the Status Value match.
        ///// </summary>
        ///// <param name="obj">Object to compare with this object.</param>
        ///// <returns>TRUE = Status Value matched.</returns>
        //public override bool Equals(object obj)
        //{
        //    //Check for null and compare run-time types.
        //    if (obj == null || GetType() != obj.GetType()) return false;

        //    AdcpStatus p = (AdcpStatus)obj;

        //    return Status == p.Status;
        //}

        ///// <summary>
        ///// Determine if the two AdcpStatus Value given are the equal.
        ///// </summary>
        ///// <param name="stat1">First AdcpStatus to check.</param>
        ///// <param name="stat2">AdcpStatus to check against.</param>
        ///// <returns>True if there strings match.</returns>
        //public static bool operator ==(AdcpStatus stat1, AdcpStatus stat2)
        //{
        //    // If both are null, or both are same instance, return true.
        //    if (System.Object.ReferenceEquals(stat1, stat2))
        //    {
        //        return true;
        //    }

        //    // If one is null, but not both, return false.
        //    if (((object)stat1 == null) || ((object)stat2 == null))
        //    {
        //        return false;
        //    }

        //    // Return true if the fields match:
        //    return stat1.Status == stat2.Status;
        //}

        ///// <summary>
        ///// Return the opposite of ==.
        ///// </summary>
        ///// <param name="stat1">First AdcpStatus to check.</param>
        ///// <param name="stat2">AdcpStatus to check against.</param>
        ///// <returns>Return the opposite of ==.</returns>
        //public static bool operator !=(AdcpStatus stat1, AdcpStatus stat2)
        //{
        //    return !(stat1 == stat2);
        //}

        #endregion
    }
}
