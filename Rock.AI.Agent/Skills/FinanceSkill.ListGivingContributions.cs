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

using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Lists transactions of type 'Contribution' that match the provided filters." )]
    [AgentToolGuid( "20FF0B2E-E403-48CE-B0C9-0CB6D80A7291" )]
    public IAgentToolResult ListGivingContributions(
        string personIdKey = null,
        string campusIdKey = null,
        List<string> accountIdKeys = null,
        string paymentMethodTypeValueIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1 )
    {
        // When accountIdKeys and/or campusIdKey are provided, only transactions
        // that contribute (>0) to the resolved set of accounts are returned,
        // and the per-row TotalAmount/Accounts reflect only those contributing
        // details (same behavior as summarize).

        // Require at least one filter, or punt to summarize tool.
        if ( personIdKey.IsNullOrWhiteSpace()
            && campusIdKey.IsNullOrWhiteSpace()
            && ( accountIdKeys == null || !accountIdKeys.Any() )
            && paymentMethodTypeValueIdKey.IsNullOrWhiteSpace()
            && !startDate.HasValue
            && !endDate.HasValue )
        {
            return Error( "At least one filter must be provided to list financial transactions." )
                .WithInstructions( $"Call the {nameof( GetGivingContributionInsights )} tool to get an aggregated form of the request." );
        }

        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid(), AgentRequestContext.RockContext ).Id;
        var qry = new FinancialTransactionService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( ft => ft.TransactionTypeValueId == contributionTransactionValueId );

        qry = WherePersonOrGivingGroup( qry, helper, personIdKey );

        var result = GetFinancialTransactionResult( helper, AgentRequestContext, qry, campusIdKey, accountIdKeys, paymentMethodTypeValueIdKey, startDate, endDate, pageNumber, null );

        if ( !helper.HasErrors && personIdKey.IsNotNullOrWhiteSpace() )
        {
            result = result.WithInstructions( "Note: This list may include transactions made by other people in the same giving group as the specified person. This is typically the same family, but not always." );
        }

        return result;
    }

    #endregion
}
