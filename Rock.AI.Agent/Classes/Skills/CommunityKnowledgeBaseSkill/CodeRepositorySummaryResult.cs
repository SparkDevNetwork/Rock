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

namespace Rock.AI.Agent.Classes.Skills.CommunityKnowledgeBaseSkill;

/// <summary>
/// One indexed code repository as it appears in the overview.
/// </summary>
/// <remarks>
/// Code is indexed per Rock release and has no "all versions" option, so
/// <see cref="RockVersion"/> is the field that decides whether the code tools can
/// answer anything at all for the running Rock.
/// </remarks>
internal class CodeRepositorySummaryResult
{
    /// <summary>
    /// The repository name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The Rock release this repository's indexed files come from.
    /// </summary>
    public string RockVersion { get; set; }

    /// <summary>
    /// How many files are indexed for this repository and release.
    /// </summary>
    public int FileCount { get; set; }
}
