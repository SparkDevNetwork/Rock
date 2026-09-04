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
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/*
    8/28/2026 - CLAUDE

    Companion to ForgeContentBuilderSkill. An authored Forge Content
    component needs data, and hunting for an existing REST endpoint is the
    worst-shaped step in that flow: Rock has hundreds of endpoints, almost
    none return the shape a specific dashboard wants, and their permissions
    are separate from the page's. Writing Lava avoids all three, so this
    skill creates the endpoint instead of searching for one.

    This skill and ForgeContentBuilderSkill were briefly merged into one
    CodeBuilderSkill during the Forge Content rename, then split again after
    a side-by-side evaluation: the focused per-skill guidance is what ships.
    The Coding Guide pointer riding the application-created
    result exists because tool results are the one channel that reaches
    every client, and weaker models read nothing that is not mandated by
    exact key.

    Reason: Lava endpoint authoring that feeds the Forge Content flow, with
    guidance delivered on channels that survive instruction drift.
*/

/// <summary>
/// Agent skill that creates and edits <see cref="LavaEndpoint"/> records so
/// an authored Forge Content component has a data source shaped for exactly
/// what it renders.
/// </summary>
[Description( "Create and edit Lava applications and endpoints that return JSON data to authored components." )]
[AgentPurpose( "Create the data endpoints an authored Forge Content component calls, by writing Lava rather than searching for an existing REST endpoint." )]
[AgentUsage( "When an authored Forge Content component needs data, create a Lava application with AddOrUpdateLavaApplication, then create its endpoints with AddOrUpdateLavaEndpoint. Do not search for an existing Rock REST endpoint first; write the Lava that returns exactly the JSON the component renders." )]
[AgentUsage( "Create one application per block, named after the dashboard, and group all of the block's endpoints under it by passing the same applicationSlug each time. Use GetLavaApplication to see what an application already contains before adding to it." )]
[AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp' and bind the application once with useLavaApp('application-slug'). Endpoints are identified by application slug, endpoint slug, and HTTP method. invoke defaults to Post; call a Get endpoint with lavaApp.invoke('endpoint-slug', parametersOrUndefined, { method: 'GET' }). Do not hand-roll the URL, CSRF header, or JSON parsing." )]
[AgentUsage( "Before reporting a component and endpoint integration as verified, compare every invoke call with the saved endpoint: application slug, endpoint slug, HTTP method, parameter location and names, and response shape must all match. Testing an endpoint independently does not verify that the component invokes it correctly." )]
[AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
[AgentUsage( "Values sent by invoke arrive in the template under the 'Body' merge field for Post endpoints and 'QueryString' for Get, never as bare merge fields. Read '{{ Body.teamId }}', not '{{ teamId }}'. A bare parameter renders as empty with no error, so a query built from one silently returns wrong data." )]
[AgentUsage( "The endpoint runs as whoever views the page, not as you. Write the template for the least-privileged viewer. AddOrUpdateLavaApplication requires an audience when creating an application and configures its read access; endpoint write modes and page or block security remain separate decisions." )]
[AgentUsage( "A deleteentity command only deletes the one entity. Child rows whose foreign key does not cascade will block it, and the failure surfaces as a foreign key error rather than anything the user can act on. Check how Rock's own code deletes that entity and remove the same children first." )]
[AgentUsage( "These tools change site configuration and can run privileged Lava. Derive the application name, endpoint slug, and enabled commands from the stated feature, inspect existing applications before adding, and ask only when an unresolved choice materially changes security or behavior." )]
[AgentUsage( "Before writing any endpoint template, call the Community Knowledge Base skill's GetKnowledgeBaseOverview tool and locate the Rock Coding Guide topic. Pass the topic key returned by the overview unchanged to GetTopic, open the root article listed by that topic, and follow the guide's own routing for the endpoint outcome. Never construct or guess a topic or article key." )]
[AgentUsage( "For every endpoint that creates or updates a Rock entity, follow the entity-write procedure selected through the Rock Coding Guide and retrieve the Contracts it names. Then find the version-matching data-model topic in the GetKnowledgeBaseOverview result, pass its returned key unchanged to GetTopic, locate the target entity article, and read it with GetArticle before authoring the write. SearchKnowledge is not authoritative evidence for entity property names and does not replace the entity article." )]
[AgentUsage( "Before authoring a modify block, make a property evidence list with every property to be written, its exact case-sensitive schema name, whether it is required or nullable, and its server-side or user-input value source. Do not call AddOrUpdateLavaEndpoint while any proposed property is absent from that evidence. Use the playbook's research process to resolve domain defaults, save behavior, picker boundaries, and authorization. The absence of a dedicated entity recipe or exhaustive contract is not a blocker." )]
[AgentUsage( "For an endpoint that retrieves and structures Rock data, follow the read procedure selected through the Rock Coding Guide and retrieve the Contracts and Reference articles it assigns." )]
[AgentUsage( "Use entity commands for everything. Reading is '{% connectionrequest where:'...' %}' with the 'RockEntity' command, adding and updating is '{% modifyconnectionrequest %}' with 'RockEntityModify', and deleting is '{% deleteconnectionrequest %}' with 'RockEntityDelete'. Substitute the entity's friendly name with the spaces removed for any other entity." )]
[AgentUsage( "Charts and totals do not justify raw SQL. Fetch rows with the entity command and group them in Lava with assign and increment, or return the rows and aggregate them in the component. Reach for a wider entity query before reaching for SQL." )]
[AgentGuardrail( "Never enable the 'Sql' Lava command without first explaining to the user why the entity commands cannot do the job and receiving their explicit approval. The tool rejects a request for 'Sql' that arrives without a sqlJustification." )]
[AgentGuardrail( "Raw SQL bypasses Rock's per-row security. An endpoint runs as whoever views the page, and '{% sql %}' returns every row the query matches regardless of that person's rights, while the entity commands filter results by them automatically. Treat SQL as a last resort that needs the user's informed consent, not a convenience." )]
[AgentSkillGuid( "71B4E9D2-C685-4A30-BF17-5D208C4E96A1" )]
[EntityTypeGuid( "94D07F3B-6E21-45C8-A5B4-1F8E3D62C079" )]
internal sealed partial class LavaApplicationBuilderSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LavaApplicationBuilderSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public LavaApplicationBuilderSkill( ILogger<LavaApplicationBuilderSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
