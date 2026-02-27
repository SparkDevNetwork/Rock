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

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// A single step record.
    /// </summary>
    internal class StepResult : EntityResultBase
    {
        /// <summary>
        /// The status of the step.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The type of step.
        /// </summary>
        public StepTypeResult StepType { get; set; }

        /// <summary>
        /// The date the step was started.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// The date the step was ended.
        /// </summary>
        public DateTime? EndDateTime {  get; set; }

        /// <summary>
        /// The date and time the step was completed.
        /// </summary>
        public DateTime? CompletedDateTime { get; set; }
    }
}
