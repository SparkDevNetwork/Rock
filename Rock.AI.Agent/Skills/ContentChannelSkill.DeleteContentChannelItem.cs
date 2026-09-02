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
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ContentChannelSkill
{
    #region Tool(s)

    /// <summary>
    /// Deletes a content channel item.
    /// </summary>
    /// <remarks>
    /// An item still referenced by other records, such as a child item, cannot be
    /// deleted.
    /// </remarks>
    [AgentGuardrail( "This permanently deletes the content channel item and its content. Confirm the exact item with the user before proceeding." )]
    [Description( "Deletes a content channel item. An item referenced by other records cannot be deleted." )]
    [AgentToolPreamble( "Deleting the content channel item." )]
    [AgentToolPrerequisite( "Call ListContentChannelItems to determine the contentChannelItemIdKey." )]
    [AgentToolGuid( "F4BB7AAC-65DC-42C2-B9F7-5869CD25265F" )]
    public AgentToolResult DeleteContentChannelItem( string contentChannelItemIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var contentChannelItemService = new ContentChannelItemService( rockContext );

        var contentChannelItem = helper.GetRequiredEntity<ContentChannelItem>( contentChannelItemIdKey, checkSecurity: true );

        if ( contentChannelItem == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListContentChannelItems function to determine the available content channel items." );
        }

        if ( !contentChannelItem.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to delete that content channel item." );
        }

        if ( !contentChannelItemService.CanDelete( contentChannelItem, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        var title = contentChannelItem.Title;

        contentChannelItemService.Delete( contentChannelItem );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( $"The '{title}' content channel item has been deleted." );
    }

    #endregion
}
