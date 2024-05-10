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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialScheduledTransactionList;
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
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

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
    [Rock.SystemGuid.BlockTypeGuid( "2db92ea3-f3b3-496e-a1f0-8eebd8dc928a" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware( typeof( Person ) )]
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
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterCategory = "filter-category";
            public const string FilterActiveStatus = "filter-active-status";
        }

        #endregion Keys

        #region Properties

        //protected string FilterTitle => GetBlockPersonPreferences()
        //    .GetValue( PreferenceKey.FilterActiveStatus );

        //protected string FilterRequest => GetBlockPersonPreferences()
        //    .GetValue( PreferenceKey.FilterRequest );

        //protected string FilterActive => GetBlockPersonPreferences()
        //    .GetValue( PreferenceKey.FilterActive );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
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
            var options = new FinancialScheduledTransactionListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new FinancialScheduledTransaction();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.ViewPage, "FinancialScheduledTransactionId", "((Key))" )
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


            var person = GetPerson( rockContext );
            if ( person != null )
            {
                personId = person.Id;
                givingGroupId = person.GivingGroupId;
            }

            var qry = new FinancialScheduledTransactionService( rockContext )
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

            //// Amount Range
            //var nre = new NumberRangeEditor();
            //nre.DelimitedValues = gfSettings.GetFilterPreference( "Amount" );
            //if ( nre.LowerValue.HasValue )
            //{
            //    qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) >= nre.LowerValue.Value );
            //}

            //if ( nre.UpperValue.HasValue )
            //{
            //    qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) <= nre.UpperValue.Value );
            //}

            //// Frequency
            //int? frequencyTypeId = gfSettings.GetFilterPreference( "Frequency" ).AsIntegerOrNull();
            //if ( frequencyTypeId.HasValue )
            //{
            //    qry = qry.Where( t => t.TransactionFrequencyValueId == frequencyTypeId.Value );
            //}

            //// Date Range
            //var drp = new DateRangePicker();
            //drp.DelimitedValues = gfSettings.GetFilterPreference( "Created" );
            //if ( drp.LowerValue.HasValue )
            //{
            //    qry = qry.Where( t => t.CreatedDateTime >= drp.LowerValue.Value );
            //}

            //if ( drp.UpperValue.HasValue )
            //{
            //    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
            //    qry = qry.Where( t => t.CreatedDateTime < upperDate );
            //}

            //// Account Id
            //int accountId = int.MinValue;
            //if ( int.TryParse( gfSettings.GetFilterPreference( "Account" ), out accountId ) && ddlAccount.Visible )
            //{
            //    qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => d.AccountId == accountId ) );
            //}

            //// Active only (no filter)
            //if ( string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Include Inactive" ) ) )
            //{
            //    qry = qry.Where( t => t.IsActive );
            //}

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
                        .ThenByDescending( t => t.FinancialScheduledTransaction.AuthorizedPersonAlias.Person )
                        .ThenByDescending( t => t.FinancialScheduledTransaction.AuthorizedPersonAlias.Person );
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialScheduledTransactionData> GetListQueryable( RockContext rockContext )
        {
            return GetScheduledTransactionQueryable( rockContext )
                .Select( a => new FinancialScheduledTransactionData
                {
                    FinancialScheduledTransaction = a
                } );
        }

        /// <inheritdoc/>
        protected override List<FinancialScheduledTransactionData> GetListItems( IQueryable<FinancialScheduledTransactionData> queryable, RockContext rockContext )
        {
            // Load all the scheduleTransaction into memory.
            var items = queryable.ToList();
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            // Translate the account summary data into a format that can be
            // sent to the client.
            foreach ( var item in items )
            {
                var accounts = item.FinancialScheduledTransaction.ScheduledTransactionDetails
                    .Select( d => new
                    {
                        Id = accountGuids.Any() && !accountGuids.Contains( d.Account.Guid ) ? 0 : d.AccountId,
                        Order = d.Account.Order,
                        Name = d.Account.Name,
                        Amount = d.Amount,
                        IsOther = accountGuids.Any() && !accountGuids.Contains( d.Account.Guid )
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

            return items;
        }



        /// <inheritdoc/>
        protected override GridBuilder<FinancialScheduledTransactionData> GetGridBuilder()
        {
            return new GridBuilder<FinancialScheduledTransactionData>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.FinancialScheduledTransaction.IdKey )
                .AddPersonField( "authorized", a => a.FinancialScheduledTransaction.AuthorizedPersonAlias?.Person )
                .AddTextField( "transactionFrequency", a => a.FinancialScheduledTransaction.TransactionFrequencyValue?.Value )
                .AddTextField( "transactionType", a => a.FinancialScheduledTransaction.TransactionTypeValue?.Value )
                .AddTextField( "gatewayScheduleId", a => a.FinancialScheduledTransaction.GatewayScheduleId )
                .AddField( "amount", a => a.FinancialScheduledTransaction.TotalAmount )
                .AddDateTimeField( "createdDateTime", a => a.FinancialScheduledTransaction.CreatedDateTime )
                .AddDateTimeField( "startDate", a => a.FinancialScheduledTransaction.StartDate )
                .AddDateTimeField( "endDate", a => a.FinancialScheduledTransaction.EndDate )
                .AddDateTimeField( "nextPayment", a => a.FinancialScheduledTransaction.NextPaymentDate )
                .AddTextField( "currencyType", a => a.FinancialScheduledTransaction.FinancialPaymentDetail.CurrencyTypeValue?.Value )
                .AddField( "accounts", a => a.Accounts )
                .AddField( "isActive", a => a.FinancialScheduledTransaction.IsActive );
        }

        /// <summary>
        /// Gets the person object.
        /// </summary>
        /// <returns>A <see cref="Person"/> object to use or <c>null</c> if we were unable to determine one.</returns>
        private Person GetPerson( RockContext rockContext )
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = RequestContext.GetPageParameter( "personId" );

            return new PersonService( rockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
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
