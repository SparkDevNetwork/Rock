using System.IO;
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Rock.Apps.CheckScannerUtility
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
        public ScanningPage( BatchPage value )
        {
            InitializeComponent();
            this.batchPage = value;
        }

        /// <summary>
        /// Handles the Click event of the btnStartStop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStartStop_Click( object sender, RoutedEventArgs e )
        {
            batchPage.HandleScanButtonClick( sender, e, false );
        }

        /// <summary>
        /// Shows the check information.
        /// </summary>
        /// <param name="scannedCheckInfo">The scanned check info.</param>
        public void ShowCheckInformation( ScannedCheckInfo scannedCheckInfo )
        {
            if ( scannedCheckInfo.FrontImageData != null )
            {
                BitmapImage bitmapImageFront = new BitmapImage();
                bitmapImageFront.BeginInit();
                bitmapImageFront.StreamSource = new MemoryStream( scannedCheckInfo.FrontImageData );
                bitmapImageFront.EndInit();
                imgFront.Source = bitmapImageFront;
            }
            else
            {
                imgFront.Source = null;
            }

            if ( scannedCheckInfo.BackImageData != null )
            {
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedCheckInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgBack.Source = bitmapImageBack;
            }
            else
            {
                imgBack.Source = null;
            }

            lblRoutingNumber.Content = string.Format( "Routing Number: {0}", scannedCheckInfo.RoutingNumber );
            lblAccountNumber.Content = string.Format( "Account Number: {0}", scannedCheckInfo.AccountNumber );
            lblCheckNumber.Content = string.Format( "Check Number: {0}", scannedCheckInfo.CheckNumber );
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDone_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.Navigate( batchPage );
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            lblScanWarning.Visibility = Visibility.Collapsed;
            ShowCheckInformation( new ScannedCheckInfo() );
        }
    }
}
