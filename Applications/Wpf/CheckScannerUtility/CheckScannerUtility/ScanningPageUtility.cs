
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Rock.Apps.CheckScannerUtility.Models;
using Rock.Client;
using Rock.Client.Enums;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    public static class ScanningPageUtility
    {
        public static void Initalize()
        {
            ItemsUploaded = 0;
            KeepScanning = true;
            ItemsToProcess = 0;
            ItemsSkipped = 0;
            ItemsScanned = 0;
            TotalAmountScanned = 0;
            BatchAmount = 0;
    }

        public static BatchPage batchPage { get; set; }
        public static string DebugLogFilePath { get; set; }

        public static int ItemsUploaded { get; set; }

        public static bool KeepScanning { get; set; }

        public static int? ItemsToProcess { get; set; }

        public static int ItemsSkipped { get; set; }

        public static int ItemsScanned { get; set; }

        public static decimal TotalAmountScanned { get; set; }
        public static decimal BatchAmount { get;set; }
        public static List<FinancialTransaction> CurrentFinacialTransactions { get; set; }
        public static List<FinancialAccount> Accounts { get; set; }

        public static void ResumeScanning()
        {
            if ( batchPage.rangerScanner != null )
            {
                // StartFeeding doesn't work if the Scanner isn't in ReadyToFeed state, so assign StartRangerFeedingWhenReady if it isn't ready yet
                RangerTransportStates xportState = ( RangerTransportStates ) batchPage.rangerScanner.GetTransportState();
                if ( xportState == RangerTransportStates.TransportReadyToFeed )
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
        /// Writes to debug log.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteToDebugLog( string message )
        {
            if ( !string.IsNullOrWhiteSpace( DebugLogFilePath ) )
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
        public static ScannedDocInfo ConfirmUploadBadScannedDoc { get; set; }

        /// <summary>
        /// Shows the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void ShowException( Exception ex, Label lblExceptions )
        {
            App.LogException( ex );
            lblExceptions.Content = "ERROR: " + ex.Message;
            lblExceptions.Visibility = Visibility.Visible;
        }

        #region Upload Scanned Items

        /// <summary>
        /// Gets or sets the client that stays connected 
        /// </summary>
        /// <value>
        /// The persisted client.
        /// </value>
        public static RockRestClient UploadScannedItemClient { get; set; }


        /// <summary>
        /// Gets or sets the binary file type contribution for uploading transactions
        /// </summary>
        /// <value>
        /// The binary file type contribution.
        /// </value>
        private static BinaryFileType binaryFileTypeContribution { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value contribution for uploading transactions
        /// </summary>
        /// <value>
        /// The transaction type value contribution.
        /// </value>
        public static DefinedValue transactionTypeValueContribution { get; set; }

        /// <summary>
        /// Determines whether [is duplicate scan] [the specified scanned document].
        /// </summary>
        /// <param name="scannedDoc">The scanned document.</param>
        /// <returns></returns>
        public static bool IsDuplicateScan( ScannedDocInfo scannedDoc )
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
        public static bool UploadScannedItem( ScannedDocInfo scannedDocInfo, Action<int> UpdateProgressBarCallback = null, List<DisplayAccountValueModel> accounts = null )
        {
            try
            {
                RockRestClient client = EnsureUploadScanRestClient();

                // upload image of front of doc (if was successfully scanned)
                int? frontImageBinaryFileId = null;
                if ( scannedDocInfo.FrontImageData != null )
                {
                    string frontImageFileName = string.Format( "image1_{0}.png", DateTime.Now.ToString( "o" ).RemoveSpecialCharacters() );
                    frontImageBinaryFileId = client.UploadBinaryFile( frontImageFileName, Rock.Client.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid(), scannedDocInfo.FrontImagePngBytes, false );
                }

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
                if ( accounts != null )
                {
                    var accountsWithValues = accounts.Where( a => a.Amount > 0 ).ToList();
                    if ( accountsWithValues != null && accountsWithValues.Count() > 0 )
                    {

                        AddFinancialTransactionDetailForEachAccount( accountsWithValues, financialTransaction );
                    }
                }
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
                    //FinancialTransactionDetail
                    uploadedTransactionId = client.PostData<FinancialTransaction>( "api/FinancialTransactions", financialTransaction as FinancialTransaction ).AsIntegerOrNull();
                }

                // upload FinancialTransactionImage records for front/back
                if ( frontImageBinaryFileId.HasValue )
                {
                    FinancialTransactionImage financialTransactionImageFront = new FinancialTransactionImage();
                    financialTransactionImageFront.BinaryFileId = frontImageBinaryFileId.Value;
                    financialTransactionImageFront.TransactionId = uploadedTransactionId.Value;
                    financialTransactionImageFront.Order = 0;
                    financialTransactionImageFront.Guid = Guid.NewGuid();
                    client.PostData<FinancialTransactionImage>( "api/FinancialTransactionImages", financialTransactionImageFront );
                }

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

                ItemsUploaded++;
                if ( UpdateProgressBarCallback != null )
                {
                    UpdateProgressBarCallback( ItemsUploaded );
                }


                return true;
            }
            catch ( Exception e )
            {
                return false;
            }

        }

    
        private static void AddFinancialTransactionDetailForEachAccount( List<DisplayAccountValueModel> accounts,FinancialTransaction financialTransaction )
        {
            var tranactionDetails = new List<FinancialTransactionDetail>();
            if ( financialTransaction.TransactionDetails == null )
            {
                financialTransaction.TransactionDetails = new List<FinancialTransactionDetail>();
            }
            foreach ( var displayAccount in accounts )
            {
                var account = displayAccount.Account;
                financialTransaction.TransactionDetails.Add( new FinancialTransactionDetail { AccountId = account.Id,Amount=(decimal) displayAccount.Amount,Guid=Guid.NewGuid()} ); 
            }
        }

        /// <summary>
        /// Initializes the RestClient for Uploads and loads any data that is needed for the scan session (if it isn't already initialized)
        /// </summary>
        /// <returns></returns>
        public static RockRestClient EnsureUploadScanRestClient()
        {
            if ( UploadScannedItemClient == null )
            {
                RockConfig rockConfig = RockConfig.Load();
                UploadScannedItemClient = new RockRestClient( rockConfig.RockBaseUrl );
                UploadScannedItemClient.Login( rockConfig.Username, rockConfig.Password );
            }

            if ( binaryFileTypeContribution == null || transactionTypeValueContribution == null )
            {
                binaryFileTypeContribution = UploadScannedItemClient.GetDataByGuid<BinaryFileType>( "api/BinaryFileTypes", new Guid( Rock.Client.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE ) );
                transactionTypeValueContribution = UploadScannedItemClient.GetDataByGuid<DefinedValue>( "api/DefinedValues", new Guid( Rock.Client.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
            }

            return UploadScannedItemClient;
        }

        /// <summary>
        /// Starts the ranger feeding when ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public static void StartRangerFeedingWhenReady( object sender, AxRANGERLib._DRangerEvents_TransportReadyToFeedStateEvent e )
        {
            // only fire this event once
            batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
            batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
        }

        #endregion


        /// <summary>
        /// Handles the TransportIsDead event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public static void rangerScanner_TransportIsDead( object sender, EventArgs e, Action callback )
        {
            KeepScanning = false;
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportIsDead", DateTime.Now.ToString( "o" ) ) );
            callback.DynamicInvoke();
         
        }

        #region Image Upload related

        /// <summary>
        /// Gets the doc image.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        public static byte[] GetImageBytesFromRanger( RangerSides side )
        {
            RangerImageColorTypes colorType = RockConfig.Load().ImageColorType;

            int imageByteCount;
            imageByteCount = batchPage.rangerScanner.GetImageByteCount( ( int ) side, ( int ) colorType );
            if ( imageByteCount > 0 )
            {
                byte[] imageBytes = new byte[imageByteCount];

                // create the pointer and assign the Ranger image address to it
                IntPtr imgAddress = new IntPtr( batchPage.rangerScanner.GetImageAddress( ( int ) side, ( int ) colorType ) );

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

        public static void ShowScannerStatus( RangerTransportStates xportStates, System.Windows.Media.Color statusColor, string statusText, ref Ellipse shapeStatus )
        {
            switch ( xportStates )
            {
                case RangerTransportStates.TransportReadyToFeed:
                    break;

                case RangerTransportStates.TransportFeeding:
                    break;
            }

            shapeStatus.ToolTip = statusText;
            shapeStatus.Fill = new System.Windows.Media.SolidColorBrush( statusColor );
        }

        public static double GetPercentageAmountComplete()
        {
            var ret = Math.Round( ( double ) ( 100 * TotalAmountScanned ) / ( double ) BatchAmount ); 
            return ( double ) Math.Round( ( double ) ( 100 * TotalAmountScanned) / ( double ) BatchAmount );
        }

    }
}
