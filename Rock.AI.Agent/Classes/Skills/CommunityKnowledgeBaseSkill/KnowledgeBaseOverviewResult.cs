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
/// A description of everything the community knowledge base holds for the current
/// scope.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here is a Rock entity, so this does not inherit <c>EntityResultBase</c>
/// and carries no IdKey or Guid. The identifiers it does carry belong to the remote
/// service and are named for what they identify.
/// </para>
/// <para>
/// This is the result the whole skill is arranged around. It is the only place the
/// valid filter values appear with document counts, and the only source of topic
/// keys, so nothing on it is trimmed for size.
/// </para>
/// </remarks>
internal class KnowledgeBaseOverviewResult
{
    /// <summary>
    /// Operator written guidance on which store answers which kind of question,
    /// passed through unedited.
    /// </summary>
    /// <remarks>
    /// This is the deployment's own answer to "where should I look", and it is more
    /// current than anything compiled into this skill.
    /// </remarks>
    public string Guidance { get; set; }

    /// <summary>
    /// The knowledge sources holding content in the current scope.
    /// </summary>
    public List<KnowledgeSourceSummaryResult> KnowledgeSources { get; set; }

    /// <summary>
    /// The indexed code repositories and the Rock releases they cover.
    /// </summary>
    public List<CodeRepositorySummaryResult> CodeRepositories { get; set; }

    /// <summary>
    /// The published curated topics, each with the key needed to open it.
    /// </summary>
    /// <remarks>
    /// The only source of topic keys in the skill. There is no LookupTopics tool.
    /// </remarks>
    public List<TopicSummaryResult> Topics { get; set; }

    /// <summary>
    /// The values every search filter accepts, with document counts.
    /// </summary>
    /// <remarks>
    /// The counts are what separate "this value is misspelled" from "this value is
    /// real but holds nothing in my scope". Filters are not validated before they
    /// are sent, so this is the only thing standing between a caller and an empty
    /// result it cannot explain.
    /// </remarks>
    public KnowledgeBaseFiltersResult Filters { get; set; }

    /// <summary>
    /// The Rock version the service actually applied, read back from the response.
    /// </summary>
    public string AppliedRockVersion { get; set; }

    /// <summary>
    /// The categories the request was scoped to, read back from the response.
    /// </summary>
    /// <remarks>
    /// Echoed so the agent can see the scope it is working within rather than
    /// inferring it from what came back.
    /// </remarks>
    public List<string> AppliedCategories { get; set; }

    /// <summary>
    /// A message from the service stating that nothing in the current scope has
    /// code indexed, or <c>null</c> when code is available.
    /// </summary>
    /// <remarks>
    /// Maps <c>data.code.no_code_in_scope</c>, which answers a different question
    /// than <c>meta.no_code_for_version</c> on the code routes. This one includes
    /// the category filter; that one is version only. They can disagree, and
    /// neither substitutes for the other.
    /// </remarks>
    public string NoCodeInScopeMessage { get; set; }
}
