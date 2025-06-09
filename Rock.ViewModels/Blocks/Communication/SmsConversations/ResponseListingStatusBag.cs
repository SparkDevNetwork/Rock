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

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Response Listing Status Bag for SMS Conversations
    /// </summary>
    public class ResponseListingStatusBag
    {
        /// <summary>
        /// Gets or sets the list of conversations.
        /// </summary>
        public List<ConversationBag> Conversations { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string ErrorMessage { get; set; }

    }
}
