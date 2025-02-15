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
using Rock.Attribute;
using Rock.Communication.Chat.DTO;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// A configuration class to fine-tune how Rock-to-Chat synchronization should be completed.
    /// </summary>
    internal class RockToChatSyncConfig
    {
        /// <inheritdoc cref="IChatProvider.ShouldEnforceDefaultGrantsPerRole"/>
        [RockInternal( "17.0" )]
        public bool ShouldEnforceDefaultGrantsPerRole { get; set; }

        /// <inheritdoc cref="IChatProvider.ShouldEnforceDefaultSettings"/>
        [RockInternal( "17.0" )]
        public bool ShouldEnforceDefaultSettings { get; set; }

        /// <summary>
        /// Gets or sets whether Rock should always ensure that <see cref="ChatUser"/>s exist within the external
        /// chat system.
        /// </summary>
        /// <remarks>
        /// If <see langword="true"/>, Rock will query the chat provider (in bulk) for a matching <see cref="ChatUser"/>,
        /// for each chat-specific, preexisting <see cref="PersonAlias"/> being synchronized. If <see langword="false"/>,
        /// the preexistence of a chat-specific <see cref="PersonAlias"/> record will be considered "enough" to assume
        /// that a matching <see cref="ChatUser"/> already exists, at the risk of encountering API errors later in the
        /// sync process.
        /// </remarks>
        public bool ShouldEnsureChatUsersExist { get; set; }
    }
}
