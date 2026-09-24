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
using System.Net.Http;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The platform token a sync is made under.
    /// </summary>
    internal static class ChatSyncCredential
    {
        /// <summary>
        /// Exchanges the church's signed sync token for a platform token.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="churchToken">The church's sync-scope token, or null when the church could not sign one.</param>
        /// <param name="failure">Why there is no token, when there is none.</param>
        /// <param name="handler">The transport, or null for the ordinary one.</param>
        /// <returns>The platform token, or null.</returns>
        public static string Exchange( ChatPlatformConfiguration configuration, string churchToken, out string failure, HttpMessageHandler handler = null )
        {
            throw new NotImplementedException();
        }
    }
}
