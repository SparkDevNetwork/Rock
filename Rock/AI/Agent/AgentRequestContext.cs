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
using System.Text.Json;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Lava;
using Rock.Net;
using Rock.Web.Cache;

using SKAuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;

namespace Rock.AI.Agent
{
    /// <summary>
    /// A request context is used to build up the context for a single chat
    /// request. This includes system messages, user messages, assistant
    /// responses and any other details that should be sent to the LLM.
    /// </summary>
    [LavaType]
    internal class AgentRequestContext
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

        /// <inheritdoc cref="Model.AIAgent.Name"/>
        public string AgentName { get; internal set; }

        /// <inheritdoc cref="Model.AIAgent.AgentType"/>
        public AgentType AgentType { get; internal set; }

        /// <inheritdoc cref="Model.AIAgent.AudienceType"/>
        public AudienceType AudienceType { get; internal set; }

        /// <summary>
        /// The context that identifies the currently executing web request.
        /// </summary>
        public RockRequestContext RockRequestContext { get; internal set; }

        /// <summary>
        /// The <see cref="RockContext"/> that can be used to query the database.
        /// This context is automatically disposed after the request is completed.
        /// This context should not be used to save changes to the database. To
        /// save changes create a new context by injecting <see cref="IRockContextFactory"/>
        /// into your skill constructor.
        /// </summary>
        public RockContext RockContext { get; internal set; }

        /// <summary>
        /// The chat agent instance that this request is being processed by.
        /// </summary>
        public IChatAgent ChatAgent { get; internal set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="AgentRequestContext"/> class.
        /// </summary>
        /// <param name="rockRequestContext">The context for the current web request in process.</param>
        /// <param name="rockContext">The database context that can be used for read-only operations.</param>
        internal AgentRequestContext( RockRequestContext rockRequestContext, RockContext rockContext )
        {
            RockRequestContext = rockRequestContext;
            RockContext = rockContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resolves ~/ and ~~/ to the proper URL format.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ResolveRockUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() || url.Contains( "://" ) )
            {
                return url;
            }

            url = RockApp.Current.ResolveRockUrl( url );

            // Chat agents should get relative URLs.
            if ( this.AgentType == AgentType.Chat )
            {
                return url;
            }

            // MCP agents will need full URLs using the appropriate application root.
            if ( AudienceType == AudienceType.Public )
            {
                return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ).RemoveLeadingForwardslash() + url;
            }

            return GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ).RemoveLeadingForwardslash() + url;
        }

        /// <summary>
        /// Clears all system messages, context anchors, chat messages, and cached chat history from the context.
        /// </summary>
        internal void Clear()
        {
            _systemMessages.Clear();
            _contextAnchors.Clear();
            _chatMessages.Clear();
            _chatHistory = null;
        }

        /// <summary>
        /// Adds a new system message to the chat context.
        /// </summary>
        /// <param name="message">The message to add as a system message.</param>
        internal void AddSystemMessage( string message )
        {
            _systemMessages.Add( new ChatMessageContent( SKAuthorRole.System, message ) );
            _chatHistory = null;
        }

        /// <summary>
        /// Adds a new user message to the chat context.
        /// </summary>
        /// <param name="message">The message to add as a user message.</param>
        internal void AddUserMessage( string message )
        {
            _chatMessages.Add( new ChatMessageContent( SKAuthorRole.User, message ) );
            _chatHistory = null;
        }

        /// <summary>
        /// Adds a new tool result to the chat context. This is usually done automatically by History Content.
        /// </summary>
        internal void AddToolResultMessage( string toolResultContentJson )
        {
            var serializerOptions = AgentSerializerOptions.GetOptions( AgentType, AudienceType );
            var toolResultContent = JsonSerializer.Deserialize<ToolResultContent>( toolResultContentJson, serializerOptions );

            var callContent = new FunctionCallContent(
                functionName: toolResultContent.ToolName,
                pluginName: toolResultContent.PluginName,
                id: toolResultContent.CallId
            );

            var resultContent = new FunctionResultContent(
                functionCall: callContent,
                result: toolResultContent.Result );

            // First, add the assistant message describing the tool invocation.
            var callContentArray = new ChatMessageContentItemCollection()
            {
                callContent
            };
            var invocationMessageContent = new ChatMessageContent( SKAuthorRole.Assistant, callContentArray );
            _chatMessages.Add( invocationMessageContent );

            // Then add the resulting message to the tool role.
            var resultContentArray = new ChatMessageContentItemCollection()
            {
                resultContent
            };
            var toolResultMessageContent = new ChatMessageContent( SKAuthorRole.Tool, resultContentArray );
            _chatMessages.Add( toolResultMessageContent );

            // Reset chat history.
            _chatHistory = null;
        }

        /// <summary>
        /// Adds a new assistant message to the chat context.
        /// </summary>
        /// <param name="message">The message to add as an assistant message.</param>
        internal void AddAssistantMessage( string message )
        {
            _chatMessages.Add( new ChatMessageContent( SKAuthorRole.Assistant, message ) );
            _chatHistory = null;
        }

        /// <summary>
        /// Adds or replaces a context anchor for the specified entity type in the chat context.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="payload">The context anchor payload to associate with the entity type.</param>
        internal void AddAnchor( int entityTypeId, string payload )
        {
            _contextAnchors[entityTypeId] = payload;
            _chatHistory = null;
        }

        /// <summary>
        /// Removes a context anchor for the specified entity type from the chat context.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        internal void RemoveAnchor( int entityTypeId )
        {
            _contextAnchors.Remove( entityTypeId );
            _chatHistory = null;
        }

        /// <summary>
        /// Gets the full chat history for the current context, including system messages, context anchors, session contexts, and chat messages.
        /// </summary>
        /// <returns>The aggregated chat history object.</returns>
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

        #endregion
    }
}
