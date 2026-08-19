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

    Exposes the Custom Component authoring loop as MCP tools so an external AI
    client can read and write the Vue source of a CustomComponentDetail block
    placement. Writes go through the same server-side compile the block's own
    save action uses (see Rock.Cms.CustomComponentCompiler): a failed compile
    stores nothing and returns the compiler's errors, because a saved-but-blank
    block with no error anywhere is the exact failure this feature exists to
    prevent.

    Control discovery is delegated to the Community Knowledge Base skill rather
    than reimplemented here. That service already indexes every Framework
    Controls .obs file with a semantic description, per release version scoping,
    and readable source, which is the search-then-fetch shape an agent needs and
    is expensive to reproduce. The skill is attached to both vibe agents by the
    same migration that seeds this one, so the usages below name its tools
    directly; an earlier iteration relied on the client connecting
    knowledge.rockrms.com as a second MCP server, which is no longer required.
    The fallback remains stated explicitly so an agent whose knowledge base
    tools are failing says so rather than inventing control APIs.

    GetRockVersion exists to make those lookups version-correct. The knowledge
    base is scoped per Rock release, so an unscoped query silently answers for
    whatever release that service considers current.

    Reason: MCP-driven authoring of Custom Components on the one server compile path.
*/

/// <summary>
/// Agent skill that lets an authorized administrator author the source of a
/// Custom Component block placement through the agent, rather than through the
/// block's in-place editor.
/// </summary>
[Description( "Author and edit the Vue source rendered by a Custom Component block placement." )]
[AgentPurpose( "Author and edit the Vue source rendered by a Custom Component block placement." )]
[AgentUsage( "Use to read or replace the authored source of a Custom Component block the user is building. The block must already exist on a page; identify it by its block id. To create one, use the Cms skill: resolve the block type with ListBlockTypes, place it with AddOrUpdateBlock, then author it here with AddOrUpdateCustomComponent." )]
[AgentUsage( "When the component needs data, create a Lava application with the Lava Application skill's AddOrUpdateLavaApplication tool, then its endpoints with AddOrUpdateLavaEndpoint. Do not search Rock for an existing REST endpoint: writing Lava lets you return exactly the shape the component renders, with permissions decided when the endpoint is created." )]
[AgentUsage( "Group all of one block's endpoints under that single Lava application, named after the dashboard, by passing the same applicationSlug to every AddOrUpdateLavaEndpoint call. Security and configuration rigging are then set once for the whole block." )]
[AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp', bind the application once with useLavaApp('application-slug'), then call lavaApp.invoke('endpoint-slug'). Never hand-roll the endpoint URL, the CSRF header, or the JSON parsing: the helper is a framework import so a fix there reaches components that are already compiled and stored." )]
[AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, show errorMessage when it fails, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
[AgentUsage( "Before writing a component, find the controls you need with the Community Knowledge Base skill's SearchCode tool, passing sourceType 'obs'. Search by concept, for example 'person picker' or 'grid with columns', rather than by a guessed filename." )]
[AgentUsage( "Read a control's real API with GetCodeFile, passing the documentId returned with each search result. The defineProps block is the authoritative list of props, their types, and their defaults, and the JSDoc comments above them explain what each one does. Never infer a control's props from its name or from a different control." )]
[AgentUsage( "Call GetRockVersion first and pass that version to every knowledge base lookup. The knowledge base is scoped per Rock release, so an unscoped query answers for a release this instance may not be running. If a prop you found does not exist when the source fails to compile, suspect a version mismatch before anything else." )]
[AgentUsage( "Controls under Framework/Controls/Internal/ are internal to Rock and are not meant for authored content. Prefer a top-level control, and if only an Internal one fits, tell the user before you use it." )]
[AgentUsage( "If the knowledge base is not available to you, say so and ask the user how to proceed. Do not guess a control's props, and do not fall back to writing plain HTML in place of a Rock control without telling the user that is what you are doing." )]
[AgentSkillGuid( "647770A9-F3D7-4924-B046-5C9C43959ECB" )]
[EntityTypeGuid( "4C833FA4-A7EF-4D49-9549-B24CBB629A73" )]
internal sealed partial class CustomComponentSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomComponentSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public CustomComponentSkill( ILogger<CustomComponentSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
