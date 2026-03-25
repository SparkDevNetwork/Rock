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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        [Description( "Gets the opinionated insights for financial transactions of type 'Event Registration' that match the filters.")]
        [AgentToolGuid( "60f99c8b-627a-4075-9bed-8a967bbda239" )]
        [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
        [AgentUsage( "Only provide a personIdKey if the request is about a specific person. Do not assume that the current person should be used." )]
        [AgentToolReturnDescription( "Summary of matching transactions: count, total, average, median, and std-dev of per-transaction amounts. Includes fund and payment-type breakdowns with amount, share of total, and contributing-transaction counts." )]
        public IAgentToolResult GetEventRegistrationPaymentInsights(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var eventRegistrationValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid(), AgentRequestContext.RockContext ).Id;
            var qry = new FinancialTransactionDetailService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( ftd => ftd.Transaction.TransactionTypeValueId == eventRegistrationValueId );

            qry = helper.WhereOptionalIdKey( qry, ftd => ftd.Transaction.AuthorizedPersonAlias.PersonId, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, ftd => ftd.Transaction.Batch.CampusId, campusIdKey );
            qry = helper.WhereOptionalIdKey( qry, ftd => ftd.Transaction.FinancialPaymentDetail.CurrencyTypeValueId, paymentMethodTypeValueIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, ftd => ftd.Transaction.TransactionDateTime, startDate, endDate );

            if ( !TryGetMatchingAccountIds( AgentRequestContext, accountIdKeys, campusIdKey, out var accountIds ) )
            {
                return NoData()
                    .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( accountIds.Any() )
            {
                qry = qry.Where( ftd => accountIds.Contains( ftd.AccountId ) );
            }

            var hasAccountFilter = accountIds.Any();

            var groupCounts = qry
                .GroupBy( ftd => new
                {
                    ftd.Transaction.FinancialPaymentDetail.CurrencyTypeValueId,
                    ftd.Transaction.FinancialPaymentDetail.CreditCardTypeValueId,
                    ftd.AccountId,
                    ftd.EntityId,
                } )
                .Select( g => new
                {
                    g.Key.CurrencyTypeValueId,
                    g.Key.CreditCardTypeValueId,
                    g.Key.AccountId,
                    g.Key.EntityId,
                    Amount = g.Sum( d => d.Amount ),
                    TransactionCount = g.Select( d => d.TransactionId ).Distinct().Count(),
                    Count = g.Count(),
                } )
                .ToList();

            // Project per-detail to get averages.
            var txAggQry = qry.Select( ftd => new
            {
                ftd.TransactionId,
                ftd.Transaction.TransactionDateTime,
                ftd.Amount,
            } );

            // Materialize once for aggregate statistics.
            var txAgg = txAggQry.ToList();

            var uniqueTransactionCount = txAgg.Select( d => d.TransactionId ).Distinct().Count();
            var totalAmount = txAgg.Sum( d => d.Amount );
            var averageAmount = 0m;
            var medianAmount = 0m;
            var stdDeviationAmount = 0m;

            if ( uniqueTransactionCount > 0 )
            {
                var ordered = txAgg.Select( d => d.Amount ).OrderBy( a => a ).ToList();
                var mid = ordered.Count / 2;
                var meanD = ( double ) averageAmount;
                var variance = ordered.Sum( a => Math.Pow( ( double ) a - meanD, 2 ) ) / ordered.Count;

                averageAmount = decimal.Round( txAgg.Average( d => d.Amount ), 2 );
                medianAmount = ordered.Count % 2 == 1
                    ? ordered[mid]
                    : decimal.Round( ( ordered[mid - 1] + ordered[mid] ) / 2m, 2 );
                stdDeviationAmount = ( decimal ) Math.Round( Math.Sqrt( variance ), 2 );
            }

            var funds = groupCounts.GroupBy( c => c.AccountId )
                .Select( g => new CurrencyBreakdown
                {
                    IdKey = IdHasher.Instance.GetHash( g.Key ),
                    Name = FinancialAccountCache.Get( g.Key, AgentRequestContext.RockContext )?.Name ?? "Unknown",
                    TotalAmount = g.Sum( a => a.Amount ),
                    UniqueTransactionCount = g.Sum( a => a.TransactionCount ),
                    PercentOfTotal = totalAmount == 0m ? 0m : ( g.Sum( a => a.Amount ) / totalAmount * 100 ),
                } )
                .ToList();

            var currencyTypes = groupCounts.GroupBy( c => c.CurrencyTypeValueId )
                .Select( g => new CurrencyBreakdown
                {
                    IdKey = g.Key.HasValue ? IdHasher.Instance.GetHash( g.Key.Value ) : null,
                    Name = g.Key.HasValue
                        ? DefinedValueCache.Get( g.Key.Value, AgentRequestContext.RockContext )?.Value ?? "Unknown"
                        : "Unknown",
                    UniqueTransactionCount = g.Sum( a => a.TransactionCount ),
                    TotalAmount = g.Sum( a => a.Amount ),
                    PercentOfTotal = totalAmount == 0m ? 0m : ( g.Sum( a => a.Amount ) / totalAmount * 100 ),
                } )
                .ToList();

            var creditCardValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid(), AgentRequestContext.RockContext ).Id;
            var creditCardTotal = groupCounts.Where( c => c.CurrencyTypeValueId == creditCardValueId ).Sum( c => c.Amount );
            var creditCardTypes = groupCounts.Where( c => c.CurrencyTypeValueId == creditCardValueId )
                .GroupBy( c => c.CreditCardTypeValueId )
                .Select( g => new CurrencyBreakdown
                {
                    IdKey = g.Key.HasValue ? IdHasher.Instance.GetHash( g.Key.Value ) : null,
                    Name = g.Key.HasValue
                        ? DefinedValueCache.Get( g.Key.Value, AgentRequestContext.RockContext )?.Value ?? "Unknown"
                        : "Unknown",
                    UniqueTransactionCount = g.Sum( a => a.TransactionCount ),
                    TotalAmount = g.Sum( a => a.Amount ),
                    PercentOfTotalCreditCards = creditCardTotal == 0m ? 0m : ( g.Sum( a => a.Amount ) / creditCardTotal * 100 ),
                } )
                .ToList();

            var registrationInstanceIds = groupCounts.Where( c => c.EntityId.HasValue ).Select( c => c.EntityId.Value ).Distinct().ToList();
            var registrationNameLookup = new RegistrationInstanceService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( ri => registrationInstanceIds.Contains( ri.Id ) )
                .Select( ri => new
                {
                    ri.Id,
                    ri.Name
                } )
                .ToDictionary( r => r.Id, r => r.Name );

            var registrationInstances = groupCounts.GroupBy( c => c.EntityId )
                .Select( g => new CurrencyBreakdown
                {
                    IdKey = g.Key.HasValue ? IdHasher.Instance.GetHash( g.Key.Value ) : null,
                    Name = registrationNameLookup.TryGetValue( g.Key ?? 0, out var regName ) ? regName : "Unknown",
                    TotalAmount = g.Sum( a => a.Amount ),
                    UniqueTransactionCount = g.Sum( a => a.TransactionCount ),
                    PercentOfTotal = totalAmount == 0m ? 0m : ( g.Sum( a => a.Amount ) / totalAmount * 100 ),
                } )
                .ToList();

            var result = new FinancialTransactionInsightsResult
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
                CurrencyTypes = currencyTypes,
                CreditCardTypes = creditCardTypes,
                RegistrationInstances = registrationInstances,
            };

            return Success( result )
                .WithInstructions( "Percents are scaled between 0 and 100." );
        }

        #endregion
    }
}
