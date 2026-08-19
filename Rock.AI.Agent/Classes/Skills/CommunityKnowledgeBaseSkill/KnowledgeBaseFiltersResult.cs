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
/// The values each search filter accepts, with document counts for the current
/// scope.
/// </summary>
/// <remarks>
/// This is load bearing. The skill does not validate filter values before sending
/// them, so an unrecognized value returns an empty result rather than an error. This
/// is where a caller learns what is real, and the counts are what distinguish a
/// misspelled value from one that exists but holds nothing here.
/// </remarks>
internal class KnowledgeBaseFiltersResult
{
    /// <summary>
    /// The categories a knowledge search may be filtered by.
    /// </summary>
    public List<FilterValueResult> Categories { get; set; }

    /// <summary>
    /// The Rock domains a knowledge search may be filtered by.
    /// </summary>
    public List<FilterValueResult> Domains { get; set; }

    /// <summary>
    /// The source types a code search may be filtered by, such as cs, js, or sql.
    /// </summary>
    public List<string> CodeSourceTypes { get; set; }
}
