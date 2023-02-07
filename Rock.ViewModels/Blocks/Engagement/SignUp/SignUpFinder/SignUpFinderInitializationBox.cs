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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpFinder
{
    /// <summary>
    /// The box that contains all the initialization information for the Sign-Up Finder block.
    /// </summary>
    public class SignUpFinderInitializationBox : BlockBox
    {
        #region Layout / Initial Page Load

        /// <summary>
        /// Gets or sets whether the group finder will load with all configured groups (no filters enabled).
        /// </summary>
        /// <value>
        /// Whether the group finder will load with all configured groups (no filters enabled).
        /// </value>
        public bool LoadResultsOnInitialPageLoad { get; set; }

        /// <summary>
        /// Gets or sets whether the project filters should be show as checkboxes or multi-select dropdowns.
        /// </summary>
        /// <value>
        /// Whether the project filters should be show as checkboxes or multi-select dropdowns.
        /// </value>
        public string DisplayProjectFiltersAs { get; set; }

        /// <summary>
        /// Gets or sets the sign-up project group types that may be considered for the search.
        /// </summary>
        /// <value>
        /// The sign-up project group types that may be considered for the search.
        /// </value>
        public int FilterColumns { get; set; }

        #endregion

        #region Filters

        /// <summary>
        /// Gets or sets the sign-up project group types that should be considered for the search.
        /// </summary>
        /// <value>
        /// The sign-up project group types that should be considered for the search.
        /// </value>
        public List<ListItemBag> ProjectTypes { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the project type filter.
        /// </summary>
        /// <value>
        /// The label to use for the Project Type Filter.
        /// </value>
        public string ProjectTypeFilterLabel { get; set; }

        /// <summary>
        /// Gets or sets the campuses that should be considered for the search.
        /// </summary>
        /// <value>
        /// The campuses that should be considered for the search.
        /// </value>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the page campus context.
        /// </summary>
        /// <value>
        /// The page campus context.
        /// </value>
        public ListItemBag PageCampusContext { get; set; }

        /// <summary>
        /// Gets or sets the named schedules that should be considered for the search.
        /// </summary>
        /// <value>
        /// The named schedules that should be considered for the search.
        /// </value>
        public List<ListItemBag> NamedSchedules { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the named schedule filter.
        /// </summary>
        /// <value>
        /// The label to use for the named schedule filter.
        /// </value>
        public string NamedScheduleFilterLabel { get; set; }

        /// <summary>
        /// Gets or sets the group attributes that should be available for an individual to filter the results by, grouped by project type [guid].
        /// </summary>
        /// <value>
        /// The group attributes that should be available for an individual to filter the results by, grouped by project type [guid].
        /// </value>
        public Dictionary<string, Dictionary<string, PublicAttributeBag>> AttributesByProjectType { get; set; }

        /// <summary>
        /// Gets or sets whether to allow individuals to filter the results by projects occurring inside a provided date range.
        /// </summary>
        /// <value>
        /// Whether to allow individuals to filter the results by projects occurring inside a provided date range.
        /// </value>
        public bool DisplayDateRange { get; set; }

        /// <summary>
        /// Gets or sets whether the location sort field should be shown.
        /// </summary>
        /// <value>
        /// Whether the location sort field should be shown.
        /// </value>
        public bool DisplayLocationSort { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the location sort filter.
        /// </summary>
        /// <value>
        /// The label to use for the location sort filter.
        /// </value>
        public string LocationSortLabel { get; set; }

        /// <summary>
        /// Gets or sets whether a filter will be shown to limit results to a specified number of miles from the location selected or their mailing address if logged in.
        /// </summary>
        /// <value>
        /// Whether a filter will be shown to limit results to a specified number of miles from the location selected or their mailing address if logged in.
        /// </value>
        public bool DisplayLocationRangeFilter { get; set; }

        /// <summary>
        /// Gets or sets whether to allow the individual to find projects with "at least" or "no more than" the provided spots available.
        /// </summary>
        /// <value>
        /// Whether to allow the individual to find projects with "at least" or "no more than" the provided spots available.
        /// </value>
        public bool DisplaySlotsAvailableFilter { get; set; }

        #endregion
    }
}
