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
/// One activity in a workflow type. An activity is a named group of actions that
/// run in order.
/// </summary>
internal class WorkflowActivityTypeResult : EntityResultBase
{
    /// <summary>
    /// The name of the activity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the activity.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The order of the activity within the workflow type.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates that the activity is active. An inactive activity never runs,
    /// which makes deactivating it the reversible alternative to deleting it.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that the activity starts as soon as the workflow starts.
    /// Normally only the first activity does; when several do, they all fire at
    /// once.
    /// </summary>
    public bool IsActivatedWithWorkflow { get; set; }

    /// <summary>
    /// The actions in the activity, in the order they run.
    /// </summary>
    /// <summary>
    /// The activity's own attributes, which are variables only this activity's
    /// actions can read and write.
    /// </summary>
    /// <remarks>
    /// Separate from the workflow's attributes rather than merged into them, because
    /// they are stored against a different entity and a key can exist in both scopes
    /// at once.
    /// </remarks>
    public List<WorkflowAttributeResult> Attributes { get; set; }

    /// <summary>
    /// The actions in the activity, in order.
    /// </summary>
    public List<WorkflowActionTypeResult> ActionTypes { get; set; }
}
