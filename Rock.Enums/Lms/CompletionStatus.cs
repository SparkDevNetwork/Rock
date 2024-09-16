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
    /// Determines the completion status of a LearningProgramCompletion.
    /// </summary>
    public enum CompletionStatus
    {
        /// <summary>
        /// The program is pending completion.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The program was completed.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// The program not completed before expiring.
        /// </summary>
        Expired = 2
    }
}
