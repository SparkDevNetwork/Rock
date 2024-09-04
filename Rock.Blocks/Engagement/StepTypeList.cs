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
using Rock.ViewModels.Blocks.Engagement.StepTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of step types.
    /// </summary>
    [DisplayName( "Step Type List" )]
    [Category( "Steps" )]
    [Description( "Shows a list of all step types for a program." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [StepProgramField(
        "Step Program",
        Key = AttributeKey.StepProgram,
        Description = "Display Step Types from a specified program. If none selected, the block will display the program from the current context.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 2 )]

    [LinkedPage(
        "Bulk Entry",
        Key = AttributeKey.BulkEntryPage,
        Description = "Linked page that allows for bulk entry of steps for a step type.",
        Category = AttributeCategory.LinkedPages,
        Order = 3 )]

    [Rock.SystemGuid.EntityTypeGuid( "f3a7b501-61c4-4784-8f73-958e2f1fc353" )]
    [Rock.SystemGuid.BlockTypeGuid( "6a7c7c71-4760-4e6c-9d6f-6926c81caf8f" )]
    [CustomizedGrid]
    public class StepTypeList : RockEntityListBlockType<StepType>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string StepProgram = "Programs";
            public const string DetailPage = "DetailPage";
            public const string BulkEntryPage = "BulkEntryPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string BulkEntryPage = "BulkEntryPage";
        }

        private static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        private static class PageParameterKey
        {
            public const string StepProgramId = "ProgramId";
            public const string StepTypeId = "StepTypeId";
        }

        private static class PreferenceKey
        {
            public const string FilterActiveStatus = "filter-active-status";
        }

        #endregion Keys

        #region Fields

        private StepProgram _stepProgram;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the IsActive status filter.
        /// </summary>
        /// <value>
        /// The IsActive status filter.
        /// </value>
        protected string FilterActiveStatus => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToStepProgram( PreferenceKey.FilterActiveStatus ) );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StepTypeListOptionsBag>();
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
        private StepTypeListOptionsBag GetBoxOptions()
        {
            var stepProgram = GetStepProgram();
            var currentPerson = GetCurrentPerson();

            var options = new StepTypeListOptionsBag()
            {
                CanEdit = GetIsAddDeleteEnabled(),
                IsSecurityColumnVisible = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ),
                IsAuthorizedToViewProgram = stepProgram?.IsAuthorized( Authorization.VIEW, currentPerson ) == true,
                IsBlockVisible = stepProgram != null,
                StepProgramIdKey = stepProgram?.IdKey,
                IsBulkEntryColumnVisible = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ) && GetAttributeValue( AttributeKey.BulkEntryPage ).IsNotNullOrWhiteSpace()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            var stepProgram = GetStepProgram();
            return stepProgram?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string> { [PageParameterKey.StepTypeId] = "((Key))" };

            var stepProgram = GetStepProgram();
            if ( stepProgram != null )
            {
                queryParams[PageParameterKey.StepProgramId] = stepProgram.IdKey;
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams ),
                [NavigationUrlKey.BulkEntryPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.StepTypeId, "((Key))" ),
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StepType> GetListQueryable( RockContext rockContext )
        {
            var queryable = new StepTypeService( rockContext ).Queryable();
            var stepProgramId = GetStepProgram()?.Id ?? 0;

            // Filter by: Step Program
            queryable = queryable.Where( x => x.StepProgramId == stepProgramId );

            // Filter by: Active
            switch ( FilterActiveStatus.ToUpperInvariant() )
            {
                case "ACTIVE":
                    return queryable.Where( a => a.IsActive );
                case "INACTIVE":
                    return queryable.Where( a => !a.IsActive );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<StepType> GetOrderedListQueryable( IQueryable<StepType> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( b => b.Order ).ThenBy( b => b.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StepType> GetGridBuilder()
        {
            // Retrieve the Step Type data models and create corresponding view models to display in the grid.
            var stepService = new StepService( RockContext );

            var startedStepsQry = stepService.Queryable();
            var completedStepsQry = stepService.Queryable().Where( x => x.StepStatus != null && x.StepStatus.IsCompleteStatus );

            // Filter by CampusId
            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                startedStepsQry = startedStepsQry.Where( s => s.CampusId == campusContext.Id );
                completedStepsQry = completedStepsQry.Where( s => s.CampusId == campusContext.Id );
            }

            return new GridBuilder<StepType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "iconCssClass", a => a.IconCssClass )
                .AddTextField( "name", a => a.Name )
                .AddField( "hasEndDate", a => a.HasEndDate )
                .AddField( "allowMultiple", a => a.AllowMultiple )
                .AddField( "startedCount", a => startedStepsQry.Count( s => s.StepTypeId == a.Id ) )
                .AddField( "completedCount", a => completedStepsQry.Count( s => s.StepTypeId == a.Id ) )
                .AddField( "isSecurityDisabled", _ => !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the step program context.
        /// </summary>
        /// <returns></returns>
        public StepProgram GetStepProgram()
        {
            if ( _stepProgram == null )
            {
                // Try to load the Step Program from the cache.
                var programGuid = GetAttributeValue( AttributeKey.StepProgram ).AsGuid();

                int programId = 0;

                // If a Step Program is specified in the block settings use it, otherwise use the PageParameters.
                if ( programGuid == Guid.Empty )
                {
                    programId = PageParameter( PageParameterKey.StepProgramId ).AsInteger();
                }

                var stepProgramService = new StepProgramService( RockContext );

                if ( programGuid != Guid.Empty )
                {
                    _stepProgram = stepProgramService.Queryable().Where( g => g.Guid == programGuid ).FirstOrDefault();
                }
                else if ( programId != 0 )
                {
                    _stepProgram = stepProgramService.Queryable().Where( g => g.Id == programId ).FirstOrDefault();
                }
            }

            return _stepProgram;
        }

        /// <summary>
        /// Makes the key unique to the current event calendar.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToStepProgram( string key )
        {
            var stepProgram = GetStepProgram();

            if ( stepProgram != null )
            {
                return $"{stepProgram.IdKey}-{key}";
            }

            return key;
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
                var entityService = new StepTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{StepType.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {StepType.FriendlyTypeName}." );
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
