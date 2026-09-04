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
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class ContentChannelSkill
{
    #region Tool(s)

    [Description( "Lists content channel items that match the filters." )]
    [AgentToolGuid( "0782bd98-5ec4-4b88-9784-9d122ad3cbb1" )]
    public AgentToolResult ListContentChannelItems(
        string contentChannelIdKey = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new ContentChannelItemService( AgentRequestContext.RockContext )
            .Queryable();

        query = helper.WhereOptionalIdKey( query, cci => cci.ContentChannelId, contentChannelIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<ContentChannelItem>( currentPerson, qry => qry
            .OrderByDescending( cci => cci.StartDateTime )
            .ThenBy( cci => cci.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( cci => new ContentChannelItemResult
            {
                Id = cci.Id,
                Guid = cci.Guid,
                Name = cci.Title,
                ContentChannel = new ContentChannelResult
                {
                    Id = cci.ContentChannelId,
                    Guid = cci.ContentChannel.Guid,
                    Name = cci.ContentChannel.Name,
                },
                AttributeValues = cci.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( cci => new KeyNameResult
        {
            Id = cci.Id,
            Guid = cci.Guid,
            Name = cci.ToString()
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
