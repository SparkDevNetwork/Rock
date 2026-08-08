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

namespace Rock.AI.Agent.Mcp.Protocol;

/// <summary>
/// Additional properties describing a Tool to clients.
/// </summary>
internal class ToolAnnotations
{
    /// <summary>
    /// A human-readable title for the tool.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// If true, the tool does not modify its environment.
    /// </summary>
    public bool? ReadOnlyHint { get; set; }

    /// <summary>
    /// If true, the tool may perform destructive updates to its environment.
    /// If false, the tool performs only additive updates.
    /// </summary>
    /// <remarks>
    /// This property is meaningful only when `readOnlyHint == false`.
    /// </remarks>
    public bool? DestructiveHint { get; set; }

    /// <summary>
    /// If true, calling the tool repeatedly with the same arguments will
    //  have no additional effect on the its environment.
    /// </summary>
    public bool? IdempotentHint { get; set; }

    /// <summary>
    /// If true, this tool may interact with an "open world" of external
    /// entities. If false, the tool's domain of interaction is closed.
    /// For example, the world of a web search tool is open, whereas that
    /// of a memory tool is not.
    /// </summary>
    public bool? OpenWorldHint { get; set; }
}
