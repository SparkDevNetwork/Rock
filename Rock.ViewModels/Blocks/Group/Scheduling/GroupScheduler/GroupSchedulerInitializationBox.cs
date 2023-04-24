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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The box that contains all the initialization information for the Group Scheduler block.
    /// </summary>
    public class GroupSchedulerInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the applied filters, to limit what is shown on the Group Scheduler.
        /// </summary>
        /// <value>
        /// The applied filters, to limit what is shown on the Group Scheduler.
        /// </value>
        public GroupSchedulerAppliedFiltersBag AppliedFilters { get; set; }
    }
}
