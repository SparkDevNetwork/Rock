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

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// Determines how the Grade and Age matching logic will work in check-in.
    /// This only affects the Grade Range, Age Range and Birthdate Range values
    /// for groups. Other filtering options are not affected.
    /// </summary>
    public enum GradeAndAgeMatchingMode
    {
        /// <summary>
        /// <para>
        /// The grade and age must both match (assuming both are specified on
        /// the group).
        /// </para>
        /// <para>
        /// If a group has a Grade Range, Age Range and Birthdate Range then
        /// the logic is: Grade must match and one of age or birthdate must
        /// match. Basically, the Grade filter runs, and then after it finishes
        /// the Age filter runs (which checks both age and birthdate).
        /// </para>
        /// </summary>
        GradeAndAgeMustMatch = 0,

        /// <summary>
        /// <para>
        /// If a Grade Range is specified on the group and the person has a
        /// matching Grade, then that will be considered sufficient and neither
        /// Age Range nor Birthdate Range will be checked even if they are
        /// specified.
        /// </para>
        /// <para>
        /// If a Grade Range is not specified but Age Range or Birthdate Range
        /// are specified, then one of either Age Range or Birthdate Range must
        /// match the person's age.
        /// </para>
        /// </summary>
        AgeMatchNotRequired = 1,

        /// <summary>
        /// The same per-group filtering logic as <see cref="AgeMatchNotRequired"/>
        /// is performed. After all groups have been filtered, if any of those
        /// matching groups were matched by Grade Range then they will take
        /// priority over all other matching groups. The other groups that
        /// did not match by grade will then be removed from the opportunity
        /// list.
        /// </summary>
        PrioritizeGradeOverAge = 2
    }
}
