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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry
{
    /// <summary>
    /// The box that contains all the initialization information for the email preference entry block.
    /// </summary>
    public class EmailPreferenceEntryInitializationBox : BlockBox
    {
        #region Shared Initialization Settings

        /// <summary>
        /// Gets or sets whether the block is currently operating in "Unsubscribe" mode.
        /// </summary>
        public bool IsUnsubscribeMode { get; set; }

        /// <summary>
        /// Gets or sets whether the header icon should be shown.
        /// </summary>
        public bool ShowHeaderIcon { get; set; }

        /// <summary>
        /// Gets or sets the header title to display to the person.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the header description to display to the person.
        /// </summary>
        public string HeaderDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the header title to display to the person when they've resubscribed after the block is
        /// initialized in "Unsubscribe" model.
        /// </summary>
        public string ResubscribedHeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the header description to display to the person when they've resubscribed after
        /// the block is initialized in "Unsubscribe" model.
        /// </summary>
        public string ResubscribedHeaderDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets whether the person may update their contact info.
        /// </summary>
        public bool IsUpdateContactInfoEnabled { get; set; }

        /// <summary>
        /// Gets or sets the person's email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the person's email preference.
        /// </summary>
        public EmailPreference EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the communication channel that was unsubscribed from upon block initialization.
        /// </summary>
        public CommunicationChannelBag UnsubscribedFromChannel { get; set; }

        #endregion Shared Initialization Settings

        #region Opt-Out Options Section

        /// <summary>
        /// Gets or sets the HTML for the "Opt-Out Options" section description.
        /// </summary>
        public string OptOutOptionsDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the email preference title to display to the person.
        /// </summary>
        public string EmailPreferenceTitle { get; set; }

        /// <summary>
        /// Gets or sets the email preference description to display to the person.
        /// </summary>
        public string EmailPreferenceDescription { get; set; }

        /// <summary>
        /// Gets or sets whether the person may remove themselves from church involvement.
        /// </summary>
        public bool IsRemoveInvolvementEnabled { get; set; }

        /// <summary>
        /// Gets or sets the remove involvement title to display to the person.
        /// </summary>
        public string RemoveInvolvementTitle { get; set; }

        /// <summary>
        /// Gets or sets the remove involvement description to display to the person.
        /// </summary>
        public string RemoveInvolvementDescription { get; set; }

        #endregion Opt-Out Options Section

        #region Current Communication Channels Section

        /// <summary>
        /// Gets or sets whether the "Current Communication Channels" section is enabled.
        /// </summary>
        public bool IsCurrentCommunicationChannelsSectionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the "Current Communication Channels" section description.
        /// </summary>
        public string CurrentCommunicationChannelsDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the list of communication channels the person is able to manage within the "Current
        /// Communication Channels" section.
        /// </summary>
        public List<CommunicationChannelBag> CurrentCommunicationChannels { get; set; }

        /// <summary>
        /// Gets or sets the number of days used to calculate how many messages a person has received per current channel.
        /// </summary>
        public int MessageCountWindowDays { get; set; }

        /// <summary>
        /// Gets or sets whether the person should be shown - and allowed to change - their communication preference
        /// for each current communication channel.
        /// </summary>
        public bool ShowMediumPreference { get; set; }

        #endregion Current Communication Channels Section

        #region Available Communication Channels Section

        /// <summary>
        /// Gets or sets whether the "Available Communication Channels" section is enabled.
        /// </summary>
        public bool IsAvailableCommunicationChannelsSectionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the "Available Communication Channels" section description.
        /// </summary>
        public string AvailableCommunicationChannelsDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the list of communication channels the person is able to manage within the "Available
        /// Communication Channels" section.
        /// </summary>
        public List<CommunicationChannelBag> AvailableCommunicationChannels { get; set; }

        #endregion Available Communication Channels Section

        #region Remove Involvement

        /// <summary>
        /// Gets or sets the header title to display to the person when they request to be removed from involvement.
        /// </summary>
        public string RemoveInvolvementConfirmationTitle { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the header description to display to the person when they request to be removed
        /// from involvement.
        /// </summary>
        public string RemoveInvolvementConfirmationDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets whether the person has been removed from church involvement.
        /// </summary>
        public bool IsRemovedFromChurchInvolvement { get; set; }

        /// <summary>
        /// Gets or sets whether the person is allowed to remove their whole family from church involvement.
        /// </summary>
        public bool IsDeactivatingFamilyEnabled { get; set; }

        /// <summary>
        /// Gets or sets the inactive reasons the person may choose when removing themselves from church involvement.
        /// </summary>
        public List<ListItemBag> InactiveReasons { get; set; }

        #endregion Remove Involvement
    }
}
