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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningActivityDetail
{
    /// <summary>
    /// The item details for the Learning Activity Detail block.
    /// </summary>
    public class LearningActivityBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the learning activity component for the activity.
        /// </summary>
        public LearningActivityComponentBag ActivityComponent { get; set; }

        /// <summary>
        /// Gets or sets the json config for the activity component before completion.
        /// </summary>
        public string ActivityComponentSettingsJson { get; set; }

        /// <summary>
        /// The participant type assigned to complete this activity.
        /// </summary>
        public AssignTo AssignTo { get; set; }

        /// <summary>
        /// The criteria used for determining the AvailableDate of the activity.
        /// </summary>
        public AvailabilityCriteria AvailabilityCriteria { get; set; }

        /// <summary>
        /// Gets or sets the calculated available date for the activity.
        /// </summary>
        public DateTime? AvailableDateCalculated { get; set; }

        /// <summary>
        /// Gets or sets the default date the activity
        /// is available for the Rock.Model.LearningParticipant to complete.
        /// </summary>
        public DateTime? AvailableDateDefault { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text for the available date.
        /// </summary>
        public string AvailableDateDescription { get; set; }

        /// <summary>
        /// The optional offset to use for calculating the AvailableDate.
        /// </summary>
        public int? AvailableDateOffset { get; set; }

        /// <summary>
        /// Gets or sets the average grade for those who've completed the activity.
        /// </summary>
        public string AverageGrade { get; set; }

        /// <summary>
        /// Gets or sets whether the average grade for the class is a passing grade.
        /// </summary>
        public bool AverageGradeIsPassing { get; set; }

        /// <summary>
        /// Gets or sets the average grade percent for those who've completed the activity.
        /// </summary>
        public double AverageGradePercent { get; set; }

        /// <summary>
        /// Gets or sets the number of students who have completed the LearningActivity.
        /// </summary>
        public int CompleteCount { get; set; }

        /// <summary>
        /// Gets or sets the workflow type that's triggered when the activity is completed.
        /// </summary>
        public ListItemBag CompletionWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the currently logged in person.
        /// </summary>
        public LearningActivityParticipantBag CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity as html.
        /// </summary>
        public string DescriptionAsHtml { get; set; }

        /// <summary>
        /// The criteria used for determining the DueDate of the activity.
        /// </summary>
        public DueDateCriteria DueDateCriteria { get; set; }

        /// <summary>
        /// Gets or sets the calculated due date for the activity.
        /// </summary>
        public DateTime? DueDateCalculated { get; set; }

        /// <summary>
        /// Gets or sets the default date the activity is due.
        /// </summary>
        public DateTime? DueDateDefault { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text for the due date.
        /// </summary>
        public string DueDateDescription { get; set; }

        /// <summary>
        /// The optional offset to use for calculating the DueDate.
        /// </summary>
        public int? DueDateOffset { get; set; }

        /// <summary>
        /// Gets or sets the number of students who have not completed the LearningActivity.
        /// </summary>
        public int IncompleteCount { get; set; }

        /// <summary>
        /// Gets or sets whether the activity's due date is in the past.
        /// </summary>
        public bool IsPastDue { get; set; }

        /// <summary>
        /// Indicates whether or not this activity allows students to comment.
        /// </summary>
        public bool IsStudentCommentingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order in which the activity should be displayed.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the percentage of students who have completed this activity.
        /// </summary>
        public double PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of points the activity is worth.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Indicates whether or not this activity sends a notification.
        /// </summary>
        public bool SendNotificationCommunication { get; set; }

        /// <summary>
        /// Gets or sets the task binary file for the activity.
        /// </summary>
        public ListItemBag TaskBinaryFile { get; set; }
    }
}
