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
    [Rock.SystemGuid.BlockTypeGuid( "E04320BC-67C3-452D-9EF6-D74D8C177154" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware]
    public class TransactionList : RockListBlockType<TransactionListData>
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
        protected override IQueryable<TransactionListData> GetListQueryable( RockContext rockContext )
        {
            // TODO Step 3: build the query for either Transactions or Accounts view mode.
            return Enumerable.Empty<TransactionListData>().AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<TransactionListData> GetOrderedListQueryable( IQueryable<TransactionListData> queryable, RockContext rockContext )
        {
            // TODO Step 3: apply the default and custom (person name) ordering.
            return queryable;
        }

        /// <inheritdoc/>
        protected override GridBuilder<TransactionListData> GetGridBuilder()
        {
            // TODO Step 3: define the grid columns for both view modes (person, date, amount, currency, accounts, comments, image, attributes, etc.).
            return new GridBuilder<TransactionListData>()
                .WithBlock( this );
        }

        #endregion Methods

        #region Block Actions

        // TODO Step 5: Delete, Reassign, and MoveToBatch block actions (with their request bags).

        #endregion Block Actions

        #region Supported Classes

        /// <summary>
        /// A single row of data for the transaction list grid. Supports both
        /// the "Transactions" and "Accounts" view modes with minimal special casing.
        /// </summary>
        public class TransactionListData
        {
            // TODO Step 3: define the row shape (transaction/detail fields, accounts, summary, etc.).
        }

        #endregion Supported Classes
    }
}
