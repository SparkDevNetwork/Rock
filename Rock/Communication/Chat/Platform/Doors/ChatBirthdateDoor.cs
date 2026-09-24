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
            throw new NotImplementedException();
        }
    }
}
