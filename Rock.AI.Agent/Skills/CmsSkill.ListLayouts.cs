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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Data;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Lists the layouts pages can render with, optionally filtered to one site. Returns the layoutIdKey that AddOrUpdatePage accepts." )]
    [AgentUsage( "A page's site is determined by its layout, so only pass AddOrUpdatePage a layout belonging to the same site as the page. Filter by the page's siteIdKey to see the valid choices." )]
    [AgentToolGuid( "82C06D71-800E-4064-B72D-98F1B2A684D7" )]
    public AgentToolResult ListLayouts(
        [Description( "The IdKey or guid of a site to limit the layouts to. Omit to list the layouts of every site." )]
        string siteIdKey = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var query = LayoutCache.All( rockContext ).AsQueryable();

        query = helper.WhereOptionalIdKey( query, l => ( int? ) l.SiteId, siteIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<LayoutCache>( AgentRequestContext.CurrentPerson, qry => qry
            .OrderBy( l => l.Name )
            .ThenBy( l => l.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( l => new LayoutResult
            {
                Id = l.Id,
                Guid = l.Guid,
                Name = l.Name,
                SiteName = l.Site?.Name,
                Description = l.Description
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( l => KeyNameResult.FromCache( l ) ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
