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

namespace Rock.Utility
{
    /// <summary>
    /// The time period of the day.
    /// </summary>
    public enum TimePeriodOfDay
    {
        /// <summary>
        /// The period of time is considered the morning.
        /// </summary>
        /// <remarks>
        /// This is defined as having an hour value &lt; 12pm.
        /// </remarks>
        Morning = 0,

        /// <summary>
        /// The period of time is considered the afternoon.
        /// </summary>
        /// <remarks>
        /// This is defined as having an hour value &gt;= 12pm and &lt; 5pm.
        /// </remarks>
        Afternoon = 1,

        /// <summary>
        /// The period of time is considered the evening.
        /// </summary>
        /// <remarks>
        /// This is defined as having an hour value &gt;= 5pm.
        /// </remarks>
        Evening = 2
    }

}
