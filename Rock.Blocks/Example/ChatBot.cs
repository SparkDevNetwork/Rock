using System.ComponentModel;
using System.Threading.Tasks;

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Enums.AI.Agent;

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

    [Rock.SystemGuid.EntityTypeGuid( "c08511a6-d9f5-40f4-a9cc-50cbe40a4ab8" )]
    [Rock.SystemGuid.BlockTypeGuid( "91a66c59-830e-49b5-a196-dcf93d0dde92" )]
    public class ChatBot : RockBlockType
    {
        private IChatAgent _agent;

        public ChatBot( IChatAgentBuilder agentBuilder )
        {
            _agent = agentBuilder.Build( 0 );
        }

        [BlockAction]
        async public Task<BlockActionResult> SendMessage( string message )
        {
            _agent.Context.ChatHistory.AddUserMessage( message );

            var result = await _agent.GetChatMessageContentAsync( ModelServiceRole.Default );

            return ActionOk( result.Content );
        }
    }
}
