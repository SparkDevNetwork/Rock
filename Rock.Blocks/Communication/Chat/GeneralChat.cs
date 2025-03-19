using Rock.Communication.Chat;
using Rock.Model;
using Rock.SystemGuid;

using System.ComponentModel;
using System.Threading.Tasks;
using Rock.Web.Cache;
using Rock.Attribute;

namespace Rock.Blocks.Communication.Chat
{
    [DisplayName( "Chat" )]
    [Category( "Communication" )]
    [Description( "Integrate StreamChat into your Rock Mobile application." )]
    [SupportedSiteTypes( SiteType.Mobile )]
    [IconCssClass( "fa-comments" )]

    [Rock.SystemGuid.EntityTypeGuid( "B3D6F875-1589-4543-9E76-5C41201B465B" )]
    [Rock.SystemGuid.BlockTypeGuid( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" )]
    public class GeneralChat : RockBlockType
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

            var sharedChannelGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL.AsGuid() );
            var directMessagingChannelGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() );

            if( !sharedChannelGroupTypeId.HasValue || !directMessagingChannelGroupTypeId.HasValue )
            {
                return ActionBadRequest( "Chat group types are not configured." );
            }

            using ( var chatHelper = new ChatHelper() )
            {
                var chatUserAuth = await chatHelper.GetChatUserAuthenticationAsync( person.Id, true );
                var sharedChannelGroupStreamKey = ChatHelper.GetChatChannelTypeKey( sharedChannelGroupTypeId.Value );
                var directMessagingChannelStreamKey = ChatHelper.GetChatChannelTypeKey( directMessagingChannelGroupTypeId.Value );

                return ActionOk( new
                {
                    Token = chatUserAuth.Token,
                    UserId = chatUserAuth.ChatUserKey,

                    // TODO: Should probably be passing these through the update package
                    // instead of here.
                    SharedChannelStreamKey = sharedChannelGroupStreamKey,
                    DirectMessagingChannelStreamKey = directMessagingChannelStreamKey
                } );
            }
        }
        
        #endregion
    }
}
