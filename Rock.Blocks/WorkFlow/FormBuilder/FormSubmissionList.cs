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
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormSubmissionList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Lists the submissions captured for a single Form Builder form.
    /// </summary>
    [DisplayName( "Form Submission List" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows a list of submissions captured for a Form Builder form." )]
    [IconCssClass( "ti ti-notes" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Description = "Page used to display a workflow submission's details.",
        Order = 0,
        Key = AttributeKey.DetailPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL )]

    [LinkedPage(
        "Entry Page",
        Description = "Page used to launch a new workflow of the form's underlying workflow type.",
        Order = 1,
        Key = AttributeKey.EntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E480E57F-8813-4221-BC23-17A94417BFE7" )]
    [Rock.SystemGuid.BlockTypeGuid( "A23592BB-25F7-4A81-90CD-46700724110A" )]
    [CustomizedGrid]
    public class FormSubmissionList : RockEntityListBlockType<Rock.Model.Workflow>
    {
        #region Keys

        private static class AttributeKey
        {
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
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string WorkflowId = "WorkflowId";
        }

        private static class PreferenceKey
        {
            public const string FilterPersonAliasGuid = "filter-person-alias-guid";
            public const string FilterCampusGuid = "filter-campus-guid";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// PersonAlias Guid for the initiator filter, scoped to the current workflow type.
        /// </summary>
        private Guid? FilterPersonAliasGuid => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.FilterPersonAliasGuid ) )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Campus Guid for the campus filter, scoped to the current workflow type.
        /// </summary>
        private Guid? FilterCampusGuid => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.FilterCampusGuid ) )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FormSubmissionListOptionsBag>();
            var builder = GetGridBuilder();

            var canEdit = GetCanEditWorkflowType();

            box.IsAddEnabled = canEdit;
            box.IsDeleteEnabled = canEdit;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Builds the options bag for the initial block render.
        /// </summary>
        private FormSubmissionListOptionsBag GetBoxOptions()
        {
            var workflowType = GetWorkflowType();

            return new FormSubmissionListOptionsBag
            {
                CanView = workflowType != null,
                IsGridVisible = workflowType != null,
                FormName = workflowType != null ? $"{workflowType.Name} Form" : string.Empty,
                WorkflowTypeIdKey = workflowType?.IdKey
            };
        }

        /// <summary>
        /// Builds the navigation URL dictionary used for row clicks and the Add button.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.WorkflowId, "((Key))" ),
                [NavigationUrlKey.EntryPage] = this.GetLinkedPageUrl( AttributeKey.EntryPage, PageParameterKey.WorkflowTypeId, "((WorkflowTypeKey))" )
            };
        }

        /// <summary>
        /// Returns the WorkflowType resolved from the WorkflowTypeId page parameter, or null if not found.
        /// </summary>
        private WorkflowTypeCache GetWorkflowType()
        {
            return WorkflowTypeCache.Get( PageParameter( PageParameterKey.WorkflowTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Determines whether the current person can edit the workflow type, falling back to
        /// category-level security when no explicit rule is set. Gates Add and Delete on the grid.
        /// </summary>
        private bool GetCanEditWorkflowType()
        {
            var workflowType = GetWorkflowType();

            if ( workflowType == null )
            {
                return false;
            }

            var explicitAuth = Authorization.AuthorizedForEntity( workflowType, Authorization.EDIT, GetCurrentPerson(), false );
            if ( explicitAuth.HasValue )
            {
                return explicitAuth.Value;
            }

            var category = workflowType.CategoryId.HasValue ? CategoryCache.Get( workflowType.CategoryId.Value ) : null;
            return category != null && category.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Workflow> GetListQueryable( RockContext rockContext )
        {
            var workflowType = GetWorkflowType();
            if ( workflowType == null )
            {
                return Enumerable.Empty<Rock.Model.Workflow>().AsQueryable();
            }

            var workflows = new WorkflowService( rockContext )
                .Queryable( "Campus,InitiatorPersonAlias.Person" )
                .AsNoTracking()
                .Where( w => w.WorkflowTypeId == workflowType.Id );

            var personAliasGuid = FilterPersonAliasGuid;
            if ( personAliasGuid.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
                if ( personAlias != null )
                {
                    workflows = workflows.Where( w => w.InitiatorPersonAliasId == personAlias.Id );
                }
            }

            var campusGuid = FilterCampusGuid;
            if ( campusGuid.HasValue )
            {
                var campus = CampusCache.Get( campusGuid.Value );
                if ( campus != null )
                {
                    workflows = workflows.Where( w => w.CampusId == campus.Id );
                }
            }

            return workflows;
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Workflow> GetOrderedListQueryable( IQueryable<Rock.Model.Workflow> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( w => w.ActivatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Rock.Model.Workflow> GetGridBuilder()
        {
            return new GridBuilder<Rock.Model.Workflow>()
                .WithBlock( this )
                .AddTextField( "idKey", w => w.IdKey )
                .AddDateTimeField( "submitted", w => w.ActivatedDateTime )
                .AddTextField( "campus", w => w.Campus != null ? w.Campus.Name : string.Empty )
                .AddPersonField( "initiator", w => w.InitiatorPersonAlias?.Person )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Headers reserved by built-in grid columns. Attribute columns whose names collide
        /// with these are filtered out to avoid duplicate headers (e.g. a form with a Person
        /// attribute named "Person" would otherwise render alongside the initiator column).
        /// </summary>
        private static readonly HashSet<string> ReservedColumnNames = new HashSet<string>(
            new[] { "Submitted", "Campus", "Person" },
            StringComparer.OrdinalIgnoreCase );

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            var availableAttributes = new List<AttributeCache>();
            var workflowType = GetWorkflowType();

            if ( workflowType == null )
            {
                return availableAttributes;
            }

            var entityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>() ?? 0;
            var workflowQualifier = workflowType.Id.ToString();

            foreach ( var attributeModel in new AttributeService( RockContext ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId
                    && a.IsGridColumn
                    && a.EntityTypeQualifierColumn == "WorkflowTypeId"
                    && a.EntityTypeQualifierValue == workflowQualifier )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                if ( ReservedColumnNames.Contains( attributeModel.Name ) )
                {
                    continue;
                }

                availableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }

            return availableAttributes;
        }

        /// <summary>
        /// Scopes a preference key to the current workflow type so filters do not bleed
        /// across forms when the user navigates between them.
        /// </summary>
        private string MakeKeyUniqueToWorkflowType( string key )
        {
            var workflowType = GetWorkflowType();
            return workflowType != null ? $"{workflowType.IdKey}-{key}" : key;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified submission. Requires edit authority on the workflow type.
        /// </summary>
        /// <param name="key">The IdKey of the workflow to delete.</param>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !GetCanEditWorkflowType() )
            {
                return ActionBadRequest( "You are not authorized to delete this submission." );
            }

            var workflowService = new WorkflowService( RockContext );
            var workflow = workflowService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( workflow == null )
            {
                return ActionBadRequest( $"{Rock.Model.Workflow.FriendlyTypeName} not found." );
            }

            if ( !workflowService.CanDelete( workflow, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            workflowService.Delete( workflow );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
