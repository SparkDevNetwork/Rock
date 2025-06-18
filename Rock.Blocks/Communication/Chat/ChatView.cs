using Rock.Communication.Chat;
using Rock.Model;
using Rock.SystemGuid;

using System.ComponentModel;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.ViewModels.Blocks.Communication.Chat.ChatView;
using Rock.Web.Cache;
using System;
using Rock.ViewModels.Controls;
using Rock.Enums.Communication.Chat;

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

    #region Shared

    [EnumField( "Chat Style",
        Description = "Choose how chat conversations are displayed. 'Conversational' offers a simple, text-message feel that's great for direct messages and small groups. 'Community' provides a more structured layout, ideal for group discussions and larger conversations.",
        DefaultEnumValue = ( int ) ChatViewStyle.Conversational,
        EnumSourceType = typeof( ChatViewStyle ),
        Key = AttributeKey.ChatViewStyle,
        Order = 0 )]

    [BooleanField( "Filter Shared Channels by Campus",
        Description = "Only show channels that match the individual's campus or have no campus set.",
        DefaultBooleanValue = false,
        Key = AttributeKey.FilterSharedChannelsByCampus,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Order = 1 )]

    [IntegerField( "Minimum Age",
        Description = "The minimum age required to use chat. If the person does not have a birthdate, the verification template will show. Leave as empty to disable this check altogether.",
        IsRequired = false,
        Key = AttributeKey.MinimumAge,
        Order = 2 )]

    #endregion

    #region Mobile-Specific

    // Even though these settings are swapped between the mobile and web blocks,
    // the order of the attributes should stay sequential to follow existing patterns.

    [CodeEditorField( "Age Verification Template",
        Description = "The XAML template displayed when the person does not have a birthdate.",
        IsRequired = false,
        Key = AttributeKey.MobileAgeVerificationTemplate,
        SiteTypes = Enums.Cms.SiteTypeFlags.Mobile,
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = _defaultMobileAgeVerificationTemplate,
        Order = 3 )]

    [CodeEditorField( "Age Restriction Template",
        Description = "The XAML template displayed when the person is under the minimum age.",
        IsRequired = false,
        Key = AttributeKey.MobileAgeRestrictionTemplate,
        SiteTypes = Enums.Cms.SiteTypeFlags.Mobile,
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = _defaultMobileAgeRestrictionTemplate,
        Order = 4 )]

    #endregion

    #region Web-Specific

    // Even though these settings are swapped between the mobile and web blocks,
    // the order of the attributes should stay sequential to follow existing patterns.

    [CodeEditorField( "Age Verification Template",
        Description = "The XAML template displayed when the person does not have a birthdate.",
        IsRequired = false,
        Key = AttributeKey.WebAgeVerificationTemplate,
        SiteTypes = Enums.Cms.SiteTypeFlags.Web,
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = _defaultWebAgeVerificationTemplate,
        Order = 5 )]

    [CodeEditorField( "Age Restriction Template",
        Description = "The XAML template displayed when the person is under the minimum age.",
        IsRequired = false,
        Key = AttributeKey.WebAgeRestrictionTemplate,
        SiteTypes = Enums.Cms.SiteTypeFlags.Web,
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = _defaultWebAgeRestrictionTemplate,
        Order = 6 )]

    #endregion

    #endregion

    [EntityTypeGuid( "B3D6F875-1589-4543-9E76-5C41201B465B" )]
    [BlockTypeGuid( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" )]
    public class ChatView : RockBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the filter shared channels by campus setting.
        /// </summary>
        protected bool FilterSharedChannelsByCampus => GetAttributeValue( AttributeKey.FilterSharedChannelsByCampus ).AsBoolean();

        /// <summary>
        /// Returns the age verification template based on the current site type.
        /// </summary>
        protected string AgeVerificationTemplate
        {
            get
            {
                if ( RequestContext.IsSiteType( SiteType.Mobile ) )
                {
                    return GetAttributeValue( AttributeKey.MobileAgeVerificationTemplate );
                }
                else
                {
                    return GetAttributeValue( AttributeKey.WebAgeVerificationTemplate );
                }
            }
        }

        /// <summary>
        /// Returns the age restriction template based on the current site type.
        /// </summary>
        protected string AgeRestrictionTemplate
        {
            get
            {
                if ( RequestContext.IsSiteType( SiteType.Mobile ) )
                {
                    return GetAttributeValue( AttributeKey.MobileAgeRestrictionTemplate );
                }
                else
                {
                    return GetAttributeValue( AttributeKey.WebAgeRestrictionTemplate );
                }
            }
        }

        /// <summary>
        /// Returns the <c>ChatViewStyle</c> of the block.
        /// </summary>
        protected ChatViewStyle ChatStyle => GetAttributeValue( AttributeKey.ChatViewStyle ).ConvertToEnumOrNull<ChatViewStyle>() ?? ChatViewStyle.Conversational;

        #endregion

        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The chat view style key.
            /// </summary>
            public const string ChatViewStyle = "ChatViewStyle";

            /// <summary>
            /// The filter shared channels by campus key.
            /// </summary>
            public const string FilterSharedChannelsByCampus = "FilterSharedChannelsByCampus";

            /// <summary>
            /// The minimum age key.
            /// </summary>
            public const string MinimumAge = "MinimumAge";

            /// <summary>
            /// The mobile age verification template key.
            /// </summary>
            public const string MobileAgeVerificationTemplate = "AgeVerificationTemplate";

            /// <summary>
            /// The mobile age restriction template key.
            /// </summary>
            public const string MobileAgeRestrictionTemplate = "AgeRestrictionTemplate";

            /// <summary>
            /// The web age verification template key.
            /// </summary>
            public const string WebAgeVerificationTemplate = "WebAgeVerificationTemplate";

            /// <summary>
            /// The web age restriction template key.
            /// </summary>
            public const string WebAgeRestrictionTemplate = "WebAgeRestrictionTemplate";
        }

        /// <summary>
        /// The page parameter key for the channel ID.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The SelctedChannelId key.
            /// </summary>
            public const string SelectedChannelId = "SelectedChannelId";

            /// <summary>
            /// The ChannelId key.
            /// </summary>
            public const string ChannelId = "ChannelId";

            /// <summary>
            /// The MessageId key.
            /// </summary>
            public const string MessageId = "MessageId";
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            if ( !ChatHelper.IsChatEnabled )
            {
                return null;
            }

            var sharedChannelGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL.AsGuid() );
            var directMessagingChannelGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() );

            // Should never be null, but just in case.
            if ( sharedChannelGroupTypeId == null || directMessagingChannelGroupTypeId == null )
            {
                return null;
            }

            using ( var chatHelper = new ChatHelper() )
            {
                var sharedChannelGroupStreamKey = ChatHelper.GetChatChannelTypeKey( sharedChannelGroupTypeId.Value );
                var directMessagingChannelStreamKey = ChatHelper.GetChatChannelTypeKey( directMessagingChannelGroupTypeId.Value );

                var allowIntegerIdentifier = !PageCache.Layout.Site.DisablePredictableIds;
                var channelId = chatHelper.GetQueryableChatChannelKey( PageParameter( PageParameterKey.ChannelId ), allowIntegerIdentifier );
                var selectedChannelId = chatHelper.GetQueryableChatChannelKey( PageParameter( PageParameterKey.SelectedChannelId ), allowIntegerIdentifier );

                return new ChatViewInitializationBox
                {
                    ChatViewConfigurationBag = new ChatViewConfigurationBag
                    {
                        FilterSharedChannelsByCampus = FilterSharedChannelsByCampus,
                        PublicApiKey = ChatHelper.GetChatConfiguration().ApiKey,
                        SharedChannelTypeKey = sharedChannelGroupStreamKey,
                        DirectMessageChannelTypeKey = directMessagingChannelStreamKey,
                        ChannelId = channelId,
                        SelectedChannelId = selectedChannelId,
                        JumpToMessageId = PageParameter( PageParameterKey.MessageId ),
                        ChatStyle = ChatStyle
                    }
                };
            }
        }

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
            if ( !ChatHelper.IsChatEnabled )
            {
                return ActionBadRequest( "Chat is not configured." );
            }

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
            if ( !ChatHelper.IsChatEnabled )
            {
                return ActionBadRequest( "Chat is not configured." );
            }

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
            if ( !ChatHelper.IsChatEnabled )
            {
                return ActionBadRequest( "Chat is not configured." );
            }

            var person = RequestContext.CurrentPerson;

            if ( person == null )
            {
                return ActionUnauthorized( "You must be logged in to view chat." );
            }

            // Check if age verification is required.
            var minimumAge = GetAttributeValue( AttributeKey.MinimumAge ).AsIntegerOrNull();

            if ( minimumAge.HasValue && minimumAge > 0 )
            {
                var age = person.Age;
                if ( age == null )
                {
                    var mergeFields = RequestContext.GetCommonMergeFields();
                    mergeFields.Add( "MinimumAge", minimumAge.Value );

                    return ActionOk( new ChatPersonDataBag
                    {
                        IsAgeVerificationRequired = true,
                        HasFailedAgeVerification = false,
                        AgeVerificationTemplate = AgeVerificationTemplate.ResolveMergeFields( mergeFields ),
                    } );
                }

                if ( age < minimumAge )
                {
                    var mergeFields = RequestContext.GetCommonMergeFields();
                    mergeFields.Add( "MinimumAge", minimumAge.Value );

                    return ActionOk( new ChatPersonDataBag
                    {
                        IsAgeVerificationRequired = false,
                        HasFailedAgeVerification = true,
                        AgeRestrictionTemplate = AgeRestrictionTemplate.ResolveMergeFields( mergeFields ),
                    } );
                }
            }

            using ( var chatHelper = new ChatHelper() )
            {
                if ( chatHelper.IsPersonBanned( person.Id ) )
                {
                    return ActionForbidden( "You are banned from using chat." );
                }

                var chatUserAuth = await chatHelper.GetChatUserAuthenticationAsync( person.Id, true );

                return ActionOk( new ChatPersonDataBag
                {
                    Token = chatUserAuth.Token,
                    UserId = chatUserAuth.ChatUserKey,
                    IsAgeVerificationRequired = false,
                    HasFailedAgeVerification = false,
                    AgeRestrictionTemplate = string.Empty,
                    AgeVerificationTemplate = string.Empty
                } );
            }
        }

        /// <summary>
        /// Updates the individual's birth date.
        /// </summary>
        /// <param name="birthDate">The birthdate to update.</param>
        /// <returns>A result depicting the operation's success.</returns>
        [BlockAction]
        public BlockActionResult UpdatePersonBirthDate( DatePartsPickerValueBag birthDate )
        {
            var currentPerson = RequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return ActionUnauthorized( "You must be logged in to update your birth date." );
            }

            var birthDateTime = new DateTime( birthDate.Year, birthDate.Month, birthDate.Day );
            currentPerson.SetBirthDate( birthDateTime );
            RockContext.SaveChanges();
            return ActionOk();
        }

        #endregion

        #region Default Templates

        /// <summary>
        /// The default template to display when age verification is required.
        /// </summary>
        private const string _defaultMobileAgeVerificationTemplate = @"<StackLayout StyleClass=""spacing-24, p-16"">
    <Rock:StyledBorder HorizontalOptions=""Center""
        StyleClass=""border-info-strong""
        BorderWidth=""4""
        CornerRadius=""70""
        HeightRequest=""140""
        WidthRequest=""140"">
        <Rock:Icon IconClass=""fa fa-shield-alt"" 
            FontSize=""84""
            HorizontalOptions=""Center""
            VerticalOptions=""Center""
            StyleClass=""text-info-strong"" />
    </Rock:StyledBorder>

    <StackLayout>
        <Label StyleClass=""title1, bold, text-interface-strongest"" 
            Text=""Let’s Verify Your Age"" />
    
        <Label StyleClass=""body, text-interface-stronger""
            Text=""The chat feature is only available to individuals above a certain age. Please confirm your birthdate to proceed."" />
    </StackLayout>
</StackLayout>";

        /// <summary>
        /// The default template to display when the person is under the minimum age.
        /// </summary>
        private const string _defaultMobileAgeRestrictionTemplate = @"<StackLayout StyleClass=""spacing-24, p-16"">
    <Rock:StyledBorder HorizontalOptions=""Center""
        StyleClass=""border-warning-strong""
        BorderWidth=""4""
        CornerRadius=""999""
        HeightRequest=""140""
        WidthRequest=""140"">
        <Rock:Icon IconClass=""fa fa-user-lock"" 
            FontSize=""72""
            StyleClass=""text-warning-strong""
            HorizontalOptions=""Center""
            VerticalOptions=""Center"" />
    </Rock:StyledBorder>

    <StackLayout>
        <Label StyleClass=""title1, bold, text-interface-strongest"" 
            Text=""Chat Unavailable"" />
    
        <Label StyleClass=""body, text-interface-stronger""
            Text=""We're sorry, but this feature is only available to individuals who are {{ MinimumAge }} years or older."" />
    </StackLayout>
</StackLayout>";

        /// <summary>
        /// The default template to display when age verification is required on web.
        /// </summary>
        private const string _defaultWebAgeVerificationTemplate = @"<!-- Age Verification Prompt -->
<div class=""age-verification-wrapper"" style=""text-align: center; padding: 1.5rem;"">
  <!-- Font Awesome shield icon -->
  <div class=""icon"" style=""margin-bottom: 1.5rem;"">
    <i class=""fas fa-shield-alt"" style=""font-size: 4rem; color: var(--color-info-strong);""></i>
  </div>
  
  <h2 style=""color: var(--color-interface-strongest)"">
      Let's Verify Your Age
  </h2>
  <!-- Instructional text -->
  <p style=""font-size: 1rem; max-width: 400px; margin: 0 auto; color: var(--color-interface-strong);"">
    The chat feature is only available to individuals above a certain age. Please confirm your birthdate to proceed.
  </p>
</div>";

        /// <summary>
        /// The default template to display when the person is under the minimum age on web.
        /// </summary>
        private const string _defaultWebAgeRestrictionTemplate = @"<!-- Age Verification Prompt -->
<div class=""age-verification-wrapper"" style=""text-align: center; padding: 1.5rem;"">
  <!-- Font Awesome shield icon -->
  <div class=""icon"" style=""margin-bottom: 1.5rem;"">
    <i class=""fas fa-user-lock"" style=""font-size: 4rem; color: var(--color-warning-strong);""></i>
  </div>
  
  <h2 style=""color: var(--color-interface-strongest)"">
      Chat Unavailable
  </h2>
  <!-- Instructional text -->
  <p style=""font-size: 1rem; max-width: 400px; margin: 0 auto; color: var(--color-interface-strong);"">
    We're sorry, but this feature is only available to individuals who are {{ MinimumAge }} years or older.
  </p>
</div>";

        #endregion
    }
}