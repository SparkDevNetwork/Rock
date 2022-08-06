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
using System.Web;
using Rock.Data;
using Rock.Web.Cache;
using Z.EntityFramework.Plus;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.GroupMemberRequirement"/> entity objects. 
    /// </summary>
    public partial class GroupMemberRequirementService
    {

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who are members of a specific group.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="groupId"></param>
        /// <param name="groupRoleId"></param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.GroupMemberRequirement">GroupMemberRequirements</see> that belong to the specified person, group and role.
        /// </returns>
        public IQueryable<GroupMemberRequirement> GetActiveByPersonIdGroupIdGroupRoleId( int personId, int groupId, int? groupRoleId )
        {
            var groupMemberRequirements = Queryable( "GroupMember,GroupMemberRequirement" )
                .Where( r => r.GroupMember.PersonId == personId && r.GroupMember.GroupId == groupId );
            if ( groupRoleId.HasValue )
            {
                groupMemberRequirements = groupMemberRequirements.Where( r => r.GroupMember.GroupRoleId == groupRoleId.Value );
            }
            return groupMemberRequirements.OrderBy( r => r.GroupMember.GroupRole.Order );
        }
    }
}
