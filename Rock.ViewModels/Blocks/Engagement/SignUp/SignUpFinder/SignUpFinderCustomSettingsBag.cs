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
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpFinder
{
    /// <summary>
    /// The settings that will be edited in the custom settings panel for the Sign-Up Finder block.
    /// </summary>
    public class SignUpFinderCustomSettingsBag
    {
        #region Layout / Initial Page Load

        /// <summary>
        /// Gets or sets whether projects that are full should be shown.
        /// </summary>
        /// <value>
        /// Whether projects that are full should be shown.
        /// </value>
        public bool HideOvercapacityProjects { get; set; }

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
        /// Gets or sets the number of columns the filters should be displayed as.
        /// </summary>
        /// <value>
        /// The number of columns the filters should be displayed as.
        /// </value>
        public int FilterColumns { get; set; }

        #endregion

        #region Project Filters

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
        /// Gets or sets the group attributes that should be available for an individual to filter the results by.
        /// </summary>
        /// <value>
        /// The group attributes that should be available for an individual to filter the results by.
        /// </value>
        public List<string> DisplayAttributeFilters { get; set; }

        #endregion

        #region Campus Filters

        /// <summary>
        /// Gets or sets whether the campus filter settings should be shown.
        /// </summary>
        /// Whether the campus filter settings should be shown.
        public bool DisplayCampusFilterSettings { get; set; }

        /// <summary>
        /// Gets or sets whether the campus filter should be shown.
        /// </summary>
        /// <value>
        /// Whether the campus filter should be shown.
        /// </value>
        public bool DisplayCampusFilter { get; set; }

        /// <summary>
        /// Gets or sets whether the page's campus context (if available) should be used as a filter.
        /// </summary>
        /// <value>
        /// Whether the page's campus context (if available) should be used as a filter.
        /// </value>
        public bool EnableCampusContext { get; set; }

        /// <summary>
        /// Gets or sets whether to hide campuses with no opportunities.
        /// </summary>
        /// <value>
        /// Whether to hide campuses with no opportunities.
        /// </value>
        public bool HideCampusesWithNoOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the types of campuses to include in the campus list.
        /// </summary>
        /// <value>
        /// The types of campuses to include in the campus list.
        /// </value>
        public List<ListItemBag> CampusTypes { get; set; }

        /// <summary>
        /// Gets or sets the statuses of the campuses to include in the campus list.
        /// </summary>
        /// <value>
        /// The statuses of the campuses to include in the campus list.
        /// </value>
        public List<ListItemBag> CampusStatuses { get; set; }

        /// <summary>
        /// Gets or sets the campuses to include in the campus list.
        /// </summary>
        /// <value>
        /// The campuses to include in the campus list.
        /// </value>
        public List<string> Campuses { get; set; }

        #endregion

        #region Schedule Filters

        /// <summary>
        /// Gets or sets whether a list of named schedules will be show as a filter.
        /// </summary>
        /// <value>
        /// Whether a list of named schedules will be show as a filter.
        /// </value>
        public bool DisplayNamedScheduleFilter { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the named schedule filter.
        /// </summary>
        /// <value>
        /// The label to use for the named schedule filter.
        /// </value>
        public string NamedScheduleFilterLabel { get; set; }

        /// <summary>
        /// Gets or sets the root schedule category to be used for the named schedule filter.
        /// </summary>
        /// <value>
        /// The root schedule category to be used for the named schedule filter.
        /// </value>
        public ListItemBag RootScheduleCategory { get; set; }

        #endregion

        #region Location Filters

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

        #endregion

        #region Additional Filters

        /// <summary>
        /// Gets or sets whether to allow individuals to filter the results by projects occurring inside a provided date range.
        /// </summary>
        /// <value>
        /// Whether to allow individuals to filter the results by projects occurring inside a provided date range.
        /// </value>
        public bool DisplayDateRange { get; set; }

        /// <summary>
        /// Gets or sets whether to allow the individual to find projects with "at least" or "no more than" the provided spots available.
        /// </summary>
        /// <value>
        /// Whether to allow the individual to find projects with "at least" or "no more than" the provided spots available.
        /// </value>
        public bool DisplaySlotsAvailableFilter { get; set; }

        #endregion

        #region Lava

        /// <summary>
        /// Gets or sets the Lava template to show with the results of the search.
        /// </summary>
        /// <value>
        /// The Lava template to show with the results of the search.
        /// </value>
        public string ResultsLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava Template to use to show the results header.
        /// </summary>
        /// <value>
        /// The Lava Template to use to show the results header.
        /// </value>
        public string ResultsHeaderLavaTemplate { get; set; }

        #endregion

        #region Linked Pages

        /// <summary>
        /// Gets or sets the page reference to pass to the Lava template for the details of the project.
        /// </summary>
        /// <value>
        /// The page reference to pass to the Lava template for the details of the project.
        /// </value>
        public PageRouteValueBag ProjectDetailPage { get; set; }

        /// <summary>
        /// Gets or sets the page reference to pass to the Lava template for the registration page.
        /// </summary>
        /// <value>
        /// The page reference to pass to the Lava template for the registration page.
        /// </value>
        public PageRouteValueBag RegistrationPage { get; set; }

        #endregion
    }
}
