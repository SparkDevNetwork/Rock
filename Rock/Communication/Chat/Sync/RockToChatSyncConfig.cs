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
        /// <summary>
        /// Gets or sets whether Rock should enforce default permission grants per role.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see langword="true"/>, Rock will be treated as the system of truth for chat permission grants, and
        /// ensure that all default grants - per role and scope combination - are set within the external chat system.
        /// If <see langword="false"/>, grants will only be set if there are not already any grants set for a given role
        /// and scope combination; in this latter case, the external chat system will be treated as the system of truth
        /// for permission grants.
        /// </para>
        /// <para>
        /// This is an internal property used for testing. It will most likely be removed in a future version of Rock.
        /// </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public bool ShouldEnforceDefaultGrantsPerRole { get; set; }

        /// <summary>
        /// Gets or sets whether Rock should enforce default settings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see langword="true"/>, Rock will be treated as the system of truth for some settings (e.g. property
        /// values for channel types, channels, Etc. that already exist in the external chat system), and Rock will
        /// enforce these required default settings when synchronizing with the external chat system. If <see langword="false"/>,
        /// existing settings within the external chat system will generally NOT be overwritten.
        /// </para>
        /// <para>
        /// This is an internal property used for testing. It will most likely be removed in a future version of Rock.
        /// </para>
        /// </remarks>
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
        public bool ShouldEnsureChatUsersExist { get; set; } = true;
    }
}
