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

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Box containing initialization information for the Communication Entry block.
    /// </summary>
    public class CommunicationEntryInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the mediums for the tab control.
        /// </summary>
        /// <value>
        /// The mediums for the tab control.
        /// </value>
        public List<ListItemBag> Mediums { get; set; }

        /// <summary>
        /// Gets or sets the authorization details for the block.
        /// </summary>
        /// <value>
        /// The authorization details for the block.
        /// </value>
        public CommunicationEntryAuthorizationBag Authorization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entire Communication Entry block is hidden.
        /// <para>There should be another block on the page that will display the Communication details in view mode when this is true.</para>
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the entire Communication Entry block is hidden; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the communication details being created/edited.
        /// </summary>
        /// <value>
        /// The communication details being created/edited.
        /// </value>
        public CommunicationEntryCommunicationBag Communication { get; set; }

        /// <summary>
        /// Gets or sets the options for the selected communication medium.
        /// </summary>
        /// <value>
        /// The options for the selected communication medium.
        /// </value>
        public CommunicationEntryMediumOptionsBaseBag MediumOptions { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of recipients allowed before communication will need to be approved.
        /// </summary>
        public int MaximumRecipientsBeforeApprovalRequired { get; set; }

        /// <summary>
        /// Gets a value indicating whether lava is supported.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if lava in the message should be resolved; otherwise, <see langword="false"/> if lava should be removed from the message without resolving it.
        /// </value>
        public bool IsLavaEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// <see cref="Mode.Simple" /> will prevent users from searching/adding new people to the communication.
        /// </value>
        public Mode Mode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in edit mode.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the block is in edit mode; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsEditMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CC/BCC entry is allowed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if CC/BCC entry is allowed; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCcBccEntryAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email metrics reminder options are shown.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the email metrics reminder options are shown; otherwise, <see langword="false"/>.
        /// </value>
        public bool AreEmailMetricsReminderOptionsShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether additional email recipients are allowed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if additional email recipients are allowed; otherwise, <see langword="false"/>.
        /// </value>
        public bool AreAdditionalEmailRecipientsAllowed { get; set; }
    }
}
