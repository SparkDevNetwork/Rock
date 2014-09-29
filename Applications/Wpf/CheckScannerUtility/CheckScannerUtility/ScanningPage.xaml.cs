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
using System;

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
        /// Handles the Click event of the btnStartStop control (only applies to Ranger scanners)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStartStop_Click( object sender, RoutedEventArgs e )
        {
            if ( batchPage.rangerScanner != null )
            {
                if ( ScanButtonText.IsStartScan( btnStartStop.Content as string ) )
                {
                    StartScanningRanger();
                }
                else
                {
                    batchPage.rangerScanner.StopFeeding();
                }
            }
        }

        /// <summary>
        /// Starts the scanning for Ranger device
        /// </summary>
        public void StartScanningRanger()
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
        /// Shows the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void ShowException( Exception ex)
        {
            lblExceptions.Content = "ERROR: " + ex.Message;
            lblExceptions.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatus( ScannedDocInfo scannedDocInfo )
        {
            lblExceptions.Visibility = Visibility.Collapsed;
            CurrentScannedDocInfo = scannedDocInfo;
            if ( !ScannedDocInfoHistory.Contains( scannedDocInfo ) )
            {
                ScannedDocInfoHistory.Add( scannedDocInfo );
            }
            
            if ( ScannedDocInfoHistory.Count > 1 )
            {
                gScannedChecksNavigation.Visibility = Visibility.Visible;
            }
            else
            {
                gScannedChecksNavigation.Visibility = Visibility.Collapsed;
            }

            NavigateTo( ScannedDocInfoHistory.Count - 1 );

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
                        }
                        else
                        {
                            ExpectingMagTekBackScan = true;
                            lblScanInstructions.Content = string.Format( "INFO: Insert the {0} again facing the other direction to get an image of the back.", scannedDocInfo.IsCheck ? "check" : "item" );
                        }
                    }
                }
                else
                {
                    UpdateScanInstructions();
                }
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
                pnlChecks.Visibility = Visibility.Visible;
                lblRoutingNumber.Content = scannedDocInfo.RoutingNumber ?? "--";
                lblAccountNumber.Content = scannedDocInfo.AccountNumber ?? "--";
                lblCheckNumber.Content = scannedDocInfo.CheckNumber ?? "--";
            }
            else
            {
                pnlChecks.Visibility = Visibility.Collapsed;
            }

            if ( scannedDocInfo.BadMicr )
            {
                if ( string.IsNullOrWhiteSpace( scannedDocInfo.AccountNumber ) )
                {
                    lblScanCheckWarningBadMicr.Content = "WARNING: Check account information not found. Try scanning again with the check facing the other direction.";
                }
                else
                {
                    lblScanCheckWarningBadMicr.Content = "WARNING: Check account information is invalid. Try scanning again.";
                }
                
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

            btnDeleteItem.Visibility = Visibility.Visible;

            if ( ScannedDocInfoHistory.Any( a => a.IsCheck && ( a.BadMicr || a.Duplicate ) ) )
            {
                lblSomeBadScans.Visibility = Visibility.Visible;
                lblScanInstructions.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblSomeBadScans.Visibility = Visibility.Collapsed;
                lblScanInstructions.Visibility = Visibility.Visible;
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

            // clear out the scanned doc history of scanned checks on this page since we are cancelling
            this.ClearScannedDocHistory();

            this.NavigationService.Navigate( batchPage );
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            if (RockConfig.Load().ScannerInterfaceType == RockConfig.InterfaceType.RangerApi)
            {
                btnStartStop.Visibility = Visibility.Visible;
            }
            else
            {
                btnStartStop.Visibility = Visibility.Hidden;
            }

            ShowStartupPage();
        }

        /// <summary>
        /// Shows the startup page.
        /// </summary>
        private void ShowStartupPage()
        {
            var rockConfig = RockConfig.Load();
            lblExceptions.Visibility = Visibility.Collapsed;
            lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi && rockConfig.EnableRearImage )
            {
                lblBack.Visibility = Visibility.Visible;
            }
            else if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 && rockConfig.PromptToScanRearImage )
            {
                lblBack.Visibility = Visibility.Visible;
            }
            else
            {
                lblBack.Visibility = Visibility.Hidden;
            }

            lblSomeBadScans.Visibility = Visibility.Collapsed;
            ExpectingMagTekBackScan = false;

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );

            gScannedChecksNavigation.Visibility = Visibility.Collapsed;
            btnDeleteItem.Visibility = Visibility.Collapsed;

            UpdateScanInstructions();
        }

        /// <summary>
        /// Updates the scan instructions.
        /// </summary>
        private void UpdateScanInstructions()
        {
            bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            if ( RockConfig.Load().ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                lblScanInstructions.Content = string.Format( "INFO: Ready to scan next {0}.", scanningChecks ? "check" : "item" );
            }
            else
            {
                lblScanInstructions.Content = string.Format( "INFO: Insert {0} into the scanner to continue.", scanningChecks ? "a check" : "an item" );
            }
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
            else
            {
                CurrentScannedDocInfo = null;
            }

            if ( !ScannedDocInfoHistory.Any() )
            {
                ShowStartupPage();
            }
            else
            {
                btnPrev.IsEnabled = CurrentScannedDocInfo != ScannedDocInfoHistory.First();
                btnNext.IsEnabled = CurrentScannedDocInfo != ScannedDocInfoHistory.Last();
            }
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
                    UpdateScanInstructions();

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
                    btnStartStop.Content = ScanButtonText.Stop;
                    btnSave.Visibility = Visibility.Hidden;
                    btnCancel.Visibility = Visibility.Hidden;
                    break;
            }

            this.shapeStatus.ToolTip = statusText;
            this.shapeStatus.Fill = new System.Windows.Media.SolidColorBrush( statusColor );
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteItem_Click( object sender, RoutedEventArgs e )
        {
            int index = this.ScannedDocInfoHistory.IndexOf( this.CurrentScannedDocInfo );
            this.ScannedDocInfoHistory.Remove( this.CurrentScannedDocInfo );
            
            // rebuild the main batchlist, but without the item we deleted
            var currentItems = this.batchPage.ScannedDocList.ToList();
            this.batchPage.ScannedDocList = new System.Collections.Concurrent.ConcurrentQueue<ScannedDocInfo>();
            foreach (var item in currentItems)
            {
                if (item != this.CurrentScannedDocInfo)
                {
                    this.batchPage.ScannedDocList.Enqueue( item );
                }
            }

            // if we weren't already on the first item, navigate to the prior item, otherwise, navigate to the 'new' first item in the list
            int newIndex = index == 0 ? 0 : index - 1;
            this.NavigateTo( newIndex );
        }
    }
}
