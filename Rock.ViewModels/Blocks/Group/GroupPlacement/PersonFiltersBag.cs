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

using Rock.Enums.Group;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents the Person Filters defined from the Placement Configuration Modal.
    /// </summary>
    public class PersonFiltersBag
    {
        /// <summary>
        /// Gets or sets the selected gender filter.
        /// </summary>
        /// <value>
        /// The gender value to filter by, or <c>null</c> if no gender filter is applied.
        /// </value>
        public int? Gender { get; set; }

        /// <summary>
        /// Gets or sets the campuses selected for filtering.
        /// </summary>
        /// <value>
        /// A list of campuses to include in the filter, or <c>null</c> if no campus filter is applied.
        /// </value>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the data views used to filter people.
        /// </summary>
        /// <value>
        /// A list of persisted data views whose results limit the people included in placement.
        /// </value>
        public List<ListItemBag> PersistedDataViews { get; set; }

        /// <summary>
        /// Gets or sets the comparison type used when filtering by age.
        /// </summary>
        /// <value>
        /// A ComparisonType indicating how the Age value is evaluated.
        /// </value>
        public ComparisonType? AgeComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the age value used for filtering.
        /// </summary>
        /// <value>
        /// The age to compare against, or <c>null</c> if no age comparison is used.
        /// </value>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the delimited age range used when the comparison type is <c>Between</c>.
        /// </summary>
        /// <value>
        /// A comma-separated string representing the lower and upper age values.
        /// </value>
        public string AgeRangeDelimited { get; set; }

        /// <summary>
        /// Gets or sets the comparison type used when filtering by grade.
        /// </summary>
        /// <value>
        /// A ComparisonType indicating how the selected Grade is evaluated.
        /// </value>
        public ComparisonType? GradeComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the selected grade used for filtering.
        /// </summary>
        /// <value>
        /// The grade item to compare against, or <c>null</c> if no grade filter is applied.
        /// </value>
        public ListItemBag Grade { get; set; }

        /// <summary>
        /// Gets or sets the placement subset to which the person filters apply.
        /// </summary>
        /// <value>
        /// A value indicating whether the filters apply to people to place,
        /// destination group members, or both.
        /// </value>
        public AppliesToPlacementConfiguration? AppliesToPlacementConfiguration { get; set; }
    }
}
