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
 * 10/31/2013      RC          3.2.0      Initial coding
 * 07/28/2014      RC          3.4.0      Added IsRecording.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0
 * 08/20/2014      RC          4.0.1      Added CloseVMCommand.
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using ReactiveUI;
    using System.Threading.Tasks;

    /// <summary>
    /// Setup the Averging options for the selected project.
    /// </summary>
    public class AveragingViewModel : PulseViewModel
    {

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Default

        /// <summary>
        /// Running Average.
        /// </summary>
        public const string RUNNING_AVG = "Running Average";
        
        /// <summary>
        /// Time average.
        /// </summary>
        public const string TIME_AVG = "Time";

        /// <summary>
        /// Sample averaging.
        /// </summary>
        public const string SAMPLE_AVG = "Samples";

        #endregion

        /// <summary>
        /// Event aggregator.
        /// </summary>
        private readonly IEventAggregator _events;

        /// <summary>
        /// ADCP connection.
        /// </summary>
        private AdcpConnection _adcpConn;

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        /// <summary>
        /// Long Term average manager.
        /// </summary>
        private AverageManager _ltaAvgMgr;

        /// <summary>
        /// Short Term average manager.
        /// </summary>
        private AverageManager _staAvgMgr;

        /// <summary>
        /// Options to store for averaging the data.
        /// </summary>
        private AverageSubsystemConfigOptions _Options;

        /// <summary>
        /// Record the STA data to a file.
        /// </summary>
        private AdcpBinaryWriter _staWriter;

        /// <summary>
        /// Record the LTA data to a file.
        /// </summary>
        private AdcpBinaryWriter _ltaWriter;

        #endregion

        #region Properties

        #region Configuration

        /// <summary>
        /// Subsystem Configuration for this view.
        /// </summary>
        private SubsystemDataConfig _Config;
        /// <summary>
        /// Subsystem Configuration for this view.
        /// </summary>
        public SubsystemDataConfig Config
        {
            get { return _Config; }
            set
            {
                _Config = value;
                this.NotifyOfPropertyChange(() => this.Config);
            }
        }

        #endregion

        #region Display

        /// <summary>
        /// Display the configuration CEPO index to indentify the 
        /// configuration.
        /// </summary>
        public string Display { get { return _Config.IndexCodeString(); } }

        /// <summary>
        /// Display the description CEPO index to indentify the 
        /// configuration.
        /// </summary>
        public string Desc { get { return _Config.DescString(); } }

        /// <summary>
        /// Flag if this view will display playback or live data.
        /// TRUE = Playback Data
        /// </summary>
        public bool IsPlayback
        {
            get
            {
                if (_Config.Source == EnsembleSource.Playback)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Flag if the data came from the serial port.
        /// </summary>
        public bool IsSerial
        {
            get
            {
                if (_Config.Source == EnsembleSource.Serial)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Flag if the data came from the Long Term Average.
        /// </summary>
        public bool IsLta
        {
            get
            {
                if (_Config.Source == EnsembleSource.LTA)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Flag if the data came from the Short Term Average.
        /// </summary>
        public bool IsSta
        {
            get
            {
                if (_Config.Source == EnsembleSource.STA)
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region List

        /// <summary>
        /// The average types.
        /// </summary>
        public List<string> AverageType { get; protected set; }

        #endregion

        #region LTA Average Manager Options

        #region Averaging Type

        /// <summary>
        /// Turn on or off LTA averaging.
        /// </summary>
        public bool IsLtaEnabled
        {
            get
            {
                return _Options.IsLtaEnabled;
            }
            set
            {
                _Options.IsLtaEnabled = value;
                this.NotifyOfPropertyChange(() => this.IsLtaEnabled);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Selected Average type.
        /// </summary>
        private string _SelectedLtaAverageType;
        /// <summary>
        /// Selected Average type.
        /// </summary>
        public string SelectedLtaAverageType
        {
            get
            {
                return _SelectedLtaAverageType;
            }
            set
            {
                _SelectedLtaAverageType = value;
                this.NotifyOfPropertyChange(() => this.SelectedLtaAverageType);

                // Check which is selected
                switch (value)
                {
                    case RUNNING_AVG:
                        IsStaAvgRunning = true;
                        break;
                    case TIME_AVG:
                        IsStaByTimer = true;
                        break;
                    case SAMPLE_AVG:
                        IsStaByNumSamples = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Set flag if LTA should be based off the timer.
        /// </summary>
        public bool IsLtaByTimer
        {
            get { return _Options.LtaAvgMgrOptions.IsAvgByTimer; }
            set
            {
                _Options.LtaAvgMgrOptions.IsAvgByTimer = value;
                _Options.LtaAvgMgrOptions.IsAvgByNumSamples = !value;
                _Options.LtaAvgMgrOptions.IsAvgRunning = !value;

                _ltaAvgMgr.IsAvgByTimer = value;

                this.NotifyOfPropertyChange(() => this.IsLtaByNumSamples);
                this.NotifyOfPropertyChange(() => this.IsLtaByTimer);
                this.NotifyOfPropertyChange(() => this.LtaIsAvgRunning);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Set flag if LTA should be based off the number of samples.
        /// </summary>
        public bool IsLtaByNumSamples
        {
            get { return _Options.LtaAvgMgrOptions.IsAvgByNumSamples; }
            set
            {
                _Options.LtaAvgMgrOptions.IsAvgByTimer = !value;
                _Options.LtaAvgMgrOptions.IsAvgByNumSamples = value;
                _Options.LtaAvgMgrOptions.IsAvgRunning = !value;

                _ltaAvgMgr.IsAvgByNumSamples = value;
                
                this.NotifyOfPropertyChange(() => this.IsLtaByNumSamples);
                this.NotifyOfPropertyChange(() => this.IsLtaByTimer);
                this.NotifyOfPropertyChange(() => this.LtaIsAvgRunning);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Flag if the long term averaging 
        /// is a running average.  A running average
        /// is removing the last ensemble from the
        /// list.  
        /// </summary>
        public bool LtaIsAvgRunning
        {
            get
            {
                return _ltaAvgMgr.IsAvgRunning;
            }
            set
            {
                _Options.LtaAvgMgrOptions.IsAvgByTimer = !value;
                _Options.LtaAvgMgrOptions.IsAvgByNumSamples = !value;
                _Options.LtaAvgMgrOptions.IsAvgRunning = value;

                _ltaAvgMgr.IsAvgRunning = value;

                this.NotifyOfPropertyChange(() => this.IsLtaByNumSamples);
                this.NotifyOfPropertyChange(() => this.IsLtaByTimer);
                this.NotifyOfPropertyChange(() => this.LtaIsAvgRunning);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// The number of milliseconds between averages.
        /// </summary>
        public uint LtaAvgTimerMilliseconds
        {
            get { return _Options.LtaAvgMgrOptions.TimerMilliseconds; }
            set
            {
                _Options.LtaAvgMgrOptions.TimerMilliseconds = value;
                this.NotifyOfPropertyChange(() => this.LtaAvgTimerMilliseconds);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Number of samples to average together for
        /// the long term average..
        /// </summary>
        public int LtaNumSamples
        {
            get
            {
                return _ltaAvgMgr.NumSamples;
            }
            set
            {
                _ltaAvgMgr.NumSamples = value;
                _Options.LtaAvgMgrOptions.NumSamples = value;
                this.NotifyOfPropertyChange(() => this.LtaNumSamples);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Flag if the long term averaging 
        /// is a running average.  A running average
        /// is removing the last ensemble from the
        /// list.  
        /// </summary>
        public bool LtaIsSampleRunningAvg
        {
            get
            {
                return _ltaAvgMgr.IsSampleRunningAverage;
            }
            set
            {
                _ltaAvgMgr.IsSampleRunningAverage = value;
                _Options.LtaAvgMgrOptions.IsSampleRunningAverage = value;
                this.NotifyOfPropertyChange(() => this.LtaIsSampleRunningAvg);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Recording

        /// <summary>
        /// Turn on or off LTA recording average.
        /// </summary>
        public bool IsLtaRecording
        {
            get
            {
                return _Options.LtaAvgMgrOptions.IsRecording;
            }
            set
            {
                _Options.LtaAvgMgrOptions.IsRecording = value;
                this.NotifyOfPropertyChange(() => this.IsLtaRecording);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Reference Layer Avg

        /// <summary>
        /// Flag if reference layer averaging.
        /// </summary>
        public bool LtaIsReferenceLayerAveraging
        {
            get
            {
                return _ltaAvgMgr.IsReferenceLayerAveraging;
            }
            set
            {
                _ltaAvgMgr.IsReferenceLayerAveraging = value;
                _Options.LtaAvgMgrOptions.IsReferenceLayerAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsReferenceLayerAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long Term Average Minimum reference layer.
        /// </summary>
        public uint LtaMinRefLayer
        {
            get
            {
                return _ltaAvgMgr.MinRefLayer;
            }
            set
            {
                _ltaAvgMgr.MinRefLayer = value;
                _Options.LtaAvgMgrOptions.MinRefLayer = value;
                this.NotifyOfPropertyChange(() => this.LtaMinRefLayer);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long Term Average Maximum reference layer.
        /// </summary>
        public uint LtaMaxRefLayer
        {
            get
            {
                return _ltaAvgMgr.MaxRefLayer;
            }
            set
            {
                _ltaAvgMgr.MaxRefLayer = value;
                _Options.LtaAvgMgrOptions.MaxRefLayer = value;
                this.NotifyOfPropertyChange(() => this.LtaMaxRefLayer);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Correlation

        /// <summary>
        /// Long Term Averaging Is correlation averaging flag.
        /// </summary>
        public bool LtaIsCorrelationAveraging
        {
            get
            {
                return _ltaAvgMgr.IsCorrelationAveraging;
            }
            set
            {
                _ltaAvgMgr.IsCorrelationAveraging = value;
                _Options.LtaAvgMgrOptions.IsCorrelationAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsCorrelationAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average correlation scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaCorrelationScale
        {
            get
            {
                return _ltaAvgMgr.CorrelationScale;
            }
            set
            {
                _ltaAvgMgr.CorrelationScale = value;
                _Options.LtaAvgMgrOptions.CorrelationScale = value;
                this.NotifyOfPropertyChange(() => this.LtaCorrelationScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Amplitude

        /// <summary>
        /// Flag if Long Term Average is averaing the Amplitude data.
        /// </summary>
        public bool LtaIsAmplitudeAveraging
        {
            get
            {
                return _ltaAvgMgr.IsAmplitudeAveraging;
            }
            set
            {
                _ltaAvgMgr.IsAmplitudeAveraging = value;
                _Options.LtaAvgMgrOptions.IsAmplitudeAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsAmplitudeAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average amplitude scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaAmplitudeScale
        {
            get
            {
                return _ltaAvgMgr.AmplitudeScale;
            }
            set
            {
                _ltaAvgMgr.AmplitudeScale = value;
                _Options.LtaAvgMgrOptions.AmplitudeScale = value;
                this.NotifyOfPropertyChange(() => this.LtaAmplitudeScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Beam Velocity

        /// <summary>
        /// Flag if Long Term Average is averaing the Beam Velocity data.
        /// </summary>
        public bool LtaIsBeamVelocityAveraging
        {
            get
            {
                return _ltaAvgMgr.IsBeamVelocityAveraging;
            }
            set
            {
                _ltaAvgMgr.IsBeamVelocityAveraging = value;
                _Options.LtaAvgMgrOptions.IsBeamVelocityAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsBeamVelocityAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Beam Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBeamVelocityScale
        {
            get
            {
                return _ltaAvgMgr.BeamVelocityScale;
            }
            set
            {
                _ltaAvgMgr.BeamVelocityScale = value;
                _Options.LtaAvgMgrOptions.BeamVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBeamVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Instrument Velocity

        /// <summary>
        /// Flag if Long Term Average is averaing the Instrument Velocity data.
        /// </summary>
        public bool LtaIsInstrumentVelocityAveraging
        {
            get
            {
                return _ltaAvgMgr.IsInstrumentVelocityAveraging;
            }
            set
            {
                _ltaAvgMgr.IsInstrumentVelocityAveraging = value;
                _Options.LtaAvgMgrOptions.IsInstrumentVelocityAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsInstrumentVelocityAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Instrument Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaInstrumentVelocityScale
        {
            get
            {
                return _ltaAvgMgr.InstrumentVelocityScale;
            }
            set
            {
                _ltaAvgMgr.InstrumentVelocityScale = value;
                _Options.LtaAvgMgrOptions.InstrumentVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.LtaInstrumentVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Earth Velocity

        /// <summary>
        /// Flag if Long Term Average is averaing the Earth Velocity data.
        /// </summary>
        public bool LtaIsEarthVelocityAveraging
        {
            get
            {
                return _ltaAvgMgr.IsEarthVelocityAveraging;
            }
            set
            {
                _ltaAvgMgr.IsEarthVelocityAveraging = value;
                _Options.LtaAvgMgrOptions.IsEarthVelocityAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsEarthVelocityAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Earth Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaEarthVelocityScale
        {
            get
            {
                return _ltaAvgMgr.EarthVelocityScale;
            }
            set
            {
                _ltaAvgMgr.EarthVelocityScale = value;
                _Options.LtaAvgMgrOptions.EarthVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.LtaEarthVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Bottom Track

        /// <summary>
        /// Flag if Long Term Average is averaing the Bottom Track data.
        /// </summary>
        public bool LtaIsBottomTrackAveraging
        {
            get
            {
                return _ltaAvgMgr.IsBottomTrackAveraging;
            }
            set
            {
                _ltaAvgMgr.IsBottomTrackAveraging = value;
                _Options.LtaAvgMgrOptions.IsBottomTrackAveraging = value;
                this.NotifyOfPropertyChange(() => this.LtaIsBottomTrackAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Bottom Track Range scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackRangeScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackRangeScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackRangeScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackRangeScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackRangeScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Bottom Track SNR scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackSnrScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackSnrScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackSnrScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackSnrScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackSnrScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Bottom Track Amplitude scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackAmplitudeScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackAmplitudeScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackAmplitudeScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackAmplitudeScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackAmplitudeScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Bottom Track Correlation scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackCorrelationScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackCorrelationScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackCorrelationScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackCorrelationScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackCorrelationScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Bottom Track Beam Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackBeamVelocityScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackBeamVelocityScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackBeamVelocityScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackBeamVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackBeamVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Long term average Bottom Track Instrument Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackInstrumentVelocityScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackInstrumentVelocityScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackInstrumentVelocityScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackInstrumentVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackInstrumentVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Earth Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float LtaBottomTrackEarthVelocityScale
        {
            get
            {
                return _ltaAvgMgr.BottomTrackEarthVelocityScale;
            }
            set
            {
                _ltaAvgMgr.BottomTrackEarthVelocityScale = value;
                _Options.LtaAvgMgrOptions.BottomTrackEarthVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.LtaBottomTrackEarthVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #endregion

        #region STA Average Manager Options

        #region Average Type

        /// <summary>
        /// Turn on or off STA averaging.
        /// </summary>
        public bool IsStaEnabled
        {
            get
            {
                return _Options.IsStaEnabled;
            }
            set
            {
                _Options.IsStaEnabled = value;
                this.NotifyOfPropertyChange(() => this.IsStaEnabled);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Selected Average type.
        /// </summary>
        private string _SelectedStaAverageType;
        /// <summary>
        /// Selected Average type.
        /// </summary>
        public string SelectedStaAverageType
        {
            get
            {
                return _SelectedStaAverageType;
            }
            set
            {
                _SelectedStaAverageType = value;
                this.NotifyOfPropertyChange(() => this.SelectedStaAverageType);

                // Check which is selected
                switch(value)
                {
                    case RUNNING_AVG:
                        IsStaAvgRunning = true;
                        break;
                    case TIME_AVG:
                        IsStaByTimer = true;
                        break;
                    case SAMPLE_AVG:
                        IsStaByNumSamples = true;
                        break;
                }   
            }
        }

        /// <summary>
        /// Set flag if STA should be based off the timer.
        /// </summary>
        public bool IsStaByTimer
        {
            get { return _Options.StaAvgMgrOptions.IsAvgByTimer; }
            set
            {
                _Options.StaAvgMgrOptions.IsAvgByTimer = value;
                _Options.StaAvgMgrOptions.IsAvgByNumSamples = !value;
                _Options.StaAvgMgrOptions.IsAvgRunning = !value;

                _staAvgMgr.IsAvgByTimer = value;

                this.NotifyOfPropertyChange(() => this.IsStaByNumSamples);
                this.NotifyOfPropertyChange(() => this.IsStaByTimer);
                this.NotifyOfPropertyChange(() => this.IsStaAvgRunning);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Set flag if STA should be based off the number of samples.
        /// </summary>
        public bool IsStaByNumSamples
        {
            get { return _Options.StaAvgMgrOptions.IsAvgByNumSamples; }
            set
            {
                _Options.StaAvgMgrOptions.IsAvgByTimer = !value;
                _Options.StaAvgMgrOptions.IsAvgByNumSamples = value;
                _Options.StaAvgMgrOptions.IsAvgRunning = !value;

                _staAvgMgr.IsAvgByNumSamples = value;

                this.NotifyOfPropertyChange(() => this.IsStaByNumSamples);
                this.NotifyOfPropertyChange(() => this.IsStaByTimer);
                this.NotifyOfPropertyChange(() => this.IsStaAvgRunning);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Flag if the short term averaging 
        /// is a running average.  A running average
        /// is removing the last ensemble from the
        /// list.  
        /// </summary>
        public bool IsStaAvgRunning
        {
            get
            {
                return _staAvgMgr.IsAvgRunning;
            }
            set
            {
                _Options.StaAvgMgrOptions.IsAvgByTimer = !value;
                _Options.StaAvgMgrOptions.IsAvgByNumSamples = !value;
                _Options.StaAvgMgrOptions.IsAvgRunning = value;

                _staAvgMgr.IsAvgRunning = value;

                this.NotifyOfPropertyChange(() => this.IsStaByNumSamples);
                this.NotifyOfPropertyChange(() => this.IsStaByTimer);
                this.NotifyOfPropertyChange(() => this.IsStaAvgRunning);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// The number of milliseconds between averages.
        /// </summary>
        public uint StaAvgTimerMilliseconds
        {
            get { return _Options.StaAvgMgrOptions.TimerMilliseconds; }
            set
            {
                _Options.StaAvgMgrOptions.TimerMilliseconds = value;
                this.NotifyOfPropertyChange(() => this.StaAvgTimerMilliseconds);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Number of samples to average together for
        /// the short term average..
        /// </summary>
        public int StaNumSamples
        {
            get
            {
                return _staAvgMgr.NumSamples;
            }
            set
            {
                _staAvgMgr.NumSamples = value;
                _Options.StaAvgMgrOptions.NumSamples = value;
                this.NotifyOfPropertyChange(() => this.StaNumSamples);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Flag if the short term averaging 
        /// is a running average.  A running average
        /// is removing the last ensemble from the
        /// list.  
        /// </summary>
        public bool StaIsSampleRunningAvg
        {
            get
            {
                return _staAvgMgr.IsSampleRunningAverage;
            }
            set
            {
                _staAvgMgr.IsSampleRunningAverage = value;
                _Options.StaAvgMgrOptions.IsSampleRunningAverage = value;
                this.NotifyOfPropertyChange(() => this.StaIsSampleRunningAvg);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Recording

        /// <summary>
        /// Turn on or off STA recording average.
        /// </summary>
        public bool IsStaRecording
        {
            get
            {
                return _Options.StaAvgMgrOptions.IsRecording;
            }
            set
            {
                _Options.StaAvgMgrOptions.IsRecording = value;
                this.NotifyOfPropertyChange(() => this.IsStaRecording);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Reference Layer Avg

        /// <summary>
        /// Flag if reference layer averaging.
        /// </summary>
        public bool StaIsReferenceLayerAveraging
        {
            get
            {
                return _staAvgMgr.IsReferenceLayerAveraging;
            }
            set
            {
                _staAvgMgr.IsReferenceLayerAveraging = value;
                _Options.StaAvgMgrOptions.IsReferenceLayerAveraging = value;

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short Term Average Minimum reference layer.
        /// </summary>
        public uint StaMinRefLayer
        {
            get
            {
                return _staAvgMgr.MinRefLayer;
            }
            set
            {
                _staAvgMgr.MinRefLayer = value;
                _Options.StaAvgMgrOptions.MinRefLayer = value;

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short Term Average Maximum reference layer.
        /// </summary>
        public uint StaMaxRefLayer
        {
            get
            {
                return _staAvgMgr.MaxRefLayer;
            }
            set
            {
                _staAvgMgr.MaxRefLayer = value;
                _Options.StaAvgMgrOptions.MaxRefLayer = value;

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Correlation

        /// <summary>
        /// Short Term Averaging Is correlation averaging flag.
        /// </summary>
        public bool StaIsCorrelationAveraging
        {
            get
            {
                return _staAvgMgr.IsCorrelationAveraging;
            }
            set
            {
                _staAvgMgr.IsCorrelationAveraging = value;
                _Options.StaAvgMgrOptions.IsCorrelationAveraging = value;
                this.NotifyOfPropertyChange(() => this.StaIsCorrelationAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average correlation scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaCorrelationScale
        {
            get
            {
                return _staAvgMgr.CorrelationScale;
            }
            set
            {
                _staAvgMgr.CorrelationScale = value;
                _Options.StaAvgMgrOptions.CorrelationScale = value;
                this.NotifyOfPropertyChange(() => this.StaCorrelationScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Amplitude

        /// <summary>
        /// Flag if Short Term Average is averaing the Amplitude data.
        /// </summary>
        public bool StaIsAmplitudeAveraging
        {
            get
            {
                return _staAvgMgr.IsAmplitudeAveraging;
            }
            set
            {
                _staAvgMgr.IsAmplitudeAveraging = value;
                _Options.StaAvgMgrOptions.IsAmplitudeAveraging = value;
                this.NotifyOfPropertyChange(() => this.StaIsAmplitudeAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average amplitude scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaAmplitudeScale
        {
            get
            {
                return _staAvgMgr.AmplitudeScale;
            }
            set
            {
                _staAvgMgr.AmplitudeScale = value;
                _Options.StaAvgMgrOptions.AmplitudeScale = value;
                this.NotifyOfPropertyChange(() => this.StaAmplitudeScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Beam Velocity

        /// <summary>
        /// Flag if Short Term Average is averaing the Beam Velocity data.
        /// </summary>
        public bool StaIsBeamVelocityAveraging
        {
            get
            {
                return _staAvgMgr.IsBeamVelocityAveraging;
            }
            set
            {
                _staAvgMgr.IsBeamVelocityAveraging = value;
                _Options.StaAvgMgrOptions.IsBeamVelocityAveraging = value;
                this.NotifyOfPropertyChange(() => this.StaIsBeamVelocityAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Beam Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBeamVelocityScale
        {
            get
            {
                return _staAvgMgr.BeamVelocityScale;
            }
            set
            {
                _staAvgMgr.BeamVelocityScale = value;
                _Options.StaAvgMgrOptions.BeamVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.StaBeamVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Instrument Velocity

        /// <summary>
        /// Flag if Short Term Average is averaing the Instrument Velocity data.
        /// </summary>
        public bool StaIsInstrumentVelocityAveraging
        {
            get
            {
                return _staAvgMgr.IsInstrumentVelocityAveraging;
            }
            set
            {
                _staAvgMgr.IsInstrumentVelocityAveraging = value;
                _Options.StaAvgMgrOptions.IsInstrumentVelocityAveraging = value;
                this.NotifyOfPropertyChange(() => this.StaIsInstrumentVelocityAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Instrument Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaInstrumentVelocityScale
        {
            get
            {
                return _staAvgMgr.InstrumentVelocityScale;
            }
            set
            {
                _staAvgMgr.InstrumentVelocityScale = value;
                _Options.StaAvgMgrOptions.InstrumentVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.StaInstrumentVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Earth Velocity

        /// <summary>
        /// Flag if Short Term Average is averaing the Earth Velocity data.
        /// </summary>
        public bool StaIsEarthVelocityAveraging
        {
            get
            {
                return _staAvgMgr.IsEarthVelocityAveraging;
            }
            set
            {
                _staAvgMgr.IsEarthVelocityAveraging = value;
                _Options.StaAvgMgrOptions.IsEarthVelocityAveraging = value;
                this.NotifyOfPropertyChange(() => this.StaIsEarthVelocityAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Earth Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaEarthVelocityScale
        {
            get
            {
                return _staAvgMgr.EarthVelocityScale;
            }
            set
            {
                _staAvgMgr.EarthVelocityScale = value;
                _Options.StaAvgMgrOptions.EarthVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.StaEarthVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #region Bottom Track

        /// <summary>
        /// Flag if Short Term Average is averaing the Bottom Track data.
        /// </summary>
        public bool StaIsBottomTrackAveraging
        {
            get
            {
                return _staAvgMgr.IsBottomTrackAveraging;
            }
            set
            {
                _staAvgMgr.IsBottomTrackAveraging = value;
                _Options.StaAvgMgrOptions.IsBottomTrackAveraging = value;
                this.NotifyOfPropertyChange(() => this.StaIsBottomTrackAveraging);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Range scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackRangeScale
        {
            get
            {
                return _staAvgMgr.BottomTrackRangeScale;
            }
            set
            {
                _staAvgMgr.BottomTrackRangeScale = value;
                _Options.StaAvgMgrOptions.BottomTrackRangeScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackRangeScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track SNR scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackSnrScale
        {
            get
            {
                return _staAvgMgr.BottomTrackSnrScale;
            }
            set
            {
                _staAvgMgr.BottomTrackSnrScale = value;
                _Options.StaAvgMgrOptions.BottomTrackSnrScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackSnrScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Amplitude scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackAmplitudeScale
        {
            get
            {
                return _staAvgMgr.BottomTrackAmplitudeScale;
            }
            set
            {
                _staAvgMgr.BottomTrackAmplitudeScale = value;
                _Options.StaAvgMgrOptions.BottomTrackAmplitudeScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackAmplitudeScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Correlation scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackCorrelationScale
        {
            get
            {
                return _staAvgMgr.BottomTrackCorrelationScale;
            }
            set
            {
                _staAvgMgr.BottomTrackCorrelationScale = value;
                _Options.StaAvgMgrOptions.BottomTrackCorrelationScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackCorrelationScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Beam Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackBeamVelocityScale
        {
            get
            {
                return _staAvgMgr.BottomTrackBeamVelocityScale;
            }
            set
            {
                _staAvgMgr.BottomTrackBeamVelocityScale = value;
                _Options.StaAvgMgrOptions.BottomTrackBeamVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackBeamVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Instrument Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackInstrumentVelocityScale
        {
            get
            {
                return _staAvgMgr.BottomTrackInstrumentVelocityScale;
            }
            set
            {
                _staAvgMgr.BottomTrackInstrumentVelocityScale = value;
                _Options.StaAvgMgrOptions.BottomTrackInstrumentVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackInstrumentVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Short term average Bottom Track Earth Velocity scale factor.
        /// This will be multiplied to the
        /// final average value.
        /// </summary>
        public float StaBottomTrackEarthVelocityScale
        {
            get
            {
                return _staAvgMgr.BottomTrackEarthVelocityScale;
            }
            set
            {
                _staAvgMgr.BottomTrackEarthVelocityScale = value;
                _Options.StaAvgMgrOptions.BottomTrackEarthVelocityScale = value;
                this.NotifyOfPropertyChange(() => this.StaBottomTrackEarthVelocityScale);

                // Save the options to DB
                SaveOptions();
            }
        }

        #endregion

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Move to the next screen.
        /// System.Reactive.Unit is equavalient to null.
        /// </summary>
        public ReactiveCommand<object> NextCommand { get; protected set; }

        /// <summary>
        /// Go back a screen.
        /// </summary>
        public ReactiveCommand<object> BackCommand { get; protected set; }

        /// <summary>
        /// Exit the wizard.
        /// </summary>
        public ReactiveCommand<object> ExitCommand { get; protected set; }

        /// <summary>
        /// Command to clear the STA average.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearStaCommand { get; protected set; }

        /// <summary>
        /// Command to clear the LTA average.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearLtaCommand { get; protected set; }

        /// <summary>
        /// Command to close this VM.
        /// </summary>
        public ReactiveCommand<object> CloseVMCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the view model.
        /// </summary>
        public AveragingViewModel(SubsystemDataConfig config)
        :base("Averaging")
        {
            // Initialize values
            _Config = config;
            _pm = IoC.Get<PulseManager>();
            _events = IoC.Get<IEventAggregator>();
            _adcpConn = IoC.Get<AdcpConnection>();

            // Initialize the options
            GetOptionsFromDatabase();

            // Average manager
            _ltaAvgMgr = new AverageManager(_Options.LtaAvgMgrOptions);
            _ltaAvgMgr.AveragedEnsemble += new AverageManager.AveragedEnsembleEventHandler(_ltaAvgMgr_AveragedEnsemble);
            _staAvgMgr = new AverageManager(_Options.StaAvgMgrOptions);
            _staAvgMgr.AveragedEnsemble += new AverageManager.AveragedEnsembleEventHandler(_staAvgMgr_AveragedEnsemble);

            // Setup the lists
            SetupList();

            // Next command
            NextCommand = ReactiveCommand.Create();
            NextCommand.Subscribe(_ => _events.PublishOnUIThread(new ViewNavEvent(ViewNavEvent.ViewId.AdcpConfigurationView)));

            // Back coommand
            BackCommand = ReactiveCommand.Create();
            BackCommand.Subscribe(_ => _events.PublishOnUIThread(new ViewNavEvent(ViewNavEvent.ViewId.Back)));

            // Exit coommand
            ExitCommand = ReactiveCommand.Create();
            ExitCommand.Subscribe(_ => _events.PublishOnUIThread(new ViewNavEvent(ViewNavEvent.ViewId.HomeView)));

            // Close the VM
            CloseVMCommand = ReactiveCommand.Create();
            CloseVMCommand.Subscribe(_ => _events.PublishOnUIThread(new CloseVmEvent(_Config)));

            // Clear the STA average
            ClearStaCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => _staAvgMgr.Clear()));                    // Clear the STA average

            // Clear the LTA average
            ClearLtaCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => _ltaAvgMgr.Clear()));                    // Clear the LTA average
        }


        /// <summary>
        /// Shutdown the view model.
        /// </summary>
        public override void Dispose()
        {
            _ltaAvgMgr.AveragedEnsemble -= _ltaAvgMgr_AveragedEnsemble;
            _staAvgMgr.AveragedEnsemble -= _staAvgMgr_AveragedEnsemble;

            if (_staWriter != null)
            {
                _staWriter.Flush();
                _staWriter.Dispose();
            }

            if (_ltaWriter != null)
            {
                _ltaWriter.Flush();
                _ltaWriter.Dispose();
            }
        }

        /// <summary>
        /// Take the ensemble and add it to the average managers.
        /// </summary>
        /// <param name="ensemble">Ensemble to average.</param>
        public void AverageEnsemble(DataSet.Ensemble ensemble)
        {
            if (_Options.IsLtaEnabled)
            {
                // LTA
                _ltaAvgMgr.AddEnsemble(ensemble);
            }

            if (_Options.IsStaEnabled)
            {
                // STA
                _staAvgMgr.AddEnsemble(ensemble);
            }
        }

        #region Update Properties

        /// <summary>
        /// Update all the properties.
        /// </summary>
        private void NotifyResultsProperties()
        {
            // Notify all the properties
            this.NotifyOfPropertyChange();
        }

        #endregion

        #region Options

        /// <summary>
        /// Get the options for this subsystem display
        /// from the database.  If the options have not
        /// been set to the database yet, default values 
        /// will be used.
        /// </summary>
        private void GetOptionsFromDatabase()
        {
            if (_pm.IsProjectSelected)
            {
                // Get the project's average options
                var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);
                _Options = _pm.AppConfiguration.GetAverageOptions(ssConfig);
            }
            else
            {
                // Get the latest average options
                var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);
                _Options = new AverageSubsystemConfigOptions(ssConfig);
                _Options.AvgOptions = _pm.GetAverageOptions();
            }

            // Notify all the properties
            NotifyResultsProperties();
        }

        /// <summary>
        /// Save the options to the project.
        /// </summary>
        private void SaveOptions()
        {
            // SubsystemDataConfig needs to be converted to a SubsystemConfiguration
            // because the SubsystemConfig will be compared in AppConfiguration to determine
            // where to save the settings.  Because SubsystemDataConfig and SubsystemConfiguration
            // are not the same type, it will not pass Equal()
            var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);

            _pm.AppConfiguration.SaveAverageOptions(ssConfig, _Options);

            // Update the average options
            _pm.UpdateAverageOptions(_Options.AvgOptions);
        }

        #endregion

        #region Record Data

        /// <summary>
        /// Record the binary data to the project.
        /// This will take the binary data and add it
        /// to the projects buffer to written to the file.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <returns>TRUE = Data written to binary file.</returns>
        private bool RecordStaData(byte[] data)
        {
            // Create the writer if it does not exist
            if (_staWriter == null)
            {
                if (_pm.IsProjectSelected)
                {
                    _staWriter = new AdcpBinaryWriter(_pm.SelectedProject, AdcpBinaryWriter.DEFAULT_BINARY_FILE_SIZE, AdcpBinaryWriter.FileType.STA);
                    _staWriter.SerialNumber = _pm.SelectedProject.SerialNumber.ToString();
                }
                else
                {
                    SerialNumber serial = new SerialNumber();
                    _staWriter = new AdcpBinaryWriter(new Project("",RTI.Pulse.Commons.GetProjectDefaultFolderPath(), serial.ToString()), AdcpBinaryWriter.DEFAULT_BINARY_FILE_SIZE, AdcpBinaryWriter.FileType.STA);
                    _staWriter.SerialNumber = serial.ToString();
                }
                _staWriter.ResetFileName();
            }

            // Add the data to the writer
            if (_staWriter != null)
            {
                _staWriter.AddIncomingData(data);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Record the binary data to the project.
        /// This will take the binary data and add it
        /// to the projects buffer to written to the file.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <returns>TRUE = Data written to binary file.</returns>
        private bool RecordLtaData(byte[] data)
        {
            // Create the writer if it does not exist
            if (_ltaWriter == null)
            {
                if (_pm.IsProjectSelected)
                {
                    _ltaWriter = new AdcpBinaryWriter(_pm.SelectedProject, AdcpBinaryWriter.DEFAULT_BINARY_FILE_SIZE, AdcpBinaryWriter.FileType.LTA);
                    _ltaWriter.SerialNumber = _pm.SelectedProject.SerialNumber.ToString();
                }
                else
                {
                    SerialNumber serial = new SerialNumber();
                    _ltaWriter = new AdcpBinaryWriter(new Project("", RTI.Pulse.Commons.GetProjectDefaultFolderPath(), serial.ToString()), AdcpBinaryWriter.DEFAULT_BINARY_FILE_SIZE, AdcpBinaryWriter.FileType.STA);
                    _ltaWriter.SerialNumber = serial.ToString();
                }
                _ltaWriter.ResetFileName();
            }

            // Add the data to the writer
            if (_ltaWriter != null)
            {
                _ltaWriter.AddIncomingData(data);
                return true;
            }

            return false;
        }

        #endregion

        #region List

        /// <summary>
        /// Setup the list.
        /// </summary>
        private void SetupList()
        {
            // List
            AverageType = new List<string>();
            AverageType.Add(RUNNING_AVG);
            AverageType.Add(TIME_AVG);
            AverageType.Add(SAMPLE_AVG);

            // LTA
            if (IsStaByNumSamples)
            {
                _SelectedStaAverageType = SAMPLE_AVG;
            }
            else if (IsStaByTimer)
            {
                _SelectedStaAverageType = TIME_AVG;
            }
            else
            {
                _SelectedStaAverageType = RUNNING_AVG;
            }
            this.NotifyOfPropertyChange(() => this.SelectedStaAverageType);

            // LTA
            if (IsLtaByNumSamples)
            {
                _SelectedLtaAverageType = SAMPLE_AVG;
            }
            else if (IsLtaByTimer)
            {
                _SelectedLtaAverageType = TIME_AVG;
            }
            else
            {
                _SelectedLtaAverageType = RUNNING_AVG;
            }
            this.NotifyOfPropertyChange(() => this.SelectedLtaAverageType);
        }

        #endregion

        #region Event Handler

        #region Averaged

        /// <summary>
        /// Receive the averaged long term average after the
        /// number of samples have been met.  Publish the data when received.
        /// </summary>
        /// <param name="ensemble">Long Term Averaged ensemble received from the average manager.</param>
        void _ltaAvgMgr_AveragedEnsemble(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                // Publish the data
                // Do not publish the data if you are importing data
                if (!_adcpConn.IsImporting)
                {
                    _events.PublishOnUIThread(new EnsembleEvent(ensemble, EnsembleSource.LTA, EnsembleType.LTA));
                }

                // Record the data
                if (_ltaAvgMgr.IsRecording)
                {
                    RecordLtaData(ensemble.Encode());
                }
            }
        }

        /// <summary>
        /// Receive the averaged short term average after the
        /// number of samples have been met.  Publish the data when received.
        /// </summary>
        /// <param name="ensemble">Short Term Averaged ensemble received from the average manager.</param>
        void _staAvgMgr_AveragedEnsemble(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                // Publish the data
                // Do not publish the data if you are importing data
                if (!_adcpConn.IsImporting)
                {
                    _events.PublishOnUIThread(new EnsembleEvent(ensemble, EnsembleSource.STA, EnsembleType.STA));
                }

                // Record the data
                if (_staAvgMgr.IsRecording)
                {
                    RecordStaData(ensemble.Encode());
                }
            }
        }

        #endregion

        #endregion

    }
}
