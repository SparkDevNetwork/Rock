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
        /// Gets or sets the completion json for the activity component.
        /// </summary>
        public string ActivityComponentCompletionJson { get; set; }

        /// <summary>
        /// Gets or sets the name of the Activity Component.
        /// </summary>
        public string ActivityComponentName { get; set; }

        /// <summary>
        /// Gets or sets the path to the Activity Component .obs file.
        /// </summary>
        public string ActivityComponentPath { get; set; }

        /// <summary>
        /// Gets or sets the settings json for the activity component.
        /// </summary>
        public string ActivityComponentSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the name of the Learning Activity.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets or sets the party responsible for completing the activity.
        /// </summary>
        public AssignTo AssignTo { get; set; }

        /// <summary>
        /// Gets or sets the binary file for use by the activity component.
        /// </summary>
        public ListItemBag BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the date the student
        /// completed the related Rock.Model.LearningActivity.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the currently logged in person.
        /// </summary>
        public LearningActivityParticipantBag CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the due date for the activity instance.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the facilitator's comment.
        /// </summary>
        public string FacilitatorComment { get; set; }

        /// <summary>
        /// Indicates whether or not the current person is a facilitator of the activity.
        /// </summary>
        public bool CurrentPersonIsFacilitator { get; set; }

        /// <summary>
        /// Gets or sets the text for the achieved grade.
        /// </summary>
        public string GradeText { get; set; }

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
        public bool IsPastDue { get; set; }

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
        /// Gets or sets the total number of points possible for the entire activity.
        /// </summary>
        public int PointsPossible { get; set; }

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
