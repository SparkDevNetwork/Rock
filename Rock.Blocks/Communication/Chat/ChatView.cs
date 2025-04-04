using Rock.Communication.Chat;
using Rock.Model;
using Rock.SystemGuid;

using System.ComponentModel;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.ViewModels.Blocks.Communication.Chat.ChatView;

namespace Rock.Blocks.Communication.Chat
{
    /// <summary>
    /// Block for displaying a chat interface using Rock's StreamChat integration.
    /// </summary>
    [DisplayName( "Chat View" )]
    [Category( "Communication" )]
    [Description( "Displays a chat interface utilizing the Rock StreamChat integration." )]
    [SupportedSiteTypes( SiteType.Mobile, SiteType.Web )]
    [IconCssClass( "fa fa-comments" )]

    #region Block Attributes

    [BooleanField( "Filter Shared Channels by Campus",
        Description = "Only show channels that match the individual's campus or have no campus set.",
        DefaultBooleanValue = false,
        Key = AttributeKey.FilterSharedChannelsByCampus,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "B3D6F875-1589-4543-9E76-5C41201B465B" )]
    [Rock.SystemGuid.BlockTypeGuid( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" )]
    public class ChatView : RockBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the filter shared channels by campus setting.
        /// </summary>
        protected bool FilterSharedChannelsByCampus => GetAttributeValue( AttributeKey.FilterSharedChannelsByCampus ).AsBoolean();

        #endregion

        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The filter shared channels by campus key.
            /// </summary>
            public const string FilterSharedChannelsByCampus = "FilterSharedChannelsByCampus";
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                FilterSharedChannelsByCampus = FilterSharedChannelsByCampus
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Retrieves the current individual's chat settings, including personalized or default values.
        /// </summary>
        /// <returns>A result containing individual-specific chat configuration flags.</returns>
        [BlockAction]
        public BlockActionResult GetChatPersonSettings()
        {
            var person = RequestContext.CurrentPerson;

            if ( person == null )
            {
                return ActionUnauthorized( "You must be logged in to view your chat settings." );
            }

            using ( var chatHelper = new ChatHelper( RockContext ) )
            {
                if ( chatHelper.IsPersonBanned( person.Id ) )
                {
                    return ActionForbidden( "You are banned from using chat." );
                }
            }

            // Pull default values from chat configuration.
            var chatConfig = ChatHelper.GetChatConfiguration();
            var isOpenDirectMessageAllowedDefaultValue = chatConfig.IsOpenDirectMessagingAllowed;
            var isProfilePublicDefaultValue = chatConfig.AreChatProfilesVisible;

            return ActionOk( new
            {
                IsChatOpenDirectMessageAllowed = person.IsChatOpenDirectMessageAllowed ?? isOpenDirectMessageAllowedDefaultValue,
                IsChatProfilePublic = person.IsChatProfilePublic ?? isProfilePublicDefaultValue
            } );
        }

        /// <summary>
        /// Updates the individual's chat preferences.
        /// </summary>
        /// <param name="options">New chat preferences from the client.</param>
        /// <returns>An action result indicating success or failure.</returns>
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
        /// Asynchronously retrieves the authentication information needed for chat usage.
        /// </summary>
        /// <returns>A result containing the chat token and user key for StreamChat authentication.</returns>
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