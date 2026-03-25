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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        [Description( "Retrieves a list of benevolence requests." )]
        [AgentPurpose( "Retrieves a list of benevolence requests." )]
        [AgentUsage( "Pass 'lookup' for any ValueIdKey parameters to get a list of valid options." )]
        [AgentToolGuid( "818fa3cd-6318-4391-97be-17bffe8d9f2f" )]
        public IAgentToolResult ListBenevolenceRequests(
            string benevolenceTypeIdKey = null,
            string personIdKey = null,
            string statusValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var benevolenceTypeIds = GetConfiguredBenevolenceTypes().Select( c => c.Id ).ToList();

            var query = new BenevolenceRequestService( AgentRequestContext.RockContext )
                .Queryable()
                .Include( br => br.BenevolenceType )
                .Where( br => benevolenceTypeIds.Contains( br.BenevolenceTypeId ) );

            query = helper.WhereOptionalIdKey( query, br => br.BenevolenceTypeId, benevolenceTypeIdKey );
            query = helper.WhereOptionalIdKey( query, br => br.RequestedByPersonAlias.PersonId, personIdKey );
            query = helper.WhereOptionalIdKey( query, br => br.RequestStatusValueId, statusValueIdKey );
            query = helper.WhereRequiredPropertyBetween( query, br => br.RequestDateTime, startDate, endDate );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var projectionQry = query
                .OrderByDescending( br => br.RequestDateTime )
                .ThenBy( br => br.Id )
                .AsExpandable()
                .Select( br => new BenevolenceRequestResult
                {
                    Id = br.Id,
                    Person = PersonResult.NameOnly( br.RequestedByPersonAlias ),
                    FirstName = br.FirstName,
                    LastName = br.LastName,
                    RequestDateTime = br.RequestDateTime,
                    RequestText = br.RequestText,
                    ResultSummary = br.ResultSummary,
                    RequestStatus = br.RequestStatusValueId.HasValue
                        ? new KeyNameResult
                        {
                            Id = br.RequestStatusValueId,
                            Name = br.RequestStatusValue.Value,
                        }
                        : null,
                } );

            var page = helper.GetPaginatedItems( projectionQry, pageNumber );

            foreach ( var item in page.Items )
            {
                if ( item.Person == null )
                {
                    if ( item.FirstName.IsNotNullOrWhiteSpace() || item.LastName.IsNotNullOrWhiteSpace() )
                    {
                        item.Person = new PersonResult
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                        };
                    }
                }

                // If they were looking for data on a specific person, then
                // include a short version of the request and result. Otherwise
                // don't include them at all.
                if ( personIdKey.IsNotNullOrWhiteSpace() )
                {
                    item.RequestText = item.RequestText?.Truncate( 200 );
                    item.ResultSummary = item.ResultSummary?.Truncate( 200 );
                }
                else
                {
                    item.RequestText = null;
                    item.ResultSummary = null;
                }
            }

            return helper.GetPaginatedResult( page );
        }

        #endregion
    }
}
