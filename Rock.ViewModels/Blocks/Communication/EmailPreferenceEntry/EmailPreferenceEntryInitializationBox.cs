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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry
{
    /// <summary>
    /// The box that contains all the initialization information for the Email Preference Entry block.
    /// </summary>
    public class EmailPreferenceEntryInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the successfully unsubscribed text.
        /// </summary>
        /// <value>
        /// The successfully unsubscribed text.
        /// </value>
        public string SuccessfullyUnsubscribedText { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe text.
        /// </summary>
        /// <value>
        /// The unsubscribe text.
        /// </value>
        public string UnsubscribeText { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe from list.
        /// </summary>
        /// <value>
        /// The unsubscribe from list.
        /// </value>
        public List<ListItemBag> UnsubscribeFromListOptions { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe from list.
        /// </summary>
        /// <value>
        /// The unsubscribe from list.
        /// </value>
        public List<ListItemBag> UnsubscribeFromList { get; set; }

        /// <summary>
        /// Gets or sets the email preference update message.
        /// </summary>
        /// <value>
        /// The email preference update message.
        /// </value>
        public string EmailPreferenceUpdateMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the email preference update alert.
        /// </summary>
        /// <value>
        /// The type of the email preference update alert.
        /// </value>
        public string EmailPreferenceUpdateAlertType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow inactivating family].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow inactivating family]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowInactivatingFamily { get; set; }

        /// <summary>
        /// Gets or sets the in active reasons.
        /// </summary>
        /// <value>
        /// The in active reasons.
        /// </value>
        public List<ListItemBag> InActiveReasons { get; set; }

        /// <summary>
        /// Gets or sets the in active reason.
        /// </summary>
        /// <value>
        /// The in active reason.
        /// </value>
        public string InActiveReason { get; set; }

        /// <summary>
        /// Gets or sets the in active note.
        /// </summary>
        /// <value>
        /// The in active note.
        /// </value>
        public string InActiveNote { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        /// <value>
        /// The email preference.
        /// </value>
        public string EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the update email address text.
        /// </summary>
        /// <value>
        /// The update email address text.
        /// </value>
        public string UpdateEmailAddressText { get; set; }

        /// <summary>
        /// Gets or sets the emails allowed text.
        /// </summary>
        /// <value>
        /// The emails allowed text.
        /// </value>
        public string EmailsAllowedText { get; set; }

        /// <summary>
        /// Gets or sets the no mass emails text.
        /// </summary>
        /// <value>
        /// The no mass emails text.
        /// </value>
        public string NoMassEmailsText { get; set; }

        /// <summary>
        /// Gets or sets the no emails text.
        /// </summary>
        /// <value>
        /// The no emails text.
        /// </value>
        public string NoEmailsText { get; set; }

        /// <summary>
        /// Gets or sets the not involved text.
        /// </summary>
        /// <value>
        /// The not involved text.
        /// </value>
        public string NotInvolvedText { get; set; }
    }
}
