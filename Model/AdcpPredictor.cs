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
 * 07/23/2012      RC          2.13       Initial coding
 * 07/30/2012      RC          2.13       Changed DEFAULT_BATTERY to be type AdcpBatteryType and added DEFAULT_BATTERY_POWER to give default power.
 * 07/31/2012      RC          2.13       Changed defaults to match commands defaults per frequency.  Changed DEFAULT_DEPLOY_DUR to 1 and DEFAULT_CEI to 1.
 * 09/05/2012      RC          2.15       Fixed bug with getting the frequency by creating FREQ_DIV_1200/600/300/150.  
 * 09/06/2012      RC          2.15       Moved FREQ_BASE and FREQ_DIV to RTI.Commons.cs.
 *                                         Changed CWPBB to CWPBB_TransmitPulseType.  Also changed its type to eCWPBB_TransmitPulseType.
 * 09/07/2012      RC          2.15       Set the WpRange to 0 if Water Profile is disabled.
 * 09/10/2012      RC          2.15       Check for divide by 0 in calculations.
 * 09/14/2012      RC          2.15       Moved BatteryType to DeploymentOptions.
 * 10/08/2012      RC          2.15       Updated Predictor Rev E.
 * 12/27/2012      RC          2.17       Replaced Subsystem.Empty with Subsystem.IsEmpty().
 * 01/02/2013      RC          2.17       Changed DEFAULT_RANGE_600000 from 65 to 50 and DEFAULT_RANGE_300000 from 155 to 125 per Steve Maier.
 * 06/05/2013      RC          3.0.0      Updated the code to Adcp Predictor Rev H.
 * 12/18/2013      RC          3.2.1      Updated the code to Adcp Predictor Rev L.
 * 12/23/2013      RC          3.2.1      Added a constructor for AdcpPredictorUserInput that takes no subsystem.
 * 12/27/2013      RC          3.2.1      Updated all the default values for all the subsystem types.
 * 08/24/2015      RC          4.2.0      Updated waves predicitions.
 * 03/14/2016      RC          4.4.3      Updated Prediction model with Power Usage for systems with vertical beams.
 * 03/15/2016      RC          4.4.3      Updated Prediction model with Power Usage to handle 8 beam systems. 
 * 03/22/2016      RC          4.4.3      Fixed bug in WavesModelPUV().
 * 06/07/2016      RC          4.4.3      Check if CWPP is set to 1 in TimeBetweenPings value.
 * 04/12/2016      RC          4.4.5      Updated the prediction model to Rev M.
 * 07/25/2016      RC          4.4.3      Fixed Maximum velocity for vertical beams.
 * 07/25/2016      RC          4.4.3      Fixed beam angle when changing subsystems.
 * 
 */

// **********************************************************
//  THIS FILE IS NOT TO BE DISTURBUTED TO CUSTOMERS.
//  IT CONTAINS PROPERITARY DATA
// **********************************************************

using System;
using System.Collections.Generic;

namespace RTI
{

    #region User Input Object

    /// <summary>
    /// User input for the ADCP predictor.
    /// </summary>
    public class AdcpPredictorUserInput
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
        /// Default System Init Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_RCV_PWR = 4.80;

        /// <summary>
        /// Default System Save Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_SAVE_PWR = 1.80;

        /// <summary>
        /// Default System Sleep Power in watts.
        /// </summary>
        public const double DEFAULT_SYS_SLEEP_PWR = 0.00125;

        #endregion

        #region Time

        /// <summary>
        /// Default time for the system to wakeup in seconds.
        /// </summary>
        public const double DEFAULT_SYS_WAKEUP_TIME = 0.4;

        /// <summary>
        /// Default time for the system to initialize in seconds.
        /// </summary>
        public const double DEFAULT_SYS_INIT_TIME = 0.25;

        /// <summary>
        /// Default time for the system to Save in seconds.
        /// </summary>
        public const double DEFAULT_SYS_SAVE_TIME = 0.15;

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
        public UInt32 DeploymentDuration { get; set; }

        /// <summary>
        /// Ensemble interval in seconds.
        /// Seconds between ensembles.
        /// Time per ensemble.
        /// </summary>
        public double CEI { get;
            set; }

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
        /// System Receive Power in watts.
        /// </summary>
        public double SystemRcvPower { get; set; }

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

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public AdcpPredictorUserInput(Subsystem ss)
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
            SystemRcvPower = DEFAULT_SYS_RCV_PWR;
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
        public AdcpPredictorUserInput()
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
            SystemRcvPower = DEFAULT_SYS_RCV_PWR;
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

    #endregion

    /// <summary>
    /// Predict the power usage, maximum range and deployment time
    /// based off values given.
    /// </summary>
    public class AdcpPredictor : IPredictor
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
        private int DEFAULT_RANGE_300000 = 100;

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
        private int DEFAULT_RANGE_150000 = 250;

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
        private int DEFAULT_RANGE_75000 = 510;

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

        #region Ensembles

        /// <summary>
        /// Profile overhead.
        /// </summary>
        public int ENS_BYTES_PROFILE_OVERHEAD = 112;

        /// <summary>
        /// Bytes per bin.
        /// </summary>
        public int ENS_BYTES_PER_BIN = 112;

        /// <summary>
        /// Bytes in the Bottom Track Dataset.
        /// </summary>
        public int ENS_BYTES_BT = 384;

        /// <summary>
        /// Overhead number of bytes in an ensemble.
        /// </summary>
        public int ENS_BYTES_OVERHEAD = 504;

        /// <summary>
        /// Bytes in the checksum of an ensemble.
        /// </summary>
        public int ENS_BYTES_CHECKSUM = 4;

        /// <summary>
        /// Bytes in the wrapper of an ensemble.
        /// </summary>
        public int ENS_BYTES_WRAPPER = 32;

        /// <summary>
        /// Number of bytes in an ensemble if no pinging is done.
        /// </summary>
        public int ENS_BYTES_NO_PING = 308;

        #endregion

        #endregion

        #region Properties

        #region User Input

        /// <summary>
        /// System frequency in Hz.
        /// </summary>
        public double SystemFrequency { get; set; }

        /// <summary>
        /// Number of days the deployment will go for.
        /// </summary>
        public UInt32 DeploymentDuration { get; set; }

        /// <summary>
        /// Ensemble interval in seconds.
        /// Seconds between ensembles.
        /// Time per ensemble.
        /// </summary>
        public double CEI { get; set; }

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
        /// On/Off Bottom Track broadband.
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
        /// Size of the Water Profile Blank.
        /// Size is in meters.
        /// </summary>
        public float CWPBL { get; set; }

        /// <summary>
        /// On/Off Water Profile broadband.
        /// TRUE = Broadband
        /// FALSE = Narrowband
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

        /// <summary>
        /// Number of ensembles per burst.
        /// Number of ensembles.
        /// </summary>
        public ushort CBI_NumEnsembles { get; set; }

        /// <summary>
        /// Number of seconds per interval within a
        /// burst.  This will give the time between
        /// each ensemble within a burst.
        /// </summary>
        public double CBI_BurstInterval { get; set; }

        #endregion

        #region Batteries

        /// <summary>
        /// Type of battery set by the user.  This is used to prevent
        /// unknown battery values from being set.
        /// </summary>
        private DeploymentOptions.AdcpBatteryType _batteryType;
        /// <summary>
        /// Type of battery set by the user.  This is used to prevent
        /// unknown battery values from being set.
        /// </summary>
        public DeploymentOptions.AdcpBatteryType BatteryType
        {
            get
            {
                return _batteryType;
            }
            set
            {
                _batteryType = value;
                BatteryWattHr = (int)value;
            }
        }

        /// <summary>
        /// Batter Watt hours.
        /// Capcity: 
        /// 21D cell Alkaline: 540 Wh-hr
        /// 7 DD cell Lithium: 800 Wh-hr
        /// </summary>
        public int BatteryWattHr { get; private set; }

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
        /// System Receive Power in watts.
        /// </summary>
        public double SystemRcvPower { get; set; }

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
        /// Time it takes for the System to Initialize in 
        /// seconds.
        /// </summary>
        public double SystemInitTime { get; set; }

        /// <summary>
        /// Time it takes for the System to Save in 
        /// seconds.
        /// </summary>
        public double SystemSaveTime { get; set; }

        #endregion

        #endregion

        #region Calculated

        #region 1200000 Table

        /// <summary>
        /// Select this table based off the
        /// given frequency of the ADCP.
        /// </summary>
        public bool Select_1200000
        {
            get
            {
                // Check if the given frequency is
                // less then this table's frequency
                if (SystemFrequency < Freq_1200000)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Frequency in Hz for the 1200000 table.
        /// </summary>
        public double Freq_1200000
        {
            get
            {
                return DEFAULT_FREQ_1200000;
            }
        }

        /// <summary>
        /// uF for the 1200000 table.
        /// </summary>
        public double uF_1200000
        {
            get
            {
                return DEFAULT_UF_1200000;
            }
        }

        /// <summary>
        /// Number of bins for 1200000 table.
        /// </summary>
        public int Bin_1200000
        {
            get
            {
                return DEFAULT_BIN_1200000;
            }
        }

        /// <summary>
        /// Range for 1200000 table.
        /// </summary>
        public int Range_1200000
        {
            get { return DEFAULT_RANGE_1200000; }
        }

        /// <summary>
        /// CPE for 1200000 table.
        /// </summary>
        public double CPE_1200000
        {
            get { return DEFAULT_CPE_1200000; }
        }

        /// <summary>
        /// DI for 1200000 table.
        /// </summary>
        public double DI_1200000
        {
            get { return 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_1200000 / WaveLength); }
        }

        /// <summary>
        /// dB for the 1200000 table.
        /// </summary>
        public double dB_1200000
        {
            get
            {
                // Check for divide by 0
                if (Bin_1200000 == 0 || CyclesPerElement == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(CWPBS / Bin_1200000) + DI - DI_1200000 - 10.0 * Math.Log10( CPE_1200000 / CyclesPerElement );
            }
        }

        /// <summary>
        /// rScale for the 1200000 table.
        /// </summary>
        public double rScale_1200000
        {
            get
            {
                return Math.Cos(BeamAngleRadian) / Math.Cos( DEFAULT_BEAM_ANGLE_1200000 / 180.0 * Math.PI); ;
            }
        }

        /// <summary>
        /// Calculated Water Profile range for the 1200000 table.
        /// This is based off the number of bins, range
        /// and dB.
        /// </summary>
        public double WpRange_1200000
        {
            get
            {
                // If selected, return a value
                if (Select_1200000)
                {
                    if (CWPON)
                    {
                        // Checck if NB
                        if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                        {
                            return rScale_1200000 * (Range_1200000 + Bin_1200000 * dB_1200000 + 20.0 * Bin_1200000);
                        }

                        return rScale_1200000 * (Range_1200000 + Bin_1200000 * dB_1200000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculated Bottom Track range for the 1200000 table.
        /// This is based off the range and a scale factor of 1.5.
        /// </summary>
        public double BtRange_1200000
        {
            get
            {
                // If selected, return a value
                if (Select_1200000)
                {
                    if (CBTON)
                    {
                        // Check if NB
                        if (CBTBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                        {
                            return 2.0 * rScale_1200000 * ( Range_1200000 + Bin_1200000 * dB_1200000 + 15.0 * Bin_1200000 );
                        }

                        return 2.0 * rScale_1200000 * (Range_1200000 +  Bin_1200000 * dB_1200000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Transmit wattage for the 1200000 table.
        /// </summary>
        public int XmtW_1200000
        {
            get
            {
                // If selected, return a value
                if (Select_1200000)
                {
                    return DEFAULT_XMIT_W_1200000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Transmit voltage for the 1200000 table.
        /// </summary>
        public int XmtV_1200000
        {
            get
            {
                // If selected, return a value
                if (Select_1200000)
                {
                    return DEFAULT_XMIT_V_1200000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Calculate the Leakage in uA for the 1200000 table.
        /// </summary>
        public double LeakageuA_1200000
        {
            get
            {
                if (Select_1200000)
                {
                    return 3.0 * Math.Sqrt(2.0 * 0.000001 * uF_1200000 * XmtV_1200000);
                }

                // Return 0 if not selected.
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Sampling for the 1200000 table.
        /// </summary>
        public double Sampling_1200000
        {
            get
            {
                if (Select_1200000)
                {
                    return DEFAULT_SAMPLING_1200000 * CPE_1200000 / CyclesPerElement;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Ref Bin for the 1200000 table.
        /// </summary>
        public double RefBin_1200000
        {
            get
            {
                if (Select_1200000)
                {
                    return DEFAULT_BIN_1200000;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        #endregion

        #region 600000 Table

        /// <summary>
        /// Select this table based off the
        /// given frequency of the ADCP.
        /// </summary>
        public bool Select_600000
        {
            get
            {
                // Check if the given frequency
                if (SystemFrequency > Freq_600000)
                {
                    if (SystemFrequency < Freq_1200000)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Frequency in Hz for the 600000 table.
        /// </summary>
        public double Freq_600000
        {
            get
            {
                return DEFAULT_FREQ_600000;
            }
        }

        /// <summary>
        /// uF for the 600000 table.
        /// </summary>
        public double uF_600000
        {
            get
            {
                return DEFAULT_UF_600000;
            }
        }

        /// <summary>
        /// Number of bins for 600000 table.
        /// </summary>
        public int Bin_600000
        {
            get
            {
                return DEFAULT_BIN_600000;
            }
        }

        /// <summary>
        /// Range for 600000 table.
        /// </summary>
        public int Range_600000
        {
            get { return DEFAULT_RANGE_600000; }
        }

        /// <summary>
        /// CPE for 600000 table.
        /// </summary>
        public double CPE_600000
        {
            get { return DEFAULT_CPE_600000; }
        }

        /// <summary>
        /// DI for 600000 table.
        /// </summary>
        public double DI_600000
        {
            get { return 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_600000 / WaveLength); }
        }

        /// <summary>
        /// dB for the 600000 table.
        /// </summary>
        public double dB_600000
        {
            get
            {
                // Check for divide by 0
                if (Bin_600000 == 0 || CyclesPerElement == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(CWPBS / Bin_600000) + DI - DI_600000 - 10.0 * Math.Log10(CPE_600000 / CyclesPerElement);
            }
        }

        /// <summary>
        /// rScale for the 600000 table.
        /// </summary>
        public double rScale_600000
        {
            get
            {
                return Math.Cos(BeamAngleRadian) / Math.Cos( DEFAULT_BEAM_ANGLE_600000 / 180.0 * Math.PI); ;;
            }
        }

        /// <summary>
        /// Calculated Water Profile range for the 600000 table.
        /// This is based off the number of bins, range
        /// and dB.
        /// </summary>
        public double WpRange_600000
        {
            get
            {
                // If selected, return a value
                if (Select_600000)
                {
                    if (CWPON)
                    {
                        // Checck if NB
                        if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                        {
                            return rScale_600000 * (Range_600000 + Bin_600000 * dB_600000 + 20.0 * Bin_600000);
                        }

                        return rScale_600000 * (Range_600000 + Bin_600000 * dB_600000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculated Bottom Track range for the 600000 table.
        /// This is based off the range and a scale factor of 1.5.
        /// </summary>
        public double BtRange_600000
        {
            get
            {
                // If selected, return a value
                if (Select_600000)
                {
                    if (CBTON)
                    {
                        // Check if NB
                        if (CBTBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                        {
                            return 2.0 * rScale_600000 * (Range_600000 + Bin_600000 * dB_600000 + 15.0 * Bin_600000);
                        }

                        return 2.0 * rScale_600000 * (Range_600000 + Bin_600000 * dB_600000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Transmit wattage for the 600000 table.
        /// </summary>
        public int XmtW_600000
        {
            get
            {
                // If selected, return a value
                if (Select_600000)
                {
                    return DEFAULT_XMIT_W_600000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Transmit voltage for the 600000 table.
        /// </summary>
        public int XmtV_600000
        {
            get
            {
                // If selected, return a value
                if (Select_600000)
                {
                    return DEFAULT_XMIT_V_600000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Calculate the Leakage in uA for the 600000 table.
        /// </summary>
        public double LeakageuA_600000
        {
            get
            {
                if (Select_600000)
                {
                    return 3.0 * Math.Sqrt(2.0 * 0.000001 * uF_600000 * XmtV_600000);
                }

                // Return 0 if not selected.
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Sampling for the 600000 table.
        /// </summary>
        public double Sampling_600000
        {
            get
            {
                if (Select_600000)
                {
                    return DEFAULT_SAMPLING_600000 * CPE_600000 / CyclesPerElement;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Ref Bin for the 600000 table.
        /// </summary>
        public double RefBin_600000
        {
            get
            {
                if (Select_600000)
                {
                    return DEFAULT_BIN_600000;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        #endregion

        #region 300000 Table

        /// <summary>
        /// Select this table based off the
        /// given frequency of the ADCP.
        /// </summary>
        public bool Select_300000
        {
            get
            {
                // Check if the given frequency
                if (SystemFrequency > Freq_300000)
                {
                    if (SystemFrequency < Freq_600000)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Frequency in Hz for the 300000 table.
        /// </summary>
        public double Freq_300000
        {
            get
            {
                return DEFAULT_FREQ_300000;
            }
        }

        /// <summary>
        /// uF for the 300000 table.
        /// </summary>
        public double uF_300000
        {
            get
            {
                return DEFAULT_UF_300000;
            }
        }

        /// <summary>
        /// Number of bins for 300000 table.
        /// </summary>
        public int Bin_300000
        {
            get
            {
                return DEFAULT_BIN_300000;
            }
        }

        /// <summary>
        /// Range for 300000 table.
        /// </summary>
        public int Range_300000
        {
            get { return DEFAULT_RANGE_300000; }
        }

        /// <summary>
        /// CPE for 300000 table.
        /// </summary>
        public double CPE_300000
        {
            get { return DEFAULT_CPE_300000; }
        }

        /// <summary>
        /// DI for 300000 table.
        /// </summary>
        public double DI_300000
        {
            get { return 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_300000 / WaveLength); }
        }

        /// <summary>
        /// dB for the 300000 table.
        /// </summary>
        public double dB_300000
        {
            get
            {
                // Check for divide by 0
                if (Bin_300000 == 0 || CyclesPerElement == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(CWPBS / Bin_300000) + DI - DI_300000 - 10.0 * Math.Log10(CPE_300000 / CyclesPerElement);
            }
        }

        /// <summary>
        /// rScale for the 300000 table.
        /// </summary>
        public double rScale_300000
        {
            get
            {
                return Math.Cos(BeamAngleRadian) / Math.Cos(DEFAULT_BEAM_ANGLE_300000 / 180.0 * Math.PI); ;
            }
        }

        /// <summary>
        /// Calculated Water Profile range for the 300000 table.
        /// This is based off the number of bins, range
        /// and dB.
        /// </summary>
        public double WpRange_300000
        {
            get
            {
                // If selected, return a value
                if (Select_300000)
                {
                    if (CWPON)
                    {
                        // Checck if NB
                        if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                        {
                            return rScale_300000 * (Range_300000 + Bin_300000 * dB_300000 + 20.0 * Bin_300000);
                        }

                        return rScale_300000 * (Range_300000 + Bin_300000 * dB_300000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculated Bottom Track range for the 300000 table.
        /// This is based off the range and a scale factor of 1.5.
        /// </summary>
        public double BtRange_300000
        {
            get
            {
                // If selected, return a value
                if (Select_300000)
                {
                    if (CBTON)
                    {
                        // Check if NB
                        if (CBTBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                        {
                            return 2.0 * rScale_300000 * (Range_300000 + Bin_300000 * dB_300000 + 15.0 * Bin_300000);
                        }

                        return 2.0 * rScale_300000 * (Range_300000 + Bin_300000 * dB_300000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Transmit wattage for the 300000 table.
        /// </summary>
        public int XmtW_300000
        {
            get
            {
                // If selected, return a value
                if (Select_300000)
                {
                    return DEFAULT_XMIT_W_300000;
                }

                // Return 0 if not selected
                return 0;
            }
        }


        /// <summary>
        /// Transmit voltage for the 300000 table.
        /// </summary>
        public int XmtV_300000
        {
            get
            {
                // If selected, return a value
                if (Select_300000)
                {
                    return DEFAULT_XMIT_V_300000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Calculate the Leakage in uA for the 300000 table.
        /// </summary>
        public double LeakageuA_300000
        {
            get
            {
                if (Select_300000)
                {
                    return 3.0 * Math.Sqrt(2.0 * 0.000001 * uF_300000 * XmtV_300000);
                }

                // Return 0 if not selected.
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Sampling for the 300000 table.
        /// </summary>
        public double Sampling_300000
        {
            get
            {
                if (Select_300000)
                {
                    return DEFAULT_SAMPLING_300000 * CPE_300000 / CyclesPerElement;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Ref Bin for the 300000 table.
        /// </summary>
        public double RefBin_300000
        {
            get
            {
                if (Select_300000)
                {
                    return DEFAULT_BIN_300000;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        #endregion

        #region 150000 Table

        /// <summary>
        /// Select this table based off the
        /// given frequency of the ADCP.
        /// </summary>
        public bool Select_150000
        {
            get
            {
                // Check if the given frequency
                if (SystemFrequency > Freq_150000)
                {
                    if (SystemFrequency < Freq_300000)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Frequency in Hz for the 150000 table.
        /// </summary>
        public double Freq_150000
        {
            get
            {
                return DEFAULT_FREQ_150000;
            }
        }

        /// <summary>
        /// uF for the 150000 table.
        /// </summary>
        public double uF_150000
        {
            get
            {
                return DEFAULT_UF_150000;
            }
        }

        /// <summary>
        /// Number of bins for 150000 table.
        /// </summary>
        public int Bin_150000
        {
            get
            {
                return DEFAULT_BIN_150000;
            }
        }

        /// <summary>
        /// Range for 150000 table.
        /// </summary>
        public int Range_150000
        {
            get { return DEFAULT_RANGE_150000; }
        }

        /// <summary>
        /// CPE for 150000 table.
        /// </summary>
        public double CPE_150000
        {
            get { return DEFAULT_CPE_150000; }
        }

        /// <summary>
        /// DI for 150000 table.
        /// </summary>
        public double DI_150000
        {
            get { return 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_150000 / WaveLength); }
        }

        /// <summary>
        /// dB for the 150000 table.
        /// </summary>
        public double dB_150000
        {
            get
            {
                // Check for divide by 0
                if (Bin_150000 == 0 || CyclesPerElement == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(CWPBS / Bin_150000) + DI - DI_150000 - 10.0 * Math.Log10(CPE_150000 / CyclesPerElement);
            }
        }

        /// <summary>
        /// rScale for the 150000 table.
        /// </summary>
        public double rScale_150000
        {
            get
            {
                return Math.Cos(BeamAngleRadian) / Math.Cos(DEFAULT_BEAM_ANGLE_150000 / 180.0 * Math.PI); ;
            }
        }

        /// <summary>
        /// Calculated Water Profile range for the 150000 table.
        /// This is based off the number of bins, range
        /// and dB.
        /// </summary>
        public double WpRange_150000
        {
            get
            {
                // If selected, return a value
                if (Select_150000)
                {
                    if (CWPON)
                    {
                        // Checck if NB
                        if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                        {
                            return rScale_150000 * (Range_150000 + Bin_150000 * dB_150000 + 20.0 * Bin_150000);
                        }

                        return rScale_150000 * (Range_150000 + Bin_150000 * dB_150000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculated Bottom Track range for the 150000 table.
        /// This is based off the range and a scale factor of 1.5.
        /// </summary>
        public double BtRange_150000
        {
            get
            {
                // If selected, return a value
                if (Select_150000)
                {
                    if (CBTON)
                    {
                        // Check if NB
                        if (CBTBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                        {
                            return 2.0 * rScale_150000 * (Range_150000 + Bin_150000 * dB_150000 + 15.0 * Bin_150000);
                        }

                        return 2.0 * rScale_150000 * (Range_150000 + Bin_150000 * dB_150000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Transmit wattage for the 150000 table.
        /// </summary>
        public int XmtW_150000
        {
            get
            {
                // If selected, return a value
                if (Select_150000)
                {
                    return DEFAULT_XMIT_W_150000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Transmit voltage for the 150000 table.
        /// </summary>
        public int XmtV_150000
        {
            get
            {
                // If selected, return a value
                if (Select_150000)
                {
                    return DEFAULT_XMIT_V_150000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Calculate the Leakage in uA for the 150000 table.
        /// </summary>
        public double LeakageuA_150000
        {
            get
            {
                if (Select_150000)
                {
                    return 3.0 * Math.Sqrt(2.0 * 0.000001 * uF_150000 * XmtV_150000);
                }

                // Return 0 if not selected.
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Sampling for the 150000 table.
        /// </summary>
        public double Sampling_150000
        {
            get
            {
                if (Select_150000)
                {
                    return DEFAULT_SAMPLING_150000 * CPE_150000 / CyclesPerElement;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Ref Bin for the 150000 table.
        /// </summary>
        public double RefBin_150000
        {
            get
            {
                if (Select_150000)
                {
                    return DEFAULT_BIN_150000;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        #endregion

        #region 75000 Table

        /// <summary>
        /// Select this table based off the
        /// given frequency of the ADCP.
        /// </summary>
        public bool Select_75000
        {
            get
            {
                // Check if the given frequency
                if (SystemFrequency > Freq_75000)
                {
                    if (SystemFrequency < Freq_150000)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Frequency in Hz for the 75000 table.
        /// </summary>
        public double Freq_75000
        {
            get
            {
                return DEFAULT_FREQ_75000;
            }
        }

        /// <summary>
        /// uF for the 75000 table.
        /// </summary>
        public double uF_75000
        {
            get
            {
                return DEFAULT_UF_75000;
            }
        }

        /// <summary>
        /// Number of bins for 75000 table.
        /// </summary>
        public int Bin_75000
        {
            get
            {
                return DEFAULT_BIN_75000;
            }
        }

        /// <summary>
        /// Range for 75000 table.
        /// </summary>
        public int Range_75000
        {
            get { return DEFAULT_RANGE_75000; }
        }

        /// <summary>
        /// CPE for 75000 table.
        /// </summary>
        public double CPE_75000
        {
            get { return DEFAULT_CPE_75000; }
        }

        /// <summary>
        /// DI for 75000 table.
        /// </summary>
        public double DI_75000
        {
            get { return 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_75000 / WaveLength); }
        }

        /// <summary>
        /// dB for the 75000 table.
        /// </summary>
        public double dB_75000
        {
            get
            {
                // Check for divide by 0
                if (Bin_75000 == 0 || CyclesPerElement == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(CWPBS / Bin_75000) + DI - DI_75000 - 10.0 * Math.Log10(CPE_75000 / CyclesPerElement);
            }
        }

        /// <summary>
        /// rScale for the 75000 table.
        /// </summary>
        public double rScale_75000
        {
            get
            {
                return Math.Cos(BeamAngleRadian) / Math.Cos( DEFAULT_BEAM_ANGLE_75000 / 180.0 * Math.PI); ;;
            }
        }

        /// <summary>
        /// Calculated Water Profile range for the 75000 table.
        /// This is based off the number of bins, range
        /// and dB.
        /// </summary>
        public double WpRange_75000
        {
            get
            {
                // If selected, return a value
                if (Select_75000)
                {
                    if (CWPON)
                    {
                        // Checck if NB
                        if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                        {
                            return rScale_75000 * (Range_75000 + Bin_75000 * dB_75000 + 20.0 * Bin_75000);
                        }

                        return rScale_75000 * (Range_75000 + Bin_75000 * dB_75000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculated Bottom Track range for the 75000 table.
        /// This is based off the range and a scale factor of 1.5.
        /// </summary>
        public double BtRange_75000
        {
            get
            {
                // If selected, return a value
                if (Select_75000)
                {
                    if (CBTON)
                    {
                        // Check if NB
                        if (CBTBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                        {
                            return 2.0 * rScale_75000 * (Range_75000 + Bin_75000 * dB_75000 + 15.0 * Bin_75000);
                        }

                        return 2.0 * rScale_75000 * (Range_75000 + Bin_75000 * dB_75000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Transmit wattage for the 75000 table.
        /// </summary>
        public int XmtW_75000
        {
            get
            {
                // If selected, return a value
                if (Select_75000)
                {
                    return DEFAULT_XMIT_W_75000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Transmit voltage for the 75000 table.
        /// </summary>
        public int XmtV_75000
        {
            get
            {
                // If selected, return a value
                if (Select_75000)
                {
                    return DEFAULT_XMIT_V_75000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Calculate the Leakage in uA for the 75000 table.
        /// </summary>
        public double LeakageuA_75000
        {
            get
            {
                if (Select_75000)
                {
                    return 3.0 * Math.Sqrt(2.0 * 0.000001 * uF_75000 * XmtV_75000);
                }

                // Return 0 if not selected.
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Sampling for the 75000 table.
        /// </summary>
        public double Sampling_75000
        {
            get
            {
                if (Select_75000)
                {
                    return DEFAULT_SAMPLING_75000 * CPE_75000 / CyclesPerElement;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Ref Bin for the 75000 table.
        /// </summary>
        public double RefBin_75000
        {
            get
            {
                if (Select_75000)
                {
                    return DEFAULT_BIN_75000;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        #endregion

        #region 38000 Table

        /// <summary>
        /// Select this table based off the
        /// given frequency of the ADCP.
        /// </summary>
        public bool Select_38000
        {
            get
            {
                // Check if the given frequency
                if (SystemFrequency > Freq_38000)
                {
                    if (SystemFrequency < Freq_75000)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Frequency in Hz for the 38000 table.
        /// </summary>
        public double Freq_38000
        {
            get
            {
                return DEFAULT_FREQ_38000;
            }
        }

        /// <summary>
        /// uF for the 38000 table.
        /// </summary>
        public double uF_38000
        {
            get
            {
                return DEFAULT_UF_38000;
            }
        }

        /// <summary>
        /// Number of bins for 38000 table.
        /// </summary>
        public int Bin_38000
        {
            get
            {
                return DEFAULT_BIN_38000;
            }
        }

        /// <summary>
        /// Range for 38000 table.
        /// </summary>
        public int Range_38000
        {
            get { return DEFAULT_RANGE_38000; }
        }

        /// <summary>
        /// CPE for 38000 table.
        /// </summary>
        public double CPE_38000
        {
            get { return DEFAULT_CPE_38000; }
        }

        /// <summary>
        /// DI for 38000 table.
        /// </summary>
        public double DI_38000
        {
            get { return 20.0 * Math.Log10(Math.PI * DEFAULT_DIAM_38000 / WaveLength); }
        }

        /// <summary>
        /// dB for the 38000 table.
        /// </summary>
        public double dB_38000
        {
            get
            {
                // Check for divide by 0
                if (Bin_38000 == 0 || CyclesPerElement == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(CWPBS / Bin_38000) + DI - DI_38000 - 10.0 * Math.Log10(CPE_38000 / CyclesPerElement);
            }
        }

        /// <summary>
        /// rScale for the 38000 table.
        /// </summary>
        public double rScale_38000
        {
            get
            {
                return Math.Cos(BeamAngleRadian) / Math.Cos(DEFAULT_BEAM_ANGLE_38000 / 180.0 * Math.PI); ;
            }
        }

        /// <summary>
        /// Calculated Water Profile range for the 38000 table.
        /// This is based off the number of bins, range
        /// and dB.
        /// </summary>
        public double WpRange_38000
        {
            get
            {
                // If selected, return a value
                if (Select_38000)
                {
                    if (CWPON)
                    {
                        // Checck if NB
                        if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                        {
                            return rScale_38000 * (Range_38000 + Bin_38000 * dB_38000 + 20.0 * Bin_38000);
                        }

                        return rScale_38000 * (Range_38000 + Bin_38000 * dB_38000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculated Bottom Track range for the 38000 table.
        /// This is based off the range and a scale factor of 1.5.
        /// </summary>
        public double BtRange_38000
        {
            get
            {
                // If selected, return a value
                if (Select_38000)
                {
                    if (CBTON)
                    {
                        // Check if NB
                        if (CBTBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCBTBB_Mode.NARROWBAND_LONG_RANGE)
                        {
                            return 2.0 * rScale_38000 * (Range_38000 + Bin_38000 * dB_38000 + 15.0 * Bin_38000);
                        }

                        return 2.0 * rScale_38000 * (Range_38000 + Bin_38000 * dB_38000);
                    }
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Transmit wattage for the 38000 table.
        /// </summary>
        public int XmtW_38000
        {
            get
            {
                // If selected, return a value
                if (Select_38000)
                {
                    return DEFAULT_XMIT_W_38000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Transmit voltage for the 38000 table.
        /// </summary>
        public int XmtV_38000
        {
            get
            {
                // If selected, return a value
                if (Select_38000)
                {
                    return DEFAULT_XMIT_V_38000;
                }

                // Return 0 if not selected
                return 0;
            }
        }

        /// <summary>
        /// Calculate the Leakage in uA for the 38000 table.
        /// </summary>
        public double LeakageuA_38000
        {
            get
            {
                if (Select_38000)
                {
                    return 3.0 * Math.Sqrt(2.0 * 0.000001 * uF_38000 * XmtV_75000);
                }

                // Return 0 if not selected.
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Sampling for the 38000 table.
        /// </summary>
        public double Sampling_38000
        {
            get
            {
                if (Select_38000)
                {
                    return DEFAULT_SAMPLING_38000 * CPE_38000 / CyclesPerElement;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the Ref Bin for the 38000 table.
        /// </summary>
        public double RefBin_38000
        {
            get
            {
                if (Select_38000)
                {
                    return DEFAULT_BIN_38000;
                }

                // Return 0 if not selected
                return 0.0;
            }
        }

        #endregion

        #region Beam Transmit Power

        /// <summary>
        /// Transmit scale.
        /// </summary>
        public double TransmitScale
        {
            get
            {
                if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                {
                    return 1;
                }
                else
                {
                    // Check for 0
                    if (LagSamples == 0)
                    {
                        return 0;
                    }

                    if (BroadbandPower)
                    {
                        return (LagSamples - 1.0) / LagSamples;
                    }
                    else
                    {
                        return 1 / LagSamples;
                    }
                }
            }
        }

        ///// <summary>
        ///// Beam Transmit Power.
        ///// Sum up all the beams that are selected and
        ///// giving power.
        ///// </summary>
        //public double BeamTransmitPower
        //{
        //    get
        //    {
        //        return TransmitScale * (XmtW_1200000 + XmtW_600000 + XmtW_300000 + XmtW_150000 + XmtW_75000 + XmtW_38000);
        //    }
        //}

        #endregion

        #region Bottom Track Pings

        /// <summary>
        /// Calculate the number of bottom track pings based
        /// off the number of pings, deployment time and ensemble
        /// interval.
        /// </summary>
        public long BottomTrackPings
        {
            get
            {
                if (CBTON)
                {
                    long value = (long)Math.Round(CWPP / 10.0);
                    if (value < 1)
                    {
                        return NumEnsembles;
                    }

                    return value * NumEnsembles;
                }

                return 0;
            }
        }

        #endregion

        #region Time Calculations

        /// <summary>
        /// Calculate the Bottom Track Time.  Sum up all the
        /// ranges the ADCP must use and multiply by 0.0015;
        /// </summary>
        public double BottomTrackTime
        {
            get
            {
                double sum = BtRange_1200000 + BtRange_600000 + BtRange_300000 + BtRange_150000 + BtRange_75000 + BtRange_38000;

                return 0.0015 * sum;
            }
        }

        /// <summary>
        /// Time between pings.
        /// Based off Water Profile number of bins, bin samples, sample rate and Water Profile time between pings.
        /// Value in seconds.
        /// </summary>
        public double TimeBetweenPings
        {
            get
            {
                // Check for divide by 0
                if (SampleRate == 0)
                {
                    return 0;
                }

                // If there is only 1 ping, then there is no time between pings.
                if(CWPP == 1)
                {
                    return 0;
                }

                if (CWPBN * BinSamples / SampleRate > CWPTBP)
                {
                    return CWPBN * BinSamples / SampleRate;
                }
                else
                {
                    return CWPTBP;
                }
            }
        }

        /// <summary>
        /// Calculate the profile time.
        /// Based off Time between pings, system wakeup time, number of bins, bin samples and sample rate.
        /// Value in seconds.
        /// </summary>
        public double ProfileTime
        {
            get
            {
                // Check for divide by 0
                if (SampleRate == 0)
                {
                    return 0;
                }

                // Single ping data
                if(CWPP == 1)
                {
                    return CWPBN * BinSamples / SampleRate;
                }

                // Sleep between pings
                if (TimeBetweenPings > 1.0)
                {
                    return CWPBN * BinSamples / SampleRate;
                }

                // No sleeping between pings
                return TimeBetweenPings;
            }
        }

        /// <summary>
        /// Lag time in seconds.
        /// Based off lag samples and sample rate.
        /// Value in seconds.
        /// </summary>
        public double LagTime
        {
            get
            {
                // Check for divide by 0
                if (SampleRate == 0)
                {
                    return 0;
                }

                return LagSamples / SampleRate;
            }
        }

        /// <summary>
        /// Bin time in seconds.
        /// Based off bin samples and sample rate.
        /// Value in seconds.
        /// </summary>
        public double BinTime
        {
            get
            {
                // Check for divide by 0
                if (SampleRate == 0)
                {
                    return 0;
                }

                return BinSamples / SampleRate;
            }
        }

        /// <summary>
        /// Receive time in seconds.
        /// Based off Profile time.
        /// Value in seconds.
        /// </summary>
        public double ReceiveTime
        {
            get
            {
                return ProfileTime;
            }
        }

        /// <summary>
        /// Transmit code time.
        /// Based off Broadband being on, code repeats, bin time ang lag time.
        /// Value in seconds.
        /// </summary>
        public double TransmitCodeTime
        {
            get
            {
                // If using Broadband
                if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.BROADBAND)
                {
                    if (CodeRepeats < 3)
                    {
                        return 2.0 * BinTime;
                    }
                    else
                    {
                        return CodeRepeats * LagTime;
                    }
                }
                else
                {
                    if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                    {
                        return BinTime;
                    }
                    else
                    {
                        return 2.0 * BinTime;
                    }
                }
            }
        }

        #endregion

        #region Power Usage Calculation

        /// <summary>
        /// Wavelength.
        /// </summary>
        public double WaveLength
        {
            get
            {
                return SpeedOfSound / SystemFrequency;
            }
        }

        /// <summary>
        /// Beam Angle in radians.
        /// </summary>
        public double BeamAngleRadian
        {
            get
            {
                return BeamAngle / 180.0 * Math.PI;
            }
        }

        /// <summary>
        /// DI value.
        /// </summary>
        public double DI
        {
            get
            {
                if (WaveLength == 0)
                {
                    return 0.0;
                }

                return 20.0 * Math.Log10(Math.PI * BeamDiameter / WaveLength);
            }
        }

        /// <summary>
        /// Transmit scale.
        /// </summary>
        public double XmtScale
        {
            get
            {
                // Checck if NB
                if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                {
                    return 1.0;
                }
                else
                {
                    // Check for bad value
                    if (LagSamples == 0)
                    {
                        return 0.0;
                    }

                    // Check wich Broadband power is used
                    if (BroadbandPower)
                    {
                        return (LagSamples - 1.0) / LagSamples;
                    }
                    else
                    {


                        return 1.0 / LagSamples;
                    }
                }
            }
        }

        /// <summary>
        /// Beam Transmit Power Profile.
        /// </summary>
        public double BeamXmtPowerProfile
        {
            get
            {
                // Get the sum of all the selected XmtW
                double sumXmtW = XmtW_1200000 + XmtW_600000 + XmtW_300000 + XmtW_150000 + XmtW_75000 +  XmtW_38000;

                return XmtScale * sumXmtW;
            }
        }

        /// <summary>
        /// Beam Transmit Power Bottom Track.
        /// </summary>
        public double BeamXmtPowerBottomTrack
        {
            get
            {
                // Get the sum of all the selected XmtW
                return XmtW_1200000 + XmtW_600000 + XmtW_300000 + XmtW_150000 + XmtW_75000 + XmtW_38000;
            }
        }

        /// <summary>
        /// Range reduction used in the Water Profile range.
        /// </summary>
        public double RangeReduction
        {
            get
            {

                // Get the sum of all the selected WP XmtW and RefBin
                double sumXmtW = XmtW_1200000 + XmtW_600000 + XmtW_300000 + XmtW_150000 + XmtW_75000 + XmtW_38000;
                double sumRefBin = RefBin_1200000 + RefBin_600000 + RefBin_300000 + RefBin_150000 + RefBin_75000 + RefBin_38000;

                // Check for bad values
                if (sumXmtW == 0)
                {
                    return 0.0;
                }

                return 10.0 * Math.Log10(BeamXmtPowerProfile / sumXmtW) * sumRefBin + 1.0;
            }
        }

        /// <summary>
        /// Bottom Track Transmit Power.
        /// Based off Bottom Track time, beam transmit power, beams, number ensembles and pings per ensemble.
        /// Value in Wh-hr.
        /// </summary>
        public double BtTransmitPower
        {
            get
            {
                return BottomTrackPings * 0.2 * (BottomTrackTime * BeamXmtPowerBottomTrack * Beams) / 3600.0;
            }
        }

        /// <summary>
        /// Bottom Track Receive Power.
        /// Based off system frequency, bottom track time, system on power, number of ensemble and pings per ensemble.
        /// Value in Wh-hr.
        /// </summary>
        public double BtReceivePower
        {
            get
            {
                double freqMult = 1;
                if (SystemFrequency > 600000.0)
                {
                    freqMult = 2;
                }

                return BottomTrackPings * (BottomTrackTime * SystemBootPower) / 3600.0 * freqMult;
            }
        }

        /// <summary>
        /// Wake up power.
        /// Based off number of wakeups, system wakeup and system on power.
        /// Value in Wh-hr.
        /// </summary>
        public double WakeupPower
        {
            get
            {
                return Wakeups * SystemWakeupTime * SystemBootPower / 3600.0;
            }
        }

        /// <summary>
        /// Init Power.
        /// Value in Wh-hr.
        /// </summary>
        public double InitPower
        {
            get
            {
                return Wakeups * SystemInitPower * SystemInitTime / 3600.0;
            }
        }

        /// <summary>
        /// Transmit Power.
        /// Based off Transmit code time, beam transmit power, beams number of ensembles and pings per ensemble.
        /// Value in Wh-hr.
        /// </summary>
        public double TransmitPower
        {
            get
            {
                return (TransmitCodeTime * BeamXmtPowerProfile * Beams * NumEnsembles * CWPP) / 3600.0;
            }
        }

        /// <summary>
        /// Receive power.
        /// Based off receive time, system on power, number of ensemble and pings per ensemble.
        /// Value in Wh-hr.
        /// </summary>
        public double ReceivePower
        {
            get
            {
                double freqMult = 1;
                if (SystemFrequency > 700000.0)
                {
                    freqMult = 2;
                }

                return (ReceiveTime * SystemRcvPower * NumEnsembles * CWPP) / 3600.0 * freqMult;
            }
        }

        /// <summary>
        /// Save Power.
        /// Value in Wh-hr.
        /// </summary>
        public double SavePower
        {
            get
            {
                return (Wakeups * SystemSavePower * SystemSaveTime) / 3600.0;
            }
        }

        /// <summary>
        /// Sleep Power.
        /// Based off system sleep power and deployment duration.
        /// Value in Wh-hr.
        /// </summary>
        public double SleepPower
        {
            get
            {
                return SystemSleepPower * DeploymentDuration * 24.0;
            }
        }

        /// <summary>
        /// Cap Charge Power.
        /// Based off System frequency, beam transmit power, beams, number of ensembles and pings per ensemble.
        /// Value in Wh-hr.
        /// </summary>
        public double CapChargePower
        {
            get
            {
                // Sum up the Xmt Voltage
                double sumXmtV = XmtV_1200000 + XmtV_600000 + XmtV_300000 + XmtV_150000 + XmtV_75000 + XmtV_38000;

                // Sum up the Leakage
                double sumLeakageuA = LeakageuA_1200000 + LeakageuA_600000 + LeakageuA_300000 + LeakageuA_150000 + LeakageuA_75000 + LeakageuA_38000;

                return 0.03 * (BtTransmitPower + TransmitPower) + 1.3 * DeploymentDuration * 24.0 * sumXmtV * 0.000001 * sumLeakageuA;
            }
        }

        /// <summary>
        /// Total power usage.
        /// Based off bottom track transmit power, bottom track receive power, transmit power, receive power, 
        /// sleep power, wakeup power, and cap charge power.
        /// Value in Wh-hr.
        /// </summary>
        public double TotalPower
        {
            get
            {
                return BtTransmitPower + BtReceivePower + WakeupPower  + InitPower + TransmitPower + ReceivePower + SavePower + SleepPower + CapChargePower;
            }
        }

        /// <summary>
        /// Give the actual battery power available.
        /// This takes into account the battery
        /// derate value.
        /// Value in Wh-hr.
        /// </summary>
        public double ActualBatteryPower
        {
            get
            {
                return BatteryWattHr * BatteryDerate - BatterySelfDischargePerYear * DeploymentDuration / 365.0; 
            }
        }

        #endregion

        #region Broadband Calculations

        /// <summary>
        /// Sample rate based off the system frequency.
        /// Value in Hz.
        /// </summary>
        public double SampleRate
        {
            get
            {
                return SystemFrequency * (Sampling_1200000 + Sampling_600000 + Sampling_300000 + Sampling_150000 + Sampling_75000 + Sampling_38000);
            }
        }

        /// <summary>
        /// Meters per Sample
        /// Based off sample rate, speed of sound and beam angle.
        /// Value in Meters.
        /// </summary>
        public double MetersPerSample
        {
            get
            {
                // Check for divide by 0
                if (SampleRate == 0)
                {
                    return 0.0;
                }

                return Math.Cos(BeamAngle / 180.0 * Math.PI) * SpeedOfSound / 2.0 / SampleRate;
            }
        }

        /// <summary>
        /// Get the integer number of bin samples.
        /// Based off the bin size and meters per sample.
        /// Value in samples.
        /// </summary>
        public int BinSamples
        {
            get
            {
                // Check for divide by 0
                if (MetersPerSample == 0)
                {
                    return 0;
                }

                return (int)Math.Truncate(CWPBS / MetersPerSample);
            }
        }

        /// <summary>
        /// Get the integer number of lag samples.
        /// Based off the Water Profile lag length and
        /// meters per samples.
        /// Value in samples.
        /// </summary>
        public int LagSamples
        {
            get
            {
                // Check for divide by 0
                if (MetersPerSample == 0)
                {
                    return 0;
                }

                return 2 * (int)Math.Truncate((Math.Truncate(CWPBB_LagLength / MetersPerSample) + 1.0) / 2.0);
            }
        }

        /// <summary>
        /// Number of code repeats.
        /// Based off the number of bin samples and lag samples.
        /// Value in number of repeats.
        /// </summary>
        public int CodeRepeats
        {
            get
            {
                // Check for divide by 0
                if (LagSamples == 0)
                {
                    return 0;
                }

                // Cased BinSamples and LagSamples to double because Truncate only takes doubles
                // Make the result of Truncate an int
                if (((int)Math.Truncate((double)BinSamples / (double)LagSamples)) + 1.0 < 2.0)
                {
                    return 2;
                }

                return ((int)Math.Truncate((double)BinSamples / (double)LagSamples)) + 1;
            }
        }

        /// <summary>
        /// Correlation.
        /// RHO.
        /// Based off Beta, Code repeats and SNR.
        /// </summary>
        public double rho
        {
            get
            {
                if ((int)CWPBB_TransmitPulseType < 2)
                {
                    // Check for divide by 0
                    if (CodeRepeats == 0 || SNR == 0)
                    {
                        return 0;
                    }

                    double snr = Math.Pow(10.0, SNR / 10.0);

                    return Beta * ((CodeRepeats - 1.0) / CodeRepeats) / (1.0 + Math.Pow(1.0/10.0, SNR/10.0));
                }
                else
                {
                    return Beta;
                }
            }
        }

        /// <summary>
        /// Ambuguity Velocity.
        /// Ua Hz.
        /// Based off Sample rate and lag samples.
        /// Value in Hz.
        /// </summary>
        public double UaHz
        {
            get
            {
                // Check for divide by 0
                if (LagSamples == 0)
                {
                    return 0.0;
                }

                return SampleRate / (2.0 * LagSamples);
            }
        }

        /// <summary>
        /// Ambuguity Velocity.
        /// Ua Radial m/s.
        /// Based off UaHz, speed of sound and System Frequency.
        /// Value in m/s.
        /// </summary>
        public double UaRadial
        {
            get
            {
                // Check for divide by 0
                if (SystemFrequency == 0)
                {
                    return 0.0;
                }

                return UaHz * SpeedOfSound / (2.0 * SystemFrequency);
            }
        }

        /// <summary>
        /// Standard Deviation of the Ua Radial value.
        /// Based off Lag samples, Bin samples and rho.
        /// Value in m/s.
        /// </summary>
        public double StdDevRadial
        {
            get
            {
                // Check for divide by 0
                if (LagSamples == 0 || BinSamples == 0)
                {
                    return 0.0;
                }

                return 0.034 * (118.0 / LagSamples) * Math.Sqrt(14.0 / BinSamples) * Math.Pow((rho / 0.5), -2.0);
            }
        }

        /// <summary>
        /// Standard deviation of the system.
        /// Based off the standard deviation radial, pings per ensemble and beam angle.
        /// Value in m/s.
        /// </summary>
        public double StdDevSystem
        {
            get
            {
                // Check for divide by 0
                if (CWPP == 0)
                {
                    return 0.0;
                }

                // Use the radial for the standard deviation
                // This is for vertical beams
                if (BeamAngle == 0)
                {
                    return StdDevRadial;
                }

                return StdDevRadial / Math.Sqrt(CWPP) / Math.Sqrt(2.0) / Math.Sin(BeamAngle / 180.0 * Math.PI);
            }
        }

        #endregion

        #region Narrowband Calculations

        /// <summary>
        /// Ta.
        /// Based off Bin size, Speed of Sound and Narrowband beam angle.
        /// Value in seconds.
        /// </summary>
        public double NbTa
        {
            get
            {
                // Check for divide by 0
                if (SpeedOfSound == 0 || BeamAngleRadian == 0)
                {
                    return 0.0;
                }

                return 2.0 * CWPBS / SpeedOfSound / Math.Cos(BeamAngleRadian);
            }
        }

        /// <summary>
        /// Bn.
        /// Based off Ta.
        /// Value in Hz.
        /// </summary>
        public double NbBn
        {
            get
            {
                // Check for divide by 0
                if (NbTa == 0)
                {
                    return 0.0;
                }

                return 1.0 / NbTa;
            }
        }

        /// <summary>
        /// Lamda.
        /// Based off Speed of sound and System frequency.
        /// Value in meters.
        /// </summary>
        public double NbLamda
        {
            get
            {
                // Check for divide by 0
                if (SystemFrequency == 0)
                {
                    return 0.0;
                }

                return SpeedOfSound / SystemFrequency;
            }
        }

        /// <summary>
        /// L.
        /// Based off Speed of sound and Ta.
        /// Value in meters.
        /// </summary>
        public double NbL
        {
            get
            {
                return 0.5 * SpeedOfSound * NbTa;
            }
        }

        /// <summary>
        /// S.D.R
        /// Based off Fudge, Speed of Sound, Lamda, L and SNR.
        /// Value in m/s.
        /// </summary>
        public double NbStdDevRadial
        {
            get
            {
                // Check for divide by 0
                if (NbL == 0 || SNR == 0)
                {
                    return 0;
                }

                return NbFudge * (SpeedOfSound * NbLamda / (8 * Math.PI * NbL)) * Math.Sqrt(1 + 36 / Math.Pow(10, (SNR / 10)) + 30 / Math.Pow(Math.Pow(10, SNR / 10), 2));
            }
        }

        /// <summary>
        /// S.D.H system.
        /// Based off SDR, Beam angle and pings per ensemble.
        /// Value in ?.
        /// </summary>
        public double NbStdDevHSystem
        {
            get
            {
                // Check for divide by 0
                if (CWPP == 0 || BeamAngle == 0)
                {
                    return 0;
                }

                return NbStdDevRadial / Math.Sin(BeamAngle / 180 * Math.PI) / Math.Sqrt(2) / Math.Sqrt(CWPP);
            }
        }

        #endregion

        #region Ensemble Bytes Calculations

        /// <summary>
        /// Calculate the number of times the system will wakeup to ping.
        /// </summary>
        public double Wakeups
        {
            get
            {
                if (CEI > 1.0)
                {
                    if (CWPTBP > 1.0)
                    {
                        return NumEnsembles * CWPP;
                    }
                    else
                    {
                        return NumEnsembles;
                    }
                }

                return 1.0;
            }
        }

        /// <summary>
        /// Calculate the number of ensembles that will be created.
        /// Based off the deployment duration and the number of ensembles
        /// per second that will be created.
        /// Value in Number of ensembles.
        /// </summary>
        public long NumEnsembles
        {
            get
            {
                // Check for divide by 0
                if (CEI == 0)
                {
                    return 0;
                }

                // Convert deployment duration to seconds
                // Then divide by time per ensemble which is in seconds
                return (long)Math.Round((DeploymentDuration * 24.0 * 3600.0) / CEI);
            }
        }

        /// <summary>
        /// Calculate the number of bytes in the Profile overhead.
        /// Value in bytes.
        /// </summary>
        public long ProfileOverhead
        {
            get
            {
                if (CWPON)
                {
                    return ENS_BYTES_PROFILE_OVERHEAD;
                }

                return 0;
            }
        }

        /// <summary>
        /// Calculate the number of bytes per bin.
        /// Based off the number of bytes in a bin and the number of bins.
        /// Value in bytes.
        /// </summary>
        public long BytesPerBin
        {
            get
            {
                if (CWPON)
                {
                    return ENS_BYTES_PER_BIN * CWPBN;
                }

                return 0;
            }
        }

        /// <summary>
        /// Return the number of bytes in the Bottom Track dataset.
        /// Based off Bottom Track being on and the number of bytes in the bottom track dataset.
        /// Value in bytes.
        /// </summary>
        public long BytesBottomTrack
        {
            get
            {
                if (CBTON)
                {
                    return ENS_BYTES_BT;
                }

                return 0;
            }
        }

        /// <summary>
        /// Calculate the number of overhead bytes.
        /// If Water Profile or Bottom Track is on, then use the number of overhead bytes.
        /// Value in bytes.
        /// </summary>
        public long BytesOverhead
        {
            get
            {
                if (CWPON || CBTON)
                {
                    return ENS_BYTES_OVERHEAD;
                }

                return 0;
            }
        }

        /// <summary>
        /// Get the number of bytes that
        /// makeup the checksum.
        /// Value in bytes.
        /// </summary>
        public long BytesChecksum
        {
            get
            {
                return ENS_BYTES_CHECKSUM;
            }
        }

        /// <summary>
        /// Get the number of bytes that makeup the wrapper.
        /// Value in bytes.
        /// </summary>
        public long BytesWrapper
        {
            get
            {
                return ENS_BYTES_WRAPPER;
            }
        }

        /// <summary>
        /// If no pinging is occuring, then this will
        /// give the minimum number of bytes for an ensemble.
        /// Value in bytes.
        /// </summary>
        public long BytesNoPing
        {
            get
            {
                if (!CWPON && !CBTON)
                {
                    return ENS_BYTES_NO_PING;
                }

                return 0;
            }
        }

        /// <summary>
        /// Accumulate all the bytes that make up an ensemble.
        /// Based off the profile overhead, number bytes in a bin, bottom track, overhead, wrapper, checksum and no ping.
        /// Value in bytes.
        /// </summary>
        public long EnsembleSizeBytes
        {
            get
            {
                return ProfileOverhead + BytesPerBin + BytesBottomTrack + BytesOverhead + BytesChecksum + BytesWrapper + BytesNoPing;
            }
        }

        #endregion

        #region XDCR Calculations

        /// <summary>
        /// Percent Bandwidth.
        /// Based off Cycles per element.
        /// Value in % bandwidth.
        /// </summary>
        public double PercentBandwidth
        {
            get
            {
                // Check for divide by 0
                if (CyclesPerElement == 0)
                {
                    return 0;
                }

                return 100.0 / CyclesPerElement;
            }
        }

        /// <summary>
        /// dB of the transducer.
        /// Based off SNR.
        /// Value in dB.
        /// </summary>
        public double dB
        {
            get
            {
                return 10 * Math.Log10(SNR);
            }
        }


        #endregion

        #region Bursts

        /// <summary>
        /// Number of bytes per a burst.
        /// </summary>
        public long BytesPerBurst
        {
            get
            {
                return WavesRecordBytesPerBurst(CBI_NumEnsembles, CWPBN);
            }
        }

        ///// <summary>
        ///// Watt-Hrs per a burst.  This is the 
        ///// power usage for a complete burst.
        ///// </summary>
        //public double WattHoursPerBurst
        //{
        //    get
        //    {
        //        return WavesWattHoursPerBurst(GetSubsystem(), CBI_NumEnsembles, CEI);
        //    }
        //}

        /// <summary>
        /// Number of bursts in the deployment. 
        /// Each ensemble in a burst is seperated by the CEI.
        /// Each burst is sepearted by CBI_BurstInterval
        /// 
        /// Burst = |ENS|__CEI__|ENS|__CEI__|ENS|...
        /// |BURST (CBI)|______|BURST (CBI)|...
        /// </summary>
        public long NumBursts
        {
            get
            {
                // Ensure is burst is enabled
                if (CBI_NumEnsembles > 0 && CBI_BurstInterval > 0)
                {
                    // secondsPerBurst = BURST + CEI
                    double secondsPerBurst = (CBI_NumEnsembles * CEI) + CBI_BurstInterval;

                    // Number seconds for the entire deployment
                    // divided by the number of seconds per burst
                    return (long)Math.Round((DeploymentDuration * 24.0 * 3600.0) / secondsPerBurst);
                }

                return 0;
            }
        }

        #endregion

        #region Results

        /// <summary>
        /// Predicted bottom track range.
        /// Based off Bottom Track range for the 1200000, 600000, 300000, 150000 tables.
        /// Value in meters.
        /// </summary>
        public double PredictedBottomRange
        {
            get
            {
                return BtRange_1200000 + BtRange_600000 + BtRange_300000 + BtRange_150000 + BtRange_75000 + BtRange_38000;
            }
        }

        /// <summary>
        /// Predicted Water Profile range.
        /// Based off Water Profile range for the 1200000, 600000, 300000, 150000 tables.
        /// The range reduction value is added to the range value.
        /// Value in meters.
        /// </summary>
        public double PredictedProfileRange
        {
            get
            {
                return WpRange_1200000 + WpRange_600000 + WpRange_300000 + WpRange_150000 + WpRange_75000 + WpRange_38000 + RangeReduction;
            }
        }

        /// <summary>
        /// Calculate the profile range from the blank, bin size and number of bins.
        /// </summary>
        public double ProfileRangeBinSize
        {
            get
            {
                return CWPBL + (CWPBS * CWPBN);
            }
        }

        /// <summary>
        /// Maximum velocity.
        /// Based off UaRadial and Beam angle.
        /// Value in m/s.
        /// </summary>
        public double MaximumVelocity
        {
            get
            {
                // Check for vertical beam.  No Beam angle
                if (BeamAngle == 0)
                {
                    return UaRadial;
                }

                return UaRadial / Math.Sin(BeamAngle / 180.0 * Math.PI);
            }
        }

        /// <summary>
        /// Standard deviation.
        /// Based off if Broadband is turned on or off and standard deviation for narrowband and broadband.
        /// Value in m/s.
        /// </summary>
        public double StandardDeviation
        {
            get
            {
                // Check if using Broadband or Narrowband
                if ((int)CWPBB_TransmitPulseType > 0)
                {
                    return StdDevSystem;
                }
                else
                {
                    return NbStdDevHSystem;
                }
            }
        }

        /// <summary>
        /// Position of first Profile bin in meters. 
        /// </summary>
        public double ProfileFirstBinPosition
        {
            get
            {
                double pos = 0.0;
                if (CWPBB_TransmitPulseType == Commands.AdcpSubsystemCommands.eCWPBB_TransmitPulseType.NARROWBAND)
                {
                    pos = (2.0 * CWPBS + 0.05) / 2.0;
                }
                else
                {
                    if ((int)CWPBB_TransmitPulseType > 1)
                    {
                        pos = CWPBS;
                    }
                    else
                    {
                        pos = (LagSamples * (CodeRepeats - 1.0) * MetersPerSample + 2.0 * CWPBS + CWPBB_LagLength) / 2.0;
                    }
                }

                return CWPBL + pos;
            }
        }

        /// <summary>
        /// Number of bytes that will be written for the ensemble data
        /// based off the deployment settings.
        /// Based off number of ensembles and ensemble size in bytes.
        /// Value in bytes.
        /// </summary>
        public long DataSizeBytes
        {
            get
            {
                // Burst mode
                if (NumBursts > 0)
                {
                    return NumBursts * BytesPerBurst;
                }

                return NumEnsembles * EnsembleSizeBytes;
            }
        }

        /// <summary>
        /// Number of battery packs used in the system for the given deployment.
        /// Based off Number of battery packs and actual battery power.
        /// Value in number of batteries.
        /// 
        /// Do not use this for burst mode. Use:
        /// AdcpPredictor.WavesRecordWattHours(...) / ActualBatteryPower;
        /// 
        /// </summary>
        public double NumberBatteryPacks
        {
            get
            {
                // Check for divide by 0
                if (ActualBatteryPower == 0)
                {
                    return 0;
                }

                return TotalPower / ActualBatteryPower;
            }
        }

        #endregion

        #endregion

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public AdcpPredictor(AdcpPredictorUserInput input)
        {
            // Initialize Values
            NbFudge = input.NbFudge;

            // Deployment
            DeploymentDuration = input.DeploymentDuration;

            // Commands
            CEI = input.CEI;

            // WP Commands
            CWPON = input.CWPON;
            CWPTBP = input.CWPTBP;
            CWPBN = input.CWPBN;
            CWPBS = input.CWPBS;
            CWPBL = input.CWPBL;
            CWPBB_LagLength = input.CWPBB_LagLength;
            CWPBB_TransmitPulseType = input.CWPBB_TransmitPulseType;
            CWPP = input.CWPP;

            // BT Commands
            CBTON = input.CBTON;
            CBTTBP = input.CBTTBP;
            CBTBB_TransmitPulseType = input.CBTBB_TransmitPulseType;

            // Batteries
            BatteryType = input.BatteryType;
            BatteryDerate = input.BatteryDerate;
            BatterySelfDischargePerYear = input.BatterySelfDischargePerYear;

            // XDCR
            SystemFrequency = input.SystemFrequency;
            SpeedOfSound = input.SpeedOfSound;
            BeamAngle = input.BeamAngle;
            CyclesPerElement = input.CyclesPerElement;
            BroadbandPower = input.BroadbandPower;
            Beta = input.Beta;
            SNR = input.SNR;
            Beams = input.Beams;
            BeamDiameter = input.BeamDiameter;

            // Power
            SystemBootPower = input.SystemBootPower;
            SystemInitPower = input.SystemInitPower;
            SystemRcvPower = input.SystemRcvPower;
            SystemSavePower = input.SystemSavePower;
            SystemSleepPower = input.SystemSleepPower;

            // Time
            SystemWakeupTime = input.SystemWakeupTime;
            SystemInitTime = input.SystemInitTime;
            SystemSaveTime = input.SystemSaveTime;
        }

        #region Subsystem

        /// <summary>
        /// Determine the subsystem based off the
        /// system frequency.
        /// </summary>
        /// <returns></returns>
        private Subsystem GetSubsystem()
        {
            // 1.2mHz
            if (SystemFrequency >= DEFAULT_FREQ_1200000)
            {
                return new Subsystem(Subsystem.SUB_1_2MHZ_4BEAM_20DEG_PISTON_2);
            }

            // 600 kHz
            if (SystemFrequency >= DEFAULT_FREQ_600000)
            {
                return new Subsystem(Subsystem.SUB_600KHZ_4BEAM_20DEG_PISTON_3);
            }

            // 300 kHz
            if (SystemFrequency >= DEFAULT_FREQ_300000)
            {
                return new Subsystem(Subsystem.SUB_300KHZ_4BEAM_20DEG_PISTON_4);
            }

            // 150 kHz
            if (SystemFrequency >= DEFAULT_FREQ_150000)
            {
                return new Subsystem(Subsystem.SUB_150KHZ_4BEAM_30DEG_ARRAY_K);
            }

            // 75 kHz
            if (SystemFrequency >= DEFAULT_FREQ_75000)
            {
                return new Subsystem(Subsystem.SUB_75KHZ_4BEAM_30DEG_ARRAY_L);
            }

            // 38 kHz
            if (SystemFrequency >= DEFAULT_FREQ_38000)
            {
                return new Subsystem(Subsystem.SUB_38KHZ_4BEAM_30DEG_ARRAY_M);
            }

            // 20 kHz
            if (SystemFrequency >= DEFAULT_FREQ_20000)
            {
                return new Subsystem(Subsystem.SUB_20KHZ_4BEAM_30DEG_ARRAY_N);
            }

            // If no frequency is found send an empty subsystem
            return new Subsystem();
        }

        #endregion

        #region Waves

        /// <summary>
        /// File size usage based off number of ensembles in a burst and
        /// the number of bins.  Results in bytes.
        /// </summary>
        /// <param name="SamplesPerBurst">Number of ensembles in a burst. CBI.</param>
        /// <param name="bins">Number of bins. CWPBN.</param>
        /// <returns>Number of bytes in the burst sample.</returns>
        public static long WavesRecordBytesPerBurst(int SamplesPerBurst, uint bins)
        {
            int WavesBytesOverhead = 616 + 4 + 32;
            int WavesBytesPerBin = 112;

            return (SamplesPerBurst * (WavesBytesOverhead + bins * WavesBytesPerBin));
        }

        /// <summary>
        /// Get the number of bytes per deployment duration.  This will determine
        /// how many burst can occur during a deployment.  It will then 
        /// calculate how many bytes are per burst.  It will then calculate
        /// the bytes per number of bursts possible.
        /// </summary>
        /// <param name="samplesPerBurst">Number of ensembles in the burst. CBI.</param>
        /// <param name="bins">Nuber of bins. CWPBN.</param>
        /// <param name="deploymentDuration">Number of days in the deployment.</param>
        /// <param name="burstInterval">The length of a burst in seconds. CBI</param>
        /// <returns>Number of bytes in a deployment.</returns>
        public static long WavesRecordBytesPerDeployment(ushort samplesPerBurst, uint bins, uint deploymentDuration, float burstInterval)
        {
            // Memory per burst
            long burstMem = WavesRecordBytesPerBurst(samplesPerBurst, bins);

            // Get the number of burst per deployment duration
            float deploymentDur = deploymentDuration * 3600.0f * 24.0f;         // Seconds for the deployment duration
            int numBurst = (int)Math.Round(deploymentDur / burstInterval);      // Divide total duration by burst duration

            return burstMem * numBurst;                                         // Multiply the number of bursts by number of bytes per burst
        }


        /// <summary>
        /// Get the power usage based off the subystem given and the ADCP Configuration.  The ADCP Configuration is needed to determine 
        /// how many beams will be transmitted and received.  
        /// 
        /// 4 Beam System
        /// - XMT: 4 Beam
        /// - RCV: 4 Beam
        /// 
        /// 5 Beam System
        /// 4Beam Ping
        /// - XMT: 4 Beam
        /// - RCV: 5 Beam
        /// Vertical Ping
        /// - XMT: 1 Beam
        /// - RCV: 5 Beam
        /// 
        /// A 5 beam system will XMT the number of beams, will receive on all channels.  So the power on this type of system will consume
        /// more power.  Even if only the vertical beam is pinged, it will consume about the same amount as the 4 beam, because the 
        /// receive channel will dominate the power usage.
        /// 
        /// </summary>
        /// <param name="subsystem">Subsystem to transmit.</param>
        /// <param name="adcpConfig">ADCP Configuration.</param>
        /// <param name="samplesPerBurst">Number of ensembles in the burst. CBI.</param>
        /// <param name="sampleRate">Number of seconds between samples. CEI.</param>
        /// <param name="deploymentDuration">Number of days in the deployment.</param>
        /// <param name="burstInterval">The length of a burst in seconds. CBI.</param>
        /// <param name="isVerticalBeam">Flag if the subsystem is a vertical beam.</param>
        /// <returns>Watt/Hrs for the current configuration.</returns>
        public static double WavesRecordWattHours(Subsystem subsystem, AdcpConfiguration adcpConfig, ushort samplesPerBurst, double sampleRate, uint deploymentDuration, float burstInterval)
        {
            // Get the info of the ADCP
            // This will find the Primary ADCP
            // The Secondary ADCP and/or the vertical beam if they exist.
            SubsystemInfo primSS = null;
            SubsystemInfo vertSS = null;
            SubsystemInfo dualSS = null;
            bool containsVertical = false;
            bool containsDual = false;
            foreach (var ss in adcpConfig.SerialNumber.SubSystemsList)
            {
                // Find the primary beams
                SubsystemInfo findSS = new SubsystemInfo(ss);
                if (findSS.IsPrimaryBeams)
                {
                    primSS = findSS;
                }

                // Check if there is any vertical beam
                if(findSS.IsVerticalBeam)
                {
                    containsVertical = true;
                    vertSS = findSS;
                }

                // Check if a dual frequecy
                // The offset will be 45 degrees
                if(findSS.OffsetAngle == 45)
                {
                    containsDual = true;
                    dualSS = findSS;
                }
            }

            // Get the info for the subsystem that needs the power usage
            SubsystemInfo info = new SubsystemInfo(subsystem);

            // If the subsystem is the a vertical beam, we need the primary ADCP
            // and the Vertical beam info.  
            //
            // If this subsystem is a vertical beam subsystem, the flag will be set.  If it is not
            // set then we must be in the primary subsystem.
            if (containsVertical)
            {
                if (primSS != null)
                {
                    // Vertical Beam power usage
                    return AdcpPredictor.WavesRecordWattHoursPerDeployment(primSS.Subsystem,                                            // Primary ADCP Subsystem 
                                                                                        vertSS.Subsystem,                               // Vertical Beam Subsystem 
                                                                                        samplesPerBurst,                                // Samples in Burst
                                                                                        sampleRate,                                     // Sample Rate
                                                                                        deploymentDuration,                             // Deployment Duration
                                                                                        burstInterval,                                  // Burst Interval
                                                                                        info.IsVerticalBeam,                            // Flag if the current subsystem is a vertical beam or 4 beam.
                                                                                        false,                                          // Flag if the current subsystem is a dual frequency 4 beam
                                                                                        primSS.NumBeams,                                // Number of beams in Primary ADCP
                                                                                        1);                                             // Number of beams in Secondary ADCP. Since its a vertical beam system, it will always be 1
                }
            }

            // Dual Frequency System
            // The Dual frequency needs to state that the receive pwr will include 8 beams.
            // We need to tell it what frequencies will be received.
            if(containsDual)
            {
                if (primSS != null)
                {
                    // Check if this subsystem is the secondary ADCP
                    // The offset will be 45 degrees
                    if (info.OffsetAngle == 45)
                    {
                        // Secondary Subsystem power usage
                        return AdcpPredictor.WavesRecordWattHoursPerDeployment(primSS.Subsystem, subsystem, samplesPerBurst, sampleRate, deploymentDuration, burstInterval, info.IsVerticalBeam, true, info.NumBeams, dualSS.NumBeams);
                    }
                    else
                    {
                        // Primary Subsystem power usage
                        return AdcpPredictor.WavesRecordWattHoursPerDeployment(subsystem, dualSS.Subsystem, samplesPerBurst, sampleRate, deploymentDuration, burstInterval, info.IsVerticalBeam, false, info.NumBeams, dualSS.NumBeams);
                    }
                }
            }


            // 4 Beam power usage
            return AdcpPredictor.WavesRecordWattHoursPerDeployment(subsystem, null, samplesPerBurst, sampleRate, deploymentDuration, burstInterval, info.IsVerticalBeam, false, info.NumBeams, 0);
        }

        /// <summary>
        /// Get the watt hours per deployment.  This will determine
        /// how many burst can occur during a deployment.  It will then 
        /// calculate how much power per burst.  It will then calculate
        /// the watt hours per number of bursts possible.
        /// </summary>
        /// <param name="ssPrimary">Subsystem on of the non 4 beam.</param>
        /// <param name="ssSecondary">Subsytem of the vertical beam. Set NULL if it does not have a vertical beam.</param>
        /// <param name="samplesPerBurst">Number of ensembles in the burst. CBI.</param>
        /// <param name="sampleRate">Number of seconds between samples. CEI.</param>
        /// <param name="deploymentDuration">Number of days in the deployment.</param>
        /// <param name="burstInterval">The length of a burst in seconds. CBI.</param>
        /// <param name="isVerticalBeam">Flag if the subsystem is a vertical beam.</param>
        /// <param name="isSecondarySS">Flag if the secondary subsystem in the dual frequency system needs to be measured.  TRUE = secondary, False = Primary.</param>
        /// <param name="numBeamsForPrimary">Number of beams in the Primary system.  May be a 3 beam system.</param>
        /// <param name="numBeamsForSecondary">Number of beams in the Secondary system.  May be a 3 beam system or vertical system.</param>
        /// <returns>Watt Hours in a deployment.</returns>
        public static double WavesRecordWattHoursPerDeployment(Subsystem ssPrimary, Subsystem ssSecondary, ushort samplesPerBurst, double sampleRate, uint deploymentDuration, float burstInterval, bool isVerticalBeam, bool isSecondarySS, int numBeamsForPrimary = 4, int numBeamsForSecondary = 4)
        {
            // Power per burst
            double burstPwr = WavesWattHoursPerBurst(ssPrimary, ssSecondary, samplesPerBurst, sampleRate, isVerticalBeam, isSecondarySS, numBeamsForPrimary, numBeamsForSecondary);

            // Get the number of burst per deployment duration
            float deploymentDur = deploymentDuration * 3600.0f * 24.0f;         // Seconds for the deployment duration
            int numBurst = (int)Math.Round(deploymentDur / burstInterval);      // Divide total duration by burst duration

            return burstPwr * numBurst;                                         // Multiply the number of bursts by power per burst
        }

        /// <summary>
        /// Power usage of a burst in Watt-Hrs.
        /// It is determined based off the subsystem, number of ensembles in the
        /// burst and the time between each ensemble in the burst.
        /// </summary>
        /// <param name="primarySS">Primary Subsystem type for 4 Beam.</param>
        /// <param name="secondarySS">Secondary Subsystem type for Vertical beam or dual frequency.  If there is no vertical beam or dual frequency, set null.</param>
        /// <param name="SamplesPerBurst">Number of ensembles in the burst. CBI_NumEnsembles command.</param>
        /// <param name="samplerate">Sample rates for the burst in seconds.  Number of seconds between ensembles in the burst.  CEI command.</param>
        /// <param name="isVerticalBeam">Flag if the subsystem is a vertical beam.</param>
        /// <param name="isSecondarySS">Flag if the secondary subsystem in the dual frequency system needs to be measured.  TRUE = secondary, False = Primary.</param>
        /// <param name="numBeamsForPrimary">Number of beams for 4 Beam system.</param>
        /// <param name="numBeamsForSecondary">Number of beams for the secondary system.</param>
        /// <returns>Watt-Hrs consumed for a burst based off settings.</returns>
        public static double WavesWattHoursPerBurst(Subsystem primarySS, Subsystem secondarySS, ushort SamplesPerBurst, double samplerate, bool isVerticalBeam, bool isSecondarySS, int numBeamsForPrimary = 4, int numBeamsForSecondary = 4)
        {
            //assumptions:            
            //ADCP01 electronics

            //example per day
            //xmt W-hr    = 24 * (2048 * 0.00155) * xWatts / 3600
            //awake W-hr  = 24 * (2048 * 0.5 + 2) * rWatts / 3600
            //asleep W-hr = 24 * 0.00125
            //for 600 kHz the 440 w-hr 2x19 pack lasts 10 days burst once per hour

            double WavesXmtWatts1250 = 20.0;
            double WavesXmtWatts625 = 60.0;
            double WavesXmtWatts313 = 200.0;
            double WavesXmtWatts156 = 1000.0;
            double WavesXmtWatts78 = 2000.0;
            double WavesXmtWatts39 = 4000.0;
            double WavesXmtWatts20 = 8000.0;

            double WavesRcvWatts4Beam = 3.8;
            double WavesRcvWatts5Beam = 4.5;
            double WavesRcvWatts7Beam = 5.25;           //(WavesRcvWatts8Beam / 8.0) * 7.0;
            double WavesRcvWatts8Beam = 6.0;

            double WavesRcvWatts = WavesRcvWatts4Beam;
            double WavesSleepWatts = 0.00125;

            double xmtWatts = 0.0;
            double xmtWattsSecondary = 0.0;
            double rcvWatts = WavesRcvWatts;
            double sleepWatts = WavesSleepWatts;

            if (secondarySS != null)
            {
                switch (secondarySS.CodeToChar())
                {   
                    case '6':
                        rcvWatts = WavesRcvWatts8Beam;                  // Dual Frequency
                        if (numBeamsForPrimary == 3)                    // Check if there are 4 or 3 beams in the primary subsystem.  3 Beams = 7Beam system.  4 beams = Dual Frequency.
                        { 
                            rcvWatts = WavesRcvWatts7Beam; 
                        } 
                        xmtWattsSecondary = WavesXmtWatts1250;
                        break;
                    case '7':
                        rcvWatts = WavesRcvWatts8Beam;                  // Dual Frequency
                        if (numBeamsForPrimary == 3)                    // Check if there are 4 or 3 beams in the primary subsystem.  3 Beams = 7Beam system.  4 beams = Dual Frequency.
                        { 
                            rcvWatts = WavesRcvWatts7Beam; 
                        } 
                        xmtWattsSecondary = WavesXmtWatts625;
                        break;
                    case '8':
                        rcvWatts = WavesRcvWatts8Beam;                  // Dual Frequency
                        if (numBeamsForPrimary == 3)                    // Check if there are 4 or 3 beams in the primary subsystem.  3 Beams = 7Beam system.  4 beams = Dual Frequency.
                        { 
                            rcvWatts = WavesRcvWatts7Beam; 
                        } 
                        xmtWattsSecondary = WavesXmtWatts313;
                        break;
                    case 'A':
                        rcvWatts = WavesRcvWatts5Beam;                  // 5 Beam
                        if (numBeamsForPrimary == 3)                    // Check if there are 4 or 3 beams in the primary subsystem.  3 Beams = 7Beam system.  4 beams = Dual Frequency.
                        { 
                            rcvWatts = WavesRcvWatts7Beam; 
                        } 
                        xmtWattsSecondary = WavesXmtWatts1250;
                        break;
                    case 'B':
                        rcvWatts = WavesRcvWatts5Beam;                  // 5 Beam
                        if (numBeamsForPrimary == 3)                    // Check if there are 4 or 3 beams in the primary subsystem.  3 Beams = 7Beam system.  4 beams = Dual Frequency.
                        { 
                            rcvWatts = WavesRcvWatts7Beam; 
                        } 
                        xmtWattsSecondary = WavesXmtWatts625;
                        break;
                    case 'C':
                        rcvWatts = WavesRcvWatts5Beam;                  // 5 Beam
                        if (numBeamsForPrimary == 3)                    // Check if there are 4 or 3 beams in the primary subsystem.  3 Beams = 7Beam system.  4 beams = Dual Frequency.
                        { 
                            rcvWatts = WavesRcvWatts7Beam; 
                        } 
                        xmtWattsSecondary = WavesXmtWatts313;
                        break;
                    default:
                    case '0':
                        break;
                }
            }

            switch (primarySS.CodeToChar())
            {
                default:
                    xmtWatts = 0;
                    rcvWatts = 0;
                    sleepWatts = 0;
                    break;
                case '2':
                case '6':
                    xmtWatts = WavesXmtWatts1250;
                    break;
                case '3':
                case '7':
                case 'I':
                case 'O':
                    xmtWatts = WavesXmtWatts625;
                    break;
                case '4':
                case '8':
                case 'J':
                case 'P':
                    xmtWatts = WavesXmtWatts313;
                    break;
                case 'K':
                case 'Q':
                    xmtWatts = WavesXmtWatts156;
                    break;
                case 'L':
                case 'R':
                    xmtWatts = WavesXmtWatts78;
                    break;
                case 'M':
                case 'S':
                    xmtWatts = WavesXmtWatts39;
                    break;
                case 'N':
                case 'T':
                    xmtWatts = WavesXmtWatts20;
                    break;
            }

            // Check if the system is a dual frequency or vertical system
            // They will consume more power because of the extra channels in the receiver board
            if (secondarySS != null)
            {
                // VERTICAL BEAM
                if (isVerticalBeam)
                {
                    // Divide by the number of beams to get just the vertical xmt watts
                    xmtWatts /= numBeamsForPrimary;
                }
                // DUAL FREQUENCY
                // Secondary Subsystem pinging
                else if (isSecondarySS)
                {
                    // Determine the number of beams and the transmit power for the secondary subsystem
                    var secondaryXmt = xmtWattsSecondary;
                    if(numBeamsForSecondary != 4)
                    {
                        secondaryXmt = (xmtWatts / 4.0) * numBeamsForSecondary;
                    }

                    // Use Secondary SS XMT power
                    xmtWatts = secondaryXmt;
                }
                // Primary Subsystem pinging
                else
                {
                    // Determine the number of beams and the transmit power for primary subsystem
                    var primaryXmt = xmtWatts;
                    if (numBeamsForPrimary != 4)
                    {
                        primaryXmt = (xmtWatts / 4.0) * numBeamsForPrimary;
                    }

                    // Use the Primaary Subsystem XMT power
                    xmtWatts = primaryXmt;
                }
            }

            double WattHours = (2.0 + SamplesPerBurst * (0.00155 * xmtWatts + samplerate * rcvWatts)) / 3600.0 + 24.0 * sleepWatts;
            return WattHours;
        }

        ///// <summary>
        ///// Take into account 2 subsystems when calculating the power usage.
        ///// </summary>
        ///// <param name="ssA">Subsystem A.  4 beam system.</param>
        ///// <param name="ssB">Subsystem B. Vertical beam system.</param>
        ///// <param name="SamplesPerBurst">Number of ensembles in burst. CBI</param>
        ///// <param name="samplerate">CEI.  Time between each sample.</param>
        ///// <param name="BurstsPerDay">Number of bursts in a day.</param>
        ///// <returns>Total Watt hours used by the system for a day.</returns>
        //public static double WavesRecordWattHoursPerDayBurst(Subsystem ssA, Subsystem ssB, int SamplesPerBurst, double samplerate, double BurstsPerDay, bool isVerticalBeam)
        //{
        //    //assumptions:            
        //    //ADCP01 electronics

        //    //example per day
        //    //xmt W-hr    = 24 * (2048 * 0.00155) * xWatts / 3600
        //    //awake W-hr  = 24 * (2048 * 0.5 + 2) * rWatts / 3600
        //    //asleep W-hr = 24 * 0.00125
        //    //for 600 kHz the 440 w-hr 2x19 pack lasts 10 days burst once per hour

        //    double WavesXmtWatts1250 = 20;
        //    double WavesXmtWatts625 = 60;
        //    double WavesXmtWatts313 = 200;
        //    double WavesXmtWatts156 = 1000;
        //    double WavesXmtWatts78 = 2000;
        //    double WavesXmtWatts39 = 4000;
        //    double WavesXmtWatts20 = 8000;

        //    double WavesRcvWatts = 4.5;
        //    double WavesSleepWatts = 0.00125;

        //    double xmtWatts = 0;
        //    double rcvWatts = WavesRcvWatts;
        //    double sleepWatts = WavesSleepWatts;

        //    switch (ssB.CodeToChar())
        //    {
        //        case 'A':
        //        case 'B':
        //        case 'C':
        //        default:
        //            rcvWatts *= 1.25;
        //            break;
        //        case '0':
        //            break;
        //    }

        //    switch (ssA.CodeToChar())
        //    {
        //        default:
        //            xmtWatts = 0;
        //            rcvWatts = 0;
        //            sleepWatts = 0;
        //            break;
        //        case '2':
        //        case '6':
        //            xmtWatts = WavesXmtWatts1250;
        //            break;
        //        case '3':
        //        case '7':
        //        case 'I':
        //        case 'O':
        //            xmtWatts = WavesXmtWatts625;
        //            break;
        //        case '4':
        //        case '8':
        //        case 'J':
        //        case 'P':
        //            xmtWatts = WavesXmtWatts313;
        //            break;
        //        case 'K':
        //        case 'Q':
        //            xmtWatts = WavesXmtWatts156;
        //            break;
        //        case 'L':
        //        case 'R':
        //            xmtWatts = WavesXmtWatts78;
        //            break;
        //        case 'M':
        //        case 'S':
        //            xmtWatts = WavesXmtWatts39;
        //            break;
        //        case 'N':
        //        case 'T':
        //            xmtWatts = WavesXmtWatts20;
        //            break;
        //    }

        //    double WattHours = BurstsPerDay * (2 + SamplesPerBurst * (0.00155 * xmtWatts + samplerate * rcvWatts)) / 3600 + 24 * sleepWatts;
        //    return WattHours;
        //}

        /// <summary>
        /// Default values based off the subsystem type.
        /// </summary>
        /// <param name="ss">Subsystem type.</param>
        /// <param name="Blank">Blank size in meters. CWPBL.</param>
        /// <param name="BinSize">Bin size in meters. CWPBS.</param>
        /// <param name="Lag">Lag length in meters. CWPBB.</param>
        public static void WavesDefaults(Subsystem ss, out double Blank, out double BinSize, out double Lag)
        {
            Blank = 0.0;
            BinSize = 0.0;
            Lag = 0.0;
            switch (ss.CodeToChar())
            {
                default:
                    break;
                case 'A':
                case '2'://1200 20
                case '6':
                    Blank = 0.5;
                    BinSize = 0.25;
                    Lag = 0.10;
                    break;
                case 'B':
                case '3'://600 20
                case '7':
                    Blank = 0.5;
                    BinSize = 0.5;
                    Lag = 0.20;
                    break;
                case 'I'://600 30
                    Blank = 0.5;
                    BinSize = 0.5;
                    Lag = 0.20;
                    break;
                case 'O'://600 15
                    Blank = 0.5;
                    BinSize = 0.5;
                    Lag = 0.20;
                    break;
                case 'C':
                case '4'://300 20
                case '8':
                    Blank = 1.0;
                    BinSize = 1.0;
                    Lag = 0.40;
                    break;
                case 'J'://300 30
                    Blank = 1.0;
                    BinSize = 1.0;
                    Lag = 0.40;
                    break;
                case 'P'://300 15
                    Blank = 1.0;
                    BinSize = 1.0;
                    Lag = 0.40;
                    break;
                case 'K'://150 30
                    Blank = 2.0;
                    BinSize = 2.0;
                    Lag = 0.80;
                    break;
                case 'Q':// 150 15
                    Blank = 2.0;
                    BinSize = 2.0;
                    Lag = 0.80;
                    break;
                case 'L'://75 30
                    Blank = 4.0;
                    BinSize = 4.0;
                    Lag = 1.60;
                    break;
                case 'R'://75 15
                    Blank = 4.0;
                    BinSize = 4.0;
                    Lag = 1.60;
                    break;
                case 'M'://38 30
                    Blank = 8.0;
                    BinSize = 8.0;
                    Lag = 3.20;
                    break;
                case 'S'://38 15
                    Blank = 8.0;
                    BinSize = 8.0;
                    Lag = 3.20;
                    break;
                case 'N'://20 30
                    Blank = 16.0;
                    BinSize = 16.0;
                    Lag = 6.40;
                    break;
                case 'T'://20 15
                    Blank = 16.0;
                    BinSize = 16.0;
                    Lag = 6.40;
                    break;
            }
        }

        /// <summary>
        /// Calculate the Range, standard deviation, Maximum velocity (ambiguity velocity) and 
        /// first bin depth based off the values given.
        /// </summary>
        /// <param name="ss">Subsystem type.</param>
        /// <param name="BinSize">Bin size in meters. CWPBS.</param>
        /// <param name="Blank">Blank size in meters. CWPBL.</param>
        /// <param name="Lag">Broadband lag length. CWPBB.</param>
        /// <param name="Range">Profile range in meters.</param>
        /// <param name="sd">Velocity Standard Deviation in meters.</param>
        /// <param name="Uar">Maximum velocity in meters per second. (Ambiguity Velocity) </param>
        /// <param name="firstbinlocation">Location of the first bin in meters.</param>
        public static void WavesModelPUV(Subsystem ss, double BinSize, double Blank, double Lag,
                                        out double Range, out double sd, out double Uar, out double firstbinlocation)
        {
            //assumptions:
            //high SNR for first bin
            //xmt = bin size
            //1490 Speed of Sound
            //ADCP01 electronics

            double Freq = 1245125.0;
            double StandardBinSize = 1.0;
            double StandardRange = 0.0;
            double BA = 1.0 / 180.0 * Math.PI;
            switch (ss.CodeToChar())
            {
                default:
                    break;
                case 'A':
                    Freq /= 1.0;
                    BA *= 0.0;
                    StandardBinSize *= 1;
                    StandardRange = 20;
                    break;
                case 'B':
                    Freq /= 2.0;
                    BA *= 0.0;
                    StandardBinSize *= 2;
                    StandardRange = 50;
                    break;
                case 'C':
                    Freq /= 4.0;
                    BA *= 0.0;
                    StandardBinSize *= 4;
                    StandardRange = 100;
                    break;
                case '2'://1200 20
                case '6':
                    Freq /= 1.0;
                    BA *= 20.0;
                    StandardBinSize *= 1.0;
                    StandardRange = 20.0;
                    break;
                case '3'://600 20
                case '7':
                    Freq /= 2.0;
                    BA *= 20.0;
                    StandardBinSize *= 2.0;
                    StandardRange = 50.0;
                    break;
                case 'I'://600 30
                    Freq /= 2.0;
                    BA *= 30.0;
                    StandardBinSize *= 2.0;
                    StandardRange = 54.0;
                    break;
                case 'O'://600 15
                    Freq /= 2.0;
                    BA *= 15.0;
                    StandardBinSize *= 2.0;
                    StandardRange = 49.0;
                    break;
                case '4'://300 20
                case '8':
                    Freq /= 4.0;
                    BA *= 20.0;
                    StandardBinSize *= 4.0;
                    StandardRange = 125.0;
                    break;
                case 'J'://300 30
                    Freq /= 4.0;
                    BA *= 30.0;
                    StandardBinSize *= 4.0;
                    StandardRange = 125.0;
                    break;
                case 'P'://300 15
                    Freq /= 4.0;
                    BA *= 15.0;
                    StandardBinSize *= 4.0;
                    StandardRange = 125.0;
                    break;
                case 'K'://150 30
                    Freq /= 8.0;
                    BA *= 30.0;
                    StandardBinSize *= 8.0;
                    StandardRange = 200.0;
                    break;
                case 'Q':// 150 15
                    Freq /= 8.0;
                    BA *= 15.0;
                    StandardBinSize *= 8.0;
                    StandardRange = 200.0;
                    break;
                case 'L'://75 30
                    Freq /= 16.0;
                    BA *= 30.0;
                    StandardBinSize *= 16.0;
                    StandardRange = 400;
                    break;
                case 'R'://75 15
                    Freq /= 16.0;
                    BA *= 15.0;
                    StandardBinSize *= 16.0;
                    StandardRange = 400.0;
                    break;
                case 'M'://38 30
                    Freq /= 32.0;
                    BA *= 30.0;
                    StandardBinSize *= 32.0;
                    StandardRange = 800.0;
                    break;
                case 'S'://38 15
                    Freq /= 32.0;
                    BA *= 15.0;
                    StandardBinSize *= 32.0;
                    StandardRange = 800.0;
                    break;
                case 'N'://20 30
                    Freq /= 64.0;
                    BA *= 30.0;
                    StandardBinSize *= 64.0;
                    StandardRange = 1500.0;
                    break;
                case 'T'://20 15
                    Freq /= 64.0;
                    BA *= 15.0;
                    StandardBinSize *= 64.0;
                    StandardRange = 1500.0;
                    break;
            }

            double SR = Freq * 4.0 / 3.0 / 16.0;
            double MetersPerSample = Math.Cos(BA) * 1490.0 / 2.0 / SR;
            double BS = (int)(BinSize / MetersPerSample);
            double LS = 2.0 * (int)((int)((Lag / MetersPerSample) + 1.0) / 2.0);
            int repeats;

            if ((int)(BS / LS) + 1.0 < 2.0)
                repeats = 2;
            else
                repeats = (int)(BS / LS) + 1;

            double rho;

            //if (Blank + 2 * BinSize < Lag)
            //{
            //    firstbinlocation = Blank + BinSize;
            //    rho = 1.0;//pulse coherent
            //}
            //else
            {
                firstbinlocation = Blank + ((repeats - 1.0) * Lag + 2.0 * BinSize + Lag) / 2.0;
                rho = (repeats - 1.0) / repeats;
            }

            Uar = SR / (2.0 * LS) * 1490.0 / (2.0 * Freq);
            sd = 0.034 * (118.0 / LS) * Math.Sqrt(14.0 / BS) * Math.Pow((rho / 0.5), -2.0);


            if (BA > 0)
            {
                Uar /= Math.Sin(BA);//bring to the horizontal
                sd /= Math.Sin(BA);//bring to the horizontal
                sd /= Math.Sqrt(2.0);//use 2 beams for horizontal solution
            }

            double dB = 10.0 * Math.Log10(BinSize / StandardBinSize);
            Range = StandardRange + dB * StandardBinSize;

        }

        #endregion


    }
}
