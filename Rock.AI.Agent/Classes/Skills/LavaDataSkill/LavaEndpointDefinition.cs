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

namespace Rock.AI.Agent.Classes.Skills.LavaDataSkill;

/// <summary>
/// The definition of a Lava endpoint being created by the LavaData skill.
/// Groups the behavior fields of the endpoint into one request object so the
/// create tool is not a wall of positional primitives.
/// </summary>
internal class LavaEndpointDefinition
{
    /// <summary>
    /// The Lava template that produces the response body.
    /// </summary>
    [Description( "The Lava template that produces the response body. Required." )]
    public string CodeTemplate { get; set; }

    /// <summary>
    /// The HTTP method the endpoint answers. Endpoints are keyed by slug and
    /// method, so the same slug with Get and with Post are two different
    /// endpoints.
    /// </summary>
    [Description( "The HTTP method the endpoint answers: Get, Post, Put or Delete. Defaults to Post, which is what useLavaApp sends when a component does not ask for anything else." )]
    public string HttpMethod { get; set; }

    /// <summary>
    /// How the endpoint authorizes execution.
    /// </summary>
    [Description( "How the endpoint authorizes execution: EndpointExecute, ApplicationView, ApplicationEdit or ApplicationAdministrate. Defaults to ApplicationView so the application's security governs." )]
    public string SecurityMode { get; set; }

    /// <summary>
    /// The comma-delimited list of Lava commands the template is allowed to
    /// use at runtime. A command the template uses but does not enable fails
    /// silently, so this must name every command the template needs.
    /// </summary>
    [Description( "A comma-delimited list of Lava commands the template needs, such as 'RockEntity' or 'RockEntity,RockEntityModify'. Must include every command the template uses or the template will fail at runtime." )]
    public string EnabledLavaCommands { get; set; }

    /// <summary>
    /// The MIME content type the endpoint's response declares. When not set
    /// the endpoint returns application/json, which is what an authored
    /// Custom Component expects.
    /// </summary>
    [Description( "The MIME content type of the endpoint's response. Defaults to application/json, which is what an authored Custom Component expects. Only set this when the endpoint deliberately returns something other than JSON." )]
    public string ContentType { get; set; }
}
