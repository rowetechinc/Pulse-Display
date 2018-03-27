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
 * 07/23/2013      RC          3.0.4      Initial coding
 * 08/13/2013      RC          3.0.7      Added MarkBadBelowBottom.
 * 05/07/2014      RC          3.2.4      When Retransform is selected, also retransform BT data.  Added Correlation and SNR threshold to retransform.
 * 08/13/2014      RC          4.0.0      Fixed a bug with CanUseGpsVel storing the value.
 * 08/20/2014      RC          4.0.1      Added CloseVMCommand.
 * 01/06/2016      RC          4.4.0      Added Retransform heading offset and GPS heading.
 * 05/11/2016      RC          4.4.3      Added ScreenBadHeading.
 * 04/26/2017      RC          4.4.6      In SetPreviousBottomTrackVelocity() checked if VTG message exists.
 * 09/05/2017      RC          4.4.7      Fixed RemovedShip to include instrument and ship transform.
 * 09/28/2017      RC          4.4.7      Added original data format to know how to retransform the data.  PD0 is differnt from RTB.
 * 09/29/2017      RC          4.4.7      Added FillInMissingWpData() to fill in data when Water Profile is turned off.
 * 03/23/2018      RC          4.8.0      Added _prevBtRange to keep a backup value of the Range.  Use it Mark Bad Below Bottom.
 * 03/27/2018      RC          4.8.1      Added Tab description.
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

    /// <summary>
    /// Set the options on how to screen the incoming data.
    /// </summary>
    public class ScreenDataViewModel : PulseViewModel
    {
        #region Variables

        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Project manager.
        /// </summary>
        private PulseManager _pm;

        /// <summary>
        /// Event aggregator.
        /// </summary>
        private IEventAggregator _events;

        /// <summary>
        /// Previous Good Bottom Track East velocity.
        /// </summary>
        private float _prevBtEast = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Good Bottom Track North velocity.
        /// </summary>
        private float _prevBtNorth = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Good Bottom Track Vertical velocity.
        /// </summary>
        private float _prevBtVert = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Ship Speed X.  Used to remove ship speed from Velocity data.
        /// </summary>
        private float _prevShipSpeedX = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Ship Speed Y.  Used to remove ship speed from Velocity data.
        /// </summary>
        private float _prevShipSpeedY = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Ship Speed Z.  Used to remove ship speed from Velocity data.
        /// </summary>
        private float _prevShipSpeedZ = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Ship Speed Transverse.  Used to remove ship speed from Velocity data.
        /// </summary>
        private float _prevShipSpeedTransverse = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Ship Speed Longitundial.  Used to remove ship speed from Velocity data.
        /// </summary>
        private float _prevShipSpeedLongitudinal = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous Ship Speed Normal.  Used to remove ship speed from Velocity data.
        /// </summary>
        private float _prevShipSpeedNormal = DataSet.Ensemble.BAD_VELOCITY;

        /// <summary>
        /// Previous good Bottom Track range.
        /// </summary>
        private float _prevBtRange = DataSet.Ensemble.BAD_RANGE;

        /// <summary>
        /// Use the previous good heading, if heading drops out.
        /// </summary>
        private float _prevHeading;

        /// <summary>
        /// Options to store for screening the data.
        /// </summary>
        private ScreenSubsystemConfigOptions _Options;

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

        /// <summary>
        /// String of the tab description of subsystem configuration.
        /// </summary>
        private string _TabDesc;
        /// <summary>
        /// String of the tab description of subsystem configuration.
        /// </summary>
        public string TabDesc
        {
            get { return _TabDesc; }
            set
            {
                _TabDesc = value;
                this.NotifyOfPropertyChange(() => this.TabDesc);
            }
        }

        #endregion

        #region Display

        /// <summary>
        /// Display the CEPO index to describe this view model.
        /// </summary>
        public string Display
        {
            get
            {
                return _Config.IndexCodeString();
            }
        }

        /// <summary>
        /// Display the CEPO index to describe this view model.
        /// </summary>
        public string Title
        {
            get
            {
                return string.Format("[{0}]{1}", _Config.CepoIndex.ToString(), _Config.SubSystem.CodedDescString());
            }
        }

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

        #region Remove Ship Speed

        /// <summary>
        /// Turn on or off removing ship speed.
        /// </summary>
        public bool IsRemoveShipSpeed
        {
            get { return _Options.IsRemoveShipSpeed; }
            set
            {
                _Options.IsRemoveShipSpeed = value;
                this.NotifyOfPropertyChange(() => this.IsRemoveShipSpeed);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Turn on or off using bottom track in removing ship speed.
        /// </summary>
        public bool CanUseBottomTrackVel
        {
            get { return _Options.CanUseBottomTrackVel; }
            set
            {
                _Options.CanUseBottomTrackVel = value;
                this.NotifyOfPropertyChange(() => this.CanUseBottomTrackVel);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Turn on or off using GPS in removing ship speed.
        /// </summary>
        public bool CanUseGpsVel
        {
            get { return _Options.CanUseGpsVel; }
            set
            {
                _Options.CanUseGpsVel = value;
                this.NotifyOfPropertyChange(() => this.CanUseGpsVel);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Turn Heading offset to use in GPS heading.  This is used for post processing only.
        /// </summary>
        public double GpsHeadingOffset
        {
            get { return _Options.GpsHeadingOffset; }
            set
            {
                _Options.GpsHeadingOffset = value;
                this.NotifyOfPropertyChange(() => this.GpsHeadingOffset);

                // Save the options
                SaveOptions();
            }
        }

        #endregion

        #region Mark Bad Below Bottom

        /// <summary>
        /// Turn on or off Mark Bad Below bottom.
        /// </summary>
        public bool IsMarkBadBelowBottom
        {
            get { return _Options.IsMarkBadBelowBottom; }
            set
            {
                _Options.IsMarkBadBelowBottom = value;
                this.NotifyOfPropertyChange(() => this.IsMarkBadBelowBottom);

                // Save the options
                SaveOptions();
            }
        }

        #endregion

        #region Force 3 Beam Solution

        /// <summary>
        /// Turn on or off Force 3 Beam solution.
        /// </summary>
        public bool IsForce3BeamSolution
        {
            get { return _Options.IsForce3BeamSolution; }
            set
            {
                _Options.IsForce3BeamSolution = value;
                this.NotifyOfPropertyChange(() => this.IsForce3BeamSolution);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Beam selected for force 3 beam solution.
        /// </summary>
        public int ForceBeamBad
        {
            get { return _Options.ForceBeamBad; }
            set
            {
                _Options.ForceBeamBad = value;
                this.NotifyOfPropertyChange(() => this.ForceBeamBad);

                // Save the options
                SaveOptions();
            }
        }

        #endregion

        #region Force Bottom Track 3 Beam Solution

        /// <summary>
        /// Turn on or off Force 3 Beam solution.
        /// </summary>
        public bool IsForce3BottomTrackBeamSolution
        {
            get { return _Options.IsForce3BottomTrackBeamSolution; }
            set
            {
                _Options.IsForce3BottomTrackBeamSolution = value;
                this.NotifyOfPropertyChange(() => this.IsForce3BottomTrackBeamSolution);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Beam selected for force Bottom Track 3 beam solution.
        /// </summary>
        public int ForceBottomTrackBeamBad
        {
            get { return _Options.ForceBottomTrackBeamBad; }
            set
            {
                _Options.ForceBottomTrackBeamBad = value;
                this.NotifyOfPropertyChange(() => this.ForceBottomTrackBeamBad);

                // Save the options
                SaveOptions();
            }
        }

        #endregion

        #region Re-Transform Data

        /// <summary>
        /// Turn on or off Re-Transform the data.
        /// </summary>
        public bool IsRetransformData
        {
            get { return _Options.IsRetransformData; }
            set
            {
                _Options.IsRetransformData = value;
                this.NotifyOfPropertyChange(() => this.IsRetransformData);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// List of available heading sources.
        /// </summary>
        private List<Transform.HeadingSource> _HeadingSourceList;
        /// <summary>
        /// List of available heading sources.
        /// </summary>
        public List<Transform.HeadingSource> HeadingSourceList
        {
            get { return _HeadingSourceList; }
            set
            {
                _HeadingSourceList = value;
                this.NotifyOfPropertyChange(() => this.HeadingSourceList);
            }
        }

        /// <summary>
        /// Selected heading sources.
        /// </summary>
        public Transform.HeadingSource SelectedHeadingSource
        {
            get { return _Options.RetransformHeadingSource; }
            set
            {
                _Options.RetransformHeadingSource = value;
                this.NotifyOfPropertyChange(() => this.SelectedHeadingSource);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Heading offset when retransforming the data.
        /// </summary>
        public float RetransformHeadingOffset
        {
            get { return _Options.RetransformHeadingOffset; }
            set
            {
                _Options.RetransformHeadingOffset = value;
                this.NotifyOfPropertyChange(() => this.RetransformHeadingOffset);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Water Profile Correlation Threshold.
        /// </summary>
        public float WpCorrThresh
        {
            get { return _Options.WpCorrThresh; }
            set
            {
                _Options.WpCorrThresh = value;
                this.NotifyOfPropertyChange(() => this.WpCorrThresh);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Bottom Track Correlation Threshold.
        /// </summary>
        public float BtCorrThresh
        {
            get { return _Options.BtCorrThresh; }
            set
            {
                _Options.BtCorrThresh = value;
                this.NotifyOfPropertyChange(() => this.BtCorrThresh);

                // Save the options
                SaveOptions();
            }
        }

        /// <summary>
        /// Bottom Track SNR Threshold.
        /// </summary>
        public float BtSnrThresh
        {
            get { return _Options.BtSnrThresh; }
            set
            {
                _Options.BtSnrThresh = value;
                this.NotifyOfPropertyChange(() => this.BtSnrThresh);

                // Save the options
                SaveOptions();
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to close this VM.
        /// </summary>
        public ReactiveCommand<object> CloseVMCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the view model with the configuration.
        /// </summary>
        /// <param name="config">Subsystem Configuration.</param>
        public ScreenDataViewModel(SubsystemDataConfig config) :
            base("ScreenDataViewModel")
        {
            _Config = config;
            _pm = IoC.Get<PulseManager>();
            TabDesc = config.IndexCodeString();

            // Get the Event Aggregator
            _events = IoC.Get<IEventAggregator>();

            // Initialize previous values
            _prevBtEast = DataSet.Ensemble.BAD_VELOCITY;
            _prevBtNorth = DataSet.Ensemble.BAD_VELOCITY;
            _prevBtVert = DataSet.Ensemble.BAD_VELOCITY;
            _prevBtRange = DataSet.Ensemble.BAD_RANGE;
            _prevHeading = 0.0f;

            //SelectedHeadingSource = Transform.HeadingSource.ADCP;
            HeadingSourceList = Enum.GetValues(typeof(Transform.HeadingSource)).Cast<Transform.HeadingSource>().ToList();

            // Initialize the options
            GetOptionsFromDatabase();

            // Close the VM
            CloseVMCommand = ReactiveCommand.Create();
            CloseVMCommand.Subscribe(_ => _events.PublishOnUIThread(new CloseVmEvent(_Config)));
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            
        }

        #region Options

        /// <summary>
        /// Get the options for this subsystem display
        /// from the database.  If the options have not
        /// been set to the database yet, default values 
        /// will be used.
        /// </summary>
        private void GetOptionsFromDatabase()
        {
            var ssConfig = new SubsystemConfiguration(_Config.SubSystem, _Config.CepoIndex, _Config.SubsystemConfigIndex);
            _Options = _pm.AppConfiguration.GetScreenOptions(ssConfig);

            // Notify all the properties
            this.NotifyOfPropertyChange();
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

            _pm.AppConfiguration.SaveScreenOptions(ssConfig, _Options);
        }

        #endregion

        #region Screen


        /// <summary>
        /// Screen the ensembles based off the options selected.
        /// The original data format is needed because the Re-Transform will be done
        /// different for PD0 data.
        /// </summary>
        /// <param name="ensemble">Ensemble to screen.</param>
        /// <param name="origDataFormat">Original Data format.</param>
        public void ScreenEnsemble(ref DataSet.Ensemble ensemble, AdcpCodec.CodecEnum origDataFormat)
        {
            // Fill in for missing data if only Bottom Track is turned on
            FillInMissingWpData(ref ensemble);

            // Screen for bad heading
            ScreenData.ScreenBadHeading.Screen(ref ensemble, _prevHeading);
            SetPreviousHeading(ensemble);

            // Force 3 Beam Solution
            if (_Options.IsForce3BeamSolution)
            {
                ScreenData.ScreenForce3BeamSolution.Force3BeamSolution(ref ensemble, _Options.ForceBeamBad, origDataFormat);
            }

            // Force 3 Beam Bottom Track solution
            if (_Options.IsForce3BottomTrackBeamSolution)
            {
                ScreenData.ScreenForce3BeamSolution.Force3BottomTrackBeamSolution(ref ensemble, _Options.ForceBottomTrackBeamBad, origDataFormat);
            }

            // Retransform the data
            if (_Options.IsRetransformData)
            {
                // PD0 has a different cooridiate matrix
                // And the beams are in different positions
                Transform.ProfileTransform(ref ensemble, origDataFormat, _Options.WpCorrThresh, _Options.RetransformHeadingSource, _Options.RetransformHeadingOffset);
                Transform.BottomTrackTransform(ref ensemble, origDataFormat, _Options.BtCorrThresh, _Options.BtSnrThresh, _Options.RetransformHeadingSource, _Options.RetransformHeadingOffset);

                // WaterMass transform data
                // This will also create the ship data
                if (ensemble.IsInstrumentWaterMassAvail)
                {
                    Transform.WaterMassTransform(ref ensemble, origDataFormat, _Options.BtCorrThresh, _Options.BtSnrThresh, _Options.RetransformHeadingSource, _Options.RetransformHeadingOffset, 0.0f);
                }
            }

            // Mark Bad Below Bottom
            if (_Options.IsMarkBadBelowBottom)
            {
                ScreenData.ScreenMarkBadBelowBottom.Screen(ref ensemble, _prevBtRange);
            }

            // Remove Ship Speed
            if (_Options.IsRemoveShipSpeed)
            {
                ScreenData.RemoveShipSpeed.RemoveVelocity(ref ensemble, _prevBtEast, _prevBtNorth, _prevBtVert, _Options.CanUseBottomTrackVel, _Options.CanUseGpsVel, _Options.GpsHeadingOffset);
                ScreenData.RemoveShipSpeed.RemoveVelocityInstrument(ref ensemble, _prevShipSpeedX, _prevShipSpeedY, _prevShipSpeedZ, _Options.CanUseBottomTrackVel, _Options.CanUseGpsVel, _Options.GpsHeadingOffset);
                ScreenData.RemoveShipSpeed.RemoveVelocityShip(ref ensemble, _prevShipSpeedTransverse, _prevShipSpeedLongitudinal, _prevShipSpeedNormal, _Options.CanUseBottomTrackVel, _Options.CanUseGpsVel, _Options.GpsHeadingOffset);

                // Create the new velocity vectors based off the new data
                DataSet.VelocityVectorHelper.CreateVelocityVector(ref ensemble);
            }

            // Record the previous ship speed values
            SetPreviousShipSpeed(ensemble);
        }

        #endregion

        #region Remove Ship Speed

        /// <summary>
        /// Store the previous Ship speed for the different velocity coordinate transforms.
        /// </summary>
        /// <param name="ens">Ensembles.</param>
        private void SetPreviousShipSpeed(DataSet.Ensemble ens)
        {
            // EARTH
            // Record the Bottom for previous values
            float[] prevShipSpeed = ScreenData.RemoveShipSpeed.GetPreviousShipSpeed(ens, (float)_Options.GpsHeadingOffset, _Options.CanUseBottomTrackVel, _Options.CanUseGpsVel);
            _prevBtEast = prevShipSpeed[0];
            _prevBtNorth = prevShipSpeed[1];
            _prevBtVert = prevShipSpeed[2];

            // Instrument
            // Record the Bottom for previous values
            float[] prevShipSpeedInstrument = ScreenData.RemoveShipSpeed.GetPreviousShipSpeedInstrument(ens, (float)_Options.GpsHeadingOffset, _Options.CanUseBottomTrackVel, _Options.CanUseGpsVel);
            _prevShipSpeedX = prevShipSpeedInstrument[0];
            _prevShipSpeedY = prevShipSpeedInstrument[1];
            _prevShipSpeedZ = prevShipSpeedInstrument[2];

            // Ship
            // Record the Bottom for previous values
            float[] prevShipSpeedShip = ScreenData.RemoveShipSpeed.GetPreviousShipSpeedShip(ens, (float)_Options.GpsHeadingOffset, _Options.CanUseBottomTrackVel, _Options.CanUseGpsVel);
            _prevShipSpeedTransverse = prevShipSpeedShip[0];
            _prevShipSpeedLongitudinal = prevShipSpeedShip[1];
            _prevShipSpeedNormal = prevShipSpeedShip[2];

            if (ens.IsBottomTrackAvail)
            {
                float range = ens.BottomTrackData.GetAverageRange();
                if (range != DataSet.Ensemble.BAD_RANGE)
                {
                    _prevBtRange = range;
                }
            }

        }

        #endregion

        #region Previous Heading

        /// <summary>
        /// Set the previous heading so we can always have the last good heading.
        /// </summary>
        /// <param name="ensemble">Ensemble to get last good heading.</param>
        private void SetPreviousHeading(DataSet.Ensemble ensemble)
        {
            if(ensemble.IsAncillaryAvail && ensemble.AncillaryData.Heading != 0.0f)
            {
                _prevHeading = ensemble.AncillaryData.Heading;
            }
        }

        #endregion

        #region Missing Water Profile

        /// <summary>
        /// Fill in missing values if water profile is turned off.  If it is turned off, it sets these values to 0 but
        /// Bottom Track has the values.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the data.</param>
        public void FillInMissingWpData(ref DataSet.Ensemble ensemble)
        {
            if(ensemble.IsBottomTrackAvail && ensemble.IsEnsembleAvail)
            {
                // If these values are zero, then most likely WP is turned off
                // Fill in the data
                if(ensemble.EnsembleData.NumBeams == 0  && ensemble.EnsembleData.NumBins == 0 && ensemble.BottomTrackData.NumBeams != ensemble.EnsembleData.NumBeams)
                {
                    ensemble.EnsembleData.NumBeams = (int)ensemble.BottomTrackData.NumBeams;

                    if (ensemble.IsAncillaryAvail)
                    {
                        ensemble.AncillaryData.Heading = ensemble.BottomTrackData.Heading;
                        ensemble.AncillaryData.Pitch = ensemble.BottomTrackData.Pitch;
                        ensemble.AncillaryData.Roll = ensemble.BottomTrackData.Roll;
                        ensemble.AncillaryData.WaterTemp = ensemble.BottomTrackData.WaterTemp;
                        ensemble.AncillaryData.SystemTemp = ensemble.BottomTrackData.SystemTemp;
                        ensemble.AncillaryData.Salinity = ensemble.BottomTrackData.Salinity;
                        ensemble.AncillaryData.Pressure = ensemble.BottomTrackData.Pressure;
                        ensemble.AncillaryData.TransducerDepth = ensemble.BottomTrackData.TransducerDepth;
                        ensemble.AncillaryData.SpeedOfSound = ensemble.BottomTrackData.SpeedOfSound;
                        ensemble.AncillaryData.FirstPingTime = ensemble.BottomTrackData.FirstPingTime;
                        ensemble.AncillaryData.LastPingTime = ensemble.BottomTrackData.LastPingTime;
                    }
                }
            }
        }

        #endregion
    }
}
