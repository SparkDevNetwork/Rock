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
using System.Linq;

using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Utility;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the person's membership
    /// in the group.
    /// </summary>
    internal class MembershipOpportunityFilter : OpportunityFilter
    {
        #region Properties

        /// <summary>
        /// Gets the group identifiers that the person is an active member of.
        /// </summary>
        /// <value>The group identifiers.</value>
        public Lazy<HashSet<string>> GroupIds { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MembershipOpportunityFilter"/> class.
        /// </summary>
        public MembershipOpportunityFilter()
        {
            // Make sure we only load the group unique identifiers once. Because
            // this is lazy initialized, it will only load the data if we come
            // across any group with an AlreadyBelongs attendance rule.
            GroupIds = new Lazy<HashSet<string>>( () =>
            {
                var personIdNumber = IdHasher.Instance.GetId( Person.Person.Id ) ?? 0;
                var groupIds = new GroupMemberService( RockContext )
                    .Queryable()
                    .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active
                        && m.Person.Id == personIdNumber
                        && m.GroupRole.IsCheckInAllowed )
                    .Select( m => m.Group.Id )
                    .ToList()
                    .Select( id => IdHasher.Instance.GetHash( id ) );

                return new HashSet<string>( groupIds );
            } );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            if ( group.CheckInAreaData.AttendanceRule == AttendanceRule.AlreadyEnrolledInGroup )
            {
                var isMember = GroupIds.Value.Contains( group.Id );

                // If the area is configured to prefer enrolled groups then mark
                // this group as preferred if they are a member.
                if ( isMember && group.CheckInAreaData.AlreadyEnrolledMatchingLogic == AlreadyEnrolledMatchingLogic.PreferEnrolledGroups )
                {
                    group.IsPreferredGroup = true;
                }

                return isMember;
            }

            return true;
        }

        #endregion
    }
}
