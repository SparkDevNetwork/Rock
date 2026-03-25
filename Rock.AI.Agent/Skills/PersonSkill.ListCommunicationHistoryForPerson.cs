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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Lists communication records for a specific person." )]
        [AgentPurpose( "Retrieves the communication records that have been sent to an individual." )]
        [AgentToolGuid( "dd7510bb-9176-4463-9b23-665000992a62" )]
        public IAgentToolResult ListCommunicationHistoryForPerson(
            string personIdKey,

            string senderPersonIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,

            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var qry = new CommunicationRecipientService( AgentRequestContext.RockContext )
                .Queryable();

            qry = helper.WhereRequiredIdKey( qry, cr => cr.PersonAlias.PersonId, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, cr => cr.Communication.SenderPersonAlias.PersonId, senderPersonIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, cr => cr.Communication.CreatedDateTime, startDate, endDate );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var orderedQry = qry
                .AsExpandable()
                .Select( cr => new CommunicationHistoryResult
                {
                    Id = cr.CommunicationId,
                    Name = cr.Communication.Name,
                    MediumEntityTypeId = cr.MediumEntityTypeId,
                    CreatedDateTime = cr.Communication.CreatedDateTime,
                    Sender = PersonResult.NameOnly( cr.Communication.SenderPersonAlias ),
                    SentDateTime = cr.SendDateTime,
                    Status = cr.Status,
                    StatusMessage = cr.StatusNote,
                } )
                .OrderByDescending( a => a.CreatedDateTime )
                .ThenBy( a => a.Id );

            var page = helper.GetPaginatedItems( orderedQry, pageNumber );
            var historyPage = page.WithItems( page.Items
                .Select( a => new KeyNameResult( a.Id, a.Name ) ) );

            return helper.GetPaginatedResult( page, historyPage );
        }

        #endregion
    }
}
