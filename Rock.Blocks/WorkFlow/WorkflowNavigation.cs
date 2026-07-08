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
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Workflow.WorkflowNavigation;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Displays a navigable tree of workflow categories and the workflow types
    /// within them, allowing individuals to launch and manage workflows.
    /// </summary>
    [DisplayName( "Workflow Navigation" )]
    [Category( "Workflow" )]
    [Description( "Block for navigating workflow types and launching and/or managing workflows." )]
    [IconCssClass( "ti ti-settings-cog" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CategoryField( "Categories",
        Description = "The categories to display.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.WorkflowType",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.Categories )]

    [BooleanField( "Include Child Categories",
        Description = "Should descendent categories of the selected Categories be included?",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.IncludeChildCategories )]

    [LinkedPage( "Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.EntryPage )]

    [LinkedPage( "Manage Page",
        Description = "Page used to manage workflows of the selected type.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.ManagePage )]

    [Rock.SystemGuid.EntityTypeGuid( "C555A6A1-440A-49C8-9353-27FA2A8E08E8" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "8D7C6172-30A1-4D90-A344-47285B312F14" )]
    [Rock.SystemGuid.BlockTypeGuid( "DDC6B004-9ED1-470F-ABF5-041250082168" )]
    public class WorkflowNavigation : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Categories = "Categories";
            public const string IncludeChildCategories = "IncludeChildCategories";
            public const string EntryPage = "EntryPage";
            public const string ManagePage = "ManagePage";
        }

        private static class NavigationUrlKey
        {
            public const string EntryPage = "EntryPage";
            public const string ManagePage = "ManagePage";
        }

        private static class PageParameterKey
        {
            public const string WorkflowTypeId = "WorkflowTypeId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CustomBlockBox<WorkflowNavigationBag, WorkflowNavigationOptionsBag>
            {
                Bag = new WorkflowNavigationBag
                {
                    Categories = GetWorkflowNavigationCategories()
                },
                Options = new WorkflowNavigationOptionsBag
                {
                    IsBlockEditAuthorized = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                },
                NavigationUrls = GetBoxNavigationUrls()
            };
        }

        /// <summary>
        /// Gets the navigation URL templates used to link each workflow type to
        /// the configured Entry and Manage pages. The <c>((WorkflowTypeKey))</c>
        /// token is replaced with each workflow type's key on the client.
        /// </summary>
        /// <returns>A dictionary of navigation URL templates keyed by name.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.EntryPage] = this.GetLinkedPageUrl( AttributeKey.EntryPage, PageParameterKey.WorkflowTypeId, "((WorkflowTypeKey))" ),
                [NavigationUrlKey.ManagePage] = this.GetLinkedPageUrl( AttributeKey.ManagePage, PageParameterKey.WorkflowTypeId, "((WorkflowTypeKey))" )
            };
        }

        /// <summary>
        /// Builds the authorized, ordered tree of workflow categories and the
        /// workflow types within them for the current person.
        /// </summary>
        /// <returns>The top-level workflow categories to display.</returns>
        private List<WorkflowNavigationCategoryBag> GetWorkflowNavigationCategories()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var workflowTypeEntityTypeId = EntityTypeCache.Get( typeof( WorkflowType ) ).Id;

            var selectedCategoryGuids = GetAttributeValue( AttributeKey.Categories )
                .SplitDelimitedValues()
                .Select( g => g.AsGuid() )
                .ToList();
            var includeChildCategories = GetAttributeValue( AttributeKey.IncludeChildCategories ).AsBoolean();

            // GetNavigationItems already filters the tree to categories the
            // current person is authorized to view.
            var navigationItems = new CategoryService( RockContext )
                .GetNavigationItems( workflowTypeEntityTypeId, selectedCategoryGuids, includeChildCategories, currentPerson );

            // Collect the category Ids in the tree so the workflow type query can
            // be scoped to only the categories that will actually be displayed.
            var categoryIds = new List<int>();
            CollectCategoryIds( navigationItems, categoryIds );

            // No authorized categories means there is nothing to display, so
            // skip the workflow type query entirely.
            if ( categoryIds.Count == 0 )
            {
                return new List<WorkflowNavigationCategoryBag>();
            }

            // Eager-load ActivityTypes.ActionTypes because HasActiveForms walks
            // them in memory and that value is not available from the cache.
            var workflowTypesByCategoryId = new WorkflowTypeService( RockContext )
                .Queryable( "ActivityTypes.ActionTypes" )
                .AsNoTracking()
                .Where( t => t.CategoryId.HasValue && categoryIds.Contains( t.CategoryId.Value ) )
                .ToList()
                .GroupBy( t => t.CategoryId.Value )
                .ToDictionary( g => g.Key, g => g.ToList() );

            return BuildCategoryBags( navigationItems, workflowTypesByCategoryId, currentPerson );
        }

        /// <summary>
        /// Recursively collects the identifiers of every category in the tree.
        /// </summary>
        /// <param name="items">The navigation items to walk.</param>
        /// <param name="categoryIds">The list that receives the category Ids.</param>
        private void CollectCategoryIds( List<CategoryNavigationItem> items, List<int> categoryIds )
        {
            foreach ( var item in items )
            {
                categoryIds.Add( item.Category.Id );
                CollectCategoryIds( item.ChildCategories, categoryIds );
            }
        }

        /// <summary>
        /// Recursively converts the category navigation tree into the bags
        /// consumed by the client, applying per-workflow-type authorization.
        /// </summary>
        /// <param name="categoryItems">The category navigation items to convert.</param>
        /// <param name="workflowTypesByCategoryId">The workflow types grouped by category Id.</param>
        /// <param name="currentPerson">The person used for authorization checks.</param>
        /// <returns>The converted category bags.</returns>
        private List<WorkflowNavigationCategoryBag> BuildCategoryBags( List<CategoryNavigationItem> categoryItems, Dictionary<int, List<WorkflowType>> workflowTypesByCategoryId, Person currentPerson )
        {
            var bags = new List<WorkflowNavigationCategoryBag>();

            foreach ( var item in categoryItems )
            {
                var category = item.Category;
                var bag = new WorkflowNavigationCategoryBag
                {
                    IdKey = category.IdKey,
                    Name = category.Name,
                    IconCssClass = category.IconCssClass,
                    ChildCategories = BuildCategoryBags( item.ChildCategories, workflowTypesByCategoryId, currentPerson ),
                    WorkflowTypes = new List<WorkflowNavigationWorkflowTypeBag>()
                };

                if ( workflowTypesByCategoryId.TryGetValue( category.Id, out var workflowTypes ) )
                {
                    foreach ( var workflowType in workflowTypes.OrderBy( t => t.Order ).ThenBy( t => t.Name ) )
                    {
                        if ( !workflowType.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            continue;
                        }

                        bag.WorkflowTypes.Add( new WorkflowNavigationWorkflowTypeBag
                        {
                            IdKey = workflowType.IdKey,
                            Name = workflowType.Name,
                            IconCssClass = workflowType.IconCssClass,
                            IsLaunchEnabled = workflowType.HasActiveForms && ( workflowType.IsActive == true ),
                            CanManage = workflowType.IsAuthorized( Authorization.EDIT, currentPerson ),
                            CanViewList = workflowType.IsAuthorized( Authorization.VIEW_LIST, currentPerson )
                        } );
                    }
                }

                bags.Add( bag );
            }

            return bags;
        }

        #endregion Methods
    }
}
