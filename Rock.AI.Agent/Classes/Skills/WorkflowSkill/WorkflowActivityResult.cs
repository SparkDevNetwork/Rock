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

using System;
using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.WorkflowSkill;

/// <summary>
/// A single activity within a workflow, including its own attribute values
/// (inherited from <see cref="EntityResultBase.AttributeValues"/>) and its
/// actions.
/// </summary>
internal class WorkflowActivityResult : EntityResultBase
{
    /// <summary>
    /// The name of the activity type this activity was created from.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates whether the activity is currently active (activated and not yet
    /// completed).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The date and time the activity was activated, or <c>null</c> when it has
    /// not been activated.
    /// </summary>
    public DateTime? ActivatedDateTime { get; set; }

    /// <summary>
    /// The date and time the activity was completed, or <c>null</c> when it is
    /// still active.
    /// </summary>
    public DateTime? CompletedDateTime { get; set; }

    /// <summary>
    /// The person the activity is assigned to, when it is assigned to an
    /// individual.
    /// </summary>
    public PersonResult AssignedToPerson { get; set; }

    /// <summary>
    /// The group the activity is assigned to, when it is assigned to a group.
    /// </summary>
    public KeyNameResult AssignedToGroup { get; set; }

    /// <summary>
    /// The actions that make up this activity, in order.
    /// </summary>
    public ICollection<WorkflowActionResult> Actions { get; set; }
}
