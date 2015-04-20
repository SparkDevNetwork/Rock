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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using Rock.Constants;
using Rock.Model;
using Rock.Net;
using Rock.Wpf;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for BatchPage.xaml
    /// </summary>
    public partial class BatchPage : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchPage" /> class.
        /// </summary>
        /// <param name="loggedInPerson">The logged in person.</param>
        public BatchPage( Person loggedInPerson )
        {
            LoggedInPerson = loggedInPerson;
            InitializeComponent();
            ScanningPage = new ScanningPage( this );
            ScanningPromptPage = new ScanningPromptPage( this );
            ScannedDocList = new ConcurrentQueue<ScannedDocInfo>();
            BatchItemDetailPage = new BatchItemDetailPage();
            FirstPageLoad = true;

            try
            {
                var micrImageHostPage = new MicrImageHostPage();
                this.micrImage = micrImageHostPage.micrImage;
                this.micrImage.MicrDataReceived += micrImage_MicrDataReceived;
            }
            catch
            {
                // intentionally nothing.  means they don't have the MagTek driver
            }

            try
            {
                var rangerScannerHostPage = new RangerScannerHostPage();
                this.rangerScanner = rangerScannerHostPage.rangerScanner;
                this.rangerScanner.TransportNewState += rangerScanner_TransportNewState;
                this.rangerScanner.TransportChangeOptionsState += rangerScanner_TransportChangeOptionsState;
                this.rangerScanner.TransportSetItemOutput += rangerScanner_TransportSetItemOutput;
            }
            catch
            {
                // intentionally nothing.  means they don't have the Ranger driver
            }
        }

        /// <summary>
        /// Gets or sets the micr image.
        /// </summary>
        /// <value>
        /// The micr image.
        /// </value>
        public AxMTMicrImage.AxMicrImage micrImage { get; set; }

        /// <summary>
        /// Gets or sets the ranger scanner.
        /// </summary>
        /// <value>
        /// The ranger scanner.
        /// </value>
        public AxRANGERLib.AxRanger rangerScanner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [first page load].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [first page load]; otherwise, <c>false</c>.
        /// </value>
        private bool FirstPageLoad { get; set; }

        /// <summary>
        /// Gets or sets the selected financial batch
        /// </summary>
        /// <value>
        /// The selected financial batch
        /// </value>
        public FinancialBatch SelectedFinancialBatch { get; set; }

        /// <summary>
        /// Gets or sets the logged in person id.
        /// </summary>
        /// <value>
        /// The logged in person id.
        /// </value>
        public Person LoggedInPerson { get; set; }

        /// <summary>
        /// Gets or sets the type of the feeder.
        /// </summary>
        /// <value>
        /// The type of the feeder.
        /// </value>
        public FeederType ScannerFeederType { get; set; }

        /// <summary>
        /// The scanning page
        /// </summary>
        public ScanningPage ScanningPage { get; set; }

        /// <summary>
        /// Gets or sets the scanning prompt page.
        /// </summary>
        /// <value>
        /// The scanning prompt page.
        /// </value>
        public ScanningPromptPage ScanningPromptPage { get; set; }

        /// <summary>
        /// Gets or sets the batch item detail page.
        /// </summary>
        /// <value>
        /// The batch item detail page.
        /// </value>
        public BatchItemDetailPage BatchItemDetailPage { get; set; }

        /// <summary>
        /// Gets or sets the scanned doc list.
        /// </summary>
        /// <value>
        /// The scanned doc list.
        /// </value>
        public ConcurrentQueue<ScannedDocInfo> ScannedDocList { get; set; }

        /// <summary>
        /// The currency value list
        /// </summary>
        public List<DefinedValue> CurrencyValueList { get; set; }

        /// <summary>
        /// Gets the selected currency value.
        /// </summary>
        /// <value>
        /// The selected currency value.
        /// </value>
        public DefinedValue SelectedCurrencyValue
        {
            get
            {
                return CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().TenderTypeValueGuid.AsGuid() );
            }
        }

        /// <summary>
        /// Gets or sets the source type value list.
        /// </summary>
        /// <value>
        /// The source type value list.
        /// </value>
        public List<DefinedValue> SourceTypeValueList { get; set; }

        /// <summary>
        /// Gets the selected source type value.
        /// </summary>
        /// <value>
        /// The selected source type value.
        /// </value>
        public DefinedValue SelectedSourceTypeValue
        {
            get
            {
                return this.SourceTypeValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            }
        }

        #region Ranger (Canon CR50/80) Scanner Events

        /// <summary>
        /// Rangers the new state of the scanner_ transport.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rangerScanner_TransportNewState( object sender, AxRANGERLib._DRangerEvents_TransportNewStateEvent e )
        {
            mnuConnect.IsEnabled = false;
            btnScan.Visibility = Visibility.Hidden;
            ScanningPage.btnSave.Visibility = Visibility.Visible;
            ScanningPage.btnCancel.Visibility = Visibility.Visible;

            string status = rangerScanner.GetTransportStateString().Replace( "Transport", string.Empty ).SplitCase();
            Color statusColor = Colors.Transparent;

            switch ( (XportStates)e.currentState )
            {
                case XportStates.TransportReadyToFeed:
                    statusColor = Colors.LimeGreen;
                    btnScan.Content = "Scan";
                    btnScan.Visibility = Visibility.Visible;
                    break;
                case XportStates.TransportShutDown:
                    statusColor = Colors.Red;
                    mnuConnect.IsEnabled = true;
                    break;
                case XportStates.TransportFeeding:
                    statusColor = Colors.Blue;
                    btnScan.Content = "Stop";
                    btnScan.Visibility = Visibility.Visible;
                    break;
                case XportStates.TransportStartingUp:
                    statusColor = Colors.Yellow;
                    break;
                default:
                    statusColor = Colors.White;
                    break;
            }

            this.shapeStatus.Fill = new SolidColorBrush( statusColor );
            this.shapeStatus.ToolTip = status;

            ScanningPage.ShowScannerStatus( (XportStates)e.currentState, statusColor, status );
        }

        /// <summary>
        /// Rangers the state of the scanner_ transport change options.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rangerScanner_TransportChangeOptionsState( object sender, AxRANGERLib._DRangerEvents_TransportChangeOptionsStateEvent e )
        {
            if ( e.previousState == (int)XportStates.TransportStartingUp )
            {
                // enable imaging
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedImaging", "True" );

                // limit splash screen
                rangerScanner.SetGenericOption( "Ranger GUI", "DisplaySplashOncePerDay", "true" );

                // turn on either color, grayscale, or bitonal(black and white) options depending on selected option
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage1", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage1", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage2", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage2", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage3", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage3", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage4", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage4", "False" );

                var rockConfig = RockConfig.Load();
                switch ( rockConfig.ImageColorType )
                {
                    case ImageColorType.ImageColorTypeColor:
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage3", "True" );
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage3", rockConfig.EnableRearImage.ToTrueFalse() );
                        break;
                    case ImageColorType.ImageColorTypeGrayscale:
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage2", "True" );
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage2", rockConfig.EnableRearImage.ToTrueFalse() );
                        break;
                    default:
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage1", "True" );
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage1", rockConfig.EnableRearImage.ToTrueFalse() );
                        break;
                }

                rangerScanner.SetGenericOption( "OptionalDevices", "NeedDoubleDocDetection", rockConfig.EnableDoubleDocDetection.ToTrueFalse() );

                rangerScanner.EnableOptions();
            }
        }

        /// <summary>
        /// Gets or sets the client that stays connected 
        /// </summary>
        /// <value>
        /// The persisted client.
        /// </value>
        private RockRestClient checkforDuplicateClient { get; set; }

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

            if ( checkforDuplicateClient == null )
            {
                var rockConfig = RockConfig.Load();
                checkforDuplicateClient = new RockRestClient( rockConfig.RockBaseUrl );
                checkforDuplicateClient.Login( rockConfig.Username, rockConfig.Password );
            }

            // first check if we have already scanned this doc during this session (we might not have uploaded it yet)
            var alreadyScanned = ScannedDocList.Any( a => a.IsCheck && a.ScannedCheckMicr == scannedDoc.ScannedCheckMicr );

            // if we didn't already scan it in this session, check the server
            if ( !alreadyScanned )
            {
                alreadyScanned = checkforDuplicateClient.PostDataWithResult<string, bool>( "api/FinancialTransactions/AlreadyScanned", scannedDoc.ScannedCheckMicr );
            }

            return alreadyScanned;
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rangerScanner_TransportSetItemOutput( object sender, AxRANGERLib._DRangerEvents_TransportSetItemOutputEvent e )
        {
            try
            {
                RockConfig rockConfig = RockConfig.Load();

                ScannedDocInfo scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = this.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = this.SelectedSourceTypeValue;

                scannedDoc.FrontImageData = GetImageBytesFromRanger( Sides.TransportFront );

                if ( rockConfig.EnableRearImage )
                {
                    scannedDoc.BackImageData = GetImageBytesFromRanger( Sides.TransportRear );
                }

                if ( scannedDoc.IsCheck )
                {
                    string checkMicr = rangerScanner.GetMicrText( 1 ).Trim();
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

                    // the last 'On-Us' symbol ('c') signifys the end of the account number
                    int lastOnUsPosition = remainingMicr.LastIndexOf( 'c' );
                    if ( lastOnUsPosition > 0 )
                    {
                        int accountNumberDigitPosition = lastOnUsPosition - 1;
                        // read all digits to the left of the last 'c' until you run into a non-numeric (except for '!' whichs means invalid)
                        while ( accountNumberDigitPosition >= 0 )
                        {
                            char accountNumberDigit = remainingMicr[accountNumberDigitPosition];
                            if ( char.IsNumber( accountNumberDigit ) || accountNumberDigit.Equals( '!' ) )
                            {
                                accountNumber = accountNumberDigit + accountNumber;
                            }
                            else
                            {
                                break;
                            }

                            accountNumberDigitPosition--;
                        }

                        remainingMicr = remainingMicr.Remove( accountNumberDigitPosition + 1, lastOnUsPosition - accountNumberDigitPosition );
                    }

                    // any remaining digits that aren't the account number and transit number are probably the check number
                    string[] remainingMicrParts = remainingMicr.Split( new char[] { 'c', ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( remainingMicrParts.Length == 1 )
                    {
                        checkNumber = remainingMicrParts[0];
                    }

                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;

                    if ( routingNumber.Length != 9 || string.IsNullOrEmpty( accountNumber ) || checkMicr.Contains('!') || string.IsNullOrEmpty(checkNumber) )
                    {
                        scannedDoc.BadMicr = true;
                        rangerScanner.StopFeeding();
                    }
                    else
                    {
                        if ( IsDuplicateScan( scannedDoc ) )
                        {
                            scannedDoc.Duplicate = true;

                            rangerScanner.StopFeeding();
                            rangerScanner.ClearTrack();
                        }
                        else
                        {
                            ScannedDocList.Enqueue( scannedDoc );
                        }
                    }
                }
                else
                {
                    ScannedDocList.Enqueue( scannedDoc );
                }

                ScanningPage.ShowScannedDocStatus( scannedDoc );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPage.ShowException( ( ex as AggregateException ).Flatten() );
                }
                else
                {
                    ScanningPage.ShowException( ex );
                }
            }
        }

        #endregion

        #region Scanner (MagTek MICRImage RS232) Events

        /// <summary>
        /// Handles the MicrDataReceived event of the micrImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void micrImage_MicrDataReceived( object sender, System.EventArgs e )
        {
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this.ScanningPage )
            {
                // only accept scans when the scanning page is showing
                micrImage.ClearBuffer();
                return;
            }

            // from MagTek Sample Code
            object dummy = null;
            string routingNumber = micrImage.FindElement( 0, "T", 0, "TT", ref dummy );
            string accountNumber = micrImage.FindElement( 0, "TT", 0, "A", ref dummy );
            string checkNumber = micrImage.FindElement( 0, "A", 0, "12", ref dummy );

            ScannedDocInfo scannedDoc = null;
            var rockConfig = RockConfig.Load();
            bool scanningMagTekBackImage = false;

            if ( ScanningPage.ExpectingMagTekBackScan )
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
                scannedDoc = ScannedDocList.Last();
            }
            else
            {
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = this.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = this.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck )
                {
                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;
                }
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
                micrImage.TransmitCurrentImage( docImageFileName, ref statusMsg );
                if ( !File.Exists( docImageFileName ) )
                {
                    throw new Exception( "Unable to retrieve image" );
                }
                else
                {
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
                        else
                        {
                            if ( IsDuplicateScan( scannedDoc ) )
                            {
                                scannedDoc.Duplicate = true;
                            }
                            else
                            {
                                ScannedDocList.Enqueue( scannedDoc );
                            }
                        }
                    }

                    ScanningPage.ShowScannedDocStatus( scannedDoc );

                    File.Delete( docImageFileName );
                }
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPage.ShowException( ( ex as AggregateException ).Flatten() );
                }
                else
                {
                    ScanningPage.ShowException( ex );
                }
            }
            finally
            {
                micrImage.ClearBuffer();
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
            imageByteCount = rangerScanner.GetImageByteCount( (int)side, (int)colorType );
            if ( imageByteCount > 0 )
            {
                byte[] imageBytes = new byte[imageByteCount];

                // create the pointer and assign the Ranger image address to it
                IntPtr imgAddress = new IntPtr( rangerScanner.GetImageAddress( (int)side, (int)colorType ) );

                // Copy the bytes from unmanaged memory to managed memory
                Marshal.Copy( imgAddress, imageBytes, 0, imageByteCount );

                return imageBytes;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Uploads the scanned docs.
        /// </summary>
        /// <param name="rockBaseUrl">The rock base URL.</param>
        private void UploadScannedDocsAsync()
        {
            if ( ScannedDocList.Where( a => !a.Uploaded ).Count() > 0 )
            {
                lblUploadProgress.Style = this.FindResource( "labelStyleAlertInfo" ) as Style;
                lblUploadProgress.Content = "Starting to Upload...";
                WpfHelper.FadeIn( lblUploadProgress );

                // use a backgroundworker to do the work so that we can have an updatable progressbar in the UI
                BackgroundWorker bwUploadScannedChecks = new BackgroundWorker();
                bwUploadScannedChecks.DoWork += bwUploadScannedChecks_DoWork;
                bwUploadScannedChecks.ProgressChanged += bwUploadScannedChecks_ProgressChanged;
                bwUploadScannedChecks.RunWorkerCompleted += bwUploadScannedChecks_RunWorkerCompleted;
                bwUploadScannedChecks.WorkerReportsProgress = true;
                bwUploadScannedChecks.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the bwUploadScannedChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void bwUploadScannedChecks_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if ( e.Error == null )
            {
                lblUploadProgress.Content = "Uploading Scanned Docs: Complete";
                WpfHelper.FadeOut( lblUploadProgress );
                UpdateBatchUI( grdBatches.SelectedValue as FinancialBatch );
            }
            else
            {
                Exception ex = e.Error;
                if ( ex is AggregateException )
                {
                    AggregateException ax = ex as AggregateException;
                    if ( ax.InnerExceptions.Count() == 1 )
                    {
                        ex = ax.InnerExceptions[0];
                    }
                }

                lblUploadProgress.Style = this.FindResource( "labelStyleAlertError" ) as Style;
                lblUploadProgress.Content = "Uploading Scanned Docs: ERROR";
                MessageBox.Show( string.Format( "Upload Error: {0}", ex.Message ) );
            }
        }

        /// <summary>
        /// Handles the ProgressChanged event of the bwUploadScannedChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void bwUploadScannedChecks_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            lblUploadProgress.Content = string.Format( "Uploading Scanned Docs {0}%", e.ProgressPercentage );
        }

        /// <summary>
        /// Handles the DoWork event of the bwUploadScannedChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void bwUploadScannedChecks_DoWork( object sender, DoWorkEventArgs e )
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            RockConfig rockConfig = RockConfig.Load();
            RockRestClient client = new RockRestClient( rockConfig.RockBaseUrl );
            client.Login( rockConfig.Username, rockConfig.Password );

            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            string appInfo = string.Format( "{0}, version: {1}", assemblyName.Name, assemblyName.Version );

            BinaryFileType binaryFileTypeContribution = client.GetDataByGuid<BinaryFileType>( "api/BinaryFileTypes", new Guid( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE ) );
            DefinedValue transactionTypeValueContribution = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );

            int totalCount = ScannedDocList.Where( a => !a.Uploaded ).Count();
            int position = 1;

            foreach ( ScannedDocInfo scannedDocInfo in ScannedDocList.Where( a => !a.Uploaded ) )
            {
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

                int percentComplete = position++ * 100 / totalCount;
                bw.ReportProgress( percentComplete );

                FinancialTransaction financialTransaction = new FinancialTransaction();

                Guid transactionGuid = Guid.NewGuid();

                financialTransaction.BatchId = SelectedFinancialBatch.Id;
                financialTransaction.TransactionCode = string.Empty;
                financialTransaction.Summary = string.Empty;

                financialTransaction.Guid = transactionGuid;
                financialTransaction.TransactionDateTime = SelectedFinancialBatch.BatchStartDateTime;

                financialTransaction.CurrencyTypeValueId = scannedDocInfo.CurrencyTypeValue.Id;
                financialTransaction.SourceTypeValueId = scannedDocInfo.SourceTypeValue.Id;

                financialTransaction.TransactionTypeValueId = transactionTypeValueContribution.Id;

                if ( scannedDocInfo.IsCheck )
                {
                    financialTransaction.TransactionCode = scannedDocInfo.CheckNumber;
                    
                    FinancialTransactionScannedCheck financialTransactionScannedCheck = new FinancialTransactionScannedCheck();

                    // Rock server will encrypt CheckMicrPlainText to this since we can't have the DataEncryptionKey in a RestClient
                    financialTransactionScannedCheck.FinancialTransaction = financialTransaction;
                    financialTransactionScannedCheck.ScannedCheckMicr = scannedDocInfo.ScannedCheckMicr;

                    client.PostData<FinancialTransactionScannedCheck>( "api/FinancialTransactions/PostScanned", financialTransactionScannedCheck );
                }
                else
                {
                    client.PostData<FinancialTransaction>( "api/FinancialTransactions", financialTransaction as FinancialTransaction );
                }

                // get the FinancialTransaction back from server so that we can get it's Id
                int transactionId = client.GetIdFromGuid( "api/FinancialTransactions/", transactionGuid );

                // upload FinancialTransactionImage records for front/back
                FinancialTransactionImage financialTransactionImageFront = new FinancialTransactionImage();
                financialTransactionImageFront.BinaryFileId = frontImageBinaryFileId;
                financialTransactionImageFront.TransactionId = transactionId;
                financialTransactionImageFront.Order = 0;
                client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageFront );

                if ( backImageBinaryFileId.HasValue )
                {
                    FinancialTransactionImage financialTransactionImageBack = new FinancialTransactionImage();
                    financialTransactionImageBack.BinaryFileId = backImageBinaryFileId.Value;
                    financialTransactionImageBack.TransactionId = transactionId;
                    financialTransactionImageBack.Order = 1;
                    client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageBack );
                }

                scannedDocInfo.Uploaded = true;
                scannedDocInfo.TransactionId = transactionId;
            }

            ScanningPage.ClearScannedDocHistory();
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void batchPage_Loaded( object sender, RoutedEventArgs e )
        {
            spBatchDetailReadOnly.Visibility = Visibility.Visible;
            spBatchDetailEdit.Visibility = Visibility.Collapsed;
            WpfHelper.FadeOut( lblUploadProgress, 0 );
            if ( !ConnectToScanner() )
            {
                NavigateToOptionsPage();
            }

            UploadScannedDocsAsync();

            if ( this.FirstPageLoad )
            {
                LoadComboBoxes();
                LoadFinancialBatchesGrid();
                this.FirstPageLoad = false;
            }
        }

        /// <summary>
        /// Loads the combo boxes.
        /// </summary>
        private void LoadComboBoxes()
        {
            RockConfig rockConfig = RockConfig.Load();
            RockRestClient client = new RockRestClient( rockConfig.RockBaseUrl );
            client.Login( rockConfig.Username, rockConfig.Password );
            List<Campus> campusList = client.GetData<List<Campus>>( "api/Campus" );

            cbCampus.SelectedValuePath = "Id";
            cbCampus.DisplayMemberPath = "Name";
            cbCampus.Items.Clear();
            cbCampus.Items.Add( new Campus { Id = None.Id, Name = None.Text } );
            foreach ( var campus in campusList.OrderBy( a => a.Name ) )
            {
                cbCampus.Items.Add( campus );
            }

            cbCampus.SelectedIndex = 0;

            var currencyTypeDefinedType = client.GetDataByGuid<DefinedType>( "api/DefinedTypes", Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() );
            this.CurrencyValueList = client.GetData<List<DefinedValue>>( "api/DefinedValues", "DefinedTypeId eq " + currencyTypeDefinedType.Id.ToString() );

            var sourceTypeDefinedType = client.GetDataByGuid<DefinedType>( "api/DefinedTypes", Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() );
            this.SourceTypeValueList = client.GetData<List<DefinedValue>>( "api/DefinedValues", "DefinedTypeId eq " + sourceTypeDefinedType.Id.ToString() );
        }

        /// <summary>
        /// Loads the financial batches grid.
        /// </summary>
        private void LoadFinancialBatchesGrid()
        {
            RockConfig config = RockConfig.Load();
            RockRestClient client = new RockRestClient( config.RockBaseUrl );
            client.Login( config.Username, config.Password );
            List<FinancialBatch> pendingBatches = client.GetDataByEnum<List<FinancialBatch>>( "api/FinancialBatches", "Status", BatchStatus.Pending );

            grdBatches.DataContext = pendingBatches.OrderByDescending( a => a.BatchStartDateTime ).ThenBy( a => a.Name );
            if ( pendingBatches.Count > 0 )
            {
                if ( SelectedFinancialBatch != null )
                {
                    // try to set the selected batch in the grid to our current batch (if it still exists in the database)
                    grdBatches.SelectedValue = pendingBatches.FirstOrDefault( a => a.Id.Equals( SelectedFinancialBatch.Id ) );
                }
                
                // if there still isn't a selected batch, set it to the first one
                if ( grdBatches.SelectedValue == null )
                {
                    grdBatches.SelectedIndex = 0;
                }
            }

            bool startWithNewBatch = !pendingBatches.Any();
            if ( startWithNewBatch )
            {
                // don't let them start without having at least one batch, so just show the list with the Add button
                HideBatch();
            }
            else
            {
                gBatchDetailList.Visibility = Visibility.Visible;
                UpdateBatchUI( grdBatches.SelectedValue as FinancialBatch );
            }
        }

        /// <summary>
        /// Updates the scanner status for magtek.
        /// </summary>
        /// <param name="connected">if set to <c>true</c> [connected].</param>
        private void UpdateScannerStatusForMagtek( bool connected )
        {
            string status;
            Color statusColor;

            if ( connected )
            {
                statusColor = Colors.LimeGreen;
                status = "Connected";
                btnScan.Visibility = Visibility.Visible;
            }
            else
            {
                statusColor = Colors.Red;
                status = "Disconnected";
                btnScan.Visibility = Visibility.Hidden;
            }

            this.shapeStatus.ToolTip = status;
            this.shapeStatus.Fill = new SolidColorBrush( statusColor );

            ScanningPage.ShowScannerStatus( connected ? XportStates.TransportReadyToFeed : XportStates.TransportShutDown, statusColor, status );
        }

        /// <summary>
        /// Connects to scanner.
        /// </summary>
        private bool ConnectToScanner()
        {
            var rockConfig = RockConfig.Load();

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                if ( micrImage == null )
                {
                    // no MagTek driver
                    return false;
                }
                
                micrImage.CommPort = rockConfig.MICRImageComPort;
                micrImage.PortOpen = false;

                UpdateScannerStatusForMagtek( false );

                object dummy = null;

                // converted from VB6 from MagTek's sample app
                if ( !micrImage.PortOpen )
                {
                    micrImage.PortOpen = true;
                    if ( micrImage.DSRHolding )
                    {
                        // Sets Switch Settings
                        // If you use the MicrImage1.Save command then these do not need to be sent
                        // every time you open the device
                        micrImage.MicrTimeOut = 1;
                        micrImage.MicrCommand( "SWA 00100010", ref dummy );
                        micrImage.MicrCommand( "SWB 00100010", ref dummy );
                        micrImage.MicrCommand( "SWC 00100000", ref dummy );
                        micrImage.MicrCommand( "HW 00111100", ref dummy );
                        micrImage.MicrCommand( "SWE 00000010", ref dummy );
                        micrImage.MicrCommand( "SWI 00000000", ref dummy );

                        // The OCX will work with any Micr Format.  You just need to know which
                        // format is being used to parse it using the FindElement Method
                        micrImage.FormatChange( "6200" );
                        micrImage.MicrTimeOut = 5;

                        ScanningPage.btnStartStop.Content = ScanButtonText.ScanCheck;

                        // get Version to test if we have a good connection to the device
                        string version = "-1";
                        try
                        {
                            this.Cursor = Cursors.Wait;
                            version = micrImage.Version();
                        }
                        finally
                        {
                            this.Cursor = null;
                        }

                        if ( !version.Equals( "-1" ) )
                        {
                            UpdateScannerStatusForMagtek( true );
                        }
                        else
                        {
                            MessageBox.Show( string.Format( "MagTek Device is not responding on COM{0}.", micrImage.CommPort ), "Scanner Error" );
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show( string.Format( "MagTek Device is not attached to COM{0}.", micrImage.CommPort ), "Missing Scanner" );
                        return false;
                    }
                }

                ScannerFeederType = FeederType.SingleItem;
            }
            else
            {
                try
                {
                    if ( this.rangerScanner == null )
                    {
                        // no ranger driver
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

                try
                {
                    this.Cursor = Cursors.Wait;
                    rangerScanner.StartUp();
                }
                finally
                {
                    this.Cursor = null;
                }

                string feederTypeName = rangerScanner.GetTransportInfo( "MainHopper", "FeederType" );
                if ( feederTypeName.Equals( "MultipleItems" ) )
                {
                    ScannerFeederType = FeederType.MultipleItems;
                }
                else
                {
                    ScannerFeederType = FeederType.SingleItem;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles the 1 event of the btnConnect_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnConnect_Click( object sender, RoutedEventArgs e )
        {
            ConnectToScanner();
        }

        /// <summary>
        /// Handles the Click event of the btnScan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnScan_Click( object sender, RoutedEventArgs e )
        {
            // set the checkforDuplicateClient to null to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            checkforDuplicateClient = null;
            this.NavigationService.Navigate( this.ScanningPromptPage );
        }

        /// <summary>
        /// Handles the Click event of the btnOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOptions_Click( object sender, RoutedEventArgs e )
        {
            NavigateToOptionsPage();
        }

        /// <summary>
        /// Navigates to options page.
        /// </summary>
        private void NavigateToOptionsPage()
        {
            var optionsPage = new OptionsPage( this );
            this.NavigationService.Navigate( optionsPage );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEdit_Click( object sender, RoutedEventArgs e )
        {
            ShowBatch( true );
        }

        /// <summary>
        /// Shows the batch edit.
        /// </summary>
        private void ShowBatch( bool showInEditMode )
        {
            gBatchDetailList.Visibility = Visibility.Visible;
            if ( showInEditMode )
            {
                spBatchDetailEdit.Visibility = Visibility.Visible;
                spBatchDetailReadOnly.Visibility = Visibility.Collapsed;
            }
            else
            {
                spBatchDetailEdit.Visibility = Visibility.Collapsed;
                spBatchDetailReadOnly.Visibility = Visibility.Visible;
            }

            grdBatches.IsEnabled = !showInEditMode;
            btnAddBatch.IsEnabled = !showInEditMode;
        }

        /// <summary>
        /// Hides the batch.
        /// </summary>
        private void HideBatch()
        {
            gBatchDetailList.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSave_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                RockConfig rockConfig = RockConfig.Load();
                RockRestClient client = new RockRestClient( rockConfig.RockBaseUrl );
                client.Login( rockConfig.Username, rockConfig.Password );

                FinancialBatch financialBatch = null;
                if ( SelectedFinancialBatch == null || SelectedFinancialBatch.Id == 0 )
                {
                    financialBatch = new FinancialBatch { Id = 0, Guid = Guid.NewGuid(), Status = BatchStatus.Pending, CreatedByPersonAliasId = LoggedInPerson.PrimaryAlias.Id };
                }
                else
                {
                    financialBatch = client.GetData<FinancialBatch>( string.Format( "api/FinancialBatches/{0}", SelectedFinancialBatch.Id ) );
                }

                txtBatchName.Text = txtBatchName.Text.Trim();
                if ( string.IsNullOrWhiteSpace( txtBatchName.Text ) )
                {
                    txtBatchName.Style = this.FindResource( "textboxStyleError" ) as Style;
                    return;
                }
                else
                {
                    txtBatchName.Style = this.FindResource( "textboxStyle" ) as Style;
                }

                financialBatch.Name = txtBatchName.Text;
                Campus selectedCampus = cbCampus.SelectedItem as Campus;
                if ( selectedCampus.Id > 0 )
                {
                    financialBatch.CampusId = selectedCampus.Id;
                }
                else
                {
                    financialBatch.CampusId = null;
                }

                financialBatch.BatchStartDateTime = dpBatchDate.SelectedDate;

                if ( !string.IsNullOrWhiteSpace( txtControlAmount.Text ) )
                {
                    financialBatch.ControlAmount = decimal.Parse( txtControlAmount.Text.Replace( "$", string.Empty ) );
                }
                else
                {
                    financialBatch.ControlAmount = 0.00M;
                }

                if ( financialBatch.Id == 0 )
                {
                    client.PostData<FinancialBatch>( "api/FinancialBatches/", financialBatch );
                }
                else
                {
                    client.PutData<FinancialBatch>( "api/FinancialBatches/", financialBatch );
                }

                if ( SelectedFinancialBatch == null || SelectedFinancialBatch.Id == 0 )
                {
                    // refetch the batch to get the Id if it was just Inserted
                    financialBatch = client.GetDataByGuid<FinancialBatch>( "api/FinancialBatches", financialBatch.Guid );

                    SelectedFinancialBatch = financialBatch;
                }

                LoadFinancialBatchesGrid();

                ShowBatch( false );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            UpdateBatchUI( grdBatches.SelectedValue as FinancialBatch );
            btnAddBatch.IsEnabled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the grdBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void grdBatches_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            FinancialBatch selectedBatch = grdBatches.SelectedValue as FinancialBatch;

            UpdateBatchUI( selectedBatch );
        }

        /// <summary>
        /// Updates the batch UI.
        /// </summary>
        /// <param name="selectedBatch">The selected batch.</param>
        private void UpdateBatchUI( FinancialBatch selectedBatch )
        {
            if ( selectedBatch == null )
            {
                HideBatch();
                return;
            }
            else
            {
                ShowBatch( false );
            }

            RockConfig rockConfig = RockConfig.Load();
            RockRestClient client = new RockRestClient( rockConfig.RockBaseUrl );
            client.Login( rockConfig.Username, rockConfig.Password );
            SelectedFinancialBatch = selectedBatch;
            lblBatchNameReadOnly.Content = selectedBatch.Name;

            lblBatchCampusReadOnly.Content = selectedBatch.CampusId.HasValue ? client.GetData<Campus>( string.Format( "api/Campus/{0}", selectedBatch.CampusId ) ).Name : None.Text;
            lblBatchDateReadOnly.Content = selectedBatch.BatchStartDateTime.Value.ToString( "d" );
            lblBatchCreatedByReadOnly.Content = client.GetData<Person>( string.Format( "api/People/GetByPersonAliasId/{0}", selectedBatch.CreatedByPersonAliasId ) ).FullName;
            lblBatchControlAmountReadOnly.Content = selectedBatch.ControlAmount.ToString( "F" );

            txtBatchName.Text = selectedBatch.Name;
            if ( selectedBatch.CampusId.HasValue )
            {
                cbCampus.SelectedValue = selectedBatch.CampusId;
            }
            else
            {
                cbCampus.SelectedValue = 0;
            }

            dpBatchDate.SelectedDate = selectedBatch.BatchStartDateTime;
            lblCreatedBy.Content = lblBatchCreatedByReadOnly.Content as string;
            txtControlAmount.Text = selectedBatch.ControlAmount.ToString( "F" );

            List<FinancialTransaction> transactions = client.GetData<List<FinancialTransaction>>( "api/FinancialTransactions/", string.Format( "BatchId eq {0}", selectedBatch.Id ) );
            foreach ( var transaction in transactions )
            {
                transaction.CurrencyTypeValue = this.CurrencyValueList.FirstOrDefault( a => a.Id == transaction.CurrencyTypeValueId );
            }

            // include CheckNumber for checks that we scanned in this session
            var scannedCheckList = ScannedDocList.Where( a => a.IsCheck ).ToList();
            var gridList = transactions.OrderBy( a => a.CreatedDateTime ).ThenBy(a=> a.Id).Select( a => new
            {
                FinancialTransaction = a,
                CheckNumber = a.CurrencyTypeValue.Guid == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid()
                    ? scannedCheckList.FirstOrDefault( s => s.TransactionId == a.Id ) != null ? scannedCheckList.FirstOrDefault( s => s.TransactionId == a.Id ).CheckNumber : "****"
                    : "-"
            } );

            lblCount.Content = string.Format( "{0} item{1}", gridList.Count(), gridList.Count() != 1 ? "s" : "" );

            grdBatchItems.DataContext = gridList;
        }

        /// <summary>
        /// Handles the Click event of the btnAddBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnAddBatch_Click( object sender, RoutedEventArgs e )
        {
            var financialBatch = new FinancialBatch { Id = 0, BatchStartDateTime = DateTime.Now.Date, CreatedByPersonAliasId = LoggedInPerson.PrimaryAlias.Id };
            UpdateBatchUI( financialBatch );
            ShowBatch( true );
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRefreshBatchList_Click( object sender, RoutedEventArgs e )
        {
            LoadFinancialBatchesGrid();
        }

        /// <summary>
        /// Handles the RowEdit event of the grdBatchItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void grdBatchItems_RowEdit( object sender, MouseButtonEventArgs e )
        {
            try
            {
                FinancialTransaction financialTransaction = grdBatchItems.SelectedValue.GetPropertyValue( "FinancialTransaction" ) as FinancialTransaction;

                if ( financialTransaction != null )
                {
                    RockConfig config = RockConfig.Load();
                    RockRestClient client = new RockRestClient( config.RockBaseUrl );
                    client.Login( config.Username, config.Password );
                    financialTransaction.Images = client.GetData<List<FinancialTransactionImage>>( "api/FinancialTransactionImages", string.Format( "TransactionId eq {0}", financialTransaction.Id ) );

                    BatchItemDetailPage.FinancialTransactionImages = financialTransaction.Images;
                    this.NavigationService.Navigate( BatchItemDetailPage );
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation );
            }
        }

        #endregion
    }
}
