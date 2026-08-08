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

using Rock.AI.Agent.Classes.Entity;
using Rock.Enums.Lms;

namespace Rock.AI.Agent.Classes.Skills.LearningManagementSkill;

/// <summary>
/// A single learning program completion record.
/// </summary>
internal class LearningProgramCompletionResult : EntityResultBase
{
    /// <summary>
    /// The person who has completed or is in progress of completing the
    /// learning program.
    /// </summary>
    public PersonResult Person { get; set; }

    /// <summary>
    /// The learning progream that has been completed or is in progress of
    /// being completed.
    /// </summary>
    public LearningProgramResult LearningProgram { get; set; }

    /// <summary>
    /// The date the person started on this program.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The date the person finished this program.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// The completion status.
    /// </summary>
    public CompletionStatus? Status { get; set; }
}
