using Rock.Communication.Chat;
using Rock.Model;
using Rock.SystemGuid;

using System.ComponentModel;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.ViewModels.Blocks.Communication.Chat.ChatView;

namespace Rock.Blocks.Communication.Chat
{
    [DisplayName( "Chat View" )]
    [Category( "Communication" )]
    [Description( "Displays a chat interface utilizing the Rock StreamChat integration." )]
    [SupportedSiteTypes( SiteType.Mobile, SiteType.Web )]
    [IconCssClass( "fa fa-comments" )]

    [Rock.SystemGuid.EntityTypeGuid( "B3D6F875-1589-4543-9E76-5C41201B465B" )]
    [Rock.SystemGuid.BlockTypeGuid( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" )]
    public class ChatView : RockBlockType
    {
        #region Block Actions

        [BlockAction]
        public BlockActionResult GetChatPersonSettings()
        {
            var person = RequestContext.CurrentPerson;

            if ( person == null )
            {
                return ActionUnauthorized( "You must be logged in to view your chat settings." );
            }

            // These settings are a little untraditional -- the properties on the Person
            // are nullable. If they are null, Stream users are synced with the default.
            // If they are not null, the user has overridden the default.
            // The mobile shell only updates these properties on user interaction,
            // so we need to send down the default values in case the user has not
            // interacted with the chat settings yet (or is just riding the 'default' wave).
            var chatConfig = ChatHelper.GetChatConfiguration();
            var isOpenDirectMessageAllowedDefaultValue = chatConfig.IsOpenDirectMessagingAllowed;
            var isProfilePublicDefaultValue = chatConfig.AreChatProfilesVisible;

            return ActionOk( new
            {
                IsChatOpenDirectMessageAllowed = person.IsChatOpenDirectMessageAllowed ?? isOpenDirectMessageAllowedDefaultValue,
                IsChatProfilePublic = person.IsChatProfilePublic ?? isProfilePublicDefaultValue
            } );
        }

        [BlockAction]
        public BlockActionResult UpdateChatPersonSettings( ChatPersonSettingsBag options )
        {
            var person = RequestContext.CurrentPerson;

            if ( person == null )
            {
                return ActionUnauthorized( "You must be logged in to update your chat settings." );
            }

            person.IsChatOpenDirectMessageAllowed = options.IsChatOpenDirectMessageAllowed;
            person.IsChatProfilePublic = options.IsChatProfilePublic;
            RockContext.SaveChanges();

            return ActionOk();
        }
        
        /// <summary>
        /// Gets the chat data.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public async Task<BlockActionResult> GetChatData()
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized( "You must be logged in to view chat." );
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
