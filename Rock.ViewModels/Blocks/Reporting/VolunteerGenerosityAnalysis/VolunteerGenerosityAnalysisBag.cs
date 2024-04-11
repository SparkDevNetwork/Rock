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
using System.Globalization;

namespace Rock.ViewModels.Blocks.Reporting.VolunteerGenerosityAnalysis
{
    /// <summary>
    /// A bag that contains all data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityDataBag
    {
        public List<VolunteerGenerosityPersonDataBag> PeopleData { get; set; } = new List<VolunteerGenerosityPersonDataBag>();
    }

    /// <summary>
    /// Bag for person-related data in the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityPersonDataBag
    {
        /// <summary>
        /// Gets or sets the person group key.
        /// </summary>
        public string PersonGroupKey { get; set; }
        /// <summary>
        /// Gets or sets the person details.
        /// </summary>
        public VolunteerGenerosityPersonDetailsBag PersonDetails { get; set; }
        /// <summary>
        /// Gets or sets the list of donations.
        /// </summary>
        public List<VolunteerGenerosityDonationBag> Donations { get; set; } = new List<VolunteerGenerosityDonationBag>();
    }

    /// <summary>
    /// Bag for detailed information about a person in the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityPersonDetailsBag
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        public string PersonId { get; set; }
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
        public string LastAttendanceDate { get; set; }
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
    }

    /// <summary>
    /// Bag for donation data within the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityDonationBag
    {
        /// <summary>
        /// Gets or sets the month name.
        /// </summary>
        public string MonthNameAbbreviated { get; set; }
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        public string Month { get; set; }
    }

    /// <summary>
    /// Bag for person-related data in the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityPersonBag
    {
        /// <summary>
        /// Gets or sets the person ID.
        /// </summary>
        public string PersonId { get; set; }
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
    }

    /// <summary>
    /// A bag that contains all data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerositySetupBag
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
    }
}

