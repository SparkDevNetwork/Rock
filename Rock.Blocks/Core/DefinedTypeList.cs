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
using Rock.ViewModels.Blocks.Core.DefinedTypeList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of defined types.
    /// </summary>

    [DisplayName( "Defined Type List" )]
    [Category( "Core" )]
    [Description( "Displays a list of defined types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the defined type details.",
        Key = AttributeKey.DetailPage )]

    [CategoryField( AttributeKey.Categories,
        Description = "If block should only display Defined Types from specific categories, select the categories here.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.DefinedType",
        Order = 1,
        IsRequired = false,
        Key = AttributeKey.Categories )]

    [Rock.SystemGuid.EntityTypeGuid( "6508dcc1-ada8-4299-9147-dc37095c2aff" )]
    [Rock.SystemGuid.BlockTypeGuid( "7faf32d3-c577-462a-bc0b-d34de3316a5b" )]
    [CustomizedGrid]
    public class DefinedTypeList : RockEntityListBlockType<DefinedType>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string Categories = "Categories";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterCategory = "filter-category";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the category identifier to use when filtering the defined types.
        /// </summary>
        /// <value>
        /// The category identifiers to use when filtering the defined types.
        /// </value>
        protected Guid? FilterCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCategory )
            .FromJsonOrNull<ListItemBag>()?.Value.AsGuid();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<DefinedTypeListOptionsBag>();
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
        private DefinedTypeListOptionsBag GetBoxOptions()
        {
            var options = new DefinedTypeListOptionsBag();
            options.ShowCategoryColumn = GetAttributeValue( AttributeKey.Categories ).IsNullOrWhiteSpace();
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new DefinedType();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "DefinedTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<DefinedType> GetListQueryable( RockContext rockContext )
        {
            var definedTypeQry = base.GetListQueryable( rockContext )
                .Include( a => a.Category );

            var categoryGuids = GetAttributeValue( AttributeKey.Categories ).SplitDelimitedValues().AsGuidList();
            if ( categoryGuids.Any() )
            {
                definedTypeQry = definedTypeQry.Where( a => a.Category != null && categoryGuids.Contains( a.Category.Guid ) );
            }
            else if ( FilterCategory.HasValue )
            {
                definedTypeQry = definedTypeQry.Where( a => a.Category != null && a.Category.Guid == FilterCategory.Value );
            }

            return definedTypeQry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<DefinedType> GetGridBuilder()
        {
            return new GridBuilder<DefinedType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "category", a => a.Category?.Name )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
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
                var entityService = new DefinedTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{DefinedType.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${DefinedType.FriendlyTypeName}." );
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
