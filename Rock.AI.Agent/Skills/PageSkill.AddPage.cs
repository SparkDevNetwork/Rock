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
using Rock.AI.Agent.Classes.Skills.PageSkill;
using Rock.Bus.Message;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PageSkill
{
    #region Tool(s)

    [Description( "Adds a new child page under a parent page, inheriting the parent's layout and therefore its site and zones. Pass a kebab-case route so the page gets a friendly URL." )]
    [AgentToolPreamble( "Creating the page." )]
    [AgentUsage( "parentPage is where the new page lives; ask the user for it if not specified. The new page inherits the parent's layout." )]
    [AgentUsage( "Pass route so the page gets a friendly kebab-case URL ('serving-dashboard'). Without it the page is only reachable at /page/id." )]
    [AgentToolGuid( "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633" )]
    public AgentToolResult AddPage(
        [Description( "The IdKey or guid of the parent page the new page will live under." )]
        string parentPage,

        [Description( "The name of the new page. Used as the internal name, page title, and browser title." )]
        string name,

        [Description( "An optional kebab-case route for the page, such as 'serving-dashboard', so it is reachable at a friendly URL rather than only /page/id." )]
        string route = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( name.IsNullOrWhiteSpace() )
        {
            helper.AddError( "A page name is required." );
        }

        var parent = helper.GetRequiredEntity<Model.Page>( parentPage, checkSecurity: false );

        // Validate the route before the page exists, so a bad route is a clean
        // error rather than a page created without the URL the caller asked for.
        var normalizedRoute = route.IsNotNullOrWhiteSpace() ? route.Trim().TrimStart( '/' ) : null;

        if ( normalizedRoute != null )
        {
            if ( normalizedRoute.Length == 0 || normalizedRoute.Contains( " " ) )
            {
                helper.AddError( "The route must be a URL path with no spaces, such as 'serving-dashboard'." );
            }
            else if ( new PageRouteService( rockContext ).Queryable().Any( r => r.Route == normalizedRoute ) )
            {
                helper.AddError( $"The route '{normalizedRoute}' is already used by another page. Choose a different route." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Authorization is checked through the cache so inherited page and
        // site security participates.
        var parentCache = PageCache.Get( parent.Id, rockContext );

        if ( parentCache == null || !parentCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to add a page under that parent." );

            return helper.ErrorResult;
        }

        var pageService = new PageService( rockContext );

        var page = new Model.Page
        {
            ParentPageId = parent.Id,
            LayoutId = parent.LayoutId,
            InternalName = name,
            PageTitle = name,
            BrowserTitle = name,
            AllowIndexing = true,
            EnableViewState = true,
            IncludeAdminFooter = true,
            MenuDisplayChildPages = true
        };

        // Place the new page at the end of its siblings, matching the admin block.
        var lastSiblingOrder = pageService.GetByParentPageId( parent.Id )
            .OrderByDescending( p => p.Order )
            .Select( p => ( int? ) p.Order )
            .FirstOrDefault();

        page.Order = lastSiblingOrder.HasValue ? lastSiblingOrder.Value + 1 : 0;

        pageService.Add( page );

        if ( !page.IsValid )
        {
            helper.AddError( page.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The page could not be created." );

            return helper.ErrorResult;
        }

        // The page must be saved before the route so the route has a PageId.
        rockContext.SaveChanges();

        if ( normalizedRoute != null )
        {
            var pageRoute = new Model.PageRoute
            {
                PageId = page.Id,
                Route = normalizedRoute
            };

            new PageRouteService( rockContext ).Add( pageRoute );

            if ( !pageRoute.IsValid )
            {
                helper.AddError( pageRoute.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? $"The route '{normalizedRoute}' could not be created." );

                return helper.ErrorResult;
            }

            rockContext.SaveChanges();

            /*
                8/17/2026 - CLAUDE

                The routing table is only rebuilt when told about the new
                route; without this the friendly URL 404s until the next
                application restart.

                Reason: The route must be registered with the running routing table.
            */
            PageRouteWasUpdatedMessage.Publish();
        }

        // A new child page inherits the parent page's authorization.
        Authorization.CopyAuthorization( parent, page, rockContext );

        // Flush the parent so the new child appears in navigation.
        PageCache.Remove( parent.Id );

        var result = Success( new AddPageResult
        {
            Id = page.Id,
            Guid = page.Guid,
            Name = page.InternalName,
            Url = normalizedRoute != null ? $"/{normalizedRoute}" : $"/page/{page.Id}"
        } );

        if ( normalizedRoute == null )
        {
            result.WithInstructions( $"The page was created without a route, so it is only reachable at /page/{page.Id}. Pass a kebab-case route to AddPage next time, or tell the user the page has no friendly URL until one is added through Page Properties." );
        }

        return result;
    }

    #endregion
}
