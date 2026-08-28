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

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;

/// <summary>
/// Result model for reading the full configuration of a Lava endpoint.
/// </summary>
internal class LavaEndpointDetailResult
{
    /// <summary>
    /// The slug of the Lava application the endpoint belongs to.
    /// </summary>
    public string ApplicationSlug { get; set; }

    /// <summary>
    /// The slug of the endpoint.
    /// </summary>
    public string EndpointSlug { get; set; }

    /// <summary>
    /// The HTTP method the endpoint answers.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The display name of the endpoint.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Whether the endpoint is active and able to answer requests.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The Lava template that produces the response body.
    /// </summary>
    public string CodeTemplate { get; set; }

    /// <summary>
    /// The comma-delimited Lava commands the template is allowed to use.
    /// </summary>
    public string EnabledLavaCommands { get; set; }

    /// <summary>
    /// How the endpoint authorizes execution.
    /// </summary>
    public string SecurityMode { get; set; }

    /// <summary>
    /// The MIME content type the endpoint's response declares.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The URL a client uses to call the endpoint.
    /// </summary>
    public string Url { get; set; }
}
