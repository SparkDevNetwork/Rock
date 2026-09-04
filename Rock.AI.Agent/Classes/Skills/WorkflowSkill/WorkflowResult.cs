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
/// A single workflow as it appears in a list. The full attribute values,
/// activities and actions come from the detail tool.
/// </summary>
internal class WorkflowResult : EntityResultBase
{
    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type this workflow was created from.
    /// </summary>
    public KeyNameResult WorkflowType { get; set; }

    /// <summary>
    /// The free-text status the workflow last reported (for example "Active" or
    /// "Completed"). This is set by the workflow itself and is not a fixed set of
    /// values.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Indicates whether the workflow is currently active (activated and not yet
    /// completed).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The date and time the workflow was activated, or <c>null</c> when it has
    /// not been activated.
    /// </summary>
    public DateTime? ActivatedDateTime { get; set; }

    /// <summary>
    /// The date and time the workflow was completed, or <c>null</c> when it is
    /// still active.
    /// </summary>
    public DateTime? CompletedDateTime { get; set; }

    /// <summary>
    /// The person that initiated the workflow, when known.
    /// </summary>
    public PersonResult InitiatedByPerson { get; set; }

    /// <summary>
    /// The names of the activities that are currently active on the workflow.
    /// </summary>
    public List<string> ActiveActivityNames { get; set; }
}
