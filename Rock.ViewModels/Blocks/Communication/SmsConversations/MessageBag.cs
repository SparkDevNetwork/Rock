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

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Message Bag for SMS Conversations
    /// </summary>
    public class MessageBag
    {
        /// <summary>
        /// Gets or sets the message key.
        /// </summary>
        public string MessageKey { get; set; }

        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        public string SMSMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was sent from Rock, vs a mobile device
        /// </summary>
        public bool IsOutbound { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person that sent the message from Rock.
        /// </summary>
        public string OutboundSenderFullName { get; set; }

        /// <summary>
        /// Gets or sets the created date time offset.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a list of Attachment URL's
        /// </summary>
        public List<string> AttachmentUrls { get; set; }
    }
}
