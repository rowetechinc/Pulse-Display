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
 * Date            Initials    Comments
 * -----------------------------------------------------------------
 * 11/15/2011      RC          Initial coding
 * 
 */


using System.Windows.Data;
using System.Windows.Media;
using System;
using OxyPlot;

namespace RTI
{
    /// <summary>
    /// Convert the given OxyColor to a SolidColorBrush.
    /// This is used to display colors in a combobox.
    /// </summary>
    [ValueConversion(typeof(OxyColor), typeof(Brush))]
    public class OxyColorToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert the color to a brush.
        /// </summary>
        /// <param name="value">OxyColor.</param>
        /// <param name="targetType">Brush type.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>SolidColorBrush of the color.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            if (!(value is OxyColor)) return null;
            SolidColorBrush scb = new SolidColorBrush((Color)ColorConverter.ConvertFromString(((OxyColor)value).ToString()));
            return scb;
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="value">.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>Not implemented.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}