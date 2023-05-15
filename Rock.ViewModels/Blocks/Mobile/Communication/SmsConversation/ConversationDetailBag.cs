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

using Rock.ViewModels.Communication;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.Communication.SmsConversation
{
    /// <summary>
    /// Details about a conversation between Rock and another individual.
    /// </summary>
    public class ConversationDetailBag
    {
        /// <summary>
        /// Gets or sets the conversation key.
        /// </summary>
        /// <value>The conversation key.</value>
        public string ConversationKey { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>The person unique identifier.</value>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person being communicated with.
        /// </summary>
        /// <value>
        /// The full name of the person being communicated with.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL for the person. Value will be <c>null</c>
        /// if no photo is available.
        /// </summary>
        /// <value>The photo URL of the person.</value>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the phone number for the person.
        /// </summary>
        /// <value>The phone number for the person.</value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the recipient is a nameless person.
        /// </summary>
        /// <value><c>true</c> if the recipient is a nameless person; otherwise, <c>false</c>.</value>
        public bool IsNamelessPerson { get; set; }

        /// <summary>
        /// Gets or sets the initial messages to be displayed.
        /// </summary>
        /// <value>The initial messages to be displayed.</value>
        public List<ConversationMessageBag> Messages { get; set; }

        /// <summary>
        /// Gets or sets the snippets available to use when sending a message.
        /// </summary>
        /// <value>The snippets available to use when sending a message.</value>
        public List<ListItemBag> Snippets { get; set; }
    }
}
