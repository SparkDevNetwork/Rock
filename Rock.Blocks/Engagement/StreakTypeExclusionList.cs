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
using Rock.ViewModels.Blocks.Engagement.StreakTypeExclusionList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of streak type exclusions.
    /// </summary>
    [DisplayName( "Streak Type Exclusion List" )]
    [Category( "Streaks" )]
    [Description( "Lists all the exclusions for a streak type." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the streak type exclusion details.",
        Key = AttributeKey.DetailPage )]
    [Rock.SystemGuid.EntityTypeGuid( "7740ecd4-1f20-4de3-8289-4a4f0aff0646" )]
    [Rock.SystemGuid.BlockTypeGuid( "70a4fbe1-511b-457d-84a3-cf6d5b0e09ae" )]
    [CustomizedGrid]
    public class StreakTypeExclusionList : RockEntityListBlockType<StreakTypeExclusion>
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
            public const string StreakTypeId = "StreakTypeId";
            public const string StreakTypeExclusionId = "StreakTypeExclusionId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Singleton instance of the streak type, should be access via <see cref="GetStreakType"/>
        /// </summary>
        private StreakType _streakType = null;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StreakTypeExclusionListOptionsBag>();
            var builder = GetGridBuilder();
            var isEnabled = GetIsAddOrDeleteEnabled();

            box.IsAddEnabled = isEnabled;
            box.IsDeleteEnabled = isEnabled;
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
        private StreakTypeExclusionListOptionsBag GetBoxOptions()
        {
            var streakType = GetStreakType();
            var options = new StreakTypeExclusionListOptionsBag()
            {
                IsBlockVisible = GetCanView(),
                StreakTypeName = streakType?.Name,
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddOrDeleteEnabled()
        {
            var streakType = GetStreakType();
            return streakType?.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) == true || streakType?.IsAuthorized( Authorization.MANAGE_MEMBERS, GetCurrentPerson() ) == true;
        }

        /// <summary>
        /// Determines if the current user can view the exclusions.
        /// </summary>
        /// <returns></returns>
        private bool GetCanView()
        {
            var streakType = GetStreakType();
            return streakType?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>()
            {
                { PageParameterKey.StreakTypeExclusionId, "((Key))" }
            };

            var streakType = GetStreakType();
            if ( streakType != null )
            {
                queryParams.Add( PageParameterKey.StreakTypeId, streakType.IdKey );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StreakTypeExclusion> GetListQueryable( RockContext rockContext )
        {
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                return new List<StreakTypeExclusion>().AsQueryable();
            }

            return new StreakTypeExclusionService( rockContext ).Queryable()
                .AsNoTracking()
                .Include( soe => soe.Location )
                .Where( soe => soe.StreakTypeId == streakType.Id );
        }

        /// <inheritdoc/>
        protected override IQueryable<StreakTypeExclusion> GetOrderedListQueryable( IQueryable<StreakTypeExclusion> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( soe => soe.Location.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StreakTypeExclusion> GetGridBuilder()
        {
            return new GridBuilder<StreakTypeExclusion>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "location", a => a.Location?.Name ?? " -- " );
        }

        /// <summary>
        /// Retrieve a singleton streak type for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private StreakType GetStreakType()
        {
            if ( _streakType == null )
            {
                var streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsIntegerOrNull();

                if ( streakTypeId.HasValue )
                {
                    _streakType = new StreakTypeService( RockContext ).Get( streakTypeId.Value );
                }
            }

            return _streakType;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new StreakTypeExclusionService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{StreakTypeExclusion.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {StreakTypeExclusion.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}