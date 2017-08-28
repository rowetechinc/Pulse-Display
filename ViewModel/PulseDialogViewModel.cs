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
 * 09/23/2013      RC          3.1.3      Initial coding
 * 08/07/2014      RC          4.0.0      Updated ReactiveCommand to 6.0.
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ReactiveUI;
    using System.Threading.Tasks;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PulseDialogViewModel : PulseViewModel
    {

        /// <summary>
        /// Ok Result of this dialog.
        /// </summary>
        private bool _IsOk;
        /// <summary>
        /// Ok Result of this dialog.
        /// </summary>
        public bool IsOk
        {
            get { return _IsOk; }
            set
            {
                _IsOk = value;
                this.NotifyOfPropertyChange(() => this.IsOk);
            }
        }

        /// <summary>
        /// Cancel result of this dialog.
        /// </summary>
        public bool IsCancel
        {
            get { return !_IsOk; }
        }

        #region Command

        /// <summary>
        /// Ok command when the OK button is pressed.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> OkCommand { get; protected set; }

        /// <summary>
        /// Cancel command when the Cancel button is pressed.
        /// </summary>
        public ReactiveCommand<System.Reactive.Unit> CancelCommand { get; protected set; }

        #endregion


        /// <summary>
        /// Initialize values.
        /// </summary>
        public PulseDialogViewModel(string name)
            : base(name)
        {
            // Initialive values
            IsOk = false;

            CancelCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => Cancel()));

            OkCommand = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => Ok()));
        }

        /// <summary>
        /// Shutdown the viewmodel.
        /// </summary>
        public override void Dispose()
        {
            TryClose();
        }

        /// <summary>
        /// If the user presses the cancel button.
        /// </summary>
        private void Cancel()
        {
            this.IsOk = false;
            TryClose();
        }

        /// <summary>
        ///  If the user presses the OK button.
        /// </summary>
        private void Ok()
        {
            this.IsOk = true;
            TryClose();
        }

    }
}
