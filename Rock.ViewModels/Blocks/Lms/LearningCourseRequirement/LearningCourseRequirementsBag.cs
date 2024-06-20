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

using Rock.Enums.Lms;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningCourseRequirement
{
    /// <summary>
    /// 
    /// </summary>
    public class LearningCourseRequirementBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the id key of the course with requirements.
        /// </summary>
        public string LearningCourseIdKey { get; set; }

        /// <summary>
        /// Gets or sets the course code of the course that's required.
        /// </summary>
        public string RequiredLearningCourseCode { get; set; }

        /// <summary>
        /// Gets or sets the id key of the course that's required.
        /// </summary>
        public string RequiredLearningCourseIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the course that's required.
        /// </summary>
        public string RequiredLearningCourseName { get; set; }

        /// <summary>
        /// Gets or sets the requirement type.
        /// </summary>
        public RequirementType RequirementType { get; set; }
    }
}
