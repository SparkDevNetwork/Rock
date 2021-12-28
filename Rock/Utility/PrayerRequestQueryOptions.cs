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
using System.Collections.Generic;

namespace Rock.Utility
{
    /// <summary>
    /// The filtering options for <see cref="Rock.Model.PrayerRequestService.GetPrayerRequests(PrayerRequestQueryOptions)"/>.
    /// </summary>
    public class PrayerRequestQueryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive prayer requests
        /// should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive prayer requests should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether non-public prayer requests
        /// should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if non-public prayer requests should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeNonPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unapproved prayer requests
        /// should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unapproved prayer requests should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeUnapproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether expired prayer requests should
        /// be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expired prayer requests should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeExpired { get; set; } = false;

        /// <summary>
        /// Gets or sets the categories to filter prayer requests to. All descendants
        /// of these categories will also be included.
        /// </summary>
        /// <value>
        /// The categories to filter prayer requests to.
        /// </value>
        public List<Guid> Categories { get; set; }

        /// <summary>
        /// Gets or sets the campuses to filter prayer requests to.
        /// </summary>
        /// <value>
        /// The campuses to filter prayer requests to.
        /// </value>
        public List<Guid> Campuses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include nullable campus if campuses exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if prayer requests with an empty campus value should be included regardless of the contents of the <see cref="Campuses"/> values; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeEmptyCampus { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include requests that are
        /// attached to a group or not. This setting will only take affect if
        /// <see cref="GroupGuids"/> is null or empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if prayer requests attached to a group should be included.
        /// </value>
        public bool IncludeGroupRequests { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of group unique identifiers to limit results to.
        /// If not null and not empty then only requests that are attached to one
        /// of these group values will be included.
        /// </summary>
        /// <value>
        /// The list of group unique identifiers to limit results to.
        /// </value>
        public List<Guid> GroupGuids { get; set; }
    }
}
