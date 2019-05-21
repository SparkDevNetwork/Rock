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
    /// In group together following suggestion
    /// </summary>
    [Description( "In Group Together" )]
    [Export( typeof( SuggestionComponent ) )]
    [ExportMetadata( "ComponentName", "InGroupTogether" )]

    [GroupTypeField("Group Type","The group type", true, order: 0 )]
    [GroupField("Group (optional)", "A specific group to evaluate (Make sure to select group with same group type as above).", false, order:1, key:"Group" )]
    [SecurityRoleField("Security Role (optional)", "A specific group to evaluate (Make sure to select group with same group type as above).", false, order:2, key:"SecurityRole")]
    [GroupRoleField( null, "Follower Group Type (optional)", "If specified, only people with this role will be be notified (Make sure to select same group type as above).", false, order:3, key:"FollowerGroupType" )]
    [GroupRoleField( null, "Followed Group Type (optional)", "If specified, only people with this role will be suggested to the follower (Make sure to select same group type as above).", false, order:4, key:"FollowedGroupType" )]
    [BooleanField("Auto-Follow", "Determines if new people added to the group should be auto-followed.", false, IsRequired = true, Key = "AutoFollow")]
    public class InGroupTogether : SuggestionComponent
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
            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ) );

            bool isAutoFollow = GetAttributeValue( followingSuggestionType, "AutoFollow" ).AsBoolean();

            // Get the grouptype guid
            Guid? groupTypeGuid = GetAttributeValue( followingSuggestionType, "GroupType" ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    var personAliasService = new PersonAliasService( rockContext );

                    // Get all the groupmember records for any follower and the selected group type
                    var followers = groupMemberService.Queryable().AsNoTracking()
                        .Where( m =>
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Group != null &&
                            m.Group.IsActive && !m.Group.IsArchived &&
                            m.Group.GroupType.Guid.Equals( groupTypeGuid.Value ) &&
                            followerPersonIds.Contains( m.PersonId ) );

                    // If a specific group or security role was specified, limit groupmembers to only those of the selected group
                    Guid? groupGuid = GetAttributeValue( followingSuggestionType, "Group" ).AsGuidOrNull();
                    if ( !groupGuid.HasValue )
                    {
                        groupGuid = GetAttributeValue( followingSuggestionType, "SecurityRole" ).AsGuidOrNull();
                    }
                    if ( groupGuid.HasValue )
                    {
                        followers = followers.Where( m => m.Group.Guid.Equals( groupGuid.Value ) );
                    }

                    // If a specific role for the follower was specified, limit groupmembers to only those with the selected role
                    Guid? followerRoleGuid = GetAttributeValue( followingSuggestionType, "FollowerGroupType" ).AsGuidOrNull();
                    if ( followerRoleGuid.HasValue )
                    {
                        followers = followers.Where( m => m.GroupRole.Guid.Equals( followerRoleGuid.Value ) );
                    }

                    // Run the query to get all the groups that follower is a member of with selected filters
                    var followerPersonGroup = followers
                        .Select( f => new
                        {
                            f.PersonId,
                            f.GroupId
                        } )
                        .ToList();

                    // Get a unique list of any of the groups that followers belong to
                    var followedGroupIds = followerPersonGroup
                        .Select( f => f.GroupId )
                        .Distinct()
                        .ToList();

                    // Start building query to get the people to follow from any group that contains a follower
                    var followed = groupMemberService
                        .Queryable().AsNoTracking()
                        .Where( m => followedGroupIds.Contains( m.GroupId ) && m.GroupMemberStatus == GroupMemberStatus.Active );

                    // If a specific role for the people being followed was specified, limit the query to only those with the selected role
                    Guid? followedRoleGuid = GetAttributeValue( followingSuggestionType, "FollowedGroupType" ).AsGuidOrNull();
                    if ( followedRoleGuid.HasValue )
                    {
                        followed = followed.Where( m => m.GroupRole.Guid.Equals( followedRoleGuid.Value ) );
                    }

                    // Get all the people in any of the groups that contain a follower
                    var followedPersonGroup = followed
                        .Select( f => new
                        {
                            f.PersonId,
                            f.GroupId
                        } )
                        .ToList();

                    // Get distinct list of people
                    var followedPersonIds = followedPersonGroup
                        .Select( f => f.PersonId )
                        .Distinct()
                        .ToList();

                    // Build a dictionary of the personid->personaliasid 
                    var personAliasIds = new Dictionary<int, int>();
                    personAliasService.Queryable().AsNoTracking()
                        .Where( a =>
                            followedPersonIds.Contains( a.PersonId ) &&
                            a.PersonId == a.AliasPersonId )
                        .ToList()
                        .ForEach( a => personAliasIds.AddOrIgnore( a.PersonId, a.Id ) );

                    // Loop through each follower/group combination
                    foreach ( var followedGroup in followerPersonGroup )
                    {
                        // Loop through the other people in that group
                        foreach ( int followedPersonId in followedPersonGroup
                            .Where( f =>
                                f.GroupId == followedGroup.GroupId &&
                                f.PersonId != followedGroup.PersonId )
                            .Select( f => f.PersonId ) )
                        {
                            // If the person has a valid personalias id
                            if ( personAliasIds.ContainsKey( followedPersonId ) )
                            {
                                if ( !isAutoFollow )
                                {
                                    // add them to the list of suggestions
                                    suggestions.Add( new PersonEntitySuggestion( followedGroup.PersonId, personAliasIds[followedPersonId] ) );
                                }
                                else
                                {
                                    // auto-add the follow
                                    
                                    var followingService = new FollowingService( rockContext );

                                    int followerPersonAliasId = personAliasIds[followedGroup.PersonId];
                                    int followeePersonAliasId = personAliasIds[followedPersonId];

                                    // if person is not already following the person
                                    bool isFollowing = followingService.Queryable().Where( f =>
                                                            f.EntityTypeId == personAliasEntityType.Id
                                                            && f.EntityId == followeePersonAliasId
                                                            && f.PersonAliasId == followerPersonAliasId ).Any();
                                    if ( !isFollowing )
                                    {
                                        var following = new Following();
                                        following.EntityTypeId = personAliasEntityType.Id;
                                        following.EntityId = personAliasIds[followedPersonId];
                                        following.PersonAliasId = personAliasIds[followedGroup.PersonId];
                                        followingService.Add( following );
                                        rockContext.SaveChanges();
                                    }
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
            foreach( var entity in entities )
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
