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

using System.Collections.Generic;

namespace Rock.AI.Agent.Classes.Skills.CommunityKnowledgeBaseSkill;

/// <summary>
/// The matches from one grep of the Rock source, plus whether the search was cut
/// short.
/// </summary>
/// <remarks>
/// The matches are wrapped rather than returned bare so that <see cref="IsTruncated"/>
/// has somewhere to live. A truncated grep presented as a complete one is the exact
/// failure the never truncate silently rule exists to prevent, and a flag with no
/// place to sit is a flag that gets dropped.
/// </remarks>
internal class CodeGrepResult
{
    /// <summary>
    /// The matching lines with their surrounding context.
    /// </summary>
    public List<CodeGrepMatchResult> Matches { get; set; }

    /// <summary>
    /// How many matches were returned.
    /// </summary>
    public int MatchCount { get; set; }

    /// <summary>
    /// Whether the service stopped searching before it ran out of matches.
    /// </summary>
    /// <remarks>
    /// When true the result is partial, and the recovery is a narrower pattern or a
    /// path filter rather than a retry. Never present a truncated grep as proof that
    /// something does not exist.
    /// </remarks>
    public bool IsTruncated { get; set; }
}
