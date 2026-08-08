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

    [Description( "Lists content channels that match the filters." )]
    [AgentToolGuid( "780523e8-4ac9-414f-ba0c-0f6a6471f37f" )]
    public AgentToolResult ListContentChannels(
        string contentChannelTypeIdKey = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = ContentChannelCache.All( AgentRequestContext.RockContext )
            .AsQueryable();

        query = helper.WhereOptionalIdKey( query, cc => cc.ContentChannelTypeId, contentChannelTypeIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<ContentChannelCache>( currentPerson, qry => qry
            .OrderBy( cc => cc.Name )
            .ThenBy( cc => cc.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( cc => new ContentChannelResult
            {
                Id = cc.Id,
                Name = cc.Name,
                ContentChannelType = new ContentChannelTypeResult
                {
                    Id = cc.ContentChannelTypeId,
                    Name = cc.ContentChannelType.Name,
                },
                AttributeValues = cc.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( cc => new KeyNameResult
        {
            Id = cc.Id,
            Name = cc.ToString()
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
