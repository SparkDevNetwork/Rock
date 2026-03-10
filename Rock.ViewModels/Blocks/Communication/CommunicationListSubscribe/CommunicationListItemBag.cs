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

using CommunicationType = Rock.Enums.Communication.CommunicationType;

namespace Rock.ViewModels.Blocks.Communication.CommunicationListSubscribe
{
    /// <summary>
    /// A bag that contains information about a single communication list item
    /// for the Communication List Subscribe block.
    /// </summary>
    public class CommunicationListItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the communication list group.
        /// </summary>
        public Guid CommunicationListGuid { get; set; }

        /// <summary>
        /// Gets or sets the display name for the communication list,
        /// which is the PublicName attribute if available, otherwise the group name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the communication list.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is subscribed
        /// (i.e., an active group member) to this communication list.
        /// </summary>
        public bool IsSubscribed { get; set; }

        /// <summary>
        /// Gets or sets the communication preference for this list.
        /// This represents the group member's communication preference,
        /// falling back to the person's preference if not explicitly set.
        /// </summary>
        public CommunicationType CommunicationPreference { get; set; }
    }
}
