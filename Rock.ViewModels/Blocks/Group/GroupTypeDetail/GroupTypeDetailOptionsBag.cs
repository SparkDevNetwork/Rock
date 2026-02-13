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

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// The additional configuration options for the Group Type Detail block.
    /// </summary>
    public class GroupTypeDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Group View Lava Template feature is enabled.
        /// </summary>
        public bool EnableGroupViewLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the chat system is currently enabled.
        /// </summary>
        public bool IsChatEnabledSystem { get; set; }

        /// <summary>
        /// Gets or sets the group requirement types.
        /// </summary>
        public List<GroupRequirementTypeBag> GroupRequirementTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the indexing option is available.
        /// </summary>
        public bool IsIndexingOptionAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group history is currently enabled for this group type.
        /// </summary>
        public bool HasHistoricalRecords { get; set; }

        /// <summary>
        /// Gets or sets the group status defined type options.
        /// </summary>
        public List<ListItemBag> DefinedTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the system communication options.
        /// </summary>
        public List<ListItemBag> SystemCommunicationOptions { get; set; }

        /// <summary>
        /// Gets or sets the RSVP system communication options.
        /// </summary>
        public List<ListItemBag> RSVPSystemCommunicationOptions { get; set; }
    }
}
