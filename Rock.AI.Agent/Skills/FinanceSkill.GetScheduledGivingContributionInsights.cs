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
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        [Description( "Retrieves a list of scheduled giving contributions." )]
        [AgentPurpose( "Retrieves a list of scheduled giving contributions." )]
        [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
        [AgentUsage( "The startDate and endDate parameters refer to the next scheduled payment date." )]
        [AgentToolGuid( "8d789711-32f2-474f-b89b-fa8d3b718fad" )]
        public IAgentToolResult GetScheduledGivingContributionInsights(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid(), AgentRequestContext.RockContext ).Id;
            var qry = new FinancialScheduledTransactionService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( fst => fst.TransactionTypeValueId == contributionTransactionValueId
                    && fst.IsActive );

            qry = WherePersonOrGivingGroup( qry, helper, personIdKey );

            qry = helper.WhereOptionalIdKey( qry, fst => fst.FinancialPaymentDetail.CurrencyTypeValueId, paymentMethodTypeValueIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, fst => fst.NextPaymentDate, startDate, endDate );

            if ( !TryGetMatchingAccountIds( AgentRequestContext, accountIdKeys, campusIdKey, out var accountIds ) )
            {
                return AgentToolResult.NoData()
                    .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
            }

            var hasAccountFilter = accountIds.Any();

            // Project per-transaction, with detail amount filtered by provided account ids if any.
            var txAgg = qry
                .Select( fst => new FinancialInsightsAggregateRow
                {
                    Id = fst.Id,
                    CurrencyTypeId = fst.FinancialPaymentDetail.CurrencyTypeValueId,
                    CurrencyType = fst.FinancialPaymentDetail.CurrencyTypeValue != null
                        ? fst.FinancialPaymentDetail.CurrencyTypeValue.Value
                        : "Unknown",
                    Frequency = fst.TransactionFrequencyValue.Value,
                    AmountFiltered = hasAccountFilter
                        ? fst.ScheduledTransactionDetails
                            .Where( d => accountIds.Contains( d.AccountId ) )
                            .Sum( d => ( decimal? ) d.Amount ) ?? 0m
                        : fst.ScheduledTransactionDetails
                            .Sum( d => ( decimal? ) d.Amount ) ?? 0m,
                } )
                .ToList();

            // Fund (account) rollup detail level honoring multi-account filter if provided.
            var detailProj = qry
                .SelectMany( t => t.ScheduledTransactionDetails.Select( d => new FinancialInsightsDetailRow
                {
                    TransactionId = t.Id,
                    AccountId = ( int? ) d.AccountId,
                    AccountName = d.Account != null ? d.Account.Name : "Unknown",
                    Amount = ( decimal? ) d.Amount ?? 0m
                } ) )
                .Where( x => x.AccountId != null && ( !hasAccountFilter || accountIds.Contains( x.AccountId.Value ) ) );

            var insightsResult = GetTransactionInsights( txAgg, detailProj, accountIds );

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
