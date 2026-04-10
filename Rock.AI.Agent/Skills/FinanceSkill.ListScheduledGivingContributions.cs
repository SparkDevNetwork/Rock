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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Retrieves a list of scheduled giving contributions." )]
    [AgentPurpose( "Retrieves a list of scheduled giving contributions." )]
    [AgentUsage( "The startDate and endDate parameters refer to the next scheduled payment date." )]
    [AgentToolGuid( "174cf77d-e122-4d20-be35-fc09f081c663" )]
    public AgentToolResult ListScheduledGivingContributions(
        string personIdKey = null,
        string campusIdKey = null,
        List<string> accountIdKeys = null,
        string paymentMethodTypeValueIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1 )
    {
        // Require at least one filter, or punt to summarize tool.
        if ( personIdKey.IsNullOrWhiteSpace()
            && campusIdKey.IsNullOrWhiteSpace()
            && ( accountIdKeys == null || !accountIdKeys.Any() )
            && paymentMethodTypeValueIdKey.IsNullOrWhiteSpace()
            && !startDate.HasValue
            && !endDate.HasValue )
        {
            return Error( "At least one filter must be provided to list scheduled giving contributions." )
                .WithInstructions( $"Call the {nameof( GetScheduledGivingContributionInsights )} tool to get an aggregated form of the request." );
        }

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

        // If we have an account filter, only return transactions that actually contribute (>0)
        // to one of the filtered accounts (mirror Summarize's "effective" set).
        if ( hasAccountFilter )
        {
            qry = qry.Where( fst => fst.ScheduledTransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
        }

        qry = qry.OrderByDescending( fst => fst.NextPaymentDate )
            .ThenByDescending( fst => fst.Id );

        var projectedQry = qry.AsExpandable().Select( fst => new FinancialScheduledTransactionResult
        {
            Id = fst.Id,
            AuthorizedPerson = PersonResult.NameOnly( fst.AuthorizedPersonAlias ),
            NextPaymentDate = fst.NextPaymentDate,

            // Only sum details that match the resolved account set (if any)
            TotalAmount = fst.ScheduledTransactionDetails
                .Where( d => !hasAccountFilter || accountIds.Contains( d.AccountId ) )
                .Sum( d => ( decimal? ) d.Amount ) ?? 0m,

            // And only list those matching account details
            Accounts = fst.ScheduledTransactionDetails
                .Where( td => !hasAccountFilter || accountIds.Contains( td.AccountId ) )
                .Select( td => new FinancialAccountTransactionResult
                {
                    Amount = td.Amount,
                    Name = td.Account.Name,
                    EntityTypeId = td.EntityTypeId,
                    EntityId = td.EntityId,
                } )
                .ToList(),

            CurrencyType = fst.FinancialPaymentDetail.CurrencyTypeValue != null
                ? new KeyNameResult
                {
                    Id = fst.FinancialPaymentDetail.CurrencyTypeValue.Id,
                    Name = fst.FinancialPaymentDetail.CurrencyTypeValue.Value
                }
                : null,

            CreditCardType = fst.FinancialPaymentDetail.CreditCardTypeValue != null
                ? new KeyNameResult
                {
                    Id = fst.FinancialPaymentDetail.CreditCardTypeValue.Id,
                    Name = fst.FinancialPaymentDetail.CreditCardTypeValue.Value
                }
                : null,

            Frequency = fst.TransactionFrequencyValue.Value,
        } );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var page = helper.GetPaginatedItems( projectedQry, pageNumber );

        var result = helper.GetPaginatedResult( page );

        if ( personIdKey.IsNotNullOrWhiteSpace() )
        {
            result = result.WithInstructions( "Note: This list may include scheduled contributions that will be made by other people in the same giving group as the specified person. This is typically the same family, but not always." );
        }

        return result;
    }

    #endregion
}
