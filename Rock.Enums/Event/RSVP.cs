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
    /// RSVP Response
    /// </summary>
    [Enums.EnumDomain( "Event" )]
    public enum RSVP
    {
        /// <summary>
        /// No
        /// </summary>
        No = 0,

        /// <summary>
        /// Yes
        /// </summary>
        Yes = 1,

        /// <summary>
        /// Here's my number, call me Maybe.
        /// Not used by Group Scheduler.
        /// </summary>
        Maybe = 2,

        /// <summary>
        /// RSVP not answered yet (or doesn't apply)
        /// </summary>
        Unknown = 3
    }
}