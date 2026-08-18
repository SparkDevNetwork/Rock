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

using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One configured setting on a workflow action.
/// </summary>
/// <remarks>
/// An object rather than a bare string because a setting value carries more than
/// the value itself. A Lava template or an HTML body can run to thousands of
/// characters, so a tree read clips it and has to say that it did. A plain
/// string-to-string map leaves nowhere to record that, and a caller cannot tell a
/// clipped template from a complete one. Wrapping the value also leaves room for
/// what a setting may need to report later without changing the shape again.
/// </remarks>
internal class WorkflowActionSettingResult
{
    /// <summary>
    /// The stored value of the setting, clipped when the read that produced it
    /// clips long values.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Indicates that <see cref="Value"/> was clipped because it exceeded the
    /// length a tree read returns. Omitted when the value is complete. Retrieve
    /// the whole value with the single action tool.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
    public bool? IsTruncated { get; set; }

    /// <summary>
    /// What <see cref="Value"/> points at, when it is a unique identifier naming an
    /// activity or attribute in the same workflow. Omitted otherwise.
    /// </summary>
    /// <remarks>
    /// A convenience rather than a necessity. Every node in the tree carries its own
    /// unique identifier, so a caller could cross-reference this itself, but doing so
    /// is the most common step when reading a workflow: working out which activity an
    /// action actually starts. The raw value is still what a write sends back.
    /// </remarks>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
    public string ReferenceName { get; set; }
}
