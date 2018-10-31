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
 * 04/23/2013      RC          3.0.0      Initial coding
 * 05/22/2013      RC          3.0.0      Added MaintenceLog.
 * 06/17/2013      RC          3.0.1      Added DisplayDataMode.
 *                                         Added AdcpBinaryCodec and EventHandler.
 *                                         Added Eventhandlers for the serial port when receiving data.
 *                                         Added ProjectManager.
 *                                         Added events to publish ensembles.
 * 06/26/2013      RC          3.0.2      Use EventAggregator to publish the ensembles.
 * 07/12/2013      RC          3.0.4      Added ClearMaintenceLog() and change AddMaintenceEntry().
 * 07/29/2013      RC          3.0.6      Subscribe to AdcpSerialPort Download Events.
 *                                         Pass serial port DownloadEvents to others.
 * 07/30/2013      RC          3.0.6      Subscribe to AdcpSerialPort Upload Events.
 *                                         Pass serial port UploadEvents to others.
 * 08/02/2013      RC          3.0.7      Added GpsSerialPort and AdcpDvlCodec.
 * 08/07/2013      RC          3.1.0      Added SerialConnectEvent and SerialDisconnectEvent.
 * 10/14/2013      RC          3.2.0      Added IsAdcpSerialConnected() to know if the serial port is connected. 
 * 10/15/2013      RC          3.2.0      Added ReconnectAdcpSerial().
 * 11/18/2013      RC          3.2.0      Added SetAdcpConfiguration() to set the configuration properly to a project.
 * 01/17/2014      RC          3.2.3      Added GPS1, GPS2, NMEA1 and NMEA2 serial ports.
 * 01/20/2014      RC          3.2.3      Initalize the GPS connections.
 * 01/24/2014      RC          3.2.3      Update the serial number to the project list in PulseDB in SetAdcpConfiguration().
 * 02/07/2014      RC          3.2.3      Added AdcpGpsBuffer to store the data in a seperate column in the project file.
 * 02/21/2014      RC          3.2.3      In CreateValidationTestWriter(), if the ADCP is in DVL mode, do not use the serial number for the file name.
 * 06/20/2014      RC          3.3.1      Added PD6_13Codec.
 * 06/27/2014      RC          3.4.0      Set the source and type when receiving the data from the codecs.  Added Source and Type to EnsembleData struct.
 * 07/14/2014      RC          3.4.0      Average the incoming data.
 * 07/24/2014      RC          3.4.0      Added AdcpTcpIp.
 * 08/05/2014      RC          3.4.0      Shutdown all the connections properly.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/22/2014      RC          4.0.2      Shutdown the GPS and NMEA serial ports properly.
 *                                         Added raw recording for all the serial ports.
 * 09/17/2014      RC          4.0.3      Replaced all the seperate codecs with AdcpCodec.
 * 09/18/2014      RC          4.1.0      Added GetAdcpConfiguration().
 * 09/22/2014      RC          4.1.0      Added STIME commands.
 * 01/16/2015      RC          4.1.0      Added PublishEnsembleData() to publish data from the importer that has already been decoded.
 * 04/16/2015      RC          4.1.2      Added TestEthernetConnection() to test the ethernet connection.
 * 05/07/2015      RC          4.1.2      Check if the ensemble is null in PublishEnsemble() before publishing.
 * 09/28/2015      RC          4.2.1      Make all the event handlers run in a thread.      
 * 12/02/2015      RC          4.4.0      Added, DataBit, Parity and Stop Bit to the ADCP serial port at startup.
 * 12/03/2015      RC          4.4.0      Changed VT file name to RTI.
 * 05/12/2016      RC          4.4.3      Check for null in StopRawAdcpRecord().
 * 09/28/2016      RC          4.4.4      Increase the time to run the DiagSpectrum test to 180 seconds for dual frequency systems.
 * 09/17/2017      RC          4.4.7      Added AdcpUdp and removed AdcpTcp.
 * 09/28/2017      RC          4.4.7      Added original data format to handle PD0 transformation.
 * 01/03/2018      RC          4.6.0      Do not send a BREAK at startup of serial port.
 * 02/07/2018      RC          4.7.2      Return the file name when recording stops in StopValidationTest().
 * 03/28/2018      RC          4.8.1      Use the DataFormatOptions when adding the data to the ADCP codec.
 * 04/23/2018      RC          4.9.0      Limit the file size recorded to 16mb.
 * 08/17/2018      RC          4.10.2     Lock the ensemble with SyncRoot when screening and averaging the data.
 * 10/31/2018      RC          4.11.0     Added EngBeamShowTest().
 * 
 */

using System.Diagnostics;

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using System.IO;
    using System.Threading;
    using System.Collections.Concurrent;
    using ReactiveUI;
    using System.Threading.Tasks;
    using System.Net;


    /// <summary>
    /// This object will contain a connection to an ADCP.
    /// This will give the ability to send and receive data
    /// from the ADCP.
    /// 
    /// The ADCP can be connected to the computer through
    /// more than 1 way.  This will make it so communication
    /// to ADCP is available with whatever communication is used.
    /// </summary>
    public class AdcpConnection : IDisposable
    {
        #region Enums and Classes

        /// <summary>
        /// Enum to set what mode the ADCP is in.
        /// Is it displaying live or playback data.
        /// </summary>
        public enum DisplayDataMode
        {
            /// <summary>
            /// Displaying live data.
            /// </summary>
            Live,

            /// <summary>
            /// Displaying playback data.
            /// </summary>
            Playback
        }

        /// <summary>
        /// Adcp Communication types.
        /// </summary>
        public enum AdcpCommTypes
        {
            /// <summary>
            /// Serial connection.
            /// </summary>
            Serial,

            /// <summary>
            /// ADCP Ethernet connection.
            /// </summary>
            Ethernet,

            /// <summary>
            /// TCP connection.
            /// This connection is used to received
            /// data from a TCP server.
            /// </summary>
            TCP,

            /// <summary>
            /// UDP connection.
            /// This connection is used to received
            /// data from a UDP server.
            /// </summary>
            UDP

        }

        /// <summary>
        /// Class to hold the binary and ensemble object to be processed.
        /// </summary>
        public class EnsembleData
        {
            /// <summary>
            /// Binary data of the ensemble.
            /// </summary>
            public byte[] BinaryEnsemble { get; set; }

            /// <summary>
            /// Ensemble object.
            /// </summary>
            public DataSet.Ensemble Ensemble { get; set; }

            /// <summary>
            /// Source of the ensemble.
            /// </summary>
            public EnsembleSource Source { get; set; }

            /// <summary>
            /// Ensemble type.  Single ensemble
            /// or averaged ensemble.
            /// </summary>
            public EnsembleType Type { get; set; }

            /// <summary>
            /// Original Data format when data was decoded.
            /// </summary>
            public AdcpCodec.CodecEnum OrigDataFormat { get; set; }

            /// <summary>
            /// Initialize the values.
            /// </summary>
            /// <param name="binaryEnsemble">Binary data of the ensemble.</param>
            /// <param name="ensemble">Ensemble object.</param>
            /// <param name="source">Source of the ensemble.</param>
            /// <param name="type">Type of ensemble.</param>
            /// <param name="origDataFormat">Original Data format.</param>
            public EnsembleData(byte[] binaryEnsemble, DataSet.Ensemble ensemble, EnsembleSource source, EnsembleType type, AdcpCodec.CodecEnum origDataFormat)
            {
                BinaryEnsemble = binaryEnsemble;
                Ensemble = ensemble;
                Source = source;
                Type = type;
                OrigDataFormat = origDataFormat;
            }
        }

        /// <summary>
        /// All the types of data that can be received that need to be
        /// processed.
        /// </summary>
        private enum ProcessDataTypes
        {
            /// <summary>
            /// ADCP Serial data.
            /// </summary>
            AdcpSerial,

            /// <summary>
            /// Ensemble data.
            /// </summary>
            Ensemble,

            /// <summary>
            /// GPS 1 serial data.
            /// </summary>
            GPS1,

            /// <summary>
            /// GPS 2 serial data.
            /// </summary>
            GPS2,

            /// <summary>
            /// NMEA 1 serial data.
            /// </summary>
            NMEA1,

            /// <summary>
            /// NMEA 2 serial data.
            /// </summary>
            NMEA2,

            /// <summary>
            /// Data received from ADCP codec.
            /// </summary>
            CODEC
        }

        /// <summary>
        /// Hold the data to be processed.
        /// This data will be queued.  The thread
        /// will determine how the process the data
        /// based off the type.
        /// </summary>
        private struct ProcessData
        {
            /// <summary>
            /// Receive data type.
            /// </summary>
            public ProcessDataTypes type;

            /// <summary>
            /// Data to process.
            /// </summary>
            public byte[] data;

            /// <summary>
            /// Ensemble is the data type stores ensembles.
            /// </summary>
            public DataSet.Ensemble ensemble;

            /// <summary>
            /// Original data format of the data.
            /// </summary>
            public AdcpCodec.CodecEnum dataFormat;
        }

        #endregion

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Project manager.
        /// </summary>
        private PulseManager _pm;

        /// <summary>
        /// ViewModel to screen the data.
        /// </summary>
        private ScreenDataBaseViewModel _screenDataVM;

        /// <summary>
        /// ViewModel to average the data.
        /// </summary>
        private AveragingBaseViewModel _averagingVM;

        /// <summary>
        /// Event Aggregator.
        /// </summary>
        private IEventAggregator _events;

        /// <summary>
        /// Decode the ADCP data into 
        /// an ensemble.
        /// </summary>
        private AdcpCodec _adcpCodec;

        /// <summary>
        /// Buffer to hold all the ensembles processed by the codecs.
        /// </summary>
        private ConcurrentQueue<EnsembleData> _ensembleBuffer;

        /// <summary>
        /// Buffer for the GPS 1 data.
        /// </summary>
        private ConcurrentQueue<byte[]> _gps1Buffer;

        /// <summary>
        /// Buffer for the GPS 2 data.
        /// </summary>
        private ConcurrentQueue<byte[]> _gps2Buffer;

        /// <summary>
        /// Buffer for the NMEA 1 data.
        /// </summary>
        private ConcurrentQueue<byte[]> _nmea1Buffer;

        /// <summary>
        /// Buffer for the NMEA 2 data.
        /// </summary>
        private ConcurrentQueue<byte[]> _nmea2Buffer;

        /// <summary>
        /// Thread to decode incoming data.
        /// </summary>
        private Thread _processDataThread;

        /// <summary>
        /// Flag used to stop the thread.
        /// </summary>
        private bool _continue;

        /// <summary>
        /// Event to cause the thread
        /// to go to sleep or wakeup.
        /// </summary>
        private EventWaitHandle _eventWaitData;

        /// <summary>
        /// Queue to hold all the data to process.
        /// </summary>
        private ConcurrentQueue<ProcessData> _processDataQueue;

        /// <summary>
        /// View model for Data format options.
        /// </summary>
        private DataFormatViewModel _dataFormatOptions ;

        /// <summary>
        /// Current raw file size.
        /// </summary>
        private int _rawFileSize;

        /// <summary>
        ///  Current Validation file size.
        /// </summary>
        private int _validationFileSize;

        /// <summary>
        /// Max file to record.
        /// </summary>
        public const int MAX_FILE_SIZE = 16777216;      // 16mbs 

        #endregion

        #region Properties

        /// <summary>
        /// Status of the ADCP.
        /// </summary>
        public AdcpStatus Status { get; set; }

        /// <summary>
        /// Adcp Communication type.
        /// </summary>
        public AdcpCommTypes AdcpCommType { get; set; }

        /// <summary>
        /// Display mode of the data.
        /// This will state whether the data is in 
        /// live or playback mode.
        /// </summary>
        public DisplayDataMode DisplayMode { get; set; }

        /// <summary>
        /// Flag to know if we are recording the current
        /// data to the project.
        /// </summary>
        public bool IsRecording { get; set; }

        /// <summary>
        /// Flag to know if we are importing data.
        /// </summary>
        public bool IsImporting { get; set; }

        /// <summary>
        /// Serial port connection to the ADCP.
        /// </summary>
        public AdcpSerialPort AdcpSerialPort { get; set; }

        /// <summary>
        /// Ethernet port connection to the ADCP.
        /// </summary>
        private AdcpEthernet AdcpEthernetPort { get; set; }

        /// <summary>
        /// TCP connection to the ADCP.
        /// </summary>
        private AdcpTcpIp AdcpTcp { get; set; }

        /// <summary>
        /// UDP connection to the ADCP.
        /// </summary>
        private AdcpUdp AdcpUdp { get; set; }

        /// <summary>
        /// Serial port connection to the GPS 1.
        /// </summary>
        public GpsSerialPort Gps1SerialPort { get; set; }

        /// <summary>
        /// Serial port connection to the GPS 2.
        /// </summary>
        public GpsSerialPort Gps2SerialPort { get; set; }

        /// <summary>
        /// Serial port connection to the NMEA 1.
        /// </summary>
        public GpsSerialPort Nmea1SerialPort { get; set; }

        /// <summary>
        /// Serial port connection to the NMEA 2.
        /// </summary>
        public GpsSerialPort Nmea2SerialPort { get; set; }

        /// <summary>
        /// Flag to know if the GPS 1 serial port is enabled.
        /// </summary>
        public bool IsGps1SerialPortEnabled { get; set; }

        /// <summary>
        /// Flag to know if the GPS 2 serial port is enabled.
        /// </summary>
        public bool IsGps2SerialPortEnabled { get; set; }

        /// <summary>
        /// Flag to know if the NMEA 1 serial port is enabled.
        /// </summary>
        public bool IsNmea1SerialPortEnabled { get; set; }

        /// <summary>
        /// Flag to know if the NMEA 2 serial port is enabled.
        /// </summary>
        public bool IsNmea2SerialPortEnabled { get; set; }

        /// <summary>
        /// Serial number for the ADCP connected.
        /// </summary>
        public SerialNumber SerialNumber { get; set; }

        /// <summary>
        /// Firmware for the ADCP connected.
        /// </summary>
        public Firmware Firmware { get; set; }

        /// <summary>
        /// Hardware string for the ADCP connected.
        /// </summary>
        public string Hardware { get; set; }

        /// <summary>
        /// Terminal view model.
        /// </summary>
        public TerminalViewModel TerminalVM { get; set; }

        /// <summary>
        /// Receive Buffer string.  Depending on which
        /// communication type.
        /// </summary>
        public string ReceiveBufferString
        {
            get
            {
                if (AdcpCommType == AdcpCommTypes.Serial)
                {
                    return AdcpSerialPort.ReceiveBufferString;
                }
                else if (AdcpCommType == AdcpCommTypes.Ethernet)
                {
                    return AdcpEthernetPort.ReceiveBufferString;
                }
                else if (AdcpCommType == AdcpCommTypes.TCP)
                {
                    return AdcpTcp.ReceiveBufferString;
                }
                else if (AdcpCommType == AdcpCommTypes.UDP)
                {
                    return AdcpUdp.ReceiveBufferString;
                }

                return "";
            }
            set
            {
                if (AdcpCommType == AdcpCommTypes.Serial)
                {
                    AdcpSerialPort.ReceiveBufferString = value;
                }
                else if (AdcpCommType == AdcpCommTypes.Ethernet)
                {
                    AdcpEthernetPort.ReceiveBufferString = value;
                }
                else if (AdcpCommType == AdcpCommTypes.TCP)
                {
                    AdcpTcp.ReceiveBufferString = value;
                }
                else if (AdcpCommType == AdcpCommTypes.UDP)
                {
                    AdcpUdp.ReceiveBufferString = value;
                }
            }
        }

        #region Validation Test

        /// <summary>
        /// Flag to know if we are recording for
        /// a validation test.
        /// </summary>
        public bool IsValidationTestRecording { get; set; }

        /// <summary>
        /// Latest Validation Test File Name.  This is used to determine
        /// what file name is being used for the validation test, so i can
        /// be recorded with the results.
        /// </summary>
        public string ValidationTestFileName { get; set; }

        /// <summary>
        /// Binary writer for validation test data.
        /// </summary>
        private BinaryWriter _validationTestBinWriter;

        /// <summary>
        /// Directory to store the validation test files.
        /// </summary>
        private string _validationTestDir;

        /// <summary>
        /// Lock for the lake test file.
        /// </summary>
        private object _validationTestFileLock;

        /// <summary>
        /// Number of bytes written to the Validation Test file.
        /// </summary>
        public long ValidationTestBytesWritten { get; set; }


        #endregion

        #region Raw ADCP Record

        /// <summary>
        /// Flag to know if we are recording for
        /// a validation test.
        /// </summary>
        public bool IsRawAdcpRecording { get; set; }

        /// <summary>
        /// Latest Raw ADCP Test File Name.  This is used to determine
        /// what file name is being used for the raw ADCP, so i can
        /// be recorded with the results.
        /// </summary>
        public string RawAdcpRecordFileName { get; set; }

        /// <summary>
        /// Number of bytes written to the raw ADCP file.
        /// </summary>
        public long RawAdcpBytesWritten { get; set; }

        /// <summary>
        /// Binary writer for raw ADCP test data.
        /// </summary>
        private BinaryWriter _rawAdcpRecordBinWriter;

        /// <summary>
        /// Directory to store the raw ADCP test files.
        /// </summary>
        private string _rawAdcpRecordDir;

        /// <summary>
        /// ADCP Record file name for raw recording.
        /// </summary>
        private string _rawAdcpRecordFile;

        /// <summary>
        /// Lock for the raw ADCP file.
        /// </summary>
        private object _rawAdcpRecordFileLock;

        #endregion

        #region Gps1 Raw Record

        /// <summary>
        /// Flag to know if we are recording for
        /// a validation test.
        /// </summary>
        public bool IsRawGps1Recording { get; set; }

        /// <summary>
        /// Latest Raw Gps1 Test File Name.  This is used to determine
        /// what file name is being used for the raw Gps1, so i can
        /// be recorded with the results.
        /// </summary>
        public string RawGps1RecordFileName { get; set; }

        /// <summary>
        /// Number of bytes written to the raw Gps1 file.
        /// </summary>
        public long RawGps1BytesWritten { get; set; }

        /// <summary>
        /// Binary writer for raw Gps1 test data.
        /// </summary>
        private BinaryWriter _rawGps1RecordBinWriter;

        /// <summary>
        /// Directory to store the raw Gps1 files.
        /// </summary>
        private string _rawGps1RecordDir;

        /// <summary>
        /// Lock for the raw Gps1 file.
        /// </summary>
        private object _rawGps1RecordFileLock;

        #endregion

        #region Gps2 Raw Record

        /// <summary>
        /// Flag to know if we are recording for
        /// a validation test.
        /// </summary>
        public bool IsRawGps2Recording { get; set; }

        /// <summary>
        /// Latest Raw Gps2 Test File Name.  This is used to determine
        /// what file name is being used for the raw Gps2, so i can
        /// be recorded with the results.
        /// </summary>
        public string RawGps2RecordFileName { get; set; }

        /// <summary>
        /// Number of bytes written to the raw Gps2 file.
        /// </summary>
        public long RawGps2BytesWritten { get; set; }

        /// <summary>
        /// Binary writer for raw Gps2 test data.
        /// </summary>
        private BinaryWriter _rawGps2RecordBinWriter;

        /// <summary>
        /// Directory to store the raw Gps2 files.
        /// </summary>
        private string _rawGps2RecordDir;

        /// <summary>
        /// Lock for the raw Gps2 file.
        /// </summary>
        private object _rawGps2RecordFileLock;

        #endregion

        #region Nmea1 Raw Record

        /// <summary>
        /// Flag to know if we are recording for
        /// a validation test.
        /// </summary>
        public bool IsRawNmea1Recording { get; set; }

        /// <summary>
        /// Latest Raw Nmea1 Test File Name.  This is used to determine
        /// what file name is being used for the raw Nmea1, so i can
        /// be recorded with the results.
        /// </summary>
        public string RawNmea1RecordFileName { get; set; }

        /// <summary>
        /// Number of bytes written to the raw Nmea1 file.
        /// </summary>
        public long RawNmea1BytesWritten { get; set; }

        /// <summary>
        /// Binary writer for raw Nmea1 test data.
        /// </summary>
        private BinaryWriter _rawNmea1RecordBinWriter;

        /// <summary>
        /// Directory to store the raw Nmea1 files.
        /// </summary>
        private string _rawNmea1RecordDir;

        /// <summary>
        /// Lock for the raw Nmea1 file.
        /// </summary>
        private object _rawNmea1RecordFileLock;

        #endregion

        #region Nmea2 Raw Record

        /// <summary>
        /// Flag to know if we are recording for
        /// a validation test.
        /// </summary>
        public bool IsRawNmea2Recording { get; set; }

        /// <summary>
        /// Latest Raw Nmea2 Test File Name.  This is used to determine
        /// what file name is being used for the raw Nmea2, so i can
        /// be recorded with the results.
        /// </summary>
        public string RawNmea2RecordFileName { get; set; }

        /// <summary>
        /// Number of bytes written to the raw Nmea2 file.
        /// </summary>
        public long RawNmea2BytesWritten { get; set; }

        /// <summary>
        /// Binary writer for raw Nmea2 test data.
        /// </summary>
        private BinaryWriter _rawNmea2RecordBinWriter;

        /// <summary>
        /// Directory to store the raw Nmea2 files.
        /// </summary>
        private string _rawNmea2RecordDir;

        /// <summary>
        /// Lock for the raw Nmea2 file.
        /// </summary>
        private object _rawNmea2RecordFileLock;

        #endregion

        #endregion

        /// <summary>
        /// Find a connection to the ADCP.
        /// </summary>
        public AdcpConnection()
        {
            Debug.Print("AdcpConnection Open");

            // Check if folder exist
            CheckRecordFolderExist();

            // Get ProjectManager
            _pm = IoC.Get<PulseManager>();

            // Get ScreenData VM
            _screenDataVM = IoC.Get<ScreenDataBaseViewModel>();

            // Get Averaging VM
            _averagingVM = IoC.Get<AveragingBaseViewModel>();

            // Get the data format options
            _dataFormatOptions = IoC.Get<DataFormatViewModel>();

            // Initialize the thread
            _processDataQueue = new ConcurrentQueue<ProcessData>();
            _continue = true;
            _eventWaitData = new EventWaitHandle(false, EventResetMode.AutoReset);
            _processDataThread = new Thread(ReceiveDataThread);
            _processDataThread.Name = "AdcpConnection serial data";
            _processDataThread.Start();

            // Get EventAggregator
            _events = IoC.Get<IEventAggregator>();

            _rawFileSize = 0;
            _validationFileSize = 0;
            _ensembleBuffer = new ConcurrentQueue<EnsembleData>();
            _gps1Buffer = new ConcurrentQueue<byte[]>();
            _gps2Buffer = new ConcurrentQueue<byte[]>();
            _nmea1Buffer = new ConcurrentQueue<byte[]>();
            _nmea2Buffer = new ConcurrentQueue<byte[]>();

            // Initialize the status
            Status = new AdcpStatus(eAdcpStatus.NotConnected);
            //AdcpCommType = AdcpCommTypes.Serial;
            AdcpCommType = GetAdcpCommType();

            // Default to live mode
            DisplayMode = DisplayDataMode.Live;

            // Turn off recording
            IsRecording = false;
            IsImporting = false;

            // Not validation testing by default
            IsValidationTestRecording = false;
            _validationTestDir = "";
            ValidationTestFileName = "";
            _validationTestFileLock = new object();
            ValidationTestBytesWritten = 0;

            // Not raw recording ADCP by default
            IsRawAdcpRecording = false;
            _rawAdcpRecordDir = "";
            _rawAdcpRecordFile = null;
            RawAdcpRecordFileName = "";
            _rawAdcpRecordFileLock = new object();

            // Not raw recording GPS1 by default
            IsRawGps1Recording = false;
            _rawGps1RecordDir = "";
            RawGps1RecordFileName = "";
            _rawGps1RecordFileLock = new object();

            // Not raw recording GPS2 by default
            IsRawGps2Recording = false;
            _rawGps2RecordDir = "";
            RawGps2RecordFileName = "";
            _rawGps2RecordFileLock = new object();

            // Not raw recording Nmea1 by default
            IsRawNmea1Recording = false;
            _rawNmea1RecordDir = "";
            RawNmea1RecordFileName = "";
            _rawNmea1RecordFileLock = new object();

            // Not raw recording Nmea2 by default
            IsRawNmea2Recording = false;
            _rawNmea2RecordDir = "";
            RawNmea2RecordFileName = "";
            _rawNmea2RecordFileLock = new object();

            IsGps1SerialPortEnabled = _pm.GetIsGps1SerialEnabled();
            IsGps2SerialPortEnabled = _pm.GetIsGps2SerialEnabled();
            IsNmea1SerialPortEnabled = _pm.GetIsNmea1SerialEnabled();
            IsNmea2SerialPortEnabled = _pm.GetIsNmea2SerialEnabled();

            // Create codec to decode the data
            // Subscribe to process event
            _adcpCodec = new AdcpCodec();
            _adcpCodec.ProcessDataEvent += new AdcpCodec.ProcessDataEventHandler(_adcpCodec_ProcessDataEvent);

            // Initialize the serial port with the last good settings
            //AdcpSerialPort = new AdcpSerialPort(new SerialOptions() { Port = _pm.GetAdcpSerialCommPort(), BaudRate = _pm.GetAdcpSerialBaudRate() });
            ConnectAdcpSerial(new SerialOptions() { Port = _pm.GetAdcpSerialCommPort(), BaudRate = _pm.GetAdcpSerialBaudRate(), DataBits = _pm.GetAdcpSerialDataBit(), Parity = _pm.GetAdcpSerialParity(), StopBits = _pm.GetAdcpSerialStopBits() });

            // Initialize the GPS serial ports
            InitializeGpsConnections();

            // Create a default connection
            //AdcpEthernetPort = new AdcpEthernet(new AdcpEthernetOptions() { IpAddrA = _pm.GetEthernetIpAddressA(), IpAddrB = _pm.GetEthernetIpAddressB(), IpAddrC = _pm.GetEthernetIpAddressC(), IpAddrD = _pm.GetEthernetIpAddressD(), Port = _pm.GetEthernetPort() });

            // Create TCP connection
            //AdcpTcp = new AdcpTcpIp(new AdcpEthernetOptions() { IpAddrA = _pm.GetEthernetIpAddressA(), IpAddrB = _pm.GetEthernetIpAddressB(), IpAddrC = _pm.GetEthernetIpAddressC(), IpAddrD = _pm.GetEthernetIpAddressD(), Port = _pm.GetEthernetPort() });
            //dcpTcp.ReceiveRawTcpDataEvent += new AdcpTcpIp.ReceiveRawTcpDataEventHandler(ReceiveAdcpSerialData);

            // Terminal View Model
            TerminalVM = new TerminalViewModel(this);

            Debug.Print("AdcpConnection Completed");
        }



        /// <summary>
        /// Shutdown the object.
        /// This will shutdown the serial port.
        /// </summary>
        public void Dispose()
        {
            // Shutdown the ethernet port
            // DISPOSE BEFORE CODECS
            if (AdcpEthernetPort != null)
            {
                AdcpEthernetPort.Dispose();
            }

            // Shutdown the tcp port
            // DISPOSE BEFORE CODECS
            if (AdcpTcp != null)
            {
                AdcpTcp.ReceiveRawTcpDataEvent -= ReceiveAdcpSerialData;
                AdcpTcp.Dispose();
            }

            // Shutdown the terminal
            if (TerminalVM != null)
            {
                TerminalVM.Dispose();
            }

            // Disconnect serial
            // DISPOSE BEFORE CODECS
            DisconnectAdcpSerial();

            // Shutdown GPS 1 serial port
            DisconnectGps1Serial();

            // Shutdown GPS 2 serial port
            DisconnectGps2Serial();

            // Shutdown NMEA 1 serial port
            DisconnectNmea1Serial();

            // Shutdown NMEA 2 serial port
            DisconnectNmea2Serial();

            // Kill the processing thread
            _continue = false;
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
            _processDataThread.Abort();

            // Shutdown codec
            if (_adcpCodec != null)
            {
                _adcpCodec.ProcessDataEvent -= _adcpCodec_ProcessDataEvent;
                _adcpCodec.Dispose();
            }

            // Shutdown the averaging managers
            if (_averagingVM != null)
            {
                _averagingVM.Dispose();
            }

            // Stop recording Raw ADCP
            if (IsRawAdcpRecording)
            {
                StopRawAdcpRecord();
            }

            // Stop recording GPS 1
            if (IsRawGps1Recording)
            {
                StopRawGps1Record();
            }

            // Stop recording GPS 2
            if (IsRawGps2Recording)
            {
                StopRawGps2Record();
            }

            // Stop recording NMEA 1
            if (IsRawNmea1Recording)
            {
                StopRawNmea1Record();
            }

            // Stop recording NMEA 2
            if (IsRawNmea2Recording)
            {
                StopRawNmea2Record();
            }
        }

        #region Check Connection

        /// <summary>
        /// Check if the port is open.
        /// </summary>
        /// <returns>True = Port Open / False = Port is NOT open.</returns>
        public bool IsOpen()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.IsOpen();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.IsOpen();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.IsOpen();
            }
            else if(AdcpCommType ==  AdcpCommTypes.UDP)
            {
                return AdcpUdp.IsOpen();
            }

            return false;
        }

        #endregion

        #region ADCP COMM Type

        /// <summary>
        /// Update with the latest Adcp COMM type.
        /// </summary>
        /// <param name="type">ADCP comm type.</param>
        public void UpdateAdcpCommType(AdcpCommTypes type)
        {
            // Update the COMM type and then update the DB
            AdcpCommType = type;

            // Set the PM with the latest option
            _pm.UpdateAdcpCommType(type);
        }

        /// <summary>
        /// Get the last used ADCP COMM type.
        /// </summary>
        /// <returns>Last ADCP COMM type used.</returns>
        public AdcpCommTypes GetAdcpCommType()
        {
            // Get the last used options 
            AdcpCommType = _pm.GetAdcpCommType();

            return AdcpCommType;
        }

        #endregion

        #region Send / Receive Terminal Data

        /// <summary>
        /// Send a soft break to the ADCP.
        /// </summary>
        public void SendBreak()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                AdcpSerialPort.SendBreak();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                AdcpEthernetPort.SendBreak();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                AdcpTcp.SendBreak();
            }
            else if(AdcpCommType == AdcpCommTypes.UDP)
            {
                AdcpUdp.SendBreak();
            }
        }

        /// <summary>
        /// Send a soft break to the ADCP.
        /// <param name="waitStates">Number of wait states to wait.</param>
        /// <param name="stateChangeWaitStates">Number of wait states after done with BREAK.</param>
        /// <param name="softBreak">Flag if try soft BREAK if hardware BREAK fails.</param>
        /// </summary>
        public void SendAdvancedBreak(int waitStates = 5, int stateChangeWaitStates = 4, bool softBreak = true)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                AdcpSerialPort.SendAdvancedBreak(waitStates, stateChangeWaitStates, softBreak);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                AdcpEthernetPort.SendBreak();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                AdcpTcp.SendBreak();
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                AdcpUdp.SendBreak();
            }
        }

        /// <summary>
        /// Send a long forced break to the ADCP.
        /// </summary>
        public void SendForceBreak()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                AdcpSerialPort.SendForceBreak();
            }
        }


        /// <summary>
        /// Manually read from the ADCP.
        /// </summary>
        /// <param name="buffer">Buffer to read to.</param>
        /// <param name="offset">Location in buffer to read to.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Number of bytes read.</returns>
        public int ReadData(byte[] buffer, int offset, int count)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                // Pause Read thread
                AdcpSerialPort.PauseReadThread(true);

                // Read data
                int result = AdcpSerialPort.ReadData(buffer, offset, count);

                // Resume read thread
                AdcpSerialPort.PauseReadThread(false);

                return result;
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.ReadData(ref buffer);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                // Pause Read thread
                AdcpTcp.PauseReadThread(true);

                // Read data
                byte[] result = AdcpTcp.ReadData();

                // Resume read thread
                AdcpTcp.PauseReadThread(false);

                return result.Length;
            }

            return 0;
        }

        /// <summary>
        /// Send a command to the ADCP.
        /// This will add the carrage return to
        /// the end of the string.
        /// </summary>
        /// <param name="data">Data to write.</param>
        public void SendData(string data)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                AdcpSerialPort.SendData(data);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                // This buffer will get recreated in the response
                byte[] buffer = new byte[100];
                AdcpEthernetPort.SendData(data, ref buffer);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                AdcpTcp.SendData(data);
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                AdcpUdp.SendData(data);
            }
        }

        /// <summary>
        /// Send the given buffer of data to the ADCP.
        /// 
        /// This will write the data in the buffer at the given
        /// offset with the given length to the serial port.
        /// 
        /// This will not add the carrage return to the
        /// end of the data.
        /// </summary>
        /// <param name="buffer">Buffer of data to write.</param>
        /// <param name="offset">Location in the buffer to write.</param>
        /// <param name="count">Number of bytes to write.</param>
        public void SendData(byte[] buffer, int offset, int count)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                AdcpSerialPort.SendData(buffer, offset, count);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                AdcpEthernetPort.SendData(buffer, offset, count);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                AdcpTcp.SendData(buffer, offset, count);
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                AdcpUdp.SendData(buffer, offset, count);
            }
        }


        /// <summary>
        /// Send data through the serial port and wait for a response
        /// back from the serial port.  If the response was the same
        /// as the message sent, then return true.
        /// 
        /// </summary>
        /// <param name="buffer">Data to send through the serial port.</param>
        /// <param name="timeout">Timeout to wait in milliseconds.  Default = 1 sec.</param>
        /// <return>Flag if was successful sending the command.</return>
        public bool SendDataWaitReply(string buffer, int timeout = 1000)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.SendDataWaitReply(buffer, timeout);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.SendDataWaitReply(buffer, timeout);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.SendDataWaitReply(buffer, timeout);
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                return AdcpUdp.SendData(buffer);
            }

            return false;
        }

        /// <summary>
        /// Send data to the serial port.  Then wait for a response.  Get the
        /// response back from the serial port from the data sent.  The
        /// wait time is based on how long the data sent needs to be processed 
        /// on the other end before it sends a response back.  The waitTime is in
        /// milliseconds.  The default value for waitTime is RTI.AdcpSerialPort.WAIT_STATE.
        /// 
        /// sendBreak is used to send a BREAK before the command is sent.  This is useful to 
        /// wake the system up before sending the command or to get the BREAK result.
        /// </summary>
        /// <param name="data">Data to send to the serial port.</param>
        /// <param name="sendBreak">Flag to send a BREAK or not.</param>
        /// <param name="waitTime">Time to wait for a response in milliseconds.  DEFAULT=RTI.AdcpSerialPort.WAIT_STATE</param>
        /// <returns>Data sent back from the serial port after the data was sent.</returns>
        public string SendDataGetReply(string data, bool sendBreak = false, int waitTime = RTI.AdcpSerialPort.WAIT_STATE)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.SendDataGetReply(data, sendBreak, waitTime);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.SendDataGetReply(data, sendBreak, waitTime);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.SendDataGetReply(data, sendBreak, waitTime);
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                AdcpUdp.SendData(data);
                return "";
            }

            return "";
        }

        /// <summary>
        /// Send a list of commands.
        /// </summary>
        /// <param name="commands">List of commands.</param>
        /// <returns>TRUE = All commands were sent successfully.</returns>
        public bool SendCommands(List<string> commands)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.SendCommands(commands);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.SendCommands(commands);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.SendCommands(commands);
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                return AdcpUdp.SendCommands(commands);
            }

            return false;
        }

        #endregion

        #region Pinging

        /// <summary>
        /// Send the commands to the ADCP to start pinging.
        /// It then checks that the command was sent properly.  If
        /// the command was not accepted, FALSE will be returned.
        /// </summary>
        /// <returns>TRUE = Command sent. FALSE = Command not accepted.</returns>
        public bool StartPinging()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.StartPinging();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.StartPinging();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.StartPinging();
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                return AdcpUdp.SendData(RTI.Commands.AdcpCommands.CMD_START_PINGING);
            }

            return false;
        }

        /// <summary>
        /// Send the commands to the ADCP to start pinging.
        /// To start pinging, send the command START.  This
        /// will also set the date and time to the system
        /// based off the computers current date and time.
        /// If any command cannot be sent, set the flag and
        /// return that a command could not be set.
        /// </summary>
        /// <param name="useLocal">Use the local time to set the ADCP time.  FALSE will set the GMT time.</param>
        /// <returns>TRUE = All commands sent. FALSE = 1 or more commands could not be sent.</returns>
        public bool StartPinging(bool useLocal = true)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.StartPinging(useLocal);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.StartPinging(useLocal);
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.StartPinging(useLocal);
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                return AdcpUdp.SendData(RTI.Commands.AdcpCommands.CMD_START_PINGING);
            }
            

            return false;
        }

        /// <summary>
        /// Send the commands to the ADCP to stop pinging.
        /// It then checks that the command was sent properly.  If
        /// the command was not accepted, FALSE will be returned.
        /// </summary>
        /// <returns>TRUE = Command sent. FALSE = Command not accepted.</returns>
        public bool StopPinging()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.StopPinging();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.StopPinging();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.StopPinging();
            }
            else if (AdcpCommType == AdcpCommTypes.UDP)
            {
                return AdcpUdp.SendData(RTI.Commands.AdcpCommands.CMD_STOP_PINGING);
            }

            return false;
        }

        #endregion

        #region Serial For ADCP

        /// <summary>
        /// Find a connection to the ADCP.
        /// This will test all the ways that a connection can be
        /// made until a connection is found.
        /// This will check all serial ports and 
        /// all baud rates.  It will then check ethernet.
        /// First ADCP it finds, it will make the connection
        /// </summary>
        /// <returns>A list of all the serial port connections.</returns>
        public List<AdcpSerialPort.AdcpSerialOptions> ScanSerialConnection()
        {
            return AdcpSerialPort.ScanSerialConnection();
        }

        /// <summary>
        /// Create a connection to the ADCP serial port with
        /// the given options.  If no options are given, return null.
        /// </summary>
        /// <param name="options">Options to connect to the serial port.</param>
        /// <returns>Adcp Serial Port based off the options</returns>
        public AdcpSerialPort ConnectAdcpSerial(SerialOptions options)
        {
            // If there is a connection, disconnect
            if (AdcpSerialPort != null)
            {
                DisconnectAdcpSerial();
            }

            if (options != null)
            {
                // Set the connection
                Status.Status = eAdcpStatus.Connected;

                // Create the connection and connect
                AdcpSerialPort = new AdcpSerialPort(options);
                AdcpSerialPort.Connect();
                //AdcpSerialPort.SendBreak();

                // Clear the codec of any data
                _adcpCodec.ClearIncomingData();

                // Subscribe to receive ADCP data
                AdcpSerialPort.ReceiveAdcpSerialDataEvent += new AdcpSerialPort.ReceiveAdcpSerialDataEventHandler(ReceiveAdcpSerialData);
                AdcpSerialPort.DownloadProgressEvent += new AdcpSerialPort.DownloadProgressEventHandler(On_DownloadProgressEvent);
                AdcpSerialPort.DownloadCompleteEvent += new AdcpSerialPort.DownloadCompleteEventHandler(On_DownloadCompleteEvent);
                AdcpSerialPort.DownloadFileSizeEvent += new AdcpSerialPort.DownloadFileSizeEventHandler(On_DownloadFileSizeEvent);
                AdcpSerialPort.UploadProgressEvent += new AdcpSerialPort.UploadProgressEventHandler(On_UploadProgressEvent);
                AdcpSerialPort.UploadCompleteEvent += new AdcpSerialPort.UploadCompleteEventHandler(On_UploadCompleteEvent);
                AdcpSerialPort.UploadFileSizeEvent += new AdcpSerialPort.UploadFileSizeEventHandler(On_UploadFileSizeEvent);

                // Publish that the ADCP serial port is new
                PublishAdcpSerialConnection();

                Debug.WriteLine(string.Format("ADCP Connect: {0}", AdcpSerialPort.ToString()));

                return AdcpSerialPort;
            }

            return null;
        }

        /// <summary>
        /// Shutdown the ADCP serial port.
        /// This will stop all the read threads
        /// for the ADCP serial port.
        /// </summary>
        public void DisconnectAdcpSerial()
        {
            try
            {
                if (AdcpSerialPort != null)
                {
                    Debug.WriteLine(string.Format("ADCP Disconnect: {0}", AdcpSerialPort.ToString()));

                    // Disconnect the serial port
                    AdcpSerialPort.Disconnect();

                    // Unscribe to ADCP SerialPort events
                    AdcpSerialPort.ReceiveAdcpSerialDataEvent -= ReceiveAdcpSerialData;
                    AdcpSerialPort.DownloadProgressEvent -= On_DownloadProgressEvent;
                    AdcpSerialPort.DownloadCompleteEvent -= On_DownloadCompleteEvent;
                    AdcpSerialPort.DownloadFileSizeEvent -= On_DownloadFileSizeEvent;
                    AdcpSerialPort.UploadProgressEvent -= On_UploadProgressEvent;
                    AdcpSerialPort.UploadCompleteEvent -= On_UploadCompleteEvent;
                    AdcpSerialPort.UploadFileSizeEvent -= On_UploadFileSizeEvent;

                    // Publish that the ADCP serial conneciton is disconnected
                    PublishAdcpSerialDisconnection();

                    // Shutdown the serial port
                    AdcpSerialPort.Dispose();
                }
                Status.Status = eAdcpStatus.NotConnected;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Disconnect then connect with the new options given.
        /// </summary>
        /// <param name="options">Options to connect the ADCP serial port.</param>
        public void ReconnectAdcpSerial(SerialOptions options)
        {
            // Disconnect
            DisconnectAdcpSerial();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Connect
            ConnectAdcpSerial(options);
        }

        /// <summary>
        /// Return if the Adcp Serial port is open and connected.
        /// </summary>
        /// <returns>TRUE = Is connected.</returns>
        public bool IsAdcpSerialConnected()
        {
            // See if the connection is open
            if (AdcpSerialPort != null && AdcpSerialPort.IsOpen())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update the Pulse database with the latest Adcp Serial port port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateAdcpSerialCommPort(string port)
        {
            // Update the port and then update the DB
            AdcpSerialPort.SerialOptions.Port = port;
        }

        /// <summary>
        /// Get the last used ADCP serial port comm port.
        /// </summary>
        /// <returns>Last ADCP Comm port used.</returns>
        public string GetAdcpSerialCommPort()
        {
            return AdcpSerialPort.SerialOptions.Port;
        }

        /// <summary>
        /// Update the connection with the latest Adcp Serial port baud rate.
        /// </summary>
        /// <param name="baud">Baudrate to update.</param>
        public void UpdateAdcpSerialBaudRate(int baud)
        {
            // Update the port and then update the DB
            AdcpSerialPort.SerialOptions.BaudRate = baud;
        }

        /// <summary>
        /// Get the last used ADCP serial port baud rate.
        /// </summary>
        /// <returns>Last used ADCP baud rate.</returns>
        public int GetAdcpSerialBaudRate()
        {
            return AdcpSerialPort.SerialOptions.BaudRate;
        }

        /// <summary>
        /// Update the connection with the latest Adcp Serial port Data Bit.
        /// </summary>
        /// <param name="bit">Data Bit to update.</param>
        public void UpdateAdcpSerialDataBit(int bit)
        {
            // Update the port and then update the DB
            AdcpSerialPort.SerialOptions.DataBits = bit;
        }

        /// <summary>
        /// Get the last used ADCP serial port Data Bit.
        /// </summary>
        /// <returns>Last used ADCP data bit.</returns>
        public int GetAdcpSerialDataBit()
        {
            return AdcpSerialPort.SerialOptions.DataBits;
        }

        /// <summary>
        /// Update the connection with the latest Adcp Serial port parity.
        /// </summary>
        /// <param name="parity">Parity to update.</param>
        public void UpdateAdcpSerialParity(System.IO.Ports.Parity parity)
        {
            // Update the port and then update the DB
            AdcpSerialPort.SerialOptions.Parity = parity;
        }

        /// <summary>
        /// Get the last used ADCP serial port Parity.
        /// </summary>
        /// <returns>Last used ADCP Parity.</returns>
        public System.IO.Ports.Parity GetAdcpSerialParity()
        {
            return AdcpSerialPort.SerialOptions.Parity;
        }

        /// <summary>
        /// Update the connection with the latest Adcp Serial port stop bit.
        /// </summary>
        /// <param name="baud">Stop Bit to update.</param>
        public void UpdateAdcpSerialStopBits(System.IO.Ports.StopBits bit)
        {
            // Update the port and then update the DB
            AdcpSerialPort.SerialOptions.StopBits = bit;
        }

        /// <summary>
        /// Get the last used ADCP serial port stop bit.
        /// </summary>
        /// <returns>Last used ADCP stop bit.</returns>
        public System.IO.Ports.StopBits GetAdcpSerialStopBits()
        {
            return AdcpSerialPort.SerialOptions.StopBits;
        }

        #endregion

        #region GPS/NMEA Initialize

        /// <summary>
        /// Initialize the GPS connections based
        /// off the previous settings.
        /// </summary>
        private void InitializeGpsConnections()
        {
            // GPS 1
            if (_pm.GetIsGps1SerialEnabled())
            {
                ConnectGps1Serial(new SerialOptions { Port = _pm.GetGps1SerialCommPort(), BaudRate = _pm.GetGps1SerialBaudRate() });
            }

            // GPS 2
            if (_pm.GetIsGps2SerialEnabled())
            {
                ConnectGps2Serial(new SerialOptions { Port = _pm.GetGps2SerialCommPort(), BaudRate = _pm.GetGps2SerialBaudRate() });
            }

            // NMEA 1
            if (_pm.GetIsNmea1SerialEnabled())
            {
                ConnectNmea1Serial(new SerialOptions { Port = _pm.GetNmea1SerialCommPort(), BaudRate = _pm.GetNmea1SerialBaudRate() });
            }

            // NMEA 2
            if (_pm.GetIsNmea2SerialEnabled())
            {
                ConnectNmea2Serial(new SerialOptions { Port = _pm.GetNmea2SerialCommPort(), BaudRate = _pm.GetNmea2SerialBaudRate() });
            }
        }

        #endregion

        #region Serial GPS 1

        /// <summary>
        /// Create a connection to the GPS 1 serial port with
        /// the given options.  If no options are given, return null.
        /// </summary>
        /// <param name="options">Options to connect to the serial port.</param>
        /// <returns>GPS Serial Port based off the options</returns>
        public GpsSerialPort ConnectGps1Serial(SerialOptions options)
        {
            // If is connected, disconnect
            if (Gps1SerialPort != null)
            {
                DisconnectGps1Serial();
            }

            if (options != null)
            {
                // Set flag if not set already
                IsGps1SerialPortEnabled = true;

                // Set the connection
                //Status.Status = eAdcpStatus.Connected;

                // Create the connection and connect
                Gps1SerialPort = new GpsSerialPort(options, true);
                Gps1SerialPort.Connect();

                // Clear the codec of any data
                _adcpCodec.ClearIncomingData();

                // Subscribe to receive GPS data
                Gps1SerialPort.ReceiveRawSerialDataEvent += new SerialConnection.ReceiveRawSerialDataEventHandler(Gps1SerialPort_ReceiveRawSerialDataEvent);

                // Publish that a GPS serial connection is made
                PublishGps1SerialConnection();

                Debug.WriteLine(string.Format("GPS 1 Connect: {0}", Gps1SerialPort.ToString()));

                return Gps1SerialPort;
            }

            return null;
        }

        /// <summary>
        /// Shutdown the GPS serial port.
        /// This will stop all the read threads
        /// for the GPS serial port.
        /// </summary>
        public void DisconnectGps1Serial()
        {
            if (Gps1SerialPort != null)
            {
                Debug.WriteLine(string.Format("GPS 1 Disconnect: {0}", Gps1SerialPort.ToString()));

                // Set flag 
                IsGps1SerialPortEnabled = false;

                // Unscribe to GPS SerialPort events
                Gps1SerialPort.ReceiveRawSerialDataEvent -= Gps1SerialPort_ReceiveRawSerialDataEvent;

                // Publish the GPS serial connection is disconnected
                PublishGps1SerialDisconnection();

                // Shutdown the serial port
                Gps1SerialPort.Dispose();
                Gps1SerialPort = null;
            }
            //Status.Status = eAdcpStatus.NotConnected;
        }

        /// <summary>
        /// Disconnect then connect with the new options given.
        /// </summary>
        /// <param name="options">Options to connect the GPS 1 serial port.</param>
        public void ReconnectGps1Serial(SerialOptions options)
        {
            // Disconnect
            DisconnectGps1Serial();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Connect
            ConnectGps1Serial(options);
        }

        #endregion

        #region Serial GPS 2

        /// <summary>
        /// Create a connection to the GPS 2 serial port with
        /// the given options.  If no options are given, return null.
        /// </summary>
        /// <param name="options">Options to connect to the serial port.</param>
        /// <returns>GPS Serial Port based off the options</returns>
        public GpsSerialPort ConnectGps2Serial(SerialOptions options)
        {
            // If is connected, disconnect
            if (Gps2SerialPort != null)
            {
                DisconnectGps2Serial();
            }

            if (options != null)
            {
                // Set flag if not set already
                IsGps2SerialPortEnabled = true;

                // Set the connection
                //Status.Status = eAdcpStatus.Connected;

                // Create the connection and connect
                Gps2SerialPort = new GpsSerialPort(options, true);
                Gps2SerialPort.Connect();

                // Clear the codec of any data
                _adcpCodec.ClearIncomingData();

                // Subscribe to receive GPS data
                //Gps2SerialPort.ReceiveGpsSerialDataEvent += new RTI.GpsSerialPort.ReceiveGpsSerialDataEventHandler(ReceiveGpsBinaryData);
                Gps2SerialPort.ReceiveRawSerialDataEvent += new SerialConnection.ReceiveRawSerialDataEventHandler(Gps2SerialPort_ReceiveRawSerialDataEvent);

                // Publish that a GPS serial connection is made
                PublishGps2SerialConnection();

                Debug.WriteLine(string.Format("GPS 2 Connect: {0}", Gps2SerialPort.ToString()));

                return Gps2SerialPort;
            }

            return null;
        }

        /// <summary>
        /// Shutdown the GPS 2 serial port.
        /// This will stop all the read threads
        /// for the GPS 2 serial port.
        /// </summary>
        public void DisconnectGps2Serial()
        {
            if (Gps2SerialPort != null)
            {
                Debug.WriteLine(string.Format("GPS 2 Disconnect: {0}", Gps2SerialPort.ToString()));

                // Set flag 
                IsGps2SerialPortEnabled = false;

                // Unscribe to GPS SerialPort events
                Gps2SerialPort.ReceiveRawSerialDataEvent -= Gps2SerialPort_ReceiveRawSerialDataEvent;

                // Publish the GPS serial connection is disconnected
                PublishGps2SerialDisconnection();

                // Shutdown the serial port
                Gps2SerialPort.Dispose();
                Gps2SerialPort = null;
            }
            //Status.Status = eAdcpStatus.NotConnected;
        }

        /// <summary>
        /// Disconnect then connect with the new options given.
        /// </summary>
        /// <param name="options">Options to connect the GPS 2 serial port.</param>
        public void ReconnectGps2Serial(SerialOptions options)
        {
            // Disconnect
            DisconnectGps2Serial();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Connect
            ConnectGps2Serial(options);
        }

        #endregion

        #region Serial NMEA 1

        /// <summary>
        /// Create a connection to the NMEA 1 serial port with
        /// the given options.  If no options are given, return null.
        /// </summary>
        /// <param name="options">Options to connect to the serial port.</param>
        /// <returns>GPS Serial Port based off the options</returns>
        public GpsSerialPort ConnectNmea1Serial(SerialOptions options)
        {
            // If is connected, disconnect
            if (Nmea1SerialPort != null)
            {
                DisconnectNmea1Serial();
            }

            if (options != null)
            {
                // Set flag if not set already
                IsNmea1SerialPortEnabled = true;

                // Set the connection
                //Status.Status = eAdcpStatus.Connected;

                // Create the connection and connect
                Nmea1SerialPort = new GpsSerialPort(options, true);
                Nmea1SerialPort.Connect();

                // Clear the codec of any data
                _adcpCodec.ClearIncomingData();

                // Subscribe to receive GPS data
                //Nmea1SerialPort.ReceiveGpsSerialDataEvent += new RTI.GpsSerialPort.ReceiveGpsSerialDataEventHandler(ReceiveGpsBinaryData);
                Nmea1SerialPort.ReceiveRawSerialDataEvent += new SerialConnection.ReceiveRawSerialDataEventHandler(Nmea1SerialPort_ReceiveRawSerialDataEvent);

                // Publish that a GPS serial connection is made
                PublishNmea1SerialConnection();

                Debug.WriteLine(string.Format("Nmea 1 Connect: {0}", Nmea1SerialPort.ToString()));

                return Nmea1SerialPort;
            }

            return null;
        }

        /// <summary>
        /// Shutdown the NMEA 1 serial port.
        /// This will stop all the read threads
        /// for the NMEA 1 serial port.
        /// </summary>
        public void DisconnectNmea1Serial()
        {
            if (Nmea1SerialPort != null)
            {
                Debug.WriteLine(string.Format("Nmea 1 Disconnect: {0}", Nmea1SerialPort.ToString()));

                // Set flag 
                IsNmea1SerialPortEnabled = false;

                // Unscribe to GPS SerialPort events
                Nmea1SerialPort.ReceiveRawSerialDataEvent -= Nmea1SerialPort_ReceiveRawSerialDataEvent;

                // Publish the GPS serial connection is disconnected
                PublishNmea1SerialDisconnection();

                // Shutdown the serial port
                Nmea1SerialPort.Dispose();
                Nmea1SerialPort = null;
            }
            //Status.Status = eAdcpStatus.NotConnected;
        }

        /// <summary>
        /// Disconnect then connect with the new options given.
        /// </summary>
        /// <param name="options">Options to connect the NMEA 1 serial port.</param>
        public void ReconnectNmea1Serial(SerialOptions options)
        {
            // Disconnect
            DisconnectNmea1Serial();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Connect
            ConnectNmea1Serial(options);
        }

        #endregion

        #region Serial NMEA 2

        /// <summary>
        /// Create a connection to the NMEA 2 serial port with
        /// the given options.  If no options are given, return null.
        /// </summary>
        /// <param name="options">Options to connect to the serial port.</param>
        /// <returns>GPS Serial Port based off the options</returns>
        public GpsSerialPort ConnectNmea2Serial(SerialOptions options)
        {
            // If is connected, disconnect
            if (Nmea2SerialPort != null)
            {
                DisconnectNmea2Serial();
            }

            if (options != null)
            {
                // Set flag if not set already
                IsNmea2SerialPortEnabled = true;

                // Set the connection
                //Status.Status = eAdcpStatus.Connected;

                // Create the connection and connect
                Nmea2SerialPort = new GpsSerialPort(options, true);
                Nmea2SerialPort.Connect();

                // Clear the codec of any data
                _adcpCodec.ClearIncomingData();

                // Subscribe to receive GPS data
                //Nmea2SerialPort.ReceiveGpsSerialDataEvent += new RTI.GpsSerialPort.ReceiveGpsSerialDataEventHandler(ReceiveGpsBinaryData);
                Nmea2SerialPort.ReceiveRawSerialDataEvent += new SerialConnection.ReceiveRawSerialDataEventHandler(Nmea2SerialPort_ReceiveRawSerialDataEvent);

                // Publish that a GPS serial connection is made
                PublishNmea2SerialConnection();

                Debug.WriteLine(string.Format("Nmea 2 Connect: {0}", Nmea2SerialPort.ToString()));

                return Nmea2SerialPort;
            }

            return null;
        }

        /// <summary>
        /// Shutdown the NMEA 2 serial port.
        /// This will stop all the read threads
        /// for the NMEA 2 serial port.
        /// </summary>
        public void DisconnectNmea2Serial()
        {
            if (Nmea2SerialPort != null)
            {
                Debug.WriteLine(string.Format("Nmea 2 Disconnect: {0}", Nmea2SerialPort.ToString()));

                // Set flag 
                IsNmea2SerialPortEnabled = false;

                // Unscribe to GPS SerialPort events
                Nmea2SerialPort.ReceiveRawSerialDataEvent -= Nmea2SerialPort_ReceiveRawSerialDataEvent;

                // Publish the GPS serial connection is disconnected
                PublishNmea2SerialDisconnection();

                // Shutdown the serial port
                Nmea2SerialPort.Dispose();
                Nmea2SerialPort = null;
            }
            //Status.Status = eAdcpStatus.NotConnected;
        }

        /// <summary>
        /// Disconnect then connect with the new options given.
        /// </summary>
        /// <param name="options">Options to connect the NMEA 2 serial port.</param>
        public void ReconnectNmea2Serial(SerialOptions options)
        {
            // Disconnect
            DisconnectNmea2Serial();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            // Connect
            ConnectNmea2Serial(options);
        }

        #endregion

        #region NMEA Buffers

        /// <summary>
        /// Create a string of all the data in the GPS 1 buffer.
        /// </summary>
        /// <returns>String of all the buffered data.</returns>
        private string Gps1BufferData()
        {
            // Copy the buffer to a list
            byte[][] buffer = _gps1Buffer.ToArray();

            // Clear the buffer
            ClearGps1Buffer();

            // Create a string of all the data in the buffer
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < buffer.Length; x++)
            {
                sb.Append(System.Text.ASCIIEncoding.ASCII.GetString(buffer[x]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a string of all the data in the GPS 2 buffer.
        /// </summary>
        /// <returns>String of all the buffered data.</returns>
        private string Gps2BufferData()
        {
            // Copy the buffer to a list
            byte[][] buffer = _gps2Buffer.ToArray();

            // Clear the buffer
            ClearGps2Buffer();

            // Create a string of all the data in the buffer
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < buffer.Length; x++)
            {
                sb.Append(System.Text.ASCIIEncoding.ASCII.GetString(buffer[x]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a string of all the data in the NMEA 1 buffer.
        /// </summary>
        /// <returns>String of all the buffered data.</returns>
        private string Nmea1BufferData()
        {
            // Copy the buffer to a list
            byte[][] buffer = _nmea1Buffer.ToArray();

            // Clear the buffer
            ClearNmea1Buffer();

            // Create a string of all the data in the buffer
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < buffer.Length; x++)
            {
                sb.Append(System.Text.ASCIIEncoding.ASCII.GetString(buffer[x]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a string of all the data in the NMEA 2 buffer.
        /// </summary>
        /// <returns>String of all the buffered data.</returns>
        private string Nmea2BufferData()
        {
            // Copy the buffer to a list
            byte[][] buffer = _nmea2Buffer.ToArray();

            // Clear the buffer
            ClearNmea2Buffer();

            // Create a string of all the data in the buffer
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < buffer.Length; x++)
            {
                sb.Append(System.Text.ASCIIEncoding.ASCII.GetString(buffer[x]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clear the GPS 1 buffer.
        /// </summary>
        private void ClearGps1Buffer()
        {
            byte[] item;
            while (_gps1Buffer.TryDequeue(out item))
            {
                // do nothing
            }
        }

        /// <summary>
        /// Clear the GPS 2 buffer.
        /// </summary>
        private void ClearGps2Buffer()
        {
            byte[] item;
            while (_gps2Buffer.TryDequeue(out item))
            {
                // do nothing
            }
        }

        /// <summary>
        /// Clear the NMEA 1 buffer.
        /// </summary>
        private void ClearNmea1Buffer()
        {
            byte[] item;
            while (_nmea1Buffer.TryDequeue(out item))
            {
                // do nothing
            }
        }

        /// <summary>
        /// Clear the NMEA 2 buffer.
        /// </summary>
        private void ClearNmea2Buffer()
        {
            byte[] item;
            while (_nmea2Buffer.TryDequeue(out item))
            {
                // do nothing
            }
        }

        #endregion

        #region Maintence Log

        /// <summary>
        /// Get the maintence log from the ADCP.
        /// 
        /// This will connect to the ADCP and download the
        /// maintence log.  It will store the downloaded log
        /// to the directory given.  It will then load the maintence
        /// log to this object and return it.
        /// 
        /// If a directory is given, it will store the file to the
        /// directory.  If you do not want to store the file, leave the
        /// dir as null.
        /// </summary>
        /// <param name="dir">Directory to store the maintence log.  Usually the project folder.  If null, it will be stored to temp path.</param>
        /// <returns>List of all the maintence entries.</returns>
        public List<MaintenceEntry> GetMaintenceLog(string dir = null)
        {
            MaintenceLog mainLog = new MaintenceLog();
            var maintenceLog = mainLog.GetMaintenceLog(AdcpSerialPort, dir);

            return maintenceLog;
        }

        /// <summary>
        /// Write a maintence entry to the list and
        /// upload it to the ADCP.  This will add the entry
        /// to the list store here.  It will then write this
        /// list to the a temp file and upload the temp 
        /// file to the ADCP.
        /// </summary>
        /// <param name="entry">Entry to add to the ADCP.</param>
        public void AddMaintenceEntry(MaintenceEntry entry)
        {
            // Write the entry to the log
            // This will create a temp file then upload
            // the file to the ADCP
            MaintenceLog mainLog = new MaintenceLog();
            mainLog.AddEntry(AdcpSerialPort, entry);
        }

        /// <summary>
        /// Clear the maintence log.
        /// </summary>
        public void ClearMaintenceLog()
        {
            MaintenceLog maintLog = new MaintenceLog();
            maintLog.ClearLog(AdcpSerialPort);
        }

        #endregion

        #region ADCP Configuration

        /// <summary>
        /// Set the ADCP configuration to the give project.
        /// This will store the temporarly the deployment options.
        /// It will then overwrite the current configuration wit the
        /// configuration from the ADCP.  It will then set the previous
        /// deployment options.  It will then set the serial number.
        /// 
        /// It will get the Hardware configuration.
        /// 
        /// It will get the directory listing and the used spaced on the internal storage.
        /// 
        /// It will then save the project.
        /// </summary>
        /// <param name="prj">Project to get the configuration from the ADCP.</param>
        public Project SetAdcpConfiguration(Project prj)
        {
            if (_pm.IsProjectSelected && AdcpSerialPort.IsOpen())
            {
                DeploymentOptions deploymentOptions = prj.Configuration.DeploymentOptions;              // Store temporarly. They will be overwritten
                prj.Configuration = AdcpSerialPort.GetAdcpConfiguration();                              // Get the Configuration from the ADCP
                prj.Configuration.DeploymentOptions = deploymentOptions;                                // Replace deployment options
                prj.SerialNumber = prj.Configuration.SerialNumber;                                      // Set the serial number to the project

                prj.Configuration.HardwareOptions = AdcpSerialPort.GetHardwareConfiguration();          // Set the hardware configuration

                RTI.Commands.AdcpDirListing listing = AdcpSerialPort.GetDirectoryListing();             // Get the Directory Listing
                prj.Configuration.DeploymentOptions.InternalMemoryCardUsed = (long)Math.Round(listing.UsedSpace * MathHelper.MB_TO_BYTES);
                prj.Configuration.DeploymentOptions.InternalMemoryCardTotalSize = (long)Math.Round(listing.TotalSpace * MathHelper.MB_TO_BYTES);

                prj.Save();                                                                             // Save the new configuration

                // Set the Serial number for the project list
                _pm.UpdateProjectModifySerialNumber(prj);
            }

            return prj;
        }

        /// <summary>
        /// Get the ADCP configuration.  This will set the
        /// subsystems, hardware options, and directory information.
        /// </summary>
        /// <returns>Adcp Configuration from the ADCP.</returns>
        public AdcpConfiguration GetAdcpConfiguration()
        {
            AdcpConfiguration adcpConfig = new AdcpConfiguration();

            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                if (AdcpSerialPort.IsOpen())
                {
                    // Get the Configuration from the ADCP
                    adcpConfig = AdcpSerialPort.GetAdcpConfiguration();
                }
            }

            return adcpConfig;
        }

        #endregion

        #region ADCP Ethernet

        /// <summary>
        /// Connect to the ADCP ethernet port.
        /// </summary>
        public void ConnectAdcpEthernet()
        {
            if (AdcpEthernetPort == null)
            {
                // Create a default connection
                AdcpEthernetPort = new AdcpEthernet(new AdcpEthernetOptions() { IpAddrA = _pm.GetEthernetIpAddressA(), IpAddrB = _pm.GetEthernetIpAddressB(), IpAddrC = _pm.GetEthernetIpAddressC(), IpAddrD = _pm.GetEthernetIpAddressD(), Port = _pm.GetEthernetPort() });
            }

            if (AdcpEthernetPort != null)
            {
                // Subscribe to receive ADCP data
                AdcpEthernetPort.ReceiveRawEthernetDataEvent += new AdcpEthernet.ReceiveRawEthernetDataEventHandler(ReceiveAdcpSerialData);
                AdcpEthernetPort.DownloadProgressEvent += new AdcpEthernet.DownloadProgressEventHandler(On_DownloadProgressEvent);
                AdcpEthernetPort.DownloadCompleteEvent += new AdcpEthernet.DownloadCompleteEventHandler(On_DownloadCompleteEvent);
                AdcpEthernetPort.DownloadFileSizeEvent += new AdcpEthernet.DownloadFileSizeEventHandler(On_DownloadFileSizeEvent);
            }
        }

        /// <summary>
        /// Disconnect to the ADCP ethernet port.
        /// </summary>
        public void DisconnectAdcpEthernet()
        {
            if (AdcpEthernetPort != null)
            {
                // Subscribe to receive ADCP data
                AdcpEthernetPort.ReceiveRawEthernetDataEvent -= ReceiveAdcpSerialData;
                AdcpEthernetPort.DownloadProgressEvent -= On_DownloadProgressEvent;
                AdcpEthernetPort.DownloadCompleteEvent -= On_DownloadCompleteEvent;
                AdcpEthernetPort.DownloadFileSizeEvent -= On_DownloadFileSizeEvent;
            }
        }


        /// <summary>
        /// Test for any ADCPs connected
        /// on the ethernet connection.
        /// 
        /// This will output a response from the ping command to the 
        /// terminal window.  If nothing is seen, then the 
        /// </summary>
        /// <returns></returns>
        public bool TestEthernetConnection()
        {
            AdcpEthernetPort.ReceiveBufferString = string.Empty;
            byte[] replyBuffer = new byte[100];
            AdcpEthernetPort.SendData("", ref replyBuffer, true, 2000, true);

            if (String.IsNullOrEmpty(AdcpEthernetPort.ReceiveBufferString))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update with the latest Adcp ethernet address A.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressA(uint addr)
        {
            if (AdcpEthernetPort != null)
            {
                // Update the address and then update the DB
                AdcpEthernetPort.Options.IpAddrA = addr;
            }

            if (AdcpTcp != null)
            {
                AdcpTcp.Options.IpAddrA = addr;
            }

            // Set the PM with the latest option
            _pm.UpdateEthernetIpAddressA(addr);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address A.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address A used.</returns>
        public uint GetEthernetIpAddressA()
        {
            if (AdcpEthernetPort != null)
            {
                return AdcpEthernetPort.Options.IpAddrA;
            }
            else
            {
                return _pm.GetEthernetIpAddressA();
            }
        }

        /// <summary>
        /// Update with the latest Adcp ethernet address B.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressB(uint addr)
        {
            if (AdcpEthernetPort != null)
            {
                // Update the address and then update the DB
                AdcpEthernetPort.Options.IpAddrB = addr;
            }

            if (AdcpTcp != null)
            {
                AdcpTcp.Options.IpAddrB = addr;
            }

            // Set the PM with the latest option
            _pm.UpdateEthernetIpAddressB(addr);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address B.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address B used.</returns>
        public uint GetEthernetIpAddressB()
        {
            if (AdcpEthernetPort != null)
            {
                return AdcpEthernetPort.Options.IpAddrB;
            }
            else
            {
                return _pm.GetEthernetIpAddressB();
            }
        }

        /// <summary>
        /// Update with the latest Adcp ethernet address C.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressC(uint addr)
        {
            if (AdcpEthernetPort != null)
            {
                // Update the address and then update the DB
                AdcpEthernetPort.Options.IpAddrC = addr;
            }

            if (AdcpTcp != null)
            {
                AdcpTcp.Options.IpAddrC = addr;
            }

            // Set the PM with the latest option
            _pm.UpdateEthernetIpAddressC(addr);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address C.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address C used.</returns>
        public uint GetEthernetIpAddressC()
        {
            if (AdcpEthernetPort != null)
            {
                return AdcpEthernetPort.Options.IpAddrC;
            }
            else
            {
                return _pm.GetEthernetIpAddressC();
            }
        }

        /// <summary>
        /// Update with the latest Adcp ethernet address D.
        /// </summary>
        /// <param name="addr">Address to update.</param>
        public void UpdateEthernetIpAddressD(uint addr)
        {
            if (AdcpEthernetPort != null)
            {
                // Update the address and then update the DB
                AdcpEthernetPort.Options.IpAddrD = addr;
            }

            if (AdcpTcp != null)
            {
                AdcpTcp.Options.IpAddrD = addr;
            }

            // Set the PM with the latest option
            _pm.UpdateEthernetIpAddressD(addr);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address D.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address D used.</returns>
        public uint GetEthernetIpAddressD()
        {
            if (AdcpEthernetPort != null)
            {
                return AdcpEthernetPort.Options.IpAddrD;
            }
            else
            {
                return _pm.GetEthernetIpAddressD();
            }
        }

        /// <summary>
        /// Update with the latest Adcp ethernet port.
        /// </summary>
        /// <param name="port">Port to update.</param>
        public void UpdateEthernetPort(uint port)
        {
            if (AdcpEthernetPort != null)
            {
                // Update the port and then update the DB
                AdcpEthernetPort.Options.Port = port;
            }

            if (AdcpTcp != null)
            {
                AdcpTcp.Options.Port = port;
            }

            // Set the PM with the latest option
            _pm.UpdateEthernetPort(port);
        }

        /// <summary>
        /// Get the last used ADCP ethernet port address D.
        /// </summary>
        /// <returns>Last ADCP ethernet port Address D used.</returns>
        public uint GetEthernetPort()
        {
            if (AdcpEthernetPort != null)
            {
                return AdcpEthernetPort.Options.Port;
            }
            else
            {
                return _pm.GetEthernetPort();
            }
        }

        #endregion

        #region TCP

        /// <summary>
        /// Connect the TCP connection.
        /// </summary>
        public void ConnectTcp()
        {
            if (AdcpTcp != null)
            {
                AdcpTcp.ConnectTCP(AdcpEthernetPort.Options.IpAddr, AdcpEthernetPort.Options.Port);
            }
        }

        /// <summary>
        /// Disconnect the TCP connection.
        /// </summary>
        public void DisconnectTcp()
        {
            if (AdcpTcp != null)
            {
                AdcpTcp.DisconnectTCP();
            }
        }

        #endregion

        #region UDP

        /// <summary>
        /// Connect to the ADCP ethernet port.
        /// </summary>
        public void ConnectAdcpUdp()
        {
            // Disconnect if connected
            if(AdcpUdp != null)
            {
                DisconnectAdcpUdp();
            }

            // Get the IP address
            //var ipAddrA = _pm.GetEthernetIpAddressA().ToString();
            //var ipAddrB = _pm.GetEthernetIpAddressB().ToString();
            //var ipAddrC = _pm.GetEthernetIpAddressC().ToString();
            //var ipAddrD = _pm.GetEthernetIpAddressD().ToString();
            //string ipStr = ipAddrA + "." + ipAddrB + "." + ipAddrC + "." + ipAddrD;

            //// Set the endpoint with IP and port
            //IPAddress ip = IPAddress.Parse(ipStr);
            //IPAddress ip = IPAddress.Any;               // To listen, use ANY IP Address, To send, use IP address and port
            int port = (int)_pm.GetEthernetPort();
            //var endpt = new IPEndPoint(ip, port);

            //// Make the connection
            //AdcpUdp = new AdcpUdp(endpt);

            AdcpUdp = new AdcpUdp(port);

            if (AdcpUdp != null)
            {
                // Subscribe to receive ADCP data
                AdcpUdp.ReceiveRawUdpDataEvent += new AdcpUdp.ReceiveRawUdpDataEventHandler(ReceiveAdcpSerialData);
            }
        }

        /// <summary>
        /// Disconnect to the ADCP ethernet port.
        /// </summary>
        public void DisconnectAdcpUdp()
        {
            if (AdcpUdp != null)
            {
                // Subscribe to receive ADCP data
                AdcpUdp.ReceiveRawUdpDataEvent -= ReceiveAdcpSerialData;

                AdcpUdp.Dispose();
                AdcpUdp = null;
            }
        }

        #endregion

        #region Download

        /// <summary>
        /// Send the command to display the Directory listing.
        /// Then return the results from the serial display.
        /// </summary>
        /// <returns>List of the directory files.</returns>
        public RTI.Commands.AdcpDirListing GetDirectoryListing()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.GetDirectoryListing();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.GetDirectoryListing();
            }

            return new Commands.AdcpDirListing();
        }

        /// <summary>
        /// Download the the given file name and save the file to the given
        /// directory.  If parseData is set true, then also add the data to the
        /// project.
        /// 
        /// Download is about 7mb/minute for ethernet.
        /// 
        /// </summary>
        /// <param name="dirName">Directory to the store the data.</param>
        /// <param name="fileName">File to download from the ADCP.</param>
        /// <param name="parseData">TRUE = Parse and add the data to the project.</param>
        /// <returns>TRUE = File was downloaded.</returns>
        public bool DownloadData(string dirName, string fileName, bool parseData = false)
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.XModemDownload(dirName, fileName, parseData);
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.DownloadData(dirName, fileName, parseData);
            }

            return false;
        }

        /// <summary>
        /// Cancel the download process.
        /// </summary>
        public void CancelDownload()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                AdcpSerialPort.CancelDownload();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                AdcpEthernetPort.CancelDownload();
            }
        }

        #endregion

        #region STIME

        /// <summary>
        /// Set the system time to the ADCP.  This will use the time
        /// currently set on the computer.  This includes
        /// the time and date.
        /// </summary>
        /// <returns>TRUE = DateTime set.</returns>
        public bool SetLocalSystemDateTime()
        {
            bool timeResult = false;

            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.SetLocalSystemDateTime();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.SetLocalSystemDateTime();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.SetLocalSystemDateTime();
            }

            return timeResult;
        }

        /// <summary>
        /// Set the UTC time to the ADCP.  This will use the time
        /// currently set on the computer, then convert it to UTC time.  This includes
        /// the time and date.
        /// </summary>
        /// <returns>TRUE = DateTime set.</returns>
        public bool SetUtcSystemDateTime()
        {
            bool timeResult = false;

            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.SetUtcSystemDateTime();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.SetUtcSystemDateTime();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.SetUtcSystemDateTime();
            }

            return timeResult;
        }

        /// <summary>
        /// Get the Date and Time from the ADCP.
        /// It will be unknown if this time is UTC
        /// or Local.  So DateTime.Kind will be unspecified.
        /// </summary>
        /// <returns>DateTime from the ADCP.</returns>
        public DateTime GetAdcpDateTime()
        {
            if (AdcpCommType == AdcpCommTypes.Serial)
            {
                return AdcpSerialPort.GetAdcpDateTime();
            }
            else if (AdcpCommType == AdcpCommTypes.Ethernet)
            {
                return AdcpEthernetPort.GetAdcpDateTime();
            }
            else if (AdcpCommType == AdcpCommTypes.TCP)
            {
                return AdcpTcp.GetAdcpDateTime();
            }

            return DateTime.Now;
        }

        #endregion

        #region Validation Test Recording

        /// <summary>
        /// Check if the default record folder path exist.  If it does not exist,
        /// then create the folder.
        /// </summary>
        private void CheckRecordFolderExist()
        {
            if(!Directory.Exists(Pulse.Commons.DEFAULT_RECORD_DIR))
            {
                // Create the folder
                Directory.CreateDirectory(Pulse.Commons.DEFAULT_RECORD_DIR);
            }
        }

        /// <summary>
        /// Set the directory for the test results and
        /// turn on the flag.
        /// </summary>
        /// <param name="dir">Directory to write the lake test data to.</param>
        public void StartValidationTest(string dir)
        {
            // Initialize the file size
            _validationFileSize = 0;

            // Set Dir
            _validationTestDir = dir;

            // Set flag
            IsValidationTestRecording = true;
        }

        /// <summary>
        /// Stop writing data to the file.
        /// Close the file
        /// </summary>
        public string StopValidationTest()
        {
            // Set flag
            IsValidationTestRecording = false;

            try
            {
                if (_validationTestBinWriter != null)
                {
                    // Flush and close the writer
                    _validationTestBinWriter.Flush();
                    _validationTestBinWriter.Close();
                    _validationTestBinWriter.Dispose();
                    _validationTestBinWriter = null;

                    return ValidationTestFileName;
                }
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Lake Test results.", e);
            }

            return "";
        }

        /// <summary>
        /// Verify the writer is created.  If it is not turned on,
        /// craete the writer.  Then write the data.
        /// Write the validation test data to the validation test file.
        /// </summary>
        /// <param name="data">Data to write to the file.</param>
        /// <param name="ensemble">Ensemble to get the serial number.</param>
        public void WriteValidationTestData(byte[] data, DataSet.Ensemble ensemble)
        {
            // Verify recording is turned on
            if (IsValidationTestRecording)
            {
                // Create the writer if it does not exist
                if (_validationTestBinWriter == null)
                {
                    // Create writer
                    CreateValidationTestWriter(ensemble);
                }

                // See if a new file needs to be created based off the max file size
                if (_validationFileSize + data.Length > MAX_FILE_SIZE)
                {
                    // Record the current file size
                    long currentSize = ValidationTestBytesWritten;

                    // Stop the current recording
                    StopValidationTest();

                    // Start a new file
                    StartValidationTest(Pulse.Commons.DEFAULT_RECORD_DIR);

                    // Create writer with the current file size
                    CreateValidationTestWriter(ensemble, currentSize);
                }

                // Verify writer is created
                if (_validationTestBinWriter != null)
                {
                    try
                    {
                        // Seen thread exceptions for trying to have
                        // multiple threads write at the same time.
                        // The serial data is coming in and it is not writing fast enough
                        lock (_validationTestFileLock)
                        {
                            // Write the data to the file
                            _validationTestBinWriter.Write(data);

                            // Accumulate the file size
                            ValidationTestBytesWritten += data.Length;

                            _validationFileSize += data.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing lake test data
                        log.Error("Error Writing lake test data..", e);
                    }
                }
            }
        }

        /// <summary>
        /// Create a binary writer to write the validation test data
        /// to the file.  Use the ensemble to get a serial number for
        /// the data.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the serial number.</param>
        /// <param name="initFileSize">Initialize file size to display.</param>
        private void CreateValidationTestWriter(DataSet.Ensemble ensemble, long initFileSize = 0)
        {
            // Get the serial number
            SerialNumber serial = new SerialNumber();
            if (ensemble.IsEnsembleAvail)
            {
                serial = ensemble.EnsembleData.SysSerialNumber;
            }

            // Create a file name
            DateTime currDateTime = DateTime.Now;

            // Get the Serial number, if the serial number is a DVL serial, set the serial to DVL
            string serialStr = serial.SystemSerialNumber.ToString("00000");
            if (serial.SystemSerialNumber == SerialNumber.DVL_SYSTEM_SERIALNUMBER)
            {
                serialStr = "DVL";
            }

            string filename = string.Format("RTI_{0:yyyyMMddHHmmss}_{1}.bin", currDateTime, serialStr);
            string filePath = string.Format("{0}\\{1}", _validationTestDir, filename);

            try
            {
                // Open the binary writer
                _validationTestBinWriter = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                // Set the lake test file name
                ValidationTestFileName = filename;

                // Reset the number of bytes written
                ValidationTestBytesWritten = initFileSize;
            }
            catch (Exception e)
            {
                log.Error("Error creating the validation test file.", e);
            }
        }

        #endregion

        #region Raw Record ADCP

        /// <summary>
        /// Set the directory for the raw recording results and
        /// turn on the flag.
        /// </summary>
        /// <param name="dir">Directory to write the raw ADCP data to.</param>
        /// <param name="file">Filename if you want to manually set it.</param>
        public void StartRawAdcpRecord(string dir, string file = null)
        {
            _rawFileSize = 0;

            // Set Dir
            _rawAdcpRecordDir = dir;

            // Set the file
            _rawAdcpRecordFile = file;

            // Set flag
            IsRawAdcpRecording = true;
        }

        /// <summary>
        /// Stop writing data to the file.
        /// Close the file
        /// </summary>
        public string StopRawAdcpRecord()
        {
            // Set flag
            IsRawAdcpRecording = false;

            string fileName = RawAdcpRecordFileName;

            try
            {
                if (_rawAdcpRecordBinWriter != null)
                {
                    // Flush and close the writer
                    _rawAdcpRecordBinWriter.Flush();
                    _rawAdcpRecordBinWriter.Close();
                    _rawAdcpRecordBinWriter.Dispose();
                    _rawAdcpRecordBinWriter = null;
                }

                _rawAdcpRecordFile = null;
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Raw ADCP Record.", e);
                return fileName;
            }

            return fileName;
        }

        /// <summary>
        /// Verify the writer is created.  If it is not turned on,
        /// craete the writer.  Then write the data.
        /// Write the raw ADCP data to the raw ADCP file.
        /// </summary>
        /// <param name="data">Data to write to the file.</param>
        public void WriteRawAdcpData(byte[] data)
        {
            // Verify recording is turned on
            if (IsRawAdcpRecording)
            {
                // Create the writer if it does not exist
                if (_rawAdcpRecordBinWriter == null)
                {
                    // Create writer
                    CreateRawAdcpWriter();
                }

                // See if a new file needs to be created based off the max file size
                if (_rawFileSize + data.Length > MAX_FILE_SIZE)
                {
                    // Store the current file size
                    long currentSize = RawAdcpBytesWritten;

                    // Stop the current recording
                    StopRawAdcpRecord();

                    // Start a new file
                    StartRawAdcpRecord(Pulse.Commons.DEFAULT_RECORD_DIR);

                    // Create a new writer with the current file size
                    CreateRawAdcpWriter(currentSize);
                }

                // Verify writer is created
                if (_rawAdcpRecordBinWriter != null)
                {
                    try
                    {
                        // Seen thread exceptions for trying to have
                        // multiple threads write at the same time.
                        // The serial data is coming in and it is not writing fast enough
                        lock (_rawAdcpRecordFileLock)
                        {
                            // Write the data to the file
                            _rawAdcpRecordBinWriter.Write(data);

                            // Accumulate the number of bytes written
                            RawAdcpBytesWritten += data.Length;

                            // Monitor the file size to create a new file when it exeeds max file size
                            _rawFileSize += data.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing lake test data
                        log.Error("Error raw ADCP data..", e);
                    }
                }
            }
        }

        /// <summary>
        /// Create a binary writer to write the raw ADCP data
        /// to the file.  Use the ensemble to get a serial number for
        /// the data.
        /// </summary>
        /// <param name="initFileSize">Initial file size.</param>
        private void CreateRawAdcpWriter(long initFileSize = 0)
        {
            // Create a file name
            DateTime currDateTime = DateTime.Now;

            string filename = string.Format("RawADCP_{0:yyyyMMddHHmmss}.ens", currDateTime);
            if (_rawAdcpRecordFile != null)
            {
                // Overwrite the file name
                filename = _rawAdcpRecordFile;
            }

            string filePath = string.Format("{0}\\{1}", _rawAdcpRecordDir, filename);

            try
            {
                // Open the binary writer
                _rawAdcpRecordBinWriter = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                // Set the raw ADCP file name
                RawAdcpRecordFileName = filePath;

                // Reset the number of bytes written
                RawAdcpBytesWritten = initFileSize;
            }
            catch (Exception e)
            {
                log.Error("Error creating the raw ADCP file.", e);
            }
        }

        #endregion

        #region Raw Record GPS1

        /// <summary>
        /// Set the directory for the raw recording results and
        /// turn on the flag.
        /// </summary>
        /// <param name="dir">Directory to write the raw Gps1 data to.</param>
        public void StartRawGps1Record(string dir)
        {
            // Set Dir
            _rawGps1RecordDir = dir;

            // Set flag
            IsRawGps1Recording = true;
        }

        /// <summary>
        /// Stop writing data to the file.
        /// Close the file
        /// </summary>
        public void StopRawGps1Record()
        {
            // Set flag
            IsRawGps1Recording = false;

            try
            {
                // Flush and close the writer
                _rawGps1RecordBinWriter.Flush();
                _rawGps1RecordBinWriter.Close();
                _rawGps1RecordBinWriter.Dispose();
                _rawGps1RecordBinWriter = null;
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Raw Gps1 Record.", e);
            }
        }

        /// <summary>
        /// Verify the writer is created.  If it is not turned on,
        /// craete the writer.  Then write the data.
        /// Write the raw Gps1 data to the raw Gps1 file.
        /// </summary>
        /// <param name="data">Data to write to the file.</param>
        private void WriteRawGps1Data(byte[] data)
        {
            // Verify recording is turned on
            if (IsRawGps1Recording)
            {
                // Create the writer if it does not exist
                if (_rawGps1RecordBinWriter == null)
                {
                    // Create writer
                    CreateRawGps1Writer();
                }

                // Verify writer is created
                if (_rawGps1RecordBinWriter != null)
                {
                    try
                    {
                        // Seen thread exceptions for trying to have
                        // multiple threads write at the same time.
                        // The serial data is coming in and it is not writing fast enough
                        lock (_rawGps1RecordFileLock)
                        {
                            // Write the data to the file
                            _rawGps1RecordBinWriter.Write(data);

                            // Accumulate the number of bytes written
                            RawGps1BytesWritten += data.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing Gps1 data
                        log.Error("Error raw Gps1 data..", e);
                    }
                }
            }
        }

        /// <summary>
        /// Create a binary writer to write the raw Gps1 data
        /// to the file.  Use the ensemble to get a serial number for
        /// the data.
        /// </summary>
        private void CreateRawGps1Writer()
        {
            // Create a file name
            DateTime currDateTime = DateTime.Now;

            string filename = string.Format("RawGps1_{0:yyyyMMddHHmmss}.bin", currDateTime);
            string filePath = string.Format("{0}\\{1}", _rawGps1RecordDir, filename);

            try
            {
                // Open the binary writer
                _rawGps1RecordBinWriter = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                // Set the raw ADCP file name
                RawGps1RecordFileName = filePath;

                // Reset the number of bytes written
                RawGps1BytesWritten = 0;
            }
            catch (Exception e)
            {
                log.Error("Error creating the raw Gps1 file.", e);
            }
        }

        #endregion

        #region Raw Record GPS2

        /// <summary>
        /// Set the directory for the raw recording results and
        /// turn on the flag.
        /// </summary>
        /// <param name="dir">Directory to write the raw Gps2 data to.</param>
        public void StartRawGps2Record(string dir)
        {
            // Set Dir
            _rawGps2RecordDir = dir;

            // Set flag
            IsRawGps2Recording = true;
        }

        /// <summary>
        /// Stop writing data to the file.
        /// Close the file
        /// </summary>
        public void StopRawGps2Record()
        {
            // Set flag
            IsRawGps2Recording = false;

            try
            {
                // Flush and close the writer
                _rawGps2RecordBinWriter.Flush();
                _rawGps2RecordBinWriter.Close();
                _rawGps2RecordBinWriter.Dispose();
                _rawGps2RecordBinWriter = null;
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Raw Gps2 Record.", e);
            }
        }

        /// <summary>
        /// Verify the writer is created.  If it is not turned on,
        /// craete the writer.  Then write the data.
        /// Write the raw Gps2 data to the raw Gps2 file.
        /// </summary>
        /// <param name="data">Data to write to the file.</param>
        private void WriteRawGps2Data(byte[] data)
        {
            // Verify recording is turned on
            if (IsRawGps2Recording)
            {
                // Create the writer if it does not exist
                if (_rawGps2RecordBinWriter == null)
                {
                    // Create writer
                    CreateRawGps2Writer();
                }

                // Verify writer is created
                if (_rawGps2RecordBinWriter != null)
                {
                    try
                    {
                        // Seen thread exceptions for trying to have
                        // multiple threads write at the same time.
                        // The serial data is coming in and it is not writing fast enough
                        lock (_rawGps2RecordFileLock)
                        {
                            // Write the data to the file
                            _rawGps2RecordBinWriter.Write(data);

                            // Accumulate the number of bytes written
                            RawGps2BytesWritten += data.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing Gps2 data
                        log.Error("Error raw Gps2 data..", e);
                    }
                }
            }
        }

        /// <summary>
        /// Create a binary writer to write the raw Gps2 data
        /// to the file.  Use the ensemble to get a serial number for
        /// the data.
        /// </summary>
        private void CreateRawGps2Writer()
        {
            // Create a file name
            DateTime currDateTime = DateTime.Now;

            string filename = string.Format("RawGps2_{0:yyyyMMddHHmmss}.bin", currDateTime);
            string filePath = string.Format("{0}\\{1}", _rawGps2RecordDir, filename);

            try
            {
                // Open the binary writer
                _rawGps2RecordBinWriter = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                // Set the raw ADCP file name
                RawGps2RecordFileName = filePath;

                // Reset the number of bytes written
                RawGps2BytesWritten = 0;
            }
            catch (Exception e)
            {
                log.Error("Error creating the raw Gps2 file.", e);
            }
        }

        #endregion

        #region Raw Record Nmea1

        /// <summary>
        /// Set the directory for the raw recording results and
        /// turn on the flag.
        /// </summary>
        /// <param name="dir">Directory to write the raw Nmea1 data to.</param>
        public void StartRawNmea1Record(string dir)
        {
            // Set Dir
            _rawNmea1RecordDir = dir;

            // Set flag
            IsRawNmea1Recording = true;
        }

        /// <summary>
        /// Stop writing data to the file.
        /// Close the file
        /// </summary>
        public void StopRawNmea1Record()
        {
            // Set flag
            IsRawNmea1Recording = false;

            try
            {
                // Flush and close the writer
                _rawNmea1RecordBinWriter.Flush();
                _rawNmea1RecordBinWriter.Close();
                _rawNmea1RecordBinWriter.Dispose();
                _rawNmea1RecordBinWriter = null;
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Raw Nmea1 Record.", e);
            }
        }

        /// <summary>
        /// Verify the writer is created.  If it is not turned on,
        /// craete the writer.  Then write the data.
        /// Write the raw Nmea1 data to the raw Nmea1 file.
        /// </summary>
        /// <param name="data">Data to write to the file.</param>
        private void WriteRawNmea1Data(byte[] data)
        {
            // Verify recording is turned on
            if (IsRawNmea1Recording)
            {
                // Create the writer if it does not exist
                if (_rawNmea1RecordBinWriter == null)
                {
                    // Create writer
                    CreateRawNmea1Writer();
                }

                // Verify writer is created
                if (_rawNmea1RecordBinWriter != null)
                {
                    try
                    {
                        // Seen thread exceptions for trying to have
                        // multiple threads write at the same time.
                        // The serial data is coming in and it is not writing fast enough
                        lock (_rawNmea1RecordFileLock)
                        {
                            // Write the data to the file
                            _rawNmea1RecordBinWriter.Write(data);

                            // Accumulate the number of bytes written
                            RawNmea1BytesWritten += data.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing Nmea1 data
                        log.Error("Error raw Nmea1 data..", e);
                    }
                }
            }
        }

        /// <summary>
        /// Create a binary writer to write the raw Nmea1 data
        /// to the file.  Use the ensemble to get a serial number for
        /// the data.
        /// </summary>
        private void CreateRawNmea1Writer()
        {
            // Create a file name
            DateTime currDateTime = DateTime.Now;

            string filename = string.Format("RawNmea1_{0:yyyyMMddHHmmss}.bin", currDateTime);
            string filePath = string.Format("{0}\\{1}", _rawNmea1RecordDir, filename);

            try
            {
                // Open the binary writer
                _rawNmea1RecordBinWriter = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                // Set the raw ADCP file name
                RawNmea1RecordFileName = filePath;

                // Reset the number of bytes written
                RawNmea1BytesWritten = 0;
            }
            catch (Exception e)
            {
                log.Error("Error creating the raw Nmea1 file.", e);
            }
        }

        #endregion

        #region Raw Record Nmea2

        /// <summary>
        /// Set the directory for the raw recording results and
        /// turn on the flag.
        /// </summary>
        /// <param name="dir">Directory to write the raw Nmea2 data to.</param>
        public void StartRawNmea2Record(string dir)
        {
            // Set Dir
            _rawNmea2RecordDir = dir;

            // Set flag
            IsRawNmea2Recording = true;
        }

        /// <summary>
        /// Stop writing data to the file.
        /// Close the file
        /// </summary>
        public void StopRawNmea2Record()
        {
            // Set flag
            IsRawNmea2Recording = false;

            try
            {
                // Flush and close the writer
                _rawNmea2RecordBinWriter.Flush();
                _rawNmea2RecordBinWriter.Close();
                _rawNmea2RecordBinWriter.Dispose();
                _rawNmea2RecordBinWriter = null;
            }
            catch (Exception e)
            {
                // Log error
                log.Error("Error closing Raw Nmea2 Record.", e);
            }
        }

        /// <summary>
        /// Verify the writer is created.  If it is not turned on,
        /// craete the writer.  Then write the data.
        /// Write the raw Nmea2 data to the raw Nmea2 file.
        /// </summary>
        /// <param name="data">Data to write to the file.</param>
        private void WriteRawNmea2Data(byte[] data)
        {
            // Verify recording is turned on
            if (IsRawNmea2Recording)
            {
                // Create the writer if it does not exist
                if (_rawNmea2RecordBinWriter == null)
                {
                    // Create writer
                    CreateRawNmea2Writer();
                }

                // Verify writer is created
                if (_rawNmea2RecordBinWriter != null)
                {
                    try
                    {
                        // Seen thread exceptions for trying to have
                        // multiple threads write at the same time.
                        // The serial data is coming in and it is not writing fast enough
                        lock (_rawNmea2RecordFileLock)
                        {
                            // Write the data to the file
                            _rawNmea2RecordBinWriter.Write(data);

                            // Accumulate the number of bytes written
                            RawNmea2BytesWritten += data.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        // Error writing Nmea2 data
                        log.Error("Error raw Nmea2 data..", e);
                    }
                }
            }
        }

        /// <summary>
        /// Create a binary writer to write the raw Nmea2 data
        /// to the file.  Use the ensemble to get a serial number for
        /// the data.
        /// </summary>
        private void CreateRawNmea2Writer()
        {
            // Create a file name
            DateTime currDateTime = DateTime.Now;

            string filename = string.Format("RawNmea2_{0:yyyyMMddHHmmss}.bin", currDateTime);
            string filePath = string.Format("{0}\\{1}", _rawNmea2RecordDir, filename);

            try
            {
                // Open the binary writer
                _rawNmea2RecordBinWriter = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));

                // Set the raw ADCP file name
                RawNmea2RecordFileName = filePath;

                // Reset the number of bytes written
                RawNmea2BytesWritten = 0;
            }
            catch (Exception e)
            {
                log.Error("Error creating the raw Nmea2 file.", e);
            }
        }

        #endregion

        #region Break Statement

        /// <summary>
        /// Get the BREAK statement info.
        /// </summary>
        /// <returns>BREAK statement info.</returns>
        public RTI.Commands.BreakStmt GetBreakInfo()
        {
            // Clear buffer
            ReceiveBufferString = "";

            // Send a Break
            SendBreak();

            // Wait for an output
            Thread.Sleep(3000);

            // Get the buffer output
            string buffer = ReceiveBufferString;

            // Decode break statement
            return RTI.Commands.AdcpCommands.DecodeBREAK(buffer);
        }

        #endregion

        #region CEMAC

        /// <summary>
        /// cemac
        /// CEMAC
        /// 
        /// MAC 02:ff:fe:fd:fc:fb
        /// IP  192.168.1.130
        /// Link OK
        /// Speed 1, FullDuplex 1
        /// </summary>
        /// <returns>String from the response.</returns>
        public string Cemac()
        {
            string result = SendDataGetReply("CEMAC");

            ReceiveBufferString = "";

            Thread.Sleep(20000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        #endregion

        #region DIAG Tests

        /// <summary>
        /// Get the DSFORMAT result.
        /// </summary>
        /// <returns></returns>
        public string DsFormatTest()
        {
            string result = SendDataGetReply("DSFORMAT");

            ReceiveBufferString = "";

            Thread.Sleep(10000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGRCV test result.
        /// </summary>
        /// <returns></returns>
        public string DiagRcvTest()
        {
            string result = SendDataGetReply("DIAGRCV");

            ReceiveBufferString = "";

            Thread.Sleep(10000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGBOOST test result.
        /// </summary>
        /// <returns></returns>
        public string DiagBoostTest()
        {
            string result = SendDataGetReply("DIAGBOOST");

            ReceiveBufferString = "";

            Thread.Sleep(45000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGXMT test result.
        /// 
        /// Send
        /// ENGXMTBITSAVE 0,20,40,30,50,75,95,75,95,11,40,20,25,-20,-25
        /// to set the proper parameters for the test.
        /// 
        /// </summary>
        /// <returns></returns>
        public string DiagXmtTest()
        {
            string result = SendDataGetReply("DIAGXMT");

            ReceiveBufferString = "";

            Thread.Sleep(3000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGRTC test result.
        /// 
        /// DIAGRTC
        /// time: 15/05/28 1 10:31:19
        /// alm1: 80 06 39 29
        /// alm2: 00 00 00
        /// cntr: 01h
        /// cntr: 01h
        /// stat: 00h
        /// temperature: 40.000 C
        /// diagrtc
        /// </summary>
        /// <returns>DIAGRTC result.</returns>
        public string DiagRtcTest()
        {
            string result = SendDataGetReply("DIAGRTC");

            ReceiveBufferString = "";

            Thread.Sleep(2000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGPRESSURE test result.
        /// </summary>
        /// <returns></returns>
        public string DiagPressureTest()
        {
            string result = SendDataGetReply("DIAGPRESSURE");

            ReceiveBufferString = "";

            Thread.Sleep(4000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGSD test result.
        /// </summary>
        /// <returns>SD card test.</returns>
        public string DiagSdTest()
        {
            string result = SendDataGetReply("DIAGSD");

            ReceiveBufferString = "";

            Thread.Sleep(4000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGPNI test result.
        /// </summary>
        /// <returns></returns>
        public string DiagPniTest()
        {
            string result = SendDataGetReply("DIAGPNI");

            ReceiveBufferString = "";

            Thread.Sleep(5000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGPNI 1 test result.
        /// </summary>
        /// <returns></returns>
        public string DiagPni1Test()
        {
            string result = SendDataGetReply("DIAGPNI 1");

            ReceiveBufferString = "";

            Thread.Sleep(2000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the DIAGSAMP test result.
        /// </summary>
        /// <returns></returns>
        public RTI.Commands.DiagSamp DiagSampTest()
        {
            string result = SendDataGetReply("DIAGSAMP");

            ReceiveBufferString = "";

            Thread.Sleep(2000);

            result += ReceiveBufferString;

            var samp = Commands.AdcpCommands.DecodeDiagSamp(result.Trim());

            return samp;
        }

        /// <summary>
        /// Get the DIAGSPECTRUM test result.
        /// </summary>
        /// <param name="ssCode">Subsystem Code.</param>
        /// <returns>Spectrum Test results</returns>
        public RTI.Commands.DiagSpectrum DiagSpectrumTest(char ssCode)
        {

            // Send the CEPO command to set only 1 subsystem
            SendDataWaitReply(string.Format("CEPO {0}", ssCode)); 

            string result = SendDataGetReply("DIAGSPECTRUM");

            ReceiveBufferString = "";

            // Wait 75 seconds to collect all the data
            Thread.Sleep(180000);

            result += ReceiveBufferString;

            var samp = Commands.AdcpCommands.DecodeDiagSpectrum(result.Trim());

            return samp;
        }

        /// <summary>
        /// Get the DIAGRUB test result.
        /// </summary>
        /// <returns></returns>
        public string DiagRubTest()
        {
            return SendDataGetReply("DIAGRUB");
        }

        /// <summary>
        /// Get the DIAGRUB # test result.
        /// Set a wait time in milliseconds.
        /// Dual frequencies will do both frequencies.
        /// </summary>
        /// <param name="beam">Beam Number.</param>
        /// <param name="waitTime">Wait time in milliseconds.</param>
        /// <returns>Result from the rub beam test.</returns>
        public string DiagRubTest(int beam, int waitTime)
        {
            string result = SendDataGetReply(string.Format("DIAGRUB {0}", beam.ToString()));

            ReceiveBufferString = "";

            Thread.Sleep(waitTime);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the ENGI2CSHOW test result.
        /// 
        /// </summary>
        /// <returns>Result from command.</returns>
        public string EngI2cShowTest()
        {
            string result = SendDataGetReply("ENGI2CSHOW");

            ReceiveBufferString = "";

            Thread.Sleep(3000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the ENGCONF test result.
        /// 
        /// </summary>
        /// <returns>Result from command.</returns>
        public string EngConfTest()
        {
            string result = SendDataGetReply("ENGCONF");

            ReceiveBufferString = "";

            Thread.Sleep(3000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the ENGBEAMSHOW test result.
        /// 
        /// </summary>
        /// <returns>Result from command.</returns>
        public string EngBeamShowTest()
        {
            string result = SendDataGetReply("ENGBEAMSHOW");

            ReceiveBufferString = "";

            Thread.Sleep(3000);

            result += ReceiveBufferString;

            return result.Trim();
        }

        /// <summary>
        /// Get the SLEEP test result.
        /// 
        /// </summary>
        /// <returns>Result from command.</returns>
        public RTI.Commands.BreakStmt SleepTest()
        {
            string result = SendDataGetReply("SLEEP");

            //ReceiveBufferString = "";

            Thread.Sleep(5000);

            return GetBreakInfo();
        }

        #endregion

        #region Update Firmware

        /// <summary>
        /// Execute the upload process.  This should be called
        /// from the async command.
        /// </summary>
        /// <param name="fileNames">File names to upload.</param>
        public void UpdateFirmware(string[] fileNames)
        {
            if (fileNames != null)
            {
                // Verify the file name is good
                if (AdcpSerialPort.IsAvailable())
                {
                    // Stop the ADCP pinging if its pinging
                    AdcpSerialPort.StopPinging();

                    // Upload all the selected files
                    foreach (var file in fileNames)
                    {
                        // Upload the file to the ADCP
                        AdcpSerialPort.XModemUpload(file);

                        // Wait for the update to complete
                        Thread.Sleep(AdcpSerialPort.WAIT_STATE * 2);

                        // Load the firmware to NAND
                        if (file.ToLower().Contains("rtisys"))
                        {
                            AdcpSerialPort.SendDataWaitReply("FMCOPYS");
                        }

                        // Load the boot code to NAND
                        if (file.ToLower().Contains("boot"))
                        {
                            AdcpSerialPort.SendDataWaitReply("FMCOPYB");
                        }
                    }


                    // Reboot the ADCP to use the new firmware
                    AdcpSerialPort.Reboot();

                    // Validate the files uploaded
                    // By downloading it and compairing it against
                    // the original file
                }
            }
        }

        #endregion

        #region Events

        #region Publish Ensemble

        /// <summary>
        /// Publish the latest ensemble to all subscribers.
        /// </summary>
        /// <param name="ens">Ensemble to publish.</param>
        /// <param name="source">Source of ensemble.</param>
        /// <param name="type">Type of ensemble.</param>
        private void PublishEnsemble(DataSet.Ensemble ens, EnsembleSource source, EnsembleType type)
        {
            //if (ReceiveEnsembleEvent != null)
            //{
            //    ReceiveEnsembleEvent(ens);
            //}

            if (ens != null)
            {
                //_events.PublishOnUIThread(new EnsembleEvent(ens, source, type));
                //_events.PublishOnUIThreadAsync(new EnsembleEvent(ens, source, type));
                EnsembleEvent ensEvent = new EnsembleEvent(ens, source, type);

                // Publish the ensemble
                _events.PublishOnBackgroundThread(ensEvent);

                // Display the ensemble
                _pm.DisplayEnsemble(ensEvent);
            }

        }

        /// <summary>
        /// Publish the latest raw ensemble to all subscribers.
        /// This data will NOT be screened, averaged or VM be applied to it.
        /// </summary>
        /// <param name="ens">Ensemble to publish.</param>
        /// <param name="source">Source of ensemble.</param>
        /// <param name="origDataFormat">Original Data format.</param>
        /// <param name="type">Type of ensemble.</param>
        private void PublishRawEnsemble(DataSet.Ensemble ens, EnsembleSource source, EnsembleType type, AdcpCodec.CodecEnum origDataFormat)
        {
            //if (ReceiveEnsembleEvent != null)
            //{
            //    ReceiveEnsembleEvent(ens);
            //}

            if (ens != null)
            {
                //_events.PublishOnUIThread(new EnsembleEvent(ens, source, type));
                //_events.PublishOnUIThreadAsync(new EnsembleEvent(ens, source, type));
                _events.PublishOnBackgroundThread(new EnsembleRawEvent(ens, source, type, origDataFormat));
            }

        }

        #endregion

        #region Publish Long Term Averaged Ensemble

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        /// <param data="ensemble">Latest Long Term Averaged ensemble.</param>
        public delegate void ReceiveLtAvgEnsembleEventHandler(DataSet.Ensemble ensemble);

        /// <summary>
        /// Subscribe to receive the latest Long Term Averaged ensemble.  Whether playback or live data,
        /// this event will give you the latest Long Term Averaged ensemble.
        /// 
        /// To subscribe:
        /// _adcpConn.ReceiveLtAvgEnsembleEvent += new _adcpConn.ReceiveLtAvgEnsembleEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.ReceiveLtAvgEnsembleEvent -= (method to call)
        /// </summary>
        public event ReceiveLtAvgEnsembleEventHandler ReceiveLtAvgEnsembleEvent;

        /// <summary>
        /// Publish the latest Long Term Averaged ensemble to all subscribers.
        /// </summary>
        /// <param name="ens">Ensemble to publish.</param>
        private void PublishLtAvgEnsemble(DataSet.Ensemble ens)
        {
            if (ReceiveLtAvgEnsembleEvent != null)
            {
                ReceiveLtAvgEnsembleEvent(ens);
            }
        }

        #endregion

        #region Publish Short Term Averaged Ensemble

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        /// <param data="ensemble">Latest Short Term Averaged ensemble.</param>
        public delegate void ReceiveStAvgEnsembleEventHandler(DataSet.Ensemble ensemble);

        /// <summary>
        /// Subscribe to receive the latest Short Term Averaged ensemble.  Whether playback or live data,
        /// this event will give you the latest Long Term Averaged ensemble.
        /// 
        /// To subscribe:
        /// _adcpConn.ReceiveStAvgEnsembleEvent += new _adcpConn.ReceiveStAvgEnsembleEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.ReceiveStAvgEnsembleEvent -= (method to call)
        /// </summary>
        public event ReceiveStAvgEnsembleEventHandler ReceiveStAvgEnsembleEvent;

        /// <summary>
        /// Publish the latest Short Term Averaged ensemble to all subscribers.
        /// </summary>
        /// <param name="ens">Ensemble to publish.</param>
        private void PublishStAvgEnsemble(DataSet.Ensemble ens)
        {
            if (ReceiveStAvgEnsembleEvent != null)
            {
                ReceiveStAvgEnsembleEvent(ens);
            }
        }

        #endregion

        #region Download Events

        #region File Size Event

        /// <summary>
        /// Event to get the file size for the given
        /// file name.  The size will be in bytes.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        public delegate void DownloadFileSizeEventHandler(string fileName, long fileSize);

        /// <summary>
        /// Subscribe to receive the event for the size of the file in bytes.
        /// 
        /// To subscribe:
        /// _adcpSerialPort.FileSizeEvent += new serialConnection.FileSizeEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.FileSizeEvent -= (method to call)
        /// </summary>
        public event DownloadFileSizeEventHandler DownloadFileSizeEvent;

        #endregion

        #region Download Progress Event

        /// <summary>
        /// Event to receive the progress of the download process.
        /// This will give the number of bytes currently written
        /// for the current file being downloaded.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="bytesWritten">Current bytes written.</param>
        public delegate void DownloadProgressEventHandler(string fileName, long bytesWritten);

        /// <summary>
        /// Subscribe to receive the event for the project of the download.  This will get the
        /// current number of bytes written for the current file being downloaded.
        /// 
        /// To subscribe:
        /// _adcpSerialPort.DownloadFileSizeEvent += new serialConnection.DownloadFileSizeEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.DownloadFileSizeEvent -= (method to call)
        /// </summary>
        public event DownloadProgressEventHandler DownloadProgressEvent;

        #endregion

        #region Download Complete Event

        /// <summary>
        /// Event to receive when the download is complete
        /// for the given file name.
        /// The parameter goodDownload is used to tell the user
        /// if the download was completed successfully or the download
        /// was aborted.
        /// </summary>
        /// <param name="fileName">Name of the file completed the download.</param>
        /// <param name="goodDownload">Flag if the download was completed successfully.</param>
        public delegate void DownloadCompleteEventHandler(string fileName, bool goodDownload);

        /// <summary>
        /// Subscribe to receive the event when the file has been completely downloaded.
        /// 
        /// To subscribe:
        /// _adcpSerialPort.DownloadCompleteEvent += new serialConnection.DownloadCompleteEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.DownloadCompleteEvent -= (method to call)
        /// </summary>
        public event DownloadCompleteEventHandler DownloadCompleteEvent;

        #endregion

        #endregion

        #region Upload Events

        #region Upload File Size Event

        /// <summary>
        /// Event to get the file size for the given
        /// file name.  The size will be in bytes.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        public delegate void UploadFileSizeEventHandler(string fileName, long fileSize);

        /// <summary>
        /// Subscribe to receive the event for the size of the file in bytes.
        /// 
        /// To subscribe:
        /// _adcpSerialPort.UploadFileSizeEvent += new serialConnection.UploadFileSizeEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.UploadFileSizeEvent -= (method to call)
        /// </summary>
        public event UploadFileSizeEventHandler UploadFileSizeEvent;

        #endregion

        #region Upload Progress Event

        /// <summary>
        /// Event to receive the progress of the upload process.
        /// This will give the number of bytes currently written
        /// for the current file being uploaded.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="bytesWritten">Current bytes written.</param>
        public delegate void UploadProgressEventHandler(string fileName, long bytesWritten);

        /// <summary>
        /// Subscribe to receive the event for the project of the upload.  This will get the
        /// current number of bytes written for the current file being uploaded.
        /// 
        /// To subscribe:
        /// _adcpSerialPort.UploadProgressEvent += new serialConnection.UploadProgressEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.UploadProgressEvent -= (method to call)
        /// </summary>
        public event UploadProgressEventHandler UploadProgressEvent;

        #endregion

        #region Upload Complete Event

        /// <summary>
        /// Event to receive when the upload is complete
        /// for the given file name.
        /// The parameter goodUpload is used to tell the user
        /// if the upload was completed successfully or the upload
        /// was aborted.
        /// </summary>
        /// <param name="fileName">Name of the file completed the upload.</param>
        /// <param name="goodUpload">Flag if the upload was completed successfully.</param>
        public delegate void UploadCompleteEventHandler(string fileName, bool goodUpload);

        /// <summary>
        /// Subscribe to receive the event when the file has been completely upload.
        /// 
        /// To subscribe:
        /// _adcpSerialPort.UploadCompleteEvent += new serialConnection.UploadCompleteEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _adcpSerialPort.UploadCompleteEvent -= (method to call)
        /// </summary>
        public event UploadCompleteEventHandler UploadCompleteEvent;

        #endregion

        #endregion

        #region Receive Ensemble From Codec

        ///// <summary>
        ///// Handle the ensembles that come from the codec.  THis will record the
        ///// data if it needs to be.  It will then publish the data to all subscribers.
        ///// If the serial number has not been set, this will also set the serial number for
        ///// the ADCP.
        ///// </summary>
        ///// <param name="binaryEnsemble">Binary data for the ensemble.</param>
        ///// <param name="ensemble">Ensemble as an object.</param>
        ///// <param name="source">Source of the ensemble.</param>
        ///// <param name="type">Ensemble type.</param>
        ///// <param name="origDataFormat">Original Data Format.</param>
        //private void ReceiveEnsembleFromCodec_old(byte[] binaryEnsemble, DataSet.Ensemble ensemble, EnsembleSource source, EnsembleType type, AdcpCodec.CodecEnum origDataFormat)
        //{
        //    // Add the buffered GPS and NMEA data to the data

        //    // Add the data to the buffer
        //    _ensembleBuffer.Enqueue(new EnsembleData(binaryEnsemble, ensemble, source, type, origDataFormat));

        //    // Process the data
        //    //ReceiveEnsembleFromCodecCommand.Execute(null);
        //    //await Task.Run(() => ReceiveEnsembleFromCodecExecute());
        //    Task.Run(() => ReceiveEnsembleFromCodecExecute());
        //}

        ///// <summary>
        ///// Process the buffered ensemble data from the codecs.
        ///// </summary>
        //private void ReceiveEnsembleFromCodecExecute()
        //{
        //    // Process all the data in the buffer
        //    while (!_ensembleBuffer.IsEmpty)
        //    {
        //        // Get the data from the buffer
        //        EnsembleData data = null;
        //        if (_ensembleBuffer.TryDequeue(out data))
        //        {
        //            // Add the GPS and NMEA data to the ensemble
        //            // and clear the buffers
        //            string adcpGpsBuffer = "";
        //            string gps1Buffer = Gps1BufferData();
        //            string gps2Buffer = Gps2BufferData();
        //            string nmea1Buffer = Nmea1BufferData();
        //            string nmea2Buffer = Nmea2BufferData();

        //            // Store the GPS data that is already in the ensemble
        //            if (data.Ensemble.IsNmeaAvail && data.Ensemble.NmeaData != null)
        //            {
        //                adcpGpsBuffer = data.Ensemble.NmeaData.ToString();
        //            }

        //            if (!data.Ensemble.IsNmeaAvail || data.Ensemble.NmeaData == null)
        //            {
        //                data.Ensemble.AddNmeaData(gps1Buffer);
        //            }
        //            else
        //            {
        //                data.Ensemble.NmeaData.MergeNmeaData(gps1Buffer);
        //            }
        //            data.Ensemble.NmeaData.MergeNmeaData(gps2Buffer);
        //            data.Ensemble.NmeaData.MergeNmeaData(nmea1Buffer);
        //            data.Ensemble.NmeaData.MergeNmeaData(nmea2Buffer);

        //            // Add the buffers
        //            data.Ensemble.AddAdcpGpsData(adcpGpsBuffer);
        //            data.Ensemble.AddGps1Data(gps1Buffer);
        //            data.Ensemble.AddGps2Data(gps2Buffer);
        //            data.Ensemble.AddNmea1Data(nmea1Buffer);
        //            data.Ensemble.AddNmea2Data(nmea2Buffer);

        //            // Check if a project is selected and is recording
        //            if (_pm.IsProjectSelected && IsRecording)
        //            {
        //                // Check if the serial number is set for the project
        //                if (_pm.SelectedProject.SerialNumber.IsEmpty())
        //                {
        //                    if (data.Ensemble.IsEnsembleAvail)
        //                    {
        //                        _pm.SelectedProject.SerialNumber = data.Ensemble.EnsembleData.SysSerialNumber;
        //                    }
        //                }

        //                // Record the data
        //                _pm.SelectedProject.RecordBinaryEnsemble(data.BinaryEnsemble);
        //                _pm.SelectedProject.RecordDbEnsemble(data.Ensemble, data.OrigDataFormat);
        //            }

        //            // Check if validation testing
        //            if (IsValidationTestRecording)
        //            {
        //                WriteValidationTestData(data.BinaryEnsemble, data.Ensemble);
        //            }

        //            // Set the ensemble
        //            DataSet.Ensemble ensemble = data.Ensemble;

        //            // Create the velocity vectors for the ensemble
        //            DataSet.VelocityVectorHelper.CreateVelocityVector(ref ensemble);

        //            // Vessel Mount Options
        //            VesselMountScreen(ref ensemble);

        //            // Screen the data
        //            if (_screenDataVM != null)
        //            {
        //                _screenDataVM.ScreenData(ref ensemble, data.OrigDataFormat);
        //            }

        //            // Average the data
        //            if (_averagingVM != null)
        //            {
        //                _averagingVM.AverageEnsemble(ensemble);
        //            }

        //            // Publish the data
        //            // Do not publish the data if you are importing data
        //            if (!IsImporting)
        //            {
        //                PublishEnsemble(ensemble, data.Source, data.Type);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Receive the ensemble from the codec, process the data, and pass it to the next subscriber.
        /// </summary>
        /// <param name="binaryEnsemble">Binary ensemble.</param>
        /// <param name="ensemble">Ensemble object.</param>
        /// <param name="source">Source that the ensemble came from.</param>
        /// <param name="type">Type of ensemble, averaged data or not averaged.</param>
        /// <param name="origDataFormat">The original data format.</param>
        private void ReceiveEnsembleFromCodec(byte[] binaryEnsemble, DataSet.Ensemble ensemble, EnsembleSource source, EnsembleType type, AdcpCodec.CodecEnum origDataFormat)
        {
            // Add the GPS and NMEA data to the ensemble
            // and clear the buffers
            string adcpGpsBuffer = "";
            string gps1Buffer = Gps1BufferData();
            string gps2Buffer = Gps2BufferData();
            string nmea1Buffer = Nmea1BufferData();
            string nmea2Buffer = Nmea2BufferData();

            // Store the GPS data that is already in the ensemble
            if (ensemble.IsNmeaAvail && ensemble.NmeaData != null)
            {
                adcpGpsBuffer = ensemble.NmeaData.ToString();
            }

            if (!ensemble.IsNmeaAvail || ensemble.NmeaData == null)
            {
                ensemble.AddNmeaData(gps1Buffer);
            }
            else
            {
                ensemble.NmeaData.MergeNmeaData(gps1Buffer);
            }
            ensemble.NmeaData.MergeNmeaData(gps2Buffer);
            ensemble.NmeaData.MergeNmeaData(nmea1Buffer);
            ensemble.NmeaData.MergeNmeaData(nmea2Buffer);

            // Add the buffers
            ensemble.AddAdcpGpsData(adcpGpsBuffer);
            ensemble.AddGps1Data(gps1Buffer);
            ensemble.AddGps2Data(gps2Buffer);
            ensemble.AddNmea1Data(nmea1Buffer);
            ensemble.AddNmea2Data(nmea2Buffer);

            // Check if a project is selected and is recording
            if (_pm.IsProjectSelected && IsRecording)
            {
                // Check if the serial number is set for the project
                if (_pm.SelectedProject.SerialNumber.IsEmpty())
                {
                    if (ensemble.IsEnsembleAvail)
                    {
                        _pm.SelectedProject.SerialNumber = ensemble.EnsembleData.SysSerialNumber;
                    }
                }

                // Record the data
                _pm.SelectedProject.RecordBinaryEnsemble(binaryEnsemble);
                _pm.SelectedProject.RecordDbEnsemble(ensemble, origDataFormat);
            }

            // Check if validation testing
            if (IsValidationTestRecording)
            {
                WriteValidationTestData(binaryEnsemble, ensemble);
            }

            // Publish the raw unscreened and averaged ensemble
            // Do not publish the data if you are importing data
            if (!IsImporting)
            {
                PublishRawEnsemble(ensemble.Clone(), source, type, origDataFormat);
            }

            // Make a copy of the ensemble to screen and average and pass to all the views
            DataSet.Ensemble newEnsemble = ensemble.Clone();

            lock (newEnsemble.SyncRoot)
            {
                // Create the velocity vectors for the ensemble
                DataSet.VelocityVectorHelper.CreateVelocityVector(ref newEnsemble);

                // Vessel Mount Options
                VesselMountScreen(ref newEnsemble);

                // Screen the data
                if (_screenDataVM != null)
                {
                    _screenDataVM.ScreenData(ref newEnsemble, origDataFormat);
                }

                // Average the data
                if (_averagingVM != null)
                {
                    _averagingVM.AverageEnsemble(newEnsemble);
                }

                // Publish the data
                // Do not publish the data if you are importing data
                if (!IsImporting)
                {
                    PublishEnsemble(newEnsemble, source, type);
                }
            }
        }

        #endregion

        #region ADCP Connect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void AdcpSerialConnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when a ADCP serial connection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.AdcpSerialConnectEvent += new adcpConnection.AdcpSerialConnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.AdcpSerialConnectEvent -= (method to call)
        /// </summary>
        public event AdcpSerialConnectEventHandler AdcpSerialConnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event.
        /// </summary>
        private void PublishAdcpSerialConnection()
        {
            if (AdcpSerialConnectEvent != null)
            {
                AdcpSerialConnectEvent();
            }
        }

        #endregion

        #region ADCP Disconnect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void AdcpSerialDisconnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when an ADCP serial disconnection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.AdcpSerialDisconnectEvent += new adcpConnection.AdcpSerialDisconnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.AdcpSerialDisconnectEvent -= (method to call)
        /// </summary>
        public event AdcpSerialDisconnectEventHandler AdcpSerialDisconnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event that the ADCP serial port is disconnected.
        /// </summary>
        private void PublishAdcpSerialDisconnection()
        {
            if (AdcpSerialDisconnectEvent != null)
            {
                AdcpSerialDisconnectEvent();
            }
        }

        #endregion

        #region GPS 1 Connect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Gps1SerialConnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when a GPS 1 serial connection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Gps1SerialConnectEvent += new adcpConnection.Gps1SerialConnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Gps1SerialConnectEvent -= (method to call)
        /// </summary>
        public event Gps1SerialConnectEventHandler Gps1SerialConnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event.
        /// </summary>
        private void PublishGps1SerialConnection()
        {
            if (Gps1SerialConnectEvent != null)
            {
                Gps1SerialConnectEvent();
            }
        }

        #endregion

        #region GPS 1 Disconnect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Gps1SerialDisconnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when an GPS 1 serial disconnection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Gps1SerialDisconnectEvent += new adcpConnection.Gps1SerialDisconnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Gps1SerialDisconnectEvent -= (method to call)
        /// </summary>
        public event Gps1SerialDisconnectEventHandler Gps1SerialDisconnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event that the GPS 1 serial port is disconnected.
        /// </summary>
        private void PublishGps1SerialDisconnection()
        {
            if (Gps1SerialDisconnectEvent != null)
            {
                Gps1SerialDisconnectEvent();
            }
        }

        #endregion

        #region GPS 2 Connect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Gps2SerialConnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when a GPS 1 serial connection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Gps2SerialConnectEvent += new adcpConnection.Gps2SerialConnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Gps2SerialConnectEvent -= (method to call)
        /// </summary>
        public event Gps2SerialConnectEventHandler Gps2SerialConnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event.
        /// </summary>
        private void PublishGps2SerialConnection()
        {
            if (Gps2SerialConnectEvent != null)
            {
                Gps2SerialConnectEvent();
            }
        }

        #endregion

        #region GPS 2 Disconnect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Gps2SerialDisconnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when an GPS 2 serial disconnection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Gps2SerialDisconnectEvent += new adcpConnection.Gps2SerialDisconnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Gps2SerialDisconnectEvent -= (method to call)
        /// </summary>
        public event Gps2SerialDisconnectEventHandler Gps2SerialDisconnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event that the GPS 2 serial port is disconnected.
        /// </summary>
        private void PublishGps2SerialDisconnection()
        {
            if (Gps2SerialDisconnectEvent != null)
            {
                Gps2SerialDisconnectEvent();
            }
        }

        #endregion

        #region NMEA 1 Connect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Nmea1SerialConnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when a NMEA 1 serial connection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Nmea1SerialConnectEvent += new adcpConnection.Nmea1SerialConnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Nmea1SerialConnectEvent -= (method to call)
        /// </summary>
        public event Nmea1SerialConnectEventHandler Nmea1SerialConnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event.
        /// </summary>
        private void PublishNmea1SerialConnection()
        {
            if (Nmea1SerialConnectEvent != null)
            {
                Nmea1SerialConnectEvent();
            }
        }

        #endregion

        #region NMEA 1 Disconnect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Nmea1SerialDisconnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when an NMEA 1 serial disconnection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Nmea1SerialDisconnectEvent += new adcpConnection.Nmea1SerialDisconnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Nmea1SerialDisconnectEvent -= (method to call)
        /// </summary>
        public event Nmea1SerialDisconnectEventHandler Nmea1SerialDisconnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event that the NMEA 1 serial port is disconnected.
        /// </summary>
        private void PublishNmea1SerialDisconnection()
        {
            if (Nmea1SerialDisconnectEvent != null)
            {
                Nmea1SerialDisconnectEvent();
            }
        }

        #endregion

        #region NMEA 2 Connect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Nmea2SerialConnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when a NMEA 2 serial connection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Nmea2SerialConnectEvent += new adcpConnection.Nmea2SerialConnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Nmea2SerialConnectEvent -= (method to call)
        /// </summary>
        public event Nmea2SerialConnectEventHandler Nmea2SerialConnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event.
        /// </summary>
        private void PublishNmea2SerialConnection()
        {
            if (Nmea2SerialConnectEvent != null)
            {
                Nmea2SerialConnectEvent();
            }
        }

        #endregion

        #region NMEA 2 Disconnect Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void Nmea2SerialDisconnectEventHandler();

        /// <summary>
        /// Subscribe to receive event when an NMEA 2 serial disconnection is made.
        /// 
        /// To subscribe:
        /// adcpConnection.Nmea2SerialDisconnectEvent += new adcpConnection.Nmea2SerialDisconnectEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.Nmea2SerialDisconnectEvent -= (method to call)
        /// </summary>
        public event Nmea2SerialDisconnectEventHandler Nmea2SerialDisconnectEvent;

        /// <summary>
        /// If there are any subscribers, send the event that the NMEA 2 serial port is disconnected.
        /// </summary>
        private void PublishNmea2SerialDisconnection()
        {
            if (Nmea2SerialDisconnectEvent != null)
            {
                Nmea2SerialDisconnectEvent();
            }
        }

        #endregion

        #region Receive Data Event

        /// <summary>
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void ReceiveDataEventHandler(byte[] data);

        /// <summary>
        /// Subscribe to receive event when an data is available.
        /// 
        /// To subscribe:
        /// adcpConnection.ReceiveDataEvent += new adcpConnection.ReceiveDataEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// adcpConnection.ReceiveDataEvent -= (method to call)
        /// </summary>
        public event ReceiveDataEventHandler ReceiveDataEvent;

        /// <summary>
        /// If there are any subscribers, send the event that data is available.
        /// </summary>
        private void PublishReceiveData(byte[] data)
        {
            if (ReceiveDataEvent != null)
            {
                ReceiveDataEvent(data);
            }
        }

        #endregion

        #region Vessel Mount Screen Data

        /// <summary>
        /// Screen the ensemble with the given options.
        /// </summary>
        /// <param name="ensemble">Ensemble to screen.</param>
        private void VesselMountScreen(ref DataSet.Ensemble ensemble)
        {
            // Vessel Mount Options
            if (_pm.IsProjectSelected && _pm.SelectedProject.Configuration.VesselMountOptions != null)
            {
                VesselMount.VesselMountScreen.Screen(ref ensemble, _pm.SelectedProject.Configuration.VesselMountOptions);
            }
            else
            {
                VesselMount.VesselMountScreen.Screen(ref ensemble, _pm.AppConfiguration.GetVesselMountOptions());
            }

        }

        #endregion

        #region Receive Data Thread

        /// <summary>
        /// Thread that handles receiving the data.
        /// </summary>
        private void ReceiveDataThread()
        {
            while (_continue)
            {
                try
                {
                    // Block until awoken when data is received
                    // Timeout every 60 seconds to see if shutdown occured
                    _eventWaitData.WaitOne(1000);

                    //Debug.WriteLine("AdcpConnection:ReceiveDataThread: STart");

                    // If wakeup was called to kill thread
                    if (!_continue)
                    {
                        return;
                    }

                    // Process any data in the queue
                    while (_processDataQueue.Count > 0)
                    {
                        //Thread.Sleep(100000);
                        //Debug.WriteLine("Process Data Queue: {0}", _processDataQueue.Count);

                        ProcessData data;
                        if(_processDataQueue.TryDequeue(out data) )
                        {
                            switch (data.type)
                            {
                                case ProcessDataTypes.AdcpSerial:
                                    ProcessAdcpSerialData(data.data);
                                    break;
                                case ProcessDataTypes.Ensemble:
                                    ProcessEnsembleData(data.data, data.ensemble, data.dataFormat);
                                    break;
                                case ProcessDataTypes.GPS1:
                                    ProcessGps1Data(data.data);
                                    break;
                                case ProcessDataTypes.GPS2:
                                    ProcessGps2Data(data.data);
                                    break;
                                case ProcessDataTypes.NMEA1:
                                    ProcessNmea1Data(data.data);
                                    break;
                                case ProcessDataTypes.NMEA2:
                                    ProcessNmea2Data(data.data);
                                    break;
                                case ProcessDataTypes.CODEC:
                                    ProcessCodecData(data.data, data.ensemble, data.dataFormat);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    //Debug.WriteLine("AdcpConnection:ReceiveDataThread: End");

                }
                catch (ThreadAbortException)
                {
                    // Thread is aborted to stop processing
                    return;
                }
                catch (Exception e)
                {
                    log.Error("Error processing data.", e);
                    return;
                }
            }
        }

        /// <summary>
        /// Process the serial data.
        /// </summary>
        /// <param name="data">Data to process.</param>
        private void ProcessAdcpSerialData(byte[] data)
        {
            // Record the raw ADCP data
            WriteRawAdcpData(data);

            // Add the data to the codec
            _adcpCodec.AddIncomingData(data,
                                        _dataFormatOptions.IsBinaryFormat,
                                        _dataFormatOptions.IsDvlFormat,
                                        _dataFormatOptions.IsPd0Format,
                                        _dataFormatOptions.IsPd6_13Format,
                                        _dataFormatOptions.IsPd4_5Format);

            // Publish the data to all subscribers of raw data
            PublishReceiveData(data);
        }

        /// <summary>
        /// Process the ensemble data.
        /// </summary>
        /// <param name="rawData">Raw binary data.</param>
        /// <param name="ensemble">Ensemble data.</param>
        /// <param name="dataFormat">Original Data Format.</param>
        private void ProcessEnsembleData(byte[] rawData, DataSet.Ensemble ensemble, AdcpCodec.CodecEnum dataFormat)
        {
            // Publish the data to all subscribers of raw data
            PublishReceiveData(rawData);

            // Publish the ensemble data
            _adcpCodec_ProcessDataEvent(rawData, ensemble, dataFormat);
        }

        /// <summary>
        /// Process the GPS 1 data.
        /// </summary>
        /// <param name="data">Raw data.</param>
        private void ProcessGps1Data(byte[] data)
        {
            // Record the raw GPS 1 data
            WriteRawGps1Data(data);

            // Convert to string
            var nmeaData = System.Text.ASCIIEncoding.ASCII.GetString(data);

            // Add the data to the codec
            _adcpCodec.AddNmeaData(nmeaData);

            // Add NMEA data to buffer
            if (_pm.IsProjectSelected && IsRecording)
            {
                // Record Data
                _pm.SelectedProject.RecordGps1(data);
            }

            // Add data to the buffer
            _gps1Buffer.Enqueue(data);
        }

        /// <summary>
        /// Process the GPS 2 data.
        /// </summary>
        /// <param name="data">Raw data.</param>
        private void ProcessGps2Data(byte[] data)
        {
            // Record the raw GPS 2 data
            WriteRawGps2Data(data);

            // Convert to string
            var nmeaData = System.Text.ASCIIEncoding.ASCII.GetString(data);

            // Add the data to the codec
            _adcpCodec.AddNmeaData(nmeaData);

            if (_pm.IsProjectSelected && IsRecording)
            {
                // Record Data
                _pm.SelectedProject.RecordGps2(data);
            }

            // Add data to the buffer
            _gps2Buffer.Enqueue(data);
        }

        /// <summary>
        /// Process the NMEA 1 data.
        /// </summary>
        /// <param name="data">Raw data.</param>
        private void ProcessNmea1Data(byte[] data)
        {
            // Record the raw Nmea 1 data
            WriteRawNmea1Data(data);

            // Convert to string
            var nmeaData = System.Text.ASCIIEncoding.ASCII.GetString(data);

            // Add the data to the codec
            _adcpCodec.AddNmeaData(nmeaData);

            if (_pm.IsProjectSelected && IsRecording)
            {
                // Record Data
                _pm.SelectedProject.RecordNmea1(data);
            }

            // Add data to the buffer
            _nmea1Buffer.Enqueue(data);
        }

        /// <summary>
        /// Process the NMEA 2 data.
        /// </summary>
        /// <param name="data">Raw data.</param>
        private void ProcessNmea2Data(byte[] data)
        {
            // Record the raw Nmea 2 data
            WriteRawNmea2Data(data);

            // Convert to string
            var nmeaData = System.Text.ASCIIEncoding.ASCII.GetString(data);

            // Add the data to the codec
            _adcpCodec.AddNmeaData(nmeaData);

            if (_pm.IsProjectSelected && IsRecording)
            {
                // Record Data
                _pm.SelectedProject.RecordNmea2(data);
            }

            // Add data to the buffer
            _nmea2Buffer.Enqueue(data);
        }

        /// <summary>
        /// Process the data from the ADCP Codec.
        /// </summary>
        /// <param name="rawData">Raw binary data.</param>
        /// <param name="ensemble">Ensemble data.</param>
        /// <param name="origDataFormat">Original data format of the data.</param>
        private void ProcessCodecData(byte[] rawData, DataSet.Ensemble ensemble, AdcpCodec.CodecEnum origDataFormat)
        {
            // Copy the data
            var ens = ensemble.Clone();
            byte[] raw = new byte[rawData.Length];
            Buffer.BlockCopy(rawData, 0, raw, 0, rawData.Length);

            //await ReceiveEnsembleFromCodec(raw, ens, EnsembleSource.Serial, EnsembleType.Single);
            ReceiveEnsembleFromCodec(raw, ens, EnsembleSource.Serial, EnsembleType.Single, origDataFormat);
        }

        #endregion

        #endregion

        #region EventHandlers

        #region Serial Port

        /// <summary>
        /// Receive binary data from the ADCP serial port.
        /// Then pass the binary data to the codec to decode the
        /// data into ensembles.
        /// 
        /// The data could be binary or dvl data.
        /// The data will go to both codec and
        /// if the codec can process the data it will.
        /// </summary>
        /// <param name="data">Data to decode.</param>
        public void ReceiveAdcpSerialData(byte[] data)
        {
            // Create the data
            var pd = new ProcessData{type = ProcessDataTypes.AdcpSerial, data = data};

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
        }

        /// <summary>
        /// Publish the ensemble data.  This is usually used when importing data from a file.
        /// </summary>
        /// <param name="rawData">Raw ensemble binary data.</param>
        /// <param name="ensemble">Ensemble object.</param>
        public void PublishEnsembleData(byte[] rawData, DataSet.Ensemble ensemble)
        {
            // Create the data
            var pd = new ProcessData { type = ProcessDataTypes.Ensemble, data = rawData, ensemble = ensemble };

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }

        }

        /// <summary>
        /// Receive the raw serial data from the GPS 1 port.
        /// </summary>
        /// <param name="data">Data from the port.</param>
        void Gps1SerialPort_ReceiveRawSerialDataEvent(byte[] data)
        {
            // Create the data
            var pd = new ProcessData { type = ProcessDataTypes.GPS1, data = data };

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
        }

        /// <summary>
        /// Receive the raw serial data from the GPS 2 port.
        /// </summary>
        /// <param name="data">Data from the port.</param>
        void Gps2SerialPort_ReceiveRawSerialDataEvent(byte[] data)
        {
            // Create the data
            var pd = new ProcessData { type = ProcessDataTypes.GPS2, data = data };

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
        }

        /// <summary>
        /// Receive the raw serial data from the NMEA 1 port.
        /// </summary>
        /// <param name="data">Data from the port.</param>
        void Nmea1SerialPort_ReceiveRawSerialDataEvent(byte[] data)
        {
            // Create the data
            var pd = new ProcessData { type = ProcessDataTypes.NMEA1, data = data };

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
        }

        /// <summary>
        /// Receive the raw serial data from the NMEA 2 port.
        /// </summary>
        /// <param name="data">Data from the port.</param>
        void Nmea2SerialPort_ReceiveRawSerialDataEvent(byte[] data)
        {
            // Create the data
            var pd = new ProcessData { type = ProcessDataTypes.NMEA2, data = data };

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
        }



        #endregion

        #region Codec

        /// <summary>
        /// Receive decoded data from the codec.  This will be 
        /// the latest data decoded.  It will include the complete
        /// binary array of the data and the ensemble object.
        /// </summary>
        /// <param name="binaryEnsemble">Binary data of the ensemble.</param>
        /// <param name="ensemble">Ensemble object.</param>
        /// <param name="dataFormat">The original format the data came in.</param>
        void _adcpCodec_ProcessDataEvent(byte[] binaryEnsemble, DataSet.Ensemble ensemble, AdcpCodec.CodecEnum dataFormat)
        {
            // Create the data
            var pd = new ProcessData { type = ProcessDataTypes.CODEC, data = binaryEnsemble, ensemble = ensemble, dataFormat = dataFormat };

            // Queue the data
            _processDataQueue.Enqueue(pd);

            // Wakeup the thread
            if (!_eventWaitData.SafeWaitHandle.IsClosed)
            {
                _eventWaitData.Set();
            }
        }

        #endregion

        #region Download Progress

        /// <summary>
        /// Event handler when a file has completed being
        /// downloaded.
        /// </summary>
        /// <param name="fileName">File name of the completed download.</param>
        /// <param name="goodDownload">Flag set to determine if the download was good or bad.</param>
        private void On_DownloadCompleteEvent(string fileName, bool goodDownload)
        {
            if (DownloadCompleteEvent != null)
            {
                DownloadCompleteEvent(fileName, goodDownload);
            }
        }

        /// <summary>
        /// Progress of the downloading file.  This will give the number
        /// of bytes currently written to the file.
        /// </summary>
        /// <param name="fileName">File name of file in progress.</param>
        /// <param name="bytesWritten">Number of bytes written to file.</param>
        private void On_DownloadProgressEvent(string fileName, long bytesWritten)
        {
            if (DownloadProgressEvent != null)
            {
                DownloadProgressEvent(fileName, bytesWritten);
            }
        }

        /// <summary>
        /// Set the file size for the given file name.
        /// This will set the file size in the list of 
        /// download files.
        /// </summary>
        /// <param name="fileName">File Name.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        private void On_DownloadFileSizeEvent(string fileName, long fileSize)
        {
            if (DownloadFileSizeEvent != null)
            {
                DownloadFileSizeEvent(fileName, fileSize);
            }
        }

        #endregion

        #region Upload Progress

        /// <summary>
        /// Event handler when a file has completed being
        /// downloaded.
        /// </summary>
        /// <param name="fileName">File name of the completed download.</param>
        /// <param name="goodDownload">Flag set to determine if the download was good or bad.</param>
        private void On_UploadCompleteEvent(string fileName, bool goodDownload)
        {
            if (UploadCompleteEvent != null)
            {
                UploadCompleteEvent(fileName, goodDownload);
            }
        }

        /// <summary>
        /// Set the file size for the file uploading.
        /// </summary>
        /// <param name="fileName">File Name.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        private void On_UploadFileSizeEvent(string fileName, long fileSize)
        {
            if (UploadFileSizeEvent != null)
            {
                UploadFileSizeEvent(fileName, fileSize);
            }
        }

        /// <summary>
        /// Progress of the uploading file.  This will give the number
        /// of bytes currently written to the file.
        /// </summary>
        /// <param name="fileName">File name of file in progress.</param>
        /// <param name="bytesWritten">Number of bytes written to file.</param>
        private void On_UploadProgressEvent(string fileName, long bytesWritten)
        {
            if (UploadProgressEvent != null)
            {
                UploadProgressEvent(fileName, bytesWritten);
            }
        }

        #endregion

        #endregion
    }
}
