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
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

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
    public partial class GroupsController
    {
        /// <summary>
        /// Gets the children (obsolete, use the other GetChildren method)
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rootGroupId">The root group id.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetChildren/{id}/{rootGroupId}/{limitToSecurityRoleGroups}/{groupTypeIds}" )]
        [Obsolete( "use the other GetChildren" )]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, string groupTypeIds )
        {
            return GetChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIds, "" );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootGroupId">The root group identifier.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="includedGroupTypeIds">The included group type ids.</param>
        /// <param name="excludedGroupTypeIds">The excluded group type ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootGroupId = 0, bool limitToSecurityRoleGroups = false, string includedGroupTypeIds = "", string excludedGroupTypeIds = "", bool includeInactiveGroups = false )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            var includedGroupTypeIdList = includedGroupTypeIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            var excludedGroupTypeIdList = excludedGroupTypeIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();

            var groupService = (GroupService)Service;
            
            // if specific group types are specified, show the groups regardless of ShowInNavigation
            bool limitToShowInNavigation = !includedGroupTypeIdList.Any();

            var qry = groupService.GetChildren( id, rootGroupId, limitToSecurityRoleGroups, includedGroupTypeIdList, excludedGroupTypeIdList, includeInactiveGroups, limitToShowInNavigation );

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
            var qryHasChildren = Get()
                .Where( g =>
                    g.ParentGroupId.HasValue &&
                    resultIds.Contains( g.ParentGroupId.Value ) );

            if ( includedGroupTypeIdList.Any() )
            {
                qryHasChildren = qryHasChildren.Where( a => includedGroupTypeIdList.Contains( a.GroupTypeId ) );
            }
            else if ( excludedGroupTypeIdList.Any() )
            {
                qryHasChildren = qryHasChildren.Where( a => !excludedGroupTypeIdList.Contains( a.GroupTypeId ) );
            }

            var qryHasChildrenList = qryHasChildren
                .Select( g => g.ParentGroupId.Value )
                .Distinct()
                .ToList();

            foreach ( var g in groupNameList )
            {
                int groupId = int.Parse( g.Id );
                g.HasChildren = qryHasChildrenList.Any( a => a == groupId );
            }

            return groupNameList.AsQueryable();
        }

        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery(MaxExpansionDepth=4)]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/GetFamilies/{personId}" )]
        public IQueryable<Group> GetFamilies( int personId )
        {
            return new PersonService( (RockContext)Service.Context ).GetFamilies( personId );
        }

        /// <summary>
        /// Gets the families by name search.
        /// </summary>
        /// <param name="searchString">String to use for search.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/GetFamiliesByPersonNameSearch/{searchString}" )]
        public IQueryable<FamilySearchResult> GetFamiliesByPersonNameSearch( string searchString )
        {
            return GetFamiliesByPersonNameSearch( searchString, 20 );
        }

        /// <summary>
        /// Gets the families by name search.
        /// </summary>
        /// <param name="searchString">String to use for search.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/GetFamiliesByPersonNameSearch/{searchString}/{maxResults}" )]
        public IQueryable<FamilySearchResult> GetFamiliesByPersonNameSearch( string searchString, int maxResults = 20 )
        {
            bool reversed;

            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );
            Guid homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

            // get list of people matching the search string
            IOrderedQueryable<Person> sortedPersonQry = personService
                .GetByFullNameOrdered( searchString, true, false, false, out reversed );

            var personResults = sortedPersonQry.AsNoTracking().ToList();

            List<FamilySearchResult> familyResults = new List<FamilySearchResult>();
            foreach (var person in personResults){
                var families = personService.GetFamilies( person.Id )
                                    .Select( f => new FamilySearchResult
                                                        { 
                                                            Id = f.Id,
                                                            Name = f.Name,
                                                            FamilyMembers = f.Members.ToList(),
                                                            HomeLocation = f.GroupLocations
                                                                            .Where( l => l.GroupLocationTypeValue.Guid == homeAddressGuid )
                                                                            .OrderByDescending( l => l.IsMailingLocation)
                                                                            .Select(l => l.Location)
                                                                            .FirstOrDefault(),
                                                            MainPhone = f.Members
                                                                            .OrderBy(m => m.GroupRole.Order)
                                                                            .ThenBy(m => m.Person.Gender)
                                                                            .FirstOrDefault()
                                                                            .Person.PhoneNumbers.OrderBy( p => p.NumberTypeValue.Order).FirstOrDefault()
                                                        })
                                                        .ToList();

                foreach ( var family in families) {
                    familyResults.Add( family );
                }
            }

            return familyResults.DistinctBy(f => f.Id).AsQueryable(); 
        }


        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/GetFamily/{familyId}" )]
        public FamilySearchResult GetFamily( int familyId )
        {
            RockContext rockContext = new RockContext();
            Guid homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

            return new GroupService( rockContext ).Queryable().AsNoTracking()
                                                    .Where( g=> g.Id == familyId)
                                                    .Select( f => new FamilySearchResult
                                                        {
                                                            Id = f.Id,
                                                            Name = f.Name,
                                                            FamilyMembers = f.Members.ToList(),
                                                            HomeLocation = f.GroupLocations
                                                                            .Where( l => l.GroupLocationTypeValue.Guid == homeAddressGuid )
                                                                            .OrderByDescending( l => l.IsMailingLocation )
                                                                            .Select( l => l.Location )
                                                                            .FirstOrDefault(),
                                                            MainPhone = f.Members
                                                                            .OrderBy( m => m.GroupRole.Order )
                                                                            .ThenBy( m => m.Person.Gender )
                                                                            .FirstOrDefault()
                                                                            .Person.PhoneNumbers.OrderBy( p => p.NumberTypeValue.Order ).FirstOrDefault()
                                                        } ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the guests (known relationship of can check-in) for given family.
        /// </summary>
        /// <param name="groupId">Group id of the family.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route("api/Groups/GetGuestsForFamily/{groupId}")]
        public IQueryable<GuestFamily> GetGuestsForFamily(int groupId)
        {
            Guid knownRelationshipGuid = new Guid(Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS);
            Guid knownRelationshipOwner = new Guid(Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER);
            Guid knownRelationshipCanCheckin = new Guid(Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN);
            
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService(rockContext);
            PersonService personService = new PersonService(rockContext);

            var familyMembers = groupMemberService.Queryable()
                                    .Where(f => f.GroupId == groupId)
                                    .Select(f => f.PersonId);
            
            var familyMembersKnownRelationshipGroups = new GroupMemberService(rockContext).Queryable()
                                    .Where(g => g.Group.GroupType.Guid == knownRelationshipGuid 
                                                    && g.GroupRole.Guid == knownRelationshipOwner 
                                                    && familyMembers.Contains(g.PersonId))
                                    .Select(m => m.GroupId);
            rockContext.Database.Log = s => System.Diagnostics.Debug.WriteLine( s );
            var guests = groupMemberService.Queryable()
                                    .Where(g => g.GroupRole.Guid == knownRelationshipCanCheckin 
                                                    && familyMembersKnownRelationshipGroups.Contains(g.GroupId))
                                    .Select(g => g.PersonId)
                                    .Distinct().ToList();
            
            var guestFamilies = new List<GuestFamily>();
            rockContext.Database.Log = null;
            foreach ( var guestPersonId in guests )
            {
                var families = personService.GetFamilies(guestPersonId);

                foreach ( var family in families )
                {
                    if ( !guestFamilies.Select(f => f.Id).Contains(family.Id) )
                    {
                        GuestFamily guestFamily = new GuestFamily();
                        guestFamily.Id = family.Id;
                        guestFamily.Guid = family.Guid;
                        guestFamily.Name = family.Name;

                        guestFamily.FamilyMembers = new List<GuestFamilyMember>();
                        foreach ( var familyMember in family.Members )
                        {
                            GuestFamilyMember guestFamilyMember = new GuestFamilyMember();
                            guestFamilyMember.Id = familyMember.PersonId;
                            guestFamilyMember.Guid = familyMember.Person.Guid;
                            guestFamilyMember.FirstName = familyMember.Person.NickName;
                            guestFamilyMember.LastName = familyMember.Person.LastName;
                            guestFamilyMember.PhotoUrl = familyMember.Person.PhotoUrl;
                            guestFamilyMember.CanCheckin = guests.Contains(familyMember.PersonId);

                            guestFamily.FamilyMembers.Add(guestFamilyMember);
                        }

                        guestFamilies.Add(guestFamily);
                    }
                }
            }

            return guestFamilies.AsQueryable();
        }

        /// <summary>
        /// Gets a group by location.
        /// </summary>
        /// <param name="geofenceGroupTypeId">The geofence group type identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/ByLocation/{geofenceGroupTypeId}/{groupTypeId}/{locationId}" )]
        public IQueryable<Group> GetByLocation( int geofenceGroupTypeId, int groupTypeId, int locationId )
        {
            var fenceGroups = new List<Group>();

            // Get the location record
            var rockContext = (RockContext)Service.Context;
            var location = new LocationService( rockContext ).Get( locationId );

            // If location was valid and address was geocoded succesfully
            if ( location != null && location.GeoPoint != null )
            {
                // Find all the groupLocation records ( belonging to groups of the "geofenceGroupType" )
                // where the geofence surrounds the location
                var groupLocationService = new GroupLocationService( rockContext );
                foreach ( var fenceGroupLocation in groupLocationService
                    .Queryable("Group,Location").AsNoTracking()
                    .Where( gl =>
                        gl.Group.GroupTypeId == geofenceGroupTypeId &&
                        gl.Location.GeoFence != null &&
                        location.GeoPoint.Intersects( gl.Location.GeoFence ) )
                    .ToList() )
                {
                    var fenceGroup = fenceGroups.FirstOrDefault( g => g.Id == fenceGroupLocation.GroupId );
                    if ( fenceGroup == null )
                    {
                        fenceGroup = fenceGroupLocation.Group;
                        fenceGroups.Add( fenceGroup );
                    }
                    fenceGroupLocation.Group = null;

                    // Find all the group groupLocation records ( with group of the "groupTypeId" ) that have a location
                    // within the fence 
                    foreach ( var group in Service
                        .Queryable( "Schedule,GroupLocations.Location" ).AsNoTracking()
                        .Where( g =>
                            g.GroupTypeId == groupTypeId &&
                            g.GroupLocations.Any( gl =>
                                gl.Location.GeoPoint != null &&
                                gl.Location.GeoPoint.Intersects( fenceGroupLocation.Location.GeoFence ) ) ) )
                    {
                        // Remove any other group locations that do not belong to fence
                        foreach ( var gl in group.GroupLocations.ToList() )
                        {
                            if ( gl.Location.GeoPoint == null ||
                                !gl.Location.GeoPoint.Intersects( fenceGroupLocation.Location.GeoFence ) )
                            {
                                group.GroupLocations.Remove( gl );
                            }
                            else
                            {
                                // Calculate distance
                                double meters = gl.Location.GeoPoint.Distance( location.GeoPoint ) ?? 0.0D;
                                gl.Location.SetDistance( meters * Location.MilesPerMeter );
                            }
                        }

                        fenceGroup.Groups.Add( group );
                    }
                }
            }

            return fenceGroups.AsQueryable();
        }

        /// <summary>
        /// Saves a group address.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationTypeId">The location type identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="street2">The street2.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/Groups/SaveAddress/{groupId}/{locationTypeId}/{street1}/{city}/{state}/{postalCode}/{country}" )]
        public virtual void SaveAddress( int groupId, int locationTypeId,
            string street1, string city, string state, string postalCode, string country, string street2 = ""  )
        {
            SetProxyCreation( true );

            var rockContext = (RockContext)Service.Context;
            var group = new GroupService( rockContext ).Get( groupId );

            var locationType = DefinedValueCache.Read( locationTypeId, rockContext );
            if ( group == null || locationType == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            GroupService.AddNewFamilyAddress( rockContext, group, locationType.Guid.ToString(),
                street1, street2, city, state, postalCode, country, true );
        }

        #region MapInfo methods

        /// <summary>
        /// Gets the map information.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}" )]
        public IQueryable<MapItem> GetMapInfo( int groupId )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

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
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Children" )]
        public IQueryable<MapItem> GetChildMapInfo( int groupId )
        {
            var person = GetPerson();

            var mapItems = new List<MapItem>();

            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            foreach ( var group in ( (GroupService)Service ).Queryable( "GroupLocations.Location" )
                .Where( g => g.ParentGroupId == groupId ) )
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
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Members" )]
        public IQueryable<MapItem> GetMemberMapInfo( int groupId )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

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
                            g.Members.Any( m => memberIds.Contains( m.PersonId ) ) )
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
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Families/{statusId}" )]
        public IQueryable<MapItem> GetFamiliesMapInfo( int groupId, int statusId )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

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
                        var activeGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();
                        var families = new GroupLocationService( (RockContext)Service.Context ).Queryable()
                            .Where( l =>
                                l.IsMappedLocation &&
                                l.Group.GroupType.Guid.Equals( familyGuid ) &&
                                l.Location.GeoPoint.Intersects( location.GeoFence ) &&
                                l.Group.Members.Any( m =>
                                    m.Person.RecordStatusValue != null &&
                                    m.Person.RecordStatusValue.Guid.Equals( activeGuid ) &&
                                    m.Person.ConnectionStatusValueId.HasValue &&
                                    m.Person.ConnectionStatusValueId.Value == statusId ) )
                            .Select( l => new
                            {
                                l.Location,
                                l.Group.Id,
                                l.Group.Name,
                                MinStatus = l.Group.Members
                                    .Where( m =>
                                        m.Person.RecordStatusValue != null &&
                                        m.Person.RecordStatusValue.Guid.Equals( activeGuid ) &&
                                        m.Person.ConnectionStatusValueId.HasValue )
                                    .OrderBy( m => m.Person.ConnectionStatusValue.Order )
                                    .Select( m => m.Person.ConnectionStatusValue.Id )
                                    .FirstOrDefault()
                            } );

                        foreach ( var family in families.Where( f => f.MinStatus == statusId ) )
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
        [System.Web.Http.Route( "api/Groups/GetMapInfoWindow/{groupId}/{locationId}" )]
        public InfoWindowResult GetMapInfoWindow( int groupId, int locationId, [FromBody] InfoWindowRequest infoWindowDetails )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            // Use new service with new context so properties can be navigated by liquid
            var group = new GroupService( new RockContext() ).Queryable( "GroupType,GroupLocations.Location,Campus,Members.Person" )
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

                        dynamic dynGroup = new ExpandoObject();
                        dynGroup.GroupId = group.Id;
                        dynGroup.GroupName = group.Name;
                        dynGroup.DetailPageUrl = groupDetailUrl;
                        dynGroup.MapPageUrl = groupMapUrl;

                        var dictCampus = new Dictionary<string, object>();
                        dictCampus.Add( "Name", group.Campus != null ? group.Campus.Name : string.Empty );
                        dynGroup.Campus = dictCampus;

                        var dictGroupType = new Dictionary<string, object>();
                        dictGroupType.Add( "Id", group.GroupType.Id );
                        dictGroupType.Add( "Guid", group.GroupType.Guid.ToString().ToUpper() );
                        dictGroupType.Add( "GroupTerm", group.GroupType.GroupTerm );
                        dictGroupType.Add( "GroupMemberTerm", group.GroupType.GroupMemberTerm );
                        dynGroup.GroupType = dictGroupType;

                        var dictLocation = new Dictionary<string, object>();
                        dictLocation.Add( "Type", grouplocation.GroupLocationTypeValue.Value );
                        dictLocation.Add( "Address", grouplocation.Location.GetFullStreetAddress().ConvertCrLfToHtmlBr() );
                        dictLocation.Add( "Street1", grouplocation.Location.Street1 );
                        dictLocation.Add( "Street2", grouplocation.Location.Street2 );
                        dictLocation.Add( "City", grouplocation.Location.City );
                        dictLocation.Add( "State", grouplocation.Location.State );
                        dictLocation.Add( "PostalCode", grouplocation.Location.PostalCode );
                        dictLocation.Add( "Country", grouplocation.Location.Country );
                        dynGroup.Location = dictLocation;

                        var members = new List<Dictionary<string, object>>();
                        foreach ( var member in group.Members.OrderBy( m => m.GroupRole.Order ).ThenBy( m => m.Person.BirthDate ) )
                        {
                            var dictMember = new Dictionary<string, object>();
                            dictMember.Add( "PersonId", member.Person.Id );
                            dictMember.Add( "ProfilePageUrl", personProfileUrl + member.Person.Id.ToString() );
                            dictMember.Add( "Role", member.GroupRole.Name );
                            dictMember.Add( "NickName", member.Person.NickName );
                            dictMember.Add( "LastName", member.Person.LastName );
                            dictMember.Add( "PhotoUrl", member.Person.PhotoId.HasValue ? member.Person.PhotoUrl : string.Empty );
                            dictMember.Add( "PhotoId", member.Person.PhotoId );
                            dictMember.Add(
                                "ConnectionStatus",
                                member.Person.ConnectionStatusValue != null ? member.Person.ConnectionStatusValue.Value : string.Empty );
                            dictMember.Add( "Email", member.Person.Email );

                            var phoneTypes = new List<Dictionary<string, object>>();
                            foreach ( PhoneNumber p in member.Person.PhoneNumbers )
                            {
                                var dictPhoneNumber = new Dictionary<string, object>();
                                dictPhoneNumber.Add( "Name", p.NumberTypeValue.Value );
                                dictPhoneNumber.Add( "Number", p.ToString() );
                                phoneTypes.Add( dictPhoneNumber );
                            }

                            dictMember.Add( "PhoneTypes", phoneTypes );

                            members.Add( dictMember );
                        }

                        dynGroup.Members = members;

                        var groupDict = dynGroup as IDictionary<string, object>;
                        string result = System.Web.HttpUtility.HtmlDecode( infoWindowDetails.Template ).ResolveMergeFields( groupDict );

                        return new InfoWindowResult( result );
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


        /// <summary>
        ///
        /// </summary>
        public class GuestFamily
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            /// <value>
            /// The id.
            /// </value>
            public int Id { get; set; }

            public string Name { get; set; }

            public Guid Guid { get; set; }

            public List<GuestFamilyMember> FamilyMembers { get; set; }

        }

        public class GuestFamilyMember
        {
            public int Id { get; set; }

            public int PersonAliasId { get; set; }

            public Guid Guid { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string PhotoUrl { get; set; }

            public bool CanCheckin { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class FamilySearchResult
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
            /// Gets or sets the family members.
            /// </summary>
            /// <value>
            /// The family members.
            /// </value>
            public List<GroupMember> FamilyMembers { get; set; }

            /// <summary>
            /// Gets or sets the home location.
            /// </summary>
            /// <value>
            /// The home location.
            /// </value>
            public Location HomeLocation { get; set; }

            /// <summary>
            /// Gets or sets the main phone.
            /// </summary>
            /// <value>
            /// The main phone.
            /// </value>
            public PhoneNumber MainPhone { get; set; }
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

            public InfoWindowResult( string result )
            {
                Result = result;
            }
        }

        #endregion
    }
}