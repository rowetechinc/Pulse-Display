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
 * 11/01/2011      RC                     Initial coding
 * 03/09/2012      RC          2.06       Created the converter to work with the ColormapBrush object.
 *       
 * 
 */

using System.Windows.Data;
using RTI;
using System;
using System.Globalization;
using System.Windows.Media;

namespace RTI
{
    /// <summary>
    /// Convert a colormap brush enum value to an actual brush.
    /// </summary>
    [ValueConversion(typeof(ColormapBrush.ColormapBrushEnum), typeof(Brush))]
    public class ColormapToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Convert the given enum value to a colormap brush.
        /// </summary>
        /// <param name="value">.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>Colormap brush with the correct brush type.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ColormapBrush brush = new ColormapBrush();

            if (targetType != typeof(Brush)) return null;
            if (!(value is ColormapBrush.ColormapBrushEnum)) return null;
            Brush scb = ColormapBrush.GetBrush((ColormapBrush.ColormapBrushEnum)value);
            return scb;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>Not implemented.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}