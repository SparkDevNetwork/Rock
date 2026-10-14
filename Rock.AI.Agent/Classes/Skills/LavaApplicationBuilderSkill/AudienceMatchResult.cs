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

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;

/// <summary>
/// One candidate audience value that ResolveAudience matched against the
/// caller's plain-English description, with the evidence for the match.
/// </summary>
internal class AudienceMatchResult
{
    /// <summary>
    /// The display name of the candidate: a security role name, or the
    /// 'Public' or 'AllAuthenticatedPeople' keyword.
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// The groupIdKey to pass to AddOrUpdateAuthorizationForEntity when the
    /// candidate is a security role; otherwise <c>null</c>.
    /// </summary>
    public string GroupIdKey { get; set; }

    /// <summary>
    /// The specialRole to pass to AddOrUpdateAuthorizationForEntity when the
    /// candidate is a keyword: <c>AllUsers</c> for everyone including
    /// anonymous visitors, or <c>AllAuthenticatedUsers</c> for anyone who is
    /// logged in; otherwise <c>null</c>.
    /// </summary>
    public string SpecialRole { get; set; }

    /// <summary>
    /// Who the audience value covers, in plain English.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A relative confidence from 0 to 100. An exact name match scores 100.
    /// Anything under 50 is a guess that should be confirmed with the user.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Why this value matched the description.
    /// </summary>
    public string Reason { get; set; }
}
