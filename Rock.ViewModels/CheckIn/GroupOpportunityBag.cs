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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Defines a single group that can be used during check-in.
    /// </summary>
    public class GroupOpportunityBag : CheckInItemBag
    {
        /// <summary>
        /// Gets or sets the area identifier that this group belongs to.
        /// </summary>
        /// <value>The area identifier.</value>
        public string AreaId { get; set; }

        /// <summary>
        /// Gets or sets the ability level identifier required to
        /// attend this group.
        /// </summary>
        /// <value>The required ability level identifier; or <c>null</c> if not required.</value>
        public string AbilityLevelId { get; set; }

        /// <summary>
        /// Gets or sets the locations that are valid for this group. These
        /// come as a LocationId and ScheduleId pair. These are in proper order
        /// of location priority.
        /// </summary>
        /// <value>The locations that are valid for this group.</value>
        public List<LocationAndScheduleBag> Locations { get; set; }
    }
}
