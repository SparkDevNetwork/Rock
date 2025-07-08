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
using System.Collections.Generic;

using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningClassActivityDetail;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningClassActivityCompletionDetail
{
    /// <summary>
    /// The item details for the Learning Class Activity Completion Detail block.
    /// </summary>
    public class LearningClassActivityCompletionBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Learning Class Activity Bag for this completion instance.
        /// </summary>
        public LearningClassActivityBag ClassActivityBag { get; set; }

        /// <summary>
        /// The values available when displaying non-configuration screens.
        /// </summary>
        public Dictionary<string, string> CompletionValues { get; set; }

        /// <summary>
        /// Gets or sets the available date for the activity instance.
        /// </summary>
        public DateTimeOffset? AvailableDate { get; set; }

        /// <summary>
        /// Gets or sets the binary file of the completion for use by the activity component.
        /// </summary>
        public ListItemBag BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the security grant for the facilitator to view the binary file.
        /// </summary>
        public string BinaryFileSecurityGrant { get; set; }

        /// <summary>
        /// Gets or sets the date the student
        /// completed the related Rock.Model.LearningActivity.
        /// </summary>
        public DateTimeOffset? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the due date for the activity instance.
        /// </summary>
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the facilitator's comment.
        /// </summary>
        public string FacilitatorComment { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias of the Person who graded the activity.
        /// </summary>
        public ListItemBag GradedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the text of the grade earned by the student.
        /// </summary>
        public string GradeName { get; set; }

        /// <summary>
        /// Gets or sets the text for the achieved grade. For example, "B (87%).
        /// </summary>
        public string GradeText { get; set; }

        /// <summary>
        /// Gets or sets the highlight color of the achieved grade.
        /// </summary>
        public string GradeColor { get; set; }

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
        /// Indicates whether or not the activity was completed late or is late (if incomplete).
        /// </summary>
        public bool IsLate { get; set; }

        /// <summary>
        /// Indicates whether or not the related activity instance has been completed by the student.
        /// </summary>
        public bool IsStudentCompleted { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the <see cref="ClassActivityBag"/>.
        /// </summary>
        public string LearningClassActivityIdKey { get; set; }

        /// <summary>
        /// Gets or sets the number of points the student earned by completing the activity.
        /// </summary>
        public int? PointsEarned { get; set; }

        /// <summary>
        /// Gets or sets whether the activity requires a facilitator to grade/score it.
        /// </summary>
        public bool RequiresScoring { get; set; }

        /// <summary>
        /// Gets or sets whether the facilitator must complete the activity.
        /// </summary>
        public bool RequiresFacilitatorCompletion { get; set; }

        /// <summary>
        /// Gets or sets the student's comment.
        /// </summary>
        public string StudentComment { get; set; }

        /// <summary>
        /// Gets or sets the student the activity is for.
        /// </summary>
        public LearningActivityParticipantBag Student { get; set; }

        /// <summary>
        /// Indicates whether or not the related Rock.Model.LearningClassActivity was completed by this student before the DueDate.
        /// </summary>
        public bool WasCompletedOnTime { get; set; }
    }
}
