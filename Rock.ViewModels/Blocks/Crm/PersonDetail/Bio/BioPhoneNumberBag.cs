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
namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Bio
{
    /// <summary>
    /// Describes a single phone number displayed by the Person Bio block.
    /// </summary>
    public class BioPhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the display text for the phone number. This is
        /// "Unlisted" when the number is unlisted.
        /// </summary>
        public string FormattedNumber { get; set; }

        /// <summary>
        /// Gets or sets the phone number type text (such as "Mobile").
        /// </summary>
        public string PhoneTypeText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the phone number is
        /// unlisted. Unlisted numbers are never rendered as links.
        /// </summary>
        public bool IsUnlisted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS messaging is enabled
        /// for the phone number.
        /// </summary>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has opted out of
        /// messaging on this phone number.
        /// </summary>
        public bool IsMessagingOptedOut { get; set; }

        /// <summary>
        /// Gets or sets the tooltip describing when the person opted out of
        /// messaging.
        /// </summary>
        public string MessagingOptedOutTooltip { get; set; }

        /// <summary>
        /// Gets or sets the raw phone number digits, used as the destination
        /// when originating a call. Null when the number is unlisted so the
        /// digits are never sent to the client.
        /// </summary>
        public string RawNumber { get; set; }

        /// <summary>
        /// Gets or sets the E.164 formatted number used to build tel: links.
        /// Null when the number is unlisted so the digits are never sent to
        /// the client.
        /// </summary>
        public string SmsTelUri { get; set; }
    }
}
