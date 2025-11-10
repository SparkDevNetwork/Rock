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

using Rock.Model;

namespace Rock.Enums.Communication
{
    /// <summary>
    /// The inferred status of a communication.
    /// </summary>
    public enum InferredCommunicationStatus
    {
        /// <inheritdoc cref="CommunicationStatus.Transient"/>
        Transient = 0,

        /// <inheritdoc cref="CommunicationStatus.Draft"/>
        Draft = 1,

        /// <inheritdoc cref="CommunicationStatus.PendingApproval"/>
        PendingApproval = 2,

        /// <inheritdoc cref="CommunicationStatus.Approved"/>
        Approved = 3,

        /// <inheritdoc cref="CommunicationStatus.Denied"/>
        Denied = 4,

        /// <summary>
        /// The communication is in the process of being sent.
        /// </summary>
        Sending = 5,

        /// <summary>
        /// The communication has been sent to all recipients.
        /// </summary>
        Sent = 6
    }
}
