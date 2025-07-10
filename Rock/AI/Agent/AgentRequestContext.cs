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
using System.Linq;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Rock.AI.Agent
{
    /// <summary>
    /// A request context is used to build up the context for a single chat
    /// request. This includes system messages, user messages, assistant
    /// responses and any other details that should be sent to the LLM.
    /// </summary>
    public class AgentRequestContext
    {
        #region Fields

        /// <summary>
        /// The list of system messages that should be included in the chat.
        /// </summary>
        private readonly List<ChatMessageContent> _systemMessages = new List<ChatMessageContent>();

        /// <summary>
        /// The context anchors that should be included in the chat. The key is
        /// the <see cref="Model.EntityType"/> identifier.
        /// </summary>
        private readonly Dictionary<int, string> _contextAnchors = new Dictionary<int, string>();

        /// <summary>
        /// The list of messages that have been exchanged in the chat.
        /// </summary>
        private readonly List<ChatMessageContent> _chatMessages = new List<ChatMessageContent>();

        /// <summary>
        /// The session context data that should be included in the chat.
        /// </summary>
        private readonly Dictionary<string, SessionContext> _sessionContext = new Dictionary<string, SessionContext>();

        /// <summary>
        /// The native Semantic Kernel object that holds the chat history. This
        /// is built on demand when <see cref="GetChatHistory()"/> is called.
        /// </summary>
        private ChatHistory _chatHistory = null;

        #endregion

        #region Properties

        /// <summary>
        /// The identifier of the <see cref="Model.AIAgent"/> that this request
        /// is being processed by.
        /// </summary>
        public int? AgentId { get; internal set; }

        /// <summary>
        /// The chat agent instance that this request is being processed by.
        /// </summary>
        public IChatAgent ChatAgent { get; internal set; }

        #endregion

        #region Methods

        internal void Clear()
        {
            _systemMessages.Clear();
            _contextAnchors.Clear();
            _chatMessages.Clear();
            _chatHistory = null;
        }

        internal void AddSystemMessage( string message )
        {
            _systemMessages.Add( new ChatMessageContent( AuthorRole.System, message ) );
            _chatHistory = null;
        }

        internal void AddUserMessage( string message )
        {
            _chatMessages.Add( new ChatMessageContent( AuthorRole.User, message ) );
            _chatHistory = null;
        }

        internal void AddAssistantMessage( string message )
        {
            _chatMessages.Add( new ChatMessageContent( AuthorRole.Assistant, message ) );
            _chatHistory = null;
        }

        internal void AddAnchor( int entityTypeId, string payload )
        {
            _contextAnchors[entityTypeId] = payload;
            _chatHistory = null;
        }

        internal void RemoveAnchor( int entityTypeId )
        {
            _contextAnchors.Remove( entityTypeId );
            _chatHistory = null;
        }

        internal void AddSessionContext( string key, SessionContext context )
        {
            if ( context.IsInternal )
            {
                return;
            }

            _sessionContext[key] = context;
            _chatHistory = null;
        }

        internal SessionContext GetSessionContext( string key )
        {
            if ( _sessionContext.TryGetValue( key, out var context ) )
            {
                return context;
            }

            return null;
        }

        internal void RemoveSessionContext( string key )
        {
            _sessionContext.Remove( key );
            _chatHistory = null;
        }

        internal ChatHistory GetChatHistory()
        {
            if ( _chatHistory == null )
            {
                var chatHistory = new ChatHistory( _systemMessages );

                foreach ( var anchor in _contextAnchors )
                {
                    chatHistory.AddSystemMessage( $"ContextAnchor|{anchor.Value}" );
                }

                foreach ( var context in _sessionContext.Values.OrderBy( c => c.CreatedDateTime ) )
                {
                    // Expiration checking is handled when the session is loaded.
                    // So we don't check it here.
                    var content = context.Content;

                    if ( context.HistoryContent.IsNotNullOrWhiteSpace() )
                    {
                        content = context.HistoryContent;
                    }

                    chatHistory.AddSystemMessage( $"Context|{content}" );
                }

                chatHistory.AddRange( _chatMessages );

                _chatHistory = chatHistory;
            }

            return _chatHistory;
        }

        #endregion
    }
}
