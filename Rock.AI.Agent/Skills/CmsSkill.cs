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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Configuration;
using Rock.Data;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/*
    8/18/2026 - CLAUDE

    The former PageSkill (SearchPages, AddPage, AddBlock) was folded into this
    skill so all CMS structure tools live in one place: sites, pages, block
    types, and blocks. The tool guids were kept, so the AISkillTool rows
    re-parent themselves to this skill on startup registration.

    The mutating tools compose with ForgeContentBuilderSkill: AddOrUpdateBlock
    returns the new block's IdKey, which is exactly what the ForgeContent
    skill's AddOrUpdateForgeContent tool needs
    (AddOrUpdatePage -> AddOrUpdateBlock -> AddOrUpdateForgeContent).

    The page and block creation logic mirrors the core admin blocks
    (Administration ZoneBlocks and Pages): inherit the parent page's layout,
    place at the end of the siblings or zone, copy the parent's authorization
    onto the new record, and flush the affected page cache. These are
    structural, privileged changes, so every mutating tool is gated on
    ADMINISTRATE of the target and carries administrator-only tool security.

    The delete tools follow the established authorization-only delete shape
    (DeleteNote, DeleteStep, DeletePrayerRequest): per-entity ADMINISTRATE
    decides what is deletable, not a provenance stamp. The one extra guard is
    that DeletePage refuses pages with children, because children cascade in
    the database and one tool call must not take down a subtree.

    Reason: One skill for CMS structure; reads for everyone, writes for administrators.
*/

/// <summary>
/// Agent skill that explores Rock's CMS structure (sites, pages, block types
/// and blocks) and creates or updates pages and blocks, so authored content
/// (for example a Forge Content block) has a place to live.
/// </summary>
[Description( "Explore and manage sites, pages, and blocks in Rock's CMS." )]
[AgentPurpose( "Explore the CMS structure of this Rock instance (sites, pages, block types, blocks) and create or update pages and blocks." )]
[AgentUsage( "When the user wants a new page but does not say where it should live, ask them for the parent page, then use SearchPages or ListPages to locate and confirm it before calling AddOrUpdatePage." )]
[AgentUsage( "When the user wants to add a block but does not say which block type, ask them which one, then resolve it with ListBlockTypes." )]
[AgentUsage( "Before adding a block to a page, call GetPage or ListBlocks to see what is already there, and update the existing block instead of adding a duplicate." )]
[AgentUsage( "The mutating tools change site structure. Confirm the parent page, page name, block type, and zone with the user before creating." )]
[AgentUsage( "Use DeletePage and DeleteBlock to clean up scratch pages and blocks when a build is abandoned. Deletes are permanent, so confirm the exact page or block with the user first. A page with child pages is refused; delete or move the children first." )]
[AgentSkillGuid( "613D7110-6453-4BAB-892B-064222F8397C" )]
[EntityTypeGuid( "7A63570D-6FC3-4573-BDF2-89CFF605D5AB" )]
internal sealed partial class CmsSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CmsSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public CmsSkill( ILogger<CmsSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Support Methods

    /// <summary>
    /// Gets the relative URL a page is reachable at: the first friendly route
    /// when one exists, otherwise the /page/id fallback.
    /// </summary>
    /// <param name="page">The cached page.</param>
    /// <returns>A relative URL string.</returns>
    private static string GetPageUrl( PageCache page )
    {
        return page.PageRoutes.Count > 0
            ? $"/{page.PageRoutes[0].Route}"
            : $"/page/{page.Id}";
    }

    /// <summary>
    /// Builds the summarized <see cref="PageResult"/> shared by the page list
    /// and search tools. Detail properties are left for GetPage to fill.
    /// </summary>
    /// <param name="page">The cached page.</param>
    /// <param name="rockContext">The context to use when accessing the cache.</param>
    /// <returns>A summarized page result.</returns>
    private static PageResult CreateSummaryPageResult( PageCache page, RockContext rockContext )
    {
        var parentPage = page.ParentPageId.HasValue
            ? PageCache.Get( page.ParentPageId.Value, rockContext )
            : null;

        return new PageResult
        {
            Id = page.Id,
            Guid = page.Guid,
            InternalName = page.InternalName,
            PageTitle = page.PageTitle,
            SiteName = page.Site,
            Url = GetPageUrl( page ),
            ParentPage = parentPage != null
                ? new KeyNameResult { Id = parentPage.Id, Name = parentPage.InternalName }
                : null,
            ChildPageCount = page.GetPages( rockContext ).Count
        };
    }

    /// <summary>
    /// Builds the summarized <see cref="BlockResult"/> shared by GetPage and
    /// ListBlocks.
    /// </summary>
    /// <param name="block">The cached block.</param>
    /// <param name="rockContext">The context to use when accessing the cache.</param>
    /// <returns>A summarized block result.</returns>
    private static BlockResult CreateSummaryBlockResult( BlockCache block, RockContext rockContext )
    {
        var blockType = block.BlockType;
        var page = block.PageId.HasValue
            ? PageCache.Get( block.PageId.Value, rockContext )
            : null;

        return new BlockResult
        {
            Id = block.Id,
            Guid = block.Guid,
            Name = block.Name,
            Zone = block.Zone,
            Order = block.Order,
            Location = block.PageId.HasValue
                ? "Page"
                : block.LayoutId.HasValue ? "Layout" : "Site",
            BlockType = blockType != null
                ? new BlockTypeResult
                {
                    Id = blockType.Id,
                    Guid = blockType.Guid,
                    Name = blockType.Name,
                    Category = blockType.Category
                }
                : null,
            PageUrl = page != null ? GetPageUrl( page ) : null
        };
    }

    /*
        8/31/2026 - CLAUDE

        Zones are <Rock:Zone> controls declared in a layout's .aspx and in the
        theme's Site.Master. The only runtime enumeration of them is RockPage
        walking its own control tree, which needs a live WebForms page, so
        reading the two files is the closest thing available to a tool. Without
        it the agent has no way to know which zones exist and guesses a name,
        which is how layout names ended up being passed as zones.

        Reason: The agent had no way to know which zones a layout actually has.
    */

    /// <summary>
    /// Matches a zone declaration in a layout or master page file.
    /// </summary>
    private static readonly Regex _zoneDeclarationRegex = new Regex( @"<Rock:Zone\s[^>]*?Name\s*=\s*""(?<name>[^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Compiled );

    /// <summary>
    /// Gets the zones a layout renders, read from the layout's own file and
    /// the theme master page every layout in the theme inherits from.
    /// </summary>
    /// <param name="layout">The cached layout.</param>
    /// <returns>The zones, or an empty list when the layout's files cannot be read.</returns>
    private static List<ZoneResult> GetLayoutZones( LayoutCache layout )
    {
        var zones = new List<ZoneResult>();
        var theme = layout?.Site?.Theme;

        if ( theme.IsNullOrWhiteSpace() || layout.FileName.IsNullOrWhiteSpace() )
        {
            return zones;
        }

        var webRootPath = RockApp.Current.HostingSettings.WebRootPath;

        if ( webRootPath.IsNullOrWhiteSpace() )
        {
            return zones;
        }

        var layoutDirectory = Path.Combine( webRootPath, "Themes", theme, "Layouts" );
        var names = new List<string>();

        AddZoneNames( Path.Combine( layoutDirectory, $"{layout.FileName}.aspx" ), names );

        // The theme master page holds the zones shared by every layout in the
        // theme, such as Header and Footer, so it has to be read as well.
        AddZoneNames( Path.Combine( layoutDirectory, "Site.Master" ), names );

        foreach ( var name in names )
        {
            // A block stores its zone with the spaces removed, which is how
            // RockPage matches a block to the zone control it renders in.
            var key = name.Replace( " ", string.Empty );

            if ( key.IsNullOrWhiteSpace() || zones.Any( z => z.Name.Equals( key, StringComparison.OrdinalIgnoreCase ) ) )
            {
                continue;
            }

            zones.Add( new ZoneResult
            {
                Name = key,
                DisplayName = key != name ? name : null
            } );
        }

        return zones;
    }

    /// <summary>
    /// Adds the zone names declared in one layout or master page file to the
    /// list, in the order they are declared.
    /// </summary>
    /// <param name="filePath">The physical path of the file to read.</param>
    /// <param name="names">The list to add the zone names to.</param>
    private static void AddZoneNames( string filePath, List<string> names )
    {
        if ( !File.Exists( filePath ) )
        {
            return;
        }

        string markup;

        try
        {
            markup = File.ReadAllText( filePath );
        }
        catch
        {
            // Intentionally ignored: zone discovery is advisory, so a theme
            // file that cannot be read should not fail the whole tool call.
            return;
        }

        foreach ( Match match in _zoneDeclarationRegex.Matches( markup ) )
        {
            names.Add( match.Groups["name"].Value );
        }
    }

    /// <summary>
    /// Gets the zones a block placed against a page, layout or site can render
    /// in. A site block renders on every layout of the site, so every one of
    /// those layouts contributes its zones.
    /// </summary>
    /// <param name="pageId">The page the block is placed on, if any.</param>
    /// <param name="layoutId">The layout the block is placed on, if any.</param>
    /// <param name="siteId">The site the block is placed on, if any.</param>
    /// <param name="rockContext">The context to use when accessing the cache.</param>
    /// <returns>The zones, or an empty list when they cannot be determined.</returns>
    private static List<ZoneResult> GetPlacementZones( int? pageId, int? layoutId, int? siteId, RockContext rockContext )
    {
        if ( pageId.HasValue )
        {
            return GetLayoutZones( PageCache.Get( pageId.Value, rockContext )?.Layout );
        }

        if ( layoutId.HasValue )
        {
            return GetLayoutZones( LayoutCache.Get( layoutId.Value, rockContext ) );
        }

        if ( !siteId.HasValue )
        {
            return new List<ZoneResult>();
        }

        var siteZones = new List<ZoneResult>();

        foreach ( var layout in LayoutCache.All( rockContext ).Where( l => l.SiteId == siteId.Value ) )
        {
            foreach ( var zone in GetLayoutZones( layout ) )
            {
                if ( !siteZones.Any( z => z.Name.Equals( zone.Name, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    siteZones.Add( zone );
                }
            }
        }

        return siteZones;
    }

    /// <summary>
    /// Resolves the zone a block should be placed in, checking it against the
    /// zones the layout actually declares. Zones cannot be verified when the
    /// theme files are unreadable, in which case the requested name is taken
    /// as given.
    /// </summary>
    /// <param name="requestedZone">The zone name the caller asked for.</param>
    /// <param name="zones">The zones available at the placement.</param>
    /// <param name="helper">The helper to report a bad zone name to.</param>
    /// <returns>The zone name to store on the block.</returns>
    private static string ResolveZoneName( string requestedZone, List<ZoneResult> zones, AgentToolHelper helper )
    {
        // A block stores its zone with the spaces removed, so a display name
        // such as "Badge Bar" still resolves to the "BadgeBar" the page renders.
        var normalizedZone = requestedZone.Replace( " ", string.Empty );

        if ( !zones.Any() )
        {
            return normalizedZone;
        }

        var match = zones.FirstOrDefault( z => z.Name.Equals( normalizedZone, StringComparison.OrdinalIgnoreCase ) );

        if ( match == null )
        {
            var zoneNames = zones.Select( z => z.Name ).ToList().AsDelimited( ", ", " and " );

            helper.AddError( $"'{requestedZone}' is not one of that layout's zones. A zone is a region within a layout, not a layout itself. The zones available here are {zoneNames}." );

            return null;
        }

        return match.Name;
    }

    #endregion
}
