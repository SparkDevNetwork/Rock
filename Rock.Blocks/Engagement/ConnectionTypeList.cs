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
using Rock.ViewModels.Blocks.Engagement.ConnectionTypeList;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of connection types.
    /// </summary>

    [DisplayName( "Connection Type List" )]
    [Category( "Engagement" )]
    [Description( "Displays a list of connection types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the connection type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "7d78f300-3df7-4ed7-bc2b-813d4f866220" )]
    [Rock.SystemGuid.BlockTypeGuid( "45f30ea2-f93b-4a63-806f-7cd375daacab" )]
    [CustomizedGrid]
    public class ConnectionTypeList : RockEntityListBlockType<ConnectionType>
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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ConnectionTypeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
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
        private ConnectionTypeListOptionsBag GetBoxOptions()
        {
            var options = new ConnectionTypeListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new ConnectionType();

            return entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ConnectionTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ConnectionType> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ConnectionType> GetGridBuilder()
        {
            return new GridBuilder<ConnectionType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "opportunityCount", a => a.ConnectionOpportunities.Count )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
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
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

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
                var entityService = new ConnectionTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{ConnectionType.FriendlyTypeName} not found." );
                }

                // var connectionOppotunityies = new Service<ConnectionOpportunity>( rockContext ).Queryable().All( a => a.ConnectionTypeId == connectionType.Id );
                var connectionOpportunities = entity.ConnectionOpportunities.ToList();
                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                ConnectionRequestActivityService connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                foreach ( var connectionOpportunity in connectionOpportunities )
                {
                    var connectionRequestActivities = new Service<ConnectionRequestActivity>( rockContext ).Queryable().Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id ).ToList();
                    foreach ( var connectionRequestActivity in connectionRequestActivities )
                    {
                        connectionRequestActivityService.Delete( connectionRequestActivity );
                    }

                    rockContext.SaveChanges();
                    string errorMessageConnectionOpportunity;
                    if ( !connectionOpportunityService.CanDelete( connectionOpportunity, out errorMessageConnectionOpportunity ) )
                    {
                        return ActionBadRequest( errorMessageConnectionOpportunity );
                    }

                    connectionOpportunityService.Delete( connectionOpportunity );
                }


                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {ConnectionType.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                rockContext.SaveChanges();

                entityService.Delete( entity );
                rockContext.SaveChanges();


                ConnectionWorkflowService.RemoveCachedTriggers();

                return ActionOk();
            }
        }

        /// <inheritdoc/>
        protected override IQueryable<ConnectionType> GetOrderedListQueryable( IQueryable<ConnectionType> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( nameof( IOrdered.Order ) )
                .ThenBy( e => e.Name );
        }

        #endregion
    }
}
