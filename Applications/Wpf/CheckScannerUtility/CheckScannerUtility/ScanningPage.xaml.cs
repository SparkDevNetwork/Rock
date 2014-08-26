// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
        /// Gets or sets a value indicating whether [expecting mag tek back scan].
        /// </summary>
        /// <value>
        /// <c>true</c> if [expecting mag tek back scan]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpectingMagTekBackScan { get; set; }

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
            if ( ScanButtonText.IsStartScan( btnStartStop.Content as string ) )
            {
                StartScanning();
            }
            else
            {
                batchPage.rangerScanner.StopFeeding();
            }
        }

        /// <summary>
        /// Starts the scanning.
        /// </summary>
        public void StartScanning()
        {
            if ( batchPage.ScannerFeederType.Equals( FeederType.SingleItem ) )
            {
                batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceManualDrop, FeedItemCount.FeedOne );
            }
            else
            {
                batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedContinuously );
            }
        }

        /// <summary>
        /// Shows the check information.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowDocInformation( ScannedDocInfo scannedDocInfo )
        {
            if ( scannedDocInfo.FrontImageData != null )
            {
                BitmapImage bitmapImageFront = new BitmapImage();
                bitmapImageFront.BeginInit();
                bitmapImageFront.StreamSource = new MemoryStream( scannedDocInfo.FrontImageData );
                bitmapImageFront.EndInit();
                imgFront.Source = bitmapImageFront;
            }
            else
            {
                imgFront.Source = null;
            }

            if ( scannedDocInfo.BackImageData != null )
            {
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedDocInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgBack.Source = bitmapImageBack;
            }
            else
            {
                imgBack.Source = null;
            }

            lblScanInstructions.Visibility = Visibility.Collapsed;
            ExpectingMagTekBackScan = false;
            if ( ( imgFront.Source == null ) && ( imgBack.Source == null ) )
            {
                lblScanInstructions.Content = string.Format( "INFO: Insert the {0} into the scanner to begin.", scannedDocInfo.IsCheck ? "check" : "item" );
                lblScanInstructions.Visibility = Visibility.Visible;
            }

            var rockConfig = RockConfig.Load();

            // If we have the front image and valid routing number, but not the back (and it's a MagTek).  Inform them to scan the back;
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                if ( ( imgFront.Source != null ) && ( imgBack.Source == null ) )
                {
                    if ( rockConfig.PromptToScanRearImage )
                    {
                        if ( scannedDocInfo.IsCheck && ( scannedDocInfo.RoutingNumber.Length != 9 || string.IsNullOrWhiteSpace( scannedDocInfo.AccountNumber ) ) )
                        {
                            ExpectingMagTekBackScan = false;
                            lblScanInstructions.Content = "INFO: Ready to re-scan check";
                            lblScanInstructions.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ExpectingMagTekBackScan = true;
                            lblScanInstructions.Content = string.Format( "INFO: Insert the {0} again facing the other direction to get an image of the back.", scannedDocInfo.IsCheck ? "check" : "item" );
                            lblScanInstructions.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    lblScanInstructions.Content = string.Format( "INFO: Ready to scan next {0}.", scannedDocInfo.IsCheck ? "check" : "item" );
                    lblScanInstructions.Visibility = Visibility.Visible;
                }
            }

            if ( scannedDocInfo.IsCheck )
            {
                pnlChecks.Visibility = System.Windows.Visibility.Visible;
                lblRoutingNumber.Content = scannedDocInfo.RoutingNumber ?? "--";
                lblAccountNumber.Content = scannedDocInfo.AccountNumber ?? "--";
                lblCheckNumber.Content = scannedDocInfo.CheckNumber ?? "--";
            }
            else
            {
                pnlChecks.Visibility = System.Windows.Visibility.Collapsed;
            }
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
            var rockConfig = RockConfig.Load();
            lblScanCheckWarning.Visibility = Visibility.Collapsed;
            if (rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi && rockConfig.EnableRearImage)
            {
                lblBack.Visibility = System.Windows.Visibility.Visible;
            }
            else if (rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 && rockConfig.PromptToScanRearImage)
            {
                lblBack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lblBack.Visibility = System.Windows.Visibility.Hidden;
            }
            
            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            ShowDocInformation( new ScannedDocInfo() );
        }
    }
}
