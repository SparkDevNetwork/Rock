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
using Rock.ViewModels.Blocks.Crm.AssessmentTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of assessment types.
    /// </summary>

    [DisplayName( "Assessment Type List" )]
    [Category( "CRM" )]
    [Description( "Displays a list of assessment types." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the assessment type details.",
        Category = AttributeCategory.LinkedPages,
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "26dd8b62-5826-44a9-82b1-c6e4e4ab61d0" )]
    [Rock.SystemGuid.BlockTypeGuid( "1fde6d4f-390a-4ff6-ad42-668ec8cc62c4" )]
    [CustomizedGrid]
    public class AssessmentTypeList : RockEntityListBlockType<AssessmentType>
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

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        private static class PreferenceKey
        {
            public const string FilterTitle = "filter-title";

            public const string FilterRequest = "filter-request";

            public const string FilterActive = "filter-active";
        }

        #endregion Keys

        #region Properties

        protected string FilterTitle => GetBlockPersonPreferences()
            .GetValue(PreferenceKey.FilterTitle);

        protected string FilterRequest => GetBlockPersonPreferences()
            .GetValue(PreferenceKey.FilterRequest);

        protected string FilterActive => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterActive );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AssessmentTypeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
        private AssessmentTypeListOptionsBag GetBoxOptions()
        {
            var options = new AssessmentTypeListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "AssessmentTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AssessmentType> GetListQueryable( RockContext rockContext )
        {
            var query = new AssessmentTypeService(rockContext)
                .Queryable()
                .AsNoTracking();

            // Filter by Title
            if (!string.IsNullOrWhiteSpace(FilterTitle))
            {
                query = query.Where(a => a.Title.Contains(FilterTitle));
            }

            // Filter by Requires Request
            if (!string.IsNullOrWhiteSpace(FilterRequest))
            {
                bool? requiresRequest = null;
                if (FilterRequest.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    requiresRequest = true;
                }
                else if (FilterRequest.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    requiresRequest = false;
                }

                if (requiresRequest.HasValue)
                {
                    query = query.Where(a => a.RequiresRequest == requiresRequest.Value);
                }
            }

            // Filter by isActive
            if (!string.IsNullOrWhiteSpace(FilterActive))
            {
                bool? isActive = null;
                if (FilterActive.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    isActive = true;
                }
                else if (FilterActive.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    isActive = false;
                }

                if (isActive.HasValue)
                {
                    query = query.Where(a => a.IsActive == isActive.Value);
                }
            }

            return query;
        }

        /// <inheritdoc/>
        protected override GridBuilder<AssessmentType> GetGridBuilder()
        {
            return new GridBuilder<AssessmentType>()
                .WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddField( "requiresRequest", a => a.RequiresRequest )
                .AddField( "isActive", a => a.IsActive )
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
                var entityService = new AssessmentTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{AssessmentType.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {AssessmentType.FriendlyTypeName}." );
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
