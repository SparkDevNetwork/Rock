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
//

using System.Collections.Generic;

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;

/// <summary>
/// Result model for ResolveAudience: the audience values that best fit a
/// plain-English description, plus every security role the instance has so
/// the caller can choose directly when no suggestion fits.
/// </summary>
internal class AudienceResolutionResult
{
    /// <summary>
    /// The description that was matched, echoed for the caller.
    /// </summary>
    public string AudienceDescription { get; set; }

    /// <summary>
    /// The audience values that best fit the description, highest score
    /// first. Empty when no keyword or security role resembled the
    /// description.
    /// </summary>
    public List<AudienceMatchResult> SuggestedAudiences { get; set; }

    /// <summary>
    /// The keyword audiences that are always available, independent of the
    /// instance's security roles.
    /// </summary>
    public List<AudienceMatchResult> KeywordAudiences { get; set; }

    /// <summary>
    /// The active security roles in this instance, alphabetically. Any name
    /// here is a valid audience value.
    /// </summary>
    public List<SecurityRoleResult> SecurityRoles { get; set; }

    /// <summary>
    /// The total number of active security roles, which exceeds the count of
    /// <see cref="SecurityRoles"/> only when the list was truncated.
    /// </summary>
    public int TotalSecurityRoleCount { get; set; }
}
