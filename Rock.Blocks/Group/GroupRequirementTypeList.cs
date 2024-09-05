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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupRequirementTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a list of group requirement types.
    /// </summary>
    [DisplayName( "Group Requirement Type List" )]
    [Category( "Group" )]
    [Description( "List of Group Requirement Types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the group requirement type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "c1d4fec2-f868-4fe7-899f-62ccfbeb29c6" )]
    [Rock.SystemGuid.BlockTypeGuid( "da7834c6-c5c6-470b-b1c8-9afa492151f8" )]
    [CustomizedGrid]
    public class GroupRequirementTypeList : RockEntityListBlockType<GroupRequirementType>
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
            var box = new ListBlockBox<GroupRequirementTypeListOptionsBag>();
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
        private GroupRequirementTypeListOptionsBag GetBoxOptions()
        {
            var options = new GroupRequirementTypeListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "GroupRequirementTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupRequirementType> GetListQueryable( RockContext rockContext )
        {
            var groupRequirementTypeService = new GroupRequirementTypeService( rockContext );
            return groupRequirementTypeService.Queryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupRequirementType> GetOrderedListQueryable( IQueryable<GroupRequirementType> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( g => g.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<GroupRequirementType> GetGridBuilder()
        {
            return new GridBuilder<GroupRequirementType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddField( "canExpire", a => a.CanExpire )
                .AddTextField( "type", a => a.RequirementCheckType.ToString() )
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
                var entityService = new GroupRequirementTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{GroupRequirementType.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {GroupRequirementType.FriendlyTypeName}." );
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
