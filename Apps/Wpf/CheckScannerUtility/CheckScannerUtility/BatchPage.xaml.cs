//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// Initializes a new instance of the <see cref="BatchPage"/> class.
        /// </summary>
        public BatchPage()
        {
            InitializeComponent();
            ScanningPage = new ScanningPage( this );
            ScannedCheckList = new ConcurrentQueue<ScannedCheckInfo>();
            BatchItemDetailPage = new BatchItemDetailPage();
        }

        /// <summary>
        /// Gets or sets the selected financial batch id.
        /// </summary>
        /// <value>
        /// The selected financial batch id.
        /// </value>
        public int SelectedFinancialBatchId { get; set; }

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
        /// Gets or sets the batch item detail page.
        /// </summary>
        /// <value>
        /// The batch item detail page.
        /// </value>
        public BatchItemDetailPage BatchItemDetailPage { get; set; }

        /// <summary>
        /// Gets or sets the scanned check list.
        /// </summary>
        /// <value>
        /// The scanned check list.
        /// </value>
        public ConcurrentQueue<ScannedCheckInfo> ScannedCheckList { get; set; }

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
            ScanningPage.btnDone.Visibility = Visibility.Visible;
            string status = rangerScanner.GetTransportStateString().Replace( "Transport", string.Empty ).SplitCase();
            shapeStatus.ToolTip = status;

            switch ( (XportStates)e.currentState )
            {
                case XportStates.TransportReadyToFeed:
                    shapeStatus.Fill = new SolidColorBrush( Colors.LimeGreen );
                    btnScan.Content = "Scan";
                    if ( ScannerFeederType.Equals( FeederType.MultipleItems ) )
                    {
                        ScanningPage.btnStartStop.Content = ScanButtonText.Scan;
                    }
                    else
                    {
                        ScanningPage.btnStartStop.Content = ScanButtonText.ScanCheck;
                    }

                    btnScan.Visibility = Visibility.Visible;
                    break;
                case XportStates.TransportShutDown:
                    shapeStatus.Fill = new SolidColorBrush( Colors.Red );
                    mnuConnect.IsEnabled = true;
                    break;
                case XportStates.TransportFeeding:
                    shapeStatus.Fill = new SolidColorBrush( Colors.Blue );
                    btnScan.Content = "Stop";
                    ScanningPage.btnStartStop.Content = ScanButtonText.Stop;
                    ScanningPage.btnDone.Visibility = Visibility.Hidden;
                    btnScan.Visibility = Visibility.Visible;
                    break;
                case XportStates.TransportStartingUp:
                    shapeStatus.Fill = new SolidColorBrush( Colors.Yellow );
                    break;
                default:
                    shapeStatus.Fill = new SolidColorBrush( Colors.White );
                    break;
            }

            ScanningPage.shapeStatus.ToolTip = this.shapeStatus.ToolTip;
            ScanningPage.shapeStatus.Fill = this.shapeStatus.Fill;
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
                rangerScanner.SetGenericOption( "Ranger GUI", "DisplaySplashOncePerDay", "True" );

                // turn on either color, grayscale, or bitonal options depending on selected option
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage1", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage1", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage2", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage2", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage3", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage3", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage4", "False" );
                rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage4", "False" );

                switch ( RockConfig.Load().ImageColorType )
                {
                    case ImageColorType.ImageColorTypeColor:
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage3", "True" );
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage3", "True" );
                        break;
                    case ImageColorType.ImageColorTypeGrayscale:
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage2", "True" );
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage2", "True" );
                        break;
                    default:
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage1", "True" );
                        rangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage1", "True" );
                        break;
                }

                rangerScanner.EnableOptions();
            }
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rangerScanner_TransportItemInPocket( object sender, AxRANGERLib._DRangerEvents_TransportItemInPocketEvent e )
        {
            BitmapImage bitImageFront = GetCheckImage( Sides.TransportFront );
            BitmapImage bitImageBack = GetCheckImage( Sides.TransportRear );

            string checkMicr = rangerScanner.GetMicrText( 1 ).Replace( "-", string.Empty ).Replace( "!", string.Empty ).Trim();
            string fileName = checkMicr.Replace( " ", "_" );

            string[] micrParts = checkMicr.Split( new char[] { 'c', 'd', ' ' }, StringSplitOptions.RemoveEmptyEntries );
            string routingNumber = micrParts.Length > 0 ? micrParts[0] : "??";
            string accountNumber = micrParts.Length > 1 ? micrParts[1] : "??";
            string checkNumber = micrParts.Length > 2 ? micrParts[2] : "??";

            ScannedCheckInfo scannedCheck = new ScannedCheckInfo();
            scannedCheck.FrontImageData = ( bitImageFront.StreamSource as MemoryStream ).ToArray();
            scannedCheck.BackImageData = ( bitImageBack.StreamSource as MemoryStream ).ToArray();
            scannedCheck.RoutingNumber = routingNumber;
            scannedCheck.AccountNumber = accountNumber;
            scannedCheck.CheckNumber = checkNumber;

            ScanningPage.ShowCheckInformation( scannedCheck );

            if ( ( micrParts.Length < 3 ) || routingNumber.Length != 9 )
            {
                ScanningPage.lblScanWarning.Visibility = Visibility.Visible;
                rangerScanner.StopFeeding();
            }
            else
            {
                ScanningPage.lblScanWarning.Visibility = Visibility.Collapsed;
                ScannedCheckList.Enqueue( scannedCheck );
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
            object dummy = null;

            string imagePath = string.Empty;
            string imageIndex = string.Empty;
            string statusMsg = string.Empty;

            ScannedCheckInfo scannedCheck = null;
            if ( ScanningPage.ExpectingMagTekBackScan )
            {
                scannedCheck = ScannedCheckList.Last();
            }
            else
            {
                scannedCheck = new ScannedCheckInfo();

                // from MagTek Sample Code
                scannedCheck.RoutingNumber = micrImage.FindElement( 0, "T", 0, "TT", ref dummy );
                scannedCheck.AccountNumber = micrImage.FindElement( 0, "TT", 0, "A", ref dummy );
                scannedCheck.CheckNumber = micrImage.FindElement( 0, "A", 0, "12", ref dummy );
            }

            imagePath = Path.GetTempPath();
            string checkImageFileName = Path.Combine( imagePath, string.Format( "check_{0}_{1}_{2}.tif", scannedCheck.RoutingNumber, scannedCheck.AccountNumber, scannedCheck.CheckNumber ).Replace('?', 'X'));

            if ( File.Exists( checkImageFileName ) )
            {
                File.Delete( checkImageFileName );
            }

            try
            {
                micrImage.TransmitCurrentImage( checkImageFileName, ref statusMsg );
                if ( !File.Exists( checkImageFileName ) )
                {
                    throw new Exception( "Unable to retrieve image" );
                }
                else
                {
                    if ( ScanningPage.ExpectingMagTekBackScan )
                    {
                        scannedCheck.BackImageData = File.ReadAllBytes( checkImageFileName );
                    }
                    else
                    {
                        scannedCheck.FrontImageData = File.ReadAllBytes( checkImageFileName );
                    }

                    ScanningPage.ShowCheckInformation( scannedCheck );

                    if ( scannedCheck.RoutingNumber.Length != 9 )
                    {
                        ScanningPage.lblScanWarning.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ScanningPage.lblScanWarning.Visibility = Visibility.Collapsed;
                        ScannedCheckList.Enqueue( scannedCheck );
                    }

                    File.Delete( checkImageFileName );
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
        /// Gets the check image.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        private BitmapImage GetCheckImage( Sides side )
        {
            ImageColorType colorType = RockConfig.Load().ImageColorType;

            int imageByteCount;
            imageByteCount = rangerScanner.GetImageByteCount( (int)side, (int)colorType );
            byte[] imageBytes = new byte[imageByteCount];

            // create the pointer and assign the Ranger image address to it
            IntPtr imgAddress = new IntPtr( rangerScanner.GetImageAddress( (int)side, (int)colorType ) );

            // Copy the bytes from unmanaged memory to managed memory
            Marshal.Copy( imgAddress, imageBytes, 0, imageByteCount );

            BitmapImage bitImage = new BitmapImage();

            bitImage.BeginInit();
            bitImage.StreamSource = new MemoryStream( imageBytes );
            bitImage.EndInit();

            return bitImage;
        }

        /// <summary>
        /// Uploads the scanned checks.
        /// </summary>
        /// <param name="rockBaseUrl">The rock base URL.</param>
        private void UploadScannedChecksAsync()
        {
            if ( ScannedCheckList.Where( a => !a.Uploaded ).Count() > 0 )
            {
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
                lblUploadProgress.Content = "Uploading Scanned Checks: Complete";
                WpfHelper.FadeOut( lblUploadProgress );
                UpdateBatchUI( grdBatches.SelectedValue as FinancialBatch );
            }
            else
            {
                WpfHelper.FadeOut( lblUploadProgress );
                MessageBox.Show( string.Format( "Upload Error: {0}", e.Error.Message ) );
            }
        }

        /// <summary>
        /// Handles the ProgressChanged event of the bwUploadScannedChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void bwUploadScannedChecks_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            lblUploadProgress.Content = string.Format( "Uploading Scanned Checks {0}%", e.ProgressPercentage );
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
            string appInfo = string.Format( "{0}, version: {1}", assemblyName.FullName, assemblyName.Version );

            BinaryFileType binaryFileTypeContribution = client.GetDataByGuid<BinaryFileType>( "api/BinaryFileTypes", new Guid( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE ) );
            DefinedValue currencyTypeValueCheck = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK ) );
            DefinedValue transactionTypeValueContribution = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
            DefinedValue transactionImageTypeValueFront = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_IMAGE_TYPE_CHECK_FRONT ) );
            DefinedValue transactionImageTypeValueBack = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_IMAGE_TYPE_CHECK_BACK ) );

            int totalCount = ScannedCheckList.Where( a => !a.Uploaded ).Count();
            int position = 1;

            foreach ( ScannedCheckInfo scannedCheckInfo in ScannedCheckList.Where( a => !a.Uploaded ) )
            {
                // upload image of front of check
                BinaryFile binaryFileFront = new BinaryFile();
                binaryFileFront.Guid = Guid.NewGuid();
                binaryFileFront.FileName = string.Format( "{0}_{1}_{2}_front.png", scannedCheckInfo.RoutingNumber, scannedCheckInfo.MaskedAccountNumber, scannedCheckInfo.CheckNumber );
                binaryFileFront.Data = new BinaryFileData();
                binaryFileFront.Data.Content = scannedCheckInfo.FrontImagePngBytes;
                binaryFileFront.BinaryFileTypeId = binaryFileTypeContribution.Id;
                binaryFileFront.IsSystem = false;
                binaryFileFront.MimeType = "image/png";
                client.PostData<BinaryFile>( "api/BinaryFiles/", binaryFileFront );

                // upload image of back of check (if it exists)
                BinaryFile binaryFileBack = null;

                if ( scannedCheckInfo.BackImageData != null )
                {
                    binaryFileBack = new BinaryFile();
                    binaryFileBack.Guid = Guid.NewGuid();
                    binaryFileBack.FileName = string.Format( "{0}_{1}_{2}_back.png", scannedCheckInfo.RoutingNumber, scannedCheckInfo.MaskedAccountNumber, scannedCheckInfo.CheckNumber );
                    binaryFileBack.Data = new BinaryFileData();
                    binaryFileBack.Data.Content = scannedCheckInfo.BackImagePngBytes;
                    binaryFileBack.BinaryFileTypeId = binaryFileTypeContribution.Id;
                    binaryFileBack.IsSystem = false;
                    binaryFileBack.MimeType = "image/png";
                    client.PostData<BinaryFile>( "api/BinaryFiles/", binaryFileBack );
                }

                int percentComplete = position++ * 100 / totalCount;
                bw.ReportProgress( percentComplete );

                FinancialTransactionScannedCheck financialTransactionScannedCheck = new FinancialTransactionScannedCheck();
                financialTransactionScannedCheck.BatchId = SelectedFinancialBatchId;
                financialTransactionScannedCheck.Amount = 0.00M;
                financialTransactionScannedCheck.TransactionCode = string.Empty;
                financialTransactionScannedCheck.Summary = string.Format( "Scanned Check from {0}", appInfo );
                financialTransactionScannedCheck.Guid = Guid.NewGuid();
                financialTransactionScannedCheck.TransactionDateTime = DateTime.Now;
                financialTransactionScannedCheck.CurrencyTypeValueId = currencyTypeValueCheck.Id;
                financialTransactionScannedCheck.CreditCardTypeValueId = null;
                financialTransactionScannedCheck.SourceTypeValueId = null;
                financialTransactionScannedCheck.AuthorizedPersonId = this.LoggedInPerson.Id;
                financialTransactionScannedCheck.TransactionTypeValueId = transactionTypeValueContribution.Id;

                // Rock server will encrypt CheckMicrPlainText to this since we can't have the DataEncryptionKey in a RestClient
                financialTransactionScannedCheck.CheckMicrEncrypted = null;

                financialTransactionScannedCheck.ScannedCheckMicr = string.Format( "{0}_{1}_{2}", scannedCheckInfo.RoutingNumber, scannedCheckInfo.AccountNumber, scannedCheckInfo.CheckNumber );

                client.PostData<FinancialTransactionScannedCheck>( "api/FinancialTransactions/PostScanned", financialTransactionScannedCheck );

                // get the FinancialTransaction back from server so that we can get it's Id
                financialTransactionScannedCheck.Id = client.GetDataByGuid<FinancialTransaction>( "api/FinancialTransactions", financialTransactionScannedCheck.Guid ).Id;

                // get the BinaryFiles back so that we can get their Ids
                binaryFileFront.Id = client.GetDataByGuid<BinaryFile>( "api/BinaryFiles", binaryFileFront.Guid ).Id;

                // upload FinancialTransactionImage records for front/back
                FinancialTransactionImage financialTransactionImageFront = new FinancialTransactionImage();
                financialTransactionImageFront.BinaryFileId = binaryFileFront.Id;
                financialTransactionImageFront.TransactionId = financialTransactionScannedCheck.Id;
                financialTransactionImageFront.TransactionImageTypeValueId = transactionImageTypeValueFront.Id;
                client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageFront );

                if ( binaryFileBack != null )
                {
                    // get the BinaryFiles back so that we can get their Ids
                    binaryFileBack.Id = client.GetDataByGuid<BinaryFile>( "api/BinaryFiles", binaryFileBack.Guid ).Id;
                    FinancialTransactionImage financialTransactionImageBack = new FinancialTransactionImage();
                    financialTransactionImageBack.BinaryFileId = binaryFileBack.Id;
                    financialTransactionImageBack.TransactionId = financialTransactionScannedCheck.Id;
                    financialTransactionImageBack.TransactionImageTypeValueId = transactionImageTypeValueBack.Id;
                    client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageBack );
                }

                scannedCheckInfo.Uploaded = true;
            }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            bdrBatchDetailReadOnly.Visibility = Visibility.Visible;
            bdrBatchDetailEdit.Visibility = Visibility.Collapsed;
            WpfHelper.FadeOut( lblUploadProgress, 0 );
            ConnectToScanner();
            LoadComboBoxes();
            LoadFinancialBatchesGrid();
            UploadScannedChecksAsync();
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
                if ( SelectedFinancialBatchId > 0 )
                {
                    grdBatches.SelectedValue = pendingBatches.FirstOrDefault( a => a.Id.Equals( SelectedFinancialBatchId ) );
                }
                else
                {
                    grdBatches.SelectedIndex = 0;
                }
            }

            UpdateBatchUI( grdBatches.SelectedValue as FinancialBatch );
        }

        /// <summary>
        /// Updates the scanner status for magtek.
        /// </summary>
        /// <param name="connected">if set to <c>true</c> [connected].</param>
        private void UpdateScannerStatusForMagtek( bool connected )
        {
            if ( connected )
            {
                shapeStatus.Fill = new SolidColorBrush( Colors.LimeGreen );
                shapeStatus.ToolTip = "Connected";
            }
            else
            {
                shapeStatus.Fill = new SolidColorBrush( Colors.Red );
                shapeStatus.ToolTip = "Disconnected";
            }
            
            ScanningPage.shapeStatus.ToolTip = this.shapeStatus.ToolTip;
            ScanningPage.shapeStatus.Fill = this.shapeStatus.Fill;
        }

        /// <summary>
        /// Connects to scanner.
        /// </summary>
        private void ConnectToScanner()
        {
            var rockConfig = RockConfig.Load();

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                micrImageHost.IsEnabled = true;
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

                        string version = null;
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
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show( string.Format("MagTek Device is not attached to COM{0}.", micrImage.CommPort), "Missing Scanner" );
                        return;
                    }
                }

                ScannerFeederType = FeederType.SingleItem;
            }
            else
            {
                rangerScanner.StartUp();
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
            HandleScanButtonClick( sender, e, true );
        }

        /// <summary>
        /// Handles the scan button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <param name="navigate">if set to <c>true</c> [navigate].</param>
        public void HandleScanButtonClick( object sender, RoutedEventArgs e, bool navigate )
        {
            Button scanButton = sender as Button;

            if ( ScanButtonText.IsStartScan( scanButton.Content as string ) )
            {
                if ( ScannerFeederType.Equals( FeederType.SingleItem ) )
                {
                    rangerScanner.StartFeeding( FeedSource.FeedSourceManualDrop, FeedItemCount.FeedOne );
                }
                else
                {
                    rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedContinuously );
                }

                if ( navigate )
                {
                    this.NavigationService.Navigate( ScanningPage );
                }
            }
            else
            {
                rangerScanner.StopFeeding();
                if ( navigate )
                {
                    this.NavigationService.Navigate( this );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOptions_Click( object sender, RoutedEventArgs e )
        {
            var optionsPage = new OptionsPage();
            optionsPage.BatchPage = this;
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
            if ( showInEditMode )
            {
                bdrBatchDetailEdit.Visibility = Visibility.Visible;
                bdrBatchDetailReadOnly.Visibility = Visibility.Collapsed;
            }
            else
            {
                bdrBatchDetailEdit.Visibility = Visibility.Collapsed;
                bdrBatchDetailReadOnly.Visibility = Visibility.Visible;
            }

            grdBatches.IsEnabled = !showInEditMode;
            btnAddBatch.IsEnabled = !showInEditMode;
        }

        /// <summary>
        /// Hides the batch.
        /// </summary>
        private void HideBatch()
        {
            bdrBatchDetailReadOnly.Visibility = Visibility.Collapsed;
            bdrBatchDetailEdit.Visibility = Visibility.Collapsed;
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
                if ( SelectedFinancialBatchId.Equals( 0 ) )
                {
                    financialBatch = new FinancialBatch { Id = 0, Guid = Guid.NewGuid(), Status = BatchStatus.Pending, CreatedByPersonId = LoggedInPerson.Id };
                }
                else
                {
                    financialBatch = client.GetData<FinancialBatch>( string.Format( "api/FinancialBatches/{0}", SelectedFinancialBatchId ) );
                }

                txtBatchName.Text = txtBatchName.Text.Trim();

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

                client.PostData<FinancialBatch>( "api/FinancialBatches/", financialBatch );

                if ( SelectedFinancialBatchId.Equals( 0 ) )
                {
                    // refetch the batch to get the Id if it was just Inserted
                    financialBatch = client.GetDataByGuid<FinancialBatch>( "api/FinancialBatches", financialBatch.Guid );

                    SelectedFinancialBatchId = financialBatch.Id;
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
            ShowBatch( false );
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
            SelectedFinancialBatchId = selectedBatch.Id;
            lblBatchNameReadOnly.Content = selectedBatch.Name;

            lblBatchCampusReadOnly.Content = selectedBatch.CampusId.HasValue ? client.GetData<Campus>( string.Format( "api/Campus/{0}", selectedBatch.CampusId ) ).Name : None.Text;
            lblBatchDateReadOnly.Content = selectedBatch.BatchStartDateTime.Value.ToString( "d" );
            lblBatchCreatedByReadOnly.Content = client.GetData<Person>( string.Format( "api/People/{0}", selectedBatch.CreatedByPersonId ) ).FullName;
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
            grdBatchItems.DataContext = transactions.OrderByDescending( a => a.TransactionDateTime );
        }

        /// <summary>
        /// Handles the Click event of the btnAddBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnAddBatch_Click( object sender, RoutedEventArgs e )
        {
            UpdateBatchUI( new FinancialBatch { Id = 0, BatchStartDateTime = DateTime.Now.Date, CreatedByPersonId = LoggedInPerson.Id } );
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
                FinancialTransaction financialTransaction = grdBatchItems.SelectedValue as FinancialTransaction;

                if ( financialTransaction != null )
                {
                    RockConfig config = RockConfig.Load();
                    RockRestClient client = new RockRestClient( config.RockBaseUrl );
                    client.Login( config.Username, config.Password );
                    financialTransaction.Images = client.GetData<List<FinancialTransactionImage>>( "api/FinancialTransactionImages", string.Format( "TransactionId eq {0}", financialTransaction.Id ) );
                    foreach ( var image in financialTransaction.Images )
                    {
                        image.BinaryFile = client.GetData<BinaryFile>( string.Format( "api/BinaryFiles/{0}", image.BinaryFileId ) );
                    }

                    BatchItemDetailPage.TransactionImageTypeValueFront = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_IMAGE_TYPE_CHECK_FRONT ) );
                    BatchItemDetailPage.TransactionImageTypeValueBack = client.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_IMAGE_TYPE_CHECK_BACK ) );
                    BatchItemDetailPage.FinancialTransaction = financialTransaction;
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
