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

using System;

namespace Rock.Utility.Enums
{
    /// <summary>
    /// Represents and indicates days of the week as bits that can be combined into a single byte
    /// </summary>
    [Flags]
    public enum DayOfWeekFlag
    {
        /// <summary>
        /// No days of the week
        /// </summary>
        None = 0x0000_0000,

        /// <summary>
        /// Sunday
        /// </summary>
        Sunday = 0x0000_0001,

        /// <summary>
        /// Monday
        /// </summary>
        Monday = 0x0000_0010,

        /// <summary>
        /// Tuesday
        /// </summary>
        Tuesday = 0x0000_0100,

        /// <summary>
        /// Wednesday
        /// </summary>
        Wednesday = 0x0000_1000,

        /// <summary>
        /// Thursday
        /// </summary>
        Thursday = 0x0001_0000,

        /// <summary>
        /// Friday
        /// </summary>
        Friday = 0x0010_0000,

        /// <summary>
        /// Saturday
        /// </summary>
        Saturday = 0x0100_0000,

        /// <summary>
        /// All days of the week
        /// </summary>
        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday
    }
}
