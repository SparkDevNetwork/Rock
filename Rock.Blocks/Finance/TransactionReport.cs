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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.TransactionReport;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Reports transactions for the currently logged-in person (or a context person) with date
    /// range and account filters.
    /// </summary>
    [DisplayName( "Transaction Report" )]
    [Category( "Finance" )]
    [Description( "Reports transactions for the currently logged in person, with date range and account filters." )]
    [IconCssClass( "ti ti-credit-card" )]

    #region Block Attributes

    [TextField( "Transaction Label",
        Description = "The label to use to describe the transactions (e.g. 'Gifts', 'Donations', etc.)",
        IsRequired = true,
        DefaultValue = "Gifts",
        Order = 0,
        Key = AttributeKey.TransactionLabel )]

    [TextField( "Account Label",
        Description = "The label to use to describe accounts.",
        IsRequired = true,
        DefaultValue = "Accounts",
        Order = 1,
        Key = AttributeKey.AccountLabel )]

    [AccountsField( "Accounts",
        Description = "List of accounts to allow the person to view. When configured, the report is limited to these accounts; when left empty, all accounts the person contributed to are shown.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.Accounts )]

    [BooleanField( "Show Transaction Code",
        Description = "Show the transaction code column in the table.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.ShowTransactionCode )]

    [BooleanField( "Show Foreign Key",
        Description = "Show the transaction foreign key column in the table.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.ShowForeignKey )]

    [DefinedValueField( "Transaction Types",
        Description = "Optional list of transaction types to limit the list to (if none are selected all types will be included).",
        IsRequired = false,
        AllowMultiple = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        Order = 5,
        Key = AttributeKey.TransactionTypes )]

    [BooleanField( "Use Person Context",
        Description = "Determines if the person context should be used instead of the CurrentPerson.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.UsePersonContext )]

    #endregion Block Attributes

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "E079FE57-E450-42B8-815F-477F533DBD6C" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "ADE82BC9-F3A8-4D34-996F-5A6DC7383F93" )]
    [Rock.SystemGuid.BlockTypeGuid( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D" )]
    public class TransactionReport : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TransactionLabel = "TransactionLabel";
            public const string AccountLabel = "AccountLabel";
            public const string Accounts = "Accounts";
            public const string ShowTransactionCode = "ShowTransactionCode";
            public const string ShowForeignKey = "ShowForeignKey";
            public const string TransactionTypes = "TransactionTypes";
            public const string UsePersonContext = "UsePersonContext";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            // The default range is the current year to date, matching the originally requested behavior.
            var defaultLowerDate = new DateTime( RockDateTime.Now.Year, 1, 1 );
            var defaultUpperDate = RockDateTime.Now;

            return new CustomBlockBox<TransactionReportDataBag, TransactionReportOptionsBag>
            {
                Bag = new TransactionReportDataBag
                {
                    GridData = GetTransactionGridData( defaultLowerDate, defaultUpperDate, null )
                },
                Options = GetOptions( defaultLowerDate, defaultUpperDate )
            };
        }

        /// <summary>
        /// Builds the static display configuration sent to the client.
        /// </summary>
        /// <param name="defaultLowerDate">The default lower date applied to the date range filter on first load.</param>
        /// <param name="defaultUpperDate">The default upper date applied to the date range filter on first load.</param>
        /// <returns>The populated options bag.</returns>
        private TransactionReportOptionsBag GetOptions( DateTime defaultLowerDate, DateTime defaultUpperDate )
        {
            var currencyInfo = new RockCurrencyCodeInfo();

            return new TransactionReportOptionsBag
            {
                TransactionLabel = GetAttributeValue( AttributeKey.TransactionLabel ).Singularize(),
                AccountLabel = GetAttributeValue( AttributeKey.AccountLabel ),
                Accounts = GetConfiguredAccounts(),
                EmptyDataText = $"No {GetAttributeValue( AttributeKey.TransactionLabel ).ToLower()} found with the provided criteria.",
                ShowTransactionCode = GetAttributeValue( AttributeKey.ShowTransactionCode ).AsBoolean(),
                ShowForeignKey = GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean(),
                GridDefinition = GetGridBuilder().BuildDefinition(),
                DefaultLowerDate = defaultLowerDate.ToString( "yyyy-MM-dd" ),
                DefaultUpperDate = defaultUpperDate.ToString( "yyyy-MM-dd" ),
                CurrencyInfo = new CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                }
            };
        }

        /// <summary>
        /// Gets the person whose transactions should be reported: the context person when the block
        /// is configured to use person context, otherwise the current person.
        /// </summary>
        /// <returns>The target person, or <c>null</c> when no person is available.</returns>
        private Person GetTargetPerson()
        {
            if ( GetAttributeValue( AttributeKey.UsePersonContext ).AsBoolean() )
            {
                return RequestContext.GetContextEntity<Person>();
            }

            return RequestContext.CurrentPerson;
        }

        /// <summary>
        /// Gets the accounts configured as the viewable whitelist for the block, ordered for display.
        /// These pre-select the account filter on the client. Returns an empty list when the block is
        /// not restricted to a specific set of accounts.
        /// </summary>
        /// <returns>The configured accounts as list items, where the value is the account's unique identifier.</returns>
        private List<ListItemBag> GetConfiguredAccounts()
        {
            var configuredAccountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();

            return FinancialAccountCache.GetByGuids( configuredAccountGuids )
                .OrderBy( a => a.Order )
                .Select( a => new ListItemBag
                {
                    Value = a.Guid.ToString(),
                    Text = a.PublicName
                } )
                .ToList();
        }

        /// <summary>
        /// Resolves the account ids the report should be filtered to, honoring the optional "Accounts"
        /// whitelist configured on the block.
        /// </summary>
        /// <returns>
        /// The account ids to filter by, or <c>null</c> when no account filter should be applied (no
        /// accounts are configured on the block).
        /// </returns>
        private List<int> GetEffectiveAccountIds( List<Guid> selectedAccountGuids )
        {
            var configuredAccountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();

            // No whitelist configured: the filter is hidden and every contributed account is included.
            if ( !configuredAccountGuids.Any() )
            {
                return null;
            }

            var checkedGuids = selectedAccountGuids ?? configuredAccountGuids;
            var allowedGuids = checkedGuids.Where( guid => configuredAccountGuids.Contains( guid ) ).ToList();

            return FinancialAccountCache.GetByGuids( allowedGuids ).Select( a => a.Id ).ToList();
        }

        /// <summary>
        /// Gets the grid builder that defines the columns and field values for the transactions grid.
        /// </summary>
        /// <returns>An instance of <see cref="GridBuilder{T}"/>.</returns>
        private GridBuilder<TransactionReportRow> GetGridBuilder()
        {
            return new GridBuilder<TransactionReportRow>()
                .WithLaunchWorkflow( this )
                .AddField( "id", r => r.Id )
                .AddDateTimeField( "transactionDateTime", r => r.TransactionDateTime )
                .AddTextField( "currencyType", r => r.CurrencyType )
                .AddTextField( "transactionCode", r => r.TransactionCode )
                .AddTextField( "foreignKey", r => r.ForeignKey )
                .AddField( "summary", r => r.Summary )
                .AddField( "totalAmount", r => r.TotalAmount );
        }

        /// <summary>
        /// Queries the transactions for the target person's giving unit and builds the grid data for
        /// the supplied filter selections.
        /// </summary>
        /// <param name="lowerDate">The inclusive lower bound of the transaction date range, or <c>null</c>.</param>
        /// <param name="upperDate">The upper bound of the transaction date range, or <c>null</c>.</param>
        /// <param name="accountGuids">The accounts to filter by; <c>null</c> or empty includes all accounts.</param>
        /// <returns>The grid data describing the matching transactions.</returns>
        private GridDataBag GetTransactionGridData( DateTime? lowerDate, DateTime? upperDate, List<Guid> accountGuids )
        {
            var targetPerson = GetTargetPerson();

            if ( targetPerson == null )
            {
                return GetGridBuilder().Build( new List<TransactionReportRow>() );
            }

            // Scope to every alias in the target person's giving unit so a family's giving is reported together.
            var givingId = targetPerson.GivingId;
            var personAliasIdQuery = new PersonAliasService( RockContext ).Queryable()
                .Where( a => a.Person.GivingId == givingId )
                .Select( a => a.Id );

            var transactionQuery = new FinancialTransactionService( RockContext )
                .Queryable( "TransactionDetails,FinancialPaymentDetail" )
                .Where( t => t.AuthorizedPersonAliasId.HasValue
                    && personAliasIdQuery.Contains( t.AuthorizedPersonAliasId.Value ) );

            // Apply the account filter, honoring the optional "Accounts" whitelist configured on the block.
            var accountIds = GetEffectiveAccountIds( accountGuids );

            if ( accountIds != null )
            {
                transactionQuery = transactionQuery.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
            }

            if ( lowerDate.HasValue )
            {
                var lowerDateValue = lowerDate.Value;
                transactionQuery = transactionQuery.Where( t => t.TransactionDateTime >= lowerDateValue );
            }

            if ( upperDate.HasValue )
            {
                // Add a day so the entire upper day is included.
                var endDate = upperDate.Value.Date.AddDays( 1 );
                transactionQuery = transactionQuery.Where( t => t.TransactionDateTime < endDate );
            }

            var transactionTypeIds = GetAttributeValue( AttributeKey.TransactionTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( guid => DefinedValueCache.GetId( guid ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            if ( transactionTypeIds.Any() )
            {
                transactionQuery = transactionQuery.Where( t => transactionTypeIds.Contains( t.TransactionTypeValueId ) );
            }

            var rows = transactionQuery
                .OrderByDescending( t => t.TransactionDateTime )
                .ToList()
                .Select( t => new TransactionReportRow
                {
                    Id = t.Id,
                    TransactionDateTime = t.TransactionDateTime,
                    CurrencyType = FormatCurrencyType( t ),
                    TransactionCode = t.TransactionCode,
                    ForeignKey = t.ForeignKey,
                    Summary = t.TransactionDetails
                        .Select( d => new TransactionReportAccountSummaryBag
                        {
                            Name = FinancialAccountCache.Get( d.AccountId )?.PublicName,
                            Amount = d.Amount
                        } )
                        .ToList(),
                    TotalAmount = t.TotalAmount
                } )
                .ToList();

            return GetGridBuilder().Build( rows );
        }

        /// <summary>
        /// Formats the currency type for a transaction as "Currency Type - Credit Card Type", or just
        /// the currency type when there is no associated credit card type.
        /// </summary>
        /// <param name="transaction">The transaction to format.</param>
        /// <returns>The formatted currency type, or an empty string when none is available.</returns>
        private string FormatCurrencyType( FinancialTransaction transaction )
        {
            var paymentDetail = transaction.FinancialPaymentDetail;

            if ( paymentDetail?.CurrencyTypeValueId == null )
            {
                return string.Empty;
            }

            var currencyType = DefinedValueCache.Get( paymentDetail.CurrencyTypeValueId.Value )?.Value ?? string.Empty;

            if ( paymentDetail.CreditCardTypeValueId.HasValue )
            {
                var creditCardType = DefinedValueCache.Get( paymentDetail.CreditCardTypeValueId.Value )?.Value ?? string.Empty;
                return $"{currencyType} - {creditCardType}";
            }

            return currencyType;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the transaction grid data for the supplied filter selections.
        /// </summary>
        /// <param name="filterBag">The filter selections to apply.</param>
        /// <returns>An action result containing the transaction data.</returns>
        [BlockAction]
        public BlockActionResult GetTransactionData( TransactionReportFilterBag filterBag )
        {
            var lowerDate = filterBag?.LowerDate.AsDateTime();
            var upperDate = filterBag?.UpperDate.AsDateTime();

            var accountGuids = filterBag?.AccountGuids?
                .Select( value => value.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => guid.Value )
                .ToList();

            return ActionOk( new TransactionReportDataBag
            {
                GridData = GetTransactionGridData( lowerDate, upperDate, accountGuids )
            } );
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <remarks>
        /// This block derives from <see cref="RockBlockType"/> rather than
        /// <see cref="RockListBlockType{T}"/>, so it does not inherit the standard
        /// entity-set action. It is provided here so the Launch Workflow grid action
        /// can operate on the selected transactions.
        /// </remarks>
        /// <param name="entitySet">The bag that describes the entity set to create.</param>
        /// <returns>An action result that contains the identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        #endregion Block Actions

        #region Helper Classes

        /// <summary>
        /// A single row in the transactions grid.
        /// </summary>
        private class TransactionReportRow
        {
            /// <summary>
            /// Gets or sets the transaction identifier (grid key).
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date and time of the transaction.
            /// </summary>
            public DateTime? TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the formatted currency type (and credit card type when present).
            /// </summary>
            public string CurrencyType { get; set; }

            /// <summary>
            /// Gets or sets the transaction code.
            /// </summary>
            public string TransactionCode { get; set; }

            /// <summary>
            /// Gets or sets the transaction foreign key.
            /// </summary>
            public string ForeignKey { get; set; }

            /// <summary>
            /// Gets or sets the per-account breakdown of the transaction.
            /// </summary>
            public List<TransactionReportAccountSummaryBag> Summary { get; set; }

            /// <summary>
            /// Gets or sets the total amount of the transaction.
            /// </summary>
            public decimal TotalAmount { get; set; }
        }

        #endregion Helper Classes
    }
}
