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
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Gets the details of a single page, including its routes, layout, and the blocks already placed on it." )]
    [AgentUsage( "Check the blocks list before adding a block to a page: update the existing block instead of adding a duplicate." )]
    [AgentToolGuid( "E2CFF69F-C4B2-47F5-B322-4041D841F37C" )]
    public AgentToolResult GetPage( string pageIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;
        var person = AgentRequestContext.CurrentPerson;

        var page = helper.GetRequiredEntity<Model.Page>( pageIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var pageCache = PageCache.Get( page.Id, rockContext );

        if ( pageCache == null )
        {
            return NoData();
        }

        var layout = pageCache.Layout;
        var parentPage = pageCache.ParentPageId.HasValue
            ? PageCache.Get( pageCache.ParentPageId.Value, rockContext )
            : null;

        var blocks = pageCache.Blocks
            .Where( b => b.IsAuthorized( Authorization.VIEW, person ) )
            .OrderBy( b => b.Zone )
            .ThenBy( b => b.Order )
            .Select( b => CreateSummaryBlockResult( b, rockContext ) )
            .ToList();

        // The zones come from the layout markup so the caller can name a real
        // zone when adding a block instead of guessing one.
        var zones = GetLayoutZones( layout );

        foreach ( var zone in zones )
        {
            zone.BlockCount = blocks.Count( b => zone.Name.Equals( b.Zone, StringComparison.OrdinalIgnoreCase ) );
        }

        return Success( new PageResult
        {
            Id = pageCache.Id,
            Guid = pageCache.Guid,
            InternalName = pageCache.InternalName,
            PageTitle = pageCache.PageTitle,
            BrowserTitle = pageCache.BrowserTitle,
            Description = pageCache.Description,
            SiteName = pageCache.Site,
            Url = GetPageUrl( pageCache ),
            ParentPage = parentPage != null
                ? new KeyNameResult { Id = parentPage.Id, Name = parentPage.InternalName }
                : null,
            Layout = layout != null
                ? new LayoutResult
                {
                    Id = layout.Id,
                    Guid = layout.Guid,
                    Name = layout.Name,
                    SiteName = layout.Site?.Name,
                    Zones = zones
                }
                : null,
            Routes = pageCache.PageRoutes.Select( r => r.Route ).ToList(),
            DisplayInNavWhen = pageCache.DisplayInNavWhen.ConvertToString( true ),
            ChildPageCount = pageCache.GetPages( rockContext ).Count,
            Blocks = blocks,
            AttributeValues = page.GetAttributeValueResults( AgentRequestContext ).ToList()
        } )
            .WithHistoryContent( new KeyNameResult
            {
                Id = pageCache.Id,
                Name = pageCache.InternalName
            } );
    }

    #endregion
}
