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

using System;

namespace Rock.Enums.Connection
{
    /// <summary>
    /// Enabled Feature Flags
    /// </summary>
    [Flags]
    public enum EnabledFeatureFlags
    {
        /// <summary>
        /// No features are enabled. Represents the default value.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Enables reminders.
        /// </summary>
        Reminder = 0x0001,

        /// <summary>
        /// Enables celebrations.
        /// </summary>
        Celebration = 0x0002,

        /// <summary>
        /// Enables group placement.
        /// </summary>
        GroupPlacement = 0x0004
    }
}
