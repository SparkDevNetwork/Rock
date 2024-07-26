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

namespace Rock.Enums.WebFarm
{
    /// <summary>
    /// The type of the Web Farm Log
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Availability
        /// </summary>
        Availability = 0,

        /// <summary>
        /// Ping
        /// </summary>
        Ping = 1,

        /// <summary>
        /// Pong
        /// </summary>
        Pong = 2,

        /// <summary>
        /// Startup
        /// </summary>
        Startup = 3,

        /// <summary>
        /// Shutdown
        /// </summary>
        Shutdown = 4,

        /// <summary>
        /// Error
        /// </summary>
        Error = 5
    }
}