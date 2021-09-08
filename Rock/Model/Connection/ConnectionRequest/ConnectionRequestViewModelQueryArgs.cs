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

namespace Rock.Model
{
    /// <summary>
    /// View Model Query Args
    /// </summary>
    public sealed class ConnectionRequestViewModelQueryArgs
    {
        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the connector person alias identifier.
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the minimum date.
        /// </summary>
        public DateTime? MinDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum date.
        /// </summary>
        public DateTime? MaxDate { get; set; }

        /// <summary>
        /// Gets or sets the requester person alias identifier.
        /// </summary>
        public int? RequesterPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the status ids.
        /// </summary>
        public List<int> StatusIds { get; set; }

        /// <summary>
        /// Gets or sets the connection states.
        /// </summary>
        public List<ConnectionState> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the last activity type ids.
        /// </summary>
        public List<int> LastActivityTypeIds { get; set; }

        /// <summary>
        /// Gets or sets the sort property.
        /// </summary>
        public ConnectionRequestViewModelSortProperty? SortProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is future follow up past due only.
        /// </summary>
        public bool IsFutureFollowUpPastDueOnly { get; set; }

        /// <summary>
        /// Gets or sets the connection request identifier.
        /// </summary>
        /// <value>
        /// The connection request identifier.
        /// </value>
        public int? ConnectionRequestId { get; set; }
    }
}
