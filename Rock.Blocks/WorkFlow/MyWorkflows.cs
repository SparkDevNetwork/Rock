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
using Rock.ViewModels.Blocks.Workflow.MyWorkflows;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Displays the workflow types that the user is authorized to view and the
    /// workflows that are currently assigned to (or were initiated by) the user.
    /// </summary>
    [DisplayName( "My Workflows" )]
    [Category( "Workflow" )]
    [Description( "Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user." )]
    [IconCssClass( "ti ti-settings-cog" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CategoryField( "Categories",
        Description = "Optional Categories to limit display to.",
        AllowMultiple = true,
        EntityType = typeof( Rock.Model.WorkflowType ),
        IsRequired = false,
        Key = AttributeKey.Categories,
        Order = 1 )]

    [LinkedPage( "Detail Page",
        Description = "Page used to view status of a workflow.",
        Key = AttributeKey.DetailPage,
        IsRequired = true,
        Order = 2 )]

    [LinkedPage( "Entry Page",
        Description = "Page used to enter form information for a workflow.",
        Key = AttributeKey.EntryPage,
        IsRequired = true,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E4A2EEDC-83B6-4717-8C59-8AE2A956889F" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9dd0104f-99b9-4194-856c-7fb0d72d1d07" )]
    [Rock.SystemGuid.BlockTypeGuid( "689B434F-DD2D-464A-8DA3-21F8768BB5BF" )]
    public class MyWorkflows : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Categories = "Categories";
            public const string DetailPage = "DetailPage";
            public const string EntryPage = "EntryPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntryPage = "EntryPage";
        }

        private static class PageParameterKey
        {
            public const string StatusFilter = "StatusFilter";
            public const string RoleFilter = "RoleFilter";
        }

        private static class PersonPreferenceKey
        {
            public const string RoleToggle = "role-toggle";
            public const string DisplayToggle = "display-toggle";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<MyWorkflowsBag, MyWorkflowsOptionsBag>();

            var (isInitiatedByMe, isActiveTypesOnly) = GetInitialFilterState();

            box.Bag = new MyWorkflowsBag
            {
                IsInitiatedByMe = isInitiatedByMe,
                IsActiveTypesOnly = isActiveTypesOnly
            };
            box.Options = new MyWorkflowsOptionsBag();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Resolves the initial role and display toggle states. Query string values
        /// take precedence over the saved person preferences.
        /// </summary>
        /// <returns>A tuple of the initial role and display toggle states.</returns>
        private (bool IsInitiatedByMe, bool IsActiveTypesOnly) GetInitialFilterState()
        {
            var queryStatusFilter = PageParameter( PageParameterKey.StatusFilter ).AsBooleanOrNull();
            var queryRoleFilter = PageParameter( PageParameterKey.RoleFilter ).AsBooleanOrNull();

            if ( queryStatusFilter.HasValue || queryRoleFilter.HasValue )
            {
                return ( queryRoleFilter.GetValueOrDefault(), queryStatusFilter.GetValueOrDefault() );
            }

            var preferences = GetBlockPersonPreferences();

            return (
                preferences.GetValue( PersonPreferenceKey.RoleToggle ).AsBoolean(),
                preferences.GetValue( PersonPreferenceKey.DisplayToggle ).AsBoolean()
            );
        }

        /// <summary>
        /// Gets the navigation URLs required for the block to operate.
        /// </summary>
        /// <returns>A dictionary of navigation URL keys and values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "WorkflowId", "((Key))" ),
                [NavigationUrlKey.EntryPage] = this.GetLinkedPageUrl( AttributeKey.EntryPage, "WorkflowTypeId", "((workflowTypeKey))" )
            };
        }

        /// <summary>
        /// Gets the active workflow types the current person can view, limited to the
        /// configured categories and ordered by name. Definition data is read from cache.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>The candidate workflow types.</returns>
        private List<WorkflowTypeCache> GetCandidateWorkflowTypes( Person currentPerson )
        {
            var workflowTypes = WorkflowTypeCache.All().Where( t => t.IsActive == true );

            var selectedCategoryGuids = GetAttributeValue( AttributeKey.Categories ).SplitDelimitedValues().AsGuidList();
            if ( selectedCategoryGuids.Any() )
            {
                var selectedCategoryIds = selectedCategoryGuids
                    .Select( g => CategoryCache.GetId( g ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                workflowTypes = workflowTypes.Where( t => t.CategoryId.HasValue && selectedCategoryIds.Contains( t.CategoryId.Value ) );
            }

            return workflowTypes
                .Where( t => t.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( t => t.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the activity types that have a form action and that the current person is
        /// authorized to view. These determine which workflow types are relevant.
        /// </summary>
        /// <param name="workflowTypes">The workflow types to inspect.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>The authorized activity types.</returns>
        private List<WorkflowActivityTypeCache> GetAuthorizedActivityTypes( IEnumerable<WorkflowTypeCache> workflowTypes, Person currentPerson )
        {
            var authorizedActivityTypes = new List<WorkflowActivityTypeCache>();

            foreach ( var workflowType in workflowTypes )
            {
                if ( workflowType.IsActive != true || !workflowType.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                foreach ( var activityType in workflowType.ActivityTypes )
                {
                    var hasFormAction = activityType.ActionTypes.Any( a => a.WorkflowFormId.HasValue );

                    if ( hasFormAction && ( activityType.IsActive ?? true ) && activityType.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        authorizedActivityTypes.Add( activityType );
                    }
                }
            }

            return authorizedActivityTypes;
        }

        /// <summary>
        /// Builds the workflow type tiles, including their badge counts, for the given filters.
        /// </summary>
        /// <param name="isInitiatedByMe">When true, counts the workflows the person initiated; otherwise counts the form actions assigned to the person.</param>
        /// <param name="isActiveTypesOnly">When true, hides relevant types that have no active assignments.</param>
        /// <returns>The list of tiles to display.</returns>
        private List<MyWorkflowsWorkflowTypeBag> GetWorkflowTypeTiles( bool isInitiatedByMe, bool isActiveTypesOnly )
        {
            var person = RequestContext.CurrentPerson;
            var personId = person?.Id ?? 0;

            var candidateTypes = GetCandidateWorkflowTypes( person );

            var authorizedActivityTypes = GetAuthorizedActivityTypes( candidateTypes, person );
            var authorizedActivityTypeIds = authorizedActivityTypes.Select( a => a.Id ).ToList();
            var relevantTypeIds = authorizedActivityTypes.Select( a => a.WorkflowTypeId ).Distinct().ToList();

            // Only types that contain at least one authorized form activity type are relevant.
            var relevantTypes = candidateTypes.Where( t => relevantTypeIds.Contains( t.Id ) ).ToList();
            if ( !relevantTypes.Any() )
            {
                return new List<MyWorkflowsWorkflowTypeBag>();
            }

            var counts = isInitiatedByMe
                ? GetInitiatedByMeCounts( personId, relevantTypeIds )
                : GetAssignedToMeCounts( personId, authorizedActivityTypeIds );

            var tiles = new List<MyWorkflowsWorkflowTypeBag>();

            foreach ( var workflowType in relevantTypes )
            {
                counts.TryGetValue( workflowType.Id, out var count );

                // Always show types with active assignments; otherwise show editable
                // types only when not limiting the display to active types.
                var isShown = count > 0
                    || ( !isActiveTypesOnly && workflowType.IsAuthorized( Authorization.EDIT, person ) );

                if ( isShown )
                {
                    tiles.Add( new MyWorkflowsWorkflowTypeBag
                    {
                        Guid = workflowType.Guid,
                        Name = workflowType.Name,
                        IconCssClass = workflowType.IconCssClass,
                        Count = count
                    } );
                }
            }

            return tiles;
        }

        /// <summary>
        /// Gets the count of active workflows initiated by the person, grouped by workflow type.
        /// </summary>
        /// <param name="personId">The current person's identifier.</param>
        /// <param name="workflowTypeIds">The workflow type identifiers to include.</param>
        /// <returns>A dictionary keyed by workflow type id with the workflow count.</returns>
        private Dictionary<int, int> GetInitiatedByMeCounts( int personId, List<int> workflowTypeIds )
        {
            return new WorkflowService( RockContext ).Queryable()
                .Where( w =>
                    w.ActivatedDateTime.HasValue &&
                    !w.CompletedDateTime.HasValue &&
                    w.InitiatorPersonAlias.PersonId == personId &&
                    workflowTypeIds.Contains( w.WorkflowTypeId ) )
                .GroupBy( w => w.WorkflowTypeId )
                .Select( g => new { WorkflowTypeId = g.Key, Count = g.Count() } )
                .ToDictionary( x => x.WorkflowTypeId, x => x.Count );
        }

        /// <summary>
        /// Gets the count of active form actions assigned to the person (directly or via a group),
        /// grouped by workflow type.
        /// </summary>
        /// <param name="personId">The current person's identifier.</param>
        /// <param name="authorizedActivityTypeIds">The authorized activity type identifiers to include.</param>
        /// <returns>A dictionary keyed by workflow type id with the active form action count.</returns>
        private Dictionary<int, int> GetAssignedToMeCounts( int personId, List<int> authorizedActivityTypeIds )
        {
            return new WorkflowActionService( RockContext ).Queryable()
                .Where( a =>
                    a.ActionType.WorkflowFormId.HasValue &&
                    !a.CompletedDateTime.HasValue &&
                    a.Activity.ActivatedDateTime.HasValue && !a.Activity.CompletedDateTime.HasValue &&
                    a.Activity.Workflow.ActivatedDateTime.HasValue && !a.Activity.Workflow.CompletedDateTime.HasValue &&
                    authorizedActivityTypeIds.Contains( a.ActionType.ActivityTypeId ) &&
                    (
                        ( a.Activity.AssignedPersonAlias != null && a.Activity.AssignedPersonAlias.PersonId == personId ) ||
                        ( a.Activity.AssignedGroup != null && a.Activity.AssignedGroup.Members.Any( m => m.PersonId == personId && m.GroupMemberStatus != GroupMemberStatus.Inactive ) )
                    ) )
                .GroupBy( a => a.Activity.Workflow.WorkflowTypeId )
                .Select( g => new { WorkflowTypeId = g.Key, Count = g.Count() } )
                .ToDictionary( x => x.WorkflowTypeId, x => x.Count );
        }

        /// <summary>
        /// Gets the workflows of the selected type for the grid, matching the role filter.
        /// Activities are eager-loaded so the active activity names can be computed without extra queries.
        /// </summary>
        /// <param name="workflowType">The selected workflow type.</param>
        /// <param name="isInitiatedByMe">When true, returns the workflows the person initiated; otherwise the workflows with a form action assigned to the person.</param>
        /// <returns>The workflows to display in the grid.</returns>
        private List<Rock.Model.Workflow> GetGridWorkflows( WorkflowTypeCache workflowType, bool isInitiatedByMe )
        {
            var personId = RequestContext.CurrentPerson?.Id ?? 0;
            var workflowService = new WorkflowService( RockContext );

            if ( isInitiatedByMe )
            {
                return workflowService.Queryable()
                    .Include( w => w.Activities )
                    .Where( w =>
                        w.WorkflowTypeId == workflowType.Id &&
                        w.ActivatedDateTime.HasValue &&
                        !w.CompletedDateTime.HasValue &&
                        w.InitiatorPersonAlias.PersonId == personId )
                    .OrderByDescending( w => w.CreatedDateTime )
                    .ToList();
            }

            var authorizedActivityTypeIds = GetAuthorizedActivityTypes( new[] { workflowType }, RequestContext.CurrentPerson )
                .Select( a => a.Id )
                .ToList();

            var workflowIds = new WorkflowActionService( RockContext ).Queryable()
                .Where( a =>
                    a.ActionType.WorkflowFormId.HasValue &&
                    !a.CompletedDateTime.HasValue &&
                    a.Activity.ActivatedDateTime.HasValue && !a.Activity.CompletedDateTime.HasValue &&
                    a.Activity.Workflow.ActivatedDateTime.HasValue && !a.Activity.Workflow.CompletedDateTime.HasValue &&
                    a.Activity.Workflow.WorkflowTypeId == workflowType.Id &&
                    authorizedActivityTypeIds.Contains( a.ActionType.ActivityTypeId ) &&
                    (
                        ( a.Activity.AssignedPersonAlias != null && a.Activity.AssignedPersonAlias.PersonId == personId ) ||
                        ( a.Activity.AssignedGroup != null && a.Activity.AssignedGroup.Members.Any( m => m.PersonId == personId && m.GroupMemberStatus != GroupMemberStatus.Inactive ) )
                    ) )
                .Select( a => a.Activity.WorkflowId )
                .Distinct()
                .ToList();

            return workflowService.Queryable()
                .Include( w => w.Activities )
                .Where( w => workflowIds.Contains( w.Id ) )
                .OrderByDescending( w => w.CreatedDateTime )
                .ToList();
        }

        /// <summary>
        /// Gets the grid column attributes (IsGridColumn) qualified to the workflow type.
        /// </summary>
        /// <param name="workflowType">The selected workflow type.</param>
        /// <returns>The attributes to render as grid columns.</returns>
        private List<AttributeCache> GetGridAttributes( WorkflowTypeCache workflowType )
        {
            var entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Workflow ) ).Id;
            var qualifier = workflowType.Id.ToString();

            return AttributeCache.All().AsQueryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn &&
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifier ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Builds the grid for the selected workflow type. Columns are dynamic based on the type's grid attributes.
        /// </summary>
        /// <param name="gridAttributes">The attribute columns to include.</param>
        /// <returns>A configured grid builder.</returns>
        private GridBuilder<Rock.Model.Workflow> GetGridBuilder( List<AttributeCache> gridAttributes )
        {
            return new GridBuilder<Rock.Model.Workflow>()
                .WithBlock( this )
                .AddTextField( "idKey", w => w.IdKey )
                .AddField( "guid", w => w.Guid )
                .AddTextField( "name", w => w.Name )
                .AddTextField( "status", w => w.Status )
                .AddField( "activeActivities", w => w.Activities
                    .Where( wa => wa.ActivatedDateTime.HasValue && !wa.CompletedDateTime.HasValue && wa.ActivityTypeCache != null && wa.ActivityTypeCache.IsActive != false )
                    .OrderBy( wa => wa.ActivityTypeCache.Order )
                    .Select( wa => wa.ActivityTypeCache.Name ) )
                .AddAttributeFields( gridAttributes );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the workflow type tiles for the current filters.
        /// </summary>
        /// <param name="isInitiatedByMe">When true, the role filter is "Initiated By Me"; otherwise "Assigned To Me".</param>
        /// <param name="isActiveTypesOnly">When true, the display filter is limited to "Active Types".</param>
        /// <returns>The list of workflow type tiles.</returns>
        [BlockAction]
        public BlockActionResult GetWorkflowTypes( bool isInitiatedByMe, bool isActiveTypesOnly )
        {
            return ActionOk( GetWorkflowTypeTiles( isInitiatedByMe, isActiveTypesOnly ) );
        }

        /// <summary>
        /// Gets the grid data for the selected workflow type.
        /// </summary>
        /// <param name="request">The request containing the selected workflow type and role filter.</param>
        /// <returns>The grid data response including definition, data, and the selected type details.</returns>
        [BlockAction]
        public BlockActionResult GetGridData( MyWorkflowsGetGridDataRequestBag request )
        {
            if ( request?.WorkflowTypeGuid == null )
            {
                return ActionBadRequest( "A workflow type must be selected." );
            }

            var workflowType = WorkflowTypeCache.Get( request.WorkflowTypeGuid.Value );
            if ( workflowType == null )
            {
                return ActionNotFound( "The selected workflow type was not found." );
            }

            if ( workflowType.IsActive != true || !workflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to view this workflow type." );
            }

            var workflows = GetGridWorkflows( workflowType, request.IsInitiatedByMe );

            var gridAttributes = GetGridAttributes( workflowType );

            GridAttributeLoader.LoadFor( workflows, w => w, gridAttributes, RockContext );

            var builder = GetGridBuilder( gridAttributes );

            return ActionOk( new MyWorkflowsGetGridDataResponseBag
            {
                GridData = builder.Build( workflows ),
                GridDefinition = builder.BuildDefinition(),
                WorkflowTypeName = workflowType.Name,
                WorkflowTypeIdKey = workflowType.IdKey
            } );
        }

        #endregion Block Actions
    }
}
