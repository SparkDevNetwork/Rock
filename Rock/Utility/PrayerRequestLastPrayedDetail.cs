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

namespace Rock.Utility
{
    /// <summary>
    /// Provides simplified details on when a prayer request was last prayed
    /// for and by whom.
    /// </summary>
    public class PrayerRequestLastPrayedDetail
    {
        /// <summary>
        /// Gets or sets the prayer request identifier.
        /// </summary>
        /// <value>
        /// The prayer request identifier.
        /// </value>
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the prayer date time.
        /// </summary>
        /// <value>
        /// The prayer date time.
        /// </value>
        public DateTime PrayerDateTime { get; set; }

        /// <summary>
        /// Gets or sets the first name of the person that prayed.
        /// </summary>
        /// <value>
        /// The first name of the person that prayed.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the person that prayed.
        /// </summary>
        /// <value>
        /// The last name of the person that prayed.
        /// </value>
        public string LastName { get; set; }
    }
}
