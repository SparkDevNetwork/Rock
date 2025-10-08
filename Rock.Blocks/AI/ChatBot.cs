using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock;
using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Enums.AI.Agent;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Allows the user to try out the chat agent.
    /// </summary>

    [DisplayName( "Chat Bot" )]
    [Category( "AI" )]
    [Description( "Allows the user to try out the chat agent." )]
    [IconCssClass( "ti ti-robot" )]
    [SupportedSiteTypes( SiteType.Web )]
    [ConfigurationChangedReload( BlockReloadMode.Block )]

    [CustomDropdownListField( "Agent",
        Description = "The AI agent to use for this chat bot.",
        IsRequired = true,
        Key = AttributeKey.Agent,
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [AIAgent] ORDER BY [Name]",
        Order = 0 )]

    [SystemGuid.EntityTypeGuid( "c08511a6-d9f5-40f4-a9cc-50cbe40a4ab8" )]
    [SystemGuid.BlockTypeGuid( "91a66c59-830e-49b5-a196-dcf93d0dde92" )]
    public class ChatBot : RockBlockType
    {
        #region Fields

        /// <summary>
        /// The agent builder used to construct chat agent instances for this block.
        /// </summary>
        private readonly IChatAgentBuilder _agentBuilder;

        #endregion

        #region Keys

        /// <summary>
        /// Keys for block attributes used in this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string Agent = "Agent";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatBot"/> class with the specified agent builder.
        /// </summary>
        /// <param name="serviceProvider">The service provider to get services from.</param>
        public ChatBot( IServiceProvider serviceProvider )
        {
            _agentBuilder = serviceProvider.GetRequiredService<IChatAgentBuilder>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async override Task<object> GetObsidianBlockInitializationAsync()
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No agent has been configured."
                };
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "You are not authorized to access this agent."
                };
            }

            // Find the recent sessions.
            var sessions = GetRecentSessions( agentCache.Id );

            var sessionId = sessions.LastOrDefault()?.Id;

            // If no session was found, create a new session.
            if ( !sessionId.HasValue )
            {
                var agent = _agentBuilder.Build( agentCache.Id );

                await agent.StartNewSessionAsync( null, null );

                sessions = GetRecentSessions( agentCache.Id );
                sessionId = sessions.Last().Id;
            }

            var messages = GetSessionMessages( sessionId.Value );
            var anchors = GetSessionAnchors( sessionId.Value );

            return new Dictionary<string, object>
            {
                ["sessionId"] = sessionId.Value,
                ["sessions"] = sessions,
                ["messages"] = messages,
                ["anchors"] = anchors,
            };
        }

        /// <summary>
        /// Retrieves a list of recent chat sessions for the current person and specified agent.
        /// </summary>
        /// <param name="agentId">The unique identifier of the agent.</param>
        /// <returns>A list of recent chat sessions.</returns>
        private List<ChatSessionBag> GetRecentSessions( int agentId )
        {
            return new AIAgentSessionService( RockContext )
                .Queryable()
                .Where( s => s.PersonAlias.PersonId == RequestContext.CurrentPerson.Id
                    && s.AIAgentId == agentId
                    && !s.RelatedEntityTypeId.HasValue
                    && !s.RelatedEntityId.HasValue )
                .OrderBy( s => s.LastMessageDateTime )
                .Select( s => new
                {
                    s.Id,
                    s.LastMessageDateTime,
                    s.Name
                } )
                .ToList()
                .Select( s => new ChatSessionBag
                {
                    Id = s.Id,
                    LastMessageDateTime = s.LastMessageDateTime.ToRockDateTimeOffset(),
                    Name = s.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Retrieves the list of chat messages for a given chat session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session.</param>
        /// <returns>A list of chat messages in the session.</returns>
        private List<ChatMessageBag> GetSessionMessages( int sessionId )
        {
            return new AIAgentSessionHistoryService( RockContext )
                .Queryable()
                .Where( h => h.AIAgentSessionId == sessionId
                    && h.MessageRole != AuthorRole.Tool
                    && !h.IsSummary )
                .OrderBy( h => h.MessageDateTime )
                .ThenBy( h => h.Id )
                .Select( h => new ChatMessageBag
                {
                    Role = h.MessageRole,
                    Message = h.Message,
                    TokenCount = h.TokenCount,
                    ConsumedTokenCount = h.ConsumedTokenCount
                } )
                .ToList();
        }

        /// <summary>
        /// Retrieves the list of active anchors for a given chat session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session.</param>
        /// <returns>A list of active anchors in the session.</returns>
        private List<ChatAnchorBag> GetSessionAnchors( int sessionId )
        {
            return new AIAgentSessionAnchorService( RockContext )
                .Queryable()
                .Where( s => s.AIAgentSessionId == sessionId
                    && s.IsActive )
                .Select( s => new
                {
                    s.Id,
                    s.EntityTypeId,
                    s.Name
                } )
                .ToList()
                .Select( s => new ChatAnchorBag
                {
                    Id = s.Id,
                    EntityTypeId = s.EntityTypeId,
                    EntityTypeName = EntityTypeCache.Get( s.EntityTypeId, RockContext )?.FriendlyName ?? string.Empty,
                    Name = s.Name
                } )
                .ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Sends a user message to the chat agent for the specified session and returns the assistant's response.
        /// </summary>
        /// <param name="message">The message from the user.</param>
        /// <param name="sessionId">The chat session identifier.</param>
        /// <returns>A block action result containing the assistant's response message and token usage metrics.</returns>
        [BlockAction]
        public async Task<BlockActionResult> SendMessage( SendMessageRequestBag request )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var startTimestamp = RockDateTime.Now;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var agent = _agentBuilder.Build( agentCache.Id, new ChatAgentOptions
            {
                IsDebugEnabled = request.IsDebugEnabled,
                IsSecurityEnabled = true
            } );

            await agent.LoadSessionAsync( request.SessionId );
            await agent.AddMessageAsync( AuthorRole.User, request.Message );
            var internalLogs = new List<ChatDebugLog>();

            if ( PageParameter( "test" ) == "true" )
            {
                var responseStream = agent.GetStreamingChatMessageResponsesAsync();

                await foreach ( var response in responseStream )
                {
                    internalLogs.Add( new ChatDebugLog( "Internal", Microsoft.Extensions.Logging.LogLevel.Trace, $"Recieved content chunk '{response.Content}'." ) );
                }

                var logs = internalLogs.Select( l => $"[@{( long )( l.Timestamp - startTimestamp ).TotalMilliseconds}ms] {l.Message}" ).ToList();
                logs.Insert( 0, $"Test completed in {sw.Elapsed.TotalMilliseconds}ms." );

                return ActionContent( System.Net.HttpStatusCode.BadRequest, logs );
            }

            async IAsyncEnumerable<SendMessageResponseBag> ResponseFactory()
            {
                var responseStream = agent.GetStreamingChatMessageResponsesAsync();

                await foreach ( var response in responseStream )
                {
                    internalLogs.Add( new ChatDebugLog( "Internal", Microsoft.Extensions.Logging.LogLevel.Trace, $"Recieved content chunk '{response.Content}'." ) );

                    if ( response.Items != null && response.Items.Any() && response.Items[0] is StreamingToolCallContent sfcc )
                    {
                        if ( sfcc.Description.IsNotNullOrWhiteSpace() )
                        {
                            yield return new SendMessageResponseBag
                            {
                                Tool = sfcc.Description
                            };
                        }

                        continue;
                    }

                    if ( string.IsNullOrEmpty( response.Content ) && response.Usage == null && response.Debug == null )
                    {
                        continue;
                    }

                    var messageBag = new ChatMessageBag
                    {
                        Role = AuthorRole.Assistant,
                        Message = response.Content,
                        TokenCount = response.Usage?.OutputTokenCount ?? 0,
                        ConsumedTokenCount = response.Usage?.TotalTokenCount ?? 0,
                        Duration = sw.ElapsedMilliseconds
                    };

                    var responseBag = new SendMessageResponseBag
                    {
                        Message = messageBag
                    };

                    if ( request.IsDebugEnabled && response.Debug != null )
                    {
                        responseBag.Logs = response.Debug
                            ?.Logs
                            ?.Select( l => new ChatLogBag
                            {
                                Category = l.Category,
                                LogLevel = ( int ) l.LogLevel,
                                LogLevelName = l.LogLevel.ToString(),
                                Message = l.Message,
                                Timestamp = ( long ) ( l.Timestamp - startTimestamp ).TotalMilliseconds
                            } ).ToList()
                            ?? new List<ChatLogBag>();

                        responseBag.Logs.AddRange( internalLogs.Select( l => new ChatLogBag
                        {
                            Category = l.Category,
                            LogLevel = ( int ) l.LogLevel,
                            LogLevelName = l.LogLevel.ToString(),
                            Message = l.Message,
                            Timestamp = ( long ) ( l.Timestamp - startTimestamp ).TotalMilliseconds
                        } ) );
                    }

                    yield return responseBag;
                }
            };

            return new ServerSentEventsBlockActionResult<SendMessageResponseBag>( ResponseFactory() );
        }

        /// <summary>
        /// Starts a new chat session with the configured agent.
        /// </summary>
        /// <returns>A block action result containing the new session details.</returns>
        [BlockAction]
        public async Task<BlockActionResult> StartNewSession()
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var agent = _agentBuilder.Build( agentCache.Id, new ChatAgentOptions
            {
                IsSecurityEnabled = true
            } );

            // Start a new session.
            await agent.StartNewSessionAsync( null, null );

            return ActionOk( new ChatSessionBag
            {
                Id = agent.SessionId.Value,
                LastMessageDateTime = RockDateTime.Now.ToRockDateTimeOffset(),
            } );
        }

        /// <summary>
        /// Loads a specific chat session and returns its messages and anchors if the session belongs to the current person.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to load.</param>
        /// <returns>A block action result containing session messages and anchors, or an error if not found.</returns>
        [BlockAction]
        public BlockActionResult LoadSession( int sessionId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var foundSessionId = new AIAgentSessionService( RockContext )
                .Queryable()
                .Where( s => s.Id == sessionId
                    && s.AIAgentId == agentCache.Id
                    && s.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                .Select( s => s.Id )
                .FirstOrDefault();

            if ( foundSessionId == 0 )
            {
                return ActionBadRequest( "Invalid session." );
            }

            var messages = GetSessionMessages( sessionId );
            var anchors = GetSessionAnchors( sessionId );

            return ActionOk( new Dictionary<string, object>
            {
                ["messages"] = messages,
                ["anchors"] = anchors
            } );
        }

        /// <summary>
        /// Clears a chat session of all chat history.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to clear.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult ClearSession( int sessionId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var sessionService = new AIAgentSessionService( RockContext );
            var session = sessionService
                .Queryable()
                .Where( s => s.PersonAlias.PersonId == RequestContext.CurrentPerson.Id
                    && s.AIAgentId == agentCache.Id
                    && s.Id == sessionId )
                .FirstOrDefault();

            if ( session == null )
            {
                return ActionBadRequest( "Session not found." );
            }

            var sessionHistoryService = new AIAgentSessionHistoryService( RockContext );
            var messages = sessionHistoryService
                .Queryable()
                .Where( h => h.AIAgentSessionId == sessionId )
                .ToList();

            sessionHistoryService.DeleteRange( messages );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes a chat session if it belongs to the current person.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to delete.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult DeleteSession( int sessionId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var sessionService = new AIAgentSessionService( RockContext );
            var session = sessionService
                .Queryable()
                .Where( s => s.PersonAlias.PersonId == RequestContext.CurrentPerson.Id
                    && s.AIAgentId == agentCache.Id
                    && s.Id == sessionId )
                .FirstOrDefault();

            if ( session == null )
            {
                return ActionBadRequest( "Session not found." );
            }

            sessionService.Delete( session );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Creates a context anchor for the specified entity within a chat session.
        /// </summary>
        /// <param name="sessionId">The chat session identifier.</param>
        /// <param name="entityTypeName">The name of the entity type.</param>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>A block action result containing the updated session anchors.</returns>
        [BlockAction]
        public async Task<BlockActionResult> CreateAnchor( int sessionId, string entityTypeName, int entityId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var entityTypeCache = EntityTypeCache.Get( "Rock.Model." + entityTypeName, false, RockContext );

            if ( entityTypeCache == null )
            {
                return ActionBadRequest( "Unknown entity type." );
            }

            var entity = Reflection.GetIEntityForEntityType( entityTypeCache.Id, entityId, RockContext );

            if ( entity == null )
            {
                return ActionBadRequest( "Entity not found." );
            }

            var agent = _agentBuilder.Build( agentCache.Id, new ChatAgentOptions
            {
                IsSecurityEnabled = true
            } );

            await agent.LoadSessionAsync( sessionId );
            await agent.AddAnchorAsync( entity );

            return ActionOk( GetSessionAnchors( sessionId ) );
        }

        /// <summary>
        /// Deletes an existing context anchor from a chat session.
        /// </summary>
        /// <param name="sessionId">The chat session identifier.</param>
        /// <param name="entityTypeId">The identifier of the entity type whose anchor should be removed.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction]
        public async Task<BlockActionResult> DeleteAnchor( int sessionId, int entityTypeId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            if ( !agentCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this agent." );
            }

            var agent = _agentBuilder.Build( agentCache.Id, new ChatAgentOptions
            {
                IsSecurityEnabled = true
            } );

            await agent.LoadSessionAsync( sessionId );
            await agent.RemoveAnchorAsync( entityTypeId );

            return ActionOk();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents summary information about a single chat session for display in the chat UI.
        /// </summary>
        private class ChatSessionBag
        {
            /// <summary>
            /// Gets or sets the unique identifier for the chat session.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date and time of the last message in the session.
            /// </summary>
            public DateTimeOffset LastMessageDateTime { get; set; }

            /// <summary>
            /// Gets or sets the display name for the session.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents a single chat message in a session, including its role, content, and token usage.
        /// </summary>
        private class ChatMessageBag
        {
            /// <summary>
            /// The number of milliseconds it took to process this message in the AI agent.
            /// </summary>
            public long Duration { get; set; }

            /// <summary>
            /// Gets or sets the role of the message author (e.g., User or Assistant).
            /// </summary>
            public AuthorRole Role { get; set; }

            /// <summary>
            /// Gets or sets the message content.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the token count used by this message.
            /// </summary>
            public int TokenCount { get; set; }

            /// <summary>
            /// Gets or sets the cumulative token count consumed up to and including this message.
            /// </summary>
            public int ConsumedTokenCount { get; set; }
        }

        private class ChatLogBag
        {
            public string Category { get; set; }

            public int LogLevel { get; set; }

            public string LogLevelName { get; set; }

            public string Message { get; set; }

            public long Timestamp { get; set; }
        }

        /// <summary>
        /// Represents a request to send a message to the chat agent.
        /// </summary>
        public class SendMessageRequestBag
        {
            /// <summary>
            /// Gets or sets the message to send to the chat agent.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the chat session.
            /// </summary>
            public int SessionId { get; set; }

            /// <summary>
            /// Requests that additional debug information be included in the response.
            /// </summary>
            public bool IsDebugEnabled { get; set; }
        }

        /// <summary>
        /// Represents the response from sending a message to the chat agent.
        /// </summary>
        private class SendMessageResponseBag
        {
            /// <summary>
            /// The response message from the chat agent.
            /// </summary>
            public ChatMessageBag Message { get; set; }

            /// <summary>
            /// The tool that was called, if any.
            /// </summary>
            public string Tool { get; set; }

            /// <summary>
            /// The debug logs that were collected during processing.
            /// </summary>
            public List<ChatLogBag> Logs { get; set; }
        }

        /// <summary>
        /// Represents a context anchor attached to a chat session, linking it to a specific entity.
        /// </summary>
        private class ChatAnchorBag
        {
            /// <summary>
            /// Gets or sets the unique identifier for the anchor.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the entity type identifier for the anchor.
            /// </summary>
            public int EntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity type for the anchor.
            /// </summary>
            public string EntityTypeName { get; set; }

            /// <summary>
            /// Gets or sets the display name of the anchor.
            /// </summary>
            public string Name { get; set; }
        }

        #endregion
    }
}
