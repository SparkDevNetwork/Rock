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
using Rock.Data;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Lists every page belonging to one site as a flat list. Each page includes its parent page so the hierarchy can be reconstructed." )]
    [AgentUsage( "Use ListPages instead when you want to walk the page tree one level at a time." )]
    [AgentToolGuid( "8968B4EF-3A1D-472A-9BC6-17A80B8F824F" )]
    public AgentToolResult ListPagesForSite(
        string siteIdKey,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var site = helper.GetRequiredEntity<Model.Site>( siteIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Pages relate to a site through their layout.
        var layoutIds = LayoutCache.All( rockContext )
            .Where( l => l.SiteId == site.Id )
            .Select( l => l.Id )
            .ToHashSet();

        var query = PageCache.All( rockContext )
            .Where( p => layoutIds.Contains( p.LayoutId ) )
            .AsQueryable();

        var paginator = new CursorPaginator<PageCache>( AgentRequestContext.CurrentPerson, qry => qry
            .OrderBy( p => p.InternalName )
            .ThenBy( p => p.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( p => CreateSummaryPageResult( p, rockContext ) )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( p => KeyNameResult.FromCache( p ) ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
