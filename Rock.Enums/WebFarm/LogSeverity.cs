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
    /// The severity of the Web Farm Log
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Info
        /// An informative log entry that may be useful for debugging
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning
        /// A warning log entry that may require DevOps attention
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Critical
        /// A critical log entry that requires DevOps attention
        /// </summary>
        Critical = 2
    }
}