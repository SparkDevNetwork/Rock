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
using System.ComponentModel;

using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One field to place on a workflow form.
/// </summary>
/// <remarks>
/// A nested parameter. C# tools are registered through KernelFunctionFactory,
/// which derives the schema from the CLR types by reflection, so the nested type
/// is described. The scalar-only ParameterSchema path applies to Lava-defined
/// tools and is not what a C# tool takes.
/// </remarks>
internal class WorkflowFormFieldInput
{
    /// <summary>
    /// The key of the workflow attribute this field edits. Required.
    /// </summary>
    [Description( "The key of the workflow attribute this field edits." )]
    public string AttributeIdKey { get; set; }

    /// <summary>
    /// Where this field sits on the form. Optional; the position in the supplied
    /// list is used when it is omitted.
    /// </summary>
    [Description( "Where this field sits on the form. Optional; the order of the list is used when omitted." )]
    public int? Order { get; set; }

    /// <summary>
    /// Whether the field is shown. Defaults to shown.
    /// </summary>
    [Description( "Whether the field is shown on the form. Defaults to true." )]
    public bool? IsVisible { get; set; }

    /// <summary>
    /// Whether a value must be supplied before the form can be submitted.
    /// </summary>
    [Description( "Whether the field must be filled in before the form can be submitted." )]
    public bool? IsRequired { get; set; }

    /// <summary>
    /// Whether the value is shown but cannot be edited.
    /// </summary>
    [Description( "Whether the value is shown but cannot be edited." )]
    public bool? IsReadOnly { get; set; }

    /// <summary>
    /// Whether the field's label is hidden.
    /// </summary>
    [Description( "Whether the field's label is hidden." )]
    public bool? HideLabel { get; set; }

    /// <summary>
    /// Markup rendered immediately before the field.
    /// </summary>
    [Description( "Markup rendered immediately before the field." )]
    public string PreHtml { get; set; }

    /// <summary>
    /// Markup rendered immediately after the field.
    /// </summary>
    [Description( "Markup rendered immediately after the field." )]
    public string PostHtml { get; set; }

    /// <summary>
    /// Conditions deciding whether the field is shown. Omit for a field that is
    /// always shown.
    /// </summary>
    [Description( "Conditions deciding whether this field is shown. Omit for a field that is always shown." )]
    public List<WorkflowFormVisibilityRuleInput> VisibilityRules { get; set; }

    /// <summary>
    /// Whether every rule must pass or only one. Defaults to every rule.
    /// </summary>
    [Description( "Whether every rule must pass (GroupAll) or only one (GroupAny). Defaults to GroupAll." )]
    public FilterExpressionType? VisibilityRuleMatch { get; set; }
}
