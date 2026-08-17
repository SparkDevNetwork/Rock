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
using Rock.ViewModels.Blocks.Workflow.WorkflowTriggerList;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Displays a list of workflow triggers.
    /// </summary>

    [DisplayName( "Workflow Trigger List" )]
    [Category( "Workflow" )]
    [Description( "Displays a list of workflow triggers." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the workflow trigger details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "dcca7838-dd89-4c66-8554-531cf9b171eb" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "b6ad6421-dfa6-41ac-a097-6738f92cdc1d" )]
    [Rock.SystemGuid.BlockTypeGuid( "72F48121-2CE2-4696-840C-CF404EAF7EEE" )]
    [CustomizedGrid]
    public class WorkflowTriggerList : RockEntityListBlockType<WorkflowTrigger>
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

        private static class PersonPreferenceKey
        {
            public const string FilterIncludeInactive = "filter-include-inactive";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<WorkflowTriggerListOptionsBag>();
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
        private WorkflowTriggerListOptionsBag GetBoxOptions()
        {
            var options = new WorkflowTriggerListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "WorkflowTriggerId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<WorkflowTrigger> GetListQueryable( RockContext rockContext )
        {
            var showInactive = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.FilterIncludeInactive ).AsBoolean();

            var queryable = base.GetListQueryable( rockContext )
                .Include( a => a.EntityType )
                .Include( a => a.WorkflowType );

            if ( showInactive )
            {
                return queryable;
            }

            return queryable.Where( a => a.IsActive == true );
        }

        /// <inheritdoc/>
        protected override IQueryable<WorkflowTrigger> GetOrderedListQueryable( IQueryable<WorkflowTrigger> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.EntityType.Name ).ThenBy( a => a.EntityTypeQualifierColumn ).ThenBy( a => a.EntityTypeQualifierValue );
        }

        /// <inheritdoc/>
        protected override GridBuilder<WorkflowTrigger> GetGridBuilder()
        {
            var grid = new GridBuilder<WorkflowTrigger>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField("entityType", a => a.EntityType.FriendlyName)
                .AddTextField( "triggerType", a => a.WorkflowTriggerType.GetDisplayName())
                .AddTextField( "qualifier", a => a.EntityTypeQualifierColumn)
                .AddTextField( "qualifierValue", a => a.EntityTypeQualifierValue )
                .AddTextField( "qualifierValuePrevious", a => a.EntityTypeQualifierValuePrevious )
                .AddField( "isQualifierValueChange", a => a.WorkflowTriggerValueChangeType == WorkflowTriggerValueChangeType.ChangeFromTo
                        && ( !string.IsNullOrEmpty( a.EntityTypeQualifierValue ) || !string.IsNullOrEmpty( a.EntityTypeQualifierValuePrevious ) ) )
                .AddTextField( "workflow", a => a.WorkflowType.Name )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem );

            return grid;
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
            var entityService = new WorkflowTriggerService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{WorkflowTrigger.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {WorkflowTrigger.FriendlyTypeName}." );
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
    }
}
