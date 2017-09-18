using System;

namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class NoteImport
    {
        /// <summary>
        /// Gets or sets the note foreign identifier.
        /// </summary>
        /// <value>
        /// The note foreign identifier.
        /// </value>
        public int NoteForeignId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the NoteType
        /// This determines the type of note and which EntityType is is for (Person, Family, etc)
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.NoteType"/>
        /// </value>
        public int NoteTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity foreign identifier.
        /// Example: This would be the PersonForeignId for Person Notes, and FamilyForeignId for FamilyNotes
        /// </summary>
        /// <value>
        /// The entity foreign identifier.
        /// </value>
        public int EntityForeignId { get; set; }

        /// <summary>
        /// Gets or sets the caption
        /// Max length is 200
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the caption of the Note.
        /// </value>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this note is an alert.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is an alert; otherwise <c>false</c>.
        /// </value>
        public bool IsAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is viewable to only the person that created the note
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private note; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivateNote { get; set; }

        /// <summary>
        /// Gets or sets the text/body of the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the text/body of the note.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// The date time that the note was created or last modified
        /// </summary>
        public DateTime? DateTime { get; set; }

        /// <summary>
        /// The ForeignId of the Person that created the note
        /// </summary>
        /// <value>
        /// The created by person identifier.
        /// </value>
        public int? CreatedByPersonForeignId { get; set; }
    }
}
