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

using Rock.Communication.Chat.DTO;

namespace Rock.Communication.Chat.Exceptions
{
    /// <summary>
    /// Represents an error encountered when unable to find a <see cref="ChatChannel"/> within the external chat system.
    /// </summary>
    internal class ChatChannelNotFoundException : Exception
    {
        /// <summary>
        /// Gets or sets the key of the <see cref="ChatChannelType"/> to which this <see cref="ChatChannel"/> should belong.
        /// </summary>
        public string ChatChannelTypeKey { get; private set; }

        /// <summary>
        /// Gets or sets the key of the <see cref="ChatChannel"/> that could not be found.
        /// </summary>
        public string ChatChannelKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatChannelNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> to which this
        /// <see cref="ChatChannel"/> should belong.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> that could not be found.</param>
        public ChatChannelNotFoundException( string message, string chatChannelTypeKey, string chatChannelKey ) : base( message )
        {
            ChatChannelTypeKey = chatChannelTypeKey;
            ChatChannelKey = chatChannelKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatChannelNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> to which this
        /// <see cref="ChatChannel"/> should belong.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> that could not be found.</param>
        public ChatChannelNotFoundException( string message, Exception innerException, string chatChannelTypeKey, string chatChannelKey ) : base( message, innerException )
        {
            ChatChannelTypeKey = chatChannelTypeKey;
            ChatChannelKey = chatChannelKey;
        }
    }
}
