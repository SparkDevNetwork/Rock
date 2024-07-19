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
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays a list of system communications.
    /// </summary>

    [DisplayName( "System Communication List" )]
    [Category( "Communication" )]
    [Description( "Lists the system communications that can be configured for use by the system and other automated (non-user) tasks." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the system communication details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "6452b97c-2777-44ce-8dca-72f32d07e500" )]
    [Rock.SystemGuid.BlockTypeGuid( "411a5ad2-d667-4283-b58d-8a8614b07b0f" )]
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

        private static class PreferenceKey
        {
            public const string FilterCategory = "filter-category";
            public const string FilterActive = "filter-active";
            public const string FilterSupports = "filter-supports";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the category to filter results by.
        /// </summary>
        /// <value>
        /// The filter category.
        /// </value>
        protected Guid? FilterCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCategory )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the active status to filter results by, if value is inactive, inactive results are returned.
        /// </summary>
        /// <value>
        /// The filter active.
        /// </value>
        protected string FilterActive => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterActive );

        /// <summary>
        /// Gets the supported media to filter results by, i.e whether results should include communications sent
        /// via SMs or PushNotifications.
        /// </summary>
        /// <value>
        /// The filter supports.
        /// </value>
        protected string FilterSupports => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSupports );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SystemCommunicationListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SystemCommunicationListOptionsBag GetBoxOptions()
        {
            var options = new SystemCommunicationListOptionsBag()
            {
                CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "SystemCommunicationId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<SystemCommunication> GetListQueryable( RockContext rockContext )
        {
            var SystemCommunicationService = new SystemCommunicationService( rockContext );

            var systemCommunicationsQuery = SystemCommunicationService.Queryable().Include( sc => sc.Category );

            // Filter By: Category
            if ( FilterCategory.HasValue )
            {
                systemCommunicationsQuery = systemCommunicationsQuery.Where( a => a.CategoryId.HasValue && a.Category.Guid == FilterCategory.Value );
            }

            // Filter By: Is Active
            switch ( FilterActive )
            {
                case "Active":
                    systemCommunicationsQuery = systemCommunicationsQuery.Where( a => a.IsActive ?? false );
                    break;
                case "Inactive":
                    systemCommunicationsQuery = systemCommunicationsQuery.Where( a => !( a.IsActive ?? false ) );
                    break;
            }

            // Filter By: Supports (Email|SMS)
            switch ( FilterSupports )
            {
                case "SMS":
                    systemCommunicationsQuery = systemCommunicationsQuery.Where( a => a.SMSMessage != null && a.SMSMessage.Trim() != "" );
                    break;
                case "Push Notification":
                    systemCommunicationsQuery = systemCommunicationsQuery.Where( a => a.PushMessage != null && a.PushMessage.Trim() != "" );
                    break;
            }

            return systemCommunicationsQuery;
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
                .AsEnumerable()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.GetCurrentPerson() ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<SystemCommunication> GetGridBuilder()
        {
            var page = PageCache.Get( Rock.SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW.AsGuid() );
            var rockContext = new RockContext();

            return new GridBuilder<SystemCommunication>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddTextField( "subject", a => a.Subject )
                .AddTextField( "category", a => a.Category?.Name )
                .AddTextField( "from", a => a.From )
                .AddTextField( "smsMessage", a => a.SMSMessage )
                .AddTextField( "pushMessage", a => a.PushMessage )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem )
                .AddTextField( "previewUrl", a => GetPreviewUrl( a, rockContext, page ) )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        private string GetPreviewUrl( SystemCommunication systemCommunication, RockContext rockContext, PageCache page )
        {
            string url = string.Empty;

            if ( page != null )
            {
                var route = new PageRouteService( rockContext ).GetByPageId( page.Id ).First();

                if ( route != null )
                {
                    url = RequestContext.ResolveRockUrl( $"~/{route.Route}/?SystemCommunicationId={systemCommunication.Id}" );
                }
            }

            return url;
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
