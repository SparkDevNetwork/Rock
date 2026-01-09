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
    /// Enabled View Flags
    /// </summary>
    [Flags]
    public enum EnabledViewFlags
    {
        /// <summary>
        /// No views are enabled. Represents the default value.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Enables the List view.
        /// </summary>
        List = 0x0001,

        /// <summary>
        /// Enables the Board view.
        /// </summary>
        Board = 0x0002,

        /// <summary>
        /// Enables the Grid view.
        /// </summary>
        Grid = 0x0004,

        /// <summary>
        /// Enables the Snapshot view for high-level summaries.
        /// </summary>
        Snapshot = 0x0008,

        /// <summary>
        /// Enables the Analytics view.
        /// </summary>
        Analytics = 0x0010
    }
}
