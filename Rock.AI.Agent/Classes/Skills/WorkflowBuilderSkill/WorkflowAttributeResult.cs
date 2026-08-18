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

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One attribute defined on a workflow type. These are the workflow's own
/// variables, the values an action reads and writes as the workflow runs.
/// </summary>
internal class WorkflowAttributeResult : EntityResultBase
{
    /// <summary>
    /// How far this variable reaches: the whole workflow, or one activity.
    /// </summary>
    /// <remarks>
    /// Reported rather than inferred because the two scopes are stored separately
    /// and the same key can exist in both at once, which would otherwise read as a
    /// duplicate.
    /// </remarks>
    public WorkflowAttributeScope Scope { get; set; }

    /// <summary>
    /// The key used to reference the attribute from Lava and from action
    /// settings.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The display name of the attribute.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the attribute.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The field type that determines how the value is stored and edited.
    /// </summary>
    public WorkflowFieldTypeResult FieldType { get; set; }

    /// <summary>
    /// Indicates that a value is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// The order of the attribute relative to the workflow type's other
    /// attributes.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The value used when a workflow starts without one supplied.
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// The field type's configuration qualifiers, which is what makes a select
    /// attribute's options visible.
    /// </summary>
    public Dictionary<string, string> ConfigurationValues { get; set; }
}
