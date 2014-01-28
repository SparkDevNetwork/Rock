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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/service class for <see cref="Rock.Model.Group"/> entity type objects that extends the functionality of <see cref="Rock.Data.Service"/>
    /// </summary>
    public partial class GroupService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group"/> entities that by their <see cref="Rock.Model.GroupType"/> Id.
        /// </summary>
        /// <param name="groupTypeId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that they belong to.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> belong to a specific <see cref="Rock.Model.GroupType"/>.</returns>
        public IEnumerable<Group> GetByGroupTypeId( int groupTypeId )
        {
            return Repository.Find( t => t.GroupTypeId == groupTypeId );
        }


        /// <summary>
        /// Returns the <see cref="Rock.Model.Group"/> containing a Guid property that matches the provided value.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> to find a <see cref="Rock.Model.Group"/> by.</param>
        /// <returns>The <see cref="Rock.Model.Group" /> who's Guid property matches the provided value.  If no match is found, returns null.</returns>
        public Group GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group">Groups</see> by their IsSecurityRole flag.
        /// </summary>
        /// <param name="isSecurityRole">A <see cref="System.Boolean"/> representing the IsSecurityRole flag value to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> that contains a IsSecurityRole flag that matches the provided value.</returns>
        public IEnumerable<Group> GetByIsSecurityRole( bool isSecurityRole )
        {
            return Repository.Find( t => t.IsSecurityRole == isSecurityRole );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Group">Groups</see> by the Id of it's parent <see cref="Rock.Model.Group"/>. 
        /// </summary>
        /// <param name="parentGroupId">A <see cref="System.Int32" /> representing the Id of the parent <see cref="Rock.Model.Group"/> to search by. This value
        /// is nullable and a null value will search for <see cref="Rock.Model.Group">Groups</see> that do not inherit from other groups.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> who's ParentGroupId matches the provided value.</returns>
        public IEnumerable<Group> GetByParentGroupId( int? parentGroupId )
        {
            return Repository.Find( t => ( t.ParentGroupId == parentGroupId || ( parentGroupId == null && t.ParentGroupId == null ) ) );
        }


        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group">Groups</see> by the Id of their parent <see cref="Rock.Model.Group"/> and by the Group's name.
        /// </summary>
        /// <param name="parentGroupId">An <see cref="System.Int32" /> representing the Id of the parent <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="name">A <see cref="System.String"/> containing the Name of the <see cref="Rock.Model.Group"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> who's ParentGroupId and Name matches the provided values.</returns>
        public IEnumerable<Group> GetByParentGroupIdAndName( int? parentGroupId, string name )
        {
            return Repository.Find( t => ( t.ParentGroupId == parentGroupId || ( parentGroupId == null && t.ParentGroupId == null ) ) && t.Name == name );
        }

        /// <summary>
        /// Gets the navigation children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootGroupId">The root group identifier.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <returns></returns>
        public IQueryable<Group> GetNavigationChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, string groupTypeIds )
        {
            var qry = Repository.AsQueryable();

            if ( id == 0 )
            {
                qry = qry.Where( a => a.ParentGroupId == null );
                if ( rootGroupId != 0 )
                {
                    qry = qry.Where( a => a.Id == rootGroupId );
                }
            }
            else
            {
                qry = qry.Where( a => a.ParentGroupId == id );
            }

            if ( limitToSecurityRoleGroups )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
            {
                if ( groupTypeIds != "0" )
                {
                    List<int> groupTypes = groupTypeIds.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();

                    qry = qry.Where( a => groupTypes.Contains( a.GroupTypeId ) );
                }
            }

            qry = qry.Where( a => a.GroupType.ShowInNavigation == true );

            return qry;
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Group">Groups</see> that are descendants of a specified group.
        /// </summary>
        /// <param name="parentGroupId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> to retrieve descendants for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Group">Groups</see> that are descendants of referenced group.</returns>
        public IEnumerable<Group> GetAllDescendents( int parentGroupId )
        {
            return Repository.ExecuteQuery( 
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
        /// Adds the person to a new family record
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        public Group SaveNewFamily( Person person, int? campusId, PersonAlias personAlias )
        {
            var groupMember = new GroupMember();
            groupMember.Person = person;

            var adultRole = new GroupTypeRoleService(this.RockContext).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
            if (adultRole != null)
            {
                groupMember.GroupRoleId = adultRole.Id;
            }

            var groupMembers = new List<GroupMember>();
            groupMembers.Add( groupMember );

            return SaveNewFamily( groupMembers, campusId, personAlias );
        }

        /// <summary>
        /// Creates a new family group and adds the list of new people to this family.  
        /// </summary>
        /// <param name="people">The people.</param>
        /// <returns></returns>
        public Group SaveNewFamily( List<GroupMember> familyMembers, int? campusId, PersonAlias personAlias )
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();

            var familyChanges = new List<string>();
            var familyMemberChanges = new Dictionary<Guid, List<string>>();
            var familyDemographicChanges = new Dictionary<Guid, List<string>>();

            if ( familyGroupType != null )
            {
                var familyGroup = new Group();

                familyChanges.Add( "Created" );

                familyGroup.GroupTypeId = familyGroupType.Id;

                familyGroup.Name = familyMembers.FirstOrDefault().Person.LastName + " Family";
                History.EvaluateChange( familyChanges, "Name", string.Empty, familyGroup.Name );

                if ( campusId.HasValue )
                {
                    History.EvaluateChange( familyChanges, "Campus", string.Empty, CampusCache.Read( campusId.Value ).Name );
                }
                familyGroup.CampusId = campusId;

                int? childRoleId = null;
                var childRole = new GroupTypeRoleService( this.RockContext ).Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) );
                if ( childRole != null )
                {
                    childRoleId = childRole.Id;
                }

                foreach ( var familyMember in familyMembers )
                {
                    var person = familyMember.Person;
                    if ( person != null )
                    {
                        familyGroup.Members.Add( familyMember );

                        var demographicChanges = new List<string>();
                        demographicChanges.Add( "Created" );

                        History.EvaluateChange( demographicChanges, "Record Type", string.Empty, person.RecordTypeValueId.HasValue ? DefinedValueCache.GetName( person.RecordTypeValueId.Value ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Record Status", string.Empty, person.RecordStatusValueId.HasValue ? DefinedValueCache.GetName( person.RecordStatusValueId.Value ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Record Status Reason", string.Empty, person.RecordStatusReasonValueId.HasValue ? DefinedValueCache.GetName( person.RecordStatusReasonValueId.Value ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Connection Status", string.Empty, person.ConnectionStatusValueId.HasValue ? DefinedValueCache.GetName( person.ConnectionStatusValueId ) : string.Empty );
                        History.EvaluateChange( demographicChanges, "Deceased", false.ToString(), ( person.IsDeceased ?? false ).ToString() );
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
                        History.EvaluateChange( demographicChanges, "Graduation Date", null, person.GraduationDate );
                        History.EvaluateChange( demographicChanges, "Email", string.Empty, person.Email );
                        History.EvaluateChange( demographicChanges, "Email Active", false.ToString(), ( person.IsEmailActive ?? false ).ToString() );
                        History.EvaluateChange( demographicChanges, "Email Note", string.Empty, person.EmailNote );
                        History.EvaluateChange( demographicChanges, "Do Not Email", false.ToString(), person.DoNotEmail.ToString() );
                        History.EvaluateChange( demographicChanges, "System Note", string.Empty, person.SystemNote );

                        familyDemographicChanges.Add( person.Guid, demographicChanges );

                        var memberChanges = new List<string>();
                        string roleName = familyGroupType.Roles[familyMember.GroupRoleId] ?? string.Empty;
                        History.EvaluateChange( memberChanges, "Role", string.Empty, roleName );
                        familyMemberChanges.Add( person.Guid, memberChanges );
                    }
                }

                Add( familyGroup, personAlias );
                Save( familyGroup, personAlias );

                var historyService = new HistoryService( this.RockContext );

                historyService.SaveChanges( typeof( Group ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                    familyGroup.Id, familyChanges, personAlias );

                var personService = new PersonService( this.RockContext );

                foreach ( var groupMember in familyMembers )
                {
                    var person = personService.Get( groupMember.PersonId );
                    if ( person != null )
                    {
                        bool updateRequired = false;
                        if ( !person.Aliases.Any( a => a.AliasPersonId == person.Id ) )
                        {
                            person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
                            updateRequired = true;
                        }
                        var changes = familyDemographicChanges[person.Guid];
                        if ( groupMember.GroupRoleId != childRoleId )
                        {
                            person.GivingGroupId = familyGroup.Id;
                            updateRequired = true;
                            History.EvaluateChange( changes, "Giving Group", string.Empty, familyGroup.Name );
                        }

                        if ( updateRequired )
                        {
                            personService.Save( person, personAlias );
                        }

                        historyService.SaveChanges( typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                            person.Id, changes, personAlias );

                        historyService.SaveChanges( typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                            person.Id, familyMemberChanges[person.Guid], familyGroup.Name, typeof( Group ), familyGroup.Id, personAlias );
                    }
                }

                return familyGroup;
            }

            return null;
        }

        public void AddNewFamilyAddress( Group family, string locationTypeGuid, string street1, string street2, string city, string state, string zip, PersonAlias personAlias )
        {
            if ( !String.IsNullOrWhiteSpace( street1 ) ||
                 !String.IsNullOrWhiteSpace( street2 ) ||
                 !String.IsNullOrWhiteSpace( city ) ||
                 !String.IsNullOrWhiteSpace( zip ) )
            {
                string addressChangeField = "Location";

                var groupLocation = new GroupLocation();

                // Get new or existing location and associate it with group
                var location = new LocationService( this.RockContext ).Get( street1, street2, city, state, zip );
                groupLocation.Location = location;
                groupLocation.IsMailingLocation = true;
                groupLocation.IsMappedLocation = true;

                Guid guid = Guid.Empty;
                if ( Guid.TryParse( locationTypeGuid, out guid ) )
                {
                    var locationType = Rock.Web.Cache.DefinedValueCache.Read( guid );
                    if ( locationType != null )
                    {
                        addressChangeField = locationType.Name;
                        groupLocation.GroupLocationTypeValueId = locationType.Id;
                    }
                }

                family.GroupLocations.Add( groupLocation );

                var familyChanges = new List<string>();
                History.EvaluateChange( familyChanges, addressChangeField, string.Empty, groupLocation.Location.ToString() );
                History.EvaluateChange( familyChanges, addressChangeField + " Is Mailing", string.Empty, groupLocation.IsMailingLocation.ToString() );
                History.EvaluateChange( familyChanges, addressChangeField + " Is Map Location", string.Empty, groupLocation.IsMappedLocation.ToString() );

                Save( family, personAlias );

                var historyService = new HistoryService(this.RockContext);
                historyService.SaveChanges( typeof( Group ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                    family.Id, familyChanges, personAlias );
            }
        }

        /// <summary>
        /// Deletes a specified group. Returns a boolean flag indicating if the deletion was successful.
        /// </summary>
        /// <param name="item">The <see cref="Rock.Model.Group"/> to delete.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns>A <see cref="System.Boolean"/> that indicates if the <see cref="Rock.Model.Group"/> was deleted successfully.</returns>
        public override bool Delete( Group item, PersonAlias personAlias )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item, personAlias );
        }
    }
}
