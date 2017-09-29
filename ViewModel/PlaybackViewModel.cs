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
 * 06/24/2013      RC          3.0.2      Initial coding
 * 07/25/2013      RC          3.0.5      Flush project when stop recording.
 * 07/26/2013      RC          3.0.6      Subscribe to know when EnsembleWriteEvent occurs to monitor file writing.
 * 08/15/2013      RC          3.0.7      Added StepEnsembleBackward and StepEnsembleForward.  Made PlayCommand start or stop playback.
 * 08/19/2013      RC          3.0.7      Changed how EnsembleWriteEvent handles the events to give live updates.
 * 09/26/2013      RC          3.1.4      Reset the recorded file size when a new project is selected.
 * 12/06/2013      RC          3.2.0      Added DisplayAllDataCommand.
 * 12/16/2013      RC          3.2.0      In IsRecordEnabled give warning is no project is selected.
 * 12/27/2013      RC          3.2.1      Added IsLoading to display a loading icon.
 * 07/14/2014      RC          3.4.0      Average the playback data.
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 09/09/2014      RC          4.0.3      Use SelectedPlayback in pulsemanager.
 * 09/12/2014      RC          4.0.3      Handle PlaybackEvent to know when a new playback settings is received.
 * 04/16/2015      RC          4.1.3      Only blink the recorder light if not importing.
 * 07/27/2015      RC          4.1.5      Set the playback name on a PlaybackEvent.
 * 11/16/2015      RC          4.3.0      Fixed bug in ChangePlaybackSpeed() setting the speed to high or low.
 * 12/03/2015      RC          4.4.0      Added recording to file to the record button.
 * 02/28/2017      RC          4.4.5      Fixed playback speed and divide by zero.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ReactiveUI;
    using Caliburn.Micro;
    using System.Timers;
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Playback the data in the project.   This will allow the
    /// user to move forwards and backwards within a project file.
    /// </summary>
    public class PlaybackViewModel : PulseViewModel, IHandle<ProjectEvent>, IHandle<EnsembleWriteEvent>, IHandle<PlaybackEvent>
    {

        #region Defaults

        #region Speed

        /// <summary>
        /// Default playback speed in milliseconds.
        /// </summary>
        public const int DEFAULT_PLAYBACK_SPEED = 1000;

        /// <summary>
        /// Number of milliseconds to increment to increase and decrease the playback speed.
        /// </summary>
        public const int PLAYBACK_INCREMENT = 125;

        /// <summary>
        /// Maximum playback speed in milliseconds.
        /// </summary>
        public const int MAX_PLAYBACK_SPEED = 125;

        /// <summary>
        /// Minimum playback speed in milliseconds.
        /// </summary>
        public const int MIN_PLAYBACK_SPEED = 2000;

        #endregion

        /// <summary>
        /// Default playback index.
        /// </summary>
        private const long DEFAULT_PLAYBACK_INDEX = 1;

        #endregion

        #region Variables

        /// <summary>
        /// Project manager.
        /// </summary>
        private PulseManager _pm;

        /// <summary>
        /// Event Aggregator.
        /// </summary>
        private IEventAggregator _events;

        /// <summary>
        /// Adcp Connection.
        /// </summary>
        private AdcpConnection _adcpConn;

        /// <summary>
        /// ViewModel to screen the data.
        /// </summary>
        private ScreenDataBaseViewModel _screenDataVM;

        /// <summary>
        /// ViewModel to average the data.
        /// </summary>
        private AveragingBaseViewModel _averagingVM;

        /// <summary>
        /// Timer used for playing back data.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Timer used for monitoring the recorder button.
        /// </summary>
        private Timer _recorderTimer;

        /// <summary>
        /// Current file size in bytes.
        /// </summary>
        private long _fileSize;

        /// <summary>
        /// Flag if processing.
        /// </summary>
        private bool _isProcessingBuffer;

        /// <summary>
        ///  Lock for _isProcessingBuffer flag.
        /// </summary>
        private object _isProcessingBufferLock = new object();

        /// <summary>
        /// Buffer the incoming data.
        /// </summary>
        private ConcurrentQueue<PlaybackArgs> _buffer;

        /// <summary>
        /// Lock for the playback index.
        /// </summary>
        private object _playbackIndexLock = new object();

        /// <summary>
        /// Project playback.
        /// </summary>
        private ProjectPlayback _projectPlayback;

        #endregion

        #region Properties

        #region IsAdmin

        /// <summary>
        /// Flag if the user is an Admin.
        /// </summary>
        public bool IsAdmin { get; set; }

        #endregion

        #region IsLooping

        /// <summary>
        /// Flag if the playback should be looping.
        /// </summary>
        public bool IsLooping { get; set; }

        #endregion

        #region Project

        /// <summary>
        /// Name of the project selected.
        /// </summary>
        private string _ProjectName;
        /// <summary>
        /// Name of the project selected.
        /// </summary>
        public string ProjectName
        {
            get { return _ProjectName; }
            set
            {
                _ProjectName = value;
                this.NotifyOfPropertyChange(() => this.ProjectName);
            }
        }

        #endregion

        #region Speed

        /// <summary>
        /// Playback speed in milliseconds.
        /// This is the time period to wait between each 
        /// ensemble.  1000ms = 1 second.
        /// </summary>
        private int _PlaybackSpeed;
        /// <summary>
        /// Playback speed in milliseconds.
        /// This is the time period to wait between each 
        /// ensemble.  1000ms = 1 second.
        /// </summary>
        public int PlaybackSpeed
        {
            get { return _PlaybackSpeed; }
            set
            {
                //if (value >= MAX_PLAYBACK_SPEED && value <= MIN_PLAYBACK_SPEED)
                //{
                    _PlaybackSpeed = value;
                    this.NotifyOfPropertyChange(() => this.PlaybackSpeed);
                //}
            }
        }

        /// <summary>
        /// Playback speed image.
        /// </summary>
        private string _PlaybackSpeedImage;
        /// <summary>
        /// Image to represent the speed
        /// of playback.  This will have 
        /// 4 speed values, representing
        /// min to max and everything between.
        /// </summary>
        public string PlaybackSpeedImage
        {
            get { return _PlaybackSpeedImage; }
            set
            {
                _PlaybackSpeedImage = value;
                this.NotifyOfPropertyChange(() => this.PlaybackSpeedImage);
            }
        }

        #endregion

        #region Playback Range

        /// <summary>
        /// Minimum range to show for the playback.
        /// </summary>
        private int _MinPlaybackRange;
        /// <summary>
        /// Minimum range to show for the playback.
        /// </summary>
        public int MinPlaybackRange
        {
            get { return _MinPlaybackRange; }
            set
            {
                _MinPlaybackRange = value;
                this.NotifyOfPropertyChange(() => this.MinPlaybackRange);
            }
        }

        /// <summary>
        /// Index to show for the playback.
        /// </summary>
        private long _PlaybackIndex;
        /// <summary>
        /// Index to show for the playback.
        /// </summary>
        public long PlaybackIndex
        {
            get { return _PlaybackIndex; }
            set
            {
                _PlaybackIndex = value;
                this.NotifyOfPropertyChange(() => this.PlaybackIndex);

                // Jump to the specificed ensemble
                JumpEnsemble(_PlaybackIndex);
            }
        }

        /// <summary>
        /// Total number of ensembles in the project.
        /// </summary>
        private long _TotalEnsembles;
        /// <summary>
        /// Total number of ensembles in the project.
        /// </summary>
        public long TotalEnsembles
        {
            get { return _TotalEnsembles; }
            set
            {
                _TotalEnsembles = value;
                this.NotifyOfPropertyChange(() => this.TotalEnsembles);
            }
        }

        #endregion

        #region Record

        /// <summary>
        /// String for the image to display for the
        /// record button.  The button can show a record
        /// on or off and a blink.  On is red, off is black.
        /// Blink is blue.
        /// </summary>
        private string _RecordImage;
        /// <summary>
        /// Record Button image.
        /// </summary>
        public string RecordImage
        {
            get { return _RecordImage; }
            set
            {
                _RecordImage = value;
                this.NotifyOfPropertyChange(() => this.RecordImage);
            }
        }


        /// <summary>
        /// Flag if currently recording property.
        /// </summary>
        private bool _IsRecordEnabled;
        /// <summary>
        /// Flag if currently recording property.
        /// </summary>
        public bool IsRecordEnabled
        {
            //get { return _adcpConn.IsRecording || _adcpConn.IsValidationTestRecording; }
            get { return _IsRecordEnabled; }
            set
            {
                if(value)
                {
                    if(_pm.IsProjectSelected)
                    {
                        // Start recording to project
                        _adcpConn.IsRecording = true;
                    }
                    else
                    {
                        // Start recording to file
                        _adcpConn.StartValidationTest(Pulse.Commons.DEFAULT_RECORD_DIR);
                    }
                }
                else
                {
                    // If stop recording
                    // flush the data to the file
                    // for the selected project
                    if (_pm.IsProjectSelected)
                    {
                        _adcpConn.IsRecording = false;
                        _pm.SelectedProject.Flush();
                    }
                    else if(_adcpConn.IsValidationTestRecording)
                    {
                        // Stop recording file
                        _adcpConn.StopValidationTest();
                    }
                }

                _IsRecordEnabled = value;
                this.NotifyOfPropertyChange(() => this.IsRecordEnabled);


                // Set the record image
                SetRecorderImage();
            }
        }

        /// <summary>
        /// Current file size recorded.
        /// </summary>
        private string _CurrentFileSize;
        /// <summary>
        /// Current file size recorded.
        /// </summary>
        public string CurrentFileSize
        {
            get { return _CurrentFileSize; }
            set
            {
                _CurrentFileSize = value;
                this.NotifyOfPropertyChange(() => this.CurrentFileSize);
            }
        }

        #endregion

        #region Play Icon

        /// <summary>
        /// Flag if the Play back is not playing.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return _timer.Enabled;
            }
        }

        #endregion

        #region IsLoading

        /// <summary>
        /// Loading Flag.
        /// </summary>
        private bool _IsLoading;
        /// <summary>
        /// Loading Flag.
        /// </summary>
        public bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                _IsLoading = value;
                this.NotifyOfPropertyChange(() => this.IsLoading);
            }
        }

        #endregion

        #region Project Playback

       

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command to start playback.
        /// </summary>
        public ReactiveCommand<object> RecordCommand { get; protected set; }

        /// <summary>
        /// Command to start playback.
        /// </summary>
        public ReactiveCommand<object> PlayCommand { get; protected set; }

        /// <summary>
        /// Command to move the ensemble count forward.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StepEnsembleFowardCommand { get; protected set; }

        /// <summary>
        /// Command to move the ensemble count backward.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> StepEnsembleBackwardCommand { get; protected set; }

        /// <summary>
        /// Command to increase the playback speed.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> IncreaseSpeedCommand { get; protected set; }

        /// <summary>
        /// Command to decrease the playback speed.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> DecreaseSpeedCommand { get; protected set; }

        /// <summary>
        /// Command to get the next ensemble to display.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> BlinkRecordImageCommand { get; protected set; }

        /// <summary>
        /// Command to display all the ensembles to the display.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> DisplayAllDataCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public PlaybackViewModel()
            :base("PlaybackViewModel")
        {

            // Get Project Manager
            _pm = IoC.Get<PulseManager>();
            _events = IoC.Get<IEventAggregator>();
            _events.Subscribe(this);
            _adcpConn = IoC.Get<AdcpConnection>();

            _buffer = new ConcurrentQueue<PlaybackArgs>();

            IsAdmin = Pulse.Commons.IsAdmin();

            // Get ScreenData VM
            _screenDataVM = IoC.Get<ScreenDataBaseViewModel>();

            // Get Averaging VM
            _averagingVM = IoC.Get<AveragingBaseViewModel>();

            // Set the record image
            SetRecorderImage();

            // Initialize the total number of ensembles
            CurrentFileSize = MathHelper.MemorySizeString(0);

            PlaybackSpeed = DEFAULT_PLAYBACK_SPEED;
            PlaybackIndex = DEFAULT_PLAYBACK_INDEX;
            SetPlaybackSpeedIndicatorImage();

            // Timer to move the slider
            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Interval = _PlaybackSpeed;
            _timer.Elapsed += new ElapsedEventHandler(On_TimerElapsed);
            IsLooping = false;
            IsLoading = false;
            _isProcessingBuffer = false;

            // Recorder Timer
            _recorderTimer = new Timer();
            _recorderTimer.AutoReset = true;
            _recorderTimer.Interval = 2000;
            _recorderTimer.Elapsed += new ElapsedEventHandler(On_recorderTimerElapsed);
            _recorderTimer.Start();

            // Command to set recording on or off
            RecordCommand = ReactiveCommand.Create();
            RecordCommand.Subscribe(_ => { IsRecordEnabled = !IsRecordEnabled; });

            // Command to begin playing back data
            PlayCommand = ReactiveCommand.Create();                          // Start the playback
            PlayCommand.Subscribe(_ => PlaybackCommandExecute());

            // Command to move the ensemble forward
            StepEnsembleFowardCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => StepEnsembleForward()));

            // Command to move the ensemble backward
            StepEnsembleBackwardCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => StepEnsembleBackward()));

            // Command to increase playback speed
            // Reduce the timer interval
            IncreaseSpeedCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => IncreasePlaybackSpeedCommandExec()));

            // Command to decrease playback speed
            // Reduce the timer interval
            DecreaseSpeedCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => DecreasePlaybackSpeedCommandExec()));

            // Blink Record image in background  
            BlinkRecordImageCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => BlinkRecordImage())); 

            // Command to display all the data in the project 
            DisplayAllDataCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => DisplayAllData()));
        }

        /// <summary>
        /// Shutdown the object.
        /// </summary>
        public override void Dispose()
        {
            // Unscribe from timer and stop it
            _timer.Elapsed -= On_TimerElapsed;
            StopTimer();
        }

        #region Record

        /// <summary>
        /// Set the recorder image based off 
        /// IsRecordEnabled set or not.
        /// </summary>
        private void SetRecorderImage()
        {
            // Set Image
            if (IsRecordEnabled)
            {
                RecordImage = Pulse.Commons.RECORD_IMAGE_ON;
            }
            else
            {
                RecordImage = Pulse.Commons.RECORD_IMAGE_OFF;
            }
        }

        #endregion

        #region Process Ensemble

        /// <summary>
        /// Set the ensemble based off the argument given.
        /// </summary>
        /// <param name="args">Arguments based off the ensemble to playback.</param>
        private void SetEnsemble(PlaybackArgs args)
        {
            _buffer.Enqueue(args);

            // Execute async
            if (!_isProcessingBuffer)
            {
                SetEnsembleExecute();
            }
        }

        /// <summary>
        /// Dequeue all the data from the buffer.  Then 
        /// </summary>
        private void SetEnsembleExecute()
        {
            lock (_isProcessingBufferLock)
            {
                _isProcessingBuffer = true;
            }

            while (_buffer.Count > 0)
            {
                PlaybackArgs args = null;
                if (_buffer.TryDequeue(out args))
                {
                    if (args != null)
                    {
                        // Set new index
                        _PlaybackIndex = args.Index;
                        this.NotifyOfPropertyChange(() => this.PlaybackIndex);

                        // Set new Total ensembles
                        _TotalEnsembles = args.TotalEnsembles;
                        this.NotifyOfPropertyChange(() => this.TotalEnsembles);

                        // Check if we hit the end of playback
                        if (_PlaybackIndex >= TotalEnsembles)
                        {
                            // Stop the playback
                            StopTimer();
                        }
                        else
                        {
                            // Display the ensemble
                            ProcessEnsemble(args.Ensemble, args.OrigDataFormat);
                        }
                    }
                }
            }

            lock (_isProcessingBufferLock)
            {
                _isProcessingBuffer = false;
            }
        }

        /// <summary>
        /// Process the incoming data.  This will screen and average the data.
        /// </summary>
        /// <param name="data">Data to display.</param>
        /// <param name="origDataFormat">Originial Format of the data.</param>
        private void ProcessEnsemble(DataSet.Ensemble data, AdcpCodec.CodecEnum origDataFormat)
        {
            // Distribute the dataset to all subscribers
            if (data != null)
            {
                // Publish the ensemble before it is screened and averaged
                _events.PublishOnBackgroundThread(new EnsembleRawEvent(data.Clone(), EnsembleSource.Playback, EnsembleType.Single, origDataFormat));

                // Make a copy of the ensemble to pass to all the views
                DataSet.Ensemble newEnsemble = data.Clone();

                // Vessel Mount Options
                VesselMountScreen(ref newEnsemble);

                // Screen the data
                _screenDataVM.ScreenData(ref newEnsemble, origDataFormat);

                // Average the data
                _averagingVM.AverageEnsemble(newEnsemble);

                // Publish the ensemble after screening and averging the data
                _events.PublishOnBackgroundThread(new EnsembleEvent(newEnsemble, EnsembleSource.Playback));
            }
        }

        /// <summary>
        /// Display all the data from the project.
        /// </summary>
        private void DisplayAllData()
        {
            // Stop playback if it is playing back
            StopTimer();
            lock (_playbackIndexLock)
            {
                PlaybackIndex = DEFAULT_PLAYBACK_INDEX;
            }

            if (_pm.IsPlaybackSelected)
            {
                IsLoading = true;

                // Get all the ensembles from the project
                Cache<long,DataSet.Ensemble> data = _pm.SelectedPlayback.GetAllEnsembles();
                AdcpCodec.CodecEnum origDataFormat = _pm.SelectedPlayback.GetOrigDataFormat();

                // Set new Total ensembles
                _TotalEnsembles = (long)data.Count();
                this.NotifyOfPropertyChange(() => this.TotalEnsembles);

                // Store the new screened data
                Cache<long, DataSet.Ensemble> screenData = new Cache<long, DataSet.Ensemble>((uint)data.Count());

                // Screen all the data
                for (int x = 0; x < data.Count(); x++)
                {
                    // Make a copy of the ensemble to pass to all the views
                    DataSet.Ensemble newEnsemble = data.IndexValue(x).Clone();

                    // Vessel Mount Options
                    VesselMountScreen(ref newEnsemble);

                    // Screen the data
                    _screenDataVM.ScreenData(ref newEnsemble, origDataFormat);

                    // Add the screened ensemble to the list
                    screenData.Add(data.IndexKey(x), newEnsemble);
                }

                // Publish all the ensembles
                _events.PublishOnBackgroundThread(new BulkEnsembleEvent(screenData, EnsembleSource.Playback));

                IsLoading = false;
            }
        }

        #endregion

        #region Timer

        /// <summary>
        /// Start or Stop the playback.
        /// </summary>
        private Task PlaybackCommandExecute()
        {
            // If the timer is enabled, stop the timer
            if (_timer.Enabled)
            {
                StopTimer();
            }
            else
            {
                // If the index is at the end
                // Make it start over
                lock (_playbackIndexLock)
                {
                    if (_PlaybackIndex == TotalEnsembles)
                    {
                        PlaybackIndex = DEFAULT_PLAYBACK_INDEX;
                    }
                }

                StartTimer();
            }

            return null;
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        private void StartTimer()
        {
            // Get the latest total
            if (_projectPlayback != null)
            {
                TotalEnsembles = _projectPlayback.GetNumberOfEnsembles();
            }

            _timer.Enabled = true;
            this.NotifyOfPropertyChange(() => this.IsPlaying);
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        private void StopTimer()
        {
            _timer.Enabled = false;
            this.NotifyOfPropertyChange(() => this.IsPlaying);
        }

        #endregion

        #region Step Ensemble

        /// <summary>
        /// When the timer goes off, increment the selected index.
        /// When the selectedindex is changed, it will ask for
        /// the dataset for the given index.  This will also move
        /// the slider.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <param name="e">No parameters used.</param>
        private void On_TimerElapsed(object sender, ElapsedEventArgs e)
        {
            StepEnsembleForward();
        }

        /// <summary>
        /// Step the Ensemble forward.
        /// </summary>
        private void StepEnsembleForward()
        {
            // Ensure there are ensembles to view.
            if (_pm.IsPlaybackSelected)
            {
                SetEnsemble(_pm.SelectedPlayback.StepForward());
            }
        }

        /// <summary>
        /// Step the Ensemble Backward.
        /// </summary>
        private void StepEnsembleBackward()
        {
            // Ensure there are ensembles to view.
            if (_pm.IsPlaybackSelected)
            {
                SetEnsemble(_pm.SelectedPlayback.StepBackward());
            }
        }

        /// <summary>
        /// Jump to a specific ensemble in the project..
        /// </summary>
        private void JumpEnsemble(long index)
        {
            // Ensure there are ensembles to view.
            if (_pm.IsPlaybackSelected)
            {
                SetEnsemble(_pm.SelectedPlayback.Jump(index));
            }
        }

        #endregion

        #region Blink

        /// <summary>
        /// Blink the record image.
        /// </summary>
        private void BlinkRecordImage()
        {
            // Make the record button blink
            // Blink the image
            RecordImage = Pulse.Commons.RECORD_IMAGE_BLINK;
            System.Threading.Thread.Sleep(200);
            SetRecorderImage();
        }

        #endregion

        #region Playback Speed

        /// <summary>
        /// Increase the playback speed.
        /// </summary>
        /// <returns></returns>
        private void IncreasePlaybackSpeedCommandExec()
        {
            ChangePlaybackSpeed(PlaybackSpeed, true);
        }

        /// <summary>
        /// Decrease the playback speed.
        /// </summary>
        /// <returns></returns>
        private void DecreasePlaybackSpeedCommandExec()
        {
            ChangePlaybackSpeed(PlaybackSpeed, false);
        }

        /// <summary>
        /// Change the Playback speed.
        /// </summary>
        /// <param name="curPlaybackSpeed">Current playback speed.</param>
        /// <param name="isIncrement">TRUE if increment the speed.</param>
        private void ChangePlaybackSpeed(int curPlaybackSpeed, bool isIncrement)
        {
            //Debug.WriteLine("PlaybackSpeed: " + _PlaybackSpeed);
            // Increment or decrement the Timer value
            if (isIncrement)
            {
                PlaybackSpeed = curPlaybackSpeed / 2;
            }
            else
            {
                PlaybackSpeed = curPlaybackSpeed * 2;
            }

            // To prevent divide by 0
            if (_PlaybackSpeed == 0)
            {
                _PlaybackSpeed = 1;
            }

            if (_PlaybackSpeed > 0)
            {
                // Set the new timer speed
                _timer.Interval = _PlaybackSpeed;

                //Debug.WriteLine("New PlaybackSpeed: " + _PlaybackSpeed);

                // Set the image
                SetPlaybackSpeedIndicatorImage();
            }
        }

        /// <summary>
        /// Set the playback speed indicator image
        /// based off the TimerInterval value.
        /// There are 4 possible values it can be, ranging
        /// from Max speed to Min speed.
        /// </summary>
        private void SetPlaybackSpeedIndicatorImage()
        {
            if (_PlaybackSpeed >= PLAYBACK_INCREMENT * 8)
                PlaybackSpeedImage = Pulse.Commons.INDICATOR_1;
            else if (_PlaybackSpeed >= PLAYBACK_INCREMENT * 4)
                PlaybackSpeedImage = Pulse.Commons.INDICATOR_2;
            else if (_PlaybackSpeed >= PLAYBACK_INCREMENT * 2)
                PlaybackSpeedImage = Pulse.Commons.INDICATOR_3;
            else if (_PlaybackSpeed >= PLAYBACK_INCREMENT * 1)
                PlaybackSpeedImage = Pulse.Commons.INDICATOR_4;
            else
                PlaybackSpeedImage = Pulse.Commons.INDICATOR_4;
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

        #region Recorder Timer Monitor

        /// <summary>
        /// When the timer goes off, check if the recorder is on and if the
        /// recorder needs to be updated.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <param name="e">No parameters used.</param>
        private void On_recorderTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_pm.IsProjectSelected)
            {
                if (_adcpConn.IsRecording != _IsRecordEnabled)
                {
                    _IsRecordEnabled = _adcpConn.IsRecording;
                    this.NotifyOfPropertyChange(() => this.IsRecordEnabled);

                    // Set the image
                    SetRecorderImage();
                }
            }
            else
            {
                if (_adcpConn.IsValidationTestRecording != _IsRecordEnabled)
                {
                    _IsRecordEnabled = _adcpConn.IsValidationTestRecording;
                    this.NotifyOfPropertyChange(() => this.IsRecordEnabled);

                    // Set the image
                    SetRecorderImage();
                }

                CurrentFileSize = MathHelper.MemorySizeString(_adcpConn.ValidationTestBytesWritten);
            }
        }

        #endregion

        #region Event Handler

        /// <summary>
        /// Handle the ProjectEvent event.
        /// </summary>
        /// <param name="message">Project event received.</param>
        public void Handle(ProjectEvent message)
        {
            if (_pm.IsProjectSelected)
            {
                // Create project playback and set selected playback
                _projectPlayback = new ProjectPlayback(_pm.SelectedProject);
                _pm.SelectedPlayback = _projectPlayback;

                TotalEnsembles = _projectPlayback.GetNumberOfEnsembles();
                ProjectName = _pm.SelectedProject.ProjectName;
            }
            else
            {
                ProjectName = "";
                TotalEnsembles = 0;
                _projectPlayback = null;
            }

            // Default project index
            lock (_playbackIndexLock)
            {
                PlaybackIndex = DEFAULT_PLAYBACK_INDEX;
            }

            // Initialize the total number of ensembles
            CurrentFileSize = MathHelper.MemorySizeString(0);

            // Stop the playback
            StopTimer();
        }

        /// <summary>
        /// Handle the ensemble write event.  This event is received
        /// when an ensemble has been written to a file.  This will determine
        /// which file.  
        /// 
        /// If it is the Project file, it will give the number of ensembles in the
        /// project.  This will set the total ensemble and make the record button blink.
        /// 
        /// If it is the Binary file, it will give the current file size of the binary file.
        /// This will also make the record button blink.
        /// </summary>
        /// <param name="message"></param>
        public void Handle(EnsembleWriteEvent message)
        {
            // Project file
            if (message.Loc == EnsembleWriteEvent.WriteLocation.Project)
            {
                // Set the total number of ensembles
                ++TotalEnsembles;
            }

            // Binary file
            if (message.Loc == EnsembleWriteEvent.WriteLocation.Binary)
            {
                // Set the current file size
                CurrentFileSize = MathHelper.MemorySizeString(_fileSize += message.Count);
            }

            // Blink the record image
            //BlinkRecordImageCommand.Execute(null);
            if (!_adcpConn.IsImporting)
            {
                Task.Run(() => BlinkRecordImage());
            }
        }


        /// <summary>
        /// Handle the PlaybackEvent event.
        /// </summary>
        /// <param name="message">Playback event received.</param>
        public void Handle(PlaybackEvent message)
        {
            // Get the latest information about the selected playback
            if (_pm.IsPlaybackSelected)
            {
                // Get the file name
                ProjectName = message.SelectedPlayback.Name;

                // Move the ensemble
                JumpEnsemble(1);
            }
        }
        #endregion
    }
}
