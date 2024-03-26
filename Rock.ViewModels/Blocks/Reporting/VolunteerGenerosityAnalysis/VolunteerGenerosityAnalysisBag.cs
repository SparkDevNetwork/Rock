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
    public class VolunteerGenerosityAnalysisBag
    {
        /// <summary>
        /// Gets or sets the analysis data.
        /// </summary>
        public List<VolunteerGenerosityDataBag> AnalysisData { get; set; }
    }

    /// <summary>
    /// A bag that contains neccessary data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGenerosityDataBag
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the nick name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the giving identifier.
        /// </summary>
        public string GivingId { get; set; }

        /// <summary>
        /// Gets or sets the list of groups.
        /// </summary>
        public List<VolunteerGroupDataBag> Groups { get; set; }

        /// <summary>
        /// Gets or sets the persisted dataset's last updated date.
        /// </summary>
        public string LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the estimated refresh time of the persisted dataset.
        /// </summary>
        public double EstimatedRefreshTime { get; set; }

        /// <summary>
        /// Gets or sets the person data.
        /// </summary>
        public PersonDtoBag Person { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        public string PhotoUrl { get; set; }
    }

    /// <summary>
    /// A bag that contains group and campus specific data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGroupDataBag
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the campus shortcode name.
        /// </summary>
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the last attendance date.
        /// </summary>
        public DateTime LastAttendanceDate { get; set; }

        /// <summary>
        /// Gets or sets the giving data.
        /// </summary>
        public List<VolunteerGivingDataBag> GivingData { get; set; }
    }

    /// <summary>
    /// A bag that contains giving data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class VolunteerGivingDataBag
    {
        /// <summary>
        /// Gets or sets the abbreviated month name.
        /// </summary>
        public string MonthNameAbbreviated { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Formats the date.
        /// </summary>
        public string MonthYearFormatted => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName( Month )} {Year}";
    }

    /// <summary>
    /// A bag that contains person specific data for the Volunteer Generosity Analysis block.
    /// </summary>
    public class PersonDtoBag
    {
        /// <summary>
        /// Gets or sets the nick name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the identifier key.
        /// </summary>
        public string IdKey { get; set; }
    }

    /// <summary>
    /// A bag that contains the list of unique campuses and groups for the Volunteer Generosity Analysis block. 
    /// </summary>
    public class VolunteerGenerosityBag
    {
        /// <summary>
        /// Gets or sets the list of unique campuses.
        /// </summary>
        public List<string> UniqueCampuses { get; set; }

        /// <summary>
        /// Gets or sets the list of unique groups.
        /// </summary>
        public List<string> UniqueGroups { get; set; }

    }
}
