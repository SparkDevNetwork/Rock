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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.AI.DocumentIntelligence;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Finance.FinancialBatchDetail;
using Rock.Common.Mobile.ViewModel;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

using BatchStatus = Rock.Model.BatchStatus;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// The Rock Mobile Financial Batch Detail block, used to display
    /// </summary>
    [DisplayName( "Financial Batch Detail" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Financial Batch Detail block." )]
    [IconCssClass( "ti ti-report-money" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]


    #region Block Attributes

    [LinkedPage( "Transaction Detail Page",
    Description = "The page linked when the user taps on a transaction in the list.",
        IsRequired = false,
        Key = AttributeKeys.DetailPage,
        Order = 1 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKeys.Accounts,
        Description = "The list of accounts to display.",
        Order = 2 )]

    [TextField( "Document Intelligence API Key",
        Key = AttributeKeys.ApiKey,
        Description = "The API key obtained from Azure Document Intelligence.",
        IsRequired = false,
        Order = 3 )]

    [TextField( "Document Intelligence Endpoint",
        Key = AttributeKeys.EndPoint,
        Description = "The endpoint used to access Azure Document Intelligence.",
        IsRequired = false,
        Order = 4 )]

    [BooleanField( "Accounts Allocation Required",
        Key = AttributeKeys.AccountAllocationRequired,
        Description = "whether account allocation fields is required.",
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField( "Required Control Amount",
        Description = "Whether or not the control amount field is required when adding financial batch.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKeys.RequiredControlAmount,
        Order = 6 )]

    [BooleanField( "Required Control Item Count",
        Description = "Whether or not the control item count field is required when adding financial batch.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKeys.RequiredControlItemCount,
        Order = 7 )]

    [DefinedValueField(
        "Currency Types",
        Description = "The currency type to show in the scan settings page.",
        DefinedTypeGuid = SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = null,
        Key = AttributeKeys.CurrencyType,
        Order = 8 )]

    [DefinedValueField(
        "Transaction Source",
        Description = "The Transaction Source to show in the scan settings page.",
        DefinedTypeGuid = SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = null,
        Key = AttributeKeys.TransactionSource,
        Order = 9 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL )]
    public class FinancialBatchDetail : RockBlockType
    {
        #region Properties

        private List<Guid> Accounts => GetAttributeValue( AttributeKeys.Accounts ).SplitDelimitedValues().AsGuidList();

        #endregion

        #region Keys

        /// <summary>
        /// Keys for the attributes used in this block.
        /// </summary>
        private static class AttributeKeys
        {
            /// <summary>
            /// The key for the Detail Page attribute.
            /// </summary>
            public const string DetailPage = "DetailPage";

            /// <summary>
            /// The key for the Required Control Amount attribute.
            /// </summary>
            public const string RequiredControlAmount = "RequiredControlAmount";

            /// <summary>
            /// The key for the Required Control Item Count attribute.
            /// </summary>
            public const string RequiredControlItemCount = "RequiredControlItemCount";

            /// <summary>
            /// The key for the Accounts attribute.
            /// </summary>
            public const string Accounts = "Accounts";

            /// <summary>
            /// The key for the Document Intelligence API Key attribute.
            /// </summary>
            public const string ApiKey = "ApiKey";

            /// <summary>
            /// The key for the Document Intelligence Endpoint attribute.
            /// </summary>
            public const string EndPoint = "EndPoint";

            /// <summary>
            /// The key for the Accounts Allocation Required attribute.
            /// </summary>
            public const string AccountAllocationRequired = "AccountAllocationRequired";

            /// <summary>
            /// The key for the Currency Types attribute.
            /// </summary>
            public const string CurrencyType = "CurrencyTypes";

            /// <summary>
            /// The key for the Transaction Source attribute.
            /// </summary>
            public const string TransactionSource = "TransactionSource";
        }

        /// <summary>
        /// The authorization action required to reopen a batch.
        /// </summary>
        private const string AuthorizationReopenBatch = "ReopenBatch";

        #endregion

        #region Methods

        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialBatch entity, out BlockActionResult error )
        {
            var entityService = new FinancialBatchService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey );
            }
            else
            {
                // Create a new entity.
                entity = new FinancialBatch();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialBatch.FriendlyTypeName} not found." );
                return false;
            }

            var isReopenDisabled = entity.Status == BatchStatus.Closed && !entity.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson );
            if ( entity.IsAutomated || !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) || isReopenDisabled )
            {
                error = ActionBadRequest( $"Not authorized to edit {FinancialBatch.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the batch transactions for the specified batch key, starting at the specified index and returning the specified count of transactions.
        /// </summary>
        /// <param name="batchKey"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<BatchTransactionsBag> GetBatchTransactions( string batchKey, int startIndex, int count )
        {
            var batch = new FinancialBatchService( RockContext ).Get( batchKey );
            if ( batch == null )
            {
                return new List<BatchTransactionsBag>();
            }


            var transactions = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( t => t.BatchId.HasValue && t.BatchId.Value == batch.Id )
                .OrderByDescending( t => t.Id )
                .Skip( startIndex )
                .Take( count )
                .ToList()
                .Select( t =>
                {
                    bool? isMicrStatusGood =
                        t.MICRStatus == MICRStatus.Success ? true
                      : t.MICRStatus == MICRStatus.Fail ? false
                      : ( bool? ) null;

                    return new BatchTransactionsBag
                    {
                        Id = t.Id,
                        IdKey = t.IdKey,
                        IsMicrStatusGood = isMicrStatusGood,
                        TransactionDateTime = t.TransactionDateTime,
                        TransactionCode = t.TransactionCode ?? "",
                        Amount = t.TotalAmount,
                        CurrencyTypeValueId = t.FinancialPaymentDetail?.CurrencyTypeValueId,
                        CurrencyTypeName = DefinedValueCache.GetName( t.FinancialPaymentDetail.CurrencyTypeValueId ) ?? "",
                        Accounts = t.TransactionDetails?.Select( td => td.Account.Name ).ToList() ?? new List<string>(),
                    };
                } )
                .ToList();

            return transactions;
        }

        /// <summary>
        /// Gets the available accounts in chunks to prevent SQL complexity errors.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private List<ListItemViewModel> GetAvailableAccounts( RockContext rockContext )
        {
            if ( !Accounts.Any() )
            {
                // If no accounts are specified, return an empty list.
                return new List<ListItemViewModel>();
            }

            var financialAccountService = new FinancialAccountService( rockContext );

            var availableAccounts = financialAccountService.Queryable()
            .Where( f =>
                f.IsActive
                    && f.IsPublic.HasValue
                    && f.IsPublic.Value
                    && ( f.StartDate == null || f.StartDate <= RockDateTime.Today )
                    && ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) );
            availableAccounts = availableAccounts.Where( a => Accounts.Contains( a.Guid ) );

            var accounts = availableAccounts.OrderBy( f => f.Order )
                .ToList()
                .Select( a => new ListItemViewModel
                {
                    Text = a.Name,
                    Value = a.IdKey,
                } )
                .ToList();

            return accounts;
        }

        /// <summary>
        /// Processes the image and generates a financial transaction based on the provided image and batch information.
        /// </summary>
        /// <param name="processImageBag"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private void ProcessImageAndGenerateTransaction( ProcessImageBag processImageBag, Rock.Model.BinaryFile image )
        {
            // Create FinancialPaymentDetail
            if ( processImageBag.BatchIdKey.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "BatchIdKey must not be null or whitespace.", nameof( processImageBag.BatchIdKey ) );
            }

            var currencyTypeValue = DefinedValueCache.Get( processImageBag.CurrencyTypesIdKey );


            FinancialPaymentDetail financialPaymentDetail = new FinancialPaymentDetail();
            financialPaymentDetail.CurrencyTypeValueId = currencyTypeValue.Id;
            financialPaymentDetail.Guid = Guid.NewGuid();

            var financialPaymentDetailService = new FinancialPaymentDetailService( RockContext );
            financialPaymentDetailService.Add( financialPaymentDetail );
            RockContext.SaveChanges();

            // Create FinancialTransaction
            FinancialTransaction financialTransaction = new FinancialTransaction();
            FinancialBatch batch = new FinancialBatchService( RockContext ).Get( processImageBag.BatchIdKey );
            financialTransaction.BatchId = batch.Id;
            financialTransaction.TransactionCode = string.Empty;
            financialTransaction.Summary = string.Empty;

            financialTransaction.Guid = Guid.NewGuid();
            financialTransaction.CreatedDateTime = RockDateTime.Now;
            financialTransaction.TransactionDateTime = batch.BatchStartDateTime;

            financialTransaction.FinancialPaymentDetailId = financialPaymentDetail.Id;
            var transactionSource = DefinedValueCache.GetByIdKey( processImageBag.TransactionSourceIdKey );
            financialTransaction.SourceTypeValueId = transactionSource.Id;

            financialTransaction.TransactionTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ) ?? 0;

            // Add FinancialTransactionDetail for each account in the batch.
            financialTransaction.TransactionDetails = new List<FinancialTransactionDetail>();
            AddFinancialTransactionDetailForEachAccount( financialTransaction, processImageBag.AccountBox );

            var financialTransactionService = new FinancialTransactionService( RockContext );
            financialTransactionService.Add( financialTransaction );

            RockContext.SaveChanges();

            var checkCurrencyType = DefinedValueCache.Get( SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid() ).Guid.ToString();
            var isCheck = processImageBag.CurrencyTypesIdKey == checkCurrencyType;

            if ( isCheck )
            {
                _ = Task.Run( () => ExtractAndPopulateMicrDataAsync( image, financialTransaction.IdKey ) );
            }

            // Create the FinancialTransactionImage
            if ( processImageBag.ImageByte != null && processImageBag.ImageByte.Any() )
            {
                FinancialTransactionImage financialTransactionImageFront = new FinancialTransactionImage();
                financialTransactionImageFront.BinaryFileId = processImageBag.ImageId;
                financialTransactionImageFront.TransactionId = financialTransaction.Id;
                financialTransactionImageFront.Order = 0;
                financialTransactionImageFront.Guid = Guid.NewGuid();

                new FinancialTransactionImageService( RockContext ).Add( financialTransactionImageFront );
                RockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Extracts the MICR data from the provided image and populates the financial transaction with the extracted data.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="idKey"></param>
        /// <returns></returns>
        private async Task ExtractAndPopulateMicrDataAsync( Rock.Model.BinaryFile image, string idKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var cts = new CancellationTokenSource( TimeSpan.FromMinutes( 2 ) );

                try
                {
                    // Extract the MICR using Azure Document Intelligence and save the information in the FinancialTransactionScannedCheck.
                    var scannedInfo = await ExtractMicrInformationAsync( image, cts.Token );

                    FinancialTransactionService financialTransactionService = new FinancialTransactionService( rockContext );
                    var financialTransaction = financialTransactionService.Get( idKey );
                    if ( financialTransaction == null )
                    {
                        Logger.LogError( "FinancialTransaction with IdKey {idKey} not found.", idKey );
                        return;
                    }

                    financialTransaction.TransactionCode = scannedInfo.CheckNumber;
                    financialTransaction.MICRStatus = scannedInfo.BadMicr ? MICRStatus.Fail : MICRStatus.Success;
                    financialTransaction.CheckMicrEncrypted = Encryption.EncryptString( scannedInfo.ScannedCheckMicrData );

                    if ( scannedInfo.BadMicr && Logger.IsEnabled( LogLevel.Error ) )
                    {
                        Logger.LogError( "Failed to Extract Micr from image. Result: {scannedInfo}.", scannedInfo.ToJson() );
                    }

                    // note: BadMicr scans don't get checked for duplicates, but just in case, make sure that CheckMicrHash isn't set if this has a bad MICR read
                    if ( financialTransaction.MICRStatus != MICRStatus.Fail )
                    {
                        financialTransaction.CheckMicrHash = Encryption.GetSHA1Hash( scannedInfo.ScannedCheckMicrData );
                    }

                    financialTransaction.CheckMicrParts = Encryption.EncryptString( scannedInfo.ScannedCheckMicrParts );

                    rockContext.SaveChanges();
                }
                catch ( OperationCanceledException )
                {
                    Logger.LogError( "MICR extraction operation was cancelled due to timeout." );
                }
                catch ( Exception ex )
                {
                    Logger.LogError( ex, "An error occurred while extracting MICR data from the image." );
                    ExceptionLogService.LogException( ex );
                }
                finally
                {
                    cts.Dispose();
                }
            }
        }

        /// <summary>
        /// Extracts the MICR data from the provided image using Azure Document Intelligence.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ScannedDocInfo> ExtractMicrInformationAsync( Rock.Model.BinaryFile image, CancellationToken cancellationToken )
        {
            BinaryData data = null;
            if ( image.DatabaseData != null )
            {
                data = new BinaryData( image.DatabaseData?.Content );
            }
            else
            {
                data = await BinaryData.FromStreamAsync( image.ContentStream );
            }

            var client = new DocumentIntelligenceClient( new Uri( GetAttributeValue( AttributeKeys.EndPoint ) ), new Azure.AzureKeyCredential( GetAttributeValue( AttributeKeys.ApiKey ) ) );
            var result = await client.AnalyzeDocumentAsync( Azure.WaitUntil.Completed, "prebuilt-check.us", data, cancellationToken );

            if ( result.Value.Documents.Any() && result.Value.Documents[0].Fields["MICR"].ValueDictionary.Any() )
            {
                var accNumber = result.Value.Documents[0].Fields["MICR"].ValueDictionary["AccountNumber"].ValueString;
                var routingNumber = result.Value.Documents[0].Fields["MICR"].ValueDictionary["RoutingNumber"].ValueString;
                var checkNumber = result.Value.Documents[0].Fields["MICR"].ValueDictionary["CheckNumber"].ValueString;
                var micrData = result.Value.Documents[0].Fields["MICR"].Content;

                return new ScannedDocInfo
                {
                    ScannedCheckMicrData = micrData,
                    ScannedCheckMicrParts = $"{routingNumber}_{accNumber}_{checkNumber}",
                    CheckNumber = checkNumber,
                    AccountNumber = accNumber,
                    RoutingNumber = routingNumber,
                };
            }

            return new ScannedDocInfo
            {
                ScannedCheckMicrData = string.Empty,
                ScannedCheckMicrParts = string.Empty,
                CheckNumber = string.Empty,
                AccountNumber = string.Empty,
                RoutingNumber = string.Empty,
                BadMicr = true
            };
        }

        /// <summary>
        /// Adds a financial transaction detail for each account in the provided accounts box list.
        /// </summary>
        /// <param name="financialTransaction"></param>
        /// <param name="accountsBoxList"></param>
        private void AddFinancialTransactionDetailForEachAccount( FinancialTransaction financialTransaction, List<ListItemViewModel> accountsBoxList )
        {
            foreach ( var accountBox in accountsBoxList )
            {
                var accountIdKey = accountBox.Text;
                FinancialAccountCache account = FinancialAccountCache.GetByIdKey( accountIdKey );
                if ( account == null )
                {
                    continue;
                }

                var financialTransactionDetail = new FinancialTransactionDetail
                {
                    AccountId = account.Id,
                    Guid = Guid.NewGuid()
                };

                var amount = accountBox.Value.AsDecimalOrNull();
                if ( amount.HasValue )
                {
                    financialTransactionDetail.Amount = amount.Value;
                }

                financialTransaction.TransactionDetails.Add( financialTransactionDetail );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the financial batch.
        /// </summary>
        /// <param name="financialBatchBag"></param>
        /// <returns></returns>
        [BlockAction( "Save" )]
        public BlockActionResult Save( AddFinancialBatchBag financialBatchBag )
        {
            if ( financialBatchBag == null )
            {
                return ActionBadRequest( "Financial batch data is required." );
            }

            var financialBatchService = new FinancialBatchService( RockContext );
            var idKey = financialBatchBag.IdKey;

            // Determine if we are editing or creating new batch.
            if ( !TryGetEntityForEditAction( idKey, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            if ( batch.Status == BatchStatus.Closed && financialBatchBag.Status.ConvertToEnum<BatchStatus>() != BatchStatus.Closed )
            {
                if ( !batch.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized( "User is not authorized to reopen a closed batch" );
                }
            }

            var isNew = batch.Id == 0;
            var isStatusChanged = financialBatchBag.Status.ConvertToEnum<BatchStatus>() != batch.Status;

            var changes = new History.HistoryChangeList();
            if ( isNew )
            {
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
            }


            var currentCampusName = CampusCache.Get( batch.CampusId ?? 0 )?.Name ?? "None";
            var newCampus = CampusCache.Get( financialBatchBag.Campus );

            History.EvaluateChange( changes, "Batch Name", batch.Name, financialBatchBag.Name );
            History.EvaluateChange( changes, "Campus", currentCampusName, newCampus?.Name ?? "None" );
            History.EvaluateChange( changes, "Status", batch?.Status, financialBatchBag.Status.ConvertToEnum<BatchStatus>() );
            History.EvaluateChange( changes, "Start Date/Time", batch.BatchStartDateTime, financialBatchBag.BatchStartDate );
            History.EvaluateChange( changes, "End Date/Time", batch.BatchEndDateTime, financialBatchBag.BatchEndDate );
            History.EvaluateChange( changes, "Control Amount", batch?.ControlAmount.FormatAsCurrency(), ( financialBatchBag.ControlAmount ?? 0.0m ).FormatAsCurrency() );
            History.EvaluateChange( changes, "Control Item Count", batch.ControlItemCount, financialBatchBag.ControlItemCount );
            //History.EvaluateChange( changes, "Accounting System Code", batch.AccountingSystemCode, financialBatchBag.AccountingSystemCode );
            History.EvaluateChange( changes, "Notes", batch.Note, financialBatchBag.Note );

            // Replicating the behavior in the Webforms block where the batch end date is set
            // to the next day after the start date if not provided.
            batch.BatchEndDateTime = financialBatchBag.BatchEndDate;
            batch.BatchStartDateTime = financialBatchBag.BatchStartDate;
            batch.CampusId = newCampus?.Id;
            batch.ControlAmount = financialBatchBag.ControlAmount ?? 0.0m;
            batch.ControlItemCount = financialBatchBag.ControlItemCount ?? 0;
            batch.Name = financialBatchBag.Name;
            batch.Note = financialBatchBag.Note;
            batch.Status = financialBatchBag.Status.ConvertToEnum<BatchStatus>();
            if ( batch.BatchEndDateTime == null )
            {
                batch.BatchEndDateTime = batch.BatchStartDateTime?.AddDays( 1 );
            }


            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                if ( changes.Any() )
                {
                    HistoryService.SaveChanges(
                        RockContext,
                        typeof( FinancialBatch ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                        batch.Id,
                        changes );
                }

                // NOT SUPPORT ATTRIBUTE IN MOBILE IT YET.
                //batch.SaveAttributeValues( RockContext );

            } );

            return ActionOk( batch.IdKey );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction( "Delete" )]
        public BlockActionResult Delete( string key )
        {
            FinancialBatchService financialBatchService = new FinancialBatchService( RockContext );

            if ( !TryGetEntityForEditAction( key, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            if ( !financialBatchService.CanDelete( batch, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            FinancialBatch financialBatch = financialBatchService.Get( key );

            FinancialTransactionService financialTransactionService = new FinancialTransactionService( RockContext );
            financialTransactionService.DeleteRange( financialBatch.Transactions );

            RockContext.SaveChanges();

            financialBatchService.Delete( batch );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction( "Edit" )]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            var bag = new FinancialBatchDetailBag
            {
                IdKey = batch.IdKey,
                Id = batch.Id,
                Name = batch.Name,
                Status = batch.Status.ToString(),
                BatchStartDate = batch.BatchStartDateTime,
                BatchEndDate = batch.BatchEndDateTime,
                TransactionAmount = batch.GetTotalTransactionAmount( RockContext ),
                ControlAmount = batch.ControlAmount,
                ControlItemCount = batch.ControlItemCount,
                Campus = batch.Campus?.Name ?? "",
                CampusGuid = batch.Campus?.Guid.ToString() ?? "",
                Note = batch.Note,
            };

            // NOT SUPPORT YET IN MOBILE.
            //bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return ActionOk( bag );
        }

        /// <summary>
        /// Gets the financial batch detail.
        /// </summary>
        /// <returns></returns>
        [BlockAction( "GetBatchDetails" )]
        public BlockActionResult GetBatchDetails( string key )
        {
            var batch = new FinancialBatchService( RockContext ).Get( key );

            // Grab the financial transaction that belong to the batch.
            var batchTransactionsQuery = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( ft => ft.BatchId.HasValue && ft.BatchId.Value == batch.Id );

            // Get the transaction count that belong to the batch.
            var transactionItemCount = batchTransactionsQuery.Count();

            // Get the Currency Totals
            var currencyTotals = batchTransactionsQuery
                .Where( t => t.FinancialPaymentDetailId.HasValue )
                .GroupBy( ft => ft.FinancialPaymentDetail.CurrencyTypeValueId )
                .ToList()
                .Select( g => new CurrencyTotalsBag
                {
                    CurrencyTypeValueId = g.Key ?? 0,
                    CurrencyName = DefinedValueCache.GetName( g.Key ),
                    Amount = g.Sum( pd => pd.TotalAmount )
                } )
                .ToList();

            // Grab the financial transaction detail that belong to the batch.
            var batchFinancialTransactionDetails = new FinancialTransactionDetailService( RockContext )
                .Queryable()
                .Where( ftd => ftd.Transaction.BatchId.HasValue && ftd.Transaction.BatchId.Value == batch.Id );

            // Get the Account Totals.
            var accountTotals = batchFinancialTransactionDetails
                .GroupBy( ftd => ftd.Account )
                .Select( g => new AccountTotalsBag
                {
                    AccountId = g.Key.Id,
                    AccountName = g.Key.Name,
                    TotalAmount = g.Sum( ftd => ftd.Amount )
                } )
                .ToList();

            return ActionOk( new FinancialBatchDetailBag
            {
                Id = batch?.Id,
                IdKey = batch?.IdKey,
                Name = batch?.Name,
                Status = batch?.Status.ConvertToString(),
                BatchStartDate = batch?.BatchStartDateTime,
                BatchEndDate = batch?.BatchEndDateTime,
                TransactionAmount = batch?.GetTotalTransactionAmount( RockContext ),
                TransactionCount = transactionItemCount,
                CurrencyTotals = currencyTotals,
                AccountTotals = accountTotals,
                ControlAmount = batch?.ControlAmount,
                ControlItemCount = batch?.ControlItemCount,
                Campus = batch?.Campus?.Name,
                CampusGuid = batch?.Campus?.Guid.ToString(),
                Note = batch?.Note
            } );
        }

        /// <summary>
        /// Gets the transactions for the specified batch key, starting at the specified index and returning the specified count of transactions.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [BlockAction( "GetTransactions" )]
        public BlockActionResult GetTransactions( BatchTransactionsOption options )
        {
            var transactions = GetBatchTransactions( options.BatchKey, options.StartIndex, options.Count );

            return ActionOk( transactions );
        }

        /// <summary>
        /// Extracts the MICR information from the specified check.
        /// </summary>
        /// <param name="processImageBag"></param>
        /// <returns></returns>
        [BlockAction( "ProcessImage" )]
        public BlockActionResult ProcessImage( ProcessImageBag processImageBag )
        {
            var binaryFileService = new BinaryFileService( RockContext );
            var image = binaryFileService.Get( processImageBag.ImageId );

            if ( image == null )
            {
                return ActionNotFound( "Binary file not found." );
            }

            if ( image.BinaryFileType.Guid != Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid() )
            {
                return ActionBadRequest( "Binary file is not a MICR check." );
            }

            try
            {
                ProcessImageAndGenerateTransaction( processImageBag, image );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                // If the image could not be loaded, return an error.
                return ActionBadRequest( "Could not load the image from the binary file." );
            }

            return ActionOk();
        }

        /// <summary>
        /// When a batch is pending, this action will open the batch for review.
        /// </summary>
        /// <param name="batchIdKey"></param>
        /// <returns></returns>
        [BlockAction( "OpenBatch" )]
        public BlockActionResult OpenBatch( string batchIdKey )
        {
            var financialBatchService = new FinancialBatchService( RockContext );
            var batch = financialBatchService.Get( batchIdKey );

            if ( batch == null )
            {
                return ActionNotFound( $"{FinancialBatch.FriendlyTypeName} not found." );
            }

            if ( batch.Status == BatchStatus.Closed && !batch.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "User is not authorized to reopen a closed batch" );
            }

            batch.Status = BatchStatus.Open;
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            var isCheckScannerConfigured = GetAttributeValue( AttributeKeys.ApiKey ).IsNotNullOrWhiteSpace() && GetAttributeValue( AttributeKeys.EndPoint ).IsNotNullOrWhiteSpace();

            var selectedCurrencyType = GetAttributeValue( AttributeKeys.CurrencyType );
            var selectedTransactionSource = GetAttributeValue( AttributeKeys.TransactionSource );

            var currencyTypeGuids = selectedCurrencyType.SplitDelimitedValues().Select( s => s.ConvertToGuidOrThrow() ).ToList();
            List<ListItemViewModel> currencyType = new List<ListItemViewModel>();
            if ( currencyTypeGuids.Count == 0 )
            {
                currencyType = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ).DefinedValues
                    .Select( dv => new ListItemViewModel
                    {
                        Text = dv.Value,
                        Value = dv.Guid.ToString(),
                    } ).ToList();
            }
            else
            {
                currencyType = DefinedValueCache.GetMany( currencyTypeGuids ).Select( dv => new ListItemViewModel
                {
                    Text = dv.Value,
                    Value = dv.Guid.ToString(),
                } ).ToList();
            }

            var transactionSourceGuids = selectedTransactionSource.SplitDelimitedValues().Select( s => s.ConvertToGuidOrThrow() ).ToList();
            List<ListItemViewModel> transactionSources = new List<ListItemViewModel>();
            if ( transactionSourceGuids.Count == 0 )
            {
                transactionSources = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ).DefinedValues
                    .Select( dv => new ListItemViewModel
                    {
                        Text = dv.Value,
                        Value = dv.IdKey,
                    } ).ToList();
            }
            else
            {
                transactionSources = DefinedValueCache.GetMany( transactionSourceGuids ).Select( dv => new ListItemViewModel
                {
                    Text = dv.Value,
                    Value = dv.IdKey,
                } ).ToList();
            }

            return new Rock.Common.Mobile.Blocks.Finance.FinancialBatchDetail.Configuration
            {
                AccountsAllocationRequired = GetAttributeValue( AttributeKeys.AccountAllocationRequired ).AsBoolean(),
                RequiredControlItemCount = GetAttributeValue( AttributeKeys.RequiredControlItemCount ).AsBoolean(),
                RequiredControlAmount = GetAttributeValue( AttributeKeys.RequiredControlAmount ).AsBoolean(),
                CheckScannerEnabled = isCheckScannerConfigured,
                TransactionDetailPageGuid = GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull(),
                Accounts = GetAvailableAccounts( RockContext ),
                CurrencyTypes = currencyType,
                TransactionSources = transactionSources,
            };
        }

        #endregion

        /// <summary>
        /// Represents the bag used to hold the financial batch detail information.
        /// </summary>
        public class ScannedDocInfo
        {
            /// <summary>
            /// Gets or sets the scanned check MICR data.
            /// </summary>
            public string ScannedCheckMicrData { get; set; }

            /// <summary>
            /// Gets the scanned check micr in the format "{RoutingNumber}_{AccountNumber}_{CheckNumber}";
            /// </summary>
            public string ScannedCheckMicrParts { get; set; }

            /// <summary>
            /// Gets or sets the check number.
            /// </summary>
            public string CheckNumber { get; set; }

            /// <summary>
            /// Gets or sets the account number.
            /// </summary>
            public string AccountNumber { get; set; }

            /// <summary>
            /// Gets or sets the routing number.
            /// </summary>
            public string RoutingNumber { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the MICR data is bad.
            /// </summary>
            public bool BadMicr { get; set; }
        }
    }
}