using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CheckScannerUtilityWPF
{
    /// <summary>
    /// Interaction logic for ScanningPage.xaml
    /// </summary>
    public partial class ScanningPage : Page
    {
        /// <summary>
        /// Gets or sets the batch page.
        /// </summary>
        /// <value>
        /// The batch page.
        /// </value>
        public BatchPage batchPage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningPage"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScanningPage(BatchPage value)
        {
            InitializeComponent();
            this.batchPage = value;
        }

        /// <summary>
        /// Handles the 1 event of the btnStartStop_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStartStop_Click_1( object sender, RoutedEventArgs e )
        {
            if ( btnStartStop.Content.Equals( "Stop" ) )
            {
                batchPage.RangerScanner.StopFeeding();
            }
            else
            {
                const int FeedSourceMainHopper = 0;
                const int FeedContinuously = 0;
                batchPage.RangerScanner.StartFeeding( FeedSourceMainHopper, FeedContinuously );
            }
        }

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1( object sender, RoutedEventArgs e )
        {
            this.NavigationService.Navigate( batchPage );
        }
    }
}
