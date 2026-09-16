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

using Rock.AI.Agent.Classes.Entity;

using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One field on a workflow form. A field surfaces one of the workflow type's
/// attributes to the person filling out the form.
/// </summary>
internal class WorkflowFormFieldResult : EntityResultBase
{
    /// <summary>
    /// The key of the workflow attribute this field edits.
    /// </summary>
    public string AttributeIdKey { get; set; }

    /// <summary>
    /// The attribute's key, which is how the value is referenced elsewhere.
    /// </summary>
    public string AttributeKey { get; set; }

    /// <summary>
    /// The attribute's display name.
    /// </summary>
    public string AttributeName { get; set; }

    /// <summary>
    /// The order of the field on the form.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates that the field is shown on the form.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Indicates that the field must be filled in before the form can be
    /// submitted.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates that the value is shown but cannot be edited.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Indicates that the field's label is hidden.
    /// </summary>
    public bool HideLabel { get; set; }

    /// <summary>
    /// Markup rendered immediately before the field.
    /// </summary>
    public string PreHtml { get; set; }

    /// <summary>
    /// Markup rendered immediately after the field.
    /// </summary>
    public string PostHtml { get; set; }

    /// <summary>
    /// Conditions deciding whether the field is shown. Omitted when the field is
    /// always shown.
    /// </summary>
    public List<WorkflowFormVisibilityRuleResult> VisibilityRules { get; set; }

    /// <summary>
    /// Whether every rule must pass or only one. Omitted when there are no rules.
    /// </summary>
    public FilterExpressionType? VisibilityRuleMatch { get; set; }
}
