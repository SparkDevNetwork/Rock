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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CategoriesController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootCategoryId">The root category identifier.</param>
        /// <param name="getCategorizedItems">if set to <c>true</c> [get categorized items].</param>
        /// <param name="entityTypeId">The entity type for the Categorys</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <param name="showCategoriesThatHaveNoChildren">if set to <c>true</c> [show categories that have no children].</param>
        /// <param name="includedCategoryIds">The included category ids.</param>
        /// <param name="excludedCategoryIds">The excluded category ids.</param>
        /// <param name="defaultIconCssClass">The default icon CSS class.</param>
        /// <param name="includeInactiveItems">if set to <c>true</c> [include inactive items].</param>
        /// <param name="itemFilterPropertyName">(Advanced) Property to FilterBy on the Item Query</param>
        /// <param name="itemFilterPropertyValue">(Advanced) Property Value to FilterBy on the Item Query</param>
        /// <param name="lazyLoad">If true then get the data for each tree node as it is selected, otherwise get the entire tree.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Categories/GetChildren/{id}" )]
        public IQueryable<CategoryItem> GetChildren(
            int id,
            int rootCategoryId = 0,
            bool getCategorizedItems = false,
            int entityTypeId = 0,
            string entityQualifier = null,
            string entityQualifierValue = null,
            bool showUnnamedEntityItems = true,
            bool showCategoriesThatHaveNoChildren = true,
            string includedCategoryIds = null,
            string excludedCategoryIds = null,
            string defaultIconCssClass = null,
            bool includeInactiveItems = true,
            string itemFilterPropertyName = null,
            string itemFilterPropertyValue = null,
            bool lazyLoad = true)
        {
            Person currentPerson = GetPerson();

            var includedCategoryIdList = includedCategoryIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            var excludedCategoryIdList = excludedCategoryIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            defaultIconCssClass = defaultIconCssClass ?? "fa fa-list-ol";

            bool hasActiveFlag = false;
            bool excludeInactiveItems = !includeInactiveItems;

            IQueryable<Category> qry = Get();

            if ( id == 0 )
            {
                if ( rootCategoryId != 0 )
                {
                    qry = qry.Where( a => a.ParentCategoryId == rootCategoryId );
                }
                else
                {
                    qry = qry.Where( a => a.ParentCategoryId == null );
                }
            }
            else
            {
                qry = qry.Where( a => a.ParentCategoryId == id );
            }

            if ( includedCategoryIdList.Any() )
            {
                // if includedCategoryIdList is specified, only get categories that are in the includedCategoryIdList
                // NOTE: no need to factor in excludedCategoryIdList since included would take precendance and the excluded ones would already not be included
                qry = qry.Where( a => includedCategoryIdList.Contains( a.Id ) );
            }
            else if ( excludedCategoryIdList.Any() )
            {
                qry = qry.Where( a => !excludedCategoryIdList.Contains( a.Id ) );
            }

            IService serviceInstance = null;

            var cachedEntityType = EntityTypeCache.Get( entityTypeId );
            if ( cachedEntityType != null )
            {
                qry = qry.Where( a => a.EntityTypeId == entityTypeId );
                if ( !string.IsNullOrWhiteSpace( entityQualifier ) )
                {
                    qry = qry.Where( a => string.Compare( a.EntityTypeQualifierColumn, entityQualifier, true ) == 0 );
                    if ( !string.IsNullOrWhiteSpace( entityQualifierValue ) )
                    {
                        qry = qry.Where( a => string.Compare( a.EntityTypeQualifierValue, entityQualifierValue, true ) == 0 );
                    }
                    else
                    {
                        qry = qry.Where( a => a.EntityTypeQualifierValue == null || a.EntityTypeQualifierValue == string.Empty );
                    }
                }

                // Get the GetByCategory method
                if ( cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();
                    if ( entityType != null )
                    {
                        Type[] modelType = { entityType };
                        Type genericServiceType = typeof( Rock.Data.Service<> );
                        Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                        serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { new RockContext() } ) as IService;

                        hasActiveFlag = typeof( IHasActiveFlag ).IsAssignableFrom( entityType );
                    }
                }
            }

            excludeInactiveItems = excludeInactiveItems && hasActiveFlag;

            List<Category> categoryList = qry.OrderBy( c => c.Order ).ThenBy( c => c.Name ).ToList();
            List<CategoryItem> categoryItemList = new List<CategoryItem>();

            foreach ( var category in categoryList )
            {
                if ( category.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    var categoryItem = new CategoryItem();
                    categoryItem.Id = category.Id.ToString();
                    categoryItem.Name = category.Name;
                    categoryItem.IsCategory = true;
                    categoryItem.IconCssClass = category.IconCssClass;
                    categoryItemList.Add( categoryItem );
                }
            }

            if ( getCategorizedItems )
            {
                // if id is zero and we have a rootCategory, show the children of that rootCategory (but don't show the rootCategory)
                int parentItemId = id == 0 ? rootCategoryId : id;

                var itemsQry = GetCategorizedItems( serviceInstance, parentItemId, showUnnamedEntityItems, excludeInactiveItems, itemFilterPropertyName, itemFilterPropertyValue );
                if ( itemsQry != null )
                {
                    // do a ToList to load from database prior to ordering by name, just in case Name is a virtual property
                    List<ICategorized> itemsList = itemsQry.ToList();

                    List<ICategorized> sortedItemsList;

                    bool isSchedule = cachedEntityType.Id == EntityTypeCache.GetId<Rock.Model.Schedule>();

                    if ( isSchedule && itemsList.OfType<Rock.Model.Schedule>() != null)
                    {
                        sortedItemsList = itemsList.OfType<Rock.Model.Schedule>().ToList().OrderByNextScheduledDateTime().OfType<ICategorized>().ToList();
                    }
                    else
                    {
                        sortedItemsList = itemsList.OrderBy( i => i.Name ).ToList();
                    }

                    foreach ( var categorizedItem in sortedItemsList )
                    {
                        if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            var categoryItem = new CategoryItem();
                            categoryItem.Id = categorizedItem.Id.ToString();
                            categoryItem.Name = categorizedItem.Name;
                            categoryItem.IsCategory = false;
                            categoryItem.IconCssClass = categorizedItem.GetPropertyValue( "IconCssClass" ) as string ?? defaultIconCssClass;
                            categoryItem.IconSmallUrl = string.Empty;

                            if ( hasActiveFlag )
                            {
                                IHasActiveFlag activatedItem = categorizedItem as IHasActiveFlag;
                                if ( activatedItem != null && !activatedItem.IsActive )
                                {
                                    categoryItem.IsActive = false;
                                }
                            }

                            categoryItemList.Add( categoryItem );
                        }
                    }
                }

                if ( lazyLoad )
                {
                    // try to figure out which items have viewable children in the existing list and set them appropriately
                    foreach ( var categoryItemListItem in categoryItemList )
                    {
                        if ( categoryItemListItem.IsCategory )
                        {
                            int parentId = int.Parse( categoryItemListItem.Id );

                            foreach ( var childCategory in Get().Where( c => c.ParentCategoryId == parentId ) )
                            {
                                if ( childCategory.IsAuthorized( Authorization.VIEW, currentPerson ) )
                                {
                                    categoryItemListItem.HasChildren = true;
                                    break;
                                }
                            }

                            if ( !categoryItemListItem.HasChildren )
                            {
                                if ( getCategorizedItems )
                                {
                                    var childItems = GetCategorizedItems( serviceInstance, parentId, showUnnamedEntityItems, excludeInactiveItems, itemFilterPropertyName, itemFilterPropertyValue );
                                    if ( childItems != null )
                                    {
                                        foreach ( var categorizedItem in childItems )
                                        {
                                            if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                                            {
                                                categoryItemListItem.HasChildren = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach ( var item in categoryItemList )
                    {
                        int parentId = int.Parse( item.Id );
                        if ( item.Children == null )
                        {
                            item.Children = new List<Web.UI.Controls.TreeViewItem>();
                        }

                        GetAllDecendents( item, currentPerson, getCategorizedItems, defaultIconCssClass, hasActiveFlag, serviceInstance, showUnnamedEntityItems, excludeInactiveItems, itemFilterPropertyName, itemFilterPropertyValue );
                    }
                }
            }
            else if ( !lazyLoad )
            {
                // Load all of the categories without the categorized items
                foreach ( var item in categoryItemList )
                {
                    int parentId = int.Parse( item.Id );
                    if ( item.Children == null )
                    {
                        item.Children = new List<Web.UI.Controls.TreeViewItem>();
                    }

                    GetAllDecendents( item, currentPerson, getCategorizedItems, defaultIconCssClass, hasActiveFlag, serviceInstance, showUnnamedEntityItems, excludeInactiveItems, itemFilterPropertyName, itemFilterPropertyValue );
                }
            }

            if ( !showCategoriesThatHaveNoChildren )
            {
                categoryItemList = categoryItemList.Where( a => !a.IsCategory || ( a.IsCategory && a.HasChildren ) ).ToList();
            }

            return categoryItemList.AsQueryable();
        }

        /// <summary>
        /// Gets all both category and non-category item decendents for the provided categoryItem.
        /// This method updates the provided categoryItem.
        /// </summary>
        /// <param name="categoryItem">The category item.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="getCategorizedItems">if set to <c>true</c> [get categorized items].</param>
        /// <param name="defaultIconCssClass">The default icon CSS class.</param>
        /// <param name="hasActiveFlag">if set to <c>true</c> [has active flag].</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <param name="excludeInactiveItems">if set to <c>true</c> [exclude inactive items].</param>
        /// <param name="itemFilterPropertyName">Name of the item filter property.</param>
        /// <param name="itemFilterPropertyValue">The item filter property value.</param>
        /// <returns></returns>
        private CategoryItem GetAllDecendents( CategoryItem categoryItem, Person currentPerson, bool getCategorizedItems, string defaultIconCssClass, bool hasActiveFlag, IService serviceInstance, bool showUnnamedEntityItems, bool excludeInactiveItems, string itemFilterPropertyName = null, string itemFilterPropertyValue = null )
        {
            if ( categoryItem.IsCategory )
            {
                int parentId = int.Parse( categoryItem.Id );
                var childCategories = Get().Where( c => c.ParentCategoryId == parentId ).OrderBy( c => c.Order).ThenBy( c => c.Name );

                foreach ( var childCategory in childCategories )
                {
                    if ( childCategory.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        // This category has child categories that the person can view so add them to categoryItemList
                        categoryItem.HasChildren = true;
                        var childCategoryItem = new CategoryItem
                        {
                            Id = childCategory.Id.ToString(),
                            Name = childCategory.Name,
                            IsCategory = true,
                            IconCssClass = childCategory.GetPropertyValue( "IconCssClass" ) as string ?? defaultIconCssClass,
                            IconSmallUrl = string.Empty
                        };
                        
                        if ( hasActiveFlag )
                        {
                            IHasActiveFlag activatedItem = childCategory as IHasActiveFlag;
                            if ( activatedItem != null && !activatedItem.IsActive )
                            {
                                childCategoryItem.IsActive = false;
                            }
                        }

                        var childCategorizedItemBranch = GetAllDecendents( childCategoryItem, currentPerson, getCategorizedItems, defaultIconCssClass, hasActiveFlag, serviceInstance, showUnnamedEntityItems, excludeInactiveItems, itemFilterPropertyName, itemFilterPropertyValue );
                        if ( categoryItem.Children == null )
                        {
                            categoryItem.Children = new List<Web.UI.Controls.TreeViewItem>();
                        }

                        categoryItem.Children.Add( childCategorizedItemBranch );
                    }
                }

                // now that we have taken care of the child categories get the items for this category.
                if ( getCategorizedItems )
                {
                    var childItems = GetCategorizedItems( serviceInstance, parentId, showUnnamedEntityItems, excludeInactiveItems, itemFilterPropertyName, itemFilterPropertyValue );
                    if ( childItems != null )
                    {
                        foreach ( var categorizedItem in childItems.OrderBy( c => c.Name ) )
                        {
                            if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                            {
                                categoryItem.HasChildren = true;
                                var childCategoryItem = new CategoryItem
                                {
                                    Id = categorizedItem.Id.ToString(),
                                    Name = categorizedItem.Name,
                                    IsCategory = false,
                                    IconCssClass = categorizedItem.GetPropertyValue( "IconCssClass" ) as string ?? defaultIconCssClass,
                                    IconSmallUrl = string.Empty
                                };

                                if ( hasActiveFlag )
                                {
                                    IHasActiveFlag activatedItem = categorizedItem as IHasActiveFlag;
                                    if ( activatedItem != null && !activatedItem.IsActive )
                                    {
                                        childCategoryItem.IsActive = false;
                                    }
                                }

                                if ( categoryItem.Children == null )
                                {
                                    categoryItem.Children = new List<Web.UI.Controls.TreeViewItem>();
                                }

                                categoryItem.Children.Add( childCategoryItem );
                            }
                        }
                    }
                }
            }

            return categoryItem;
        }

        /// <summary>
        /// Gets the categorized items.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="categoryId">The category id.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <param name="excludeInactiveItems">if set to <c>true</c> [exclude inactive items].</param>
        /// <param name="itemFilterPropertyName">(Advanced) Property to FilterBy on the Item Query</param>
        /// <param name="itemFilterPropertyValue">(Advanced) Property Value to FilterBy on the Item Query</param>
        /// <returns></returns>
        private IQueryable<ICategorized> GetCategorizedItems( IService serviceInstance, int categoryId, bool showUnnamedEntityItems, bool excludeInactiveItems, string itemFilterPropertyName = null, string itemFilterPropertyValue = null )
        {
            if ( serviceInstance != null )
            {
                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );
                if ( getMethod != null )
                {
                    ParameterExpression paramExpression = serviceInstance.ParameterExpression;

                    BinaryExpression whereExpression;
                    MemberExpression categoryPropertyExpression = Expression.Property( paramExpression, "CategoryId" );
                    ConstantExpression categoryConstantExpression = Expression.Constant( categoryId );
                    if ( categoryPropertyExpression.Type == typeof( int? ) )
                    {
                        var zeroExpression = Expression.Constant( 0 );
                        var coalesceExpression = Expression.Coalesce( categoryPropertyExpression, zeroExpression );
                        whereExpression = Expression.Equal( coalesceExpression, categoryConstantExpression );
                    }
                    else
                    {
                        whereExpression = Expression.Equal( categoryPropertyExpression, categoryConstantExpression );
                    }

                    IQueryable<ICategorized> result = null;

                    if ( excludeInactiveItems )
                    {
                        MemberExpression isActivePropertyExpression = Expression.Property( paramExpression, "IsActive" );
                        Expression isActiveConstantExpression = Expression.Convert( Expression.Constant( true ), isActivePropertyExpression.Type );
                        BinaryExpression isActiveExpression = Expression.Equal( isActivePropertyExpression, isActiveConstantExpression );
                        whereExpression = Expression.And( whereExpression, isActiveExpression );
                    }

                    if ( !string.IsNullOrEmpty(itemFilterPropertyName) )
                    {
                        MemberExpression itemFilterPropertyNameExpression = Expression.Property( paramExpression, itemFilterPropertyName );
                        ConstantExpression itemFilterPropertyValueExpression;
                        if ( itemFilterPropertyNameExpression.Type == typeof( int? ) || itemFilterPropertyNameExpression.Type == typeof( int ) )
                        {
                            itemFilterPropertyValueExpression = Expression.Constant( itemFilterPropertyValue.AsIntegerOrNull(), typeof(int?) );
                        }
                        else if ( itemFilterPropertyNameExpression.Type == typeof( Guid? ) || itemFilterPropertyNameExpression.Type == typeof( Guid ) )
                        {
                            itemFilterPropertyValueExpression = Expression.Constant( itemFilterPropertyValue.AsGuidOrNull(), typeof( Guid? ) );
                        }
                        else
                        {
                            itemFilterPropertyValueExpression = Expression.Constant( itemFilterPropertyValue );
                        }
                        
                        BinaryExpression binaryExpression = Expression.Equal( itemFilterPropertyNameExpression, itemFilterPropertyValueExpression );
                        whereExpression = Expression.And( whereExpression, binaryExpression );
                    }

                    result = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression } ) as IQueryable<ICategorized>;

                    if ( !showUnnamedEntityItems )
                    {
                        result = result.Where( a => a.Name != null && a.Name != string.Empty );
                    }

                    return result;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CategoryItem : Rock.Web.UI.Controls.TreeViewItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is category.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is category; otherwise, <c>false</c>.
        /// </value>
        public bool IsCategory { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsCategory)
            {
                return "Category:" + this.Name;
            }
            else
            {
                return this.Name;
            }
        }
    }
}
