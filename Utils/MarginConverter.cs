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
 * 08/07/2011       RC          Initial coding
 * 
 * 
*/

using System.Windows.Data;
using System.Windows;

namespace RTI
{
    /// <summary>
    /// Set the margin with the given value.  This will set the Top margin with the value given.
    /// http://stackoverflow.com/questions/6249518/binding-only-part-of-the-margin-property-of-wpf-control
    /// </summary>
    public class MarginConverter : IValueConverter
    {

        /// <summary>
        /// Set the Top margin with the value given.
        /// </summary>
        /// <param name="value">Top margin value.</param>
        /// <param name="targetType">Not Used.</param>
        /// <param name="parameter">Not Used.</param>
        /// <param name="culture">Not Used.</param>
        /// <returns>Margin with the top value changed.</returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Thickness(0, System.Convert.ToDouble(value), 0, 0);
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value">Not Used.</param>
        /// <param name="targetType">Not Used.</param>
        /// <param name="parameter">Not Used.</param>
        /// <param name="culture">Not Used.</param>
        /// <returns>Not Used.</returns>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
