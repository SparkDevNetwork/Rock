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

namespace Rock.Model.Event.RegistrationInstance.Options
{
    /// <summary>
    /// Options for retrieving available spots for a registration instance.
    /// </summary>
    internal class GetSpotsAvailableOptions
    {
        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        public int RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the maximum attendees for the registration instance.
        /// </summary>
        public int? MaxAttendees { get; set; }

        /// <summary>
        /// Gets or sets whether timeout is enabled for the registration instance.
        /// </summary>
        public bool IsTimeoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets the registration session GUID to exclude reserved spots for.
        /// </summary>
        public Guid? ExcludeReservedSpotsForRegistrationSessionGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registration sessions are excluded.
        /// </summary>
        public bool AreRegistrationSessionsExcluded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether waitlist spots should be excluded.
        /// </summary>
        public bool IsWaitListExcluded { get; set; }
    }
}
