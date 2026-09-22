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

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Security;
using Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication.Chat
{
    /// <summary>
    /// The church's own settings for chat. Chat is switched on from Spark Connected
    /// Services rather than here, so a church that has not done that sees how to,
    /// and nothing else.
    /// </summary>

    [DisplayName( "Chat Configuration" )]
    [Category( "Communication > Chat" )]
    [Description( "Settings for chat: who is visible to whom, who may be messaged, the youngest age that may take part, and the badges members carry." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "4E1EF8E8-8984-47EA-A6FC-31125C3B6153" )]
    [Rock.SystemGuid.BlockTypeGuid( "D5BE6AAE-70A2-4021-93F7-DD66A09B08CB" )]
    public class ChatConfiguration : RockBlockType
    {
        #region Keys

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        /// <summary>
        /// Where chat is enabled. Not a page reference the administrator can repoint,
        /// because there is exactly one place this can happen.
        /// </summary>
        private const string ConnectedServicesRoute = "~/admin/settings/spark-connected-services";

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var configuration = ChatPlatformConfigurationService.Read();

            var box = new ChatConfigurationInitializationBox
            {
                IsChatConfigured = configuration.IsConfigured,
                ConnectedServicesUrl = RequestContext.ResolveRockUrl( ConnectedServicesRoute ),
                NavigationUrls = new System.Collections.Generic.Dictionary<string, string>
                {
                    [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
                }
            };

            if ( configuration.IsConfigured )
            {
                box.Configuration = WithDataViewNames( ChatConfigurationPolicy.ToBag( configuration ) );
            }

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Stores the church-owned settings. The half issued when chat was enabled, and
        /// the signing key, are left where they are: this writes the fields an
        /// administrator owns and cannot reach the rest, whatever the browser sends.
        /// </summary>
        /// <param name="bag">The settings as the screen has them.</param>
        /// <returns>An empty success, or a refusal.</returns>
        [BlockAction]
        public BlockActionResult SaveConfiguration( ChatConfigurationBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No settings were supplied." );
            }

            var isAuthorizedToEdit = BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );
            var result = ChatConfigurationPolicy.Save( bag, isAuthorizedToEdit );

            if ( !result.IsSaved )
            {
                return ActionForbidden( "You are not authorized to change these settings." );
            }

            ChatPlatformConfigurationService.SaveChurchSettings( result.Configuration );

            return ActionOk();
        }

        /// <summary>
        /// Asks Rock to run the chat platform sync now, so saved settings reach chat without waiting
        /// for the schedule. Returns at once; the screen then checks on the run with
        /// <see cref="GetSyncNowStatus(int)"/>.
        /// </summary>
        /// <returns>Where the press has got to, or a refusal.</returns>
        [BlockAction]
        public BlockActionResult RequestSyncNow()
        {
            var result = ChatSyncNowPolicy.Request(
                ChatPlatformConfigurationService.Read(),
                BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ),
                () => ChatSyncNowPolicy.ReadJob( RockContext ),
                ChatSyncNowPolicy.QueueRunNow );

            return ToSyncNowActionResult( result );
        }

        /// <summary>
        /// Reports where a Sync Now press has got to.
        /// </summary>
        /// <param name="runMarker">The marker the press returned.</param>
        /// <returns>Where the press has got to, or a refusal.</returns>
        [BlockAction]
        public BlockActionResult GetSyncNowStatus( int runMarker )
        {
            var result = ChatSyncNowPolicy.Status(
                BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ),
                runMarker,
                () => ChatSyncNowPolicy.ReadRunAfter( RockContext, runMarker ) );

            return ToSyncNowActionResult( result );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Turns what the Sync Now policy decided into the block's answer.
        /// </summary>
        /// <param name="result">The decision.</param>
        /// <returns>A refusal, or the status.</returns>
        private BlockActionResult ToSyncNowActionResult( ChatSyncNowPolicy.Result result )
        {
            if ( result.IsForbidden )
            {
                return ActionForbidden( result.RefusalMessage );
            }

            if ( result.IsRefused )
            {
                return ActionBadRequest( result.RefusalMessage );
            }

            return ActionOk( result.Status );
        }

        /// <summary>
        /// Fills in the names of the Data Views the settings point at. The stored value
        /// is the identifier alone, so the screen would otherwise show a picker with
        /// something selected and no label on it.
        /// </summary>
        /// <param name="bag">The settings, carrying Data View identifiers.</param>
        /// <returns>The same settings, with names where a Data View still exists.</returns>
        private static ChatConfigurationBag WithDataViewNames( ChatConfigurationBag bag )
        {
            if ( bag.DirectMessageAccessDataView != null )
            {
                bag.DirectMessageAccessDataView.Text = NameOf( bag.DirectMessageAccessDataView.Value );
            }

            foreach ( var badge in bag.ChatBadgeDataViews ?? Enumerable.Empty<Rock.ViewModels.Utility.ListItemBag>() )
            {
                badge.Text = NameOf( badge.Value );
            }

            return bag;
        }

        private static string NameOf( string dataViewGuid )
        {
            var guid = dataViewGuid.AsGuidOrNull();

            return guid.HasValue
                ? DataViewCache.Get( guid.Value )?.Name ?? string.Empty
                : string.Empty;
        }

        #endregion Private Methods
    }
}
