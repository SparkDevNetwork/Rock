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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Follow.Suggestion
{
    /// <summary>
    /// In Followed Group following suggestion
    /// </summary>
    [Description( "In Followed Group" )]
    [Export( typeof( SuggestionComponent ) )]
    [ExportMetadata( "ComponentName", "InFollowedGroup" )]

    [GroupTypeField( "Group Type", "The group type that this suggestion applies to", true, order: 0 )]
    [GroupRoleField( null, "Group Role (optional)", "The group role that people must belong to (optional).", false, order: 3, key: "GroupRole" )]
    [BooleanField( "Auto-Follow", "Determines if new people added to the group should be auto-followed.", false, IsRequired = true, Key = "AutoFollow" )]
    public class InFollowedGroup : SuggestionComponent
    {
        #region Suggestion Component Implementation

        /// <summary>
        /// Gets the followed entity type identifier.
        /// </summary>
        /// <value>
        /// The followed entity type identifier.
        /// </value>
        public override Type FollowedType
        {
            get { return typeof( Rock.Model.PersonAlias ); }
        }

        /// <summary>
        /// Gets the suggestions.
        /// </summary>
        /// <param name="followingSuggestionType">Type of the following suggestion.</param>
        /// <param name="followerPersonIds">The follower person ids.</param>
        /// <returns></returns>
        public override List<PersonEntitySuggestion> GetSuggestions( FollowingSuggestionType followingSuggestionType, List<int> followerPersonIds )
        {
            var suggestions = new List<PersonEntitySuggestion>();
            var groupEntityType = EntityTypeCache.Get( typeof( Rock.Model.Group ) );
            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ) );
            bool isAutoFollow = GetAttributeValue( followingSuggestionType, "AutoFollow" ).AsBoolean();

            // Get the grouptype guid
            Guid? groupTypeGuid = GetAttributeValue( followingSuggestionType, "GroupType" ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {

                    var followingService = new FollowingService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );
                    var personAliasService = new PersonAliasService( rockContext );

                    var followings = followingService.Queryable()
                                .Where( a => a.EntityTypeId == groupEntityType.Id &&
                                    followerPersonIds.Contains( a.PersonAlias.PersonId ) )
                                     .Select( f => new
                                     {
                                         f.PersonAliasId,
                                         f.PersonAlias.PersonId,
                                         f.EntityId
                                     } ).Distinct()
                                    .ToList();

                    var followedGroup = followings.Select( a => a.EntityId ).Distinct().ToList();
                    // Get all the groupmember records of Followed group
                    var followedGroupMembersQry = groupMemberService.Queryable().AsNoTracking()
                        .Where( m =>
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Group != null &&
                            m.Group.IsActive && !m.Group.IsArchived &&
                            m.Group.GroupType.Guid.Equals( groupTypeGuid.Value ) &&
                            followedGroup.Contains( m.GroupId ) );


                    // If a specific role for the people being followed was specified, limit the query to only those with the selected role
                    Guid? groupRoleGuid = GetAttributeValue( followingSuggestionType, "GroupRole" ).AsGuidOrNull();
                    if ( groupRoleGuid.HasValue )
                    {
                        followedGroupMembersQry = followedGroupMembersQry.Where( m => m.GroupRole.Guid.Equals( groupRoleGuid.Value ) );
                    }

                    var followedGroupMembers = followedGroupMembersQry.ToList();

                    // Run the query to get all the groups that follower is a member of with selected filters
                    var followerPersonGroups = followings
                        .Select( f => new
                        {
                            GroupMembers = followedGroupMembers.Where( a => a.GroupId == f.EntityId ).ToList(),
                            f.PersonAliasId,
                            f.PersonId
                        } )
                        .ToList();

                    var followedMembersList = followedGroupMembers
                                        .Select( a => a.PersonId )
                                        .Distinct()
                                        .ToList();
                    // Build a dictionary of the personid->personaliasid 
                    var personAliasIds = new Dictionary<int, int>();
                    personAliasService.Queryable().AsNoTracking()
                        .Where( a =>
                            followedMembersList.Contains( a.PersonId ) &&
                            a.PersonId == a.AliasPersonId )
                        .ToList()
                        .ForEach( a => personAliasIds.AddOrIgnore( a.PersonId, a.Id ) );


                    // Loop through each follower/group combination
                    foreach ( var followerPersonGroup in followerPersonGroups )
                    {
                        // Loop through the other people in that group
                        foreach ( var member in followerPersonGroup.GroupMembers )
                        {
                            if ( !isAutoFollow )
                            {
                                // add them to the list of suggestions
                                suggestions.Add( new PersonEntitySuggestion( followerPersonGroup.PersonId, personAliasIds[member.PersonId] ) );
                            }
                            else
                            {
                                // auto-add the follow
                                int followeePersonAliasId = personAliasIds[member.PersonId];

                                // if person is not already following the person
                                bool isFollowing = followingService.Queryable().Where( f =>
                                                        f.EntityTypeId == personAliasEntityType.Id
                                                        && f.EntityId == followeePersonAliasId
                                                        && f.PersonAliasId == followerPersonGroup.PersonAliasId ).Any();
                                if ( !isFollowing )
                                {
                                    var following = new Following();
                                    following.EntityTypeId = personAliasEntityType.Id;
                                    following.EntityId = followeePersonAliasId;
                                    following.PersonAliasId = followerPersonGroup.PersonAliasId;
                                    followingService.Add( following );
                                    rockContext.SaveChanges();
                                }
                            }
                        }

                    }
                }
            }

            return suggestions;
        }

        /// <summary>
        /// Sorts the notifications.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public override List<IEntity> SortEntities( List<IEntity> entities )
        {
            var people = new List<PersonAlias>();
            foreach ( var entity in entities )
            {
                people.Add( entity as PersonAlias );
            }
            var orderedEntities = new List<IEntity>();
            people
                .OrderBy( p => p.Person.LastName )
                .ThenBy( p => p.Person.NickName )
                .ToList()
                .ForEach( p => orderedEntities.Add( p ) );

            return orderedEntities;
        }

        #endregion

    }
}
