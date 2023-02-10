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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpFinder
{
    /// <summary>
    /// Class SignUpFinderSelectedFiltersBag
    /// </summary>
    public class SignUpFinderSelectedFiltersBag
    {
        /// <summary>
        /// Gets or sets the sign-up project group type guid strings that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The sign-up project group type guid strings that should be used to limit the search results.
        /// </value>
        public List<string> ProjectTypes { get; set; }

        /// <summary>
        /// Gets or sets the campus guid strings that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The campus guid strings that should be used to limit the search results.
        /// </value>
        public List<string> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the named schedule guid strings that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The named schedule guid strings that should be used to limit the search results.
        /// </value>
        public List<string> NamedSchedules { get; set; }

        /// <summary>
        /// Gets or sets the group attribute filters that should be used to limit the search results, grouped by project type [guid].
        /// </summary>
        /// <value>
        /// The group attribute filters that should be used to limit the search results, grouped by project type [guid].
        /// </value>
        public Dictionary<string, Dictionary<string, PublicComparisonValueBag>> AttributeFiltersByProjectType { get; set; }

        /// <summary>
        /// Gets or sets the start date that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The start date that should be used to limit the search results.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The end date that should be used to limit the search results.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the location sort (zip code or city, state) that should be used to order the search results.
        /// </summary>
        /// <value>
        /// The location sort (zip code or city, state) that should be used to order the search results.
        /// </value>
        public string LocationSort { get; set; }

        /// <summary>
        /// Gets or sets the location range (in miles) that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The location range (in miles) that should be used to limit the search results.
        /// </value>
        public double? LocationRange { get; set; }

        /// <summary>
        /// Gets or sets the type of comparison ("at least" or "no more than") to be used along with the <see cref="SlotsAvailable"/> value, that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The type of comparison ("at least" or "no more than") to be used along with the <see cref="SlotsAvailable"/> value, that should be used to limit the search results.
        /// </value>
        public string SlotsAvailableComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the slots available count that should be used to limit the search results.
        /// </summary>
        /// <value>
        /// The slots available count that should be used to limit the search results.
        /// </value>
        public int? SlotsAvailable { get; set; }
    }
}
