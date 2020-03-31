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
 * 10/18/2017      RC          4.5.0      Initial commit
 * 01/03/2018      RC          4.6.0      Changed System Init Time from 0.25sec to 2.4sec.
 * 01/12/2018      RC          4.7.0      Added absorption values.
 * 01/16/2018      RC          4.7.1      Changed System Init Time from 2.4sec to 2.6sec.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// User input for the ADCP predictor.
    /// </summary>
    public class PredictionModelInput
    {
        #region Variables

        /// <summary>
        /// Default fudge value for Narrowband calculations.
        /// </summary>
        public const double DEFAULT_FUDGE = 1.4;

        #region Deployment

        /// <summary>
        /// Default deployment duration in days.
        /// </summary>
        public const int DEFAULT_DEPLOY_DUR = 1;

        #endregion

        #region Defaults

        /// <summary>
        /// Default ensemble interval in seconds.
        /// </summary>
        public const int DEFAULT_CEI = 1;

        #region Water Profile

        /// <summary>
        /// Default Water Profile on.
        /// </summary>
        public const bool DEFAULT_CWPON = true;

        /// <summary>
        /// Default Water Profile Time between pings in seconds.
        /// 
        /// Default 300kHz value.
        /// </summary>
        public const float DEFAULT_CWPTBP = 0.5f;

        /// <summary>
        /// Default number of bins for Water Profile.
        /// </summary>
        public const int DEFAULT_CWPBN = 30;

        /// <summary>
        /// Default Water Profile bin size in meters.
        /// </summary>
        public const float DEFAULT_CWPBS = 4.00f;

        /// <summary>
        /// Default Water Profile Blank size in meters.
        /// </summary>
        public const float DEFAULT_CWPBL = 0.4f;

        /// <summary>
        /// Default Water Profile Lag length in meters.
        /// </summary>
        public const double DEFAULT_WP_LAG_LENGTH = 0.5;

        /// <summary>
        /// Default Water Profile Broadband Transmit Pulse Type.
        /// </summary>
        public const RTI.Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType DEFAULT_CWPBB_TRANSMIT_PULSE_TYPE = RTI.Commands.AdcpSubsystemCommands.DEFAULT_CWPBB_TRANSMITPULSETYPE;

        /// <summary>
        /// Default Water Profile pings per ensemble.
        /// </summary>
        public const int DEFAULT_CWPP = 1;

        #endregion

        #region Bottom Track

        /// <summary>
        /// Default Bottom Track on.
        /// </summary>
        public const bool DEFAULT_CBTON = true;

        /// <summary>
        /// Default Bottom Track Time between pings in seconds.
        /// </summary>
        public const float DEFAULT_CBTTBP = 0.25f;

        /// <summary>
        /// Default Water Profile Broadband Transmit Pulse Type.
        /// </summary>
        public const RTI.Commands.AdcpSubsystemCommands.eCBTBB_Mode DEFAULT_CBTBB_TRANSMIT_PULSE_TYPE = RTI.Commands.AdcpSubsystemCommands.DEFAULT_CBTBB_MODE;

        #endregion

        #endregion

        #region Batteries

        /// <summary>
        /// Default battery selected.
        /// </summary>
        public const DeploymentOptions.AdcpBatteryType DEFAULT_BATTERY_TYPE = DeploymentOptions.AdcpBatteryType.Alkaline_38C;

        /// <summary>
        /// Default power for the battery chosen in wh-hr.
        /// </summary>
        public const int DEFAULT_BATTERY_POWER = (int)DEFAULT_BATTERY_TYPE;

        /// <summary>
        /// Default battery derate.
        /// </summary>
        public const double DEFAULT_BATTERY_DERATE = 0.85;

        /// <summary>
        /// Default Battery Discharge per year.
        /// </summary>
        public const double DEFAULT_BATTERY_SELF_DISCHARGE_PER_YEAR = 0.05;

        #endregion

        #region XDCR

        /// <summary>
        /// Default frequency for a system in Hz.
        /// 300kHz system.
        /// </summary>
        public const double DEFAULT_SYS_FREQ = 311281.25;

        /// <summary>
        /// Default speed of sound in m/s.
        /// </summary>
        public const int DEFAULT_SPEED_OF_SOUND = 1490;

        /// <summary>
        /// Default beam angle in degrees.
        /// </summary>
        public const int DEFAULT_BEAM_ANGLE = 20;

        /// <summary>
        /// Default cycles per element in cycles.
        /// </summary>
        public const int DEFAULT_CYCLES_PER_ELEMENT = 12;

        /// <summary>
        /// Default Broadband power.
        /// </summary>
        public const bool DEFAULT_BROADBAND_POWER = true;

        /// <summary>
        /// Default BETA.
        /// </summary>
        public const double DEFAULT_BETA = 1.0;

        /// <summary>
        /// Default signal to noise ratio.
        /// </summary>
        public const double DEFAULT_SNR = 30.0;

        /// <summary>
        /// Default number of beams.
        /// </summary>
        public const int DEFAULT_BEAMS = 4;

        /// <summary>
        /// Default beam diameter in meters.  Using default 300kHz 3" diameter.
        /// 3" = 0.076m.
        /// 0.075
        /// </summary>
        public const double DEFAULT_BEAM_DIAMETER = RTI.Core.Commons.CERAMIC_DIA_300_3;

        #endregion

        #region Power

        /// <summary>
        /// Default System Boot Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_BOOT_PWR = 1.80;

        /// <summary>
        /// Default System Init Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_INIT_PWR = 2.80;

        /// <summary>
        /// Default System Save Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_SAVE_PWR = 1.80;

        /// <summary>
        /// Default System Sleep Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_SLEEP_PWR = 0.024;

        #endregion

        #region Time

        /// <summary>
        /// Default time for the system to wakeup in seconds.
        /// </summary>
        public const double DEFAULT_SYS_WAKEUP_TIME = 0.4;

        /// <summary>
        /// Default time for the system to initialize in seconds.
        /// </summary>
        public const double DEFAULT_SYS_INIT_TIME = 2.6;

        /// <summary>
        /// Default time for the system to Save in seconds.
        /// </summary>
        public const double DEFAULT_SYS_SAVE_TIME = 0.15;

        #endregion

        #region Absortpion

        /// <summary>
        /// Default Temperature of the water in celcuis.
        /// </summary>
        public const double DEFAULT_TEMPERATURE = 10;

        /// <summary>
        /// Default salinity of the water.
        /// </summary>
        public const double DEFAULT_SALINITY = 35;

        /// <summary>
        /// Default depth of the transducer.
        /// </summary>
        public const double DEFAULT_XDCR_DEPTH = 0;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Subsystem for the user inputs.
        /// This is need to determine the frequency dependent values.
        /// </summary>
        private Subsystem _SubSystem;
        /// <summary>
        /// Subsystem for the user inputs.
        /// This is need to determine the frequency dependent values.
        /// </summary>
        public Subsystem SubSystem
        {
            get
            {
                return _SubSystem;
            }
            set
            {
                _SubSystem = value;

                // Also update the frequency value
                // And update the values based off the new subsystem
                SetSystemFrequency(value);
            }
        }

        /// <summary>
        /// System frequency in Hz.
        /// </summary>
        public double SystemFrequency { get; set; }

        /// <summary>
        /// Number of days the deployment will go for.
        /// </summary>
        public double DeploymentDuration { get; set; }

        /// <summary>
        /// Ensemble interval in seconds.
        /// Seconds between ensembles.
        /// Time per ensemble.
        /// </summary>
        public double CEI
        {
            get;
            set;
        }

        /// <summary>
        /// Speed of sound in meters/second.
        /// Default is 1490 m/s.
        /// </summary>
        public double SpeedOfSound { get; set; }

        /// <summary>
        /// Fudge factor for narrowband.
        /// </summary>
        public double NbFudge { get; set; }

        #region Bottom Track

        /// <summary>
        /// On/Off for Bottom Track data.
        /// TRUE = Bottom Track On
        /// FALSE = Bottom Track Off
        /// </summary>
        public bool CBTON { get; set; }

        /// <summary>
        /// Time between Bottom Track pings.
        /// Time in seconds.
        /// </summary>
        public float CBTTBP { get; set; }

        /// <summary>
        /// Bottom Track broadband Transmit Pulse Type.
        /// </summary>
        public RTI.Commands.AdcpSubsystemCommands.eCBTBB_Mode CBTBB_TransmitPulseType { get; set; }

        #endregion

        #region Water Profile

        /// <summary>
        /// On/Off for Water Profile data.
        /// TRUE = Water Profile On
        /// FALSE = Water Profile Off
        /// </summary>
        public bool CWPON { get; set; }

        /// <summary>
        /// Time between Water Profile pings.
        /// Time in seconds.
        /// </summary>
        public float CWPTBP { get; set; }

        /// <summary>
        /// Water Profile bins.
        /// Number of Water Profile bins.
        /// </summary>
        public ushort CWPBN { get; set; }

        /// <summary>
        /// Size of a Water Profile bin.
        /// Size is in meters.
        /// </summary>
        public float CWPBS { get; set; }

        /// <summary>
        /// Size of the Water Profie Blank.
        /// Size in meters.
        /// </summary>
        public float CWPBL { get; set; }

        /// <summary>
        /// Water Profile broadband Transmit Pulse Type.
        /// </summary>
        public RTI.Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType CWPBB_TransmitPulseType { get; set; }

        /// <summary>
        /// Lag length for Water Profile.
        /// Lag length is in meters.
        /// </summary>
        public double CWPBB_LagLength { get; set; }

        /// <summary>
        /// Pings per ensemble.
        /// Number of pings.
        /// </summary>
        public ushort CWPP { get; set; }

        #endregion

        #region Burst

        /// <summary>
        /// Flag if this is a burst calculation.
        /// </summary>
        public bool IsBurst { get; set; }

        /// <summary>
        /// Number of ensembles per burst in the CBI command.
        /// </summary>
        public int CBI_SamplesPerBurst { get; set; }

        /// <summary>
        /// Lefth of burst in seconds for the CBI command.
        /// </summary>
        public double CBI_BurstInterval { get; set; }

        /// <summary>
        /// Set the flag if this burst is interleaved with the next burst.
        /// </summary>
        public int CBI_IsInterleaved { get; set; }

        #endregion

        #region CED

        /// <summary>
        /// Is dataset E0000001 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000001 { get; set; }

        /// <summary>
        /// Is dataset E0000002 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000002 { get; set; }

        /// <summary>
        /// Is dataset E0000003 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000003 { get; set; }

        /// <summary>
        /// Is dataset E0000004 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000004 { get; set; }

        /// <summary>
        /// Is dataset E0000005 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000005 { get; set; }

        /// <summary>
        /// Is dataset E0000006 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000006 { get; set; }

        /// <summary>
        /// Is dataset E0000007 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000007 { get; set; }

        /// <summary>
        /// Is dataset E0000008 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000008 { get; set; }

        /// <summary>
        /// Is dataset E0000009 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000009 { get; set; }

        /// <summary>
        /// Is dataset E0000010 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000010 { get; set; }

        /// <summary>
        /// Is dataset E0000011 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000011 { get; set; }

        /// <summary>
        /// Is dataset E0000012 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000012 { get; set; }

        /// <summary>
        /// Is dataset E0000013 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000013 { get; set; }

        /// <summary>
        /// Is dataset E0000014 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000014 { get; set; }

        /// <summary>
        /// Is dataset E0000015 turned on or off in CED command.
        /// </summary>
        public bool CED_IsE0000015 { get; set; }

        #endregion

        #region Batteries

        /// <summary>
        /// Battery Type.  This will also set BatteryWattHr.
        /// </summary>
        public DeploymentOptions.AdcpBatteryType BatteryType { get; set; }

        /// <summary>
        /// Battery derate.
        /// </summary>
        public double BatteryDerate { get; set; }

        /// <summary>
        /// Discharge rate per year of a battery.
        /// </summary>
        public double BatterySelfDischargePerYear { get; set; }

        #endregion

        #region XDCR

        /// <summary>
        /// Beam angle of the transducer.
        /// </summary>
        public double BeamAngle { get; set; }

        /// <summary>
        /// Diameter of each beam.
        /// </summary>
        public double BeamDiameter { get; set; }

        /// <summary>
        /// Cycles per element.
        /// 100/value = % bw.
        /// </summary>
        public int CyclesPerElement { get; set; }

        /// <summary>
        /// Broadband power.
        /// </summary>
        public bool BroadbandPower { get; set; }

        /// <summary>
        /// Beta.
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// Signal to Noise ratio.
        /// 10*LOG(value) = dB
        /// </summary>
        public double SNR { get; set; }

        /// <summary>
        /// Number of beams on the transducer.
        /// </summary>
        public int Beams { get; set; }

        /// <summary>
        /// System Boot Power in watts.
        /// </summary>
        public double SystemBootPower { get; set; }

        /// <summary>
        /// System Init Power in watts.
        /// </summary>
        public double SystemInitPower { get; set; }

        /// <summary>
        /// System Save Power in watts.
        /// </summary>
        public double SystemSavePower { get; set; }

        /// <summary>
        /// Power used when System is asleep in watts.
        /// </summary>
        public double SystemSleepPower { get; set; }

        /// <summary>
        /// Time it takes for the System to wake in 
        /// seconds.
        /// </summary>
        public double SystemWakeupTime { get; set; }


        /// <summary>
        /// Time it takes for the System to initialize in 
        /// seconds.
        /// </summary>
        public double SystemInitTime { get; set; }

        /// <summary>
        /// Time it takes for the System to Save in 
        /// seconds.
        /// </summary>
        public double SystemSaveTime { get; set; }

        #endregion

        #region Absorption

        /// <summary>
        /// Temperature of the water in degrees Celcuis.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Salinity of the water in ppt.
        /// </summary>
        public double Salinity { get; set; }

        /// <summary>
        /// Depth of the transducer in meters.
        /// </summary>
        public double XdcrDepth { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public PredictionModelInput(Subsystem ss)
        {
            // Initialize Values
            _SubSystem = ss;

            NbFudge = DEFAULT_FUDGE;

            // Deployment
            DeploymentDuration = DEFAULT_DEPLOY_DUR;

            // Commands
            CEI = DEFAULT_CEI;

            // WP Commands
            CWPON = DEFAULT_CWPON;
            CWPTBP = DEFAULT_CWPTBP;
            CWPBN = DEFAULT_CWPBN;
            CWPBS = DEFAULT_CWPBS;
            CWPBL = DEFAULT_CWPBL;
            CWPBB_LagLength = DEFAULT_WP_LAG_LENGTH;
            CWPBB_TransmitPulseType = DEFAULT_CWPBB_TRANSMIT_PULSE_TYPE;
            CWPP = DEFAULT_CWPP;

            // BT Commands
            CBTON = DEFAULT_CBTON;
            CBTTBP = DEFAULT_CBTTBP;
            CBTBB_TransmitPulseType = DEFAULT_CBTBB_TRANSMIT_PULSE_TYPE;

            // Absorption
            Temperature = DEFAULT_TEMPERATURE;
            Salinity = DEFAULT_SALINITY;
            XdcrDepth = DEFAULT_XDCR_DEPTH;

            // CBI
            IsBurst = false;
            CBI_SamplesPerBurst = 4096;
            CBI_BurstInterval = 3600;
            CBI_IsInterleaved = 0;

            // CED
            CED_IsE0000001 = true;
            CED_IsE0000002 = true;
            CED_IsE0000003 = true;
            CED_IsE0000004 = true;
            CED_IsE0000005 = true;
            CED_IsE0000006 = true;
            CED_IsE0000007 = true;
            CED_IsE0000008 = true;
            CED_IsE0000009 = true;
            CED_IsE0000010 = true;
            CED_IsE0000011 = true;
            CED_IsE0000012 = true;
            CED_IsE0000013 = true;
            CED_IsE0000014 = true;
            CED_IsE0000015 = true;

            // Batteries
            BatteryType = DEFAULT_BATTERY_TYPE;
            BatteryDerate = DEFAULT_BATTERY_DERATE;
            BatterySelfDischargePerYear = DEFAULT_BATTERY_SELF_DISCHARGE_PER_YEAR;

            // XDCR
            SystemFrequency = DEFAULT_SYS_FREQ;
            SpeedOfSound = DEFAULT_SPEED_OF_SOUND;
            BeamAngle = DEFAULT_BEAM_ANGLE;
            CyclesPerElement = DEFAULT_CYCLES_PER_ELEMENT;
            BroadbandPower = DEFAULT_BROADBAND_POWER;
            Beta = DEFAULT_BETA;
            SNR = DEFAULT_SNR;
            Beams = DEFAULT_BEAMS;
            BeamDiameter = DEFAULT_BEAM_DIAMETER;

            // Power
            SystemBootPower = DEFAULT_SYS_BOOT_PWR;
            SystemInitPower = DEFAULT_SYS_INIT_PWR;
            SystemSavePower = DEFAULT_SYS_SAVE_PWR;
            SystemSleepPower = DEFAULT_SYS_SLEEP_PWR;

            // Time
            SystemWakeupTime = DEFAULT_SYS_WAKEUP_TIME;
            SystemInitTime = DEFAULT_SYS_INIT_TIME;
            SystemSaveTime = DEFAULT_SYS_SAVE_TIME;

            // If no subsystem is set, then set a a default subsystem based
            // off the default frequency (300KHz)
            if (ss.IsEmpty())
            {
                // Use the default frequency
                SubSystem = new Subsystem(Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4, 0);
            }

            // Load the subsystem dependent values
            SetSubsystemValues(ss);
        }

        /// <summary>
        /// Initialize the values.  No subsystem is given so a default subsystem of a 
        /// 300kHz piston system will be used.
        /// </summary>
        public PredictionModelInput()
        {
            // Initialize Values
            NbFudge = DEFAULT_FUDGE;

            // Deployment
            DeploymentDuration = DEFAULT_DEPLOY_DUR;

            // Commands
            CEI = DEFAULT_CEI;

            // WP Commands
            CWPON = DEFAULT_CWPON;
            CWPTBP = DEFAULT_CWPTBP;
            CWPBN = DEFAULT_CWPBN;
            CWPBS = DEFAULT_CWPBS;
            CWPBL = DEFAULT_CWPBL;
            CWPBB_LagLength = DEFAULT_WP_LAG_LENGTH;
            CWPBB_TransmitPulseType = DEFAULT_CWPBB_TRANSMIT_PULSE_TYPE;
            CWPP = DEFAULT_CWPP;

            // BT Commands
            CBTON = DEFAULT_CBTON;
            CBTTBP = DEFAULT_CBTTBP;
            CBTBB_TransmitPulseType = DEFAULT_CBTBB_TRANSMIT_PULSE_TYPE;

            // Absorption
            Temperature = DEFAULT_TEMPERATURE;
            Salinity = DEFAULT_SALINITY;
            XdcrDepth = DEFAULT_XDCR_DEPTH;

            // CBI
            IsBurst = false;
            CBI_SamplesPerBurst = 4096;
            CBI_BurstInterval = 3600;
            CBI_IsInterleaved = 0;

            // CED
            CED_IsE0000001 = true;
            CED_IsE0000002 = true;
            CED_IsE0000003 = true;
            CED_IsE0000004 = true;
            CED_IsE0000005 = true;
            CED_IsE0000006 = true;
            CED_IsE0000007 = true;
            CED_IsE0000008 = true;
            CED_IsE0000009 = true;
            CED_IsE0000010 = true;
            CED_IsE0000011 = true;
            CED_IsE0000012 = true;
            CED_IsE0000013 = true;
            CED_IsE0000014 = true;
            CED_IsE0000015 = true;

            // Batteries
            BatteryType = DEFAULT_BATTERY_TYPE;
            BatteryDerate = DEFAULT_BATTERY_DERATE;
            BatterySelfDischargePerYear = DEFAULT_BATTERY_SELF_DISCHARGE_PER_YEAR;

            // XDCR
            SystemFrequency = DEFAULT_SYS_FREQ;
            SpeedOfSound = DEFAULT_SPEED_OF_SOUND;
            BeamAngle = DEFAULT_BEAM_ANGLE;
            CyclesPerElement = DEFAULT_CYCLES_PER_ELEMENT;
            BroadbandPower = DEFAULT_BROADBAND_POWER;
            Beta = DEFAULT_BETA;
            SNR = DEFAULT_SNR;
            Beams = DEFAULT_BEAMS;
            BeamDiameter = DEFAULT_BEAM_DIAMETER;

            // Power
            SystemBootPower = DEFAULT_SYS_BOOT_PWR;
            SystemInitPower = DEFAULT_SYS_INIT_PWR;
            SystemSavePower = DEFAULT_SYS_SAVE_PWR;
            SystemSleepPower = DEFAULT_SYS_SLEEP_PWR;

            // Time
            SystemWakeupTime = DEFAULT_SYS_WAKEUP_TIME;
            SystemInitTime = DEFAULT_SYS_INIT_TIME;
            SystemSaveTime = DEFAULT_SYS_SAVE_TIME;

            // If no subsystem is set, then set a a default subsystem based
            // off the default frequency (300KHz)
            // Use the default frequency
            _SubSystem = new Subsystem(Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4, 0);

            // Load the subsystem dependent values
            SetSubsystemValues(SubSystem);
        }

        #region Subsystems

        /// <summary>
        /// Set the values based off the Subsystem code.
        /// </summary>
        /// <param name="ss">Subsystem to get the code.</param>
        private void SetSubsystemValues(Subsystem ss)
        {
            switch (ss.Code)
            {
                case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_2:
                case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_45OFFSET_6:
                case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_OPPOSITE_FACING_c:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_1200;                      // Frequency
                    BeamAngle = 20;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_1200_2;                                                 // Default 1200kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPBS;                                          // Default 1200kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPBN;                                          // Default 1200kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPBL;                                          // Default 1200kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPP;                                            // Default 1200kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPTBP;                                        // Default 1200kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_1200_CBTTBP;                                        // Default 1200kHz BT Time between pings
                    break;
                case Subsystem.SUB_1_2MHZ_VERT_PISTON_A:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_1200;                      // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_1200_2;                                                 // Default 1200kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPBS;                                          // Default 1200kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPBN;                                          // Default 1200kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPBL;                                          // Default 1200kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPP;                                            // Default 1200kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_1200_CWPTBP;                                        // Default 1200kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_1200_CBTTBP;                                        // Default 1200kHz BT Time between pings
                    break;
                case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_3:
                case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_45OFFSET_7:
                case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_OPPOSITE_FACING_d:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 20;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBS;                                           // Default 600kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBN;                                           // Default 600kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBL;                                           // Default 600kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPP;                                             // Default 600kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPTBP;                                         // Default 600kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CBTTBP;                                         // Default 600kHz BT Time between pings
                    break;
                case Subsystem.SUB_600KHZ_4BEAM_30DEG_ARRAY_I:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBS;                                           // Default 600kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBN;                                           // Default 600kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBL;                                           // Default 600kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPP;                                             // Default 600kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPTBP;                                         // Default 600kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CBTTBP;                                         // Default 600kHz BT Time between pings
                    break;
                case Subsystem.SUB_600KHZ_4BEAM_15DEG_ARRAY_O:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBS;                                           // Default 600kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBN;                                           // Default 600kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBL;                                           // Default 600kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPP;                                             // Default 600kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPTBP;                                         // Default 600kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CBTTBP;                                         // Default 600kHz BT Time between pings
                    break;
                case Subsystem.SUB_600KHZ_VERT_PISTON_B:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBS;                                           // Default 600kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBN;                                           // Default 600kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPBL;                                           // Default 600kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPP;                                             // Default 600kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CWPTBP;                                         // Default 600kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_600_CBTTBP;                                         // Default 600kHz BT Time between pings
                    break;
                case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4:
                case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_45OFFSET_8:
                case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_OPPOSITE_FACING_e:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 20;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBS;                                           // Default 300kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBN;                                           // Default 300kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;                                           // Default 300kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPP;                                             // Default 300kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPTBP;                                         // Default 300kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CBTTBP;                                         // Default 300kHz BT Time between pings
                    break;
                case Subsystem.SUB_300KHZ_4BEAM_30DEG_ARRAY_J:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBS;                                           // Default 300kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBN;                                           // Default 300kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;                                           // Default 300kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPP;                                             // Default 300kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPTBP;                                         // Default 300kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CBTTBP;                                         // Default 300kHz BT Time between pings
                    break;
                case Subsystem.SUB_300KHZ_4BEAM_15DEG_ARRAY_P:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBS;                                           // Default 300kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBN;                                           // Default 300kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;                                           // Default 300kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPP;                                             // Default 300kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPTBP;                                         // Default 300kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CBTTBP;                                         // Default 300kHz BT Time between pings
                    break;
                case Subsystem.SUB_300KHZ_1BEAM_0DEG_ARRAY_V:
                case Subsystem.SUB_300KHZ_VERT_PISTON_C:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBS;                                           // Default 300kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBN;                                           // Default 300kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;                                           // Default 300kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPP;                                             // Default 300kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPTBP;                                         // Default 300kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_300_CBTTBP;                                         // Default 300kHz BT Time between pings
                    break;
                case Subsystem.SUB_150KHZ_1BEAM_0DEG_ARRAY_W:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBS;                                           // Default 150kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBN;                                           // Default 150kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBL;                                           // Default 150kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPP;                                             // Default 150kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPTBP;                                         // Default 150kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CBTTBP;                                         // Default 150kHz BT Time between pings
                    break;
                case Subsystem.SUB_150KHZ_4BEAM_15DEG_ARRAY_Q:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBS;                                           // Default 150kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBN;                                           // Default 150kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBL;                                           // Default 150kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPP;                                             // Default 150kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPTBP;                                         // Default 150kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CBTTBP;                                         // Default 150kHz BT Time between pings
                    break;
                case Subsystem.SUB_150KHZ_4BEAM_30DEG_ARRAY_K:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBS;                                           // Default 150kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBN;                                           // Default 150kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBL;                                           // Default 150kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPP;                                             // Default 150kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPTBP;                                         // Default 150kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CBTTBP;                                         // Default 150kHz BT Time between pings
                    break;
                case Subsystem.SUB_150KHZ_VERT_PISTON_D:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBS;                                           // Default 150kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBN;                                           // Default 150kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPBL;                                           // Default 150kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPP;                                             // Default 150kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CWPTBP;                                         // Default 150kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_150_CBTTBP;                                         // Default 150kHz BT Time between pings
                    break;
                case Subsystem.SUB_38KHZ_1BEAM_0DEG_ARRAY_Y:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBS;                                            // Default 38kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBN;                                            // Default 38kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBL;                                            // Default 38kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPP;                                              // Default 38kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPTBP;                                          // Default 38kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CBTTBP;                                          // Default 38kHz BT Time between pings
                    break;
                case Subsystem.SUB_38KHZ_4BEAM_15DEG_ARRAY_S:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBS;                                            // Default 38kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBN;                                            // Default 38kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBL;                                            // Default 38kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPP;                                              // Default 38kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPTBP;                                          // Default 38kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CBTTBP;                                          // Default 38kHz BT Time between pings
                    break;
                case Subsystem.SUB_38KHZ_4BEAM_30DEG_ARRAY_M:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBS;                                            // Default 38kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBN;                                            // Default 38kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBL;                                            // Default 38kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPP;                                              // Default 38kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPTBP;                                          // Default 38kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CBTTBP;                                          // Default 38kHz BT Time between pings
                    break;
                case Subsystem.SUB_38KHZ_VERT_PISTON_F:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBS;                                            // Default 38kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBN;                                            // Default 38kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPBL;                                            // Default 38kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPP;                                              // Default 38kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CWPTBP;                                          // Default 38kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_38_CBTTBP;                                          // Default 38kHz BT Time between pings
                    break;
                case Subsystem.SUB_75KHZ_1BEAM_0DEG_ARRAY_X:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBS;                                            // Default 75kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBN;                                            // Default 75kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBL;                                            // Default 75kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPP;                                              // Default 75kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPTBP;                                          // Default 75kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CBTTBP;                                          // Default 75kHz BT Time between pings
                    break;
                case Subsystem.SUB_75KHZ_4BEAM_15DEG_ARRAY_R:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBS;                                            // Default 75kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBN;                                            // Default 75kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBL;                                            // Default 75kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPP;                                              // Default 75kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPTBP;                                          // Default 75kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CBTTBP;                                          // Default 75kHz BT Time between pings
                    break;
                case Subsystem.SUB_75KHZ_4BEAM_30DEG_ARRAY_L:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBS;                                            // Default 75kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBN;                                            // Default 75kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBL;                                            // Default 75kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPP;                                              // Default 75kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPTBP;                                          // Default 75kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CBTTBP;                                          // Default 75kHz BT Time between pings
                    break;
                case Subsystem.SUB_75KHZ_VERT_PISTON_E:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    CWPBS = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBS;                                            // Default 75kHz Bin Size
                    CWPBN = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBN;                                            // Default 75kHz Number of bins
                    CWPBL = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPBL;                                            // Default 75kHz Blank
                    CWPP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPP;                                              // Default 75kHz Pings per ensemble
                    CWPTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CWPTBP;                                          // Default 75kHz WP Time between pings
                    CBTTBP = Commands.AdcpSubsystemCommands.DEFAULT_75_CBTTBP;                                          // Default 75kHz BT Time between pings
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set the system frquency based off the subsystem given.
        /// </summary>
        /// <param name="ss">Subsystem to get the frequency.</param>
        private void SetSystemFrequency(Subsystem ss)
        {
            switch (ss.Code)
            {
                case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_2:
                case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_45OFFSET_6:
                case Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_OPPOSITE_FACING_c:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_1200;                      // Frequency
                    BeamAngle = 20;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_1200_2;                                                 // Default 1200kHz Beam Diameter
                    break;
                case Subsystem.SUB_1_2MHZ_VERT_PISTON_A:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_1200;                      // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_1200_2;                                                 // Default 1200kHz Beam Diameter
                    break;
                case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_3:
                case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_45OFFSET_7:
                case Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_OPPOSITE_FACING_d:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 20;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    break;
                case Subsystem.SUB_600KHZ_4BEAM_30DEG_ARRAY_I:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    break;
                case Subsystem.SUB_600KHZ_4BEAM_15DEG_ARRAY_O:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    break;
                case Subsystem.SUB_600KHZ_VERT_PISTON_B:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_600;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_600_3;                                                  // Default 600kHz Beam Diameter
                    break;
                case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4:
                case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_45OFFSET_8:
                case Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_OPPOSITE_FACING_e:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 20;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    break;
                case Subsystem.SUB_300KHZ_4BEAM_30DEG_ARRAY_J:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    break;
                case Subsystem.SUB_300KHZ_4BEAM_15DEG_ARRAY_P:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    break;
                case Subsystem.SUB_300KHZ_1BEAM_0DEG_ARRAY_V:
                case Subsystem.SUB_300KHZ_VERT_PISTON_C:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_300;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_300_3;                                                  // Default 300kHz Beam Diameter
                    break;
                case Subsystem.SUB_150KHZ_1BEAM_0DEG_ARRAY_W:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    break;
                case Subsystem.SUB_150KHZ_4BEAM_15DEG_ARRAY_Q:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    break;
                case Subsystem.SUB_150KHZ_4BEAM_30DEG_ARRAY_K:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    break;
                case Subsystem.SUB_150KHZ_VERT_PISTON_D:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_150;                       // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_150_6;                                                  // Default 150kHz Beam Diameter
                    break;
                case Subsystem.SUB_38KHZ_1BEAM_0DEG_ARRAY_Y:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    break;
                case Subsystem.SUB_38KHZ_4BEAM_15DEG_ARRAY_S:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    break;
                case Subsystem.SUB_38KHZ_4BEAM_30DEG_ARRAY_M:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    break;
                case Subsystem.SUB_38KHZ_VERT_PISTON_F:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_38;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_38_24;                                                  // Default 38kHz Beam Diameter
                    break;
                case Subsystem.SUB_75KHZ_1BEAM_0DEG_ARRAY_X:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    break;
                case Subsystem.SUB_75KHZ_4BEAM_15DEG_ARRAY_R:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 15;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    break;
                case Subsystem.SUB_75KHZ_4BEAM_30DEG_ARRAY_L:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 30;                                                                                     // Beam Angle
                    Beams = 4;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    break;
                case Subsystem.SUB_75KHZ_VERT_PISTON_E:
                    SystemFrequency = RTI.Core.Commons.FREQ_BASE / RTI.Core.Commons.FREQ_DIV_75;                        // Frequency
                    BeamAngle = 0;                                                                                      // Beam Angle
                    Beams = 1;                                                                                          // Number of Beams
                    BeamDiameter = RTI.Core.Commons.CERAMIC_DIA_75_10;                                                  // Default 75kHz Beam Diameter
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
