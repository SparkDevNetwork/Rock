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

using Rock.Enums.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The search settings for a check-in configuration. Configures how people are searched for and matched
    /// during the check-in process.
    /// </summary>
    public class CheckInSearchSettingsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the defined value that controls the type of search that is available
        /// after a person clicks the check-in button on the welcome screen.
        /// </summary>
        public Guid? SearchType { get; set; }

        /// <summary>
        /// Gets or sets the display-only formatted projection of <see cref="SearchType"/> (the human-readable label
        /// such as "Phone Number" or "Name and Phone").
        /// </summary>
        public string SearchTypeFormatted { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of digits that must be entered for a phone number search
        /// (default is 4).
        /// </summary>
        public int? MinPhoneLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of digits that may be entered for a phone number search
        /// (default is 10).
        /// </summary>
        public int? MaxPhoneLength { get; set; }

        /// <summary>
        /// Gets or sets how a person's phone number should be compared to the digits entered by the
        /// individual when checking in (Contains or Ends With).
        /// </summary>
        public PhoneSearchMode? PhoneSearchType { get; set; }

        /// <summary>
        /// Gets or sets the display-only formatted projection of <see cref="PhoneSearchType"/> (the human-readable
        /// label such as "Contains" or "Ends With"). Computed server-side; shown on the view panel.
        /// </summary>
        public string PhoneNumberCompare { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of search results to return when searching (default is 100).
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Gets or sets an optional regular expression that is run against any search input before the
        /// search is performed. Useful for stripping special characters.
        /// </summary>
        public string SearchRegex { get; set; }
    }
}
