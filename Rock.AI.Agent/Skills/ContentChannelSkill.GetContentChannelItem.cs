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

internal partial class ContentChannelSkill
{
    [Description( "Gets the details of an existing content channel item." )]
    [AgentToolGuid( "3a721eb2-757d-4236-97cf-b7cdf8c11357" )]
    public AgentToolResult GetContentChannelItem( string contentChannelItemIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var contentChannelItem = helper.GetRequiredEntity<ContentChannelItem>( contentChannelItemIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( GetFullContentChannelItemResult( contentChannelItem ) )
            .WithHistoryContent( new KeyNameResult
            {
                Id = contentChannelItem.Id,
                Name = contentChannelItem.ToString(),
            } );
    }
}
