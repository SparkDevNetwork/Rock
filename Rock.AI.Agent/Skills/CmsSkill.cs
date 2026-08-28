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

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
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
[AgentUsage( "When the user wants to add a block but does not say which block type, ask them which one, then resolve it with ListBlockTypes. For AI-authored content use the 'Forge Content' block type, then call the Forge Content Builder skill's AddOrUpdateForgeContent with the block id AddOrUpdateBlock returns." )]
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

    #endregion
}
