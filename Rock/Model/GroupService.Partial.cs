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
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

using Z.EntityFramework.Plus;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Group"/> objects.
    /// </summary>
    public partial class GroupService
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Group">Groups</see>, excluding archived groups
        /// </summary>
        /// <returns></returns>
        public override IQueryable<Group> Queryable()
        {
            // override Group Queryable so that Archived groups are never included
            return base.Queryable().Where( a => a.IsArchived == false );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Group">Groups</see>, excluding archived groups,
        /// with eager loading of properties specified in includes
        /// </summary>
        /// <param name="includes"></param>
        /// <returns></returns>
        public override IQueryable<Group> Queryable( string includes )
        {
            // override Group Queryable so that Archived groups are never included
            return base.Queryable( includes ).Where( a => a.IsArchived == false );
        }

        /// <summary>
        /// Returns a queryable of archived groups
        /// </summary>
        /// <returns></returns>
        public IQueryable<Group> GetArchived()
        {
            return this.AsNoFilter().Where( a => a.IsArchived == true );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group"/> entities that by their <see cref="Rock.Model.GroupType"/> Id.
        /// </summary>
        /// <param name="groupTypeId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that they belong to.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> belong to a specific <see cref="Rock.Model.GroupType"/>.</returns>
        public IQueryable<Group> GetByGroupTypeId( int groupTypeId )
        {
            return Queryable().Where( t => t.GroupTypeId == groupTypeId );
        }

        /// <summary>
        /// Returns the <see cref="Rock.Model.Group"/> containing a Guid property that matches the provided value.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> to find a <see cref="Rock.Model.Group"/> by.</param>
        /// <returns>The <see cref="Rock.Model.Group" /> who's Guid property matches the provided value.  If no match is found, returns null.</returns>
        public Group GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group">Groups</see> by their IsSecurityRole flag.
        /// </summary>
        /// <param name="isSecurityRole">A <see cref="System.Boolean"/> representing the IsSecurityRole flag value to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> that contains a IsSecurityRole flag that matches the provided value.</returns>
        public IQueryable<Group> GetByIsSecurityRole( bool isSecurityRole )
        {
            return Queryable().Where( t => t.IsSecurityRole == isSecurityRole );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Group">Groups</see> by the Id of its parent <see cref="Rock.Model.Group"/>. 
        /// </summary>
        /// <param name="parentGroupId">A <see cref="System.Int32" /> representing the Id of the parent <see cref="Rock.Model.Group"/> to search by. This value
        /// is nullable and a null value will search for <see cref="Rock.Model.Group">Groups</see> that do not inherit from other groups.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> who's ParentGroupId matches the provided value.</returns>
        public IQueryable<Group> GetByParentGroupId( int? parentGroupId )
        {
            return Queryable().Where( t => ( t.ParentGroupId == parentGroupId || ( parentGroupId == null && t.ParentGroupId == null ) ) );
        }


        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group">Groups</see> by the Id of their parent <see cref="Rock.Model.Group"/> and by the Group's name.
        /// </summary>
        /// <param name="parentGroupId">An <see cref="System.Int32" /> representing the Id of the parent <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="name">A <see cref="System.String"/> containing the Name of the <see cref="Rock.Model.Group"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> who's ParentGroupId and Name matches the provided values.</returns>
        public IQueryable<Group> GetByParentGroupIdAndName( int? parentGroupId, string name )
        {
            return Queryable().Where( t => ( t.ParentGroupId == parentGroupId || ( parentGroupId == null && t.ParentGroupId == null ) ) && t.Name == name );
        }

        #region Geospatial Queries

        /// <summary>
        /// Gets the family groups that are geofenced by any of the selected group's locations
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencedFamilies( int groupId )
        {
            // Get the geofences for the group
            var groupGeofences = this.Queryable().AsNoTracking()
                .Where( g => g.Id == groupId )
                .SelectMany( g => g.GroupLocations )
                .Where( g => g.Location.GeoFence != null )
                .Select( g => g.Location.GeoFence )
                .ToList();

            return GetGeofencedFamilies( groupGeofences );
        }

        /// <summary>
        /// Gets the family groups that are geofenced by any of the selected group's locations
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencedFamilies( Guid groupGuid )
        {
            // Get the geofences for the group
            var groupGeofences = this.Queryable().AsNoTracking()
                .Where( g => g.Guid.Equals( groupGuid ) )
                .SelectMany( g => g.GroupLocations )
                .Where( g => g.Location.GeoFence != null )
                .Select( g => g.Location.GeoFence )
                .ToList();

            return GetGeofencedFamilies( groupGeofences );
        }

        /// <summary>
        /// Gets the family groups that are geofenced by any of the selected geofences
        /// </summary>
        /// <param name="geofences">The geofences.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencedFamilies( List<DbGeography> geofences )
        {
            var rockContext = (RockContext)this.Context;
            var groupLocationService = new GroupLocationService( rockContext );

            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            return groupLocationService.GetMappedLocationsByGeofences( geofences )
                .Where( l =>
                    l.Group != null &&
                    l.Group.GroupTypeId == familyGroupTypeId )
                .Select( l => l.Group );
        }

        /// <summary>
        /// Gets the groups of a particular type that geofence the selected person's mapped location(s)
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencingGroups( int personId, int groupTypeId )
        {
            var rockContext = (RockContext)this.Context;
            var personService = new PersonService( rockContext );
            var personGeopoints = personService.GetGeopoints( personId );
            return GetGeofencingGroups( personGeopoints, groupTypeId );
        }

        /// <summary>
        /// Gets the groups of a particular type that geofence the selected person's mapped location(s)
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencingGroups( int personId, Guid groupTypeGuid )
        {
            var rockContext = (RockContext)this.Context;
            var personService = new PersonService( rockContext );
            var personGeopoints = personService.GetGeopoints( personId );
            return GetGeofencingGroups( personGeopoints, groupTypeGuid );
        }
        
        /// <summary>
        /// Gets the groups of a selected type that have a geofence location that surrounds any of the
        /// selected points
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencingGroups( IQueryable<DbGeography> points, int groupTypeId )
        {
            // Get the groups that have a location that intersects with any of the family's locations
            return this.Queryable()
                .Where( g =>
                    g.GroupTypeId.Equals( groupTypeId ) &&
                    g.IsActive &&
                    g.GroupLocations.Any( l =>
                        l.Location.GeoFence != null &&
                        points.Any( p => p.Intersects( l.Location.GeoFence ) )
                    ) );
        }

        /// <summary>
        /// Gets the groups of a selected type that have a geofence location that surrounds any of the
        /// selected points
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetGeofencingGroups( IQueryable<DbGeography> points, Guid groupTypeGuid )
        {
            // Get the groups that have a location that intersects with any of the family's locations
            return this.Queryable()
                .Where( g =>
                    g.GroupType.Guid.Equals( groupTypeGuid ) &&
                    g.IsActive &&
                    g.GroupLocations.Any( l =>
                        l.Location.GeoFence != null &&
                        points.Any( p => p.Intersects( l.Location.GeoFence ) )
                    ) );
        }

        /// <summary>
        /// Gets the nearest group.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public Group GetNearestGroup( int personId, int groupTypeId )
        {
            var rockContext = (RockContext)this.Context;
            var personService = new PersonService( rockContext );
            var personGeopoint = personService.GetGeopoints( personId ).FirstOrDefault();
            if ( personGeopoint != null )
            {
                var groupLocation = this.Queryable()
                    .Where( g =>
                        g.GroupTypeId.Equals( groupTypeId ) )
                    .SelectMany( g =>
                        g.GroupLocations
                            .Where( gl =>
                                gl.Location != null &&
                                gl.Location.GeoPoint != null
                            )
                    )
                    .OrderBy( gl => gl.Location.GeoPoint.Distance( personGeopoint ) )
                    .FirstOrDefault();

                if ( groupLocation != null )
                {
                    return groupLocation.Group;
                }
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Gets immediate navigation children of a group (id) or a rootGroupId. Specify 0 for both Id and rootGroupId to get top level groups limited 
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootGroupId">The root group identifier.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIncludedIds">The group type included ids.</param>
        /// <param name="groupTypeExcludedIds">The group type excluded ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <returns></returns>
        public IQueryable<Group> GetNavigationChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, List<int> groupTypeIncludedIds, List<int> groupTypeExcludedIds, bool includeInactiveGroups = true )
        {
            return this.GetChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIncludedIds, groupTypeExcludedIds, includeInactiveGroups, true );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootGroupId">The root group identifier.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIncludedIds">The group type included ids.</param>
        /// <param name="groupTypeExcludedIds">The group type excluded ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <param name="limitToShowInNavigation">if set to <c>true</c> [limit to show in navigation].</param>
        /// <returns></returns>
        public IQueryable<Group> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, List<int> groupTypeIncludedIds, List<int> groupTypeExcludedIds, bool includeInactiveGroups, bool limitToShowInNavigation )
        {
            return this.GetChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIncludedIds, groupTypeExcludedIds, includeInactiveGroups, limitToShowInNavigation, 0, false, false );
        }

        /// <summary>
        /// Gets immediate children of a group (id) or a rootGroupId. Specify 0 for both Id and rootGroupId to get top level groups limited
        /// </summary>
        /// <param name="id">The ID of the Group to get the children of (or 0 to use rootGroupId)</param>
        /// <param name="rootGroupId">The root group ID</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIncludedIds">The group type included ids.</param>
        /// <param name="groupTypeExcludedIds">The group type excluded ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <param name="limitToShowInNavigation">if set to <c>true</c> [limit to show in navigation].</param>
        /// <param name="limitToPublic">if set to <c>true</c> [limit to public groups].</param>
        /// <returns></returns>
        public IQueryable<Group> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, List<int> groupTypeIncludedIds, List<int> groupTypeExcludedIds, bool includeInactiveGroups, bool limitToShowInNavigation, bool limitToPublic = false )
        {
            return this.GetChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIncludedIds, groupTypeExcludedIds, includeInactiveGroups, limitToShowInNavigation, 0, false, limitToPublic );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootGroupId">The root group identifier.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIncludedIds">The group type included ids.</param>
        /// <param name="groupTypeExcludedIds">The group type excluded ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <param name="limitToShowInNavigation">if set to <c>true</c> [limit to show in navigation].</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="includeNoCampus">if set to <c>true</c> [include no campus].</param>
        /// <returns></returns>
        public IQueryable<Group> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, List<int> groupTypeIncludedIds, List<int> groupTypeExcludedIds, bool includeInactiveGroups, bool limitToShowInNavigation, int campusId, bool includeNoCampus )
        {
            return this.GetChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIncludedIds, groupTypeExcludedIds, includeInactiveGroups, limitToShowInNavigation, 0, includeNoCampus, false );
        }

        /// <summary>
        /// Gets immediate children of a group (id) or a rootGroupId. Specify 0 for both Id and rootGroupId to get top level groups limited
        /// </summary>
        /// <param name="id">The ID of the Group to get the children of (or 0 to use rootGroupId)</param>
        /// <param name="rootGroupId">The root group ID</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIncludedIds">The group type included ids.</param>
        /// <param name="groupTypeExcludedIds">The group type excluded ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <param name="limitToShowInNavigation">if set to <c>true</c> [limit to show in navigation].</param>
        /// <param name="campusId">if set it will filter groups based on campus</param>
        /// <param name="includeNoCampus">if campus set and set to <c>true</c> [include groups with no campus].</param>
        /// <param name="limitToPublic">if set to <c>true</c> [limit to public groups].</param>
        /// <returns></returns>
        public IQueryable<Group> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, List<int> groupTypeIncludedIds, List<int> groupTypeExcludedIds, bool includeInactiveGroups, bool limitToShowInNavigation, int campusId, bool includeNoCampus, bool limitToPublic = false)
        {
            var qry = Queryable();

            if ( id == 0 )
            {
                if ( rootGroupId != 0 )
                {
                    qry = qry.Where( a => a.ParentGroupId == rootGroupId );
                }
                else
                {
                    qry = qry.Where( a => a.ParentGroupId == null );
                }
            }
            else
            {
                qry = qry.Where( a => a.ParentGroupId == id );
            }

            if ( !includeInactiveGroups )
            {
                qry = qry.Where( a => a.IsActive );
            }

            if ( limitToPublic )
            {
                qry = qry.Where( a => a.IsPublic );
            }

            if ( limitToSecurityRoleGroups )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            if ( campusId > 0 )
            {
                if ( includeNoCampus )
                {
                    qry = qry.Where( a => a.CampusId == campusId || a.Campus == null );
                }
                else
                {
                    qry = qry.Where( a => a.CampusId == campusId );
                }
            }

            if ( groupTypeIncludedIds.Any() )
            {
                // if groupTypeIncludedIds is specified, only get grouptypes that are in the groupTypeIncludedIds
                // NOTE: no need to factor in groupTypeExcludedIds since included would take precendance and the excluded ones would already not be included
                qry = qry.Where( a => groupTypeIncludedIds.Contains( a.GroupTypeId ) );
            }
            else if (groupTypeExcludedIds.Any() )
            {
                qry = qry.Where( a => !groupTypeExcludedIds.Contains( a.GroupTypeId ) );
            }

            if ( limitToShowInNavigation )
            {
                qry = qry.Where( a => a.GroupType.ShowInNavigation == true );
            }

            return qry;
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group">Groups</see> that are descendants of a specified group.
        /// </summary>
        /// <param name="parentGroupId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> to retrieve descendants for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> that are descendants of referenced group.</returns>
        public IEnumerable<Group> GetAllDescendents( int parentGroupId )
        {
            return this.ExecuteQuery(
                @"
                with CTE as (
                select * from [Group] where [ParentGroupId]={0}
                union all
                select [a].* from [Group] [a]
                inner join CTE pcte on pcte.Id = [a].[ParentGroupId]
                )
                select * from CTE
                ", parentGroupId );
        }

        /// <summary>
        /// Returns an enumerable collection of the <see cref="Rock.Model.Group" /> Ids that are ancestors of a specified groupId sorted starting with the most immediate parent
        /// </summary>
        /// <param name="childGroupId">The child group identifier.</param>
        /// <returns>
        /// An enumerable collection of the group Ids that are descendants of referenced groupId.
        /// </returns>
        public IOrderedEnumerable<int> GetAllAncestorIds( int childGroupId )
        {
            var result = this.Context.Database.SqlQuery<int>(
                @"
                with CTE as (
                select *, 0 as [Level] from [Group] where [Id]={0}
                union all
                select [a].*, [Level] + 1 as [Level] from [Group] [a]
                inner join CTE pcte on pcte.ParentGroupId = [a].[Id]
                )
                select Id from CTE where Id != {0} order by Level
                ", childGroupId );

            // already ordered within the sql, so do a dummy order by to get IOrderedEnumerable
            return result.OrderBy(a => 0);
        }

        /// <summary>
        /// Groups the name of the ancestor path.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public string GroupAncestorPathName( int groupId )
        {
            return this.Context.Database.SqlQuery<string>( @"
                WITH CTE AS 
                (
	                SELECT [ParentGroupId], CAST ( [Name] AS VARCHAR(MAX) ) AS [Name]
	                FROM [Group] 
	                WHERE [Id] = {0}
	
	                UNION ALL
	
	                SELECT G.[ParentGroupId], CAST ( G.[Name] + ' > ' + CTE.[Name] AS VARCHAR(MAX) )
	                FROM [Group] G
	                INNER JOIN CTE ON CTE.[ParentGroupId] = G.[Id]
                )

                SELECT [Name]
                FROM CTE
                WHERE [ParentGroupId] IS NULL
", groupId ).FirstOrDefault();

        }

        /// <summary>
        /// Check if the group has the person as a member.
        /// Returns false if the group is not found or if the person id is null.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public bool GroupHasMember(Guid groupGuid, int? personId )
        {
            if (personId == null)
            {
                return false;
            }

            Group group = this.GetByGuid( groupGuid );
            if (group ==  null)
            {
                return false;
            }

            return group.Members.Where( m => m.PersonId == personId ).Any();
        }

        /// <summary>
        /// Groups the members not meeting requirements.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="includeWarnings">if set to <c>true</c> [include warnings].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use GroupMembersNotMeetingRequirements( roup, includeWarnings, includeInactive) instead", true )]
        public Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>> GroupMembersNotMeetingRequirements( int groupId, bool includeWarnings, bool includeInactive = false )
        {
            var group = new GroupService( this.Context as RockContext ).Get( groupId );
            return GroupMembersNotMeetingRequirements( group, includeWarnings, includeInactive );
        }

        /// <summary>
        /// Groups the members not meeting requirements.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="includeWarnings">if set to <c>true</c> [include warnings].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>> GroupMembersNotMeetingRequirements( Group group, bool includeWarnings, bool includeInactive = false )
        {
            Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>> results = new Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>>();
            
            var rockContext = this.Context as RockContext;
            var groupRequirementService = new GroupRequirementService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );

            var qryGroupRequirements = groupRequirementService.Queryable().Where( a => ( a.GroupId.HasValue && a.GroupId == group.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == group.GroupTypeId ) ).ToList();
            bool hasGroupRequirements = qryGroupRequirements.Any();
            if ( !hasGroupRequirements )
            {
                // if no group requirements, then there are no members that don't meet the requirements, so return an empty dictionary
                return new Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>>();
            }

            var qryGroupMembers = groupMemberService.Queryable().Where( a => a.GroupId == group.Id );
            var groupMemberRequirementList = groupMemberRequirementService.Queryable().Where( a => a.GroupMember.GroupId == group.Id ).Select( a => new
            {
                a.GroupMemberId,
                a.RequirementWarningDateTime,
                a.RequirementFailDateTime,
                a.RequirementMetDateTime,
                a.GroupRequirement
            } ).ToList();

            if ( !includeInactive )
            {
                qryGroupMembers = qryGroupMembers.Where( a => a.GroupMemberStatus == GroupMemberStatus.Active );
            }

            var groupMemberList = qryGroupMembers.Include(a => a.GroupMemberRequirements).ToList();

            // get a list of group member ids that don't meet all the requirements
            List<int> groupMemberIdsThatLackGroupRequirementsList = groupMemberList
                .Where( a =>
                    !qryGroupRequirements
                        .Where( r => 
                            !r.GroupRoleId.HasValue ||
                            r.GroupRoleId.Value == a.GroupRoleId )
                        .Select( x => x.Id )
                        .All( r =>
                            a.GroupMemberRequirements
                                .Where( mr => mr.RequirementMetDateTime.HasValue )
                                .Select( x => x.GroupRequirementId )
                                .Contains( r ) ) )
                .Select( a => a.Id )
                .ToList();

            IEnumerable<GroupMember> membersWithIssuesList;

            if ( includeWarnings )
            {
                List<int> groupMemberIdsWithRequirementWarningsList = groupMemberRequirementList
                    .Where(
                        a =>
                            a.RequirementWarningDateTime != null ||
                            a.RequirementFailDateTime != null )
                    .Select( a => a.GroupMemberId )
                    .Distinct().ToList();

                membersWithIssuesList = groupMemberList.Where( a => groupMemberIdsThatLackGroupRequirementsList.Contains( a.Id ) || groupMemberIdsWithRequirementWarningsList.Contains( a.Id ) );
            }
            else
            {
                membersWithIssuesList = groupMemberList.Where( a => groupMemberIdsThatLackGroupRequirementsList.Contains( a.Id ) );
            }

            var groupMemberWithIssuesList = membersWithIssuesList.Select( a => new
            {
                GroupMember = a,
                GroupRequirementStatuses = groupMemberRequirementList.Where( x => x.GroupMemberId == a.Id )
            } ).ToList();

            var currentDateTime = RockDateTime.Now;

            foreach (var groupMemberWithIssues in groupMemberWithIssuesList )
            {
                Dictionary<PersonGroupRequirementStatus, DateTime> statuses = new Dictionary<PersonGroupRequirementStatus, DateTime>();
                
                // populate where the status is known
                foreach ( var requirementStatus in groupMemberWithIssues.GroupRequirementStatuses )
                {
                    PersonGroupRequirementStatus status = new PersonGroupRequirementStatus();
                    status.GroupRequirement = requirementStatus.GroupRequirement;
                    status.PersonId = groupMemberWithIssues.GroupMember.PersonId;

                    DateTime occurrenceDate = new DateTime();

                    if ( requirementStatus.RequirementMetDateTime == null)
                    {
                        status.MeetsGroupRequirement = MeetsGroupRequirement.NotMet;
                        occurrenceDate = requirementStatus.RequirementFailDateTime ?? currentDateTime; 
                    }
                    else if (requirementStatus.RequirementWarningDateTime.HasValue)
                    {
                        status.MeetsGroupRequirement = MeetsGroupRequirement.MeetsWithWarning;
                        occurrenceDate = requirementStatus.RequirementWarningDateTime.Value;
                    }
                    else
                    {
                        status.MeetsGroupRequirement = MeetsGroupRequirement.Meets;
                        occurrenceDate = requirementStatus.RequirementMetDateTime.Value;
                    }
                    
                    statuses.Add( status, occurrenceDate );
                }

                // also add any groupRequirements that they don't have statuses for (and therefore haven't met)
                foreach (var groupRequirement in qryGroupRequirements)
                {
                    if ( !statuses.Any( x => x.Key.GroupRequirement.Id == groupRequirement.Id) )
                    {
                        PersonGroupRequirementStatus status = new PersonGroupRequirementStatus();
                        status.GroupRequirement = groupRequirement;
                        status.PersonId = groupMemberWithIssues.GroupMember.PersonId;
                        status.MeetsGroupRequirement = MeetsGroupRequirement.NotMet;
                        statuses.Add( status, currentDateTime );
                    }
                }

                var statusesWithIssues = statuses.Where( a => a.Key.MeetsGroupRequirement != MeetsGroupRequirement.Meets ).ToDictionary( k => k.Key, v => v.Value );

                if ( statusesWithIssues.Any() )
                {
                    results.Add( groupMemberWithIssues.GroupMember, statusesWithIssues );
                }
            }

            return results;
        }

        /// <summary>
        /// Saves the new family.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="familyMembers">The family members.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="savePersonAttributes">if set to <c>true</c> [save person attributes].</param>
        /// <returns></returns>
        public static Group SaveNewFamily( RockContext rockContext, List<GroupMember> familyMembers, int? campusId, bool savePersonAttributes )
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            string familyName = familyMembers.FirstOrDefault().Person.LastName + " Family";
            return SaveNewGroup( rockContext, familyGroupType.Id, null, familyName, familyMembers, campusId, savePersonAttributes );
        }

        /// <summary>
        /// Saves the new group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="parentGroupGuid">The parent group unique identifier.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="groupMembers">The group members.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="savePersonAttributes">if set to <c>true</c> [save person attributes].</param>
        /// <returns></returns>
        public static Group SaveNewGroup( RockContext rockContext, int groupTypeId, Guid? parentGroupGuid, string groupName, List<GroupMember> groupMembers, int? campusId, bool savePersonAttributes )
        {
            var groupType = GroupTypeCache.Get( groupTypeId );

            if ( groupType != null )
            {
                var isFamilyGroupType = groupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

                var groupService = new GroupService( rockContext );

                var group = new Group();

                group.GroupTypeId = groupType.Id;

                if ( parentGroupGuid.HasValue )
                {
                    var parentGroup = groupService.Get( parentGroupGuid.Value );
                    if ( parentGroup != null )
                    {
                        group.ParentGroupId = parentGroup.Id;
                    }
                }

                group.Name = groupName;

                if ( isFamilyGroupType )
                {
                    group.CampusId = campusId;
                }

                int? adultRoleId = null;
                var adultRole = new GroupTypeRoleService( rockContext ).Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) );
                if ( adultRole != null )
                {
                    adultRoleId = adultRole.Id;
                }

                foreach ( var groupMember in groupMembers )
                {
                    var person = groupMember.Person;
                    if ( person != null )
                    {
                        person.FirstName = person.FirstName.FixCase();
                        person.NickName = person.NickName.FixCase();
                        person.MiddleName = person.MiddleName.FixCase();
                        person.LastName = person.LastName.FixCase();

                        group.Members.Add( groupMember );
                        groupMember.Group = group;
                    }

                    if ( !groupMember.IsValidGroupMember(rockContext) )
                    {
                        throw new GroupMemberValidationException( groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" ) );
                    }
                }

                groupService.Add( group );
                rockContext.SaveChanges();

                var personService = new PersonService( rockContext );

                foreach ( var groupMember in groupMembers )
                {
                    var person = groupMember.Person;

                    if ( savePersonAttributes )
                    {
                        var newValues = person.AttributeValues;

                        person.LoadAttributes();
                        foreach ( var attributeCache in person.Attributes.Select( a => a.Value ) )
                        {
                            string oldValue = person.GetAttributeValue( attributeCache.Key ) ?? string.Empty;
                            string newValue = string.Empty;
                            if ( newValues != null &&
                                newValues.ContainsKey( attributeCache.Key ) &&
                                newValues[attributeCache.Key] != null )
                            {
                                newValue = newValues[attributeCache.Key].Value ?? string.Empty;
                            }

                            if ( !oldValue.Equals( newValue ) )
                            {
                                Rock.Attribute.Helper.SaveAttributeValue( person, attributeCache, newValue );
                            }
                        }
                    }

                    person = personService.Get( groupMember.PersonId );
                    if ( person != null )
                    {
                        bool updateRequired = false;
                        if ( groupMember.GroupRoleId == adultRoleId )
                        {
                            person.GivingGroupId = group.Id;
                            updateRequired = true;
                        }

                        if ( updateRequired )
                        {
                            rockContext.SaveChanges();
                        }

                        int? modifiedByPersonAliasId = person.ModifiedAuditValuesAlreadyUpdated ? person.ModifiedByPersonAliasId : ( int? ) null;
                    }
                }

                return group;
            }

            return null;
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid,
            string street1, string street2, string city, string state, string postalCode, string country )
        {
            AddNewGroupAddress( rockContext, group, locationTypeGuid, street1, street2, city, state, postalCode, country, false );
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid,
            string street1, string street2, string city, string state, string postalCode, string country, bool moveExistingToPrevious )
        {
            var isMappedMailing = locationTypeGuid != SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS; // Mapped and Mailing = true unless location type is Previous
            AddNewGroupAddress( rockContext, group, locationTypeGuid, street1, street2, city, state, postalCode, country, moveExistingToPrevious, "", isMappedMailing, isMappedMailing );
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        /// <param name="modifiedBy">The description of the page or process that called the function.</param>
        /// <param name="isMailingLocation">Sets the Is Mailing option on the new address.</param>
        /// <param name="isMappedLocation">Sets the Is Mapped option on the new address.</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid,
            string street1, string street2, string city, string state, string postalCode, string country, bool moveExistingToPrevious,
            string modifiedBy, bool isMailingLocation, bool isMappedLocation )
        {
            if ( !string.IsNullOrWhiteSpace( street1 ) ||
                 !string.IsNullOrWhiteSpace( street2 ) ||
                 !string.IsNullOrWhiteSpace( city ) ||
                 !string.IsNullOrWhiteSpace( postalCode ) ||
                 !string.IsNullOrWhiteSpace( country ) )
            {
                var location = new LocationService( rockContext ).Get( street1, street2, city, state, postalCode, country, group, true );
                AddNewGroupAddress( rockContext, group, locationTypeGuid, location, moveExistingToPrevious, modifiedBy, isMailingLocation, isMappedLocation );
            }
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid, int? locationId )
        {
            AddNewGroupAddress( rockContext, group, locationTypeGuid, locationId, false );
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid,
            int? locationId, bool moveExistingToPrevious )
        {
            var isMappedMailing = locationTypeGuid != SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS; // Mapped and Mailing = true unless location type is Previous
            AddNewGroupAddress( rockContext, group, locationTypeGuid, locationId, moveExistingToPrevious, "", isMappedMailing, isMappedMailing );
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        /// <param name="modifiedBy">The modified by.</param>
        /// <param name="isMailingLocation">Sets the Is Mailing option on the new address.</param>
        /// <param name="isMappedLocation">Sets the Is Mapped option on the new address.</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid, 
            int? locationId, bool moveExistingToPrevious, string modifiedBy, bool isMailingLocation, bool isMappedLocation )
        {
            if ( locationId.HasValue )
            {
                var location = new LocationService( rockContext ).Get( locationId.Value );
                AddNewGroupAddress( rockContext, group, locationTypeGuid, location, moveExistingToPrevious, modifiedBy, isMailingLocation, isMappedLocation );
            }
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="location">The location.</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid, Location location )
        {
            AddNewGroupAddress( rockContext, group, locationTypeGuid, location, false );
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid, Location location, bool moveExistingToPrevious )
        {
            var isMappedMailing = locationTypeGuid != SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS; // Mapped and Mailing = true unless location type is Previous
            AddNewGroupAddress( rockContext, group, locationTypeGuid, location, moveExistingToPrevious, "", isMappedMailing, isMappedMailing );
        }

        /// <summary>
        /// Adds the new group address (it is doesn't already exist) and saves changes to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        /// <param name="modifiedBy">The description of the page or process that called the function.</param>
        /// <param name="isMailingLocation">Sets the Is Mailing option on the new address.</param>
        /// <param name="isMappedLocation">Sets the Is Mapped option on the new address.</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid,
            Location location, bool moveExistingToPrevious, string modifiedBy, bool isMailingLocation, bool isMappedLocation )
        {
            if ( location != null )
            {
                var groupType = GroupTypeCache.Get( group.GroupTypeId );
                if ( groupType != null )
                {
                    var locationType = groupType.LocationTypeValues.FirstOrDefault( l => l.Guid.Equals( locationTypeGuid.AsGuid() ) );
                    if ( locationType != null )
                    {
                        var groupLocationService = new GroupLocationService( rockContext );
                        if ( !groupLocationService.Queryable()
                            .Where( gl =>
                                gl.GroupId == group.Id &&
                                gl.GroupLocationTypeValueId == locationType.Id &&
                                gl.LocationId == location.Id )
                            .Any() )
                        {
                            if ( moveExistingToPrevious )
                            {
                                var prevLocationType = groupType.LocationTypeValues.FirstOrDefault( l => l.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() ) );
                                if ( prevLocationType != null )
                                {
                                    foreach ( var prevLoc in groupLocationService.Queryable( "Location,GroupLocationTypeValue" )
                                        .Where( gl =>
                                            gl.GroupId == group.Id &&
                                            gl.GroupLocationTypeValueId == locationType.Id ) )
                                    {
                                        prevLoc.GroupLocationTypeValueId = prevLocationType.Id;
                                        prevLoc.IsMailingLocation = false;
                                        prevLoc.IsMappedLocation = false;
                                    }
                                }
                            }

                            string addressChangeField = locationType.Value;

                            var groupLocation = groupLocationService.Queryable()
                                .Where( gl =>
                                    gl.GroupId == group.Id &&
                                    gl.LocationId == location.Id )
                                .FirstOrDefault();
                            if ( groupLocation == null )
                            {
                                groupLocation = new GroupLocation();
                                groupLocation.Location = location;
                                groupLocation.IsMailingLocation = isMailingLocation;
                                groupLocation.IsMappedLocation = isMappedLocation;
                                group.GroupLocations.Add( groupLocation );
                            }
                            groupLocation.GroupLocationTypeValueId = locationType.Id;

                            rockContext.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes or Archives (Soft-Deletes) Group record depending on GroupType.EnableGroupHistory and if the Group has history snapshots. Returns a boolean flag indicating if the deletion was successful.
        /// </summary>
        /// <param name="item">The <see cref="Rock.Model.Group" /> to delete.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> that indicates if the <see cref="Rock.Model.Group" /> was deleted successfully.
        /// </returns>
        public override bool Delete( Group item )
        {
            var groupTypeCache = GroupTypeCache.Get( item.GroupTypeId );
            if ( groupTypeCache?.EnableGroupHistory == true )
            {
                var rockContext = this.Context as RockContext;
                var groupHistoricalService = new GroupHistoricalService( rockContext );
                var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
                if ( groupHistoricalService.Queryable().Any( a => a.GroupId == item.Id ) || groupMemberHistoricalService.Queryable().Any( a => a.GroupId == item.Id ) )
                {
                    // if this group's GroupType has GroupHistory enabled, and this group has group or group member history snapshots, then we need to Archive instead of Delete
                    this.Archive( item, null, false );
                    return true;
                }
            }

            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item );
        }

        /// <summary>
        /// Deletes or Archives (Soft-Deletes) Group record depending on GroupType.EnableGroupHistory and if the Group has history snapshots, with an option to
        /// remove it from Auth if it is a security role
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="removeFromAuthTables">if set to <c>true</c> [remove from authentication tables].</param>
        public void Delete( Group group, bool removeFromAuthTables )
        {
            bool isSecurityRoleGroup = group.IsActive && ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) );
            if ( removeFromAuthTables && isSecurityRoleGroup )
            {
                AuthService authService = new AuthService( this.Context as RockContext );

                foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                {
                    authService.Delete( auth );
                }

                Rock.Security.Authorization.Clear();
            }

            this.Delete( group );
        }

        /// <summary>
        /// Archives the specified group and removes it from Auth if it is a security role
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="removeFromAuthTables">if set to <c>true</c> remove from auth if this group is a security role.</param>
        public void Archive( Group group, int? currentPersonAliasId, bool removeFromAuthTables)
        {
            group.IsArchived = true;
            group.ArchivedByPersonAliasId = currentPersonAliasId;
            group.ArchivedDateTime = RockDateTime.Now;

            bool isSecurityRoleGroup = group.IsActive && ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) );
            if ( removeFromAuthTables && isSecurityRoleGroup )
            {
                AuthService authService = new AuthService( this.Context as RockContext );
                
                foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                {
                    authService.Delete( auth );
                }

                Rock.Security.Authorization.Clear();
            }
        }

        /// <summary>
        /// Checks to see if there is an Archived Member of the group for the specified personId and groupRoleId
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="archivedGroupMember">The archived group member record (if there are multiple, this will be the most recently archived record</param>
        /// <returns></returns>
        public bool ExistsAsArchived( Group group, int personId, int groupRoleId, out GroupMember archivedGroupMember )
        {
            var groupMemberService = new GroupMemberService( this.Context as RockContext );
            archivedGroupMember = groupMemberService.GetArchived().Where( a => a.GroupId == group.Id && a.PersonId == personId && a.GroupRoleId == groupRoleId ).OrderByDescending( a => a.ArchivedDateTime ).FirstOrDefault();
            return archivedGroupMember != null;
        }

        /// <summary>
        /// Returns true if duplicate group members are allowed in groups
        /// Normally this is false, but there is a web.config option to allow it
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        [Obsolete( "Please use the static method with no parameters. The group parameter is inconsequential.", false )]
        [RockObsolete( "1.9" )]
        public bool AllowsDuplicateMembers( Group group )
        {
            return AllowsDuplicateMembers();
        }

        /// <summary>
        /// Returns true if duplicate group members are allowed in groups
        /// Normally this is false, but there is a web.config option to allow it
        /// </summary>
        /// <returns></returns>
        public static bool AllowsDuplicateMembers()
        {
            var allowDuplicateGroupMembers = System.Configuration.ConfigurationManager.AppSettings["AllowDuplicateGroupMembers"].AsBoolean();
            return allowDuplicateGroupMembers;
        }

        /// <summary>
        /// Checks to see if there is an (unarchived) member of the group for the specified personId and groupRoleId
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="groupMember">The group member.</param>
        /// <returns></returns>
        public bool ExistsAsMember( Group group, int personId, int groupRoleId, out GroupMember groupMember )
        {
            var groupMemberService = new GroupMemberService( this.Context as RockContext );
            groupMember = groupMemberService.AsNoFilter().Where( a => a.IsArchived == false && a.GroupId == group.Id && a.PersonId == personId && a.GroupRoleId == groupRoleId ).FirstOrDefault();
            return groupMember != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class GroupServiceExtensions
    {
        /// <summary>
        /// Given an IQueryable of Groups, returns just the heads of households for those groups
        /// </summary>
        /// <param name="groups">The groups.</param>
        /// <returns></returns>
        public static IQueryable<Person> HeadOfHouseholds( this IQueryable<Group> groups )
        {
            return groups
                .SelectMany( f => f.Members )
                .GroupBy( m =>
                    m.GroupId,
                    ( key, g ) => g
                        .OrderBy( m => m.GroupRole.Order )
                        .ThenBy( m => m.Person.Gender )
                        .ThenBy( m => m.Person.BirthYear )
                        .ThenBy( m => m.Person.BirthMonth )
                        .ThenBy( m => m.Person.BirthDay )
                        .FirstOrDefault() )
                .Select( m => m.Person );
        }

        /// <summary>
        /// Given an IQueryable of members (i.e. family members), returns the head of household for those members
        /// </summary>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        public static Person HeadOfHousehold( this IQueryable<GroupMember> members )
        {
            return members
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.Gender )
                .ThenBy( m => m.Person.BirthYear )
                .ThenBy( m => m.Person.BirthMonth )
                .ThenBy( m => m.Person.BirthDay )
                .Select( m => m.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// For the group, gets a family member that matches the given person. A match
        /// is found if the nickname matches the nickname or first name, the last name matches, and,
        /// if there is a birth date on the potential match, the birth date matches.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="personToMatch">The person to match.</param>
        /// <returns>a person (if a match was found) otherwise null</returns>
        public static Person MatchingFamilyMember( this Group group, Person personToMatch )
        {
            return group.Members
            .Where( m =>
                ( m.Person.NickName == personToMatch.NickName || m.Person.FirstName == personToMatch.NickName ) &&
                m.Person.LastName == personToMatch.LastName &&
                m.Person.BirthDate.HasValue &&
                m.Person.BirthDate.Value == personToMatch.BirthDate.Value )
            .Select( m => m.Person )
            .FirstOrDefault();
        }

        /// <summary>
        /// Gets the active members.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static IEnumerable<GroupMember> ActiveMembers( this Group group )
        {
            return group.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
        }
    }
}
