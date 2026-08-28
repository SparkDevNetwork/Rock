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
    8/28/2026 - CLAUDE

    Exposes the Forge Content authoring loop as agent tools so an AI client
    can read and write the Vue source of a ForgeContentDetail block
    placement. Writes go through the same server-side compile the block's own
    save action uses (see Rock.Cms.ForgeContentCompiler): a failed compile
    stores nothing and returns the compiler's errors.

    This skill and LavaApplicationBuilderSkill were briefly merged into one
    CodeBuilderSkill during the Forge Content rename, then split again after
    a side-by-side evaluation: the focused per-skill guidance is what ships.
    The guidance riders on the tool results (the composition-rules pointer on
    GetRockVersion, the self-verify instruction on AddOrUpdateForgeContent)
    exist because tool results are the one channel that reaches every client,
    including third-party MCP clients that never see seeded instructions, and
    weaker models read nothing that is not mandated by exact key.

    Control discovery is delegated to the Community Knowledge Base skill,
    which is attached to the same agents by the seeding migration.

    Reason: Forge Content authoring on the one server compile path, with
    guidance delivered on channels that survive instruction drift.
*/

/// <summary>
/// Agent skill that lets an authorized administrator author the source of a
/// Forge Content block placement through the agent, rather than through the
/// block's in-place editor.
/// </summary>
[Description( "Author and edit the Vue source rendered by a Forge Content block placement." )]
[AgentPurpose( "Author and edit the Vue source rendered by a Forge Content block placement." )]
[AgentUsage( "Use to read or replace the authored source of a Forge Content block the user is building. The block must already exist on a page; identify it by its block id. To create one, use the Cms skill: resolve the block type with ListBlockTypes, place it with AddOrUpdateBlock, then author it here with AddOrUpdateForgeContent." )]
[AgentUsage( "When the component needs data, create a Lava application with the Lava Application Builder skill's AddOrUpdateLavaApplication tool, then its endpoints with AddOrUpdateLavaEndpoint. Do not search Rock for an existing REST endpoint: writing Lava lets you return exactly the shape the component renders, with permissions decided when the endpoint is created." )]
[AgentUsage( "Group all of one block's endpoints under that single Lava application, named after the dashboard, by passing the same applicationSlug to every AddOrUpdateLavaEndpoint call. Security and configuration rigging are then set once for the whole block." )]
[AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp', bind the application once with useLavaApp('application-slug'), then call lavaApp.invoke('endpoint-slug'). Never hand-roll the endpoint URL, the CSRF header, or the JSON parsing: the helper is a framework import so a fix there reaches components that are already compiled and stored." )]
[AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, show errorMessage when it fails, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
[AgentUsage( "Before writing a component, read the Composition Rules article with the Community Knowledge Base skill's GetArticle tool, articleKey 'coding-guide/conventions-and-guardrails/composition-rules'. It decides which control to use, in which mode, composed how. When the design matches one of the guide's recipes, read that recipe too and follow its Composition table; a recipe never overrides the rules." )]
[AgentUsage( "Verify every control you plan to use in the Coding Guide's Controls Catalog: GetArticle with articleKey 'coding-guide/controls' lists every top-level control, and a linked control's child article (for example 'coding-guide/controls/context-slicer') carries its verified props, v-model type, and gotchas. The catalog is authoritative for the controls it covers regardless of which Rock releases have source code indexed, so a missing code index is never by itself a reason to stop building." )]
[AgentUsage( "For a control or API the guide does not cover, find it with the Community Knowledge Base skill's SearchCode tool, passing sourceType 'obs'. Search by concept, for example 'person picker' or 'grid with columns', rather than by a guessed filename." )]
[AgentUsage( "Read an uncovered control's real API with GetCodeFile, passing the documentId returned with each search result. The defineProps block is the authoritative list of props, their types, and their defaults, and the JSDoc comments above them explain what each one does. Never infer a control's props from its name or from a different control." )]
[AgentUsage( "Call GetRockVersion first and pass that version to every knowledge base lookup. The knowledge base is scoped per Rock release, so an unscoped query answers for a release this instance may not be running. If a prop you found does not exist when the source fails to compile, suspect a version mismatch before anything else. When no source code is indexed for this release, fall back to the newest indexed release for code lookups and say you did; the Coding Guide's articles still apply either way." )]
[AgentUsage( "Controls under Framework/Controls/Internal/ are internal to Rock and are not meant for authored content. Prefer a top-level control, and if only an Internal one fits, tell the user before you use it." )]
[AgentUsage( "If the knowledge base is not available to you, say so and ask the user how to proceed. Do not guess a control's props, and do not fall back to writing plain HTML in place of a Rock control without telling the user that is what you are doing." )]
[AgentSkillGuid( "0F3D6B8A-52C1-4E97-A6D3-84B2E7F91C05" )]
[EntityTypeGuid( "6C2E94D7-1B58-4A3F-9E60-D74A5C813F29" )]
internal sealed partial class ForgeContentBuilderSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgeContentBuilderSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public ForgeContentBuilderSkill( ILogger<ForgeContentBuilderSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
