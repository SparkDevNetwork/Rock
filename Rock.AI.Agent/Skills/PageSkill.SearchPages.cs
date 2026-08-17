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
using Rock.AI.Agent.Classes.Skills.PageSkill;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PageSkill
{
    #region Tool(s)

    [Description( "Searches CMS pages by a partial name match so a page can be resolved and confirmed with the user before adding a child page under it or adding a block to it." )]
    [AgentToolPreamble( "Looking up pages." )]
    [AgentUsage( "query is matched against the page's internal name and title. Returns the IdKey and guid to pass to AddPage or AddBlock." )]
    [AgentToolGuid( "C668CAE0-CFA7-4AFF-87FF-5025860170BA" )]
    public AgentToolResult SearchPages(
        [Description( "A partial page name to search for. Matched against the page's internal name and title." )]
        string query )
    {
        var person = AgentRequestContext.CurrentPerson;

        var matches = PageCache.All( AgentRequestContext.RockContext )
            .Where( p => query.IsNullOrWhiteSpace()
                || ( p.InternalName != null && p.InternalName.IndexOf( query, StringComparison.OrdinalIgnoreCase ) >= 0 )
                || ( p.PageTitle != null && p.PageTitle.IndexOf( query, StringComparison.OrdinalIgnoreCase ) >= 0 ) )
            .Where( p => p.IsAuthorized( Authorization.VIEW, person ) )
            .OrderBy( p => p.InternalName )
            .Take( 25 )
            .Select( p => new PageResult
            {
                Id = p.Id,
                Guid = p.Guid,
                InternalName = p.InternalName,
                PageTitle = p.PageTitle,
                SiteName = p.Layout?.Site?.Name
            } )
            .ToList();

        if ( !matches.Any() )
        {
            return NoData();
        }

        // Store only essential properties in session context to keep it lean.
        var trimmedForHistory = matches.Select( p => new
        {
            p.IdKey,
            p.InternalName
        } );

        return Success( matches )
            .WithHistoryContent( trimmedForHistory, "page-list" );
    }

    #endregion
}
