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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail
{
    /// <summary>
    /// A single additional request filter row.
    /// </summary>
    public class ConnectionTypeAdditionalRequestToShowBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for this filter row.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the source connection type for additional requests.
        /// </summary>
        public ListItemBag ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the states to include.
        /// </summary>
        public List<ConnectionState> StatesToShow { get; set; }

        /// <summary>
        /// Gets or sets the number of days to limit recent requests.
        /// </summary>
        public int? LimitToRecentRequestsDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether family member requests are included.
        /// </summary>
        public bool IncludeFamilyMemberRequests { get; set; }
    }
}
