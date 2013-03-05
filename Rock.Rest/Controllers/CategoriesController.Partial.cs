//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    public partial class CategoriesController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "CategoriesGetChildren",
                routeTemplate: "api/Categories/GetChildren/{id}/{entityTypeName}",
                defaults: new
                {
                    controller = "Categories",
                    action = "GetChildren"
                } );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        [Authenticate]
        public IQueryable<CategoryItem> GetChildren( int id, string entityTypeName )
        {
            var user = CurrentUser();
            Person currentPerson = user != null ? user.Person : null;

            IQueryable<Category> qry;
            qry = Get().Where( a => ( a.ParentCategoryId ?? 0 ) == id );

            int? entityTypeId = null;

            object serviceInstance = null;

            if ( !string.IsNullOrWhiteSpace( entityTypeName ) )
            {
                var cachedEntityType = EntityTypeCache.Read( entityTypeName );
                if ( cachedEntityType != null )
                {
                    entityTypeId = cachedEntityType.Id;
                    qry = qry.Where( a => a.EntityTypeId == entityTypeId );

                    // Get the GetByCategory method
                    if ( cachedEntityType.AssemblyName != null )
                    {
                        Type entityType = cachedEntityType.GetEntityType();
                        if ( entityType != null )
                        {
                            Type[] modelType = { entityType };
                            Type genericServiceType = typeof( Rock.Data.Service<> );
                            Type modelServiceType = genericServiceType.MakeGenericType( modelType );

                            serviceInstance = Activator.CreateInstance( modelServiceType );
                        }
                    }
                }
            }

            List<Category> categoryList = qry.ToList();
            List<CategoryItem> categoryItemList = new List<CategoryItem>();

            var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
            string imageUrlFormat = Path.Combine( appPath, "Image.ashx?id={0}&width=25&height=25" );

            foreach ( var category in categoryList )
            {
                if ( category.IsAuthorized( "View", currentPerson ) )
                {
                    var categoryItem = new CategoryItem();
                    categoryItem.Id = category.Id;
                    categoryItem.Name = System.Web.HttpUtility.HtmlEncode( category.Name );
                    categoryItem.IsCategory = true;

                    // if there a IconCssClass is assigned, use that as the Icon.  Otherwise, use the SmallIcon (if assigned)
                    if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
                    {
                        categoryItem.IconCssClass = category.IconCssClass;
                    }
                    else
                    {
                        categoryItem.IconSmallUrl = category.IconSmallFileId != null ? string.Format( imageUrlFormat, category.IconSmallFileId ) : string.Empty;
                    }

                    categoryItemList.Add( categoryItem );
                }
            }

            IQueryable items = GetCategorizedItems( serviceInstance, id ) as IQueryable;
            if (items != null)
            {
                foreach(var item in items)
                {
                    ICategorized categorizedItem = item as ICategorized;
                    if ( categorizedItem != null && categorizedItem.IsAuthorized("View", currentPerson ) )
                    {
                        var categoryItem = new CategoryItem();
                        categoryItem.Id = categorizedItem.Id;
                        categoryItem.Name = categorizedItem.Name;
                        categoryItem.IsCategory = false;
                        categoryItem.IconCssClass = "icon-list-ol";
                        categoryItem.IconSmallUrl = string.Empty;
                        categoryItemList.Add( categoryItem );
                    }
                }
            }

            // try to figure out which items have viewable children
            foreach ( var g in categoryItemList )
            {
                if ( g.IsCategory )
                {
                    foreach ( var childCategory in Get().Where( c => c.ParentCategoryId == g.Id ) )
                    {
                        if ( childCategory.IsAuthorized( "View", currentPerson ) )
                        {
                            g.HasChildren = true;
                            break;
                        }
                    }

                    if ( !g.HasChildren )
                    {
                        IQueryable childItems = GetCategorizedItems( serviceInstance, g.Id ) as IQueryable;
                        if ( childItems != null )
                        {
                            foreach ( var item in childItems )
                            {
                                ICategorized categorizedItem = item as ICategorized;
                                if ( categorizedItem != null && categorizedItem.IsAuthorized( "View", currentPerson ) )
                                {
                                    g.HasChildren = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return categoryItemList.AsQueryable();
        }

        private object GetCategorizedItems( object serviceInstance, int categoryId )
        {
            if ( serviceInstance != null )
            {
                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );
                if ( getMethod != null )
                {
                    var paramExpression = serviceInstance.GetType().GetProperty( "ParameterExpression" ).GetValue( serviceInstance ) as ParameterExpression;
                    var propertyExpreesion = Expression.Property( paramExpression, "CategoryId" );
                    var zeroExpression = Expression.Constant( 0 );
                    var coalesceExpression = Expression.Coalesce( propertyExpreesion, zeroExpression );
                    var constantExpression = Expression.Constant( categoryId );
                    var compareExpression = Expression.Equal( coalesceExpression, constantExpression );

                    return getMethod.Invoke( serviceInstance, new object[] { paramExpression, compareExpression } );
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CategoryItem
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is category.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is category; otherwise, <c>false</c>.
        /// </value>
        public bool IsCategory { get; set; }

        /// <summary>
        /// Gets or sets the group type icon CSS class.
        /// </summary>
        /// <value>
        /// The group type icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the group type icon small id.
        /// </summary>
        /// <value>
        /// The group type icon small id.
        /// </value>
        public string IconSmallUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren { get; set; }
    }
}
