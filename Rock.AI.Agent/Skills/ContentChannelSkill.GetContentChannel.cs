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
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ContentChannelSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal partial class ContentChannelSkill
{
    [Description( "Gets the details of an existing content channel." )]
    [AgentToolGuid( "7c90154a-2752-4850-a825-144bddabc978" )]
    public AgentToolResult GetContentChannel( string contentChannelIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var contentChannel = helper.GetRequiredEntity<ContentChannel>( contentChannelIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( new ContentChannelResult
        {
            Id = contentChannel.Id,
            Guid = contentChannel.Guid,
            Name = contentChannel.Name,
            ContentChannelType = new ContentChannelTypeResult
            {
                Id = contentChannel.ContentChannelTypeId,
                Name = contentChannel.ContentChannelType.Name,
            },
            RequiresApproval = contentChannel.RequiresApproval,
            Description = contentChannel.Description,
            AttributeValues = contentChannel.GetAttributeValueResults( AgentRequestContext ).ToList(),
        } )
            .WithHistoryContent( new KeyNameResult
            {
                Id = contentChannel.Id,
                Name = contentChannel.ToString(),
            } );
    }
}
