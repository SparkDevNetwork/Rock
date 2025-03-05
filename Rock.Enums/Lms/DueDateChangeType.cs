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
    /// Determines the method for updating the due date of LearningClassActivityCompletion
    /// records when the LearningClassActivity.DueDate is modified.
    /// </summary>
    public enum DueDateChangeType
    {
        /// <summary>
        /// Update only LearningClassActivityCompletion records whose due date exactly matches the previous value.
        /// </summary>
        UpdateMatching = 0,

        /// <summary>
        /// Update all LearningClassActivityCompletion records to use the new due date.
        /// </summary>
        UpdateAll = 1
    }
}
