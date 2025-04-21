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
//

using System;

using Rock.ViewModels.Utility;
using Rock.Enums.Lms;
using Rock.ViewModels.Blocks.Crm.PersonDetail.GivingConfiguration;

namespace Rock.ViewModels.Blocks.Lms.LearningProgramCompletionDetail
{
    /// <summary>
    /// The item details for the Learning Program Completion Detail block.
    /// </summary>
    public class LearningProgramCompletionBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the student's completion status for the program.
        /// </summary>
        public CompletionStatus CompletionStatus { get; set; }

        /// <summary>
        /// Gets or sets the date the student completed the Rock.Model.LearningProgram.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the date the student started the Rock.Model.LearningProgram.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the related person.
        /// </summary>
        public string PersonName { get; set; }
    }
}
