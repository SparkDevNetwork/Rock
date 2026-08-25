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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The response returned when the Group Member Detail block's quick
    /// communication modal is opened.
    /// </summary>
    public class CommunicationOptionsBag
    {
        #region Recipient

        /// <summary>
        /// Gets or sets the recipient's full name for the displaycard.
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets the recipient's group role name for the
        /// displaycard.
        /// </summary>
        public string RecipientRoleName { get; set; }

        /// <summary>
        /// Gets or sets the recipient's photo URL for the displaycard.
        /// </summary>
        public string RecipientPhotoUrl { get; set; }

        #endregion Recipient

        #region Email

        /// <summary>
        /// Gets or sets a value indicating whether the from email address
        /// is editable, per the Allow Selecting From block setting.
        /// </summary>
        public bool IsFromEditable { get; set; }

        /// <summary>
        /// Gets or sets the default from email address (the logged-in
        /// person's email).
        /// </summary>
        public string DefaultFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the static from display text ("{Name} ({email})")
        /// shown when the from address is not editable.
        /// </summary>
        public string FromDisplayText { get; set; }

        /// <summary>
        /// Gets or sets the warning shown on the email tab (no sender
        /// email, or the member has no active email). Null when email can
        /// be sent.
        /// </summary>
        public string EmailWarningMessage { get; set; }

        #endregion Email

        #region SMS

        /// <summary>
        /// Gets or sets a value indicating whether the SMS tab is shown.
        /// Requires the Enable SMS block setting and an SMS-enabled number
        /// on the member.
        /// </summary>
        public bool IsSmsTabShown { get; set; }

        /// <summary>
        /// Gets or sets the static from number text shown when exactly one
        /// authorized system phone number exists.
        /// </summary>
        public string SmsFromNumberText { get; set; }

        /// <summary>
        /// Gets or sets the from number options shown when multiple
        /// authorized system phone numbers exist.
        /// </summary>
        public List<ListItemBag> SmsFromNumberItems { get; set; }

        /// <summary>
        /// Gets or sets the warning shown on the SMS tab (no system number,
        /// or the member has no SMS-enabled phone). Null when SMS can be
        /// sent.
        /// </summary>
        public string SmsWarningMessage { get; set; }

        #endregion SMS
    }
}
