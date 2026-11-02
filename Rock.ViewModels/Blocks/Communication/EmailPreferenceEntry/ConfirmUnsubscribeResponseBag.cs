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

namespace Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry
{
    /// <summary>
    /// A bag that contains information about the outcome of a request to confirm a pending unsubscribe for the
    /// Email Preference Entry block.
    /// </summary>
    public class ConfirmUnsubscribeResponseBag
    {
        /// <summary>
        /// Gets or sets the header title to display to the person.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the header description to display to the person.
        /// </summary>
        public string HeaderDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the header title to display to the person if they resubscribe.
        /// </summary>
        public string ResubscribedHeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the header description to display to the person if they resubscribe.
        /// </summary>
        public string ResubscribedHeaderDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the "Opt-Out Options" section description.
        /// </summary>
        public string OptOutOptionsDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the "Current Communication Channels" section description.
        /// </summary>
        public string CurrentCommunicationChannelsDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the "Available Communication Channels" section description.
        /// </summary>
        public string AvailableCommunicationChannelsDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the header description to display to the person when they request to be removed
        /// from involvement.
        /// </summary>
        public string RemoveInvolvementConfirmationDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the person's email preference.
        /// </summary>
        public EmailPreference EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the communication channel that was unsubscribed from.
        /// </summary>
        public CommunicationChannelBag UnsubscribedFromChannel { get; set; }

        /// <summary>
        /// Gets or sets the list of communication channels the person is able to manage within the "Current
        /// Communication Channels" section.
        /// </summary>
        public List<CommunicationChannelBag> CurrentCommunicationChannels { get; set; }
    }
}
