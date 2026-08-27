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

using System.ComponentModel;

namespace Rock.AI.Agent.Classes.Skills.CodeBuilderSkill;

/// <summary>
/// The definition of a Lava endpoint being added or updated by the Lava
/// Application skill. Groups the behavior fields of the endpoint into one
/// request object so the upsert tool is not a wall of positional primitives.
/// </summary>
internal class LavaEndpointDefinition
{
    /// <summary>
    /// The Lava template that produces the response body. Required on both
    /// the add and the update path; an update replaces the whole template.
    /// </summary>
    [Description( "The Lava template that produces the response body. Required. When the endpoint already exists this replaces the whole template, so send the complete Lava rather than a fragment." )]
    public string CodeTemplate { get; set; }

    /// <summary>
    /// The HTTP method the endpoint answers. Endpoints are keyed by slug and
    /// method, so the same slug with Get and with Post are two different
    /// endpoints, and this value decides which one is being addressed.
    /// </summary>
    [Description( "The HTTP method the endpoint answers: Get, Post, Put or Delete. Endpoints are keyed by slug and method. Defaults to Post, which is what useLavaApp sends when a component does not ask for anything else." )]
    public string HttpMethod { get; set; }

    /// <summary>
    /// How the endpoint authorizes execution. Defaults to ApplicationView
    /// when the endpoint is being created; leaving it unset on an update
    /// keeps the stored mode, so a template-only edit cannot quietly change
    /// who is allowed to run the endpoint.
    /// </summary>
    [Description( "How the endpoint authorizes execution: EndpointExecute, ApplicationView, ApplicationEdit or ApplicationAdministrate. Defaults to ApplicationView on create so the application's security governs. Omit when updating to leave the stored mode unchanged." )]
    public string SecurityMode { get; set; }

    /// <summary>
    /// The comma-delimited list of Lava commands the template is allowed to
    /// use at runtime. A command the template uses but does not enable fails
    /// silently, so this must name every command the template needs. Leaving
    /// the whole value unset on an update keeps the stored commands; sending
    /// clearValue removes them all.
    /// </summary>
    [Description( "The comma-delimited Lava commands the template needs, such as 'RockEntity' or 'RockEntity,RockEntityModify'. Must include every command the template uses or the template will fail at runtime. Omit when updating to leave the stored commands unchanged, or send clearValue to remove them all." )]
    public SetOrClear<string> EnabledLavaCommands { get; set; }

    /// <summary>
    /// The MIME content type the endpoint's response declares. When not set
    /// a new endpoint returns application/json, which is what an authored
    /// Forge Content expects, and an existing endpoint keeps its stored
    /// content type.
    /// </summary>
    [Description( "The MIME content type of the endpoint's response. Defaults to application/json on create, which is what an authored Forge Content expects. Omit when updating to leave the stored content type unchanged. Only set this when the endpoint deliberately returns something other than JSON." )]
    public string ContentType { get; set; }
}
