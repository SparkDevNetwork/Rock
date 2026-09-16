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

namespace Rock.ViewModels.Blocks.Crm.PersonDuplicateList
{
    /// <summary>
    /// The additional configuration options for the Person Duplicate List block.
    /// </summary>
    public class PersonDuplicateListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the site has multiple active campuses.
        /// When false, the campus column is hidden from the grid.
        /// </summary>
        public bool HasMultipleCampuses { get; set; }

        /// <summary>
        /// Gets or sets the high confidence score threshold.
        /// Scores at or above this value are displayed with a success (green) label.
        /// </summary>
        public double? ConfidenceScoreHigh { get; set; }

        /// <summary>
        /// Gets or sets the low confidence score threshold.
        /// Scores at or below this value are displayed with a default (gray) label.
        /// Scores between low and high are displayed with a warning (yellow) label.
        /// </summary>
        public double? ConfidenceScoreLow { get; set; }
    }
}
