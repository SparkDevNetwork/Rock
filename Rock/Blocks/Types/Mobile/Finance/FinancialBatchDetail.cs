using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Finance.FinancialBatchDetail;
using Rock.Common.Mobile.ViewModel;
using Rock.Data;
using Rock.Model;
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
    [IconCssClass( "fa fa-money-bills" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "Page to link to when user taps on a transaction list.",
        IsRequired = false,
        Key = AttributeKeys.DetailPage,
        Order = 1 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKeys.Accounts,
        Description = "The accounts to display.",
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL )]
    public class FinancialBatchDetail : RockBlockType
    {
        #region Properties

        private List<Guid> Accounts => GetAttributeValue( AttributeKeys.Accounts ).SplitDelimitedValues().AsGuidList();

        #endregion
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
            /// The key for the Accounts attribute.
            /// </summary>
            public const string Accounts = "Accounts";
        }

        private const string AuthorizationReopenBatch = "ReopenBatch";

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
                .Select( t => new BatchTransactionsBag
                {
                    Id = t.Id,
                    IdKey = t.IdKey,
                    TransactionDateTime = t.TransactionDateTime,
                    TransactionCode = t.TransactionCode ?? "",
                    Amount = t.TotalAmount,
                    CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                    CurrencyTypeName = DefinedValueCache.GetName( t.FinancialPaymentDetail.CurrencyTypeValueId ) ?? "",
                    Accounts = t.TransactionDetails.Select( td => td.Account.Name ).ToList() ?? new List<string>(),
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
                    && ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
            .Include( f => f.ImageBinaryFile );
            availableAccounts = availableAccounts.Where( a => Accounts.Contains( a.Guid ) );

            var accounts = availableAccounts.OrderBy( f => f.Order )
                .Select( a => new ListItemViewModel
                {
                    Text = a.Name,
                    Value = a.Id.ToString(),
                } )
                .ToList();

            return accounts;
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

            BlockActionResult error;

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
            var entityService = new FinancialBatchService( RockContext );

            if ( !TryGetEntityForEditAction( key, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( batch, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( batch );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
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

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Finance.FinancialBatchDetail.Configuration
            {
                TransactionDetailPageGuid = GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull(),
                Accounts = GetAvailableAccounts( RockContext ),
                CurrencyTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() )
                    .DefinedValues
                    .Select( dv => new ListItemViewModel
                    {
                        Text = dv.Value,
                        Value = dv.Id.ToString()
                    } )
                    .ToList(),
                TransactionSources = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() )
                    .DefinedValues
                    .Select( dv => new ListItemViewModel
                    {
                        Text = dv.Value,
                        Value = dv.Id.ToString()
                    } )
                    .ToList(),
            };
        }

        #endregion
    }
}