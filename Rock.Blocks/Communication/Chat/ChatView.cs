using Rock.Communication.Chat;
using Rock.Model;
using Rock.SystemGuid;

using System.ComponentModel;
using System.Threading.Tasks;
using Rock.Attribute;

namespace Rock.Blocks.Communication.Chat
{
    [DisplayName( "Chat View" )]
    [Category( "Communication" )]
    [Description( "Displays a chat interface utilizing the Rock StreamChat integration." )]
    [SupportedSiteTypes( SiteType.Mobile | SiteType.Web )]
    [IconCssClass( "fa fa-comments" )]

    [Rock.SystemGuid.EntityTypeGuid( "B3D6F875-1589-4543-9E76-5C41201B465B" )]
    [Rock.SystemGuid.BlockTypeGuid( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" )]
    public class ChatView : RockBlockType
    {
        #region Block Actions

        /// <summary>
        /// Gets the chat data.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public async Task<BlockActionResult> GetChatData()
        {
            if( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized("You must be logged in to view chat.");
            }

            var person = RequestContext.CurrentPerson;

            using ( var chatHelper = new ChatHelper() )
            {
                var chatUserAuth = await chatHelper.GetChatUserAuthenticationAsync( person.Id, true );

                return ActionOk( new
                {
                    Token = chatUserAuth.Token,
                    UserId = chatUserAuth.ChatUserKey
                } );
            }
        }
        
        #endregion
    }
}
