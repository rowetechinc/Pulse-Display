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
 * 12/20/2013      RC          3.2.1       Initial coding
 * 08/07/2014      RC          4.0.0       Updated ReactiveCommand to 6.0.
 * 05/20/2015      RC          4.1.3       Update all the properties after setting defaults.
 * 06/07/2016      RC          4.4.3       If CWPP is 1, set CWPTBP to 0.
 * 10/19/2016      RC          4.4.4       Added the Command Set to the display.
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
    using System.IO;

    /// <summary>
    /// Allow the user to test out settings in the prediction model.
    /// </summary>
    public class AdcpPredictionModelViewModel : PulseViewModel
    {
        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        /// <summary>
        /// ADCP Predictor User Input to save to the database.
        /// </summary>
        AdcpPredictorUserInput _UserInput;

        /// <summary>
        /// ADCP Configuration to keep the command file.
        /// </summary>
        AdcpConfiguration _adcpConfig;

        #endregion

        #region Properties

        #region CommandFile
        private string _AdcpCommandSet;

        public string AdcpCommandSet
        {
            get { return _AdcpCommandSet; }
            set { _AdcpCommandSet = value;
                this.NotifyOfPropertyChange(() => this.AdcpCommandSet);
            }
        }

        public string _secondaryAdcpCommandSet;

        public string secondaryAdcpCommandSet
        {
            get { return _secondaryAdcpCommandSet; }
            set
            {
                _secondaryAdcpCommandSet = value;
                this.NotifyOfPropertyChange(() => this.secondaryAdcpCommandSet);
            }
        }

        private void ClearCommandSet()
        {
            AdcpCommandSet = "";
        }

        


        #endregion

        #region Subsystem

        /// <summary>
        /// List of all the subsystems with a description.
        /// Used to populate the combobox.
        /// </summary>
        public SubsystemList ListOfSubsystems { get; set; }

        /// <summary>
        /// Selected Subsystem.
        /// </summary>
        private RTI.SubsystemList.SubsystemCodeDesc _SelectedSubsystem;
        /// <summary>
        /// Selected Subsystem.
        /// </summary>
        public RTI.SubsystemList.SubsystemCodeDesc SelectedSubsystemss
        {
            get { return _SelectedSubsystem; }
            set
            {
                _SelectedSubsystem = value;
                this.NotifyOfPropertyChange(() => this.SelectedSubsystem);

                //SetSubsystem(_SelectedSubsystem.Code);
            }
        }
        public RTI.SubsystemList.SubsystemCodeDesc SelectedSubsystem
        {
            get { return _SelectedSubsystem; }
            set
            {
                _SelectedSubsystem = value;
                this.NotifyOfPropertyChange(() => this.SelectedSubsystem);

                // Set the new subsystem
                SetSubsystem(_SelectedSubsystem.Code);

                // Save the input
                _UserInput.SubSystem = new Subsystem(value.Code);
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Set defaults to ADCP Config
                SetAdcpDefaults();
                // Save Configuration
                //AddSubsystemConfig();
                UpdateCommandSet();

            }
        }

        #endregion

        #region Predictor

        /// <summary>
        /// ADCP Predictor
        /// </summary>
        private AdcpPredictor _Predictor;
        /// <summary>
        /// ADCP Predictor
        /// </summary>
        public AdcpPredictor Predictor
        {
            get { return _Predictor; }
            set
            {
                _Predictor = value;
                this.NotifyOfPropertyChange(() => this.Predictor);
            }
        }

        #endregion

        #region Result Strings

        /// <summary>
        /// Bottom Track Range.
        /// </summary>
        public string PredictedBottomRange
        {
            get { return Predictor.PredictedBottomRange.ToString("0.000"); }
        }

        /// <summary>
        /// Water Profile Range.
        /// </summary>
        public string PredictedProfileRange
        {
            get { return Predictor.PredictedProfileRange.ToString("0.000"); }
        }

        /// <summary>
        /// Maximum Velocity.
        /// </summary>
        public string MaximumVelocity
        {
            get { return Predictor.MaximumVelocity.ToString("0.000"); }
        }

        /// <summary>
        /// Standard Deviation.
        /// </summary>
        public string StandardDeviation
        {
            get { return Predictor.StandardDeviation.ToString("0.000"); }
        }

        /// <summary>
        /// First Bin Position.
        /// </summary>
        public string ProfileFirstBinPosition
        {
            get { return Predictor.ProfileFirstBinPosition.ToString("0.000"); }
        }

        /// <summary>
        /// Number of Battery Packs.
        /// </summary>
        public string NumberBatteryPacks
        {
            get { return Predictor.NumberBatteryPacks.ToString("0.000"); }
        }

        /// <summary>
        /// Watt/Hours.
        /// </summary>
        public string WattHours
        {
            get { return Predictor.TotalPower.ToString("0.000"); }
        }

        /// <summary>
        /// Data Size as a string.
        /// </summary>
        public string DataSize
        {
            get { return MathHelper.MemorySizeString(Predictor.DataSizeBytes); }
        }

        #endregion

        #region User Input

        /// <summary>
        /// Deployment Duration.
        /// </summary>
        public uint DeploymentDuration
        {
            get { return Predictor.DeploymentDuration; }
            set
            {
                Predictor.DeploymentDuration = value;
                this.NotifyOfPropertyChange(() => this.DeploymentDuration);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.DeploymentDuration = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
               
            }
        }

        /// <summary>
        /// Ensemble Interval.  In seconds.
        /// </summary>
        public double CEI
        {
            get { return Predictor.CEI; }
            set
            {
                Predictor.CEI = value;
                this.NotifyOfPropertyChange(() => this.CEI);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CEI = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.Commands.CEI = new Commands.TimeValue((float)value);
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// CEI description.
        /// </summary>
        public string CEI_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCeiDesc();
            }
        }

        /// <summary>
        /// Turn on or off water profile.
        /// </summary>
        public bool CWPON
        {
            get { return Predictor.CWPON; }
            set
            {
                Predictor.CWPON = value;
                this.NotifyOfPropertyChange(() => this.CWPON);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPON = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPON = value;
                UpdateCommandSet();
               
            }
        }

        /// <summary>
        /// CWPON description.
        /// </summary>
        public string CWPON_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwponDesc();
            }
        }

        /// <summary>
        /// Turn on or off Bottom Track.
        /// </summary>
        public bool CBTON
        {
            get { return Predictor.CBTON; }
            set
            {
                Predictor.CBTON = value;
                this.NotifyOfPropertyChange(() => this.CBTON);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CBTON = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTON = value;
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// Bottom Track Time Between Pings.
        /// </summary>
        public float CBTTBP
        {
            get { return Predictor.CBTTBP; }
            set
            {
                Predictor.CBTTBP = value;
                this.NotifyOfPropertyChange(() => this.CBTTBP);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CBTTBP = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTTBP = value;
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// List of Bottom Track Broadband modes.
        /// </summary>
        public List<Commands.AdcpSubsystemCommands.eCBTBB_Mode> BTBB_List { get; set; }

        /// <summary>
        /// Bottom Track Broadband mode.
        /// </summary>
        public Commands.AdcpSubsystemCommands.eCBTBB_Mode CBTBB_TransmitPulseType
        {
            get { return Predictor.CBTBB_TransmitPulseType; }
            set
            {
                Predictor.CBTBB_TransmitPulseType = value;
                this.NotifyOfPropertyChange(() => this.CBTBB_TransmitPulseType);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CBTBB_TransmitPulseType = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTBB_Mode = value;
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// Water Profile Time Between Ping.
        /// </summary>
        public float CWPTBP
        {
            get { return Predictor.CWPTBP; }
            set
            {
                Predictor.CWPTBP = value;
                this.NotifyOfPropertyChange(() => this.CWPTBP);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPTBP = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPTBP = value;
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// Water Profile Number of bins.
        /// </summary>
        public ushort CWPBN
        {
            get { return Predictor.CWPBN; }
            set
            {
                Predictor.CWPBN = value;
                this.NotifyOfPropertyChange(() => this.CWPBN);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPBN = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBN = value;
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// Water Profile Bin size.
        /// </summary>
        public float CWPBS
        {
            get { return Predictor.CWPBS; }
            set
            {
                Predictor.CWPBS = value;
                this.NotifyOfPropertyChange(() => this.CWPBS);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPBS = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBS = value;
                UpdateCommandSet();
               
            }
        }

        /// <summary>
        /// Water Profile blank.
        /// </summary>
        public float CWPBL
        {
            get { return Predictor.CWPBL; }
            set
            {
                Predictor.CWPBL = value;
                this.NotifyOfPropertyChange(() => this.CWPBL);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPBL = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBL = value;
                UpdateCommandSet();
            }
        }

        public string CWPBL_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwpblDesc();
            }
        }

        public string CWPBS_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwpbsDesc();
            }
        }

        public string CWPBN_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwpbnDesc();
            }
        }

        public string CWPP_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwppDesc();
            }
        }


        public string CWPTBP_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwptbpDesc();
            }
        }

        public string CBTON_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCbtonDesc();
            }
        }

        public string CBTBB_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCbtbbDesc();
            }
        }

        public string CBTTBP_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCbttbpDesc();
            }
        }

        /// <summary>
        /// Water Profile Broadband lag length.
        /// </summary>
        public double CWPBB_LagLength
        {
            get { return Predictor.CWPBB_LagLength; }
            set
            {
                Predictor.CWPBB_LagLength = value;
                this.NotifyOfPropertyChange(() => this.CWPBB_LagLength);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPBB_LagLength = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBB_LagLength = (float)value;
                UpdateCommandSet();
            }
        }


        public string CWPBB_Mode_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwpbbModeDesc();
            }
        }

        /// <summary>
        /// CEI description.
        /// </summary>
        public string CWPBB_LagLength_Desc
        {
            get
            {
                return Commands.AdcpCommands.GetCwpbbLagLengthDesc();
            }
        }

        /// <summary>
        /// List of Water Profile Broadband modes.
        /// </summary>
        public List<Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType> WPBB_List { get; set; }

        /// <summary>
        /// Water Profile Broadband pulse type.
        /// </summary>
        public Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType CWPBB_TransmitPulseType
        {
            get { return Predictor.CWPBB_TransmitPulseType; }
            set
            {
                Predictor.CWPBB_TransmitPulseType = value;
                this.NotifyOfPropertyChange(() => this.CWPBB_TransmitPulseType);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPBB_TransmitPulseType = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBB_TransmitPulseType = value;
                UpdateCommandSet();
            }
        }

        /// <summary>
        /// Water Profile number of pings.
        /// </summary>
        public ushort CWPP
        {
            get { return Predictor.CWPP; }
            set
            {
                // Check if the given value is less than 0.
                if(value <= 0)
                {
                    value = 1;
                }

                // If there is only 1 ping, then set the TBP to 0.
                if(value == 1)
                {
                    CWPTBP = 0;
                }

                Predictor.CWPP = value;
                this.NotifyOfPropertyChange(() => this.CWPP);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CWPP = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);

                // Save Configuration
                _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPP = value;
                UpdateCommandSet();
            }
        }


        #endregion

        #region Advanced User Input

        /// <summary>
        /// Cycles per Element.
        /// </summary>
        public int CyclesPerElement
        {
            get { return Predictor.CyclesPerElement; }
            set
            {
                Predictor.CyclesPerElement = value;
                this.NotifyOfPropertyChange(() => this.CyclesPerElement);
                this.NotifyOfPropertyChange(() => this.CyclesPerElementPercentBandwidth);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.CyclesPerElement = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Cycles per Element converted to percent bandwidth.
        /// </summary>
        public string CyclesPerElementPercentBandwidth
        {
            get { return (100.0/Predictor.CyclesPerElement).ToString("0.000"); }
        }

        /// <summary>
        /// Broadband power high or low.
        /// </summary>
        public bool BroadbandPower
        {
            get { return Predictor.BroadbandPower; }
            set
            {
                Predictor.BroadbandPower = value;
                this.NotifyOfPropertyChange(() => this.BroadbandPower);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.BroadbandPower = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// List of all the battery types.
        /// </summary>
        public List<DeploymentOptions.AdcpBatteryType> BatteryTypeList { get; set; }

        /// <summary>
        /// Battery Type.
        /// </summary>
        public DeploymentOptions.AdcpBatteryType BatteryType
        {
            get { return Predictor.BatteryType; }
            set
            {
                Predictor.BatteryType = value;
                this.NotifyOfPropertyChange(() => this.BatteryType);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.BatteryType = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Battery Derate.
        /// </summary>
        public double BatteryDerate
        {
            get { return Predictor.BatteryDerate; }
            set
            {
                Predictor.BatteryDerate = value;
                this.NotifyOfPropertyChange(() => this.BatteryDerate);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.BatteryDerate = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Battery Self Discharge per Year.
        /// </summary>
        public double BatterySelfDischargePerYear
        {
            get { return Predictor.BatterySelfDischargePerYear; }
            set
            {
                Predictor.BatterySelfDischargePerYear = value;
                this.NotifyOfPropertyChange(() => this.BatterySelfDischargePerYear);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.BatterySelfDischargePerYear = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Speed of Sound.
        /// </summary>
        public double SpeedOfSound
        {
            get { return Predictor.SpeedOfSound; }
            set
            {
                Predictor.SpeedOfSound = value;
                this.NotifyOfPropertyChange(() => this.SpeedOfSound);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SpeedOfSound = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Beam Angle.
        /// </summary>
        public double BeamAngle
        {
            get { return Predictor.BeamAngle; }
            set
            {
                Predictor.BeamAngle = value;
                this.NotifyOfPropertyChange(() => this.BeamAngle);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.BeamAngle = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Beam Diameter.
        /// </summary>
        public double BeamDiameter
        {
            get { return Predictor.BeamDiameter; }
            set
            {
                Predictor.BeamDiameter = value;
                this.NotifyOfPropertyChange(() => this.BeamDiameter);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.BeamDiameter = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System Boot power in watts.
        /// </summary>
        public double SystemBootPower
        {
            get { return Predictor.SystemBootPower; }
            set
            {
                Predictor.SystemBootPower = value;
                this.NotifyOfPropertyChange(() => this.SystemBootPower);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemBootPower = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System Init power in watts.
        /// </summary>
        public double SystemInitPower
        {
            get { return Predictor.SystemInitPower; }
            set
            {
                Predictor.SystemInitPower = value;
                this.NotifyOfPropertyChange(() => this.SystemInitPower);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemInitPower = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System Receive power in watts.
        /// </summary>
        public double SystemRcvPower
        {
            get { return Predictor.SystemRcvPower; }
            set
            {
                Predictor.SystemRcvPower = value;
                this.NotifyOfPropertyChange(() => this.SystemRcvPower);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemRcvPower = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System Save power in watts.
        /// </summary>
        public double SystemSavePower
        {
            get { return Predictor.SystemSavePower; }
            set
            {
                Predictor.SystemSavePower = value;
                this.NotifyOfPropertyChange(() => this.SystemSavePower);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemSavePower = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System sleep power in watts.
        /// </summary>
        public double SystemSleepPower
        {
            get { return Predictor.SystemSleepPower; }
            set
            {
                Predictor.SystemSleepPower = value;
                this.NotifyOfPropertyChange(() => this.SystemSleepPower);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemSleepPower = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System wakeup time in seconds.
        /// </summary>
        public double SystemWakeupTime
        {
            get { return Predictor.SystemWakeupTime; }
            set
            {
                Predictor.SystemWakeupTime = value;
                this.NotifyOfPropertyChange(() => this.SystemWakeupTime);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemWakeupTime = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System Init time in seconds.
        /// </summary>
        public double SystemInitTime
        {
            get { return Predictor.SystemInitTime; }
            set
            {
                Predictor.SystemInitTime = value;
                this.NotifyOfPropertyChange(() => this.SystemInitTime);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemInitTime = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// System Save time in seconds.
        /// </summary>
        public double SystemSaveTime
        {
            get { return Predictor.SystemSaveTime; }
            set
            {
                Predictor.SystemSaveTime = value;
                this.NotifyOfPropertyChange(() => this.SystemSaveTime);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SystemSaveTime = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Beta value.  Environmental decorrelation.
        /// </summary>
        public double Beta
        {
            get { return Predictor.Beta; }
            set
            {
                Predictor.Beta = value;
                this.NotifyOfPropertyChange(() => this.Beta);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.Beta = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Signal to Ratio in dB.  Used for Standard Deviation calculation.
        /// </summary>
        public double SNR
        {
            get { return Predictor.SNR; }
            set
            {
                Predictor.SNR = value;
                this.NotifyOfPropertyChange(() => this.SNR);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.SNR = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        /// <summary>
        /// Number of beams in the ADCP.
        /// </summary>
        public int Beams
        {
            get { return Predictor.Beams; }
            set
            {
                Predictor.Beams = value;
                this.NotifyOfPropertyChange(() => this.Beams);

                // Update the Results Properties
                NotifyResultsProperties();

                // Save the input
                _UserInput.Beams = value;
                _pm.UpdateAdcpPredictorUserInput(_UserInput);
            }
        }

        #endregion

        #region Command Set


        /// <summary>
        /// Command set file path.
        /// </summary>
        private string _CommandSetFilePath;
        /// <summary>
        /// Command set file path.
        /// </summary>
        public string CommandSetFilePath
        {
            get { return _CommandSetFilePath; }
            set
            {
                _CommandSetFilePath = value;
                this.NotifyOfPropertyChange(() => this.CommandSetFilePath);
            }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Set the default values for the selected subsystem.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SetDefaultCommand { get; protected set; }

        /// <summary>
        /// Clear the command set.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> ClearCommandSetCommand { get; protected set; }

        /// <summary>
        /// Save the command set.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> SaveCommandSetCommand { get; protected set; }

        /// <summary>
        /// Command to import a command set.
        /// </summary>
        
        public ReactiveCommand<object> ImportCommandSetCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        
        public AdcpPredictionModelViewModel()
            : base("Prediction Model")
        {
            // Initialize the values
            _pm = IoC.Get<PulseManager>();
            _UserInput = _pm.GetAdcpPredictorUserInput();
            Predictor = new AdcpPredictor(_UserInput);

            // Populate the subsystem list
            PopulateLists();

            // Add Subsystem to configuration and
            // Setup ADCP Command set
            AddSubsystemConfig();
            UpdateCommandSet();                                                 

            //this.NotifyOfPropertyChange(null);

            SetDefaultCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => SetDefaults()));

            ClearCommandSetCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => ClearCommandSet()));

            // Save the command set to a file
            SaveCommandSetCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => SaveCommandSet()));

            // Import an ADCP command set
            ImportCommandSetCommand = ReactiveCommand.Create();
            ImportCommandSetCommand.Subscribe(_ => ImportCommandSet());
        }

        /// <summary>
        /// Shutdown the view model.
        /// </summary>
        public override void Dispose()
        {

        }


        #region Subsystem

        /// <summary>
        /// Create a list of all the subsystem types.
        /// Then select the first first subsystem.
        /// </summary>
        private void PopulateLists()
        {

          
            try
            {
                _adcpConfig.Commands.CEI.Second = _adcpConfig.Commands.CEI.Second;
            }
            catch (Exception e)
            {
            }
            // Create the list
            ListOfSubsystems = new SubsystemList();

           // Set selected subsystem
            if (ListOfSubsystems.Count > 0)
            {
                foreach (var ss in ListOfSubsystems)
                {
                    if (ss.Code == _UserInput.SubSystem.Code)
                    {
                        SelectedSubsystemss = ss;
                    }
                }
            }

            BTBB_List = new List<Commands.AdcpSubsystemCommands.eCBTBB_Mode>();
            BTBB_List.Add(Commands.AdcpSubsystemCommands.eCBTBB_Mode.BROADBAND_CODED);
            BTBB_List.Add(Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE);

            WPBB_List = new List<Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType>();
            WPBB_List.Add(Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.BROADBAND);
            WPBB_List.Add(Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND);
            WPBB_List.Add(Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.BROADBAND_PULSE_TO_PULSE);
            WPBB_List.Add(Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NONCODED_BROADBAND_PULSE_TO_PULSE);

            BatteryTypeList = DeploymentOptions.GetBatteryList();
        }

        /// <summary>
        /// Set the new subsystem based off the code given.
        /// </summary>
        /// <param name="code">Subsystem code.</param>
        private void SetSubsystem(byte code)
        {
            // Create the input
            //AdcpPredictorUserInput input = new AdcpPredictorUserInput(new Subsystem(code));
            _UserInput.SubSystem = new Subsystem(code);

            // Create a new predictor
            Predictor = new AdcpPredictor(_UserInput);

            // Add Subsystem to configuration
            AddSubsystemConfig();

            // Update all the properties
            NotifyResultsProperties();
        }

        #endregion

        #region Set Defaults

        /// <summary>
        /// Use the current subsystem to set the default values.
        /// </summary>
        private void SetDefaults()
        {
            // Set the new User Input and new predictor
            _UserInput = new AdcpPredictorUserInput(_UserInput.SubSystem);

            Predictor = new AdcpPredictor(_UserInput);

            // Save the new input
            _pm.UpdateAdcpPredictorUserInput(_UserInput);

            // Set defaults to ADCP Config
            SetAdcpDefaults();

            UpdateCommandSet();

            // Update all the properties
            NotifyResultsProperties();
        }

        /// <summary>
        /// Set the default settings for the ADCP configuration.
        /// </summary>
        private void SetAdcpDefaults()
        {
            // Set all the values to the prediction input
            AddSubsystemConfig();
            _adcpConfig.Commands.CEI = new Commands.TimeValue((float)_UserInput.CEI);
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPON = _UserInput.CWPON;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTON = _UserInput.CBTON;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTTBP = _UserInput.CBTTBP;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTBB_Mode = _UserInput.CBTBB_TransmitPulseType;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBL = _UserInput.CWPBL;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBN = _UserInput.CWPBN;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBS = _UserInput.CWPBS;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBB_LagLength = (float)_UserInput.CWPBB_LagLength;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBB_TransmitPulseType = _UserInput.CWPBB_TransmitPulseType;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPP = _UserInput.CWPP;
            _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPTBP = _UserInput.CWPTBP;
        }

        #endregion

        #region Update Properties

        /// <summary>
        /// Notify all the Results properties of a change.
        /// </summary>
        private void NotifyResultsProperties()
        {
            // Update all the properties
            this.NotifyOfPropertyChange(null);

            this.NotifyOfPropertyChange(() => this.PredictedBottomRange);
            this.NotifyOfPropertyChange(() => this.PredictedProfileRange);
            this.NotifyOfPropertyChange(() => this.MaximumVelocity);
            this.NotifyOfPropertyChange(() => this.StandardDeviation);
            this.NotifyOfPropertyChange(() => this.ProfileFirstBinPosition);
            this.NotifyOfPropertyChange(() => this.NumberBatteryPacks);
            this.NotifyOfPropertyChange(() => this.DataSize);
            this.NotifyOfPropertyChange(() => this.WattHours);

            this.NotifyOfPropertyChange(() => this.DeploymentDuration);
            this.NotifyOfPropertyChange(() => this.CEI);
            this.NotifyOfPropertyChange(() => this.CWPON);
            this.NotifyOfPropertyChange(() => this.CBTON);
            this.NotifyOfPropertyChange(() => this.CBTTBP);
            this.NotifyOfPropertyChange(() => this.CBTBB_TransmitPulseType);
            this.NotifyOfPropertyChange(() => this.CWPTBP);
            this.NotifyOfPropertyChange(() => this.CWPBB_LagLength);
            this.NotifyOfPropertyChange(() => this.CWPBN);
            this.NotifyOfPropertyChange(() => this.CWPBS);
            this.NotifyOfPropertyChange(() => this.CWPBL);
            this.NotifyOfPropertyChange(() => this.CWPBB_TransmitPulseType);
            this.NotifyOfPropertyChange(() => this.CWPP);

            //this.NotifyOfPropertyChange(() => this.CyclesPerElement);
            //this.NotifyOfPropertyChange(() => this.CyclesPerElementPercentBandwidth);
            //this.NotifyOfPropertyChange(() => this.BroadbandPower);
            //this.NotifyOfPropertyChange(() => this.BatteryType);
            //this.NotifyOfPropertyChange(() => this.BatteryDerate);
            //this.NotifyOfPropertyChange(() => this.BatterySelfDischargePerYear);
            //this.NotifyOfPropertyChange(() => this.SpeedOfSound);
            //this.NotifyOfPropertyChange(() => this.BeamAngle);
            //this.NotifyOfPropertyChange(() => this.BeamDiameter);
            //this.NotifyOfPropertyChange(() => this.SystemBootPower);
            //this.NotifyOfPropertyChange(() => this.SystemInitPower);
            //this.NotifyOfPropertyChange(() => this.SystemRcvPower);
            //this.NotifyOfPropertyChange(() => this.SystemSavePower);
            //this.NotifyOfPropertyChange(() => this.SystemSleepPower);
            //this.NotifyOfPropertyChange(() => this.SystemWakeupTime);
            //this.NotifyOfPropertyChange(() => this.SystemInitTime);
            //this.NotifyOfPropertyChange(() => this.SystemSaveTime);
            //this.NotifyOfPropertyChange(() => this.Beta);
            //this.NotifyOfPropertyChange(() => this.SNR);
            //this.NotifyOfPropertyChange(() => this.Beams);
            //PredictionCommands();
        }

        #endregion

        #region Command Set

        /// <summary>
        /// Add a new subsystem configuration to the ADCP Configuration.
        /// This is used to keep track of the commands.
        /// </summary>
        private void AddSubsystemConfig()
        {
            // Create an ADCP Config
            _adcpConfig = new AdcpConfiguration();                              // Create default configuration
            _adcpConfig.SerialNumber = new SerialNumber();                      // Add Serial Number
            _adcpConfig.SerialNumber.AddSubsystem(_UserInput.SubSystem);        // Add Subsystem to serial number to pass validation
            AdcpSubsystemConfig ss = null;
            _adcpConfig.AddConfiguration(_UserInput.SubSystem, out ss);         // Add Subsystem to configuration
        }

        /// <summary>
        /// Get the command set from the configuration created.
        /// </summary>
        /// <returns>List of all the commands.</returns>
        private List<string> GetCommandSet()
        {
            List<string> commands = new List<string>();
            commands.Add(RTI.Commands.AdcpCommands.CMD_CDEFAULT);
            if (_adcpConfig != null)
            {
                // Add the system commands
                commands.AddRange(_adcpConfig.Commands.GetPredictionCommandList());

                // Add the subsystem commands
                foreach (var subConfig in _adcpConfig.SubsystemConfigDict.Values)
                {
                    commands.AddRange(subConfig.Commands.GetPredictionCommandList());
                }
            }

            // Add the CSAVE command
           // commands.Add(RTI.Commands.AdcpCommands.CMD_CSAVE);

            return commands;
        }

        /// <summary>
        /// Update the command set with the latest information.
        /// </summary>
        public void UpdateCommandSet()

        {
            //AdcpCommandSet = "CDEFAULT\nCEI[0] " + _adcpConfig.Commands.CEI + "\nCEPO[0] " + _adcpConfig.SubsystemConfigDict.First().Value.SubsystemConfig.SubSystem.ToString().Substring(2) + "\n" + _adcpConfig.SubsystemConfigDict.First().Value.Commands.ToString() + "CSAVE";
            // Get the command set from the configuration
            StringBuilder sb = new StringBuilder();
            List<string> commands = GetCommandSet();
            try
            {
                List<string> splitter = secondaryAdcpCommandSet.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> spaceSplitter = secondaryAdcpCommandSet.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < splitter.Count; i++)
                {
                    for (int k = 0; k < commands.Count; k++)
                        if (splitter.Count != 0)
                        {
                            if (splitter[i].Contains(commands[k].Substring(0, 3)))
                            {
                                splitter.Remove(splitter[i]);
                            }
                        }
                }
                foreach (string s in splitter)
                {
                    sb.AppendLine(s);

                }
                secondaryAdcpCommandSet = sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
            // Update the string
            UpdateCommandSetStr(commands);
        }

        /// <summary>
        /// Create a string of all the commands.
        /// </summary>
        /// <param name="commands">Commands to create the string.</param>
        private void UpdateCommandSetStr(List<string> commands)
        {
            //Go through all the commands
           StringBuilder sb = new StringBuilder();
            foreach (var cmd in commands)
            {
                sb.AppendLine(cmd);
            }

            // Update the string
            AdcpCommandSet = sb.ToString();
        }

        #endregion


        #region Command File

        /// <summary>
        /// Save the command set to a file.
        /// </summary>
        private void SaveCommandSet()
        {
            try
            {
                // Get the project dir
                // Create the file name
                string prjDir = @"c:\RTI_Configuration_Files";
                System.IO.Directory.CreateDirectory(prjDir);

                DateTime now = DateTime.Now;
                string year = now.Year.ToString("0000");
                string month = now.Month.ToString("00");
                string day = now.Day.ToString("00");
                string hours = now.Hour.ToString("00");
                string minutes = now.Minute.ToString("00");
                string seconds = now.Second.ToString("00");
                string fileName = string.Format("Commands_{0}{1}{2}{3}{4}{5}.txt", year, month, day, hours, minutes, seconds);
                string cmdFilePath = prjDir + @"\" + fileName;

                // Get the commands
                string[] lines = GetCommandSet().ToArray();

                // Create a text file in the project
                System.IO.File.WriteAllLines(cmdFilePath, lines);

                CommandSetFilePath = "File saved to: " + cmdFilePath;
            }
            catch (Exception e) 
            {
                log.Error("Error writing configuration file.", e);
            }
        }

        /// <summary>
        /// Import a command set from a file.
        /// </summary>
      
        private void ImportCommandSet()
        {
           
            string fileName = "";
            try
            {
                // Show the FolderBrowserDialog.
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "All files (*.*)|*.*";
                dialog.Multiselect = false;

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Get the files selected
                    fileName = dialog.FileName;

                    new Task(() => ImportCommandSetAsync(fileName)).Start();
                }
                // Check for CEPO command
                if (!AdcpCommandSet.Contains("CEPO"))
                {
                    AdcpCommandSet = "CEPO " + (SelectedSubsystem + "").Substring(0, 1) + "\n" + AdcpCommandSet;
                    System.Windows.Forms.MessageBox.Show("No Subsystem Detected, using Subsystem " + (SelectedSubsystem + "").Substring(0, 1) + ".\n Please set a Subsystem type.");
                }
            }
            catch (Exception e)//$exception	{"Current thread must be set to single thread apartment (STA) mode before OLE calls can be made. Ensure that your Main function has STAThreadAttribute marked on it. This exception is only raised if a debugger is attached to the process."}	System.Threading.ThreadStateException

            {
                Error();
                log.Error(string.Format("Error reading command set from {0}", fileName), e);
            }
        }

        private void ImportCommandSetAsync(string fileName)
        {
            try
            {
                // Set the command set
                AdcpCommandSet = File.ReadAllText(fileName);
                int subSystemAmount = 0;
                
                string[] results = AdcpCommandSet.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < results.Length; x++)
                {

                    //To Noah, this code won't work, we tried making a general case for the commands, but we cant check if the command only has letters because some commands have numbers in their names.
                    //if (results[x].IndexOf("[") != -1 && results[x].IndexOf("]") != -1)
                    //{
                    //    int cmdGood = results[x].IndexOf("[") - 1;
                    //    for (int i = 0; i < results[x].Substring(0, results[x].IndexOf("[")).Length; i++)
                    //    {
                    //        if (!char.IsDigit(results[x][i]))
                    //        {
                    //            string srt = results[x].Substring(0, results[x].IndexOf("[") - 1);
                    //            cmdGood--;
                    //        }
                    //        else
                    //        {
                    //            Error(results[x], "You have an error with your command name.");
                    //        }
                    //    }
                    //    if (cmdGood == 0)
                    //    {
                    //        string subsystemNumber = results[x].Substring(results[x].IndexOf("[") + 1, results[x].IndexOf("]") - results[x].IndexOf("[") - 1);
                    //        // cmdGood = subsystemNumber/*results[x].Substring(results[x].IndexOf("[") + 1, results[x].IndexOf("]") - results[x].IndexOf("[") - 1)*/.Length;
                    //        for (int i = 0; i < subsystemNumber/*results[x].Substring(results[x].IndexOf("[") + 1, results[x].IndexOf("]") - results[x].IndexOf("[") - 1)*/.Length; i++)
                    //        {
                    //            char charmander = results[x][results[x].IndexOf("[") + i + 1];
                    //            if (char.IsDigit(charmander/*results[x][results[x].IndexOf("[") + i + 1]*/))
                    //            {
                    //                Error(results[x], "You have an error with your brackets.");

                    //            }
                    //        }
                    //    }
                    //}
                    //else if (results[x].IndexOf("[") != -1 && results[x].IndexOf("[") == -1) Error(results[x], "Command does not properly use brackets."); //if the command has one bracket, but not the other, throw an error.
                    //else if (results[x].IndexOf("[") == -1 && results[x].IndexOf("]") != -1) Error(results[x], "Command does not properly use brackets."); //if the command has one bracket, but not the other, throw an error.
                   
                    try
                    {
                        if (results[x].Contains("CEPO")) //Gets the amount of subsystems CEPO has
                        {

                            string[] str1 = results[x].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            subSystemAmount= str1[1].Length;

                        }
                        if (!results[x].Contains("CEPO"))//  Checks if the command's subsystem is within the number of subsystems CEPO has
                        {
                            bool works = false;
                            for (int i = 0; i < subSystemAmount; i++)
                            {
                                if (results[x].Substring(results[x].IndexOf("[") + 1, results[x].IndexOf("]") - results[x].IndexOf("[") - 1).Equals(i+""))
                                    works = true;
                                
                            }

                            if (!works) Error(results[x], "Command does not reference correct subsystem.");

                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Error importing file in prediction model", e);
                    }

                    string[] bracketCheck = results[x].Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                    if (bracketCheck.Length > 2)
                        Error(results[x], "Each subsystem must have its own line for a command.");

                    if (results[x].IndexOf("]") == results[x].Length)
                    {
                        Error(results[x]);
                    }
                   
                    if (results[x].Substring(0, 1).Contains(" ") || results[x].Contains("  "))
                    {

                        while (results[x].Contains("  "))// check for any spacing errors and remove double spaces.
                        {
                            results[x] = results[x].Remove(results[x].IndexOf("  "), 1);

                        }
                        Error(results[x], "Your spacing is incorrect.");
                    }
                   

                    if ((results[x].Contains("[") && !results[x].Contains("]")) || (!results[x].Contains("[") && results[x].Contains("]")))//check if command has one bracket, but not the other
                    {
                        Error(results[x], "Command does not properly use brackets.");
                    }
                    if (results[x].Contains("[]"))//check if command is missing subsystem
                    {
                        Error(results[x], "Command does not specify subsystem.");
                    }

                    if (results[x].Substring(0, 1).Contains("["))//check if a command has a subsystem bracket as the first line, and throw an error.
                    {
                        Error(results[x]);
                       
                    }
                    try
                    {

                        int ind1 = results[x].IndexOf('[');
                        if (results[x].Substring(0, ind1).Contains(" "))//check if command has no command attached
                        {
                            Error(results[x]);
                        }
                        int ind2 = results[x].IndexOf(']');
                        if (!results[x].Substring(ind2, 2).Contains(' ')) Error(results[x], "Your spacing is incorrect");
                      
                    }
                    catch (Exception e)
                    {
                      
                    }
                }
                AdcpCommandSet = "";
                for (int i = 0; i < results.Length; i++)
                {
                    AdcpCommandSet += results[i] + "\r\n";
                }


                // Decode the command set to apply to the configuration
                //AdcpConfig = RTI.Commands.AdcpCommands.DecodeCSHOW(AdcpCommandSet, new SerialNumber());
                _adcpConfig = RTI.Commands.AdcpCommands.DecodeCommandSet(AdcpCommandSet, new SerialNumber());
                secondaryAdcpCommandSet = AdcpCommandSet;
                // Setup the configuraion
                SetupConfiguration();
            }
            catch (Exception e)
            {

               
            }
        }

        /// <summary>
        /// Throw error message to user due to incorect imported commands.
        /// </summary>

        private void Error()
        {

            System.Windows.Forms.MessageBox.Show("An error occurred in the command set.");
        }
        private void Error(string Command, string error)
        {
            System.Windows.Forms.MessageBox.Show("An error was found in the command set.\nYour " + Command+ " command has an error. "+error);
        }
        private void Error(string Command)
        {
            System.Windows.Forms.MessageBox.Show("An error was found in the command set.\nYour " + Command + " command has an error.");
        }


        /// <summary>
        /// Populate the prediction model with the command set given by the user.
        /// </summary>
        private void SetupConfiguration()
        {
            // Set the subsystem
            _UserInput.SubSystem = _adcpConfig.SubsystemConfigDict.First().Value.SubsystemConfig.SubSystem;
            
            // Set all the values to the prediction input
            PopulateLists();
            CEI = _adcpConfig.Commands.CEI.Second;
            CWPP = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPP;
            CWPON = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPON;
            CBTON = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTON;
            CBTTBP = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTTBP;
            CBTBB_TransmitPulseType = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CBTBB_Mode;
            CWPBL = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBL;
            CWPBN = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBN;
            CWPBS = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBS;
            CWPBB_LagLength = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBB_LagLength;
            CWPBB_TransmitPulseType = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPBB_TransmitPulseType;
            CWPTBP = _adcpConfig.SubsystemConfigDict.First().Value.Commands.CWPTBP;
            NotifyResultsProperties();
        }

        #endregion
    }
}
