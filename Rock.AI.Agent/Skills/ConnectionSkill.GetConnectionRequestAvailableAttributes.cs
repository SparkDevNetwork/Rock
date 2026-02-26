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
//

using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class ConnectionSkill
    {
        #region Tool(s)

        [Description( "Gets the available attributes that can be set when adding or updating a connection request." )]
        [AgentPurpose( "Provides a list of attribute definitions for Connection Requests and any value format instructions." )]
        [AgentToolGuid( "c660989a-ba62-42f8-8eed-49c0bf7e8bf6" )]
        public IAgentToolResult GetConnectionRequestAvailableAttributes(
            string connectionRequestIdKey = null,
            string connectionOpportunityIdKey = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            ConnectionRequest connectionRequest;

            if ( connectionRequestIdKey.IsNotNullOrWhiteSpace() )
            {
                connectionRequest = helper.GetRequiredEntity<ConnectionRequest>( connectionRequestIdKey, checkSecurity: true );

                if ( connectionRequest == null )
                {
                    return helper.ErrorResult;
                }
            }
            else
            {
                var opportunity = helper.GetRequiredEntity<ConnectionOpportunity>( connectionOpportunityIdKey, checkSecurity: true );

                if ( opportunity == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( LookupConnectionTypesAndOpportunities )} function to determine available opportunities." );
                }

                connectionRequest = new ConnectionRequest
                {
                    ConnectionOpportunityId = opportunity.Id,
                    ConnectionTypeId = opportunity.ConnectionTypeId,
                };
            }

            connectionRequest.LoadAttributes( AgentRequestContext.RockContext );

            return Success( helper.GetAvailableAttributes( connectionRequest ) );
        }


        #endregion
    }
}
