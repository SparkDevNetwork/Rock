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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One configured action inside an activity.
/// </summary>
/// <remarks>
/// Shared by the tree read and the single-action read. The single-action read
/// fills in the parent references and skips clipping; both must build this
/// through the same renderer, or the same setting reads differently depending on
/// which tool returned it.
/// </remarks>
internal class WorkflowActionTypeResult : EntityResultBase
{
    /// <summary>
    /// The name of the action as it appears in the workflow.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The order of the action within its activity, which is the order it runs
    /// in.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The key of the action component's entity type. This is what identifies the
    /// component when asking for the action's available settings.
    /// </summary>
    public string ActionEntityTypeIdKey { get; set; }

    /// <summary>
    /// The full class name of the action component. Output only.
    /// </summary>
    public string ActionClassName { get; set; }

    /// <summary>
    /// The friendly name of the action component, such as "Set Attribute Value".
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// Indicates that the action marks itself complete when it succeeds.
    /// </summary>
    public bool IsActionCompletedOnSuccess { get; set; }

    /// <summary>
    /// Indicates that the whole activity completes when this action succeeds.
    /// </summary>
    public bool IsActivityCompletedOnSuccess { get; set; }

    /// <summary>
    /// Indicates that the action counts as complete when its criteria are not
    /// met, rather than blocking the activity.
    /// </summary>
    public bool IsActionCompletedIfCriteriaUnmet { get; set; }

    /// <summary>
    /// The condition deciding whether the action runs, or <c>null</c> when it
    /// always runs.
    /// </summary>
    public WorkflowActionCriteriaResult Criteria { get; set; }

    /// <summary>
    /// The action's configuration, keyed by setting key. Each entry reports its
    /// stored value and whether that value was clipped.
    /// </summary>
    public Dictionary<string, WorkflowActionSettingResult> Settings { get; set; }

    /// <summary>
    /// The form shown to a person, when this action is a user entry action.
    /// </summary>
    public WorkflowActionFormResult Form { get; set; }

    /// <summary>
    /// The activity this action belongs to. Populated only by the single-action
    /// tool, where a caller may have arrived without the surrounding tree.
    /// </summary>
    public KeyNameResult ActivityType { get; set; }

    /// <summary>
    /// The workflow type this action belongs to. Populated only by the
    /// single-action tool.
    /// </summary>
    public KeyNameResult WorkflowType { get; set; }
}
