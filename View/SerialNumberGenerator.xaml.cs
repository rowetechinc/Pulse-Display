﻿/*
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
 * 11/15/2012      RC          2.16       Initial coding
 * 
 */

using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace RTI
{
    /// <summary>
    /// Interaction logic for SerialNumberGeneratorView.xaml
    /// </summary>
    public partial class SerialNumberGeneratorView : UserControl
    {

        /// <summary>
        /// Constructor
        /// Usings MEF dependency objects.
        /// </summary>
        public SerialNumberGeneratorView()
        {
            InitializeComponent();
        }
    }
}
