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
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// A whole workflow type: its settings, its attributes, and every activity and
/// action beneath it.
/// </summary>
/// <remarks>
/// Returned whole on every call rather than split by activity. An activity's own
/// properties are a hundred characters or so; all the bulk sits on the actions
/// underneath. A per-activity tool would hand back the same total in more calls
/// while removing nothing expensive, and partial views break editing, because
/// inserting a step needs the surrounding order and every write needs keys that
/// only the full read provides.
/// </remarks>
internal class WorkflowTypeDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the workflow type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the workflow type.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The category the workflow type is filed under.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// Indicates that the workflow type is active and can be started.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that instances are saved to the database rather than running
    /// only in memory.
    /// </summary>
    public bool IsPersisted { get; set; }

    /// <summary>
    /// Indicates that this workflow type is owned by Form Builder rather than by
    /// the workflow editor.
    /// </summary>
    /// <remarks>
    /// Its forms are laid out in sections that this skill does not author, so
    /// <c>AddOrUpdateWorkflowActionForm</c> refuses to rewrite them. Surfaced here so
    /// an agent can see that before it plans an edit it cannot make.
    /// </remarks>
    public bool IsFormBuilder { get; set; }

    /// <summary>
    /// How much detail is written to the workflow log.
    /// </summary>
    public WorkflowLoggingLevel LoggingLevel { get; set; }

    /// <summary>
    /// The noun used for one instance of this workflow, such as "Request".
    /// </summary>
    public string WorkTerm { get; set; }

    /// <summary>
    /// The CSS class of the icon shown for the workflow type.
    /// </summary>
    public string IconCssClass { get; set; }

    /// <summary>
    /// The Lava template summarizing an instance.
    /// </summary>
    public string SummaryViewText { get; set; }

    /// <summary>
    /// The message shown when a workflow reaches a point with nothing for the
    /// person to do.
    /// </summary>
    public string NoActionMessage { get; set; }

    /// <summary>
    /// The prefix put in front of each instance's number, such as "REQ".
    /// </summary>
    public string WorkflowIdPrefix { get; set; }

    /// <summary>
    /// The URL fragment the workflow's form is reached by when it is used as a
    /// public entry form.
    /// </summary>
    public string Slug { get; set; }

    /// <summary>
    /// How long the workflow waits between processing passes, in seconds. Zero
    /// means it processes immediately.
    /// </summary>
    public int? ProcessingIntervalSeconds { get; set; }

    /// <summary>
    /// How many days of workflow log entries are kept, or <c>null</c> to keep them
    /// indefinitely.
    /// </summary>
    public int? LogRetentionPeriod { get; set; }

    /// <summary>
    /// How many days completed instances are kept before removal.
    /// </summary>
    public int? CompletedWorkflowRetentionPeriod { get; set; }

    /// <summary>
    /// How many days an incomplete instance may live before it is removed
    /// regardless of state.
    /// </summary>
    public int? MaxWorkflowAgeDays { get; set; }

    /// <summary>
    /// The workflow type's own attributes, which are the variables its actions
    /// read and write.
    /// </summary>
    public List<WorkflowAttributeResult> Attributes { get; set; }

    /// <summary>
    /// The activities in the workflow type, in order.
    /// </summary>
    public List<WorkflowActivityTypeResult> ActivityTypes { get; set; }
}
