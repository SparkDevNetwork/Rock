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
using Rock.ViewModels.Blocks.Finance.FinancialGatewayList;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial gateways.
    /// </summary>

    [DisplayName( "Gateway List" )]
    [Category( "Finance" )]
    [Description( "Block for viewing list of financial gateways." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the financial gateway details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "9158f560-4eae-4e1d-80ff-da24c351e241" )]
    [Rock.SystemGuid.BlockTypeGuid( "0f99866a-7fab-462d-96eb-9f9534322c57" )]
    [CustomizedGrid]
    public class FinancialGatewayList : RockEntityListBlockType<FinancialGateway>
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
            var box = new ListBlockBox<FinancialGatewayListOptionsBag>();
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
        private FinancialGatewayListOptionsBag GetBoxOptions()
        {
            using ( var rockContext = new RockContext() )
            {
                var options = new FinancialGatewayListOptionsBag
                {
                    IsInactiveGatewayNotificationVisible = GetListQueryable( rockContext ).Any( g => !g.IsActive )
                };
                return options;
            }
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new FinancialGateway();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "GatewayId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialGateway> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Include( a => a.EntityType );
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialGateway> GetOrderedListQueryable( IQueryable<FinancialGateway> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialGateway> GetGridBuilder()
        {
            return new GridBuilder<FinancialGateway>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "entityType", a => GetComponentDisplayName( a.EntityType ) )
                .AddField( "isActive", a => a.IsActive );
        }

        /// <summary>
        /// Gets the display name of the Entity Type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns></returns>
        private string GetComponentDisplayName( EntityType entityType )
        {
            if ( entityType != null )
            {
                var gatewayEntityType = EntityTypeCache.Get( entityType.Guid );
                var name = Rock.Reflection.GetDisplayName( gatewayEntityType.GetEntityType() );

                // If it has a DisplayName, use it as is
                if ( !string.IsNullOrWhiteSpace( name ) )
                {
                    return name;
                }
                else
                {
                    // Otherwise use the previous logic with SplitCase on the ComponentName
                    return Rock.Financial.GatewayContainer.GetComponentName( entityType.Name ).ToStringSafe().SplitCase();
                }
            }

            return string.Empty;
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
                var entityService = new FinancialGatewayService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{FinancialGateway.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${FinancialGateway.FriendlyTypeName}." );
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
