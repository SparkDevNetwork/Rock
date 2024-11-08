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
namespace Rock.Enums.Lms
{
    /// <summary>
    /// The type of course requirement.
    /// </summary>
    public enum RequirementType
    {
        /// <summary>
        /// Prior completion of the LearningCourse.
        /// </summary>
        Prerequisite = 0,

        /// <summary>
        /// Simultaneous enrollment of the LearningCourse.
        /// </summary>
        Corequisite = 1,

        /// <summary>
        /// Completion of an equivalent LearningCourse.
        /// </summary>
        Equivalent = 2
    }
}
