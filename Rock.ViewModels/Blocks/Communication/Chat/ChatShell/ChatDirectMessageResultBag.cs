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

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatShell
{
    /// <summary>
    /// What starting a direct message came to: the conversation to send the first message into,
    /// or why there is none.
    /// </summary>
    public class ChatDirectMessageResultBag
    {
        /// <summary>
        /// Gets or sets the outcome as a stable code: "ok" when the conversation exists, otherwise
        /// the reason it does not.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the conversation, which is the chat group's Guid, when the code is "ok".
        /// </summary>
        public Guid? ChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock made the conversation but the chat platform
        /// had not taken it yet when the answer was sent, so the first message waits for it.
        /// </summary>
        public bool IsPending { get; set; }

        /// <summary>
        /// Gets or sets the sentence to show the person when the conversation was refused.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the alias, as the request gave it, of the person who could not be put in
        /// the conversation, when the refusal is about one person.
        /// </summary>
        public Guid? PersonAliasGuid { get; set; }
    }
}
