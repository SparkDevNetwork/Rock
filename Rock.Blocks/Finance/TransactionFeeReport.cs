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
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.TransactionFeeReport;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Reports the processing-fee coverage collected over a date range, split by payment type.
    /// </summary>
    [DisplayName( "Transaction Fee Report" )]
    [Category( "Finance" )]
    [Description( "Block that reports transaction fees." )]
    [IconCssClass( "ti ti-file-dollar" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "06DEA3D2-6C2F-471B-BA0A-880E5B3A7BF8" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A83896E0-A4D4-4BF9-86A8-04917F9A2EE5" )]
    [Rock.SystemGuid.BlockTypeGuid( "D75AF7AE-94B8-4604-B768-A124A2F55449" )]
    public class TransactionFeeReport : RockBlockType
    {
        #region Keys

        private static class PreferenceKey
        {
            public const string Accounts = "AccountIds";
            public const string DateRange = "SlidingDateRangeDelimitedValues";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the accounts selected in the filter, resolved from saved person preferences. An empty
        /// list means all accounts.
        /// </summary>
        private List<FinancialAccountCache> FilterAccounts => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.Accounts )
            .SplitDelimitedValues()
            .Select( GetAccountOrNull )
            .Where( account => account != null )
            .ToList();

        /// <summary>
        /// Gets the sliding date range selected in the filter, from saved person preferences.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.DateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// The default sliding date range (last 3 months) applied when the individual has no saved preference.
        /// </summary>
        private static SlidingDateRangeBag DefaultDateRange => new SlidingDateRangeBag
        {
            RangeType = SlidingDateRangeType.Last,
            TimeUnit = TimeUnitType.Month,
            TimeValue = 3
        };

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CustomBlockBox<TransactionFeeReportBag, TransactionFeeReportOptionsBag>
            {
                Bag = GetReportResults(),
                Options = GetFilterOptions()
            };
        }

        /// <summary>
        /// Builds the restored filter selections and currency display info for the client.
        /// </summary>
        /// <returns>The populated options bag.</returns>
        private TransactionFeeReportOptionsBag GetFilterOptions()
        {
            var currencyInfo = new RockCurrencyCodeInfo();

            return new TransactionFeeReportOptionsBag
            {
                SelectedAccounts = FilterAccounts.ToListItemBagList(),
                DateRangeDelimitedValue = ( FilterDateRange ?? DefaultDateRange ).ToDelimitedSlidingDateRangeOrNull(),
                CurrencyInfo = new CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                }
            };
        }

        /// <summary>
        /// Computes the fee-coverage totals and counts for the saved filter selections.
        /// </summary>
        /// <returns>The populated report bag.</returns>
        private TransactionFeeReportBag GetReportResults()
        {
            var financialTransactionDetailService = new FinancialTransactionDetailService( RockContext );
            var qry = financialTransactionDetailService.Queryable();

            // Restrict to the selected date range, applying each bound only when it is present.
            var actualDateRange = FilterDateRange.Validate( DefaultDateRange ).ActualDateRange;

            if ( actualDateRange.Start.HasValue )
            {
                var startDateTime = actualDateRange.Start.Value;
                qry = qry.Where( a => a.Transaction.TransactionDateTime >= startDateTime );
            }

            if ( actualDateRange.End.HasValue )
            {
                var endDateTime = actualDateRange.End.Value;
                qry = qry.Where( a => a.Transaction.TransactionDateTime < endDateTime );
            }

            // Restrict to the selected accounts; no selection means all accounts.
            var selectedAccountIds = FilterAccounts.Select( a => a.Id ).ToList();
            if ( selectedAccountIds.Any() )
            {
                qry = qry.Where( a => selectedAccountIds.Contains( a.AccountId ) );
            }

            // Only include transactions that have at least one detail carrying a fee coverage amount.
            qry = qry.Where( a => a.Transaction.TransactionDetails.Any( x => x.FeeCoverageAmount.HasValue ) );

            // Aggregate the fee coverage per transaction in the database. Currency type is one-per-transaction,
            // so grouping by it alongside the transaction id produces the same groups as grouping by the
            // transaction id alone, while letting SQL do the summing.
            var totalsByTransaction = qry
                .GroupBy( a => new { a.TransactionId, a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId } )
                .Select( g => new
                {
                    g.Key.CurrencyTypeValueId,
                    FeeCoverageAmount = g.Sum( x => x.FeeCoverageAmount )
                } )
                .ToList();

            var currencyTypeIdCreditCard = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var currencyTypeIdAch = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );

            var creditCardTransactions = totalsByTransaction.Where( a => a.CurrencyTypeValueId == currencyTypeIdCreditCard ).ToList();
            var achTransactions = totalsByTransaction.Where( a => a.CurrencyTypeValueId == currencyTypeIdAch ).ToList();

            var creditCardCount = creditCardTransactions.Count;
            var creditCardFeeCoverageTotal = creditCardTransactions.Sum( a => a.FeeCoverageAmount ?? 0M );
            var achCount = achTransactions.Count;
            var achFeeCoverageTotal = achTransactions.Sum( a => a.FeeCoverageAmount ?? 0M );

            return new TransactionFeeReportBag
            {
                TotalFeeCoverageAmount = creditCardFeeCoverageTotal + achFeeCoverageTotal,
                CreditCardFeeCoverageAmount = creditCardFeeCoverageTotal,
                AchFeeCoverageAmount = achFeeCoverageTotal,
                TotalTransactionCount = creditCardCount + achCount,
                CreditCardTransactionCount = creditCardCount,
                AchTransactionCount = achCount
            };
        }

        /// <summary>
        /// Resolves a saved account preference token to its cache item. New selections store the
        /// account Guid; selections saved by the legacy WebForms block stored the integer Id under
        /// the same preference key, so an Id lookup is used as a fallback to preserve the filter.
        /// </summary>
        /// <param name="token">The saved account token to resolve.</param>
        /// <returns>The matching account, or <c>null</c> if the token does not resolve.</returns>
        private static FinancialAccountCache GetAccountOrNull( string token )
        {
            if ( Guid.TryParse( token, out var accountGuid ) )
            {
                return FinancialAccountCache.Get( accountGuid );
            }

            // Newly saved selections store the account Guid. Selections saved by the
            // legacy WebForms block stored the integer Id under this same preference
            // key, so fall back to an Id lookup to preserve a user's filter across
            // the conversion.
            if ( int.TryParse( token, out var accountId ) )
            {
                return FinancialAccountCache.Get( accountId );
            }

            return null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Recomputes the report for the individual's just-saved filter selections.
        /// </summary>
        /// <returns>An action result containing the report bag.</returns>
        [BlockAction]
        public BlockActionResult GetReportData()
        {
            return ActionOk( GetReportResults() );
        }

        #endregion Block Actions
    }
}
