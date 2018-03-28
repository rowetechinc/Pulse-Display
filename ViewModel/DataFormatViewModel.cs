using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTI
{
    /// <summary>
    /// Data Format View Model.
    /// </summary>
    public class DataFormatViewModel : PulseViewModel
    {

        #region Variables

        /// <summary>
        /// Options to store for averaging the data.
        /// </summary>
        private DataFormatOptions _Options;

        /// <summary>
        /// Pulse manager.
        /// </summary>
        private readonly PulseManager _pm;

        #endregion

        #region Properties

        /// <summary>
        /// Turn on or off Binary Format decoding.
        /// </summary>
        public bool IsBinaryFormat
        {
            get
            {
                return _Options.IsBinaryFormat;
            }
            set
            {
                _Options.IsBinaryFormat = value;
                this.NotifyOfPropertyChange(() => this.IsBinaryFormat);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// Binary format description.
        /// </summary>
        public string BinaryFormatDesc
        {
            get
            {
                return "Default Format.  Rowe Tech Binary Format is a binary format. The format is bascially the MATLAB file format with an additional header added to the beginning of the MATLAB format and a checksum at the end.";
            }
        }

        /// <summary>
        /// Turn on or off DVL Format decoding.
        /// </summary>
        public bool IsDvlFormat
        {
            get
            {
                return _Options.IsDvlFormat;
            }
            set
            {
                _Options.IsDvlFormat = value;
                this.NotifyOfPropertyChange(() => this.IsDvlFormat);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// DVL format description.
        /// </summary>
        public string DvlFormatDesc
        {
            get
            {
                return "Rowe Tech DVL Format is a ASCII format. The format is resembles NMEA GPS data starting with a $ID and ending with a *checksum.";
            }
        }

        /// <summary>
        /// Turn on or off PD0 Format decoding.
        /// </summary>
        public bool IsPd0Format
        {
            get
            {
                return _Options.IsPd0Format;
            }
            set
            {
                _Options.IsPd0Format = value;
                this.NotifyOfPropertyChange(() => this.IsPd0Format);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// PD0 format description.
        /// </summary>
        public string Pd0FormatDesc
        {
            get
            {
                return "PD0 binary format.  Industry standard binary format typically used in Teledyne RD Instruments ADCPs.";
            }
        }

        /// <summary>
        /// Turn on or off PD6/PD13 Format decoding.
        /// </summary>
        public bool IsPd6_13Format
        {
            get
            {
                return _Options.IsPd6_13Format;
            }
            set
            {
                _Options.IsPd6_13Format = value;
                this.NotifyOfPropertyChange(() => this.IsPd6_13Format);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// PD6/PD13 format description.
        /// </summary>
        public string Pd6_13FormatDesc
        {
            get
            {
                return "PD6/PD13 ASCII DVL format.  Industry standard ASCII format typically used in Teledyne RD Instruments ADCPs and DVLs.";
            }
        }

        /// <summary>
        /// Turn on or off PD4/PD5 Format decoding.
        /// </summary>
        public bool IsPd4_5Format
        {
            get
            {
                return _Options.IsPd4_5Format;
            }
            set
            {
                _Options.IsPd4_5Format = value;
                this.NotifyOfPropertyChange(() => this.IsPd4_5Format);

                // Save the options to DB
                SaveOptions();
            }
        }

        /// <summary>
        /// PD4/PD5 format description.
        /// </summary>
        public string Pd4_5FormatDesc
        {
            get
            {
                return "PD4/PD5 Binary DVL format.  Industry standard Binary format typically used in Teledyne RD Instruments ADCPs and DVLs.  This binary format reduces the amount of data output to DVL data.";
            }
        }

        #endregion

        /// <summary>
        /// Initialize the view.
        /// </summary>
        public DataFormatViewModel()
            :base("Data Format")
        {
            // Get objects
            _pm = IoC.Get<PulseManager>();

            // Initialize the options
            GetOptionsFromDatabase();
        }

        public override void Dispose()
        {
    
        }

        #region Get and Save Options

        /// <summary>
        /// Get the options from the database.
        /// </summary>
        private void GetOptionsFromDatabase()
        {
            _Options = _pm.GetDataFormatOptions();
        }

        /// <summary>
        /// Save the options in the database.
        /// </summary>
        private void SaveOptions()
        {
            _pm.UpdateDataFormatOptions(_Options);
        }

        #endregion
    }
}
