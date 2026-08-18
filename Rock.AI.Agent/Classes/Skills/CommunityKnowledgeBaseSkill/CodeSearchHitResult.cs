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
/// One matching file from a semantic code search.
/// </summary>
/// <remarks>
/// Carries no code. The code tools are a sequence: search to find the file, grep to
/// find the line, then read the lines around it.
/// </remarks>
internal class CodeSearchHitResult
{
    /// <summary>
    /// The identifier the code reading tools take.
    /// </summary>
    /// <remarks>
    /// Deserialized from <c>id</c> on this route and from <c>code_document_id</c> on
    /// grep. The published OpenAPI documents <c>code_document_id</c> for both, which
    /// is wrong for search today, so both names are read. See
    /// <c>CommunityKnowledgeBaseSkill.SearchCode</c>.
    /// </remarks>
    public string DocumentId { get; set; }

    /// <summary>
    /// The path of the file within its repository.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// A link to the file, for citation.
    /// </summary>
    public string FileUrl { get; set; }

    /// <summary>
    /// The repository the file belongs to.
    /// </summary>
    public string Repository { get; set; }

    /// <summary>
    /// The file's language, such as cs, js, or sql.
    /// </summary>
    public string SourceType { get; set; }

    /// <summary>
    /// The relevance score the service assigned to this file.
    /// </summary>
    public double Score { get; set; }
}
