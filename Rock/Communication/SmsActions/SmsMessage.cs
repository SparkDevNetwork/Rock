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
using Rock.Model;

namespace Rock.Communication.SmsActions
{
    public class SmsMessage
    {
        /// <summary>
        /// Gets or sets the number the message was sent to.
        /// </summary>
        /// <value>
        /// The number the message was sent to.
        /// </value>
        public string ToNumber { get; set; }

        /// <summary>
        /// Gets or sets the number that the message was sent from.
        /// </summary>
        /// <value>
        /// The number that the message was sent from.
        /// </value>
        public string FromNumber { get; set; }

        /// <summary>
        /// Gets or sets the person identified as the sender of the message.
        /// </summary>
        /// <value>
        /// The person identified as the sender of the message.
        /// </value>
        public Person FromPerson { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        /// <value>
        /// The message content.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        public List<BinaryFile> Attachments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMessage"/> class.
        /// </summary>
        public SmsMessage()
        {
            Attachments = new List<BinaryFile>();
        }
    }
}
