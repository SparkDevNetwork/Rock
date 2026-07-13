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

using RestSharp.Extensions;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.TransactionList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
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

    [SecurityAction( SecurityActionKey.FilterByPerson, "The roles and/or users that can filter transactions by person." )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page used to view or edit transaction details.",
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
        Description = "The page used to view batch details.",
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

    [Rock.SystemGuid.EntityTypeGuid( "36AAA558-649E-49AF-8372-5ED6BD5C9657" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "D129A0C7-4A7F-42BC-8E0C-428C4A4122D2" )]
    [Rock.SystemGuid.BlockTypeGuid( "E04320BC-67C3-452D-9EF6-D74D8C177154" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware]
    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
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
        /// Custom block security action keys.
        /// </summary>
        private static class SecurityActionKey
        {
            /// <summary>
            /// Controls which roles/users may filter the transaction list by a specific person.
            /// </summary>
            public const string FilterByPerson = "FilterByPerson";
        }

        /// <summary>
        /// Person Preference Keys.
        /// </summary>
        private static class PreferenceKey
        {
            public const string ViewMode = "TransactionViewMode";

            public const string ShowImages = "show-images";

            public const string FilterDateRange = "filter-date-range";
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

            // The WebForms block persisted the Accounts view under the legacy value
            // "Transaction Details". Read once in CurrentViewMode for backward compatibility so a
            // view mode chosen before the conversion is honored; it is never written.
            public const string LegacyAccounts = "Transaction Details";
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

        /// <summary>
        /// Cached person preferences for this block so that every property access does not
        /// call <see cref="RockBlockType.GetBlockPersonPreferences"/> separately.
        /// </summary>
        private PersonPreferenceCollection _personPreferences;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the default transaction date range applied when the user has not chosen one: the
        /// last 6 months. Keeps the result set bounded so the grid does not load the full history.
        /// </summary>
        private static SlidingDateRangeBag DefaultDateRange => new SlidingDateRangeBag
        {
            RangeType = SlidingDateRangeType.Last,
            TimeUnit = TimeUnitType.Month,
            TimeValue = 6
        };

        /// <summary>
        /// Gets a value indicating whether the user-facing filters are available. Filters are only
        /// shown (and applied) when the list is not already scoped to a specific batch, scheduled
        /// transaction, or registration, since those contexts already bound the result set.
        /// </summary>
        private bool AreFiltersVisible => _batch == null && _scheduledTransaction == null && _registration == null;

        /// <summary>
        /// Gets a value indicating whether the current user can edit transactions in this block.
        /// A user may be granted edit either through the block itself or, when scoped to a batch,
        /// through the <see cref="FinancialBatch"/> entity's security. Finance roles such as
        /// "RSR - Finance Administration" receive their transaction-management rights via the batch
        /// entity's authorization rather than the block, so the batch grant must be honored for the
        /// Add, Delete, and Move actions to be available to them.
        /// </summary>
        private bool CanEdit
        {
            get
            {
                InitializeContextEntities();

                return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                    || ( _batch != null && _batch.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user is authorized to filter the list by a
        /// specific person (the "FilterByPerson" custom security action).
        /// </summary>
        private bool CanFilterByPerson => BlockCache.IsAuthorized( SecurityActionKey.FilterByPerson, RequestContext.CurrentPerson );

        /// <summary>
        /// Gets a value indicating whether the batch context (when present) is open and editable
        /// (i.e. not closed and not automated).
        /// </summary>
        private bool IsBatchEditable => _batch != null && _batch.Status != BatchStatus.Closed && !_batch.IsAutomated;

        /// <summary>
        /// Gets the cached block person preferences, lazily loaded on first access.
        /// </summary>
        private PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        /// <summary>
        /// Gets the resolved current view mode, accounting for the saved person preference
        /// (including the legacy "Transaction Details" value) and the block's default.
        /// </summary>
        private string CurrentViewMode
        {
            get
            {
                var preference = PersonPreferences.GetValue( PreferenceKey.ViewMode );

                /*
                    6/23/2026 - CH

                    The WebForms block persisted the Accounts view as "Transaction Details" under
                    this same "TransactionViewMode" preference key. Translate that legacy value
                    forward so a view mode chosen before the conversion is honored. Once the user
                    toggles the view here, SetViewMode overwrites it with the new "Accounts" value
                    and this no longer applies.

                    Reason: Backward compatibility for the renamed view mode preference value.
                */
                if ( preference == ViewMode.LegacyAccounts )
                {
                    preference = ViewMode.Accounts;
                }

                if ( preference == ViewMode.Transactions || preference == ViewMode.Accounts )
                {
                    return preference;
                }

                var defaultViewMode = GetAttributeValue( AttributeKey.DefaultTransactionView );

                return ( defaultViewMode == ViewMode.Accounts )
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
        private bool ShowImages => IsImagesToggleVisible && PersonPreferences.GetValue( PreferenceKey.ShowImages ).AsBoolean();

        /// <summary>
        /// Gets the grid attributes for the current view mode: FinancialTransactionDetail attributes
        /// in Accounts mode, otherwise FinancialTransaction attributes.
        /// </summary>
        private Lazy<List<AttributeCache>> GridAttributes => CurrentViewMode == ViewMode.Accounts
            ? _accountGridAttributes
            : _transactionGridAttributes;

        // Filter properties — each reads from the current view mode's prefixed preference key.

        /// <summary>
        /// Returns the value of a filter preference for the current view mode.
        /// Filter preferences are stored with a <c>"{ViewMode}-"</c> prefix so each
        /// view mode maintains its own independent filter state.
        /// </summary>
        /// <param name="key">The base preference key from <see cref="PreferenceKey"/>.</param>
        private string FilterPreference( string key ) => PersonPreferences.GetValue( $"{CurrentViewMode}-{key}" );

        /// <summary>
        /// Gets the resolved transaction date range from the sliding date range preference for the
        /// current view mode, falling back to <see cref="DefaultDateRange"/> (the last 6 months)
        /// when no valid range has been selected. This filter is required so that the grid payload
        /// stays bounded rather than attempting to load the entire transaction history.
        /// </summary>
        private DateRange FilterDateRange => FilterPreference( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull()
            .Validate( DefaultDateRange )
            .ActualDateRange;

        /// <summary>Gets the minimum transaction amount filter.</summary>
        private decimal? FilterAmountRangeFrom => FilterPreference( PreferenceKey.FilterAmountRangeFrom ).AsDecimalOrNull();

        /// <summary>Gets the maximum transaction amount filter.</summary>
        private decimal? FilterAmountRangeTo => FilterPreference( PreferenceKey.FilterAmountRangeTo ).AsDecimalOrNull();

        /// <summary>Gets the currency type defined value GUID filter.</summary>
        private Guid? FilterCurrencyType => FilterPreference( PreferenceKey.FilterCurrencyType ).FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>Gets the credit card type defined value GUID filter.</summary>
        private Guid? FilterCreditCardType => FilterPreference( PreferenceKey.FilterCreditCardType ).FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>Gets the transaction source type defined value GUID filter.</summary>
        private Guid? FilterSourceType => FilterPreference( PreferenceKey.FilterSourceType ).FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>Gets the transaction type defined value GUID filter.</summary>
        private Guid? FilterTransactionType => FilterPreference( PreferenceKey.FilterTransactionType ).FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>Gets the transaction code text filter.</summary>
        private string FilterTransactionCode => FilterPreference( PreferenceKey.FilterTransactionCode );

        /// <summary>Gets the foreign key text filter.</summary>
        private string FilterForeignKey => FilterPreference( PreferenceKey.FilterForeignKey );

        /// <summary>Gets the account GUID filter (AccountPicker stores the account's Guid as the ListItemBag value).</summary>
        private string FilterAccount => FilterPreference( PreferenceKey.FilterAccount ).FromJsonOrNull<ListItemBag>()?.Value;

        /// <summary>Gets the batch campus GUID filter (CampusPicker stores the campus Guid as the ListItemBag value).</summary>
        private string FilterCampusOfBatch => FilterPreference( PreferenceKey.FilterCampusOfBatch ).FromJsonOrNull<ListItemBag>()?.Value;

        /// <summary>Gets the account campus GUID filter (CampusPicker stores the campus Guid as the ListItemBag value).</summary>
        private string FilterCampusOfAccount => FilterPreference( PreferenceKey.FilterCampusOfAccount ).FromJsonOrNull<ListItemBag>()?.Value;

        /// <summary>Gets the person primary-alias GUID filter (PersonPicker stores primaryAliasGuid as the ListItemBag value).</summary>
        private string FilterPerson => FilterPreference( PreferenceKey.FilterPerson ).FromJsonOrNull<ListItemBag>()?.Value;

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
            // Filters are only available when the list is not already scoped to a
            // specific batch, scheduled transaction, or registration.
            var areFiltersVisible = AreFiltersVisible;

            var currencyInfo = new RockCurrencyCodeInfo();

            var options = new TransactionListOptionsBag
            {
                TransactionAttributeOptions = GetAttributeOptions( false ),
                TransactionDetailAttributeOptions = GetAttributeOptions( true ),
                Title = GetAttributeValue( AttributeKey.Title ),
                ViewMode = CurrentViewMode,
                IsImagesToggleVisible = IsImagesToggleVisible,
                ShowImages = ShowImages,
                ImageHeight = GetAttributeValue( AttributeKey.ImageHeight ).AsIntegerOrNull() ?? 200,
                ShowAccountSummary = GetAttributeValue( AttributeKey.ShowAccountSummary ).AsBoolean(),
                AccountConfigured = GetAttributeValue( AttributeKey.Accounts ).HasValue(),
                IsActiveAccountsOnlyFilter = GetAttributeValue( AttributeKey.ActiveAccountsOnlyFilter ).AsBoolean(),
                ShowForeignKeyColumn = GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean(),
                IsForeignCurrencyEnabled = GetAttributeValue( AttributeKey.EnableForeignCurrency ).AsBoolean(),
                ShowDaysSinceLastTransaction = GetAttributeValue( AttributeKey.ShowDaysSinceLastTransaction ).AsBoolean(),
                IsPersonContext = _person != null,
                IsBatchContext = _batch != null,
                IsScheduledTransactionContext = _scheduledTransaction != null,
                IsRegistrationContext = _registration != null,
                AreFiltersVisible = areFiltersVisible,
                IsPersonFilterVisible = areFiltersVisible && _person == null && CanFilterByPerson,
                IsReassignVisible = _person != null && CanEdit,
                IsMoveToBatchVisible = _batch != null && CanEdit && IsBatchEditable,
                ShowClosedBatchWarning = _batch != null && _batch.Status == BatchStatus.Closed,
                MoveToBatchTargets = GetMoveToBatchTargets(),
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
        /// Gets the open batches (excluding the current batch) that selected transactions can be moved
        /// to. Returns an empty list unless the block is in an editable batch context, since the
        /// Move to Batch toolbar is only available there.
        /// </summary>
        /// <returns>The candidate batches as list items keyed by IdKey.</returns>
        private List<ListItemBag> GetMoveToBatchTargets()
        {
            if ( _batch == null || !CanEdit || !IsBatchEditable )
            {
                return new List<ListItemBag>();
            }

            using ( var rockContext = new RockContext() )
            {
                var currentBatchId = _batch.Id;

                var batches = new FinancialBatchService( rockContext )
                    .Queryable()
                    .Where( b => b.Status == BatchStatus.Open
                        && b.BatchStartDateTime.HasValue
                        && b.Id != currentBatchId )
                    .OrderBy( b => b.Id )
                    .Select( b => new
                    {
                        b.Id,
                        b.Name,
                        b.BatchStartDateTime
                    } )
                    .ToList();

                return batches
                    .Select( b => new ListItemBag
                    {
                        Value = IdHasher.Instance.GetHash( b.Id ),
                        Text = $"#{b.Id} {b.Name} ({b.BatchStartDateTime.Value:d})"
                    } )
                    .ToList();
            }
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

            var contextEntityType = GetContextEntityType();

            if ( contextEntityType == typeof( Person ) )
            {
                _person = RequestContext.GetContextEntity( contextEntityType ) as Person;
            }
            else if ( contextEntityType == typeof( FinancialBatch ) )
            {
                _batch = RequestContext.GetContextEntity( contextEntityType ) as FinancialBatch;
            }
            else if ( contextEntityType == typeof( FinancialScheduledTransaction ) )
            {
                _scheduledTransaction = RequestContext.GetContextEntity( contextEntityType ) as FinancialScheduledTransaction;
            }
            else if ( contextEntityType == typeof( Registration ) )
            {
                _registration = RequestContext.GetContextEntity( contextEntityType ) as Registration;
            }

            _contextInitialized = true;
        }

        /// <inheritdoc/>
        protected override IQueryable<TransactionListRow> GetListQueryable( RockContext rockContext )
        {
            InitializeContextEntities();

            var isContextEntityRequired = GetContextEntityType() != null;

            if ( isContextEntityRequired && _person == null && _batch == null && _scheduledTransaction == null && _registration == null )
            {
                return new List<TransactionListRow>().AsQueryable();
            }

            // The two view modes build rows from different entities: one row per transaction,
            // or one row per transaction detail (account line).
            var qry = CurrentViewMode == ViewMode.Accounts
                ? GetAccountsModeQueryable()
                : GetTransactionsModeQueryable();

            return ApplyFilters( qry );
        }

        /// <summary>
        /// Builds the rows for "Transactions" view mode (one row per <see cref="FinancialTransaction"/>).
        /// </summary>
        /// <returns>A queryable of transaction rows.</returns>
        private IQueryable<TransactionListRow> GetTransactionsModeQueryable()
        {
            var qry = new FinancialTransactionService( RockContext ).Queryable().AsNoTracking();

            // Include future-dated transactions only when the block setting opts in; otherwise
            // restrict to rows that have already been posted (TransactionDateTime is set).
            if ( GetAttributeValue( AttributeKey.ShowFutureTransactions ).AsBoolean() )
            {
                qry = qry.Where( t => t.TransactionDateTime.HasValue || t.FutureProcessingDateTime.HasValue );
            }
            else
            {
                qry = qry.Where( t => t.TransactionDateTime.HasValue );
            }

            if ( GetAttributeValue( AttributeKey.HideTransactionsInPendingBatches ).AsBoolean() )
            {
                qry = qry.Where( t => t.Batch == null || t.Batch.Status != BatchStatus.Pending );
            }

            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( t => t.TransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
            }

            // Context entity filters — restrict results to the page's entity context.
            if ( _batch != null )
            {
                qry = qry.Where( t => t.BatchId == _batch.Id );
            }
            else if ( _scheduledTransaction != null )
            {
                qry = qry.Where( t => t.ScheduledTransactionId == _scheduledTransaction.Id );
            }
            else if ( _registration != null )
            {
                var registrationEntityTypeId = EntityTypeCache.Get( typeof( Registration ) )?.Id;
                if ( registrationEntityTypeId.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDetails.Any( d =>
                        d.EntityTypeId.HasValue &&
                        d.EntityTypeId.Value == registrationEntityTypeId.Value &&
                        d.EntityId.HasValue &&
                        d.EntityId.Value == _registration.Id ) );
                }
            }
            else if ( _person != null )
            {
                // Use GivingId so family members who give together are all included.
                var personAliasQry = new PersonAliasService( RockContext )
                    .Queryable()
                    .Where( a => a.Person.GivingId == _person.GivingId )
                    .Select( a => a.Id );

                qry = qry.Where( t => t.AuthorizedPersonAliasId.HasValue
                    && personAliasQry.Contains( t.AuthorizedPersonAliasId.Value ) );
            }

            return qry.Select( t => new TransactionListRow
                {
                    // Referenced (not constructed) so it is materialized for GridAttributeLoader.
                    // Only its scalars/attributes are read later — never its navigations.
                    Transaction = t,
                    Id = t.Id,
                    TransactionId = t.Id,
                    Person = t.AuthorizedPersonAlias.Person,
                    TransactionDateTime = t.TransactionDateTime ?? t.FutureProcessingDateTime,
                    FutureProcessingDateTime = t.FutureProcessingDateTime,
                    TotalAmount = t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ),
                    CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = t.FinancialPaymentDetail.CreditCardTypeValueId,
                    ForeignCurrencyCodeValueId = t.ForeignCurrencyCodeValueId,
                    SourceTypeValueId = t.SourceTypeValueId,
                    TransactionTypeValueId = t.TransactionTypeValueId,
                    TransactionCode = t.TransactionCode,
                    ForeignKey = t.ForeignKey,
                    BatchId = t.BatchId,
                    BatchCampusGuid = ( Guid? ) t.Batch.Campus.Guid,
                    Summary = t.Summary,
                    Status = t.Status,
                    SettledDate = t.SettledDate,
                    SettledGroupId = t.SettledGroupId,
                    AuthorizedPersonAliasId = t.AuthorizedPersonAliasId,
                    AccountGuids = t.TransactionDetails
                        .Select( d => d.Account.Guid )
                        .ToList(),
                    AccountCampusGuids = t.TransactionDetails
                        .Where( d => d.Account.CampusId.HasValue )
                        .Select( d => d.Account.Campus.Guid )
                        .Distinct()
                        .ToList(),
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
        /// <returns>A queryable of transaction detail rows.</returns>
        private IQueryable<TransactionListRow> GetAccountsModeQueryable()
        {
            var qry = new FinancialTransactionDetailService( RockContext ).Queryable().AsNoTracking();

            if ( GetAttributeValue( AttributeKey.ShowFutureTransactions ).AsBoolean() )
            {
                qry = qry.Where( d => d.Transaction.TransactionDateTime.HasValue || d.Transaction.FutureProcessingDateTime.HasValue );
            }
            else
            {
                qry = qry.Where( d => d.Transaction.TransactionDateTime.HasValue );
            }

            if ( GetAttributeValue( AttributeKey.HideTransactionsInPendingBatches ).AsBoolean() )
            {
                qry = qry.Where( d => d.Transaction.Batch == null || d.Transaction.Batch.Status != BatchStatus.Pending );
            }

            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( d => accountGuids.Contains( d.Account.Guid ) );
            }

            // Context entity filters — restrict results to the page's entity context.
            if ( _batch != null )
            {
                qry = qry.Where( d => d.Transaction.BatchId == _batch.Id );
            }
            else if ( _scheduledTransaction != null )
            {
                qry = qry.Where( d => d.Transaction.ScheduledTransactionId == _scheduledTransaction.Id );
            }
            else if ( _registration != null )
            {
                var registrationEntityTypeId = EntityTypeCache.Get( typeof( Registration ) )?.Id;
                if ( registrationEntityTypeId.HasValue )
                {
                    qry = qry.Where( d =>
                        d.EntityTypeId.HasValue &&
                        d.EntityTypeId.Value == registrationEntityTypeId.Value &&
                        d.EntityId.HasValue &&
                        d.EntityId.Value == _registration.Id );
                }
            }
            else if ( _person != null )
            {
                // Use GivingId so family members who give together are all included.
                var personAliasQry = new PersonAliasService( RockContext )
                    .Queryable()
                    .Where( a => a.Person.GivingId == _person.GivingId )
                    .Select( a => a.Id );

                qry = qry.Where( d => d.Transaction.AuthorizedPersonAliasId.HasValue
                    && personAliasQry.Contains( d.Transaction.AuthorizedPersonAliasId.Value ) );
            }

            return qry.Select( d => new TransactionListRow
                {
                    // Referenced (not constructed) so it is materialized for GridAttributeLoader.
                    // Only its scalars/attributes are read later — never its navigations.
                    TransactionDetail = d,
                    Id = d.Id,
                    TransactionId = d.TransactionId,
                    Person = d.Transaction.AuthorizedPersonAlias.Person,
                    TransactionDateTime = d.Transaction.TransactionDateTime ?? d.Transaction.FutureProcessingDateTime,
                    FutureProcessingDateTime = d.Transaction.FutureProcessingDateTime,
                    TotalAmount = d.Amount,
                    CurrencyTypeValueId = d.Transaction.FinancialPaymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = d.Transaction.FinancialPaymentDetail.CreditCardTypeValueId,
                    ForeignCurrencyCodeValueId = d.Transaction.ForeignCurrencyCodeValueId,
                    SourceTypeValueId = d.Transaction.SourceTypeValueId,
                    TransactionTypeValueId = d.Transaction.TransactionTypeValueId,
                    TransactionCode = d.Transaction.TransactionCode,
                    ForeignKey = d.Transaction.ForeignKey,
                    BatchId = d.Transaction.BatchId,
                    BatchCampusGuid = ( Guid? ) d.Transaction.Batch.Campus.Guid,
                    Summary = d.Transaction.Summary,
                    Status = d.Transaction.Status,
                    SettledDate = d.Transaction.SettledDate,
                    SettledGroupId = d.Transaction.SettledGroupId,
                    AuthorizedPersonAliasId = d.Transaction.AuthorizedPersonAliasId,
                    AccountGuids = d.Transaction.TransactionDetails
                        .Where( x => x.Id == d.Id )
                        .Select( x => x.Account.Guid )
                        .ToList(),
                    AccountCampusGuids = d.Transaction.TransactionDetails
                        .Where( x => x.Id == d.Id && x.Account.CampusId.HasValue )
                        .Select( x => x.Account.Campus.Guid )
                        .ToList(),
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

        /// <summary>
        /// Applies the active person preference filters to <paramref name="query"/>.
        /// Each filter is applied only when its preference value is non-empty; missing or
        /// blank preferences are treated as "no filter" for that criterion.
        /// </summary>
        /// <param name="query">The row queryable to filter.</param>
        /// <returns>The filtered queryable.</returns>
        private IQueryable<TransactionListRow> ApplyFilters( IQueryable<TransactionListRow> query )
        {
            // Block-level attribute filters — admin-configured, applied to all results regardless of user preferences.
            var transactionTypeIds = GetAttributeValue( AttributeKey.TransactionTypes )
                .SplitDelimitedValues().AsGuidList()
                .Select( g => DefinedValueCache.Get( g ) )
                .Where( dv => dv != null )
                .Select( dv => dv.Id )
                .ToList();
            if ( transactionTypeIds.Any() )
            {
                query = query.Where( r => r.TransactionTypeValueId.HasValue && transactionTypeIds.Contains( r.TransactionTypeValueId.Value ) );
            }

            var sourceTypeIds = GetAttributeValue( AttributeKey.SourceTypes )
                .SplitDelimitedValues().AsGuidList()
                .Select( g => DefinedValueCache.Get( g ) )
                .Where( dv => dv != null )
                .Select( dv => dv.Id )
                .ToList();
            if ( sourceTypeIds.Any() )
            {
                query = query.Where( r => r.SourceTypeValueId.HasValue && sourceTypeIds.Contains( r.SourceTypeValueId.Value ) );
            }

            if ( AreFiltersVisible )
            {
                var dateRange = FilterDateRange;

                if ( dateRange?.Start.HasValue == true )
                {
                    var start = dateRange.Start.Value;
                    query = query.Where( r => r.TransactionDateTime >= start );
                }

                if ( dateRange?.End.HasValue == true )
                {
                    var end = dateRange.End.Value;
                    query = query.Where( r => r.TransactionDateTime <= end );
                }
            }

            // Amount range.
            var amountFrom = FilterAmountRangeFrom;
            if ( amountFrom.HasValue )
            {
                query = query.Where( r => r.TotalAmount >= amountFrom.Value );
            }

            var amountTo = FilterAmountRangeTo;
            if ( amountTo.HasValue )
            {
                query = query.Where( r => r.TotalAmount <= amountTo.Value );
            }

            // Defined-value filters — resolve GUID to Id via cache then compare the projected int column.
            var currencyTypeId = FilterCurrencyType.HasValue ? DefinedValueCache.Get( FilterCurrencyType.Value )?.Id : null;
            if ( currencyTypeId.HasValue )
            {
                query = query.Where( r => r.CurrencyTypeValueId == currencyTypeId.Value );
            }

            var creditCardTypeId = FilterCreditCardType.HasValue ? DefinedValueCache.Get( FilterCreditCardType.Value )?.Id : null;
            if ( creditCardTypeId.HasValue )
            {
                query = query.Where( r => r.CreditCardTypeValueId == creditCardTypeId.Value );
            }

            var sourceTypeId = FilterSourceType.HasValue ? DefinedValueCache.Get( FilterSourceType.Value )?.Id : null;
            if ( sourceTypeId.HasValue )
            {
                query = query.Where( r => r.SourceTypeValueId == sourceTypeId.Value );
            }

            var transactionTypeId = FilterTransactionType.HasValue ? DefinedValueCache.Get( FilterTransactionType.Value )?.Id : null;
            if ( transactionTypeId.HasValue )
            {
                query = query.Where( r => r.TransactionTypeValueId == transactionTypeId.Value );
            }

            // Free-text filters.
            var transactionCode = FilterTransactionCode;
            if ( transactionCode.IsNotNullOrWhiteSpace() )
            {
                query = query.Where( r => r.TransactionCode.Contains( transactionCode ) );
            }

            var foreignKey = FilterForeignKey;
            if ( foreignKey.IsNotNullOrWhiteSpace() )
            {
                query = query.Where( r => r.ForeignKey.Contains( foreignKey ) );
            }

            // Entity GUID filters — pickers store GUIDs; compare directly against projected GUID columns.
            var accountGuid = FilterAccount.AsGuidOrNull();
            if ( accountGuid.HasValue )
            {
                query = query.Where( r => r.AccountGuids.Contains( accountGuid.Value ) );
            }

            var batchCampusGuid = FilterCampusOfBatch.AsGuidOrNull();
            if ( batchCampusGuid.HasValue )
            {
                query = query.Where( r => r.BatchCampusGuid == batchCampusGuid.Value );
            }

            var accountCampusGuid = FilterCampusOfAccount.AsGuidOrNull();
            if ( accountCampusGuid.HasValue )
            {
                query = query.Where( r => r.AccountCampusGuids.Contains( accountCampusGuid.Value ) );
            }

            // Person — PersonPicker stores the primary alias GUID. Resolve it to a PersonId, then
            // match any of that person's aliases so merged records still appear. Only honored when
            // the user is authorized for the "FilterByPerson" security action.
            var personAliasGuid = CanFilterByPerson ? FilterPerson.AsGuidOrNull() : null;
            if ( personAliasGuid.HasValue )
            {
                var personAliasService = new PersonAliasService( RockContext );
                var personId = personAliasService.Queryable()
                    .Where( a => a.Guid == personAliasGuid.Value )
                    .Select( a => ( int? ) a.PersonId )
                    .FirstOrDefault();

                if ( personId.HasValue )
                {
                    var personAliasIds = personAliasService.Queryable()
                        .Where( a => a.PersonId == personId.Value )
                        .Select( a => a.Id );

                    query = query.Where( r => r.AuthorizedPersonAliasId.HasValue
                        && personAliasIds.Contains( r.AuthorizedPersonAliasId.Value ) );
                }
            }

            return query;
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
                .AddTextField( "processorBatchId", a => a.SettledGroupId )
                .AddAttributeFieldsFrom( a => CurrentViewMode == ViewMode.Accounts ? a.TransactionDetail : a.Transaction, GridAttributes.Value );

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
        /// Gets the URL of the first image for the transaction, or <c>null</c> when the transaction has no image.
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
        private static List<AttributeCache> BuildGridAttributes( bool isAccountsMode )
        {
            var entityTypeId = isAccountsMode
                ? EntityTypeCache.Get<FinancialTransactionDetail>( false )?.Id
                : EntityTypeCache.Get<FinancialTransaction>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }

        /// <summary>
        /// Builds the list of attribute column definitions for the grid options bag.
        /// </summary>
        /// <param name="isAccountsMode"><c>true</c> returns <see cref="FinancialTransactionDetail"/> attributes; <c>false</c> returns <see cref="FinancialTransaction"/> attributes.</param>
        /// <returns>A list of attribute field definitions, one per grid-visible attribute.</returns>
        private List<AttributeFieldDefinitionBag> GetAttributeOptions( bool isAccountsMode )
        {
            var textFieldTypeGuid = SystemGuid.FieldType.TEXT.AsGuid();
            var attributes = isAccountsMode ? _accountGridAttributes.Value : _transactionGridAttributes.Value;
            var fields = new List<AttributeFieldDefinitionBag>();

            foreach ( var attribute in attributes )
            {
                fields.Add( new AttributeFieldDefinitionBag
                {
                    Name = $"attr_{attribute.Key}",
                    Title = attribute.Name,
                    FieldTypeGuid = attribute.FieldType?.Guid ?? textFieldTypeGuid
                } );
            }

            return fields;
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

            PersonPreferences.SetValue( PreferenceKey.ViewMode, viewMode );
            PersonPreferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Deletes a row from the grid. In Transactions mode the row is a <see cref="FinancialTransaction"/>;
        /// in Accounts mode it is a <see cref="FinancialTransactionDetail"/>. Mirrors the WebForms guards:
        /// rows belonging to a closed or automated batch cannot be deleted.
        /// </summary>
        /// <param name="key">The IdKey of the row to delete.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !CanEdit )
            {
                return ActionBadRequest( $"Not authorized to delete {FinancialTransaction.FriendlyTypeName}." );
            }

            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;

            // In Accounts mode each row is a transaction detail; otherwise it is a whole transaction.
            if ( CurrentViewMode == ViewMode.Accounts )
            {
                return DeleteTransactionDetail( key, allowIntegerId );
            }

            return DeleteTransaction( key, allowIntegerId );
        }

        /// <summary>
        /// Deletes a <see cref="FinancialTransaction"/> by IdKey, recording the change in the batch's history.
        /// </summary>
        /// <param name="key">The IdKey of the transaction to delete.</param>
        /// <param name="allowIntegerId">Whether integer Ids are permitted in addition to IdKeys.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        private BlockActionResult DeleteTransaction( string key, bool allowIntegerId )
        {
            var transactionService = new FinancialTransactionService( RockContext );
            var transaction = transactionService.Get( key, allowIntegerId );

            if ( transaction == null )
            {
                return ActionBadRequest( $"{FinancialTransaction.FriendlyTypeName} not found." );
            }

            if ( !transactionService.CanDelete( transaction, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var batchRestriction = GetBatchDeleteRestriction( transaction.Batch );
            if ( batchRestriction != null )
            {
                return ActionBadRequest( batchRestriction );
            }

            // Record the deletion in the batch's history, mirroring the WebForms behavior.
            if ( transaction.BatchId.HasValue )
            {
                var caption = transaction.AuthorizedPersonAlias?.Person != null
                    ? transaction.AuthorizedPersonAlias.Person.FullName
                    : $"Transaction: {transaction.Id}";

                var changes = new History.HistoryChangeList();
                changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

                HistoryService.SaveChanges(
                    RockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    transaction.BatchId.Value,
                    changes,
                    caption,
                    typeof( FinancialTransaction ),
                    transaction.Id,
                    false );
            }

            transactionService.Delete( transaction );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes a <see cref="FinancialTransactionDetail"/> by IdKey (Accounts view mode).
        /// </summary>
        /// <param name="key">The IdKey of the transaction detail to delete.</param>
        /// <param name="allowIntegerId">Whether integer Ids are permitted in addition to IdKeys.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        private BlockActionResult DeleteTransactionDetail( string key, bool allowIntegerId )
        {
            var detailService = new FinancialTransactionDetailService( RockContext );
            var transactionDetail = detailService.Get( key, allowIntegerId );

            if ( transactionDetail == null )
            {
                return ActionBadRequest( $"{FinancialTransactionDetail.FriendlyTypeName} not found." );
            }

            var batchRestriction = GetBatchDeleteRestriction( transactionDetail.Transaction?.Batch );
            if ( batchRestriction != null )
            {
                return ActionBadRequest( batchRestriction );
            }

            detailService.Delete( transactionDetail );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Returns an error message when the given batch prevents deletion (it is closed or automated),
        /// or <c>null</c> when deletion is allowed.
        /// </summary>
        /// <param name="batch">The batch the transaction belongs to, if any.</param>
        /// <returns>The restriction message, or <c>null</c> when there is no restriction.</returns>
        private string GetBatchDeleteRestriction( FinancialBatch batch )
        {
            if ( batch == null )
            {
                return null;
            }

            if ( batch.Status == BatchStatus.Closed )
            {
                return $"This {FinancialTransaction.FriendlyTypeName} is assigned to a closed {FinancialBatch.FriendlyTypeName} and cannot be deleted.";
            }

            if ( batch.IsAutomated )
            {
                return $"This {FinancialTransaction.FriendlyTypeName} is assigned to an automated {FinancialBatch.FriendlyTypeName} and cannot be deleted.";
            }

            return null;
        }

        /// <summary>
        /// Moves the selected transactions to the specified batch, adjusting the control amounts on
        /// both the source and destination batches and recording the changes in their history.
        /// Only available in Transactions view mode within an editable batch context.
        /// </summary>
        /// <param name="transactionKeys">The IdKeys of the transactions to move.</param>
        /// <param name="batchKey">The IdKey of the destination batch.</param>
        /// <returns>A result describing how many transactions were moved and to which batch.</returns>
        [BlockAction]
        public BlockActionResult MoveTransactions( List<string> transactionKeys, string batchKey )
        {
            InitializeContextEntities();

            if ( !CanEdit || _batch == null || !IsBatchEditable )
            {
                return ActionBadRequest( "Not authorized to move transactions." );
            }

            if ( transactionKeys == null || !transactionKeys.Any() )
            {
                return ActionBadRequest( "There were not any transactions selected." );
            }

            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;

            var batchService = new FinancialBatchService( RockContext );
            var newBatch = batchService.Get( batchKey, allowIntegerId );
            var oldBatch = batchService.Get( _batch.Id );

            if ( oldBatch == null || newBatch == null || newBatch.Status != BatchStatus.Open )
            {
                return ActionBadRequest( "The selected batch does not exist, or is no longer open." );
            }

            var transactionIds = transactionKeys
                .Select( key => IdHasher.Instance.GetId( key ) ?? ( allowIntegerId ? key.AsIntegerOrNull() : null ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var transactions = new FinancialTransactionService( RockContext )
                .Queryable()
                .Include( t => t.TransactionDetails )
                .Where( t => transactionIds.Contains( t.Id ) )
                .ToList();

            var oldControlAmount = oldBatch.ControlAmount;
            var newControlAmount = newBatch.ControlAmount;

            foreach ( var transaction in transactions )
            {
                transaction.BatchId = newBatch.Id;
                oldControlAmount -= transaction.TotalAmount;
                newControlAmount += transaction.TotalAmount;
            }

            // Record the control amount change on the source batch.
            var oldBatchChanges = new History.HistoryChangeList();
            History.EvaluateChange( oldBatchChanges, "Control Amount", oldBatch.ControlAmount.FormatAsCurrency(), oldControlAmount.FormatAsCurrency() );
            oldBatch.ControlAmount = oldControlAmount;

            HistoryService.SaveChanges(
                RockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                oldBatch.Id,
                oldBatchChanges,
                false );

            // Record the control amount change on the destination batch.
            var newBatchChanges = new History.HistoryChangeList();
            History.EvaluateChange( newBatchChanges, "Control Amount", newBatch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
            newBatch.ControlAmount = newControlAmount;

            HistoryService.SaveChanges(
                RockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                newBatch.Id,
                newBatchChanges,
                false );

            RockContext.SaveChanges();

            return ActionOk( new
            {
                MovedCount = transactions.Count,
                BatchName = newBatch.Name
            } );
        }

        /// <summary>
        /// Reassigns the selected transactions to a different person, and optionally moves or copies
        /// that source person's saved bank accounts to the target person. Only available in a Person
        /// context with edit permission. In Accounts view mode the selected keys are transaction
        /// details, whose parent transactions are reassigned.
        /// </summary>
        /// <param name="transactionKeys">The IdKeys of the selected transactions (Transactions mode) or transaction details (Accounts mode).</param>
        /// <param name="personAliasKey">The Guid of the target person's alias (from the person picker).</param>
        /// <param name="bankAccountAction">How to handle the source person's saved bank accounts: "MOVE", "COPY", or "NONE".</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReassignTransactions( List<string> transactionKeys, string personAliasKey, string bankAccountAction )
        {
            InitializeContextEntities();

            if ( !CanEdit || _person == null )
            {
                return ActionBadRequest( "Not authorized to reassign transactions." );
            }

            if ( transactionKeys == null || !transactionKeys.Any() )
            {
                return ActionBadRequest( "There were not any transactions selected." );
            }

            var targetPersonAlias = new PersonAliasService( RockContext ).Get( personAliasKey.AsGuid() );
            if ( targetPersonAlias == null )
            {
                return ActionBadRequest( "The selected person could not be found." );
            }

            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;
            var selectedIds = transactionKeys
                .Select( key => IdHasher.Instance.GetId( key ) ?? ( allowIntegerId ? key.AsIntegerOrNull() : null ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var transactionService = new FinancialTransactionService( RockContext );

            // In Accounts mode the keys are transaction details, so reassign their parent transactions;
            // in Transactions mode the keys are the transactions themselves.
            var transactionsToReassign = CurrentViewMode == ViewMode.Accounts
                ? transactionService.Queryable().Where( t => t.TransactionDetails.Any( d => selectedIds.Contains( d.Id ) ) )
                : transactionService.Queryable().Where( t => selectedIds.Contains( t.Id ) );

            foreach ( var transaction in transactionsToReassign.ToList() )
            {
                transaction.AuthorizedPersonAliasId = targetPersonAlias.Id;
            }

            RockContext.SaveChanges();

            ReassignBankAccounts( bankAccountAction, targetPersonAlias );

            return ActionOk();
        }

        /// <summary>
        /// Moves or copies the context person's saved bank accounts to the target person, depending on
        /// the requested action. Accounts the target already has (matched by secured account number)
        /// are skipped, and the change is recorded in each person's history. Does nothing when the
        /// action is "NONE" or the source person has no saved bank accounts.
        /// </summary>
        /// <param name="bankAccountAction">"MOVE", "COPY", or "NONE".</param>
        /// <param name="targetPersonAlias">The alias of the person receiving the accounts.</param>
        private void ReassignBankAccounts( string bankAccountAction, PersonAlias targetPersonAlias )
        {
            if ( bankAccountAction == "NONE" || _person == null )
            {
                return;
            }

            var bankAccountService = new FinancialPersonBankAccountService( RockContext );

            var sourceBankAccounts = bankAccountService.Queryable()
                .Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == _person.Id )
                .ToList();

            if ( !sourceBankAccounts.Any() )
            {
                return;
            }

            // Secured account numbers the target already has, used to avoid creating duplicates.
            var targetSecuredNumbers = bankAccountService.Queryable()
                .Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == targetPersonAlias.PersonId )
                .Select( a => a.AccountNumberSecured )
                .ToList();

            var isMove = bankAccountAction == "MOVE";

            foreach ( var bankAccount in sourceBankAccounts )
            {
                var targetAlreadyHasAccount = targetSecuredNumbers.Contains( bankAccount.AccountNumberSecured );

                if ( isMove )
                {
                    // Reassign the account to the target, deleting it instead when the target already has it.
                    if ( targetAlreadyHasAccount )
                    {
                        bankAccountService.Delete( bankAccount );
                    }
                    else
                    {
                        bankAccount.PersonAliasId = targetPersonAlias.Id;
                    }
                }
                else if ( !targetAlreadyHasAccount )
                {
                    // Copy: leave the source account in place and add a matching one for the target.
                    bankAccountService.Add( new FinancialPersonBankAccount
                    {
                        PersonAliasId = targetPersonAlias.Id,
                        AccountNumberMasked = bankAccount.AccountNumberMasked,
                        AccountNumberSecured = bankAccount.AccountNumberSecured
                    } );
                }
            }

            RockContext.SaveChanges();

            var modifiedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            // Moving removes the accounts from the source person, so log that on their history.
            if ( isMove )
            {
                var sourceChanges = new History.HistoryChangeList();
                sourceChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Acct/Routing information" );
                HistoryService.SaveChanges( RockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                    _person.Id, sourceChanges, true, modifiedByPersonAliasId );
            }

            var targetChanges = new History.HistoryChangeList();
            targetChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Acct/Routing information" );
            HistoryService.SaveChanges( RockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                targetPersonAlias.PersonId, targetChanges, true, modifiedByPersonAliasId );
        }

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

            public int? SourceTypeValueId { get; set; }

            public int? TransactionTypeValueId { get; set; }

            public string TransactionCode { get; set; }

            public string ForeignKey { get; set; }

            public int? BatchId { get; set; }

            /// <summary>
            /// The GUID of the campus of the batch this transaction belongs to, for campus-of-batch filtering.
            /// </summary>
            public Guid? BatchCampusGuid { get; set; }

            public string Summary { get; set; }

            public string Status { get; set; }

            public DateTime? SettledDate { get; set; }

            public string SettledGroupId { get; set; }

            /// <summary>
            /// The alias id of the authorized person, for person filtering.
            /// </summary>
            public int? AuthorizedPersonAliasId { get; set; }

            /// <summary>
            /// The GUIDs of every account on this row. One per detail in Transactions mode;
            /// a single entry in Accounts mode. Used for account filtering.
            /// </summary>
            public List<Guid> AccountGuids { get; set; }

            /// <summary>
            /// The campus GUIDs of every account on this row (nulls excluded). Used for
            /// campus-of-account filtering.
            /// </summary>
            public List<Guid> AccountCampusGuids { get; set; }

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
