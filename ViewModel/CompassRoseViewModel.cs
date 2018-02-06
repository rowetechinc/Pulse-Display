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
 * 09/30/2014      RC          4.1.0       Initial coding
 * 10/02/2014      RC          4.1.0       Added averaging.
 * 12/04/2015      RC          4.4.0       Added try catch for averaging. Put a thread in to buffer the data.
 * 
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Display a compass rose with heading,
    /// pitch and roll.
    /// </summary>
    public class CompassRoseViewModel : PulseViewModel
    {

        #region Class and Enum

        /// <summary>
        /// Store the Heading, pitch and roll.
        /// </summary>
        private struct HPR
        {
            /// <summary>
            /// Heading.
            /// </summary>
            public float Heading;

            /// <summary>
            /// Pitch.
            /// </summary>
            public float Pitch;

            /// <summary>
            /// Roll.
            /// </summary>
            public float Roll;
        }

        #endregion

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default max length.
        /// </summary>
        private const int DEFAULT_MAX_LENGTH = 5;

        /// <summary>
        /// List of headings to average together.
        /// </summary>
        private List<float> _averageHeadingList;

        /// <summary>
        /// List of pitch to average together.
        /// </summary>
        private List<float> _averagePitchList;

        /// <summary>
        /// List of roll to average together.
        /// </summary>
        private List<float> _averageRollList;

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<HPR> _buffer;

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
        /// Limit how often the display updates.
        /// </summary>
        private int _displayCounter;

        /// <summary>
        /// Lock the data when averaging to prevent the data from be modified 
        /// while it is being averaged.
        /// </summary>
        private object _AverageLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Flag if the data should be averaged.
        /// </summary>
        private bool _IsAveraging;
        /// <summary>
        /// Flag if the data should be averaged.
        /// </summary>
        public bool IsAveraging
        {
            get { return _IsAveraging; }
            set
            {
                _IsAveraging = value;
                this.NotifyOfPropertyChange(() => this.IsAveraging);

                // Clear the data
                ClearIncomingData();
            }
        }

        /// <summary>
        /// Maximum size of the list to average.
        /// </summary>
        private int _MaxLength;
        /// <summary>
        /// Maximum size of the list to average.
        /// </summary>
        public int MaxLength
        {
            get { return _MaxLength; }
            set
            {
                _MaxLength = value;
                this.NotifyOfPropertyChange(() => this.MaxLength);
            }
        }

        /// <summary>
        /// Heading value in degrees.
        /// </summary>
        private float _Heading;
        /// <summary>
        /// Heading value in degrees.
        /// </summary>
        public float Heading
        {
            get { return _Heading; }
            set
            {
                _Heading = value;
                this.NotifyOfPropertyChange(() => this.Heading);
            }
        }

        /// <summary>
        /// Pitch value in degrees.
        /// </summary>
        private float _Pitch;
        /// <summary>
        /// Pitch value in degrees.
        /// </summary>
        public float Pitch
        {
            get { return _Pitch; }
            set
            {
                _Pitch = value;
                this.NotifyOfPropertyChange(() => this.Pitch);
            }
        }

        /// <summary>
        /// Roll value in degrees.
        /// </summary>
        private float _Roll;
        /// <summary>
        /// Roll value in degrees.
        /// </summary>
        public float Roll
        {
            get { return _Roll; }
            set
            {
                _Roll = value;
                this.NotifyOfPropertyChange(() => this.Roll);
            }
        }

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public CompassRoseViewModel(bool isAveraging = true, int maxLength = DEFAULT_MAX_LENGTH)
            : base("Compass Rose")
        {
            _averageHeadingList = new List<float>();
            _averagePitchList = new List<float>();
            _averageRollList = new List<float>();
            IsAveraging = isAveraging;
            MaxLength = maxLength;
            Heading = 0.0f;
            Pitch = 0.0f;
            Roll = 0.0f;

            //_isProcessingBuffer = false;
            _buffer = new ConcurrentQueue<HPR>();

            // Initialize the thread
            _continue = true;
            _displayCounter = 0;
            _eventWaitData = new EventWaitHandle(false, EventResetMode.AutoReset);
            _processDataThread = new Thread(ProcessDataThread);
            _processDataThread.Name = string.Format("Compass View:");
            _processDataThread.Start();
        }

        /// <summary>
        /// Dispose the view model.
        /// </summary>
        public override void Dispose()
        {

        }

        /// <summary>
        /// Add incoming data.  If averaging is turned on,
        /// the data will be added to the list and the average
        /// output.  If averaging is turned off, the value 
        /// will just be set.
        /// </summary>
        /// <param name="heading">Heading in degrees.</param>
        /// <param name="pitch">Pitch in degrees.</param>
        /// <param name="roll">Roll in degrees.</param>
        public void AddIncomingData(float heading, float pitch, float roll)
        {
            // If the data is not good, do not add anything
            if(heading == 0.0f && pitch == 0.0f && roll == 0.0f)
            {
                return;
            }

            HPR hpr = new HPR();
            hpr.Heading = heading; 
            hpr.Pitch = pitch;
            hpr.Roll = roll;

            // Buffer the data
            _buffer.Enqueue(hpr);

            if ((++_displayCounter % 5) == 0)
            {
                // Wake up the thread to process data
                _eventWaitData.Set();

                _displayCounter = 0;
            }
        }

        /// <summary>
        /// Execute the displaying of the data async.
        /// </summary>
        private void ProcessDataThread()
        {
            while (_continue)
            {
                // Wakeup the thread with a signal
                // Have a 2 second timeout to see if we need to shutdown the thread
                _eventWaitData.WaitOne(2000);

                while (!_buffer.IsEmpty)
                {
                    // Get the latest data from the buffer
                    HPR hpr;
                    if (_buffer.TryDequeue(out hpr))
                    {

                        if (_IsAveraging)
                        {
                            try
                            {
                                // Lock the data to prevent the list from being modified
                                // while the averaging is being done
                                lock (_AverageLock)
                                {
                                    AddHeading(hpr.Heading);
                                    AddPitch(hpr.Pitch);
                                    AddRoll(hpr.Roll);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("Error averaging in compass rose.", e);
                            }
                        }
                        else
                        {
                            Heading = hpr.Heading;
                            Pitch = hpr.Pitch;
                            Roll = hpr.Roll;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear the data.
        /// </summary>
        public void ClearIncomingData()
        {
            _averageHeadingList.Clear();
            _averagePitchList.Clear();
            _averageRollList.Clear();
        }

        #region Add Values

        /// <summary>
        /// Add the heading value.
        /// </summary>
        /// <param name="heading">Heading value in degrees.</param>
        private void AddHeading(float heading)
        {
            // Add the value to the list
            _averageHeadingList.Add(heading);

            // If the list is full, remove the first value
            if(_averageHeadingList.Count > _MaxLength)
            {
                _averageHeadingList.RemoveAt(0);
            }

            try
            {
                // Set the averaged heading
                Heading = _averageHeadingList.Average();
            }
            catch(Exception e)
            {
                log.Error("Error averaging heading in compass rose.", e);
            }
        }

        /// <summary>
        /// Add the pitch value.
        /// </summary>
        /// <param name="pitch">Pitch value in degrees.</param>
        private void AddPitch(float pitch)
        {
            // Add the value to the list
            _averagePitchList.Add(pitch);

            // If the list is full, remove the first value
            if (_averagePitchList.Count > _MaxLength)
            {
                _averagePitchList.RemoveAt(0);
            }

            try
            {
                // Set the averaged pitch
                Pitch = _averagePitchList.Average();
            }
            catch (Exception e)
            {
                log.Error("Error averaging pitch in compass rose.", e);
            }
        }

        /// <summary>
        /// Add the roll value.
        /// </summary>
        /// <param name="roll">Roll value in degrees.</param>
        private void AddRoll(float roll)
        {
            // Add the value to the list
            _averageRollList.Add(roll);

            // If the list is full, remove the first value
            if (_averageRollList.Count > _MaxLength)
            {
                _averageRollList.RemoveAt(0);
            }

            try
            { 
                // Set the averaged pitch
                Roll = _averageRollList.Average();
            }
            catch (Exception e)
            {
                log.Error("Error averaging Roll in compass rose.", e);
            }
        }

        #endregion

    }
}
