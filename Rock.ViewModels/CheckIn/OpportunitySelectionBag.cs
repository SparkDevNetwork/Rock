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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A set of items that represent the current selections that should be
    /// made for an attendee.
    /// </summary>
    public class OpportunitySelectionBag
    {
        /// <summary>
        /// Gets or sets the ability level.
        /// </summary>
        /// <value>The ability level.</value>
        public CheckInItemBag AbilityLevel { get; set; }

        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>The area.</value>
        public CheckInItemBag Area { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>The group.</value>
        public CheckInItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public CheckInItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>The schedule.</value>
        public CheckInItemBag Schedule { get; set; }
    }
}
