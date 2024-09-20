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

using Rock.Enums.Lms;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningActivityDetail;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningActivityCompletionDetail
{
    /// <summary>
    /// The item details for the Learning Activity Completion Detail block.
    /// </summary>
    public class LearningActivityCompletionBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Learning Activity Bag for this completion instance.
        /// </summary>
        public LearningActivityBag ActivityBag { get; set; }

        /// <summary>
        /// Gets or sets the completion json for the activity component.
        /// </summary>
        public string ActivityComponentCompletionJson { get; set; }

        /// <summary>
        /// Gets or sets the available date for the activity instance.
        /// </summary>
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Gets or sets the binary file of the completion for use by the activity component.
        /// </summary>
        public ListItemBag BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the date the student
        /// completed the related Rock.Model.LearningActivity.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the due date for the activity instance.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the facilitator's comment.
        /// </summary>
        public string FacilitatorComment { get; set; }

        /// <summary>
        /// Gets or sets the text of the grade earned by the student.
        /// </summary>
        public string GradeName { get; set; }

        /// <summary>
        /// Gets or sets the text for the achieved grade. For example, "B (87%).
        /// </summary>
        public string GradeText { get; set; }

        /// <summary>
        /// Indicates whether or not the activity is currently available.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Indicates whether or not the activity is due within a week.
        /// </summary>
        public bool IsDueSoon => DueDate.HasValue && DueDate >= DateTime.Now && DueDate.Value <= DateTime.Now.AddDays( 7 );

        /// <summary>
        /// Indicates whether or not the grade is a passing grade.
        /// </summary>
        public bool IsGradePassing { get; set; }

        /// <summary>
        /// Indicates whether or not the related activity instance for the student has been completed by the facilitator.
        /// </summary>
        public bool IsFacilitatorCompleted { get; set; }

        /// <summary>
        /// Indicates whether or not the related activity instance for the student is currently past due.
        /// </summary>
        public bool IsPastDue => DueDate != null && DueDate <= DateTime.Now;

        /// <summary>
        /// Indicates whether or not student commenting is enabled for this activity.
        /// </summary>
        public bool IsStudentCommentingEnabled { get; set; }

        /// <summary>
        /// Indicates whether or not the related activity instance has been completed by the student.
        /// </summary>
        public bool IsStudentCompleted { get; set; }

        /// <summary>
        /// Gets or sets the number of points the student earned by completing the activity.
        /// </summary>
        public int PointsEarned { get; set; }

        /// <summary>
        /// Gets or sets the student's comment.
        /// </summary>
        public string StudentComment { get; set; }

        /// <summary>
        /// Gets or sets the student the activity is for.
        /// </summary>
        public LearningActivityParticipantBag Student { get; set; }

        /// <summary>
        /// Indicates whether or not the related Rock.Model.LearningActivity was completed by this student before the DueDate.
        /// </summary>
        public bool WasCompletedOnTime { get; set; }
    }
}
