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

namespace Rock.ViewModels.Blocks.Crm.PersonSearch
{
    /// <summary>
    /// The additional configuration options for the Person Search block.
    /// </summary>
    public class PersonSearchOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the birthdate column is visible.
        /// </summary>
        public bool IsBirthdateColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the age column is visible.
        /// </summary>
        public bool IsAgeColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gender column is visible.
        /// </summary>
        public bool IsGenderColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the spouse column is visible.
        /// </summary>
        public bool IsSpouseColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the envelope number column is visible.
        /// This requires both the giving envelope feature to be globally enabled and the block setting to be turned on.
        /// </summary>
        public bool IsEnvelopeNumberColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the highlight indicators column is visible.
        /// This is only visible when one or more highlight data views are configured; otherwise the empty
        /// column would reserve grid space and force the Person cell to wrap.
        /// </summary>
        public bool IsHighlightIndicatorsColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search duration should be displayed below the grid.
        /// </summary>
        public bool IsPerformanceShown { get; set; }

        /// <summary>
        /// Gets or sets the absolute URL the browser should be redirected to when the search matches exactly one person.
        /// This is <c>null</c> unless the current search resolved to a single result.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of alternate name matches shown as "Other Possible Matches" for name searches.
        /// Each item's <see cref="ListItemBag.Text"/> is the suggested name and its <see cref="ListItemBag.Value"/> is the URL that re-runs the search for that name.
        /// </summary>
        public List<ListItemBag> AlternateMatches { get; set; }
    }
}
