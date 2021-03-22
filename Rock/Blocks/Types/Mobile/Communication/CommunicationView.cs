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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Communication
{
    /// <summary>
    /// Displays a communication to the user.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Communication View" )]
    [Category( "Mobile > Communication" )]
    [Description( "Displays a communication to the user." )]
    [IconCssClass( "fa fa-envelope-open-text" )]

    #region Block Attributes

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the content.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW,
        IsRequired = true,
        DefaultValue = "39B8B16D-D213-46FD-9B8F-710453806193",
        Key = AttributeKeys.Template,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 1 )]

    #endregion

    public class CommunicationView : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the MobileContent block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The content key
            /// </summary>
            public const string Template = "Template";

            /// <summary>
            /// The enabled lava commands key
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        /// <summary>
        /// Gets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        protected string EnabledLavaCommands => GetAttributeValue( AttributeKeys.EnabledLavaCommands );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Content";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var additionalSettings = GetAdditionalSettings();

            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                ProcessLava = additionalSettings.ProcessLavaOnClient,
                CacheDuration = additionalSettings.CacheDuration,
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the XAML content to display when a communication is not found.
        /// </summary>
        /// <returns>A <see cref="CallbackResponse"/> to be sent to the client.</returns>
        private CallbackResponse GetNotFoundContent()
        {
            var content = "<Rock:NotificationBox NotificationType=\"Warning\" Text=\"Communication not found.\" />";

            return new CallbackResponse
            {
                Content = content
            };
        }

        /// <summary>
        /// Writes the opened interaction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="recipient">The recipient.</param>
        private void WriteInteraction( RockContext rockContext, CommunicationRecipient recipient )
        {
            var interactionService = new InteractionService( rockContext );

            InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                                .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(),
                                    recipient.Communication.Id, recipient.Communication.Subject );
            rockContext.SaveChanges();

            var clientType = "None";
            var clientOs = string.Empty;
            var ipAddress = RequestContext.ClientInformation.IpAddress;
            var site = MobileHelper.GetCurrentApplicationSite( false, rockContext );
            var now = RockDateTime.Now;

            //
            // Determine if this is a phone or tablet.
            //
            var deviceData = RequestContext.GetHeader( "X-Rock-DeviceData" )
                .FirstOrDefault()
                ?.FromJsonOrNull<DeviceData>();
            if ( deviceData != null )
            {
                clientType = deviceData.DeviceType == Common.Mobile.Enums.DeviceType.Phone ? "Mobile" : "Tablet";
                clientOs = deviceData.DevicePlatform.ToString();
            }

            recipient.Status = CommunicationRecipientStatus.Opened;
            recipient.OpenedDateTime = now;
            recipient.OpenedClient = $"{clientOs} {site.Name} ({clientType})";

            interactionService.AddInteraction( interactionComponent.Id, recipient.Id, "Opened", "", recipient.PersonAliasId, now, site.Name, clientOs, clientType, string.Empty, ipAddress, null );

            rockContext.SaveChanges();
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The initial content.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            var communicationRecipientGuid = RequestContext.GetPageParameter( "CommunicationRecipientGuid" ).AsGuidOrNull();

            if ( !communicationRecipientGuid.HasValue )
            {
                return GetNotFoundContent();
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationRecipientService = new CommunicationRecipientService( rockContext );
                var recipient = communicationRecipientService.Queryable()
                    .Include( a => a.Communication )
                    .Include( a => a.PersonAlias.Person )
                    .Where( a => a.Guid == communicationRecipientGuid.Value )
                    .SingleOrDefault();

                if ( recipient == null )
                {
                    return GetNotFoundContent();
                }

                //
                // Configure the standard merge fields.
                //
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "CurrentPage", PageCache );
                mergeFields.AddOrReplace( "Communication", recipient.Communication );
                mergeFields.AddOrReplace( "Person", recipient.PersonAlias.Person );

                //
                // Add in all the additional merge fields from grids.
                //
                foreach ( var mergeField in recipient.AdditionalMergeValues )
                {
                    if ( !mergeFields.ContainsKey( mergeField.Key ) )
                    {
                        mergeFields.Add( mergeField.Key, mergeField.Value );
                    }
                }

                var content = Template.ResolveMergeFields( mergeFields, null, EnabledLavaCommands );

                WriteInteraction( rockContext, recipient );

                return new CallbackResponse
                {
                    Content = content
                };
            }
        }

        #endregion
    }
}
