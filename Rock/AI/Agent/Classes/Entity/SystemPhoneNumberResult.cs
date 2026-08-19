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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents a system (configured) phone number that can be used for communication actions
    /// such as sending SMS messages. Includes meta data about the number and (optionally) the
    /// person currently assigned to monitor or respond from this number.
    /// </summary>
    /// <remarks>
    /// This class serves both the lookup and the detail tool. The lookup fills in a small
    /// subset and leaves the rest null; the serializer omits null values, so the lookup's
    /// output is unaffected by the properties it does not set. Every property must therefore
    /// be nullable. A non-nullable value type would serialize its default (<c>false</c> or
    /// <c>0</c>) on every row the lookup returns, which reads as a real answer rather than
    /// an absent one.
    /// </remarks>
    internal class SystemPhoneNumberResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the display name of the system phone number.
        /// </summary>
        /// <value>The friendly name of the number (e.g. "Main SMS Line").</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the system phone number.
        /// </summary>
        /// <value>A longer description to aid the AI or users in choosing an appropriate number.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the formatted phone number value.
        /// </summary>
        /// <value>The actual phone number (usually E.164 or a formatted local representation).</value>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the person currently assigned to this number (if any).
        /// </summary>
        /// <value>A <see cref="PersonResult"/> describing the assigned individual or <c>null</c>.</value>
        public PersonResult AssignedToPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this number is enabled for SMS messaging.
        /// </summary>
        /// <value><c>true</c> if the number can send and receive SMS; otherwise, <c>false</c>.</value>
        public bool? IsSmsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the number is active. Populated by
        /// <c>GetSystemPhoneNumber</c> only, since the lookup returns active numbers alone.
        /// </summary>
        /// <value><c>true</c> if the number is in use; otherwise, <c>false</c>.</value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the number. Populated by
        /// <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value>An ascending sort value, where a lower number sorts first.</value>
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether incoming messages are forwarded to the
        /// assigned person. Populated by <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value><c>true</c> if incoming messages are forwarded; otherwise, <c>false</c>.</value>
        public bool? IsSmsForwardingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the workflow type launched when an SMS message arrives on this
        /// number. Populated by <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value>A reference to the workflow type, or <c>null</c> when none is configured.</value>
        public KeyNameResult SmsReceivedWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the group whose active members are notified when a new SMS message
        /// arrives on this number. Populated by <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value>A reference to the group, or <c>null</c> when none is configured.</value>
        public KeyNameResult SmsNotificationGroup { get; set; }

        /// <summary>
        /// Gets or sets the mobile application site used to decide which devices receive
        /// push notifications. Populated by <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value>A reference to the site, or <c>null</c> when none is configured.</value>
        public KeyNameResult MobileApplicationSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock is prevented from sending automatic
        /// replies to opt-in and opt-out messages. Populated by
        /// <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value><c>true</c> when the messaging provider handles these replies instead.</value>
        public bool? SuppressSmsOptInOutAutoReplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock is prevented from updating a
        /// person's SMS status when they opt in or out. Populated by
        /// <c>GetSystemPhoneNumber</c> only.
        /// </summary>
        /// <value><c>true</c> when the organization tracks opt-in and opt-out itself.</value>
        public bool? DisableSmsOptInOutTracking { get; set; }
    }
}
