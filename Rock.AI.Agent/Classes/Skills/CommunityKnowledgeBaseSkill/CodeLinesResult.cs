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
/// A range of lines read from one Rock source file.
/// </summary>
internal class CodeLinesResult
{
    /// <summary>
    /// The path of the file the lines came from.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// The first line number returned, after the service clamped the request to the
    /// file.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// The last line number returned.
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// How many lines the whole file has.
    /// </summary>
    /// <remarks>
    /// This is what lets a caller widen a range deliberately rather than by trial,
    /// and it is how the size of a GetCodeFile call becomes known before making it.
    /// </remarks>
    public int TotalLines { get; set; }

    /// <summary>
    /// Whether more lines follow the range returned.
    /// </summary>
    /// <remarks>
    /// Read from the service rather than derived by comparing
    /// <see cref="EndLine"/> against <see cref="TotalLines"/>. The derived check
    /// fails at boundaries, notably when a per call cap returns fewer lines than
    /// were asked for without reaching the end of the file.
    /// </remarks>
    public bool HasMore { get; set; }

    /// <summary>
    /// The requested lines.
    /// </summary>
    public List<string> Lines { get; set; }
}
