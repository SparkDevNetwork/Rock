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
using System.Data.Entity.Spatial;
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
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootGroupId">The root group identifier.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="includedGroupTypeIds">The included group type ids.</param>
        /// <param name="excludedGroupTypeIds">The excluded group type ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <param name="countsType">Type of the counts.</param>
        /// <param name="campusId">if set it will filter groups based on campus</param>
        /// <param name="includeNoCampus">if campus set and set to <c>true</c> [include groups with no campus].</param>
        /// <param name="limitToPublic">if set to <c>true</c> [limit to public groups].</param>
        /// <param name="limitToSchedulingEnabled">if set to <c>true</c> only includes groups that have SchedulingEnabled (or has a child group that has SchedulingEnabled).</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren(
            int id,
            int rootGroupId = 0,
            bool limitToSecurityRoleGroups = false,
            string includedGroupTypeIds = "",
            string excludedGroupTypeIds = "",
            bool includeInactiveGroups = false,
            TreeViewItem.GetCountsType countsType = TreeViewItem.GetCountsType.None,
            int campusId = 0,
            bool includeNoCampus = false,
            bool limitToPublic  = false,
            bool limitToSchedulingEnabled = false)
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            var includedGroupTypeIdList = includedGroupTypeIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            var excludedGroupTypeIdList = excludedGroupTypeIds.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();

            var groupService = (GroupService)Service;

            // if specific group types are specified, show the groups regardless of ShowInNavigation
            bool limitToShowInNavigation = !includedGroupTypeIdList.Any();

            var qry = groupService.GetChildren( id, rootGroupId, limitToSecurityRoleGroups, includedGroupTypeIdList, excludedGroupTypeIdList, includeInactiveGroups, limitToShowInNavigation, campusId, includeNoCampus, limitToPublic );

            List<Group> groupList = new List<Group>();
            List<TreeViewItem> groupNameList = new List<TreeViewItem>();

            var person = GetPerson();

            foreach ( var group in qry.OrderBy( g => g.Order ).ThenBy( g => g.Name ) )
            {
                if ( group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var groupType = GroupTypeCache.Get( group.GroupTypeId );
                    bool includeGroup = true;
                    if ( limitToSchedulingEnabled )
                    {
                        includeGroup = false;
                        if ( groupType?.IsSchedulingEnabled == true )
                        {
                            includeGroup = true;
                        }
                        else
                        {
                            bool hasChildScheduledEnabledGroups = groupService.GetAllDescendents( group.Id ).Any( a => a.GroupType.IsSchedulingEnabled == true );
                            if ( hasChildScheduledEnabledGroups )
                            {
                                includeGroup = true;
                            }
                        }
                    }

                    if ( includeGroup )
                    {
                        groupList.Add( group );
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = group.Id.ToString();
                        treeViewItem.Name = group.Name;
                        treeViewItem.IsActive = group.IsActive;

                        // if there a IconCssClass is assigned, use that as the Icon.
                        treeViewItem.IconCssClass = groupType?.IconCssClass;

                        if ( countsType == TreeViewItem.GetCountsType.GroupMembers )
                        {
                            int groupMemberCount = new GroupMemberService( this.Service.Context as RockContext ).Queryable().Where( a => a.GroupId == group.Id && a.GroupMemberStatus == GroupMemberStatus.Active ).Count();
                            treeViewItem.CountInfo = groupMemberCount;
                        }
                        else if ( countsType == TreeViewItem.GetCountsType.ChildGroups )
                        {
                            treeViewItem.CountInfo = groupService.Queryable().Where( a => a.ParentGroupId.HasValue && a.ParentGroupId == group.Id ).Count();
                        }

                        groupNameList.Add( treeViewItem );
                    }
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
                int groupId = g.Id.AsInteger();
                g.HasChildren = qryHasChildrenList.Any( a => a == groupId );
            }

            return groupNameList.AsQueryable();
        }

        /// <summary>
        /// Gets the families sorted by the person's GroupOrder (GroupMember.GroupOrder)
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery( MaxExpansionDepth = 4 )]
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
        /// <param name="maxResults">The maximum results.</param>
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
            foreach ( var person in personResults )
            {
                var families = personService.GetFamilies( person.Id )
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
                                                        } )
                                                        .ToList();

                foreach ( var family in families )
                {
                    familyResults.Add( family );
                }
            }

            return familyResults.DistinctBy( f => f.Id ).AsQueryable();
        }

        /// <summary>
        /// Gets the family.
        /// </summary>
        /// <param name="familyId">The family identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/GetFamily/{familyId}" )]
        public FamilySearchResult GetFamily( int familyId )
        {
            RockContext rockContext = new RockContext();
            Guid homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

            return new GroupService( rockContext ).Queryable().AsNoTracking()
                                                    .Where( g => g.Id == familyId )
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
        [System.Web.Http.Route( "api/Groups/GetGuestsForFamily/{groupId}" )]
        public IQueryable<GuestFamily> GetGuestsForFamily( int groupId )
        {
            Guid knownRelationshipGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            Guid knownRelationshipOwner = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER );
            Guid knownRelationshipCanCheckin = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN );

            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            PersonService personService = new PersonService( rockContext );

            var familyMembers = groupMemberService.Queryable()
                                    .Where( f => f.GroupId == groupId )
                                    .Select( f => f.PersonId );

            var familyMembersKnownRelationshipGroups = new GroupMemberService( rockContext ).Queryable()
                                    .Where( g => g.Group.GroupType.Guid == knownRelationshipGuid
                                                    && g.GroupRole.Guid == knownRelationshipOwner
                                                    && familyMembers.Contains( g.PersonId ) )
                                    .Select( m => m.GroupId );
            var guests = groupMemberService.Queryable()
                                    .Where( g => g.GroupRole.Guid == knownRelationshipCanCheckin
                                                    && familyMembersKnownRelationshipGroups.Contains( g.GroupId ) )
                                    .Select( g => g.PersonId )
                                    .Distinct().ToList();

            var guestFamilies = new List<GuestFamily>();
            rockContext.Database.Log = null;
            foreach ( var guestPersonId in guests )
            {
                var families = personService.GetFamilies( guestPersonId );

                foreach ( var family in families )
                {
                    if ( !guestFamilies.Select( f => f.Id ).Contains( family.Id ) )
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
                            guestFamilyMember.PersonAliasId = familyMember.Person.PrimaryAliasId.Value;
                            guestFamilyMember.Guid = familyMember.Person.Guid;
                            guestFamilyMember.FirstName = familyMember.Person.NickName;
                            guestFamilyMember.LastName = familyMember.Person.LastName;
                            guestFamilyMember.PhotoUrl = familyMember.Person.PhotoUrl;
                            guestFamilyMember.CanCheckin = guests.Contains( familyMember.PersonId );
                            guestFamilyMember.Role = familyMember.GroupRole.Name;
                            guestFamilyMember.Age = familyMember.Person.Age;
                            guestFamilyMember.Gender = familyMember.Person.Gender;

                            guestFamily.FamilyMembers.Add( guestFamilyMember );
                        }

                        guestFamilies.Add( guestFamily );
                    }
                }
            }

            return guestFamilies.AsQueryable();
        }

        /// <summary>
        /// Gets a list of groups surrounding the specified the location, optionally limited to the specified geofenceGroupTypeId
        /// If geofenceGroupTypeId is specified, the list of GeoFence groups will be returned with the groups as child groups of that geofence group.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="sortByDistance">if set to <c>true</c> [sort by distance].</param>
        /// <param name="maxDistanceMiles">The maximum distance miles.</param>
        /// <param name="geofenceGroupTypeId">The geofence group type identifier.</param>
        /// <param name="queryOptions">The query options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/ByLocation" )]
        public IQueryable GetByLocation( int groupTypeId, int locationId, bool? sortByDistance = true, double? maxDistanceMiles = null, int? geofenceGroupTypeId = null, System.Web.Http.OData.Query.ODataQueryOptions<Group> queryOptions = null )
        {
            // Get the location record
            var rockContext = (RockContext)Service.Context;
            var specifiedLocation = new LocationService( rockContext ).Get( locationId );

            // If location was valid and address was geocoded successfully
            DbGeography geoPoint = specifiedLocation != null ? specifiedLocation.GeoPoint : null;

            return GetByGeoPoint( groupTypeId, geoPoint, sortByDistance, maxDistanceMiles, geofenceGroupTypeId, queryOptions );
        }

        /// <summary>
        /// Gets a list of groups surrounding the specified lat/long, optionally limited to the specified geofenceGroupTypeId
        /// If geofenceGroupTypeId is specified, the list of GeoFence groups will be returned with the groups as child groups of that geofence group.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="sortByDistance">The sort by distance.</param>
        /// <param name="maxDistanceMiles">The maximum distance miles.</param>
        /// <param name="geofenceGroupTypeId">The geofence group type identifier.</param>
        /// <param name="queryOptions">The query options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Groups/ByLatLong" )]
        public IQueryable GetByLatLong( int groupTypeId, double latitude, double longitude, bool? sortByDistance = true, double? maxDistanceMiles = null, int? geofenceGroupTypeId = null, System.Web.Http.OData.Query.ODataQueryOptions<Group> queryOptions = null )
        {
            string geoText = string.Format( "POINT({0} {1})", longitude, latitude );
            DbGeography geoPoint = DbGeography.FromText( geoText );

            return GetByGeoPoint( groupTypeId, geoPoint, sortByDistance, maxDistanceMiles, geofenceGroupTypeId, queryOptions );
        }

        /// <summary>
        /// Gets a list of groups surrounding the specified geopoint of the specified GroupTypeid
        /// If geofenceGroupTypeId is specified, the list of GeoFence groups will be returned with the groups as child groups of that geofence group.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="geoPoint">The geo point.</param>
        /// <param name="sortByDistance">if set to <c>true</c> [sort by distance].</param>
        /// <param name="maxDistanceMiles">The maximum distance miles.</param>
        /// <param name="geofenceGroupTypeId">The geofence group type identifier.</param>
        /// <param name="queryOptions">The query options.</param>
        /// <returns></returns>
        private IQueryable GetByGeoPoint( int groupTypeId, DbGeography geoPoint, bool? sortByDistance, double? maxDistanceMiles, int? geofenceGroupTypeId, System.Web.Http.OData.Query.ODataQueryOptions<Group> queryOptions )
        {
            var rockContext = (RockContext)Service.Context;
            IEnumerable<Group> resultGroups = new List<Group>();

            if ( geoPoint != null )
            {
                if ( geofenceGroupTypeId.HasValue && geofenceGroupTypeId.Value > 0 )
                {
                    var fenceGroups = new List<Group>();

                    // Find all the groupLocation records ( belonging to groups of the "geofenceGroupType" )
                    // where the geofence surrounds the location
                    var groupLocationService = new GroupLocationService( rockContext );
                    foreach ( var fenceGroupLocation in groupLocationService
                        .Queryable( "Group,Location" ).AsNoTracking()
                        .Where( gl =>
                            gl.Group.GroupTypeId == geofenceGroupTypeId &&
                            gl.Location.GeoFence != null &&
                            geoPoint.Intersects( gl.Location.GeoFence ) )
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
                            }

                            fenceGroup.Groups.Add( group );
                        }
                    }

                    resultGroups = fenceGroups;
                }
                else
                {
                    // if a geoFence is not specified, just get all groups of this group type
                    resultGroups = Service.Queryable( "Schedule,GroupLocations.Location" ).AsNoTracking().Where( a => a.GroupTypeId == groupTypeId ).Include( a => a.GroupLocations ).ToList();
                }
            }

            // calculate the distance of each of the groups locations from the specified geoFence
            foreach ( var group in resultGroups )
            {
                foreach ( var gl in group.GroupLocations )
                {
                    // Calculate distance
                    if ( gl.Location.GeoPoint != null )
                    {
                        double meters = gl.Location.GeoPoint.Distance( geoPoint ) ?? 0.0D;
                        gl.Location.SetDistance( meters * Location.MilesPerMeter );
                    }
                }
            }

            // remove groups that don't have a GeoPoint
            resultGroups = resultGroups.Where( a => a.GroupLocations.Any( x => x.Location.GeoPoint != null ) );

            // remove groups that don't have a location within the specified radius
            if ( maxDistanceMiles.HasValue )
            {
                resultGroups = resultGroups.Where( a => a.GroupLocations.Any( x => x.Location.Distance <= maxDistanceMiles.Value ) );
            }

            var querySettings = new System.Web.Http.OData.Query.ODataQuerySettings();
            if ( sortByDistance.HasValue && sortByDistance.Value )
            {
                resultGroups = resultGroups.OrderBy( a => a.GroupLocations.FirstOrDefault() != null ? a.GroupLocations.FirstOrDefault().Location.Distance : int.MaxValue ).ToList();

                // if we are sorting by distance, tell OData not to re-sort them by Id
                querySettings.EnsureStableOrdering = false;
            }

            // manually apply any OData parameters to the InMemory Query
            var qryResults = queryOptions.ApplyTo( resultGroups.AsQueryable(), querySettings );

            return qryResults.AsQueryable();
        }

        /// <summary>
        /// Saves a group address.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationTypeId">The location type identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/Groups/SaveAddress/{groupId}/{locationTypeId}" )]
        public virtual void SaveAddress( int groupId, int locationTypeId, string street1 = "", string street2 = "", string city = "", string state = "", string postalCode = "", string country = "" )
        {
            SetProxyCreation( true );

            var rockContext = (RockContext)Service.Context;
            var group = new GroupService( rockContext ).Get( groupId );

            var locationType = DefinedValueCache.Get( locationTypeId );
            if ( group == null || locationType == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

            GroupService.AddNewGroupAddress( rockContext, group, locationType.Guid.ToString(), street1, street2, city, state, postalCode, country, true );
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
                        mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
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

        /// <summary>
        /// Gets the child map information.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupTypeIds">The group type ids (comma delimited).</param>
        /// <param name="includeDescendants">if set to <c>true</c> [include descendants].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Children" )]
        public IQueryable<MapItem> GetChildMapInfo( int groupId, string groupTypeIds = null, bool includeDescendants = false )
        {
            var person = GetPerson();

            var mapItems = new List<MapItem>();

            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            var groupService = (GroupService)Service;
            var groupLocationService = new GroupLocationService( groupService.Context as RockContext );
            IEnumerable<Group> childGroups;

            if ( !includeDescendants )
            {
                childGroups = groupService.Queryable().Where( g => g.ParentGroupId == groupId );
            }
            else
            {
                childGroups = groupService.GetAllDescendents( groupId );
            }

            if ( !string.IsNullOrWhiteSpace( groupTypeIds ) ) 
            {
                var groupTypeIdList = groupTypeIds.Split( ',' ).AsIntegerList();
                if ( groupTypeIdList.Any() )
                {
                    childGroups = childGroups.Where( a => groupTypeIdList.Contains( a.GroupTypeId ) );
                }
            }

            var childGroupIds = childGroups.Select( a => a.Id ).ToList();

            // fetch all the groupLocations for all the groups we are going to show (to reduce SQL traffic)
            var groupsLocationList = groupLocationService.Queryable().Where( a => childGroupIds.Contains( a.GroupId ) && a.Location.GeoPoint != null || a.Location.GeoFence != null ).Select( a => new
            {
                a.GroupId,
                a.Location
            } ).ToList();

            foreach ( var group in childGroups )
            {
                if ( group != null && group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var groupLocations = groupsLocationList.Where( a => a.GroupId == group.Id ).Select( a => a.Location );
                    foreach ( var location in groupLocations )
                    {
                        var mapItem = new MapItem( location );
                        mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
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

        /// <summary>
        /// Gets the member map information.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberStatus">The group member status.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Members/{groupMemberStatus?}" )]
        public IQueryable<MapItem> GetMemberMapInfo( int groupId, GroupMemberStatus? groupMemberStatus = null )
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
                    var members = group.Members;

                    if ( groupMemberStatus.HasValue )
                    {
                        members = members.Where(a=>a.GroupMemberStatus == groupMemberStatus.Value).ToList();
                    }

                    var memberIds = members.Select( m => m.PersonId ).Distinct().ToList();
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
                            mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
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

        /// <summary>
        /// Gets the families map information.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="statusId">The status identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Families/{statusId}" )]
        public IQueryable<MapItem> GetFamiliesMapInfo( int groupId, int statusId )
        {
            return GetFamiliesMapInfo( groupId, statusId, null );
        }

        /// <summary>
        /// Gets the families map information.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="statusId">The status identifier.</param>
        /// <param name="campusIds">If specified, only show families that are associated with any of the campus ids.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Groups/GetMapInfo/{groupId}/Families/{statusId}" )]
        public IQueryable<MapItem> GetFamiliesMapInfo( int groupId, int statusId, string campusIds)
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
                        var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                        var recordStatusActiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                        
                        var families = new GroupLocationService( (RockContext)Service.Context ).Queryable()
                            .Where( l =>
                                l.IsMappedLocation &&
                                l.Group.GroupTypeId == familyGroupTypeId &&
                                l.Location.GeoPoint.Intersects( location.GeoFence ) &&
                                l.Group.Members.Any( m =>
                                    m.Person.RecordStatusValueId.HasValue &&
                                    m.Person.RecordStatusValueId == recordStatusActiveId &&
                                    m.Person.ConnectionStatusValueId.HasValue &&
                                    m.Person.ConnectionStatusValueId.Value == statusId ) )
                            .Select( l => new
                            {
                                l.Location,
                                l.Group.Id,
                                l.Group.Name,
                                l.Group.CampusId,
                                MinStatus = l.Group.Members
                                    .Where( m =>
                                        m.Person.RecordStatusValueId.HasValue &&
                                        m.Person.RecordStatusValueId == recordStatusActiveId &&
                                        m.Person.ConnectionStatusValueId.HasValue )
                                    .OrderBy( m => m.Person.ConnectionStatusValue.Order )
                                    .Select( m => m.Person.ConnectionStatusValue.Id )
                                    .FirstOrDefault()
                            } );

                        var campusIdList = ( campusIds ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
                        if ( campusIdList.Any() )
                        {
                            families = families.Where( a => a.CampusId.HasValue && campusIdList.Contains( a.CampusId.Value ) );
                        }

                        foreach ( var family in families.Where( f => f.MinStatus == statusId ) )
                        {
                            var mapItem = new MapItem( family.Location );
                            mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
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

        /// <summary>
        /// Gets the map information window.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="infoWindowDetails">The information window details.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
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

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the family members.
            /// </summary>
            /// <value>
            /// The family members.
            /// </value>
            public List<GuestFamilyMember> FamilyMembers { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class GuestFamilyMember
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public int PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the photo URL.
            /// </summary>
            /// <value>
            /// The photo URL.
            /// </value>
            public string PhotoUrl { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can checkin.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance can checkin; otherwise, <c>false</c>.
            /// </value>
            public bool CanCheckin { get; set; }

            /// <summary>
            /// Gets or sets the age.
            /// </summary>
            /// <value>
            /// The age.
            /// </value>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the role.
            /// </summary>
            /// <value>
            /// The role.
            /// </value>
            public string Role { get; set; }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public Gender Gender { get; set; }
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

        /// <summary>
        /// 
        /// </summary>
        public class InfoWindowRequest
        {
            /// <summary>
            /// Gets or sets the group page.
            /// </summary>
            /// <value>
            /// The group page.
            /// </value>
            public string GroupPage { get; set; }

            /// <summary>
            /// Gets or sets the person profile page.
            /// </summary>
            /// <value>
            /// The person profile page.
            /// </value>
            public string PersonProfilePage { get; set; }

            /// <summary>
            /// Gets or sets the map page.
            /// </summary>
            /// <value>
            /// The map page.
            /// </value>
            public string MapPage { get; set; }

            /// <summary>
            /// Gets or sets the template.
            /// </summary>
            /// <value>
            /// The template.
            /// </value>
            public string Template { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class InfoWindowResult
        {
            /// <summary>
            /// Gets or sets the result.
            /// </summary>
            /// <value>
            /// The result.
            /// </value>
            public string Result { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="InfoWindowResult"/> class.
            /// </summary>
            /// <param name="result">The result.</param>
            public InfoWindowResult( string result )
            {
                Result = result;
            }
        }

        #endregion
    }
}