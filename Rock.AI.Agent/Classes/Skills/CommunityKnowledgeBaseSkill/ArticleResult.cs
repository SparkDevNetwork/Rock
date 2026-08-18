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
/// One curated article, in full, with the articles beneath it.
/// </summary>
/// <remarks>
/// There is no segment paging here. The REST route returns the whole article in one
/// response and carries no paging fields at all. Segmentation exists only on the
/// service's MCP surface, which this skill does not use, so paging logic written
/// against those field names would find them absent, read that as "no more", and
/// appear to work while being dead code.
/// </remarks>
internal class ArticleResult
{
    /// <summary>
    /// The article's own retrieval key, echoed back by the service.
    /// </summary>
    public string ArticleKey { get; set; }

    /// <summary>
    /// The topic this article belongs to.
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// The article's title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// A short description of what the article covers.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// The article's full content, as Markdown.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The articles sitting beneath this one, each with the key needed to read it.
    /// </summary>
    /// <remarks>
    /// This is what makes progressive disclosure work: read an article, then choose
    /// which of its children to open next.
    /// </remarks>
    public List<ArticleSummaryResult> ChildArticles { get; set; }
}
