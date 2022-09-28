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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CampusDetail
{
    /// <summary>
    /// Class CampusScheduleBag.
    /// </summary>
    public class CampusScheduleBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>The schedule.</value>
        public ListItemBag Schedule { get; set; }

        /// <summary>
        /// Gets or sets the schedule type value.
        /// </summary>
        /// <value>The schedule type value.</value>
        public ListItemBag ScheduleTypeValue { get; set; }
    }
}
