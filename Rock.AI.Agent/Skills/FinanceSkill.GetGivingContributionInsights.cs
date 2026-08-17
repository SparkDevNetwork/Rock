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

namespace Rock.AI.Agent.Skills;

internal sealed partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Gets the opinionated insights for financial transactions of type 'Event Contribution' that match the filters." )]
    [AgentToolGuid( "8AE2C3D2-6965-47E2-AC82-0D422A1EF2FC" )]
    [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
    [AgentUsage( "Only provide a personIdKey if the request is about a specific person. Do not assume that the current person should be used." )]
    [AgentToolReturnDescription( "Summary of matching transactions: count, total, average, median, and std-dev of per-transaction amounts. Includes fund and payment-type breakdowns with amount, share of total, and contributing-transaction counts." )]
    public AgentToolResult GetGivingContributionInsights(
        string personIdKey = null,
        string campusIdKey = null,
        List<string> accountIdKeys = null,
        string paymentMethodTypeValueIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid(), AgentRequestContext.RockContext ).Id;
        var qry = new FinancialTransactionService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( ft => ft.TransactionTypeValueId == contributionTransactionValueId );

        qry = WherePersonOrGivingGroup( qry, helper, personIdKey );

        qry = helper.WhereOptionalIdKey( qry, ft => ft.Batch.CampusId, campusIdKey );
        qry = helper.WhereOptionalIdKey( qry, ft => ft.FinancialPaymentDetail.CurrencyTypeValueId, paymentMethodTypeValueIdKey );
        qry = helper.WhereOptionalPropertyBetween( qry, ft => ft.TransactionDateTime, startDate, endDate );

        if ( !TryGetMatchingAccountIds( AgentRequestContext, accountIdKeys, campusIdKey, out var accountIds ) )
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
        var txAgg = qry
            .Select( t => new FinancialInsightsAggregateRow
            {
                Id = t.Id,
                TransactionDateTime = t.TransactionDateTime,
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
            } )
            .ToList();

        // Fund (account) rollup detail level honoring multi-account filter if provided.
        var detailProj = qry
            .SelectMany( t => t.TransactionDetails.Select( d => new FinancialInsightsDetailRow
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
