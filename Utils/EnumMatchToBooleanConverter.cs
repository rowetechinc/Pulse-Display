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
 * 9/30/2011       RC          Initial coding
 * 
 * 
*/

using System.Windows.Data;
using System;
using System.Globalization;

namespace RTI
{
    /// <summary>
    /// Code originally found at: http://www.wpftutorial.net/RadioButton.html
    /// 
    /// The radio button control has a known issue with data binding. If you bind the 
    /// IsChecked property to a boolean and check the RadioButton, the value gets True. 
    /// But when you check another RadioButton, the databound value still remains true.
    /// 
    /// The reason for this is, that the Binding gets lost during the unchecking, 
    /// because the controls internally calls ClearValue() on the dependency property.
    /// </summary>
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Convert radiobutton value.
        /// </summary>
        /// <param name="value">.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns>.</returns>
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue,
                     StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Convert Radiobutton value.
        /// </summary>
        /// <param name="value">.</param>
        /// <param name="targetType">.</param>
        /// <param name="parameter">.</param>
        /// <param name="culture">.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool useValue = (bool)value;
            string targetValue = parameter.ToString();
            if (useValue)
                return Enum.Parse(targetType, targetValue);

            return null;
        }
    }
}