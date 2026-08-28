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
/// Summarized view of one endpoint inside a
/// <see cref="LavaApplicationDetailResult"/>. The template is deliberately
/// excluded; read it with GetLavaEndpoint.
/// </summary>
internal class LavaEndpointSummaryResult
{
    /// <summary>
    /// The slug of the endpoint.
    /// </summary>
    public string EndpointSlug { get; set; }

    /// <summary>
    /// The HTTP method the endpoint answers. Endpoints are keyed by slug and
    /// method together.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The name of the endpoint.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The security mode the endpoint runs under.
    /// </summary>
    public string SecurityMode { get; set; }

    /// <summary>
    /// Whether the endpoint can be called.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The URL a client uses to call the endpoint.
    /// </summary>
    public string Url { get; set; }
}
