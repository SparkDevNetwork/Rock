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
//

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    /*
        7/23/2026 - CLAUDE

        Companion to ObsidianVibeCodingSkill. This skill scaffolds CMS structure so the
        agent can stand up a place for authored content: find a parent page, create a
        child page under it, and add a block to a page. The AddBlock tool returns the new
        block's IdKey, which is exactly what ObsidianVibeCodingSkill.SetContentSource needs,
        so the two skills compose (CreatePage -> AddBlock -> SetContentSource).

        The page and block creation logic mirrors the core admin blocks (Administration
        ZoneBlocks and Pages): inherit the parent page's layout, place at the end of the
        siblings/zone, copy the parent's authorization onto the new record, and flush the
        affected page cache. These are structural, privileged changes, so every mutating
        tool is gated on ADMINISTRATE of the target (parent page for CreatePage, page for
        AddBlock).

        Reason: MCP-driven page and block scaffolding that feeds the vibe-coding flow.
    */

    /// <summary>
    /// Agent skill that creates CMS pages and adds blocks to them, so authored content
    /// (for example an Obsidian Content block) has a place to live.
    /// </summary>
    [Description( "Create CMS pages and add blocks to them." )]
    [AgentSkillName( "PageBuilder" )]
    [AgentPurpose( "Create a new page and add blocks to pages in Rock's CMS." )]
    [AgentUsage( "When the user wants a new page but does not say where it should live, ask them for the parent page, then use FindPages to locate and confirm it before calling CreatePage." )]
    [AgentUsage( "When the user wants to add a block but does not say which block type, ask them which one. For vibe-coded content add the 'Obsidian Content Detail' block, then call the ObsidianVibeCoding skill's SetContentSource with the block id AddBlock returns." )]
    [AgentUsage( "These tools change site structure. Confirm the parent page, page name, block type, and zone with the user before creating." )]
    [Rock.SystemGuid.EntityTypeGuid( "1D5FD674-F94D-4166-BC10-F2EA86412C4B" )]
    [Rock.SystemGuid.AgentSkillGuid( "EE27BE5A-1276-433F-A636-1BEF3550EC1E" )]
    internal class PageBuilderSkill : AgentSkillComponent
    {
        #region Tools

        /// <summary>
        /// Finds pages by a partial name match so the agent can resolve and confirm a
        /// parent page with the user before creating a child page.
        /// </summary>
        /// <param name="query">A partial page name to search for.</param>
        /// <returns>The matching pages the current person can view.</returns>
        [AgentToolName( "FindPages" )]
        [AgentToolPreamble( "Looking up pages." )]
        [AgentUsage( "query is matched against the page's internal name and title. Returns the guid to pass to CreatePage or AddBlock." )]
        [Rock.SystemGuid.AgentToolGuid( "C668CAE0-CFA7-4AFF-87FF-5025860170BA" )]
        public AgentToolResult FindPages( string query )
        {
            var person = AgentRequestContext.CurrentPerson;

            var matches = PageCache.All()
                .Where( p => query.IsNullOrWhiteSpace()
                    || ( p.InternalName != null && p.InternalName.IndexOf( query, System.StringComparison.OrdinalIgnoreCase ) >= 0 )
                    || ( p.PageTitle != null && p.PageTitle.IndexOf( query, System.StringComparison.OrdinalIgnoreCase ) >= 0 ) )
                .Where( p => p.IsAuthorized( Authorization.VIEW, person ) )
                .OrderBy( p => p.InternalName )
                .Take( 25 )
                .Select( p => new
                {
                    p.Guid,
                    p.InternalName,
                    p.PageTitle,
                    Site = p.Layout?.Site?.Name
                } )
                .ToList();

            if ( !matches.Any() )
            {
                return NoData();
            }

            return Success( matches );
        }

        /// <summary>
        /// Creates a new child page under the specified parent page, inheriting the
        /// parent's layout (and therefore its site and zones).
        /// </summary>
        /// <param name="parentPage">The parent page identifier (guid or id).</param>
        /// <param name="name">The name of the new page.</param>
        /// <param name="route">An optional kebab-case route for the page, such as "serving-dashboard", so it is reachable at a friendly URL rather than only /page/id.</param>
        /// <returns>The new page's identifiers and URL, or an error.</returns>
        [AgentToolName( "CreatePage" )]
        [AgentToolPreamble( "Creating the page." )]
        [AgentUsage( "parentPage is where the new page lives; ask the user for it if not specified. The new page inherits the parent's layout." )]
        [AgentUsage( "Pass route so the page gets a friendly kebab-case URL ('serving-dashboard'). Without it the page is only reachable at /page/id." )]
        [Rock.SystemGuid.AgentToolGuid( "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633" )]
        public AgentToolResult CreatePage( string parentPage, string name, string route = null )
        {
            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( "A page name is required." );
            }

            using ( var rockContext = new RockContext() )
            {
                var pageService = new PageService( rockContext );
                var parent = pageService.Get( parentPage, allowIntegerIdentifier: true );

                if ( parent == null )
                {
                    return Error( "The parent page was not found. Use FindPages to locate it." );
                }

                var parentCache = PageCache.Get( parent.Id );

                if ( parentCache == null || !parentCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
                {
                    return Error( "You are not authorized to add a page under that parent." );
                }

                // Validate the route before the page exists, so a bad route is a clean
                // error rather than a page created without the URL the caller asked for.
                var normalizedRoute = route.IsNotNullOrWhiteSpace() ? route.Trim().TrimStart( '/' ) : null;

                if ( normalizedRoute != null )
                {
                    if ( normalizedRoute.Length == 0 || normalizedRoute.Contains( " " ) )
                    {
                        return Error( "The route must be a URL path with no spaces, such as 'serving-dashboard'." );
                    }

                    if ( new PageRouteService( rockContext ).Queryable().Any( r => r.Route == normalizedRoute ) )
                    {
                        return Error( $"The route '{normalizedRoute}' is already used by another page. Choose a different route." );
                    }
                }

                var page = new Rock.Model.Page
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
                    return Error( page.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The page could not be created." );
                }

                rockContext.SaveChanges();

                if ( normalizedRoute != null )
                {
                    var pageRoute = new PageRoute
                    {
                        PageId = page.Id,
                        Route = normalizedRoute
                    };

                    new PageRouteService( rockContext ).Add( pageRoute );

                    if ( !pageRoute.IsValid )
                    {
                        return Error( pageRoute.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? $"The route '{normalizedRoute}' could not be created." );
                    }

                    rockContext.SaveChanges();

                    // The routing table is only rebuilt when told about the new route; without
                    // this the friendly URL 404s until the next application restart.
                    Rock.Bus.Message.PageRouteWasUpdatedMessage.Publish();
                }

                // A new child page inherits the parent page's authorization.
                Authorization.CopyAuthorization( parent, page, rockContext );

                // Flush the parent so the new child appears in navigation.
                PageCache.Remove( parent.Id );

                var result = Success( new
                {
                    page.Guid,
                    page.IdKey,
                    Name = page.InternalName,
                    Url = normalizedRoute != null ? $"/{normalizedRoute}" : $"/page/{page.Id}"
                } );

                if ( normalizedRoute == null )
                {
                    result.WithInstructions( $"The page was created without a route, so it is only reachable at /page/{page.Id}. Pass a kebab-case route to CreatePage next time, or tell the user the page has no friendly URL until one is added through Page Properties." );
                }

                return result;
            }
        }

        /// <summary>
        /// Adds a block to a page in the specified zone.
        /// </summary>
        /// <param name="page">The page identifier (guid or id) to add the block to.</param>
        /// <param name="blockType">The block type name or guid to add.</param>
        /// <param name="zone">The zone to place the block in. Defaults to "Main".</param>
        /// <param name="name">An optional name for the block. Defaults to the block type name.</param>
        /// <returns>The new block's IdKey (for use with SetContentSource), or an error.</returns>
        [AgentToolName( "AddBlock" )]
        [AgentToolPreamble( "Adding the block to the page." )]
        [AgentUsage( "blockType is the block type name or guid; ask the user which block if not specified. Returns the blockId to pass to the ObsidianVibeCoding SetContentSource tool." )]
        [Rock.SystemGuid.AgentToolGuid( "05C9C108-4516-46B7-85FB-5C8FE6212CCF" )]
        public AgentToolResult AddBlock( string page, string blockType, string zone = null, string name = null )
        {
            if ( blockType.IsNullOrWhiteSpace() )
            {
                return Error( "A block type is required. Ask the user which block to add." );
            }

            // Resolve the block type by guid first, then by name.
            var blockTypeCache = BlockTypeCache.Get( blockType.AsGuid() )
                ?? BlockTypeCache.All().FirstOrDefault( bt => bt.Name.Equals( blockType, System.StringComparison.OrdinalIgnoreCase ) );

            if ( blockTypeCache == null )
            {
                /*
                    8/11/2026 - CLAUDE

                    The name match is exact, so a near miss ("Obsidian Content" for
                    "Obsidian Content Detail") used to fail with nothing to go on, and the
                    agent's only recovery was guessing again. Suggesting the closest names
                    turns the retry into a selection.

                    Reason: An exact-match failure gave the agent no path to the right name.
                */
                var suggestions = BlockTypeCache.All()
                    .Where( bt => bt.Name != null && bt.Name.IndexOf( blockType, System.StringComparison.OrdinalIgnoreCase ) >= 0 )
                    .OrderBy( bt => bt.Name.Length )
                    .Take( 5 )
                    .Select( bt => bt.Name )
                    .ToList();

                return Error( suggestions.Any()
                    ? $"No block type is named exactly '{blockType}'. Close matches: {string.Join( ", ", suggestions )}. Call again with one of these exact names."
                    : $"No block type matched '{blockType}'." );
            }

            using ( var rockContext = new RockContext() )
            {
                var pageService = new PageService( rockContext );
                var targetPage = pageService.Get( page, allowIntegerIdentifier: true );

                if ( targetPage == null )
                {
                    return Error( "The page was not found. Use FindPages to locate it." );
                }

                var pageCache = PageCache.Get( targetPage.Id );

                if ( pageCache == null || !pageCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
                {
                    return Error( "You are not authorized to add blocks to that page." );
                }

                var blockService = new BlockService( rockContext );

                var block = new Block
                {
                    PageId = targetPage.Id,
                    Zone = zone.IsNullOrWhiteSpace() ? "Main" : zone,
                    BlockTypeId = blockTypeCache.Id,
                    Name = name.IsNullOrWhiteSpace() ? blockTypeCache.Name : name
                };

                blockService.Add( block );

                // Place the new block at the end of its zone.
                block.Order = blockService.GetMaxOrder( block );

                rockContext.SaveChanges();

                // A new block inherits the page's authorization rules.
                Authorization.CopyAuthorization( targetPage, block, rockContext );

                // Flush the page so the new block renders.
                PageCache.Remove( targetPage.Id );

                return Success( new
                {
                    BlockId = block.IdKey,
                    block.Guid,
                    block.Zone,
                    PageUrl = $"/page/{targetPage.Id}"
                } );
            }
        }

        #endregion Tools
    }
}
