// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <param name="showCategoriesThatHaveNoChildren">if set to <c>true</c> [show categories that have no children].</param>
        /// <param name="includedCategoryIds">The included category ids.</param>
        /// <param name="excludedCategoryIds">The excluded category ids.</param>
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
            string excludedCategoryIds = null )
        {
            Person currentPerson = GetPerson();

            var includedCategoryIdList = includedCategoryIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            var excludedCategoryIdList = excludedCategoryIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();

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

            var cachedEntityType = EntityTypeCache.Read( entityTypeId );
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
                    }
                }
            }

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

                var itemsQry = GetCategorizedItems( serviceInstance, parentItemId, showUnnamedEntityItems );
                if ( itemsQry != null )
                {
                    // do a ToList to load from database prior to ordering by name, just in case Name is a virtual property
                    var itemsList = itemsQry.ToList();

                    foreach ( var categorizedItem in itemsList.OrderBy( i => i.Name ) )
                    {
                        if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            var categoryItem = new CategoryItem();
                            categoryItem.Id = categorizedItem.Id.ToString();
                            categoryItem.Name = categorizedItem.Name;
                            categoryItem.IsCategory = false;
                            categoryItem.IconCssClass = categorizedItem.GetPropertyValue( "IconCssClass" ) as string ?? "fa fa-list-ol";
                            categoryItem.IconSmallUrl = string.Empty;
                            categoryItemList.Add( categoryItem );
                        }
                    }
                }
            }

            // try to figure out which items have viewable children
            foreach ( var g in categoryItemList )
            {
                if ( g.IsCategory )
                {
                    int parentId = int.Parse( g.Id );

                    foreach ( var childCategory in Get().Where( c => c.ParentCategoryId == parentId ) )
                    {
                        if ( childCategory.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            g.HasChildren = true;
                            break;
                        }
                    }

                    if ( !g.HasChildren )
                    {
                        if ( getCategorizedItems )
                        {
                            var childItems = GetCategorizedItems( serviceInstance, parentId, showUnnamedEntityItems );
                            if ( childItems != null )
                            {
                                foreach ( var categorizedItem in childItems )
                                {
                                    if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                                    {
                                        g.HasChildren = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ( !showCategoriesThatHaveNoChildren )
            {
                categoryItemList = categoryItemList.Where( a => !a.IsCategory || ( a.IsCategory && a.HasChildren ) ).ToList();
            }

            return categoryItemList.AsQueryable();
        }

        /// <summary>
        /// Gets the categorized items.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="categoryId">The category id.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <returns></returns>
        private IQueryable<ICategorized> GetCategorizedItems( IService serviceInstance, int categoryId, bool showUnnamedEntityItems )
        {
            if ( serviceInstance != null )
            {
                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );
                if ( getMethod != null )
                {
                    ParameterExpression paramExpression = serviceInstance.ParameterExpression;
                    MemberExpression propertyExpression = Expression.Property( paramExpression, "CategoryId" );
                    BinaryExpression compareExpression = null;
                    ConstantExpression constantExpression = Expression.Constant( categoryId );

                    if ( propertyExpression.Type == typeof( int? ) )
                    {
                        var zeroExpression = Expression.Constant( 0 );
                        var coalesceExpression = Expression.Coalesce( propertyExpression, zeroExpression );
                        compareExpression = Expression.Equal( coalesceExpression, constantExpression );
                    }
                    else
                    {
                        compareExpression = Expression.Equal( propertyExpression, constantExpression );
                    }

                    var result = getMethod.Invoke( serviceInstance, new object[] { paramExpression, compareExpression } ) as IQueryable<ICategorized>;

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
    }
}
