using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Data;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        /// <summary>
        /// Produces an analytic summary of financial transactions matching the supplied optional filters.
        /// Includes descriptive statistics (count, total, mean, median, standard deviation) and breakdowns by
        /// fund (account) and payment method (currency type). When a *ValueIdKey argument equals "lookup" an
        /// instructional error is returned containing selectable values instead of analytics.
        /// </summary>
        /// <param name="personIdKey">Optional Person IdKey to restrict to transactions authorized by that person.</param>
        /// <param name="campusIdKey">Optional Campus (Batch Campus) IdKey.</param>
        /// <param name="accountIdKeys">Optional Account/Fund IdKey. When supplied only amounts contributed to this fund are counted in statistics.</param>
        /// <param name="paymentMethodTypeValueIdKey">Optional currency / tender defined value IdKey or the literal "lookup".</param>
        /// <param name="startDate">Inclusive start date filter.</param>
        /// <param name="endDate">Inclusive end date filter.</param>
        /// <returns>Analytics wrapped in <see cref="FinancialTransactionSummaryResult"/>.</returns>
        [AgentToolGuid( "8AE2C3D2-6965-47E2-AC82-0D422A1EF2FC" )]
        [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
        [AgentUsage( "Only provide a personIdKey if the request is about a specific person. Do not assume that the current person should be used." )]
        [AgentToolReturnDescription( "Summary of matching transactions: count, total, average, median, and std-dev of per-transaction amounts. Includes fund and payment-type breakdowns with amount, share of total, and contributing-transaction counts." )]
        public RockToolResult SummarizeFinancialTransactions(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();

            // Handle "lookup" for currency type defined values.
            if ( TryGetDefinedValueLookup( rockContext, Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, paymentMethodTypeValueIdKey ) is List<KeyNameResult> lookups )
            {
                return RockToolResult.Error( "Lookups Required" )
                    .WithContent( lookups )
                    .WithInstructions( "Use the following data to determine the proper IdKey for the tool." );
            }

            // Decode IdKeys.
            var personId = personIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( personIdKey ) : null;
            var campusId = campusIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( campusIdKey ) : null;
            var paymentMethodTypeId = paymentMethodTypeValueIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( paymentMethodTypeValueIdKey ) : null;


            var options = new FinancialTransactionQueryOptions
            {
                PersonId = personId,
                //BatchCampusId = campusId,
                PaymentMethodTypeId = paymentMethodTypeId,
                StartDate = startDate,
                EndDate = endDate
            };

            // Base transaction scope (no AccountId filter here on purpose).
            var baseQry = GetFinancialTransactionsQueryable( rockContext, options )
                .AsNoTracking();

            List<int> accountIds = new List<int>();

            if ( accountIdKeys?.Any() ?? false || campusIdKey.IsNotNullOrWhiteSpace() )
            {
                accountIds = GetFinancialAccountsForQuery( accountIdKeys ?? new List<string>(), campusIdKey, rockContext )
                    .Select( a => a.Id )
                    .ToList();

                if ( !accountIds.Any() )
                {
                    return RockToolResult.NoData()
                        .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
                }
            }

            var hasAccountFilter = accountIds?.Any() == true;

            // Project per-transaction, with detail amount filtered by provided account ids if any.
            var txAggQry = baseQry.Select( t => new
            {
                t.Id,
                t.TransactionDateTime,
                CurrencyTypeId = ( int? ) t.FinancialPaymentDetail.CurrencyTypeValueId,
                CurrencyType = t.FinancialPaymentDetail.CurrencyTypeValue != null
                    ? t.FinancialPaymentDetail.CurrencyTypeValue.Value
                    : "Unknown",
                AmountFiltered = ( hasAccountFilter
                    ? t.TransactionDetails
                        .Where( d => accountIds.Contains( d.AccountId ) )
                        .Select( d => ( decimal? ) d.Amount )
                        .Sum()
                    : t.TransactionDetails
                        .Select( d => ( decimal? ) d.Amount )
                        .Sum() ) ?? 0m
            } );

            // Materialize once for stats and currency breakdown.
            var txAgg = txAggQry.ToList();

            // Effective set for stats: if filtering by accounts only include transactions that contributed (>0), else all.
            var effectiveAmounts = ( hasAccountFilter
                ? txAgg.Where( x => x.AmountFiltered > 0m )
                : txAgg ).Select( x => x.AmountFiltered ).ToList();

            var uniqueTransactionCount = effectiveAmounts.Count;
            var totalAmount = effectiveAmounts.Sum();
            decimal averageAmount = 0m, medianAmount = 0m, stdDeviationAmount = 0m;

            if ( uniqueTransactionCount > 0 )
            {
                averageAmount = decimal.Round( effectiveAmounts.Average(), 2 );
                var ordered = effectiveAmounts.OrderBy( a => a ).ToList();
                var mid = ordered.Count / 2;
                medianAmount = ordered.Count % 2 == 1
                    ? ordered[mid]
                    : decimal.Round( ( ordered[mid - 1] + ordered[mid] ) / 2m, 2 );
                var meanD = ( double ) averageAmount;
                var variance = ordered.Sum( a => Math.Pow( ( double ) a - meanD, 2 ) ) / ordered.Count;
                stdDeviationAmount = ( decimal ) Math.Round( Math.Sqrt( variance ), 2 );
            }

            // Fund (account) rollup detail level honoring multi-account filter if provided.
            var detailProj = baseQry
                .SelectMany( t => t.TransactionDetails.Select( d => new
                {
                    TransactionId = t.Id,
                    AccountId = ( int? ) d.AccountId,
                    AccountName = d.Account != null ? d.Account.Name : "Unknown",
                    Amount = ( decimal? ) d.Amount ?? 0m
                } ) )
                .Where( x => x.AccountId != null && ( !hasAccountFilter || accountIds.Contains( x.AccountId.Value ) ) );

            var fundRows = detailProj
                .GroupBy( x => new { x.AccountId, x.AccountName } )
                .Select( g => new
                {
                    g.Key.AccountId,
                    g.Key.AccountName,
                    TotalAmount = g.Sum( x => x.Amount ),
                    UniqueTransactionCount = g.Select( x => x.TransactionId ).Distinct().Count()
                } )
                .OrderByDescending( x => x.TotalAmount )
                .ToList();

            var denom = totalAmount == 0m ? 1m : totalAmount;
            var funds = fundRows.Select( fr => new FundBreakdown
            {
                IdKey = IdHasher.Instance.GetHash( fr.AccountId!.Value ),
                Name = fr.AccountName ?? "Unknown",
                TotalAmount = fr.TotalAmount,
                PercentOfTotal = fr.TotalAmount / denom,
                UniqueTransactionCount = fr.UniqueTransactionCount
            } ).ToList();

            // Currency/tender breakdown — count only contributing (>0) transactions.
            var currencyTypeRows = txAgg
                .GroupBy( x => new { x.CurrencyTypeId, x.CurrencyType } )
                .Select( g => new
                {
                    Type = g.Key.CurrencyType ?? "Unknown",
                    UniqueTransactionCount = g.Count( x => x.AmountFiltered > 0m ),
                    TotalAmount = g.Where( x => x.AmountFiltered > 0m ).Sum( x => x.AmountFiltered )
                } )
                .OrderByDescending( r => r.TotalAmount )
                .ToList();

            var currencyTypes = currencyTypeRows.Select( r => new CurrencyTypeBreakdown
            {
                Type = r.Type,
                UniqueTransactionCount = r.UniqueTransactionCount,
                TotalAmount = r.TotalAmount,
                PercentOfTotal = totalAmount == 0m ? 0m : ( r.TotalAmount / totalAmount )
            } ).ToList();

            var result = new FinancialTransactionSummaryResult
            {
                Currency = "USD",
                Totals = new FinancialTotalsBreakdown
                {
                    UniqueTransactionCount = uniqueTransactionCount,
                    TotalAmount = totalAmount,
                    AverageAmountPerTransaction = averageAmount,
                    MedianAmountPerTransaction = medianAmount,
                    StandardDeviationAmountPerTransaction = stdDeviationAmount
                },
                Funds = funds,
                CurrencyTypes = currencyTypes
            };

            return RockToolResult.Success( result );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns the list of <see cref="DefinedValue"/> items for the specified defined type when the supplied
        /// <paramref name="lookupKey"/> equals the literal "lookup". Otherwise returns <c>null</c> so the caller
        /// knows to continue normal processing.
        /// </summary>
        /// <param name="rockContext">The Rock data context.</param>
        /// <param name="definedTypeGuid">The defined type Guid (as string) to resolve.</param>
        /// <param name="lookupKey">The user supplied value which may request a lookup.</param>
        /// <returns>A collection of <see cref="KeyNameResult"/> for selection or <c>null</c>.</returns>
        private List<KeyNameResult> TryGetDefinedValueLookup( RockContext rockContext, string definedTypeGuid, string lookupKey )
        {
            if ( lookupKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( !lookupKey.Equals( "lookup", StringComparison.OrdinalIgnoreCase ) )
            {
                return null;
            }

            var paymentMethodDvs = DefinedTypeCache.Get( definedTypeGuid.AsGuid(), rockContext )
                ?.DefinedValues
                .Select( dv => new KeyNameResult
                {
                    IdKey = dv.IdKey,
                    Name = dv.Value
                } )
                .ToList();

            return paymentMethodDvs;
        }

        #endregion
    }
}
