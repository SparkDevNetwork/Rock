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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Rock.Model;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for ScanningPage.xaml
    /// </summary>
    public partial class ScanningPage : System.Windows.Controls.Page
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
        /// The ScannedDocInfo of a bad scan that the user need to confirm before upload
        /// </summary>
        /// <value>
        /// The confirm upload bad scanned document.
        /// </value>
        public ScannedDocInfo ConfirmUploadBadScannedDoc { get; set; }

        /// <summary>
        /// Shows the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void ShowException( Exception ex )
        {
            System.Diagnostics.Debug.WriteLine( ex.StackTrace );
            lblExceptions.Content = "ERROR: " + ex.Message;
            lblExceptions.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatusAndUpload( ScannedDocInfo scannedDocInfo )
        {
            lblExceptions.Visibility = Visibility.Collapsed;

            DisplayScannedDocInfo( scannedDocInfo );

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
                            lblScanInstructions.Content = "Ready to re-scan check";
                        }
                        else
                        {
                            ExpectingMagTekBackScan = true;
                            lblScanInstructions.Content = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scannedDocInfo.IsCheck ? "check" : "item" );
                        }
                    }
                }
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
            }

            if ( IsDuplicateScan( scannedDocInfo ) )
            {
                scannedDocInfo.Duplicate = true;
                scannedDocInfo.Upload = false;
            }

            if ( scannedDocInfo.Upload )
            {
                this.UploadScannedItem( scannedDocInfo );
                if ( _keepScanning )
                {
                    ResumeScanning();
                }
            }
            else
            {
                ShowUploadWarnings( scannedDocInfo );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnIgnoreAndUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnIgnoreAndUpload_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            var scannedDocInfo = this.ConfirmUploadBadScannedDoc;
            scannedDocInfo.Upload = true;
            this.UploadScannedItem( scannedDocInfo );
            ResumeScanning();
        }

        /// <summary>
        /// Handles the Click event of the btnSkipAndContinue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSkipAndContinue_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            this.ConfirmUploadBadScannedDoc = null;
            _itemsSkipped++;
            ShowUploadStats();
            ResumeScanning();
        }

        /// <summary>
        /// Handles the Click event of the btnStopAndRetry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopAndRetry_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts( false );
        }

        /// <summary>
        /// Shows the upload warnings.
        /// </summary>
        private void ShowUploadWarnings( ScannedDocInfo scannedDocInfo )
        {
            ConfirmUploadBadScannedDoc = scannedDocInfo;
            lblScanCheckWarningDuplicate.Visibility = scannedDocInfo.Duplicate ? Visibility.Visible : Visibility.Collapsed;
            lblScanCheckWarningBadMicr.Visibility = scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = scannedDocInfo.Duplicate || scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            pnlStartStopClose.IsEnabled = pnlPromptForUpload.Visibility == System.Windows.Visibility.Visible ? false : true;
        }

        /// <summary>
        /// Hides the warnings messages and prompts
        /// </summary>
        private void HideUploadWarningPrompts( bool hideWarningMessages = true )
        {
            if ( hideWarningMessages )
            {
                lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
                lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            }

            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
            pnlStartStopClose.IsEnabled = true;
        }

        /// <summary>
        /// Shows the upload success.
        /// </summary>
        private void ShowUploadSuccess()
        {
            lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
            lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Visible;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
            pnlStartStopClose.IsEnabled = true;
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
                lblFront.Visibility = Visibility.Visible;
            }
            else
            {
                lblFront.Visibility = Visibility.Hidden;
                imgFront.Source = null;
            }

            if ( scannedDocInfo.BackImageData != null )
            {
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedDocInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgBack.Source = bitmapImageBack;
                lblBack.Visibility = Visibility.Visible;
            }
            else
            {
                imgBack.Source = null;
                lblBack.Visibility = Visibility.Hidden;
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
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            // set the uploadScannedItemClient to null to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            uploadScannedItemClient = null;
            ShowStartupPage();
            _itemsUploaded = 0;
            _itemsSkipped = 0;
            ShowUploadStats();
            StartScanning();
            lblScanItemCountInfo.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the startup page.
        /// </summary>
        private void ShowStartupPage()
        {
            var rockConfig = RockConfig.Load();
            HideUploadWarningPrompts();
            lblExceptions.Visibility = Visibility.Collapsed;
            lblStartupInfo.Visibility = Visibility.Visible;
            ExpectingMagTekBackScan = false;

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );

            bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            if ( RockConfig.Load().ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                lblScanInstructions.Content = string.Format( "Ready to scan next {0}.", scanningChecks ? "check" : "item" );
            }
            else
            {
                lblScanInstructions.Content = string.Format( "Insert {0} into the scanner, then click Start to continue.", scanningChecks ? "a check" : "an item" );
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
                    btnStart.IsEnabled = true;
                    btnStopScanning.IsEnabled = false;
                    break;

                case XportStates.TransportFeeding:
                    btnStart.IsEnabled = false;
                    btnStopScanning.IsEnabled = true;

                    lblScanInstructions.Visibility = Visibility.Hidden;

                    btnClose.Visibility = Visibility.Hidden;
                    break;
            }

            this.shapeStatus.ToolTip = statusText;
            this.shapeStatus.Fill = new System.Windows.Media.SolidColorBrush( statusColor );
        }

        /// <summary>
        /// Rangers the scanner_ transport item suspended.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportItemSuspended( object sender, AxRANGERLib._DRangerEvents_TransportItemSuspendedEvent e )
        {
            System.Diagnostics.Debug.WriteLine( "rangerScanner_TransportItemSuspended" );
        }

        /// <summary>
        /// Rangers the scanner_ transport passthrough event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportPassthroughEvent( object sender, AxRANGERLib._DRangerEvents_TransportPassthroughEventEvent e )
        {
            System.Diagnostics.Debug.WriteLine( "rangerScanner_TransportPassthroughEvent" );
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportItemInPocket( object sender, AxRANGERLib._DRangerEvents_TransportItemInPocketEvent e )
        {
            System.Diagnostics.Debug.WriteLine( "rangerScanner_TransportItemInPocket" );
        }

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            System.Diagnostics.Debug.WriteLine( "rangerScanner_TransportFeedingStopped" );
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnClose_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.Navigate( batchPage );
        }

        #region Upload Scanned Items

        /// <summary>
        /// Gets or sets the client that stays connected 
        /// </summary>
        /// <value>
        /// The persisted client.
        /// </value>
        private RockRestClient uploadScannedItemClient { get; set; }

        /// <summary>
        /// Gets or sets the binary file type contribution for uploading transactions
        /// </summary>
        /// <value>
        /// The binary file type contribution.
        /// </value>
        private BinaryFileType binaryFileTypeContribution { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value contribution for uploading transactions
        /// </summary>
        /// <value>
        /// The transaction type value contribution.
        /// </value>
        private DefinedValue transactionTypeValueContribution { get; set; }

        /// <summary>
        /// Determines whether [is duplicate scan] [the specified scanned document].
        /// </summary>
        /// <param name="scannedDoc">The scanned document.</param>
        /// <returns></returns>
        private bool IsDuplicateScan( ScannedDocInfo scannedDoc )
        {
            if ( !scannedDoc.IsCheck )
            {
                return false;
            }

            if ( scannedDoc.BadMicr )
            {
                return false;
            }

            if ( uploadScannedItemClient == null )
            {
                var rockConfig = RockConfig.Load();
                uploadScannedItemClient = new RockRestClient( rockConfig.RockBaseUrl );
                uploadScannedItemClient.Login( rockConfig.Username, rockConfig.Password );
            }

            var alreadyScanned = uploadScannedItemClient.PostDataWithResult<string, bool>( "api/FinancialTransactions/AlreadyScanned", scannedDoc.ScannedCheckMicr );
            return alreadyScanned;
        }

        /// <summary>
        /// Uploads the scanned item.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void UploadScannedItem( ScannedDocInfo scannedDocInfo )
        {
            if ( uploadScannedItemClient == null )
            {
                RockConfig rockConfig = RockConfig.Load();
                uploadScannedItemClient = new RockRestClient( rockConfig.RockBaseUrl );
                uploadScannedItemClient.Login( rockConfig.Username, rockConfig.Password );
            }

            if ( binaryFileTypeContribution == null || transactionTypeValueContribution == null )
            {
                binaryFileTypeContribution = uploadScannedItemClient.GetDataByGuid<BinaryFileType>( "api/BinaryFileTypes", new Guid( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE ) );
                transactionTypeValueContribution = uploadScannedItemClient.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
            }

            RockRestClient client = uploadScannedItemClient;

            // upload image of front of doc
            string frontImageFileName = string.Format( "image1_{0}.png", RockDateTime.Now.ToString( "o" ).RemoveSpecialCharacters() );
            int frontImageBinaryFileId = client.UploadBinaryFile( frontImageFileName, Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid(), scannedDocInfo.FrontImagePngBytes, false );

            // upload image of back of doc (if it exists)
            int? backImageBinaryFileId = null;
            if ( scannedDocInfo.BackImageData != null )
            {
                // upload image of back of doc
                string backImageFileName = string.Format( "image2_{0}.png", RockDateTime.Now.ToString( "o" ).RemoveSpecialCharacters() );
                backImageBinaryFileId = client.UploadBinaryFile( backImageFileName, Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid(), scannedDocInfo.BackImagePngBytes, false );
            }

            FinancialTransaction financialTransaction = new FinancialTransaction();

            Guid transactionGuid = Guid.NewGuid();

            financialTransaction.BatchId = batchPage.SelectedFinancialBatch.Id;
            financialTransaction.TransactionCode = string.Empty;
            financialTransaction.Summary = string.Empty;

            financialTransaction.Guid = transactionGuid;
            financialTransaction.TransactionDateTime = batchPage.SelectedFinancialBatch.BatchStartDateTime;

            financialTransaction.CurrencyTypeValueId = scannedDocInfo.CurrencyTypeValue.Id;
            financialTransaction.SourceTypeValueId = scannedDocInfo.SourceTypeValue.Id;

            financialTransaction.TransactionTypeValueId = transactionTypeValueContribution.Id;

            int? uploadedTransactionId;

            if ( scannedDocInfo.IsCheck )
            {
                financialTransaction.TransactionCode = scannedDocInfo.CheckNumber;

                FinancialTransactionScannedCheck financialTransactionScannedCheck = new FinancialTransactionScannedCheck();

                // Rock server will encrypt CheckMicrPlainText to this since we can't have the DataEncryptionKey in a RestClient
                financialTransactionScannedCheck.FinancialTransaction = financialTransaction;
                financialTransactionScannedCheck.ScannedCheckMicr = scannedDocInfo.ScannedCheckMicr;

                uploadedTransactionId = client.PostData<FinancialTransactionScannedCheck>( "api/FinancialTransactions/PostScanned", financialTransactionScannedCheck ).AsIntegerOrNull();
            }
            else
            {
                uploadedTransactionId = client.PostData<FinancialTransaction>( "api/FinancialTransactions", financialTransaction as FinancialTransaction ).AsIntegerOrNull();
            }

            if ( !uploadedTransactionId.HasValue )
            {
                // just in case we didn't get the Id from the POST...
                uploadedTransactionId = client.GetIdFromGuid( "api/FinancialTransactions/", transactionGuid );
            }

            // upload FinancialTransactionImage records for front/back
            FinancialTransactionImage financialTransactionImageFront = new FinancialTransactionImage();
            financialTransactionImageFront.BinaryFileId = frontImageBinaryFileId;
            financialTransactionImageFront.TransactionId = uploadedTransactionId.Value;
            financialTransactionImageFront.Order = 0;
            client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageFront );

            if ( backImageBinaryFileId.HasValue )
            {
                FinancialTransactionImage financialTransactionImageBack = new FinancialTransactionImage();
                financialTransactionImageBack.BinaryFileId = backImageBinaryFileId.Value;
                financialTransactionImageBack.TransactionId = uploadedTransactionId.Value;
                financialTransactionImageBack.Order = 1;
                client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageBack );
            }

            scannedDocInfo.TransactionId = uploadedTransactionId;
            financialTransaction.Id = uploadedTransactionId ?? 0;
            financialTransaction.CreatedDateTime = financialTransaction.CreatedDateTime ?? DateTime.Now;

            var transactionList = batchPage.grdBatchItems.DataContext as BindingList<FinancialTransaction>;
            transactionList.Insert( 0, financialTransaction );

            _itemsUploaded++;
            ShowUploadStats();
            ShowUploadSuccess();
        }

        /// <summary>
        /// Shows the upload stats.
        /// </summary>
        private void ShowUploadStats()
        {
            List<string> statsList = new List<string>();
            if ( _itemsUploaded > 0 )
            {
                statsList.Add( string.Format( "Uploaded: {0}", _itemsUploaded ) );
            }

            if ( _itemsSkipped > 0 )
            {
                statsList.Add( string.Format( "Skipped: {0}", _itemsSkipped ) );
            }

            lblScanItemCountInfo.Visibility = statsList.Any() ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemCountInfo.Content = statsList.AsDelimited( ", " );
        }

        /// <summary>
        /// The _keep scanning
        /// </summary>
        private bool _keepScanning;

        private int _itemsSkipped;

        private int _itemsUploaded;

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            StartScanning();
        }

        /// <summary>
        /// Starts the scanning as soon as items are in the hopper
        /// </summary>
        public void StartScanning()
        {
            _keepScanning = true;
            ResumeScanning();
        }

        /// <summary>
        /// Handles the Click event of the btnStopScanning control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopScanning_Click( object sender, RoutedEventArgs e )
        {
            StopScanning();
        }

        /// <summary>
        /// Stops the scanning.
        /// </summary>
        private void StopScanning()
        {
            _keepScanning = false;
            if ( batchPage.rangerScanner != null )
            {
                // remove the StartRangerFeedingWhenReady (in case it is assigned) so it doesn't restart after getting into ReadyToFeed state
                batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
                batchPage.rangerScanner.StopFeeding();
            }
        }

        /// <summary>
        /// Resumes scanning.
        /// </summary>
        private void ResumeScanning()
        {
            if ( batchPage.rangerScanner != null )
            {
                // StartFeeding doesn't work if the Scanner isn't in ReadyToFeed state, so assign StartRangerFeedingWhenReady if it isn't ready yet
                XportStates xportState = (XportStates)batchPage.rangerScanner.GetTransportState();
                if ( xportState == XportStates.TransportReadyToFeed )
                {
                    batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
                }
                else
                {
                    // ensure the event is only registered once
                    batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
                    batchPage.rangerScanner.TransportReadyToFeedState += StartRangerFeedingWhenReady;
                }
            }
        }

        /// <summary>
        /// Starts the ranger feeding when ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void StartRangerFeedingWhenReady( object sender, AxRANGERLib._DRangerEvents_TransportReadyToFeedStateEvent e )
        {
            // only fire this event once
            batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;

            if ( _keepScanning )
            {
                batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
            }
        }

        #endregion
    }
}
