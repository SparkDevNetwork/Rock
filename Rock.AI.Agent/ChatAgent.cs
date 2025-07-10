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
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

using Rock.Data;
using Rock.Model;
using Rock.Net;

using AuthorRole = Rock.Enums.Core.AI.Agent.AuthorRole;

namespace Rock.AI.Agent
{
    internal class ChatAgent : IChatAgent
    {
        #region Constants

        /// <summary>
        /// The core system prompt that is included in every chat session. This
        /// cannot be removed or overridden by the agent configuration.
        /// </summary>
        private static readonly string CoreSystemPrompt = @"CoreSystem|
                You are a chatbot for Rock RMS process the tasks given. When you're unsure or don't know please reply 
                with I'm sorry I can't assist you with that. The context anchor is the entity (e.g. person, group, etc) that is currently 
                being focused on. The id of this anchor can be used by functions for completing work.

                If you don't know something, check the Knowledge Base.
                ".NormalizeWhiteSpace();

        /// <summary>
        /// The prompt that will be used when asking the language model to
        /// summarize the current chat history. This is used when the current
        /// history grows beyond the threshold defined in the agent.
        /// </summary>
        private const string AutoSummarizePrompt = "Provide a very brief summary of the following conversation, including only the most important details."
            + " This will be used when sending subsequent requests to the language model."
            + " It should reduce extra whitespace and doesn't need to be user-friendly:\n\n";

        #endregion

        #region Fields

        /// <summary>
        /// The configuration data for the agent.
        /// </summary>
        private readonly AgentConfiguration _agentConfiguration;

        /// <summary>
        /// The <see cref="Kernel"/> instance that will be used to communicate
        /// with the language model.
        /// </summary>
        private readonly Kernel _kernel;

        /// <summary>
        /// The factory used when creating new <see cref="RockContext"/> objects.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        /// <summary>
        /// The object that will provide the current <see cref="RockRequestContext"/>
        /// associated with the current request.
        /// </summary>
        private readonly RockRequestContext _requestContext;

        /// <summary>
        /// The context for the current request. This is used to build up the
        /// chat history, anchors and session context.
        /// </summary>
        private readonly AgentRequestContext _context;

        /// <summary>
        /// The tokenizer that will be used to count tokens when adding messages
        /// when the provider doesn't give us a valid count.
        /// </summary>
        private readonly Lazy<TiktokenTokenizer> _tokenizer = new Lazy<TiktokenTokenizer>( CreateTokenizer );

        /// <summary>
        /// Indicates whether the chat history needs to be summarized before
        /// sending a new message to the language model.
        /// </summary>
        private bool _historyNeedsSummary = false;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public int? SessionId { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgent"/> class.
        /// </summary>
        /// <param name="kernel">The <see cref="Kernel"/> instance that will be used to communicate with the language model.</param>
        /// <param name="agentConfiguration">The configuration data for the agent.</param>
        /// <param name="rockContextFactory">The factory used when creating new <see cref="RockContext"/> objects.</param>
        /// <param name="rockRequestContextAccessor">The object that will provide the current <see cref="RockRequestContext"/> associated with the current request.</param>
        public ChatAgent( Kernel kernel, AgentConfiguration agentConfiguration, IRockContextFactory rockContextFactory, IRockRequestContextAccessor rockRequestContextAccessor )
        {
            _kernel = kernel;
            _agentConfiguration = agentConfiguration;
            _rockContextFactory = rockContextFactory;
            _requestContext = rockRequestContextAccessor.RockRequestContext;

            _context = kernel.Services.GetRequiredService<AgentRequestContext>();
            _context.AgentId = _agentConfiguration.AgentId;
            _context.ChatAgent = this;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public Task StartNewSessionAsync( int? entityTypeId, int? entityId )
        {
            if ( _requestContext?.CurrentPerson?.PrimaryAliasId == null )
            {
                throw new Exception( "Cannot start a new session without a current person." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var sessionService = new AIAgentSessionService( rockContext );
                var session = new AIAgentSession
                {
                    AIAgentId = _agentConfiguration.AgentId,
                    PersonAliasId = _requestContext.CurrentPerson.PrimaryAliasId.Value,
                    RelatedEntityTypeId = entityTypeId,
                    RelatedEntityId = entityId
                };

                sessionService.Add( session );

                rockContext.SaveChanges();

                _context.Clear();
                AddSystemMessages();

                SessionId = session.Id;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task LoadSessionAsync( int sessionId )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var session =  new AIAgentSessionService( rockContext ).Get( sessionId )
                    ?? throw new Exception( "The specified session could not be found." );

                // Get all the messages by either the user or the assistant.
                var messages = new AIAgentSessionHistoryService( rockContext ).Queryable()
                    .Where( s => s.AIAgentSessionId == sessionId
                        && s.IsCurrentlyInContext )
                    .OrderBy( s => s.MessageDateTime )
                    .ThenBy( s => s.Id )
                    .Select( s => new
                    {
                        s.MessageRole,
                        s.Message,
                        s.TokenCount
                    } )
                    .ToList();

                // Get all entity anchors that are still active.
                var anchors = new AIAgentSessionAnchorService( rockContext ).Queryable()
                    .Where( a => a.AIAgentSessionId == sessionId
                        && a.IsActive )
                    .OrderByDescending( a => a.AddedDateTime )
                    .Select( a => new
                    {
                        a.EntityTypeId,
                        a.PayloadJson
                    } )
                    .ToList();

                var contexts = session.GetSessionContextDictionary();

                _context.Clear();
                AddSystemMessages();

                // Add all the entity anchors, skipping any duplicates.
                var anchorEntities = new List<int>( anchors.Count );
                foreach ( var anchor in anchors )
                {
                    if ( anchorEntities.Contains( anchor.EntityTypeId ) )
                    {
                        continue;
                    }

                    anchorEntities.Add( anchor.EntityTypeId );

                    _context.AddAnchor( anchor.EntityTypeId, anchor.PayloadJson );
                }

                // Add all session context data.
                foreach ( var contextData in contexts )
                {
                    _context.AddSessionContext( contextData.Key, contextData.Value );
                }

                // Add all the user and assistant messages.
                foreach ( var message in messages )
                {
                    if ( message.MessageRole == AuthorRole.User )
                    {
                        _context.AddUserMessage( message.Message );
                    }
                    else if ( message.MessageRole == AuthorRole.Assistant )
                    {
                        _context.AddAssistantMessage( message.Message );
                    }
                }

                SessionId = sessionId;
                _historyNeedsSummary = messages.Sum( m => m.TokenCount ) >= _agentConfiguration.AutoSummarizeThreshold;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddMessageAsync( AuthorRole role, string message )
        {
            if ( role != AuthorRole.User && role != AuthorRole.Assistant )
            {
                throw new ArgumentOutOfRangeException( nameof( role ), "An invalid author role was specified." );
            }

            return AddMessageAsync( role, message, CountTokens( message ), 0 );
        }

        /// <inheritdoc/>
        async private Task AddMessageAsync( AuthorRole role, string message, int tokenCount, int consumedTokenCount )
        {
            if ( SessionId.HasValue )
            {
                if ( role == AuthorRole.User && _historyNeedsSummary )
                {
                    await SummarizeChatHistoryAsync();
                }

                using ( var rockContext = _rockContextFactory.CreateRockContext() )
                {
                    var historyService = new AIAgentSessionHistoryService( rockContext );

                    var history = new AIAgentSessionHistory
                    {
                        AIAgentSessionId = SessionId.Value,
                        MessageRole = role,
                        Message = message,
                        IsCurrentlyInContext = true,
                        MessageDateTime = RockDateTime.Now,
                        TokenCount = tokenCount,
                        ConsumedTokenCount = consumedTokenCount
                    };

                    historyService.Add( history );

                    var session = new AIAgentSessionService( rockContext ).Get( SessionId.Value );

                    session.LastMessageDateTime = RockDateTime.Now;

                    rockContext.SaveChanges();
                }
            }

            if ( role == AuthorRole.User )
            {
                _context.AddUserMessage( message );
            }
            else
            {
                _context.AddAssistantMessage( message );
            }
        }

        /// <inheritdoc/>
        public Task<ContextAnchor> AddAnchorAsync( IEntity entity )
        {
            var entityTypeId = entity.TypeId;

            var anchor = new AIAgentSessionAnchor
            {
                EntityTypeId = entityTypeId,
                EntityId = entity.Id,
                IsActive = true
            };

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                AIAgentSessionAnchorService.UpdateFromEntity( anchor, rockContext );

                if ( SessionId.HasValue )
                {
                    InactivateEntityAnchor( entityTypeId );

                    var anchorService = new AIAgentSessionAnchorService( rockContext );

                    anchor.AIAgentSessionId = SessionId.Value;
                    anchorService.Add( anchor );

                    rockContext.SaveChanges();
                }
            }

            _context.AddAnchor( anchor.EntityTypeId, anchor.PayloadJson );

            return Task.FromResult( anchor.PayloadJson.FromJsonOrNull<ContextAnchor>() );
        }

        /// <inheritdoc/>
        public Task RemoveAnchorAsync( int entityTypeId )
        {
            _context.RemoveAnchor( entityTypeId );

            if ( SessionId.HasValue )
            {
                InactivateEntityAnchor( entityTypeId );
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Inactivates any existing entity anchors for the specified entity type.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="EntityType"/> whose anchor should be marked inactive.</param>
        private void InactivateEntityAnchor( int entityTypeId )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var anchorService = new AIAgentSessionAnchorService( rockContext );
                var anchorsToInactivate = anchorService.Queryable()
                    .Where( a => a.AIAgentSessionId == SessionId.Value
                        && a.EntityTypeId == entityTypeId
                        && a.IsActive )
                    .ToList();

                anchorsToInactivate.ForEach( a => a.IsActive = false );

                rockContext.SaveChanges();
            }
        }

        /// <inheritdoc/>
        public Task AddSessionContextAsync( string key, SessionContext context )
        {
            _context.AddSessionContext( key, context );

            if ( SessionId.HasValue )
            {
                using ( var rockContext = _rockContextFactory.CreateRockContext() )
                {
                    var session = new AIAgentSessionService( rockContext ).Get( SessionId.Value );

                    session.SetSessionContext( key, context );
                    rockContext.SaveChanges();
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public string GetSessionContextContent( string key )
        {
            return _context.GetSessionContext( key )?.Content;
        }

        /// <inheritdoc/>
        public Task RemoveSessionContextAsync( string key )
        {
            _context.RemoveSessionContext( key );

            if ( SessionId.HasValue )
            {
                using ( var rockContext = _rockContextFactory.CreateRockContext() )
                {
                    var session = new AIAgentSessionService( rockContext ).Get( SessionId.Value );

                    session.SetSessionContext( key, null );
                    rockContext.SaveChanges();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds the system messages to the chat context. This is used to
        /// define the core personality and behavior of the assistant. It also
        /// provides some common context information, such as the current
        /// person.
        /// </summary>
        private void AddSystemMessages()
        {
            _context.AddSystemMessage( CoreSystemPrompt );
            _context.AddSystemMessage( $"Persona|{_agentConfiguration.Persona}" );

            if ( _requestContext?.CurrentPerson != null )
            {
                _context.AddSystemMessage( $"CurrentPerson|The current person is {_requestContext.CurrentPerson.FullName} (id: {_requestContext.CurrentPerson.Id}) he is the Discipleship Pastor." );
            }
        }

        /// <summary>
        /// Summarizes the chat history for the current session. This should
        /// only be called just before adding a user message, otherwise the
        /// results will be unexpected.
        /// </summary>
        async private Task SummarizeChatHistoryAsync()
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var messages = new AIAgentSessionHistoryService( rockContext ).Queryable()
                    .Where( s => s.AIAgentSessionId == SessionId.Value
                        && s.IsCurrentlyInContext )
                    .OrderBy( s => s.MessageDateTime )
                    .ThenBy( s => s.Id )
                    .ToList();

                if ( messages.Count == 0 )
                {
                    return;
                }

                var chatHistoryText = string.Join( "\n", messages.Select( m => $"{m.MessageRole}: {m.Message}" ) );
                var prompt = AutoSummarizePrompt + chatHistoryText;

                var chat = _kernel.GetRequiredService<IChatCompletionService>( _agentConfiguration.Role.ToString() );
                var result = await chat.GetChatMessageContentAsync(
                    new ChatHistory { new ChatMessageContent( Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User, prompt ) },
                    executionSettings: _agentConfiguration.Provider.GetChatCompletionPromptExecutionSettings(),
                    kernel: _kernel
                );

                var historyService = new AIAgentSessionHistoryService( rockContext );
                var usage = GetMetricUsageFromResult( result );

                messages.ForEach( m => m.IsCurrentlyInContext = false );

                var history = new AIAgentSessionHistory
                {
                    AIAgentSessionId = SessionId.Value,
                    MessageRole = AuthorRole.Assistant,
                    Message = result.Content,
                    IsCurrentlyInContext = true,
                    IsSummary = true,
                    MessageDateTime = RockDateTime.Now,
                    TokenCount = usage?.OutputTokenCount ?? CountTokens( result.Content ),
                    ConsumedTokenCount = usage?.TotalTokenCount ?? 0
                };

                historyService.Add( history );

                rockContext.SaveChanges();
                _historyNeedsSummary = false;
            }

            // Reload the session data.
            await LoadSessionAsync( SessionId.Value );
        }

        /// <inheritdoc/>
        public async Task<ChatMessageContent> GetChatMessageContentAsync()
        {
            var chat = _kernel.GetRequiredService<IChatCompletionService>( _agentConfiguration.Role.ToString() );

            var result = await chat.GetChatMessageContentAsync(
                _context.GetChatHistory(),
                executionSettings: _agentConfiguration.Provider.GetChatCompletionPromptExecutionSettings(),
                kernel: _kernel );

            var usage = GetMetricUsageFromResult( result );

            await AddMessageAsync( AuthorRole.Assistant, result.Content, usage?.OutputTokenCount ?? CountTokens( result.Content ), usage?.TotalTokenCount ?? 0 );

            return result;
        }

        //async internal Task<ChatMessageContent> GetChatMessageContentAsync( ChatHistory chatHistory, PromptExecutionSettings executionSettings )
        //{
        //    var chat = _kernel.GetRequiredService<IChatCompletionService>( _agentConfiguration.Role.ToString() );

        //    var result = await chat.GetChatMessageContentAsync( chatHistory, executionSettings, _kernel );

        //    return result;
        //}

        /// <inheritdoc/>
        public UsageMetric GetMetricUsageFromResult( ChatMessageContent result )
        {
            return _agentConfiguration.Provider.GetMetricUsageFromResult( result );
        }

        /// <summary>
        /// Creates the tokenizer for the GPT-4o-Mini model. This is called
        /// lazily and is used to count tokens when adding messages to the
        /// history if the provider didn't give us a valid count.
        /// </summary>
        /// <returns>A new instance of <see cref="TiktokenTokenizer"/>.</returns>
        private static TiktokenTokenizer CreateTokenizer()
        {
            try
            {
                return TiktokenTokenizer.CreateForModel( "gpt-4o-mini" );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Counts the tokens in the specified text.
        /// </summary>
        /// <param name="text">The text to be tokenized.</param>
        /// <returns>The number of tokens for <paramref name="text"/> or <c>0</c> if it could not be counted.</returns>
        private int CountTokens( string text )
        {
            try
            {
                return _tokenizer.Value?.CountTokens( text ) ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}
