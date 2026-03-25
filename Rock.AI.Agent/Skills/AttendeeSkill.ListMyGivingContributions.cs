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
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class AttendeeSkill
{
    #region Tool(s)

    [Description( "Lists giving contributions for the currently logged in person." )]
    [AgentUsage( "Lists financial contribution records for the currently logged in user." )]
    [AgentToolGuid( "13c5b8b5-64b1-48b3-908e-5af8edce9767" )]
    public IAgentToolResult ListMyGivingContributions(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1 )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;

        if ( currentPerson == null )
        {
            return Error( "A user must be logged in to list their giving contributions." );
        }

        // Require at least one filter, or punt to summarize tool.
        if ( !startDate.HasValue
            && !endDate.HasValue )
        {
            return Error( "At least one filter must be provided to list giving contributions." );
        }

        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid(), AgentRequestContext.RockContext ).Id;
        var qry = new FinancialTransactionService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( ft => ft.TransactionTypeValueId == contributionTransactionValueId
                && ft.AuthorizedPersonAlias.PersonId == currentPerson.Id );

        var result = FinanceSkill.GetFinancialTransactionResult( helper, AgentRequestContext, qry, null, null, null, startDate, endDate, pageNumber, null );

        if ( !helper.HasErrors )
        {
            result = result.WithInstructions( "Note: This list may include transactions made by other people in the same giving group as the specified person. This is typically the same family, but not always." );
        }

        return result;
    }

    #endregion
}
