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
/// A reference to one article, as it appears in a table of contents or in a parent
/// article's child list.
/// </summary>
/// <remarks>
/// These two places, plus the overview's topic list, are the only legitimate sources
/// of a retrieval key.
/// </remarks>
internal class ArticleSummaryResult
{
    /// <summary>
    /// The retrieval key, which is exactly what GetArticle takes.
    /// </summary>
    /// <remarks>
    /// Keys contain slashes and are used as a path. Never construct, edit, or guess
    /// one; an invented key returns not found.
    /// </remarks>
    public string ArticleKey { get; set; }

    /// <summary>
    /// The article's title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// A short description of what the article covers.
    /// </summary>
    public string Summary { get; set; }
}
