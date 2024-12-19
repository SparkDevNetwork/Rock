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
