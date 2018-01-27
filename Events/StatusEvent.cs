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
 * 04/10/2012      RC          2.08       Initial coding
 * 04/12/2012      RC          2.08       Changed the yellow color.
 */
using System.Windows;
using System.Windows.Media;
namespace RTI
{


    /// <summary>
    /// Give a status event.  This is an event that
    /// occurs that the user should be notified about.
    /// This include events such as "Download Complete"
    /// or "Download Failed".  This is to replace a MessageBox.
    /// </summary>
    public class StatusEvent
    {
        #region Colors

        /// <summary>
        /// Blue color to represent Information, or positive feedback.
        /// </summary>
        public const string COLOR_BLUE = "DeepSkyBlue"; //"#FF32328B"; //DeepSkyBlue

        /// <summary>
        /// Red color to represent a issue.
        /// </summary>
        public const string COLOR_RED = "Red";

        /// <summary>
        /// Yellow color for warnings.
        /// </summary>
        public const string COLOR_YELLOW = "#EEEF0E";  // #FCFE00 Brighter

        #endregion

        #region Variables

        /// <summary>
        /// Default duration is 5 seconds.
        /// </summary>
        public const int DEFAULT_DURATION = 5;

        /// <summary>
        /// Default statis is information.
        /// </summary>
        public const MessageBoxImage DEFAULT_STATUS = MessageBoxImage.None;

        /// <summary>
        /// Default color to use.  A positive color.
        /// </summary>
        public Color DEFAULT_COLOR = Colors.DeepSkyBlue;

        #endregion

        #region Properties

        /// <summary>
        /// Message for the Event.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Color of the event.  This is a color
        /// that can be used differenticate between
        /// a warning and a positive message.  Red and Blue.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Time to display the message in seconds.  This
        /// is the timeperiod the message will be displayed to the 
        /// user.  The message will appear.  Remain on for Duration-1 seconds.
        /// Then disappear.
        /// </summary>
        public int Duration { get; set; }

        #endregion

        /// <summary>
        /// Set the status using a message, a duration and a MessageBoxImage
        /// to depict the status.  The MessageBoxImage will set the color based
        /// of the MessageBoxImage type chosen.
        /// </summary>
        /// <param name="message">Message to display to the user.</param>
        /// <param name="status">Status of the message.  Default is Information.</param>
        /// <param name="duration">Time period to display message.  Default is 5 seconds.</param>
        public StatusEvent(string message, MessageBoxImage status = DEFAULT_STATUS, int duration = DEFAULT_DURATION)
        {
            Message = message;
            SetColor(status);
            Duration = duration;
        }

        /// <summary>
        /// Set the status with the given message, duration and color.  The color will be
        /// converted to a string.
        /// </summary>
        /// <param name="message">Message to give the user.</param>
        /// <param name="color">Color to set for the message.</param>
        /// <param name="duration">Time to display the image.</param>
        public StatusEvent(string message, Color color, int duration = DEFAULT_DURATION)
        {
            Message = message;
            Color = color.ToString();
            Duration = duration;
        }

        /// <summary>
        /// Set the color based off the MessageBoxImage given.
        /// There are 2 colors to choose from, red or blue, negative or positive.
        /// </summary>
        /// <param name="status">Status to set the color.</param>
        private void SetColor(MessageBoxImage status)
        {
            switch(status)
            {
                case MessageBoxImage.Error:
                    Color = COLOR_RED;
                    break;
                case MessageBoxImage.Warning:
                    Color = COLOR_YELLOW;
                    break;
                case MessageBoxImage.Information:
                case MessageBoxImage.None:
                case MessageBoxImage.Question:
                default:
                    Color = COLOR_BLUE;
                    break;

            }
        }
    }
}
