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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GroupsController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "GroupsGetChildren",
                routeTemplate: "api/Groups/GetChildren/{id}/{rootGroupId}/{limitToSecurityRoleGroups}/{groupTypeIds}",
                defaults: new
                {
                    controller = "Groups",
                    action = "GetChildren"
                } );

            routes.MapHttpRoute(
                name: "GroupsMapInfo",
                routeTemplate: "api/Groups/GetMapInfo/{groupId}",
                defaults: new
                {
                    controller = "Groups",
                    action = "GetMapInfo"
                } );

            routes.MapHttpRoute(
                name: "GroupsChildMapInfo",
                routeTemplate: "api/Groups/GetMapInfo/{groupId}/Children",
                defaults: new
                {
                    controller = "Groups",
                    action = "GetChildMapInfo"
                } );

            routes.MapHttpRoute(
                name: "GroupsMemberMapInfo",
                routeTemplate: "api/Groups/GetMapInfo/{groupId}/Members",
                defaults: new
                {
                    controller = "Groups",
                    action = "GetMemberMapInfo"
                } );

            routes.MapHttpRoute(
                name: "GroupsFamiliesMapInfo",
                routeTemplate: "api/Groups/GetMapInfo/{groupId}/Families/{statusId}",
                defaults: new
                {
                    controller = "Groups",
                    action = "GetFamiliesMapInfo"
                } );

            routes.MapHttpRoute(
               name: "GroupsMapInfoWindow",
               routeTemplate: "api/Groups/GetMapInfoWindow/{groupId}/{locationId}",
               defaults: new
               {
                   controller = "Groups",
                   action = "GetMapInfoWindow"
               } );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rootGroupId">The root group id.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, string groupTypeIds )
        {
            var qry = ((GroupService)Service).GetNavigationChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIds );

            List<Group> groupList = new List<Group>();
            List<TreeViewItem> groupNameList = new List<TreeViewItem>();

            var person = GetPerson();

            foreach ( var group in qry.OrderBy( g => g.Name ) )
            {
                if ( group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    groupList.Add( group );
                    var treeViewItem = new TreeViewItem();
                    treeViewItem.Id = group.Id.ToString();
                    treeViewItem.Name = group.Name;

                    // if there a IconCssClass is assigned, use that as the Icon.
                    var groupType = Rock.Web.Cache.GroupTypeCache.Read( group.GroupTypeId );
                    if ( groupType != null )
                    {
                        treeViewItem.IconCssClass = groupType.IconCssClass;
                    }

                    groupNameList.Add( treeViewItem );
                }
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = groupList.Select( a => a.Id ).ToList();

            var qryHasChildren = from x in Get().Select( a => a.ParentGroupId )
                                    where resultIds.Contains( x.Value )
                                    select x.Value;

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var g in groupNameList )
            {
                int groupId = int.Parse( g.Id );
                g.HasChildren = qryHasChildrenList.Any( a => a == groupId );
            }

            return groupNameList.AsQueryable();
        }

        /// <summary>
        /// Gets the map information.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        public IQueryable<MapItem> GetMapInfo( int groupId )
        {
            var group = ( (GroupService)Service ).Queryable("GroupLocations.Location")
                .Where(g => g.Id == groupId)
                .FirstOrDefault();

            if (group != null)
            {
                var person = GetPerson();

                if (group.IsAuthorized( Rock.Security.Authorization.VIEW, person ))
                {
                    var mapItems = new List<MapItem>();
                    foreach ( var location in group.GroupLocations
                        .Where( l => l.Location.GeoPoint != null || l.Location.GeoFence != null )
                        .Select( l => l.Location ) )
                    {
                        var mapItem = new MapItem( location );
                        mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                        mapItem.EntityId = group.Id;
                        mapItem.Name = group.Name;
                        if ( mapItem.Point != null || mapItem.PolygonPoints.Any() )
                        {
                            mapItems.Add( mapItem );
                        }
                    }

                    return mapItems.AsQueryable();
                }
                else
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }

        [Authenticate, Secured]
        public IQueryable<MapItem> GetChildMapInfo( int groupId )
        {
            var person = GetPerson();

            var mapItems = new List<MapItem>();

            foreach ( var group in ( (GroupService)Service ).Queryable( "GroupLocations.Location" )
                .Where( g => g.ParentGroupId == groupId ))
            {
                if ( group != null && group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    foreach ( var location in group.GroupLocations
                        .Where( l => l.Location.GeoPoint != null || l.Location.GeoFence != null )
                        .Select( l => l.Location ) )
                    {
                        var mapItem = new MapItem( location );
                        mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                        mapItem.EntityId = group.Id;
                        mapItem.Name = group.Name;
                        if ( mapItem.Point != null || mapItem.PolygonPoints.Any() )
                        {
                            mapItems.Add( mapItem );
                        }
                    }

                }
            }

            return mapItems.AsQueryable();
        }

        [Authenticate, Secured]
        public IQueryable<MapItem> GetMemberMapInfo( int groupId )
        {
            var group = ( (GroupService)Service ).Queryable( "Members" )
                .Where( g => g.Id == groupId )
                .FirstOrDefault();

            if ( group != null )
            {
                var person = GetPerson();

                if ( group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var mapItems = new List<MapItem>();

                    Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                    var memberIds = group.Members.Select( m => m.PersonId ).Distinct().ToList();
                    var families = ( (GroupService)Service ).Queryable( "GroupLocations.Location" )
                        .Where( g => 
                            g.GroupType.Guid == familyGuid &&
                            g.Members.Any( m => memberIds.Contains(m.PersonId)))
                        .Distinct();

                    foreach ( var family in families )
                    {
                        foreach ( var location in family.GroupLocations
                            .Where( g => g.IsMappedLocation && g.Location.GeoPoint != null )
                            .Select( g => g.Location ) )
                        {
                            var mapItem = new MapItem( location );
                            mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                            mapItem.EntityId = family.Id;
                            mapItem.Name = family.Name;
                            if ( mapItem.Point != null || mapItem.PolygonPoints.Any() )
                            {
                                mapItems.Add( mapItem );
                            }
                        }
                    }

                    return mapItems.AsQueryable();
                }
                else
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }

        [Authenticate, Secured]
        public IQueryable<MapItem> GetFamiliesMapInfo( int groupId, int statusId )
        {
            var group = ( (GroupService)Service ).Queryable( "GroupLocations.Location" )
                .Where( g => g.Id == groupId )
                .FirstOrDefault();

            if ( group != null )
            {
                var person = GetPerson();

                if ( group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var mapItems = new List<MapItem>();

                    foreach ( var location in group.GroupLocations
                        .Where( l => l.Location.GeoFence != null )
                        .Select( l => l.Location ) )
                    {
                        var familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                        var families = new GroupLocationService( (RockContext)Service.Context ).Queryable()
                            .Where( l =>
                                l.Group.GroupType.Guid.Equals( familyGuid ) &&
                                l.Location.GeoPoint.Intersects( location.GeoFence ) &&
                                l.Group.Members.Any( m =>
                                    m.Person.ConnectionStatusValueId.HasValue &&
                                    m.Person.ConnectionStatusValueId.Value == statusId ) )
                            .Select( l => new
                            {
                                l.Location,
                                l.Group.Id,
                                l.Group.Name,
                                MinStatus = l.Group.Members
                                    .Where( m => m.Person.ConnectionStatusValueId.HasValue )
                                    .OrderBy( m => m.Person.ConnectionStatusValue.Order )
                                    .Select( m => m.Person.ConnectionStatusValue.Id )
                                    .FirstOrDefault()
                            } );
                            
                        foreach( var family in families.Where( f => f.MinStatus == statusId ))
                        {
                            var mapItem = new MapItem( family.Location );
                            mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                            mapItem.EntityId = family.Id;
                            mapItem.Name = family.Name;
                            if ( mapItem.Point != null || mapItem.PolygonPoints.Any() )
                            {
                                mapItems.Add( mapItem );
                            }
                        }
                    }

                    return mapItems.AsQueryable();
                }
                else
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }

        [Authenticate, Secured]
        [HttpPost]
        public InfoWindowResult GetMapInfoWindow( int groupId, int locationId, [FromBody] InfoWindowRequest infoWindowDetails )
        {
            // Use new service with new context so properties can be navigated by liquid
            var group = new GroupService( new RockContext() ).Queryable( "GroupLocations.Location" )
                .Where( g => g.Id == groupId )
                .FirstOrDefault();

            if ( group != null )
            {
                var person = GetPerson();

                if ( group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    string infoWindow = group.Name;

                    if ( infoWindowDetails != null )
                    {
                        var groupPageParams = new Dictionary<string, string>();
                        groupPageParams.Add( "GroupId", group.Id.ToString() );
                        var groupDetailUrl = new PageReference( infoWindowDetails.GroupPage, groupPageParams ).BuildUrl();

                        var personPageParams = new Dictionary<string, string>();
                        personPageParams.Add( "PersonId", string.Empty );
                        var personProfileUrl = new PageReference( infoWindowDetails.PersonProfilePage, personPageParams ).BuildUrl();

                        var mapPageParams = new Dictionary<string, string>();
                        mapPageParams.Add( "GroupId", group.Id.ToString() );
                        var groupMapUrl = new PageReference( infoWindowDetails.MapPage, mapPageParams ).BuildUrl();

                        var grouplocation = group.GroupLocations
                            .Where( g => g.LocationId == locationId )
                            .FirstOrDefault();

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "GroupDetailUrl", groupDetailUrl );
                        mergeFields.Add( "PersonProfileUrl", personProfileUrl );
                        mergeFields.Add( "GroupMapUrl", groupMapUrl );
                        mergeFields.Add( "Group", group );
                        mergeFields.Add( "GroupLocation", grouplocation );

                        infoWindow = System.Web.HttpUtility.HtmlDecode( infoWindowDetails.Template as string );
                        infoWindow = infoWindow.ResolveMergeFields( mergeFields );
                    }

                    return new InfoWindowResult( infoWindow );
                }
                else
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }

        public class InfoWindowRequest
        {
            public string GroupPage { get; set; }
            public string PersonProfilePage { get; set; }
            public string MapPage { get; set; }
            public string Template { get; set; }
        }

        public class InfoWindowResult
        {
            public string Result { get; set; }
            public InfoWindowResult(string result)
            {
                Result = result;
            }
        }

    }
}

    
