// <copyright>
// Copyright by the Central Christian Church
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
using com.centralaz.RoomManagement.Model;
using Rock.Data;
using Rock.Model;
using Rock.Rest;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    public partial class ScheduledResourcesController : Rock.Rest.ApiController<Rock.Model.Category>
    {
        public ScheduledResourcesController() : base( new Rock.Model.CategoryService( new Rock.Data.RockContext() ) ) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ScheduledResourcesController
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
        [System.Web.Http.Route( "api/ScheduledResources/GetChildren/{id}" )]
        public IQueryable<ScheduledCategoryItem> GetChildren(
            int id,
            int rootCategoryId = 0,
            bool getCategorizedItems = false,
            int? reservationId = null,
            string iCalendarContent = "",
            int? setupTime = null,
            int? cleanupTime = null,
            string entityQualifier = null,
            string entityQualifierValue = null,
            bool showUnnamedEntityItems = true,
            bool showCategoriesThatHaveNoChildren = true,
            string includedCategoryIds = null,
            string excludedCategoryIds = null,
            string defaultIconCssClass = null )
        {
            Person currentPerson = GetPerson();

            var includedCategoryIdList = includedCategoryIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            var excludedCategoryIdList = excludedCategoryIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            int entityTypeId = EntityTypeCache.Read( com.centralaz.RoomManagement.SystemGuid.EntityType.RESOURCE.AsGuid() ).Id;
            defaultIconCssClass = defaultIconCssClass ?? "fa fa-list-ol";

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

            qry = qry.Where( c => c.EntityTypeId == entityTypeId );

            ResourceService resourceService = new ResourceService( new RockContext() );

            List<Category> categoryList = qry.OrderBy( c => c.Order ).ThenBy( c => c.Name ).ToList();
            List<ScheduledCategoryItem> categoryItemList = new List<ScheduledCategoryItem>();

            foreach ( var category in categoryList )
            {
                if ( category.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    var scheduledCategoryItem = new ScheduledCategoryItem();
                    scheduledCategoryItem.Id = category.Id.ToString();
                    scheduledCategoryItem.Name = category.Name;
                    scheduledCategoryItem.IsCategory = true;
                    scheduledCategoryItem.IconCssClass = category.IconCssClass;
                    categoryItemList.Add( scheduledCategoryItem );
                }
            }

            if ( getCategorizedItems )
            {
                var rockContext = new RockContext();
                var reservationResourceService = new ReservationResourceService( rockContext );
                // if id is zero and we have a rootCategory, show the children of that rootCategory (but don't show the rootCategory)
                int parentItemId = id == 0 ? rootCategoryId : id;

                var newReservation = new Reservation() { Id = reservationId ?? 0, Schedule = new Schedule() { iCalendarContent = iCalendarContent }, SetupTime = setupTime, CleanupTime = cleanupTime };

                var resourceQry = resourceService.Queryable();
                if ( resourceQry.Where( r => r.CategoryId == parentItemId ) != null )
                {
                    // do a ToList to load from database prior to ordering by name, just in case Name is a virtual property
                    var itemsList = resourceQry.Where( r => r.CategoryId == parentItemId ).ToList();

                    foreach ( var categorizedItem in itemsList.OrderBy( i => i.Name ) )
                    {
                        if ( categorizedItem != null && categorizedItem.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            var availableQuantity = reservationResourceService.GetAvailableResourceQuantity( categorizedItem, newReservation );

                            var scheduledCategoryItem = new ScheduledCategoryItem();
                            scheduledCategoryItem.Id = categorizedItem.Id.ToString();
                            scheduledCategoryItem.Name = String.Format( "{0} ({1})", categorizedItem.Name, availableQuantity );
                            scheduledCategoryItem.IsCategory = false;
                            scheduledCategoryItem.IconCssClass = categorizedItem.GetPropertyValue( "IconCssClass" ) as string ?? defaultIconCssClass;
                            scheduledCategoryItem.IconSmallUrl = string.Empty;
                            scheduledCategoryItem.IsActive = availableQuantity > 0;
                            categoryItemList.Add( scheduledCategoryItem );
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
                            var childItems = GetCategorizedItems( resourceService, parentId, showUnnamedEntityItems );
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
        private IQueryable<Resource> GetCategorizedItems( IService serviceInstance, int categoryId, bool showUnnamedEntityItems )
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

                    var result = getMethod.Invoke( serviceInstance, new object[] { paramExpression, compareExpression } ) as IQueryable<Resource>;

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
    public class ScheduledCategoryItem : Rock.Web.UI.Controls.TreeViewItem
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
