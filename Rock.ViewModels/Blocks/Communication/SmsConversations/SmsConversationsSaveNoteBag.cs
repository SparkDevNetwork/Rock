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
using Rock.ViewModels.Blocks.Core.Notes;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Save Note Bag for SMS Conversations
    /// </summary>
    public class SmsConversationsSaveNoteBag
    {
        /// <summary>
        /// Gets or sets the alias ID key for the selected person.
        /// </summary>
        public string SelectedPersonAliasIdKey { get; set; }

        /// <summary>
        /// Gets or sets the note request bag containing the details for saving a note.
        /// </summary>
        public SaveNoteRequestBag NoteRequestBag { get; set; }

    }
}
