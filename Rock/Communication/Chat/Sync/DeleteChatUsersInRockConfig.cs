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
using Rock.Communication.Chat.DTO;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// A configuration class to fine-tune how how <see cref="ChatUser"/>s are deleted in Rock.
    /// </summary>
    internal class DeleteChatUsersInRockConfig
    {
        /// <summary>
        /// Gets or sets whether the <see cref="ChatUser"/>s should be removed from the Rock "Chat Ban List"
        /// <see cref="Group"/>, if a member.
        /// </summary>
        public bool ShouldUnban { get; set; }

        /// <summary>
        /// Gets or sets whether to clear the foreign key values of chat-specific <see cref="PersonAlias"/>es whose
        /// <see cref="ChatUser"/>s were deleted.
        /// </summary>
        /// <remarks>
        /// Default is <see langword="true"/>. Set to <see langword="false"/> if a follow-up process will handle this.
        /// </remarks>
        public bool ShouldClearChatPersonAliasForeignKey { get; set; } = true;
    }
}
