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

using System;

namespace Rock.AI.Agent.Classes.Skills.CommunityKnowledgeBaseSkill;

/// <summary>
/// One matching passage from a knowledge search.
/// </summary>
/// <remarks>
/// Results are chunk level rather than document level, so one document can return
/// several of these. That is expected and is not deduplicated here; the passages are
/// the answer material.
/// </remarks>
internal class KnowledgeSearchHitResult
{
    /// <summary>
    /// The title of the document this passage came from.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The matching passage.
    /// </summary>
    public string Snippet { get; set; }

    /// <summary>
    /// The knowledge source this passage came from.
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// The category this document is filed under.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The Rock domain this document covers.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// Where this passage came from, so an answer can point back to it.
    /// </summary>
    /// <remarks>
    /// Maps the service's <c>original_location</c>, which is non null on every hit.
    /// This is a citation and not necessarily a working link: for manually uploaded
    /// documents it references the uploaded file rather than a resolvable URL.
    /// Present it as a citation, never as a hyperlink, because a fabricated link in
    /// an answer is worse than a plain reference.
    /// </remarks>
    public string Citation { get; set; }

    /// <summary>
    /// When the document was published, or <c>null</c> when it carries no date.
    /// </summary>
    /// <remarks>
    /// Nullable on purpose. Plenty of documents have no publish date, and the
    /// service's ranking treats an absent date as unaffected rather than as old.
    /// Substituting a default here would present undated content as ancient.
    /// </remarks>
    public DateTime? PublishDate { get; set; }

    /// <summary>
    /// The relevance score the service assigned to this passage.
    /// </summary>
    public double Score { get; set; }
}
