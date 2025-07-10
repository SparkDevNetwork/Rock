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
using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Extension methods for <see cref="IChatAgent"/>. These provide additional
    /// convenience methods that build on the core methods provided by the
    /// interface.
    /// </summary>
    /// <remarks>
    /// This allows unit tests to provide mocked <see cref="IChatAgent"/>
    /// implementations without having to implement all the various overloads.
    /// </remarks>
    [RockInternal( "18.0" )]
    public static class IChatAgentExtensions
    {
        /// <summary>
        /// Starts a new session in the database without associating it with
        /// a specific entity.
        /// </summary>
        /// <param name="chatAgent">The chat agent instance.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        public static Task StartNewSessionAsync( this IChatAgent chatAgent )
        {
            return chatAgent.StartNewSessionAsync( null, null );
        }

        /// <summary>
        /// Adds a new session context item with the given key. If no session
        /// has been created or loaded then the context will only exist
        /// in-memory. If any existing context already exists with the same
        /// <paramref name="key"/> then it will be overwritten. The lifetime
        /// of the context will be 1 hour.
        /// </summary>
        /// <param name="chatAgent">The chat agent instance.</param>
        /// <param name="key">The unique key that identifies the context data.</param>
        /// <param name="content">The content that will be used internally and sent to the language model.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        public static Task AddSessionContextAsync( this IChatAgent chatAgent, string key, string content )
        {
            return AddSessionContextAsync( chatAgent, key, content, null, TimeSpan.FromHours( 1 ), false );
        }

        /// <summary>
        /// Adds a new session context item with the given key. If no session
        /// has been created or loaded then the context will only exist
        /// in-memory. If any existing context already exists with the same
        /// <paramref name="key"/> then it will be overwritten.
        /// </summary>
        /// <param name="chatAgent">The chat agent instance.</param>
        /// <param name="key">The unique key that identifies the context data.</param>
        /// <param name="content">The content that will be used internally and sent to the language model.</param>
        /// <param name="lifetime">The duration of time that this context will remain in the chat history before it is automatically expired.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        public static Task AddSessionContextAsync( this IChatAgent chatAgent, string key, string content, TimeSpan lifetime )
        {
            return AddSessionContextAsync( chatAgent, key, content, null, lifetime, false );
        }

        /// <summary>
        /// Adds a new session context item with the given key. If no session
        /// has been created or loaded then the context will only exist
        /// in-memory. If any existing context already exists with the same
        /// <paramref name="key"/> then it will be overwritten.
        /// </summary>
        /// <param name="chatAgent">The chat agent instance.</param>
        /// <param name="key">The unique key that identifies the context data.</param>
        /// <param name="content">The content that will be used internally.</param>
        /// <param name="historyContent">The content that will be sent to the language model. Pass <c>null</c> or an empty string to use the same value as <paramref name="content"/>.</param>
        /// <param name="lifetime">The duration of time that this context will remain in the chat history before it is automatically expired.</param>
        /// <param name="isInternal"><c>true</c> if this context should be kept internal and not sent to the language model in the chat history.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        public static Task AddSessionContextAsync( this IChatAgent chatAgent, string key, string content, string historyContent, TimeSpan lifetime, bool isInternal )
        {
            var context = new SessionContext
            {
                Content = content,
                CreatedDateTime = RockDateTime.Now,
                ExpireDateTime = RockDateTime.Now.Add( lifetime ),
                HistoryContent = historyContent,
                IsInternal = isInternal,
            };

            return chatAgent.AddSessionContextAsync( key, context );
        }
    }
}
