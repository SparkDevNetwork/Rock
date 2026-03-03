using System;
using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        /// <summary>
        /// Produces an analytic summary of giving financial transactions
        /// matching the supplied filters.
        /// </summary>
        /// <param name="personIdKey">Encoded person identifier.</param>
        /// <param name="campusIdKey">Encoded campus identifier.</param>
        /// <param name="accountIdKeys">Encoded account identifiers.</param>
        /// <param name="paymentMethodTypeValueIdKey">Encoded payment method identifier,</param>
        /// <param name="startDate">The start date to limit results to.</param>
        /// <param name="endDate">The end date to limit results to.</param>
        /// <returns>Analytics wrapped in <see cref="FinancialTransactionInsightsResult"/>.</returns>
        [AgentToolGuid( "8AE2C3D2-6965-47E2-AC82-0D422A1EF2FC" )]
        [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
        [AgentUsage( "Only provide a personIdKey if the request is about a specific person. Do not assume that the current person should be used." )]
        [AgentToolReturnDescription( "Summary of matching transactions: count, total, average, median, and std-dev of per-transaction amounts. Includes fund and payment-type breakdowns with amount, share of total, and contributing-transaction counts." )]
        public IAgentToolResult GetGivingContributionInsights(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid(), AgentRequestContext.RockContext ).Id;
            var qry = new FinancialTransactionService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( ft => ft.TransactionTypeValueId == contributionTransactionValueId );

            qry = WherePersonOrGivingGroup( qry, helper, personIdKey );

            qry = helper.WhereOptionalIdKey( qry, ft => ft.Batch.CampusId, campusIdKey );
            qry = helper.WhereOptionalIdKey( qry, ft => ft.FinancialPaymentDetail.CurrencyTypeValueId, paymentMethodTypeValueIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, ft => ft.TransactionDateTime, startDate, endDate );

            if ( !TryGetMatchingAccountIds( accountIdKeys, campusIdKey, out var accountIds ) )
            {
                return NoData()
                    .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var hasAccountFilter = accountIds.Any();

            // Project per-transaction, with detail amount filtered by provided account ids if any.
            var txAggQry = qry.Select( t => new
            {
                t.Id,
                t.TransactionDateTime,
                CurrencyTypeId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                CurrencyType = t.FinancialPaymentDetail.CurrencyTypeValue != null
                    ? t.FinancialPaymentDetail.CurrencyTypeValue.Value
                    : "Unknown",
                AmountFiltered = hasAccountFilter
                    ? t.TransactionDetails
                        .Where( d => accountIds.Contains( d.AccountId ) )
                        .Sum( d => ( decimal? ) d.Amount ) ?? 0m
                    : t.TransactionDetails
                        .Sum( d => ( decimal? ) d.Amount ) ?? 0m,
            } );

            // Materialize once for stats and currency breakdown.
            var txAgg = txAggQry.ToList();

            // Effective set for stats: if filtering by accounts only include transactions that contributed (>0), else all.
            var effectiveAmounts = hasAccountFilter
                ? txAgg.Where( x => x.AmountFiltered > 0m ).Select( x => x.AmountFiltered ).ToList()
                : txAgg.Select( x => x.AmountFiltered ).ToList();

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
            var detailProj = qry
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
            var funds = fundRows.Select( fr => new CurrencyBreakdown
            {
                IdKey = IdHasher.Instance.GetHash( fr.AccountId!.Value ),
                Name = fr.AccountName ?? "Unknown",
                TotalAmount = fr.TotalAmount,
                PercentOfTotal = fr.TotalAmount / denom * 100,
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

            var currencyTypes = currencyTypeRows.Select( r => new CurrencyBreakdown
            {
                Name = r.Type,
                UniqueTransactionCount = r.UniqueTransactionCount,
                TotalAmount = r.TotalAmount,
                PercentOfTotal = totalAmount == 0m ? 0m : ( r.TotalAmount / totalAmount * 100 )
            } ).ToList();

            var insightsResult = new FinancialTransactionInsightsResult
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

            var result = Success( insightsResult )
                .WithInstructions( "Percents are scaled between 0 and 100." );

            if ( !helper.HasErrors && personIdKey.IsNotNullOrWhiteSpace() )
            {
                result = result.WithInstructions( "Note: These insights may include transactions made by other people in the same giving group as the specified person. This is typically the same family, but not always." );
            }

            return result;
        }

        #endregion
    }
}
