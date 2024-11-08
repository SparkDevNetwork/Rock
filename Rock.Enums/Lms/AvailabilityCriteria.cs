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
    /// Determines the method for calculating the available date.
    /// </summary>
    public enum AvailabilityCriteria
    {
        /// <summary>
        /// A specific date.
        /// </summary>
        Specific = 0,

        /// <summary>
        /// An offset of the class start date.
        /// </summary>
        ClassStartOffset = 1,

        /// <summary>
        /// An offset of the class enrollment date.
        /// </summary>
        EnrollmentOffset = 2,

        /// <summary>
        /// Always available.
        /// </summary>
        AlwaysAvailable = 3,

        /// <summary>
        /// No calculation (becomes available after previous is completed).
        /// </summary>
        AfterPreviousCompleted = 4
    }
}
