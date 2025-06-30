using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Enums.Core.AI.Agent;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Allows the user to try out the chat agent.
    /// </summary>

    [DisplayName( "Chat Bot" )]
    [Category( "Obsidian > Example" )]
    [Description( "Allows the user to try out the chat agent." )]
    [IconCssClass( "ti ti-robot" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CustomDropdownListField( "Agent",
        Description = "The AI agent to use for this chat bot.",
        IsRequired = true,
        Key = AttributeKey.Agent,
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [AIAgent] ORDER BY [Name]",
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "c08511a6-d9f5-40f4-a9cc-50cbe40a4ab8" )]
    [Rock.SystemGuid.BlockTypeGuid( "91a66c59-830e-49b5-a196-dcf93d0dde92" )]
    public class ChatBot : RockBlockType
    {
        private readonly IChatAgentBuilder _agentBuilder;

        private static class AttributeKey
        {
            public const string Agent = "Agent";
        }

        public ChatBot( IChatAgentBuilder agentBuilder )
        {
            _agentBuilder = agentBuilder;
        }

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

            // Find the recent sessions.
            var sessions = GetRecentSessions();

            var sessionId = sessions.LastOrDefault()?.Id;

            // If no session was found, create a new session.
            if ( !sessionId.HasValue )
            {
                var agent = _agentBuilder.Build( agentCache.Id );

                await agent.StartNewSessionAsync( null, null );

                sessions = GetRecentSessions();
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

        private List<ChatSessionBag> GetRecentSessions()
        {
            return new AIAgentSessionService( RockContext )
                .Queryable()
                .Where( s => s.PersonAlias.PersonId == RequestContext.CurrentPerson.Id
                    && !s.RelatedEntityTypeId.HasValue
                    && !s.RelatedEntityId.HasValue )
                .OrderBy( s => s.LastMessageDateTime )
                .Take( 10 )
                .Select( s => new
                {
                    s.Id,
                    s.LastMessageDateTime
                } )
                .ToList()
                .Select( s => new ChatSessionBag
                {
                    Id = s.Id,
                    LastMessageDateTime = s.LastMessageDateTime.ToRockDateTimeOffset(),
                    Name = $"#{s.Id}"
                } )
                .ToList();
        }

        private List<ChatMessageBag> GetSessionMessages( int sessionId )
        {
            return new AIAgentSessionHistoryService( RockContext )
                .Queryable()
                .Where( h => h.AIAgentSessionId == sessionId
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

        [BlockAction]
        public async Task<BlockActionResult> SendMessage( string message, int sessionId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            var agent = _agentBuilder.Build( agentCache.Id );

            await agent.LoadSessionAsync( sessionId );
            await agent.AddMessageAsync( AuthorRole.User, message );

            var result = await agent.GetChatMessageContentAsync();
            var usage = agent.GetMetricUsageFromResult( result );

            return ActionOk( new ChatMessageBag
            {
                Role = AuthorRole.Assistant,
                Message = result.Content,
                TokenCount = usage.OutputTokenCount,
                ConsumedTokenCount = usage.TotalTokenCount
            } );
        }

        [BlockAction]
        public async Task<BlockActionResult> StartNewSession()
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            var agent = _agentBuilder.Build( agentCache.Id );

            // Start a new session.
            await agent.StartNewSessionAsync( null, null );

            return ActionOk( new ChatSessionBag
            {
                Id = agent.SessionId.Value,
                LastMessageDateTime = RockDateTime.Now.ToRockDateTimeOffset(),
                Name = $"#{agent.SessionId}"
            } );
        }

        [BlockAction]
        public BlockActionResult LoadSession( int sessionId )
        {
            var foundSessionId = new AIAgentSessionService( RockContext )
                .Queryable()
                .Where( s => s.Id == sessionId
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

        [BlockAction]
        public async Task<BlockActionResult> CreateAnchor( int sessionId, string entityTypeName, int entityId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
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

            var agent = _agentBuilder.Build( agentCache.Id );

            await agent.LoadSessionAsync( sessionId );
            await agent.AddAnchorAsync( entity );

            return ActionOk( GetSessionAnchors( sessionId ) );
        }

        [BlockAction]
        public async Task<BlockActionResult> DeleteAnchor( int sessionId, int entityTypeId )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            var agent = _agentBuilder.Build( agentCache.Id );

            await agent.LoadSessionAsync( sessionId );
            await agent.RemoveAnchorAsync( entityTypeId );

            return ActionOk();
        }

        private class ChatSessionBag
        {
            public int Id { get; set; }

            public DateTimeOffset LastMessageDateTime { get; set; }

            public string Name { get; set; }
        }

        private class ChatMessageBag
        {
            public AuthorRole Role { get; set; }

            public string Message { get; set; }

            public int TokenCount { get; set; }

            public int ConsumedTokenCount { get; set; }
        }

        private class ChatAnchorBag
        {
            public int Id { get; set; }

            public int EntityTypeId { get; set; }

            public string EntityTypeName { get; set; }

            public string Name { get; set; }
        }
    }
}
