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

using Rock.Enums.Communication;
using Rock.ViewModels.Blocks.Core.Notes;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The SMS Conversations Initialization Box
    /// </summary>
    public class SmsConversationsInitializationBox
    {
        /// <summary>
        /// Gets or sets the list of System Phone Numbers
        /// </summary>
        public List<ListItemBag> SystemPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the list of Conversations
        /// </summary>
        public List<ConversationBag> Conversations { get; set; }

        /// <summary>
        /// Gets or sets the filter used to filter communication messages.
        /// </summary>
        public CommunicationMessageFilter MessageFilter { get; set; }

        /// <summary>
        /// Gets or sets the list of available note types.
        /// </summary>
        public List<NoteTypeBag> NoteTypes { get; set; }

        /// <summary>
        /// Gets or sets the list of available snippets.
        /// </summary>
        public List<SnippetBag> Snippets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "New Message" button is visible.
        /// </summary>
        public bool IsNewMessageButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
