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

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Group"/> objects.
    /// </summary>
    public partial class GroupService
    {
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

            Guid familyTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            return groupLocationService.GetMappedLocationsByGeofences( geofences )
                .Where( l =>
                    l.Group != null &&
                    l.Group.GroupType != null &&
                    l.Group.GroupType.Guid.Equals( familyTypeGuid ) )
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
        /// Gets immediate children of a group (id) or a rootGroupId. Specify 0 for both Id and rootGroupId to get top level groups limited
        /// </summary>
        /// <param name="id">The ID of the Group to get the children of (or 0 to use rootGroupId)</param>
        /// <param name="rootGroupId">The root group ID</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIncludedIds">The group type included ids.</param>
        /// <param name="groupTypeExcludedIds">The group type excluded ids.</param>
        /// <param name="includeInactiveGroups">if set to <c>true</c> [include inactive groups].</param>
        /// <param name="limitToShowInNavigation">if set to <c>true</c> [limit to show in navigation].</param>
        /// <returns></returns>
        public IQueryable<Group> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, List<int> groupTypeIncludedIds, List<int> groupTypeExcludedIds, bool includeInactiveGroups, bool limitToShowInNavigation )
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

            if ( limitToSecurityRoleGroups )
            {
                qry = qry.Where( a => a.IsSecurityRole );
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
        /// Groups the members not meeting requirements.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="includeWarnings">if set to <c>true</c> [include warnings].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>> GroupMembersNotMeetingRequirements( int groupId, bool includeWarnings, bool includeInactive = false )
        {
            Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>> results = new Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>>();
            
            var rockContext = this.Context as RockContext;
            var groupRequirementService = new GroupRequirementService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );

            var qryGroupRequirements = groupRequirementService.Queryable().Where( a => a.GroupId == groupId ).ToList();
            bool hasGroupRequirements = qryGroupRequirements.Any();
            if ( !hasGroupRequirements )
            {
                // if no group requirements, then there are no members that don't meet the requirements, so return an empty dictionary
                return new Dictionary<GroupMember, Dictionary<PersonGroupRequirementStatus, DateTime>>();
            }

            var qryGroupMembers = groupMemberService.Queryable().Where( a => a.GroupId == groupId );
            var qryGroupMemberRequirements = groupMemberRequirementService.Queryable().Where( a => a.GroupMember.GroupId == groupId );

            if ( !includeInactive )
            {
                qryGroupMembers = qryGroupMembers.Where( a => a.GroupMemberStatus == GroupMemberStatus.Active );
            }

            var groupMembers = qryGroupMembers.ToList();

            // get a list of group member ids that don't meet all the requirements
            List<int> qryGroupMemberIdsThatLackGroupRequirements = groupMembers
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

            IEnumerable<GroupMember> qryMembersWithIssues;

            if ( includeWarnings )
            {
                IQueryable<int> qryGroupMemberIdsWithRequirementWarnings = qryGroupMemberRequirements
                    .Where( 
                        a => 
                            a.RequirementWarningDateTime != null || 
                            a.RequirementFailDateTime != null )
                    .Select( a => a.GroupMemberId )
                    .Distinct();

                qryMembersWithIssues = groupMembers.Where( a => qryGroupMemberIdsThatLackGroupRequirements.Contains( a.Id ) || qryGroupMemberIdsWithRequirementWarnings.Contains( a.Id ) );
            }
            else
            {
                qryMembersWithIssues = groupMembers.Where( a => qryGroupMemberIdsThatLackGroupRequirements.Contains( a.Id ) );
            }

            var qry = qryMembersWithIssues.Select( a => new
            {
                GroupMember = a,
                GroupRequirementStatuses = qryGroupMemberRequirements.Where( x => x.GroupMemberId == a.Id )
            } );

            var currentDateTime = RockDateTime.Now;

            foreach (var groupMemberWithIssues in qry)
            {
                Dictionary<PersonGroupRequirementStatus, DateTime> statuses = new Dictionary<PersonGroupRequirementStatus, DateTime>();
                
                // populate where the status is known
                foreach ( var requirementStatus in groupMemberWithIssues.GroupRequirementStatuses )
                {
                    PersonGroupRequirementStatus status = new PersonGroupRequirementStatus();
                    status.GroupRequirement = requirementStatus.GroupRequirement;
                    status.PersonId = groupMemberWithIssues.GroupMember.PersonId;

                    DateTime occuranceDate = new DateTime();

                    if ( requirementStatus.RequirementMetDateTime == null)
                    {
                        status.MeetsGroupRequirement = MeetsGroupRequirement.NotMet;
                        occuranceDate = requirementStatus.RequirementFailDateTime ?? currentDateTime; 
                    }
                    else if (requirementStatus.RequirementWarningDateTime.HasValue)
                    {
                        status.MeetsGroupRequirement = MeetsGroupRequirement.MeetsWithWarning;
                        occuranceDate = requirementStatus.RequirementWarningDateTime.Value;
                    }
                    else
                    {
                        status.MeetsGroupRequirement = MeetsGroupRequirement.Meets;
                        occuranceDate = requirementStatus.RequirementMetDateTime.Value;
                    }
                    
                    statuses.Add( status, occuranceDate );
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
            var groupType = GroupTypeCache.Read( groupTypeId );
            var familyChanges = new List<string>();
            var familyMemberChanges = new Dictionary<Guid, List<string>>();
            var personDemographicChanges = new Dictionary<Guid, List<string>>();

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

                History.EvaluateChange( familyChanges, "Family", string.Empty, group.Name );

                if ( isFamilyGroupType )
                {
                    if ( campusId.HasValue )
                    {
                        History.EvaluateChange( familyChanges, "Campus", string.Empty, CampusCache.Read( campusId.Value ).Name );
                    }
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

                        var demographicChanges = new List<string>();
                        demographicChanges.Add( "Created" );

                        History.EvaluateChange( demographicChanges, "Record Type", string.Empty, person.RecordTypeValueId.HasValue ? DefinedValueCache.GetName( person.RecordTypeValueId.Value ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Record Status", string.Empty, person.RecordStatusValueId.HasValue ? DefinedValueCache.GetName( person.RecordStatusValueId.Value ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Record Status Reason", string.Empty, person.RecordStatusReasonValueId.HasValue ? DefinedValueCache.GetName( person.RecordStatusReasonValueId.Value ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Connection Status", string.Empty, person.ConnectionStatusValueId.HasValue ? DefinedValueCache.GetName( person.ConnectionStatusValueId ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Deceased", false.ToString(), ( person.IsDeceased ).ToString() );
                        History.EvaluateChange( demographicChanges, "Title", string.Empty, person.TitleValueId.HasValue ? DefinedValueCache.GetName( person.TitleValueId ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "First Name", string.Empty, person.FirstName );
                        History.EvaluateChange( demographicChanges, "Nick Name", string.Empty, person.NickName );
                        History.EvaluateChange( demographicChanges, "Middle Name", string.Empty, person.MiddleName );
                        History.EvaluateChange( demographicChanges, "Last Name", string.Empty, person.LastName );
                        History.EvaluateChange( demographicChanges, "Suffix", string.Empty, person.SuffixValueId.HasValue ? DefinedValueCache.GetName( person.SuffixValueId ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Birth Date", null, person.BirthDate );
                        History.EvaluateChange( demographicChanges, "Gender", null, person.Gender );
                        History.EvaluateChange( demographicChanges, "Marital Status", string.Empty, person.MaritalStatusValueId.HasValue ? DefinedValueCache.GetName( person.MaritalStatusValueId ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Anniversary Date", null, person.AnniversaryDate );
                        History.EvaluateChange( demographicChanges, "Graduation Year", null, person.GraduationYear );
                        History.EvaluateChange( demographicChanges, "Email", string.Empty, person.Email );
                        History.EvaluateChange( demographicChanges, "Email Active", false.ToString(), person.IsEmailActive.ToString() );
                        History.EvaluateChange( demographicChanges, "Email Note", string.Empty, person.EmailNote );
                        History.EvaluateChange( demographicChanges, "Email Preference", null, person.EmailPreference );
                        History.EvaluateChange( demographicChanges, "Inactive Reason Note", string.Empty, person.InactiveReasonNote );
                        History.EvaluateChange( demographicChanges, "System Note", string.Empty, person.SystemNote );

                        personDemographicChanges.Add( person.Guid, demographicChanges );

                        if ( isFamilyGroupType )
                        {
                            var memberChanges = new List<string>();

                            string roleName = groupType.Roles
                                .Where( r => r.Id == groupMember.GroupRoleId )
                                .Select( r => r.Name )
                                .FirstOrDefault();

                            History.EvaluateChange( memberChanges, "Role", string.Empty, roleName );
                            familyMemberChanges.Add( person.Guid, memberChanges );
                        }
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
                                History.EvaluateChange( personDemographicChanges[person.Guid], attributeCache.Name,
                                    attributeCache.FieldType.Field.FormatValue( null, oldValue, attributeCache.QualifierValues, false ),
                                    attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                Rock.Attribute.Helper.SaveAttributeValue( person, attributeCache, newValue );
                            }
                        }
                    }

                    person = personService.Get( groupMember.PersonId );
                    if ( person != null )
                    {
                        bool updateRequired = false;
                        var changes = personDemographicChanges[person.Guid];
                        if ( groupMember.GroupRoleId == adultRoleId )
                        {
                            person.GivingGroupId = group.Id;
                            updateRequired = true;
                            History.EvaluateChange( changes, "Giving Group", string.Empty, group.Name );
                        }

                        if ( updateRequired )
                        {
                            rockContext.SaveChanges();
                        }

                        int? modifiedByPersonAliasId = person.ModifiedAuditValuesAlreadyUpdated ? person.ModifiedByPersonAliasId : ( int? ) null;

                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                            person.Id, changes, true, modifiedByPersonAliasId );

                        if ( isFamilyGroupType )
                        {
                            HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                person.Id, familyMemberChanges[person.Guid], group.Name, typeof( Group ), group.Id, true, modifiedByPersonAliasId );

                            HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                person.Id, familyChanges, group.Name, typeof( Group ), group.Id, true, modifiedByPersonAliasId );
                        }
                    }
                }

                return group;
            }

            return null;
        }

        /// <summary>
        /// Adds the new group address.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="family">The family.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        [Obsolete("Use AddNewGroupAddress instead.")]
        public static void AddNewFamilyAddress( RockContext rockContext, Group family, string locationTypeGuid,
            string street1, string street2, string city, string state, string postalCode, string country, bool moveExistingToPrevious = false )
        {
            AddNewGroupAddress( rockContext, family, locationTypeGuid, street1, street2, city, state, postalCode, country, moveExistingToPrevious );
        }

        /// <summary>
        /// Adds the new group address.
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
            string street1, string street2, string city, string state, string postalCode, string country, bool moveExistingToPrevious = false )
        {
            if ( !String.IsNullOrWhiteSpace( street1 ) ||
                 !String.IsNullOrWhiteSpace( street2 ) ||
                 !String.IsNullOrWhiteSpace( city ) ||
                 !String.IsNullOrWhiteSpace( postalCode ) ||
                 !string.IsNullOrWhiteSpace( country ) )
            {
                var location = new LocationService( rockContext ).Get( street1, street2, city, state, postalCode, country );
                AddNewGroupAddress( rockContext, group, locationTypeGuid, location, moveExistingToPrevious );
            }
        }

        /// <summary>
        /// Adds the new family address.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="family">The family.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        [Obsolete( "Use AddNewGroupAddress instead." )]
        public static void AddNewFamilyAddress( RockContext rockContext, Group family, string locationTypeGuid,
            int? locationId, bool moveExistingToPrevious = false )
        {
            AddNewGroupAddress( rockContext, family, locationTypeGuid, locationId, moveExistingToPrevious );
        }

        /// <summary>
        /// Adds the new group address.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid, 
            int? locationId, bool moveExistingToPrevious = false )
        {
            if ( locationId.HasValue )
            {
                var location = new LocationService( rockContext ).Get( locationId.Value );
                AddNewGroupAddress( rockContext, group, locationTypeGuid, location, moveExistingToPrevious );
            }
        }

        /// <summary>
        /// Adds the new family address.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="family">The family.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        [Obsolete( "Use AddNewGroupAddress instead." )]
        public static void AddNewFamilyAddress( RockContext rockContext, Group family, string locationTypeGuid,
            Location location, bool moveExistingToPrevious = false )
        {
            AddNewGroupAddress( rockContext, family, locationTypeGuid, location, moveExistingToPrevious );
        }

        /// <summary>
        /// Adds the new group address.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="moveExistingToPrevious">if set to <c>true</c> [move existing to previous].</param>
        public static void AddNewGroupAddress( RockContext rockContext, Group group, string locationTypeGuid,
            Location location, bool moveExistingToPrevious = false )
        {
            if ( location != null )
            {
                var groupType = GroupTypeCache.Read( group.GroupTypeId );
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
                            var familyChanges = new List<string>();

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
                                        History.EvaluateChange( familyChanges, prevLoc.Location.ToString(), prevLoc.GroupLocationTypeValue.Value, prevLocationType.Value );
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
                                groupLocation.IsMailingLocation = true;
                                groupLocation.IsMappedLocation = true;
                                group.GroupLocations.Add( groupLocation );
                            }
                            groupLocation.GroupLocationTypeValueId = locationType.Id;

                            History.EvaluateChange( familyChanges, addressChangeField, string.Empty, groupLocation.Location.ToString() );
                            History.EvaluateChange( familyChanges, addressChangeField + " Is Mailing", string.Empty, groupLocation.IsMailingLocation.ToString() );
                            History.EvaluateChange( familyChanges, addressChangeField + " Is Map Location", string.Empty, groupLocation.IsMappedLocation.ToString() );

                            rockContext.SaveChanges();

                            if ( groupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ) )
                            {
                                foreach ( var fm in group.Members )
                                {
                                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                        fm.PersonId, familyChanges, group.Name, typeof( Group ), group.Id );
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a specified group. Returns a boolean flag indicating if the deletion was successful.
        /// </summary>
        /// <param name="item">The <see cref="Rock.Model.Group" /> to delete.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> that indicates if the <see cref="Rock.Model.Group" /> was deleted successfully.
        /// </returns>
        public override bool Delete( Group item )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item );
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
    }
}
