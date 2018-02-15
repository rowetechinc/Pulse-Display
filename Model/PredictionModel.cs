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
 * 10/18/2017      RC          4.5.0      Fix bug in GetBurstDataStorage() when calculating number of burst per deployment.
 * 10/23/2017      RC          4.5.0      fix bug in CalculatePowerBurst() when calculating number of burst per deployment.
 * 01/12/2018      RC          4.7.0      Added absorption values.
 * 01/16/2018      RC          4.7.1      Updated table for Range and Absorption scale factor.
 * 02/15/2018      RC          4.7.3      Calculate power differently for a burst.
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Predicition model engineering values.  These are values that  
    /// the customer would not know to enter in.  They are based off 
    /// testing and other models.
    /// </summary>
    public class PredictionEngValues
    {
        #region Properties

        /// <summary>
        /// Beam angle in degrees.
        /// </summary>
        public double BeamAngle { get; set; }
                
        /// <summary>
        /// Speed of Sound in m/s.
        /// </summary>
        public double SpeedOfSound { get; set; }
                
        /// <summary>
        /// System boot power in watts.
        /// </summary>
        public double SystemBootPower { get; set; }

        /// <summary>
        /// System Wakeup time in seconds.
        /// </summary>
        public double SystemWakeupTime { get; set; }

        /// <summary>
        /// System init power in watts.
        /// </summary>
        public double SystemInitPower { get; set; }

        /// <summary>
        /// System init time in seconds.
        /// </summary>
        public double SystemInitTime { get; set; }
        
        /// <summary>
        /// Flag if using broadband high power.
        /// </summary>
        public bool BroadbandPower { get; set; }
            
        /// <summary>
        /// System save power in watts.
        /// </summary>
        public double SystemSavePower { get; set; }

        /// <summary>
        /// System save time in seconds.
        /// </summary>
        public double SystemSaveTime { get; set; }

        /// <summary>
        /// System sleep power in watt.
        /// </summary>
        public double SystemSleepPower { get; set; }

        /// <summary>
        /// Beam diameter in meters.
        /// </summary>
        public double BeamDiameter { get; set; }

        /// <summary>
        /// Cycles per element.
        /// </summary>
        public double CyclesPerElement { get; set; }

        /// <summary>
        /// Signal to Noise ratio in dB.
        /// </summary>
        public double SNR { get; set; }
        
        /// <summary>
        /// Environmental decorrelation value.
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// Narrowband fudge number.
        /// </summary>
        public double NbFudge { get; set; }

        /// <summary>
        /// Battery Derate value.
        /// </summary>
        public double BatteryDerate { get; set; }

        /// <summary>
        /// Battery Self Discharge per year.
        /// </summary>
        public double BatterySelfDischarge { get; set; }

        #endregion

        /// <summary>
        /// Initialize the predicition engineering values.
        /// </summary>
        public PredictionEngValues()
        {
            Init();
        }

        /// <summary>
        /// Initialize the values with default values.
        /// </summary>
        public void Init()
        {
            BeamAngle = 20;
            SpeedOfSound = 1490;
            SystemBootPower = 1.80;
            SystemWakeupTime = 0.40;
            SystemInitPower = 2.80;
            SystemInitTime = 0.25;
            BroadbandPower = true;
            SystemSavePower = 1.80;
            SystemSaveTime = 0.15;
            SystemSleepPower = 0.024;
            BeamDiameter = 0.075;
            CyclesPerElement = 12;
            SNR = 30.00;
            Beta = 1.0;
            NbFudge = 1.4;
            BatteryDerate = 0.85;
            BatterySelfDischarge = 0.5;
        }
    }

    /// <summary>
    /// ADCP Predicition model.  Used to calculate the power usage,
    /// the data usage, the STD, the maximum velocity and the start
    /// of the first bin.
    /// </summary>
    public class PredictionModel
    {
        #region Variables

        #region Defaults

        #region 1200000 Table

        /// <summary>
        /// Default frequency for the 1200000 table.
        /// </summary>
        private double DEFAULT_FREQ_1200000 = 1100000;

        /// <summary>
        /// Default uF for the 1200000 table.
        /// </summary>
        private double DEFAULT_UF_1200000 = 11000;

        /// <summary>
        /// Default transmit voltage for 1200000 table.
        /// </summary>
        private int DEFAULT_XMIT_V_1200000 = 24;

        /// <summary>
        /// Default number of bins for 1200000 table.
        /// </summary>
        private int DEFAULT_BIN_1200000 = 1;

        /// <summary>
        /// Default range for 1200000 table.
        /// </summary>
        private int DEFAULT_RANGE_1200000 = 18;

        /// <summary>
        /// Absorption Scale for 1200000 table.
        /// </summary>
        private double ABSORPTION_SCALE_1200000 = 0.55;

        /// <summary>
        /// Default transmit wattage for 1200000 table.
        /// </summary>
        private int DEFAULT_XMIT_W_1200000 = 5;

        /// <summary>
        /// Default beam angle for 1200000 table.
        /// </summary>
        private int DEFAULT_BEAM_ANGLE_1200000 = 20;

        /// <summary>
        /// Default beam Diameter for 1200000 table.
        /// </summary>
        private double DEFAULT_DIAM_1200000 = 0.0508;

        /// <summary>
        /// Default Sampling for 1200000 table.
        /// </summary>
        private double DEFAULT_SAMPLING_1200000 = 4.0 / 3.0 / 16.0;

        /// <summary>
        /// Default CPE for 1200000 table.
        /// </summary>
        private int DEFAULT_CPE_1200000 = 12;

        #endregion

        #region 600000 Table

        /// <summary>
        /// Default frequency for the 600000 table.
        /// </summary>
        private double DEFAULT_FREQ_600000 = 550000;

        /// <summary>
        /// Default uF for the 600000 table.
        /// </summary>
        private double DEFAULT_UF_600000 = 22000;

        /// <summary>
        /// Default transmit voltage for 600000 table.
        /// </summary>
        private int DEFAULT_XMIT_V_600000 = 18;

        /// <summary>
        /// Default number of bins for 600000 table.
        /// </summary>
        private int DEFAULT_BIN_600000 = 2;

        /// <summary>
        /// Default range for 600000 table.
        /// </summary>
        private int DEFAULT_RANGE_600000 = 50;

        /// <summary>
        /// Absorption Scale for 600000 table.
        /// </summary>
        private double ABSORPTION_SCALE_600000 = 0.17;

        /// <summary>
        /// Default transmit wattage for 600000 table.
        /// </summary>
        private int DEFAULT_XMIT_W_600000 = 15;

        /// <summary>
        /// Default beam angle for 600000 table.
        /// </summary>
        private int DEFAULT_BEAM_ANGLE_600000 = 20;

        /// <summary>
        /// Default beam Diameter for 60000 table.
        /// </summary>
        private double DEFAULT_DIAM_600000 = 0.0762;

        /// <summary>
        /// Default Sampling for 600000 table.
        /// </summary>
        private double DEFAULT_SAMPLING_600000 = 4.0 / 3.0 / 16.0;

        /// <summary>
        /// Default CPE for 600000 table.
        /// </summary>
        private int DEFAULT_CPE_600000 = 12;

        #endregion

        #region 300000 Table

        /// <summary>
        /// Default frequency for the 300000 table.
        /// </summary>
        private double DEFAULT_FREQ_300000 = 275000;

        /// <summary>
        /// Default uF for the 300000 table.
        /// </summary>
        private double DEFAULT_UF_300000 = 44000;

        /// <summary>
        /// Default transmit voltage for 300000 table.
        /// </summary>
        private int DEFAULT_XMIT_V_300000 = 24;

        /// <summary>
        /// Default number of bins for 300000 table.
        /// </summary>
        private int DEFAULT_BIN_300000 = 4;

        /// <summary>
        /// Default range for 300000 table.
        /// </summary>
        private double DEFAULT_RANGE_300000 = 100.0;

        /// <summary>
        /// Absorption Scale for 300000 table.
        /// </summary>
        private double ABSORPTION_SCALE_300000 = 0.073;

        /// <summary>
        /// Default transmit wattage for 300000 table.
        /// </summary>
        private int DEFAULT_XMIT_W_300000 = 50;

        /// <summary>
        /// Default beam angle for 300000 table.
        /// </summary>
        private int DEFAULT_BEAM_ANGLE_300000 = 20;

        /// <summary>
        /// Default beam Diameter for 30000 table.
        /// </summary>
        private double DEFAULT_DIAM_300000 = 0.0762;

        /// <summary>
        /// Default Sampling for 300000 table.
        /// </summary>
        private double DEFAULT_SAMPLING_300000 = 4.0 / 3.0 / 16.0;

        /// <summary>
        /// Default CPE for 300000 table.
        /// </summary>
        private int DEFAULT_CPE_300000 = 12;

        #endregion

        #region 150000 Table

        /// <summary>
        /// Default frequency for the 150000 table.
        /// </summary>
        private double DEFAULT_FREQ_150000 = 137500;

        /// <summary>
        /// Default uF for the 150000 table.
        /// </summary>
        private double DEFAULT_UF_150000 = 16000;

        /// <summary>
        /// Default transmit voltage for 150000 table.
        /// </summary>
        private int DEFAULT_XMIT_V_150000 = 28;

        /// <summary>
        /// Default number of bins for 150000 table.
        /// </summary>
        private int DEFAULT_BIN_150000 = 8;

        /// <summary>
        /// Default range for 150000 table.
        /// </summary>
        private int DEFAULT_RANGE_150000 = 240;

        /// <summary>
        /// Absorption Scale for 150000 table.
        /// </summary>
        private double ABSORPTION_SCALE_150000 = 0.045;

        /// <summary>
        /// Default transmit wattage for 150000 table.
        /// </summary>
        private int DEFAULT_XMIT_W_150000 = 250;

        /// <summary>
        /// Default beam angle for 150000 table.
        /// </summary>
        private int DEFAULT_BEAM_ANGLE_150000 = 30;

        /// <summary>
        /// Default beam Diameter for 15000 table.
        /// </summary>
        private double DEFAULT_DIAM_150000 = 0.1524;

        /// <summary>
        /// Default Sampling for 150000 table.
        /// </summary>
        private double DEFAULT_SAMPLING_150000 = 4.0 / 5.0 / 16.0;

        /// <summary>
        /// Default CPE for 150000 table.
        /// </summary>
        private int DEFAULT_CPE_150000 = 20;

        #endregion

        #region 75000 Table

        /// <summary>
        /// Default frequency for the 75000 table.
        /// </summary>
        private double DEFAULT_FREQ_75000 = 68750;

        /// <summary>
        /// Default uF for the 75000 table.
        /// </summary>
        private double DEFAULT_UF_75000 = 16000;

        /// <summary>
        /// Default transmit voltage for 75000 table.
        /// </summary>
        private int DEFAULT_XMIT_V_75000 = 28;

        /// <summary>
        /// Default number of bins for 75000 table.
        /// </summary>
        private int DEFAULT_BIN_75000 = 16;

        /// <summary>
        /// Default range for 75000 table.
        /// </summary>
        private int DEFAULT_RANGE_75000 = 620;

        /// <summary>
        /// Absorption Scale for 75000 table.
        /// </summary>
        private double ABSORPTION_SCALE_75000 = 0.025;

        /// <summary>
        /// Default transmit wattage for 75000 table.
        /// </summary>
        private int DEFAULT_XMIT_W_75000 = 250;

        /// <summary>
        /// Default beam angle for 75000 table.
        /// </summary>
        private int DEFAULT_BEAM_ANGLE_75000 = 30;

        /// <summary>
        /// Default beam Diameter for 75000 table.
        /// </summary>
        private double DEFAULT_DIAM_75000 = 0.254;

        /// <summary>
        /// Default Sampling for 75000 table.
        /// </summary>
        private double DEFAULT_SAMPLING_75000 = 4.0 / 5.0 / 16.0;

        /// <summary>
        /// Default CPE for 75000 table.
        /// </summary>
        private int DEFAULT_CPE_75000 = 20;

        #endregion

        #region 38000 Table

        /// <summary>
        /// Default frequency for the 38000 table.
        /// </summary>
        private double DEFAULT_FREQ_38000 = 34375;

        /// <summary>
        /// Default uF for the 38000 table.
        /// </summary>
        private double DEFAULT_UF_38000 = 16000;

        /// <summary>
        /// Default transmit voltage for 38000 table.
        /// </summary>
        private int DEFAULT_XMIT_V_38000 = 28;

        /// <summary>
        /// Default number of bins for 38000 table.
        /// </summary>
        private int DEFAULT_BIN_38000 = 32;

        /// <summary>
        /// Default range for 38000 table.
        /// </summary>
        private int DEFAULT_RANGE_38000 = 1000;

        /// <summary>
        /// Absorption Scale for 38000 table.
        /// </summary>
        private double ABSORPTION_SCALE_38000 = 0.01;

        /// <summary>
        /// Default transmit wattage for 38000 table.
        /// </summary>
        private int DEFAULT_XMIT_W_38000 = 250;

        /// <summary>
        /// Default beam angle for 38000 table.
        /// </summary>
        private int DEFAULT_BEAM_ANGLE_38000 = 30;

        /// <summary>
        /// Default beam Diameter for 38000 table.
        /// </summary>
        private double DEFAULT_DIAM_38000 = 0.6096;

        /// <summary>
        /// Default Sampling for 38000 table.
        /// </summary>
        private double DEFAULT_SAMPLING_38000 = 4.0 / 5.0 / 16.0;

        /// <summary>
        /// Default CPE for 38000 table.
        /// </summary>
        private int DEFAULT_CPE_38000 = 20;

        #endregion

        /// <summary>
        /// Default frequency for the 20000 table.
        /// </summary>
        private double DEFAULT_FREQ_20000 = 20000;

        #endregion

        #endregion

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public PredictionModel()
        {

        }

        #region Calculate Power

        /// <summary>
        /// Calculate the power based off the input values.
        /// 
        /// If this is burst calculation, set _IsBurst_ to true.  Then give the number of ensembles in the burst for _EnsemblesInBurst_.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Power calculationed for the input in watts.</returns>
        public double CalculatePower(PredictionModelInput input)
        {
            return CalculatePower(input.CEI, input.DeploymentDuration, input.Beams, input.SystemFrequency,
                input.CWPON, input.CWPBL, input.CWPBS, input.CWPBN, input.CWPBB_LagLength, input.CWPBB_TransmitPulseType, input.CWPP, input.CWPTBP,
                input.CBTON, input.CBTBB_TransmitPulseType,
                input.BeamAngle, input.SpeedOfSound,
                input.SystemBootPower, input.SystemWakeupTime, input.SystemInitPower, input.SystemInitTime, input.BroadbandPower, input.SystemSavePower, input.SystemSaveTime,
                input.SystemSleepPower, input.BeamDiameter, input.CyclesPerElement,
                input.Temperature, input.Salinity, input.XdcrDepth,
                input.IsBurst, input.CBI_SamplesPerBurst);
        }

        /// <summary>
        /// Calculate the power usage based off the commands.
        /// 
        /// If this is burst calculation, set _IsBurst_ to true.  Then give the number of ensembles in the burst for _EnsemblesInBurst_.
        /// </summary>
        /// <param name="_CEI_">Ensemble interval in seconds.</param>
        /// <param name="_DeploymentDuration_">Deployment duration in days.</param>
        /// <param name="_Beams_">Number of beams.</param>
        /// <param name="_SystemFrequency_">System frequency in Hz.</param>
        /// <param name="_CWPON_">Flag if Water Profile is turned on.</param>
        /// <param name="_CWPBL_">Water Profile blank distance in meters.</param>
        /// <param name="_CWPBS_">Water Profile bin size in meters.</param>
        /// <param name="_CWPBN_">Water Profile number of bins.</param>
        /// <param name="_CWPBB_LagLength_">Water Profile lag length in meters.</param>
        /// <param name="_CWPBB_TransmitPulseType_">Water Profile transmit pulse type.  BB or NB.</param>
        /// <param name="_CWPP_">Number of Water Profile pings.</param>
        /// <param name="_CWPTBP_">Water Profile time between pings.</param>
        /// <param name="_CBTON_">Flag if Bottom Track is turned on.</param>
        /// <param name="_CBTBB_TransmitPulseType_">Bottom Track transmit pulse type.  BB or NB.</param>
        /// <param name="_BeamAngle_">Beam angle in degrees.</param>
        /// <param name="_SpeedOfSound_">Speed of Sound in meters per second.</param>
        /// <param name="_SystemBootPower_">System boot power in watts.</param>
        /// <param name="_SystemWakeupTime_">System wakeup time in seconds.</param>
        /// <param name="_SystemInitPower_">System init power in watts.</param>
        /// <param name="_SystemInitTime_">System init time in seconds.</param>
        /// <param name="_BroadbandPower_">Flag if using broadband power.</param>
        /// <param name="_SystemSavePower_">System save power in watts.</param>
        /// <param name="_SystemSaveTime_">System save time in seconds.</param>
        /// <param name="_SystemSleepPower_">System sleep power in watts.</param>
        /// <param name="_BeamDiameter_">Beam diameter in meters.</param>
        /// <param name="_CyclesPerElement_">Cycles per element.</param>
        /// <param name="_IsBurst_">Set the flag if calculating a burst.</param>
        /// <param name="_EnsemblesPerBurst_">If calculating a burst, give the number of ensembles in the burst.</param>
        /// <returns>The amount of power used for the deployment in watt/hours.</returns>
        public double CalculatePower(double _CEI_,
                                    double _DeploymentDuration_,
                                    int _Beams_,
                                    double _SystemFrequency_,
                                    bool _CWPON_,
                                    double _CWPBL_,
                                    double _CWPBS_,
                                    double _CWPBN_,
                                    double _CWPBB_LagLength_,
                                    Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType _CWPBB_TransmitPulseType_,
                                    double _CWPP_,
                                    double _CWPTBP_,
                                    bool _CBTON_,
                                    Commands.AdcpSubsystemCommands.eCBTBB_Mode _CBTBB_TransmitPulseType_,
                                    double _BeamAngle_,
                                    double _SpeedOfSound_,
                                    double _SystemBootPower_,
                                    double _SystemWakeupTime_,
                                    double _SystemInitPower_,
                                    double _SystemInitTime_,
                                    bool _BroadbandPower_,
                                    double _SystemSavePower_,
                                    double _SystemSaveTime_,
                                    double _SystemSleepPower_,
                                    double _BeamDiameter_,
                                    double _CyclesPerElement_,
                                    double _Temperature_,
                                    double _Salinity_,
                                    double _XdcrDepth_,
                                    bool _IsBurst_ = false,
                                    int _EnsemblesPerBurst_ = 0)
        {
            #region Number of Ensembles

            // Check for divide by 0
            long numEnsembles = 0;
            if (_CEI_ == 0)
            {
                numEnsembles = 0;
            }
            else
            {
                // Convert deployment duration to seconds
                // Then divide by time per ensemble which is in seconds
                numEnsembles = (long)Math.Round((_DeploymentDuration_ * 24.0 * 3600.0) / _CEI_);
            }

            // If this is a burst, then give the power for a burst.
            if(_IsBurst_)
            {
                numEnsembles = _EnsemblesPerBurst_;
            }

            #endregion

            #region Wakeups  (Question about CEI)

            double wakeups = 1;
            if (_CEI_ > 1.0)
            {
                if (_CWPTBP_ > 1.0)
                {
                    wakeups = numEnsembles * _CWPP_;
                }
                else
                {
                    wakeups = numEnsembles;
                }
            }

            #endregion

            #region Bottom Track Pings

            double bottomTrackPings = 0.0;
            if (_CBTON_)
            {
                double value = _CWPP_ / 10.0;
                if (value < 1)
                {
                    bottomTrackPings = numEnsembles;
                }
                else
                {
                    bottomTrackPings = (long)Math.Round(_CWPP_ / 10.0) * numEnsembles;
                }
            }

            #endregion

            #region Bottom Track Time

            double bottomTrackRange = GetPredictedRange(_CWPON_, _CWPBB_TransmitPulseType_, _CWPBS_, _CWPBN_, _CWPBL_, _CBTON_, _CBTBB_TransmitPulseType_, _SystemFrequency_, _BeamDiameter_, _CyclesPerElement_, _BeamAngle_, _SpeedOfSound_, _CWPBB_LagLength_, _BroadbandPower_, _Salinity_, _Temperature_, _XdcrDepth_).BottomTrack;
            double bottomTrackTime = 0.0015 * bottomTrackRange;

            #endregion

            #region Transmit Power Bottom Track

            //double beamXmtPowerBottomTrack = XmtW_1200000 + XmtW_600000 + XmtW_300000 + XmtW_150000 + XmtW_75000 + XmtW_38000;
            double beamXmtPowerBottomTrack = 0.0;
            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                beamXmtPowerBottomTrack = DEFAULT_XMIT_W_1200000;
            }

            // 600khz
            if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                beamXmtPowerBottomTrack = DEFAULT_XMIT_W_600000;
            }

            // 300khz
            if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                beamXmtPowerBottomTrack = DEFAULT_XMIT_W_300000;
            }

            // 150khz
            if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                beamXmtPowerBottomTrack = DEFAULT_XMIT_W_150000;
            }

            // 75khz
            if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                beamXmtPowerBottomTrack = DEFAULT_XMIT_W_75000;
            }

            // 38khz
            if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                beamXmtPowerBottomTrack = DEFAULT_XMIT_W_38000;
            }

            #endregion

            #region Bottom Track Transmit Power

            double btTransmitPower = bottomTrackPings * 0.2 * (bottomTrackTime * beamXmtPowerBottomTrack * _Beams_) / 3600.0;

            #endregion

            #region Bottom Track Receiver Power

            double freqMult = 1;
            if (_SystemFrequency_ > 600000.0)
            {
                freqMult = 2;
            }

            double btReceivePower = bottomTrackPings * (bottomTrackTime * _SystemBootPower_) / 3600.0 * freqMult;

            #endregion

            #region Wakeup Power

            double wakeupPower = wakeups * _SystemWakeupTime_ * _SystemBootPower_ / 3600.0;

            #endregion

            #region Init Power

            double initPower = wakeups * _SystemInitPower_ * _SystemInitTime_ / 3600.0; ;

            #endregion

            #region Sample Rate

            double sumSampling = 0.0;
            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_1200000 * DEFAULT_CPE_1200000 / _CyclesPerElement_;
            }

            // 600khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_600000 * DEFAULT_CPE_600000 / _CyclesPerElement_;
            }

            // 300khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumSampling += DEFAULT_SAMPLING_300000 * DEFAULT_CPE_300000 / _CyclesPerElement_;
            }

            // 150khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumSampling += DEFAULT_SAMPLING_150000 * DEFAULT_CPE_150000 / _CyclesPerElement_;
            }

            // 75khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumSampling += DEFAULT_SAMPLING_75000 * DEFAULT_CPE_75000 / _CyclesPerElement_;
            }

            // 38khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumSampling += DEFAULT_SAMPLING_38000 * DEFAULT_CPE_38000 / _CyclesPerElement_;
            }

            double sampleRate = _SystemFrequency_ * (sumSampling);

            #endregion

            #region Meters Per Sample

            // Check for divide by 0
            double metersPerSample = 0;
            if (sampleRate == 0)
            {
                metersPerSample = 0.0;
            }
            else
            {
                metersPerSample = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) * _SpeedOfSound_ / 2.0 / sampleRate;
            }

            #endregion

            #region Bin Samples

            // Check for divide by 0
            int binSamples = 0;
            if (metersPerSample == 0)
            {
                binSamples = 0;
            }
            else
            {
                binSamples = (int)Math.Truncate(_CWPBS_ / metersPerSample);
            }

            #endregion

            #region Bin Time

            double binTime = 1;
            // Check for divide by 0
            if (sampleRate == 0)
            {
                binTime = 0;
            }
            else
            {
                binTime = binSamples / sampleRate;
            }

            #endregion

            #region Lag Samples

            // Check for divide by 0
            int lagSamples = 0;
            if (metersPerSample == 0)
            {
                lagSamples = 0;
            }
            else
            {
                lagSamples = 2 * (int)Math.Truncate((Math.Truncate(_CWPBB_LagLength_ / metersPerSample) + 1.0) / 2.0);
            }

            #endregion

            #region Code Repeats

            double codeRepeats = 0;

            // Check for divide by 0
            if (lagSamples == 0)
            {
                codeRepeats = 0;
            }

            // Cased BinSamples and LagSamples to double because Truncate only takes doubles
            // Make the result of Truncate an int
            if (((int)Math.Truncate((double)binSamples / (double)lagSamples)) + 1.0 < 2.0)
            {
                codeRepeats = 2;
            }
            else
            {
                codeRepeats = ((int)Math.Truncate((double)binSamples / (double)lagSamples)) + 1;
            }

            #endregion

            #region Lag Time

            // Check for divide by 0
            double lagTime = 0.0;
            if (sampleRate == 0)
            {
                lagTime = 0.0;
            }
            else
            {
                lagTime = lagSamples / sampleRate;
            }

            #endregion

            #region Transmit Code Time

            double transmitCodeTime = 1;
            // If using Broadband
            if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.BROADBAND)
            {
                if (codeRepeats < 3)
                {
                    transmitCodeTime = 2.0 * binTime;
                }
                else
                {
                    transmitCodeTime = codeRepeats * lagTime;
                }
            }
            else
            {
                if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                {
                    transmitCodeTime = binTime;
                }
                else
                {
                    transmitCodeTime = 2.0 * binTime;
                }
            }

            #endregion

            #region Transmit Scale

            double xmtScale = 0.0;

            // Checck if NB
            if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
            {
                xmtScale = 1.0;
            }
            else
            {
                // Check for bad value
                if (lagSamples == 0)
                {
                    xmtScale = 0.0;
                }

                // Check wich Broadband power is used
                else if (_BroadbandPower_)
                {
                    xmtScale = (lagSamples - 1.0) / lagSamples;
                }
                else
                {


                    xmtScale = 1.0 / lagSamples;
                }
            }

            #endregion

            #region Transmit Watt

            // Get the sum of all the selected XmtW
            double sumXmtW = 0.0;

            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumXmtW = DEFAULT_XMIT_W_1200000;
            }

            // 600khz
            if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumXmtW = DEFAULT_XMIT_W_600000;
            }

            // 300khz
            if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumXmtW = DEFAULT_XMIT_W_300000;
            }

            // 150khz
            if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumXmtW = DEFAULT_XMIT_W_150000;
            }

            // 75khz
            if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumXmtW = DEFAULT_XMIT_W_75000;
            }

            // 38khz
            if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumXmtW = DEFAULT_XMIT_W_38000;
            }

            #endregion

            #region Beam Transmit Power Profile

            double beamXmtPowerProfile = xmtScale * sumXmtW;

            #endregion

            #region Transmit Power

            double transmitPower = 0.0;
            if (!_IsBurst_)
            {
                transmitPower = (transmitCodeTime * beamXmtPowerProfile * _Beams_ * numEnsembles * _CWPP_) / 3600.0;
            }
            else
            {
                transmitPower = (transmitCodeTime * beamXmtPowerProfile * _Beams_ * _EnsemblesPerBurst_ * _CWPP_) / 3600.0;
            }

            #endregion

            #region Time Between Pings

            double timeBetweenPings = 0.0;
            // Check for divide by 0
            if (sampleRate == 0)
            {
                timeBetweenPings = _CWPTBP_;
            }
            else if (_CWPBN_ * binSamples / sampleRate > _CWPTBP_)
            {
                timeBetweenPings = _CWPBN_ * binSamples / sampleRate;
            }
            else
            {
                timeBetweenPings = _CWPTBP_;
            }

            #endregion

            #region Profile Time / Receive Time
            
            double receiveTime = 0.0;

            if (sampleRate == 0)                        //Default if issue with divide by zero
            {
                receiveTime = 0;
            }
            else if (_CWPP_ == 1)                       //1 Ping only, so no time between ping needed
            {
                receiveTime = 0;
            }
            else if (timeBetweenPings > 1.0)            //Time between Pings is greater than 1, sleeping between pings
            {
                receiveTime = _CWPBN_ * binSamples / sampleRate;
            }
            else                                        //Use the greatest sleep time found
            {
                receiveTime = timeBetweenPings;
            }

            if (_IsBurst_)                              //if in burst mode, use different default timing
            {
                if (_CWPP_ == 1)                        //If single ping, use CEI for time
                    receiveTime = _CEI_;

                else if (sampleRate == 0)               //Default if issue with divide by zero
                    receiveTime = _CEI_;

                else if (timeBetweenPings > 1.0)        //Time between pings is greater than 1, sleeping between pings
                    receiveTime = _CWPBN_ * binSamples / sampleRate;

                else                                    //use the greatest sleep time we found
                    receiveTime = timeBetweenPings;
            }
            #endregion

            #region Receive Power

            double systemRcvPower = SystemRcvPower(_Beams_);
           

            double receivePower = 0.0;
            double freqMultRcvPwr = 1;
            if (_SystemFrequency_ > 700000.0)
            {
                freqMultRcvPwr = 2;
            }

            receivePower = (receiveTime * systemRcvPower * numEnsembles * _CWPP_) / 3600.0 * freqMultRcvPwr;

            #endregion

            #region Save Power

            double savePower = (wakeups * _SystemSavePower_ * _SystemSaveTime_) / 3600.0;

            #endregion

            #region Sleep Power

            double sleepPower = _SystemSleepPower_ * _DeploymentDuration_ * 24.0;
            if (_IsBurst_)
                sleepPower = _SystemSleepPower_;

            #endregion

            #region Transmit Voltage

            // Sum up the Xmt Voltage
            double sumXmtV = 0.0;

            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumXmtV = DEFAULT_XMIT_V_1200000;
            }

            // 600khz
            if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumXmtV = DEFAULT_XMIT_V_600000;
            }

            // 300khz
            if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumXmtV = DEFAULT_XMIT_V_300000;
            }

            // 150khz
            if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumXmtV = DEFAULT_XMIT_V_150000;
            }

            // 75khz
            if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumXmtV = DEFAULT_XMIT_V_75000;
            }

            // 38khz
            if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumXmtV = DEFAULT_XMIT_V_38000;
            }

            #endregion

            #region Leakage

            // Sum up the Leakage
            double sumLeakageuA = 0.0;

            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumLeakageuA = 3.0 * Math.Sqrt(2.0 * 0.000001 * DEFAULT_UF_1200000 * DEFAULT_XMIT_V_1200000); ;
            }

            // 600khz
            if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumLeakageuA = 3.0 * Math.Sqrt(2.0 * 0.000001 * DEFAULT_UF_600000 * DEFAULT_XMIT_V_600000);
            }

            // 300khz
            if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumLeakageuA = 3.0 * Math.Sqrt(2.0 * 0.000001 * DEFAULT_UF_300000 * DEFAULT_XMIT_V_300000); ;
            }

            // 150khz
            if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumLeakageuA = 3.0 * Math.Sqrt(2.0 * 0.000001 * DEFAULT_UF_150000 * DEFAULT_XMIT_V_150000); ;
            }

            // 75khz
            if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumLeakageuA = 3.0 * Math.Sqrt(2.0 * 0.000001 * DEFAULT_UF_75000 * DEFAULT_XMIT_V_75000); ;
            }

            // 38khz
            if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumLeakageuA = 3.0 * Math.Sqrt(2.0 * 0.000001 * DEFAULT_UF_38000 * DEFAULT_XMIT_V_38000); ;
            }

            #endregion

            #region Cap Charge Power

            double capChargePower = 0.03 * (btTransmitPower + transmitPower) + 1.3 * _DeploymentDuration_ * 24.0 * sumXmtV * 0.000001 * sumLeakageuA;
            if (_IsBurst_)
                capChargePower = 0.03 * (btTransmitPower + transmitPower) + 1.3 * sumXmtV * 0.000001 * sumLeakageuA;

            #endregion

            return btTransmitPower + btReceivePower + wakeupPower + initPower + transmitPower + receivePower + savePower + sleepPower + capChargePower;
        }

        public double SystemRcvPower(int beams)
        {
            double systemRcvPower = 3.80;
            if (beams == 4)
            {
                systemRcvPower = 3.8;     // 1200khz 4Beam system test result 
            }
            else if (beams == 5)
            {
                systemRcvPower = 4.30;    // 600/600khz 5Beam system test result
            }
            else if (beams >= 7)
            {
                systemRcvPower = 5.00;    // 300/1200khz 8Beam system test result, 7Beam taken from waves model
            }
            return systemRcvPower;
        }
        #endregion

        #region Burst Calculate Power

        /// <summary>
        /// Calculate the power for a burst.
        /// </summary>
        /// <param name="input">Give the input values.</param>
        /// <returns>Power used for a burst in watts.</returns>
        public double CalculatePowerBurst(PredictionModelInput input)
        {
            return CalculatePowerBurst(input.CEI, input.DeploymentDuration, input.Beams, input.SystemFrequency,
                input.CWPON, input.CWPBL, input.CWPBS, input.CWPBN, input.CWPBB_LagLength, input.CWPBB_TransmitPulseType, input.CWPP, input.CWPTBP,
                input.CBTON, input.CBTBB_TransmitPulseType,
                input.BeamAngle, input.SpeedOfSound,
                input.SystemBootPower, input.SystemWakeupTime, input.SystemInitPower, input.SystemInitTime,
                input.BroadbandPower, input.SystemSavePower, input.SystemSaveTime, input.SystemSleepPower,
                input.BeamDiameter, 
                input.CyclesPerElement, 
                input.Temperature, input.Salinity, input.XdcrDepth,
                input.CBI_SamplesPerBurst, input.CBI_BurstInterval, input.CBI_IsInterleaved);
        }

        /// <summary>
        /// Calculate the power usage based off the commands.
        /// 
        /// If this is burst calculation, set _IsBurst_ to true.  Then give the number of ensembles in the burst for _EnsemblesInBurst_.
        /// </summary>
        /// <param name="_CEI_">Ensemble interval in seconds.</param>
        /// <param name="_DeploymentDuration_">Deployment duration in days.</param>
        /// <param name="_Beams_">Number of beams.</param>
        /// <param name="_SystemFrequency_">System frequency in Hz.</param>
        /// <param name="_CWPON_">Flag if Water Profile is turned on.</param>
        /// <param name="_CWPBL_">Water Profile blank distance in meters.</param>
        /// <param name="_CWPBS_">Water Profile bin size in meters.</param>
        /// <param name="_CWPBN_">Water Profile number of bins.</param>
        /// <param name="_CWPBB_LagLength_">Water Profile lag length in meters.</param>
        /// <param name="_CWPBB_TransmitPulseType_">Water Profile transmit pulse type.  BB or NB.</param>
        /// <param name="_CWPP_">Number of Water Profile pings.</param>
        /// <param name="_CWPTBP_">Water Profile time between pings.</param>
        /// <param name="_CBTON_">Flag if Bottom Track is turned on.</param>
        /// <param name="_CBTBB_TransmitPulseType_">Bottom Track transmit pulse type.  BB or NB.</param>
        /// <param name="_BeamAngle_">Beam angle in degrees.</param>
        /// <param name="_SpeedOfSound_">Speed of Sound in meters per second.</param>
        /// <param name="_SystemBootPower_">System boot power in watts.</param>
        /// <param name="_SystemWakeupTime_">System wakeup time in seconds.</param>
        /// <param name="_SystemInitPower_">System init power in watts.</param>
        /// <param name="_SystemInitTime_">System init time in seconds.</param>
        /// <param name="_BroadbandPower_">Flag if using broadband power.</param>
        /// <param name="_SystemSavePower_">System save power in watts.</param>
        /// <param name="_SystemSaveTime_">System save time in seconds.</param>
        /// <param name="_SystemSleepPower_">System sleep power in watts.</param>
        /// <param name="_BeamDiameter_">Beam diameter in meters.</param>
        /// <param name="_CyclesPerElement_">Cycles per element.</param>
        /// <param name="_CBI_BurstInterval_">Length of a burst.</param>
        /// <param name="_CBI_EnsemblesPerBurst_">If calculating a burst, give the number of ensembles in the burst.</param>
        /// <param name="_CBI_IsInterleaved_">Set flag if this is an interleaved burst.</param>
        /// <returns>The amount of power used for the deployment in watt/hours.</returns>
        public double CalculatePowerBurst(double _CEI_,
                                    double _DeploymentDuration_,
                                    int _Beams_,
                                    double _SystemFrequency_,
                                    bool _CWPON_,
                                    double _CWPBL_,
                                    double _CWPBS_,
                                    double _CWPBN_,
                                    double _CWPBB_LagLength_,
                                    Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType _CWPBB_TransmitPulseType_,
                                    double _CWPP_,
                                    double _CWPTBP_,
                                    bool _CBTON_,
                                    Commands.AdcpSubsystemCommands.eCBTBB_Mode _CBTBB_TransmitPulseType_,
                                    double _BeamAngle_,
                                    double _SpeedOfSound_,
                                    double _SystemBootPower_,
                                    double _SystemWakeupTime_,
                                    double _SystemInitPower_,
                                    double _SystemInitTime_,
                                    bool _BroadbandPower_,
                                    double _SystemSavePower_,
                                    double _SystemSaveTime_,
                                    double _SystemSleepPower_,
                                    double _BeamDiameter_,
                                    double _CyclesPerElement_,
                                    double _Temperature_,
                                    double _Salinity_,
                                    double _XdcrDepth_,
                                    int _CBI_EnsemblesPerBurst_,
                                    double _CBI_BurstInterval_,
                                    bool _CBI_IsInterleaved_)
        {
            // Power per burst
            // Set 1 day for deployment duration so only 1 burst is calculated
            double burstPwr = CalculatePower(_CEI_, 1, _Beams_, _SystemFrequency_, _CWPON_, _CWPBL_, _CWPBS_, _CWPBN_, _CWPBB_LagLength_, _CWPBB_TransmitPulseType_, _CWPP_, _CWPTBP_, _CBTON_, _CBTBB_TransmitPulseType_, _BeamAngle_, _SpeedOfSound_, _SystemBootPower_, _SystemWakeupTime_, _SystemInitPower_, _SystemInitTime_, _BroadbandPower_, _SystemSavePower_, _SystemSaveTime_, _SystemSleepPower_, _BeamDiameter_, _CyclesPerElement_, _Temperature_, _Salinity_, _XdcrDepth_, true, _CBI_EnsemblesPerBurst_);

            // Get the number of burst per deployment duration
            double deploymentDur = _DeploymentDuration_ * 3600.0 * 24.0;                // Seconds for the deployment duration
            int numBurst = (int)Math.Round(deploymentDur / _CBI_BurstInterval_);        // Divide total duration by burst duration

            return burstPwr * numBurst;                                                 // Multiply the number of bursts by power per burst
        }

        #endregion

        #region Battery Usage

        public double BatteryUsage(PredictionModelInput input)
        {
            if (input.IsBurst)
                return BatteryUsage(CalculatePowerBurst(input), input.DeploymentDuration, PredictionModelInput.DEFAULT_BATTERY_POWER, input.BatteryDerate, input.BatterySelfDischargePerYear);
            return BatteryUsage(CalculatePower(input), input.DeploymentDuration, PredictionModelInput.DEFAULT_BATTERY_POWER, input.BatteryDerate, input.BatterySelfDischargePerYear);
        }

        /// <summary>
        /// Battery needed based off the power usage.
        /// </summary>
        /// <param name="powerUsage">Power that will be used in watts.</param>
        /// <param name="_DeploymentDuration_">Number of days of the deployment.</param>
        /// <param name="_BatteryCapacity_">Battery Capacity.</param>
        /// <param name="_BatteryDerate_">Battery derate.</param>
        /// <param name="_BatterySelfDischarge_">Battery self discharge over a year.</param>
        /// <returns>Total number of batteries required.</returns>
        public double BatteryUsage(double powerUsage, double _DeploymentDuration_,  double _BatteryCapacity_, double _BatteryDerate_, double _BatterySelfDischarge_)
        {
            // Current battery power available
            double batteryPwr = _BatteryCapacity_ * _BatteryDerate_ - _BatterySelfDischarge_ * _DeploymentDuration_ / 365.0;

            return powerUsage / batteryPwr;
        }

        #endregion

        #region Predicted Range

        /// <summary>
        /// Predicted ranges for water profile and bottom track.
        /// </summary>
        public struct PredictedRanges
        {
            /// <summary>
            /// Water Profile predicted range in meters.
            /// </summary>
            public double WaterProfile { get; set; }

            /// <summary>
            /// Bottom Track predicted range in meters.
            /// </summary>
            public double BottomTrack { get; set; }

            /// <summary>
            /// First bin position in meters.
            /// </summary>
            public double FirstBinPosition { get; set; }

            /// <summary>
            /// The profile range based off the commands.  This will 
            /// use the bin size, number of bins and blank.
            /// </summary>
            public double ProfileRangeSettings { get; set; }
        }

        /// <summary>
        /// Calculate the predicted ranges with the given input values.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Predicted ranges.</returns>
        public PredictedRanges GetPredictedRange(PredictionModelInput input)
        {
            return GetPredictedRange(input.CWPON, input.CWPBB_TransmitPulseType, input.CWPBS, input.CWPBN, input.CWPBL,
                input.CBTON, input.CBTBB_TransmitPulseType,
                input.SystemFrequency, input.BeamDiameter, input.CyclesPerElement, input.BeamAngle,
                input.SpeedOfSound, input.CWPBB_LagLength, input.BroadbandPower,
                input.Salinity, input.Temperature, input.XdcrDepth);
        }

        /// <summary>
        /// Get the predicted range.  This will return the bottom track range, water profile range, and the first bin position.
        /// </summary>
        /// <param name="_CWPON_">Flag if Water Profile is turned on.</param>
        /// <param name="_CWPBB_TransmitPulseType_">Water Profile transmit type.  NB or BB.</param>
        /// <param name="_CWPBS_">Bin size in meters.</param>
        /// <param name="_CWPBN_">Number of bins.</param>
        /// <param name="_CWPBL_">Blank size in meters.</param>
        /// <param name="_CBTON_">Flag if Bottom Track is turned on.</param>
        /// <param name="_CBTBB_TransmitPulseType_">Bottom Track transmit type.  NB or BB.</param>
        /// <param name="_SystemFrequency_">System frequency in Hz.</param>
        /// <param name="_BeamDiameter_">Beam diameter in meters.</param>
        /// <param name="_CyclesPerElement_">Cycles per element.</param>
        /// <param name="_BeamAngle_">Beam angle in degrees.</param>
        /// <param name="_SpeedOfSound_">Speed of Sound in m/s.</param>
        /// <param name="_CWPBB_LagLength_">Water Profile Lag length in meters.</param>
        /// <param name="_BroadbandPower_">Flag if using Broadband power.</param>
        /// <param name="_RangeReduction_">Range Reduction Value.</param>
        /// <param name="_Salinity_">Salinity in ppt.</param>
        /// <param name="_Temperature_">Temperature in Celcuis.</param>
        /// <param name="_XdcrDepth_">Tranducer depth in meters.</param>
        /// <returns>Predicted ranges.</returns>
        public PredictedRanges GetPredictedRange(
                                bool _CWPON_,
                                Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType _CWPBB_TransmitPulseType_,
                                double _CWPBS_,
                                double _CWPBN_,
                                double _CWPBL_,
                                bool _CBTON_,
                                Commands.AdcpSubsystemCommands.eCBTBB_Mode _CBTBB_TransmitPulseType_,
                                double _SystemFrequency_,
                                double _BeamDiameter_,
                                double _CyclesPerElement_,
                                double _BeamAngle_,
                                double _SpeedOfSound_,
                                double _CWPBB_LagLength_,
                                bool _BroadbandPower_, 
                                double _Salinity_, 
                                double _Temperature_, 
                                double _XdcrDepth_)
        {
            #region WaveLength

            double waveLength = _SpeedOfSound_ / _SystemFrequency_;

            #endregion

            #region DI

            double dI = 0.0;
            if (waveLength == 0)
            {
                dI = 0.0;
            }
            else
            {
                dI = 20.0 * Math.Log10(Math.PI * _BeamDiameter_ / waveLength);
            }

            #endregion

            #region Absorption

            //double _freq_, double _speedOfSound_, double _salinity_, double _temperature_, double _xdcrDepth_
            double absorption = CalcAbsorption(_SystemFrequency_, _SpeedOfSound_, _Salinity_, _Temperature_, _XdcrDepth_);

            #endregion

            #region 1200khz

            double btRange_1200000 = 0.0;
            double wpRange_1200000 = 0.0;
            double refBin_1200000 = 0.0;
            double xmtW_1200000 = 0.0;

            #region rScale

            double rScale_1200000 = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) / Math.Cos(DEFAULT_BEAM_ANGLE_1200000 / 180.0 * Math.PI);

            #endregion

            #region DI

            double dI_1200000 = 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_1200000 / waveLength);

            #endregion

            #region db

            // Check for divide by 0
            double dB_1200000 = 0.0;
            if (DEFAULT_BIN_1200000 == 0 || _CyclesPerElement_ == 0)
            {
                dB_1200000 = 0.0;
            }
            else
            {
                dB_1200000 = 10.0 * Math.Log10(_CWPBS_ / DEFAULT_BIN_1200000) + dI - dI_1200000 - 10.0 * Math.Log10(DEFAULT_CPE_1200000 / _CyclesPerElement_);
            }

            #endregion

            #region Absorption Range

            double absorption_range_1200000 = DEFAULT_RANGE_1200000 + ((ABSORPTION_SCALE_1200000 - absorption) * DEFAULT_RANGE_1200000);

            #endregion

            // If selected, return a value
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                // Ref Bin and xmt watt
                refBin_1200000 = DEFAULT_BIN_1200000;
                xmtW_1200000 = DEFAULT_XMIT_W_1200000;

                if (_CBTON_)
                {
                    // Check if NB
                    if (_CBTBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                    {
                        btRange_1200000 = 2.0 * rScale_1200000 * (absorption_range_1200000 + DEFAULT_BIN_1200000 * dB_1200000 + 15.0 * DEFAULT_BIN_1200000);
                    }
                    else
                    {
                        btRange_1200000 = 2.0 * rScale_1200000 * (absorption_range_1200000 + DEFAULT_BIN_1200000 * dB_1200000);
                    }
                }
                else
                {
                    btRange_1200000 = 0.0;
                }

                if (_CWPON_)
                {
                    // Checck if NB
                    if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        wpRange_1200000 = rScale_1200000 * (absorption_range_1200000 + DEFAULT_BIN_1200000 * dB_1200000 + 20.0 * DEFAULT_BIN_1200000);
                    }
                    else
                    {
                        wpRange_1200000 = rScale_1200000 * (absorption_range_1200000 + DEFAULT_BIN_1200000 * dB_1200000);
                    }
                }
                else
                {
                    wpRange_1200000 = 0.0;
                }

            }
            else
            {
                btRange_1200000 = 0.0;
                wpRange_1200000 = 0.0;
            }

            #endregion

            #region 600khz

            double btRange_600000 = 0.0;
            double wpRange_600000 = 0.0;
            double refBin_600000 = 0.0;
            double xmtW_600000 = 0.0;

            #region rScale

            double rScale_600000 = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) / Math.Cos(DEFAULT_BEAM_ANGLE_600000 / 180.0 * Math.PI);

            #endregion

            #region DI

            double dI_600000 = 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_600000 / waveLength);

            #endregion

            #region db

            double dB_600000 = 0.0;
            // Check for divide by 0
            if (DEFAULT_BIN_600000 == 0 || _CyclesPerElement_ == 0)
            {
                dB_600000 = 0.0;
            }
            else
            {
                dB_600000 = 10.0 * Math.Log10(_CWPBS_ / DEFAULT_BIN_600000) + dI - dI_600000 - 10.0 * Math.Log10(DEFAULT_CPE_600000 / _CyclesPerElement_);
            }

            #endregion

            #region Absorption Range

            double absorption_range_600000 = DEFAULT_RANGE_600000 + ((ABSORPTION_SCALE_600000 - absorption) * DEFAULT_RANGE_600000);

            #endregion

            // If selected, return a value
            if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                // Ref Bin and xmt watt
                refBin_600000 = DEFAULT_BIN_600000;
                xmtW_600000 = DEFAULT_XMIT_W_600000;

                if (_CBTON_)
                {
                    // Check if NB
                    if (_CBTBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                    {
                        btRange_600000 = 2.0 * rScale_600000 * (absorption_range_600000 + DEFAULT_BIN_600000 * dB_600000 + 15.0 * DEFAULT_BIN_600000);
                    }
                    else
                    {
                        btRange_600000 = 2.0 * rScale_600000 * (absorption_range_600000 + DEFAULT_BIN_600000 * dB_600000);
                    }
                }
                else
                {
                    btRange_600000 = 0.0;
                }

                if (_CWPON_)
                {
                    // Checck if NB
                    if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        wpRange_600000 = rScale_600000 * (absorption_range_600000 + DEFAULT_BIN_600000 * dB_600000 + 20.0 * DEFAULT_BIN_600000);
                    }
                    else
                    {
                        wpRange_600000 = rScale_600000 * (absorption_range_600000 + DEFAULT_BIN_600000 * dB_600000);
                    }
                }
                else
                {
                    wpRange_600000 = 0.0;
                }
            }
            else
            {
                // Return 0 if not selected
                btRange_600000 = 0.0;
                wpRange_600000 = 0.0;
            }

            #endregion

            #region 300khz

            double btRange_300000 = 0.0;
            double wpRange_300000 = 0.0;
            double refBin_300000 = 0.0;
            double xmtW_300000 = 0.0;

            #region rScale

            double rScale_300000 = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) / Math.Cos(DEFAULT_BEAM_ANGLE_300000 / 180.0 * Math.PI);

            #endregion

            #region DI

            double dI_300000 = 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_300000 / waveLength);

            #endregion

            #region db

            double dB_300000 = 0.0;
            // Check for divide by 0
            if (DEFAULT_BIN_300000 == 0 || _CyclesPerElement_ == 0)
            {
                dB_300000 = 0.0;
            }
            else
            {
                dB_300000 = 10.0 * Math.Log10(_CWPBS_ / DEFAULT_BIN_300000) + dI - dI_300000 - 10.0 * Math.Log10(DEFAULT_CPE_300000 / _CyclesPerElement_);
            }

            #endregion

            #region Absorption Range

            double absorption_range_300000 = DEFAULT_RANGE_300000 + ((ABSORPTION_SCALE_300000 - absorption) * DEFAULT_RANGE_300000);

            #endregion

            // If selected, return a value
            if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                // Ref Bin and xmt watt
                refBin_300000 = DEFAULT_BIN_300000;
                xmtW_300000 = DEFAULT_XMIT_W_300000;

                if (_CBTON_)
                {
                    // Check if NB
                    if (_CBTBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                    {
                        btRange_300000 = 2.0 * rScale_300000 * (absorption_range_300000 + DEFAULT_BIN_300000 * dB_300000 + 15.0 * DEFAULT_BIN_300000);
                    }
                    else
                    {
                        btRange_300000 = 2.0 * rScale_300000 * (absorption_range_300000 + DEFAULT_BIN_300000 * dB_300000);
                    }
                }
                else
                {
                    btRange_300000 = 0.0;
                }

                if (_CWPON_)
                {
                    // Checck if NB
                    if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        wpRange_300000 = rScale_300000 * (absorption_range_300000 + DEFAULT_BIN_300000 * dB_300000 + 20.0 * DEFAULT_BIN_300000);
                    }
                    else
                    {
                        wpRange_300000 = rScale_300000 * (absorption_range_300000 + DEFAULT_BIN_300000 * dB_300000);
                    }
                }
                else
                {
                    wpRange_300000 = 0.0;
                }
            }
            else
            {
                // Return 0 if not selected
                btRange_300000 = 0.0;
                wpRange_300000 = 0.0;
            }

            #endregion

            #region 150khz

            double btRange_150000 = 0.0;
            double wpRange_150000 = 0.0;
            double refBin_150000 = 0.0;
            double xmtW_150000 = 0.0;

            #region rScale

            double rScale_150000 = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) / Math.Cos(DEFAULT_BEAM_ANGLE_150000 / 180.0 * Math.PI);

            #endregion

            #region DI

            double dI_150000 = 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_150000 / waveLength);

            #endregion

            #region db

            double dB_150000 = 0.0;
            // Check for divide by 0
            if (DEFAULT_BIN_150000 == 0 || _CyclesPerElement_ == 0)
            {
                dB_150000 = 0.0;
            }
            else
            {
                dB_150000 = 10.0 * Math.Log10(_CWPBS_ / DEFAULT_BIN_150000) + dI - dI_150000 - 10.0 * Math.Log10(DEFAULT_CPE_150000 / _CyclesPerElement_);
            }

            #endregion

            #region Absorption Range

            double absorption_range_150000 = DEFAULT_RANGE_150000 + ((ABSORPTION_SCALE_150000 - absorption) * DEFAULT_RANGE_150000);

            #endregion

            // If selected, return a value
            if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                // Ref Bin and xmt watt
                refBin_150000 = DEFAULT_BIN_150000;
                xmtW_150000 = DEFAULT_XMIT_W_150000;

                if (_CBTON_)
                {
                    // Check if NB
                    if (_CBTBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                    {
                        btRange_150000 = 2.0 * rScale_150000 * (absorption_range_150000 + DEFAULT_BIN_150000 * dB_150000 + 15.0 * DEFAULT_BIN_150000);
                    }
                    else
                    {
                        btRange_150000 = 2.0 * rScale_150000 * (absorption_range_150000 + DEFAULT_BIN_150000 * dB_150000);
                    }
                }
                else
                {
                    btRange_150000 = 0.0;
                }
                if (_CWPON_)
                {
                    // Checck if NB
                    if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        wpRange_150000 = rScale_150000 * (absorption_range_150000 + DEFAULT_BIN_150000 * dB_150000 + 20.0 * DEFAULT_BIN_150000);
                    }
                    else
                    {
                        wpRange_150000 = rScale_150000 * (absorption_range_150000 + DEFAULT_BIN_150000 * dB_150000);
                    }
                }
                else
                {
                    wpRange_150000 = 0.0;
                }
            }
            else
            {
                // Return 0 if not selected
                btRange_150000 = 0.0;
                wpRange_150000 = 0.0;
            }

            #endregion

            #region 75khz

            double btRange_75000 = 0.0;
            double wpRange_75000 = 0.0;
            double refBin_75000 = 0.0;
            double xmtW_75000 = 0.0;

            #region rScale

            double rScale_75000 = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) / Math.Cos(DEFAULT_BEAM_ANGLE_75000 / 180.0 * Math.PI);

            #endregion

            #region DI

            double dI_75000 = 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_75000 / waveLength);

            #endregion

            #region db

            double dB_75000 = 0.0;
            // Check for divide by 0
            if (DEFAULT_BIN_75000 == 0 || _CyclesPerElement_ == 0)
            {
                dB_75000 = 0.0;
            }
            else
            {
                dB_75000 = 10.0 * Math.Log10(_CWPBS_ / DEFAULT_BIN_75000) + dI - dI_75000 - 10.0 * Math.Log10(DEFAULT_CPE_75000 / _CyclesPerElement_);
            }

            #endregion

            #region Absorption Range

            double absorption_range_75000 = DEFAULT_RANGE_75000 + ((ABSORPTION_SCALE_75000 - absorption) * DEFAULT_RANGE_75000);

            #endregion

            // If selected, return a value
            if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                // Ref Bin and xmt watt
                refBin_75000 = DEFAULT_BIN_75000;
                xmtW_75000 = DEFAULT_XMIT_W_75000;

                if (_CBTON_)
                {
                    // Check if NB
                    if (_CBTBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                    {
                        btRange_75000 = 2.0 * rScale_75000 * (absorption_range_75000 + DEFAULT_BIN_75000 * dB_75000 + 15.0 * DEFAULT_BIN_75000);
                    }
                    else
                    {
                        btRange_75000 = 2.0 * rScale_75000 * (absorption_range_75000 + DEFAULT_BIN_75000 * dB_75000);
                    }
                }
                else
                {
                    btRange_75000 = 0.0;
                }

                if (_CWPON_)
                {
                    // Checck if NB
                    if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        wpRange_75000 = rScale_75000 * (absorption_range_75000 + DEFAULT_BIN_75000 * dB_75000 + 20.0 * DEFAULT_BIN_75000);
                    }
                    else
                    {
                        wpRange_75000 = rScale_75000 * (absorption_range_75000 + DEFAULT_BIN_75000 * dB_75000);
                    }
                }
                else
                {
                    wpRange_75000 = 0.0;
                }
            }
            else
            {
                // Return 0 if not selected
                btRange_75000 = 0.0;
                wpRange_75000 = 0.0;
            }

            #endregion

            #region 38khz

            double btRange_38000 = 0.0;
            double wpRange_38000 = 0.0;
            double refBin_38000 = 0.0;
            double xmtW_38000 = 0.0;

            #region rScale

            double rScale_38000 = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) / Math.Cos(DEFAULT_BEAM_ANGLE_38000 / 180.0 * Math.PI);

            #endregion

            #region DI

            double dI_38000 = 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_38000 / waveLength);

            #endregion

            #region db

            double dB_38000 = 0.0;
            // Check for divide by 0
            if (DEFAULT_BIN_38000 == 0 || _CyclesPerElement_ == 0)
            {
                dB_38000 = 0.0;
            }
            else
            {
                dB_38000 = 10.0 * Math.Log10(_CWPBS_ / DEFAULT_BIN_38000) + dI - dI_38000 - 10.0 * Math.Log10(DEFAULT_CPE_38000 / _CyclesPerElement_);
            }

            #endregion

            #region Absorption Range

            double absorption_range_38000 = DEFAULT_RANGE_38000 + ((ABSORPTION_SCALE_38000 - absorption) * DEFAULT_RANGE_38000);

            #endregion

            // If selected, return a value
            if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                // Ref Bin and xmt watt
                refBin_38000 = DEFAULT_BIN_38000;
                xmtW_38000 = DEFAULT_XMIT_W_38000;

                if (_CBTON_)
                {
                    // Check if NB
                    if (_CBTBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                    {
                        btRange_38000 = 2.0 * rScale_38000 * (absorption_range_38000 + DEFAULT_BIN_38000 * dB_38000 + 15.0 * DEFAULT_BIN_38000);
                    }
                    else
                    {
                        btRange_38000 = 2.0 * rScale_38000 * (absorption_range_38000 + DEFAULT_BIN_38000 * dB_38000);
                    }
                }
                else
                {
                    btRange_38000 = 0.0;
                }

                if (_CWPON_)
                {
                    // Checck if NB
                    if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        wpRange_38000 = rScale_38000 * (absorption_range_38000 + DEFAULT_BIN_38000 * dB_38000 + 20.0 * DEFAULT_BIN_38000);
                    }
                    else
                    {
                        wpRange_38000 = rScale_38000 * (absorption_range_38000 + DEFAULT_BIN_38000 * dB_38000);
                    }
                }
                else
                {
                    wpRange_38000 = 0.0;
                }
            }
            else
            {
                // Return 0 if not selected
                btRange_38000 = 0.0;
                wpRange_38000 = 0.0;
            }

            #endregion

            #region Sample Rate

            double sumSampling = 0.0;
            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_1200000 * DEFAULT_CPE_1200000 / _CyclesPerElement_;
            }

            // 600khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_600000 * DEFAULT_CPE_600000 / _CyclesPerElement_;
            }

            // 300khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumSampling += DEFAULT_SAMPLING_300000 * DEFAULT_CPE_300000 / _CyclesPerElement_;
            }

            // 150khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumSampling += DEFAULT_SAMPLING_150000 * DEFAULT_CPE_150000 / _CyclesPerElement_;
            }

            // 75khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumSampling += DEFAULT_SAMPLING_75000 * DEFAULT_CPE_75000 / _CyclesPerElement_;
            }

            // 38khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumSampling += DEFAULT_SAMPLING_38000 * DEFAULT_CPE_38000 / _CyclesPerElement_;
            }

            double sampleRate = _SystemFrequency_ * (sumSampling);

            #endregion

            #region Meters Per Sample

            // Check for divide by 0
            double metersPerSample = 0;
            if (sampleRate == 0)
            {
                metersPerSample = 0.0;
            }
            else
            {
                metersPerSample = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) * _SpeedOfSound_ / 2.0 / sampleRate;
            }

            #endregion

            #region Lag Samples

            // Check for divide by 0
            int lagSamples = 0;
            if (metersPerSample == 0)
            {
                lagSamples = 0;
            }
            else
            {
                lagSamples = 2 * (int)Math.Truncate((Math.Truncate(_CWPBB_LagLength_ / metersPerSample) + 1.0) / 2.0);
            }

            #endregion

            #region Xmt Scale

            double xmtScale = 1.0;

            // Checck if NB
            if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
            {
                xmtScale = 1.0;
            }
            else
            {
                // Check for bad value
                if (lagSamples == 0)
                {
                    xmtScale = 1.0;
                }

                // Check wich Broadband power is used
                if (_BroadbandPower_)
                {
                    xmtScale = (lagSamples - 1.0) / lagSamples;
                }
                else
                {
                    xmtScale = 1.0 / lagSamples;
                }
            }

            #endregion

            #region Range Reduction

            double rangeReduction = 0.0;

            // Get the sum of all the selected WP XmtW and RefBin
            double sumXmtW = xmtW_1200000 + xmtW_600000 + xmtW_300000 + xmtW_150000 + xmtW_75000 + xmtW_38000;
            double sumRefBin = refBin_1200000 + refBin_600000 + refBin_300000 + refBin_150000 + refBin_75000 + refBin_38000;

            double beamXmtPowerProfile = xmtScale * sumXmtW;

            // Check for bad values
            if (sumXmtW == 0)
            {
                rangeReduction = 0.0;
            }
            else
            {
                rangeReduction = 10.0 * Math.Log10(beamXmtPowerProfile / sumXmtW) * sumRefBin + 1.0;
            }

            #endregion

            #region Bin Samples

            // Check for divide by 0
            int binSamples = 0;
            if (metersPerSample == 0)
            {
                binSamples = 0;
            }
            else
            {
                binSamples = (int)Math.Truncate(_CWPBS_ / metersPerSample);
            }

            #endregion

            #region Code Repeats

            double codeRepeats = 0;

            // Check for divide by 0
            if (lagSamples == 0)
            {
                codeRepeats = 0;
            }

            // Cased BinSamples and LagSamples to double because Truncate only takes doubles
            // Make the result of Truncate an int
            if (((int)Math.Truncate((double)binSamples / (double)lagSamples)) + 1.0 < 2.0)
            {
                codeRepeats = 2;
            }
            else
            {
                codeRepeats = ((int)Math.Truncate((double)binSamples / (double)lagSamples)) + 1;
            }

            #endregion

            #region First Bin Position

            double pos = 0.0;
            if (_CWPBB_TransmitPulseType_ == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
            {
                pos = (2.0 * _CWPBS_ + 0.05) / 2.0;
            }
            else
            {
                if ((int)_CWPBB_TransmitPulseType_ > 1)
                {
                    pos = _CWPBS_;
                }
                else
                {
                    pos = (lagSamples * (codeRepeats - 1.0) * metersPerSample + _CWPBS_ + _CWPBB_LagLength_) / 2.0;
                }
            }

            double firstBinPosition = _CWPBL_ + pos;

            #endregion

            #region Profile Range based off Settings

            double profileRangeSettings = _CWPBL_ + (_CWPBS_ * _CWPBN_);

            #endregion

            // Set the predicted ranges
            PredictedRanges pr = new PredictedRanges();

            double wp = 0.0;
            double bt = 0.0;
            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                bt = btRange_1200000;
                wp = wpRange_1200000 + rangeReduction;
            }

            // 600khz
            if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                bt = btRange_600000;
                wp = wpRange_600000 + rangeReduction;
            }

            // 300khz
            if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                bt = btRange_300000;
                wp = wpRange_300000 + rangeReduction;
            }

            // 150khz
            if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                bt = btRange_150000;
                wp = wpRange_150000 + rangeReduction;
            }

            // 75khz
            if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                bt = btRange_75000;
                wp = wpRange_75000 + rangeReduction;
            }

            // 38khz
            if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                bt = btRange_38000;
                wp = wpRange_38000 + rangeReduction;
            }

            pr.BottomTrack = bt;
            pr.WaterProfile = wp;
            pr.FirstBinPosition = firstBinPosition;
            pr.ProfileRangeSettings = profileRangeSettings;


            return pr;
        }

        #endregion

        #region Max Velocity

        /// <summary>
        /// Get the predicted maximum velocity in m/s.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Predicted maximum velocity in m/s.</returns>
        public double GetMaxVelocity(PredictionModelInput input)
        {
            return GetMaxVelocity(input.CWPBB_LagLength, input.BeamAngle, input.SystemFrequency, input.SpeedOfSound, input.CyclesPerElement);
        }

        /// <summary>
        /// Predicted maximum velocity.
        /// </summary>
        /// <param name="_CWPBB_LagLength_">Water Profile lag length in meters/sec.</param>
        /// <param name="_BeamAngle_">Beam angle in degrees.</param>
        /// <param name="_SystemFrequency_">System frequency in Hz.</param>
        /// <param name="_SpeedOfSound_">Speed of Sound in meters/sec.</param>
        /// <param name="_CyclesPerElement_">Cycles per element.</param>
        /// <returns>Predicted maximum velocity in meters/sec.</returns>
        public double GetMaxVelocity(double _CWPBB_LagLength_,
                                    double _BeamAngle_,
                                    double _SystemFrequency_,
                                    double _SpeedOfSound_,
                                    double _CyclesPerElement_)
        {
            #region Sample Rate

            double sumSampling = 0.0;
            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_1200000 * DEFAULT_CPE_1200000 / _CyclesPerElement_;
            }

            // 600khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_600000 * DEFAULT_CPE_600000 / _CyclesPerElement_;
            }

            // 300khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumSampling += DEFAULT_SAMPLING_300000 * DEFAULT_CPE_300000 / _CyclesPerElement_;
            }

            // 150khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumSampling += DEFAULT_SAMPLING_150000 * DEFAULT_CPE_150000 / _CyclesPerElement_;
            }

            // 75khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumSampling += DEFAULT_SAMPLING_75000 * DEFAULT_CPE_75000 / _CyclesPerElement_;
            }

            // 38khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumSampling += DEFAULT_SAMPLING_38000 * DEFAULT_CPE_38000 / _CyclesPerElement_;
            }

            double sampleRate = _SystemFrequency_ * (sumSampling);

            #endregion

            #region Meters Per Sample

            // Check for divide by 0
            double metersPerSample = 0;
            if (sampleRate == 0)
            {
                metersPerSample = 0.0;
            }
            else
            {
                metersPerSample = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) * _SpeedOfSound_ / 2.0 / sampleRate;
            }

            #endregion

            #region Lag Samples

            // Check for divide by 0
            int lagSamples = 0;
            if (metersPerSample == 0)
            {
                lagSamples = 0;
            }
            else
            {
                lagSamples = 2 * (int)Math.Truncate((Math.Truncate(_CWPBB_LagLength_ / metersPerSample) + 1.0) / 2.0);
            }

            #endregion

            #region Ua Hz

            double uaHz = 0.0;
            // Check for divide by 0
            if (lagSamples == 0)
            {
                uaHz = 0.0;
            }
            else
            {
                uaHz = sampleRate / (2.0 * lagSamples);
            }

            #endregion

            #region Ua Radial

            double uaRadial = 0.0;
            // Check for divide by 0
            if (_SystemFrequency_ == 0)
            {
                uaRadial = 0.0;
            }
            else
            {
                uaRadial = uaHz * _SpeedOfSound_ / (2.0 * _SystemFrequency_);

            }
            #endregion

            // Check for vertical beam.  No Beam angle
            if (_BeamAngle_ == 0)
            {
                return uaRadial;
            }

            return uaRadial / Math.Sin(_BeamAngle_ / 180.0 * Math.PI);
        }

        #endregion

        #region Standard Deviation

        /// <summary>
        /// Get the standard deviation in m/s.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Predicted standard deviation in m/s.</returns>
        public double GetStandardDeviation(PredictionModelInput input)
        {
            return GetStandardDeviation(input.CWPP, input.CWPBS, input.CWPBB_LagLength, input.BeamAngle, input.CWPBB_TransmitPulseType,
                input.SystemFrequency, input.SpeedOfSound, input.CyclesPerElement, input.SNR, input.Beta, input.NbFudge);
        }

        /// <summary>
        /// Standard deviation prediction.
        /// </summary>
        /// <param name="_CWPP_">Water Profile pings.</param>
        /// <param name="_CWPBS_">Bin size in meters.</param>
        /// <param name="_CWPBB_LagLength_">Water Profile lag length in meters.</param>
        /// <param name="_BeamAngle_">Beam angle in degrees.</param>
        /// <param name="_CWPBB_TransmitPulseType_">Water Profile Transmit pulse type.  BB or NB.</param>
        /// <param name="_SystemFrequency_">System frequency in Hz.</param>
        /// <param name="_SpeedOfSound_">Speed of Sound in meters/sec.</param>
        /// <param name="_CyclesPerElement_">Cycles per element.</param>
        /// <param name="_SNR_">SNR in db.</param>
        /// <param name="_Beta_">Environmental decorrelation.</param>
        /// <param name="_NbFudge_">Narrowband fudge number.</param>
        /// <returns>Predicted standard deivaition in m/s.</returns>
        public double GetStandardDeviation(int _CWPP_,
                                            double _CWPBS_,
                                            double _CWPBB_LagLength_,
                                            double _BeamAngle_,
                                            Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType _CWPBB_TransmitPulseType_,
                                            double _SystemFrequency_,
                                            double _SpeedOfSound_,
                                            double _CyclesPerElement_,
                                            double _SNR_,
                                            double _Beta_,
                                            double _NbFudge_)
        {
            #region Sample Rate

            double sumSampling = 0.0;
            // 1200khz
            if (_SystemFrequency_ > DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_1200000 * DEFAULT_CPE_1200000 / _CyclesPerElement_;
            }

            // 600khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_600000 && _SystemFrequency_ < DEFAULT_FREQ_1200000)
            {
                sumSampling += DEFAULT_SAMPLING_600000 * DEFAULT_CPE_600000 / _CyclesPerElement_;
            }

            // 300khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_300000 && _SystemFrequency_ < DEFAULT_FREQ_600000)
            {
                sumSampling += DEFAULT_SAMPLING_300000 * DEFAULT_CPE_300000 / _CyclesPerElement_;
            }

            // 150khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_150000 && _SystemFrequency_ < DEFAULT_FREQ_300000)
            {
                sumSampling += DEFAULT_SAMPLING_150000 * DEFAULT_CPE_150000 / _CyclesPerElement_;
            }

            // 75khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_75000 && _SystemFrequency_ < DEFAULT_FREQ_150000)
            {
                sumSampling += DEFAULT_SAMPLING_75000 * DEFAULT_CPE_75000 / _CyclesPerElement_;
            }

            // 38khz
            else if (_SystemFrequency_ > DEFAULT_FREQ_38000 && _SystemFrequency_ < DEFAULT_FREQ_75000)
            {
                sumSampling += DEFAULT_SAMPLING_38000 * DEFAULT_CPE_38000 / _CyclesPerElement_;
            }

            double sampleRate = _SystemFrequency_ * (sumSampling);

            #endregion

            #region Meters Per Sample

            // Check for divide by 0
            double metersPerSample = 0;
            if (sampleRate == 0)
            {
                metersPerSample = 0.0;
            }
            else
            {
                metersPerSample = Math.Cos(_BeamAngle_ / 180.0 * Math.PI) * _SpeedOfSound_ / 2.0 / sampleRate;
            }

            #endregion

            #region Lag Samples

            // Check for divide by 0
            int lagSamples = 0;
            if (metersPerSample == 0)
            {
                lagSamples = 0;
            }
            else
            {
                lagSamples = 2 * (int)Math.Truncate((Math.Truncate(_CWPBB_LagLength_ / metersPerSample) + 1.0) / 2.0);
            }

            #endregion

            #region Bin Samples

            // Check for divide by 0
            int binSamples = 0;
            if (metersPerSample == 0)
            {
                binSamples = 0;
            }
            else
            {
                binSamples = (int)Math.Truncate(_CWPBS_ / metersPerSample);
            }

            #endregion

            #region Code Repeats

            double codeRepeats = 0;

            // Check for divide by 0
            if (lagSamples == 0)
            {
                codeRepeats = 0;
            }

            // Cased BinSamples and LagSamples to double because Truncate only takes doubles
            // Make the result of Truncate an int
            if (((int)Math.Truncate((double)binSamples / (double)lagSamples)) + 1.0 < 2.0)
            {
                codeRepeats = 2;
            }
            else
            {
                codeRepeats = ((int)Math.Truncate((double)binSamples / (double)lagSamples)) + 1;
            }

            #endregion

            #region rho

            double rho = 0.0;
            if ((int)_CWPBB_TransmitPulseType_ < 2)
            {
                // Check for divide by 0
                if (codeRepeats == 0 || _SNR_ == 0)
                {
                    rho = 0;
                }
                else
                {
                    double snr = Math.Pow(10.0, _SNR_ / 10.0);

                    rho = _Beta_ * ((codeRepeats - 1.0) / codeRepeats) / (1.0 + Math.Pow(1.0 / 10.0, _SNR_ / 10.0));
                }
            }
            else
            {
                rho = _Beta_;
            }

            #endregion

            #region STD Radial

            double stdDevRadial = 0.0;
            // Check for divide by 0
            if (lagSamples == 0 || binSamples == 0)
            {
                stdDevRadial = 0.0;
            }
            else
            {
                stdDevRadial = 0.034 * (118.0 / lagSamples) * Math.Sqrt(14.0 / binSamples) * Math.Pow((rho / 0.5), -2.0);
            }

            #endregion

            #region Broadband STD

            double stdDevSystem = 0.0;
            // Check for divide by 0
            if (_CWPP_ == 0)
            {
                stdDevSystem = 0.0;
            }
            // Use the radial for the standard deviation
            // This is for vertical beams
            else if (_BeamAngle_ == 0)
            {
                stdDevSystem = stdDevRadial;
            }
            else
            {
                stdDevSystem = stdDevRadial / Math.Sqrt(_CWPP_) / Math.Sqrt(2.0) / Math.Sin(_BeamAngle_ / 180.0 * Math.PI);
            }

            #endregion

            #region NbLamda

            double nbLamda = 0.0;

            // Check for divide by 0
            if (_SystemFrequency_ == 0)
            {
                nbLamda = 0.0;
            }
            else
            {
                nbLamda = _SpeedOfSound_ / _SystemFrequency_;
            }

            #endregion

            #region NbTa

            double nbTa = 0.0;

            // Check for divide by 0
            if (_SpeedOfSound_ == 0 || (_BeamAngle_ / 180.0 * Math.PI) == 0)
            {
                nbTa = 0.0;
            }
            else
            {
                nbTa = 2.0 * _CWPBS_ / _SpeedOfSound_ / Math.Cos(_BeamAngle_ / 180.0 * Math.PI);
            }

            #endregion

            #region NbL

            double nbL = 0.5 * _SpeedOfSound_ * nbTa;

            #endregion

            #region Narrowband STD Radial

            double nbStdDevRadial = 0.0;

            // Check for divide by 0
            if (nbL == 0 || _SNR_ == 0)
            {
                nbStdDevRadial = 0;
            }
            else
            {
                nbStdDevRadial = _NbFudge_ * (_SpeedOfSound_ * nbLamda / (8 * Math.PI * nbL)) * Math.Sqrt(1 + 36 / Math.Pow(10, (_SNR_ / 10)) + 30 / Math.Pow(Math.Pow(10, _SNR_ / 10), 2));
            }

            #endregion

            #region Narrowband STD

            double nbStdDevHSystem = 0.0;
            // Check for divide by 0
            if (_CWPP_ == 0 || _BeamAngle_ == 0)
            {
                nbStdDevHSystem = 0.0;
            }
            else
            {
                nbStdDevHSystem = nbStdDevRadial / Math.Sin(_BeamAngle_ / 180 * Math.PI) / Math.Sqrt(2) / Math.Sqrt(_CWPP_);
            }

            #endregion

            // Check if using Broadband or Narrowband
            if ((int)_CWPBB_TransmitPulseType_ > 0)
            {
                return stdDevSystem;                    // Broadband
            }
            else
            {
                return nbStdDevHSystem;                 // Narrowband
            }
        }

        #endregion

        #region Data Storage

        /// <summary>
        /// Get the amount of data storage in bytes.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Number of bytes used in the deployment.</returns>
        public long GetDataStorage(PredictionModelInput input)
        {
            return GetDataStorage(input.CWPBN, input.Beams, input.DeploymentDuration, input.CEI,
                input.CED_IsE0000001, input.CED_IsE0000002, input.CED_IsE0000003, input.CED_IsE0000004,
                input.CED_IsE0000005, input.CED_IsE0000006, input.CED_IsE0000007, input.CED_IsE0000008,
                input.CED_IsE0000009, input.CED_IsE0000010, input.CED_IsE0000011, input.CED_IsE0000012,
                input.CED_IsE0000013, input.CED_IsE0000014, input.CED_IsE0000015);
        }

        /// <summary>
        /// Get the amount of data storage.
        /// </summary>
        /// <param name="_CWPBN_">Number of bins.</param>
        /// <param name="_Beams_">Number of beams.</param>
        /// <param name="_DeploymentDuration_">Deployment duration in days.</param>
        /// <param name="_CEI_">Ensemble intervals in seconds.</param>
        /// <param name="IsE0000001">Flag if E0000001 is turned on.  CED.</param>
        /// <param name="IsE0000002">Flag if E0000002 is turned on.  CED.</param>
        /// <param name="IsE0000003">Flag if E0000003 is turned on.  CED.</param>
        /// <param name="IsE0000004">Flag if E0000004 is turned on.  CED.</param>
        /// <param name="IsE0000005">Flag if E0000005 is turned on.  CED.</param>
        /// <param name="IsE0000006">Flag if E0000006 is turned on.  CED.</param>
        /// <param name="IsE0000007">Flag if E0000007 is turned on.  CED.</param>
        /// <param name="IsE0000008">Flag if E0000008 is turned on.  CED.</param>
        /// <param name="IsE0000009">Flag if E0000009 is turned on.  CED.</param>
        /// <param name="IsE0000010">Flag if E0000010 is turned on.  CED.</param>
        /// <param name="IsE0000011">Flag if E0000011 is turned on.  CED.</param>
        /// <param name="IsE0000012">Flag if E0000012 is turned on.  CED.</param>
        /// <param name="IsE0000013">Flag if E0000013 is turned on.  CED.</param>
        /// <param name="IsE0000014">Flag if E0000014 is turned on.  CED.</param>
        /// <param name="IsE0000015">Flag if E0000015 is turned on.  CED.</param>
        /// <returns>The amount of data consumed in the deployment.</returns>
        public long GetDataStorage(int _CWPBN_,
                                    int _Beams_,
                                    double _DeploymentDuration_,
                                    double _CEI_,
                                    bool IsE0000001,
                                    bool IsE0000002,
                                    bool IsE0000003,
                                    bool IsE0000004,
                                    bool IsE0000005,
                                    bool IsE0000006,
                                    bool IsE0000007,
                                    bool IsE0000008,
                                    bool IsE0000009,
                                    bool IsE0000010,
                                    bool IsE0000011,
                                    bool IsE0000012,
                                    bool IsE0000013,
                                    bool IsE0000014,
                                    bool IsE0000015)
        {
            long ensembleSize = GetEnsembleSize(_CWPBN_, _Beams_, IsE0000001, IsE0000002, IsE0000003, IsE0000004, IsE0000005, IsE0000006, IsE0000007, IsE0000008, IsE0000009, IsE0000010, IsE0000011, IsE0000012, IsE0000013, IsE0000014, IsE0000015);

            #region Number of Ensembles

            long ensembles = (long)Math.Round(_DeploymentDuration_ * 24 * 3600 / _CEI_);

            #endregion

            return ensembles * ensembleSize;
        }

        #endregion

        #region Burst Data Storage

        /// <summary>
        /// Get the amount of data storage in bytes.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Number of bytes used in the deployment.</returns>
        public long GetDataStorageBurst(PredictionModelInput input)
        {
            return GetDataStorageBurst(input.CBI_SamplesPerBurst, input.CBI_BurstInterval, input.CWPBN, input.DeploymentDuration, input.CEI, input.Beams,
                input.CED_IsE0000001, input.CED_IsE0000002, input.CED_IsE0000003, input.CED_IsE0000004,
                input.CED_IsE0000005, input.CED_IsE0000006, input.CED_IsE0000007, input.CED_IsE0000008,
                input.CED_IsE0000009, input.CED_IsE0000010, input.CED_IsE0000011, input.CED_IsE0000012,
                input.CED_IsE0000013, input.CED_IsE0000014, input.CED_IsE0000015);
        }

        /// <summary>
        /// Calculate the number of bytes used per deployment using burst pinging.
        /// </summary>
        /// <param name="_CBI_SamplesPerBurst_">Number of ensembles in a burst.</param>
        /// <param name="_CBI_BurstInterval">Length of time for the burst.</param>
        /// <param name="_CWPBN_">Number of bins.</param>
        /// <param name="_DeploymentDuration_">Number of days in the deployment.</param>
        /// <param name="_CEI_">Time between each ensemble.</param>
        /// <param name="_Beams_">Number of beams.</param>
        /// <param name="IsE0000001">Flag if E0000001 is turned on.  CED.</param>
        /// <param name="IsE0000002">Flag if E0000002 is turned on.  CED.</param>
        /// <param name="IsE0000003">Flag if E0000003 is turned on.  CED.</param>
        /// <param name="IsE0000004">Flag if E0000004 is turned on.  CED.</param>
        /// <param name="IsE0000005">Flag if E0000005 is turned on.  CED.</param>
        /// <param name="IsE0000006">Flag if E0000006 is turned on.  CED.</param>
        /// <param name="IsE0000007">Flag if E0000007 is turned on.  CED.</param>
        /// <param name="IsE0000008">Flag if E0000008 is turned on.  CED.</param>
        /// <param name="IsE0000009">Flag if E0000009 is turned on.  CED.</param>
        /// <param name="IsE0000010">Flag if E0000010 is turned on.  CED.</param>
        /// <param name="IsE0000011">Flag if E0000011 is turned on.  CED.</param>
        /// <param name="IsE0000012">Flag if E0000012 is turned on.  CED.</param>
        /// <param name="IsE0000013">Flag if E0000013 is turned on.  CED.</param>
        /// <param name="IsE0000014">Flag if E0000014 is turned on.  CED.</param>
        /// <param name="IsE0000015">Flag if E0000015 is turned on.  CED.</param>
        /// <returns>Number of bytes in a deployment using burst timing.</returns>
        public long GetDataStorageBurst(int _CBI_SamplesPerBurst_, double _CBI_BurstInterval, int _CWPBN_, double _DeploymentDuration_, double _CEI_,
                                            int _Beams_,
                                            bool IsE0000001,
                                            bool IsE0000002,
                                            bool IsE0000003,
                                            bool IsE0000004,
                                            bool IsE0000005,
                                            bool IsE0000006,
                                            bool IsE0000007,
                                            bool IsE0000008,
                                            bool IsE0000009,
                                            bool IsE0000010,
                                            bool IsE0000011,
                                            bool IsE0000012,
                                            bool IsE0000013,
                                            bool IsE0000014,
                                            bool IsE0000015)
        {
            // Number of bytes per ensemble
            long ensembleSize = GetEnsembleSize(_CWPBN_, _Beams_, IsE0000001, IsE0000002, IsE0000003, IsE0000004, IsE0000005, IsE0000006, IsE0000007, IsE0000008, IsE0000009, IsE0000010, IsE0000011, IsE0000012, IsE0000013, IsE0000014, IsE0000015);

            // Memory per burst
            long burstMem = _CBI_SamplesPerBurst_ * ensembleSize;

            // Get the number of burst per deployment duration
            double deploymentDur = _DeploymentDuration_ * 3600.0 * 24.0;            // Seconds for the deployment duration

            double burstDur = _CBI_BurstInterval;                                // Set the burst length
            if(_CBI_SamplesPerBurst_ * _CEI_ > _CBI_BurstInterval)
            {
                burstDur = _CBI_SamplesPerBurst_ * _CEI_;                        // Check if the time it takes to collect the data is greater than the burst interval
            }

            int numBursts = (int)Math.Round(deploymentDur / burstDur);                 // Divide total duration by burst duration to get number of burst in the deployment

            return burstMem * numBursts;  
        }



        #endregion

        #region EnsembleSize

        /// <summary>
        /// Get the amount of bytes in a single ensemble.
        /// </summary>
        /// <param name="input">Input values.</param>
        /// <returns>Number of bytes used in a single ensemble.</returns>
        public long GetEnsembleSize(PredictionModelInput input)
        {
            return GetEnsembleSize(input.CWPBN, input.Beams,
                input.CED_IsE0000001, input.CED_IsE0000002, input.CED_IsE0000003, input.CED_IsE0000004,
                input.CED_IsE0000005, input.CED_IsE0000006, input.CED_IsE0000007, input.CED_IsE0000008,
                input.CED_IsE0000009, input.CED_IsE0000010, input.CED_IsE0000011, input.CED_IsE0000012,
                input.CED_IsE0000013, input.CED_IsE0000014, input.CED_IsE0000015);
        }

        /// <summary>
        /// Get the ensemble size based off the bins, beams and which datasets are turned on.
        /// </summary>
        /// <param name="_CWPBN_">Number of bins.</param>
        /// <param name="_Beams_">Number of beams.</param>
        /// <param name="IsE0000001">Flag if E0000001 is turned on.  CED.</param>
        /// <param name="IsE0000002">Flag if E0000002 is turned on.  CED.</param>
        /// <param name="IsE0000003">Flag if E0000003 is turned on.  CED.</param>
        /// <param name="IsE0000004">Flag if E0000004 is turned on.  CED.</param>
        /// <param name="IsE0000005">Flag if E0000005 is turned on.  CED.</param>
        /// <param name="IsE0000006">Flag if E0000006 is turned on.  CED.</param>
        /// <param name="IsE0000007">Flag if E0000007 is turned on.  CED.</param>
        /// <param name="IsE0000008">Flag if E0000008 is turned on.  CED.</param>
        /// <param name="IsE0000009">Flag if E0000009 is turned on.  CED.</param>
        /// <param name="IsE0000010">Flag if E0000010 is turned on.  CED.</param>
        /// <param name="IsE0000011">Flag if E0000011 is turned on.  CED.</param>
        /// <param name="IsE0000012">Flag if E0000012 is turned on.  CED.</param>
        /// <param name="IsE0000013">Flag if E0000013 is turned on.  CED.</param>
        /// <param name="IsE0000014">Flag if E0000014 is turned on.  CED.</param>
        /// <param name="IsE0000015">Flag if E0000015 is turned on.  CED.</param>
        /// <returns>Size of an ensemble in bytes.</returns>
        public long GetEnsembleSize(int _CWPBN_,
                                    int _Beams_,
                                    bool IsE0000001,
                                    bool IsE0000002,
                                    bool IsE0000003,
                                    bool IsE0000004,
                                    bool IsE0000005,
                                    bool IsE0000006,
                                    bool IsE0000007,
                                    bool IsE0000008,
                                    bool IsE0000009,
                                    bool IsE0000010,
                                    bool IsE0000011,
                                    bool IsE0000012,
                                    bool IsE0000013,
                                    bool IsE0000014,
                                    bool IsE0000015)
        {
            #region E0000001

            int E0000001 = 0;
            if (IsE0000001)
            {
                E0000001 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000002

            int E0000002 = 0;
            if (IsE0000002)
            {
                E0000002 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000003

            int E0000003 = 0;
            if (IsE0000003)
            {
                E0000003 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000004

            int E0000004 = 0;
            if (IsE0000004)
            {
                E0000004 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000005

            int E0000005 = 0;
            if (IsE0000005)
            {
                E0000005 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000006

            int E0000006 = 0;
            if (IsE0000006)
            {
                E0000006 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000007

            int E0000007 = 0;
            if (IsE0000007)
            {
                E0000007 = 4 * (_CWPBN_ * _Beams_ + 7);
            }

            #endregion

            #region E0000008

            int E0000008 = 0;
            if (IsE0000008)
            {
                E0000008 = 4 * (23 + 7);
            }

            #endregion

            #region E0000009

            int E0000009 = 0;
            if (IsE0000009)
            {
                E0000009 = 4 * (19 + 7);
            }

            #endregion

            #region E0000010

            int E0000010 = 0;
            if (IsE0000010)
            {
                E0000010 = 4 * (14 + 15 * _Beams_ + 7);
            }

            #endregion

            #region E0000011

            int E0000011 = 0;
            if (IsE0000011)
            {
                E0000011 = 0;
            }

            #endregion

            #region E0000012

            int E0000012 = 0;
            if (IsE0000012)
            {
                E0000012 = 4 * (23 + 7);
            }

            #endregion

            #region E0000013

            int E0000013 = 0;
            if (IsE0000013)
            {
                E0000013 = 4 * (30 + 7);
            }

            #endregion

            #region E0000014

            int E0000014 = 0;
            if (IsE0000014)
            {
                E0000014 = 4 * (23 + 7);
            }

            #endregion

            #region E0000015

            int E0000015 = 0;
            if (IsE0000015)
            {
                E0000015 = 4 * (8 * _Beams_ + 1 + 7);
            }

            #endregion

            int bytesPerEnsemble = E0000001 + E0000002 + E0000003 + E0000004 + E0000005 + E0000006 + E0000007 + E0000008 + E0000009 + E0000010 + E0000011 + E0000012 + E0000013 + E0000014 + E0000015;
            int checksum = 4;
            int wrapper = 32;       // Header

            return bytesPerEnsemble + checksum + wrapper;
        }

        #endregion

        #region Absorption

        /// <summary>
        /// Absorption of water calculation.
        /// </summary>
        /// <param name="_freq_">System frequency in kHz.</param>
        /// <param name="_speedOfSound_">Speed of sound in m/s</param>
        /// <param name="_salinity_">Salinity in ppt.</param>
        /// <param name="_temperature_">Temperature in celcuis.</param>
        /// <param name="_xdcrDepth_">Depth of the transducer in meters.</param>
        /// <returns>Absorption of the water in dB/m.</returns>
        public double CalcAbsorption(double _freq_, double _speedOfSound_, double _salinity_, double _temperature_, double _xdcrDepth_)
        {
            if(_speedOfSound_ == 0 || _salinity_ == 0 || _freq_ == 0)
            {
                return 0;
            }

            const double pH = 8.0;
            const double P1 = 1.0;

            #region Frequency
 
            double freq = _freq_ / 1000.0;

            #endregion

            #region A1
            
            // dB Km^-1 KHz^-1
            double A1 = 8.68 / _speedOfSound_ * Math.Pow(10.0, 0.78 * pH - 5.0);

            #endregion

            #region f1

            // kHz
            double f1 = 2.8 * Math.Pow(_salinity_ / 35.0, 0.5) * Math.Pow(10.0, 4.0 - 1245 / (273.0 + _temperature_));

            #endregion

            #region A2 

            // dB km^-1 kHz^-1
            double A2 = 21.44 * _salinity_ / _speedOfSound_ * (1.0 + 0.025 * _temperature_);

            #endregion

            #region P2

            double P2 = 1.0 - 1.37 * Math.Pow(10.0, -4.0) * _xdcrDepth_ + 6.2 * Math.Pow(10, -9.0) * Math.Pow(_xdcrDepth_, 2);

            #endregion

            #region f2

            // kHz
            double f2 = 8.17 * Math.Pow(10.0, 8.0 - 1990.0 / (273.0 + _temperature_)) / (1.0 + 0.0018 * (_salinity_ - 35.0));

            #endregion

            #region A3

            double A3 = 4.93 * Math.Pow(10.0, -4) - 2.59 * Math.Pow(10.0, -5.0) * _temperature_ + 9.11 * Math.Pow(10.0, -7.0) * Math.Pow(_temperature_, 2.0);

            #endregion

            #region P3

            double P3 = 1.0 - 3.83 * Math.Pow(10.0, -5.0) * _xdcrDepth_ + 4.9 * Math.Pow(10.0, -10.0) * Math.Pow(_xdcrDepth_, 2.0);

            #endregion

            #region Boric Acid Relaxation

            double bar = A1 * P1 * f1 * Math.Pow(freq, 2.0) / (Math.Pow(freq, 2.0) + Math.Pow(f1, 2.0)) / 1000.0; 

            #endregion

            #region MgSO3 Magnesium Sulphate Relaxation

            double msr = A2 * P2 * f2 * Math.Pow(freq, 2.0) / (Math.Pow(freq, 2.0) + Math.Pow(f2, 2.0)) / 1000.0;

            #endregion

            #region Freshwater Attenuation

            double fa = A3 * P3 * Math.Pow(freq, 2.0) / 1000.0;

            #endregion

            #region Absorption

            double absorption = bar + msr + fa;

            #endregion

            return absorption;

        }

        #endregion

    }
}
