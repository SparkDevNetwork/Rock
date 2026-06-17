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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.TransactionList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using static Rock.Blocks.Finance.TransactionList;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial transactions which can be filtered by date, account, transaction type, etc.
    /// </summary>

    [DisplayName( "Transaction List" )]
    [Category( "Finance" )]
    [Description( "Builds a list of all financial transactions which can be filtered by date, account, transaction type, etc." )]
    [IconCssClass( "ti ti-credit-card" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.DetailPage )]

    [TextField( "Title",
        Description = "Title to display above the grid. Leave blank to hide.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.Title )]

    [BooleanField( "Show Only Active Accounts on Filter",
        Description = "If account filter is displayed, only list active accounts",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.ActiveAccountsOnlyFilter )]

    [BooleanField( "Show Images Toggle",
        Description = "Determines whether the 'Show Images' option is available in the grid options menu.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.ShowImagesToggle )]

    [IntegerField( "Image Height",
        Description = "If the Show Images option is selected, the image height",
        IsRequired = false,
        DefaultIntegerValue = 200,
        Order = 4,
        Key = AttributeKey.ImageHeight )]

    [DefinedValueField( "Transaction Types",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        Description = "Optional list of transaction types to limit the list to (if none are selected all types will be included).",
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = "",
        Order = 5,
        Key = AttributeKey.TransactionTypes )]

    [CustomDropdownListField( "Default Transaction View",
        Description = "Select whether you want to initially see Transactions or Accounts",
        ListSource = "Transactions,Accounts",
        IsRequired = false,
        DefaultValue = "Transactions",
        Order = 6,
        Key = AttributeKey.DefaultTransactionView )]

    [LinkedPage( "Batch Page",
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.BatchPage )]

    [BooleanField( "Show Foreign Key",
        Description = "Should the transaction foreign key column be displayed?",
        DefaultBooleanValue = false,
        Order = 8,
        Key = AttributeKey.ShowForeignKey )]

    [BooleanField( "Show Account Summary",
        Description = "Should the account summary be displayed at the bottom of the list?",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.ShowAccountSummary )]

    [AccountsField( "Accounts",
        Description = "Limit the results to transactions that match the selected accounts.",
        IsRequired = false,
        DefaultValue = "",
        Order = 10,
        Key = AttributeKey.Accounts )]

    [BooleanField( "Show Future Transactions",
        Description = "Should future transactions (transactions scheduled to be charged) be shown in this list?",
        DefaultBooleanValue = false,
        Order = 11,
        Key = AttributeKey.ShowFutureTransactions )]

    [DefinedValueField( "Source Types",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        Description = "Optional list of financial source types to limit the list to (if none are selected all types will be included).",
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = "",
        Order = 12,
        Key = AttributeKey.SourceTypes )]

    [BooleanField( "Enable Foreign Currency",
        Description = "Shows the transaction's currency code field if enabled.",
        DefaultBooleanValue = false,
        Order = 13,
        Key = AttributeKey.EnableForeignCurrency )]

    [BooleanField( "Show Days Since Last Transaction",
        Description = "Show the number of days between the transaction and the transaction listed next to the transaction",
        DefaultBooleanValue = false,
        Order = 14,
        Key = AttributeKey.ShowDaysSinceLastTransaction )]

    [BooleanField( "Hide Transactions in Pending Batches",
        Description = "When enabled, transactions in a batch whose status is 'Pending' will be filtered out from the list.",
        DefaultBooleanValue = false,
        Order = 15,
        Key = AttributeKey.HideTransactionsInPendingBatches )]

    #endregion Block Attributes

    [SecurityAction( SecurityActionKey.FilterByPerson, "The roles and/or users that can filter transactions by person." )]

    [Rock.SystemGuid.EntityTypeGuid( "36AAA558-649E-49AF-8372-5ED6BD5C9657" )]
    [Rock.SystemGuid.BlockTypeGuid( "D129A0C7-4A7F-42BC-8E0C-428C4A4122D2" )]
    //[Rock.SystemGuid.BlockTypeGuid( "E04320BC-67C3-452D-9EF6-D74D8C177154" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware]
    public class TransactionList : RockListBlockType<TransactionListRow>
    {
        #region Keys

        /// <summary>
        /// Block Attribute Keys.
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string Title = "Title";
            public const string ActiveAccountsOnlyFilter = "ActiveAccountsOnlyFilter";
            public const string ShowImagesToggle = "ShowImagesToggle";
            public const string ImageHeight = "ImageHeight";
            public const string TransactionTypes = "TransactionTypes";
            public const string DefaultTransactionView = "DefaultTransactionView";
            public const string BatchPage = "BatchPage";
            public const string ShowForeignKey = "ShowForeignKey";
            public const string ShowAccountSummary = "ShowAccountSummary";
            public const string Accounts = "Accounts";
            public const string ShowFutureTransactions = "ShowFutureTransactions";
            public const string SourceTypes = "SourceTypes";
            public const string EnableForeignCurrency = "EnableForeignCurrency";
            public const string ShowDaysSinceLastTransaction = "ShowDaysSinceLastTransaction";
            public const string HideTransactionsInPendingBatches = "HideTransactionsInPendingBatches";
        }

        /// <summary>
        /// Custom Security Action Keys.
        /// </summary>
        private static class SecurityActionKey
        {
            public const string FilterByPerson = "FilterByPerson";
        }

        /// <summary>
        /// Navigation URL Keys.
        /// </summary>
        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string BatchPage = "BatchPage";
        }

        /// <summary>
        /// Page Parameter Keys.
        /// </summary>
        private static class PageParameterKey
        {
            public const string TransactionId = "TransactionId";
            public const string BatchId = "BatchId";
            public const string PersonId = "PersonId";
        }

        /// <summary>
        /// Person Preference Keys.
        /// </summary>
        private static class PreferenceKey
        {
            public const string ViewMode = "view-mode";
            public const string ShowImages = "show-images";

            public const string FilterDateRangeLower = "filter-date-range-lower";
            public const string FilterDateRangeUpper = "filter-date-range-upper";
            public const string FilterAmountRangeFrom = "filter-amount-range-from";
            public const string FilterAmountRangeTo = "filter-amount-range-to";
            public const string FilterCurrencyType = "filter-currency-type";
            public const string FilterCreditCardType = "filter-credit-card-type";
            public const string FilterTransactionCode = "filter-transaction-code";
            public const string FilterForeignKey = "filter-foreign-key";
            public const string FilterAccount = "filter-account";
            public const string FilterTransactionType = "filter-transaction-type";
            public const string FilterSourceType = "filter-source-type";
            public const string FilterCampusOfBatch = "filter-campus-of-batch";
            public const string FilterCampusOfAccount = "filter-campus-of-account";
            public const string FilterPerson = "filter-person";
        }

        /// <summary>
        /// The transaction list view modes.
        /// </summary>
        private static class ViewMode
        {
            public const string Transactions = "Transactions";
            public const string Accounts = "Accounts";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The resolved context entity, if any. Indicates whether the block has been
        /// scoped to a single person, batch, scheduled transaction, or registration.
        /// </summary>
        private Person _person;
        private FinancialBatch _batch;
        private FinancialScheduledTransaction _scheduledTransaction;
        private Registration _registration;

        /// <summary>
        /// Tracks whether <see cref="InitializeContextEntities"/> has run for this request.
        /// </summary>
        private bool _contextInitialized;

        /// <summary>
        /// The FinancialTransaction attributes configured to show on the grid (Transactions view mode).
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _transactionGridAttributes = new Lazy<List<AttributeCache>>( () => BuildGridAttributes( false ) );

        /// <summary>
        /// The FinancialTransactionDetail attributes configured to show on the grid (Accounts view mode).
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _accountGridAttributes = new Lazy<List<AttributeCache>>( () => BuildGridAttributes( true ) );

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current user can edit transactions in this block.
        /// </summary>
        private bool CanEdit => BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

        /// <summary>
        /// Gets a value indicating whether the batch context (when present) is open and editable
        /// (i.e. not closed and not automated).
        /// </summary>
        private bool IsBatchEditable => _batch != null && _batch.Status != BatchStatus.Closed && !_batch.IsAutomated;

        /// <summary>
        /// Gets the resolved current view mode, accounting for the saved person preference
        /// (including the legacy "Transaction Details" value) and the block's default.
        /// </summary>
        private string CurrentViewMode
        {
            get
            {
                var preference = GetBlockPersonPreferences().GetValue( PreferenceKey.ViewMode );

                if ( preference == ViewMode.Transactions || preference == ViewMode.Accounts )
                {
                    return preference;
                }

                /*
                    6/16/2026 - CH

                    The WebForms block persisted this preference as "Transaction Details". When converting
                    we renamed that mode to "Accounts", so map the legacy value forward instead of
                    stranding users who already had the old preference saved.

                    Reason: Backward compatibility for the renamed view mode preference.
                */
                if ( preference == "Transaction Details" )
                {
                    return ViewMode.Accounts;
                }

                var defaultViewMode = GetAttributeValue( AttributeKey.DefaultTransactionView );

                return ( defaultViewMode == ViewMode.Accounts || defaultViewMode == "Transaction Details" )
                    ? ViewMode.Accounts
                    : ViewMode.Transactions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the "Show Images" option is offered in the grid menu.
        /// </summary>
        private bool IsImagesToggleVisible => GetAttributeValue( AttributeKey.ShowImagesToggle ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether the image column should currently be shown
        /// (the user's preference, only honored when the toggle is available).
        /// </summary>
        private bool ShowImages => IsImagesToggleVisible && GetBlockPersonPreferences().GetValue( PreferenceKey.ShowImages ).AsBoolean();

        /// <summary>
        /// Gets the grid attributes for the current view mode: FinancialTransactionDetail attributes
        /// in Accounts mode, otherwise FinancialTransaction attributes.
        /// </summary>
        private Lazy<List<AttributeCache>> GridAttributes => CurrentViewMode == ViewMode.Accounts
            ? _accountGridAttributes
            : _transactionGridAttributes;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            InitializeContextEntities();

            var box = new ListBlockBox<TransactionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = GetIsDeleteEnabled();
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
        private TransactionListOptionsBag GetBoxOptions()
        {
            var isFilterByPersonAuthorized = BlockCache.IsAuthorized( SecurityActionKey.FilterByPerson, RequestContext.CurrentPerson );

            // Filters are only available when the list is not already scoped to a
            // specific batch, scheduled transaction, or registration.
            var areFiltersVisible = _batch == null && _scheduledTransaction == null && _registration == null;

            var currencyInfo = new RockCurrencyCodeInfo();

            var options = new TransactionListOptionsBag
            {
                Title = GetAttributeValue( AttributeKey.Title ),
                ViewMode = CurrentViewMode,
                IsImagesToggleVisible = IsImagesToggleVisible,
                ShowImages = ShowImages,
                ImageHeight = GetAttributeValue( AttributeKey.ImageHeight ).AsIntegerOrNull() ?? 200,
                ShowAccountSummary = GetAttributeValue( AttributeKey.ShowAccountSummary ).AsBoolean(),
                ShowForeignKeyColumn = GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean(),
                IsForeignCurrencyEnabled = GetAttributeValue( AttributeKey.EnableForeignCurrency ).AsBoolean(),
                ShowDaysSinceLastTransaction = GetAttributeValue( AttributeKey.ShowDaysSinceLastTransaction ).AsBoolean(),
                IsPersonContext = _person != null,
                IsBatchContext = _batch != null,
                IsScheduledTransactionContext = _scheduledTransaction != null,
                IsRegistrationContext = _registration != null,
                AreFiltersVisible = areFiltersVisible,
                IsPersonFilterVisible = areFiltersVisible && _person == null && isFilterByPersonAuthorized,
                IsReassignVisible = _person != null && CanEdit,
                IsMoveToBatchVisible = _batch != null && CanEdit && IsBatchEditable,
                ShowClosedBatchWarning = _batch != null && _batch.Status == BatchStatus.Closed,
                CurrencyInfo = new CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                }
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid. The Add button is only
        /// available in a batch context when the batch is open, not automated, the user has edit
        /// permission, and a detail page has been configured.
        /// </summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return _batch != null
                && CanEdit
                && IsBatchEditable
                && GetAttributeValue( AttributeKey.DetailPage ).IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines if delete should be enabled in the grid. Deletes are only available in a
        /// batch context when the batch is open, not automated, and the user has edit permission.
        /// </summary>
        /// <returns>A boolean value that indicates if delete should be enabled.</returns>
        private bool GetIsDeleteEnabled()
        {
            return _batch != null && CanEdit && IsBatchEditable;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var detailPageParams = new Dictionary<string, string>
            {
                [PageParameterKey.TransactionId] = "((Key))"
            };

            // Carry the context identifier through to the detail page so it can return correctly.
            if ( _batch != null )
            {
                detailPageParams[PageParameterKey.BatchId] = _batch.Id.ToString();
            }
            else if ( _person != null )
            {
                detailPageParams[PageParameterKey.PersonId] = _person.Id.ToString();
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, detailPageParams ),
                [NavigationUrlKey.BatchPage] = this.GetLinkedPageUrl( AttributeKey.BatchPage, PageParameterKey.BatchId, "((Key))" )
            };
        }

        /// <summary>
        /// Resolves the context entity (if any) and stores it in the appropriate field. The block
        /// is configurable, so a single context type is configured per block instance.
        /// </summary>
        private void InitializeContextEntities()
        {
            if ( _contextInitialized )
            {
                return;
            }

            var contextEntity = GetContextEntity();

            if ( contextEntity is Person person )
            {
                _person = person;
            }
            else if ( contextEntity is FinancialBatch batch )
            {
                _batch = batch;
            }
            else if ( contextEntity is FinancialScheduledTransaction scheduledTransaction )
            {
                _scheduledTransaction = scheduledTransaction;
            }
            else if ( contextEntity is Registration registration )
            {
                _registration = registration;
            }

            _contextInitialized = true;
        }

        /// <inheritdoc/>
        protected override IQueryable<TransactionListRow> GetListQueryable( RockContext rockContext )
        {
            InitializeContextEntities();

            // The two view modes build rows from different entities: one row per transaction,
            // or one row per transaction detail (account line).
            return CurrentViewMode == ViewMode.Accounts
                ? GetAccountsModeQueryable( rockContext )
                : GetTransactionsModeQueryable( rockContext );
        }

        /// <summary>
        /// Builds the rows for "Transactions" view mode (one row per <see cref="FinancialTransaction"/>).
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A queryable of transaction rows.</returns>
        private IQueryable<TransactionListRow> GetTransactionsModeQueryable( RockContext rockContext )
        {
            return new FinancialTransactionService( rockContext ).Queryable().AsNoTracking()
                .Select( t => new TransactionListRow
                {
                    // Referenced (not constructed) so it is materialized for GridAttributeLoader.
                    // Only its scalars/attributes are read later — never its navigations.
                    Transaction = t,
                    Id = t.Id,
                    TransactionId = t.Id,
                    Person = t.AuthorizedPersonAlias.Person,
                    TransactionDateTime = t.TransactionDateTime,
                    FutureProcessingDateTime = t.FutureProcessingDateTime,
                    TotalAmount = t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ),
                    CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = t.FinancialPaymentDetail.CreditCardTypeValueId,
                    ForeignCurrencyCodeValueId = t.ForeignCurrencyCodeValueId,
                    TransactionCode = t.TransactionCode,
                    ForeignKey = t.ForeignKey,
                    BatchId = t.BatchId,
                    Summary = t.Summary,
                    Status = t.Status,
                    SettledDate = t.SettledDate,
                    SettledGroupId = t.SettledGroupId,
                    Accounts = t.TransactionDetails
                        .OrderBy( d => d.Account.Order )
                        .Select( d => new AccountAmount { Name = d.Account.Name, Amount = d.Amount } )
                        .ToList(),
                    ImageBinaryFileIds = t.Images
                        .OrderBy( i => i.Order )
                        .Select( i => i.BinaryFileId )
                        .ToList()
                } );
        }

        /// <summary>
        /// Builds the rows for "Accounts" view mode (one row per <see cref="FinancialTransactionDetail"/>).
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A queryable of transaction detail rows.</returns>
        private IQueryable<TransactionListRow> GetAccountsModeQueryable( RockContext rockContext )
        {
            return new FinancialTransactionDetailService( rockContext ).Queryable().AsNoTracking()
                .Select( d => new TransactionListRow
                {
                    // Referenced (not constructed) so it is materialized for GridAttributeLoader.
                    // Only its scalars/attributes are read later — never its navigations.
                    TransactionDetail = d,
                    Id = d.Id,
                    TransactionId = d.TransactionId,
                    Person = d.Transaction.AuthorizedPersonAlias.Person,
                    TransactionDateTime = d.Transaction.TransactionDateTime,
                    FutureProcessingDateTime = d.Transaction.FutureProcessingDateTime,
                    TotalAmount = d.Amount,
                    CurrencyTypeValueId = d.Transaction.FinancialPaymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = d.Transaction.FinancialPaymentDetail.CreditCardTypeValueId,
                    ForeignCurrencyCodeValueId = d.Transaction.ForeignCurrencyCodeValueId,
                    TransactionCode = d.Transaction.TransactionCode,
                    ForeignKey = d.Transaction.ForeignKey,
                    BatchId = d.Transaction.BatchId,
                    Summary = d.Transaction.Summary,
                    Status = d.Transaction.Status,
                    SettledDate = d.Transaction.SettledDate,
                    SettledGroupId = d.Transaction.SettledGroupId,
                    Accounts = d.Transaction.TransactionDetails
                        .Where( x => x.Id == d.Id )
                        .Select( x => new AccountAmount { Name = x.Account.Name, Amount = x.Amount } )
                        .ToList(),
                    ImageBinaryFileIds = d.Transaction.Images
                        .OrderBy( i => i.Order )
                        .Select( i => i.BinaryFileId )
                        .ToList()
                } );
        }

        /// <inheritdoc/>
        protected override IQueryable<TransactionListRow> GetOrderedListQueryable( IQueryable<TransactionListRow> queryable, RockContext rockContext )
        {
            // In a batch context, default to the natural (Id) order; otherwise show future/pending
            // charges first, then the most recent transactions, with newest Id breaking ties.
            if ( _batch != null )
            {
                return queryable.OrderBy( r => r.Id );
            }

            return queryable
                .OrderByDescending( r => r.FutureProcessingDateTime )
                .ThenByDescending( r => r.TransactionDateTime )
                .ThenByDescending( r => r.Id );
        }

        /// <inheritdoc/>
        protected override List<TransactionListRow> GetListItems( IQueryable<TransactionListRow> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            // Load the grid attribute values against the entity for the current view mode.
            if ( CurrentViewMode == ViewMode.Accounts )
            {
                GridAttributeLoader.LoadFor( items, i => i.TransactionDetail, GridAttributes.Value, RockContext );
            }
            else
            {
                GridAttributeLoader.LoadFor( items, i => i.Transaction, GridAttributes.Value, RockContext );
            }

            BuildDaysSinceLastTransaction( items );

            return items;
        }

        /// <summary>
        /// Calculates the number of days between each transaction and the chronologically adjacent
        /// transaction in the ordered result set. Computed in memory so the query does not pay the
        /// cost of a windowing calculation. The adjacent transaction may be the next or previous row
        /// depending on the active sort.
        /// </summary>
        /// <param name="items">The materialized rows, in their displayed order.</param>
        private void BuildDaysSinceLastTransaction( List<TransactionListRow> items )
        {
            if ( !GetAttributeValue( AttributeKey.ShowDaysSinceLastTransaction ).AsBoolean() )
            {
                return;
            }

            for ( var index = 0; index < items.Count; index++ )
            {
                var currentDate = items[index].TransactionDateTime;
                if ( !currentDate.HasValue )
                {
                    continue;
                }

                var nextRow = index + 1 < items.Count ? items[index + 1] : null;
                var prevRow = index - 1 >= 0 ? items[index - 1] : null;

                var nextDate = nextRow?.TransactionDateTime;
                var prevDate = prevRow?.TransactionDateTime;

                if ( nextDate.HasValue && nextDate.Value < currentDate.Value && items[index].Id != nextRow.Id )
                {
                    items[index].DaysSinceLastTransaction = ( int ) Math.Round( ( currentDate.Value - nextDate.Value ).TotalDays, 0 );
                }
                else if ( prevDate.HasValue && prevDate.Value < currentDate.Value && items[index].Id != prevRow.Id )
                {
                    items[index].DaysSinceLastTransaction = ( int ) Math.Round( ( currentDate.Value - prevDate.Value ).TotalDays, 0 );
                }
            }
        }

        /// <inheritdoc/>
        protected override GridBuilder<TransactionListRow> GetGridBuilder()
        {
            var gridBuilder = new GridBuilder<TransactionListRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => IdHasher.Instance.GetHash( a.Id ) )
                .AddTextField( "transactionIdKey", a => IdHasher.Instance.GetHash( a.TransactionId ) )
                .AddPersonField( "person", a => a.Person )
                .AddDateTimeField( "transactionDateTime", a => a.TransactionDateTime )
                .AddField( "daysSinceLastTransaction", a => a.DaysSinceLastTransaction )
                .AddField( "totalAmount", a => a.TotalAmount )
                .AddTextField( "currencyType", a => GetCurrencyTypeText( a ) )
                .AddTextField( "foreignCurrency", a => GetForeignCurrencyText( a ) )
                .AddTextField( "transactionCode", a => a.TransactionCode )
                .AddTextField( "foreignKey", a => a.ForeignKey )
                .AddField( "batchId", a => a.BatchId )
                .AddTextField( "batchIdKey", a => a.BatchId.HasValue ? IdHasher.Instance.GetHash( a.BatchId.Value ) : null )
                .AddField( "accounts", a => a.Accounts )
                .AddTextField( "summary", a => GetSummaryText( a ) )
                .AddField( "image", a => GetTransactionImageUrl( a ) )
                .AddTextField( "status", a => a.Status )
                .AddDateTimeField( "settledDate", a => a.SettledDate )
                .AddTextField( "processorBatchId", a => a.SettledGroupId );

            // Attribute columns target the entity that matches the current view mode.
            if ( CurrentViewMode == ViewMode.Accounts )
            {
                gridBuilder.AddAttributeFieldsFrom( a => a.TransactionDetail, GridAttributes.Value );
            }
            else
            {
                gridBuilder.AddAttributeFieldsFrom( a => a.Transaction, GridAttributes.Value );
            }

            return gridBuilder;
        }

        /// <summary>
        /// Gets the display text for the currency type column (currency type, optionally with the credit card type).
        /// </summary>
        /// <param name="row">The transaction row.</param>
        /// <returns>The currency type display text.</returns>
        private string GetCurrencyTypeText( TransactionListRow row )
        {
            if ( !row.CurrencyTypeValueId.HasValue )
            {
                return string.Empty;
            }

            var currencyType = DefinedValueCache.GetValue( row.CurrencyTypeValueId );

            if ( row.CreditCardTypeValueId.HasValue )
            {
                var creditCardType = DefinedValueCache.GetValue( row.CreditCardTypeValueId );
                return $"{currencyType} - {creditCardType}";
            }

            return currencyType;
        }

        /// <summary>
        /// Gets the display text for the foreign currency column (currency code and symbol).
        /// </summary>
        /// <param name="row">The transaction row.</param>
        /// <returns>The foreign currency display text.</returns>
        private string GetForeignCurrencyText( TransactionListRow row )
        {
            if ( !row.ForeignCurrencyCodeValueId.HasValue )
            {
                return string.Empty;
            }

            var currencyCode = DefinedValueCache.Get( row.ForeignCurrencyCodeValueId.Value );
            if ( currencyCode == null )
            {
                return string.Empty;
            }

            return $"{currencyCode.Value} {currencyCode.GetAttributeValue( "Symbol" )}";
        }

        /// <summary>
        /// Gets the summary (Comments) text for the row, prefixed when the transaction is pending a future charge.
        /// </summary>
        /// <param name="row">The transaction row.</param>
        /// <returns>The summary text.</returns>
        private string GetSummaryText( TransactionListRow row )
        {
            return row.FutureProcessingDateTime.HasValue
                ? $"[charge pending] {row.Summary}"
                : row.Summary;
        }

        /// <summary>
        /// Gets the URL of the first image for the transaction, or <c>null</c> when images are not
        /// being shown or the transaction has no image.
        /// </summary>
        /// <param name="row">The transaction row.</param>
        /// <returns>The image URL or <c>null</c>.</returns>
        private string GetTransactionImageUrl( TransactionListRow row )
        {
            if ( !ShowImages )
            {
                return null;
            }

            var firstImageBinaryFileId = row.ImageBinaryFileIds?.FirstOrDefault();
            if ( !firstImageBinaryFileId.HasValue || firstImageBinaryFileId.Value == 0 )
            {
                return null;
            }

            var options = new GetImageUrlOptions
            {
                Height = GetAttributeValue( AttributeKey.ImageHeight ).AsIntegerOrNull() ?? 200
            };

            return FileUrlHelper.GetImageUrl( firstImageBinaryFileId.Value, options );
        }

        /// <summary>
        /// Builds the list of grid attributes for the given view mode's entity type.
        /// </summary>
        /// <param name="isAccountsView"><c>true</c> for FinancialTransactionDetail attributes; otherwise FinancialTransaction.</param>
        /// <returns>The ordered grid attributes.</returns>
        private static List<AttributeCache> BuildGridAttributes( bool isAccountsView )
        {
            var entityTypeId = isAccountsView
                ? EntityTypeCache.Get<FinancialTransactionDetail>( false )?.Id
                : EntityTypeCache.Get<FinancialTransaction>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the selected view mode ("Transactions" or "Accounts") as a block person preference.
        /// The grid reloads its data afterward, which re-reads this preference to build the rows.
        /// </summary>
        /// <param name="viewMode">The view mode to save.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult SetViewMode( string viewMode )
        {
            if ( viewMode != ViewMode.Transactions && viewMode != ViewMode.Accounts )
            {
                return ActionBadRequest( "Invalid view mode." );
            }

            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( PreferenceKey.ViewMode, viewMode );
            preferences.Save();

            var definition = GetGridBuilder().BuildDefinition();

            return ActionOk( definition.AttributeFields );
        }

        // TODO Step 5: Delete, Reassign, and MoveToBatch block actions (with their request bags).

        #endregion Block Actions

        #region Supported Classes

        /// <summary>
        /// A single row of data for the transaction list grid. Rows are built from either
        /// transactions or transaction details, but the grid is always built on this type.
        /// </summary>
        public class TransactionListRow
        {
            /// <summary>
            /// The financial transaction, carried only so its attributes can be loaded
            /// (Transactions view mode). Do not read its navigation properties in memory.
            /// </summary>
            public FinancialTransaction Transaction { get; set; }

            /// <summary>
            /// The financial transaction detail, carried only so its attributes can be loaded
            /// (Accounts view mode). Do not read its navigation properties in memory.
            /// </summary>
            public FinancialTransactionDetail TransactionDetail { get; set; }

            /// <summary>
            /// The row's own entity id: the transaction id in Transactions mode, or the
            /// transaction detail id in Accounts mode. Used as the grid key.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The number of days between this transaction and the chronologically adjacent
            /// transaction, when that column is enabled.
            /// </summary>
            public int? DaysSinceLastTransaction { get; set; }

            /// <summary>
            /// The parent transaction's id (same as <see cref="Id"/> in Transactions mode).
            /// Used for navigation and entity-level actions.
            /// </summary>
            public int TransactionId { get; set; }

            public Person Person { get; set; }

            public DateTime? TransactionDateTime { get; set; }

            public DateTime? FutureProcessingDateTime { get; set; }

            public decimal? TotalAmount { get; set; }

            public int? CurrencyTypeValueId { get; set; }

            public int? CreditCardTypeValueId { get; set; }

            public int? ForeignCurrencyCodeValueId { get; set; }

            public string TransactionCode { get; set; }

            public string ForeignKey { get; set; }

            public int? BatchId { get; set; }

            public string Summary { get; set; }

            public string Status { get; set; }

            public DateTime? SettledDate { get; set; }

            public string SettledGroupId { get; set; }

            /// <summary>
            /// The account name/amount entries shown in the Accounts column. One entry per detail
            /// in Transactions mode; a single entry in Accounts mode.
            /// </summary>
            public List<AccountAmount> Accounts { get; set; }

            /// <summary>
            /// The binary file ids of the transaction's images, ordered, for the image column.
            /// </summary>
            public List<int> ImageBinaryFileIds { get; set; }
        }

        /// <summary>
        /// A single account name and amount shown in the Accounts column.
        /// </summary>
        public class AccountAmount
        {
            public string Name { get; set; }

            public decimal Amount { get; set; }
        }

        #endregion Supported Classes
    }
}
