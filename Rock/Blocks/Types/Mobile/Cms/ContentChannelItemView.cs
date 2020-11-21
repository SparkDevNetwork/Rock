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
using System;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Displays custom XAML content on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Content Channel Item View" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays a content channel item by formatting it with XAML." )]
    [IconCssClass( "fa fa-chalkboard" )]

    #region Block Attributes

    [CodeEditorField( "Content Template",
        Description = "The XAML to use when rendering the block. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.ContentTemplate,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block, only affects Lava rendered on the server.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 1 )]

    [ContentChannelField( "Content Channel",
        Description = "Limits content channel items to a specific channel.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.ContentChannel,
        Order = 2 )]

    [BooleanField( "Log Interactions",
        Description = "If enabled then an interaction will be saved when the user views the content channel item.",
        DefaultBooleanValue = false,
        Key = AttributeKeys.LogInteractions,
        Order = 3 )]

    #endregion

    public class ContentChannelItemView : RockMobileBlockType
    {
        /// <summary>
        /// The block settings attribute keys for the MobileContentChannelItemView block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The content template key
            /// </summary>
            public const string ContentTemplate = "ContentTemplate";

            /// <summary>
            /// The enabled lava commands key
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            /// <summary>
            /// The content channel key
            /// </summary>
            public const string ContentChannel = "ContentChannel";

            /// <summary>
            /// The log interactions key
            /// </summary>
            public const string LogInteractions = "LogInteractions";
        }

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

            //
            // The client shell ignores this value and always requests the current config from
            // the server, so just put some placeholder data.
            //
            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                Content = string.Empty,
                ProcessLava = additionalSettings.ProcessLavaOnClient,
                CacheDuration = additionalSettings.CacheDuration,
                DynamicContent = true
            };
        }

        #endregion

        #region Actions

        /// <summary>
        /// Gets the initial content for the Content block.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public object GetInitialContent()
        {
            var content = GetAttributeValue( AttributeKeys.ContentTemplate );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "Item", GetContentChannelItem() );

            content = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );

            return new CallbackResponse
            {
                Content = content
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the content channel item.
        /// </summary>
        /// <returns></returns>
        private ContentChannelItem GetContentChannelItem()
        {
            var contentChannelItemParameterValue = GetContentChannelItemParameterValue();

            if ( string.IsNullOrEmpty( contentChannelItemParameterValue ) )
            {
                return null;
            }

            ContentChannelItem contentChannelItem = GetContentChannelItemFromKey( contentChannelItemParameterValue );

            if ( contentChannelItem == null )
            {
                return null;
            }

            //if ( contentChannelItem.ContentChannel.RequiresApproval )
            //{
            //    var statuses = new List<ContentChannelItemStatus>();
            //    foreach ( var status in ( GetAttributeValue( "Status" ) ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            //    {
            //        var statusEnum = status.ConvertToEnumOrNull<ContentChannelItemStatus>();
            //        if ( statusEnum != null )
            //        {
            //            statuses.Add( statusEnum.Value );
            //        }
            //    }

            //    if ( !statuses.Contains( contentChannelItem.Status ) )
            //    {
            //        return null;
            //    }
            //}

            //
            // If a Channel was specified, verify that the ChannelItem is part of the channel
            //
            var channelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            if ( channelGuid.HasValue )
            {
                var channel = ContentChannelCache.Get( channelGuid.Value );
                if ( channel != null )
                {
                    if ( contentChannelItem.ContentChannelId != channel.Id )
                    {
                        return null;
                    }
                }
            }

            LaunchInteraction( contentChannelItem );

            return contentChannelItem;
        }

        /// <summary>
        /// Gets the content channel item using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private ContentChannelItem GetContentChannelItemFromKey( string contentChannelItemKey )
        {
            Guid? contentChannelGuid = GetAttributeValue( "ContentChannel" ).AsGuidOrNull();

            ContentChannelItem contentChannelItem = null;

            if ( string.IsNullOrEmpty( contentChannelItemKey ) )
            {
                return null;
            }

            //
            // Look up the ContentChannelItem from either the Id, Guid, or
            // Slug depending on the datatype of the ContentChannelQueryParameter value
            //
            int? contentChannelItemId = contentChannelItemKey.AsIntegerOrNull();
            Guid? contentChannelItemGuid = contentChannelItemKey.AsGuidOrNull();

            var rockContext = new RockContext();
            if ( contentChannelItemId.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemId.Value );
            }
            else if ( contentChannelItemGuid.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemGuid.Value );
            }
            else
            {
                var contentChannelQuery = new ContentChannelItemService( rockContext ).Queryable();
                if ( contentChannelGuid.HasValue )
                {

                    contentChannelQuery = contentChannelQuery.Where( c => c.ContentChannel.Guid == contentChannelGuid );
                }

                contentChannelItem = contentChannelQuery
                    .Where( a => a.ContentChannelItemSlugs.Any( s => s.Slug == contentChannelItemKey ) )
                    .FirstOrDefault();
            }

            return contentChannelItem;
        }

        /// <summary>
        /// Gets the content channel item parameter using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private string GetContentChannelItemParameterValue()
        {
            string contentChannelItemKey = null;

            // Determine the ContentChannelItem from the ContentChannelQueryParameter or the first parameter
            string contentChannelQueryParameter = this.GetAttributeValue( "ContentChannelQueryParameter" );
            if ( !string.IsNullOrEmpty( contentChannelQueryParameter ) )
            {
                contentChannelItemKey = RequestContext.GetPageParameter( contentChannelQueryParameter );
            }
            else
            {
                contentChannelItemKey = RequestContext.GetPageParameters()
                    .Select( p => p.Value )
                    .FirstOrDefault();
            }

            return contentChannelItemKey;
        }

        /// <summary>
        /// Launches the interaction if configured
        /// </summary>
        private void LaunchInteraction( ContentChannelItem contentChannelItem )
        {
            bool logInteractions = this.GetAttributeValue( "LogInteractions" ).AsBoolean();
            if ( !logInteractions )
            {
                return;
            }

            bool writeInteractionOnlyIfIndividualLoggedIn = true;// this.GetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn" ).AsBoolean();
            if ( writeInteractionOnlyIfIndividualLoggedIn && RequestContext.CurrentPerson == null )
            {
                // don't log interaction if WriteInteractionOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var mediumType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() );
            var interactionTransaction = new InteractionTransaction(
                mediumType,
                contentChannelItem.ContentChannel,
                contentChannelItem,
                new InteractionTransactionInfo { InteractionSummary = contentChannelItem.Title, PersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId } );

            interactionTransaction.Enqueue();
        }

        #endregion

    }
}
