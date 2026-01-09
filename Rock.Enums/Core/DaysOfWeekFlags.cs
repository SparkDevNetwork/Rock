﻿// <copyright>
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

namespace Rock.Enums.Core
{
    /// <summary>
    /// Flags enumeration of the days of the week.
    /// </summary>
    [Flags]
    public enum DaysOfWeekFlags
    {
        /// <summary>
        /// No days selected.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Sunday
        /// </summary>
        Sunday = 0x01, // 1

        /// <summary>
        /// Monday
        /// </summary>
        Monday = 0x02, // 2

        /// <summary>
        /// Tuesday
        /// </summary>
        Tuesday = 0x04, // 4

        /// <summary>
        /// Wednesday
        /// </summary>
        Wednesday = 0x08, // 8

        /// <summary>
        /// Thursday
        /// </summary>
        Thursday = 0x10, // 16

        /// <summary>
        /// Friday
        /// </summary>
        Friday = 0x20, // 32

        /// <summary>
        /// Saturday
        /// </summary>
        Saturday = 0x40,  // 64

        /// <summary>
        /// All days of the week.
        /// </summary>
        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday,
    }
}
