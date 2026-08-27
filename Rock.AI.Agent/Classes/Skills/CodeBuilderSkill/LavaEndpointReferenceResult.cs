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

namespace Rock.AI.Agent.Classes.Skills.CodeBuilderSkill;

/// <summary>
/// Trimmed reference to a Lava endpoint, used as the history content for
/// results whose full payload (templates, test output) is too large to keep
/// in session context.
/// </summary>
internal class LavaEndpointReferenceResult
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
    /// The URL a client uses to call the endpoint.
    /// </summary>
    public string Url { get; set; }
}
