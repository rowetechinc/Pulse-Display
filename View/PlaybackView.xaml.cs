using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace RTI
{
    /// <summary>
    /// Interaction logic for PlaybackView.xaml
    /// </summary>
    public partial class PlaybackView : UserControl
    {
        /// <summary>
        /// Initialize view.
        /// </summary>
        public PlaybackView()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Debug.WriteLine("Start Playback Init...");
            InitializeComponent();
            Debug.WriteLine("End Playback Init.");
            stopwatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopwatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Debug.WriteLine(elapsedTime);
        }
    }
}
