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
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the Push Medium options for the Communication Entry block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.Communication.CommunicationEntry.CommunicationEntryMediumOptionsBaseBag" />
    public class CommunicationEntryPushMediumOptionsBag : CommunicationEntryMediumOptionsBaseBag
    {
        /// <summary>
        /// Gets the type of the medium.
        /// </summary>
        /// <value>
        /// The type of the medium.
        /// </value>
        public override MediumType MediumType => MediumType.Push;

        /// <summary>
        /// Gets or sets the character limit for the message.
        /// </summary>
        public int CharacterLimit { get; set; } = 160;

        /// <summary>
        /// Gets or sets the list of applications to which push notifications can be sent.
        /// </summary>
        public List<ListItemBag> Applications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Push Notification Medium is using the Rock Mobile Push Transport.
        /// </summary>
        public bool IsUsingRockMobilePushTransport { get; set; }
    }
}
