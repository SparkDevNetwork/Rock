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

namespace Rock.Communication.Chat.Exceptions
{
    /// <summary>
    /// Represents an error encountered when syncing data between Rock and the external chat system.
    /// </summary>
    /// <seealso cref="Exception"/>
    internal class ChatSyncException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatSyncException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ChatSyncException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatSyncException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ChatSyncException( string message, Exception innerException ) : base( message, innerException )
        {
        }
    }
}
