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
    /// The classification of how a scheduled attendance instance matches the preference of the individual.
    /// </summary>
    [Enums.EnumDomain( "Event" )]
    public enum ScheduledAttendanceItemMatchesPreference
    {
        /// <summary>
        /// Person (group member) has a scheduling preference for the selected schedule and location.
        /// </summary>
        MatchesPreference = 0,

        /// <summary>
        /// Person (group member) has a scheduling preference for a different schedule (or the selected schedule but different location).
        /// </summary>
        NotMatchesPreference = 1,

        /// <summary>
        /// Person (group member) has no scheduling preferences for the group (or the person isn't a member of the group ).
        /// </summary>
        NoPreference = 2
    }
}
