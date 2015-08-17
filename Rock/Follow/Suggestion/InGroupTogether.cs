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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Attribute;
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
    [GroupRoleField( "", "Follower Group Type (optional)", "If specified, only people with this role will be be notified (Make sure to select same group type as above).", false, order:1, key:"FollowerGroupType" )]
    [GroupRoleField( "", "Followed Group Type (optional)", "If specified, only people with this role will be suggested to the follower (Make sure to select same group type as above).", false, order:2, key:"FollowedGroupType" )]
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

            Guid? groupTypeGuid = GetAttributeValue( followingSuggestionType, "GroupType" ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );

                    var followers = groupMemberService.Queryable()
                        .Where( m =>
                            m.Group != null &&
                            m.Group.GroupType.Guid.Equals( groupTypeGuid.Value ) &&
                            followerPersonIds.Contains( m.PersonId ) );

                    Guid? followerRoleGuid = GetAttributeValue( followingSuggestionType, "FollowerGroupType" ).AsGuidOrNull();
                    if ( followerRoleGuid.HasValue )
                    {
                        followers = followers.Where( m => m.GroupRole.Guid.Equals( followerRoleGuid.Value ) );
                    }

                    var followed = groupMemberService.Queryable();

                    Guid? followedRoleGuid = GetAttributeValue( followingSuggestionType, "FollowedGroupType" ).AsGuidOrNull();
                    if ( followedRoleGuid.HasValue )
                    {
                        followed = followed.Where( m => m.GroupRole.Guid.Equals( followedRoleGuid.Value ) );
                    }

                    foreach ( var suggestion in followers
                        .Join( followed, s => s.GroupId, t => t.GroupId, ( s, t ) => new { s, t } )
                        .Where( j => j.s.PersonId != j.t.PersonId )

                        .Select( j => new
                        {
                            FollowerPersonId = j.s.PersonId,
                            FollowedPerson = j.t.Person
                        } ) )
                    {
                        int? entityId = suggestion.FollowedPerson.PrimaryAliasId;
                        if ( entityId.HasValue )
                        {
                            suggestions.Add( new PersonEntitySuggestion( suggestion.FollowerPersonId, entityId.Value ) );
                        }
                    }
                }
            }

            return suggestions;
        }

        #endregion

    }
}
