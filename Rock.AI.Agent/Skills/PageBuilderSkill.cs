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
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/*
    8/17/2026 - CLAUDE

    Companion to CustomComponentSkill. This skill scaffolds CMS structure so the
    agent can stand up a place for authored content: find a parent page, create
    a child page under it, and add a block to a page. The AddBlock tool returns
    the new block's IdKey, which is exactly what the CustomComponent skill's
    SetComponentSource tool needs, so the two skills compose
    (CreatePage -> AddBlock -> SetComponentSource).

    The page and block creation logic mirrors the core admin blocks
    (Administration ZoneBlocks and Pages): inherit the parent page's layout,
    place at the end of the siblings or zone, copy the parent's authorization
    onto the new record, and flush the affected page cache. These are
    structural, privileged changes, so every mutating tool is gated on
    ADMINISTRATE of the target (the parent page for CreatePage, the page for
    AddBlock).

    Reason: MCP-driven page and block scaffolding that feeds the Custom Component authoring flow.
*/

/// <summary>
/// Agent skill that creates CMS pages and adds blocks to them, so authored
/// content (for example a Custom Component block) has a place to live.
/// </summary>
[Description( "Create CMS pages and add blocks to them." )]
[AgentPurpose( "Create a new page and add blocks to pages in Rock's CMS." )]
[AgentUsage( "When the user wants a new page but does not say where it should live, ask them for the parent page, then use FindPages to locate and confirm it before calling CreatePage." )]
[AgentUsage( "When the user wants to add a block but does not say which block type, ask them which one. For vibe-coded content add the 'Custom Component' block, then call the CustomComponent skill's SetComponentSource with the block id AddBlock returns." )]
[AgentUsage( "These tools change site structure. Confirm the parent page, page name, block type, and zone with the user before creating." )]
[AgentSkillGuid( "EE27BE5A-1276-433F-A636-1BEF3550EC1E" )]
[EntityTypeGuid( "1D5FD674-F94D-4166-BC10-F2EA86412C4B" )]
internal sealed partial class PageBuilderSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilderSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public PageBuilderSkill( ILogger<PageBuilderSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
