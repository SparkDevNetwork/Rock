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
//

namespace Rock.ViewModels.Connection
{
    /// <summary>
    /// Bag that contains the request counts that have been calculated.
    /// </summary>
    public class ConnectionRequestCountsBag
    {
        /// <summary>
        /// Gets or sets the number of requests in the opportunity that
        /// are assigned to the specified person.
        /// </summary>
        /// <value>
        /// The number of requests assigned to you.
        /// </value>
        public int AssignedToYouCount { get; set; }

        /// <summary>
        /// Gets or sets the number of active requests in the opportunity.
        /// </summary>
        /// <value>
        /// The total number of active requests.
        /// </value>
        public int TotalCount { get; set; }
    }
}
