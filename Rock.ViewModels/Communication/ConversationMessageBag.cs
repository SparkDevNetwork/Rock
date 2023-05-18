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

namespace Rock.ViewModels.Communication
{
    /// <summary>
    /// Class ConversationMessageBag
    /// </summary>
    public class ConversationMessageBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the conversation that
        /// this message belongs to.
        /// </summary>
        /// <value>
        /// The unique identifier of the conversation.
        /// </value>
        public string ConversationKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this message. This can
        /// be used to determine if the message has already been seen.
        /// </summary>
        /// <value>
        /// The unique identifier for this message.
        /// </value>
        public string MessageKey { get; set; }

        /// <summary>
        /// Gets or sets the contact key of the Rock side of the conversation.
        /// For an SMS message this would be the Guid of the Rock phone number.
        /// </summary>
        /// <value>The contact key of the Rock side of the conversation.</value>
        public string RockContactKey { get; set; }

        /// <summary>
        /// Gets or sets the contact key of the recipient. This would contain
        /// a phone number, e-mail address, or other transport specific key
        /// to allow communication.
        /// </summary>
        /// <value>The contact key of the recipient.</value>
        public string ContactKey { get; set; }

        /// <summary>
        /// Gets or sets the created date time of the most recent message.
        /// </summary>
        /// <value>
        /// The created date time of the most recent message.
        /// </value>
        public DateTimeOffset? MessageDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was sent from Rock.
        /// </summary>
        /// <value>
        ///   <c>true</c> if message was sent from Rock; otherwise, <c>false</c>.
        /// </value>
        public bool IsOutbound { get; set; }

        /// <summary>
        /// Gets or sets the content of the most recent message.
        /// </summary>
        /// <value>The content of the most recent message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the most recent
        /// message has been read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the most recent message has been read; otherwise, <c>false</c>.
        /// </value>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the person being
        /// communicated with.
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
        /// Gets or sets a value indicating whether the recipient is a nameless person.
        /// </summary>
        /// <value><c>true</c> if the recipient is a nameless person; otherwise, <c>false</c>.</value>
        public bool IsNamelessPerson { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person that send the message from
        /// Rock. This is only valid if <see cref="IsOutbound"/> is true.
        /// </summary>
        /// <value>
        /// The full name of the person that sent the message from Rock.
        /// </value>
        public string OutboundSenderFullName { get; set; }

        /// <summary>
        /// Gets or sets the attachments to this message.
        /// </summary>
        /// <value>
        /// The attachments to this message.
        /// </value>
        public List<ConversationAttachmentBag> Attachments { get; set; }
    }

}
