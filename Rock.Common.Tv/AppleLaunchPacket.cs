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

namespace Rock.Common.Tv
{
    /// <summary>
    /// POCO for an Apple Launch Packet
    /// </summary>
    public class AppleLaunchPacket
    {
        /// <summary>
        /// Gets or sets a value indicating whether [log page views].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log page views]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePageViews { get; set; }

        /// <summary>
        /// Gets or sets the application script.
        /// </summary>
        /// <value>
        /// The application script.
        /// </value>
        public string ApplicationScript { get; set; }

        /// <summary>
        /// Gets or sets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public TvPerson CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the rock version.
        /// </summary>
        /// <value>
        /// The rock version.
        /// </value>
        public string RockVersion { get; set; }

        /// <summary>
        /// Gets or sets the personal device unique identifier.
        /// </summary>
        /// <value>
        /// The personal device unique identifier.
        /// </value>
        public Guid? PersonalDeviceGuid { get; set; }

        /// <summary>
        /// Gets or sets the homepage unique identifier.
        /// </summary>
        /// <value>
        /// The homepage unique identifier.
        /// </value>
        public Guid HomepageGuid { get; set; }
    }
}
