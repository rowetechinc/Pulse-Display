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
 * 08/03/2012      RC          2.13       Initial coding
 * 08/27/2012      RC          2.13       Start the animation in the code to prevent resource issues.
 * 
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RTI
{
	/// <summary>
	/// Interaction logic for PulseAnime.xaml
	/// </summary>
	public partial class PulseAnime : UserControl
    {

        #region Variables

        #region Defaults

        /// <summary>
        /// Default depth of the bottom in meters.
        /// </summary>
        private const double DEFAULT_DEPTH_OF_BOTTOM = 100.0;

        /// <summary>
        /// Default pulse depth in meters.
        /// </summary>
        private const double DEFAULT_PULSE_DEPTH = DEFAULT_DEPTH_OF_BOTTOM;

        /// <summary>
        /// Default Water Profile blank distance in meters.
        /// </summary>
        private const float DEFAULT_WP_BLANK = Commands.AdcpSubsystemCommands.DEFAULT_300_CWPBL;

        #endregion

        /// <summary>
        /// Depth of the bottom in meters.
        /// </summary>
        private double _depthToBottom;

        /// <summary>
        /// Depth the pulse will travel in meters.
        /// </summary>
        private double _pulseDepth;

        /// <summary>
        /// Water Profile Blank distance in meters.
        /// </summary>
        private float _wpBlank;

        #region Translate Transform

        /// <summary>
        /// Height of the page.
        /// </summary>
        private const double TOTAL_TRANSFORM_LENGTH = 300;

        #region Pulse

        /// <summary>
        /// Maximum value the translate transform value should
        /// be to travel to the bottom of the control.
        /// </summary>
        private const double MIN_TRANSLATE_TRANSFORM_PULSE = -155;

        /// <summary>
        /// Maximum value the translate transform value should
        /// be to travel to the bottom of the control.
        /// </summary>
        private const double MAX_TRANSLATE_TRANSFORM_PULSE = 75;

        /// <summary>
        /// Total length of the transform.
        /// </summary>
        private const double TOTAL_TRANSLATE_TRANSFORM_PULSE = MAX_TRANSLATE_TRANSFORM_PULSE - MIN_TRANSLATE_TRANSFORM_PULSE;

        #endregion

        #region Text

        /// <summary>
        /// Maximum value the translate transform value should
        /// be to travel to the bottom of the control.
        /// </summary>
        private const double MIN_TRANSLATE_TRANSFORM_TEXT = -10;

        /// <summary>
        /// Maximum value the translate transform value should
        /// be to travel to the bottom of the control.
        /// </summary>
        private const double MAX_TRANSLATE_TRANSFORM_TEXT = 260;

        /// <summary>
        /// Total length of the transform.
        /// </summary>
        private const double TOTAL_TRANSLATE_TRANSFORM_TEXT = MAX_TRANSLATE_TRANSFORM_TEXT - MIN_TRANSLATE_TRANSFORM_TEXT;

        #endregion

        #region Blank line

        /// <summary>
        /// Minimum Y position for the Blank Line.
        /// </summary>
        private const double MIN_TRANSLATE_TRANSFORM_BLANK_LINE = 0;

        /// <summary>
        /// Maximum Y position for the Blank line.
        /// </summary>
        private const double MAX_TRANSLATE_TRANSFORM_BLANK_LINE = 300;

        #endregion

        #endregion

        #endregion

        #region Properties

        #region Color

        /// <summary>
        /// Set the color of the pulse.
        /// {Binding Color, RelativeSource={RelativeSource TemplatedParent}}
        /// </summary>
		public Brush Color
		{
			get
			{
                return arc.Stroke;
			}
			set
			{
                arc.Stroke = value;
			}
		}

        #endregion

        #region PulseDepth

        /// <summary>
        /// Pulse Depth Dependency Property.
        /// This will handle binding values for PulseDepth.
        /// Value in meters.
        /// </summary>
        public static readonly DependencyProperty PulseDepthProperty =
            DependencyProperty.RegisterAttached(
            "PulseDepth",
            typeof(double),
            typeof(PulseAnime),
            new FrameworkPropertyMetadata(MAX_TRANSLATE_TRANSFORM_PULSE,                                                           // Default value
                                            FrameworkPropertyMetadataOptions.Inherits,                  // Inherits
                                            OnPulseDepthPropertyChanged));                              // Property Change callback

        /// <summary>
        /// Get and set the PulseDepth value.
        /// Value in meters.
        /// </summary>
        public double PulseDepth
        {
            get { return (double)GetValue(PulseDepthProperty); }
            set { SetValue(PulseDepthProperty, value); }
        }

        /// <summary>
        /// Callback method when someone tries to modify the PulseDepth property.  This will verify
        /// the source is this object.  If someone tries to set the value without a binding, the source
        /// will be the location of the object and not this object.  Set the new value for the pulse depth.
        /// Value is in meters.
        /// </summary>
        /// <param name="source">Location this property was changed.</param>
        /// <param name="e">Old and new value for the property.</param>
        public static void OnPulseDepthPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            // Get the source (this object)
            PulseAnime pulseAnime = source as PulseAnime;

            // Get the new value
            double value = (double)e.NewValue;

            // This would be null if the source was not PulseAnime
            if (pulseAnime != null)
            {
                //// Set the value
                pulseAnime.SetPulseDepth(value);
            }
        }

        #endregion

        #region DepthToBottom

        /// <summary>
        /// Pulse Depth Dependency Property.
        /// This will handle binding values for PulseDepth.
        /// Value in meters.
        /// </summary>
        public static readonly DependencyProperty DepthToBottomProperty =
            DependencyProperty.RegisterAttached(
            "DepthToBottom",
            typeof(double),
            typeof(PulseAnime),
            new FrameworkPropertyMetadata(DEFAULT_DEPTH_OF_BOTTOM,                                          // Default value
                                            FrameworkPropertyMetadataOptions.Inherits,                      // Inherits
                                            OnDepthToBottomPropertyChanged));                               // Property Change callback

        /// <summary>
        /// Get and set the PulseDepth value.
        /// Value in meters.
        /// </summary>
        public double DepthToBottom
        {
            get { return (double)GetValue(DepthToBottomProperty); }
            set { SetValue(DepthToBottomProperty, value); }
        }

        /// <summary>
        /// Callback method when someone tries to modify the PulseDepth property.  This will verify
        /// the source is this object.  If someone tries to set the value without a binding, the source
        /// will be the location of the object and not this object.  Set the new value for the pulse depth.
        /// Value is in meters.
        /// </summary>
        /// <param name="source">Location this property was changed.</param>
        /// <param name="e">Old and new value for the property.</param>
        public static void OnDepthToBottomPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            // Get the source (this object)
            PulseAnime pulseAnime = source as PulseAnime;

            // Get the new value
            double value = (double)e.NewValue;

            // This would be null if the source was not PulseAnime
            if (pulseAnime != null)
            {
                //// Set the value
                pulseAnime.SetDepthToBottom(value);
            }
        }

        #endregion

        #region Blank

        /// <summary>
        /// Pulse Depth Dependency Property.
        /// This will handle binding values for PulseDepth.
        /// Value in meters.
        /// </summary>
        public static readonly DependencyProperty WpBlankProperty =
            DependencyProperty.RegisterAttached(
            "WpBlank",
            typeof(float),
            typeof(PulseAnime),
            new FrameworkPropertyMetadata(DEFAULT_WP_BLANK,                                       // Default value
                                            FrameworkPropertyMetadataOptions.Inherits,            // Inherits
                                            OnWpBlankPropertyChanged));                             // Property Change callback

        /// <summary>
        /// Get and set the Water Profile Blank value.
        /// Value in meters.
        /// </summary>
        public double WpBlank
        {
            get { return (double)GetValue(WpBlankProperty); }
            set { SetValue(WpBlankProperty, value); }
        }

        /// <summary>
        /// Callback method when someone tries to modify the PulseDepth property.  This will verify
        /// the source is this object.  If someone tries to set the value without a binding, the source
        /// will be the location of the object and not this object.  Set the new value for the pulse depth.
        /// Value is in meters.
        /// </summary>
        /// <param name="source">Location this property was changed.</param>
        /// <param name="e">Old and new value for the property.</param>
        public static void OnWpBlankPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            // Get the source (this object)
            PulseAnime pulseAnime = source as PulseAnime;

            // Get the new value
            float value = (float)e.NewValue;

            // This would be null if the source was not PulseAnime
            if (pulseAnime != null)
            {
                //// Set the value
                pulseAnime.SetWpBlank(value);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Display and ADCP pulse.  This will be an animation of
        /// the pulse going down in the water.
        /// </summary>
		public PulseAnime()
		{
			this.InitializeComponent();

            // Initialize Values
            Color = new SolidColorBrush(Colors.Gray);
            _depthToBottom = DEFAULT_DEPTH_OF_BOTTOM;
            _pulseDepth = DEFAULT_PULSE_DEPTH;
            _wpBlank = DEFAULT_WP_BLANK;

            // Start the Animation
            StartAnimation();
		}

        #region Start Aninimation

        /// <summary>
        /// Start the animation.  This will get the storyboard and
        /// begin the animation.  The animation is started in code and
        /// not in the xmal to prevent a resource issue.  If in xmal, 
        /// every time the animation is viewed, another trigger is set
        /// and the animation will start a new animation.
        /// http://www.galasoft.ch/mydotnet/articles/article-2006102701.aspx
        /// </summary>
        private void StartAnimation()
        {
            Storyboard sb = (Storyboard)FindResource("pulseAnimationStoryboard");
            sb.Begin(this);
        }

        #endregion

        #region Position Calc

        /// <summary>
        /// Calculate the position of the text based off the 
        /// depth the pulse will travel.
        /// </summary>
        private void CalcPos()
        {
            // Set Text position
            //depthTextTransform.Y = CalculatePulseTextPosition(PulseDepth, _depthToBottom);

            // Set Pulse Length
            SetPulseLength(CalculatePulsePosition(PulseDepth, _depthToBottom));
        }

        #endregion

        #region Set Methods

        #region PulseDepth

        /// <summary>
        /// Set the new Pulse Depth based off the value given.
        /// This will change the text of the depth.
        /// It will modify the storyboard to increase or reduce
        /// the distance the pulse will travel.
        /// </summary>
        /// <param name="value">New pulse depth.</param>
        public void SetPulseDepth(double value)
        {
            // Set the Text value
            //pulseDepthText.Text = value.ToString("0.0") + "m";

            // Set the value
            _pulseDepth = value;
            
            // Calculate the positions for the text and pulse
            CalcPos();
        }

        /// <summary>
        /// Set the Pulse length to the storyboard transform.
        /// This will go into the storyboard to get the translate
        /// transform.  It will then set its value.
        /// 
        /// The value is based off the same proportion of the min and max percentage of the control
        /// and the depth and depth to bottom percentage.
        /// </summary>
        /// <param name="value">Pulse length.</param>
        private void SetPulseLength(double value)
        {
            //Setup the storyboard
            //Verify the storyboard exist
            if (Resources.Contains("Storyboard1") == true)
            {
                Storyboard sb = Resources["Storyboard1"] as Storyboard;

                // Verify the key frame exist
                if (sb.Children.Count > 0 && sb.Children[0] is DoubleAnimationUsingKeyFrames)
                {
                    DoubleAnimationUsingKeyFrames dbkf = sb.Children[0] as DoubleAnimationUsingKeyFrames;

                    // Verify the double key frame exist
                    if (dbkf.KeyFrames.Count > 0 && dbkf.KeyFrames[0] is EasingDoubleKeyFrame)
                    {
                        EasingDoubleKeyFrame dkf = dbkf.KeyFrames[0] as EasingDoubleKeyFrame;

                        // Set the value
                        dkf.Value = value;
                    }
                }
            }
        }

        #endregion

        #region DepthOfBottom

        /// <summary>
        /// Set the depth of the bottom.  Then recalculate the pulse depth.
        /// </summary>
        /// <param name="value">Depth to the bottom in meters.</param>
        private void SetDepthToBottom(double value)
        {
            _depthToBottom = value;

            // Recalculate the pulse position
            CalcPos();
        }

        #endregion

        #region Blank

        /// <summary>
        /// Set the Water Profile blank position.  This is the location the pulse will start.
        /// The blank is the area under transducer where the pulse begins.  The 
        /// blank is the gap area between the transducer and the beginning of the
        /// pulse.
        /// </summary>
        /// <param name="value">Blank distance in meters.</param>
        private void SetWpBlank(float value)
        {
            // If the blank exceeds the pulse depth,
            // set the blank to the pulse depth.
            if (value >= _pulseDepth)
            {
                _wpBlank = (float)_pulseDepth;
            }
            else
            {
                _wpBlank = value;
            }

            TransformGroup tg = arc.RenderTransform as TransformGroup;
            if (tg != null)
            {
                TranslateTransform tt = tg.Children[3] as TranslateTransform;

                if (tt != null)
                {
                    //double percent = _wpBlank / _depthOfBottom;                                                     // Percent the pulse will start
                    //double totalLength = MAX_TRANSLATE_TRANSFORM_PULSE - MIN_TRANSLATE_TRANSFORM_PULSE;             // Calculate length in pixels
                    //double pos = MIN_TRANSLATE_TRANSFORM_PULSE + (TOTAL_TRANSFORM_LENGTH * percent);                           // Convert the length in meters to pixels
                    //tt.Y = CalculateBlankPosition(_wpBlank, _depthOfBottom);

                    //double blankLineTotalLength = MAX_TRANSLATE_TRANSFORM_BLANK_LINE - MIN_TRANSLATE_TRANSFORM_BLANK_LINE;  // Calculate length in pixels
                    //double blankLinepos = MIN_TRANSLATE_TRANSFORM_BLANK_LINE + (TOTAL_TRANSFORM_LENGTH * percent);            // Convert the length in meters to pixels
                    //wpBlankLine.Y1 = CalculateBlankLinePosition(_wpBlank, _depthOfBottom);
                    //wpBlankLine.Y2 = CalculateBlankLinePosition(_wpBlank, _depthOfBottom);
                }
            }

        }

        #endregion

        #endregion

        #region Calculate Positions

        #region Pulse

        /// <summary>
        /// Calculate what percentage down from the top the Pulse will travel.  Then convert that
        /// percentage to the total number of pixels in the display.
        /// </summary>
        /// <param name="pulse">Pulse distance in meters.</param>
        /// <param name="depth">Depth to the bottom in meters.</param>
        /// <returns>Height in pixels for the Pulse.</returns>
        public static double CalculatePulsePosition(double pulse, double depth)
        {
            double percent = pulse / depth;                                                                    // Percent the pulse will travel to the depth
            double pos = MIN_TRANSLATE_TRANSFORM_PULSE + (TOTAL_TRANSLATE_TRANSFORM_PULSE * percent);      // Convert the length in meters to pixels
            return pos;
        }

        /// <summary>
        /// Calculate what percentage down from the top the Pulse will travel.  Then convert that
        /// percentage to the total number of pixels in the display.
        /// </summary>
        /// <param name="pulse">Pulse distance in meters.</param>
        /// <param name="depth">Depth to the bottom in meters.</param>
        /// <returns>Height in pixels for the Pulse Text.</returns>
        public static double CalculatePulseTextPosition(double pulse, double depth)
        {
            double percent = pulse / depth;                                                                    // Percent the pulse will travel to the depth
            double pos = MIN_TRANSLATE_TRANSFORM_TEXT + (TOTAL_TRANSLATE_TRANSFORM_TEXT * percent);        // Convert the length in meters to pixels
            return pos;
        }

        #endregion

        #region Blank

        /// <summary>
        /// Calculate what percentage down from the top is the blank.  Then convert that
        /// percentage to the total number of pixels in the display.
        /// </summary>
        /// <param name="blank">Blank distance in meters.</param>
        /// <param name="depth">Depth to the bottom in meters.</param>
        /// <returns>Height of Blank in pixels.</returns>
        public static double CalculateBlankPosition(double blank, double depth)
        {
            double percent = blank / depth;                                                                 // Percent the pulse will start
            double pos = MIN_TRANSLATE_TRANSFORM_PULSE + (TOTAL_TRANSFORM_LENGTH * percent);                // Convert the length in meters to pixels
            return pos;
        }

        /// <summary>
        /// Calculate what percentage down from the top is the blank.  Then convert that
        /// percentage to the total number of pixels in the display.
        /// </summary>
        /// <param name="blank">Blank distance in meters.</param>
        /// <param name="depth">Depth to the bottom in meters.</param>
        /// <returns>Height of Blank Line in pixels.</returns>
        public static double CalculateBlankLinePosition(double blank, double depth)
        {
            double percent = blank / depth;                                                                 // Percent the pulse will start
            double pos = MIN_TRANSLATE_TRANSFORM_BLANK_LINE + (TOTAL_TRANSFORM_LENGTH * percent);           // Convert the length in meters to pixels
            return pos;
        }

        #endregion

        #endregion
    }
}