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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Rock.Client;
using Rock.Client.Enums;
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
        /// Gets or sets the name of the log file.
        /// If specified, the raw MICR scans will be written to this this
        /// </summary>
        /// <value>
        /// The name of the log file.
        /// </value>
        public string DebugLogFilePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningPage"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScanningPage( BatchPage value )
        {
            InitializeComponent();
            this.batchPage = value;

            try
            {
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                DebugLogFilePath = config.AppSettings.Settings["DebugLogFilePath"].Value;
                bool isDirectory = !string.IsNullOrWhiteSpace(DebugLogFilePath) && Directory.Exists( this.DebugLogFilePath );
                if (isDirectory)
                {
                    DebugLogFilePath = Path.Combine( DebugLogFilePath, "CheckScanner.log" );
                }
            }
            catch
            {
                // ignore any exceptions
            }
        }

        /// <summary>
        /// Writes the message to a debug log (if DebugLogFilePath is configured in AppSettings)
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteToDebugLog( string message )
        {
            if ( !string.IsNullOrWhiteSpace( this.DebugLogFilePath ) )
            {
                try
                {
                    
                    File.AppendAllText( DebugLogFilePath, message + Environment.NewLine );
                }
                catch
                {
                    //
                }
            }
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
            App.LogException( ex );
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

            var rockConfig = RockConfig.Load();

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            // if they don't enable smart scan, don't warn about bad micr's. For example, they might be scanning a mixture of checks and envelopes
            if ( rockConfig.EnableSmartScan )
            {
                if ( scannedDocInfo.BadMicr )
                {
                    if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
                    {
                        lblScanCheckWarningBadMicr.Content = @"Unable to read check information
    Click 'Skip' to reject this check and continue scanning. To retry this check, put the check back into the feed tray.   
    Click 'Upload' to upload the check as-is.
    Click 'Stop' to reject this check and stop scanning.";
                    }
                    else
                    {
                        lblScanCheckWarningBadMicr.Content = @"Unable to read check information
    Click 'Skip' to reject this check.    
    Click 'Upload' to upload the check as-is.";
                    }
                }
            }
            else
            {
                // if Enable Smart Scan is disabled, upload even if there is a bad or missing MICR
                if ( !scannedDocInfo.Upload )
                {
                    scannedDocInfo.Upload = true;
                }
            }

            if ( scannedDocInfo.Upload && IsDuplicateScan( scannedDocInfo ) )
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
        /// Shows the upload warnings.
        /// </summary>
        private void ShowUploadWarnings( ScannedDocInfo scannedDocInfo )
        {
            var rockConfig = RockConfig.Load();
            ConfirmUploadBadScannedDoc = scannedDocInfo;
            lblScanCheckWarningDuplicate.Visibility = scannedDocInfo.Duplicate ? Visibility.Visible : Visibility.Collapsed;
            lblScanCheckWarningBadMicr.Visibility = scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = scannedDocInfo.Duplicate || scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            btnStart.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
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
            else
            {
                // update the bad micr warning to not include the btn instructions
                lblScanCheckWarningBadMicr.Content = @"Unable to read check information";
            }

            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
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
            // set the uploadScannedItemClient to null and reconnect to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            _uploadScannedItemClient = null;
            EnsureUploadScanRestClient();

            ShowStartupPage();
            _itemsUploaded = 0;
            _itemsSkipped = 0;
            _itemsScanned = 0;
            _firstNoItemsWarning = true;
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
            lblNoItemsFound.Visibility = Visibility.Collapsed;
            lblScannerNotReady.Visibility = Visibility.Collapsed;

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );

            bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            lblNoItemsFound.Content = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
            lblScanBackInstructions.Content = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "check" : "item" );
            lblScanBackInstructions.Visibility = Visibility.Collapsed;
            if ( RockConfig.Load().ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                lblStartupInfo.Content = string.Format( "Ready to scan next {0}.", scanningChecks ? "check" : "item" );
                
                // no need for a stop/start button when in MagTek mode
                btnStart.Visibility = Visibility.Hidden;
                btnStopScanning.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStart.Visibility = Visibility.Visible;
                btnStopScanning.Visibility = Visibility.Visible;
                lblStartupInfo.Content = "Click Start to begin";
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
                    break;

                case XportStates.TransportFeeding:
                    break;
            }

            this.shapeStatus.ToolTip = statusText;
            this.shapeStatus.Fill = new System.Windows.Media.SolidColorBrush( statusColor );
        }

        #region Ranger (Canon CR50/80) Scanner Events

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportFeedingStopped", DateTime.Now.ToString( "o" ) ) );
            if ( pnlPromptForUpload.Visibility != Visibility.Visible )
            {
                btnStart.IsEnabled = true;
            }

            btnClose.IsEnabled = true;
            if ( pnlPromptForUpload.Visibility != Visibility.Visible )
            {
                btnStopScanning.IsEnabled = false;
            }
            
            if ( _itemsScanned == 0 )
            {
                // show the Startup Info "Welcome" message if no check images are shown yet
                if ( lblFront.Visibility != Visibility.Visible )
                {
                    lblStartupInfo.Visibility = Visibility.Visible;
                }

                // show a "No Items" warning if they clicked Start but no items were scanned, and this is the 2nd+ time they tried
                if ( !_firstNoItemsWarning )
                {
                    lblNoItemsFound.Visibility = Visibility.Visible;
                }

                _firstNoItemsWarning = false;
            }
        }

        /// <summary>
        /// Handles the TransportNewItem event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportNewItem( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportNewItem", DateTime.Now.ToString( "o" ) ) );
            _itemsScanned++;
        }

        /// <summary>
        /// Handles the TransportFeedingState event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportFeedingState( object sender, EventArgs e )
        {
            lblStartupInfo.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportFeedingState", DateTime.Now.ToString( "o" ) ) );
            lblNoItemsFound.Visibility = Visibility.Collapsed;
            btnStart.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportSetItemOutput( object sender, AxRANGERLib._DRangerEvents_TransportSetItemOutputEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportSetItemOutput", DateTime.Now.ToString( "o" ) ) );
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                batchPage.micrImage.ClearBuffer();
                return;
            }

            try
            {
                lblStartupInfo.Visibility = Visibility.Collapsed;

                RockConfig rockConfig = RockConfig.Load();

                ScannedDocInfo scannedDoc = new ScannedDocInfo();

                // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                scannedDoc.Upload = true;
                scannedDoc.CurrencyTypeValue = batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = batchPage.SelectedSourceTypeValue;

                scannedDoc.FrontImageData = GetImageBytesFromRanger( Sides.TransportFront );

                if ( rockConfig.EnableRearImage )
                {
                    scannedDoc.BackImageData = GetImageBytesFromRanger( Sides.TransportRear );
                }

                if ( scannedDoc.IsCheck )
                {
                    string checkMicr = batchPage.rangerScanner.GetMicrText( 1 );
                    WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), checkMicr ) );
                    string remainingMicr = checkMicr;
                    string accountNumber = string.Empty;
                    string routingNumber = string.Empty;
                    string checkNumber = string.Empty;

                    // there should always be two transit symbols ('d').  The transit number is between them
                    int transitSymbol1 = remainingMicr.IndexOf( 'd' );
                    int transitSymbol2 = remainingMicr.LastIndexOf( 'd' );
                    int transitStart = transitSymbol1 + 1;
                    int transitLength = transitSymbol2 - transitSymbol1 - 1;
                    if ( transitLength > 0 )
                    {
                        routingNumber = remainingMicr.Substring( transitStart, transitLength );
                        remainingMicr = remainingMicr.Remove( transitStart - 1, transitLength + 2 );
                    }

                    // the last 'On-Us' symbol ('c') signifies the end of the account number
                    int lastOnUsPosition = remainingMicr.LastIndexOf( 'c' );
                    if ( lastOnUsPosition > 0 )
                    {
                        int accountNumberDigitPosition = lastOnUsPosition - 1;

                        // read all digits to the left of the last 'c' until you run into another 'c' or 'd'
                        while ( accountNumberDigitPosition >= 0 )
                        {
                            char accountNumberDigit = remainingMicr[accountNumberDigitPosition];
                            if ( accountNumberDigit == 'c' || accountNumberDigit == 'd' )
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
                    string[] remainingMicrParts = remainingMicr.Split( new char[] { 'c', ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( remainingMicrParts.Any() )
                    {
                        checkNumber = remainingMicrParts.Last();
                    }

                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;

                    scannedDoc.ScannedCheckMicrData = checkMicr;

                    // look for the "can't read" symbol (or completely blank read ) to detect if the check micr couldn't be read
                    // from http://www.sbulletsupport.com/forum/index.php?topic=172.0
                    if ( checkMicr.Contains('!') || string.IsNullOrWhiteSpace(checkMicr) )
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
                    ShowException( ( ex as AggregateException ).Flatten() );
                }
                else
                {
                    ShowException( ex );
                }
            }
        }

        #endregion

        #region Scanner (MagTek MICRImage RS232) Events

        private ScannedDocInfo _currentMagtekScannedDoc { get; set; }

        /// <summary>
        /// Handles the MicrDataReceived event of the micrImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void micrImage_MicrDataReceived( object sender, System.EventArgs e )
        {
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                batchPage.micrImage.ClearBuffer();
                return;
            }

            lblStartupInfo.Visibility = Visibility.Collapsed;

            // from MagTek Sample Code
            object dummy = null;
            string routingNumber = batchPage.micrImage.FindElement( 0, "T", 0, "TT", ref dummy );
            string accountNumber = batchPage.micrImage.FindElement( 0, "TT", 0, "A", ref dummy );
            string checkNumber = batchPage.micrImage.FindElement( 0, "A", 0, "12", ref dummy );
            short trackNumber = 0;
            var rawMICR = batchPage.micrImage.GetTrack( ref trackNumber );

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
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = batchPage.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck )
                {
                    scannedDoc.ScannedCheckMicrData = rawMICR;
                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;

                    WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), scannedDoc.ScannedCheckMicrData ) );
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
                batchPage.micrImage.TransmitCurrentImage( docImageFileName, ref statusMsg );
                if ( !File.Exists( docImageFileName ) )
                {
                    throw new Exception( "Unable to retrieve image" );
                }

                if ( scanningMagTekBackImage )
                {
                    scannedDoc.BackImageData = File.ReadAllBytes( docImageFileName );
                }
                else
                {
                    scannedDoc.FrontImageData = File.ReadAllBytes( docImageFileName );

                    // MagTek puts the symbol '?' for parts of the MICR that it can't read
                    bool gotValidMicr = !string.IsNullOrWhiteSpace( scannedDoc.AccountNumber ) && !scannedDoc.AccountNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.RoutingNumber ) && !scannedDoc.RoutingNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.CheckNumber ) && !scannedDoc.CheckNumber.Contains( '?' );

                    if ( scannedDoc.IsCheck && !gotValidMicr )
                    {
                        scannedDoc.BadMicr = true;
                    }
                }

                if ( scannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
                {
                    // scanning the front image, but still need to scan the back
                    lblScanBackInstructions.Visibility = Visibility.Visible;
                    HideUploadWarningPrompts( true );
                    DisplayScannedDocInfo( scannedDoc );
                }
                else
                {
                    // scanned both sides (or just the front if they don't want to scan both sides )
                    lblScanBackInstructions.Visibility = Visibility.Collapsed;
                    scannedDoc.Upload = !scannedDoc.IsCheck || !( scannedDoc.BadMicr || scannedDoc.Duplicate );
                    this.ShowScannedDocStatusAndUpload( scannedDoc );
                }

                File.Delete( docImageFileName );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ShowException( ( ex as AggregateException ).Flatten() );
                }
                else
                {
                    ShowException( ex );
                }
            }
            finally
            {
                batchPage.micrImage.ClearBuffer();
            }
        }

        #endregion

        #region Image Upload related

        /// <summary>
        /// Gets the doc image.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        private byte[] GetImageBytesFromRanger( Sides side )
        {
            ImageColorType colorType = RockConfig.Load().ImageColorType;

            int imageByteCount;
            imageByteCount = batchPage.rangerScanner.GetImageByteCount( (int)side, (int)colorType );
            if ( imageByteCount > 0 )
            {
                byte[] imageBytes = new byte[imageByteCount];

                // create the pointer and assign the Ranger image address to it
                IntPtr imgAddress = new IntPtr( batchPage.rangerScanner.GetImageAddress( (int)side, (int)colorType ) );

                // Copy the bytes from unmanaged memory to managed memory
                Marshal.Copy( imgAddress, imageBytes, 0, imageByteCount );

                return imageBytes;
            }
            else
            {
                return null;
            }
        }

        #endregion

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
        private RockRestClient _uploadScannedItemClient { get; set; }

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

            var uploadClient = EnsureUploadScanRestClient();

            if ( uploadClient == null )
            {
                var rockConfig = RockConfig.Load();
                uploadClient = new RockRestClient( rockConfig.RockBaseUrl );
                uploadClient.Login( rockConfig.Username, rockConfig.Password );
            }

            var alreadyScanned = uploadClient.PostDataWithResult<string, bool>( "api/FinancialTransactions/AlreadyScanned", scannedDoc.ScannedCheckMicrData );
            return alreadyScanned;
        }

        /// <summary>
        /// Uploads the scanned item.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void UploadScannedItem( ScannedDocInfo scannedDocInfo )
        {
            RockRestClient client = EnsureUploadScanRestClient();

            // upload image of front of doc
            string frontImageFileName = string.Format( "image1_{0}.png", DateTime.Now.ToString( "o" ).RemoveSpecialCharacters() );
            int frontImageBinaryFileId = client.UploadBinaryFile( frontImageFileName, Rock.Client.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid(), scannedDocInfo.FrontImagePngBytes, false );

            // upload image of back of doc (if it exists)
            int? backImageBinaryFileId = null;
            if ( scannedDocInfo.BackImageData != null )
            {
                // upload image of back of doc
                string backImageFileName = string.Format( "image2_{0}.png", DateTime.Now.ToString( "o" ).RemoveSpecialCharacters() );
                backImageBinaryFileId = client.UploadBinaryFile( backImageFileName, Rock.Client.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid(), scannedDocInfo.BackImagePngBytes, false );
            }


            FinancialPaymentDetail financialPaymentDetail = new FinancialPaymentDetail();
            financialPaymentDetail.CurrencyTypeValueId = scannedDocInfo.CurrencyTypeValue.Id;
            financialPaymentDetail.Guid = Guid.NewGuid();
            var financialPaymentDetailId = client.PostData<FinancialPaymentDetail>( "api/FinancialPaymentDetails", financialPaymentDetail ).AsIntegerOrNull();
            
            FinancialTransaction financialTransaction = new FinancialTransaction();

            financialTransaction.BatchId = batchPage.SelectedFinancialBatch.Id;
            financialTransaction.TransactionCode = string.Empty;
            financialTransaction.Summary = string.Empty;

            financialTransaction.Guid = Guid.NewGuid();
            financialTransaction.TransactionDateTime = batchPage.SelectedFinancialBatch.BatchStartDateTime;

            financialTransaction.FinancialPaymentDetailId = financialPaymentDetailId;
            financialTransaction.SourceTypeValueId = scannedDocInfo.SourceTypeValue.Id;

            financialTransaction.TransactionTypeValueId = transactionTypeValueContribution.Id;

            int? uploadedTransactionId;

            if ( scannedDocInfo.IsCheck )
            {
                financialTransaction.TransactionCode = scannedDocInfo.CheckNumber;
                financialTransaction.MICRStatus = scannedDocInfo.BadMicr ? MICRStatus.Fail : MICRStatus.Success;

                FinancialTransactionScannedCheck financialTransactionScannedCheck = new FinancialTransactionScannedCheck();

                // Rock server will encrypt CheckMicrPlainText to this since we can't have the DataEncryptionKey in a RestClient
                financialTransactionScannedCheck.FinancialTransaction = financialTransaction;
                financialTransactionScannedCheck.ScannedCheckMicrData = scannedDocInfo.ScannedCheckMicrData;
                financialTransactionScannedCheck.ScannedCheckMicrParts = scannedDocInfo.ScannedCheckMicrParts;

                uploadedTransactionId = client.PostData<FinancialTransactionScannedCheck>( "api/FinancialTransactions/PostScanned", financialTransactionScannedCheck ).AsIntegerOrNull();
            }
            else
            {
                uploadedTransactionId = client.PostData<FinancialTransaction>( "api/FinancialTransactions", financialTransaction as FinancialTransaction ).AsIntegerOrNull();
            }

            // upload FinancialTransactionImage records for front/back
            FinancialTransactionImage financialTransactionImageFront = new FinancialTransactionImage();
            financialTransactionImageFront.BinaryFileId = frontImageBinaryFileId;
            financialTransactionImageFront.TransactionId = uploadedTransactionId.Value;
            financialTransactionImageFront.Order = 0;
            financialTransactionImageFront.Guid = Guid.NewGuid();
            client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageFront );

            if ( backImageBinaryFileId.HasValue )
            {
                FinancialTransactionImage financialTransactionImageBack = new FinancialTransactionImage();
                financialTransactionImageBack.BinaryFileId = backImageBinaryFileId.Value;
                financialTransactionImageBack.TransactionId = uploadedTransactionId.Value;
                financialTransactionImageBack.Order = 1;
                financialTransactionImageBack.Guid = Guid.NewGuid();
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
        /// Initializes the RestClient for Uploads and loads any data that is needed for the scan session (if it isn't already initialized)
        /// </summary>
        /// <returns></returns>
        private RockRestClient EnsureUploadScanRestClient()
        {
            if ( _uploadScannedItemClient == null )
            {
                RockConfig rockConfig = RockConfig.Load();
                _uploadScannedItemClient = new RockRestClient( rockConfig.RockBaseUrl );
                _uploadScannedItemClient.Login( rockConfig.Username, rockConfig.Password );
            }

            if ( binaryFileTypeContribution == null || transactionTypeValueContribution == null )
            {
                binaryFileTypeContribution = _uploadScannedItemClient.GetDataByGuid<BinaryFileType>( "api/BinaryFileTypes", new Guid( Rock.Client.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE ) );
                transactionTypeValueContribution = _uploadScannedItemClient.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.Client.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
            }

            return _uploadScannedItemClient;
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
            lblScanItemCountInfo.Content = string.Join(", ", statsList);
        }

        //
        private bool _keepScanning;
        private int _itemsSkipped;
        private int _itemsScanned;
        private bool _firstNoItemsWarning;
        private int _itemsUploaded;

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            lblScannerNotReady.Visibility = Visibility.Collapsed;

            XportStates[] xportStatesNotConnected = new XportStates[] { XportStates.TransportShutDown, XportStates.TransportShuttingDown, XportStates.TransportExceptionInProgress };

            var transportState = (XportStates)batchPage.rangerScanner.GetTransportState();
            if ( xportStatesNotConnected.Contains( transportState ) )
            {
                batchPage.ConnectToScanner();
            }
            else
            {
                StartScanning();
            }
        }

        /// <summary>
        /// Handles the TransportIsDead event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportIsDead( object sender, EventArgs e )
        {
            _keepScanning = false;
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportIsDead", DateTime.Now.ToString( "o" ) ) );
            lblScannerNotReady.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Starts the scanning as soon as items are in the hopper
        /// </summary>
        public void StartScanning()
        {
            _itemsUploaded = 0;
            _itemsSkipped = 0;
            _itemsScanned = 0;
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
            HideUploadWarningPrompts( false );
            StopScanning();
            btnStart.IsEnabled = true;
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
