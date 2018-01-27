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
 * 01/03/2012      RC          1.11       Initial coding.
 *       
 * 
 */

using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace RTI
{
    /// <summary>
    /// Convert the string of the image path to a BitmapImage.
    /// This is used to prevent the image being left opened so it can be
    /// viewed and modified.
    /// Code found at: http://stackoverflow.com/questions/690150/delete-an-image-bound-to-a-control
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class UriToImageConverter : IValueConverter
    {
        /// <summary>
        /// Convert the given path to a bitmap image.
        /// This will load the bitmap but will ensure the image
        /// is not left open or locked.  It will load the information
        /// from the file into a BitmapImage object.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapImage ConvertToImage(string path)
        {
            if (!File.Exists(path))
                return null;

            BitmapImage bitmapImage = null;
            try
            {
                // Ensure cache mode is set to OnLoad
                // If it is not set to OnLoad, the file will give an exception that 
                // it cannot access the file because another process is using
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.StreamSource.Dispose();
            }
            catch (IOException)
            {
            }
            return bitmapImage;
        }

        #region IValueConverter Members

        /// <summary>
        /// Convert the path to an image.
        /// </summary>
        /// <param name="value">Path to the image.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>BitmapImage of the image from file.</returns>
        public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string))
                return null;

            var path = value as string;

            return ConvertToImage(path);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>.</returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}