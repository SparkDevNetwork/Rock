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

namespace Rock.Enums.Communication
{
    /// <summary>
    /// The activity that can occur for a communication recipient.
    /// </summary>
    public enum CommunicationRecipientActivity
    {
        /// <summary>
        /// The communication is pending and has not yet been sent.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The communication was cancelled prior to sending to the recipient.
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// The communication was sent to the recipient.
        /// </summary>
        Sent = 2,

        /// <summary>
        /// The communication was delivered to the recipient.
        /// </summary>
        Delivered = 3,

        /// <summary>
        /// The communication failed to be delivered to the recipient.
        /// </summary>
        DeliveryFailed = 4,

        /// <summary>
        /// The recipient opened the communication.
        /// </summary>
        Opened = 5,

        /// <summary>
        /// The recipient clicked on a link within the communication.
        /// </summary>
        Clicked = 6,

        /// <summary>
        /// The recipient marked the communication as spam.
        /// </summary>
        MarkedAsSpam = 7,

        /// <summary>
        /// The recipient unsubscribed from the communication.
        /// </summary>
        Unsubscribed = 8
    }
}
 