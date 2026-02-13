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
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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
    // Was [Rock.SystemGuid.BlockTypeGuid( "5284b259-a9ec-431c-b949-661780bfcd68" )]
    [Rock.SystemGuid.BlockTypeGuid( "429A817E-1379-4BCC-AEFE-01D9C75273E5" )]
    [CustomizedGrid]
    public class StepProgramList : RockEntityListBlockType<StepProgram>
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
        }

        #endregion Keys

        #region Properties

        protected string FilterActive => GetBlockPersonPreferences()
            .GetValue(PreferenceKey.FilterActive);

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
            var options = new StepProgramListOptionsBag()
            {
                // This is based on the old webforms block: bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
                IsReOrderColumnVisible = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
            };

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
        protected override IQueryable<StepProgram> GetListQueryable( RockContext rockContext )
        {
            var query = base.GetListQueryable( rockContext );

            // Filter by Category
            var categoryGuids = GetAttributeValue( AttributeKey.Categories ).SplitDelimitedValues().AsGuidList();
            if ( categoryGuids.Any() )
            {
                query = query.Where( sp => sp.Category != null && categoryGuids.Contains( sp.Category.Guid ) );
            }


            // Filter by isActive
            if ( !string.IsNullOrWhiteSpace( FilterActive ) )
            {
                bool isActive = FilterActive.Equals( "Active", StringComparison.OrdinalIgnoreCase );
                query = ( IOrderedQueryable<StepProgram> ) query.Where( sp => sp.IsActive == isActive );
            }

            return query;
        }

        /// <inheritdoc/>
        protected override List<StepProgram> GetListItems( IQueryable<StepProgram> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();
            return items.Where( sp => sp.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) ).ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<StepProgram> GetOrderedListQueryable( IQueryable<StepProgram> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( sp => sp.Order ).ThenBy( sp => sp.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StepProgram> GetGridBuilder()
        {
            var completedStepsQry = new StepService(  RockContext ).Queryable().Where( x => x.StepStatus != null && x.StepStatus.IsCompleteStatus && x.StepType.IsActive );

            return new GridBuilder<StepProgram>()
                .WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "icon", a => a.IconCssClass )
                .AddTextField( "category", a => a.Category?.Name )
                .AddField( "order", a => a.Order )
                .AddField( "stepType", a => a.StepTypes.Count( m => m.IsActive ) )
                .AddField( "stepsTaken", a => completedStepsQry.Count( y => y.StepType.StepProgramId == a.Id ) )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddField( "isSystem", a => a.IsSystem )
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
            var stepProgramService = new StepProgramService( RockContext );

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
            var entityService = new StepProgramService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{StepProgram.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {StepProgram.FriendlyTypeName}." );
            }

            if ( entity.IsSystem )
            {
                return ActionBadRequest( "You cannot delete a system Step Program." );
            }

            string errorMessage = null;
            RockContext.WrapTransaction( () =>
            {
                var stepTypes = entity.StepTypes.ToList();
                var stepTypeService = new StepTypeService( RockContext );

                foreach ( var stepType in stepTypes )
                {
                    if ( stepType.IsSystem )
                    {
                        errorMessage = $"This program contains the Step Type, '{stepType.Name}', which is a system Step Type and cannot be deleted.";
                        return;
                    }

                    if ( !stepTypeService.CanDelete( stepType, out errorMessage ) )
                    {
                        return;
                    }

                    stepTypeService.Delete( stepType );
                }

                RockContext.SaveChanges();

                if ( !entityService.CanDelete( entity, out errorMessage ) )
                {
                    return;
                }

                entityService.Delete( entity );
                RockContext.SaveChanges();
            } );

            if ( !string.IsNullOrWhiteSpace( errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        #endregion
    }
}
