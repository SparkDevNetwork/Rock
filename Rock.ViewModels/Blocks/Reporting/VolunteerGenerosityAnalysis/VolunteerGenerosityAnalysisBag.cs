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
        /// <summary>
        /// Gets or sets the people data.
        /// </summary>
        public List<PersonData> PeopleData { get; set; } = new List<PersonData>();
        /// <summary>
        /// Gets or sets the giving data.
        /// </summary>
        public List<GivingDataItem> GivingData { get; set; } = new List<GivingDataItem>();
        /// <summary>
        /// Gets or sets the group data.
        /// </summary>
        public List<GroupData> GroupData { get; set; } = new List<GroupData>();
    }

    public class PersonData
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
        /// Gets or sets the photo URL.
        /// </summary>
        public string PhotoUrl { get; set; }
        /// <summary>
        /// Gets or sets the giving identifier.
        /// </summary>
        public string GivingId { get; set; }
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public List<string> GroupIds { get; set; } = new List<string>();
        /// <summary>
        /// Gets or sets the last attendance date.
        /// </summary>
        public string LastAttendanceDate { get; set; }
    }

    public class GivingDataItem
    {
        /// <summary>
        /// Gets or sets the giving identifier.
        /// </summary>
        public string GivingId { get; set; }
        /// <summary>
        /// Gets or sets the giving amount.
        /// </summary>
        public List<MonthlyGivingData> Donations { get; set; } = new List<MonthlyGivingData>();
    }

    public class MonthlyGivingData
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// Gets or sets themonth abrreviated name.
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

    public class GroupData
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int CampusId { get; set; }
        /// <summary>
        /// Gets or sets the campus name.
        /// </summary>
        public string CampusShortCode { get; set; }
    }
}

