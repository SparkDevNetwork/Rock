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
using Rock.Model;

namespace Rock.ViewModels.Blocks.Lms.LearningActivityDetail
{
    /// <summary>
    /// The item details for the Learning Activity Detail block.
    /// </summary>
    public class LearningActivityBag : EntityBagBase
    {
        /// <summary>
        /// The id of the related ActivityComponent for this LearningActivity.
        /// </summary>
        public int ActivityComponentId { get; set; }

        /// <summary>
        /// Gets or sets the json config for the activity component before completion.
        /// </summary>
        public string ActivityComponentSettingsJson { get; set; }

        /// <summary>
        /// The participant type assigned to complete this activity.
        /// </summary>
        public AssignTo AssignTo { get; set; }

        /// <summary>
        /// The calculation method used for determing the AvailableDate of the activity.
        /// </summary>
        public AvailableDateCalculationMethod AvailableDateCalculationMethod { get; set; }

        /// <summary>
        /// Gets or sets the default date the activity
        /// is available for the Rock.Model.LearningParticipant to complete.
        /// </summary>
        public DateTime? AvailableDateDefault { get; set; }

        /// <summary>
        /// The optional offset to use for calculating the AvailableDate.
        /// </summary>
        public int? AvailableDateOffset { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The calculation method used for determing the DueDate of the activity.
        /// </summary>
        public DueDateCalculationMethod DueDateCalculationMethod { get; set; }

        /// <summary>
        /// Gets or sets the default date the activity is due.
        /// </summary>
        public DateTime? DueDateDefault { get; set; }

        /// <summary>
        /// The optional offset to use for calculating the DueDate.
        /// </summary>
        public int? DueDateOffset { get; set; }

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
        /// Gets or sets the maximum number of points the activity is worth.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Indicates whether or not this activity sends a notification.
        /// </summary>
        public bool SendNotificationCommunication { get; set; }
    }
}
