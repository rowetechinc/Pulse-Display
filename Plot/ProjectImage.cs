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
 * 01/04/2012      RC          1.11       Initial coding
 * 01/10/2012      RC          1.12       Added queue and thread.  
 *                                         Improve performance in ProjectImage.  Dispose of graphics, images and brushes.
 *                                         Stop thread on shutdown.
 * 03/11/2013      RC          2.18       Improved the performance of GetBrush().
 * 06/28/2013      RC          2.19       Replaced Shutdown() with IDisposable.
 * 08/07/2013      RC          3.0.7      In GenerateImage() remove the old image before creating the new file.
 * 08/16/2013      RC          3.0.7      Added Bottom Track Line and screen the data before creating the image.
 * 
 */

using System.Windows.Media;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System;
using log4net;
using System.Collections.Generic;
using System.Threading;

namespace RTI
{
    /// <summary>
    /// Create an image that will represent this
    /// project.  The image is created by taking the
    /// velocity data and creating a plot of the velocity
    /// for each bin.  
    /// This will create small images in batches and then
    /// combine them.  This will prevent a large image from 
    /// being held in memory if the project is large.
    /// </summary>
    public class ProjectImage: IDisposable
    {
        #region Variables

        /// <summary>
        /// Setup logger to report errors.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default color to use when no color or a
        /// bad value is given.
        /// </summary>
        private System.Drawing.Color DEFAULT_EMPTY_COLOR = System.Drawing.Color.FromKnownColor(KnownColor.Black);
        //private SolidColorBrush DEFAULT_EMPTY_COLOR = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Default color map to use on plot.
        /// Winter is dark blue for Min and Light Green for Max.
        /// </summary>
        private ColormapBrush.ColormapBrushEnum DEFAULT_COLORMAP = ColormapBrush.ColormapBrushEnum.Winter;

        /// <summary>
        /// Default minimum velocity in meters per second (m/s).
        /// </summary>
        private const double DEFAULT_MIN_VELOCITY = 0;

        /// <summary>
        /// Default maximum velocity in meters per second (m/s).
        /// </summary>
        private const double DEFAULT_MAX_VELOCITY = 2.0;

        /// <summary>
        /// Rectangle size in pixels.  A rectangle
        /// is one box containing the color that 
        /// is displayed as the plot.
        /// </summary>
        private const int RECT_SIZE = 5;

        /// <summary>
        /// Color scheme for the plot.
        /// This will be a range of color
        /// from minimum velocity to maximum velocity.
        /// </summary>
        private ColormapBrush _colormap;

        /// <summary>
        /// Codec to retrieve ensembles from the
        /// database.
        /// </summary>
        private AdcpDatabaseCodec _adcpDbCodec;

        /// <summary>
        /// Number of ensembles in the 
        /// project.  The overall size of the
        /// image will be determined based off
        /// this value. 
        /// Number of columns.
        /// </summary>
        private int _numEnsembles;

        /// <summary>
        /// When trying to remove bottom track velocity data,
        /// previous values will be stored to ensure ship
        /// speed can be removed.  This represents 
        /// the last good Bottom Track East Velocity.
        /// </summary>
        private float _prevGoodBtEast;

        /// <summary>
        /// When trying to remove bottom track velocity data,
        /// previous values will be stored to ensure ship
        /// speed can be removed.  This represents 
        /// the last good Bottom Track North Velocity.
        /// </summary>
        private float _prevGoodBtNorth;

        /// <summary>
        /// When trying to remove bottom track velocity data,
        /// previous values will be stored to ensure ship
        /// speed can be removed.  This represents 
        /// the last good Bottom Track Vertical Velocity.
        /// </summary>
        private float _prevGoodBtVert;

        /// <summary>
        /// Previous Good Bottom Track East velocity.
        /// </summary>
        private float _prevBtEast;

        /// <summary>
        /// Previous Good Bottom Track North velocity.
        /// </summary>
        private float _prevBtNorth;

        /// <summary>
        /// Previous Good Bottom Track Vertical velocity.
        /// </summary>
        private float _prevBtVert;

        /// <summary>
        /// Queue to hold all incoming data.
        /// This queue holds all the byte arrays
        /// received.
        /// </summary>
        private Queue<DataSet.Ensemble> _incomingDataQueue;

        /// <summary>
        /// Lock for the buffer.
        /// </summary>
        private readonly object _bufferLock = new object();

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

        #region Properties

        /// <summary>
        /// Project to get ensembles from and
        /// create the image for.
        /// </summary>
        public Project SelectedProject { get; set; }

        /// <summary>
        /// Minimum velocity set by user.
        /// This will represent the minimum 
        /// color in the color scheme.
        /// Meters per second (m/s).
        /// </summary>
        public double MinVelocity { get; set; }

        /// <summary>
        /// Maximum velocity set by user.
        /// This will represent the maximum
        /// color in the color scheme.
        /// Meters per second (m/s).
        /// </summary>
        public double MaxVelocity { get; set; } 

        #endregion

        #endregion

        /// <summary>
        /// Create a project image that
        /// will subscribe to receive events of
        /// the latest data to record.  It will 
        /// then add the data to the project image.
        /// </summary>
        /// <param name="prj">Project to add image to.</param>
        /// <param name="minVelocity">Minimum velocity.</param>
        /// <param name="maxVelocity">Maximum velocity.</param>
        public ProjectImage(Project prj, double minVelocity, double maxVelocity)
        {
            // Initalize values
            InitializeValues();

            SelectedProject = prj;
            MinVelocity = minVelocity;
            MaxVelocity = maxVelocity;
            _prevBtEast = 0.0f;
            _prevBtNorth = 0.0f;
            _prevBtVert = 0.0f;

            _incomingDataQueue = new Queue<DataSet.Ensemble>();

            // Subscribe to receive datasets that need to be recorded
            //CurrentDataSetManager.Instance.ReceiveCurrentDataset += new CurrentDataSetManager.CurrentDatasetEventHandler(On_ReceiveCurrentDataset);

            // Initialize the thread
            _continue = true;
            _eventWaitData = new EventWaitHandle(false, EventResetMode.AutoReset);
            _processDataThread = new Thread(ProcessDataThread);
            _processDataThread.Name = "Project Image: " + prj.ProjectName;
            _processDataThread.Start();
        }

        /// <summary>
        /// Shutdown this object.
        /// </summary>
        public void Dispose()
        {
            StopThread();
        }

        /// <summary>
        /// Generate an image of the project
        /// based off the velocity data.
        /// </summary>
        /// <param name="prj">Project to create the image.</param>
        /// <param name="maxVelocity">Maximum velocity for the image.</param>
        /// <param name="minVelocity">Minimum velocity for the image.</param>
        public void GenerateImage(Project prj, double minVelocity, double maxVelocity)
        {
            // Codec to read the database
            _adcpDbCodec = new AdcpDatabaseCodec();

            // Set the project
            SelectedProject = prj;

            // Set the min and max velocity
            MinVelocity = minVelocity;
            MaxVelocity = maxVelocity;

            // Verify the project is good
            if (SelectedProject != null)
            {
                // Get the number of ensembles
                _numEnsembles = GetNumberOfEnsembles(prj);

                // Verfiy there is data in the project
                if (_numEnsembles > 0)
                {
                    // Remove the old file
                    if (File.Exists(SelectedProject.GetProjectImagePath()))
                    {
                        try
                        {
                            File.Delete(SelectedProject.GetProjectImagePath());
                        }
                        catch (Exception e)
                        {
                            log.Error("Error Deleting Existing Image", e);
                        }
                    }

                    // Produce the image based off the
                    // set project.
                    ProduceImage();
                }
            }
        }

        /// <summary>
        /// Add an ensemble to the image.
        /// This will add to the existing
        /// image the new data.
        /// </summary>
        /// <param name="ensemble">Ensemble containing the data.</param>
        public void AddEnsemble(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                // Add the ensemble to the queue
                _incomingDataQueue.Enqueue(ensemble);

                // Wakeup Queue
                _eventWaitData.Set();
            }
        }

        #region Methods
        
        /// <summary>
        /// Initialize the values.
        /// </summary>
        private void InitializeValues()
        {
            // Set values
            SelectedProject = null;

            // Set previous BT good values
            _prevGoodBtEast = DataSet.Ensemble.BAD_VELOCITY;
            _prevGoodBtNorth = DataSet.Ensemble.BAD_VELOCITY;
            _prevGoodBtVert = DataSet.Ensemble.BAD_VELOCITY;

            // Initialize the value
            _numEnsembles = 0;
            MinVelocity = DEFAULT_MIN_VELOCITY;
            MaxVelocity = DEFAULT_MAX_VELOCITY;

            // Set the color map to use
            _colormap = new ColormapBrush();
            _colormap.ColormapBrushType = DEFAULT_COLORMAP;
        }

        /// <summary>
        /// Get the number of ensembles in this project.
        /// </summary>
        /// <param name="prj">Project to check for number of ensembles.</param>
        /// <returns>Number of ensembles in the project.</returns>
        private int GetNumberOfEnsembles(Project prj)
        {
            return AdcpDatabaseCodec.GetNumberOfEnsembles(prj);
        }

        /// <summary>
        /// Get the ensemble from the project.
        /// </summary>
        /// <param name="prj">Project containing the ensemble.</param>
        /// <param name="index">Index for the ensemble.</param>
        /// <returns>Ensemble for the given index within the given project.</returns>
        private DataSet.Ensemble GetEnsemble(Project prj, int index)
        {
            return _adcpDbCodec.QueryForDataSet(prj, index);
        }

        /// <summary>
        /// Get the ensembles from the project and add them
        /// to the image.  Each ensemble will create a column 
        /// of data.
        /// </summary>
        private void ProduceImage()
        {
            // Go through all the ensembles in the project
            for (int ensIndex = 0; ensIndex < _numEnsembles; ensIndex++)
            {
                // Get the ensemble
                DataSet.Ensemble ensemble = GetEnsemble(SelectedProject, ensIndex);

                // Screen the ensemble
                ScreenEnsemble(ref ensemble);

                if (ensemble != null)
                {
                    // Combine with the project image
                    CombineWithProjectImage(CreateBitmapFromEnsemble(ensemble));
                }
            }
        }

        /// <summary>
        /// Create a bitmap based off the ensemble given.
        /// This will verify the data exist to create the 
        /// bitmap.  It will then generate an array of velocity
        /// vectors from the ensemble.  The velocity vector array
        /// will be used to create the bitmap image.
        /// </summary>
        /// <param name="ensemble">Ensemble to create the bitmap image.</param>
        /// <returns>Bitmap image of velocity data.</returns>
        private Bitmap CreateBitmapFromEnsemble(DataSet.Ensemble ensemble)
        {
            // Create a bitmap for the ensemble and add to the queue
            if (ensemble != null)
            {
                if(ensemble.IsEarthVelocityAvail)
                {
                    if(ensemble.EarthVelocityData.IsVelocityVectorAvail)
                    {
                        // Add the VelocityVector to the image
                        return CreateImage(ensemble);
                    }
                }

                //// Ensure the correct dataset exist
                //if (ensemble.IsEnsembleAvail && ensemble.IsAncillaryAvail && ensemble.IsEarthVelocityAvail)
                //{
                //    //// Set bottom track data
                //    //if (ensemble.IsBottomTrackAvail)
                //    //{
                //    //    SetBottomTrackValues(ensemble);
                //    //}

                //    //// Set GPS Speed value if its available
                //    //double gpsSpeed = DataSet.Ensemble.BAD_VELOCITY;
                //    //if (ensemble.IsNmeaAvail)
                //    //{
                //    //    if (ensemble.NmeaData.IsGpvtgAvail())
                //    //    {
                //    //        if (ensemble.NmeaData.GPVTG.Speed.Value != DotSpatial.Positioning.Speed.Invalid.Value)
                //    //        {
                //    //            gpsSpeed = ensemble.NmeaData.GPVTG.Speed.Value;
                //    //        }
                //    //    }
                //    //}

                //    float btEast = DataSet.Ensemble.BAD_VELOCITY;
                //    float btNorth = DataSet.Ensemble.BAD_VELOCITY;
                //    float btVert = DataSet.Ensemble.BAD_VELOCITY;
                //    double gpsSpeed = DataSet.Ensemble.BAD_VELOCITY;

                //    // If available, get Bottom Track velocities
                //    if (ensemble.IsBottomTrackAvail)
                //    {
                //        btEast = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_0_INDEX];
                //        btNorth = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_1_INDEX];
                //        btVert = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_2_INDEX];
                //    }

                //    // If available, get a GPS speed
                //    if (ensemble.IsNmeaAvail)
                //    {
                //        if (ensemble.NmeaData.IsGpvtgAvail())
                //        {
                //            if (ensemble.NmeaData.GPVTG.Speed.Value != DotSpatial.Positioning.Speed.Invalid.Value)
                //            {
                //                gpsSpeed = ensemble.NmeaData.GPVTG.Speed.Value;
                //            }
                //            else
                //            {
                //                // Set GPS Speed to bad velocity
                //                gpsSpeed = DataSet.Ensemble.BAD_VELOCITY;
                //            }
                //        }
                //    }

                //    // If the Bottom Track data is good, store it
                //    // and use it.
                //    if (btEast != DataSet.Ensemble.BAD_VELOCITY &&
                //        btNorth != DataSet.Ensemble.BAD_VELOCITY &&
                //        btVert != DataSet.Ensemble.BAD_VELOCITY)
                //    {
                //        _prevGoodBtEast = btEast;
                //        _prevGoodBtNorth = btNorth;
                //        _prevGoodBtVert = btVert;
                //    }
                //    // Bottom Track data is bad, so try and use GPS speed
                //    else
                //    {
                //        // Check if GPS is good, if it is not, then set the bottom track
                //        // velocities to the previous, and use them.
                //        if (gpsSpeed == DataSet.Ensemble.BAD_VELOCITY)
                //        {
                //            btEast = _prevGoodBtEast;
                //            btNorth = _prevGoodBtNorth;
                //            btVert = _prevGoodBtVert;
                //        }
                //    }

                //    // Get the VelocityVector array for the ensemble
                //    DataSet.VelocityVector[] vv = ensemble.EarthVelocityData.GetVelocityVectors(btEast, btNorth, btVert, gpsSpeed);

                //    // Add the VelocityVector to the image
                //    return CreateImage(vv);
                //}
            }
            
            // If the ensemble was bad, return an empty bitmap
            return new Bitmap(1, 1);
        }
        
        /// <summary>
        /// Create an image based off the VelocityVector 
        /// array given. This will generate a column of
        /// rectangles.  Each rectangle will be filled with
        /// a color based off the magnitude.
        /// </summary>
        /// <param name="ensemble">Ensemble for each bin.</param>
        /// <returns>Bitmap of rectangles.</returns>
        private Bitmap CreateImage(DataSet.Ensemble ensemble)
        {
            DataSet.VelocityVector[] vv = ensemble.EarthVelocityData.VelocityVectors;

            // Get the Bottom Track Range
            // Then determine which bin the range is in
            double btRange = DataSet.Ensemble.BAD_RANGE;
            if(ensemble.IsBottomTrackAvail)
            {
                btRange = ensemble.BottomTrackData.GetAverageRange();
            }

            double btBin = 1;
            if (ensemble.IsAncillaryAvail)
            {
                btBin = btRange / ensemble.AncillaryData.BinSize;
            }

            int width = RECT_SIZE;
            int height = RECT_SIZE * vv.Length;

            // Bottom Track line color
            SolidBrush btBrush = new SolidBrush(System.Drawing.Color.White);

            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics gfx = Graphics.FromImage(bitmap))
            {
                for (int bin = 0; bin < vv.Length; bin++)
                {
                    // Get Color
                    using (SolidBrush brush = new SolidBrush(GenerateColor(vv[bin].Magnitude)))
                    {

                        // Draw Rectangle
                        gfx.FillRectangle(
                            brush,                  // Color
                            0,                      // X
                            bin * RECT_SIZE,        // Y
                            RECT_SIZE,              // Width
                            RECT_SIZE);             // Height

                        // Draw Bottom Track Line
                        // Only draw the line if the range is greater than 0
                        if (btRange > 0)
                        {
                            gfx.FillRectangle(
                                btBrush,
                                0,
                                (int)Math.Round(btBin * RECT_SIZE),
                                RECT_SIZE,
                                (int)Math.Round(RECT_SIZE / 2.0));
                        }

                        // Save results
                        gfx.Save();
                    }
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Combine the existing project image with the image given.
        /// If a project image does not exist, then make this given
        /// image the project image.
        /// </summary>
        /// <param name="bitmap">Bitmap to combine with Project image.</param>
        private void CombineWithProjectImage(Bitmap bitmap)
        {
            try
            {
                // Ensure a project is set
                if (SelectedProject != null)
                {
                    // Generate image filename
                    string projectImageName = SelectedProject.GetProjectImagePath();

                    // Check if the project image exist
                    if (File.Exists(projectImageName))
                    {
                        // Get the specs of the project image
                        MemoryStream ms = new MemoryStream(File.ReadAllBytes(projectImageName));
                        Image projectImage = Image.FromStream(ms);
                        Graphics projectGraphics = Graphics.FromImage(projectImage);

                        // Create a new image to store both images
                        using (Bitmap newProjectImage = new Bitmap(projectImage.Width + bitmap.Width, Math.Max(projectImage.Height, bitmap.Height)))
                        {
                            // Add the new image to the image stored
                            using (Graphics newProjectGraphics = Graphics.FromImage(newProjectImage))
                            {
                                //newProjectGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                newProjectGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                                newProjectGraphics.DrawImage(projectImage, 0, 0);
                                newProjectGraphics.DrawImage(bitmap, projectImage.Width, 0);
                                newProjectGraphics.Save();
                            }

                            // Save the image
                            try
                            {
                                newProjectImage.Save(projectImageName, ImageFormat.Png);

                                // Send an event that the image was updated
                                // This is to prevent the file being opened while the image is being saved
                                if (ImageUpdatedEvent != null)
                                {
                                    ImageUpdatedEvent();
                                }
                            }
                            catch (Exception e)
                            {
                                // Do nothing
                                log.Warn("Error creating Project Image", e);
                            }
                            finally
                            {
                                // Dispose of graphics and images
                                projectGraphics.Dispose();
                                projectImage.Dispose();
                                bitmap.Dispose();
                            }
                        }
                    }
                    else
                    {
                        // File did not exist so
                        // we do not need to combine and just
                        // save the image
                        try
                        {
                            bitmap.Save(projectImageName, ImageFormat.Png);
                        }
                        catch (Exception e)
                        {
                            // Do nothing
                            log.Warn("Error creating Project Image", e);
                        }
                        finally
                        {
                            // Dispose bitmap 
                            bitmap.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Unknown Error Creating Project Image.", e);
            }
        }

        /// <summary>
        /// Set the Bottom Track values if they are good.
        /// This is to keep track of the last good Bottom Track
        /// data so it can be used to remove the Bottom Track speed
        /// from the velocity data.
        /// </summary>
        /// <param name="ensemble">Ensemble containing Bottom Track data.</param>
        private void SetBottomTrackValues(DataSet.Ensemble ensemble)
        {
            // Check that all the velocity values are good
            if (ensemble.BottomTrackData.IsEarthVelocityGood())
            {
                _prevGoodBtEast = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                _prevGoodBtNorth = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                _prevGoodBtVert = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
            }
        }

        #endregion

        #region Screen Data

        private void ScreenEnsemble(ref DataSet.Ensemble ensemble)
        {
            // Mark Bad Below Bottom
            ScreenData.ScreenMarkBadBelowBottom.Screen(ref ensemble);

            // Remove Ship Speed
            ScreenData.RemoveShipSpeed.RemoveVelocity(ref ensemble, _prevBtEast, _prevBtNorth, _prevBtVert, true, true);

            // Record the Bottom for previous values
            SetPreviousBottomTrackVelocity(ensemble);

            // Record the GPS for previous values
            SetPreviousNmea(ensemble);
        }

        #region Remove Ship Speed

        /// <summary>
        /// Record the previous ensembles so when trying to remove the ship speed
        /// if the current Bottom Track values are not good, we can use the previous
        /// values to get close.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the Bottom Track data.</param>
        private void SetPreviousBottomTrackVelocity(DataSet.Ensemble ensemble)
        {
            if (ensemble != null)
            {
                // Check that Bottom Track exist
                if (ensemble.IsBottomTrackAvail)
                {
                    // Check that the values are good
                    if (ensemble.BottomTrackData.IsEarthVelocityGood())
                    {
                        // Check that it is not a 3 beam solution
                        // All the Beam values should be good
                        if (ensemble.BottomTrackData.IsBeamVelocityGood())
                        {
                            // Ensure at least a 4 beam system
                            if (ensemble.BottomTrackData.NumBeams >= DataSet.Ensemble.DEFAULT_NUM_BEAMS_BEAM)
                            {
                                _prevBtEast = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                                _prevBtNorth = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                                _prevBtVert = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
                            }
                            else
                            {
                                _prevBtEast = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                            }
                        }
                    }
                }
            }
        }

         /// <summary>
        /// Record the previous ensembles so when trying to remove the ship speed
        /// if the current Bottom Track values are not good, we can use the previous
        /// values to get close.
        /// </summary>
        /// <param name="ensemble">Ensemble to get the Bottom Track data.</param>
        private void SetPreviousNmea(DataSet.Ensemble ensemble)
        {

        }

        #endregion

        #endregion

        #region Thread

        /// <summary>
        /// Thread to process all the incoming data
        /// in the queue.
        /// </summary>
        private void ProcessDataThread()
        {
            while (_continue)
            {
                // Block until awoken when data is received
                _eventWaitData.WaitOne();

                // If wakeup was called to kill thread
                if (!_continue)
                {
                    return;
                }

                // Get all the data from the queue and add to the image
                while (_incomingDataQueue.Count > 0)
                {
                    // Kill Thread if still processing and shutdown called
                    if (!_continue)
                    {
                        return;
                    }

                    // Get the data from the queue
                    DataSet.Ensemble ensemble = _incomingDataQueue.Dequeue();

                    // Add the data to the project image
                    //AddEnsemble(ensemble);
                    // Create image from ensemble and
                    // combine with the project image
                    CombineWithProjectImage(CreateBitmapFromEnsemble(ensemble));
                }
            }
        }

        /// <summary>
        /// Stop the thread.
        /// </summary>
        private void StopThread()
        {
            _continue = false;

            // Wake up the thread to stop thread
            _eventWaitData.Set();
        }

        #endregion

        #region Plot Colors

        /// <summary>
        /// Create a brush based off the value given.
        /// The value will be based against the min and max velocity value.
        /// </summary>
        /// <param name="value">Value to convert to a color brush.</param>
        /// <returns>Color brush with the color based off the value, min and max velocity.</returns>
        private System.Drawing.Color GenerateColor(double value)
        {
            // Bad Values get an empty color
            if (value == DataSet.Ensemble.BAD_VELOCITY || value == 0)
            {
                return DEFAULT_EMPTY_COLOR;
            }

            return GetBrush(value, MinVelocity, MaxVelocity);
        }

        /// <summary>
        /// Create a brush based off the value given and a min and max value.
        /// The value will be based against the min and max value given.
        /// </summary>
        /// <param name="value">Value to generate a color.</param>
        /// <param name="valueMin">Min value.</param>
        /// <param name="valueMax">Max Value.</param>
        /// <returns>Color brush based off the value, min and max.</returns>
        private System.Drawing.Color GetBrush(double value, double valueMin, double valueMax)
        {
            // Get the color
            SolidColorBrush brush = _colormap.GetColormapColor(value, valueMin, valueMax);

            // Convert to System.Drawing.Color
            System.Drawing.Color color = System.Drawing.Color.FromArgb(brush.Color.A,
                                                                        brush.Color.R,
                                                                        brush.Color.G,
                                                                        brush.Color.B);

            return color;
        }

        #endregion

        #region Event Handler

        /// <summary>
        /// Receive the latest dataset to record.
        /// Add the plot data to the project image.
        /// </summary>
        /// <param name="ensemble">Ensemble to get plot data.</param>
        private void On_ReceiveCurrentDataset(DataSet.Ensemble ensemble)
        {
            // Add the ensemble to the queue
            AddEnsemble(ensemble);
        }

        #endregion

        #region Events

        /// <summary>
        /// Need a way to tell the application that the image has
        /// been updated.  This will also prevent the image being
        /// opened while the image is being modfied here.  Common
        /// error message seen if trying to open or modifiy the image
        /// while another part of the app is trying to open the file:
        /// A first chance exception of type 'System.Runtime.InteropServices.ExternalException' occurred in System.Drawing.dll
        /// Event To subscribe to.  This gives the paramater
        /// that will be passed when subscribing to the event.
        /// </summary>
        public delegate void ImageUpdatedEventHandler();

        /// <summary>
        /// Subscribe to this event.  This will hold all subscribers.
        /// 
        /// To subscribe:
        /// _projectImage.ImageUpdatedEvent += new _projectImage.ImageUpdatedEventHandler(method to call);
        /// 
        /// To Unsubscribe:
        /// _projectImage.ImageUpdatedEvent -= (method to call)
        /// </summary>
        public event ImageUpdatedEventHandler ImageUpdatedEvent;

        #endregion

    }
}