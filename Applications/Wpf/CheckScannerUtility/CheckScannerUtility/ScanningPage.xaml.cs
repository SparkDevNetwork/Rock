// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageSafeInterop;
using Rock.Apps.CheckScannerUtility.Models;
using Rock.Wpf.Controls;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for ScanningPage.xaml
    /// </summary>
    public partial class ScanningPage : System.Windows.Controls.Page
    {
        private RockConfig.InterfaceType _interfaceType;

        private ScannedDocInfo _currentMagtekScannedDoc { get; set; }

        private bool _isBackScan;

        /// <summary>
        /// Gets the batch page.
        /// </summary>
        /// <value>
        /// The batch page.
        /// </value>
        internal BatchPage _batchPage => ScanningPageUtility.batchPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningPage"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScanningPage( BatchPage batchPage )
        {
            ScanningPageUtility.batchPage = batchPage;
            InitializeComponent();

            try
            {
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration( System.Configuration.ConfigurationUserLevel.None );
                ScanningPageUtility.DebugLogFilePath = config.AppSettings.Settings["DebugLogFilePath"].Value;
                bool isDirectory = !string.IsNullOrWhiteSpace( ScanningPageUtility.DebugLogFilePath ) && Directory.Exists( ScanningPageUtility.DebugLogFilePath );
                if ( isDirectory )
                {
                    ScanningPageUtility.DebugLogFilePath = Path.Combine( ScanningPageUtility.DebugLogFilePath, "CheckScanner.log" );
                }
            }
            catch ( Exception ex )
            {
                // Safe to ignore since we are just setting up debug logging
                System.Diagnostics.Debug.WriteLine( $"ScanningPage: {ex}" );
            }
        }

        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatusAndUpload( ScannedDocInfo scannedDocInfo )
        {
            this.HideAlertMessage();
            this.HideUploadResultMessage();

            DisplayScannedDocInfo( scannedDocInfo );

            var rockConfig = RockConfig.Load();

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            imgScannedItemNone.Visibility = Visibility.Collapsed;

            // f they don't enable smart scan, don't warn about bad MICRs, they might be scanning a mixture of checks and envelopes.
            // However, if there was a failure getting an image, show a warning
            if ( ( scannedDocInfo.BadMicr && rockConfig.EnableSmartScan ) || scannedDocInfo.ImageFailure )
            {
                var scannedItemName = scanningChecks ? "check" : "item";
                StringBuilder alertMessageBuilder = new StringBuilder();
                string imageTypeName = "image";
                if ( scannedDocInfo.ImageFailure )
                {
                    if ( scannedDocInfo.FrontImageData != null && scannedDocInfo.BackImageData == null )
                    {
                        imageTypeName = "back image";
                    }
                }

                if ( scannedDocInfo.BadMicr && scannedDocInfo.ImageFailure )
                {
                    alertMessageBuilder.AppendLine( $"Unable to read {scannedItemName} information or {scannedItemName} {imageTypeName}." );
                    imgScannedItemNone.Visibility = Visibility.Visible;
                }
                else if ( scannedDocInfo.BadMicr )
                {
                    alertMessageBuilder.AppendLine( $"Unable to read {scannedItemName} information." );
                }
                else if ( scannedDocInfo.ImageFailure )
                {
                    alertMessageBuilder.AppendLine( $"Unable to read {scannedItemName} {imageTypeName}." );
                    imgScannedItemNone.Visibility = Visibility.Visible;
                }

                alertMessageBuilder.AppendLine( $"Click 'Skip {scannedItemName}' to reject this {scannedItemName} and continue scanning." );
                alertMessageBuilder.AppendLine( $"To retry this check, click 'Skip {scannedItemName}' then put the {scannedItemName} back into the feed tray." );
                alertMessageBuilder.AppendLine( string.Empty );
                alertMessageBuilder.Append( $"Click 'Accept {scannedItemName}' to upload the {scannedItemName} as-is." );

                this.DisplayAlertMessage( AlertMessageType.Warning, alertMessageBuilder.ToString() );
                ShowUploadWarnings( scannedDocInfo );
                return;
            }
            else
            {
                scannedDocInfo.Upload = true;
            }

            if ( scannedDocInfo.Upload && ScanningPageUtility.IsDuplicateScan( scannedDocInfo ) )
            {
                var scannedItemName = scanningChecks ? "check" : "item";

                scannedDocInfo.Duplicate = true;
                scannedDocInfo.Upload = false;
                StringBuilder alertMessageBuilder = new StringBuilder();
                alertMessageBuilder.AppendLine( $"A {scannedItemName} with the same account information and check number has already been scanned." );
                alertMessageBuilder.AppendLine( $"Click 'Skip {scannedItemName}' to reject this {scannedItemName}." );
                alertMessageBuilder.Append( $"Click 'Accept {scannedItemName}' to upload the {scannedItemName} as-is." );

                this.DisplayAlertMessage( AlertMessageType.Warning, alertMessageBuilder.ToString() );

                ShowUploadWarnings( scannedDocInfo );

                if ( scannedDocInfo.IsCheck )
                {
                    btnSkipAndContinue.Focus();
                }
            }

            if ( scannedDocInfo.Upload )
            {
                if ( rockConfig.CaptureAmountOnScan )
                {
                    PromptForAmounts( scannedDocInfo );
                    return;
                }

                ScanningPageUtility.UploadScannedItem( scannedDocInfo );
                this.ShowUploadResult( UploadResult.Uploaded );

                ShowUploadStats();

                if ( ScanningPageUtility.KeepScanning )
                {
                    if ( this._interfaceType == RockConfig.InterfaceType.RangerApi )
                    {
                        ResumeRangerScanning();
                    }
                }
            }
        }

        /// <summary>
        /// Resumes the ranger scanning.
        /// </summary>
        public void ResumeRangerScanning()
        {
            if ( this._batchPage.rangerScanner != null && RockConfig.Load().ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
            {
                // StartFeeding doesn't work if the Scanner isn't in ReadyToFeed state, so assign StartRangerFeedingWhenReady if it isn't ready yet
                RangerTransportStates xportState = ( RangerTransportStates ) this._batchPage.rangerScanner.GetTransportState();
                if ( xportState == RangerTransportStates.TransportReadyToFeed )
                {
                    this._batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
                }
                else if ( xportState == RangerTransportStates.TransportShutDown )
                {
                    DisplayAlertMessage( AlertMessageType.Warning, "Scanner is not ready. Verify that the scanner is powered on and connected." );
                    btnStart.IsEnabled = true;
                    btnStopScanning.IsEnabled = false;
                }
                else
                {
                    // ensure the event is only registered once
                    this._batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
                    this._batchPage.rangerScanner.TransportReadyToFeedState += StartRangerFeedingWhenReady;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnIgnoreAndUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnIgnoreAndUpload_Click( object sender, RoutedEventArgs e )
        {
            ProcessIgnoreUploadResponse( UploadResponse.Upload );
        }

        /// <summary>
        /// Handles the Click event of the btnSkipAndContinue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSkipAndContinue_Click( object sender, RoutedEventArgs e )
        {
            ProcessIgnoreUploadResponse( UploadResponse.Ignore );
        }

        /// <summary>
        /// Handles the Click event of the BtnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnNext_Click( object sender, RoutedEventArgs e )
        {
            ProcessIgnoreUploadResponse( UploadResponse.UploadWithAmount );
        }

        /// <summary>
        /// 
        /// </summary>
        private enum UploadResponse
        {
            Ignore,
            Upload,
            UploadWithAmount
        }

        /// <summary>
        /// Processes the ignore upload response.
        /// </summary>
        /// <param name="uploadResponse">The upload response.</param>
        private bool ProcessIgnoreUploadResponse( UploadResponse uploadResponse )
        {
            HideUploadWarningPrompts();
            HideAlertMessage();

            if ( uploadResponse == UploadResponse.Upload )
            {
                var scannedDocInfo = ScanningPageUtility.ConfirmUploadBadScannedDoc;
                scannedDocInfo.Upload = true;

                var rockConfig = RockConfig.Load();

                if ( rockConfig.CaptureAmountOnScan )
                {
                    PromptForAmounts( scannedDocInfo );
                    return false;
                }

                ScanningPageUtility.UploadScannedItem( scannedDocInfo );
                this.ShowUploadResult( UploadResult.Uploaded );
            }
            else if ( uploadResponse == UploadResponse.Ignore )
            {
                this.ShowUploadResult( UploadResult.Skipped );
                ScanningPageUtility.ConfirmUploadBadScannedDoc = null;

                ScanningPageUtility.ItemsSkipped++;
            }
            else if ( uploadResponse == UploadResponse.UploadWithAmount )
            {
                var scannedDocInfo = ScanningPageUtility.PromptForAmountScannedDoc;
                if ( scannedDocInfo == null || scannedDocInfo.TransactionId > 0 )
                {
                    // nothing to upload or already uploaded
                    return true;
                }

                if ( RockConfig.Load().CaptureAmountOnScan )
                {
                    var accountAmountCaptureList = lvAccountDetailsEntry.ItemsSource as List<DisplayAccountValueModel>;
                    var amountEntered = accountAmountCaptureList.Any( a => a.Amount.HasValue && a.Amount.Value > 0.00M );
                    if ( !amountEntered )
                    {
                        DisplayAlertMessage( AlertMessageType.Warning, "Please enter an amount" );
                        return false;
                    }
                    else
                    {
                        scannedDocInfo.AccountAmountCaptureList = accountAmountCaptureList;
                    }
                }

                scannedDocInfo.Upload = true;
                ScanningPageUtility.PromptForAmountScannedDoc = null;
                ScanningPageUtility.UploadScannedItem( scannedDocInfo );
                this.ShowUploadResult( UploadResult.Uploaded );
            }

            ShowUploadStats();

            if ( this._interfaceType == RockConfig.InterfaceType.RangerApi )
            {
                if ( ScanningPageUtility.KeepScanning )
                {
                    ResumeRangerScanning();
                }
            }
            else
            {
                btnStart.IsEnabled = true;
                btnNext.IsEnabled = true;
            }

            return true;
        }

        /// <summary>
        /// Shows the upload warnings.
        /// </summary>
        private void ShowUploadWarnings( ScannedDocInfo scannedDocInfo )
        {
            ScanningPageUtility.ConfirmUploadBadScannedDoc = scannedDocInfo;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            lblScanItemUploadSkipped.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = ( scannedDocInfo.Duplicate || scannedDocInfo.BadMicr || scannedDocInfo.ImageFailure ) ? Visibility.Visible : Visibility.Collapsed;

            btnStart.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
        }

        /// <summary>
        /// Hides the warnings messages and prompts
        /// </summary>
        private void HideUploadWarningPrompts()
        {
            btnStart.IsEnabled = false;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Prompts for amounts.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void PromptForAmounts( ScannedDocInfo scannedDocInfo )
        {
            ScanningPageUtility.PromptForAmountScannedDoc = scannedDocInfo;
            spAccounts.Visibility = Visibility.Visible;
            lvAccountDetailsEntry.Visibility = Visibility.Visible;
            lvAccountDetailsDisplay.Visibility = Visibility.Collapsed;

            List<DisplayAccountValueModel> displayedAccountList = lvAccountDetailsEntry.ItemsSource as List<DisplayAccountValueModel>;
            displayedAccountList.ForEach( a => a.Amount = null );
            lblTotals.Content = $"{0.00:C}";
            btnNext.Visibility = Visibility.Visible;
            btnStart.IsEnabled = false;

            if ( _firstAmountBox != null )
            {
                Keyboard.Focus( _firstAmountBox );
            }
        }

        /// <summary>
        /// Updates the progress bars and updates Next/Complete for the capture workflow
        /// </summary>
        private void UpdateCaptureProgress()
        {
            var currentFinancialBatch = this._batchPage.SelectedFinancialBatch;

            var uploadedTotalCount = currentFinancialBatch.Transactions.Count();
            var uploadedTotalAmount = currentFinancialBatch.Transactions.Sum( a => a.TransactionDetails.Sum( d => d.Amount ) );

            pbControlAmounts.Value = ( double ) uploadedTotalAmount;
            pbControlItems.Value = uploadedTotalCount;
            this.lblControlAmountProgressMessage.Content = $"{uploadedTotalAmount:C} of {currentFinancialBatch.ControlAmount:C}";
            this.lblControlItemCountProgressMessage.Content = $"{uploadedTotalCount} of {currentFinancialBatch.ControlItemCount}";
            var controlLabelStyle = Application.Current.Resources["labelStyleSmall"] as Style;
            var controlLabelStyleError = Application.Current.Resources["labelStyleSmallError"] as Style;

            this.lblControlAmountProgressMessage.Style = controlLabelStyle;
            this.lblControlItemCountProgressMessage.Style = controlLabelStyle;

            var rockConfig = RockConfig.Load();

            bool controlAmountExceeded = ( uploadedTotalAmount > currentFinancialBatch.ControlAmount ) && rockConfig.RequireControlAmount;
            bool controlCountExceeded = ( uploadedTotalCount > currentFinancialBatch.ControlItemCount ) && rockConfig.RequireControlItemCount;

            bool controlAmountMatched = uploadedTotalAmount == currentFinancialBatch.ControlAmount;
            bool controlCountMatched = uploadedTotalCount == currentFinancialBatch.ControlItemCount;

            bool controlTotalsMatched = false;

            if ( rockConfig.RequireControlItemCount && rockConfig.RequireControlAmount )
            {
                controlTotalsMatched = controlAmountMatched && controlCountMatched;
            }
            else if ( rockConfig.RequireControlItemCount == true )
            {
                controlTotalsMatched = controlCountMatched;
            }
            else if ( rockConfig.RequireControlAmount == true )
            {
                controlTotalsMatched = controlAmountMatched;
            }

            if ( controlTotalsMatched )
            {
                HideStartScanningPrompts();
                DisplayAlertMessage( AlertMessageType.Info, "Control totals match. Press Complete to return to the main screen." );
                btnNext.Visibility = Visibility.Collapsed;
                btnComplete.Visibility = Visibility.Visible;
                StopScanning();
            }
            else if ( controlAmountExceeded || controlCountExceeded )
            {
                //// if either of the control totals are exceeded. Prevent them to return to the main screen to adjust control or transactions.
                //// This will allow them to fix stuff either to make things match, or to continue scanning after increasing control amounts.

                HideStartScanningPrompts();
                List<string> controlTotalsWarningList = new List<string>();
                if ( controlAmountExceeded && controlCountExceeded )
                {
                    controlTotalsWarningList.Add( "Control values exceeded." );
                    this.lblControlAmountProgressMessage.Style = controlLabelStyleError;
                    this.lblControlItemCountProgressMessage.Style = controlLabelStyleError;
                }
                else if ( controlCountExceeded )
                {
                    controlTotalsWarningList.Add( "Control item count exceeded." );
                    this.lblControlItemCountProgressMessage.Style = controlLabelStyleError;
                }
                else if ( controlAmountExceeded )
                {
                    controlTotalsWarningList.Add( "Control amount exceeded." );
                    this.lblControlAmountProgressMessage.Style = controlLabelStyleError;
                }

                DisplayAlertMessage( AlertMessageType.Warning, controlTotalsWarningList.JoinStrings( " and " ) + Environment.NewLine + "Press Complete to return to the main screen to make adjustments to transactions or control values." );
                btnNext.Visibility = Visibility.Collapsed;
                btnStart.Visibility = Visibility.Collapsed;
                btnComplete.Visibility = Visibility.Visible;
                StopScanning();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum UploadResult
        {
            Uploaded,
            Skipped
        }

        /// <summary>
        /// Shows the upload success.
        /// </summary>
        private void ShowUploadResult( UploadResult uploadResult )
        {
            if ( uploadResult == UploadResult.Uploaded )
            {
                lblScanItemUploadSuccess.Visibility = Visibility.Visible;
                lblScanItemUploadSkipped.Visibility = Visibility.Collapsed;
            }

            if ( uploadResult == UploadResult.Skipped )
            {
                lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
                lblScanItemUploadSkipped.Visibility = Visibility.Visible;
            }

            pnlPromptForUpload.Visibility = Visibility.Collapsed;
            lvAccountDetailsEntry.Visibility = Visibility.Collapsed;

            if ( lvAccountDetailsDisplay.ItemsSource != null )
            {
                CollectionViewSource.GetDefaultView( lvAccountDetailsDisplay.ItemsSource )?.Refresh();
            }

            lvAccountDetailsDisplay.Visibility = Visibility.Visible;

            btnNext.Visibility = Visibility.Collapsed;

            var rockConfig = RockConfig.Load();
            if ( rockConfig.CaptureAmountOnScan )
            {
                UpdateCaptureProgress();
            }
        }

        /// <summary>
        /// Hides the upload success/skipped messages
        /// </summary>
        private void HideUploadResultMessage()
        {
            lblScanItemUploadSkipped.Visibility = Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the scanned document information.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void DisplayScannedDocInfo( ScannedDocInfo scannedDocInfo )
        {
            if ( scannedDocInfo.FrontImageData != null )
            {
                this.spScannedItemDisplay.Visibility = Visibility.Visible;
                BitmapImage bitmapImageFront = new BitmapImage();
                bitmapImageFront.BeginInit();
                bitmapImageFront.StreamSource = new MemoryStream( scannedDocInfo.FrontImageData );
                bitmapImageFront.EndInit();
                imgScannedItemFront.Source = bitmapImageFront;
                imgFrontThumb.Source = bitmapImageFront;
                Rock.Wpf.WpfHelper.FadeIn( imgScannedItemFront, 100 );
            }
            else
            {
                imgScannedItemFront.Source = null;
            }

            if ( scannedDocInfo.BackImageData != null )
            {
                grdImageThumbnailsButtons.Visibility = Visibility.Visible;
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedDocInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgScannedItemBack.Source = bitmapImageBack;
                imgBackThumb.Source = bitmapImageBack;
            }
            else
            {
                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
            }

            spScannedItemDisplay.Visibility = Visibility.Visible;

            if ( scannedDocInfo.IsCheck )
            {
                gCheckMICRInfo.Visibility = Visibility.Visible;
                lblMicrRoutingAccountValue.Content = string.Format( "{0} / {1}", scannedDocInfo.RoutingNumber, scannedDocInfo.AccountNumber );
                lblMicrCheckNumber.Content = scannedDocInfo.CheckNumber;
                lblMicrOtherData.Content = scannedDocInfo.OtherData;
                spMicrOtherData.Visibility = !string.IsNullOrWhiteSpace( scannedDocInfo.OtherData ) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            if ( _interfaceType == RockConfig.InterfaceType.RangerApi )
            {
                // ranger will continue scanning all items in the hopper (when in non-capture mode), so we need to have a stop scanning button if they want to stop it before it is done scanning all items.
                btnStopScanning.Visibility = Visibility.Visible;
            }
            else
            {
                this.btnStopScanning.Visibility = Visibility.Collapsed;
            }

            var rockConfig = RockConfig.Load();
            this._interfaceType = rockConfig.ScannerInterfaceType;
            this._isBackScan = false;
            this._currentMagtekScannedDoc = null;
            this.gCheckMICRInfo.Visibility = Visibility.Collapsed;
            this.spScannedItemDisplay.Visibility = Visibility.Collapsed;
            this.spAccounts.Visibility = Visibility.Collapsed;

            // set the uploadScannedItemClient to null and reconnect to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            ScanningPageUtility.UploadScannedItemClient = null;
            ScanningPageUtility.EnsureUploadScanRestClient();
            ScanningPageUtility.Initialize();

            ShowStartScanningPrompts();
            ShowUploadStats();

            if ( rockConfig.CaptureAmountOnScan )
            {
                LoadAccounts();
                grdCaptureProgressBars.Visibility = Visibility.Visible;

                spControlItemProgressbar.Visibility = ( rockConfig.RequireControlItemCount && _batchPage.SelectedFinancialBatch.ControlItemCount.HasValue ) ? Visibility.Visible : Visibility.Collapsed;
                spControlAmountProgressBar.Visibility = rockConfig.RequireControlAmount ? Visibility.Visible : Visibility.Collapsed;

                colMain.Width = new GridLength( 2, GridUnitType.Star );
                colAmounts.Width = new GridLength( 1, GridUnitType.Star );

                // Each scan will prompt for amounts, so there is not need to tell the device to stop (continuous) scanning
                this.btnStopScanning.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdCaptureProgressBars.Visibility = Visibility.Collapsed;
                colMain.Width = new GridLength( 1, GridUnitType.Star );
                colAmounts.Width = new GridLength( 0, GridUnitType.Pixel );
            }

            lblScanItemCountInfo.Visibility = Visibility.Collapsed;
            pbControlItems.Maximum = this._batchPage.SelectedFinancialBatch?.ControlItemCount ?? 0;
            pbControlAmounts.Maximum = ( double ) ( this._batchPage.SelectedFinancialBatch?.ControlAmount ?? 0.00M );

            if ( rockConfig.CaptureAmountOnScan )
            {
                UpdateCaptureProgress();
            }

            if ( ScanningPageUtility.KeepScanning )
            {
                StartScanning();
            }
        }

        /// <summary>
        /// Loads the accounts.
        /// </summary>
        private void LoadAccounts()
        {
            List<DisplayAccountValueModel> sortedDisplayedAccountList = ScanningPageUtility.GetVisibleAccountsSortedAndFlattened();

            this.lvAccountDetailsEntry.ItemsSource = sortedDisplayedAccountList;
            this.lvAccountDetailsDisplay.ItemsSource = sortedDisplayedAccountList;
        }

        /// <summary>
        /// Hides the start scanning prompts.
        /// </summary>
        private void HideStartScanningPrompts()
        {
            this.lblStartInfo.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the start scanning prompts.
        /// </summary>
        private void ShowStartScanningPrompts()
        {
            var rockConfig = RockConfig.Load();
            HideUploadWarningPrompts();
            this.HideUploadResultMessage();
            lblAlert.Visibility = Visibility.Collapsed;
            lblExceptions.Visibility = Visibility.Collapsed;
            lblStartInfo.Visibility = Visibility.Visible;
            btnStart.Visibility = Visibility.Visible;
            btnNext.Visibility = Visibility.Collapsed;
            btnComplete.Visibility = Visibility.Collapsed;

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                lblStartInfo.Content = string.Format( "Ready to scan next {0}.", scanningChecks ? "check" : "item" );
                lblStartInfo.Visibility = Visibility.Visible;

                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
                btnStopScanning.Visibility = Visibility.Collapsed;
                btnStart.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                lblStartInfo.Content = "Click Start to begin";
            }

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MagTekImageSafe )
            {
                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
                btnStopScanning.Visibility = Visibility.Collapsed;
                btnStart.IsEnabled = true;
                return;
            }

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = this._batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );
        }

        /// <summary>
        /// Shows the scanner status.
        /// </summary>
        /// <param name="statusColor">Color of the status.</param>
        /// <param name="statusText">The status text.</param>
        internal void ShowScannerStatus( System.Windows.Media.Color statusColor, string statusText )
        {
            shapeStatus.ToolTip = statusText;
            shapeStatus.Fill = new System.Windows.Media.SolidColorBrush( statusColor );
        }

        #region Ranger (Canon CR50/80) Scanner Events

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            RangerFeedingStoppedReasons rangerFeedingStoppedReason = ( RangerFeedingStoppedReasons ) e.reason;

            bool promptingForSomething = btnNext.Visibility == Visibility.Visible || pnlPromptForUpload.Visibility == Visibility.Visible;

            if ( !promptingForSomething )
            {
                btnStart.IsEnabled = true;
            }

            var rockConfig = RockConfig.Load();

            btnClose.IsEnabled = true;
            if ( !promptingForSomething )
            {
                btnStopScanning.IsEnabled = false;
            }

            if ( ScanningPageUtility.ItemsScanned == 0 && ScanningPageUtility.KeepScanning )
            {
                // show the Startup Info "Welcome" message if no check images are shown yet
                if ( spScannedItemDisplay.Visibility != Visibility.Visible )
                {
                    lblStartInfo.Visibility = Visibility.Visible;
                }

                // show a "No Items" warning if they clicked Start but it stopped because of MainHopperEmpty
                if ( rangerFeedingStoppedReason == RangerFeedingStoppedReasons.MainHopperEmpty )
                {
                    bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                    var noItemfound = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
                    DisplayAlertMessage( AlertMessageType.Warning, noItemfound );
                }
            }

            if ( rockConfig.CaptureAmountOnScan )
            {
                UpdateCaptureProgress();
            }
        }

        /// <summary>
        /// Handles the TransportNewItem event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportNewItem( object sender, EventArgs e )
        {
            ScanningPageUtility.ItemsScanned++;
        }

        /// <summary>
        /// Handles the TransportFeedingState event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportFeedingState( object sender, EventArgs e )
        {
            lblStartInfo.Visibility = Visibility.Collapsed;
            HideAlertMessage();
            btnStart.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnStopScanning.IsEnabled = true;

            RockConfig rockConfig = RockConfig.Load();

            if ( rockConfig.CaptureAmountOnScan )
            {
                UpdateCaptureProgress();
            }
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportSetItemOutput( object sender, AxRANGERLib._DRangerEvents_TransportSetItemOutputEvent e )
        {
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                this._batchPage.micrImage.ClearBuffer();
                return;
            }

            try
            {
                HideStartScanningPrompts();
                HideUploadResultMessage();

                this.lblAlert.Visibility = Visibility.Collapsed;

                RockConfig rockConfig = RockConfig.Load();

                ScannedDocInfo scannedDoc = new ScannedDocInfo();

                // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                scannedDoc.Upload = true;
                scannedDoc.CurrencyTypeValue = this._batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = this._batchPage.SelectedSourceTypeValue;

                scannedDoc.FrontImageData = ScanningPageUtility.GetImageBytesFromRanger( RangerSides.TransportFront );

                if ( rockConfig.EnableRearImage )
                {
                    scannedDoc.BackImageData = ScanningPageUtility.GetImageBytesFromRanger( RangerSides.TransportRear );
                }

                if ( scannedDoc.IsCheck )
                {
                    string checkMicr = this._batchPage.rangerScanner.GetMicrText( 1 );
                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), checkMicr ) );
                    string remainingMicr = checkMicr;
                    string accountNumber = string.Empty;
                    string routingNumber = string.Empty;
                    string checkNumber = string.Empty;

                    // there should always be two transit symbols ('d').  The transit number is between them
                    int transitSymbol1 = remainingMicr.IndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                    int transitSymbol2 = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                    int transitStart = transitSymbol1 + 1;
                    int transitLength = transitSymbol2 - transitSymbol1 - 1;
                    if ( transitLength > 0 )
                    {
                        routingNumber = remainingMicr.Substring( transitStart, transitLength );
                        remainingMicr = remainingMicr.Remove( transitStart - 1, transitLength + 2 );
                    }

                    char[] separatorSymbols = new char[] { ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol, ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ( char ) RangerE13BMicrSymbols.E13B_AmountSymbol };

                    // the last 'On-Us' symbol ('c') signifies the end of the account number
                    int lastOnUsPosition = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol );
                    if ( lastOnUsPosition > 0 )
                    {
                        int accountNumberDigitPosition = lastOnUsPosition - 1;

                        // read all digits to the left of the last 'OnUs' until you run into another seperator symbol
                        while ( accountNumberDigitPosition >= 0 )
                        {
                            char accountNumberDigit = remainingMicr[accountNumberDigitPosition];
                            if ( separatorSymbols.Contains( accountNumberDigit ) )
                            {
                                break;
                            }
                            else
                            {
                                accountNumber = accountNumberDigit + accountNumber;
                                accountNumber = accountNumber.Trim();
                            }

                            accountNumberDigitPosition--;
                        }

                        remainingMicr = remainingMicr.Remove( accountNumberDigitPosition + 1, lastOnUsPosition - accountNumberDigitPosition );
                    }

                    // any remaining digits that aren't the account number and transit number are probably the check number
                    string[] remainingMicrParts = remainingMicr.Split( new char[] { ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    string otherData = null;
                    if ( remainingMicrParts.Any() )
                    {
                        // Now that we've indentified Routing and AccountNumber, the remaining MICR part is probably the CheckNumber. However, there might be multiple Parts left. We'll have to make a best guess on which chunk is the CheckNumber.
                        // In those cases, assume the 'longest' chunk to the CheckNumber. (Other chunks tend to be short 1 or 2 digit numbers that mean something special to the bank)
                        checkNumber = remainingMicrParts.OrderBy( p => p.Length ).Last();

                        // throw any remaining data into 'otherData' (a reject symbol could be in the other data)
                        remainingMicr = remainingMicr.Replace( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' );
                        remainingMicr = remainingMicr.Replace( checkNumber, string.Empty );
                        otherData = remainingMicr;
                    }

                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;
                    scannedDoc.OtherData = otherData;

                    scannedDoc.ScannedCheckMicrData = checkMicr;

                    // look for the "can't read" symbol (or completely blank read ) to detect if the check micr couldn't be read
                    // from http://www.sbulletsupport.com/forum/index.php?topic=172.0
                    if ( checkMicr.Contains( ( char ) RangerCommonSymbols.RangerRejectSymbol ) || string.IsNullOrWhiteSpace( checkMicr ) )
                    {
                        scannedDoc.BadMicr = true;
                        scannedDoc.Upload = false;
                    }
                }

                ShowScannedDocStatusAndUpload( scannedDoc );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPageUtility.ShowException( ( ex as AggregateException ).Flatten(), this.lblExceptions );
                }
                else
                {
                    ScanningPageUtility.ShowException( ex, this.lblExceptions );
                }
            }
        }

        /// <summary>
        /// Handles the TransportIsDead event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportIsDead( object sender, EventArgs e )
        {
            Debug.WriteLine( "rangerScanner_TransportIsDead" );
            ScanningPageUtility.rangerScanner_TransportIsDead( sender, e, () =>
            {
                DisplayAlertMessage( AlertMessageType.Warning, "Scanner is not ready. Verify that the scanner is powered on and connected." );
            } );
        }

        #endregion

        #region Scanner (MagTek MICRImage RS232) Events

        /// <summary>
        /// Handles the MicrDataReceived event of the micrImage (RS232) control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void micrImage_MicrDataReceived( object sender, System.EventArgs e )
        {
            HideStartScanningPrompts();
            HideUploadResultMessage();

            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                this._batchPage.micrImage.ClearBuffer();
                return;
            }

            // from MagTek Sample Code
            object dummy = null;
            string routingNumber = this._batchPage.micrImage.FindElement( 0, "T", 0, "TT", ref dummy );
            string accountNumber = this._batchPage.micrImage.FindElement( 0, "TT", 0, "A", ref dummy );
            string checkNumber = this._batchPage.micrImage.FindElement( 0, "A", 0, "12", ref dummy );
            short trackNumber = 0;
            var rawMICR = this._batchPage.micrImage.GetTrack( ref trackNumber );

            ScannedDocInfo scannedDoc = null;
            var rockConfig = RockConfig.Load();
            bool scanningMagTekBackImage = false;

            if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
            {
                //// if we didn't get a routingnumber, and we are expecting a back scan, use the scan as the back image
                //// However, if we got a routing number, assuming we are scanning a new check regardless

                if ( string.IsNullOrWhiteSpace( routingNumber ) )
                {
                    scanningMagTekBackImage = true;
                }
                else
                {
                    scanningMagTekBackImage = false;
                }
            }

            if ( scanningMagTekBackImage )
            {
                scannedDoc = _currentMagtekScannedDoc;
            }
            else
            {
                ScanningPageUtility.ItemsScanned++;
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = this._batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = this._batchPage.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck )
                {
                    scannedDoc.ScannedCheckMicrData = rawMICR;
                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;

                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), scannedDoc.ScannedCheckMicrData ) );
                }

                // set the _currentMagtekScannedDoc in case we are going to scan the back of the image
                _currentMagtekScannedDoc = scannedDoc;
            }

            string imagePath = Path.GetTempPath();
            string docImageFileName = Path.Combine( imagePath, string.Format( "scanned_item_{0}.tif", Guid.NewGuid() ) );
            if ( File.Exists( docImageFileName ) )
            {
                File.Delete( docImageFileName );
            }

            try
            {
                string statusMsg = string.Empty;

                // Writing To File 
                this._batchPage.micrImage.TransmitCurrentImage( docImageFileName, ref statusMsg );
                byte[] imageData;

                if ( File.Exists( docImageFileName ) )
                {
                    imageData = File.ReadAllBytes( docImageFileName );
                }
                else
                {
                    imageData = null;
                }

                if ( scanningMagTekBackImage )
                {
                    scannedDoc.BackImageData = imageData;
                }
                else
                {
                    scannedDoc.FrontImageData = imageData;

                    // MagTek puts the symbol '?' for parts of the MICR that it can't read
                    bool gotValidMicr = !string.IsNullOrWhiteSpace( scannedDoc.AccountNumber ) && !scannedDoc.AccountNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.RoutingNumber ) && !scannedDoc.RoutingNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.CheckNumber ) && !scannedDoc.CheckNumber.Contains( '?' );

                    if ( scannedDoc.IsCheck && !gotValidMicr )
                    {
                        scannedDoc.BadMicr = true;
                    }
                }

                bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                if ( scannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
                {
                    // scanning the front image, but still need to scan the back
                    HideUploadWarningPrompts();

                    // scanning the front image, but still need to scan the back
                    var message = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "check" : "item" );
                    this.DisplayAlertMessage( AlertMessageType.Info, message );
                    DisplayScannedDocInfo( scannedDoc );
                }
                else
                {
                    // scanned both sides (or just the front if they don't want to scan both sides )
                    scannedDoc.Upload = !scannedDoc.IsCheck || !( scannedDoc.BadMicr || scannedDoc.Duplicate );
                    this.ShowScannedDocStatusAndUpload( scannedDoc );
                }

                File.Delete( docImageFileName );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPageUtility.ShowException( ( ex as AggregateException ).Flatten(), this.lblExceptions );
                }
                else
                {
                    ScanningPageUtility.ShowException( ex, this.lblExceptions );
                }
            }
            finally
            {
                this._batchPage.micrImage.ClearBuffer();
            }
        }

        #endregion

        #region MagTek USB

        /// <summary>
        /// Images the safe callback.
        /// </summary>
        /// <param name="e">The e.</param>
        private void imageSafeCallback( CheckData e )
        {
            HideStartScanningPrompts();
            HideUploadResultMessage();

            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                this._batchPage.micrImage.ClearBuffer();
                return;
            }

            ScannedDocInfo scannedDoc = _currentMagtekScannedDoc;
            var rockConfig = RockConfig.Load();
            bool scanningImageSafeBackImage = false;
            if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
            {
                //// if we didn't get a routingnumber, and we are expecting a back scan, use the scan as the back image
                //// However, if we got a routing number, assuming we are scanning a new check regardless

                if ( string.IsNullOrWhiteSpace( e.RoutingNumber ) )
                {
                    scanningImageSafeBackImage = true;
                }
                else
                {
                    scanningImageSafeBackImage = false;
                }
            }

            if ( scanningImageSafeBackImage )
            {
                scannedDoc = _currentMagtekScannedDoc;
            }
            else
            {
                ScanningPageUtility.ItemsScanned++;
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = this._batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = this._batchPage.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck && !e.HasError )
                {
                    scannedDoc.ScannedCheckMicrData = e.ScannedCheckMicrData;
                    scannedDoc.RoutingNumber = e.RoutingNumber;
                    scannedDoc.AccountNumber = e.AccountNumber;
                    scannedDoc.CheckNumber = e.CheckNumber;
                    scannedDoc.ScannedCheckMicrData = e.ScannedCheckMicrData;
                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), scannedDoc.ScannedCheckMicrData ) );
                }
            }

            // set the _currentMagtekScannedDoc in case we are going to scan the back of the image
            _currentMagtekScannedDoc = scannedDoc;

            bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            if ( e.HasError )
            {
                var timeoutError = e.ErrorMessage.Contains( "Timeout" );
                if ( timeoutError )
                {
                    var noItemfound = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
                    DisplayAlertMessage( AlertMessageType.Warning, noItemfound );
                }
                else
                {
                    DisplayAlertMessage( AlertMessageType.Danger, e.ErrorMessage );
                }

                return;
            }

            // Bad MICR Read
            if ( scanningChecks )
            {
                if ( !_isBackScan && ( e.ScannedCheckMicrData == null || e.ScannedCheckMicrData.Contains( "?" ) ) )
                {
                    scannedDoc.BadMicr = true;
                }
            }

            // Image failed to capture
            scannedDoc.ImageFailure = e.ImageData == null;

            // We set the back scan when we scan front after prompt and are scanning the back
            if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.FrontImageData != null )
            {
                _currentMagtekScannedDoc.BackImageData = e.ImageData;
            }
            else
            {
                _currentMagtekScannedDoc.FrontImageData = scannedDoc.FrontImageData;
            }

            if ( !scannedDoc.ImageFailure && ( _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage ) )
            {
                // scanning the front image, but still need to scan the back
                HideUploadWarningPrompts();

                // scanning the front image, but still need to scan the back
                var message = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "checks" : "items" );
                this.DisplayAlertMessage( AlertMessageType.Info, message );
                btnStart.IsEnabled = true;

                _currentMagtekScannedDoc.FrontImageData = e.ImageData;
                _isBackScan = true;
                DisplayScannedDocInfo( _currentMagtekScannedDoc );
            }
            else
            {
                //// Non Prompt 
                // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                scannedDoc.Upload = true;
                scannedDoc.CurrencyTypeValue = this._batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = this._batchPage.SelectedSourceTypeValue;

                if ( _isBackScan )
                {
                    scannedDoc = _currentMagtekScannedDoc;
                    scannedDoc.BackImageData = e.ImageData;
                }
                else
                {
                    // Check Bad Read without prompt
                    if ( scannedDoc.IsCheck && e.ScannedCheckMicrData.Contains( "?" ) )
                    {
                        scannedDoc.BadMicr = true;
                    }

                    scannedDoc.FrontImageData = e.ImageData;
                }

                _isBackScan = false;

                ShowScannedDocStatusAndUpload( scannedDoc );
            }
        }

        #endregion

        #region Image Upload related

        #endregion

        /// <summary>
        /// Handles the Click event of the BtnComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnComplete_Click( object sender, RoutedEventArgs e )
        {
            if ( ProcessIgnoreUploadResponse( UploadResponse.UploadWithAmount ) )
            {
                btnClose_Click( null, null );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnClose_Click( object sender, RoutedEventArgs e )
        {
            this._batchPage.UpdateBatchUI( this._batchPage.SelectedFinancialBatch );
            this.NavigationService.Navigate( this._batchPage );
        }

        /// <summary>
        /// Shows the upload stats.
        /// </summary>
        private void ShowUploadStats()
        {
            List<string> statsList = new List<string>();
            if ( ScanningPageUtility.ItemsUploaded > 0 )
            {
                statsList.Add( string.Format( "Uploaded: {0}", ScanningPageUtility.ItemsUploaded ) );
            }

            if ( ScanningPageUtility.ItemsSkipped > 0 )
            {
                statsList.Add( string.Format( "Skipped: {0}", ScanningPageUtility.ItemsSkipped ) );
            }

            lblScanItemCountInfo.Visibility = statsList.Any() ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemCountInfo.Content = string.Join( ", ", statsList );
        }

        /// <summary>
        /// Handles the Click event of the BtnImageToggle_FrontBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnImageToggle_FrontBack_Click( object sender, RoutedEventArgs e )
        {
            var parameter = ( ( Button ) e.OriginalSource ).CommandParameter as string;
            switch ( parameter )
            {
                case "Front":
                    this.imgScannedItemBack.Visibility = Visibility.Collapsed;
                    this.imgScannedItemFront.Visibility = Visibility.Visible;
                    break;
                case "Back":
                    this.imgScannedItemBack.Visibility = Visibility.Visible;
                    this.imgScannedItemFront.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            this.lblAlert.Visibility = Visibility.Collapsed;

            var rockConfig = RockConfig.Load();
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
            {
                RangerTransportStates[] xportStatesNotConnected = new RangerTransportStates[] {
                    RangerTransportStates.TransportShutDown,
                    RangerTransportStates.TransportShuttingDown,
                    RangerTransportStates.TransportExceptionInProgress,
                    RangerTransportStates.TransportChangeOptions };

                var transportState = ( RangerTransportStates ) this._batchPage.rangerScanner.GetTransportState();
                if ( xportStatesNotConnected.Contains( transportState ) )
                {
                    this._batchPage.ConnectToScanner();
                }
                else
                {
                    StartScanning();
                }
            }
            else
            {
                if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MagTekImageSafe )
                {
                    ImageSafeHelper.ProcessDocument( imageSafeCallback );
                }
            }
        }

        /// <summary>
        /// Starts the scanning as soon as items are in the hopper
        /// </summary>
        public void StartScanning()
        {
            ScanningPageUtility.KeepScanning = true;

            if ( this._interfaceType == RockConfig.InterfaceType.RangerApi )
            {
                ResumeRangerScanning();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStopScanning control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopScanning_Click( object sender, RoutedEventArgs e )
        {
            StopScanning();
            btnStart.IsEnabled = true;
        }

        /// <summary>
        /// Stops the scanning.
        /// </summary>
        private void StopScanning()
        {
            ScanningPageUtility.KeepScanning = false;
            if ( this._batchPage.rangerScanner != null )
            {
                // remove the StartRangerFeedingWhenReady (in case it is assigned) so it doesn't restart after getting into ReadyToFeed state
                this._batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
                this._batchPage.rangerScanner.StopFeeding();
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
            this._batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;

            if ( ScanningPageUtility.KeepScanning )
            {
                this._batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOptions_Click( object sender, RoutedEventArgs e )
        {
            this._batchPage.NavigateToOptionsPage();
        }

        /// <summary>
        /// Hides the alert message.
        /// </summary>
        private void HideAlertMessage()
        {
            this.lblAlert.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the alert message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The message.</param>
        private void DisplayAlertMessage( AlertMessageType messageType, string message )
        {
            this.lblAlert.Visibility = message.IsNotNullOrWhiteSpace() ? Visibility.Visible : Visibility.Collapsed;
            this.lblAlert.AlertType = messageType;
            this.lblAlert.Message = message;
        }

        #region Image Capture Related

        /// <summary>
        /// Handles the LostKeyboardFocus event of the TbAccountDetailAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
        private void TbAccountDetailAmount_LostKeyboardFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
        {
            HandleDetailAmountChange( sender );
        }

        /// <summary>
        /// Handles the KeyUp event of the TbAccountDetailAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TbAccountDetailAmount_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if ( e.Key == System.Windows.Input.Key.Decimal )
            {
                return;
            }

            HandleDetailAmountChange( sender );

            if ( e.Key == System.Windows.Input.Key.Return && !string.IsNullOrWhiteSpace( ( ( CurrencyBox ) sender ).Text ) )
            {
                btnNext.Focus();

                ProcessIgnoreUploadResponse( UploadResponse.UploadWithAmount );
            }
        }

        private CurrencyBox _firstAmountBox = null;

        /// <summary>
        /// Handles the Loaded event of the TbAccountDetailAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void TbAccountDetailAmount_Loaded( object sender, RoutedEventArgs e )
        {
            // set focus to the first amount entry box
            CurrencyBox currencyBox = sender as CurrencyBox;
            if ( ( currencyBox?.DataContext as DisplayAccountValueModel )?.DisplayIndex == 0 )
            {
                _firstAmountBox = currencyBox;
            }
        }

        /// <summary>
        /// Handles the detail amount change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void HandleDetailAmountChange( object sender )
        {
            var tbAccountDetailAmount = sender as TextBox;
            List<DisplayAccountValueModel> displayAccountValueModels = lvAccountDetailsEntry.ItemsSource as List<DisplayAccountValueModel>;
            DisplayAccountValueModel editingDisplayAccountValueModel = tbAccountDetailAmount.DataContext as DisplayAccountValueModel;
            var displayFinancialTransactionDetail = displayAccountValueModels.FirstOrDefault( a => a.AccountId == editingDisplayAccountValueModel.AccountId );
            var otherAccounts = displayAccountValueModels.Where( a => a.AccountId != editingDisplayAccountValueModel.AccountId && a.Amount.HasValue ).Sum( a => a.Amount.Value );
            var editingAmount = tbAccountDetailAmount.Text.AsDecimalOrNull();
            var totalDetailAmounts = otherAccounts + ( editingAmount ?? 0.00M );

            lblTotals.Content = totalDetailAmounts.ToString( "C" );
        }

        #endregion
    }
}