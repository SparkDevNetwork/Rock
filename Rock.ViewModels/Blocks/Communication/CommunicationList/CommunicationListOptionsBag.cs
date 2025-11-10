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

namespace Rock.ViewModels.Blocks.Communication.CommunicationList
{
    /// <summary>
    /// The additional configuration options for the Communication List block.
    /// </summary>
    public class CommunicationListOptionsBag
    {
        /// <summary>
        /// Gets or sets whether to show the "sent by" filter controls.
        /// </summary>
        public bool ShowSentByFilter { get; set; }

        /// <summary>
        /// Gets or sets whether an active email transport exists.
        /// </summary>
        public bool HasActiveEmailTransport { get; set; }

        /// <summary>
        /// Gets or sets whether an active SMS transport exists.
        /// </summary>
        public bool HasActiveSmsTransport { get; set; }

        /// <summary>
        /// Gets or sets whether an active push transport exists.
        /// </summary>
        public bool HasActivePushTransport { get; set; }

        /// <summary>
        /// Gets or sets the list of communication topic defined value items - both active and inactive - that the
        /// individual can filter by.
        /// </summary>
        public List<ListItemBag> TopicItems { get; set; }
    }
}
