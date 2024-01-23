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
using Rock.ViewModels.Blocks.Engagement.StepProgramList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of step programs.
    /// </summary>

    [DisplayName( "Step Program List" )]
    [Category( "Steps" )]
    [Description( "Displays a list of step programs." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]


    [CategoryField(
        "Categories",
        Key = AttributeKey.Categories,
        Description = "If block should only display Step Programs from specific categories, select the categories here.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.StepProgram",
        IsRequired = false,
        Order = 1)]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the step program details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "ef0d9904-48be-4ba5-9950-e77d318a4cfa" )]
    [Rock.SystemGuid.BlockTypeGuid( "5284b259-a9ec-431c-b949-661780bfcd68" )]
    [CustomizedGrid]
    public class StepProgramList : RockListBlockType<StepProgramListBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Categories = "Categories";
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterActive = "filter-active";

            public const string FilterCategory = "filter-category";
        }

        #endregion Keys

        #region Properties
        protected string FilterActive => GetBlockPersonPreferences()
            .GetValue(PreferenceKey.FilterActive);

        protected string FilterCategory => GetBlockPersonPreferences()
            .GetValue(PreferenceKey.FilterCategory);
        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StepProgramListOptionsBag>();
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
        private StepProgramListOptionsBag GetBoxOptions()
        {
            var options = new StepProgramListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ProgramId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StepProgramListBag> GetListQueryable( RockContext rockContext )
        {
            var stepService = new StepService( rockContext );
            var completedStepsQry = stepService.Queryable()
                .Where( x => x.StepStatus != null && x.StepStatus.IsCompleteStatus && x.StepType.IsActive );

            var stepProgramsQry = new StepProgramService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( a => a.Category )
                .Include( a => a.StepTypes )
                .OrderBy( sp => sp.Order );

            // Filter by Category
            if ( !string.IsNullOrWhiteSpace( FilterCategory ) )
            {
                stepProgramsQry = ( IOrderedQueryable<StepProgram> )stepProgramsQry.Where( sp => sp.Category.Name == FilterCategory );
            }

            // Filter by isActive
            if ( !string.IsNullOrWhiteSpace( FilterActive ) )
            {
                bool isActive = FilterActive.Equals( "Active", StringComparison.OrdinalIgnoreCase );
                stepProgramsQry = ( IOrderedQueryable<StepProgram> )stepProgramsQry.Where( sp => sp.IsActive == isActive );
            }

            return stepProgramsQry.Select( sp => new StepProgramListBag
            {
                Id = sp.Id,
                Name = sp.Name,
                IconCssClass = sp.IconCssClass,
                Category = sp.Category.Name,
                Order = sp.Order,
                StepTypeCount = sp.StepTypes.Count( m => m.IsActive ),
                StepCompletedCount = completedStepsQry.Count( y => y.StepType.StepProgramId == sp.Id )
            } );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StepProgramListBag> GetGridBuilder()
        {
            return new GridBuilder<StepProgramListBag>()
                .WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "idKey", a => a.Id.ToString() )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "icon", a => a.IconCssClass )
                .AddTextField( "category", a => a.Category )
                .AddField( "order", a => a.Order )
                .AddField( "stepType", a => a.StepTypeCount )
                .AddField( "stepsTaken", a => a.StepCompletedCount );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="idKey">The identifier of the item that will be moved.</param>
        /// <param name="beforeIdKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( int idKey, int? beforeIdKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );

                // Get all step programs
                var allStepPrograms = stepProgramService.Queryable().OrderBy( sp => sp.Order ).ToList();

                // Find the current and new index for the moved item
                int currentIndex = allStepPrograms.FindIndex( sp => sp.Id == idKey );
                int newIndex = beforeIdKey.HasValue ? allStepPrograms.FindIndex( sp => sp.Id == beforeIdKey.Value ) : allStepPrograms.Count - 1;

                // Perform the reordering
                if ( currentIndex != newIndex )
                {
                    stepProgramService.Reorder( allStepPrograms, currentIndex, newIndex );
                    rockContext.SaveChanges();
                }

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
                var entityService = new StepProgramService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{StepProgram.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${StepProgram.FriendlyTypeName}." );
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
