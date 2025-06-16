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
    public class AgentRequestContext
    {
        private List<ChatMessageContent> _systemMessages;

        private Dictionary<int, string> _contextAnchors;

        private List<ChatMessageContent> _chatMessages;

        private ChatHistory _chatHistory = null;

        public int? AgentId { get; internal set; } = null;

        //public IReadOnlyList<ChatMessageContent> ChatHistory => InternalChatHistory;

        //public List<ContextAnchor> ContextAnchors { get; private set; } = new List<ContextAnchor>();

        internal void Clear()
        {
            _systemMessages = new List<ChatMessageContent>();
            _contextAnchors = new Dictionary<int, string>();
            _chatMessages = new List<ChatMessageContent>();
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

        internal ChatHistory GetChatHistory()
        {
            if ( _chatHistory == null )
            {
                var chatHistory = new ChatHistory( _systemMessages );

                foreach ( var anchor in _contextAnchors )
                {
                    chatHistory.AddSystemMessage( $"ContextAnchor|{anchor.Value}" );
                }

                chatHistory.AddRange( _chatMessages );

                _chatHistory = chatHistory;
            }

            return _chatHistory;
        }

        //internal void CopyFrom( AgentRequestContext other )
        //{
        //    AgentId = other.AgentId;
        //    InternalChatHistory = new ChatHistory( other.InternalChatHistory );
        //    ContextAnchors = new List<ContextAnchor>( other.ContextAnchors );

        //    // If the last chat message is a tool call, then we are copying
        //    // history for a currently executing function call. So we need to
        //    // remove that message otherwise there will be an error when
        //    // executing the new request.
        //    if ( InternalChatHistory.Count > 0 )
        //    {
        //        var lastMessage = InternalChatHistory[InternalChatHistory.Count - 1];

        //        if ( lastMessage.Items.Any( it => it is FunctionCallContent ) )
        //        {
        //            InternalChatHistory.RemoveAt( InternalChatHistory.Count - 1 );
        //        }
        //    }
        //}

        // TODO: This should move to a configuration object.
        internal static string _coreSystemPrompt = @"CoreSystem|
                You are a chatbot for Rock RMS process the tasks given. When you're unsure or don't know please reply 
                with I'm sorry I can't assist you with that. The context anchor is the entity (e.g. person, group, etc) that is currently 
                being focused on. The id of this anchor can be used by functions for completing work.

                If you don't know something, check the Knowledge Base.
                ".NormalizeWhiteSpace();
    }
}
