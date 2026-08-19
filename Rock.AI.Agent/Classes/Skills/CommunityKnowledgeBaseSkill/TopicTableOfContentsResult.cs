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
/// One curated topic's table of contents.
/// </summary>
/// <remarks>
/// There is deliberately no title here. The route returns
/// <c>{ topic, instructions, articles }</c>, where <c>topic</c> is the key echoed
/// back rather than a display name, so there is nothing to populate a title with.
/// Carrying the name forward from the overview was considered and rejected: it would
/// be present in a normal conversation and null in a replay or a test that calls this
/// tool alone, and a field that is usually populated is worse than one that never is.
/// The agent already has the name from the overview.
/// </remarks>
internal class TopicTableOfContentsResult
{
    /// <summary>
    /// The topic's retrieval key, echoed back by the service.
    /// </summary>
    public string TopicKey { get; set; }

    /// <summary>
    /// The operator written guidance for working through this topic.
    /// </summary>
    public string Guidance { get; set; }

    /// <summary>
    /// The topic's top level articles, each with the key needed to read it.
    /// </summary>
    /// <remarks>
    /// An empty list is a miss rather than a success. A topic scoped to a different
    /// Rock release returns its guidance with no articles, so this is the field that
    /// decides whether the tool found anything.
    /// </remarks>
    public List<ArticleSummaryResult> Articles { get; set; }
}
