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
namespace Rock.Model
{
    /// <summary>
    /// Determines the completion status of a LearningProgramCompletion.
    /// </summary>
    [Enums.EnumDomain( "Lms" )]
    public enum DueDateCalculationMethod
    {
        /// <summary>
        /// A specific date.
        /// </summary>
        Specific = 1,

        /// <summary>
        /// An offset of the class start date.
        /// </summary>
        ClassStartOffset = 2,

        /// <summary>
        /// An offset of the class enrollment date.
        /// </summary>
        EnrollmentOffset = 3,

        /// <summary>
        /// No calculation.
        /// </summary>
        NoDate = 4
    }
}
