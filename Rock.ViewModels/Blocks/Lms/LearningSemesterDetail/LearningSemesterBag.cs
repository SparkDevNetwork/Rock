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

namespace Rock.ViewModels.Blocks.Lms.LearningSemesterDetail
{
    /// <summary>
    /// The item details for the Learning Semester Detail block.
    /// </summary>
    public class LearningSemesterBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the date the semester ends.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the date that students must enroll by.
        /// </summary>
        public DateTime? EnrollmentCloseDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the LearningSemester.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date the semester starts
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the program for the semester.
        /// </summary>
        public int LearningProgramId { get; set; }
    }
}
