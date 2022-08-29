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
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    public class PersonNoteCsvMapper
    {
        public static SlingshotCore.Model.PersonNote Map( IDictionary<string, object> csvEntry, Dictionary<string, string> headerMapper, ref HashSet<string> parserErrors )
        {
            string noteType = "PERSON_TIMELINE_NOTE";
            string csvColumnNote = headerMapper.GetValueOrNull( CSVHeaders.Note );
            if ( csvColumnNote == null )
            {
                return null;
            }
            var personNote = new SlingshotCore.Model.PersonNote
            {
                PersonId = csvEntry[headerMapper[CSVHeaders.Id]].ToIntSafe(),
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
    }
}