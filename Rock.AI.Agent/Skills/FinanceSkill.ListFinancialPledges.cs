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
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Retrieves a list of financial pledges." )]
    [AgentPurpose( "Retrieves a list of financial pledges." )]
    [AgentUsage( "The startDate and endDate parameters refer to the pledge start date, pledges may not have dates." )]
    [AgentToolGuid( "c13b096e-4bc6-4780-8b9d-30133cde0194" )]
    public AgentToolResult ListFinancialPledges(
        string personIdKey = null,
        string financialAccountIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var query = new FinancialPledgeService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( fp => fp.Account );

        query = helper.WhereOptionalIdKey( query, fp => fp.PersonAlias.PersonId, personIdKey );
        query = helper.WhereOptionalIdKey( query, fp => fp.AccountId, financialAccountIdKey );
        query = helper.WhereOptionalPropertyBetween( query, fp => fp.StartDate, startDate, endDate );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var projectionQry = query
            .OrderByDescending( fp => fp.StartDate )
            .ThenBy( fp => fp.Id )
            .AsExpandable()
            .Select( fp => new FinancialPledgeResult
            {
                Id = fp.Id,
                Person = PersonResult.NameOnly( fp.PersonAlias ),
                StartDate = fp.StartDate != DateTime.MinValue ? fp.StartDate : null,
                EndDate = fp.EndDate != DateTime.MaxValue ? fp.EndDate : null,
                FinancialAccount = new FinancialAccountResult
                {
                    Id = fp.Account.Id,
                    Name = fp.Account.Name,
                },
                TotalAmount = fp.TotalAmount,
                PaymentSchedule = fp.PledgeFrequencyValue.Value,
            } );

        var page = helper.GetPaginatedItems( projectionQry, pageNumber );

        return helper.GetPaginatedResult( page );
    }

    #endregion
}
