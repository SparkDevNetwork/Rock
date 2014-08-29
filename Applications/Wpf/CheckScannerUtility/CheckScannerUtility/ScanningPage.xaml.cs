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
            this.ScannedDocInfoHistory = new List<ScannedDocInfo>();
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
        /// Gets or sets the current scanned document information.
        /// </summary>
        /// <value>
        /// The current scanned document information.
        /// </value>
        private ScannedDocInfo CurrentScannedDocInfo { get; set; }

        /// <summary>
        /// Gets or sets the scanned document information history.
        /// </summary>
        /// <value>
        /// The scanned document information history.
        /// </value>
        private List<ScannedDocInfo> ScannedDocInfoHistory { get; set; }

        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatus( ScannedDocInfo scannedDocInfo )
        {
            CurrentScannedDocInfo = scannedDocInfo;
            ScannedDocInfoHistory.Add( scannedDocInfo );
            bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            if (scanningChecks && ScannedDocInfoHistory.Count > 1)
            {
                gScannedChecksNavigation.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                gScannedChecksNavigation.Visibility = System.Windows.Visibility.Collapsed;
            }

            NavigateTo( ScannedDocInfoHistory.Count - 1 );

            lblScanInstructions.Visibility = Visibility.Collapsed;
            ExpectingMagTekBackScan = false;

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

            if ( scanningChecks && ScannedDocInfoHistory.Any( a => a.BadMicr || a.Duplicate ) )
            {
                lblScanInstructions.Content = "WARNING: One or more check scans have issues. Review the scanned checks before continuing. You might need to rescan some checks.";
            }
        }

        /// <summary>
        /// Displays the scanned document information.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void DisplayScannedDocInfo( ScannedDocInfo scannedDocInfo )
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

            if ( scannedDocInfo.BadMicr )
            {
                lblScanCheckWarningBadMicr.Visibility = Visibility.Visible;
            }
            else
            {
                lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            }

            if ( scannedDocInfo.Duplicate )
            {
                lblScanCheckWarningDuplicate.Visibility = Visibility.Visible;
            }
            else
            {
                lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Clears the scanned document history.
        /// </summary>
        public void ClearScannedDocHistory()
        {
            ScannedDocInfoHistory.Clear();
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSave_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.Navigate( batchPage );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            // cancelled the scanned checks, so rebuild the batchPage.ScannedDocList to only include the ones that have already been uploaded
            var uploadedScans = batchPage.ScannedDocList.Where( a => a.Uploaded ).ToList();
            batchPage.ScannedDocList = new System.Collections.Concurrent.ConcurrentQueue<ScannedDocInfo>();
            foreach ( var scannedDoc in uploadedScans )
            {
                batchPage.ScannedDocList.Enqueue( scannedDoc );
            }

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
            lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi && rockConfig.EnableRearImage )
            {
                lblBack.Visibility = System.Windows.Visibility.Visible;
            }
            else if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 && rockConfig.PromptToScanRearImage )
            {
                lblBack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lblBack.Visibility = System.Windows.Visibility.Hidden;
            }

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );

            gScannedChecksNavigation.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            if ( CurrentScannedDocInfo != null )
            {
                int navIndex = ScannedDocInfoHistory.IndexOf( CurrentScannedDocInfo ) - 1;
                NavigateTo( navIndex );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if ( CurrentScannedDocInfo != null )
            {
                int navIndex = ScannedDocInfoHistory.IndexOf( CurrentScannedDocInfo ) + 1;
                NavigateTo( navIndex );
            }
        }

        /// <summary>
        /// Navigates to.
        /// </summary>
        /// <param name="navIndex">Index of the nav.</param>
        private void NavigateTo( int navIndex )
        {
            if ( navIndex >= 0 && navIndex < ScannedDocInfoHistory.Count )
            {
                CurrentScannedDocInfo = ScannedDocInfoHistory[navIndex];
                DisplayScannedDocInfo( CurrentScannedDocInfo );
            }

            btnPrev.IsEnabled = CurrentScannedDocInfo != ScannedDocInfoHistory.First();
            btnNext.IsEnabled = CurrentScannedDocInfo != ScannedDocInfoHistory.Last();
        }

        /// <summary>
        /// Shows the scanner status.
        /// </summary>
        /// <param name="xportStates">The xport states.</param>
        public void ShowScannerStatus( XportStates xportStates, System.Windows.Media.Color statusColor, string statusText )
        {
            switch ( xportStates )
            {
                case XportStates.TransportReadyToFeed:
                    bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                    if ( scanningChecks && ScannedDocInfoHistory.Any( a => a.BadMicr || a.Duplicate ) )
                    {
                        // leave the scan instructions as they are
                    }
                    else
                    {
                        lblScanInstructions.Content = string.Format( "INFO: Insert {0} into the scanner to begin.", scanningChecks ? "a check" : "an item" );
                    }

                    lblScanInstructions.Visibility = Visibility.Visible;

                    if ( batchPage.ScannerFeederType.Equals( FeederType.MultipleItems ) )
                    {
                        btnStartStop.Content = ScanButtonText.Scan;
                    }
                    else
                    {
                        btnStartStop.Content = ScanButtonText.ScanCheck;
                    }

                    break;

                case XportStates.TransportFeeding:
                    lblScanInstructions.Content = "INFO: Waiting for scan output...";
                    lblScanInstructions.Visibility = Visibility.Visible;
                    btnStartStop.Content = ScanButtonText.Stop;
                    btnSave.Visibility = Visibility.Hidden;
                    btnCancel.Visibility = Visibility.Hidden;
                    break;
            }

            this.shapeStatus.ToolTip = statusText;
            this.shapeStatus.Fill = new System.Windows.Media.SolidColorBrush( statusColor );
        }
    }
}
