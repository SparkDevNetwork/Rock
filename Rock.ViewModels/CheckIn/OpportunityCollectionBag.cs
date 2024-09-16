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
    /// Defines the opportunities available to a single individual during
    /// check-in.
    /// </summary>
    public class OpportunityCollectionBag
    {
        /// <summary>
        /// Gets or sets the ability levels available to select from.
        /// </summary>
        /// <value>The list of ability levels.</value>
        public List<AbilityLevelOpportunityBag> AbilityLevels { get; set; }

        /// <summary>
        /// Gets or sets the areas that are available for check-in.
        /// </summary>
        /// <value>The list of areas.</value>
        public List<AreaOpportunityBag> Areas { get; set; }

        /// <summary>
        /// Gets or sets the groups that are available for check-in.
        /// </summary>
        /// <value>The list of groups.</value>
        public List<GroupOpportunityBag> Groups { get; set; }

        /// <summary>
        /// Gets or sets the locations that are available for check-in.
        /// </summary>
        /// <value>The list of locations.</value>
        public List<LocationOpportunityBag> Locations { get; set; }

        /// <summary>
        /// Gets or sets the schedules that are available for check-in.
        /// </summary>
        /// <value>The list of schedules.</value>
        public List<ScheduleOpportunityBag> Schedules { get; set; }
    }
}
