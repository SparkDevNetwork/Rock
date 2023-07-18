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

using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Rock.ClientService.Core.Category.Options;
using Rock.Data;
using Rock.Model;
using Rock.Model.Core.Category.Options;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Rock.ClientService.Core.Category
{
    /// <summary>
    /// Provides methods to work with <see cref="Category"/> and translate
    /// information into data that can be consumed by the clients.
    /// </summary>
    /// <seealso cref="Rock.ClientService.ClientServiceBase" />
    public class CategoryClientService : ClientServiceBase
    {
        #region Default Options

        /// <summary>
        /// The default category item tree options.
        /// </summary>
        private static readonly CategoryItemTreeOptions DefaultCategoryItemTreeOptions = new CategoryItemTreeOptions();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public CategoryClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the categorized tree items. This supports various options
        /// for filtering and determine how much data to load.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A list of view models that describe the tree of categories and items.</returns>
        public List<TreeItemBag> GetCategorizedTreeItems( CategoryItemTreeOptions options = null )
        {
            return GetCategorizedTreeItems<ICategorized>( options, null );
        }

        /// <summary>
        /// Gets the categorized tree items. This supports various options
        /// for filtering and determine how much data to load.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <param name="filterMethod">A Function to filter out Categories</param>
        /// <returns>A list of view models that describe the tree of categories and items.</returns>
        public List<TreeItemBag> GetCategorizedTreeItems<T>( CategoryItemTreeOptions options = null, Func<T, bool> filterMethod = null ) where T : ICategorized
        {
            options = options ?? DefaultCategoryItemTreeOptions;

            // Initialize the basic query.
            var categoryService = new CategoryService( RockContext );

            var childQueryOptions = new ChildCategoryQueryOptions
            {
                ParentGuid = options.ParentGuid,
                EntityTypeGuid = options.EntityTypeGuid,
                IncludeCategoryGuids = options.IncludeCategoryGuids,
                ExcludeCategoryGuids = options.ExcludeCategoryGuids,
                EntityTypeQualifierColumn = options.EntityTypeQualifierColumn,
                EntityTypeQualifierValue = options.EntityTypeQualifierValue
            };

            var qry = categoryService.GetChildCategoryQuery( childQueryOptions );

            // Cache the service instance for later use. If we have specified an
            // entity type then this ends up getting set.
            IService serviceInstance = null;
            var cachedEntityType = options.EntityTypeGuid.HasValue
                ? EntityTypeCache.Get( options.EntityTypeGuid.Value )
                : null;

            // If we have been requested to limit the results to a specific entity
            // type then apply those filters.
            if ( cachedEntityType != null )
            {
                // Attempt to initialize the entity service instance. This also
                // checks if the entity supports the active flag.
                if ( cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();
                    if ( entityType != null )
                    {
                        Type[] modelType = { entityType };
                        Type genericServiceType = typeof( Rock.Data.Service<> );
                        Type modelServiceType = genericServiceType.MakeGenericType( modelType );

                        serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { new RockContext() } ) as IService;
                    }
                }
            }

            var categoryList = qry.OrderBy( c => c.Order ).ThenBy( c => c.Name ).ToList();

            // Get all the categories from the query and then filter on security.
            var categoryItemList = categoryList
                .Where( c => c.IsAuthorized( Authorization.VIEW, Person ) || options.SecurityGrant?.IsAccessGranted( c, Authorization.VIEW ) == true )
                .Select( c => new TreeItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name,
                    IsFolder = true,
                    IconCssClass = c.IconCssClass
                } )
                .ToList();

            if ( options.GetCategorizedItems )
            {
                var itemOptions = new CategorizedItemQueryOptions
                {
                    CategoryGuid = options.ParentGuid,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    ItemFilterPropertyName = options.ItemFilterPropertyName,
                    ItemFilterPropertyValue = options.ItemFilterPropertyValue
                };

                var itemsQry = categoryService.GetCategorizedItemQuery( serviceInstance, itemOptions );

                if ( itemsQry != null )
                {
                    var childItems = GetChildrenItems<ICategorized>( options, cachedEntityType, itemsQry );
                    categoryItemList.AddRange( childItems );
                }
            }

            if ( options.LazyLoad )
            {
                // Try to figure out which items have viewable children in
                // the existing list and set them appropriately.
                foreach ( var categoryItemListItem in categoryItemList )
                {
                    if ( categoryItemListItem.IsFolder )
                    {
                        categoryItemListItem.HasChildren = DoesCategoryHaveChildren( options, categoryService, serviceInstance, categoryItemListItem.Value );
                    }
                }
            }
            else
            {
                foreach ( var item in categoryItemList )
                {
                    var parentGuid = item.Value.AsGuidOrNull();

                    if ( item.Children == null )
                    {
                        item.Children = new List<TreeItemBag>();
                    }

                    GetAllDescendants( item, Person, categoryService, serviceInstance, cachedEntityType, options, filterMethod );
                }
            }

            // If they don't want empty categories then filter out categories
            // that do not have child items.
            if ( !options.IncludeCategoriesWithoutChildren )
            {
                categoryItemList = categoryItemList
                    .Where( a => !a.IsFolder || ( a.IsFolder && a.HasChildren ) )
                    .ToList();
            }

            return categoryItemList;
        }

        /// <summary>
        /// Determines if the category has any child categories or items.
        /// </summary>
        /// <param name="options">The options that describe the current operation.</param>
        /// <param name="categoryService">The category service.</param>
        /// <param name="serviceInstance">The service instance used to access the items.</param>
        /// <param name="categoryId">The category item identifier.</param>
        /// <returns><c>true</c> if the category has children, <c>false</c> otherwise.</returns>
        private bool DoesCategoryHaveChildren( CategoryItemTreeOptions options, CategoryService categoryService, IService serviceInstance, string categoryId )
        {
            var parentGuid = categoryId.AsGuid();

            // First try a simple query on categories. This is the cheaper
            // of the two operations.
            foreach ( var childCategory in categoryService.Queryable().Where( c => c.ParentCategory.Guid == parentGuid ) )
            {
                if ( childCategory.IsAuthorized( Authorization.VIEW, Person ) || options.SecurityGrant?.IsAccessGranted( childCategory, Authorization.VIEW ) == true )
                {
                    return true;
                }
            }

            // If we didn't find any children from the above then try looking
            // for any items that reference this category.

            var itemOptions = new CategorizedItemQueryOptions
            {
                CategoryGuid = parentGuid,
                IncludeInactiveItems = options.IncludeInactiveItems,
                IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                ItemFilterPropertyName = options.ItemFilterPropertyName,
                ItemFilterPropertyValue = options.ItemFilterPropertyValue
            };

            var childItems = categoryService.GetCategorizedItemQuery( serviceInstance, itemOptions );

            if ( childItems != null )
            {
                foreach ( var categorizedItem in childItems )
                {
                    if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, Person ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the sorted list of children items from the query.
        /// </summary>
        /// <param name="options">The options that describe the current operation.</param>
        /// <param name="cachedEntityType">The cached entity type object that describes the items.</param>
        /// <param name="itemsQry">The items query.</param>
        /// <param name="filterMethod">A Function to filter out Categories</param>
        /// <returns>A list of child items that should be included in the results.</returns>
        private List<TreeItemBag> GetChildrenItems<T>( CategoryItemTreeOptions options, EntityTypeCache cachedEntityType, IQueryable<ICategorized> itemsQry, Func<T, bool> filterMethod = null ) where T : ICategorized
        {
            // Do a ToList() to load from database prior to ordering
            // by name, just in case Name is a virtual property.
            var itemsList = itemsQry.ToList();
            if ( filterMethod != null )
            {
                itemsList = itemsList.Where( i => filterMethod.Invoke( ( T ) i ) ).ToList();
            }
            var entityTypeIsSchedule = cachedEntityType.Id == EntityTypeCache.GetId<Rock.Model.Schedule>();

            List<ICategorized> sortedItemsList;

            // Sort the items by the name, unless it is a schedule.
            if ( entityTypeIsSchedule && itemsList.OfType<Rock.Model.Schedule>() != null )
            {
                sortedItemsList = itemsList.OfType<Rock.Model.Schedule>().ToList().OrderByOrderAndNextScheduledDateTime().OfType<ICategorized>().ToList();
            }
            else
            {
                sortedItemsList = itemsList.OrderBy( i => i.Name ).ToList();
            }

            var children = new List<TreeItemBag>();

            // Walk each item from the sorted list and determine if we it
            // should be added to the list of children.
            foreach ( var categorizedItem in sortedItemsList )
            {
                // Ensure the person is authorized to view this item and that
                // the item is of type IEntity so we can get to the Guid.
                if ( categorizedItem.IsAuthorized( Authorization.VIEW, Person ) && categorizedItem is IEntity categorizedEntityItem )
                {
                    var categoryItem = new TreeItemBag
                    {
                        Value = categorizedEntityItem.Guid.ToString(),
                        Text = categorizedItem.Name,
                        IconCssClass = categorizedItem.GetPropertyValue( "IconCssClass" ) as string ?? options.DefaultIconCssClass
                    };

                    if ( categorizedItem is IHasActiveFlag activatedItem )
                    {
                        categoryItem.IsActive = activatedItem.IsActive;
                    }

                    children.Add( categoryItem );
                }
            }

            return children;
        }

        /// <summary>
        /// Gets all both category and non-category item decendents for the provided categoryItem.
        /// This method updates the provided categoryItem.
        /// </summary>
        /// <param name="categoryItem">The category item.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="categoryService">The category service.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="cachedEntityType">The cached entity type of the items.</param>
        /// <param name="options">The options that describe the current operation.</param>
        /// <param name="filterMethod">A Function to filter out Categories</param>
        /// <returns></returns>
        private TreeItemBag GetAllDescendants<T>( TreeItemBag categoryItem, Person currentPerson, CategoryService categoryService, IService serviceInstance, EntityTypeCache cachedEntityType, CategoryItemTreeOptions options, Func<T, bool> filterMethod = null ) where T : ICategorized
        {
            if ( categoryItem.IsFolder )
            {
                var parentGuid = categoryItem.Value.AsGuidOrNull();
                var childCategories = categoryService.Queryable()
                    .AsNoTracking()
                    .Where( c => c.ParentCategory.Guid == parentGuid )
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name );

                foreach ( var childCategory in childCategories )
                {
                    if ( childCategory.IsAuthorized( Authorization.VIEW, currentPerson ) || options.SecurityGrant?.IsAccessGranted( childCategory, Authorization.VIEW ) == true )
                    {
                        // This category has child categories that the person can view so add them to categoryItemList
                        categoryItem.HasChildren = true;
                        var childCategoryItem = new TreeItemBag
                        {
                            Value = childCategory.Guid.ToString(),
                            Text = childCategory.Name,
                            IsFolder = true,
                            IconCssClass = childCategory.GetPropertyValue( "IconCssClass" ) as string ?? options.DefaultIconCssClass
                        };

                        if ( childCategory is IHasActiveFlag activatedItem )
                        {
                            childCategoryItem.IsActive = activatedItem.IsActive;
                        }

                        var childCategorizedItemBranch = GetAllDescendants<T>( childCategoryItem, currentPerson, categoryService, serviceInstance, cachedEntityType, options, filterMethod );
                        if ( categoryItem.Children == null )
                        {
                            categoryItem.Children = new List<TreeItemBag>();
                        }

                        categoryItem.Children.Add( childCategorizedItemBranch );
                    }
                }

                // now that we have taken care of the child categories get the items for this category.
                if ( options.GetCategorizedItems )
                {
                    var itemOptions = new CategorizedItemQueryOptions
                    {
                        CategoryGuid = parentGuid,
                        IncludeInactiveItems = options.IncludeInactiveItems,
                        IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                        ItemFilterPropertyName = options.ItemFilterPropertyName,
                        ItemFilterPropertyValue = options.ItemFilterPropertyValue
                    };

                    var childQry = categoryService.GetCategorizedItemQuery( serviceInstance, itemOptions );

                    if ( childQry != null )
                    {
                        var childItems = GetChildrenItems<T>( options, cachedEntityType, childQry, filterMethod );

                        if ( categoryItem.Children == null )
                        {
                            categoryItem.Children = new List<TreeItemBag>();
                        }
                        categoryItem.Children.AddRange( childItems );
                    }
                }

                categoryItem.HasChildren = categoryItem.Children?.Any() ?? false;
            }

            return categoryItem;
        }

        #endregion
    }
}
