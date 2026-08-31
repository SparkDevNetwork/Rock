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
using Rock.Data;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Lists the block types available to place on a page, filtered by a partial name or category. Returns the blockTypeIdKey that AddOrUpdateBlock needs." )]
    [AgentUsage( "Prefer Obsidian block types over WebForms ones when both exist for a purpose." )]
    [AgentToolGuid( "F9A5AC4D-E40C-4FAF-895D-8C0E10A37EEC" )]
    public AgentToolResult ListBlockTypes(
        [Description( "A partial block type name to filter by, such as 'custom component'." )]
        string name = null,

        [Description( "A category to filter by, such as 'CMS'. Matched exactly, ignoring case." )]
        string category = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var query = BlockTypeCache.All( rockContext )
            .Where( bt => name.IsNullOrWhiteSpace()
                || ( bt.Name != null && bt.Name.IndexOf( name, StringComparison.OrdinalIgnoreCase ) >= 0 ) )
            .Where( bt => category.IsNullOrWhiteSpace()
                || ( bt.Category != null && bt.Category.Equals( category, StringComparison.OrdinalIgnoreCase ) ) )
            .AsQueryable();

        var paginator = new CursorPaginator<BlockTypeCache>( AgentRequestContext.CurrentPerson, qry => qry
            .OrderBy( bt => bt.Name )
            .ThenBy( bt => bt.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( bt => new Classes.Skills.CmsSkill.BlockTypeResult
            {
                Id = bt.Id,
                Guid = bt.Guid,
                Name = bt.Name,
                Category = bt.Category,
                Description = bt.Description,
                // Entity-based block types have no path; path-based ones are
                // legacy WebForms controls.
                Platform = bt.Path.IsNotNullOrWhiteSpace() ? "WebForms" : "Obsidian"
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( bt => new KeyNameResult
        {
            Id = bt.Id,
            Name = bt.Name
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
