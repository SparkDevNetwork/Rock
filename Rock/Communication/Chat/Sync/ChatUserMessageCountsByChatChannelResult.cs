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

using Rock.Communication.Chat.DTO;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents the result of querying the external chat system for message counts for each <see cref="ChatUser"/>
    /// within each <see cref="ChatChannel"/>.
    /// </summary>
    internal class ChatUserMessageCountsByChatChannelResult
    {
        /// <summary>
        /// Gets or sets the message date for which the counts were queried.
        /// </summary>
        public DateTime MessageDate { get; set; }

        /// <summary>
        /// Gets or sets the message counts for each <see cref="ChatUser"/> within each <see cref="ChatChannel"/>.
        /// </summary>
        /// <remarks>
        /// The outer dictionary key is <see cref="ChatChannel.Key"/> and the inner dictionary key is <see cref="ChatUser.Key"/>.
        /// </remarks>
        public Dictionary<string, Dictionary<string, int>> MessageCounts { get; set; }

        /// <summary>
        /// Gets or sets the exception - if any - that occurred during the querying operation.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets a value indicating whether an exception occurred during the querying operation.
        /// </summary>
        public bool HasException => Exception != null;
    }
}
