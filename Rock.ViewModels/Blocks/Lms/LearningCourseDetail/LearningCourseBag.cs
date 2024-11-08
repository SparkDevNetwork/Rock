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

using Rock.ViewModels.Blocks.Lms.LearningCourseRequirement;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningCourseDetail
{
    /// <summary>
    /// A bag containing Learning Course information.
    /// </summary>
    public class LearningCourseBag : EntityBagBase
    {
        /// <summary>
        /// Indicates whether or not this course allows students to access after completion.
        /// </summary>
        public bool AllowHistoricalAccess { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.LearningCourse.Category for the LearningCourse.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Category highlight color.
        /// </summary>
        public string CategoryColor { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the default learning class for the course.
        /// </summary>
        public string DefaultLearningClassIdKey { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight for the the learning program.
        /// </summary>
        public string ProgramHighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for the the learning program.
        /// </summary>
        public string ProgramIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.WorkflowType of the LearningCourse.
        /// </summary>
        public ListItemBag CompletionWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the code for the course.
        /// </summary>
        public string CourseCode { get; set; }

        /// <summary>
        /// Gets or sets the number of credits awarded for successful completion of the course.
        /// </summary>
        public int Credits { get; set; }

        /// <summary>
        /// Gets or sets the description of the course.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the description of the course as HTML.
        /// </summary>
        public string DescriptionAsHtml { get; set; }

        /// <summary>
        /// Indicates whether or not this course allows announcements.
        /// </summary>
        public bool EnableAnnouncements { get; set; }

        /// <summary>
        /// Gets or sets the ImageBinaryFile for the LearningCourse.
        /// </summary>
        public ListItemBag ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this course is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether or not this course should 
        /// be displayed in public contexts (e.g. on a public site).
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the number of students at which to stop accepting enrollments.
        /// </summary>
        public int? MaxStudents { get; set; }

        /// <summary>
        /// Gets or sets the name of the course.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name of the course.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the summary text of the course.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the course requirements for this course.
        /// </summary>
        public List<LearningCourseRequirementBag> CourseRequirements { get; set; }
    }
}
