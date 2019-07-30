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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Rock.Apps.CheckScannerUtility.Models;
using Rock.Client;
using Rock.Client.Enums;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{

    public static class ScanningPageUtility
    {
        public static void Initialize()
        {
            ItemsUploaded = 0;
            KeepScanning = true;
            ItemsToProcess = 0;
            ItemsSkipped = 0;
            ItemsScanned = 0;
        }

        public static BatchPage batchPage { get; set; }

        public static string DebugLogFilePath { get; set; }

        public static int ItemsUploaded { get; set; }

        public static bool KeepScanning { get; set; }

        public static int? ItemsToProcess { get; set; }

        public static int ItemsSkipped { get; set; }

        public static int ItemsScanned { get; set; }

        public static List<FinancialAccount> Accounts { get; set; }

        /// <summary>
        /// Gets the visible accounts in a flat list sorted by parent order then child order recursively 
        /// </summary>
        /// <param name="visibleAccountIds">The visible account ids.</param>
        /// <param name="accountValueList">The account value list.</param>
        /// <returns></returns>
        public static List<DisplayAccountValueModel> GetVisibleAccountsSortedAndFlattened()
        {
            var visibleAccountIds = RockConfig.Load().SelectedAccountForAmountsIds;

            var allAccounts = ScanningPageUtility.Accounts.ToList();
            List<DisplayAccountValueModel> accountValueList = allAccounts.Select( a => new DisplayAccountValueModel( a, allAccounts ) ).ToList();

            var sortedRootAccounts = accountValueList.Where( a => a.ParentAccountId == null ).OrderBy( a => a.AccountOrder ).ThenBy( a => a.AccountDisplayName ).ToList();

            List<DisplayAccountValueModel> sortedDisplayedAccountList = new List<DisplayAccountValueModel>();

            foreach ( var rootAccount in sortedRootAccounts )
            {
                AddAccountRecursive( sortedDisplayedAccountList, rootAccount, visibleAccountIds );
            }

            int index = 0;
            foreach ( var displayedAccount in sortedDisplayedAccountList )
            {
                displayedAccount.DisplayIndex = index++;
            }

            return sortedDisplayedAccountList;
        }

        /// <summary>
        /// Adds the account and children accounts recursively 
        /// </summary>
        /// <param name="sortedDisplayedAccountList">The sorted displayed account list.</param>
        /// <param name="account">The root account.</param>
        private static void AddAccountRecursive( List<DisplayAccountValueModel> sortedDisplayedAccountList, DisplayAccountValueModel account, int[] visibleAccountIds )
        {
            if ( visibleAccountIds?.Contains( account.AccountId ) == true )
            {
                sortedDisplayedAccountList.Add( account );
            }

            foreach ( var childAccount in account.ChildAccounts )
            {
                AddAccountRecursive( sortedDisplayedAccountList, childAccount, visibleAccountIds );
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
        /// The ScannedDocInfo of a scan that the user needs to enter amounts for before upload
        /// </summary>
        /// <value>
        /// The prompt for amount scanned document.
        /// </value>
        public static ScannedDocInfo PromptForAmountScannedDoc { get; set; }

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
        public static void UploadScannedItem( ScannedDocInfo scannedDocInfo )
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
            var accounts = scannedDocInfo.AccountAmountCaptureList;
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
            batchPage.SelectedFinancialBatch.Transactions.Add( financialTransaction );

            var transactionList = batchPage.grdBatchItems.DataContext as BindingList<FinancialTransaction>;
            transactionList.Insert( 0, financialTransaction );

            ItemsUploaded++;

        }

        /// <summary>
        /// Adds the financial transaction detail for each account.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        /// <param name="financialTransaction">The financial transaction.</param>
        private static void AddFinancialTransactionDetailForEachAccount( List<DisplayAccountValueModel> accounts, FinancialTransaction financialTransaction )
        {
            var tranactionDetails = new List<FinancialTransactionDetail>();
            if ( financialTransaction.TransactionDetails == null )
            {
                financialTransaction.TransactionDetails = new List<FinancialTransactionDetail>();
            }
            foreach ( var displayAccount in accounts )
            {
                var account = displayAccount.Account;
                financialTransaction.TransactionDetails.Add( new FinancialTransactionDetail { AccountId = account.Id, Amount = ( decimal ) displayAccount.Amount, Guid = Guid.NewGuid() } );
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
    }
}
