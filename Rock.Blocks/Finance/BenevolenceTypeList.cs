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
using Rock.ViewModels.Blocks.Finance.BenevolenceTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of benevolence types.
    /// </summary>
    [DisplayName( "Benevolence Type List" )]
    [Category( "Finance" )]
    [Description( "Block to display the benevolence types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the benevolence type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "76b2f803-5259-4fd3-a08d-18e546b2c45e" )]
    [Rock.SystemGuid.BlockTypeGuid( "f61c0fdf-e8a0-457a-b8af-42cac8a18718" )]
    [CustomizedGrid]
    public class BenevolenceTypeList : RockEntityListBlockType<BenevolenceType>
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
            var box = new ListBlockBox<BenevolenceTypeListOptionsBag>();
            var builder = GetGridBuilder();

            var canAddOrDelete = GetIsAddDeleteEnabled();
            box.IsAddEnabled = canAddOrDelete;
            box.IsDeleteEnabled = canAddOrDelete;
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
        private BenevolenceTypeListOptionsBag GetBoxOptions()
        {
            var options = new BenevolenceTypeListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "BenevolenceTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<BenevolenceType> GetListQueryable( RockContext rockContext )
        {
            return new BenevolenceTypeService( rockContext ).Queryable().Include( b => b.BenevolenceRequests );
        }

        /// <inheritdoc/>
        protected override IQueryable<BenevolenceType> GetOrderedListQueryable( IQueryable<BenevolenceType> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( b => b.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<BenevolenceType> GetGridBuilder()
        {
            return new GridBuilder<BenevolenceType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "showFinancialResults", a => a.ShowFinancialResults )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "hasRequests", a => a.BenevolenceRequests.Count > 0 )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
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
                var entityService = new BenevolenceTypeService( rockContext );
                var benevolenceRequestService = new BenevolenceRequestService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{BenevolenceType.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${BenevolenceType.FriendlyTypeName}." );
                }

                string errorMessage = null;
                rockContext.WrapTransaction( () =>
                {
                    var benevolenceRequests = entity.BenevolenceRequests.ToList();

                    foreach ( var benevolenceRequest in benevolenceRequests )
                    {
                        if ( !benevolenceRequestService.CanDelete( benevolenceRequest, out errorMessage ) )
                        {
                            return;
                        }

                        benevolenceRequestService.Delete( benevolenceRequest );
                    }

                    rockContext.SaveChanges();

                    if ( !entityService.CanDelete( entity, out errorMessage ) )
                    {
                        return;
                    }

                    entityService.Delete( entity );
                    rockContext.SaveChanges();
                } );

                BenevolenceWorkflowService.RemoveCachedTriggers();

                return !string.IsNullOrWhiteSpace( errorMessage ) ? ActionBadRequest( errorMessage ) : ActionOk();
            }
        }

        #endregion
    }
}
