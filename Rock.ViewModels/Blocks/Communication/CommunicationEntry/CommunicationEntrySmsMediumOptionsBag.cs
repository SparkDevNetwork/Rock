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
    /// Bag containing the Email Medium options for the Communication Entry block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.Communication.CommunicationEntry.CommunicationEntryMediumOptionsBaseBag" />
    public class CommunicationEntrySmsMediumOptionsBag : CommunicationEntryMediumOptionsBaseBag
    {
        /// <summary>
        /// Gets the type of the medium.
        /// </summary>
        /// <value>
        /// The type of the medium.
        /// </value>
        public override MediumType MediumType => MediumType.Sms;

        /// <summary>
        /// Gets or sets the SMS from numbers.
        /// </summary>
        /// <value>
        /// The SMS from numbers.
        /// </value>
        public List<ListItemBag> SmsFromNumbers { get; set; }
    }
}
