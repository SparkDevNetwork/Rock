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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The request sent to the Group Member Detail block's SendCommunication
    /// action.
    /// </summary>
    public class SendCommunicationRequestBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the group member to communicate with.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the communication is an
        /// SMS message instead of an email.
        /// </summary>
        public bool IsSms { get; set; }

        /// <summary>
        /// Gets or sets the from email address. Only honored when the Allow
        /// Selecting From block setting is enabled; otherwise the server
        /// uses the logged-in person's email.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the email subject. Required for email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message body. Required.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the system phone number to send
        /// the SMS from.
        /// </summary>
        public int? FromSystemPhoneNumberId { get; set; }
    }
}
