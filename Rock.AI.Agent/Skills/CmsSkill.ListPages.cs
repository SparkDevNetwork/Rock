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

    [Description( "Lists one level of the page tree: the root pages of every site, or the immediate children of a parent page. Call repeatedly with a returned page's IdKey to walk deeper." )]
    [AgentUsage( "Each result includes a childPageCount so you know whether a page has children worth walking into. Use ListPagesForSite instead when you want every page of one site as a flat list." )]
    [AgentToolGuid( "1F7C1F00-F481-468A-860F-314D1B43A477" )]
    public AgentToolResult ListPages(
        [Description( "The IdKey or guid of the page whose immediate children should be listed. Omit to list the root pages of every site." )]
        string parentPageIdKey = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var query = PageCache.All( rockContext ).AsQueryable();

        if ( parentPageIdKey.IsNotNullOrWhiteSpace() )
        {
            query = helper.WhereRequiredIdKey( query, p => p.ParentPageId, parentPageIdKey );
        }
        else
        {
            query = query.Where( p => !p.ParentPageId.HasValue );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<PageCache>( AgentRequestContext.CurrentPerson, qry => qry
            .OrderBy( p => p.Order )
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
