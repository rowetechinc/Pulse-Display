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
 * 08/16/2012      RC          2.13       Initial coding
 * 01/23/2013      RC          2.17       Reboot the ADCP after firmware has been uploaded.
 * 08/04/2014      RC          3.4.0      Removed the TerminalVM reference.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 08/12/2014      RC          4.0.0      Stop pinging and reboot when updating the firmware.
 * 07/06/2015      RC          4.1.3      Send FMCOPYS and FMCOPYB after uploading the firmware.
 * 09/24/2015      RC          4.2.0      Create a timer to update the terminal display to reduce refreshes.
 * 
 */

namespace RTI
{
    using System.Windows.Input;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using log4net;
    using System.Windows;
    using System.Threading;
    using Caliburn.Micro;
    using ReactiveUI;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Uplodate data to the ADCP through the serial port.
    /// </summary> 
    [Export]
    public class UpdateFirmwareViewModel : PulseViewModel
    {

        #region Variables

        // Setup logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// EventAggregator to handle passing events.
        /// </summary>
        private IEventAggregator _eventAggregator;

        /// <summary>
        /// Connection to the ADCP.
        /// </summary>
        private AdcpConnection _adcpConn;

        /// <summary>
        /// Timer to reduce the number of update calls the terminal window.
        /// </summary>
        private System.Timers.Timer _displayTimer;

        #endregion

        #region Properties

        ///// <summary>
        ///// ADCP Serial port.
        ///// </summary>
        //private AdcpSerialPort _adcpSerialPort;
        /// <summary>
        /// ADCP Serial port.
        /// </summary>
        public AdcpSerialPort AdcpSerialPort
        {
            get { return _adcpConn.AdcpSerialPort; }
        }

        #region Upload Data

        /// <summary>
        /// The current number of bytes written uploaded.
        /// </summary>
        private long _uploadFilePogress;
        /// <summary>
        /// The current number of bytes written uploaded.
        /// </summary>
        public long UploadFileProgress
        {
            get { return _uploadFilePogress; }
            set
            {
                _uploadFilePogress = value;
                this.NotifyOfPropertyChange(() => this.UploadFileProgress);
            }
        }

        /// <summary>
        /// The size of the file being uploaded in bytes.
        /// </summary>
        private long _uploadFileSize;
        /// <summary>
        /// The size of the file being uploaded in bytes.
        /// </summary>
        public long UploadFileSize
        {
            get { return _uploadFileSize; }
            set
            {
                _uploadFileSize = value;
                this.NotifyOfPropertyChange(() => this.UploadFileSize);
            }
        }

        #endregion

        #region ADCP Receive Buffer

        /// <summary>
        /// Display the receive buffer from the connected ADCP serial port.
        /// </summary>
        public string AdcpReceiveBuffer
        {
            get { return _adcpConn.ReceiveBufferString; }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to update to the firmware.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> UpdateFirmwareCommand { get; protected set; }

        /// <summary>
        /// Command to cancel the upload.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CancelUpdateFirmwareCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize values.
        /// </summary>
        public UpdateFirmwareViewModel()
            : base("UpdateFirmwareViewModel")
        {
            // Update the display
            _displayTimer = new System.Timers.Timer(500);
            _displayTimer.Elapsed += _displayTimer_Elapsed;
            _displayTimer.AutoReset = true;
            _displayTimer.Enabled = true;

            // Initialize values
            _adcpConn = IoC.Get<AdcpConnection>();
            _adcpConn.ReceiveDataEvent += new AdcpConnection.ReceiveDataEventHandler(_adcpConnection_ReceiveDataEvent);
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            // Subscribe to receive upload events
            SubscribeUploadEvents();

            // Command to get the file to update
            UpdateFirmwareCommand = ReactiveCommand.CreateAsyncTask(_ => OnUpdateFirmware());

            // Create a command to cancel updating the firmware
            CancelUpdateFirmwareCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => OnCancelUpdateFirmware()));
        }

        /// <summary>
        /// Shutdown the view.
        /// </summary>
        public override void Dispose()
        {
            // Unsubscribe
            _adcpConn.ReceiveDataEvent -= _adcpConnection_ReceiveDataEvent;

            // Unsubscribe
            UnsubscribeUploadEvents();
        }

        #region Update Display

        /// <summary>
        /// Reduce the number of times the display is updated.
        /// This will update the display based off the timer and not
        /// based off when data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _displayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);
        }

        #endregion

        #region Subscribe Upload Events

        /// <summary>
        /// Subscribe to the serial port to get the latest download progress.
        /// </summary>
        private void SubscribeUploadEvents()
        {
            // Subscribe to recevie upload events from the serial port
            if (_adcpConn != null)
            {
                _adcpConn.UploadProgressEvent += new AdcpConnection.UploadProgressEventHandler(On_UploadProgressEvent);
                _adcpConn.UploadCompleteEvent += new AdcpConnection.UploadCompleteEventHandler(On_UploadCompleteEvent);
                _adcpConn.UploadFileSizeEvent += new AdcpConnection.UploadFileSizeEventHandler(On_UploadFileSizeEvent);
            }
        }

        /// <summary>
        /// Unsubscribe from the serial port if the serial port is going to change.
        /// </summary>
        public void UnsubscribeUploadEvents()
        {
            // Unsubscribe
            if (_adcpConn != null)
            {
                _adcpConn.UploadProgressEvent -= On_UploadProgressEvent;
                _adcpConn.UploadCompleteEvent -= On_UploadCompleteEvent;
                _adcpConn.UploadFileSizeEvent -= On_UploadFileSizeEvent;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler when a file has completed being
        /// downloaded.
        /// </summary>
        /// <param name="fileName">File name of the completed download.</param>
        /// <param name="goodDownload">Flag set to determine if the download was good or bad.</param>
        private void On_UploadCompleteEvent(string fileName, bool goodDownload)
        {
            UploadFileProgress = UploadFileSize;
            _eventAggregator.PublishOnUIThread(new StatusEvent("Upload Complete.", MessageBoxImage.Information));
        }

        /// <summary>
        /// Set the file size for the file uploading.
        /// </summary>
        /// <param name="fileName">File Name.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        private void On_UploadFileSizeEvent(string fileName, long fileSize)
        {
            UploadFileSize = fileSize;
        }

        /// <summary>
        /// Progress of the uploading file.  This will give the number
        /// of bytes currently written to the file.
        /// </summary>
        /// <param name="fileName">File name of file in progress.</param>
        /// <param name="bytesWritten">Number of bytes written to file.</param>
        private void On_UploadProgressEvent(string fileName, long bytesWritten)
        {
            // Set the progress of uploading the current file
            UploadFileProgress = bytesWritten;
        }

        /// <summary>
        /// Event handler when receiving serial data.
        /// </summary>
        /// <param name="data">Data received from the serial port.</param>
        private void _adcpConnection_ReceiveDataEvent(byte[] data)
        {
            //this.NotifyOfPropertyChange(() => this.AdcpReceiveBuffer);

        }

        #endregion

        #region Commands

        #region Update Firmware Command

        /// <summary>
        /// Populate the list of available files to download.
        /// </summary>
        private async Task OnUpdateFirmware()
        {
            try
            {
                // Get the file
                System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                openFileDialog1.InitialDirectory = "";
                openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*|bin files (*.bin)|*.bin";//"txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 2;
                //openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Multiselect = true;

                UploadFileSize = 0;
                UploadFileProgress = 0;

                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Call the command to start the upload process async
                    //ExecuteUpdateFirmwareCommand.Execute(openFileDialog1.FileNames);
                    await Task.Run(() => _adcpConn.UpdateFirmware(openFileDialog1.FileNames));
                }
            }
            catch (AccessViolationException ae)
            {
                log.Error("Error trying to open firmware file", ae);
            }
            catch (Exception e)
            {
                log.Error("Error trying to open firmware file", e);
            }
        }

        /// <summary>
        /// Execute the upload process.  This should be called
        /// from the async command.
        /// </summary>
        /// <param name="fileName">File name to upload.</param>
        private void ExecuteUpdateFirmware(object fileName)
        {
            // Convert the object to a string array
            var files = fileName as string[];

            if (files != null)
            {
                // Stop the ADCP pinging if its pinging
                AdcpSerialPort.StopPinging();
                
                // Upload all the selected files
                foreach (var file in files)
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

        #endregion

        #region Cancel Update Firmware Command

        /// <summary>
        /// Cancel uploading the file to the ADCP.
        /// </summary>
        private void OnCancelUpdateFirmware()
        {
            AdcpSerialPort.CancelUpload();
        }

        #endregion

        #endregion

    }
}
