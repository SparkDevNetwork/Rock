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

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationSkill;

/// <summary>
/// Result model for a Lava application that was deleted by the skill.
/// </summary>
internal class LavaApplicationDeleteResult
{
    /// <summary>
    /// Whether the application was deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The slug of the application that was deleted.
    /// </summary>
    public string ApplicationSlug { get; set; }

    /// <summary>
    /// How many of the skill's own endpoints were deleted along with the
    /// application.
    /// </summary>
    public int DeletedEndpointCount { get; set; }
}
