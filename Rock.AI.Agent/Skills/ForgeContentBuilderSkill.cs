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
    The guidance riders on the tool results (the Coding Guide pointer on
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
[AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp' and bind the application once with useLavaApp('application-slug'). Endpoints are identified by application slug, endpoint slug, and HTTP method. invoke defaults to Post; call a Get endpoint with lavaApp.invoke('endpoint-slug', parametersOrUndefined, { method: 'GET' }). Never hand-roll the endpoint URL, CSRF header, or JSON parsing." )]
[AgentUsage( "Before reporting a component and endpoint integration as verified, compare every invoke call with the saved endpoint: application slug, endpoint slug, HTTP method, parameter location and names, and response shape must all match. Testing an endpoint independently does not verify that the component invokes it correctly." )]
[AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, show errorMessage when it fails, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
[AgentUsage( "Before writing a component, call the Community Knowledge Base skill's GetKnowledgeBaseOverview tool and locate the Rock Coding Guide topic. Pass the topic key returned by the overview unchanged to GetTopic, open the root article listed by that topic, and follow the guide's own routing for the requested outcome. Retrieve only the supporting material assigned by the selected Playbook. Never construct or guess a topic or article key." )]
[AgentUsage( "For control, Grid, field-type, configuration, or value-shape questions, follow the lookup route provided by the Rock Coding Guide. Verify only the focused Reference articles needed by the component." )]
[AgentUsage( "For a control or API the guide does not cover, find it with the Community Knowledge Base skill's SearchCode tool, passing sourceType 'obs'. Search by concept, for example 'person picker' or 'grid with columns', rather than by a guessed filename." )]
[AgentUsage( "Read an uncovered control's real API with GetCodeFile, passing the documentId returned with each search result. The defineProps block is the authoritative list of props, their types, and their defaults, and the JSDoc comments above them explain what each one does. Never infer a control's props from its name or from a different control." )]
[AgentUsage( "Call GetRockVersion first, then call GetKnowledgeBaseOverview once before the first knowledge base lookup. The knowledge base tools automatically scope lookups to the connected Rock version; do not try to pass a version argument they do not accept. If a prop you found does not exist when the source fails to compile, suspect a version mismatch before anything else. When no source code is indexed for this release, use another indexed release only as a disclosed comparison rather than silently treating it as the current contract." )]
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
