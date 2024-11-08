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
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Workflow.WorkflowList;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Displays a list of workflows.
    /// </summary>
    [DisplayName( "Workflow List" )]
    [Category( "Workflow" )]
    [Description( "Lists all the workflows." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Key = AttributeKey.EntryPage )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the workflow details.",
        Key = AttributeKey.DetailPage )]

    [WorkflowTypeField( "Default WorkflowType",
        Description = "The default workflow type to use. If provided the query string will be ignored.",
        Key = AttributeKey.DefaultWorkflowType )]

    [Rock.SystemGuid.EntityTypeGuid( "1208bfdd-18cf-4539-b36b-9744b10d7635" )]
    [Rock.SystemGuid.BlockTypeGuid( "ea76c61f-aa94-4e8b-b105-1effc0fea59a" )]
    [CustomizedGrid]
    public class WorkflowList : RockEntityListBlockType<Rock.Model.Workflow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntryPage = "EntryPage";
            public const string DefaultWorkflowType = "DefaultWorkflowType";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntryPage = "EntryPage";
        }

        private static class PageParameterKey
        {
            public const string WorkflowTypeId = "WorkflowTypeId";
        }

        private static class PreferenceKey
        {
            public const string FilterActivatedDateRangeUpperValue = "filter-activated-date-range-upper-value";

            public const string FilterActivatedDateRangeLowerValue = "filter-activated-date-range-lower-value";

            public const string FilterCompletedDateRangeUpperValue = "filter-completed-date-range-upper-value";

            public const string FilterCompletedDateRangeLowerValue = "filter-completed-date-range-lower-value";
        }

        #endregion Keys

        #region Fields

        private WorkflowType _workflowType;

        #endregion

        #region Properties

        protected DateTime? FilterActivatedDateRangeUpperValue => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.FilterActivatedDateRangeUpperValue ) )
            .AsDateTime();

        protected DateTime? FilterActivatedDateRangeLowerValue => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.FilterActivatedDateRangeLowerValue ) )
            .AsDateTime();

        protected DateTime? FilterCompletedDateRangeUpperValue => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.FilterCompletedDateRangeUpperValue ) )
            .AsDateTime();

        protected DateTime? FilterCompletedDateRangeLowerValue => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.FilterCompletedDateRangeLowerValue ) )
            .AsDateTime();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<WorkflowListOptionsBag>();
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
        private WorkflowListOptionsBag GetBoxOptions()
        {
            var workflowType = GetWorkflowType();
            var options = new WorkflowListOptionsBag()
            {
                CanView = GetCanView(),
                IsWorkflowIdColumnVisible = workflowType?.WorkflowIdPrefix.IsNotNullOrWhiteSpace() == true,
                IsGridVisible = GetCanView() && workflowType != null,
                ItemTerm = workflowType?.WorkTerm,
                WorkflowTypeIdKey = workflowType?.IdKey,
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            var workflowType = GetWorkflowType();
            return BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) || workflowType.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );
        }

        private bool GetCanView()
        {
            var workflowType = GetWorkflowType();
            return workflowType != null && ( GetIsAddDeleteEnabled()
                || ( workflowType.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) && workflowType.IsAuthorized( "ViewList", GetCurrentPerson() ) ) );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "WorkflowId", "((Key))" ),
                [NavigationUrlKey.EntryPage] = this.GetLinkedPageUrl( AttributeKey.EntryPage, "WorkflowTypeId", "((workflowTypeKey))" ),
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Workflow> GetListQueryable( RockContext rockContext )
        {
            IEnumerable<Rock.Model.Workflow> workflows = new List<Rock.Model.Workflow>().AsQueryable();

            if ( GetCanView() && _workflowType != null )
            {
                var workflowType = GetWorkflowType();
                var workflowService = new WorkflowService( rockContext );

                workflows = workflowService
                    .Queryable( "Activities.ActivityType,InitiatorPersonAlias.Person" ).AsNoTracking()
                    .Where( w => w.WorkflowTypeId.Equals( workflowType.Id ) );

                // Activated Date Range Filter
                if ( FilterActivatedDateRangeLowerValue.HasValue )
                {
                    workflows = workflows.Where( w => w.ActivatedDateTime >= FilterActivatedDateRangeLowerValue.Value );
                }
                if ( FilterActivatedDateRangeUpperValue.HasValue )
                {
                    DateTime upperDate = FilterActivatedDateRangeUpperValue.Value.Date.AddDays( 1 );
                    workflows = workflows.Where( w => w.ActivatedDateTime.Value < upperDate );
                }

                // Completed Date Range Filter
                if ( FilterCompletedDateRangeLowerValue.HasValue )
                {
                    workflows = workflows.Where( w => w.CompletedDateTime.HasValue && w.CompletedDateTime.Value >= FilterCompletedDateRangeLowerValue.Value );
                }
                if ( FilterCompletedDateRangeUpperValue.HasValue )
                {
                    DateTime upperDate = FilterCompletedDateRangeUpperValue.Value.Date.AddDays( 1 );
                    workflows = workflows.Where( w => w.CompletedDateTime.HasValue && w.CompletedDateTime.Value < upperDate );
                }
            }

            return workflows.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<Model.Workflow> GetOrderedListQueryable( IQueryable<Model.Workflow> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( s => s.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Rock.Model.Workflow> GetGridBuilder()
        {
            return new GridBuilder<Rock.Model.Workflow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "workflowId", a => a.WorkflowId )
                .AddTextField( "name", a => a.Name )
                .AddPersonField( "initiator", a => a.InitiatorPersonAlias?.Person )
                .AddField( "activities", a => a.Activities.Where( wa => wa.ActivatedDateTime.HasValue && !wa.CompletedDateTime.HasValue ).OrderBy( wa => wa.ActivityType.Order ).Select( wa => wa.ActivityType.Name ) )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddTextField( "status", a => a.Status )
                .AddField( "isCompleted", a => a.CompletedDateTime.HasValue )
                .AddField( "guid", a => a.Guid )
                .AddField( "workflowTypeIdKey", a => a.WorkflowType.IdKey )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the type of the workflow.
        /// </summary>
        /// <returns></returns>
        private WorkflowType GetWorkflowType()
        {
            if ( _workflowType == null )
            {
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DefaultWorkflowType ) ) )
                {
                    Guid.TryParse( GetAttributeValue( AttributeKey.DefaultWorkflowType ), out Guid workflowTypeGuid );
                    _workflowType = new WorkflowTypeService( RockContext ).Get( workflowTypeGuid );
                }
                else
                {
                    var workflowTypeId = PageParameter( PageParameterKey.WorkflowTypeId ).AsInteger();
                    _workflowType = new WorkflowTypeService( RockContext ).Get( workflowTypeId );
                }
            }
            return _workflowType;
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            var availableAttributes = new List<AttributeCache>();
            var workflowType = GetWorkflowType();

            if ( workflowType != null )
            {
                int entityTypeId = new Rock.Model.Workflow().TypeId;
                string workflowQualifier = workflowType.Id.ToString();
                foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( workflowQualifier ) )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    availableAttributes.Add( AttributeCache.Get( attributeModel ) );
                }
            }

            return availableAttributes;
        }

        /// <summary>
        /// Makes the key unique to the current workflow type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToWorkflowType( string key )
        {
            var workflowType = GetWorkflowType();

            if ( workflowType != null )
            {
                return $"{workflowType.IdKey}-{key}";
            }

            return key;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var workflowType = GetWorkflowType();
            var breadCrumbs = new List<IBreadCrumb>();

            if ( workflowType != null )
            {
                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
                breadCrumbs.Add( new BreadCrumbLink( workflowType.Name, breadCrumbPageRef ) );
            }

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
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
            var entityService = new WorkflowService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Rock.Model.Workflow.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Rock.Model.Workflow.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified workflows.
        /// </summary>
        /// <param name="workflowGuids">The key.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult DeleteWorkflows( List<Guid> workflowGuids )
        {
            var entityService = new WorkflowService( RockContext );
            var workflowIds = entityService.GetByGuids( workflowGuids ).Select( wf => wf.Id ).ToList();

            var deleteWorkflowsMsg = new DeleteWorkflows.Message
            {
                WorkflowIds = workflowIds
            };

            deleteWorkflowsMsg.Send();

            return ActionOk();
        }

        #endregion
    }
}
