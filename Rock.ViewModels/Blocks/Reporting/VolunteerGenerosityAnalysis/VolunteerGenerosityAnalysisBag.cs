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
    /// <summary>
    /// A ViewModel representing the overall data for the Volunteer Generosity Analysis block.
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
        public string PersonGroupKey { get; set; }
        public VolunteerGenerosityPersonDetailsBag PersonDetails { get; set; }
        public List<VolunteerGenerosityDonationBag> Donations { get; set; } = new List<VolunteerGenerosityDonationBag>();
    }

    /// <summary>
    /// Bag for detailed information about a person in the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityPersonDetailsBag
    {
        public string PersonId { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string PhotoUrl { get; set; }
        public string GivingId { get; set; }
        public string LastAttendanceDate { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string CampusId { get; set; }
        public string CampusShortCode { get; set; }
    }

    /// <summary>
    /// Bag for donation data within the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityDonationBag
    {
        public string MonthNameAbbreviated { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
    }

    public class VolunteerGenerosityPersonBag
    {
        public string PersonId { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
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
    }
}

