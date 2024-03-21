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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialBatchList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial batches.
    /// </summary>

    [DisplayName( "Financial Batch List" )]
    [Category( "Finance" )]
    [Description( "Displays a list of financial batches." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the financial batch details.",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [BooleanField( "Show Accounting System Code",
        Description = "Should the code from the accounting system column be displayed.",
        Key = AttributeKey.ShowAccountingSystemCode,
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField( "Show Accounts Column",
        Description = "Should the accounts column be displayed.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowAccountsColumn,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "a68dd358-1392-475f-92b4-dea544ff219e" )]
    [Rock.SystemGuid.BlockTypeGuid( "f1950524-e959-440f-9cf6-1a8b9b7527d8" )]
    [CustomizedGrid]
    public class FinancialBatchList : RockListBlockType<FinancialBatchList.BatchData>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";

            public const string ShowAccountingSystemCode = "ShowAccountingCode";

            public const string ShowAccountsColumn = "ShowAccountsColumn";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterDaysBack = "filter-days-back";

            public const string FilterContainsSource = "filter-contains-source";

            public const string FilterContainsTransactionType = "filter-contains-transaction-type";

            public const string FilterAccounts = "filter-accounts";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the number of days back to include in the results. A value of
        /// zero means no filtering should be performed.
        /// </summary>
        /// <value>
        /// The number of days back to include in the results.
        /// </value>
        protected int FilterDaysBack => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDaysBack )
            .AsIntegerOrNull() ?? 180;

        /// <summary>
        /// Gets the source identifier to use when filtering the batches. Only
        /// batches that have a transaction matching this source value will be
        /// included.
        /// </summary>
        /// <value>
        /// The source identifier to use when filtering the batches.
        /// </value>
        protected Guid? FilterContainsSource => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterContainsSource )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the transaction type identifier to use when filtering the batches.
        /// Only batches that have a transaction matching this transaction type
        /// value will be included.
        /// </summary>
        /// <value>
        /// The transaction type identifier to use when filtering the batches.
        /// </value>
        protected Guid? FilterContainsTransactionType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterContainsTransactionType )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the account identifiers to use when filtering the batches. Only
        /// batches that have a transaction with a detail item going to one of
        /// these accounts will be included.
        /// </summary>
        /// <value>
        /// The account identifiers to use when filtering the batches.
        /// </value>
        protected List<Guid> FilterAccounts => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterAccounts )
            .FromJsonOrNull<List<ListItemBag>>()
            ?.Select( li => li.Value.AsGuid() )
            .ToList() ?? new List<Guid>();

        #endregion

        #region Fields

        /// <summary>
        /// The batch attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FinancialBatchListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = box.IsAddEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialBatchListOptionsBag GetBoxOptions()
        {
            var options = new FinancialBatchListOptionsBag();

            if ( CampusCache.All().Count( c => c.IsActive == true ) > 1 )
            {
                options.HasMultipleCampuses = true;
            }

            options.ShowAccountingSystemCodeColumn = GetAttributeValue( AttributeKey.ShowAccountingSystemCode ).AsBoolean();
            options.ShowAccountsColumn = GetAttributeValue( AttributeKey.ShowAccountsColumn ).AsBoolean();

            options.TransactionTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )
                .DefinedValues
                .OrderBy( dv => dv.Order )
                .ToListItemBagList();

            options.Sources = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE )
                .DefinedValues
                .OrderBy( dv => dv.Order )
                .ToListItemBagList();

            var currencyInfo = new RockCurrencyCodeInfo();
            options.CurrencyInfo = new ViewModels.Utility.CurrencyInfoBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces,
                SymbolLocation = currencyInfo.SymbolLocation
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "BatchId", "((Key))" )
            };
        }

        /// <summary>
        /// Get a queryable for batches that is properly filtered.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A queryable for <see cref="FinancialBatch"/>.</returns>
        private IQueryable<FinancialBatch> GetBatchQueryable( RockContext rockContext )
        {
            var qry = new FinancialBatchService( rockContext )
                .Queryable()
                .Where( b => b.BatchStartDateTime.HasValue );

            if ( FilterDaysBack > 0 )
            {
                var filterDate = RockDateTime.Today.AddDays( -FilterDaysBack );

                qry = qry.Where( b => b.BatchStartDateTime.Value >= filterDate );
            }

            if ( FilterContainsSource.HasValue )
            {
                var sourceTypeId = DefinedValueCache.GetId( FilterContainsSource.Value ) ?? 0;

                qry = qry.Where( b => b.Transactions.Any( t => t.SourceTypeValueId == sourceTypeId ) );
            }

            if ( FilterContainsTransactionType.HasValue )
            {
                var transactionTypeId = DefinedValueCache.GetId( FilterContainsTransactionType.Value ) ?? 0;

                qry = qry.Where( b => b.Transactions.Any( t => t.TransactionTypeValueId == transactionTypeId ) );
            }

            if ( FilterAccounts.Any() )
            {
                var accountIdsQry = new FinancialAccountService( rockContext )
                    .Queryable()
                    .Where( a => FilterAccounts.Contains( a.Guid ) )
                    .Select( a => a.Id );

                var transactionIdQry = new FinancialTransactionDetailService( rockContext )
                    .Queryable()
                    .Where( ftd => accountIdsQry.Contains( ftd.AccountId )
                        && ftd.Transaction.BatchId.HasValue )
                    .Select( ftd => ftd.TransactionId );

                qry = qry.Where( b => b.Transactions.Any( t => transactionIdQry.Contains( t.Id ) ) );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<BatchData> GetListQueryable( RockContext rockContext )
        {
            return GetBatchQueryable( rockContext )
                .Select( b => new BatchData
                {
                    Batch = b,
                    TransactionCount = b.Transactions.Count()
                } );
        }

        /// <inheritdoc/>
        protected override IQueryable<BatchData> GetOrderedListQueryable( IQueryable<BatchData> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( b => b.Batch.BatchStartDateTime );
        }

        /// <inheritdoc/>
        protected override List<BatchData> GetListItems( IQueryable<BatchData> queryable, RockContext rockContext )
        {
            // Load all the batches into memory.
            var items = queryable.ToList();

            // Load any attribute column configuration.
            var gridAttributeIds = _gridAttributes.Value.Select( a => a.Id ).ToList();
            Helper.LoadFilteredAttributes( items.Select( d => d.Batch ), rockContext, a => gridAttributeIds.Contains( a.Id ) );

            // Load the account summary data into memory.
            var batchIdQry = GetBatchQueryable( rockContext ).Select( b => b.Id );
            var accountSummaries = new FinancialTransactionDetailService( rockContext )
                .Queryable()
                .Where( ftd => ftd.Transaction.BatchId.HasValue
                    && batchIdQry.Contains( ftd.Transaction.BatchId.Value ) )
                .GroupBy( ftd => new
                {
                    BatchId = ftd.Transaction.BatchId.Value,
                    ftd.AccountId
                } )
                .Select( grp => new
                {
                    grp.Key.BatchId,
                    grp.Key.AccountId,
                    Amount = grp.Sum( ftd => ftd.Amount )
                } )
                .ToList()
                .GroupBy( a => a.BatchId )
                .ToDictionary( grp => grp.Key, grp => grp.ToList() );

            // FinancialAccountCache.Get() goes through about 8 methods to
            // get to the real method. Since we might be calling it over 1
            // million times in this loop, keep a local cache that cuts out
            // most of those method chain calls to improve performance 200%.
            var accountNameCache = new Dictionary<int, string>();

            // Translate the account summary data into a format that can be
            // sent to the client.
            foreach ( var item in items )
            {
                if ( accountSummaries.TryGetValue( item.Batch.Id, out var accounts ) )
                {
                    item.Accounts = accounts
                        .Select( a =>
                        {
                            if ( !accountNameCache.TryGetValue( a.AccountId, out var accountName ) )
                            {
                                accountName = FinancialAccountCache.Get( a.AccountId )?.Name ?? $"#{a.AccountId}";
                                accountNameCache.Add( a.AccountId, accountName );
                            }

                            return new AccountData
                            {
                                IdKey = IdHasher.Instance.GetHash( a.AccountId ),
                                Name = accountName,
                                Amount = a.Amount
                            };
                        } )
                        .ToList();
                }
                else
                {
                    item.Accounts = new List<AccountData>();
                }
            }

            return items;
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<FinancialBatch>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }

        /// <inheritdoc/>
        protected override GridBuilder<BatchData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<BatchData>
            {
                LavaObject = row => row.Batch
            };

            return new GridBuilder<BatchData>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.Batch.IdKey )
                .AddField( "id", a => a.Batch.Id )
                .AddTextField( "name", a => a.Batch.Name )
                .AddTextField( "note", a => a.Batch.Note )
                .AddField( "accounts", a => a.Accounts )
                .AddField( "accountSystemCode", a => a.Batch.AccountingSystemCode )
                .AddField( "controlAmount", a => a.Batch.ControlAmount )
                .AddField( "controlItemCount", a => a.Batch.ControlItemCount )
                .AddTextField( "campus", a => a.Batch.CampusId.HasValue ? CampusCache.Get( a.Batch.CampusId.Value )?.Name : null )
                .AddField( "status", a => a.Batch.Status )
                .AddDateTimeField( "startDateTime", a => a.Batch.BatchStartDateTime )
                .AddField( "remoteSettlementAmount", a => a.Batch.RemoteSettlementAmount )
                .AddField( "remoteSettlementKey", a => a.Batch.RemoteSettlementBatchKey )
                .AddTextField( "remoteSettlementUrl", a => a.Batch.RemoteSettlementBatchUrl )
                .AddField( "transactionCount", a => a.TransactionCount )
                .AddAttributeFieldsFrom( a => a.Batch, _gridAttributes.Value );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialBatchService( rockContext );
                var transactionService = new FinancialTransactionService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{FinancialBatch.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.DELETE, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${FinancialBatch.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    var transactionsToDelete = transactionService
                        .Queryable()
                        .Where( t => t.BatchId == entity.Id );

                    transactionService.DeleteRange( transactionsToDelete );

                    var changes = new History.HistoryChangeList();
                    changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Batch" );
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( FinancialBatch ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                        entity.Id,
                        changes );

                    entityService.Delete( entity );
                    rockContext.SaveChanges();
                } );

                return ActionOk();
            }
        }

        /// <summary>
        /// Opens or closes a set of batches in a single operation.
        /// </summary>
        /// <param name="request">The options that describe which batches to be opened or closed.</param>
        /// <returns>An instance of <see cref="SetBulkBatchStatusResponseBag"/> that provides the results of the operation.</returns>
        [BlockAction]
        public BlockActionResult SetBulkBatchStatus( SetBulkBatchStatusRequestBag request )
        {
            if ( request == null || request.Keys == null || !request.Keys.Any() )
            {
                return ActionBadRequest( "No batch identifiers were provided." );
            }

            var newStatus = request.Open ? BatchStatus.Open : BatchStatus.Closed;

            using ( var rockContext = new RockContext() )
            {
                string message = null;
                List<string> errors = null;
                var batchService = new FinancialBatchService( rockContext );

                var selectedBatchIds = Reflection.GetEntityIdsForEntityType( EntityTypeCache.Get<FinancialBatch>(), request.Keys, !PageCache.Layout.Site.DisablePredictableIds, rockContext )
                    .Values
                    .ToList();

                var batchesToUpdate = batchService.Queryable()
                    .Where( b => selectedBatchIds.Contains( b.Id )
                        && b.Status != newStatus )
                    .ToList();

                rockContext.WrapTransactionIf( () =>
                {
                    foreach ( var batch in batchesToUpdate )
                    {
                        var changes = new History.HistoryChangeList();
                        History.EvaluateChange( changes, "Status", batch.Status, newStatus );

                        if ( !batch.IsValidBatchStatusChange( batch.Status, newStatus, RequestContext.CurrentPerson, out message ) )
                        {
                            return false;
                        }

                        if ( batch.IsAutomated && batch.Status == BatchStatus.Pending && newStatus != BatchStatus.Pending )
                        {
                            message = $"'{batch.Name}' is an automated batch and the status can not be modified when the status is pending. The system will automatically set this batch to OPEN when all transactions have been downloaded.";
                            return false;
                        }

                        batch.Status = newStatus;

                        if ( !batch.IsValid )
                        {
                            message = $"Unable to update status for the batch '{batch.Name}'.";
                            errors = batch.ValidationResults.Select( r => r.ErrorMessage ).ToList();
                            return false;
                        }

                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            batch.Id,
                            changes,
                            false );
                    }

                    rockContext.SaveChanges();

                    return true;
                } );

                if ( message.IsNullOrWhiteSpace() )
                {
                    return ActionOk( new SetBulkBatchStatusResponseBag
                    {
                        IsSuccess = true,
                        Message = $"{batchesToUpdate.Count:N0} batches were {( newStatus == BatchStatus.Open ? "opened" : "closed" )}."
                    } );
                }
                else
                {
                    return ActionOk( new SetBulkBatchStatusResponseBag
                    {
                        IsSuccess = false,
                        Message = message,
                        Errors = errors
                    } );
                }
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The temporary data format to use when building the results for the
        /// grid.
        /// </summary>
        public class BatchData
        {
            /// <summary>
            /// Gets or sets the whole batch object from the database.
            /// </summary>
            /// <value>
            /// The whole batch object from the database.
            /// </value>
            public FinancialBatch Batch { get; set; }

            /// <summary>
            /// Gets or sets the account data for this batch.
            /// </summary>
            /// <value>
            /// The account data for this batch.
            /// </value>
            public IEnumerable<AccountData> Accounts { get; set; }

            /// <summary>
            /// Gets or sets the number of transactions in this batch.
            /// </summary>
            /// <value>
            /// The number of transactions in this batch.
            /// </value>
            public int TransactionCount { get; set; }
        }

        /// <summary>
        /// The data about a single account's totals in a batch.
        /// </summary>
        public class AccountData
        {
            /// <summary>
            /// Gets or sets the identifier of the account.
            /// </summary>
            /// <value>
            /// The identifier of the account.
            /// </value>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the name of the account.
            /// </summary>
            /// <value>
            /// The name of the account.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the amount for this account.
            /// </summary>
            /// <value>
            /// The amount for this account.
            /// </value>
            public decimal Amount { get; set; }
        }

        #endregion
    }
}
