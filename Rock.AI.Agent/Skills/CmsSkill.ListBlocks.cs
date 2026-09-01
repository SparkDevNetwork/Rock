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

    [Description( "Lists the blocks placed on a page, a layout, or a site. At least one filter is required." )]
    [AgentUsage( "Layout and site blocks render on every page that uses the layout or site, so check all three scopes when asking what renders on a page." )]
    [AgentToolGuid( "98F33433-0712-4248-9C71-EAE4D9F9CA38" )]
    public AgentToolResult ListBlocks(
        string pageIdKey = null,

        string layoutIdKey = null,

        string siteIdKey = null,

        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        helper.RequireAtLeastOneFilter( new object[] { pageIdKey, layoutIdKey, siteIdKey } );

        var query = BlockCache.All( rockContext ).AsQueryable();

        query = helper.WhereOptionalIdKey( query, b => b.PageId, pageIdKey );
        query = helper.WhereOptionalIdKey( query, b => b.LayoutId, layoutIdKey );
        query = helper.WhereOptionalIdKey( query, b => b.SiteId, siteIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<BlockCache>( AgentRequestContext.CurrentPerson, qry => qry
            .OrderBy( b => b.Zone )
            .ThenBy( b => b.Order )
            .ThenBy( b => b.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( b => CreateSummaryBlockResult( b, rockContext ) )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( b => new KeyNameResult
        {
            Id = b.Id,
            Name = b.Name
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
