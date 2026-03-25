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

using System.ComponentModel;

using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal partial class ConnectionSkill
{
    [Description( "Gets the details of an existing connection request." )]
    [AgentToolGuid( "3e00b0ef-9aa8-4e77-8bcc-961a6ea6fd9c" )]
    public IAgentToolResult GetConnectionRequest( string connectionRequestIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var connectionRequest = helper.GetRequiredEntity<ConnectionRequest>( connectionRequestIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( GetFullConnectionRequestResult( connectionRequest ) )
            .WithHistoryContent( new KeyNameResult
            {
                Id = connectionRequest.Id,
                Name = connectionRequest.ToString(),
            } );
    }
}
