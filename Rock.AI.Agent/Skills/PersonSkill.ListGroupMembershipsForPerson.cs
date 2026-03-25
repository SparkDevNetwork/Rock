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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Lists group membership records for a specific person." )]
        [AgentPurpose( "Retrieves the GroupMember records that identify the group's a person is a member of." )]
        [AgentToolGuid( "a02698ca-3c3a-48ed-adba-36f6f9b29cae" )]
        public IAgentToolResult ListGroupMembershipsForPerson(
            string personIdKey,
            string groupTypeIdKey = null,

            [Description( "Leave blank for a default choice based on other filters." )]
            bool? includeInactive = null,

            string cursor = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            // We intentionally keep tracking on so that security checks can
            // walk up the tree faster by re-using already loaded groups.
            IQueryable<GroupMember> qry = new GroupMemberService( AgentRequestContext.RockContext )
                .Queryable()
                .Include( gm => gm.Group.GroupType )
                .Include( gm => gm.GroupRole );

            qry = helper.WhereRequiredIdKey( qry, gm => gm.PersonId, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, gm => gm.Group.GroupTypeId, groupTypeIdKey );

            if ( includeInactive != true )
            {
                qry = qry.Where( gm => gm.GroupMemberStatus != GroupMemberStatus.Inactive );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var paginator = new CursorPaginator<GroupMember>( AgentRequestContext.CurrentPerson, q => q
                .OrderBy( gm => gm.Group.Name )
                .ThenBy( gm => gm.Id ) );

            var page = helper.GetCursorPaginatedItems( qry, paginator, cursor );

            var resultPage = page.WithItems( page.Items.Select( gm => new
            {
                GroupMemberIdKey = gm.IdKey,
                Group = new KeyNameResult( gm.Group.Id, gm.Group.Name ),
                GroupType = new KeyNameResult( gm.Group.GroupTypeId, gm.Group.GroupType.Name ),
                Role = new KeyNameResult( gm.GroupRole.Id, gm.GroupRole.Name ),
                gm.GroupRole.IsLeader,
                Status = gm.GroupMemberStatus,
                gm.DateTimeAdded,
                gm.InactiveDateTime,
            } ) );

            var historyPage = page.WithItems( page.Items.Select( gm => new
            {
                GroupIdKey = gm.Group.IdKey,
                GroupName = gm.Group.Name,
            } ) );

            return helper.GetPaginatedResult( resultPage, historyPage );
        }

        #endregion
    }
}
