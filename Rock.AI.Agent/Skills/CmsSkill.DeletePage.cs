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
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Bus.Message;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Deletes a page along with its blocks, routes, and optionally its interaction history. Pages with child pages are refused; delete or move the children first." )]
    [AgentToolPreamble( "Deleting the page." )]
    [AgentUsage( "Deleting a page is permanent and destructive: the page, its blocks, and its routes are removed and cannot be recovered, and when deleteInteractions is true the page's interaction (page view) history is permanently removed as well. Before calling this tool, warn the user about exactly what will be deleted, name the exact page being deleted, and get their explicit confirmation. Ask the user whether the interaction history should also be deleted rather than deciding for them. Never delete a page the user did not name explicitly." )]
    [AgentToolGuid( "BB6C42F3-C448-49D5-BB85-4072960178FC" )]
    public AgentToolResult DeletePage(
        [Description( "The IdKey or guid of the page to delete." )]
        string pageIdKey,
        [Description( "Whether the page's interaction (page view) history is also permanently deleted. Defaults to true, matching the administrative UI. Ask the user before choosing." )]
        bool deleteInteractions = true )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var page = helper.GetRequiredEntity<Model.Page>( pageIdKey, checkSecurity: false );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        /*
            8/18/2026 - CLAUDE

            A page's children cascade with it in the database, so a single
            call could silently take down an entire subtree. Refusing pages
            with children forces deliberate bottom-up deletion, where each
            page is named one call at a time.

            Reason: One tool call must not be able to delete a subtree.
        */
        var pageService = new PageService( rockContext );

        if ( pageService.GetByParentPageId( page.Id ).Any() )
        {
            helper.AddError( $"The '{page.InternalName}' page has child pages, so it cannot be deleted here. Delete or move the child pages first." );

            return helper.ErrorResult;
        }

        // Authorization is checked through the cache so inherited page and
        // site security participates.
        var pageCache = PageCache.Get( page.Id, rockContext );

        if ( pageCache == null || !pageCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to delete that page." );

            return helper.ErrorResult;
        }

        /*
            8/24/2026 - CLAUDE

            The admin Pages block silently clears a site's default, login,
            and registration page references before deleting the page they
            point at. A site losing one of those pages is a bigger decision
            than an agent should make as a side effect of a delete, so this
            tool refuses instead and names the site and the role the page
            plays. Without this guard the delete fails on the foreign key
            with an error the model cannot act on.

            Reason: A site losing its default or login page must be a deliberate act.
        */
        var referencingSites = new SiteService( rockContext )
            .Queryable()
            .Where( s =>
                s.DefaultPageId == page.Id
                || s.LoginPageId == page.Id
                || s.RegistrationPageId == page.Id
                || s.ChangePasswordPageId == page.Id
                || s.PageNotFoundPageId == page.Id
                || s.CommunicationPageId == page.Id
                || s.MobilePageId == page.Id )
            .ToList();

        foreach ( var site in referencingSites )
        {
            var roles = new List<string>();

            if ( site.DefaultPageId == page.Id )
            {
                roles.Add( "default page" );
            }

            if ( site.LoginPageId == page.Id )
            {
                roles.Add( "login page" );
            }

            if ( site.RegistrationPageId == page.Id )
            {
                roles.Add( "registration page" );
            }

            if ( site.ChangePasswordPageId == page.Id )
            {
                roles.Add( "change password page" );
            }

            if ( site.PageNotFoundPageId == page.Id )
            {
                roles.Add( "404 (page not found) page" );
            }

            if ( site.CommunicationPageId == page.Id )
            {
                roles.Add( "communication page" );
            }

            if ( site.MobilePageId == page.Id )
            {
                roles.Add( "mobile redirect page" );
            }

            helper.AddError( $"The '{page.InternalName}' page is the {roles.AsDelimited( ", ", " and " )} of the '{site.Name}' site, so it cannot be deleted. An administrator must point the site at a different page first." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var name = page.InternalName;
        var pageId = page.Id;
        var siteId = page.SiteId;
        var parentPageId = page.ParentPageId;
        var hadRoutes = pageCache.PageRoutes.Count > 0;
        var deletedBlockCount = new BlockService( rockContext )
            .Queryable()
            .Count( b => b.PageId == page.Id );

        // Blocks and routes cascade with the page, matching how the admin
        // Pages block deletes.
        pageService.Delete( page );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        PageCache.FlushPage( page.Id );

        if ( parentPageId.HasValue )
        {
            // Flush the parent so the deleted child leaves navigation.
            PageCache.Remove( parentPageId.Value );
        }

        if ( hadRoutes )
        {
            // The routing table only forgets the route when told; without
            // this the dead route lingers until the next application restart.
            PageRouteWasUpdatedMessage.Publish();
        }

        if ( deleteInteractions )
        {
            // Interactions reference the page by loose EntityId, so nothing
            // cascades. The volume can be large, so a background task deletes
            // them in chunks; it takes raw identifiers because the page is
            // gone by the time it runs. This matches the admin Pages block.
            new DeleteInteractions.Message
            {
                PageId = pageId,
                SiteId = siteId
            }.Send();
        }

        return Success( new PageDeleteResult
        {
            IsDeleted = true,
            Name = name,
            DeletedBlockCount = deletedBlockCount,
            IsInteractionDeleteQueued = deleteInteractions
        } );
    }

    #endregion
}
