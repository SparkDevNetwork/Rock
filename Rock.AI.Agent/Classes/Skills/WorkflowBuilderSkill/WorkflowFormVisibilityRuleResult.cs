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
/// One condition deciding whether a form field or section is shown.
/// </summary>
/// <remarks>
/// The attribute is reported by name as well as by key, for the same reason an
/// action's criteria are: a rule that reads as one identifier compared to a string is
/// not reviewable, and an agent should not have to walk the tree to explain a form.
/// </remarks>
internal class WorkflowFormVisibilityRuleResult
{
    /// <summary>
    /// The key of the attribute whose value decides visibility.
    /// </summary>
    public string ComparedToAttributeIdKey { get; set; }

    /// <summary>
    /// The programmatic key of that attribute.
    /// </summary>
    public string ComparedToAttributeKey { get; set; }

    /// <summary>
    /// The name of that attribute.
    /// </summary>
    public string ComparedToAttributeName { get; set; }

    /// <summary>
    /// How the attribute's value is compared.
    /// </summary>
    public ComparisonType ComparisonType { get; set; }

    /// <summary>
    /// The value compared against.
    /// </summary>
    public string ComparedToValue { get; set; }
}
