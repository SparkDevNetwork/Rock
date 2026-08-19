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
/// One Rock source file, in full.
/// </summary>
/// <remarks>
/// Assembled here rather than deserialized. The raw route returns
/// <c>text/plain</c> and carries no response envelope, so neither
/// <see cref="FilePath"/> nor <see cref="TotalLines"/> arrives from the service.
/// Both are supplied by the tool.
/// </remarks>
internal class CodeFileResult
{
    /// <summary>
    /// The path of the file, carried from whichever tool supplied the document id.
    /// </summary>
    /// <remarks>
    /// Null when the caller reached this tool without a prior search or grep in the
    /// conversation. Left null rather than guessed.
    /// </remarks>
    public string FilePath { get; set; }

    /// <summary>
    /// How many lines the file has, counted from the content.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// The complete text of the file.
    /// </summary>
    /// <remarks>
    /// Never clipped. A file too large to return is refused with an error naming its
    /// real line count, because clipped source still parses and reads as complete,
    /// which makes silent truncation here worse than usual.
    /// </remarks>
    public string Content { get; set; }
}
