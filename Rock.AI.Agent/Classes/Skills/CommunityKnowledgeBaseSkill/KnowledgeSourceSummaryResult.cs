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
/// One knowledge source as it appears in the overview.
/// </summary>
/// <remarks>
/// <see cref="Name"/> is what the <c>source</c> filter on a knowledge search
/// accepts. The overview omits sources holding nothing in the current scope, so a
/// source absent from this list is one a search would find nothing in.
/// </remarks>
internal class KnowledgeSourceSummaryResult
{
    /// <summary>
    /// The source name, which is the value the source filter takes.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// How many documents this source holds in the current scope.
    /// </summary>
    public int DocumentCount { get; set; }

    /// <summary>
    /// The Rock version this source's content describes, or "all versions".
    /// </summary>
    public string RockVersion { get; set; }
}
