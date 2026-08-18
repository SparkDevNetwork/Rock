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

using System.ComponentModel;

using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One condition deciding whether a form field or section is shown.
/// </summary>
/// <remarks>
/// Rules are attached to the field or section they govern rather than listed on the
/// tool, which is what keeps a rule with the thing it controls when the form is
/// replaced. This is the third level of nesting on the form parameter.
/// </remarks>
internal class WorkflowFormVisibilityRuleInput
{
    /// <summary>
    /// The key of the attribute whose value decides visibility. Required.
    /// </summary>
    /// <remarks>
    /// May be any attribute the form's action can reach: the workflow's own, or the
    /// containing activity's. It does not have to be a field on this form, because an
    /// earlier action may have set the value.
    /// </remarks>
    [Description( "The key of the attribute whose value decides whether this is shown." )]
    public string ComparedToAttributeIdKey { get; set; }

    /// <summary>
    /// How the attribute's value is compared.
    /// </summary>
    [Description( "How the attribute's value is compared, such as EqualTo or IsNotBlank." )]
    public ComparisonType ComparisonType { get; set; }

    /// <summary>
    /// The value compared against. Leave empty for comparisons that take no value,
    /// such as a blank check.
    /// </summary>
    /// <remarks>
    /// Stored unchanged, so it is one of the four slots in this skill that hold a
    /// guid rather than an idKey. Named in the skill description; keep the two in
    /// step if a fifth is ever added.
    /// </remarks>
    [Description( "The value compared against. Leave empty for comparisons that take no value, such as IsBlank. Stored unchanged, so when the compared-to attribute's field type references another record this must be the record's guid, not its idKey." )]
    public string ComparedToValue { get; set; }
}
