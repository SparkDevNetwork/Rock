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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Bus.Message;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Adds a new child page under a parent page or updates an existing page. New pages inherit the parent's layout unless a layout is specified. Pass a kebab-case route so the page gets a friendly URL." )]
    [AgentToolPreamble( "Saving the page." )]
    [AgentUsage( "When adding, parentPageIdKey is where the new page lives; ask the user for it if not specified. When updating, pass the pageIdKey and only the properties to change." )]
    [AgentUsage( "Pass route so the page gets a friendly kebab-case URL ('serving-dashboard'). Without it the page is only reachable at /page/id. Setting a route on an existing page replaces its current routes." )]
    [AgentToolGuid( "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633" )]
    public AgentToolResult AddOrUpdatePage(
        [Description( "Required when editing an existing page. Do not provide when adding a new page." )]
        string pageIdKey = null,

        [Description( "The IdKey or guid of the parent page the new page will live under. Required when adding, not allowed when updating." )]
        string parentPageIdKey = null,

        [Description( "The IdKey or guid of the layout the page renders with. When adding, defaults to the parent page's layout." )]
        string layoutIdKey = null,

        [Description( "The administrative name of the page. Required when adding; also used as the default page title and browser title." )]
        SetOrClear<string> internalName = null,

        SetOrClear<string> pageTitle = null,

        SetOrClear<string> browserTitle = null,

        SetOrClear<string> description = null,

        [Description( "A kebab-case route for the page, such as 'serving-dashboard', so it is reachable at a friendly URL rather than only /page/id. Replaces any existing routes; clear to remove them." )]
        SetOrClear<string> route = null,

        [Description( "When the page is shown in navigation menus." )]
        DisplayInNavWhen? displayInNavWhen = null,

        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var isAdd = pageIdKey.IsNullOrWhiteSpace();
        Model.Page page = null;
        Model.Page parent = null;

        if ( isAdd )
        {
            parent = helper.GetRequiredEntity<Model.Page>( parentPageIdKey, checkSecurity: false );

            if ( internalName?.Value.IsNullOrWhiteSpace() != false || internalName.ClearValue )
            {
                helper.AddError( "An internal name is required when adding a page." );
            }
        }
        else
        {
            page = helper.GetRequiredEntity<Model.Page>( pageIdKey, checkSecurity: false );

            if ( parentPageIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( $"A page cannot be moved to a new parent, do not provide a {nameof( parentPageIdKey )} when editing." );
            }

            if ( internalName?.ClearValue == true )
            {
                helper.AddError( "The internal name of a page cannot be cleared." );
            }
        }

        var layout = helper.GetOptionalEntity<Model.Layout>( layoutIdKey, checkSecurity: false );

        // A page's site is derived through its layout, so a layout from
        // another site would silently move the page to that site. The admin
        // UI scopes its layout picker to the page's site for the same reason.
        if ( layout != null )
        {
            var currentLayoutId = isAdd ? parent?.LayoutId : page?.LayoutId;
            var currentSiteId = currentLayoutId.HasValue
                ? LayoutCache.Get( currentLayoutId.Value, rockContext )?.SiteId
                : null;

            if ( currentSiteId.HasValue && layout.SiteId != currentSiteId.Value )
            {
                helper.AddError( $"The '{layout.Name}' layout belongs to a different site than the page. Pick a layout from the page's own site; call {nameof( ListLayouts )} with the site's IdKey to see the choices." );
            }
        }

        // Validate the route before the page is saved, so a bad route is a
        // clean error rather than a page saved without the URL the caller
        // asked for.
        var normalizedRoute = route?.ClearValue == false && route.Value.IsNotNullOrWhiteSpace()
            ? route.Value.Trim().TrimStart( '/' )
            : null;

        if ( route != null && !route.ClearValue )
        {
            // The page identifier is resolved to a plain value before the
            // query is built. Referencing the page entity inside the
            // expression tree makes Entity Framework try to translate the
            // entity itself into a constant, which it cannot do.
            var currentPageId = page?.Id ?? 0;

            if ( normalizedRoute.IsNullOrWhiteSpace() || normalizedRoute.Contains( " " ) )
            {
                helper.AddError( "The route must be a URL path with no spaces, such as 'serving-dashboard'." );
            }
            else if ( new PageRouteService( rockContext ).Queryable().Any( r => r.Route == normalizedRoute && r.PageId != currentPageId ) )
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
        var securityTargetId = isAdd ? parent.Id : page.Id;
        var securityTargetCache = PageCache.Get( securityTargetId, rockContext );

        if ( securityTargetCache == null || !securityTargetCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( isAdd
                ? "You are not authorized to add a page under that parent."
                : "You are not authorized to edit that page." );

            return helper.ErrorResult;
        }

        var pageService = new PageService( rockContext );

        if ( isAdd )
        {
            page = new Model.Page
            {
                ParentPageId = parent.Id,
                LayoutId = layout?.Id ?? parent.LayoutId,
                InternalName = internalName.Value,
                PageTitle = internalName.Value,
                BrowserTitle = internalName.Value,
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
        }
        else
        {
            helper.UpdateProperty( page, p => p.InternalName, internalName );

            if ( layout != null )
            {
                page.LayoutId = layout.Id;
            }
        }

        helper.UpdateProperty( page, p => p.PageTitle, pageTitle );
        helper.UpdateProperty( page, p => p.BrowserTitle, browserTitle );
        helper.UpdateProperty( page, p => p.Description, description );
        helper.UpdateProperty( page, p => p.DisplayInNavWhen, displayInNavWhen );
        helper.SetAttributeValues( page, attributeValues );

        if ( !page.IsValid )
        {
            helper.AddError( page.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The page could not be saved." );
        }

        // The page must be saved before the route so the route has a PageId.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( route != null )
        {
            var pageRouteService = new PageRouteService( rockContext );

            // Setting a route replaces the existing routes rather than adding
            // a second one; clearing just removes them.
            var existingRoutes = pageRouteService.Queryable()
                .Where( r => r.PageId == page.Id )
                .ToList();

            pageRouteService.DeleteRange( existingRoutes );

            if ( normalizedRoute != null )
            {
                var pageRoute = new Model.PageRoute
                {
                    PageId = page.Id,
                    Route = normalizedRoute
                };

                pageRouteService.Add( pageRoute );

                if ( !pageRoute.IsValid )
                {
                    helper.AddError( pageRoute.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? $"The route '{normalizedRoute}' could not be created." );

                    return helper.ErrorResult;
                }
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

        if ( isAdd )
        {
            // A new child page inherits the parent page's authorization.
            Authorization.CopyAuthorization( parent, page, rockContext );

            // Flush the parent so the new child appears in navigation.
            PageCache.Remove( parent.Id );
        }
        else
        {
            PageCache.FlushPage( page.Id );
        }

        var routes = new PageRouteService( rockContext ).Queryable()
            .Where( r => r.PageId == page.Id )
            .Select( r => r.Route )
            .ToList();

        var url = routes.Count > 0 ? $"/{routes[0]}" : $"/page/{page.Id}";

        var result = Success( new PageResult
        {
            Id = page.Id,
            Guid = page.Guid,
            InternalName = page.InternalName,
            PageTitle = page.PageTitle,
            BrowserTitle = page.BrowserTitle,
            Description = page.Description,
            Url = url,
            Routes = routes,
            ParentPage = page.ParentPageId.HasValue
                ? new KeyNameResult
                {
                    Id = page.ParentPageId.Value,
                    Name = parent?.InternalName ?? PageCache.Get( page.ParentPageId.Value, rockContext )?.InternalName
                }
                : null
        } )
            .WithHistoryContent( new KeyNameResult
            {
                Id = page.Id,
                Name = page.InternalName
            } )
            .WithInstructions( $"The page has been {( isAdd ? "created" : "updated" )}." );

        if ( routes.Count == 0 )
        {
            result.WithInstructions( $"The page has no route, so it is only reachable at /page/{page.Id}. Pass a kebab-case route to {nameof( AddOrUpdatePage )}, or tell the user the page has no friendly URL until one is added through Page Properties." );
        }

        return result;
    }

    #endregion
}
