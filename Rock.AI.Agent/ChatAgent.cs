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
        private readonly AgentConfiguration _agentConfiguration;

        private readonly Kernel _kernel;

        private readonly IRockContextFactory _rockContextFactory;

        private readonly RockRequestContext _requestContext;

        private readonly Lazy<TiktokenTokenizer> _tokenizer = new Lazy<TiktokenTokenizer>( CreateTokenizer );

        public int? SessionId { get; private set; }

        public AgentRequestContext Context { get; }

        public ChatAgent( Kernel kernel, AgentConfiguration agentConfiguration, IRockContextFactory rockContextFactory, IRockRequestContextAccessor rockRequestContextAccessor )
        {
            _kernel = kernel;
            _agentConfiguration = agentConfiguration;
            _rockContextFactory = rockContextFactory;
            _requestContext = rockRequestContextAccessor.RockRequestContext;

            Context = kernel.Services.GetRequiredService<AgentRequestContext>();
        }

        public void StartNewSession( int? entityTypeId, int? entityId )
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

                var chatHistory = CreateCommonHistory();

                Context.InternalChatHistory = chatHistory;
                SessionId = session.Id;
            }
        }

        public void LoadSession( int sessionId )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var messages = new AIAgentSessionHistoryService( rockContext ).Queryable()
                    .Where( s => s.AIAgentSessionId == sessionId
                        && s.IsCurrentlyInContext )
                    .OrderBy( s => s.MessageDateTime )
                    .Select( s => new
                    {
                        s.MessageRole,
                        s.Message
                    } )
                    .ToList();

                var anchors = new AIAgentSessionAnchorService( rockContext ).Queryable()
                    .Where( a => a.AIAgentSessionId == sessionId
                        && a.IsActive )
                    .OrderByDescending( a => a.AddedDateTime )
                    .Select( a => new
                    {
                        a.EntityTypeId,
                        a.EntityId,
                        a.PayloadJson
                    } )
                    .ToList();

                var chatHistory = CreateCommonHistory();

                var anchorEntities = new List<(int EntityTypeId, int EntityId)>();
                foreach ( var anchor in anchors )
                {
                    // Skip duplicates.
                    if ( anchor.EntityTypeId.HasValue && anchor.EntityId.HasValue )
                    {
                        if ( anchorEntities.Contains( (anchor.EntityTypeId.Value, anchor.EntityId.Value) ) )
                        {
                            continue;
                        }

                        anchorEntities.Add( (anchor.EntityTypeId.Value, anchor.EntityId.Value) );
                    }

                    chatHistory.AddSystemMessage( $"ContextAnchor|{anchor.PayloadJson}" );
                }

                foreach ( var message in messages )
                {
                    if ( message.MessageRole == AuthorRole.User )
                    {
                        chatHistory.AddUserMessage( message.Message );
                    }
                    else if ( message.MessageRole == AuthorRole.Assistant )
                    {
                        chatHistory.AddAssistantMessage( message.Message );
                    }
                }

                Context.InternalChatHistory = chatHistory;
                SessionId = sessionId;
            }
        }

        public void AddMessage( AuthorRole role, string message )
        {
            if ( role != AuthorRole.User && role != AuthorRole.Assistant )
            {
                throw new ArgumentOutOfRangeException( nameof( role ), "An invalid author role was specified." );
            }

            AddMessage( role, message, CountTokens( message ), 0 );
        }

        private void AddMessage( AuthorRole role, string message, int tokenCount, int consumedTokenCount )
        {
            if ( SessionId.HasValue )
            {
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

            var chatRole = role == AuthorRole.User
                ? Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User
                : Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant;

            Context.InternalChatHistory.AddMessage( chatRole, message );
        }

        private ChatHistory CreateCommonHistory()
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage( AgentRequestContext._coreSystemPrompt );
            chatHistory.AddSystemMessage( $"Persona|{_agentConfiguration.Persona}" );

            if ( _requestContext?.CurrentPerson != null )
            {
                chatHistory.AddSystemMessage( $"CurrentPerson|The current person is {_requestContext.CurrentPerson.FullName} (id: {_requestContext.CurrentPerson.Id}) he is the Discipleship Pastor." );
            }

            return chatHistory;
        }

        async public Task<ChatMessageContent> GetChatMessageContentAsync()
        {
            var chat = _kernel.GetRequiredService<IChatCompletionService>( _agentConfiguration.Role.ToString() );

            var result = await chat.GetChatMessageContentAsync(
                Context.InternalChatHistory,
                executionSettings: _agentConfiguration.Provider.GetChatCompletionPromptExecutionSettings(),
                kernel: _kernel );

            var usage = GetMetricUsageFromResult( result );

            AddMessage( AuthorRole.Assistant, result.Content, usage?.OutputTokenCount ?? 0, usage?.TotalTokenCount ?? 0 );

            return result;
        }

        public UsageMetric GetMetricUsageFromResult( ChatMessageContent result )
        {
            return _agentConfiguration.Provider.GetMetricUsageFromResult( result );
        }

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
    }
}
