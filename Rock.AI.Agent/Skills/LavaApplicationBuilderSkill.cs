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
    The writing-endpoint-lava article pointer riding the application-created
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
[AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp', bind the application once with useLavaApp('application-slug'), then call lavaApp.invoke('endpoint-slug'). Do not hand-roll the URL, the CSRF header, or the JSON parsing." )]
[AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
[AgentUsage( "Values sent by invoke arrive in the template under the 'Body' merge field for Post endpoints and 'QueryString' for Get, never as bare merge fields. Read '{{ Body.teamId }}', not '{{ teamId }}'. A bare parameter renders as empty with no error, so a query built from one silently returns wrong data." )]
[AgentUsage( "The endpoint runs as whoever views the page, not as you. Write the template for the least-privileged viewer, and remember that a newly created application has no security rules until an administrator adds them." )]
[AgentUsage( "A deleteentity command only deletes the one entity. Child rows whose foreign key does not cascade will block it, and the failure surfaces as a foreign key error rather than anything the user can act on. Check how Rock's own code deletes that entity and remove the same children first." )]
[AgentUsage( "These tools change site configuration and can run privileged Lava. Confirm the application name, endpoint slug, and enabled Lava commands with the user before creating." )]
[AgentUsage( "Before writing any endpoint template, read the Writing Endpoint Lava article with the Community Knowledge Base skill's GetArticle tool, articleKey 'coding-guide/data-and-endpoints/writing-endpoint-lava'. It governs entity commands versus sql, aggregates, parameters under Body and QueryString, explicit limits, and the JSON output pattern." )]
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
