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

using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;

namespace Rock.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// Records the birthdate a person gives chat when chat cannot open without one. It
    /// writes only what Rock does not already know, so it can never be used to change a
    /// recorded age, and only when chat is asking for it.
    /// </summary>
    internal static class ChatBirthdateDoor
    {
        /// <summary>The birthdate was written.</summary>
        public const string Saved = "saved";

        /// <summary>Rock already holds a birthdate, or a part of one this date disagrees with.</summary>
        public const string BirthdateRecorded = "birthdate_recorded";

        /// <summary>Chat is not asking this person for a birthdate.</summary>
        public const string NotAsked = "not_asked";

        /// <summary>The date is incomplete, does not exist, or is after today.</summary>
        public const string InvalidDate = "invalid_date";

        /// <summary>Nobody is signed in.</summary>
        public const string SignInRequired = "sign_in_required";

        /// <summary>
        /// Writes the person's birthdate when chat is asking for it and Rock does not already
        /// hold it, then describes the session as it now stands.
        /// </summary>
        /// <param name="personId">The signed-in person, or null when nobody is signed in.</param>
        /// <param name="year">The year given, or zero when none was.</param>
        /// <param name="month">The month given, or zero when none was.</param>
        /// <param name="day">The day given, or zero when none was.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used to read and write the person, and by the gates.</param>
        /// <returns>What happened, and the session after it.</returns>
        public static ChatBirthdateResultBag Save( int? personId, int year, int month, int day, ChatSessionContext context, RockContext rockContext )
        {
            var person = personId.HasValue ? new PersonService( rockContext ).Get( personId.Value ) : null;

            if ( person == null )
            {
                return Refused( SignInRequired, null, context, rockContext );
            }

            if ( person.BirthYear.HasValue && person.BirthMonth.HasValue && person.BirthDay.HasValue )
            {
                return Refused( BirthdateRecorded, person, context, rockContext );
            }

            // Every gate ahead of the age gate runs first, so a person chat would refuse for any
            // other reason is never asked, and nothing is written for them.
            if ( ChatSessionHelper.Evaluate( person, context, rockContext ).Gate != ChatMintGate.AgeVerificationRequired )
            {
                return Refused( NotAsked, person, context, rockContext );
            }

            var birthDate = ToDate( year, month, day );

            if ( !birthDate.HasValue )
            {
                return Refused( InvalidDate, person, context, rockContext );
            }

            var isRecordedPartDifferent = ( person.BirthYear.HasValue && person.BirthYear.Value != year )
                || ( person.BirthMonth.HasValue && person.BirthMonth.Value != month )
                || ( person.BirthDay.HasValue && person.BirthDay.Value != day );

            if ( isRecordedPartDifferent )
            {
                return Refused( BirthdateRecorded, person, context, rockContext );
            }

            person.SetBirthDate( birthDate.Value );
            rockContext.SaveChanges();

            return new ChatBirthdateResultBag
            {
                Code = Saved,
                Session = ChatShellSession.Open( person, context, rockContext )
            };
        }

        /// <summary>
        /// The date the three parts name, when they name a real one no later than today.
        /// </summary>
        /// <param name="year">The year given.</param>
        /// <param name="month">The month given.</param>
        /// <param name="day">The day given.</param>
        /// <returns>The date, or null when the parts do not make one chat may record.</returns>
        private static DateTime? ToDate( int year, int month, int day )
        {
            // Rock stores year 1 as "no year", so a date in it would be saved without one.
            if ( year <= DateTime.MinValue.Year || year > 9999 || month < 1 || month > 12 )
            {
                return null;
            }

            if ( day < 1 || day > DateTime.DaysInMonth( year, month ) )
            {
                return null;
            }

            var date = new DateTime( year, month, day );

            return date > RockDateTime.Today ? ( DateTime? ) null : date;
        }

        /// <summary>
        /// A refusal, carrying the session as it stands so the shell can follow it.
        /// </summary>
        /// <param name="code">Why nothing was written.</param>
        /// <param name="person">The person, or null when nobody is signed in.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used by the gates.</param>
        /// <returns>The refusal.</returns>
        private static ChatBirthdateResultBag Refused( string code, Person person, ChatSessionContext context, RockContext rockContext )
        {
            return new ChatBirthdateResultBag
            {
                Code = code,
                Session = ChatShellSession.Open( person, context, rockContext )
            };
        }
    }
}
