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

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    public partial class GroupMemberHistorical
    {
        /// <summary>
        /// Creates a GroupMemberHistorical with CurrentRowIndicator = true for the specified groupmember
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="effectiveDateTime">The effective date time.</param>
        /// <returns></returns>
        public static GroupMemberHistorical CreateCurrentRowFromGroupMember( GroupMember groupMember, DateTime effectiveDateTime )
        {
            var groupMemberHistoricalCurrent = new GroupMemberHistorical
            {
                GroupMemberId = groupMember.Id,
                GroupId = groupMember.GroupId,
                GroupRoleId = groupMember.GroupRoleId,
                GroupRoleName = groupMember.GroupRole.Name,
                IsLeader = groupMember.GroupRole.IsLeader,
                GroupMemberStatus = groupMember.GroupMemberStatus,
                IsArchived = groupMember.IsArchived,
                ArchivedDateTime = groupMember.ArchivedDateTime,
                ArchivedByPersonAliasId = groupMember.ArchivedByPersonAliasId,
                InactiveDateTime = groupMember.InactiveDateTime,

                // Set the Modified/Created fields for GroupMemberHistorical to be the current values from GroupMember table
                ModifiedDateTime = groupMember.ModifiedDateTime,
                ModifiedByPersonAliasId = groupMember.ModifiedByPersonAliasId,
                CreatedByPersonAliasId = groupMember.CreatedByPersonAliasId,
                CreatedDateTime = groupMember.CreatedDateTime,

                // Set HistoricalTracking fields
                CurrentRowIndicator = true,
                EffectiveDateTime = effectiveDateTime,
                ExpireDateTime = HistoricalTracking.MaxExpireDateTime
            };

            return groupMemberHistoricalCurrent;
        }

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => GroupMember ?? base.ParentAuthority;

        #endregion
    }
}
