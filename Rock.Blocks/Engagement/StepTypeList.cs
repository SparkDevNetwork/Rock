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
using Rock.ViewModels.Blocks.Engagement.StepTypeList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of step types.
    /// </summary>
    [DisplayName( "Step Type List" )]
    [Category( "Steps" )]
    [Description( "Shows a list of all step types for a program." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "f3a7b501-61c4-4784-8f73-958e2f1fc353" )]
    // Was [Rock.SystemGuid.BlockTypeGuid( "6a7c7c71-4760-4e6c-9d6f-6926c81caf8f" )]
    [Rock.SystemGuid.BlockTypeGuid( "3EFB4302-9AB4-420F-A818-48B1B06AD109" )]
    [CustomizedGrid]
    [ContextAware( typeof( Campus ) )]
    public class StepTypeList : RockListBlockType<StepTypeList.StepTypeWithCounts>
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

        /// <summary>
        /// The StepType attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

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
        /// </summary>
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
                [NavigationUrlKey.BulkEntryPage] = this.GetLinkedPageUrl( AttributeKey.BulkEntryPage, PageParameterKey.StepTypeId, "((Key))" ),
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StepTypeWithCounts> GetListQueryable( RockContext rockContext )
        {
            var stepProgram = GetStepProgram();
            var stepTypeService = new StepTypeService( rockContext );
            var stepTypeQueryable = stepTypeService
                .Queryable()
                .Include( st => st.OrganizationalObjectiveValue );

            // Filter by: Step Program
            if ( stepProgram != null )
            {
                stepTypeQueryable = stepTypeQueryable.Where( x => x.StepProgramId == stepProgram.Id );
            }

            // Filter by: Active
            switch ( FilterActiveStatus.ToUpperInvariant() )
            {
                case "ACTIVE":
                    stepTypeQueryable = stepTypeQueryable.Where( a => a.IsActive );
                    break;
                case "INACTIVE":
                    stepTypeQueryable = stepTypeQueryable.Where( a => !a.IsActive );
                    break;
            }

            var stepQueryable = new StepService( rockContext ).Queryable().AsNoTracking();

            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                stepQueryable = stepQueryable.Where( s => s.CampusId == campusContext.Id );
            }

            var queryable = stepTypeQueryable
                .GroupJoin(
                    stepQueryable,
                    st => st.Id,
                    s => s.StepTypeId,
                    ( st, steps ) => new StepTypeWithCounts
                    {
                        StepType = st,
                        StartedCount = steps.Count(),
                        CompletedCount = steps.Count( s => s.StepStatus != null && s.StepStatus.IsCompleteStatus )
                    } );

            return queryable;
        }

        /// <inheritdoc/>
        protected override List<StepTypeWithCounts> GetListItems( IQueryable<StepTypeWithCounts> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();
            return items.Where( st => st.StepType.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) ).ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<StepTypeWithCounts> GetOrderedListQueryable( IQueryable<StepTypeWithCounts> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( b => b.StepType.Order ).ThenBy( b => b.StepType.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StepTypeWithCounts> GetGridBuilder()
        {
            return new GridBuilder<StepTypeWithCounts>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.StepType.IdKey )
                .AddTextField( "iconCssClass", a => a.StepType.IconCssClass )
                .AddTextField( "name", a => a.StepType.Name )
                .AddField( "engagementType", a => a.StepType.EngagementType )
                .AddField( "impactWeight", a => a.StepType.ImpactWeight )
                .AddField( "organizationalObjective", a => a.StepType.OrganizationalObjectiveValue?.Value )
                .AddField( "hasEndDate", a => a.StepType.HasEndDate )
                .AddField( "allowMultiple", a => a.StepType.AllowMultiple )
                .AddField( "startedCount", a => a.StartedCount )
                .AddField( "completedCount", a => a.CompletedCount )
                .AddField( "isSecurityDisabled", _ => !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddField( "isSystem", a => a.StepType.IsSystem )
                .AddAttributeFieldsFrom( a => a.StepType, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<StepType>( false )?.Id;
            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }
            return new List<AttributeCache>();
        }

        /// <summary>
        /// Gets the step program context.
        /// </summary>
        /// <returns></returns>
        public StepProgramCache GetStepProgram()
        {
            var stepProgramGuid = GetAttributeValue( AttributeKey.StepProgram ).AsGuidOrNull();

            if ( stepProgramGuid.HasValue )
            {
                // Try to load the Step Program from the cache.
                return StepProgramCache.Get( stepProgramGuid.Value );
            }

            return StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Makes the key unique to the current step program.
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
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );
            var stepTypes = items.Select( i => i.StepType ).ToList();
            if ( !stepTypes.ReorderEntity( key, beforeKey ) )
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
            var entityService = new StepTypeService( RockContext );
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
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Helper Classes
        public class StepTypeWithCounts
        {
            public StepType StepType { get; set; }
            public int StartedCount { get; set; }
            public int CompletedCount { get; set; }
        }

        #endregion
    }
}
