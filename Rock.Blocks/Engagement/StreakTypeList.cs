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
using Rock.ViewModels.Blocks.Engagement.StreakTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of streak types.
    /// </summary>
    [DisplayName( "Streak Type List" )]
    [Category( "Streaks" )]
    [Description( "Shows a list of all streak types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the streak type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "fb234106-94fd-4206-aa85-4377f1d2c512" )]
    [Rock.SystemGuid.BlockTypeGuid( "6f0f3ad2-4989-4f50-b394-0de3c7af35ad" )]
    [CustomizedGrid]
    public class StreakTypeList : RockEntityListBlockType<StreakType>
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
            public const string FilterActiveStatus = "filter-active-status";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// If true only active streak types are included in the result.
        /// </summary>
        /// <value>
        /// The active status filter.
        /// </value>
        protected string FilterActiveStatus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterActiveStatus );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StreakTypeListOptionsBag>();
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
        private StreakTypeListOptionsBag GetBoxOptions()
        {
            var options = new StreakTypeListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "StreakTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StreakType> GetListQueryable( RockContext rockContext )
        {
            // Don't use the streak type cache here since the users will expect to see the instant changes to this
            // query when they add, edit, etc
            var queryable = new StreakTypeService( rockContext ).Queryable().AsNoTracking();

            // Filter by: Active
            switch ( FilterActiveStatus )
            {
                case "Active":
                    return queryable.Where( s => s.IsActive );
                case "Inactive":
                    return queryable.Where( s => !s.IsActive );
                default:
                    return queryable;
            }
        }

        /// <inheritdoc/>
        protected override IQueryable<StreakType> GetOrderedListQueryable( IQueryable<StreakType> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( s => s.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StreakType> GetGridBuilder()
        {
            return new GridBuilder<StreakType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddField( "isActive", a => a.IsActive )
                .AddTextField( "occurrenceFrequency", a => a.OccurrenceFrequency.ToString() )
                .AddDateTimeField( "startDate", a => a.StartDate )
                .AddField( "enrollmentCount", a => a.Streaks.Count )
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
                var entityService = new StreakTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{StreakType.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {StreakType.FriendlyTypeName}." );
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
