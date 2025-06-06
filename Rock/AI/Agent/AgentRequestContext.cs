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
using System.Text.Json;

using Microsoft.SemanticKernel.ChatCompletion;

namespace Rock.AI.Agent
{
    public class AgentRequestContext
    {
        private bool _chatHistoryDirty = true;
        private ChatHistory _chatHistory = new ChatHistory();

        public int? AgentId { get; set; } = null;

        public ChatHistory ChatHistory
        {
            get
            {
                if ( _chatHistoryDirty )
                {
                    RebuildChatHistory();
                }

                return _chatHistory;
            }
        }

        public List<ContextAnchor> ContextAnchors { get; set; } = new List<ContextAnchor>();

        public void AddOrUpdateContextAnchor( int entityTypeId, int entityId, string entityName, string entityMetadata = null )
        {
            // Check if the anchor of that type already exists
            var existingAnchor = ContextAnchors.FirstOrDefault( a => a.EntityTypeId == entityTypeId );
            if ( existingAnchor != null )
            {
                // Update the existing anchor
                existingAnchor.EntityId = entityId;
                existingAnchor.EntityName = entityName;
                existingAnchor.EntityMetaData = entityMetadata;
            }
            else
            {
                // Add the new anchor
                ContextAnchors.Add( new ContextAnchor()
                {
                    EntityId = entityId,
                    EntityTypeId = entityTypeId,
                    EntityName = entityName,
                    EntityMetaData = entityMetadata
                } );
            }

            _chatHistoryDirty = true;
        }

        public void RemoveContextAnchor( int entityTypeId )
        {
            // Remove the anchor of that type
            ContextAnchors.RemoveAll( a => a.EntityTypeId == entityTypeId );

            _chatHistoryDirty = true;
        }

        /// <summary>
        /// We will rebuild the chat history each time a context anchor is added or removed.
        /// </summary>
        private void RebuildChatHistory()
        {
            // We need to rebuild this each time its accessed so that we have the latest anchors
            // TODO: worried if this is a performance issue
            var chatHistory = new ChatHistory();

            // Add our global core system message. Here we can provide some basic instructions to the agent.
            chatHistory.AddSystemMessage( _coreSystemPrompt );

            // Add the agent persona prompt
            // TODO: this should be read from the AgentCache in the future
            chatHistory.AddSystemMessage( "Persona|You are a chatbot named Chip for Rock Solid Church aka RSC. Please assist with helping people. Be friendly but concise." );

            // Add current person context TODO: make this dynamic
            chatHistory.AddSystemMessage( $"CurrentPerson|The current person is Ted Decker (id: 23423) he is the Discipleship Pastor." );

            // Add the context anchors
            foreach ( var anchor in ContextAnchors )
            {
                chatHistory.AddSystemMessage( $"ContextAnchor|{JsonSerializer.Serialize( anchor )})" );
            }

            // Add the existing assistant and user messages
            foreach ( var message in _chatHistory.Where( h =>
                ( h.Role == AuthorRole.Assistant || h.Role == AuthorRole.User )
                && h.Content != null ) )
            {
                chatHistory.AddMessage( message.Role, message.Content );
            }

            _chatHistory = chatHistory;
            _chatHistoryDirty = false;
        }


        // TODO: This should move to a configuration object.
        private static string _coreSystemPrompt = @"CoreSystem|
                You are a chatbot for Rock RMS process the tasks given. When you're unsure or don't know please reply 
                with I'm sorry I can't assist you with that. The context anchor is the entity (e.g. person, group, etc) that is currently 
                being focused on. The id of this anchor can be used by functions for completing work.

                If you don't know something, check the Knowledge Base.
                ".NormalizeWhiteSpace();
    }
}
