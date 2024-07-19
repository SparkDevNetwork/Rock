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

namespace Rock.ViewModels.Blocks.Finance.VolunteerGenerosityAnalysis
{
    /// <summary>
    /// Bag for person-related data in the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityDataBag
    {
        /// <summary>
        /// Gets or sets the person group key.
        /// </summary>
        public string PersonGroupKey { get; set; }
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        public int PersonId { get; set; }
        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the person's nickname.
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// Gets or sets the person's photo URL.
        /// </summary>
        public string PhotoUrl { get; set; }
        /// <summary>
        /// Gets or sets the person's giving identifier.
        /// </summary>
        public string GivingId { get; set; }
        /// <summary>
        /// Gets or sets the person's last attendance date.
        /// </summary>
        public DateTime? LastAttendanceDate { get; set; }
        /// <summary>
        /// Gets or sets the Group ID.
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// Gets or sets the Group Name.
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// Gets or sets the Campus ID.
        /// </summary>
        public string CampusId { get; set; }
        /// <summary>
        /// Gets or sets the Campus Short Code
        /// </summary>
        public string CampusShortCode { get; set; }
        /// <summary>
        /// Gets or sets the IsActive flag.
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Gets or sets the person's connection status
        /// </summary>
        public string ConnectionStatus { get; set; }
        /// <summary>
        /// Gets or sets the person's donation months
        /// </summary>
        public string DonationMonths { get; set; }
        /// <summary>
        /// Gets or sets the person's bit mask
        /// </summary>
        public string DonationMonthYearBitmask { get; set; }

    }

    /// <summary>
    /// Bag for person-related data in the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityPersonBag
    {
        /// <summary>
        /// Gets or sets the person ID.
        /// </summary>
        public int PersonId { get; set; }
        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the person's nickname.
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// Gets or sets the person's photo URL.
        /// </summary>
        public string PhotoUrl { get; set; }
        /// <summary>
        /// Gets or sets the person's connection status
        /// </summary>
        public string ConnectionStatus { get; set; }
    }

    /// <summary>
    /// A bag that contains all data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityInitializationBox
    {
        /// <summary>
        /// Gets or sets the list of unique campuses.
        /// </summary>
        public List<string> UniqueCampuses { get; set; }
        /// <summary>
        /// Gets or sets the list of unique groups 
        /// </summary>
        public List<string> UniqueGroups { get; set; }
        /// <summary>
        /// Gets or sets the last updated date time. 
        /// </summary>
        public string LastUpdated { get; set; }
        /// <summary>
        /// Gets or sets the estimated refresh time of the persisted dataset. 
        /// </summary>
        public double EstimatedRefreshTime { get; set; }
        /// <summary>
        /// Gets or sets the bool that shows/hides the campus filter.
        /// </summary>
        public bool ShowCampusFilter { get; set; }
        /// <summary>
        /// Gets or sets the list of people data.
        /// </summary>
        public List<VolunteerGenerosityDataBag> PeopleData { get; set; }
    }
}

