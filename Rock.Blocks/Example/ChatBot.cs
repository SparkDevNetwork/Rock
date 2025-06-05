using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Enums.Core.AI.Agent;
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

        [BlockAction]
        async public Task<BlockActionResult> SendMessage( string message, List<ChatMessage> history )
        {
            var agentGuid = GetAttributeValue( AttributeKey.Agent ).AsGuidOrNull();
            var agentCache = agentGuid.HasValue ? AIAgentCache.Get( agentGuid.Value, RockContext ) : null;

            if ( agentCache == null )
            {
                return ActionBadRequest( "No agent has been configured." );
            }

            var agent = _agentBuilder.Build( agentCache.Id );

            foreach ( var historyItem in history )
            {
                if ( historyItem.IsUserMessage )
                {
                    agent.Context.ChatHistory.AddUserMessage( historyItem.Content );
                }
                else
                {
                    agent.Context.ChatHistory.AddAssistantMessage( historyItem.Content );
                }
            }

            agent.Context.ChatHistory.AddUserMessage( message );

            var result = await agent.GetChatMessageContentAsync( ModelServiceRole.Default );

            return ActionOk( result.Content );
        }

        public class ChatMessage
        {
            public string Content { get; set; }

            public bool IsUserMessage { get; set; }
        }
    }
}
