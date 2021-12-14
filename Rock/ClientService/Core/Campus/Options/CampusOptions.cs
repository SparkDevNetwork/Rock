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
using System.Collections.Generic;

namespace Rock.ClientService.Core.Campus.Options
{
    /// <summary>
    /// Behavioral options for retrieving a list of campuses to be sent to
    /// the client.
    /// </summary>
    public class CampusOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive campuses should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive campuses will be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets the campus types used for filtering. If this is not empty
        /// then only campuses that match one of these campus types will be
        /// included.
        /// </summary>
        /// <value>
        /// The campus types filter.
        /// </value>
        public List<Guid> LimitCampusTypes { get; set; }

        /// <summary>
        /// Gets or sets the campus status used for filtering. If this is not
        /// empty then only campuses that match one of these campus types will be
        /// included.
        /// </summary>
        /// <value>
        /// The campus status filter.
        /// </value>
        public List<Guid> LimitCampusStatuses { get; set; }
    }
}
