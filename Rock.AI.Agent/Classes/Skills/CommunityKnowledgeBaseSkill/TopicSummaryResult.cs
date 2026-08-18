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
/// One curated topic as it appears in the overview.
/// </summary>
/// <remarks>
/// Curated topics are never returned by search, and there is no LookupTopics tool,
/// so the overview is the only place these keys exist. Every field here is kept for
/// that reason.
/// </remarks>
internal class TopicSummaryResult
{
    /// <summary>
    /// The retrieval key, which is exactly what GetTopic takes.
    /// </summary>
    /// <remarks>
    /// Never construct, edit, or guess one of these. Keys are opaque and an invented
    /// key returns not found.
    /// </remarks>
    public string TopicKey { get; set; }

    /// <summary>
    /// The topic's display name.
    /// </summary>
    /// <remarks>
    /// Worth carrying forward, because the topic table of contents route does not
    /// return a display name of its own.
    /// </remarks>
    public string Name { get; set; }

    /// <summary>
    /// The operator written note describing when this topic is useful.
    /// </summary>
    /// <remarks>
    /// This is the field the decision to open a topic gets made from, so it is
    /// returned whole rather than summarized.
    /// </remarks>
    public string Hint { get; set; }

    /// <summary>
    /// The Rock version this topic describes, or "all versions".
    /// </summary>
    public string RockVersion { get; set; }

    /// <summary>
    /// How many articles the topic holds.
    /// </summary>
    /// <remarks>
    /// The only signal of how much work opening a topic represents, and it comes
    /// free in a response that is already small.
    /// </remarks>
    public int ArticleCount { get; set; }
}
