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

using Rock.AI.Agent.Annotations;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal partial class ContentChannelSkill
{
    #region Tool(s)

    [Description( "Gets the available attributes that can be set when adding or updating a content channel item." )]
    [AgentPurpose( "Provides a list of attribute definitions for Content Channel Items and any value format instructions." )]
    [AgentToolGuid( "dc8e603d-5f97-47cd-b87f-3f128e512bf9" )]
    public AgentToolResult GetContentChannelItemAvailableAttributes(
        string contentChannelItemIdKey = null,
        string contentChannelIdKey = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        ContentChannelItem contentChannelItem;

        if ( contentChannelItemIdKey.IsNotNullOrWhiteSpace() )
        {
            contentChannelItem = helper.GetRequiredEntity<ContentChannelItem>( contentChannelItemIdKey, checkSecurity: true );

            if ( contentChannelItem == null )
            {
                return helper.ErrorResult;
            }
        }
        else
        {
            var contentChannel = helper.GetRequiredEntity<ContentChannel>( contentChannelIdKey, checkSecurity: true );

            if ( contentChannel == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListContentChannels )} function to determine available content channels." );
            }

            contentChannelItem = new ContentChannelItem
            {
                ContentChannelId = contentChannel.Id,
                ContentChannelTypeId = contentChannel.ContentChannelTypeId,
            };
        }

        contentChannelItem.LoadAttributes( AgentRequestContext.RockContext );

        return Success( helper.GetAvailableAttributes( contentChannelItem ) );
    }

    #endregion
}
