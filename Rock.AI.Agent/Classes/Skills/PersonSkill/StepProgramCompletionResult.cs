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

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Represents a single step program completion record.
    /// </summary>
    internal class StepProgramCompletionResult : EntityResultBase
    {
        /// <summary>
        /// The program that was completed.
        /// </summary>
        public KeyNameResult StepProgram { get; set; }

        /// <summary>
        /// The date the program was started.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// The date the program was completed.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// The steps that were completed.
        /// </summary>
        public List<StepResult> Steps { get; set; }
    }
}
