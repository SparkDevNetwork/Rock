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
using System.Collections.Generic;

using Rock.Enums.Communication.Chat;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatView
{
    /// <summary>
    /// A bag of settings for the chat view block.
    /// </summary>
    public class ChatViewConfigurationBag
    {
        /// <summary>
        /// Gets or sets the public API key for the chat service.
        /// </summary>
        public string PublicApiKey { get; set; }

        /// <summary>
        /// Whether or not to filter shared channels by campus.
        /// </summary>
        public bool FilterSharedChannelsByCampus { get; set; }

        /// <summary>
        /// Gets or sets the key for the shared channel type.
        /// </summary>
        public string SharedChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the key for the direct message channel type.
        /// </summary>
        public string DirectMessageChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the selected channel ID.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the selected channel ID.
        /// </summary>
        public string SelectedChannelId { get; set; }

        /// <summary>
        /// Gets or sets the selected message ID.
        /// </summary>
        public string JumpToMessageId { get; set; }

        /// <summary>
        /// Gets or sets the style of the chat view.
        /// </summary>
        public ChatViewStyle ChatStyle { get; set; }

        /// <summary>
        /// Gets or sets the supported reactions.
        /// </summary>
        public List<ChatReactionBag> Reactions { get; set; }
    }
}
