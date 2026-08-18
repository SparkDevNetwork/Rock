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
using System.Collections.Generic;
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

    [Description( "Searches CMS pages by a partial name match so a page can be resolved and confirmed with the user before adding a child page under it or adding a block to it." )]
    [AgentToolPreamble( "Looking up pages." )]
    [AgentUsage( "query is matched against the page's internal name and title. Returns the IdKey to pass to AddOrUpdatePage or AddOrUpdateBlock." )]
    [AgentToolGuid( "C668CAE0-CFA7-4AFF-87FF-5025860170BA" )]
    public AgentToolResult SearchPages(
        [Description( "A partial page name to search for. Matched against the page's internal name and title." )]
        string query,

        [Description( "The IdKey or guid of a site to limit the search to. Omit to search every site." )]
        string siteIdKey = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        // Pages relate to a site through their layout, so a site filter
        // becomes a set of layout ids.
        HashSet<int> layoutIds = null;

        if ( siteIdKey.IsNotNullOrWhiteSpace() )
        {
            var site = helper.GetRequiredEntity<Model.Site>( siteIdKey, checkSecurity: true );

            if ( site != null )
            {
                layoutIds = LayoutCache.All( rockContext )
                    .Where( l => l.SiteId == site.Id )
                    .Select( l => l.Id )
                    .ToHashSet();
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var pageQuery = PageCache.All( rockContext )
            .Where( p => query.IsNullOrWhiteSpace()
                || ( p.InternalName != null && p.InternalName.IndexOf( query, StringComparison.OrdinalIgnoreCase ) >= 0 )
                || ( p.PageTitle != null && p.PageTitle.IndexOf( query, StringComparison.OrdinalIgnoreCase ) >= 0 ) )
            .Where( p => layoutIds == null || layoutIds.Contains( p.LayoutId ) )
            .AsQueryable();

        var paginator = new CursorPaginator<PageCache>( AgentRequestContext.CurrentPerson, qry => qry
            .OrderBy( p => p.InternalName )
            .ThenBy( p => p.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( pageQuery, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( p => CreateSummaryPageResult( p, rockContext ) )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( p => new KeyNameResult
        {
            Id = p.Id,
            Name = p.InternalName
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
