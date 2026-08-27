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

    Exposes the Forge Content authoring loop as MCP tools so an external AI
    client can read and write the Vue source of a ForgeContentDetail block
    placement. Writes go through the same server-side compile the block's own
    save action uses (see Rock.Cms.ForgeContentCompiler): a failed compile
    stores nothing and returns the compiler's errors, because a saved-but-blank
    block with no error anywhere is the exact failure this feature exists to
    prevent.

    Control discovery is delegated to the Community Knowledge Base skill rather
    than reimplemented here. That service already indexes every Framework
    Controls .obs file with a semantic description, per release version scoping,
    and readable source, which is the search-then-fetch shape an agent needs and
    is expensive to reproduce. The skill is attached to both Code Composer
    agents by the same migration that seeds this one, so the usages below name
    its tools directly; an earlier iteration relied on the client connecting
    knowledge.rockrms.com as a second MCP server, which is no longer required.
    The fallback remains stated explicitly so an agent whose knowledge base
    tools are failing says so rather than inventing control APIs.

    GetRockVersion exists to make those lookups version-correct. The knowledge
    base is scoped per Rock release, so an unscoped query silently answers for
    whatever release that service considers current.

    Reason: MCP-driven authoring of Forge Content on the one server compile path.
*/

/*
    8/27/2026 - CLAUDE

    The Composition Rules article (retrieval key
    coding-guide/conventions-and-guardrails/composition-rules) is the normative
    reference for which control to use, in which mode, composed how. The pointer
    to it is delivered twice on purpose: in the usages below, and on the
    GetRockVersion result. Seeded agent instructions do not reach third-party
    MCP clients, and models skip vague multi-hop lookups, so the pointer names
    the exact article key on channels that always land in the client's context.

    Reason: Steer every client to the composition rules before it authors UI.
*/

/*
    8/27/2026 - CLAUDE

    This skill is the merge of the former CustomComponentSkill and
    LavaApplicationSkill, renamed in the same pass that renamed Custom
    Component to Forge Content. They were combined because neither half stood
    alone: every authored component needs endpoints and every endpoint exists
    for a component, the two skills were attached to exactly the same agents,
    and the split forced cross-skill references in usage text. The merged
    skill keeps the former CustomComponentSkill's AgentSkillGuid and
    EntityType guid so the seeded AISkill row carries forward; the Lava tool
    rows re-parent onto it by guid in the seeding migration.

    Reason: One skill for one authoring loop, surviving the Forge Content rename.
*/

/// <summary>
/// Agent skill that lets an authorized administrator build a Forge Content
/// component end to end: the Vue source the block renders, and the Lava
/// applications and endpoints that feed it data.
/// </summary>
[Description( "Author the Vue source rendered by a Forge Content block placement and the Lava applications and endpoints that feed it data." )]
[AgentPurpose( "Build Forge Content components end to end: author the Vue source a Forge Content block renders, and create the Lava endpoints that return exactly the JSON the component needs." )]
[AgentUsage( "Use to read or replace the authored source of a Forge Content block the user is building. The block must already exist on a page; identify it by its block id. To create one, use the Cms skill: resolve the block type with ListBlockTypes, place it with AddOrUpdateBlock, then author it here with AddOrUpdateForgeContent." )]
[AgentUsage( "When the component needs data, create a Lava application with AddOrUpdateLavaApplication, then its endpoints with AddOrUpdateLavaEndpoint. Do not search Rock for an existing REST endpoint: writing Lava lets you return exactly the shape the component renders, with permissions decided when the endpoint is created." )]
[AgentUsage( "Group all of one block's endpoints under that single Lava application, named after the dashboard, by passing the same applicationSlug to every AddOrUpdateLavaEndpoint call. Security and configuration rigging are then set once for the whole block. Use GetLavaApplication to see what an application already contains before adding to it." )]
[AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp', bind the application once with useLavaApp('application-slug'), then call lavaApp.invoke('endpoint-slug'). Never hand-roll the endpoint URL, the CSRF header, or the JSON parsing: the helper is a framework import so a fix there reaches components that are already compiled and stored." )]
[AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, show errorMessage when it fails, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
[AgentUsage( "Values sent by invoke arrive in the template under the 'Body' merge field for Post endpoints and 'QueryString' for Get, never as bare merge fields. Read '{{ Body.teamId }}', not '{{ teamId }}'. A bare parameter renders as empty with no error, so a query built from one silently returns wrong data." )]
[AgentUsage( "The endpoint runs as whoever views the page, not as you. Write the template for the least-privileged viewer, and remember that a newly created application has no security rules until an administrator adds them." )]
[AgentUsage( "Use entity commands for everything. Reading is '{% connectionrequest where:'...' %}' with the 'RockEntity' command, adding and updating is '{% modifyconnectionrequest %}' with 'RockEntityModify', and deleting is '{% deleteconnectionrequest %}' with 'RockEntityDelete'. Substitute the entity's friendly name with the spaces removed for any other entity." )]
[AgentUsage( "Charts and totals do not justify raw SQL. Fetch rows with the entity command and group them in Lava with assign and increment, or return the rows and aggregate them in the component. Reach for a wider entity query before reaching for SQL." )]
[AgentUsage( "A deleteentity command only deletes the one entity. Child rows whose foreign key does not cascade will block it, and the failure surfaces as a foreign key error rather than anything the user can act on. Check how Rock's own code deletes that entity and remove the same children first." )]
[AgentUsage( "These tools change site configuration and can run privileged Lava. Confirm the application name, endpoint slug, and enabled Lava commands with the user before creating." )]
[AgentUsage( "Before writing a component, read the Composition Rules article with the Community Knowledge Base skill's GetArticle tool, articleKey 'coding-guide/conventions-and-guardrails/composition-rules'. It decides which control to use, in which mode, composed how. When the design matches one of the guide's recipes, read that recipe too and follow its Composition table; a recipe never overrides the rules." )]
[AgentUsage( "Then find the controls you need with the Community Knowledge Base skill's SearchCode tool, passing sourceType 'obs'. Search by concept, for example 'person picker' or 'grid with columns', rather than by a guessed filename." )]
[AgentUsage( "Read a control's real API with GetCodeFile, passing the documentId returned with each search result. The defineProps block is the authoritative list of props, their types, and their defaults, and the JSDoc comments above them explain what each one does. Never infer a control's props from its name or from a different control." )]
[AgentUsage( "Call GetRockVersion first and pass that version to every knowledge base lookup. The knowledge base is scoped per Rock release, so an unscoped query answers for a release this instance may not be running. If a prop you found does not exist when the source fails to compile, suspect a version mismatch before anything else." )]
[AgentUsage( "Controls under Framework/Controls/Internal/ are internal to Rock and are not meant for authored content. Prefer a top-level control, and if only an Internal one fits, tell the user before you use it." )]
[AgentUsage( "If the knowledge base is not available to you, say so and ask the user how to proceed. Do not guess a control's props, and do not fall back to writing plain HTML in place of a Rock control without telling the user that is what you are doing." )]
[AgentGuardrail( "Never enable the 'Sql' Lava command without first explaining to the user why the entity commands cannot do the job and receiving their explicit approval. The tool rejects a request for 'Sql' that arrives without a sqlJustification." )]
[AgentGuardrail( "Raw SQL bypasses Rock's per-row security. An endpoint runs as whoever views the page, and '{% sql %}' returns every row the query matches regardless of that person's rights, while the entity commands filter results by them automatically. Treat SQL as a last resort that needs the user's informed consent, not a convenience." )]
[AgentSkillGuid( "647770A9-F3D7-4924-B046-5C9C43959ECB" )]
[EntityTypeGuid( "4C833FA4-A7EF-4D49-9549-B24CBB629A73" )]
internal sealed partial class CodeBuilderSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBuilderSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public CodeBuilderSkill( ILogger<CodeBuilderSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
