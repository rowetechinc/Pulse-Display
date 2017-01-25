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
 * 08/02/2012      RC          2.13       Initial coding
 * 08/28/2012      RC          2.13       Added Shutdown method.
 * 01/28/2013      RC          2.18       Added Water Profile First Bin Postion.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;

    /// <summary>
    /// Display the range animation based off the values given for the
    /// profile and bottom track depth.
    /// </summary>
    public class AdcpRangePlannerViewModel : PulseViewModel
    {

        #region Variable

        /// <summary>
        /// Default Water Profile Range.
        /// </summary>
        private const double DEFAULT_WP_RANGE = 20.0;

        /// <summary>
        /// Default Bottom Track Range.
        /// </summary>
        private const double DEFAULT_BT_RANGE = 30.0;

        /// <summary>
        /// Divisior to create the cross line positive and negative start location.
        /// </summary>
        private const int DIV = 10;

        /// <summary>
        /// Total length of the display in pixels.  This is used
        /// to convert meters to pixels.
        /// </summary>
        private const double DISPLAY_TOTAL_LENGTH = 300;

        /// <summary>
        /// Default ADCP image.
        /// </summary>
        private const string DEFAULT_ADCP_IMAGE = "../Images/adcp_med.png";

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        #endregion

        #region Properties

        #region Water Profile Line

        private double _waterProfileRange;
        /// <summary>
        /// Water Profile range.
        /// </summary>
        public double WaterProfileRange
        {
            get { return _waterProfileRange; }
            set
            {
                _waterProfileRange = value;
                this.NotifyOfPropertyChange(() => this.WaterProfileRange);
                this.NotifyOfPropertyChange(() => this.WaterProfileRangeStr);

                // Set all the related values
                SetValues();
            }
        }

        /// <summary>
        /// String for the Water Profile Range.
        /// </summary>
        public string WaterProfileRangeStr
        {
            get { return WaterProfileRange.ToString("0") + " m [WP]"; }
        }

        /// <summary>
        /// Water Profile Range Position.
        /// </summary>
        private double _waterProfileRangePos;
        /// <summary>
        /// Water Profile Range Position.
        /// </summary>
        public double WaterProfileRangePos
        {
            get { return _waterProfileRangePos; }
            set
            {
                _waterProfileRangePos = value;
                this.NotifyOfPropertyChange(() => this.WaterProfileRangePos);
            }
        }

        #endregion

        #region Bottom Track Lines

        /// <summary>
        /// Bottom Track Range
        /// </summary>
        private double _bottomTrackRange;
        /// <summary>
        /// Bottom Track Range
        /// </summary>
        public double BottomTrackRange
        {
            get { return _bottomTrackRange; }
            set
            {
                _bottomTrackRange = value;
                this.NotifyOfPropertyChange(() => this.BottomTrackRange);
                this.NotifyOfPropertyChange(() => this.BottomTrackRangeStr);

                // Set all the related values
                SetValues();
            }
        }

        /// <summary>
        /// String for the Bottom Track Range.
        /// </summary>
        public string BottomTrackRangeStr
        {
            get { return BottomTrackRange.ToString("0") + " m [BT]"; }
        }

        /// <summary>
        /// Bottom Track Range
        /// </summary>
        private double _bottomTrackRangePos;
        /// <summary>
        /// Bottom Track Range
        /// </summary>
        public double BottomTrackRangePos
        {
            get { return _bottomTrackRangePos; }
            set
            {
                _bottomTrackRangePos = value;
                this.NotifyOfPropertyChange(() => this.BottomTrackRangePos);
            }
        }

        #endregion

        #region Blank

        /// <summary>
        /// Water Profile Blank for the tranducer in meters. 
        /// </summary>
        private double _wpBlank;
        /// <summary>
        /// Water Profile Blank for the tranducer in meters. 
        /// </summary>
        public double WpBlank
        {
            get { return _wpBlank; }
            set
            {
                _wpBlank = value;
                this.NotifyOfPropertyChange(() => this.WpBlank);
                this.NotifyOfPropertyChange(() => this.WpBlankStr);

                // Set all the related values
                SetValues();
            }
        }

        /// <summary>
        /// String for the Water Profile Blank.
        /// </summary>
        public string WpBlankStr
        {
            get { return WpBlank.ToString("0") + " m"; }
        }

        /// <summary>
        /// Bottom Track Blank for the tranducer in meters. 
        /// </summary>
        private double _btBlank;
        /// <summary>
        /// Bottom Track Blank for the tranducer in meters. 
        /// </summary>
        public double BtBlank
        {
            get { return _btBlank; }
            set
            {
                _btBlank = value;
                this.NotifyOfPropertyChange(() => this.BtBlank);
                this.NotifyOfPropertyChange(() => this.BtBlankStr);

                // Set all the related values
                SetValues();
            }
        }

        /// <summary>
        /// String for the Bottom Track Blank.
        /// </summary>
        public string BtBlankStr
        {
            get { return BtBlank.ToString("0") + " m"; }
        }

        /// <summary>
        /// Line to designate the end of the Water Profile blank area.
        /// Value is in pixels.
        /// </summary>
        private double _wpBlankPos;
        /// <summary>
        /// Line to designate the end of the Water Profile  blank area.
        /// Value is in pixels.
        /// </summary>
        public double WpBlankPos
        {
            get { return _wpBlankPos; }
            set
            {
                _wpBlankPos = value;
                this.NotifyOfPropertyChange(() => this.WpBlankPos);
            }
        }

        /// <summary>
        /// Line to designate the end of the Bottom Track blank area.
        /// Value is in pixels.
        /// </summary>
        private double _btBlankPos;
        /// <summary>
        /// Line to designate the end of the Bottom Track blank area.
        /// Value is in pixels.
        /// </summary>
        public double BtBlankPos
        {
            get { return _btBlankPos; }
            set
            {
                _btBlankPos = value;
                this.NotifyOfPropertyChange(() => this.BtBlankPos);
            }
        }

        #endregion

        #region Water Profile First Bin Range


        /// <summary>
        /// Water Profile First Bin Range for the tranducer in meters. 
        /// </summary>
        private double _wpFirstBinRange;
        /// <summary>
        /// Water Profile Blank for the tranducer in meters. 
        /// </summary>
        public double WpFirstBinRange
        {
            get { return _wpFirstBinRange; }
            set
            {
                _wpFirstBinRange = value;
                this.NotifyOfPropertyChange(() => this.WpFirstBinRange);
                this.NotifyOfPropertyChange(() => this.WpFirstBinRangeStr);

                // Set all the related values
                SetValues();
            }
        }

        /// <summary>
        /// String for the Water Profile Blank.
        /// </summary>
        public string WpFirstBinRangeStr
        {
            get { return WpFirstBinRange.ToString("0.00") + " m"; }
        }

        /// <summary>
        /// Line to designate the end of the Water Profile First Bin Range area.
        /// Value is in pixels.
        /// </summary>
        private double _wpFirstBinRangePos;
        /// <summary>
        /// Line to designate the end of the Water Profile First Bin Range area.
        /// Value is in pixels.
        /// </summary>
        public double WpFirstBinRangePos
        {
            get { return _wpFirstBinRangePos; }
            set
            {
                _wpFirstBinRangePos = value;
                this.NotifyOfPropertyChange(() => this.WpFirstBinRangePos);
            }
        }

        #endregion

        #region Depth To Bottom

        /// <summary>
        /// Depth to the bottom of the sea floor in meters.
        /// </summary>
        private double _depthToBottom;
        /// <summary>
        /// Depth to the bottom of the sea floor in meters.
        /// </summary>
        public double DepthToBottom
        {
            get { return _depthToBottom; }
            set
            {
                _depthToBottom = value;
                this.NotifyOfPropertyChange(() => this.DepthToBottom);
                this.NotifyOfPropertyChange(() => this.DepthToBottomStr);

                // Update values
                SetValues();
            }
        }

        /// <summary>
        /// Position on the screen the Depth To Bottom should be displayed.
        /// This is in pixels.
        /// </summary>
        private double _depthToBottomPos;
        /// <summary>
        /// Position on the screen the Depth To Bottom should be displayed.
        /// This is in pixels.
        /// </summary>
        public double DepthToBottomPos
        {
            get { return _depthToBottomPos; }
            set
            {
                _depthToBottomPos = value;
                this.NotifyOfPropertyChange(() => this.DepthToBottomPos);
            }
        }

        /// <summary>
        /// String for the Depth to Bottom.
        /// </summary>
        public string DepthToBottomStr
        {
            get { return DepthToBottom.ToString("0") + " m"; }
        }

        #endregion

        #region ADCP Image

        /// <summary>
        /// ADCP Image based off selected project.
        /// </summary>
        private string _AdcpImage;
        /// <summary>
        /// ADCP Image based off selected project.
        /// </summary>
        public string AdcpImage
        {
            get { return _AdcpImage; }
            set
            {
                _AdcpImage = value;
                this.NotifyOfPropertyChange(() => this.AdcpImage);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Initialize the view model with the water profile and bottom track
        /// current settings.
        /// </summary>
        /// <param name="wp">Water Profile Range.</param>
        /// <param name="bt">Bottom Track Range.</param>
        public AdcpRangePlannerViewModel(double wp, double bt) :
            base("AdcprangePlannerViewModel")
        {
            // Initialize the lines
            _pm = IoC.Get<PulseManager>();
            _waterProfileRange = wp;
            _bottomTrackRange = bt;
            _wpBlank = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;
            _btBlank = Commands.AdcpSubsystemCommands.DEFAULT_300_CBTBL;
            _wpFirstBinRange = 0.0;
            _depthToBottom = Math.Max(_waterProfileRange, _bottomTrackRange);

            SetValues();
        }

        /// <summary>
        /// Initialize the view model with the water profile and bottom track
        /// current settings.
        /// </summary>
        public AdcpRangePlannerViewModel() :
            base("AdcprangePlannerViewModel")
        {
            // Initialize the lines
            _pm = IoC.Get<PulseManager>();
            _waterProfileRange = DEFAULT_WP_RANGE;
            _bottomTrackRange = DEFAULT_BT_RANGE;
            _wpBlank = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;
            _btBlank = Commands.AdcpSubsystemCommands.DEFAULT_300_CBTBL;
            _wpFirstBinRange = 0.0;
            _depthToBottom = Math.Max(_waterProfileRange, _bottomTrackRange);



            SetValues();
        }

        /// <summary>
        /// Shutdown the ViewModel.
        /// </summary>
        public override void Dispose()
        {

        }

        #region Methods


        #region Set Values

        /// <summary>
        /// Update all the values based off new user input.
        /// </summary>
        private void SetValues()
        {
            // Adcp Image
            if (_pm.IsProjectSelected)
            {
                AdcpImage = ProductImage.GetProductImage(_pm.SelectedProject);
            }
            else
            {
                AdcpImage = DEFAULT_ADCP_IMAGE;
            }

            // Pulse lines
            WpBlankPos = CalculatePosition(_wpBlank);
            BtBlankPos = CalculatePosition(_btBlank);
            WpFirstBinRangePos = CalculatePosition(_wpFirstBinRange); 

            // Calculate positions
            DepthToBottomPos = CalculatePosition(_depthToBottom);
            WaterProfileRangePos = CalculatePosition(_waterProfileRange);
            BottomTrackRangePos = CalculatePosition(_bottomTrackRange);
        }


        #endregion

        #region Calculate Position

        /// <summary>
        /// Calculate what percentage down from the top the element will travel.  Then convert that
        /// percentage to the total number of pixels in the display.
        /// 
        /// This will get the max depth of the bottom, wp or bt ping. Then calculate what percentage of
        /// the depth from the max depth this element will have.  It will then convert the percentage
        /// to pixels by getting the percentage of total pixels in the display.
        /// </summary>
        /// <param name="pulse">Distance in meters.</param>
        /// <returns>Depth in pixels for the element.</returns>
        private double CalculatePosition(double pulse)
        {
            double maxDepth = FindMaxDepth();                   // Find the bottom element, the greatest range
            double percent = pulse / maxDepth;                  // Percent the pulse will travel to the max depth
            return DISPLAY_TOTAL_LENGTH * percent;              // Convert the length in meters to pixels
        }

        /// <summary>
        /// Determine what has the greatest depth, the bottom, the water profile pulse or the bottom track pulse.
        /// </summary>
        /// <returns>Return the greatest depth in meters.</returns>
        private double FindMaxDepth()
        {
            return Math.Max(_depthToBottom, Math.Max(_waterProfileRange, _bottomTrackRange));
        }

        #endregion

        #endregion
    }
}
