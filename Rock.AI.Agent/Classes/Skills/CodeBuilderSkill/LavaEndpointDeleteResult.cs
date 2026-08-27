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
/// Result model for a Lava endpoint that was deleted by the skill.
/// </summary>
internal class LavaEndpointDeleteResult
{
    /// <summary>
    /// Whether the endpoint was deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The slug of the Lava application the endpoint belonged to.
    /// </summary>
    public string ApplicationSlug { get; set; }

    /// <summary>
    /// The slug of the endpoint that was deleted.
    /// </summary>
    public string EndpointSlug { get; set; }

    /// <summary>
    /// How many endpoints remain in the application after the delete.
    /// </summary>
    public int RemainingEndpointCount { get; set; }
}
