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
using Rock.ViewModels.Blocks.Engagement.ConnectionOpportunityList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of connection opportunities.
    /// </summary>

    [DisplayName( "Connection Opportunity List" )]
    [Category( "Engagement" )]
    [Description( "Displays a list of connection opportunities." )]
    [IconCssClass( "ti ti-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the connection opportunity details.",
        Key = AttributeKey.DetailPage )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "02713f10-e574-45e0-9178-a02f7957b3a4" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "8eb82e1e-c0bd-4591-9d7a-f120a871fec3" )]
    [Rock.SystemGuid.BlockTypeGuid( "481AE184-4654-48FB-A2B4-90F6604B59B8" )]
    [CustomizedGrid]
    public class ConnectionOpportunityList : RockEntityListBlockType<ConnectionOpportunity>
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
            public const string ConnectionTypeId = "ConnectionTypeId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Singleton instance of the connection type, should be accessed via <see cref="GetConnectionType"/>.
        /// </summary>
        private ConnectionTypeCache _connectionType;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ConnectionOpportunityListOptionsBag>();
            var builder = GetGridBuilder();
            var isAddDeleteEnabled = GetIsAddDeleteEnabled();

            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private ConnectionOpportunityListOptionsBag GetBoxOptions()
        {
            var options = new ConnectionOpportunityListOptionsBag
            {
                IsReOrderColumnVisible = GetIsAddDeleteEnabled()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add and delete actions should be enabled in the grid. This mirrors the
        /// legacy Web Forms block: edit rights are granted by either block-level Edit security or Edit
        /// security on the connection type the opportunities belong to.
        /// </summary>
        /// <returns>A boolean value that indicates if the add and delete actions should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            if ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return true;
            }

            var connectionType = GetConnectionType();
            return connectionType?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) ?? false;
        }

        /// <summary>
        /// Determines whether the current person is authorized to delete the specified connection
        /// opportunity.
        /// </summary>
        /// <param name="entity">The connection opportunity being deleted.</param>
        /// <returns><c>true</c> if the current person is authorized to delete the opportunity; otherwise <c>false</c>.</returns>
        private bool IsAuthorizedToDelete( ConnectionOpportunity entity )
        {
            return GetIsAddDeleteEnabled()
                || entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether the current person may view the opportunity grid at all. This mirrors the
        /// legacy Web Forms block, which hid the entire panel unless the person had Edit rights (block-level
        /// or on the connection type) or View rights on the connection type. Individual rows are still
        /// filtered by per-opportunity View in <see cref="GetListItems"/>.
        /// </summary>
        /// <returns><c>true</c> if the current person may view the opportunity grid; otherwise <c>false</c>.</returns>
        private bool GetIsViewAuthorized()
        {
            if ( GetIsAddDeleteEnabled() )
            {
                return true;
            }

            var connectionType = GetConnectionType();
            return connectionType?.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) ?? false;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>
            {
                ["ConnectionOpportunityId"] = "((Key))",
                ["autoEdit"] = "true",
                ["returnUrl"] = this.GetCurrentPageUrl()
            };

            var connectionType = GetConnectionType();
            if ( connectionType != null )
            {
                queryParams[PageParameterKey.ConnectionTypeId] = connectionType.IdKey;
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ConnectionOpportunity> GetListQueryable( RockContext rockContext )
        {
            var connectionType = GetConnectionType();
            if ( connectionType == null )
            {
                return new List<ConnectionOpportunity>().AsQueryable();
            }

            // Mirror the legacy block: the entire grid was hidden unless the person could view ( or edit )
            // the connection type. Rows are additionally filtered by per-opportunity View in GetListItems.
            if ( !GetIsViewAuthorized() )
            {
                return new List<ConnectionOpportunity>().AsQueryable();
            }

            return base.GetListQueryable( rockContext ).Where( c => c.ConnectionTypeId == connectionType.Id );
        }

        /// <inheritdoc/>
        protected override List<ConnectionOpportunity> GetListItems( IQueryable<ConnectionOpportunity> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();
            return items.Where( co => co.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) ).ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<ConnectionOpportunity> GetGridBuilder()
        {
            return new GridBuilder<ConnectionOpportunity>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "summary", a => a.Summary )
                .AddField( "isActive", a => a.IsActive )
                .AddTextField( "publicName", a => a.PublicName )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <inheritdoc/>
        protected override IQueryable<ConnectionOpportunity> GetOrderedListQueryable( IQueryable<ConnectionOpportunity> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( co => co.Order ).ThenBy( co => co.Name );
        }

        /// <summary>
        /// Retrieve a singleton connection type for data operations in this block.
        /// </summary>
        private ConnectionTypeCache GetConnectionType()
        {
            if ( _connectionType == null )
            {
                _connectionType = ConnectionTypeCache.Get( PageParameter( PageParameterKey.ConnectionTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
            }

            return _connectionType;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            // Mirror the legacy block: reordering was only available with Edit rights ( block-level or
            // on the connection type ). The reorder column was hidden from everyone else.
            if ( !GetIsAddDeleteEnabled() )
            {
                return ActionBadRequest( $"Not authorized to reorder {ConnectionOpportunity.FriendlyTypeName}." );
            }

            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new ConnectionOpportunityService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
            }

            if ( !IsAuthorizedToDelete( entity ) )
            {
                return ActionBadRequest( $"Not authorized to delete {ConnectionOpportunity.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            // Clear cached connection workflow triggers so the
            // deleted opportunity's triggers are no longer evaluated.
            ConnectionWorkflowService.RemoveCachedTriggers();

            return ActionOk();
        }

        #endregion
    }
}
