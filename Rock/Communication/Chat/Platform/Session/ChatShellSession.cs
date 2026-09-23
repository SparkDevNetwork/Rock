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

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;

namespace Rock.Communication.Chat.Platform.Session
{
    /// <summary>
    /// What the chat shell block answers when it opens and when it is asked for a token. It
    /// lives here rather than in the block because the unit tests cannot reach the blocks
    /// assembly, and a rule that lives only in a block can be removed with every test green.
    /// </summary>
    internal static class ChatShellSession
    {
        /// <summary>
        /// Runs the session gates for the person opening chat, enrols them on their first open,
        /// and describes the outcome for the browser. Never carries a token or a key.
        /// </summary>
        /// <param name="person">The person opening chat, or null when nobody is signed in.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used by the gates and the enrolment write.</param>
        /// <returns>The gate outcome and, when it passed, the public platform settings.</returns>
        public static ChatShellSessionBag Open( Person person, ChatSessionContext context, RockContext rockContext )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Re-runs every session gate and, when they pass, signs a church token for the person.
        /// </summary>
        /// <param name="person">The person asking, or null when nobody is signed in.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used by the gates.</param>
        /// <returns>The token and its expiry, or the gate that refused it.</returns>
        public static ChatChurchTokenBag MintToken( Person person, ChatSessionContext context, RockContext rockContext )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The stable snake_case code a gate outcome is known by outside Rock.
        /// </summary>
        /// <param name="gate">The gate outcome.</param>
        /// <returns>The code.</returns>
        public static string ToGateCode( ChatMintGate gate )
        {
            throw new NotImplementedException();
        }
    }
}
