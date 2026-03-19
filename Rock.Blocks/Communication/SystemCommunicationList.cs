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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.SystemCommunicationList;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays a list of system communications.
    /// </summary>
    [DisplayName( "System Communication List" )]
    [Category( "Communication" )]
    [Description( "Lists the system communications that can be configured for use by the system and other automated (non-user) tasks." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the system communication details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "6452b97c-2777-44ce-8dca-72f32d07e500" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "411a5ad2-d667-4283-b58d-8a8614b07b0f" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.SYSTEM_COMMUNICATION_LIST )]
    [CustomizedGrid]
    public class SystemCommunicationList : RockEntityListBlockType<SystemCommunication>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var canAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            var builder = GetGridBuilder();

            var box = new ListBlockBox<SystemCommunicationListOptionsBag>();
            box.IsAddEnabled = canAdministrate;
            box.IsDeleteEnabled = canAdministrate;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = new SystemCommunicationListOptionsBag { CanAdministrate = canAdministrate };
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string> { [PageParameterKey.CommunicationId] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<SystemCommunication> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Include( sc => sc.Category );
        }

        /// <inheritdoc/>
        protected override IQueryable<SystemCommunication> GetOrderedListQueryable( IQueryable<SystemCommunication> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Category.Name ).ThenBy( a => a.Title );
        }

        /// <inheritdoc/>
        protected override List<SystemCommunication> GetListItems( IQueryable<SystemCommunication> queryable, RockContext rockContext )
        {
            return queryable
                .ToList()
                .Where( a => a.IsAuthorized( Authorization.VIEW, this.GetCurrentPerson() ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<SystemCommunication> GetGridBuilder()
        {
            return new GridBuilder<SystemCommunication>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddTextField( "subject", a => a.Subject )
                .AddTextField( "category", a => a.Category?.Name ?? "" )
                .AddTextField( "from", a => a.From ?? "" )
                .AddTextField( "smsMessage", a => a.SMSMessage )
                .AddTextField( "pushMessage", a => a.PushMessage )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem )
                .AddTextField( "previewUrl", a => GetPreviewUrl( a ) )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the preview URL for the specified system communication,
        /// or an empty string if the preview page or its route cannot be found.
        /// </summary>
        /// <param name="systemCommunication">The system communication to get the preview URL for.</param>
        /// <returns>The resolved preview URL, or an empty string.</returns>
        private string GetPreviewUrl( SystemCommunication systemCommunication )
        {
            var page = PageCache.Get( Rock.SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW.AsGuid() );
            if ( page == null )
            {
                return string.Empty;
            }

            var route = new PageRouteService( RockContext ).GetByPageId( page.Id ).FirstOrDefault();
            if ( route == null )
            {
                return string.Empty;
            }

            return RequestContext.ResolveRockUrl( $"~/{route.Route}/?SystemCommunicationId={systemCommunication.Id}" );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SystemCommunicationService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{SystemCommunication.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {SystemCommunication.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
