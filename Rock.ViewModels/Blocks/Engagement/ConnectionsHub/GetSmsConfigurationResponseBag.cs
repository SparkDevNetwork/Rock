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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a response containing SMS configuration details for sending communications to a set of connection requests.
    /// </summary>
    public class GetSmsConfigurationResponseBag
    {
        /// <summary>
        /// Gets or sets the collection of recipients for the communication.
        /// </summary>
        public List<CommunicationRecipientBag> CommunicationRecipients { get; set; }

        /// <summary>
        /// Gets or sets the SMS From numbers that can be selected for sending the SMS communication.
        /// </summary>
        public List<ListItemBag> SmsFromSystemPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the SMS snippets that can be selected for the SMS communication.
        /// </summary>
        public List<ListItemBag> SmsSnippets { get; set; }
    }
}
