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
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.WorkFlow
{
    /// <summary>
    /// Displays the active workflows assigned to or initiated by the current person, rendered with a configurable Lava template.
    /// </summary>
    [DisplayName( "My Workflows Lava" )]
    [Category( "WorkFlow" )]
    [Description( "Block to display active workflow activities assigned to the current user that have a form entry action. The display format is controlled by a lava template." )]
    [SupportedSiteTypes( SiteType.Web )]
    [ConfigurationChangedReload( Rock.Enums.Cms.BlockReloadMode.Page )]

    #region Block Attributes

    [CustomRadioListField( "Role",
        Description = "Display the active workflows that the current user Initiated, or is currently Assigned To.",
        ListSource = "0^Assigned To,1^Initiated",
        IsRequired = true,
        DefaultValue = RoleValue.AssignedTo,
        Key = AttributeKey.Role,
        Order = 0 )]

    [CategoryField( "Categories",
        Description = "Optional categories to limit display to.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.WorkflowType",
        IsRequired = false,
        Key = AttributeKey.Categories,
        Order = 1 )]

    [BooleanField( "Include Child Categories",
        Description = "Should descendent categories of the selected Categories be included?",
        DefaultBooleanValue = true,
        Key = AttributeKey.IncludeChildCategories,
        Order = 2 )]

    [CodeEditorField( "Contents",
        Description = "The Lava template to use for displaying activities assigned to current user.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = "{% include '/Assets/Lava/MyWorkflowsSortable.lava' %}",
        Key = AttributeKey.Contents,
        Order = 3 )]

    [TextField( "Set Panel Title",
        Description = "The title to display in the panel header. Leave empty to have the block name.",
        IsRequired = false,
        Key = AttributeKey.SetPanelTitle,
        Order = 4 )]

    [TextField( "Set Panel Icon",
        Description = "The icon to display in the panel header.",
        IsRequired = false,
        Key = AttributeKey.SetPanelIcon,
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "36E5D730-4B91-46BB-BF64-BF898EB02DF5" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "BD91C6BC-97D8-4645-A863-FBCF2986B4C1" )]
    [Rock.SystemGuid.BlockTypeGuid( "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1" )]
    public class MyWorkflowsLava : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Role = "Role";
            public const string Categories = "Categories";
            public const string IncludeChildCategories = "IncludeChildCategories";
            public const string Contents = "Contents";
            public const string SetPanelTitle = "SetPanelTitle";
            public const string SetPanelIcon = "SetPanelIcon";
        }

        private static class RoleValue
        {
            public const string AssignedTo = "0";
            public const string Initiated = "1";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            try
            {
                return GetWorkflowsHtml();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return "<div class='alert alert-danger'>An error occurred while getting workflows.</div>";
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Resolves the configured Lava template against the current person's workflow activities.
        /// </summary>
        /// <returns>The rendered HTML.</returns>
        private string GetWorkflowsHtml()
        {
            var role = GetAttributeValue( AttributeKey.Role );
            if ( role.IsNullOrWhiteSpace() )
            {
                role = RoleValue.AssignedTo;
            }

            var actions = role == RoleValue.Initiated
                ? GetInitiatedWorkflowActions()
                : GetAssignedFormActions();

            // Resolve theme and application relative URLs in the template before rendering.
            var template = GetAttributeValue( AttributeKey.Contents ) ?? string.Empty;
            var appRoot = RequestContext.ResolveRockUrl( "~/" );
            var themeRoot = RequestContext.ResolveRockUrl( "~~/" );
            template = template.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Role", role );
            mergeFields.Add( "Actions", actions.OrderByDescending( a => a.CreatedDateTime ).ToList() );
            mergeFields.Add( "PanelTitle", GetAttributeValue( AttributeKey.SetPanelTitle ) );
            mergeFields.Add( "PanelIcon", GetAttributeValue( AttributeKey.SetPanelIcon ) );

            return template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the first active form action for each activity assigned to and viewable by the current person.
        /// </summary>
        /// <returns>The list of form actions to display.</returns>
        private List<WorkflowAction> GetAssignedFormActions()
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return new List<WorkflowAction>();
            }

            var activeForms = new WorkflowActionService( RockContext ).GetActiveForms( currentPerson );

            // Reduce to the first form action per activity.
            var firstForms = activeForms
                .GroupBy( a => a.ActivityId )
                .Select( g => g.First() )
                .ToList();

            var categoryIds = GetCategoryIds();
            if ( categoryIds.Any() )
            {
                firstForms = firstForms
                    .Where( a => IsActionInCategories( a, categoryIds ) )
                    .ToList();
            }

            return firstForms;
        }

        /// <summary>
        /// Gets the active workflows initiated by the current person, wrapped in the action -&gt; activity -&gt; workflow
        /// shape the Lava template iterates.
        /// </summary>
        /// <returns>The list of synthetic actions to display.</returns>
        private List<WorkflowAction> GetInitiatedWorkflowActions()
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return new List<WorkflowAction>();
            }

            var query = new WorkflowService( RockContext )
                .Queryable()
                .Include( w => w.WorkflowType )
                .Where( w =>
                    w.ActivatedDateTime.HasValue &&
                    !w.CompletedDateTime.HasValue &&
                    w.InitiatorPersonAlias.PersonId == currentPerson.Id );

            var categoryIds = GetCategoryIds();
            if ( categoryIds.Any() )
            {
                query = query.Where( w =>
                    w.WorkflowType.CategoryId.HasValue &&
                    categoryIds.Contains( w.WorkflowType.CategoryId.Value ) );
            }

            var workflows = query.OrderBy( w => w.ActivatedDateTime ).ToList();

            return workflows
                .Select( workflow => new WorkflowAction
                {
                    Activity = new WorkflowActivity
                    {
                        Workflow = workflow
                    }
                } )
                .ToList();
        }

        /// <summary>
        /// Determines whether the action's workflow type belongs to one of the supplied categories.
        /// </summary>
        /// <param name="action">The workflow action to test.</param>
        /// <param name="categoryIds">The category identifiers to match against.</param>
        /// <returns><c>true</c> if the action's workflow type is in one of the categories; otherwise <c>false</c>.</returns>
        private bool IsActionInCategories( WorkflowAction action, List<int> categoryIds )
        {
            var workflowTypeId = action.Activity?.Workflow?.WorkflowTypeId;
            if ( !workflowTypeId.HasValue )
            {
                return false;
            }

            // Resolve the workflow type from cache to avoid lazy-loading the action-type chain.
            var workflowType = WorkflowTypeCache.Get( workflowTypeId.Value );
            return workflowType?.CategoryId.HasValue == true && categoryIds.Contains( workflowType.CategoryId.Value );
        }

        /// <summary>
        /// Gets the workflow type category identifiers to filter by, including child categories when configured.
        /// When no categories are configured, this returns every workflow type category the current person is
        /// authorized to view, matching the original block so the filter still excludes uncategorized workflow
        /// types and types in categories the person cannot view.
        /// </summary>
        /// <returns>The category identifiers to filter by.</returns>
        private List<int> GetCategoryIds()
        {
            var selectedCategoryGuids = GetAttributeValue( AttributeKey.Categories )
                .SplitDelimitedValues()
                .Select( value => value.AsGuid() )
                .ToList();

            var includeChildCategories = GetAttributeValue( AttributeKey.IncludeChildCategories ).AsBoolean();
            var workflowTypeEntityTypeId = EntityTypeCache.Get( typeof( WorkflowType ) ).Id;

            var navigationItems = new CategoryService( RockContext )
                .GetNavigationItems( workflowTypeEntityTypeId, selectedCategoryGuids, includeChildCategories, GetCurrentPerson() );

            var categoryIds = new List<int>();
            AddCategoryIds( categoryIds, navigationItems );
            return categoryIds;
        }

        /// <summary>
        /// Recursively flattens the category navigation tree into a list of category identifiers.
        /// </summary>
        /// <param name="ids">The list to append category identifiers to.</param>
        /// <param name="items">The category navigation items to flatten.</param>
        private void AddCategoryIds( List<int> ids, List<CategoryNavigationItem> items )
        {
            foreach ( var item in items )
            {
                ids.Add( item.Category.Id );
                AddCategoryIds( ids, item.ChildCategories );
            }
        }

        #endregion
    }
}
