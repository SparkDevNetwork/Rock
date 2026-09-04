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
using Rock.ViewModels.Blocks.Finance.FinancialScheduledTransactionList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using static Rock.Blocks.Finance.FinancialScheduledTransactionList;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial scheduled transactions.
    /// </summary>

    [DisplayName( "Financial Scheduled Transaction List" )]
    [Category( "Finance" )]
    [Description( "Displays a list of financial scheduled transactions." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "View Page",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.ViewPage )]

    [LinkedPage( "Add Page",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.AddPage )]

    [AccountsField( "Accounts",
        Description = "Limit the results to scheduled transactions that match the selected accounts.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.Accounts )]

    [IntegerField( "Person Token Expire Minutes",
        Description = "When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.",
        IsRequired = true,
        DefaultIntegerValue = 60,
        Order = 3,
        Key = AttributeKey.PersonTokenExpireMinutes )]

    [IntegerField( "Person Token Usage Limit",
        Description = "When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 4,
        Key = AttributeKey.PersonTokenUsageLimit )]

    [BooleanField( "Show Transaction Type Column",
        Description = "Show the Transaction Type column.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 5,
        Key = AttributeKey.ShowTransactionTypeColumn )]


    [Rock.SystemGuid.EntityTypeGuid( "946127ec-adec-46c9-8181-a405c137a8a3" )]
    [Rock.SystemGuid.BlockTypeGuid( "694FF260-8C6F-4A59-93C9-CF3793FE30E6" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "2db92ea3-f3b3-496e-a1f0-8eebd8dc928a" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware]
    public class FinancialScheduledTransactionList : RockListBlockType<FinancialScheduledTransactionData>
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The view page
            /// </summary>
            public const string ViewPage = "ViewPage";
            /// <summary>
            /// The add page
            /// </summary>
            public const string AddPage = "AddPage";
            /// <summary>
            /// The accounts
            /// </summary>
            public const string Accounts = "Accounts";
            /// <summary>
            /// The person token expire minutes
            /// </summary>
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";
            /// <summary>
            /// The person token usage limit
            /// </summary>
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";
            /// <summary>
            /// The show transaction type column attribute key
            /// </summary>
            public const string ShowTransactionTypeColumn = "ShowTransactionTypeColumn";
        }

        private static class NavigationUrlKey
        {
            public const string ViewPage = "ViewPage";
            public const string AddPage = "AddPage";
        }

        private static class PageParameterKey
        {
            public const string Person = "Person";
        }

        private static class PreferenceKey
        {
            public const string FilterAmountRangeFrom = "filter-amount-range-from";
            public const string FilterAmountRangeTo = "filter-amount-range-to";
            public const string FilterAccount = "filter-account";
            public const string FilterFrequency = "filter-frequency";
            public const string FilterIncludeInctiveSchedules = "filter-include-inctive-schedules";
            public const string FilterDateRangeLower = "filter-date-range-lower";
            public const string FilterDateRangeUpper = "filter-date-range-upper";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The scheduled transaction attributes configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new System.Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Fields

        private Person _person;

        #endregion

        #region Properties
        protected DateTime? FilterDateRangeUpper => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRangeUpper )
            .AsDateTime();

        protected DateTime? FilterDateRangeLower => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRangeLower )
            .AsDateTime();

        /// <summary>
        /// Determines whether or not to include inactive schedules in the result.
        /// </summary>
        /// <value>
        /// The filter include schedules.
        /// </value>
        protected string FilterIncludeInctiveSchedules => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIncludeInctiveSchedules );

        /// <summary>
        /// Gets frequency guid with which to filter the results.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected Guid? FilterFrequency => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterFrequency )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected decimal? FilterAmountRangeFrom => GetBlockPersonPreferences()
           .GetValue( PreferenceKey.FilterAmountRangeFrom ).AsDecimalOrNull();

        protected decimal? FilterAmountRangeTo => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterAmountRangeTo ).AsDecimalOrNull();

        /// <summary>
        /// Gets the account identifier to use when filtering the scheduled transactions. Only
        /// scheduled transactions with a detail item going to
        /// this account will be included.
        /// </summary>
        /// <value>
        /// The account identifier to use when filtering the scheduled transactions.
        /// </value>
        protected Guid? FilterAccount => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterAccount )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            InitializeContextEntities();
            var box = new ListBlockBox<FinancialScheduledTransactionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();
            box.Options.ShowTransactionTypeColumn = GetAttributeValue( AttributeKey.ShowTransactionTypeColumn ).AsBoolean();
            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialScheduledTransactionListOptionsBag GetBoxOptions()
        {
            // Provide the organization's default currency so the client formats amounts
            // with the correct symbol and decimal places instead of a hard-coded "$".
            var currencyInfo = new RockCurrencyCodeInfo();

            var options = new FinancialScheduledTransactionListOptionsBag
            {
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
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new FinancialScheduledTransaction();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.AddPage ) );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var addPageLinkedUrl = string.Empty;
            var addScheduledTransactionPage = new Rock.Web.PageReference( GetAttributeValue( AttributeKey.AddPage ) );
            if ( addScheduledTransactionPage != null )
            {
                if ( _person != null && _person.IsPersonTokenUsageAllowed() )
                {
                    // create a limited-use personkey that will last long enough for them to go thru all the 'postbacks' while posting a transaction
                    var personKey = _person.GetImpersonationToken(
                        RockDateTime.Now.AddMinutes( this.GetAttributeValue( AttributeKey.PersonTokenExpireMinutes ).AsIntegerOrNull() ?? 60 ), this.GetAttributeValue( AttributeKey.PersonTokenUsageLimit ).AsIntegerOrNull(), addScheduledTransactionPage.PageId );

                    if ( personKey.IsNotNullOrWhiteSpace() )
                    {
                        addScheduledTransactionPage.QueryString[PageParameterKey.Person] = personKey;
                        addPageLinkedUrl = addScheduledTransactionPage.BuildUrl();
                    }
                }
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ViewPage] = this.GetLinkedPageUrl( AttributeKey.ViewPage, "ScheduledTransactionId", "((Key))" ),
                [NavigationUrlKey.AddPage] = addPageLinkedUrl
            };
        }

        /// <summary>
        /// Get a queryable for scheduled transactions that is properly filtered.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A queryable for <see cref="FinancialScheduledTransaction"/>.</returns>
        private IQueryable<FinancialScheduledTransaction> GetScheduledTransactionQueryable( RockContext rockContext )
        {
            int? personId = null;
            int? givingGroupId = null;

            if ( _person != null )
            {
                personId = _person.Id;
                givingGroupId = _person.GivingGroupId;
            }

            IQueryable<FinancialScheduledTransaction> qry = new FinancialScheduledTransactionService( RockContext )
                .Queryable()
                .Include( t => t.ScheduledTransactionDetails )
                .Include( t => t.FinancialPaymentDetail.CurrencyTypeValue )
                .Include( t => t.FinancialPaymentDetail.CreditCardTypeValue );

            if ( GetAttributeValue( AttributeKey.ShowTransactionTypeColumn ).AsBoolean() )
            {
                // Include the TransactionTypeValue when the column should be shown.
                qry = qry.Include( t => t.TransactionTypeValue );
            }

            qry = qry.AsNoTracking();

            // Valid Accounts
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
            }

            // Amount Range
            if ( FilterAmountRangeFrom.HasValue )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) >= FilterAmountRangeFrom.Value );
            }

            if ( FilterAmountRangeTo.HasValue )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) <= FilterAmountRangeTo.Value );
            }

            // Frequency
            if ( FilterFrequency.HasValue )
            {
                qry = qry.Where( t => t.TransactionFrequencyValue.Guid == FilterFrequency.Value );
            }

            // Date Range
            if ( FilterDateRangeLower.HasValue )
            {
                qry = qry.Where( t => t.CreatedDateTime >= FilterDateRangeLower.Value );
            }

            if ( FilterDateRangeUpper.HasValue )
            {
                DateTime upperDate = FilterDateRangeUpper.Value.Date.AddDays( 1 );
                qry = qry.Where( t => t.CreatedDateTime < upperDate );
            }

            // Account Id
            if ( FilterAccount.HasValue )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => d.Account.Guid == FilterAccount.Value ) );
            }

            // filter down to active only based on person preference
            bool includeInctiveSchedules = FilterIncludeInctiveSchedules.AsBoolean();
            if ( !includeInctiveSchedules )
            {
                qry = qry.Where( t => t.IsActive );
            }

            if ( givingGroupId.HasValue )
            {
                //  Person contributes with family
                qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == givingGroupId );
            }
            else if ( personId.HasValue )
            {
                // Person contributes individually
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == personId );
            }

            return qry;


        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialScheduledTransactionData> GetOrderedListQueryable( IQueryable<FinancialScheduledTransactionData> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( t => t.FinancialScheduledTransaction.AuthorizedPersonAlias.Person.LastName )
                        .ThenBy( t => t.FinancialScheduledTransaction.AuthorizedPersonAlias.Person.NickName )
                        .ThenByDescending( t => t.FinancialScheduledTransaction.IsActive )
                        .ThenByDescending( t => t.FinancialScheduledTransaction.StartDate );
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialScheduledTransactionData> GetListQueryable( RockContext rockContext )
        {
            return GetScheduledTransactionQueryable( RockContext )
                .Select( a => new FinancialScheduledTransactionData
                {
                    FinancialScheduledTransaction = a,
                    AuthorizedPerson = a.AuthorizedPersonAlias.Person,
                    CurrencyTypeValueId = a.FinancialPaymentDetail.CurrencyTypeValueId,
                    AccountLines = a.ScheduledTransactionDetails.Select( d => new AccountLine
                    {
                        AccountId = d.AccountId,
                        AccountGuid = d.Account.Guid,
                        Order = d.Account.Order,
                        Name = d.Account.Name,
                        Amount = d.Amount
                    } )
                } );
        }

        /// <inheritdoc/>
        protected override List<FinancialScheduledTransactionData> GetListItems( IQueryable<FinancialScheduledTransactionData> queryable, RockContext rockContext )
        {
            var contextEntityType = GetContextEntityType();
            if ( contextEntityType == typeof( Person ) && _person == null )
            {
                return new List<FinancialScheduledTransactionData>();
            }

            // Load all the scheduleTransaction into memory.
            var items = queryable.ToList();
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            // Translate the account summary data into a format that can be
            // sent to the client.
            foreach ( var item in items )
            {
                var accounts = item.AccountLines
                    .Select( d => new
                    {
                        Id = accountGuids.Any() && !accountGuids.Contains( d.AccountGuid ) ? 0 : d.AccountId,
                        Order = d.Order,
                        Name = d.Name,
                        Amount = d.Amount,
                        IsOther = accountGuids.Any() && !accountGuids.Contains( d.AccountGuid )
                    } )
                    .OrderBy( d => d.IsOther )
                    .ThenBy( d => d.Order )
                    .ToList();

                if ( accounts.Any() )
                {
                    item.Accounts = accounts
                        .GroupBy( a => a.Id )
                        .Select( a =>
                        {
                            return new AccountData
                            {
                                Name = a.Select( b => b.Name ).First(),
                                Amount = a.Sum( b => b.Amount )
                            };
                        } )
                        .ToList();
                }
                else
                {
                    item.Accounts = new List<AccountData>();
                }
            }

            GridAttributeLoader.LoadFor( items, i => i.FinancialScheduledTransaction, _gridAttributes.Value, RockContext );

            return items;
        }



        /// <inheritdoc/>
        protected override GridBuilder<FinancialScheduledTransactionData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<FinancialScheduledTransactionData>
            {
                LavaObject = row => row.FinancialScheduledTransaction
            };
            return new GridBuilder<FinancialScheduledTransactionData>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.FinancialScheduledTransaction.IdKey )
                .AddTextField( "id", a => a.FinancialScheduledTransaction.Id.ToString() )
                .AddPersonField( "authorized", a => a.AuthorizedPerson )
                .AddTextField( "transactionFrequency", a => DefinedValueCache.GetValue( a.FinancialScheduledTransaction.TransactionFrequencyValueId ) )
                .AddTextField( "transactionType", a => DefinedValueCache.GetValue( a.FinancialScheduledTransaction.TransactionTypeValueId ) )
                .AddTextField( "gatewayScheduleId", a => a.FinancialScheduledTransaction.GatewayScheduleId )
                .AddField( "amount", a => a.AccountLines.Sum( l => l.Amount ) )
                .AddDateTimeField( "createdDateTime", a => a.FinancialScheduledTransaction.CreatedDateTime )
                .AddDateTimeField( "startDate", a => a.FinancialScheduledTransaction.StartDate )
                .AddDateTimeField( "endDate", a => a.FinancialScheduledTransaction.EndDate )
                .AddDateTimeField( "nextPayment", a => a.FinancialScheduledTransaction.NextPaymentDate )
                .AddTextField( "currencyType", a => DefinedValueCache.GetValue( a.CurrencyTypeValueId ) )
                .AddField( "accounts", a => a.Accounts )
                .AddField( "isActive", a => a.FinancialScheduledTransaction.IsActive )
                .AddAttributeFieldsFrom( a => a.FinancialScheduledTransaction, _gridAttributes.Value );
        }

        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<FinancialScheduledTransaction>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }

        /// <summary>
        /// Initializes the context entities and returns a boolean value indicating whether or not the block has a valid context entity
        /// based on the configuration of the <see cref="Rock.Web.UI.ContextAwareAttribute"/> attribute.
        /// </summary>
        /// <returns></returns>
        private bool InitializeContextEntities()
        {
            var contextEntityType = GetContextEntityType();
            if ( contextEntityType == typeof( Person ) )
            {
                _person = RequestContext.GetContextEntity( contextEntityType ) as Person;
                return _person != null;
            }

            return true;
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
            var entityService = new FinancialScheduledTransactionService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{FinancialScheduledTransaction.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {FinancialScheduledTransaction.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Supported Classes

        /// <summary>
        ///
        /// </summary>
        public class FinancialScheduledTransactionData
        {
            /// <summary>
            /// Gets or sets the amount
            /// </summary>
            /// <value>
            /// The amount.
            /// </value>
            public decimal? Amount { get; set; }

            /// <summary>
            /// Gets or sets the whole financial scheduled Transaction object from the database.
            /// </summary>
            /// <value>
            /// The whole financial scheduled Transaction object from the database.
            /// </value>
            public FinancialScheduledTransaction FinancialScheduledTransaction { get; set; }

            /// <summary>
            /// Gets or sets the account data for this batch.
            /// </summary>
            /// <value>
            /// The account data for this batch.
            /// </value>
            public IEnumerable<AccountData> Accounts { get; set; }

            /// <summary>
            /// Gets or sets the raw per-account detail lines for this scheduled
            /// transaction. Projected directly in the list query so the fund data
            /// rides along in a single joined query instead of lazy-loading the
            /// detail and account navigations per row.
            /// </summary>
            public IEnumerable<AccountLine> AccountLines { get; set; }

            /// <summary>
            /// Gets or sets the authorized person, projected in the list query so the
            /// grid's person column does not lazy-load the alias and person per row.
            /// </summary>
            public Person AuthorizedPerson { get; set; }

            /// <summary>
            /// Gets or sets the currency type defined value identifier from the payment
            /// detail, projected in the list query so the grid can resolve the currency
            /// type from cache without lazy-loading the payment detail per row.
            /// </summary>
            public int? CurrencyTypeValueId { get; set; }
        }

        /// <summary>
        /// A single scheduled-transaction detail line, flattened with its account's
        /// display fields so the grid can build the account summary without touching
        /// the entity navigations.
        /// </summary>
        public class AccountLine
        {
            /// <summary>
            /// Gets or sets the account identifier for this line.
            /// </summary>
            public int AccountId { get; set; }

            /// <summary>
            /// Gets or sets the account unique identifier, used to test membership
            /// against the block's configured accounts.
            /// </summary>
            public Guid AccountGuid { get; set; }

            /// <summary>
            /// Gets or sets the account's display order.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the account name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the amount for this line.
            /// </summary>
            public decimal Amount { get; set; }
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
