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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about the selected person's additional time sign-ups for the group schedule toolbox block.
    /// </summary>
    public class SignUpsBag
    {
        /// <summary>
        /// Gets or sets the selected person's schedulable groups.
        /// </summary>
        public List<GroupBag> SchedulableGroups { get; set; }

        /// <summary>
        /// Gets or sets the selected group.
        /// </summary>
        public GroupBag SelectedGroup { get; set; }

        /// <summary>
        /// Gets or sets the selected person's additional time sign-up occurrences for the selected group.
        /// </summary>
        public List<SignUpOccurrenceBag> Occurrences { get; set; }
    }
}
