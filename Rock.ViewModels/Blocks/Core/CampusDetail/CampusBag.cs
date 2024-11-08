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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CampusDetail
{
    /// <summary>
    /// Class CampusBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class CampusBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the campus schedules.
        /// </summary>
        /// <value>The campus schedules.</value>
        public List<CampusScheduleBag> CampusSchedules { get; set; }

        /// <summary>
        /// Gets or sets the campus status value.
        /// </summary>
        /// <value>The campus status value.</value>
        public ListItemBag CampusStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the campus type value.
        /// </summary>
        /// <value>The campus type value.</value>
        public ListItemBag CampusTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>null</c> if [is active] contains no value, <c>true</c> if [is active]; otherwise, <c>false</c>.</value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value><c>true</c> if this instance is system; otherwise, <c>false</c>.</value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the leader person alias.
        /// </summary>
        /// <value>The leader person alias.</value>
        public ListItemBag LeaderPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public ListItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the service times.
        /// </summary>
        /// <value>The service times.</value>
        public List<ListItemBag> ServiceTimes { get; set; }

        /// <summary>
        /// Gets or sets the short code.
        /// </summary>
        /// <value>The short code.</value>
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the time zone identifier.
        /// </summary>
        /// <value>The time zone identifier.</value>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; set; }

        /// <summary>
        /// Get or sets the Country Code of the Phone Number
        /// </summary>
        /// <value>The Phone Number Country Code.</value>
        public string PhoneNumberCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the campus topics. The Campus Topic is a Defined Value.
        /// </summary>
        /// <value>The campus topics.</value>
        public List<CampusTopicBag> CampusTopics { get; set; }
    }
}
