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

using Rock.Enums.Lms;
using Rock.ViewModels.Blocks.Lms.LearningActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningClassDetail;
using Rock.ViewModels.Blocks.Lms.LearningParticipantDetail;

namespace Rock.ViewModels.Blocks.Lms.PublicLearningClassWorkspace
{
    /// <summary>
    /// 
    /// </summary>
    public class PublicLearningClassWorkspaceBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the list of activities for this learning class.
        /// </summary>
        public List<LearningActivityCompletionBag> Activities { get; set; }

        /// <summary>
        /// Gets or sets the id for the class.
        /// </summary>
        public string ClassIdKey { get; set; }

        /// <summary>
        /// Gets or sets the id for the course.
        /// </summary>
        public string CourseIdKey { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the image for the course.
        /// </summary>
        public Guid? CourseImageGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the course.
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// Gets or sets the summary of the course.
        /// </summary>
        public string CourseSummary { get; set; }

        /// <summary>
        /// Gets or sets the list of facilitators for the learning class.
        /// </summary>
        public List<LearningClassFacilitatorBag> Facilitators { get; set; }

        /// <summary>
        /// Gets or sets the HTML to be rendered for the class workspace header content.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the number of notifications to show on the class overview page.
        /// </summary>
        public int NumberOfNotificationsToShow { get; set; }

        /// <summary>
        /// Gets or sets the participant accessing the course for the learning class.
        /// </summary>
        public LearningParticipantBag ParticipantBag { get; set; }

        /// <summary>
        /// Gets or sets the Learning Program's configuration mode.
        /// </summary>
        public ConfigurationMode ProgamConfigurationMode { get; set; }

        /// <summary>
        /// Whether to show grades on the class overview page.
        /// </summary>
        public bool ShowGrades { get; set; }
    }
}
