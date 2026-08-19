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
/// One matching line from a literal or regular expression search of the Rock source.
/// </summary>
internal class CodeGrepMatchResult
{
    /// <summary>
    /// The path of the file the match was found in.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// The identifier the code reading tools take.
    /// </summary>
    /// <remarks>
    /// Deserialized from <c>code_document_id</c> here, not from <c>id</c> as on
    /// code search.
    /// </remarks>
    public string DocumentId { get; set; }

    /// <summary>
    /// The one based line number of the match.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// The matching line.
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// The lines immediately before the match.
    /// </summary>
    public List<string> ContextBefore { get; set; }

    /// <summary>
    /// The lines immediately after the match.
    /// </summary>
    public List<string> ContextAfter { get; set; }
}
