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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Aggregate linkages payload for the Overview card. Set to <c>null</c> when no linkage of
    /// any kind exists for the group, which omits the entire section.
    /// </summary>
    public class GroupLinkagesBag
    {
        /// <summary>
        /// Gets or sets the registrations linked to the group.
        /// </summary>
        public List<GroupLinkageBag> Registrations { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrences linked to the group.
        /// </summary>
        public List<GroupLinkageBag> EventItemOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the content channel items linked to the group.
        /// </summary>
        public List<GroupLinkageBag> ContentItems { get; set; }
    }
}
