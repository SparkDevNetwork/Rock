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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningClassDetail
{
    /// <summary>
    /// The item details for the Learning Class Detail block.
    /// </summary>
    public class LearningClassBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.Campus that this Group is associated with.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the related LearningCourse code.
        /// </summary>
        public string CourseCode { get; set; }

        /// <summary>
        /// Gets or sets the related LearningCourse name.
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// Gets or sets the optional description of the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of facilitators for this learning class.
        /// </summary>
        public List<LearningClassFacilitatorBag> Facilitators { get; set; }

        /// <summary>
        /// Gets or sets the related Rock.Model.LearningGradingSystem.
        /// </summary>
        public ListItemBag GradingSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active group. This value is required.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group should be shown in group finders
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or Sets the Location that is associated with the Class.
        /// </summary>
        public ListItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Group. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Schedule.
        /// </summary>
        public ListItemBag Schedule { get; set; }

        /// <summary>
        /// Gets or sets the related Rock.Model.LearningSemester.
        /// </summary>
        public ListItemBag Semester { get; set; }

        /// <summary>
        /// Gets or sets the number of students in the class.
        /// </summary>
        public int StudentCount { get; set; }

        /// <summary>
        /// Gets or sets whether this class takes attendance.
        /// </summary>
        public bool TakesAttendance { get; set; }
    }

    /// <summary>
    /// The facilitator details for the Learning Class Detail block.
    /// </summary>
    public class LearningClassFacilitatorBag
    {
        /// <summary>
        /// Gets or sets the facilitator name.
        /// </summary>
        public string FacilitatorName { get; set; }

        /// <summary>
        /// Gets or sets the facilitator role.
        /// </summary>
        public string FacilitatorRole { get; set; }

        /// <summary>
        /// Gets or sets the facilitator list item.
        /// </summary>
        public ListItemBag Facilitator { get; set; }
    }
}
