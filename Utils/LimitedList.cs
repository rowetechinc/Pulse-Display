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
 * 06/19/2013      RC          3.0.1      Initial coding
 * 08/13/2013      RC          3.0.7      Added CheckSize() to check if the size has exceed the limit.
 * 
 */
namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Create a list with a limited size.
    /// If the size exceeds the limit, it will
    /// remove the items from the beginning of the list.
    /// </summary>
    public class LimitedList<T> : List<T>
    {
        #region Properties

        /// <summary>
        /// Limit of the size of the list.
        /// </summary>
        private int _Limit;
        /// <summary>
        /// Limit of the size of the list.
        /// </summary>
        public int Limit 
        {
            get { return _Limit; } 
            set
            {
                _Limit = value;

                // Check list size
                CheckSize();
            }
        }

        #endregion

        /// <summary>
        /// Initialize the object with the size limit.
        /// </summary>
        /// <param name="limit">Max size of the list.</param>
        public LimitedList(int limit = 100)
        {
            // Set the limit
            Limit = limit;
        }

        /// <summary>
        /// Add the item to the list.  If the list
        /// exceeds the limit, remove the first item
        /// in the list.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public new void Add(T item)
        {
            // Add item
            base.Add(item);

            // Check list size
            CheckSize();
        }

        /// <summary>
        /// Add the list to this list.
        /// If the list exceeds the limit,
        /// remove the first item in the list
        /// until the list is less then the limit.
        /// </summary>
        /// <param name="list">List of values to add.</param>
        public new void AddRange(IEnumerable<T> list)
        {
            // Add the range
            base.AddRange(list);

            // Check list size
            CheckSize();
        }

        /// <summary>
        /// Check if the size of the list exceeds the limit.
        /// If it does, then remove the first entry until the
        /// list size is correct.
        /// </summary>
        private void CheckSize()
        {
            // Check if exceeds limit
            while (this.Count > Limit)
            {
                // Remove first item in the list
                base.RemoveAt(0);
            }
        }

    }
}
