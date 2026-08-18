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

    [Description( "Deletes a page along with its blocks and routes. Pages with child pages are refused; delete or move the children first." )]
    [AgentToolPreamble( "Deleting the page." )]
    [AgentUsage( "Deleting a page is permanent and takes its blocks and routes with it. Confirm the exact page with the user before deleting, and never delete a page the user did not name explicitly." )]
    [AgentToolGuid( "BB6C42F3-C448-49D5-BB85-4072960178FC" )]
    public AgentToolResult DeletePage(
        [Description( "The IdKey or guid of the page to delete." )]
        string pageIdKey )
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

        var name = page.InternalName;
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

        return Success( new PageDeleteResult
        {
            IsDeleted = true,
            Name = name,
            DeletedBlockCount = deletedBlockCount
        } );
    }

    #endregion
}
