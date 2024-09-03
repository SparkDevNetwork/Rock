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

namespace Rock.ViewModels.Blocks.Lms.LearningClassList
{
    /// <summary>
    /// 
    /// </summary>
    public class LearningClassListOptionsBag
    {

        /// <summary>
        /// Gets or sets whether the block has a valid course parameter.
        /// </summary>
        /// <remarks>
        /// If the course parameter is not present we can't add new classes. This is used to tell the grid to hide the Add button.
        /// </remarks>
        public bool HasValidCourse { get; set; }

        /// <summary>
        /// Gets or sets whether the course column is shown.
        /// </summary>
        public bool ShowCourseColumn { get; set; }

        /// <summary>
        /// Gets or sets whether the location column is shown.
        /// </summary>
        public bool ShowLocationColumn { get; set; }

        /// <summary>
        /// Gets or sets whether the schedule column is shown.
        /// </summary>
        public bool ShowScheduleColumn { get; set; }

        /// <summary>
        /// Gets or sets whether the semester column is shown.
        /// </summary>
        public bool ShowSemesterColumn { get; set; }

        /// <summary>
        /// Gets or sets whether the secondary lists block should be shown.
        /// </summary>
        public bool ShowBlock { get; set; }

    }
}
