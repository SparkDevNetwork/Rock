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

namespace Rock.Enums.Blocks.Group.Scheduling
{
    /// <summary>
    /// The mode to be used when updating a group member's scheduling preferences for the group scheduler.
    /// </summary>
    public enum UpdateSchedulePreferenceMode
    {
        /// <summary>
        /// Removes other group preferences while adding or updating the current schedule preference.
        /// <para>
        /// "Other" is defined as any preference belonging to this group member, but not tied to the same schedule instance.
        /// </para>
        /// </summary>
        ReplacePreference = 0,

        /// <summary>
        /// Leaves other group preferences in place while adding or updating the current schedule preference.
        /// <para>
        /// "Other" is defined as any preference belonging to this group member, but not tied to the same schedule instance.
        /// </para>
        /// </summary>
        AddToPreference = 1
    }
}
