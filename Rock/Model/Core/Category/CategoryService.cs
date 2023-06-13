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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Rock.Attribute;
using Rock.Data;
using Rock.Model.Core.Category.Options;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Category"/> objects.
    /// </summary>
    public partial class CategoryService
    {
        #region Default Options

        /// <summary>
        /// The default child category query options.
        /// </summary>
        private static ChildCategoryQueryOptions DefaultChildCategoryQueryOptions = new ChildCategoryQueryOptions();

        /// <summary>
        /// The default categorized item query options.
        /// </summary>
        private static CategorizedItemQueryOptions DefaultCategorizedItemQueryOptions = new CategorizedItemQueryOptions();

        #endregion

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Category">Categories</see> by parent <see cref="Rock.Model.Category"/> and <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="parentId">A <see cref="System.Int32"/> representing the CategoryID of the parent <see cref="Rock.Model.Category"/> to search by. To find <see cref="Rock.Model.Category">Categories</see>
        /// that do not inherit from a parent category, this value will be null.</param>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Category">Categories</see> that meet the specified criteria. </returns>
        public IQueryable<Category> Get( int? parentId, int? entityTypeId )
        {
            var query = Queryable()
                .Where( c => ( c.ParentCategoryId ?? 0 ) == ( parentId ?? 0 ) );

            if ( entityTypeId.HasValue )
            {
                query = query.Where( c => c.EntityTypeId == entityTypeId.Value );
            }

            return query
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Category">Categories</see> by Name, <see cref="Rock.Model.EntityType"/>, Qualifier Column and Qualifier Value.
        /// </summary>
        /// <param name="name">A <see cref="System.String"/> representing the name to search by.</param>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityType of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <param name="entityTypeQualifierColumn">A <see cref="System.String"/> representing the name of the Qualifier Column to search by.</param>
        /// <param name="entityTypeQualifierValue">A <see cref="System.String"/> representing the name of the Qualifier Value to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Category">Categories</see> that meet the search criteria.</returns>
        public IQueryable<Category> Get( string name, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            return Queryable()
                .Where( c =>
                    string.Compare( c.Name, name, true ) == 0 &&
                    c.EntityTypeId == entityTypeId &&
                    c.EntityTypeQualifierColumn == entityTypeQualifierColumn &&
                    c.EntityTypeQualifierValue == entityTypeQualifierValue );
        }

        /// <summary>
        /// Returns a enumerable collection of <see cref="Rock.Model.Category">Categories</see> by <see cref="Rock.Model.EntityType"/>
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Category">Categories</see> are used for the specified <see cref="Rock.Model.Category"/>.</returns>
        public IQueryable<Category> GetByEntityTypeId( int? entityTypeId )
        {
            return Queryable()
                .Where( t => ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue ) ) )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Category">Category</see> that are descendants of a <see cref="Rock.Model.Category" />
        /// </summary>
        /// <param name="parentCategoryId">A <see cref="System.Int32" /> representing the Id of the <see cref="Rock.Model.Category" /></param>
        /// <returns>
        /// A collection of <see cref="Rock.Model.Category" /> entities that are descendants of the provided parent <see cref="Rock.Model.Category" />.
        /// </returns>
        public IEnumerable<Category> GetAllDescendents( int parentCategoryId )
        {
            return ExecuteQuery(
                @"
                with CTE as (
                select * from [Category] where [ParentCategoryId]={0}
                union all
                select [a].* from [Category] [a]
                inner join CTE pcte on pcte.Id = [a].[ParentCategoryId]
                )
                select * from CTE
                ", parentCategoryId );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Category">Category</see> that are descendants of a <see cref="Rock.Model.Category" />
        /// </summary>
        /// <param name="parentCategoryGuid">The parent category unique identifier.</param>
        /// <returns>
        /// A collection of <see cref="Rock.Model.Category" /> entities that are descendants of the provided parent <see cref="Rock.Model.Category" />.
        /// </returns>
        public IEnumerable<Category> GetAllDescendents( Guid parentCategoryGuid )
        {
            var parentCategory = this.Get( parentCategoryGuid );
            return GetAllDescendents( parentCategory != null ? parentCategory.Id : 0 );
        }

        /// <summary>
        /// Gets all ancestors for the provided Category ID.
        /// If the ID is available use it over the GUID and the GUID overload finds the ID
        /// and then executes this method.
        /// </summary>
        /// <param name="childCategoryId">The child category identifier.</param>
        /// <returns></returns>
        public IEnumerable<Category> GetAllAncestors( int childCategoryId )
        {
            return ExecuteQuery( $@"
                WITH CTE AS (
	                SELECT *
	                FROM [Category]
	                WHERE [Id] = {childCategoryId}
	                UNION ALL
	                SELECT c.*
	                FROM [Category] c
	                JOIN CTE on c.[Id] = CTE.ParentCategoryId
                )

                SELECT * FROM CTE
                ORDER BY [ParentCategoryId]" );
        }

        /// <summary>
        /// Gets all ancestors for the provided Category GUID.
        /// If the ID is available then use the ID overload instead. This method
        /// looks up the ID and then executes the ID overload.
        /// </summary>
        /// <param name="childCategoryGuid">The child category unique identifier.</param>
        /// <returns></returns>
        public IEnumerable<Category> GetAllAncestors( Guid childCategoryGuid )
        {
            var childCategory = this.Get( childCategoryGuid );
            return GetAllAncestors( childCategory != null ? childCategory.Id : 0 );
        }

        /// <summary>
        /// Gets the ancestors for the provided Category IDs.
        /// </summary>
        /// <param name="level">The level of ancestors to retrieve (1 = parent, 2 = grandparent, etc.)</param>
        /// <param name="childCategoryIds">The child category identifiers.</param>
        /// <returns></returns>
        [RockInternal( "1.15.2" )]
        internal IEnumerable<Category> GetAncestors( int level, params int[] childCategoryIds )
        {
            return ExecuteQuery( $@"
                WITH CTE AS (
	                SELECT *, 0 AS [DepthLevel]
	                FROM [Category]
	                WHERE [Id] IN ({string.Join( ", ", childCategoryIds )})
	                UNION ALL
	                SELECT c.*, CTE.[DepthLevel] + 1 AS [DepthLevel]
	                FROM [Category] c
	                JOIN CTE on c.[Id] = CTE.[ParentCategoryId]
                    WHERE CTE.[DepthLevel] < {level}
                )
                SELECT * FROM CTE
                ORDER BY [DepthLevel] DESC" );
        }

        /// <summary>
        /// Gets all of the categories that were selected and optionally all the child categories of the selected categories
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="selectedCategoryGuids">The selected category guids.</param>
        /// <param name="includeAllChildren">if set to <c>true</c> will include all the child categories of any selected category.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public List<CategoryNavigationItem> GetNavigationItems( int? entityTypeId, List<Guid> selectedCategoryGuids, bool includeAllChildren, Person currentPerson )
        {
            var allCategories = GetByEntityTypeId( entityTypeId ).ToList();
            return GetNavigationChildren( null, allCategories, selectedCategoryGuids, selectedCategoryGuids.Any(), includeAllChildren, currentPerson );
        }

        /// <summary>
        /// Gets the navigation children.
        /// </summary>
        /// <param name="parentCategoryId">The parent category identifier.</param>
        /// <param name="categories">The categories.</param>
        /// <param name="selectedCategoryGuids">The selected category guids.</param>
        /// <param name="checkSelected">if set to <c>true</c> [check selected].</param>
        /// <param name="includeAllChildren">if set to <c>true</c> [include all children].</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        private List<CategoryNavigationItem> GetNavigationChildren( int? parentCategoryId, IEnumerable<Category> categories, List<Guid> selectedCategoryGuids, bool checkSelected, bool includeAllChildren, Person currentPerson )
        {
            var items = new List<CategoryNavigationItem>();

            foreach ( var category in categories
                .Where( c =>
                    c.ParentCategoryId == parentCategoryId ||
                    ( !c.ParentCategoryId.HasValue && !parentCategoryId.HasValue ) )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name ) )
            {
                if ( category.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    bool includeCategory = !checkSelected || selectedCategoryGuids.Contains( category.Guid );
                    bool checkChildSelected = checkSelected;

                    if ( includeCategory )
                    {
                        if ( checkSelected && includeAllChildren )
                        {
                            checkChildSelected = false;
                        }

                        var categoryItem = new CategoryNavigationItem( category );
                        items.Add( categoryItem );

                        // Recurse child categories
                        categoryItem.ChildCategories = GetNavigationChildren( category.Id, categories, selectedCategoryGuids, checkChildSelected, includeAllChildren, currentPerson );
                    }
                    else
                    {
                        foreach ( var categoryItem in GetNavigationChildren( category.Id, categories, selectedCategoryGuids, checkChildSelected, includeAllChildren, currentPerson ) )
                        {
                            items.Add( categoryItem );
                        }
                    }
                }

            }

            return items;
        }

        /// <summary>
        /// Gets the Guid for the Category that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = CategoryCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

        /// <summary>
        /// Gets the child category query that contains the child categories that
        /// match the specified options.
        /// </summary>
        /// <param name="options">The options for which child categories to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"/> of <see cref="Category"/> objects that matches the request.</returns>
        public IQueryable<Category> GetChildCategoryQuery( ChildCategoryQueryOptions options = null )
        {
            options = options ?? DefaultChildCategoryQueryOptions;

            // Initialize the basic query.
            var qry = Queryable().AsNoTracking();

            // Specify the "root" of our query from the ParentGuid if one was
            // set.
            if ( options.ParentGuid.HasValue )
            {
                qry = qry.Where( c => c.ParentCategory.Guid == options.ParentGuid.Value );
            }
            else
            {
                qry = qry.Where( c => c.ParentCategoryId == null );
            }

            // Limit the results to either IncludeCategoryGuids or ExcludeCategoryGuids.
            // The two options are exclusive and IncludeCategoryGuids takes precedence.
            if ( options.IncludeCategoryGuids?.Any() ?? false )
            {
                qry = qry.Where( c => options.IncludeCategoryGuids.Contains( c.Guid ) );
            }
            else if ( options.ExcludeCategoryGuids?.Any() ?? false )
            {
                qry = qry.Where( c => !options.ExcludeCategoryGuids.Contains( c.Guid ) );
            }

            // If we have been requested to limit the results to a specific entity
            // type then apply those filters.
            if ( options.EntityTypeGuid.HasValue )
            {
                qry = qry.Where( c => c.EntityType.Guid == options.EntityTypeGuid.Value );
            }

            // If they specified a qualifier column then make sure the results
            // match those specifications.
            if ( options.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( c => string.Compare( c.EntityTypeQualifierColumn, options.EntityTypeQualifierColumn, true ) == 0 );

                if ( options.EntityTypeQualifierValue.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( c => string.Compare( c.EntityTypeQualifierValue, options.EntityTypeQualifierValue, true ) == 0 );
                }
                else
                {
                    qry = qry.Where( c => c.EntityTypeQualifierValue == null || c.EntityTypeQualifierValue == string.Empty );
                }
            }

            return qry;
        }

        /// <summary>
        /// Gets the categorized item query that matches the options.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="options">The options that describe the current operation.</param>
        /// <returns>A queryable of items in this category.</returns>
        public IQueryable<ICategorized> GetCategorizedItemQuery( IService serviceInstance, CategorizedItemQueryOptions options )
        {
            options = options ?? DefaultCategorizedItemQueryOptions;

            if ( serviceInstance != null )
            {
                var getMethod = serviceInstance.GetType()
                    .GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );

                if ( getMethod != null )
                {
                    var paramExpression = serviceInstance.ParameterExpression;
                    BinaryExpression whereExpression;

                    // Build the expression that checks the categoryGuid value for
                    // a match.
                    var categoryPropertyExpression = Expression.Property( paramExpression, "Category" );
                    var categoryGuidPropertyExpression = Expression.Property( categoryPropertyExpression, "Guid" );

                    if ( options.CategoryGuid.HasValue )
                    {
                        var categoryConstantExpression = Expression.Constant( options.CategoryGuid );
                        whereExpression = Expression.Equal( categoryGuidPropertyExpression, categoryConstantExpression );
                    }
                    else
                    {
                        var categoryIdPropertyExpression = Expression.Property( paramExpression, "CategoryId" );

                        if ( categoryIdPropertyExpression.Type == typeof( int ) )
                        {
                            // Some models, like MergeTemplate, actually use a non-nullable
                            // column and then a special C# property to map the nullable
                            // ICategorized.CategoryId to the real property. This handles
                            // those situations.
                            var zeroConstant = Expression.Constant( 0, typeof( int ) );

                            whereExpression = Expression.Equal( categoryIdPropertyExpression, zeroConstant );
                        }
                        else
                        {
                            var nullConstant = Expression.Constant( null, typeof( int? ) );

                            whereExpression = Expression.Equal( categoryIdPropertyExpression, nullConstant );
                        }
                    }

                    // Exclude any inactive items. This value is only set if we
                    // have already determined that the entity has an IsActive
                    // property to query.
                    if ( !options.IncludeInactiveItems && typeof( IHasActiveFlag ).IsAssignableFrom( paramExpression.Type ) )
                    {
                        var isActivePropertyExpression = Expression.Property( paramExpression, "IsActive" );
                        var isActiveConstantExpression = Expression.Convert( Expression.Constant( true ), isActivePropertyExpression.Type );
                        var isActiveExpression = Expression.Equal( isActivePropertyExpression, isActiveConstantExpression );
                        whereExpression = Expression.And( whereExpression, isActiveExpression );
                    }

                    // Custom filtering based on property names and values provided
                    // by the caller.
                    if ( !string.IsNullOrEmpty( options.ItemFilterPropertyName ) )
                    {
                        var itemFilterPropertyNameExpression = Expression.Property( paramExpression, options.ItemFilterPropertyName );
                        ConstantExpression itemFilterPropertyValueExpression;

                        // Special checks for integer and Guid values supplied
                        // by the caller. Otherwise treat it as a string.
                        if ( itemFilterPropertyNameExpression.Type == typeof( int? ) || itemFilterPropertyNameExpression.Type == typeof( int ) )
                        {
                            itemFilterPropertyValueExpression = Expression.Constant( options.ItemFilterPropertyValue.AsIntegerOrNull(), typeof( int? ) );
                        }
                        else if ( itemFilterPropertyNameExpression.Type == typeof( Guid? ) || itemFilterPropertyNameExpression.Type == typeof( Guid ) )
                        {
                            itemFilterPropertyValueExpression = Expression.Constant( options.ItemFilterPropertyValue.AsGuidOrNull(), typeof( Guid? ) );
                        }
                        else
                        {
                            itemFilterPropertyValueExpression = Expression.Constant( options.ItemFilterPropertyValue );
                        }

                        // Build the comparison as a standard equality check.
                        var binaryExpression = Expression.Equal( itemFilterPropertyNameExpression, itemFilterPropertyValueExpression );

                        whereExpression = Expression.And( whereExpression, binaryExpression );
                    }

                    var result = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression } ) as IQueryable<ICategorized>;

                    // If they don't want to include entities without a name
                    // then exclude them from the results.
                    if ( !options.IncludeUnnamedEntityItems )
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
    /// Helper class used to return navigation tree of selected categories
    /// </summary>
    public class CategoryNavigationItem
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public Category Category { get; set; }

        /// <summary>
        /// Gets or sets the child categories.
        /// </summary>
        /// <value>
        /// The child categories.
        /// </value>
        public List<CategoryNavigationItem> ChildCategories { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryNavigationItem"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        public CategoryNavigationItem( Category category )
        {
            Category = category;
        }
    }
}
