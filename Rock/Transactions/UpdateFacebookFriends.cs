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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Update Facebook Friend Relationships
    /// </summary>
    public class UpdateFacebookFriends : ITransaction
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the facebook ids.
        /// </summary>
        /// <value>
        /// The facebook ids.
        /// </value>
        public List<string> FacebookIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFacebookFriends" /> class.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="facebookIds">The facebook ids.</param>
        public UpdateFacebookFriends( int personId, List<string> facebookIds )
        {
            PersonId = personId;
            FacebookIds = facebookIds;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var relationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                if ( relationshipGroupType != null )
                {
                    var ownerRole = relationshipGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                        .FirstOrDefault();

                    var friendRole = relationshipGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_FACEBOOK_FRIEND.AsGuid() ) )
                        .FirstOrDefault();

                    if ( ownerRole != null && friendRole != null )
                    {
                        var userLoginService = new UserLoginService( rockContext );
                        var groupMemberService = new GroupMemberService( rockContext );

                        // Convert list of facebook ids into list of usernames
                        var friendUserNames = FacebookIds.Select( i => "FACEBOOK_" + i ).ToList();

                        // Get the list of person ids associated with friends usernames
                        var friendPersonIds = userLoginService.Queryable()
                            .Where( l =>
                                l.PersonId.HasValue &&
                                l.PersonId != PersonId &&
                                friendUserNames.Contains( l.UserName ) )
                            .Select( l => l.PersonId.Value )
                            .Distinct()
                            .ToList();

                        // Get the person's group id
                        var personGroup = groupMemberService.Queryable()
                            .Where( m =>
                                m.PersonId == PersonId &&
                                m.GroupRoleId == ownerRole.Id &&
                                m.Group.GroupTypeId == relationshipGroupType.Id )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        // Verify that a 'known relationships' type group existed for the person, if not create it
                        if ( personGroup == null )
                        {
                            var groupMember = new GroupMember();
                            groupMember.PersonId = PersonId;
                            groupMember.GroupRoleId = ownerRole.Id;

                            personGroup = new Group();
                            personGroup.Name = relationshipGroupType.Name;
                            personGroup.GroupTypeId = relationshipGroupType.Id;
                            personGroup.Members.Add( groupMember );

                            var groupService = new GroupService( rockContext );
                            groupService.Add( personGroup );
                            rockContext.SaveChanges();
                        }

                        // Get the person's relationship group id
                        var personGroupId = personGroup.Id;

                        // Get all of the friend's relationship group ids
                        var friendGroupIds = groupMemberService.Queryable()
                            .Where( m =>
                                m.Group.GroupTypeId == relationshipGroupType.Id &&
                                m.GroupRoleId == ownerRole.Id &&
                                friendPersonIds.Contains( m.PersonId ) )
                            .Select( m => m.GroupId )
                            .Distinct()
                            .ToList();

                        // Find all the existing friend relationships in Rock ( both directions )
                        var existingFriends = groupMemberService.Queryable()
                            .Where( m =>
                                m.Group.GroupTypeId == relationshipGroupType.Id && (
                                    ( friendPersonIds.Contains( m.PersonId ) && m.GroupId == personGroupId ) ||
                                    ( m.PersonId == PersonId && m.GroupId != personGroupId )
                                ) )
                            .ToList();

                        // Create temp group members for current Facebook friends 
                        var currentFriends = new List<GroupMember>();

                        // ( Person > Friend )
                        foreach ( int personId in friendPersonIds )
                        {
                            var groupMember = new GroupMember();
                            groupMember.GroupId = personGroupId;
                            groupMember.PersonId = personId;
                            groupMember.GroupRoleId = friendRole.Id;
                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            currentFriends.Add( groupMember );
                        }

                        // ( Friend > Person )
                        foreach ( int familyId in friendGroupIds )
                        {
                            var groupMember = new GroupMember();
                            groupMember.GroupId = familyId;
                            groupMember.PersonId = PersonId;
                            groupMember.GroupRoleId = friendRole.Id;
                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            currentFriends.Add( groupMember );
                        }

                        //  Add any current friends that do not exist in Rock yet
                        foreach ( var groupMember in currentFriends
                            .Where( f => !existingFriends.Any( e => e.IsEqualTo( f ) ) ) )
                        {
                            groupMemberService.Add( groupMember );
                        }

                        // Delete any existing friends that are no longer facebook friends
                        foreach ( var groupMember in existingFriends
                            .Where( f => !currentFriends.Any( e => e.IsEqualTo( f ) ) ) )
                        {
                            groupMemberService.Delete( groupMember );
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}