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
    /// Determines the learning completion status for a ( learning ) class.
    /// </summary>
    public enum LearningCompletionStatus
    {
        /// <summary>
        /// The class is not yet complete.
        /// </summary>
        Incomplete = 0,

        /// <summary>
        /// The class has been compelted with a failing grade.
        /// </summary>
        Fail = 1,

        /// <summary>
        /// The class has been completed with a passing grade.
        /// </summary>
        Pass = 2
    }
}
