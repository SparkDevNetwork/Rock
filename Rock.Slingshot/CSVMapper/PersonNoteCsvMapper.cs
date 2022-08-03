using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Rock;
using Slingshot.Core.Model;

public class PersonNoteCsvMapper
{
    public static PersonNote Map( IDictionary<string, object> csvEntry, Dictionary<string, string> headerMapper, ref HashSet<string> parserErrors )
    {
        string noteType = "PERSON_TIMELINE_NOTE";
        if ( headerMapper.TryGetValue( "Note", out string csvColumnNote ) )
        {
            var personNote = new PersonNote
            {
                PersonId = csvEntry[headerMapper["Id"]].ToIntSafe(),
                Text = csvEntry[csvColumnNote].ToStringSafe(),
                NoteType = noteType
            };

            // generate a unique note id
            // A note Id is mandatory and it needs to be unique for a note from a person from the same external source
            // Referring https://github.com/SparkDevNetwork/Slingshot/blob/8f590afab01714299397485a93d81b023e54ea15/Slingshot.ACS/Slingshot.ACS/Utilities/Translators/AcsPersonNote.cs#L31
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash( Encoding.UTF8.GetBytes( personNote.PersonId.ToString() + personNote.Text + personNote.NoteType ) );
            personNote.Id = Math.Abs( BitConverter.ToInt32( hashed, 0 ) ); // used abs to ensure positive number
            return personNote;
        }
        return null;
    }
}
