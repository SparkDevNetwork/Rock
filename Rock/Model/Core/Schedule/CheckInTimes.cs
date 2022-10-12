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

namespace Rock.Model
{
    /// <summary>
    /// Start/End Times for Check-in
    /// </summary>
    public class CheckInTimes
    {
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the check in start.
        /// </summary>
        /// <value>
        /// The check in start.
        /// </value>
        public DateTime CheckInStart { get; set; }

        /// <summary>
        /// Gets or sets the check in end.
        /// </summary>
        /// <value>
        /// The check in end.
        /// </value>
        public DateTime CheckInEnd { get; set; }
    }

}
