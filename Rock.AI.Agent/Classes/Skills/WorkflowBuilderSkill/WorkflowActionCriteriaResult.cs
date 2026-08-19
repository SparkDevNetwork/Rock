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

using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// The condition that decides whether an action runs.
/// </summary>
/// <remarks>
/// Rendered as its own object rather than three loose properties, because the
/// three are meaningless apart. Null when the action always runs.
/// </remarks>
internal class WorkflowActionCriteriaResult
{
    /// <summary>
    /// The key of the workflow attribute being tested.
    /// </summary>
    public string AttributeKey { get; set; }

    /// <summary>
    /// The display name of the attribute being tested.
    /// </summary>
    public string AttributeName { get; set; }

    /// <summary>
    /// How the attribute's value is compared against <see cref="Value"/>.
    /// </summary>
    public ComparisonType ComparisonType { get; set; }

    /// <summary>
    /// The value the attribute is compared against. This may itself be an
    /// attribute key rather than a literal.
    /// </summary>
    public string Value { get; set; }
}
